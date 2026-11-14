using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NCTool
{
    class FileItem
    {
        public string Path, Base, Error;
        public Prims Prims;
        public MatRow Row;
        public Result Result;
        // Vat lieu / do day rieng cua tung file khi khong lay tu Excel: gia tri luc them file, hoac do nguoi dung chinh (Override)
        public string MGroup; public double MThick; public bool Override;
    }

    class Buf : Panel { public Buf() { DoubleBuffered = true; } }

    class MainForm : Form
    {
        Buf header = new Buf(), footer = new Buf(), body = new Buf(), left = new Buf(), right = new Buf(), content = new Buf(), listHead = new Buf();
        RoundButton btnXls = new RoundButton(), btnTheme = new RoundButton(), btnAdd = new RoundButton(), btnSave = new RoundButton(), btnSaveAll = new RoundButton();
        Chip chip = new Chip();
        ToolTip tip = new ToolTip();
        Label listTitle = new Label(), status = new Label(), lblMat = new Label(), lblThk = new Label();
        ComboBox cmbMat = new ComboBox();
        NumericUpDown nudThk = new NumericUpDown();
        static readonly string[] Groups = { "SS", "SUS", "AL", "CU" };
        FileList list = new FileList();
        DropZone drop = new DropZone();
        Tiles tiles = new Tiles();
        TabBar tabs = new TabBar();
        PreviewPanel preview = new PreviewPanel();
        RichTextBox nc = new RichTextBox();
        WarnList warn = new WarnList();
        List<FileItem> items = new List<FileItem>();
        MatTable table; string excelPath, tableError, manGroup = "SS"; bool dark; double manThick = 3.2; bool loading = true, syncing = false;
        static readonly string SettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NC_Tool.ini");

        public MainForm()
        {
            Text = "DXF To NC Conversion Tool";
            Size = new Size(1180, 800); MinimumSize = new Size(1160, 640);
            StartPosition = FormStartPosition.CenterScreen; AllowDrop = true; KeyPreview = true;
            Font = new Font("Segoe UI", 9.5f);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            LoadSettings(); Theme.Apply(dark);

            // header
            header.Dock = DockStyle.Top; header.Height = 74;
            btnXls.Text = "Chọn File Excel"; btnXls.LeadIcon = 1; btnXls.Width = 200; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
            Font hf = new Font("Segoe UI", 11f);
            btnTheme.Width = 46;
            lblMat.Text = "Vật Liệu"; lblThk.Text = "Độ Dày"; lblMat.AutoSize = true; lblThk.AutoSize = true; lblMat.Font = hf; lblThk.Font = hf; cmbMat.Font = hf; nudThk.Font = hf;
            cmbMat.DropDownStyle = ComboBoxStyle.DropDownList; cmbMat.FlatStyle = FlatStyle.Flat; cmbMat.Width = 190;
            cmbMat.Items.AddRange(new object[] { "SS – Thép", "SUS – Inox", "AL – Nhôm", "CU – Đồng / Đồng thau" });
            nudThk.DecimalPlaces = 1; nudThk.Increment = 0.1M; nudThk.Minimum = 0.1M; nudThk.Maximum = 30; nudThk.Width = 74; nudThk.BorderStyle = BorderStyle.FixedSingle;
            cmbMat.SelectedIndex = Math.Max(0, Array.IndexOf(Groups, manGroup)); nudThk.Value = (decimal)manThick;
            header.Controls.Add(lblMat); header.Controls.Add(cmbMat); header.Controls.Add(lblThk); header.Controls.Add(nudThk);
            header.Controls.Add(btnXls); header.Controls.Add(btnTheme); header.Controls.Add(chip);
            header.Resize += delegate { LayoutHeader(); };
            header.Paint += PaintHeader;

            // footer
            footer.Dock = DockStyle.Bottom; footer.Height = 72;
            Font ff = new Font("Segoe UI", 11.5f);
            btnSave.Text = "Lưu File .nc"; btnSave.Primary = true; btnSave.Width = 180; btnSave.Height = 46; btnSave.Font = ff;
            btnSaveAll.Text = "Lưu Tất Cả"; btnSaveAll.Width = 160; btnSaveAll.Height = 46; btnSaveAll.Font = ff;
            status.AutoSize = false; status.Left = 16; status.Top = 0; status.Height = 72; status.TextAlign = ContentAlignment.MiddleLeft;
            footer.Controls.Add(btnSave); footer.Controls.Add(btnSaveAll); footer.Controls.Add(status);
            footer.Resize += delegate { LayoutFooter(); };
            footer.Paint += (s, e) => { using (var p = new Pen(Theme.Border)) e.Graphics.DrawLine(p, 0, 0, footer.Width, 0); };

            // trai
            left.Dock = DockStyle.Left; left.Width = 320; left.Padding = new Padding(10, 8, 4, 12);
            listHead.Dock = DockStyle.Top; listHead.Height = 38;
            listTitle.AutoSize = false; listTitle.Dock = DockStyle.Fill; listTitle.TextAlign = ContentAlignment.MiddleLeft; listTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnAdd.Text = "+"; btnAdd.Width = 34; btnAdd.Height = 28; btnAdd.Dock = DockStyle.Right;
            listHead.Controls.Add(listTitle); listHead.Controls.Add(btnAdd);
            drop.Dock = DockStyle.Bottom; drop.Height = 74;
            list.Dock = DockStyle.Fill;
            var gap = new Buf { Dock = DockStyle.Bottom, Height = 8 };
            left.Controls.Add(list); left.Controls.Add(gap); left.Controls.Add(drop); left.Controls.Add(listHead);

            // phai
            right.Dock = DockStyle.Fill; right.Padding = new Padding(8, 8, 14, 10);
            tiles.Dock = DockStyle.Top; tiles.Height = 82;
            var g1 = new Buf { Dock = DockStyle.Top, Height = 8 };
            tabs.Dock = DockStyle.Top; tabs.Height = 34;
            warn.Dock = DockStyle.Bottom; warn.Height = 112;
            var g2 = new Buf { Dock = DockStyle.Bottom, Height = 8 };
            content.Dock = DockStyle.Fill; content.Padding = new Padding(0);
            preview.Dock = DockStyle.Fill;
            nc.Dock = DockStyle.Fill; nc.ReadOnly = true; nc.BorderStyle = BorderStyle.None; nc.WordWrap = false; nc.Visible = false; nc.DetectUrls = false;
            content.Controls.Add(preview); content.Controls.Add(nc);
            content.Paint += (s, e) => { using (var p = new Pen(Theme.Border)) e.Graphics.DrawRectangle(p, 0, 0, content.Width - 1, content.Height - 1); };
            right.Controls.Add(content); right.Controls.Add(g2); right.Controls.Add(warn); right.Controls.Add(tabs); right.Controls.Add(g1); right.Controls.Add(tiles);

            body.Dock = DockStyle.Fill; body.Controls.Add(right); body.Controls.Add(left);
            Controls.Add(body); Controls.Add(footer); Controls.Add(header);

            btnXls.Click += delegate { OpenExcel(); };
            cmbMat.SelectedIndexChanged += delegate { ManualChanged(); };
            nudThk.ValueChanged += delegate { ManualChanged(); };
            btnTheme.Click += delegate { dark = !dark; Theme.Apply(dark); ApplyTheme(); SaveSettings(); };
            btnAdd.Click += delegate { OpenDxf(); };
            drop.Click += delegate { OpenDxf(); };
            list.SelectedIndexChanged += delegate { ShowSelected(); };
            list.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) RemoveItems(list.SelectedItems.Cast<FileItem>().ToList()); };
            tabs.Changed += delegate { preview.Visible = tabs.Index == 0; nc.Visible = tabs.Index == 1; };
            btnSave.Click += delegate { SaveOne(); };
            btnSaveAll.Click += delegate { SaveAll(); };
            Activated += delegate { CheckStamp(); };
            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.O) { OpenDxf(); e.SuppressKeyPress = true; }
                else if (e.Control && e.KeyCode == Keys.S) { SaveOne(); e.SuppressKeyPress = true; }
            };
            FormClosing += delegate { SaveSettings(); };
            Hook(this);
            ApplyTheme(); loading = false; LoadTable();
        }

        void Hook(Control c)
        {
            c.AllowDrop = true;
            c.DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            c.DragDrop += (s, e) => AddFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            foreach (Control k in c.Controls) Hook(k);
        }

        void PaintHeader(object s, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var p = new Pen(Theme.Border)) g.DrawLine(p, 0, header.Height - 1, header.Width, header.Height - 1);
            using (var p = Gfx.RR(new RectangleF(16, 13, 48, 48), 12)) using (var b = new SolidBrush(Color.FromArgb(37, 99, 235))) g.FillPath(b, p);
            Gfx.DrawLaser(g, 21, 19, 38, Color.White, Color.FromArgb(255, 183, 52));
            using (var f = new Font("Segoe UI", 15f, FontStyle.Bold)) using (var b = new SolidBrush(Theme.Text)) g.DrawString("DXF To NC Conversion Tool", f, b, new RectangleF(78, 0, 360, header.Height), Gfx.Left);
        }

        void LayoutHeader()
        {
            btnTheme.Top = (header.Height - btnTheme.Height) / 2; btnTheme.Left = header.Width - btnTheme.Width - 14;
            btnXls.Top = btnTheme.Top; btnXls.Left = btnTheme.Left - btnXls.Width - 8;
            nudThk.Top = (header.Height - nudThk.Height) / 2; nudThk.Left = btnXls.Left - nudThk.Width - 16;
            lblThk.Top = (header.Height - lblThk.Height) / 2; lblThk.Left = nudThk.Left - lblThk.Width - 4;
            cmbMat.Top = (header.Height - cmbMat.Height) / 2; cmbMat.Left = lblThk.Left - cmbMat.Width - 14;
            lblMat.Top = lblThk.Top; lblMat.Left = cmbMat.Left - lblMat.Width - 4;
            chip.Top = (header.Height - chip.Height) / 2; chip.Left = lblMat.Left - chip.Width - 14;
        }
        void LayoutFooter()
        {
            btnSave.Top = (footer.Height - btnSave.Height) / 2; btnSave.Left = footer.Width - btnSave.Width - 16;
            btnSaveAll.Top = btnSave.Top; btnSaveAll.Left = btnSave.Left - btnSaveAll.Width - 8;
            status.Width = Math.Max(10, btnSaveAll.Left - 24);
        }

        void ApplyTheme()
        {
            BackColor = Theme.Page; ForeColor = Theme.Text;
            foreach (Buf p in new[] { header, footer }) p.BackColor = Theme.Card;
            foreach (Buf p in new[] { body, left, right, listHead }) p.BackColor = Theme.Page;
            foreach (Control c in left.Controls) if (c is Buf) c.BackColor = Theme.Page;
            foreach (Control c in right.Controls) if (c is Buf) c.BackColor = Theme.Page;
            content.BackColor = Theme.Card;
            list.BackColor = Theme.Page; warn.BackColor = Theme.Page;
            listTitle.BackColor = Theme.Page; listTitle.ForeColor = Theme.Text;
            status.BackColor = Theme.Card; status.ForeColor = Theme.Muted;
            nc.BackColor = Theme.Card; nc.ForeColor = Theme.Text;
            foreach (Label l in new[] { lblMat, lblThk }) { l.BackColor = Theme.Card; l.ForeColor = Theme.Muted; }
            cmbMat.BackColor = Theme.Card; cmbMat.ForeColor = Theme.Text; nudThk.BackColor = Theme.Card; nudThk.ForeColor = Theme.Text;
            btnTheme.Glyph = dark ? 1 : 2; tip.SetToolTip(btnTheme, dark ? "Chuyển sang chế độ sáng" : "Chuyển sang chế độ tối");
            LayoutHeader(); LayoutFooter();
            ShowSelected();
            InvalidateAll(this);
        }
        void InvalidateAll(Control c) { c.Invalidate(); foreach (Control k in c.Controls) InvalidateAll(k); }

        void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                var l = File.ReadAllLines(SettingsPath);
                excelPath = l.Length > 0 ? l[0].Trim() : null; dark = l.Length > 1 && l[1].Trim() == "dark";
                if (l.Length > 2 && Array.IndexOf(Groups, l[2].Trim()) >= 0) manGroup = l[2].Trim();
                double th; if (l.Length > 3 && double.TryParse(l[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out th) && th >= 0.1 && th <= 30) manThick = th;
            }
            catch { }
        }
        void SaveSettings() { try { File.WriteAllLines(SettingsPath, new[] { excelPath ?? "", dark ? "dark" : "light", Groups[Math.Max(0, cmbMat.SelectedIndex)], ((double)nudThk.Value).ToString(CultureInfo.InvariantCulture) }); } catch { } }

        void OpenExcel()
        {
            using (var d = new OpenFileDialog { Filter = "Excel / CSV (*.xlsx;*.csv)|*.xlsx;*.csv" })
                if (d.ShowDialog() == DialogResult.OK) { excelPath = d.FileName; LoadTable(); }
        }
        void OpenDxf()
        {
            using (var d = new OpenFileDialog { Filter = "File DXF (*.dxf)|*.dxf", Multiselect = true })
                if (d.ShowDialog() == DialogResult.OK) AddFiles(d.FileNames);
        }

        void LoadTable()
        {
            table = null; tableError = null;
            if (string.IsNullOrEmpty(excelPath)) tableError = "Chưa chọn file Excel danh sách vật liệu";
            else if (!File.Exists(excelPath)) tableError = "Không thấy file Excel: " + excelPath;
            else
            {
                try { table = MatTable.Load(excelPath); }
                catch (NCError e) { tableError = e.Message; }
                catch (Exception e) { tableError = "Không đọc được file Excel: " + e.Message; }
            }
            chip.SetText(table != null ? System.IO.Path.GetFileName(excelPath) + " · " + table.Rows.Count + " mã" : "");
            LayoutHeader();
            foreach (var f in items) Convert(f);
            Refill();
        }

        // Doi vat lieu / do day: chi ap dung cho file dang duoc chon (co the chon nhieu file bang Shift/Ctrl); cac file khac giu nguyen
        void ManualChanged()
        {
            if (loading || syncing) return;
            var sel = list.SelectedItems.Cast<FileItem>().ToList();
            if (sel.Count == 0) return;
            foreach (var f in sel) { f.Override = true; f.MGroup = Groups[Math.Max(0, cmbMat.SelectedIndex)]; f.MThick = (double)nudThk.Value; Convert(f); }
            Refill();
        }

        // Dua o Vat Lieu / Do Day ve thong so cua file dang chon (khong kich hoat chuyen doi lai)
        void SyncCombos(FileItem f)
        {
            if (f == null || f.Row == null) return;
            int gi = Array.IndexOf(Groups, f.Row.Group); decimal th = (decimal)f.Row.Thick;
            syncing = true;
            try
            {
                if (gi >= 0) cmbMat.SelectedIndex = gi;
                nudThk.Value = Math.Max(nudThk.Minimum, Math.Min(nudThk.Maximum, th));
            }
            finally { syncing = false; }
        }

        void CheckStamp()
        {
            try { if (table != null && File.GetLastWriteTime(excelPath) != table.Stamp) LoadTable(); } catch { }
        }

        void Convert(FileItem f)
        {
            f.Result = null; f.Row = null;
            if (f.Prims == null) return;
            MatRow tr = table != null ? table.Find(f.Base) : null;   // dong Excel (van dung de lay khach hang khi nguoi dung chinh tay vat lieu / do day)
            f.Row = !f.Override ? tr : null;
            if (f.Row == null)   // khong co trong Excel (hoac nguoi dung da chinh tay) -> dung vat lieu / do day rieng cua file
            {
                if (f.MGroup == null) { f.MGroup = Groups[Math.Max(0, cmbMat.SelectedIndex)]; f.MThick = (double)nudThk.Value; }   // gia tri o thanh tren luc them file
                f.Row = new MatRow { Code = f.Base, MaterialText = "Nhập Tay", Group = f.MGroup, Thick = f.MThick, Line = 0, Customer = tr != null ? tr.Customer : null };
            }
            try { f.Error = null; f.Result = Core.Generate(f.Prims, f.Base, f.Row.Group, f.Row.Thick, f.Row.Customer); }
            catch (NCError e) { f.Error = e.Message; }
            catch (Exception e) { f.Error = "Lỗi: " + e.Message; }
        }

        void AddFiles(IEnumerable<string> paths)
        {
            FileItem lastAdded = null;
            foreach (string p in paths)
            {
                if (!p.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase)) continue;
                var f = new FileItem { Path = p, Base = System.IO.Path.GetFileNameWithoutExtension(p) };
                try { f.Prims = Core.ParseDxf(File.ReadAllBytes(p)); }
                catch (NCError e) { f.Error = e.Message; }
                catch (Exception e) { f.Error = "Không đọc được file: " + e.Message; }
                Convert(f); items.Add(f); lastAdded = f;
            }
            Refill(lastAdded);
        }

        void Refill(FileItem select = null)
        {
            var keep = list.SelectedItems.Cast<FileItem>().Where(x => items.Contains(x)).ToList();
            list.BeginUpdate();
            list.Items.Clear();
            foreach (var f in items) list.Items.Add(f);
            if (select != null) list.SelectedItem = select;
            else if (keep.Count > 0) foreach (var f in keep) list.SelectedItems.Add(f);
            else if (list.Items.Count > 0) list.SelectedIndex = 0;
            list.EndUpdate();
            ShowSelected();
        }

        void RemoveItems(List<FileItem> rm)
        {
            if (rm.Count == 0) return;
            foreach (var f in rm) items.Remove(f);
            list.ClearSelected(); Refill();
        }

        FileItem Selected { get { return list.SelectedItem as FileItem; } }

        void ShowSelected()
        {
            var f = Selected; bool ok = f != null && f.Result != null;
            SyncCombos(f);
            listTitle.Text = "Danh Sách File (" + items.Count + ")";
            btnSave.Enabled = ok; btnSaveAll.Enabled = items.Any(x => x.Result != null);
            status.Text = "";
            preview.R = ok ? f.Result : null;
            preview.Placeholder = f == null ? "Kéo thả file DXF vào cửa sổ" : "Không có hình xem trước cho file này";
            preview.ResetView();
            warn.Items.Clear();
            if (!ok)
            {
                tiles.Values = new[] { "—", "—", "—", "—" }; tiles.Invalidate();
                nc.Text = "";
                if (f == null) warn.Items.Add(new Msg { Kind = 0, Text = "Chưa có file. Kéo thả file .dxf vào cửa sổ hoặc bấm \"+\"." });
                else warn.Items.Add(new Msg { Kind = 2, Text = f.Error });
                return;
            }
            var r = f.Result;
            tiles.Values = new[] { r.W.ToString("0.0", CultureInfo.InvariantCulture) + " × " + r.H.ToString("0.0", CultureInfo.InvariantCulture), r.Cut.ToString("0", CultureInfo.InvariantCulture) + " mm", r.Rapid.ToString("0", CultureInfo.InvariantCulture) + " mm", r.Beam.ToString() };
            tiles.Invalidate();
            nc.Rtf = NcRtf.Build(r.Text);
            warn.Items.Add(new Msg { Kind = f.Row.Line == 0 ? 1 : 3, Text = (f.Row.Line == 0 ? "Không có trong Excel, dùng vật liệu/độ dày nhập tay: " + f.Row.Group + ", dày " + f.Row.Thick.ToString("0.##", CultureInfo.InvariantCulture) + " | piercing ngoài " + r.PierceOut + ", joint " + r.JointOut + ", piercing lỗ " + r.PierceHole : "Excel dòng " + f.Row.Line + ": " + f.Row.Code + " | " + f.Row.MaterialText + " → nhóm " + f.Row.Group + " | dày " + f.Row.Thick.ToString("0.##", CultureInfo.InvariantCulture) + " | piercing ngoài " + r.PierceOut + ", joint " + r.JointOut + ", piercing lỗ " + r.PierceHole) });
            foreach (string w in r.Warnings) warn.Items.Add(new Msg { Kind = 1, Text = w });
            if (table != null) foreach (string w in table.Warnings) warn.Items.Add(new Msg { Kind = 1, Text = w });
        }

        static void WriteNc(string path, string text) { File.WriteAllText(path, text, new ASCIIEncoding()); }

        // Sau khi luu xong: bo chon file Excel (xoa dong ten file tren thanh dau); lan sau phai chon lai. Cac file con lai trong danh sach giu nguyen ket qua da tinh.
        void ClearExcel()
        {
            excelPath = null; table = null; tableError = null;
            chip.SetText(""); LayoutHeader(); SaveSettings();
        }

        // Luu xong -> go file DXF da luu khoi danh sach (file tren o dia giu nguyen)
        void SaveOne()
        {
            var sel = list.SelectedItems.Cast<FileItem>().Where(x => x.Result != null).ToList();
            if (sel.Count > 1) { SaveList(sel); return; }
            var f = Selected; if (f == null || f.Result == null) return;
            using (var d = new SaveFileDialog { Filter = "File NC (*.nc)|*.nc", FileName = f.Base + ".nc", InitialDirectory = System.IO.Path.GetDirectoryName(f.Path) })
                if (d.ShowDialog() == DialogResult.OK)
                {
                    WriteNc(d.FileName, f.Result.Text);
                    RemoveItems(new List<FileItem> { f });
                    status.Text = "Đã lưu " + d.FileName + " và gỡ " + f.Base + ".dxf khỏi danh sách";
                    ClearExcel();
                    ThemedDialog.Show(this, "Đã Chuyển Đổi Thành Công", "File NC: " + d.FileName, true);
                }
        }

        void SaveAll() { SaveList(items.Where(x => x.Result != null).ToList()); }

        void SaveList(List<FileItem> fl)
        {
            var saved = new List<FileItem>();
            foreach (var f in fl)
            {
                string p = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(f.Path), f.Base + ".nc");
                try { WriteNc(p, f.Result.Text); saved.Add(f); } catch (Exception e) { ThemedDialog.Show(this, "Không Lưu Được File", p + "\n" + e.Message, false); }
            }
            RemoveItems(saved);
            status.Text = "Đã lưu " + saved.Count + " file .nc cạnh file DXF và gỡ khỏi danh sách";
            if (saved.Count > 0) ClearExcel();
            if (saved.Count > 0)
                ThemedDialog.Show(this, "Đã Chuyển Đổi Thành Công " + saved.Count + " File", "File .nc được lưu cạnh file DXF tương ứng và đã gỡ khỏi danh sách.", true);
        }
    }
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // Dong lenh: NC_Tool.exe --convert in.dxf out.nc danh_sach.xlsx   (hoac: ... SS 3.2)
            if (args.Length >= 4 && args[0] == "--convert")
            {
                try
                {
                    var P = Core.ParseDxf(File.ReadAllBytes(args[1]));
                    string bn = System.IO.Path.GetFileNameWithoutExtension(args[1]); string grp = args[3]; double th; string cus = args.Length >= 6 ? args[5] : null;   // tuy chon: ma khach hang (vd 824)
                    if (args.Length >= 5) th = double.Parse(args[4], CultureInfo.InvariantCulture);
                    else
                    {
                        var row = MatTable.Load(args[3]).Find(bn);
                        if (row == null) throw new NCError("Mã " + bn + " không có trong Excel");
                        grp = row.Group; th = row.Thick; cus = row.Customer;
                    }
                    var r = Core.Generate(P, bn, grp, th, cus);
                    File.WriteAllText(args[2], r.Text, new ASCIIEncoding());
                    File.WriteAllText(args[2] + ".log", string.Join("\r\n", r.Warnings) + (Environment.GetEnvironmentVariable("NC_DEBUG") == "1" ? "\r\n" + Core.DebugAreas : ""), Encoding.UTF8);
                    return 0;
                }
                catch (Exception e) { File.WriteAllText(args[2] + ".log", "ERR " + e.Message, Encoding.UTF8); return 1; }
            }
            if (args.Length == 3 && args[0] == "--list")
            {
                try { var T = MatTable.Load(args[1]); File.WriteAllLines(args[2], T.Rows.Select(r => r.Line + "|" + r.Code + "|" + r.MaterialText + "|" + r.Group + "|" + r.Thick + "|" + r.Customer).Concat(T.Warnings), Encoding.UTF8); }
                catch (Exception e) { File.WriteAllText(args[2], "ERR " + e.Message, Encoding.UTF8); }
                return 0;
            }
            if (args.Length == 2 && args[0] == "--icon")
            {
                // ICO nhieu kich thuoc (PNG 32-bit) de Windows hien dung mau o moi do phan giai
                Theme.Apply(false);
                int[] sizes = { 16, 24, 32, 48, 64, 128, 256 };
                var pngs = new List<byte[]>();
                foreach (int sz in sizes)
                {
                    using (var bmp = new Bitmap(sz, sz, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; g.Clear(Color.Transparent);
                            float k = sz / 64f;
                            using (var gp = Gfx.RR(new RectangleF(1 * k, 1 * k, 62 * k, 62 * k), 14 * k)) using (var br = new SolidBrush(Color.FromArgb(37, 99, 235))) g.FillPath(br, gp);
                            Gfx.DrawLaser(g, 8 * k, 8 * k, 48 * k, Color.White, Color.FromArgb(255, 183, 52));
                        }
                        using (var ms = new MemoryStream()) { bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png); pngs.Add(ms.ToArray()); }
                    }
                }
                using (var fs = File.Create(args[1]))
                using (var w = new BinaryWriter(fs))
                {
                    w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
                    int off = 6 + 16 * sizes.Length;
                    for (int i = 0; i < sizes.Length; i++)
                    {
                        byte dim = (byte)(sizes[i] >= 256 ? 0 : sizes[i]);
                        w.Write(dim); w.Write(dim); w.Write((byte)0); w.Write((byte)0); w.Write((short)1); w.Write((short)32);
                        w.Write(pngs[i].Length); w.Write(off); off += pngs[i].Length;
                    }
                    foreach (var png in pngs) w.Write(png);
                }
                return 0;
            }            if (args.Length == 2 && args[0] == "--template")
            {
                MatTable.WriteTemplate(args[1], new[] { new[] { "Mã DXF", "Vật liệu", "Độ dày" }, new[] { "Template_NC", "SPHCP", "3.2" }, new[] { "26-060094", "SUS304", "2" }, new[] { "26-060100", "A5052", "3" } });
                return 0;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new MainForm();
            var files = args.Where(a => File.Exists(a)).ToArray();
            if (files.Length > 0) form.Shown += delegate { typeof(MainForm).GetMethod("AddFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(form, new object[] { files }); };
            Application.Run(form);
            return 0;
        }
    }
}
