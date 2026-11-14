// NC Tool core: DXF (tam kim loai trai phoi) -> NC CamMagic LA (Mitsubishi)
// Quy tac mau (Layer 0):
//   by layer : LINE/ARC/CIRCLE -> bien ngoai, lo, (duong ho = khe cat)  M98P9010 ; POINT -> lo trang M98P9016
//   xanh la (ACI 3): LINE/ARC -> khe cat ; POINT -> piercing xuyen (M98P9010)
//   do (ACI 1): LINE/ARC/CIRCLE -> kegaki M98P9017
//   net dut (DASHED) -> duong uon, bo qua
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NCTool
{
    public class NCError : Exception { public NCError(string m) : base(m) { } }

    public class Seg
    {
        public char T;               // 'L' | 'A'
        public double[] P, Q, C;
        public double R, Sweep;
        public bool Ccw = true;
        public string Col;           // "L" (by layer) | "green" | "red"
        public bool Spl;             // doan thang sinh ra tu SPLINE
        public Seg Copy() { return (Seg)MemberwiseClone(); }
    }
    public class Circ { public double Cx, Cy, R; public string Col; }
    public class Pnt { public double X, Y; public string Col; }

    public class Prims
    {
        public List<Seg> Segs = new List<Seg>();
        public List<Circ> Circles = new List<Circ>();
        public List<Pnt> Points = new List<Pnt>();
        public List<string> Warnings = new List<string>();
        public Dictionary<string, double[]> Marks = new Dictionary<string, double[]>();   // diem danh dau LJ (phoi dai hep)
        public List<double[]> Js = new List<double[]>();   // cac diem danh dau J (co the nhieu diem)
        public List<double[]> Hull = new List<double[]>();   // diem dieu khien cua SPLINE (dung de dat goc toa do)
    }

    public class PLine { public string Col; public List<double[]> Pts = new List<double[]>(); }
    public class Step { public double X, Y; public int Code; }
    public class Preview
    {
        public List<PLine> Lines = new List<PLine>();
        public List<Circ> Circles;
        public List<Pnt> Points;
        public List<Step> Steps = new List<Step>();
        public double X0, Y0, X1, Y1;
    }
    public class Result
    {
        public string Text;
        public List<string> Warnings;
        public double Cut, Rapid, W, H;
        public int Beam;
        public Preview Preview;
        public double PierceOut, JointOut, PierceHole;
    }

    public static class Core
    {
        public static string DebugAreas = "";
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        const double TAU = 2 * Math.PI;
        static double Mod(double a, double m) { return ((a % m) + m) % m; }
        static string Key(double[] p) { return Math.Round(p[0] * 1000, MidpointRounding.AwayFromZero).ToString(Inv) + "," + Math.Round(p[1] * 1000, MidpointRounding.AwayFromZero).ToString(Inv); }
        static double Dist(double[] a, double[] b) { return Math.Sqrt((a[0] - b[0]) * (a[0] - b[0]) + (a[1] - b[1]) * (a[1] - b[1])); }
        static double[] R(double a, double b, double c, double d) { return new[] { a, b, c, d }; }
        static double[] Pt(double x, double y) { return new double[] { x, y }; }

        public static string Fmt(double v)
        {
            v = Math.Sign(v) * Math.Round(Math.Abs(v) * 1000, MidpointRounding.AwayFromZero) / 1000;
            if (Math.Abs(v) < 1e-9) v = 0;
            return v.ToString("0.000", Inv).TrimEnd('0');      // 21.000 -> "21." ; 46.800 -> "46.8"
        }

        // ---------- Bang thong so: piercing ngoai, joint ngoai, piercing lo ----------
        public static void Params(string group, double t, out double pierceOut, out double jointOut, out double pierceHole)
        {
            if (!(t > 0)) throw new NCError("Độ dày không hợp lệ");
            double[][] rows;   // {tmax, pierceOut, jointOut, pierceHole}
            switch (group.ToUpperInvariant())
            {
                case "SS": rows = new[] { R(3.2, 3, 0.8, 6), R(5, 6, 1, 6), R(9, 6, 1, 6), R(16, 10, 1, 10) }; break;
                case "SUS": rows = new[] { R(8, 6, 0.7, 6), R(16, 10, 1, 10) }; break;
                case "AL": rows = new[] { R(8, 6, 0.7, 6), R(18, 10, 1, 10) }; break;
                case "CU": rows = new[] { R(99, 6, 0.7, 6) }; break;
                default: throw new NCError("Nhóm vật liệu không hợp lệ: " + group);
            }
            foreach (double[] r in rows)
                if (t <= r[0] + 1e-9) { pierceOut = r[1]; jointOut = r[2]; pierceHole = r[3]; return; }
            if (group.ToUpperInvariant() == "SS")
            {
                if (t >= 19 - 1e-9) { pierceOut = 10; jointOut = 3; pierceHole = 10; return; }   // SS >= 19: piercing cong, joint ngoai 3 (theo bang goc va ca 26-061939), piercing lo 10
                pierceOut = 10; jointOut = 1; pierceHole = 10; return;   // SS dày trên 16 và dưới 19: làm như thép t10-12 (piercing thẳng 10, joint ngoài 1, piercing lỗ 10)
            }
            if (group.ToUpperInvariant() == "AL" && t >= 19 - 1e-9) { pierceOut = 10; jointOut = 0.01; pierceHole = 10; return; }   // AL >= 19: piercing cong, joint ngoai ~0 (0.01), piercing lo 10, joint lo 0
            if (group.ToUpperInvariant() == "SUS")
            {
                if (t >= 19 - 1e-9) { pierceOut = 10; jointOut = 3; pierceHole = 10; return; }   // SUS >= 19: piercing cong 10 + 7 + R3, joint ngoai 3, piercing lo 10
                pierceOut = 10; jointOut = 1; pierceHole = 10; return;   // SUS dày trên 16 và dưới 19: làm như inox t9-12 (piercing 10, joint ngoài 1, piercing lỗ 10, joint lỗ 1)
            }
            throw new NCError("Độ dày " + t.ToString(Inv) + " của nhóm " + group + " dùng piercing cong (≥19t): chưa hỗ trợ");
        }

        // Piercing ngoai cong (duong thang 7 + cung R3 vao tiep tuyen): chi ap dung cho thep SS day >= 19
        public static bool Curved(string group, double t) { return group.ToUpperInvariant() == "SS" && t >= 19 - 1e-9; }

        // ---------- Doc DXF ----------
        class Ent { public string Type; public List<KeyValuePair<string, string>> G = new List<KeyValuePair<string, string>>(); }
        static string Get(Ent e, string c) { foreach (var kv in e.G) if (kv.Key == c) return kv.Value; return null; }
        static double Num(Ent e, string c) { return double.Parse(Get(e, c), Inv); }

        static string ColorOf(Ent e)
        {
            string c = Get(e, "62");
            int n = 256;
            if (c != null) int.TryParse(c, NumberStyles.Integer, Inv, out n);
            if (n == 256 || n == 0 || n == 7) return "L";
            if (n == 1) return "red";
            if (n == 3) return "green";
            return "other";
        }

        class Mat
        {
            public double S = 1, Rot = 0, Tx = 0, Ty = 0;
            public double[] Ap(double x, double y) { double c = Math.Cos(Rot), s = Math.Sin(Rot); return new[] { Tx + S * (c * x - s * y), Ty + S * (s * x + c * y) }; }
            public Mat Then(Mat inner) { var t = Ap(inner.Tx, inner.Ty); return new Mat { S = S * inner.S, Rot = Rot + inner.Rot, Tx = t[0], Ty = t[1] }; }
        }

        // ---------- SPLINE -> duong gap khuc ----------
        // Chia doi tham so de quy: mot doan [ua, ub] duoc chia doi khi do vong lon nhat so voi day cung (do tai 1/4, 1/2, 3/4 tham so) > SplineTol.
        // Khop voi ViDu_Spline.nc (dung cho moi diem, 165 diem chia).
        const double SplineTol = 0.01;
        static double Sag(double[] a, double[] m, double[] b)
        {
            double dx = b[0] - a[0], dy = b[1] - a[1], L = Math.Sqrt(dx * dx + dy * dy);
            if (L < 1e-12) return Dist(a, m);
            return Math.Abs((m[0] - a[0]) * dy - (m[1] - a[1]) * dx) / L;
        }
        static List<double[]> SplinePolyline(List<double[]> cps, List<double> wts, List<double> kn, int deg, List<double[]> fit)
        {
            int n = cps.Count - 1;
            if (deg < 1 || n < deg || kn.Count != cps.Count + deg + 1)
            {
                if (fit.Count >= 2) return new List<double[]>(fit);   // khong du du lieu B-spline: noi thang cac diem fit
                throw new NCError("SPLINE thiếu dữ liệu điểm điều khiển/knot. Hãy Explode hoặc lưu lại DXF.");
            }
            bool rational = wts.Count == cps.Count;
            double u0 = kn[deg], u1 = kn[n + 1];
            Func<double, double[]> eval = u =>
            {
                int k = deg;
                if (u >= u1) { k = n; while (k > deg && !(kn[k] < kn[k + 1])) k--; }
                else for (int i = deg; i <= n; i++) if (u >= kn[i] && u < kn[i + 1]) { k = i; break; }
                var d = new double[deg + 1][];
                for (int j = 0; j <= deg; j++) { var c = cps[j + k - deg]; double w = rational ? wts[j + k - deg] : 1; d[j] = new[] { c[0] * w, c[1] * w, w }; }
                for (int r = 1; r <= deg; r++)
                    for (int j = deg; j >= r; j--)
                    {
                        double den = kn[j + 1 + k - r] - kn[j + k - deg];
                        double a = den == 0 ? 0 : (u - kn[j + k - deg]) / den;
                        for (int t = 0; t < 3; t++) d[j][t] = (1 - a) * d[j - 1][t] + a * d[j][t];
                    }
                return Pt(d[deg][0] / d[deg][2], d[deg][1] / d[deg][2]);
            };
            var res = new List<double[]>();
            Action<double, double, double[], double[], int> rec = null;
            rec = (ua, ub, pa, pb, depth) =>
            {
                double um = (ua + ub) / 2; var pm = eval(um);
                var q1 = eval((ua + um) / 2); var q3 = eval((um + ub) / 2);
                double dev = Math.Max(Sag(pa, pm, pb), Math.Max(Sag(pa, q1, pb), Sag(pa, q3, pb)));
                if (dev > SplineTol && depth < 16) { rec(ua, um, pa, pm, depth + 1); res.Add(pm); rec(um, ub, pm, pb, depth + 1); }
            };
            var s0 = eval(u0); var s1 = eval(u1);
            res.Add(s0); rec(u0, u1, s0, s1, 0); res.Add(s1);
            return res;
        }

        public static Prims ParseDxf(byte[] data)
        {
            string text = Encoding.UTF8.GetString(data);
            if (text.StartsWith("AutoCAD Binary DXF")) throw new NCError("DXF dạng binary: hãy lưu lại dạng ASCII");
            string[] lines = text.Split('\n');
            var pairs = new List<string[]>();
            for (int i = 0; i + 1 < lines.Length; i += 2) pairs.Add(new[] { lines[i].Trim(), lines[i + 1].Trim() });

            // Doc 2 section: BLOCKS (dinh nghia khoi) va ENTITIES
            var ents = new List<Ent>(); var blocks = new Dictionary<string, List<Ent>>(); var bases = new Dictionary<string, double[]>();
            string sect = null; Ent cur = null; List<Ent> curBlock = null; bool sawEnt = false;
            Action flush = () =>
            {
                if (cur == null) return;
                if (sect == "ENTITIES") ents.Add(cur);
                else if (sect == "BLOCKS")
                {
                    if (cur.Type == "BLOCK")
                    {
                        string nm = Get(cur, "2") ?? ""; curBlock = new List<Ent>(); blocks[nm] = curBlock;
                        double bx = 0, by = 0;
                        double.TryParse(Get(cur, "10") ?? "0", NumberStyles.Float, Inv, out bx); double.TryParse(Get(cur, "20") ?? "0", NumberStyles.Float, Inv, out by);
                        bases[nm] = new[] { bx, by };
                    }
                    else if (cur.Type == "ENDBLK") curBlock = null;
                    else if (curBlock != null) curBlock.Add(cur);
                }
                cur = null;
            };
            for (int i = 0; i < pairs.Count; i++)
            {
                string c = pairs[i][0], v = pairs[i][1];
                if (c == "0" && v == "SECTION") { flush(); sect = null; continue; }
                if (c == "2" && sect == null && i > 0 && pairs[i - 1][0] == "0" && pairs[i - 1][1] == "SECTION") { sect = v; if (v == "ENTITIES") sawEnt = true; continue; }
                if (c == "0" && v == "ENDSEC") { flush(); sect = null; continue; }
                if (sect == "ENTITIES" || sect == "BLOCKS")
                {
                    if (c == "0") { flush(); cur = new Ent { Type = v }; }
                    else if (cur != null) cur.G.Add(new KeyValuePair<string, string>(c, v));
                }
            }
            flush();
            if (!sawEnt) throw new NCError("Không tìm thấy section ENTITIES trong DXF");

            var P = new Prims();
            var ignored = new Dictionary<string, int>();
            Action<string> ign = k => { int n; ignored.TryGetValue(k, out n); ignored[k] = n + 1; };
            int expanded = 0, splineSegs = 0, splineCount = 0;

            Action<double[], double[], double, string> pushBulge = (p, q, b, col) =>
            {
                if (Math.Abs(b) < 1e-9) { P.Segs.Add(new Seg { T = 'L', P = p, Q = q, Col = col }); return; }
                double d = Dist(p, q); if (d < 1e-9) return;
                double th = 4 * Math.Atan(b);
                double r = d / (2 * Math.Sin(Math.Abs(th) / 2));
                double ux = (q[0] - p[0]) / d, uy = (q[1] - p[1]) / d;
                double off = (d / 2) / Math.Tan(th / 2);
                double[] c = Pt((p[0] + q[0]) / 2 - uy * off, (p[1] + q[1]) / 2 + ux * off);
                if (b > 0) P.Segs.Add(new Seg { T = 'A', C = c, R = r, P = p, Q = q, Sweep = Math.Abs(th), Col = col });
                else P.Segs.Add(new Seg { T = 'A', C = c, R = r, P = q, Q = p, Sweep = Math.Abs(th), Col = col });
            };

            Action<Ent, Mat, string, int> add = null;
            add = (en, m, parentCol, depth) =>
            {
                string lt = (Get(en, "6") ?? "").ToUpperInvariant();
                bool dashed = lt != "" && lt != "CONTINUOUS" && lt != "BYLAYER" && lt != "BYBLOCK";
                string col = ColorOf(en);
                if (parentCol != null && (Get(en, "62") ?? "") == "0") col = parentCol;   // ByBlock -> mau cua INSERT
                double rotDeg = m.Rot * 180 / Math.PI;
                // He toa do doi tuong (OCS) theo vector phap tuyen 210/220/230 (thuat toan truc tuy y cua AutoCAD): CIRCLE/ARC/LWPOLYLINE/POINT ve
                // trong UCS bi lat guong (230 = -1) co hoanh do dao dau
                double nx = 0, ny = 0, nz = 1;
                if (Get(en, "230") != null) { double.TryParse(Get(en, "210") ?? "0", NumberStyles.Float, Inv, out nx); double.TryParse(Get(en, "220") ?? "0", NumberStyles.Float, Inv, out ny); double.TryParse(Get(en, "230"), NumberStyles.Float, Inv, out nz); }
                double nl = Math.Sqrt(nx * nx + ny * ny + nz * nz); if (nl < 1e-12) { nx = 0; ny = 0; nz = 1; nl = 1; }
                nx /= nl; ny /= nl; nz /= nl;
                double[] axv, ayv;
                if (Math.Abs(nx) < 1.0 / 64 && Math.Abs(ny) < 1.0 / 64) { axv = new[] { 1 * nz - 0 * ny, 0 * nx - 0 * nz, 0 * ny - 1 * nx }; }   // Wy x N
                else { axv = new[] { 0 * nz - 1 * ny, 1 * nx - 0 * nz, 0 }; }   // Wz x N
                double axl = Math.Sqrt(axv[0] * axv[0] + axv[1] * axv[1] + axv[2] * axv[2]); axv[0] /= axl; axv[1] /= axl; axv[2] /= axl;
                ayv = new[] { ny * axv[2] - nz * axv[1], nz * axv[0] - nx * axv[2], nx * axv[1] - ny * axv[0] };   // N x Ax
                bool ocsId = Math.Abs(nx) < 1e-9 && Math.Abs(ny) < 1e-9 && nz > 0;
                double ocsDet = axv[0] * ayv[1] - axv[1] * ayv[0];
                Func<double, double, double[]> wxy = (x, y) => ocsId ? new[] { x, y } : new[] { x * axv[0] + y * ayv[0], x * axv[1] + y * ayv[1] };
                switch (en.Type)
                {
                    case "LINE": case "ARC": case "CIRCLE": case "LWPOLYLINE":
                        if (dashed) { ign("đường nét đứt (đường uốn)"); break; }
                        if (col == "other") { ign("đối tượng màu khác (không phải by layer/xanh/đỏ)"); break; }
                        if (en.Type == "LINE")
                            P.Segs.Add(new Seg { T = 'L', P = m.Ap(Num(en, "10"), Num(en, "20")), Q = m.Ap(Num(en, "11"), Num(en, "21")), Col = col });
                        else if (en.Type == "ARC")
                        {
                            var wc = wxy(Num(en, "10"), Num(en, "20")); double[] cc = m.Ap(wc[0], wc[1]); double rr = Num(en, "40") * m.S;
                            double a0 = Num(en, "50") * Math.PI / 180, a1 = Num(en, "51") * Math.PI / 180;
                            if (!ocsId)
                            {
                                var d0 = wxy(Math.Cos(a0), Math.Sin(a0)); var d1 = wxy(Math.Cos(a1), Math.Sin(a1));
                                a0 = Math.Atan2(d0[1], d0[0]); a1 = Math.Atan2(d1[1], d1[0]);
                                if (ocsDet < 0) { double tmpA = a0; a0 = a1; a1 = tmpA; }   // lat guong: doi chieu cung
                            }
                            a0 += rotDeg * Math.PI / 180; a1 += rotDeg * Math.PI / 180;
                            double sw = Mod(a1 - a0, TAU); if (sw < 1e-9) sw = TAU;
                            P.Segs.Add(new Seg { T = 'A', C = cc, R = rr, P = Pt(cc[0] + rr * Math.Cos(a0), cc[1] + rr * Math.Sin(a0)), Q = Pt(cc[0] + rr * Math.Cos(a1), cc[1] + rr * Math.Sin(a1)), Sweep = sw, Col = col });
                        }
                        else if (en.Type == "CIRCLE")
                        {
                            var wc = wxy(Num(en, "10"), Num(en, "20")); double[] cc = m.Ap(wc[0], wc[1]);
                            P.Circles.Add(new Circ { Cx = cc[0], Cy = cc[1], R = Num(en, "40") * m.S, Col = col });
                        }
                        else
                        {
                            var vs = new List<double[]>();   // x, y, bulge
                            foreach (var kv in en.G)
                            {
                                if (kv.Key == "10") vs.Add(new[] { double.Parse(kv.Value, Inv), 0, 0 });
                                else if (kv.Key == "20" && vs.Count > 0) vs[vs.Count - 1][1] = double.Parse(kv.Value, Inv);
                                else if (kv.Key == "42" && vs.Count > 0) vs[vs.Count - 1][2] = double.Parse(kv.Value, Inv);
                            }
                            int fl = 0; int.TryParse(Get(en, "70") ?? "0", out fl);
                            bool closed = (fl & 1) == 1;
                            int cnt = closed ? vs.Count : vs.Count - 1;
                            for (int k = 0; k < cnt; k++)
                            {
                                double[] va = vs[k], vb = vs[(k + 1) % vs.Count];
                                var wa = wxy(va[0], va[1]); var wb = wxy(vb[0], vb[1]);
                                pushBulge(m.Ap(wa[0], wa[1]), m.Ap(wb[0], wb[1]), ocsDet < 0 ? -va[2] : va[2], col);
                            }
                        }
                        break;
                    case "TEXT":
                        {
                            string tx = (Get(en, "1") ?? "").Trim().ToUpperInvariant();
                            if (tx == "LJ") P.Marks[tx] = m.Ap(Num(en, "10"), Num(en, "20"));
                            else if (tx == "J") P.Js.Add(m.Ap(Num(en, "10"), Num(en, "20")));
                            break;
                        }
                    case "POINT":
                        if (col == "other") { ign("POINT màu khác"); break; }
                        { var wp = wxy(Num(en, "10"), Num(en, "20")); var pp = m.Ap(wp[0], wp[1]); P.Points.Add(new Pnt { X = pp[0], Y = pp[1], Col = col }); }
                        break;
                    case "INSERT":
                        {
                            string bn = Get(en, "2") ?? "";
                            List<Ent> kids;
                            if (!blocks.TryGetValue(bn, out kids)) { ign("INSERT thiếu định nghĩa khối \"" + bn + "\""); break; }
                            if (depth >= 8) throw new NCError("Khối (INSERT) lồng quá sâu: " + bn);
                            double sx = double.Parse(Get(en, "41") ?? "1", Inv), sy = double.Parse(Get(en, "42") ?? "1", Inv);
                            double rot = double.Parse(Get(en, "50") ?? "0", Inv) * Math.PI / 180;
                            if (Math.Abs(sx - sy) > 1e-9 || sx <= 0) throw new NCError("Khối (INSERT) \"" + bn + "\" có tỉ lệ không đều hoặc đối xứng (chưa hỗ trợ). Hãy Explode khối trước.");
                            double ix = Num(en, "10"), iy = Num(en, "20"); var bp = bases[bn];
                            var local = new Mat { S = sx, Rot = rot };
                            local.Tx = ix - sx * (Math.Cos(rot) * bp[0] - Math.Sin(rot) * bp[1]);
                            local.Ty = iy - sx * (Math.Sin(rot) * bp[0] + Math.Cos(rot) * bp[1]);
                            expanded++;
                            var total = m.Then(local);
                            foreach (var ch in kids) add(ch, total, col, depth + 1);
                            break;
                        }
                    case "SPLINE":
                        {
                            if (dashed) { ign("đường nét đứt (đường uốn)"); break; }
                            if (col == "other") { ign("đối tượng màu khác (không phải by layer/xanh/đỏ)"); break; }
                            int deg = 3; int.TryParse(Get(en, "71") ?? "3", out deg);
                            var kn = new List<double>(); var cps = new List<double[]>(); var wts = new List<double>(); var fit = new List<double[]>();
                            foreach (var kv in en.G)
                            {
                                double dv; if (!double.TryParse(kv.Value, NumberStyles.Float, Inv, out dv)) continue;
                                if (kv.Key == "40") kn.Add(dv);
                                else if (kv.Key == "41") wts.Add(dv);
                                else if (kv.Key == "10") cps.Add(new[] { dv, 0 });
                                else if (kv.Key == "20" && cps.Count > 0) cps[cps.Count - 1][1] = dv;
                                else if (kv.Key == "11") fit.Add(new[] { dv, 0 });
                                else if (kv.Key == "21" && fit.Count > 0) fit[fit.Count - 1][1] = dv;
                            }
                            var cpsT = cps.Select(c => m.Ap(c[0], c[1])).ToList();
                            var poly = SplinePolyline(cpsT, wts, kn, deg, fit.Select(c => m.Ap(c[0], c[1])).ToList());
                            P.Hull.AddRange(cpsT);
                            int segs = 0;
                            for (int k = 0; k + 1 < poly.Count; k++)
                                if (Dist(poly[k], poly[k + 1]) > 1e-9) { P.Segs.Add(new Seg { T = 'L', P = poly[k], Q = poly[k + 1], Col = col, Spl = true }); segs++; }
                            splineSegs += segs; splineCount++;
                            break;
                        }
                    case "POLYLINE": case "ELLIPSE":
                        if (dashed) { ign("đường nét đứt (đường uốn)"); break; }
                        throw new NCError("DXF có đối tượng " + en.Type + " (chưa hỗ trợ). Hãy Explode / chuyển thành LINE + ARC trước.");
                }
            };
            foreach (Ent en0 in ents) add(en0, new Mat(), null, 0);
            SnapEndpoints(P, 0.02);
            // doan thang ngan hon dung sai noi dau mut (vd 0.008 mm o goc khia) bi gop thanh do dai 0 -> bo di de khong tao nut bac 4
            int tiny = P.Segs.RemoveAll(sg0 => sg0.T == 'L' && Dist(sg0.P, sg0.Q) < 1e-6);
            if (tiny > 0) P.Warnings.Add("Đã bỏ " + tiny + " đoạn thẳng quá ngắn (nhỏ hơn dung sai nối đầu mút 0.02 mm)");
            if (P.Marks.Count > 0 || P.Js.Count > 0) P.Points.RemoveAll(pt => P.Marks.Values.Concat(P.Js).Any(mk => Dist(mk, Pt(pt.X, pt.Y)) < 0.05));   // POINT trung moi diem danh dau J / LJ khong phai lo trang
            if (splineCount > 0) P.Warnings.Add("Đã chuyển " + splineCount + " spline thành " + splineSegs + " đoạn thẳng (sai số võng ≤ " + SplineTol.ToString("0.##", Inv) + " mm)");
            if (expanded > 0) P.Warnings.Add("Đã bung " + expanded + " khối (INSERT) thành đối tượng thường");
            foreach (var kv in ignored) P.Warnings.Add("Bỏ qua " + kv.Value + " " + kv.Key);
            return P;
        }
        // Gop cac dau mut cach nhau <= tol (khe ho rat nho do ve cung tiep tuyen) thanh 1 diem de bien dang khep kin
        static void SnapEndpoints(Prims P, double tol)
        {
            var list = new List<double[]>();   // moi phan tu: {x, y, segIndex*2 + (0=P|1=Q)}
            for (int i = 0; i < P.Segs.Count; i++) { list.Add(new[] { P.Segs[i].P[0], P.Segs[i].P[1], i * 2 }); list.Add(new[] { P.Segs[i].Q[0], P.Segs[i].Q[1], i * 2 + 1 }); }
            if (list.Count == 0) return;
            var ord = Enumerable.Range(0, list.Count).OrderBy(i => list[i][0]).ToArray();
            int[] par = new int[list.Count]; for (int i = 0; i < par.Length; i++) par[i] = i;
            Func<int, int> find = null; find = x => { while (par[x] != x) { par[x] = par[par[x]]; x = par[x]; } return x; };
            // Cac cap dau mut cach nhau <= tol, gop tu gan den xa. Khong gop neu nhom moi chua ca hai dau cua cung mot doan
            // (doan ngan hon dung sai, vd 0.008 mm o goc khia, phai giu nguyen khong bi thanh do dai 0)
            var pairs = new List<double[]>();
            for (int a = 0; a < ord.Length; a++)
                for (int b = a + 1; b < ord.Length && list[ord[b]][0] - list[ord[a]][0] <= tol; b++)
                {
                    double dx = list[ord[a]][0] - list[ord[b]][0], dy = list[ord[a]][1] - list[ord[b]][1];
                    // chi noi dau mut giua cac doan cung mau (kegaki khong bi dinh vao bien ngoai khi cach 0.01 mm, ca 26-065663)
                    if (dx * dx + dy * dy <= tol * tol && P.Segs[(int)list[ord[a]][2] / 2].Col == P.Segs[(int)list[ord[b]][2] / 2].Col) pairs.Add(new double[] { dx * dx + dy * dy, ord[a], ord[b] });
                }
            var members = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < list.Count; i++) members[i] = new HashSet<int> { (int)list[i][2] / 2 };
            foreach (var pr in pairs.OrderBy(q => q[0]))
            {
                int ra = find((int)pr[1]), rb = find((int)pr[2]);
                if (ra == rb) continue;
                var sa = members[ra]; var sb = members[rb];
                if (sa.Overlaps(sb)) continue;
                if (sa.Count + sb.Count > 2) continue;   // moi nut chi noi toi da 2 dau mut (khe ho joint 0.005 mm cua duong dan vao piercing cong khong bi lap kin, ca ViDu_AL_t19)
                par[ra] = rb; sb.UnionWith(sa);
            }
            var rep = new Dictionary<int, double[]>();
            for (int i = 0; i < list.Count; i++) { int r = find(i); if (!rep.ContainsKey(r)) rep[r] = new[] { list[i][0], list[i][1] }; }
            for (int i = 0; i < list.Count; i++)
            {
                var pt = rep[find(i)]; int si = (int)list[i][2] / 2;
                if (((int)list[i][2] & 1) == 0) P.Segs[si].P = pt; else P.Segs[si].Q = pt;
            }
        }

        // ---------- Hinh hoc ----------
        static Seg DirSeg(Seg s, bool rev)
        {
            Seg c = s.Copy();
            if (rev) { c.P = s.Q; c.Q = s.P; c.Ccw = !s.Ccw; }
            return c;
        }
        static List<Seg> ReverseLoop(List<Seg> L)
        {
            var o = new List<Seg>();
            for (int i = L.Count - 1; i >= 0; i--) o.Add(DirSeg(L[i], true));
            return o;
        }

        static double LoopArea(List<Seg> loop)
        {
            double s = 0;
            foreach (Seg g in loop) s += g.P[0] * g.Q[1] - g.Q[0] * g.P[1];
            s /= 2;
            foreach (Seg g in loop) if (g.T == 'A')
                {
                    double seg = g.R * g.R / 2 * (g.Sweep - Math.Sin(g.Sweep));
                    s += g.Ccw ? seg : -seg;
                }
            return s;
        }

        static List<double[]> ArcPoints(Seg g, int n)
        {
            double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]);
            double sw = g.Ccw ? g.Sweep : -g.Sweep;
            var o = new List<double[]>();
            for (int k = 0; k <= n; k++) o.Add(Pt(g.C[0] + g.R * Math.Cos(a0 + sw * k / n), g.C[1] + g.R * Math.Sin(a0 + sw * k / n)));
            return o;
        }

        class BB { public double X0 = double.MaxValue, Y0 = double.MaxValue, X1 = double.MinValue, Y1 = double.MinValue; }
        static BB LoopBBox(List<Seg> loop)
        {
            var b = new BB();
            Action<double[]> add = p => { b.X0 = Math.Min(b.X0, p[0]); b.X1 = Math.Max(b.X1, p[0]); b.Y0 = Math.Min(b.Y0, p[1]); b.Y1 = Math.Max(b.Y1, p[1]); };
            foreach (Seg g in loop)
            {
                add(g.P); add(g.Q);
                if (g.T == 'A')
                {
                    double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]);
                    for (int k = 0; k < 4; k++)
                    {
                        double ang = k * Math.PI / 2;
                        double d = g.Ccw ? Mod(ang - a0, TAU) : Mod(a0 - ang, TAU);
                        if (d > 1e-9 && d < g.Sweep - 1e-9) add(Pt(g.C[0] + g.R * Math.Cos(ang), g.C[1] + g.R * Math.Sin(ang)));
                    }
                }
            }
            return b;
        }

        static List<List<Seg>> GroupLoops(List<Seg> segs)
        {
            int[] par = new int[segs.Count];
            for (int i = 0; i < par.Length; i++) par[i] = i;
            Func<int, int> f = null;
            f = i => { while (par[i] != i) { par[i] = par[par[i]]; i = par[i]; } return i; };
            var first = new Dictionary<string, int>();
            for (int i = 0; i < segs.Count; i++)
                foreach (string k in new[] { Key(segs[i].P), Key(segs[i].Q) })
                {
                    int j;
                    if (first.TryGetValue(k, out j)) par[f(i)] = f(j); else first[k] = i;
                }
            var grp = new Dictionary<int, List<Seg>>(); var order = new List<int>();
            for (int i = 0; i < segs.Count; i++)
            {
                int r = f(i);
                if (!grp.ContainsKey(r)) { grp[r] = new List<Seg>(); order.Add(r); }
                grp[r].Add(segs[i]);
            }
            return order.Select(r => grp[r]).ToList();
        }

        static List<double[]> Endpoints(List<Seg> L)
        {
            var deg = new Dictionary<string, int>(); var pts = new Dictionary<string, double[]>(); var ord = new List<string>();
            foreach (Seg g in L)
                foreach (double[] p in new[] { g.P, g.Q })
                {
                    string k = Key(p); int n;
                    if (!deg.TryGetValue(k, out n)) { pts[k] = p; ord.Add(k); }
                    deg[k] = n + 1;
                }
            return ord.Where(k => deg[k] == 1).Select(k => pts[k]).ToList();
        }
        static bool IsOpen(List<Seg> L) { return Endpoints(L).Count > 0; }

        static List<Seg> Chain(List<Seg> segs)
        {
            var o = new List<Seg> { DirSeg(segs[0], false) };
            var used = new HashSet<int> { 0 };
            while (used.Count < segs.Count)
            {
                Seg last = o[o.Count - 1]; bool ok = false;
                for (int i = 0; i < segs.Count; i++)
                {
                    if (used.Contains(i)) continue;
                    Seg s = segs[i];
                    if (Key(s.P) == Key(last.Q)) o.Add(DirSeg(s, false));
                    else if (Key(s.Q) == Key(last.Q)) o.Add(DirSeg(s, true));
                    else continue;
                    used.Add(i); ok = true; break;
                }
                if (!ok) throw new NCError("Biên dạng bị hở hoặc rẽ nhánh tại (" + Fmt(last.Q[0]) + ", " + Fmt(last.Q[1]) + ")");
            }
            if (Key(o[0].P) != Key(o[o.Count - 1].Q)) throw new NCError("Biên dạng không kín");
            return LoopArea(o) > 0 ? ReverseLoop(o) : o;
        }

        static List<Seg> ChainOpenFrom(List<Seg> L, double[] start)
        {
            var seq = new List<Seg>(); double[] cur = start; var rest = new List<Seg>(L);
            while (rest.Count > 0)
            {
                bool ok = false;
                for (int i = 0; i < rest.Count; i++)
                {
                    Seg g = rest[i];
                    if (Key(g.P) == Key(cur)) { seq.Add(DirSeg(g, false)); cur = g.Q; }
                    else if (Key(g.Q) == Key(cur)) { seq.Add(DirSeg(g, true)); cur = g.P; }
                    else continue;
                    rest.RemoveAt(i); ok = true; break;
                }
                if (!ok) throw new NCError("Đường hở bị đứt tại (" + Fmt(cur[0]) + ", " + Fmt(cur[1]) + ")");
            }
            return seq;
        }

        static bool Inside(double[] pt, List<Seg> loop)
        {
            var pts = new List<double[]>();
            foreach (Seg g in loop)
            {
                if (g.T == 'A') { var a = ArcPoints(g, 12); a.RemoveAt(a.Count - 1); pts.AddRange(a); } else pts.Add(g.P);
            }
            double x = pt[0], y = pt[1]; bool c = false;
            for (int i = 0; i < pts.Count; i++)
            {
                double[] a = pts[i], b = pts[(i + 1) % pts.Count];
                if ((a[1] > y) != (b[1] > y) && x < a[0] + (y - a[1]) * (b[0] - a[0]) / (b[1] - a[1])) c = !c;
            }
            return c;
        }

        static bool OnSeg(Seg g, double[] pt, double tol)
        {
            if (g.T != 'L') return false;
            double L = Dist(g.P, g.Q); if (L < 1e-9) return false;
            double t = ((pt[0] - g.P[0]) * (g.Q[0] - g.P[0]) + (pt[1] - g.P[1]) * (g.Q[1] - g.P[1])) / (L * L);
            if (t < -1e-9 || t > 1 + 1e-9) return false;
            return Dist(pt, Pt(g.P[0] + t * (g.Q[0] - g.P[0]), g.P[1] + t * (g.Q[1] - g.P[1]))) < tol;
        }

        static double[] ExtendOut(double[] e, List<Seg> outer, double d)
        {
            foreach (Seg g in outer)
                if (OnSeg(g, e, 1e-3))
                {
                    double L = Dist(g.P, g.Q), nx = -(g.Q[1] - g.P[1]) / L, ny = (g.Q[0] - g.P[0]) / L;
                    return Pt(e[0] + nx * d, e[1] + ny * d);
                }
            foreach (Seg g in outer)   // dau chay tren cung bo goc cua bien ngoai cung tinh la cham bien
                if (g.T == 'A' && DistToSeg(g, e) < 1e-3)
                {
                    double ax = e[0] - g.C[0], ay = e[1] - g.C[1], L = Math.Sqrt(ax * ax + ay * ay);
                    return L < 1e-9 ? e : Pt(e[0] + ax / L * d, e[1] + ay / L * d);
                }
            return null;
        }

        class Split { public List<Seg> Loop; public double[] St; public double? R; public int Ax; }
        static Split SplitAtEntry(List<Seg> loop)
        {
            var bq = LoopBBox(loop);   // kich thuoc that (ke ca dinh cung) de xet lo nam ngang hay dung
            int ax = (bq.X1 - bq.X0) >= (bq.Y1 - bq.Y0) - 1e-3 ? 0 : 1;
            double target = ax == 0 ? Math.PI : -Math.PI / 2;
            double bk1 = 0; int bk2 = 0, bi = -1, bkind = 0; double[] bpt = null; double bd = 0;
            for (int i = 0; i < loop.Count; i++)
            {
                Seg g = loop[i];
                var cand = new List<object[]> { new object[] { g.P, 0, 0.0 } };   // pt, kind(0 vertex,1 arc split,2 mid), d
                if (g.T == 'A')
                {
                    double ap = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]);
                    double d = g.Ccw ? Mod(target - ap, TAU) : Mod(ap - target, TAU);
                    if (d * g.R > 1e-3 && (g.Sweep - d) * g.R > 1e-3) cand.Add(new object[] { Pt(g.C[0] + g.R * Math.Cos(target), g.C[1] + g.R * Math.Sin(target)), 1, d });
                }
                else if ((ax == 0 && Math.Abs(g.P[0] - g.Q[0]) < 1e-3 && Math.Abs(g.P[1] - g.Q[1]) > 1e-3) || (ax == 1 && Math.Abs(g.P[1] - g.Q[1]) < 1e-3 && Math.Abs(g.P[0] - g.Q[0]) > 1e-3))
                    cand.Add(new object[] { Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), 2, 0.0 });
                foreach (object[] c in cand)
                {
                    double[] pt = (double[])c[0]; int kind = (int)c[1];
                    double k1 = Math.Round(pt[ax] * 100) / 100; int k2 = kind != 0 ? 0 : 1;
                    if (bi < 0 || k1 < bk1 || (k1 == bk1 && k2 < bk2)) { bk1 = k1; bk2 = k2; bi = i; bpt = pt; bkind = kind; bd = (double)c[2]; }
                }
            }
            Seg gg = loop[bi]; var res = new Split { St = bpt, Ax = ax };
            var o = new List<Seg>();
            Func<int, int, IEnumerable<Seg>> rng = (a, b) => loop.Skip(a).Take(b - a);
            if (bkind == 0) { o.AddRange(loop.Skip(bi)); o.AddRange(loop.Take(bi)); }
            else if (gg.T == 'A')
            {
                Seg g1 = gg.Copy(); g1.Q = bpt; g1.Sweep = bd;
                Seg g2 = gg.Copy(); g2.P = bpt; g2.Sweep = gg.Sweep - bd;
                o.Add(g2); o.AddRange(loop.Skip(bi + 1)); o.AddRange(loop.Take(bi)); o.Add(g1); res.R = gg.R;
            }
            else
            {
                Seg g1 = gg.Copy(); g1.Q = bpt; Seg g2 = gg.Copy(); g2.P = bpt;
                o.Add(g2); o.AddRange(loop.Skip(bi + 1)); o.AddRange(loop.Take(bi)); o.Add(g1);
            }
            res.Loop = o;
            return res;
        }

        // Bien dang de kiem tra khoang cach joint (bien ngoai, lo khac)
        class Contour { public object Owner; public List<Seg> Loop; public double[] C; public double R; public bool IsCircle; }

        static double DistToSeg(Seg g, double[] p)
        {
            if (g.T == 'L')
            {
                double dx = g.Q[0] - g.P[0], dy = g.Q[1] - g.P[1], l2 = dx * dx + dy * dy;
                double t = l2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, ((p[0] - g.P[0]) * dx + (p[1] - g.P[1]) * dy) / l2));
                return Dist(p, Pt(g.P[0] + t * dx, g.P[1] + t * dy));
            }
            double ap = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ang = Math.Atan2(p[1] - g.C[1], p[0] - g.C[0]);
            double d = g.Ccw ? Mod(ang - ap, TAU) : Mod(ap - ang, TAU);
            if (d <= g.Sweep) return Math.Abs(Dist(p, g.C) - g.R);
            return Math.Min(Dist(p, g.P), Dist(p, g.Q));
        }

        static double MinDist(List<Contour> cs, object self, double[] p)
        {
            double best = double.MaxValue;
            foreach (var k in cs)
            {
                if (ReferenceEquals(k.Owner, self)) continue;
                double d = k.IsCircle ? Math.Abs(Dist(p, k.C) - k.R) : k.Loop.Min(g => DistToSeg(g, p));
                if (d < best) best = d;
            }
            return best;
        }

        class Pick { public double[] Pt, Dir; public double? R; public int Tier; }

        // Huong cua diem so voi tam lo: 0 trai, 1 phai, 2 duoi, 3 tren (thu tu uu tien khi chon vi tri joint)
        static int DirRank(double[] pt, double cx, double cy)
        {
            double dx = pt[0] - cx, dy = pt[1] - cy;
            if (Math.Abs(dx) >= Math.Abs(dy)) return dx > 0 ? 1 : 0;
            return dy < 0 ? 2 : 3;
        }

        // Danh sach diem vao theo thu tu uu tien: (1) nua tron / mui nhon loi (tong goc >= 150 do), ban kinh nho truoc,
        // cung ban kinh thi lo ngang: trai nhat, lo dung: thap nhat; (2) tam giac: giua canh thang dai nhat;
        // (3) quy tac cu (trai nhat / thap nhat); (4) trung diem cac canh va cung (dung khi can xoay joint)
        static List<Pick> EntryCandidates(List<Seg> loop, bool vertTop)
        {
            var res = new List<Pick>(); int n = loop.Count; if (n == 0) return res;
            Action<Pick> add = pk => { if (!res.Any(x => Dist(x.Pt, pk.Pt) < 1e-6)) res.Add(pk); };
            Func<Seg, Seg, bool> cont = (a, b) => a.T == 'A' && b.T == 'A' && a.Ccw == b.Ccw && Dist(a.C, b.C) < 1e-3 && Math.Abs(a.R - b.R) < 1e-3;
            int start = 0;
            for (int i = 0; i < n; i++) if (!cont(loop[(i + n - 1) % n], loop[i])) { start = i; break; }
            var groups = new List<List<Seg>>();
            for (int k = 0; k < n; k++)
            {
                Seg g = loop[(start + k) % n];
                if (groups.Count > 0 && cont(groups[groups.Count - 1][groups[groups.Count - 1].Count - 1], g)) groups[groups.Count - 1].Add(g);
                else groups.Add(new List<Seg> { g });
            }
            var bb = LoopBBox(loop); bool horiz = (bb.X1 - bb.X0) >= (bb.Y1 - bb.Y0) - 1e-3;
            var caps = groups.Where(gr => gr[0].T == 'A' && gr[0].Ccw && gr.Sum(x => x.Sweep) >= 150 * Math.PI / 180 - 1e-6 && gr.Sum(x => x.Sweep) < TAU - 1e-6)
                .Select(gr =>
                {
                    double a0 = Math.Atan2(gr[0].P[1] - gr[0].C[1], gr[0].P[0] - gr[0].C[0]), sw = gr.Sum(x => x.Sweep), r = gr[0].R;
                    return new { r, x = gr[0].C[0] + r * Math.Cos(a0 + sw / 2), y = gr[0].C[1] + r * Math.Sin(a0 + sw / 2), cx = gr[0].C[0], cy = gr[0].C[1] };
                }).ToList();
            var ordered = horiz ? caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.x, 4)).ThenBy(c => c.y)
                                : (vertTop ? caps.OrderBy(c => Math.Round(c.r, 3)).ThenByDescending(c => Math.Round(c.y, 4)).ThenBy(c => c.x)   // lo dung khong joint: dau tren truoc (ca 26-067424)
                                           : caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.y, 4)).ThenBy(c => c.x));   // lo dung co joint: dau duoi truoc (Vi_Du_1)
            var radii = ordered.Select(c => Math.Round(c.r, 3)).Distinct().ToList();
            foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r, Tier = radii.IndexOf(Math.Round(c.r, 3)) });
            bool tri = false;
            if (caps.Count == 0)
            {
                var lines = loop.Where(g => g.T == 'L').ToList();
                if (lines.Count >= 3)
                {
                    double maxLen = lines.Max(g => Dist(g.P, g.Q));
                    var longs = lines.Where(g => Dist(g.P, g.Q) >= 0.2 * maxLen).ToList();
                    if (longs.Count == 3)
                    {
                        Seg g = longs.OrderByDescending(x => Dist(x.P, x.Q)).First(); double len = Dist(g.P, g.Q);
                        add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 10 });
                        tri = true;
                    }
                }
            }
            // Lo nghieng: toan canh thang, khong co canh nao song song Ox/Oy, khong co phan nho -> vao giua canh bat dau tu dinh thap nhat (theo chieu cat nguoc kim dong ho)
            if (!tri && caps.Count == 0 && loop.All(g => g.T == 'L' && Math.Abs(g.P[0] - g.Q[0]) > 1e-3 && Math.Abs(g.P[1] - g.Q[1]) > 1e-3))
            {
                // canh ngan nhat (sai so 0.01), bang nhau thi canh thap nhat (trung diem Y nho nhat) - ca 26-062754 va 26-065394
                int lo = 0; double bestLen = double.MaxValue, bestMidY = double.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    double li = Dist(loop[i].P, loop[i].Q), my = (loop[i].P[1] + loop[i].Q[1]) / 2;
                    if (li < bestLen - 0.01 || (Math.Abs(li - bestLen) <= 0.01 && my < bestMidY)) { lo = i; bestLen = li; bestMidY = my; }
                }
                Seg g = loop[lo]; double len = Dist(g.P, g.Q);
                add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 10 });
            }
            var fb = SplitAtEntry(loop);
            add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R, Tier = 11 });
            foreach (Seg g in loop.OrderByDescending(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep))
            {
                if (g.T == 'L')
                {
                    double len = Dist(g.P, g.Q); if (len < 1e-6) continue;
                    add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 12 });
                }
                else
                {
                    double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), am = a0 + (g.Ccw ? 1 : -1) * g.Sweep / 2;
                    double mx = g.C[0] + g.R * Math.Cos(am), my = g.C[1] + g.R * Math.Sin(am);
                    add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 12 });
                }
            }
            return res;
        }

        static string IJ(double i, double j)
        {
            string w = "";
            if (Math.Abs(Math.Round(i * 1000)) > 0) w += "I" + Fmt(i);
            if (Math.Abs(Math.Round(j * 1000)) > 0) w += "J" + Fmt(j);
            return w;
        }
        // Xoay vong (CCW) de bat dau tai diem pt tren bien dang; cat doi cung / doan thang neu pt nam giua
        static Split SplitAtPoint(List<Seg> loop, double[] pt)
        {
            for (int i = 0; i < loop.Count; i++)
            {
                Seg g = loop[i];
                if (Dist(g.P, pt) < 1e-3) return new Split { Loop = loop.Skip(i).Concat(loop.Take(i)).ToList(), St = g.P, Ax = 0 };   // trung dinh: khong cat thanh doan do dai ~0
                if (g.T == 'A')
                {
                    if (Math.Abs(Dist(pt, g.C) - g.R) > 1e-3) continue;
                    double ap = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ang = Math.Atan2(pt[1] - g.C[1], pt[0] - g.C[0]);
                    double d = g.Ccw ? Mod(ang - ap, TAU) : Mod(ap - ang, TAU);
                    if (d * g.R > 1e-3 && (g.Sweep - d) * g.R > 1e-3)
                    {
                        Seg g1 = g.Copy(); g1.Q = pt; g1.Sweep = d; Seg g2 = g.Copy(); g2.P = pt; g2.Sweep = g.Sweep - d;
                        var o = new List<Seg> { g2 }; o.AddRange(loop.Skip(i + 1)); o.AddRange(loop.Take(i)); o.Add(g1);
                        return new Split { Loop = o, St = pt, R = g.R, Ax = 0 };
                    }
                }
                else if (OnSeg(g, pt, 1e-3))
                {
                    Seg g1 = g.Copy(); g1.Q = pt; Seg g2 = g.Copy(); g2.P = pt;
                    var o = new List<Seg> { g2 }; o.AddRange(loop.Skip(i + 1)); o.AddRange(loop.Take(i)); o.Add(g1);
                    return new Split { Loop = o, St = pt, Ax = 0 };
                }
            }
            return SplitAtEntry(loop);
        }

        // Phoi dai hep co marker J/LJ: bien ngoai (CW) tach 2 duong cat; moi duong bat dau tai marker M (duc ngoai, vuong goc voi canh)
        // va dung truoc marker E mot doan joint ngoai
        static void SplitPath(List<Seg> sg, double[] M, double[] E, double joint, double pierceOut, out double[] pierce, out List<Seg> seq)
        {
            int n = sg.Count, iM = -1;
            for (int i = 0; i < n; i++) if (OnSeg(sg[i], M, 1e-3)) { iM = i; break; }
            if (iM < 0) throw new NCError("Marker J/LJ không nằm trên cạnh thẳng của biên dạng ngoài");
            Seg g = sg[iM]; double dx = g.Q[0] - g.P[0], dy = g.Q[1] - g.P[1], ln = Math.Sqrt(dx * dx + dy * dy);
            double[] nrm = Pt(-dy / ln, dx / ln);   // phap tuyen ngoai (CW -> ben trai huong chay)
            var all = new List<Seg>(); Seg first = g.Copy(); first.P = M; all.Add(first);
            for (int k = 1; k < n; k++) all.Add(sg[(iM + k) % n]);
            Seg lastS = g.Copy(); lastS.Q = M; all.Add(lastS);
            all.RemoveAll(x => x.T == 'L' && Dist(x.P, x.Q) < 1e-6);
            var o = new List<Seg>();
            foreach (var x in all)
            {
                if (OnSeg(x, E, 1e-3)) { Seg cp = x.Copy(); cp.Q = E; o.Add(cp); break; }
                o.Add(x);
            }
            Seg ls = o[o.Count - 1];
            if (ls.T != 'L' || Dist(ls.P, ls.Q) <= joint) throw new NCError("Marker J/LJ: đoạn cuối trước joint không phải đoạn thẳng đủ dài");
            double ux = ls.Q[0] - ls.P[0], uy = ls.Q[1] - ls.P[1], l2 = Math.Sqrt(ux * ux + uy * uy);
            Seg lc = ls.Copy(); lc.Q = Pt(E[0] - ux / l2 * joint, E[1] - uy / l2 * joint); o[o.Count - 1] = lc;
            pierce = Pt(M[0] + nrm[0] * pierceOut, M[1] + nrm[1] * pierceOut); seq = o;
        }

        // Vi tri (chieu dai cung tinh tu dau bien dang) cua diem M nam tren canh thang cua bien ngoai
        static double PathPos(List<Seg> sg, double[] M)
        {
            double cum = 0;
            foreach (Seg g in sg)
            {
                if (OnSeg(g, M, 1e-3)) return cum + Dist(g.P, M);
                cum += g.T == 'L' ? Dist(g.P, g.Q) : g.R * g.Sweep;
            }
            throw new NCError("Marker J/LJ không nằm trên cạnh thẳng của biên dạng ngoài");
        }

        // Giao cua duong thang keo dai cua doan thang `line` voi doan ke `other` (thang hoac cung); chon giao diem gan `near` nhat
        static double[] VirtualVertex(Seg line, Seg other, double[] near)
        {
            double dx = line.Q[0] - line.P[0], dy = line.Q[1] - line.P[1];
            var cands = new List<double[]>();
            if (other.T == 'L')
            {
                double ex = other.Q[0] - other.P[0], ey = other.Q[1] - other.P[1], den = dx * ey - dy * ex;
                if (Math.Abs(den) < 1e-12) return null;
                double t = ((other.P[0] - line.P[0]) * ey - (other.P[1] - line.P[1]) * ex) / den;
                cands.Add(Pt(line.P[0] + t * dx, line.P[1] + t * dy));
            }
            else
            {
                double fx = line.P[0] - other.C[0], fy = line.P[1] - other.C[1];
                double a = dx * dx + dy * dy, b = 2 * (dx * fx + dy * fy), c = fx * fx + fy * fy - other.R * other.R, disc = b * b - 4 * a * c;
                if (disc < 0) return null;
                double sq = Math.Sqrt(disc);
                foreach (double t in new[] { (-b - sq) / (2 * a), (-b + sq) / (2 * a) }) cands.Add(Pt(line.P[0] + t * dx, line.P[1] + t * dy));
            }
            return cands.OrderBy(q => Dist(q, near)).First();
        }

        // Huong tiep tuyen tai dau/cuoi doan (theo chieu di cua doan)
        static double[] TanAt(Seg g, bool atEnd)
        {
            if (g.T == 'L') { double l = Dist(g.P, g.Q); return l < 1e-9 ? Pt(1, 0) : Pt((g.Q[0] - g.P[0]) / l, (g.Q[1] - g.P[1]) / l); }
            double[] p = atEnd ? g.Q : g.P; double phi = Math.Atan2(p[1] - g.C[1], p[0] - g.C[0]);
            return g.Ccw ? Pt(-Math.Sin(phi), Math.Cos(phi)) : Pt(Math.Sin(phi), -Math.Cos(phi));
        }

        // Cac dinh noi hai doan khong tiep tuyen (goc nhon / goc chua bo R): lech huong > 0.5 do
        static List<double[]> SharpCorners(List<Seg> loop)
        {
            var res = new List<double[]>();
            var L = loop.Where(g => !(g.T == 'L' && Dist(g.P, g.Q) < 1e-9)).ToList(); int n = L.Count;
            for (int i = 0; i < n; i++)
            {
                Seg a = L[i], b = L[(i + 1) % n];
                double[] ta = TanAt(a, true), tb = TanAt(b, false);
                if (ta[0] * tb[0] + ta[1] * tb[1] < Math.Cos(0.5 * Math.PI / 180)) res.Add(a.Q);
            }
            return res;
        }

        // Lo dai cong (rang cong): hai dau bo tron (cung ~180 do, cung ban kinh) noi bang hai canh bat ky. Chieu dai truc = trung binh do dai hai canh
        // (lo dai thang: bang tam-tam). Dung de xet joint lo: truc >= 10
        static bool SlotLength(List<Seg> loop, out double len)
        {
            len = 0; int n = loop.Count; if (n < 4) return false;
            Func<Seg, Seg, bool> cont = (a, b) => a.T == 'A' && b.T == 'A' && a.Ccw == b.Ccw && Dist(a.C, b.C) < 1e-3 && Math.Abs(a.R - b.R) < 1e-3;
            int start = -1;
            for (int i = 0; i < n; i++) if (!cont(loop[(i + n - 1) % n], loop[i])) { start = i; break; }
            if (start < 0) return false;
            var groups = new List<List<Seg>>();
            for (int k = 0; k < n; k++)
            {
                Seg g = loop[(start + k) % n];
                if (groups.Count > 0 && cont(groups[groups.Count - 1][groups[groups.Count - 1].Count - 1], g)) groups[groups.Count - 1].Add(g);
                else groups.Add(new List<Seg> { g });
            }
            if (groups.Count != 4) return false;
            Func<List<Seg>, bool> isCap = gr => { double sw = gr.Where(x => x.T == 'A').Sum(x => x.Sweep); return gr[0].T == 'A' && gr[0].Ccw && sw >= 150 * Math.PI / 180 && sw <= 210 * Math.PI / 180; };
            Func<List<Seg>, double> glen = gr => gr.Sum(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep);
            int c0 = (isCap(groups[0]) && isCap(groups[2])) ? 0 : ((isCap(groups[1]) && isCap(groups[3])) ? 1 : -1);
            if (c0 < 0) return false;
            if (Math.Abs(groups[c0][0].R - groups[c0 + 2][0].R) > 0.01) return false;
            len = (glen(groups[(c0 + 1) % 4]) + glen(groups[(c0 + 3) % 4])) / 2;
            return true;
        }

        static bool IsOblong(List<Seg> L, out double cc)
        {
            cc = 0;
            if (L.Count != 4) return false;
            var arcs = L.Where(g => g.T == 'A').ToList(); var lines = L.Where(g => g.T == 'L').ToList();
            if (arcs.Count != 2 || lines.Count != 2) return false;
            if (Math.Abs(arcs[0].R - arcs[1].R) > 1e-3) return false;
            if (arcs.Any(a => Math.Abs(a.Sweep - Math.PI) > 1e-3)) return false;
            cc = Dist(arcs[0].C, arcs[1].C); return true;
        }

        // Cat ngan duong chay mot doan j tu cuoi (de chua joint)
        static List<Seg> TrimEnd(List<Seg> segs, double j)
        {
            if (j <= 0) return segs;
            var o = new List<Seg>(segs); double rem = j;
            while (rem > 1e-9 && o.Count > 0)
            {
                Seg g = o[o.Count - 1].Copy(); double len = g.T == 'L' ? Dist(g.P, g.Q) : g.R * g.Sweep;
                if (len <= rem + 1e-9) { o.RemoveAt(o.Count - 1); rem -= len; continue; }
                if (g.T == 'L') { double k = (len - rem) / len; g.Q = Pt(g.P[0] + (g.Q[0] - g.P[0]) * k, g.P[1] + (g.Q[1] - g.P[1]) * k); }
                else
                {
                    double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ns = g.Sweep - rem / g.R, sgn = g.Ccw ? 1 : -1;
                    g.Q = Pt(g.C[0] + g.R * Math.Cos(a0 + sgn * ns), g.C[1] + g.R * Math.Sin(a0 + sgn * ns)); g.Sweep = ns;
                }
                o[o.Count - 1] = g; rem = 0;
            }
            return o;
        }

        // ---------- Bien doi ----------
        static Prims MapPrims(Prims P, Func<double[], double[]> f)
        {
            var o = new Prims { Warnings = P.Warnings };
            foreach (var kv in P.Marks) o.Marks[kv.Key] = f(kv.Value);
            foreach (var j in P.Js) o.Js.Add(f(j));
            foreach (var h in P.Hull) o.Hull.Add(f(h));
            foreach (Seg s in P.Segs) { Seg c = s.Copy(); c.P = f(s.P); c.Q = f(s.Q); if (s.C != null) c.C = f(s.C); o.Segs.Add(c); }
            foreach (Circ c in P.Circles) { var q = f(Pt(c.Cx, c.Cy)); o.Circles.Add(new Circ { Cx = q[0], Cy = q[1], R = c.R, Col = c.Col }); }
            foreach (Pnt p in P.Points) { var q = f(Pt(p.X, p.Y)); o.Points.Add(new Pnt { X = q[0], Y = q[1], Col = p.Col }); }
            return o;
        }
        static Prims Rotate(Prims P, int k)
        {
            if (k == 0) return P;
            return MapPrims(P, p => { double x = p[0], y = p[1]; for (int i = 0; i < k; i++) { double t = x; x = -y; y = t; } return Pt(x, y); });
        }
        static Prims Translate(Prims P, double dx, double dy) { return MapPrims(P, p => Pt(p[0] + dx, p[1] + dy)); }

        class Analysis { public List<Seg> Outer; public BB Bb; public List<List<Seg>> Holes, Opens; public Circ OuterCircle; public bool Drawn; }
        // Day >= 19: DXF co the da ve san duong dan vao piercing cong (2 doan thang + cung R3) va khe ho joint -> bien ngoai la duong ho, cat dung theo duong ve
        static bool DrawnLeadMode;
        static Analysis Analyze(Prims P)
        {
            var baseSegs = P.Segs.Where(s => s.Col == "L").ToList();
            var loops = GroupLoops(baseSegs);
            // Biên dạng/lỗ Layer 0 by layer phải khép kín; đường hở chỉ hợp lệ với màu xanh (khe cắt) hoặc đỏ (kẻ dấu)
            var openLoops = loops.Where(IsOpen).ToList();
            if (DrawnLeadMode && openLoops.Count == 1 && Endpoints(openLoops[0]).Count == 2)
            {
                var open = openLoops[0]; var oe = Endpoints(open); var ob = LoopBBox(open);
                double ocx = (ob.X0 + ob.X1) / 2, ocy = (ob.Y0 + ob.Y1) / 2;
                var ostart = oe.OrderByDescending(e => Dist(e, Pt(ocx, ocy))).First();   // dau tu do xa tam nhat = dau duong dan vao
                var chainO = ChainOpenFrom(open, ostart);
                int lead = 0;   // doan dan vao: cac doan dau chuoi toi het cung nho dau tien (trong 4 doan dau)
                for (int i = 0; i < Math.Min(4, chainO.Count); i++) if (chainO[i].T == 'A' && chainO[i].R <= 5 + 1e-9) { lead = i + 1; break; }
                var partO = chainO.Skip(lead).ToList(); if (partO.Count == 0) partO = chainO;
                var holesO = loops.Where(L => !IsOpen(L)).OrderByDescending(L => { var b = LoopBBox(L); return (b.X1 - b.X0) * (b.Y1 - b.Y0); }).ToList();
                return new Analysis { Outer = chainO, Bb = LoopBBox(partO), Holes = holesO, Opens = new List<List<Seg>>(), Drawn = true };
            }
            if (openLoops.Count > 0)
            {
                var ends = openLoops.SelectMany(L => Endpoints(L)).Take(6).Select(p => "(" + Fmt(p[0]) + ", " + Fmt(p[1]) + ")").ToList();
                throw new NCError("Biên dạng hoặc lỗ bị hở (LINE/ARC Layer 0, by layer chưa khép kín) tại " + string.Join(", ", ends) + ". Hãy nối kín đường biên trong DXF.");
            }
            var closed = loops.Where(L => !IsOpen(L)).ToList();
            Func<List<Seg>, double> area = L => { var b = LoopBBox(L); return (b.X1 - b.X0) * (b.Y1 - b.Y0); };
            closed = closed.OrderByDescending(area).ToList();
            // phoi hinh tron: CIRCLE by layer lon nhat (lon hon moi vong kin LINE/ARC) la bien ngoai
            var bigC = P.Circles.Where(c => c.Col == "L").OrderByDescending(c => c.R).FirstOrDefault();
            if (bigC != null && (closed.Count == 0 || 4 * bigC.R * bigC.R > area(closed[0])))
            {
                var half = new List<Seg> {
                    new Seg { T = 'A', C = Pt(bigC.Cx, bigC.Cy), R = bigC.R, P = Pt(bigC.Cx + bigC.R, bigC.Cy), Q = Pt(bigC.Cx - bigC.R, bigC.Cy), Sweep = Math.PI, Col = "L" },
                    new Seg { T = 'A', C = Pt(bigC.Cx, bigC.Cy), R = bigC.R, P = Pt(bigC.Cx - bigC.R, bigC.Cy), Q = Pt(bigC.Cx + bigC.R, bigC.Cy), Sweep = Math.PI, Col = "L" } };
                var oc = Chain(half);
                return new Analysis { Outer = oc, Bb = LoopBBox(oc), Holes = closed, Opens = loops.Where(IsOpen).ToList(), OuterCircle = bigC };
            }
            if (closed.Count == 0) throw new NCError("Không tìm thấy biên dạng ngoài kín (LINE/ARC hoặc CIRCLE Layer 0, by layer)");
            var outer = Chain(closed[0]);
            return new Analysis { Outer = outer, Bb = LoopBBox(outer), Holes = closed.Skip(1).ToList(), Opens = loops.Where(IsOpen).ToList() };
        }

        // ---------- Tao NC ----------
        class Op
        {
            public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle, HasJoint; public double Area, Joint, Cx, Cy, Rad, StartAng;
        }

        // Khach hang dac biet (cot 得意先 trong Excel). 824: khong tao joint cho lo ben trong (moi vat lieu, ke ca SUS, AL, CU)
        public static bool NoHoleJointCustomer(string customer) { return !string.IsNullOrEmpty(customer) && Regex.IsMatch(customer, @"^\s*0*824(\D|$)"); }

        public static Result Generate(Prims prims0, string name, string group, double t) { return Generate(prims0, name, group, t, null); }

        public static Result Generate(Prims prims0, string name, string group, double t, string customer)
        {
            double pierceOut, jointOut, pierceHole;
            Params(group, t, out pierceOut, out jointOut, out pierceHole);
            double jointHole = (group.ToUpperInvariant() == "SUS" && t >= 19 - 1e-9) ? 1.0 : (group.ToUpperInvariant() == "AL" && t >= 19 - 1e-9) ? 0 : jointOut;   // do rong joint lo (SUS >= 19: 1.0, AL >= 19: 0)
            var warnings = new List<string>(prims0.Warnings);
            if (NoHoleJointCustomer(customer)) { jointHole = 0; warnings.Add("Khách hàng 824: không tạo joint cho lỗ bên trong"); }
            bool isSS = group.ToUpperInvariant() == "SS";
            DrawnLeadMode = t >= 19 - 1e-9;

            // Thep SS day >= 19: moi goc (trong va ngoai, ke ca mat vat) phai duoc bo R, khong con goc nhon
            if (Curved(group, t) && !Analyze(prims0).Drawn)
            {
                var A1 = Analyze(prims0);
                var loops1 = new List<List<Seg>> { A1.Outer };
                foreach (var Lh in A1.Holes) loops1.Add(Chain(Lh));
                foreach (var Lg in GroupLoops(prims0.Segs.Where(s => s.Col == "green").ToList())) if (!IsOpen(Lg)) loops1.Add(Chain(Lg));
                var sharp = new List<double[]>();
                foreach (var lp in loops1) sharp.AddRange(SharpCorners(lp));
                if (sharp.Count > 0)
                    throw new NCError("Thép (SS) dày " + t.ToString("0.##", Inv) + " phải bo R3 cả góc trong và ngoài, nhưng DXF còn " + sharp.Count + " góc nhọn tại " +
                        string.Join(", ", sharp.Take(6).Select(p => "(" + Fmt(p[0]) + ", " + Fmt(p[1]) + ")")) + (sharp.Count > 6 ? ", ..." : "") + ". Hãy bo R3 các góc này trong DXF.");
            }

            // Phoi dai hep (canh ngan <= 100, canh dai >= 1000) bat buoc phai co diem danh dau J va LJ
            {
                var bb0 = Analyze(prims0).Bb; double w0 = bb0.X1 - bb0.X0, h0 = bb0.Y1 - bb0.Y0;
                if (Math.Min(w0, h0) <= 100 && Math.Max(w0, h0) >= 1000)
                {
                    var miss = new List<string>();
                    if (prims0.Js.Count == 0) miss.Add("J");
                    if (!prims0.Marks.ContainsKey("LJ")) miss.Add("LJ");
                    if (miss.Count > 0) throw new NCError("Phôi dài hẹp (" + w0.ToString("0.#", Inv) + " × " + h0.ToString("0.#", Inv) + ") thiếu điểm đánh dấu " + string.Join(" và ", miss) + ". Hãy thêm TEXT \"J\" và \"LJ\" (kèm POINT) trên cạnh dài trong DXF.");
                }
            }

            Prims P = null; Analysis A = null; int chRot = 0; double[] chShift = new double[] { 0, 0 };
            // Bien ngoai co SPLINE hoac CUNG loi ra trai nhat (khong co canh thang dung sat diem trai nhat) va co canh day nam tren Ox:
            // khong xoay, dat goc theo bao diem dieu khien, duc o canh day (ca ViDu_Spline_1)
            bool splBottom = false;
            {   // Duong bien ngoai da ve san duong dan vao piercing cong (day >= 19): khong xoay, khong xet canh trai, cat dung theo duong ve
                var aD = Analyze(prims0);
                if (aD.Drawn)
                {
                    double[] shiftD = new double[] { 0, 0 }; Prims PrD = prims0;
                    if (Math.Abs(aD.Bb.X0) > 1e-4 || Math.Abs(aD.Bb.Y0) > 1e-4) { shiftD = new[] { -aD.Bb.X0, -aD.Bb.Y0 }; PrD = Translate(prims0, shiftD[0], shiftD[1]); }
                    P = PrD; A = Analyze(PrD); chShift = shiftD;
                    warnings.Add("Biên ngoài là đường hở có sẵn đường dẫn vào piercing cong (dày ≥ 19): không xoay phôi, đục tại đầu tự do và cắt đúng theo đường vẽ");
                }
            }
            if (A == null && !(prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ")))
            {
                var a0 = Analyze(prims0);
                if (a0.OuterCircle == null)
                {
                    double x0 = a0.Bb.X0, y0 = a0.Bb.Y0;
                    var atLeft = a0.Outer.Where(g => LoopBBox(new List<Seg> { g }).X0 <= x0 + 1e-2).ToList();
                    bool hasBottom = a0.Outer.Any(g => !g.Spl && g.T == 'L' && Math.Abs(g.P[1] - y0) < 1e-2 && Math.Abs(g.Q[1] - y0) < 1e-2 && Math.Abs(g.P[0] - g.Q[0]) >= 10);
                    if (atLeft.Count > 0 && !atLeft.Any(g => g.T == 'L' && !g.Spl && Math.Abs(g.P[0] - x0) < 1e-2 && Math.Abs(g.Q[0] - x0) < 1e-2 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6) && hasBottom)
                    {
                        double hx = x0, hy = y0;
                        foreach (var h in prims0.Hull) { hx = Math.Min(hx, h[0]); hy = Math.Min(hy, h[1]); }
                        double[] shift = new double[] { 0, 0 }; Prims Pr = prims0;
                        if (Math.Abs(hx) > 1e-4 || Math.Abs(hy) > 1e-4) { shift = new[] { -hx, -hy }; Pr = Translate(prims0, shift[0], shift[1]); }
                        P = Pr; A = Analyze(Pr); chShift = shift; splBottom = true;
                        warnings.Add("Biên ngoài không có cạnh thẳng đứng chạm điểm trái nhất (trái là đường cong hoặc cạnh xiên): không xoay phôi, đục ở cạnh đáy nằm trên Ox");
                    }
                }
            }
            // Khong tu xoay hinh (tu 14-11-2026): DXF the nao thi giu nguyen huong the do. Chi thu huong goc (rot = 0);
            // khong co canh thang dung sat Oy thi dung quy tac phoi cong / xien ben duoi (noLeft)
            for (int rot = 0; rot < 1 && A == null; rot++)
            {
                Prims Pr = Rotate(prims0, rot); Analysis An = Analyze(Pr); double[] shift = new double[] { 0, 0 };
                if (Math.Abs(An.Bb.X0) > 1e-4 || Math.Abs(An.Bb.Y0) > 1e-4)
                {
                    shift = new[] { -An.Bb.X0, -An.Bb.Y0 }; Pr = Translate(Pr, shift[0], shift[1]); An = Analyze(Pr);
                }
                if (prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ") || An.OuterCircle != null || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-2 && Math.Abs(g.Q[0]) < 1e-2 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
                { P = Pr; A = An; chRot = rot; chShift = shift; }
            }
            bool noLeft = false;
            if (A == null)   // phoi cong / xien: khong co canh thang dung sat Oy o moi huong xoay -> giu nguyen huong, dung quy tac "canh phia duoi dinh trai nhat"
            {
                Prims Pr = prims0; Analysis An = Analyze(Pr); double[] shift = new double[] { 0, 0 };
                if (Math.Abs(An.Bb.X0) > 1e-4 || Math.Abs(An.Bb.Y0) > 1e-4) { shift = new[] { -An.Bb.X0, -An.Bb.Y0 }; Pr = Translate(Pr, shift[0], shift[1]); An = Analyze(Pr); }
                P = Pr; A = An; chRot = 0; chShift = shift; noLeft = true;
            }
            if (chRot != 0) warnings.Add("Đã xoay hình " + (chRot * 90) + "° (ngược chiều kim đồng hồ) để có cạnh thẳng sát Oy");
            if (Math.Abs(chShift[0]) > 1e-2 || Math.Abs(chShift[1]) > 1e-2)
                warnings.Add("Đã dời gốc tọa độ (" + Fmt(chShift[0]) + ", " + Fmt(chShift[1]) + ") để phôi nằm sát Ox/Oy");
            var sg = A.Outer; BB bb = A.Bb;

            var pts16 = new List<double[]>(); var ptsG = new List<double[]>();
            var kegaki = new List<Op>(); var holes = new List<Op>(); var slitLoops = new List<List<Seg>>();
            foreach (Pnt p in P.Points)
            {
                if (p.Col == "L") pts16.Add(Pt(p.X, p.Y));
                else if (p.Col == "green") ptsG.Add(Pt(p.X, p.Y));
            }
            if (P.Points.Any(p => p.Col == "red")) warnings.Add("POINT màu đỏ: không có quy tắc, đã bỏ qua");

            // bien dang dung de kiem tra khoang cach joint lo (bien ngoai + cac lo)
            var contours = new List<Contour> { new Contour { Owner = "outer", Loop = sg } };
            foreach (var L in A.Holes) contours.Add(new Contour { Owner = L, Loop = Chain(L) });
            var greenLoops = GroupLoops(P.Segs.Where(s => s.Col == "green").ToList());
            foreach (var L in greenLoops) if (!IsOpen(L)) contours.Add(new Contour { Owner = L, Loop = Chain(L) });
            foreach (Circ cc0 in P.Circles) if (cc0.Col != "red" && !ReferenceEquals(cc0, A.OuterCircle)) contours.Add(new Contour { Owner = cc0, IsCircle = true, C = Pt(cc0.Cx, cc0.Cy), R = cc0.R });
            Func<Circ, int, Op> circHole = (c, code) =>
            {
                double px = c.R < pierceHole ? c.Cx : c.Cx - c.R + pierceHole;
                return new Op
                {
                    Area = Math.PI * c.R * c.R, Pp = Pt(px, c.Cy), St = Pt(c.Cx - c.R, c.Cy), Code = code, Circle = true, Cx = c.Cx, Cy = c.Cy, Rad = c.R, StartAng = Math.PI,
                    Segs = new List<Seg> { new Seg { T = 'A', C = Pt(c.Cx, c.Cy), R = c.R, P = Pt(c.Cx - c.R, c.Cy), Q = Pt(c.Cx - c.R, c.Cy), Sweep = TAU, Ccw = true } }
                };
            };
            foreach (Circ c in P.Circles)
            {
                if (ReferenceEquals(c, A.OuterCircle)) continue;
                if (c.Col == "red")   // kegaki tron: duc tai goc 225 do, cat CW, khong dan vao
                {
                    double s2 = Math.Sqrt(0.5);
                    kegaki.Add(new Op { Pp = Pt(c.Cx - c.R * s2, c.Cy - c.R * s2), Code = 9017, KCircle = true, Closed = true, Cx = c.Cx, Cy = c.Cy, Rad = c.R });
                }
                else
                {
                    Op ch = circHole(c, 9010);
                    if (2 * c.R >= 50 - 1e-9) { ch.HasJoint = true; ch.Joint = isSS ? 0 : jointHole; }   // lo tron phi >= 50 co joint (SS: rong 0)
                    // Diem vao (va vi tri joint neu co): (1) cach bien dang khac > 3 mm; (2) trai, phai, duoi, tren. Ap dung cho moi lo tron (ca 26-063378)
                    {
                        double[] angs = { Math.PI, 0, 1.5 * Math.PI, 0.5 * Math.PI };
                        var dl = angs.Select(an => new { ang = an, d = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(an), c.Cy + c.R * Math.Sin(an))) }).ToList();
                        var okc = dl.FirstOrDefault(x => x.d >= 3 - 1e-6);
                        double chosen = okc != null ? okc.ang : (ch.HasJoint ? dl.OrderByDescending(x => x.d).First().ang : angs[0]);
                        string cs = (2 * c.R).ToString("0.##", Inv), what = ch.HasJoint ? "joint" : "điểm vào";
                        if (okc == null) { if (ch.HasJoint) warnings.Add("Joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(dl.Max(x => x.d)) + " mm (≤ 3 mm), không có vị trí tốt hơn"); }
                        else if (dl[0].d < 3 - 1e-6) warnings.Add("Đã xoay " + what + " lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
                        ch.StartAng = chosen; ch.St = Pt(c.Cx + c.R * Math.Cos(chosen), c.Cy + c.R * Math.Sin(chosen));
                        double lenp = c.R < pierceHole ? c.R : pierceHole;
                        ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
                    }                    holes.Add(ch);
                }
            }
            Func<List<Seg>, int, Op> loopHole = (L, code) =>
            {
                var ccw = ReverseLoop(Chain(L));
                double area = Math.Abs(LoopArea(ccw));
                // Lo co joint: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15. Do rong joint: SUS/AL/CU theo bang, vat lieu khac (SS) bang 0
                // nhung van ap dung quy tac vi tri joint
                bool hasJ = false;
                {
                    double cc;
                    double slotLen;
                    if (IsOblong(ccw, out cc)) hasJ = cc >= 10 - 1e-9;
                    else if (SlotLength(ccw, out slotLen)) hasJ = slotLen >= 10 - 1e-9;   // lo dai cong (ca 26-063771)
                    else { var bq = LoopBBox(ccw); hasJ = Math.Max(bq.X1 - bq.X0, bq.Y1 - bq.Y0) >= 15 - 1e-9; }
                }
                double jt = (hasJ && !isSS) ? jointHole : 0;
                // Diem vao: uu tien phan bien dang nho nhot ra; lo hinh tam giac vao giua canh thang dai nhat.
                // Joint nam gan (<= 3 mm) bien ngoai hoac lo khac thi xoay sang diem vao khac.
                var cands = EntryCandidates(ccw, !hasJ);
                Pick sel = cands[0];
                if (hasJ)
                {
                    // Thu tu uu tien vi tri joint: (1) cach cac bien dang xung quanh > 3 mm; (2) ben trai, ben phai, phia duoi, phia tren
                    var bbj = LoopBBox(ccw); double mcx = (bbj.X0 + bbj.X1) / 2, mcy = (bbj.Y0 + bbj.Y1) / 2;
                    var info = cands.Select(cd => new { cd, d = MinDist(contours, L, cd.Pt), rank = DirRank(cd.Pt, mcx, mcy) }).ToList();
                    var okList = info.Where(x => x.d >= 3 - 1e-6).OrderBy(x => x.cd.Tier).ThenBy(x => x.cd.Tier < 10 ? 0 : x.rank).ToList();   // phan nho (cap): theo thu tu truc, khong theo huong tam
                    var wish = info.OrderBy(x => x.cd.Tier).ThenBy(x => x.cd.Tier < 10 ? 0 : x.rank).First();   // vi tri mong muon neu khong xet khoang cach
                    if (okList.Count > 0)
                    {
                        sel = okList[0].cd;
                        if (wish.d < 3 - 1e-6 && wish.cd != sel) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
                    }
                    else
                    {
                        var far = info.OrderByDescending(x => x.d).First(); sel = far.cd;
                        warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(far.d) + " mm (≤ 3 mm), không có vị trí tốt hơn");
                    }
                }                var r = SplitAtPoint(ccw, sel.Pt);
                var bh = LoopBBox(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
                double perp = Math.Abs(sel.Dir[0]) >= Math.Abs(sel.Dir[1]) ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
                double dpi = (sel.R.HasValue && sel.R.Value < pierceHole) ? sel.R.Value : Math.Min(pierceHole, perp / 2);
                double[] pp = Pt(sel.Pt[0] + sel.Dir[0] * dpi, sel.Pt[1] + sel.Dir[1] * dpi);                return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt, HasJoint = hasJ };
            };
            foreach (var L in A.Holes)
            {
                holes.Add(loopHole(L, 9010));
            }
            // duong do -> kegaki
            foreach (var L in GroupLoops(P.Segs.Where(s => s.Col == "red").ToList()))
            {
                if (!IsOpen(L))
                {
                    var seq = ReverseLoop(Chain(L));   // kegaki kin: nguoc chieu kim dong ho, duc ngay tai dinh nen khong co G41
                    int k = 0; double best = double.MaxValue;
                    for (int i = 0; i < seq.Count; i++) { double d = Dist(Pt(0, 0), seq[i].P); if (d < best) { best = d; k = i; } }
                    seq = seq.Skip(k).Concat(seq.Take(k)).ToList();
                    kegaki.Add(new Op { Pp = seq[0].P, Segs = seq, Code = 9017, Closed = true, G41 = false });
                }
                else
                {
                    var ends = Endpoints(L);
                    if (ends.Count != 2) throw new NCError("Đường kegaki (đỏ) không phải 1 đường hở đơn");
                    // dau ccham bien ngoai: duc keo dai 3 mm nguoc chieu doan dau cua duong kegaki (khong theo phap tuyen cua bien)
                    var cands = ends.Select(e =>
                    {
                        double[] exDir = null;
                        if (ExtendOut(e, sg, 3) != null) { var tg = TanAt(ChainOpenFrom(L, e)[0], false); exDir = Pt(e[0] - 3 * tg[0], e[1] - 3 * tg[1]); }
                        return new { e, ext = exDir };
                    }).ToList();
                    var touching = cands.Where(c => c.ext != null).ToList();
                    var pool = touching.Count > 0 ? touching : cands;
                    var ch = pool.OrderBy(c => Dist(Pt(0, 0), c.ext ?? c.e)).First();
                    var seq = ChainOpenFrom(L, ch.e);
                    if (ch.ext != null) seq.Insert(0, new Seg { T = 'L', P = ch.ext, Q = ch.e });
                    kegaki.Add(new Op { Pp = ch.ext ?? ch.e, Segs = seq, Code = 9017, G41 = false });   // kegaki khong G41 sau khi bat tia
                }
            }
            // duong ho by layer + xanh -> khe cat ; vong kin xanh -> nhu lo
            foreach (var L in greenLoops)
            {
                if (IsOpen(L)) slitLoops.Add(L); else holes.Add(loopHole(L, 9010));
            }
            foreach (var L in A.Opens) { slitLoops.Add(L); warnings.Add("Có đường hở Layer 0 by layer: xử lý như khe cắt (M98P9010)"); }
            var slits = new List<Op>();
            foreach (var L in slitLoops)
            {
                var ends = Endpoints(L);
                if (ends.Count != 2) throw new NCError("Khe cắt không phải 1 đường hở đơn");
                var outs = ends.Where(e => !Inside(e, sg)).ToList();
                if (outs.Count != 1) warnings.Add("Khe cắt có " + (outs.Count == 0 ? "cả 2 đầu nằm trong" : "cả 2 đầu nằm ngoài") + " chi tiết: chọn đầu " + Fmt(ends[0][0]) + "," + Fmt(ends[0][1]) + " để đục, hãy kiểm tra");
                var st = outs.Count == 1 ? outs[0] : ends[0];
                slits.Add(new Op { Pp = st, Segs = ChainOpenFrom(L, st), Code = 9010, G41 = true });
            }

            // ---- phat NC ----
            var body = new List<string>(); var steps = new List<Step>();
            var xs = new List<double> { bb.X0, bb.X1, 0 }; var ys = new List<double> { bb.Y0, bb.Y1, 0 };   // goc (0,0) cung tinh vao kich thuoc tieu de
            double[] pos = Pt(0, 0); double cut = 0, rapid = 0; int beam = 0;
            Action<double[], int> pierce = (pt, code) =>
            {
                rapid += Dist(pos, pt);
                body.Add("G00X" + Fmt(pt[0]) + "Y" + Fmt(pt[1]) + "G40"); body.Add("M98P" + code);
                beam++; xs.Add(pt[0]); ys.Add(pt[1]); steps.Add(new Step { X = pt[0], Y = pt[1], Code = code }); pos = pt;
            };
            Action endCut = () => { body.Add("M121"); body.Add("M199"); };
            Func<List<Seg>, double[], bool, double[]> emit = (segs, prev, g41) =>
            {
                for (int i = 0; i < segs.Count; i++)
                {
                    Seg g = segs[i]; double[] q = g.Q; string pre = (g41 && i == 0) ? "G41" : "";
                    if (g.T == 'L') { body.Add("G01" + pre + "X" + Fmt(q[0]) + "Y" + Fmt(q[1])); cut += Dist(prev, q); }
                    else
                    {
                        if (pre != "") { body.Add("G01G41"); pre = ""; }
                        double i_ = g.C[0] - prev[0], j_ = g.C[1] - prev[1];
                        string w = (g.Ccw ? "G03" : "G02") + pre + "X" + Fmt(q[0]) + "Y" + Fmt(q[1]);
                        if (Math.Abs(Math.Round(i_ * 1000)) > 0) w += "I" + Fmt(i_);
                        if (Math.Abs(Math.Round(j_ * 1000)) > 0) w += "J" + Fmt(j_);
                        body.Add(w); cut += g.R * g.Sweep;
                    }
                    prev = q;
                }
                return prev;
            };
            Action<List<double[]>> ptsLoop = null;
            Action<List<Op>, Action<Op>> takeAll = (arr, fn) =>
            {
                var rem = new List<Op>(arr);
                while (rem.Count > 0)
                {
                    Op x = rem.OrderBy(o => Dist(pos, o.Pp)).First();   // OrderBy on doubles is stable -> first minimum
                    rem.Remove(x); fn(x);
                }
            };
            Action<List<double[]>, int> takePoints = (arr, code) =>
            {
                var rem = new List<double[]>(arr);
                while (rem.Count > 0)
                {
                    double[] x = rem.OrderBy(o => Dist(pos, o)).First();
                    rem.Remove(x); pierce(x, code); endCut();
                }
            };

            takePoints(pts16, 9016);
            takeAll(kegaki, k =>
            {
                pierce(k.Pp, 9017);
                if (k.KCircle)
                {
                    body.Add("G02X" + Fmt(k.Pp[0]) + "Y" + Fmt(k.Pp[1]) + "I" + Fmt(k.Cx - k.Pp[0]) + "J" + Fmt(k.Cy - k.Pp[1])); cut += TAU * k.Rad;
                    pos = k.Pp;
                }
                else pos = emit(k.Segs, k.Pp, k.G41);
                endCut();
            });
            takePoints(ptsG, 9010);
            takeAll(slits, s => { pierce(s.Pp, 9010); pos = emit(s.Segs, s.Pp, false); endCut(); });   // khe cat khong G41 (theo ca 26-061922)

            DebugAreas = string.Join("\n", holes.Select(h => "area=" + h.Area.ToString("0.0", Inv) + " pierce=(" + Fmt(h.Pp[0]) + "," + Fmt(h.Pp[1]) + ") joint=" + h.Joint));
            // lo: dien tich nho -> lon ; cung dien tich chon lo gan nhat
            var remH = holes.OrderBy(h => h.Area).ToList();
            while (remH.Count > 0)
            {
                var grp = remH.Where(h => Math.Abs(h.Area - remH[0].Area) < Math.Max(1e-3, 0.06 * remH[0].Area)).ToList();
                Op hh = grp.OrderBy(x => Dist(pos, x.Pp)).First();
                remH.Remove(hh);
                pierce(hh.Pp, hh.Code);
                body.Add("G01G41X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1])); cut += Dist(hh.Pp, hh.St);
                double[] endP = hh.St;   // vi tri ket thuc (co joint thi dung truoc diem vao) de tinh chay khong
                if (hh.Circle)
                {
                    double rr = hh.Rad;
                    if (hh.Joint > 0)   // dung truoc diem vao mot doan joint
                    {
                        double ea = hh.StartAng - hh.Joint / rr;
                        endP = Pt(hh.Cx + rr * Math.Cos(ea), hh.Cy + rr * Math.Sin(ea));
                        body.Add("G03X" + Fmt(endP[0]) + "Y" + Fmt(endP[1]) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr - hh.Joint;
                    }
                    else { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr; }
                }
                else endP = emit(TrimEnd(hh.Segs, hh.Joint), hh.St, false);
                endCut(); pos = endP;
            }

            // bien ngoai (CW, G41)
            // Kieu dan vao cong cho day >= 19: 1 = thep SS (thang 7 + cung R3), 2 = inox/nhom (thang 10, thang 7, cung R3). Neo tai diem vao E, tiep tuyen tv, phap tuyen ngoai nv.
            int leadStyle = Curved(group, t) ? 1 : ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9) ? 2 : 0;
            Action<double[], double[], double[], int> emitLead = (E, tv, nv, style) =>
            {
                double[] S1 = Pt(E[0] + 3 * nv[0] - 3 * tv[0], E[1] + 3 * nv[1] - 3 * tv[1]);
                var lead = new Seg { T = 'A', C = Pt(E[0] + 3 * nv[0], E[1] + 3 * nv[1]), R = 3, P = S1, Q = E, Sweep = Math.PI / 2, Ccw = true };
                if (style == 1)
                {
                    double[] Pp0 = Pt(E[0] + pierceOut * nv[0] - 3 * tv[0], E[1] + pierceOut * nv[1] - 3 * tv[1]);
                    pierce(Pp0, 9010);
                    body.Add("G01G41X" + Fmt(S1[0]) + "Y" + Fmt(S1[1])); cut += Dist(Pp0, S1);
                }
                else
                {
                    double[] S0 = Pt(E[0] + 10 * nv[0] - 3 * tv[0], E[1] + 10 * nv[1] - 3 * tv[1]);
                    double[] Pp0 = Pt(S0[0] + 10 * tv[0], S0[1] + 10 * tv[1]);
                    pierce(Pp0, 9010);
                    body.Add("G01G41X" + Fmt(S0[0]) + "Y" + Fmt(S0[1])); cut += Dist(Pp0, S0);
                    body.Add("G01X" + Fmt(S1[0]) + "Y" + Fmt(S1[1])); cut += Dist(S0, S1);
                }
                emit(new List<Seg> { lead }, S1, false);
            };
            bool split = P.Js.Count > 0 && P.Marks.ContainsKey("LJ");
            if (A.Drawn)
            {
                double[] psD = sg[0].P;   // dau tu do cua duong ve
                pierce(psD, 9010);
                pos = emit(sg, psD, true);
                body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
            }
            else if (split)
            {
                // sap xep LJ va cac J theo chieu CW tren bien ngoai, bat dau tu LJ; moi cap lien tiep (marker -> marker ke tiep) la 1 duong cat
                double total = sg.Sum(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep), pLJ = PathPos(sg, P.Marks["LJ"]);
                var mk = new List<double[]> { P.Marks["LJ"] };
                mk.AddRange(P.Js.OrderBy(j => (PathPos(sg, j) - pLJ + 2 * total) % total));
                var pierces = new List<double[]>(); var seqs = new List<List<Seg>>();
                for (int k = 0; k < mk.Count; k++)
                {
                    double[] pk; List<Seg> sk;
                    SplitPath(sg, mk[k], mk[(k + 1) % mk.Count], jointOut, pierceOut, out pk, out sk);   // duong k: bat dau tai marker k, dung truoc marker ke tiep
                    pierces.Add(pk); seqs.Add(sk);
                }
                for (int pi = 0; pi < mk.Count; pi++)
                {
                    var sq = seqs[pi]; double[] M0 = sq[0].P;
                    pierce(pierces[pi], 9010);
                    body.Add("G01G41X" + Fmt(M0[0]) + "Y" + Fmt(M0[1])); cut += pierceOut;
                    pos = emit(sq, M0, false);
                    body.Add("M121"); body.Add("M199");
                }
                body.Add("G01G40"); body.Add("M30");
            }
            else if (A.OuterCircle != null)
            {
                // phoi tron: diem chuan tai goc 225 do (huong tam -> goc duoi trai), cat CW mot vong, dung truoc diem vao mot doan joint ngoai
                var oc = A.OuterCircle; double ang0 = 1.25 * Math.PI;
                if (leadStyle == 0)
                {
                    double ux = Math.Cos(ang0), uy = Math.Sin(ang0);
                    double[] E0 = Pt(oc.Cx + oc.R * ux, oc.Cy + oc.R * uy), Pc = Pt(E0[0] + pierceOut * ux, E0[1] + pierceOut * uy);
                    pierce(Pc, 9010);
                    body.Add("G01G41X" + Fmt(E0[0]) + "Y" + Fmt(E0[1])); cut += pierceOut;
                    double ea = ang0 + jointOut / oc.R; double[] E1 = Pt(oc.Cx + oc.R * Math.Cos(ea), oc.Cy + oc.R * Math.Sin(ea));
                    body.Add("G02X" + Fmt(E1[0]) + "Y" + Fmt(E1[1]) + IJ(oc.Cx - E0[0], oc.Cy - E0[1])); cut += TAU * oc.R - jointOut;
                    pos = E1;
                }
                else
                {
                    // day >= 19: diem vao (kieu inox/nhom) lech 3 mm theo chieu cat so voi diem chuan; dan vao cong tiep tuyen tai diem vao
                    double angE = leadStyle == 2 ? ang0 - 3 / oc.R : ang0;
                    double[] Ec = Pt(oc.Cx + oc.R * Math.Cos(angE), oc.Cy + oc.R * Math.Sin(angE));
                    emitLead(Ec, Pt(Math.Sin(angE), -Math.Cos(angE)), Pt(Math.Cos(angE), Math.Sin(angE)), leadStyle);
                    double ea = angE + jointOut / oc.R; double[] E1 = Pt(oc.Cx + oc.R * Math.Cos(ea), oc.Cy + oc.R * Math.Sin(ea));
                    body.Add("G02X" + Fmt(E1[0]) + "Y" + Fmt(E1[1]) + IJ(oc.Cx - Ec[0], oc.Cy - Ec[1])); cut += TAU * oc.R - jointOut;
                    pos = E1;
                }
                body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
            }            else if (noLeft)
            {
                // phoi cong / xien: vao tai canh nam ngay phia duoi dinh trai nhat (doan truoc dinh do theo chieu CW, bo qua cung goc R nho);
                // canh <= 100: vao giua canh; > 100: cach dau gan dinh trai nhat 40 (do theo duong thang). Duc ngoai theo phap tuyen ngoai cua canh.
                int n0 = sg.Count, ic = 0;
                for (int i = 0; i < n0; i++) { if (LoopBBox(new List<Seg> { sg[i] }).X0 <= bb.X0 + 1e-4) { ic = i; break; } }
                int ie = ic;
                if (sg[ic].T == 'A' && sg[ic].R <= 5 + 1e-9)   // dinh trai nhat la cung goc nho -> lay canh truoc do
                {
                    ie = (ic - 1 + n0) % n0;
                    for (int gd = 0; gd < n0 && sg[ie].T == 'A' && sg[ie].R <= 5 + 1e-9; gd++) ie = (ie - 1 + n0) % n0;
                }
                Seg ed = sg[ie]; double Le = ed.T == 'L' ? Dist(ed.P, ed.Q) : ed.R * ed.Sweep;
                double sdist = Le - (Le <= 100 + 1e-9 ? Le / 2 : 40);   // khoang cach tu dau doan (theo chieu CW)
                double[] Ee = null, nrm = null;
                // phoi gan tron: doan trai nhat la cung lon (tong cung cung tam/ban kinh quet >= 180 do) va diem 225 do nam tren cung -> quy tac phoi tron
                if (sg[ic].T == 'A' && sg[ic].R > 5 + 1e-9)
                {
                    Seg la = sg[ic]; double a225 = 1.25 * Math.PI;
                    var same = sg.Where(q => q.T == 'A' && q.Ccw == la.Ccw && Dist(q.C, la.C) < 1e-3 && Math.Abs(q.R - la.R) < 1e-3).ToList();
                    bool on225 = same.Any(q =>
                    {
                        double ap225 = Math.Atan2(q.P[1] - q.C[1], q.P[0] - q.C[0]), ang225 = q.Ccw ? Mod(a225 - ap225, TAU) : Mod(ap225 - a225, TAU);
                        return ang225 <= q.Sweep + 1e-9;
                    });
                    if (same.Sum(q => q.Sweep) >= Math.PI - 1e-6 && on225)
                    {
                        Ee = Pt(la.C[0] + la.R * Math.Cos(a225), la.C[1] + la.R * Math.Sin(a225)); nrm = Pt(Math.Cos(a225), Math.Sin(a225));
                        warnings.Add("Phôi gần tròn (cung R" + Fmt(la.R) + " quét ≥ 180°): đục tại góc 225° của cung, hướng ra ngoài");
                    }
                }
                if (Ee != null) { }
                else if (ed.T == 'L')
                {
                    double ux2 = (ed.Q[0] - ed.P[0]) / Le, uy2 = (ed.Q[1] - ed.P[1]) / Le;
                    Ee = Pt(ed.P[0] + ux2 * sdist, ed.P[1] + uy2 * sdist); nrm = Pt(-uy2, ux2);
                    if (Le > 100 + 1e-9)
                    {
                        // canh thang > 100: cach DINH DUOI cua canh 40 mm (theo duong thang). Dinh = giao cua canh voi canh ke ben duoi, bo qua cung bo goc R nho
                        // (ca 26-064456: canh trai nghieng, vao cach dinh duoi 40 mm chu khong phai dau tren)
                        bool pLower = ed.P[1] <= ed.Q[1];
                        int nb = pLower ? (ie - 1 + n0) % n0 : (ie + 1) % n0;
                        for (int gd = 0; gd < n0 && sg[nb].T == 'A' && sg[nb].R <= 5 + 1e-9; gd++) nb = pLower ? (nb - 1 + n0) % n0 : (nb + 1) % n0;
                        double[] lowEnd = pLower ? ed.P : ed.Q, hiEnd = pLower ? ed.Q : ed.P;
                        double[] vv = VirtualVertex(ed, sg[nb], lowEnd);
                        if (vv == null || Dist(vv, lowEnd) > 3) vv = lowEnd;
                        double ln = Dist(vv, hiEnd);
                        Ee = Pt(vv[0] + (hiEnd[0] - vv[0]) / ln * 40, vv[1] + (hiEnd[1] - vv[1]) / ln * 40);
                    }
                }
                else
                {
                    if (Le > 100 + 1e-9 && 40 < 2 * ed.R) sdist = Le - ed.R * 2 * Math.Asin(40 / (2 * ed.R));   // 40 mm do theo day cung
                    double a0e = Math.Atan2(ed.P[1] - ed.C[1], ed.P[0] - ed.C[0]), ame = a0e + (ed.Ccw ? 1 : -1) * sdist / ed.R;
                    Ee = Pt(ed.C[0] + ed.R * Math.Cos(ame), ed.C[1] + ed.R * Math.Sin(ame));
                    nrm = ed.Ccw ? Pt(-Math.Cos(ame), -Math.Sin(ame)) : Pt(Math.Cos(ame), Math.Sin(ame));
                }
                if (leadStyle == 0)
                {
                    pierce(Pt(Ee[0] + pierceOut * nrm[0], Ee[1] + pierceOut * nrm[1]), 9010);
                    body.Add("G01G41X" + Fmt(Ee[0]) + "Y" + Fmt(Ee[1])); cut += pierceOut;
                    var spl = SplitAtPoint(sg, Ee);
                    pos = emit(TrimEnd(spl.Loop, jointOut), Ee, false);
                }
                else
                {
                    // day >= 19: kieu inox/nhom dich diem vao ra truoc 3 mm doc canh (theo chieu cat) so voi diem chuan
                    double[] Ep = Ee;
                    if (leadStyle == 2)
                    {
                        var s0 = SplitAtPoint(sg, Ee).Loop[0];
                        if (s0.T == 'L') { double l0 = Dist(s0.P, s0.Q); Ep = Pt(Ee[0] + 3 * (s0.Q[0] - s0.P[0]) / l0, Ee[1] + 3 * (s0.Q[1] - s0.P[1]) / l0); }
                        else { double aa = Math.Atan2(Ee[1] - s0.C[1], Ee[0] - s0.C[0]) + (s0.Ccw ? 1 : -1) * 3 / s0.R; Ep = Pt(s0.C[0] + s0.R * Math.Cos(aa), s0.C[1] + s0.R * Math.Sin(aa)); }
                    }
                    var splE = SplitAtPoint(sg, Ep);
                    double[] tvE = TanAt(splE.Loop[0], false), nvE = Pt(-tvE[1], tvE[0]);
                    emitLead(Ep, tvE, nvE, leadStyle);
                    pos = emit(TrimEnd(splE.Loop, jointOut), Ep, false);
                }
                body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
            }            else
            {
            double xmin = bb.X0;
            var cand = new List<double[]>();   // len, idx
            for (int i = 0; i < sg.Count; i++)
            {
                Seg g = sg[i];
                if (g.T == 'L' && Math.Abs(g.P[0] - xmin) < 1e-2 && Math.Abs(g.Q[0] - xmin) < 1e-2) cand.Add(new[] { Math.Abs(g.Q[1] - g.P[1]), i, Math.Min(g.P[1], g.Q[1]) });
            }
            // canh dung dai nhat sat Oy; bang nhau (sai so 0.001) thi lay canh thap nhat (Y nho nhat)
            double Lr = 0; int i0 = 0;
            if (!splBottom)
            {
                var cbest = cand.OrderBy(c => c[2]).ThenByDescending(c => c[0]).First();   // canh dung thap nhat sat Oy (ca 26-063416)
                Lr = cbest[0]; i0 = (int)cbest[1];
            }
            double[] a = sg[i0].P, b2 = sg[i0].Q, start, end, pp2;   // splBottom: Lr = 0 nen vao nhanh canh day
            if (Lr < 10)   // quy tac 14/10/2026: canh trai < 10 -> duc o canh tren Ox
            {
                var cb = new List<double[]>();
                for (int k = 0; k < sg.Count; k++)
                {
                    Seg g = sg[k];
                    if (g.T == 'L' && !g.Spl && Math.Abs(g.P[1] - bb.Y0) < 1e-2 && Math.Abs(g.Q[1] - bb.Y0) < 1e-2) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k, Math.Min(g.P[0], g.Q[0]) });
                }
                if (cb.Count == 0) throw new NCError("Cạnh trái < 10 và không có cạnh thẳng trên Ox để đục piercing ngoài");
                var bestB = cb.OrderBy(c => c[2]).ThenByDescending(c => c[0]).First();   // canh day trai nhat tren Ox (ca 26-065395), giong canh trai thap nhat
                double Lb = bestB[0]; i0 = (int)bestB[1];
                a = sg[i0].P; b2 = sg[i0].Q;
                start = Pt(Math.Min(a[0], b2[0]) + (Lb <= 100 + 1e-9 ? Lb / 2 : 40), bb.Y0);   // canh duoi <= 100: giua canh; > 100: cach dau trai 40
                double dx = (b2[0] - a[0]) / Lb;
                end = Pt(start[0] - dx * jointOut, bb.Y0);
                pp2 = Pt(start[0], start[1] - pierceOut);
            }
            else
            {
                int d = b2[1] > a[1] ? 1 : -1;
                double sy = Lr <= 100 + 1e-9 ? (a[1] + b2[1]) / 2 : Math.Min(a[1], b2[1]) + 40;
                start = Pt(xmin, sy); end = Pt(xmin, sy - d * jointOut); pp2 = Pt(xmin - pierceOut, sy);
            }
            var seqO = new List<Seg>();
            for (int k = 1; k < sg.Count; k++) seqO.Add(sg[(i0 + k) % sg.Count]);
            double[] entryPt = start;
            if (Curved(group, t))
            {
                // dan vao cong: pierce cach canh pierceOut, duong thang (pierceOut - 3), cung R3 CCW vao tiep tuyen tai diem vao
                double tl = Dist(a, b2); double[] tv = Pt((b2[0] - a[0]) / tl, (b2[1] - a[1]) / tl), nv = Pt(-tv[1], tv[0]);
                double[] Pp0 = Pt(start[0] + pierceOut * nv[0] - 3 * tv[0], start[1] + pierceOut * nv[1] - 3 * tv[1]);
                double[] S0 = Pt(start[0] + 3 * nv[0] - 3 * tv[0], start[1] + 3 * nv[1] - 3 * tv[1]);
                pierce(Pp0, 9010);
                body.Add("G01G41X" + Fmt(S0[0]) + "Y" + Fmt(S0[1])); cut += Dist(Pp0, S0);
                var lead = new Seg { T = 'A', C = Pt(start[0] + 3 * nv[0], start[1] + 3 * nv[1]), R = 3, P = S0, Q = start, Sweep = Math.PI / 2, Ccw = true };
                emit(new List<Seg> { lead }, S0, false);
            }
            else if ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9)
            {
                // inox >= 19: pierce cach canh 10 va lech 10 doc canh; thang 10 xuong diem chuan, thang 7 vao, cung R3 CCW toi diem vao (lech +3 doc canh);
                // duong cat dung tai diem chuan (chua joint 3)
                double tl = Dist(a, b2); double[] tv = Pt((b2[0] - a[0]) / tl, (b2[1] - a[1]) / tl), nv = Pt(-tv[1], tv[0]);
                double[] S0 = Pt(start[0] + 10 * nv[0], start[1] + 10 * nv[1]);
                double[] Pp0 = Pt(S0[0] + 10 * tv[0], S0[1] + 10 * tv[1]);
                double[] S1 = Pt(start[0] + 3 * nv[0], start[1] + 3 * nv[1]);
                entryPt = Pt(start[0] + 3 * tv[0], start[1] + 3 * tv[1]);
                end = Pt(entryPt[0] - jointOut * tv[0], entryPt[1] - jointOut * tv[1]);   // dung truoc diem vao mot doan joint ngoai (SUS 3 -> diem chuan; AL 0.01)
                pierce(Pp0, 9010);
                body.Add("G01G41X" + Fmt(S0[0]) + "Y" + Fmt(S0[1])); cut += Dist(Pp0, S0);
                body.Add("G01X" + Fmt(S1[0]) + "Y" + Fmt(S1[1])); cut += Dist(S0, S1);
                var lead2 = new Seg { T = 'A', C = Pt(S1[0] + 3 * tv[0], S1[1] + 3 * tv[1]), R = 3, P = S1, Q = entryPt, Sweep = Math.PI / 2, Ccw = true };
                emit(new List<Seg> { lead2 }, S1, false);
            }
            else
            {
                pierce(pp2, 9010);
                body.Add("G01G41X" + Fmt(start[0]) + "Y" + Fmt(start[1])); cut += pierceOut;
            }
            body.Add("G01X" + Fmt(b2[0]) + "Y" + Fmt(b2[1])); cut += Dist(entryPt, b2);
            double[] last = emit(seqO, b2, false);
            body.Add("G01X" + Fmt(end[0]) + "Y" + Fmt(end[1])); cut += Dist(last, end);
            body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
            }

            double w2 = xs.Max() - xs.Min(), h2 = ys.Max() - ys.Min();
            Func<double, string> f81 = v => v.ToString("F1", Inv).PadLeft(8);
            var text = new List<string> {
                "(#_)%(#_)",
                "(" + name.ToUpperInvariant() + ".DXF / " + w2.ToString("F1", Inv) + " X " + h2.ToString("F1", Inv) + ")",
                "#501=106", "M66", "G90", "G92X0.Y0."
            };
            text.AddRange(body);
            text.Add("(CUTTING_LENGTH = " + f81(cut) + "MM  RAPID_LENGTH = " + f81(rapid) + "MM)");
            text.Add("(TOTAL_LENGTH = " + f81(cut + rapid) + "MM)");
            text.Add("(BEAM_ON = " + beam + "  KEGAKI_ON = 0  PREPIERCE_ON = 0)");
            text.Add("(#_)%(#_)");

            var pv = new Preview { Circles = P.Circles, Points = P.Points, Steps = steps, X0 = xs.Min(), Y0 = ys.Min(), X1 = xs.Max(), Y1 = ys.Max() };
            foreach (Seg s in P.Segs)
            {
                var pl = new PLine { Col = s.Col };
                if (s.T == 'L') { pl.Pts.Add(s.P); pl.Pts.Add(s.Q); } else pl.Pts.AddRange(ArcPoints(s, Math.Max(4, (int)Math.Ceiling(s.Sweep * 8))));
                pv.Lines.Add(pl);
            }
            return new Result
            {
                Text = string.Join("\r\n", text), Warnings = warnings, Cut = cut, Rapid = rapid, Beam = beam, W = w2, H = h2, Preview = pv,
                PierceOut = pierceOut, JointOut = jointOut, PierceHole = pierceHole
            };
        }
    }
}
