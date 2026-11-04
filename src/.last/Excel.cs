// Doc danh sach ma DXF / vat lieu / do day tu file Excel (.xlsx) hoac .csv
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NCTool
{
    public class MatRow
    {
        public string Code, MaterialText, Group;
        public double Thick;
        public int Line;
    }

    public class MatTable
    {
        public string Path;
        public DateTime Stamp;
        public List<MatRow> Rows = new List<MatRow>();
        public List<string> Warnings = new List<string>();

        static string Norm(string s)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            s = Regex.Replace(s, @"\.DXF$", "");
            return s;
        }
        static string NoAccent(string s)
        {
            string d = (s ?? "").Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString().Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
        }

        // Vat lieu -> nhom (Quy_Tac_Laser.md muc 1)
        public static string GroupOf(string mat)
        {
            string m = (mat ?? "").Trim().ToUpperInvariant().Replace(" ", "");
            if (m == "") return null;
            if (m == "SS" || m == "SUS" || m == "AL" || m == "CU") return m;
            string na = NoAccent(mat).Replace(" ", "");
            string[] sus = { "SUS", "INOX", "2B", "HL", "304", "316", "430", "301", "310S", "410S", "VBI", "#400", "#1", "NO1", "ヘアライン", "片研", "両研", "エンボス" };
            string[] al = { "A5052", "A505", "A2017", "AL", "NHOM", "アルミ" };
            string[] cu = { "CU", "DONG", "THAU", "C1100", "C2801", "BRASS", "COPPER", "銅", "黄銅", "真鍮", "シンチュウ", "C2600", "C2680", "C3604" };   // CU gom ca dong thau (真鍮): thong so giong dong
            string[] ss = { "SS", "SPHC", "SPCC", "SECC", "SK", "S45C", "S48C", "SPC", "STPL", "STEN", "SUP", "NO205", "THEP", "鋼", "黒皮", "酸洗", "ボンデ", "みがき", "リボン", "ハイテン", "ベーナイト" };
            if (sus.Any(k => m.Contains(k) || na.Contains(k.ToLowerInvariant()))) return "SUS";
            if (al.Any(k => m.Contains(k) || na.Contains(k.ToLowerInvariant()))) return "AL";
            if (cu.Any(k => m.Contains(k) || na.Contains(k.ToLowerInvariant()))) return "CU";
            if (ss.Any(k => m.Contains(k) || na.Contains(k.ToLowerInvariant()))) return "SS";
            return null;
        }

        static bool ParseThick(string s, out double t)
        {
            t = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var m = Regex.Match(s.Replace(',', '.'), @"\d+(\.\d+)?");
            return m.Success && double.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out t) && t > 0;
        }

        public static MatTable Load(string path)
        {
            var T = new MatTable { Path = path, Stamp = File.GetLastWriteTime(path) };
            List<List<string>> grid;
            string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".xlsx" || ext == ".xlsm") grid = ReadXlsx(path);
            else if (ext == ".csv" || ext == ".txt") grid = ReadCsv(path);
            else throw new NCError("Chỉ hỗ trợ file .xlsx hoặc .csv");

            // tim dong tieu de
            int hdr = -1, cCode = 0, cMat = 1, cThk = 2;
            for (int r = 0; r < Math.Min(grid.Count, 15) && hdr < 0; r++)
            {
                int a = -1, b = -1, c = -1;
                for (int i = 0; i < grid[r].Count; i++)
                {
                    string h = NoAccent(grid[r][i]).Trim();
                    if (h == "") continue;
                    if (b < 0 && Regex.IsMatch(h, @"vat lieu|material|材質|材料|^vl$")) b = i;
                    else if (c < 0 && Regex.IsMatch(h, @"do day|day|thick|板厚|^t$|\(mm\)")) c = i;
                    else if (a < 0 && Regex.IsMatch(h, @"^(ma|code|ten|file|dxf|so don|図番|品番|作業)|\bma\b|dxf|ten file")) a = i;
                }
                if (a >= 0 && b >= 0 && c >= 0) { hdr = r; cCode = a; cMat = b; cThk = c; }
            }
            var seen = new HashSet<string>();
            for (int r = hdr + 1; r < grid.Count; r++)
            {
                var row = grid[r];
                Func<int, string> cell = i => i < row.Count ? (row[i] ?? "").Trim() : "";
                string code = cell(cCode), mat = cell(cMat); double t;
                // Mau CAD依頼書: cot 材質 chua ma so (vd 12), ten vat lieu nam o cot ben canh (SPHCP...)
                if (GroupOf(mat) == null) { string alt = cell(cMat + 1); if (GroupOf(alt) != null) mat = alt; }
                if (code == "" && mat == "") continue;
                if (!ParseThick(cell(cThk), out t))
                {
                    if (hdr < 0 && r == 0) continue;           // khong co tieu de: dong dau la tieu de
                    T.Warnings.Add("Dòng " + (r + 1) + " (" + code + "): độ dày '" + cell(cThk) + "' không hợp lệ, bỏ qua");
                    continue;
                }
                string g = GroupOf(mat);
                if (g == null) { T.Warnings.Add("Dòng " + (r + 1) + " (" + code + "): vật liệu '" + mat + "' không nhận dạng được, bỏ qua"); continue; }
                if (code == "") continue;
                if (!seen.Add(Norm(code))) T.Warnings.Add("Mã " + code + " bị lặp trong Excel: dùng dòng đầu tiên");
                else T.Rows.Add(new MatRow { Code = code, MaterialText = mat, Group = g, Thick = t, Line = r + 1 });
            }
            if (T.Rows.Count == 0) throw new NCError("Không có dòng hợp lệ trong file Excel. Cần 3 cột: Mã DXF | Vật liệu | Độ dày.");
            return T;
        }

        // Tim dong theo ten file DXF (khong duoi): khop chinh xac, neu khong thi ma la phan dau/ nam trong ten file
        public MatRow Find(string baseName)
        {
            string b = Norm(baseName);
            var exact = Rows.FirstOrDefault(r => Norm(r.Code) == b);
            if (exact != null) return exact;
            return Rows.Where(r => b.StartsWith(Norm(r.Code)) || b.Contains(Norm(r.Code))).OrderByDescending(r => r.Code.Length).FirstOrDefault();
        }

        // ---------- CSV ----------
        static List<List<string>> ReadCsv(string path)
        {
            byte[] data;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var ms = new MemoryStream()) { fs.CopyTo(ms); data = ms.ToArray(); }
            string text = data.Length > 2 && data[0] == 0xEF ? Encoding.UTF8.GetString(data, 3, data.Length - 3) : Encoding.UTF8.GetString(data);
            var rows = new List<List<string>>();
            foreach (string line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                if (line.Trim() == "") continue;
                char sep = line.Contains(";") && !line.Contains(",") ? ';' : (line.Contains("\t") ? '\t' : ',');
                var cells = new List<string>(); var sb = new StringBuilder(); bool q = false;
                foreach (char c in line)
                {
                    if (c == '"') q = !q;
                    else if (c == sep && !q) { cells.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(c);
                }
                cells.Add(sb.ToString()); rows.Add(cells);
            }
            return rows;
        }

        // ---------- XLSX ----------
        static int ColIndex(string cellRef)
        {
            int n = 0;
            foreach (char c in cellRef) { if (!char.IsLetter(c)) break; n = n * 26 + (char.ToUpperInvariant(c) - 'A' + 1); }
            return n - 1;
        }

        static XDocument ReadXml(ZipArchive z, string name)
        {
            var e = z.GetEntry(name);
            if (e == null) return null;
            using (var s = e.Open()) return XDocument.Load(s);
        }

        static List<List<string>> ReadXlsx(string path)
        {
            byte[] data;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))   // doc duoc ca khi Excel dang mo
            using (var ms = new MemoryStream()) { fs.CopyTo(ms); data = ms.ToArray(); }
            using (var z = new ZipArchive(new MemoryStream(data), ZipArchiveMode.Read))
            {
                XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                XNamespace rn = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                XNamespace pr = "http://schemas.openxmlformats.org/package/2006/relationships";
                var sst = new List<string>();
                var sd = ReadXml(z, "xl/sharedStrings.xml");
                if (sd != null) foreach (var si in sd.Root.Elements(ns + "si")) sst.Add(string.Concat(si.Elements(ns + "t").Concat(si.Elements(ns + "r").Elements(ns + "t")).Select(t => t.Value)));

                string sheetPath = "xl/worksheets/sheet1.xml";
                var wb = ReadXml(z, "xl/workbook.xml"); var rels = ReadXml(z, "xl/_rels/workbook.xml.rels");
                if (wb != null && rels != null)
                {
                    var sh = wb.Root.Descendants(ns + "sheet").FirstOrDefault();
                    if (sh != null)
                    {
                        var rid = (string)sh.Attribute(rn + "id");
                        var rel = rels.Root.Elements(pr + "Relationship").FirstOrDefault(x => (string)x.Attribute("Id") == rid);
                        if (rel != null)
                        {
                            string tg = (string)rel.Attribute("Target");
                            sheetPath = tg.StartsWith("/") ? tg.TrimStart('/') : "xl/" + tg;
                        }
                    }
                }
                var sheet = ReadXml(z, sheetPath);
                if (sheet == null) throw new NCError("Không đọc được sheet đầu tiên của file Excel");
                var rows = new List<List<string>>();
                foreach (var r in sheet.Descendants(ns + "row"))
                {
                    int ri = r.Attribute("r") != null ? int.Parse((string)r.Attribute("r")) - 1 : rows.Count;
                    while (rows.Count <= ri) rows.Add(new List<string>());
                    var cells = rows[ri];
                    foreach (var c in r.Elements(ns + "c"))
                    {
                        int ci = ColIndex((string)c.Attribute("r") ?? "A1");
                        string t = (string)c.Attribute("t"), v = (string)c.Element(ns + "v"), val = "";
                        if (t == "s" && v != null) { int k; if (int.TryParse(v, out k) && k < sst.Count) val = sst[k]; }
                        else if (t == "inlineStr") val = string.Concat(c.Descendants(ns + "t").Select(x => x.Value));
                        else if (v != null)
                        {
                            double d;
                            val = (t == null || t == "n") && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d) ? d.ToString("0.########", CultureInfo.InvariantCulture) : v;
                        }
                        while (cells.Count <= ci) cells.Add("");
                        cells[ci] = val;
                    }
                }
                return rows;
            }
        }

        // ---------- Tao file mau .xlsx ----------
        public static void WriteTemplate(string path, string[][] data)
        {
            Func<string, string> esc = s => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><cols><col min=\"1\" max=\"1\" width=\"26\" customWidth=\"1\"/><col min=\"2\" max=\"2\" width=\"18\" customWidth=\"1\"/><col min=\"3\" max=\"3\" width=\"12\" customWidth=\"1\"/></cols><sheetData>");
            for (int r = 0; r < data.Length; r++)
            {
                sb.Append("<row r=\"" + (r + 1) + "\">");
                for (int c = 0; c < data[r].Length; c++)
                {
                    string refc = ((char)('A' + c)).ToString() + (r + 1), v = data[r][c]; double d;
                    if (r > 0 && c == 2 && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) sb.Append("<c r=\"" + refc + "\"><v>" + v + "</v></c>");
                    else sb.Append("<c r=\"" + refc + "\" t=\"inlineStr\"><is><t>" + esc(v) + "</t></is></c>");
                }
                sb.Append("</row>");
            }
            sb.Append("</sheetData></worksheet>");
            if (File.Exists(path)) File.Delete(path);
            using (var z = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                Action<string, string> add = (n, content) => { var e = z.CreateEntry(n); using (var w = new StreamWriter(e.Open(), new UTF8Encoding(false))) w.Write(content); };
                add("[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
                add("_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                add("xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"DanhSach\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                add("xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
                add("xl/worksheets/sheet1.xml", sb.ToString());
            }
        }
    }
}
