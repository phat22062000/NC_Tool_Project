# Nhat ky thay doi ma nguon ngay 18-10-2026


## 09:12 - bien dich thanh cong
```diff
--- Core.cs
+                     case "INSERT":
+                         {
+                             string bn = Get(e, "2") ?? "";
+                             if (bn.StartsWith("*")) { ign("khối ẩn danh (INSERT " + bn + ")"); break; }
+                             throw new NCError("DXF có khối (INSERT) tên \"" + bn + "\" (chưa hỗ trợ). Hãy Explode khối thành LINE + ARC trước.");
+                         }
+                     case "POLYLINE": case "SPLINE": case "ELLIPSE":
+         static bool IsOblong(List<Seg> L, out double cc)
+             cc = 0;
+             if (L.Count != 4) return false;
+             var arcs = L.Where(g => g.T == 'A').ToList(); var lines = L.Where(g => g.T == 'L').ToList();
+             if (arcs.Count != 2 || lines.Count != 2) return false;
+             if (Math.Abs(arcs[0].R - arcs[1].R) > 1e-3) return false;
+             if (arcs.Any(a => Math.Abs(a.Sweep - Math.PI) > 1e-3)) return false;
+             cc = Dist(arcs[0].C, arcs[1].C); return true;
+         // Cat ngan duong chay mot doan j tu cuoi (de chua joint)
+         static List<Seg> TrimEnd(List<Seg> segs, double j)
+             if (j <= 0) return segs;
+             var o = new List<Seg>(segs); double rem = j;
+             while (rem > 1e-9 && o.Count > 0)
+                 Seg g = o[o.Count - 1].Copy(); double len = g.T == 'L' ? Dist(g.P, g.Q) : g.R * g.Sweep;
+                 if (len <= rem + 1e-9) { o.RemoveAt(o.Count - 1); rem -= len; continue; }
+                 if (g.T == 'L') { double k = (len - rem) / len; g.Q = Pt(g.P[0] + (g.Q[0] - g.P[0]) * k, g.P[1] + (g.Q[1] - g.P[1]) * k); }
+                     double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ns = g.Sweep - rem / g.R, sgn = g.Ccw ? 1 : -1;
+                     g.Q = Pt(g.C[0] + g.R * Math.Cos(a0 + sgn * ns), g.C[1] + g.R * Math.Sin(a0 + sgn * ns)); g.Sweep = ns;
+                 o[o.Count - 1] = g; rem = 0;
+             return o;
+         {
+             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle; public double Area, Joint, Cx, Cy, Rad;
+         }
+         {
+                 if (c.Col == "red")   // kegaki tron: duc tai goc 225 do, cat CW, khong dan vao
+                     double s2 = Math.Sqrt(0.5);
+                     kegaki.Add(new Op { Pp = Pt(c.Cx - c.R * s2, c.Cy - c.R * s2), Code = 9017, KCircle = true, Closed = true, Cx = c.Cx, Cy = c.Cy, Rad = c.R });
+                 else
+                     Op ch = circHole(c, 9010);
+                     if (!isSS && 2 * c.R >= 50 - 1e-9) ch.Joint = jointOut;   // joint lo SUS/AL: lo tron phi >= 50
+                     holes.Add(ch);
+                 double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
+                     double cc;
+                     if (IsOblong(ccw, out cc)) { if (cc >= 10 - 1e-9) jt = jointOut; }
+                     else { var bq = LoopBBox(ccw); if (Math.Max(bq.X1 - bq.X0, bq.Y1 - bq.Y0) >= 15 - 1e-9) jt = jointOut; }
+                 return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt };
+                 else
+                         if (pre != "") { body.Add("G01G41"); pre = ""; }
+                 {
+                 }
+                 if (k.KCircle)
+                 {
+                     body.Add("G01G41");
+                     body.Add("G02X" + Fmt(k.Pp[0]) + "Y" + Fmt(k.Pp[1]) + "I" + Fmt(k.Cx - k.Pp[0]) + "J" + Fmt(k.Cy - k.Pp[1])); cut += TAU * k.Rad;
+                     pos = k.Pp;
+                 }
+                 if (hh.Circle)
+                 {
+                     double rr = hh.Segs[0].R;
+                     if (hh.Joint > 0)   // dung truoc diem vao mot doan joint
+                     {
+                         double ang = hh.Joint / rr;
+                         body.Add("G03X" + Fmt(hh.St[0] + rr - rr * Math.Cos(ang)) + "Y" + Fmt(hh.St[1] + rr * Math.Sin(ang)) + "I" + Fmt(rr)); cut += TAU * rr - hh.Joint;
+                     }
+                     else { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + "I" + Fmt(rr)); cut += TAU * rr; }
+                 }
+                 else emit(TrimEnd(hh.Segs, hh.Joint), hh.St, false);
+                 {
+                 }
+ 
+ 
+             }
+             {
+         }
-                     case "POLYLINE": case "SPLINE": case "ELLIPSE": case "INSERT":
-             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41; public double Area;
-                 if (!isSS && 2 * c.R >= 50 && c.Col != "red") throw new NCError("Lỗ ≥ φ50 trên SUS/AL/CU cần joint lỗ: chưa hỗ trợ");
-                 if (c.Col == "red") { Op k = circHole(c, 9017); k.Closed = true; kegaki.Add(k); }
-                 else holes.Add(circHole(c, 9010));
-                 return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code };
-                     var b = LoopBBox(L);
-                     if (Math.Max(b.X1 - b.X0, b.Y1 - b.Y0) >= 15) warnings.Add("Lỗ hình " + Fmt(b.X1 - b.X0) + "×" + Fmt(b.Y1 - b.Y0) + " trên SUS/AL/CU có thể cần joint lỗ (chưa hỗ trợ)");
-                 if (k.Circle)
-                     body.Add("G01G41X" + Fmt(k.St[0]) + "Y" + Fmt(k.St[1])); cut += Dist(k.Pp, k.St);
-                     body.Add("G03X" + Fmt(k.St[0]) + "Y" + Fmt(k.St[1]) + "I" + Fmt(k.Segs[0].R)); cut += TAU * k.Segs[0].R;
-                     pos = k.St;
-                 if (hh.Circle) { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + "I" + Fmt(hh.Segs[0].R)); cut += TAU * hh.Segs[0].R; }
-                 else emit(hh.Segs, hh.St, false);
```

## 09:13 - bien dich thanh cong
```diff
--- Core.cs
+             SnapEndpoints(P, 0.02);
+         // Gop cac dau mut cach nhau <= tol (khe ho rat nho do ve cung tiep tuyen) thanh 1 diem de bien dang khep kin
+         static void SnapEndpoints(Prims P, double tol)
+             var list = new List<double[]>();   // moi phan tu: {x, y, segIndex*2 + (0=P|1=Q)}
+             for (int i = 0; i < P.Segs.Count; i++) { list.Add(new[] { P.Segs[i].P[0], P.Segs[i].P[1], i * 2 }); list.Add(new[] { P.Segs[i].Q[0], P.Segs[i].Q[1], i * 2 + 1 }); }
+             if (list.Count == 0) return;
+             var ord = Enumerable.Range(0, list.Count).OrderBy(i => list[i][0]).ToArray();
+             int[] par = new int[list.Count]; for (int i = 0; i < par.Length; i++) par[i] = i;
+             Func<int, int> find = null; find = x => { while (par[x] != x) { par[x] = par[par[x]]; x = par[x]; } return x; };
+             for (int a = 0; a < ord.Length; a++)
+                 for (int b = a + 1; b < ord.Length && list[ord[b]][0] - list[ord[a]][0] <= tol; b++)
+                     double dx = list[ord[a]][0] - list[ord[b]][0], dy = list[ord[a]][1] - list[ord[b]][1];
+                     if (dx * dx + dy * dy <= tol * tol) par[find(ord[a])] = find(ord[b]);
+             var rep = new Dictionary<int, double[]>();
+             for (int i = 0; i < list.Count; i++) { int r = find(i); if (!rep.ContainsKey(r)) rep[r] = new[] { list[i][0], list[i][1] }; }
+             for (int i = 0; i < list.Count; i++)
+                 var pt = rep[find(i)]; int si = (int)list[i][2] / 2;
+                 if (((int)list[i][2] & 1) == 0) P.Segs[si].P = pt; else P.Segs[si].Q = pt;
+         {
+                 {
+                 }
+ 
+             }
+             {
+         }
```

## 09:14 - bien dich thanh cong
```diff
--- Core.cs
+         class Mat
+             public double S = 1, Rot = 0, Tx = 0, Ty = 0;
+             public double[] Ap(double x, double y) { double c = Math.Cos(Rot), s = Math.Sin(Rot); return new[] { Tx + S * (c * x - s * y), Ty + S * (s * x + c * y) }; }
+             public Mat Then(Mat inner) { var t = Ap(inner.Tx, inner.Ty); return new Mat { S = S * inner.S, Rot = Rot + inner.Rot, Tx = t[0], Ty = t[1] }; }
+             // Doc 2 section: BLOCKS (dinh nghia khoi) va ENTITIES
+             var ents = new List<Ent>(); var blocks = new Dictionary<string, List<Ent>>(); var bases = new Dictionary<string, double[]>();
+             string sect = null; Ent cur = null; List<Ent> curBlock = null; bool sawEnt = false;
+             Action flush = () =>
+                 if (cur == null) return;
+                 if (sect == "ENTITIES") ents.Add(cur);
+                 else if (sect == "BLOCKS")
+                     if (cur.Type == "BLOCK")
+                         string nm = Get(cur, "2") ?? ""; curBlock = new List<Ent>(); blocks[nm] = curBlock;
+                         double bx = 0, by = 0;
+                         double.TryParse(Get(cur, "10") ?? "0", NumberStyles.Float, Inv, out bx); double.TryParse(Get(cur, "20") ?? "0", NumberStyles.Float, Inv, out by);
+                         bases[nm] = new[] { bx, by };
+                     else if (cur.Type == "ENDBLK") curBlock = null;
+                     else if (curBlock != null) curBlock.Add(cur);
+                 cur = null;
+                 if (c == "0" && v == "SECTION") { flush(); sect = null; continue; }
+                 if (c == "2" && sect == null && i > 0 && pairs[i - 1][0] == "0" && pairs[i - 1][1] == "SECTION") { sect = v; if (v == "ENTITIES") sawEnt = true; continue; }
+                 if (c == "0" && v == "ENDSEC") { flush(); sect = null; continue; }
+                 if (sect == "ENTITIES" || sect == "BLOCKS")
+                     if (c == "0") { flush(); cur = new Ent { Type = v }; }
+                     else if (cur != null) cur.G.Add(new KeyValuePair<string, string>(c, v));
+             flush();
+             if (!sawEnt) throw new NCError("Không tìm thấy section ENTITIES trong DXF");
+             int expanded = 0;
+             Action<Ent, Mat, string, int> add = null;
+             add = (en, m, parentCol, depth) =>
+                 string lt = (Get(en, "6") ?? "").ToUpperInvariant();
+                 string col = ColorOf(en);
+                 if (parentCol != null && (Get(en, "62") ?? "") == "0") col = parentCol;   // ByBlock -> mau cua INSERT
+                 double rotDeg = m.Rot * 180 / Math.PI;
+                 switch (en.Type)
+                         if (en.Type == "LINE")
+                             P.Segs.Add(new Seg { T = 'L', P = m.Ap(Num(en, "10"), Num(en, "20")), Q = m.Ap(Num(en, "11"), Num(en, "21")), Col = col });
+                         else if (en.Type == "ARC")
+                             double[] cc = m.Ap(Num(en, "10"), Num(en, "20")); double rr = Num(en, "40") * m.S;
+                             double a0 = (Num(en, "50") + rotDeg) * Math.PI / 180, a1 = (Num(en, "51") + rotDeg) * Math.PI / 180;
+                             P.Segs.Add(new Seg { T = 'A', C = cc, R = rr, P = Pt(cc[0] + rr * Math.Cos(a0), cc[1] + rr * Math.Sin(a0)), Q = Pt(cc[0] + rr * Math.Cos(a1), cc[1] + rr * Math.Sin(a1)), Sweep = sw, Col = col });
+                         else if (en.Type == "CIRCLE")
+                             double[] cc = m.Ap(Num(en, "10"), Num(en, "20"));
+                             P.Circles.Add(new Circ { Cx = cc[0], Cy = cc[1], R = Num(en, "40") * m.S, Col = col });
+                             foreach (var kv in en.G)
+                             int fl = 0; int.TryParse(Get(en, "70") ?? "0", out fl);
+                             int cnt = closed ? vs.Count : vs.Count - 1;
+                             for (int k = 0; k < cnt; k++)
+                                 double[] va = vs[k], vb = vs[(k + 1) % vs.Count];
+                                 pushBulge(m.Ap(va[0], va[1]), m.Ap(vb[0], vb[1]), va[2], col);
+                         { var pp = m.Ap(Num(en, "10"), Num(en, "20")); P.Points.Add(new Pnt { X = pp[0], Y = pp[1], Col = col }); }
+                         {
+                             string bn = Get(en, "2") ?? "";
+                             List<Ent> kids;
+                             if (!blocks.TryGetValue(bn, out kids)) { ign("INSERT thiếu định nghĩa khối \"" + bn + "\""); break; }
+                             if (depth >= 8) throw new NCError("Khối (INSERT) lồng quá sâu: " + bn);
+                             double sx = double.Parse(Get(en, "41") ?? "1", Inv), sy = double.Parse(Get(en, "42") ?? "1", Inv);
+                             double rot = double.Parse(Get(en, "50") ?? "0", Inv) * Math.PI / 180;
+                             if (Math.Abs(sx - sy) > 1e-9 || sx <= 0) throw new NCError("Khối (INSERT) \"" + bn + "\" có tỉ lệ không đều hoặc đối xứng (chưa hỗ trợ). Hãy Explode khối trước.");
+                             double ix = Num(en, "10"), iy = Num(en, "20"); var bp = bases[bn];
+                             var local = new Mat { S = sx, Rot = rot };
+                             local.Tx = ix - sx * (Math.Cos(rot) * bp[0] - Math.Sin(rot) * bp[1]);
+                             local.Ty = iy - sx * (Math.Sin(rot) * bp[0] + Math.Cos(rot) * bp[1]);
+                             expanded++;
+                             var total = m.Then(local);
+                             foreach (var ch in kids) add(ch, total, col, depth + 1);
+                             break;
+                         }
+                         throw new NCError("DXF có đối tượng " + en.Type + " (chưa hỗ trợ). Hãy Explode / chuyển thành LINE + ARC trước.");
+             foreach (Ent en0 in ents) add(en0, new Mat(), null, 0);
+             if (expanded > 0) P.Warnings.Add("Đã bung " + expanded + " khối (INSERT) thành đối tượng thường");
+         {
+                     {
+                     }
+             };
+ 
+             {
+             };
+         }
-             var ents = new List<Ent>(); Ent cur = null; bool inEnt = false;
-                 if (!inEnt)
-                     if (c == "2" && v == "ENTITIES" && i > 0 && pairs[i - 1][0] == "0" && pairs[i - 1][1] == "SECTION") inEnt = true;
-                     continue;
-                 if (c == "0")
-                     if (cur != null) ents.Add(cur);
-                     cur = null;
-                     if (v == "ENDSEC") break;
-                     cur = new Ent { Type = v };
-                 else if (cur != null) cur.G.Add(new KeyValuePair<string, string>(c, v));
-             if (!inEnt) throw new NCError("Không tìm thấy section ENTITIES trong DXF");
-             foreach (Ent e in ents)
-                 string lt = (Get(e, "6") ?? "").ToUpperInvariant();
-                 string col = ColorOf(e);
-                 switch (e.Type)
-                         if (e.Type == "LINE")
-                             P.Segs.Add(new Seg { T = 'L', P = Pt(Num(e, "10"), Num(e, "20")), Q = Pt(Num(e, "11"), Num(e, "21")), Col = col });
-                         else if (e.Type == "ARC")
-                             double[] c = Pt(Num(e, "10"), Num(e, "20")); double r = Num(e, "40");
-                             double a0 = Num(e, "50") * Math.PI / 180, a1 = Num(e, "51") * Math.PI / 180;
-                             P.Segs.Add(new Seg { T = 'A', C = c, R = r, P = Pt(c[0] + r * Math.Cos(a0), c[1] + r * Math.Sin(a0)), Q = Pt(c[0] + r * Math.Cos(a1), c[1] + r * Math.Sin(a1)), Sweep = sw, Col = col });
-                         else if (e.Type == "CIRCLE")
-                             P.Circles.Add(new Circ { Cx = Num(e, "10"), Cy = Num(e, "20"), R = Num(e, "40"), Col = col });
-                             foreach (var kv in e.G)
-                             int fl = 0; int.TryParse(Get(e, "70") ?? "0", out fl);
-                             int n = closed ? vs.Count : vs.Count - 1;
-                             for (int i = 0; i < n; i++)
-                                 double[] a = vs[i], b = vs[(i + 1) % vs.Count];
-                                 pushBulge(Pt(a[0], a[1]), Pt(b[0], b[1]), a[2], col);
-                         P.Points.Add(new Pnt { X = Num(e, "10"), Y = Num(e, "20"), Col = col });
-                             string bn = Get(e, "2") ?? "";
-                             if (bn.StartsWith("*")) { ign("khối ẩn danh (INSERT " + bn + ")"); break; }
-                             throw new NCError("DXF có khối (INSERT) tên \"" + bn + "\" (chưa hỗ trợ). Hãy Explode khối thành LINE + ARC trước.");
-                         throw new NCError("DXF có đối tượng " + e.Type + " (chưa hỗ trợ). Hãy Explode / chuyển thành LINE + ARC trước.");
-             }
```

## 09:17 - bien dich thanh cong
```diff
--- Core.cs
+             var bq = LoopBBox(loop);   // kich thuoc that (ke ca dinh cung) de xet lo nam ngang hay dung
+             int ax = (bq.X1 - bq.X0) >= (bq.Y1 - bq.Y0) ? 0 : 1;
+                 var bh = LoopBBox(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
+                 double perp = r.Ax == 0 ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
+                 double dpi = (r.R.HasValue && r.R.Value < pierceHole) ? r.R.Value : Math.Min(pierceHole, perp / 2);
-             var xs = loop.SelectMany(g => new[] { g.P[0], g.Q[0] }).ToList();
-             var ys = loop.SelectMany(g => new[] { g.P[1], g.Q[1] }).ToList();
-             int ax = (xs.Max() - xs.Min()) >= (ys.Max() - ys.Min()) ? 0 : 1;
-                 double w = r.Loop.SelectMany(g => new[] { g.P[r.Ax], g.Q[r.Ax] }).Max() - r.St[r.Ax];
-                 double dpi = (r.R.HasValue && r.R.Value < pierceHole) ? r.R.Value : Math.Min(pierceHole, w / 2);
```

## 09:18 - bien dich thanh cong
```diff
--- Core.cs
+         public static string DebugAreas = "";
+             DebugAreas = string.Join("\n", holes.Select(h => "area=" + h.Area.ToString("0.0", Inv) + " pierce=(" + Fmt(h.Pp[0]) + "," + Fmt(h.Pp[1]) + ") joint=" + h.Joint));
--- Program.cs
+                     File.WriteAllText(args[2] + ".log", string.Join("\r\n", r.Warnings) + (Environment.GetEnvironmentVariable("NC_DEBUG") == "1" ? "\r\n" + Core.DebugAreas : ""), Encoding.UTF8);
-                     File.WriteAllText(args[2] + ".log", string.Join("\r\n", r.Warnings), Encoding.UTF8);
```

## 09:36 - bien dich thanh cong
```diff
--- Core.cs
+             int ax = (bq.X1 - bq.X0) >= (bq.Y1 - bq.Y0) - 1e-3 ? 0 : 1;
+         class Pick { public double[] Pt, Dir; public double? R; }
+         // Chon diem vao cho lo: (1) nua tron / mui nhon loi (tong goc >= 150 do) ban kinh nho nhat; (2) tam giac: giua canh thang dai nhat
+         static Pick PickEntry(List<Seg> loop)
+             int n = loop.Count; if (n == 0) return null;
+             Func<Seg, Seg, bool> cont = (a, b) => a.T == 'A' && b.T == 'A' && a.Ccw == b.Ccw && Dist(a.C, b.C) < 1e-3 && Math.Abs(a.R - b.R) < 1e-3;
+             int start = 0;
+             for (int i = 0; i < n; i++) if (!cont(loop[(i + n - 1) % n], loop[i])) { start = i; break; }
+             var groups = new List<List<Seg>>();
+             for (int k = 0; k < n; k++)
+                 Seg g = loop[(start + k) % n];
+                 if (groups.Count > 0 && cont(groups[groups.Count - 1][groups[groups.Count - 1].Count - 1], g)) groups[groups.Count - 1].Add(g);
+                 else groups.Add(new List<Seg> { g });
+             var caps = groups.Where(gr => gr[0].T == 'A' && gr[0].Ccw && gr.Sum(x => x.Sweep) >= 150 * Math.PI / 180 - 1e-6 && gr.Sum(x => x.Sweep) < TAU - 1e-6).ToList();
+             var bb = LoopBBox(loop);
+             if (caps.Count > 0)
+                 double minR = caps.Min(gr => gr[0].R);
+                 var best = new List<double[]>();   // {apexX, apexY, cx, cy, r}
+                 foreach (var gr in caps.Where(gr => gr[0].R <= minR + 1e-3))
+                     double a0 = Math.Atan2(gr[0].P[1] - gr[0].C[1], gr[0].P[0] - gr[0].C[0]), sw = gr.Sum(x => x.Sweep);
+                     best.Add(new[] { gr[0].C[0] + gr[0].R * Math.Cos(a0 + sw / 2), gr[0].C[1] + gr[0].R * Math.Sin(a0 + sw / 2), gr[0].C[0], gr[0].C[1], gr[0].R });
+                 bool horiz = (bb.X1 - bb.X0) >= (bb.Y1 - bb.Y0) - 1e-3;
+                 var pick = horiz ? best.OrderBy(b => Math.Round(b[0], 4)).ThenBy(b => b[1]).First() : best.OrderBy(b => Math.Round(b[1], 4)).ThenBy(b => b[0]).First();
+                 return new Pick { Pt = Pt(pick[0], pick[1]), Dir = Pt((pick[2] - pick[0]) / pick[4], (pick[3] - pick[1]) / pick[4]), R = pick[4] };
+             var lines = loop.Where(g => g.T == 'L').ToList();
+             if (lines.Count >= 3)
+                 double maxLen = lines.Max(g => Dist(g.P, g.Q));
+                 var longs = lines.Where(g => Dist(g.P, g.Q) >= 0.2 * maxLen).ToList();
+                 if (longs.Count == 3)
+                     Seg g = longs.OrderByDescending(x => Dist(x.P, x.Q)).First(); double len = Dist(g.P, g.Q);
+                     return new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null };
+             return null;
+         // Xoay vong (CCW) de bat dau tai diem pt tren bien dang; cat doi cung / doan thang neu pt nam giua
+         static Split SplitAtPoint(List<Seg> loop, double[] pt)
+             for (int i = 0; i < loop.Count; i++)
+                 Seg g = loop[i];
+                 if (Dist(g.P, pt) < 1e-6) return new Split { Loop = loop.Skip(i).Concat(loop.Take(i)).ToList(), St = g.P, Ax = 0 };
+                 if (g.T == 'A')
+                     if (Math.Abs(Dist(pt, g.C) - g.R) > 1e-3) continue;
+                     double ap = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ang = Math.Atan2(pt[1] - g.C[1], pt[0] - g.C[0]);
+                     double d = g.Ccw ? Mod(ang - ap, TAU) : Mod(ap - ang, TAU);
+                     if (d > 1e-9 && d < g.Sweep - 1e-9)
+                         Seg g1 = g.Copy(); g1.Q = pt; g1.Sweep = d; Seg g2 = g.Copy(); g2.P = pt; g2.Sweep = g.Sweep - d;
+                         var o = new List<Seg> { g2 }; o.AddRange(loop.Skip(i + 1)); o.AddRange(loop.Take(i)); o.Add(g1);
+                         return new Split { Loop = o, St = pt, R = g.R, Ax = 0 };
+                 else if (OnSeg(g, pt, 1e-3))
+                     Seg g1 = g.Copy(); g1.Q = pt; Seg g2 = g.Copy(); g2.P = pt;
+                     var o = new List<Seg> { g2 }; o.AddRange(loop.Skip(i + 1)); o.AddRange(loop.Take(i)); o.Add(g1);
+                     return new Split { Loop = o, St = pt, Ax = 0 };
+             return SplitAtEntry(loop);
+         {
+         }
+         {
+                 // Diem vao: uu tien phan bien dang nho nhot ra (nua tron / mui nhon, ban kinh nho nhat);
+                 // lo hinh tam giac thi vao giua canh thang dai nhat; con lai dung quy tac cu (lo ngang: trai nhat, lo dung: thap nhat)
+                 var pk = PickEntry(ccw);
+                 Split r; double[] pp; var bh = LoopBBox(ccw);
+                 if (pk != null)
+                     r = SplitAtPoint(ccw, pk.Pt);
+                     double perp = Math.Abs(pk.Dir[0]) >= Math.Abs(pk.Dir[1]) ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
+                     double dpi = (pk.R.HasValue && pk.R.Value < pierceHole) ? pk.R.Value : Math.Min(pierceHole, perp / 2);
+                     pp = Pt(pk.Pt[0] + pk.Dir[0] * dpi, pk.Pt[1] + pk.Dir[1] * dpi);
+                     r = SplitAtEntry(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
+                     double perp = r.Ax == 0 ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
+                     double dpi = (r.R.HasValue && r.R.Value < pierceHole) ? r.R.Value : Math.Min(pierceHole, perp / 2);
+                     pp = r.Ax == 0 ? Pt(r.St[0] + dpi, r.St[1]) : Pt(r.St[0], r.St[1] + dpi);
+                 }                double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
+                 else
+                 {
+                 {
+                 }
+                 {
+                 }
+                 {
+                 }
+                 {
+                     {
+                     }
+                 }
+ 
+             }
+             {
+                 {
+                 }
+             }
+             {
+             }
+ 
+ 
+             {
+             }
+             {
+         }
-             int ax = (bq.X1 - bq.X0) >= (bq.Y1 - bq.Y0) ? 0 : 1;
-                 var r = SplitAtEntry(ccw);
-                 var bh = LoopBBox(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
-                 double perp = r.Ax == 0 ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
-                 double dpi = (r.R.HasValue && r.R.Value < pierceHole) ? r.R.Value : Math.Min(pierceHole, perp / 2);
-                 double[] pp = r.Ax == 0 ? Pt(r.St[0] + dpi, r.St[1]) : Pt(r.St[0], r.St[1] + dpi);
-                 double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
```

## 09:56 - LOI bien dich
```diff
--- Core.cs
+         // Bien dang de kiem tra khoang cach joint (bien ngoai, lo khac)
+         class Contour { public object Owner; public List<Seg> Loop; public double[] C; public double R; public bool IsCircle; }
+         static double DistToSeg(Seg g, double[] p)
+             if (g.T == 'L')
+                 double dx = g.Q[0] - g.P[0], dy = g.Q[1] - g.P[1], l2 = dx * dx + dy * dy;
+                 double t = l2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, ((p[0] - g.P[0]) * dx + (p[1] - g.P[1]) * dy) / l2));
+                 return Dist(p, Pt(g.P[0] + t * dx, g.P[1] + t * dy));
+             double ap = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ang = Math.Atan2(p[1] - g.C[1], p[0] - g.C[0]);
+             double d = g.Ccw ? Mod(ang - ap, TAU) : Mod(ap - ang, TAU);
+             if (d <= g.Sweep) return Math.Abs(Dist(p, g.C) - g.R);
+             return Math.Min(Dist(p, g.P), Dist(p, g.Q));
+         static double MinDist(List<Contour> cs, object self, double[] p)
+             double best = double.MaxValue;
+             foreach (var k in cs)
+                 if (ReferenceEquals(k.Owner, self)) continue;
+                 double d = k.IsCircle ? Math.Abs(Dist(p, k.C) - k.R) : k.Loop.Min(g => DistToSeg(g, p));
+                 if (d < best) best = d;
+             return best;
+         class Pick { public double[] Pt, Dir; public double? R; }
+         // Danh sach diem vao theo thu tu uu tien: (1) nua tron / mui nhon loi (tong goc >= 150 do), ban kinh nho truoc,
+         // cung ban kinh thi lo ngang: trai nhat, lo dung: thap nhat; (2) tam giac: giua canh thang dai nhat;
+         // (3) quy tac cu (trai nhat / thap nhat); (4) trung diem cac canh va cung (dung khi can xoay joint)
+         static List<Pick> EntryCandidates(List<Seg> loop)
+             var res = new List<Pick>(); int n = loop.Count; if (n == 0) return res;
+             Action<Pick> add = pk => { if (!res.Any(x => Dist(x.Pt, pk.Pt) < 1e-6)) res.Add(pk); };
+             var bb = LoopBBox(loop); bool horiz = (bb.X1 - bb.X0) >= (bb.Y1 - bb.Y0) - 1e-3;
+             var caps = groups.Where(gr => gr[0].T == 'A' && gr[0].Ccw && gr.Sum(x => x.Sweep) >= 150 * Math.PI / 180 - 1e-6 && gr.Sum(x => x.Sweep) < TAU - 1e-6)
+                 .Select(gr =>
+                     double a0 = Math.Atan2(gr[0].P[1] - gr[0].C[1], gr[0].P[0] - gr[0].C[0]), sw = gr.Sum(x => x.Sweep), r = gr[0].R;
+                     return new { r, x = gr[0].C[0] + r * Math.Cos(a0 + sw / 2), y = gr[0].C[1] + r * Math.Sin(a0 + sw / 2), cx = gr[0].C[0], cy = gr[0].C[1] };
+                 }).ToList();
+             var ordered = horiz ? caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.x, 4)).ThenBy(c => c.y)
+                                 : caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.y, 4)).ThenBy(c => c.x);
+             foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r });
+             if (caps.Count == 0)
+                 var lines = loop.Where(g => g.T == 'L').ToList();
+                 if (lines.Count >= 3)
+                     double maxLen = lines.Max(g => Dist(g.P, g.Q));
+                     var longs = lines.Where(g => Dist(g.P, g.Q) >= 0.2 * maxLen).ToList();
+                     if (longs.Count == 3)
+                         Seg g = longs.OrderByDescending(x => Dist(x.P, x.Q)).First(); double len = Dist(g.P, g.Q);
+                         add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
+             var fb = SplitAtEntry(loop);
+             add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R });
+             foreach (Seg g in loop.OrderByDescending(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep))
+                 if (g.T == 'L')
+                     double len = Dist(g.P, g.Q); if (len < 1e-6) continue;
+                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
+                     double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), am = a0 + (g.Ccw ? 1 : -1) * g.Sweep / 2;
+                     double mx = g.C[0] + g.R * Math.Cos(am), my = g.C[1] + g.R * Math.Sin(am);
+                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null });
+             return res;
+         static string IJ(double i, double j)
+             string w = "";
+             if (Math.Abs(Math.Round(i * 1000)) > 0) w += "I" + Fmt(i);
+             if (Math.Abs(Math.Round(j * 1000)) > 0) w += "J" + Fmt(j);
+             return w;
+         {
+         }
+         {
+             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle; public double Area, Joint, Cx, Cy, Rad, StartAng;
+         }
+         {
+             // bien dang dung de kiem tra khoang cach joint lo (bien ngoai + cac lo)
+             var contours = new List<Contour> { new Contour { Owner = "outer", Loop = sg } };
+             foreach (var L in A.Holes) contours.Add(new Contour { Owner = L, Loop = Chain(L) });
+             var greenLoops = GroupLoops(P.Segs.Where(s => s.Col == "green").ToList());
+             foreach (var L in greenLoops) if (!IsOpen(L)) contours.Add(new Contour { Owner = L, Loop = Chain(L) });
+             foreach (Circ cc0 in P.Circles) if (cc0.Col != "red") contours.Add(new Contour { Owner = cc0, IsCircle = true, C = Pt(cc0.Cx, cc0.Cy), R = cc0.R });
+                     Area = Math.PI * c.R * c.R, Pp = Pt(px, c.Cy), St = Pt(c.Cx - c.R, c.Cy), Code = code, Circle = true, Cx = c.Cx, Cy = c.Cy, Rad = c.R, StartAng = Math.PI,
+                     if (ch.Joint > 0)   // joint nam gan (<= 3 mm) bien dang khac -> xoay sang huong khac
+                         double bestD = -1, bestA = Math.PI; bool okc = false; double firstA = Math.PI;
+                         foreach (double ang in new[] { Math.PI, 1.5 * Math.PI, 0, 0.5 * Math.PI })
+                         {
+                             double dm = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(ang), c.Cy + c.R * Math.Sin(ang)));
+                             if (dm > 3 + 1e-9) { bestA = ang; okc = true; break; }
+                             if (dm > bestD) { bestD = dm; bestA = ang; }
+                         }
+                         if (bestA != firstA)
+                         {
+                             ch.StartAng = bestA; ch.St = Pt(c.Cx + c.R * Math.Cos(bestA), c.Cy + c.R * Math.Sin(bestA));
+                             double lenp = c.R < pierceHole ? c.R : pierceHole;
+                             ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
+                             warnings.Add("Đã xoay joint lỗ tròn φ" + Fmt(2 * c.R) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì cách biên dạng khác ≤ 3 mm");
+                         }
+                         if (!okc) warnings.Add("Joint lỗ tròn φ" + Fmt(2 * c.R) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn");
+                 double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
+                 // Diem vao: uu tien phan bien dang nho nhot ra; lo hinh tam giac vao giua canh thang dai nhat.
+                 // Joint nam gan (<= 3 mm) bien ngoai hoac lo khac thi xoay sang diem vao khac.
+                 var cands = EntryCandidates(ccw);
+                 Pick sel = cands[0];
+                 if (jt > 0)
+                     double bestD = -1; Pick bestC = sel; bool okc = false;
+                     foreach (var cd in cands)
+                     {
+                         double dm = MinDist(contours, L, cd.Pt);
+                         if (dm > 3 + 1e-9) { sel = cd; okc = true; break; }
+                         if (dm > bestD) { bestD = dm; bestC = cd; }
+                     }
+                     if (!okc) { sel = bestC; warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn"); }
+                     else if (sel != cands[0]) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí đầu tiên cách biên dạng khác ≤ 3 mm");
+                 var r = SplitAtPoint(ccw, sel.Pt);
+                 var bh = LoopBBox(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
+                 double perp = Math.Abs(sel.Dir[0]) >= Math.Abs(sel.Dir[1]) ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
+                 double dpi = (sel.R.HasValue && sel.R.Value < pierceHole) ? sel.R.Value : Math.Min(pierceHole, perp / 2);
+                 double[] pp = Pt(sel.Pt[0] + sel.Dir[0] * dpi, sel.Pt[1] + sel.Dir[1] * dpi);                return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt };
+             foreach (var L in greenLoops)
+                     {
+                     }
+ 
+                     double rr = hh.Rad;
+                     {
+                         double ea = hh.StartAng - hh.Joint / rr;
+                         body.Add("G03X" + Fmt(hh.Cx + rr * Math.Cos(ea)) + "Y" + Fmt(hh.Cy + rr * Math.Sin(ea)) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr - hh.Joint;
+                     }
+                     else { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr; }
+ 
+                 {
+                 }
+             }
+ 
+ 
+             {
+             }
+             {
+         }
-         // Chon diem vao cho lo: (1) nua tron / mui nhon loi (tong goc >= 150 do) ban kinh nho nhat; (2) tam giac: giua canh thang dai nhat
-         static Pick PickEntry(List<Seg> loop)
-             int n = loop.Count; if (n == 0) return null;
-             var caps = groups.Where(gr => gr[0].T == 'A' && gr[0].Ccw && gr.Sum(x => x.Sweep) >= 150 * Math.PI / 180 - 1e-6 && gr.Sum(x => x.Sweep) < TAU - 1e-6).ToList();
-             var bb = LoopBBox(loop);
-             if (caps.Count > 0)
-                 double minR = caps.Min(gr => gr[0].R);
-                 var best = new List<double[]>();   // {apexX, apexY, cx, cy, r}
-                 foreach (var gr in caps.Where(gr => gr[0].R <= minR + 1e-3))
-                     double a0 = Math.Atan2(gr[0].P[1] - gr[0].C[1], gr[0].P[0] - gr[0].C[0]), sw = gr.Sum(x => x.Sweep);
-                     best.Add(new[] { gr[0].C[0] + gr[0].R * Math.Cos(a0 + sw / 2), gr[0].C[1] + gr[0].R * Math.Sin(a0 + sw / 2), gr[0].C[0], gr[0].C[1], gr[0].R });
-                 bool horiz = (bb.X1 - bb.X0) >= (bb.Y1 - bb.Y0) - 1e-3;
-                 var pick = horiz ? best.OrderBy(b => Math.Round(b[0], 4)).ThenBy(b => b[1]).First() : best.OrderBy(b => Math.Round(b[1], 4)).ThenBy(b => b[0]).First();
-                 return new Pick { Pt = Pt(pick[0], pick[1]), Dir = Pt((pick[2] - pick[0]) / pick[4], (pick[3] - pick[1]) / pick[4]), R = pick[4] };
-             var lines = loop.Where(g => g.T == 'L').ToList();
-             if (lines.Count >= 3)
-                 double maxLen = lines.Max(g => Dist(g.P, g.Q));
-                 var longs = lines.Where(g => Dist(g.P, g.Q) >= 0.2 * maxLen).ToList();
-                 if (longs.Count == 3)
-                     Seg g = longs.OrderByDescending(x => Dist(x.P, x.Q)).First(); double len = Dist(g.P, g.Q);
-                     return new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null };
-             return null;
-             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle; public double Area, Joint, Cx, Cy, Rad;
-                     Area = Math.PI * c.R * c.R, Pp = Pt(px, c.Cy), St = Pt(c.Cx - c.R, c.Cy), Code = code, Circle = true,
-                 // Diem vao: uu tien phan bien dang nho nhot ra (nua tron / mui nhon, ban kinh nho nhat);
-                 // lo hinh tam giac thi vao giua canh thang dai nhat; con lai dung quy tac cu (lo ngang: trai nhat, lo dung: thap nhat)
-                 var pk = PickEntry(ccw);
-                 Split r; double[] pp; var bh = LoopBBox(ccw);
-                 if (pk != null)
-                     r = SplitAtPoint(ccw, pk.Pt);
-                     double perp = Math.Abs(pk.Dir[0]) >= Math.Abs(pk.Dir[1]) ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
-                     double dpi = (pk.R.HasValue && pk.R.Value < pierceHole) ? pk.R.Value : Math.Min(pierceHole, perp / 2);
-                     pp = Pt(pk.Pt[0] + pk.Dir[0] * dpi, pk.Pt[1] + pk.Dir[1] * dpi);
-                     r = SplitAtEntry(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
-                     double perp = r.Ax == 0 ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
-                     double dpi = (r.R.HasValue && r.R.Value < pierceHole) ? r.R.Value : Math.Min(pierceHole, perp / 2);
-                     pp = r.Ax == 0 ? Pt(r.St[0] + dpi, r.St[1]) : Pt(r.St[0], r.St[1] + dpi);
-                 }                double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
-                 return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt };
-             foreach (var L in GroupLoops(P.Segs.Where(s => s.Col == "green").ToList()))
-                     double rr = hh.Segs[0].R;
-                         double ang = hh.Joint / rr;
-                         body.Add("G03X" + Fmt(hh.St[0] + rr - rr * Math.Cos(ang)) + "Y" + Fmt(hh.St[1] + rr * Math.Sin(ang)) + "I" + Fmt(rr)); cut += TAU * rr - hh.Joint;
-                     else { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + "I" + Fmt(rr)); cut += TAU * rr; }
```

## 09:57 - bien dich thanh cong
```diff
--- Core.cs
+         // Bien dang de kiem tra khoang cach joint (bien ngoai, lo khac)
+         class Contour { public object Owner; public List<Seg> Loop; public double[] C; public double R; public bool IsCircle; }
+         static double DistToSeg(Seg g, double[] p)
+             if (g.T == 'L')
+                 double dx = g.Q[0] - g.P[0], dy = g.Q[1] - g.P[1], l2 = dx * dx + dy * dy;
+                 double t = l2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, ((p[0] - g.P[0]) * dx + (p[1] - g.P[1]) * dy) / l2));
+                 return Dist(p, Pt(g.P[0] + t * dx, g.P[1] + t * dy));
+             double ap = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), ang = Math.Atan2(p[1] - g.C[1], p[0] - g.C[0]);
+             double d = g.Ccw ? Mod(ang - ap, TAU) : Mod(ap - ang, TAU);
+             if (d <= g.Sweep) return Math.Abs(Dist(p, g.C) - g.R);
+             return Math.Min(Dist(p, g.P), Dist(p, g.Q));
+         static double MinDist(List<Contour> cs, object self, double[] p)
+             double best = double.MaxValue;
+             foreach (var k in cs)
+                 if (ReferenceEquals(k.Owner, self)) continue;
+                 double d = k.IsCircle ? Math.Abs(Dist(p, k.C) - k.R) : k.Loop.Min(g => DistToSeg(g, p));
+                 if (d < best) best = d;
+             return best;
+         // Danh sach diem vao theo thu tu uu tien: (1) nua tron / mui nhon loi (tong goc >= 150 do), ban kinh nho truoc,
+         // cung ban kinh thi lo ngang: trai nhat, lo dung: thap nhat; (2) tam giac: giua canh thang dai nhat;
+         // (3) quy tac cu (trai nhat / thap nhat); (4) trung diem cac canh va cung (dung khi can xoay joint)
+         static List<Pick> EntryCandidates(List<Seg> loop)
+             var res = new List<Pick>(); int n = loop.Count; if (n == 0) return res;
+             Action<Pick> add = pk => { if (!res.Any(x => Dist(x.Pt, pk.Pt) < 1e-6)) res.Add(pk); };
+             var bb = LoopBBox(loop); bool horiz = (bb.X1 - bb.X0) >= (bb.Y1 - bb.Y0) - 1e-3;
+             var caps = groups.Where(gr => gr[0].T == 'A' && gr[0].Ccw && gr.Sum(x => x.Sweep) >= 150 * Math.PI / 180 - 1e-6 && gr.Sum(x => x.Sweep) < TAU - 1e-6)
+                 .Select(gr =>
+                     double a0 = Math.Atan2(gr[0].P[1] - gr[0].C[1], gr[0].P[0] - gr[0].C[0]), sw = gr.Sum(x => x.Sweep), r = gr[0].R;
+                     return new { r, x = gr[0].C[0] + r * Math.Cos(a0 + sw / 2), y = gr[0].C[1] + r * Math.Sin(a0 + sw / 2), cx = gr[0].C[0], cy = gr[0].C[1] };
+                 }).ToList();
+             var ordered = horiz ? caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.x, 4)).ThenBy(c => c.y)
+                                 : caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.y, 4)).ThenBy(c => c.x);
+             foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r });
+             if (caps.Count == 0)
+                 var lines = loop.Where(g => g.T == 'L').ToList();
+                 if (lines.Count >= 3)
+                     double maxLen = lines.Max(g => Dist(g.P, g.Q));
+                     var longs = lines.Where(g => Dist(g.P, g.Q) >= 0.2 * maxLen).ToList();
+                     if (longs.Count == 3)
+                         Seg g = longs.OrderByDescending(x => Dist(x.P, x.Q)).First(); double len = Dist(g.P, g.Q);
+                         add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
+             var fb = SplitAtEntry(loop);
+             add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R });
+             foreach (Seg g in loop.OrderByDescending(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep))
+                 if (g.T == 'L')
+                     double len = Dist(g.P, g.Q); if (len < 1e-6) continue;
+                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
+                     double a0 = Math.Atan2(g.P[1] - g.C[1], g.P[0] - g.C[0]), am = a0 + (g.Ccw ? 1 : -1) * g.Sweep / 2;
+                     double mx = g.C[0] + g.R * Math.Cos(am), my = g.C[1] + g.R * Math.Sin(am);
+                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null });
+             return res;
+         static string IJ(double i, double j)
+             string w = "";
+             if (Math.Abs(Math.Round(i * 1000)) > 0) w += "I" + Fmt(i);
+             if (Math.Abs(Math.Round(j * 1000)) > 0) w += "J" + Fmt(j);
+             return w;
+         {
+         }
+         {
+             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle; public double Area, Joint, Cx, Cy, Rad, StartAng;
+         }
+         {
+             // bien dang dung de kiem tra khoang cach joint lo (bien ngoai + cac lo)
+             var contours = new List<Contour> { new Contour { Owner = "outer", Loop = sg } };
+             foreach (var L in A.Holes) contours.Add(new Contour { Owner = L, Loop = Chain(L) });
+             var greenLoops = GroupLoops(P.Segs.Where(s => s.Col == "green").ToList());
+             foreach (var L in greenLoops) if (!IsOpen(L)) contours.Add(new Contour { Owner = L, Loop = Chain(L) });
+             foreach (Circ cc0 in P.Circles) if (cc0.Col != "red") contours.Add(new Contour { Owner = cc0, IsCircle = true, C = Pt(cc0.Cx, cc0.Cy), R = cc0.R });
+                     Area = Math.PI * c.R * c.R, Pp = Pt(px, c.Cy), St = Pt(c.Cx - c.R, c.Cy), Code = code, Circle = true, Cx = c.Cx, Cy = c.Cy, Rad = c.R, StartAng = Math.PI,
+                     if (ch.Joint > 0)   // joint nam gan (<= 3 mm) bien dang khac -> xoay sang huong khac
+                         double bestD = -1, bestA = Math.PI; bool okc = false; double firstA = Math.PI;
+                         foreach (double ang in new[] { Math.PI, 1.5 * Math.PI, 0, 0.5 * Math.PI })
+                         {
+                             double dm = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(ang), c.Cy + c.R * Math.Sin(ang)));
+                             if (dm > 3 + 1e-9) { bestA = ang; okc = true; break; }
+                             if (dm > bestD) { bestD = dm; bestA = ang; }
+                         }
+                         if (bestA != firstA)
+                         {
+                             ch.StartAng = bestA; ch.St = Pt(c.Cx + c.R * Math.Cos(bestA), c.Cy + c.R * Math.Sin(bestA));
+                             double lenp = c.R < pierceHole ? c.R : pierceHole;
+                             ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
+                             warnings.Add("Đã xoay joint lỗ tròn φ" + Fmt(2 * c.R) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì cách biên dạng khác ≤ 3 mm");
+                         }
+                         if (!okc) warnings.Add("Joint lỗ tròn φ" + Fmt(2 * c.R) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn");
+                     }
+                 double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
+                 // Diem vao: uu tien phan bien dang nho nhot ra; lo hinh tam giac vao giua canh thang dai nhat.
+                 // Joint nam gan (<= 3 mm) bien ngoai hoac lo khac thi xoay sang diem vao khac.
+                 var cands = EntryCandidates(ccw);
+                 Pick sel = cands[0];
+                 if (jt > 0)
+                     double bestD = -1; Pick bestC = sel; bool okc = false;
+                     foreach (var cd in cands)
+                     {
+                         double dm = MinDist(contours, L, cd.Pt);
+                         if (dm > 3 + 1e-9) { sel = cd; okc = true; break; }
+                         if (dm > bestD) { bestD = dm; bestC = cd; }
+                     if (!okc) { sel = bestC; warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn"); }
+                     else if (sel != cands[0]) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí đầu tiên cách biên dạng khác ≤ 3 mm");
+                 var r = SplitAtPoint(ccw, sel.Pt);
+                 var bh = LoopBBox(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
+                 double perp = Math.Abs(sel.Dir[0]) >= Math.Abs(sel.Dir[1]) ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
+                 double dpi = (sel.R.HasValue && sel.R.Value < pierceHole) ? sel.R.Value : Math.Min(pierceHole, perp / 2);
+                 double[] pp = Pt(sel.Pt[0] + sel.Dir[0] * dpi, sel.Pt[1] + sel.Dir[1] * dpi);                return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt };
+             foreach (var L in greenLoops)
+                     {
+                     }
+                     double rr = hh.Rad;
+                     {
+                         double ea = hh.StartAng - hh.Joint / rr;
+                         body.Add("G03X" + Fmt(hh.Cx + rr * Math.Cos(ea)) + "Y" + Fmt(hh.Cy + rr * Math.Sin(ea)) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr - hh.Joint;
+                     }
+                     else { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr; }
+ 
+                 {
+                 }
+             }
+ 
+ 
+             {
+             }
+             {
+         }
-         // Chon diem vao cho lo: (1) nua tron / mui nhon loi (tong goc >= 150 do) ban kinh nho nhat; (2) tam giac: giua canh thang dai nhat
-         static Pick PickEntry(List<Seg> loop)
-             int n = loop.Count; if (n == 0) return null;
-             var caps = groups.Where(gr => gr[0].T == 'A' && gr[0].Ccw && gr.Sum(x => x.Sweep) >= 150 * Math.PI / 180 - 1e-6 && gr.Sum(x => x.Sweep) < TAU - 1e-6).ToList();
-             var bb = LoopBBox(loop);
-             if (caps.Count > 0)
-                 double minR = caps.Min(gr => gr[0].R);
-                 var best = new List<double[]>();   // {apexX, apexY, cx, cy, r}
-                 foreach (var gr in caps.Where(gr => gr[0].R <= minR + 1e-3))
-                     double a0 = Math.Atan2(gr[0].P[1] - gr[0].C[1], gr[0].P[0] - gr[0].C[0]), sw = gr.Sum(x => x.Sweep);
-                     best.Add(new[] { gr[0].C[0] + gr[0].R * Math.Cos(a0 + sw / 2), gr[0].C[1] + gr[0].R * Math.Sin(a0 + sw / 2), gr[0].C[0], gr[0].C[1], gr[0].R });
-                 bool horiz = (bb.X1 - bb.X0) >= (bb.Y1 - bb.Y0) - 1e-3;
-                 var pick = horiz ? best.OrderBy(b => Math.Round(b[0], 4)).ThenBy(b => b[1]).First() : best.OrderBy(b => Math.Round(b[1], 4)).ThenBy(b => b[0]).First();
-                 return new Pick { Pt = Pt(pick[0], pick[1]), Dir = Pt((pick[2] - pick[0]) / pick[4], (pick[3] - pick[1]) / pick[4]), R = pick[4] };
-             var lines = loop.Where(g => g.T == 'L').ToList();
-             if (lines.Count >= 3)
-                 double maxLen = lines.Max(g => Dist(g.P, g.Q));
-                 var longs = lines.Where(g => Dist(g.P, g.Q) >= 0.2 * maxLen).ToList();
-                 if (longs.Count == 3)
-                     Seg g = longs.OrderByDescending(x => Dist(x.P, x.Q)).First(); double len = Dist(g.P, g.Q);
-                     return new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null };
-             return null;
-             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle; public double Area, Joint, Cx, Cy, Rad;
-                     Area = Math.PI * c.R * c.R, Pp = Pt(px, c.Cy), St = Pt(c.Cx - c.R, c.Cy), Code = code, Circle = true,
-                 // Diem vao: uu tien phan bien dang nho nhot ra (nua tron / mui nhon, ban kinh nho nhat);
-                 // lo hinh tam giac thi vao giua canh thang dai nhat; con lai dung quy tac cu (lo ngang: trai nhat, lo dung: thap nhat)
-                 var pk = PickEntry(ccw);
-                 Split r; double[] pp; var bh = LoopBBox(ccw);
-                 if (pk != null)
-                     r = SplitAtPoint(ccw, pk.Pt);
-                     double perp = Math.Abs(pk.Dir[0]) >= Math.Abs(pk.Dir[1]) ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
-                     double dpi = (pk.R.HasValue && pk.R.Value < pierceHole) ? pk.R.Value : Math.Min(pierceHole, perp / 2);
-                     pp = Pt(pk.Pt[0] + pk.Dir[0] * dpi, pk.Pt[1] + pk.Dir[1] * dpi);
-                     r = SplitAtEntry(ccw);   // diem duc cach diem vao toi da bang piercing lo, khong qua nua be rong vuong goc voi huong duc
-                     double perp = r.Ax == 0 ? (bh.Y1 - bh.Y0) : (bh.X1 - bh.X0);
-                     double dpi = (r.R.HasValue && r.R.Value < pierceHole) ? r.R.Value : Math.Min(pierceHole, perp / 2);
-                     pp = r.Ax == 0 ? Pt(r.St[0] + dpi, r.St[1]) : Pt(r.St[0], r.St[1] + dpi);
-                 }                double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
-                 return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt };
-             foreach (var L in GroupLoops(P.Segs.Where(s => s.Col == "green").ToList()))
-                     double rr = hh.Segs[0].R;
-                         double ang = hh.Joint / rr;
-                         body.Add("G03X" + Fmt(hh.St[0] + rr - rr * Math.Cos(ang)) + "Y" + Fmt(hh.St[1] + rr * Math.Sin(ang)) + "I" + Fmt(rr)); cut += TAU * rr - hh.Joint;
-                     else { body.Add("G03X" + Fmt(hh.St[0]) + "Y" + Fmt(hh.St[1]) + "I" + Fmt(rr)); cut += TAU * rr; }
```

## 09:58 - bien dich thanh cong
```diff
--- Core.cs
+                             warnings.Add("Đã xoay joint lỗ tròn φ" + (2 * c.R).ToString("0.##", Inv) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì cách biên dạng khác ≤ 3 mm");
+                         if (!okc) warnings.Add("Joint lỗ tròn φ" + (2 * c.R).ToString("0.##", Inv) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn");
-                             warnings.Add("Đã xoay joint lỗ tròn φ" + Fmt(2 * c.R) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì cách biên dạng khác ≤ 3 mm");
-                         if (!okc) warnings.Add("Joint lỗ tròn φ" + Fmt(2 * c.R) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn");
```

## 10:12 - LOI bien dich
```diff
--- Core.cs
+         class Pick { public double[] Pt, Dir; public double? R; public int Tier; }
+         // Huong cua diem so voi tam lo: 0 phai, 1 trai, 2 duoi, 3 tren (thu tu uu tien khi chon vi tri joint)
+         static int DirRank(double[] pt, double cx, double cy)
+             double dx = pt[0] - cx, dy = pt[1] - cy;
+             if (Math.Abs(dx) >= Math.Abs(dy)) return dx > 0 ? 0 : 1;
+             return dy < 0 ? 2 : 3;
+             var radii = ordered.Select(c => Math.Round(c.r, 3)).Distinct().ToList();
+             foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r, Tier = radii.IndexOf(Math.Round(c.r, 3)) });
+                         add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 10 });
+             add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R, Tier = 11 });
+                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 12 });
+                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 12 });
+         {
+                     if (ch.Joint > 0)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) phai, trai, duoi, tren
+                         double[] angs = { 0, Math.PI, 1.5 * Math.PI, 0.5 * Math.PI };
+                         var dl = angs.Select(a => new { a, d = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(a), c.Cy + c.R * Math.Sin(a))) }).ToList();
+                         var okc = dl.FirstOrDefault(x => x.d > 3 + 1e-9);
+                         double chosen = okc != null ? okc.a : dl.OrderByDescending(x => x.d).First().a;
+                         string cs = (2 * c.R).ToString("0.##", Inv);
+                         if (okc == null) warnings.Add("Joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(dl.Max(x => x.d)) + " mm (≤ 3 mm), không có vị trí tốt hơn");
+                         else if (dl[0].d <= 3 + 1e-9) warnings.Add("Đã xoay joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
+                         ch.StartAng = chosen; ch.St = Pt(c.Cx + c.R * Math.Cos(chosen), c.Cy + c.R * Math.Sin(chosen));
+                         double lenp = c.R < pierceHole ? c.R : pierceHole;
+                         ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
+                     }                    holes.Add(ch);
+                     // Thu tu uu tien vi tri joint: (1) cach cac bien dang xung quanh > 3 mm; (2) ben phai, ben trai, phia duoi, phia tren
+                     var bbj = LoopBBox(ccw); double mcx = (bbj.X0 + bbj.X1) / 2, mcy = (bbj.Y0 + bbj.Y1) / 2;
+                     var info = cands.Select(cd => new { cd, d = MinDist(contours, L, cd.Pt), rank = DirRank(cd.Pt, mcx, mcy) }).ToList();
+                     var okList = info.Where(x => x.d > 3 + 1e-9).OrderBy(x => x.cd.Tier).ThenBy(x => x.rank).ToList();
+                     var wish = info.OrderBy(x => x.cd.Tier).ThenBy(x => x.rank).First();   // vi tri mong muon neu khong xet khoang cach
+                     if (okList.Count > 0)
+                         sel = okList[0].cd;
+                         if (wish.d <= 3 + 1e-9 && wish.cd != sel) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
+                         var far = info.OrderByDescending(x => x.d).First(); sel = far.cd;
+                         warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(far.d) + " mm (≤ 3 mm), không có vị trí tốt hơn");
+                 }                var r = SplitAtPoint(ccw, sel.Pt);
+                     else
+                     {
+ 
+         }
-         class Pick { public double[] Pt, Dir; public double? R; }
-             foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r });
-                         add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
-             add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R });
-                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
-                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null });
-                     if (ch.Joint > 0)   // joint nam gan (<= 3 mm) bien dang khac -> xoay sang huong khac
-                         double bestD = -1, bestA = Math.PI; bool okc = false; double firstA = Math.PI;
-                         foreach (double ang in new[] { Math.PI, 1.5 * Math.PI, 0, 0.5 * Math.PI })
-                         {
-                             double dm = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(ang), c.Cy + c.R * Math.Sin(ang)));
-                             if (dm > 3 + 1e-9) { bestA = ang; okc = true; break; }
-                             if (dm > bestD) { bestD = dm; bestA = ang; }
-                         }
-                         if (bestA != firstA)
-                         {
-                             ch.StartAng = bestA; ch.St = Pt(c.Cx + c.R * Math.Cos(bestA), c.Cy + c.R * Math.Sin(bestA));
-                             double lenp = c.R < pierceHole ? c.R : pierceHole;
-                             ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
-                             warnings.Add("Đã xoay joint lỗ tròn φ" + (2 * c.R).ToString("0.##", Inv) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì cách biên dạng khác ≤ 3 mm");
-                         }
-                         if (!okc) warnings.Add("Joint lỗ tròn φ" + (2 * c.R).ToString("0.##", Inv) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn");
-                     holes.Add(ch);
-                     double bestD = -1; Pick bestC = sel; bool okc = false;
-                     foreach (var cd in cands)
-                         double dm = MinDist(contours, L, cd.Pt);
-                         if (dm > 3 + 1e-9) { sel = cd; okc = true; break; }
-                         if (dm > bestD) { bestD = dm; bestC = cd; }
-                     if (!okc) { sel = bestC; warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn"); }
-                     else if (sel != cands[0]) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí đầu tiên cách biên dạng khác ≤ 3 mm");
-                 var r = SplitAtPoint(ccw, sel.Pt);
-                 }
```

## 10:12 - bien dich thanh cong
```diff
--- Core.cs
+         class Pick { public double[] Pt, Dir; public double? R; public int Tier; }
+         // Huong cua diem so voi tam lo: 0 phai, 1 trai, 2 duoi, 3 tren (thu tu uu tien khi chon vi tri joint)
+         static int DirRank(double[] pt, double cx, double cy)
+             double dx = pt[0] - cx, dy = pt[1] - cy;
+             if (Math.Abs(dx) >= Math.Abs(dy)) return dx > 0 ? 0 : 1;
+             return dy < 0 ? 2 : 3;
+             var radii = ordered.Select(c => Math.Round(c.r, 3)).Distinct().ToList();
+             foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r, Tier = radii.IndexOf(Math.Round(c.r, 3)) });
+                         add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 10 });
+             add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R, Tier = 11 });
+                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 12 });
+                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 12 });
+         {
+                     if (ch.Joint > 0)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) phai, trai, duoi, tren
+                         double[] angs = { 0, Math.PI, 1.5 * Math.PI, 0.5 * Math.PI };
+                         var dl = angs.Select(an => new { ang = an, d = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(an), c.Cy + c.R * Math.Sin(an))) }).ToList();
+                         var okc = dl.FirstOrDefault(x => x.d > 3 + 1e-9);
+                         double chosen = okc != null ? okc.ang : dl.OrderByDescending(x => x.d).First().ang;
+                         string cs = (2 * c.R).ToString("0.##", Inv);
+                         if (okc == null) warnings.Add("Joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(dl.Max(x => x.d)) + " mm (≤ 3 mm), không có vị trí tốt hơn");
+                         else if (dl[0].d <= 3 + 1e-9) warnings.Add("Đã xoay joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
+                         ch.StartAng = chosen; ch.St = Pt(c.Cx + c.R * Math.Cos(chosen), c.Cy + c.R * Math.Sin(chosen));
+                         double lenp = c.R < pierceHole ? c.R : pierceHole;
+                         ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
+                     }                    holes.Add(ch);
+                     // Thu tu uu tien vi tri joint: (1) cach cac bien dang xung quanh > 3 mm; (2) ben phai, ben trai, phia duoi, phia tren
+                     var bbj = LoopBBox(ccw); double mcx = (bbj.X0 + bbj.X1) / 2, mcy = (bbj.Y0 + bbj.Y1) / 2;
+                     var info = cands.Select(cd => new { cd, d = MinDist(contours, L, cd.Pt), rank = DirRank(cd.Pt, mcx, mcy) }).ToList();
+                     var okList = info.Where(x => x.d > 3 + 1e-9).OrderBy(x => x.cd.Tier).ThenBy(x => x.rank).ToList();
+                     var wish = info.OrderBy(x => x.cd.Tier).ThenBy(x => x.rank).First();   // vi tri mong muon neu khong xet khoang cach
+                     if (okList.Count > 0)
+                         sel = okList[0].cd;
+                         if (wish.d <= 3 + 1e-9 && wish.cd != sel) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
+                         var far = info.OrderByDescending(x => x.d).First(); sel = far.cd;
+                         warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(far.d) + " mm (≤ 3 mm), không có vị trí tốt hơn");
+                 }                var r = SplitAtPoint(ccw, sel.Pt);
+                     else
+                     {
+ 
+         }
-         class Pick { public double[] Pt, Dir; public double? R; }
-             foreach (var c in ordered) add(new Pick { Pt = Pt(c.x, c.y), Dir = Pt((c.cx - c.x) / c.r, (c.cy - c.y) / c.r), R = c.r });
-                         add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
-             add(new Pick { Pt = fb.St, Dir = fb.Ax == 0 ? Pt(1, 0) : Pt(0, 1), R = fb.R });
-                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null });
-                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null });
-                     if (ch.Joint > 0)   // joint nam gan (<= 3 mm) bien dang khac -> xoay sang huong khac
-                         double bestD = -1, bestA = Math.PI; bool okc = false; double firstA = Math.PI;
-                         foreach (double ang in new[] { Math.PI, 1.5 * Math.PI, 0, 0.5 * Math.PI })
-                         {
-                             double dm = MinDist(contours, c, Pt(c.Cx + c.R * Math.Cos(ang), c.Cy + c.R * Math.Sin(ang)));
-                             if (dm > 3 + 1e-9) { bestA = ang; okc = true; break; }
-                             if (dm > bestD) { bestD = dm; bestA = ang; }
-                         }
-                         if (bestA != firstA)
-                         {
-                             ch.StartAng = bestA; ch.St = Pt(c.Cx + c.R * Math.Cos(bestA), c.Cy + c.R * Math.Sin(bestA));
-                             double lenp = c.R < pierceHole ? c.R : pierceHole;
-                             ch.Pp = Pt(ch.St[0] + (c.Cx - ch.St[0]) / c.R * lenp, ch.St[1] + (c.Cy - ch.St[1]) / c.R * lenp);
-                             warnings.Add("Đã xoay joint lỗ tròn φ" + (2 * c.R).ToString("0.##", Inv) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì cách biên dạng khác ≤ 3 mm");
-                         }
-                         if (!okc) warnings.Add("Joint lỗ tròn φ" + (2 * c.R).ToString("0.##", Inv) + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn");
-                     holes.Add(ch);
-                     double bestD = -1; Pick bestC = sel; bool okc = false;
-                     foreach (var cd in cands)
-                         double dm = MinDist(contours, L, cd.Pt);
-                         if (dm > 3 + 1e-9) { sel = cd; okc = true; break; }
-                         if (dm > bestD) { bestD = dm; bestC = cd; }
-                     if (!okc) { sel = bestC; warnings.Add("Joint lỗ tại (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") cách biên dạng khác " + Fmt(bestD) + " mm (≤ 3 mm), không có vị trí tốt hơn"); }
-                     else if (sel != cands[0]) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí đầu tiên cách biên dạng khác ≤ 3 mm");
-                 var r = SplitAtPoint(ccw, sel.Pt);
-                 }
```

## 10:17 - bien dich thanh cong
```diff
--- Core.cs
+             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle, HasJoint; public double Area, Joint, Cx, Cy, Rad, StartAng;
+                     if (2 * c.R >= 50 - 1e-9) { ch.HasJoint = true; ch.Joint = isSS ? 0 : jointOut; }   // lo tron phi >= 50 co joint (SS: rong 0)
+                     if (ch.HasJoint)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) phai, trai, duoi, tren
+                 // Lo co joint: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15. Do rong joint: SUS/AL/CU theo bang, vat lieu khac (SS) bang 0
+                 // nhung van ap dung quy tac vi tri joint
+                 bool hasJ = false;
+                     if (IsOblong(ccw, out cc)) hasJ = cc >= 10 - 1e-9;
+                     else { var bq = LoopBBox(ccw); hasJ = Math.Max(bq.X1 - bq.X0, bq.Y1 - bq.Y0) >= 15 - 1e-9; }
+                 double jt = (hasJ && !isSS) ? jointOut : 0;
+                 if (hasJ)
+                 double[] pp = Pt(sel.Pt[0] + sel.Dir[0] * dpi, sel.Pt[1] + sel.Dir[1] * dpi);                return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt, HasJoint = hasJ };
-             public double[] Pp, St; public List<Seg> Segs; public int Code; public bool Circle, Closed, G41, KCircle; public double Area, Joint, Cx, Cy, Rad, StartAng;
-                     if (!isSS && 2 * c.R >= 50 - 1e-9) ch.Joint = jointOut;   // joint lo SUS/AL: lo tron phi >= 50
-                     if (ch.Joint > 0)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) phai, trai, duoi, tren
-                 double jt = 0;   // joint lo SUS/AL: lo dai c-c >= 10 ; lo vuong/hinh co canh lon >= 15
-                 if (!isSS)
-                     if (IsOblong(ccw, out cc)) { if (cc >= 10 - 1e-9) jt = jointOut; }
-                     else { var bq = LoopBBox(ccw); if (Math.Max(bq.X1 - bq.X0, bq.Y1 - bq.Y0) >= 15 - 1e-9) jt = jointOut; }
-                 if (jt > 0)
-                 double[] pp = Pt(sel.Pt[0] + sel.Dir[0] * dpi, sel.Pt[1] + sel.Dir[1] * dpi);                return new Op { Area = area, Pp = pp, St = r.St, Segs = r.Loop, Code = code, Joint = jt };
```

## 10:18 - bien dich thanh cong
```diff
--- Core.cs
+                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 11 });
+                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 11 });
-                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 12 });
-                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 12 });
```

## 10:40 - bien dich thanh cong
```diff
--- Core.cs
+         public Dictionary<string, double[]> Marks = new Dictionary<string, double[]>();   // diem danh dau J / LJ (phoi dai hep)
+                     case "TEXT":
+                             string tx = (Get(en, "1") ?? "").Trim().ToUpperInvariant();
+                             if (tx == "J" || tx == "LJ") P.Marks[tx] = m.Ap(Num(en, "10"), Num(en, "20"));
+                         {
+                             break;
+                         }
+             if (P.Marks.Count > 0) P.Points.RemoveAll(pt => P.Marks.Values.Any(mk => Dist(mk, Pt(pt.X, pt.Y)) < 0.05));   // POINT trung diem danh dau khong phai lo trang
+         // Phoi dai hep co marker J/LJ: bien ngoai (CW) tach 2 duong cat; moi duong bat dau tai marker M (duc ngoai, vuong goc voi canh)
+         // va dung truoc marker E mot doan joint ngoai
+         static void SplitPath(List<Seg> sg, double[] M, double[] E, double joint, double pierceOut, out double[] pierce, out List<Seg> seq)
+             int n = sg.Count, iM = -1;
+             for (int i = 0; i < n; i++) if (OnSeg(sg[i], M, 1e-3)) { iM = i; break; }
+             if (iM < 0) throw new NCError("Marker J/LJ không nằm trên cạnh thẳng của biên dạng ngoài");
+             Seg g = sg[iM]; double dx = g.Q[0] - g.P[0], dy = g.Q[1] - g.P[1], ln = Math.Sqrt(dx * dx + dy * dy);
+             double[] nrm = Pt(-dy / ln, dx / ln);   // phap tuyen ngoai (CW -> ben trai huong chay)
+             var all = new List<Seg>(); Seg first = g.Copy(); first.P = M; all.Add(first);
+             for (int k = 1; k < n; k++) all.Add(sg[(iM + k) % n]);
+             Seg lastS = g.Copy(); lastS.Q = M; all.Add(lastS);
+             all.RemoveAll(x => x.T == 'L' && Dist(x.P, x.Q) < 1e-6);
+             var o = new List<Seg>();
+             foreach (var x in all)
+                 if (OnSeg(x, E, 1e-3)) { Seg cp = x.Copy(); cp.Q = E; o.Add(cp); break; }
+                 o.Add(x);
+             Seg ls = o[o.Count - 1];
+             if (ls.T != 'L' || Dist(ls.P, ls.Q) <= joint) throw new NCError("Marker J/LJ: đoạn cuối trước joint không phải đoạn thẳng đủ dài");
+             double ux = ls.Q[0] - ls.P[0], uy = ls.Q[1] - ls.P[1], l2 = Math.Sqrt(ux * ux + uy * uy);
+             Seg lc = ls.Copy(); lc.Q = Pt(E[0] - ux / l2 * joint, E[1] - uy / l2 * joint); o[o.Count - 1] = lc;
+             pierce = Pt(M[0] + nrm[0] * pierceOut, M[1] + nrm[1] * pierceOut); seq = o;
+             foreach (var kv in P.Marks) o.Marks[kv.Key] = f(kv.Value);
+         {
+                 if (prims0.Marks.ContainsKey("J") && prims0.Marks.ContainsKey("LJ") || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
+             bool split = P.Marks.ContainsKey("J") && P.Marks.ContainsKey("LJ");
+             double shortSide = Math.Min(bb.X1 - bb.X0, bb.Y1 - bb.Y0), longSide = Math.Max(bb.X1 - bb.X0, bb.Y1 - bb.Y0);
+             if (!split && shortSide <= 100 && longSide >= 1000) warnings.Add("Phôi dài hẹp (cạnh ngắn ≤ 100, cạnh dài ≥ 1000) nhưng DXF không có marker J/LJ: đang dùng piercing cạnh trái thông thường");
+             if (split)
+                 double[] pa, pb; List<Seg> sa, sb;
+                 SplitPath(sg, P.Marks["LJ"], P.Marks["J"], jointOut, pierceOut, out pa, out sa);   // duong 1: bat dau tai LJ, dung truoc J
+                 SplitPath(sg, P.Marks["J"], P.Marks["LJ"], jointOut, pierceOut, out pb, out sb);   // duong 2: bat dau tai J, dung truoc LJ
+                 for (int pi = 0; pi < 2; pi++)
+                     var sq = pi == 0 ? sa : sb; double[] M0 = sq[0].P;
+                     pierce(pi == 0 ? pa : pb, 9010);
+                     body.Add("G01G41X" + Fmt(M0[0]) + "Y" + Fmt(M0[1])); cut += pierceOut;
+                     pos = emit(sq, M0, false);
+                     body.Add("M121"); body.Add("M199");
+                 body.Add("G01G40"); body.Add("M30");
+                 {
+                 }
+             else
+             {
+             }
+             }
+ 
+             {
+             }
+             {
+         }
-                 if (An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
```

## 10:50 - bien dich thanh cong
```diff
--- Core.cs
+             // Phoi dai hep (canh ngan <= 100, canh dai >= 1000) bat buoc phai co diem danh dau J va LJ
+                 var bb0 = Analyze(prims0).Bb; double w0 = bb0.X1 - bb0.X0, h0 = bb0.Y1 - bb0.Y0;
+                 if (Math.Min(w0, h0) <= 100 && Math.Max(w0, h0) >= 1000)
+                     var miss = new List<string>();
+                     if (!prims0.Marks.ContainsKey("J")) miss.Add("J");
+                     if (!prims0.Marks.ContainsKey("LJ")) miss.Add("LJ");
+                     if (miss.Count > 0) throw new NCError("Phôi dài hẹp (" + w0.ToString("0.#", Inv) + " × " + h0.ToString("0.#", Inv) + ") thiếu điểm đánh dấu " + string.Join(" và ", miss) + ". Hãy thêm TEXT \"J\" và \"LJ\" (kèm POINT) trên cạnh dài trong DXF.");
+                 {
+                 }
+ 
+             }
+             {
-             double shortSide = Math.Min(bb.X1 - bb.X0, bb.Y1 - bb.Y0), longSide = Math.Max(bb.X1 - bb.X0, bb.Y1 - bb.Y0);
-             if (!split && shortSide <= 100 && longSide >= 1000) warnings.Add("Phôi dài hẹp (cạnh ngắn ≤ 100, cạnh dài ≥ 1000) nhưng DXF không có marker J/LJ: đang dùng piercing cạnh trái thông thường");
```

## 11:00 - bien dich thanh cong
```diff
--- Core.cs
+                 start = Pt(Math.Min(a[0], b2[0]) + (Lb <= 100 + 1e-9 ? Lb / 2 : 40), bb.Y0);   // canh duoi <= 100: giua canh; > 100: cach dau trai 40
-                 start = Pt(Math.Min(a[0], b2[0]) + Math.Min(40, Lb / 2), bb.Y0);
```

## 11:22 - bien dich thanh cong
```diff
--- Core.cs
+             if (group.ToUpperInvariant() == "SS")
+                 if (t >= 19 - 1e-9) { pierceOut = 10; jointOut = 0; pierceHole = 10; return; }   // SS >= 19: piercing cong, joint ngoai 0 (theo vi du), piercing lo 10
+                 throw new NCError("Thép (SS) dày " + t.ToString(Inv) + ": chưa có quy tắc cho độ dày trên 16 và dưới 19");
+         // Piercing ngoai cong (duong thang 7 + cung R3 vao tiep tuyen): chi ap dung cho thep SS day >= 19
+         public static bool Curved(string group, double t) { return group.ToUpperInvariant() == "SS" && t >= 19 - 1e-9; }
+             if (Curved(group, t))
+                 // dan vao cong: pierce cach canh pierceOut, duong thang (pierceOut - 3), cung R3 CCW vao tiep tuyen tai diem vao
+                 double tl = Dist(a, b2); double[] tv = Pt((b2[0] - a[0]) / tl, (b2[1] - a[1]) / tl), nv = Pt(-tv[1], tv[0]);
+                 double[] Pp0 = Pt(start[0] + pierceOut * nv[0] - 3 * tv[0], start[1] + pierceOut * nv[1] - 3 * tv[1]);
+                 double[] S0 = Pt(start[0] + 3 * nv[0] - 3 * tv[0], start[1] + 3 * nv[1] - 3 * tv[1]);
+                 pierce(Pp0, 9010);
+                 body.Add("G01G41X" + Fmt(S0[0]) + "Y" + Fmt(S0[1])); cut += Dist(Pp0, S0);
+                 var lead = new Seg { T = 'A', C = Pt(start[0] + 3 * nv[0], start[1] + 3 * nv[1]), R = 3, P = S0, Q = start, Sweep = Math.PI / 2, Ccw = true };
+                 emit(new List<Seg> { lead }, S0, false);
+             else
+             {
+                 pierce(pp2, 9010);
+                 body.Add("G01G41X" + Fmt(start[0]) + "Y" + Fmt(start[1])); cut += pierceOut;
+             }
+             }
+ 
+             {
+             }
+             {
-             pierce(pp2, 9010);
-             body.Add("G01G41X" + Fmt(start[0]) + "Y" + Fmt(start[1])); cut += pierceOut;
```

## 11:47 - bien dich thanh cong
```diff
--- Core.cs
+         // Huong tiep tuyen tai dau/cuoi doan (theo chieu di cua doan)
+         static double[] TanAt(Seg g, bool atEnd)
+             if (g.T == 'L') { double l = Dist(g.P, g.Q); return l < 1e-9 ? Pt(1, 0) : Pt((g.Q[0] - g.P[0]) / l, (g.Q[1] - g.P[1]) / l); }
+             double[] p = atEnd ? g.Q : g.P; double phi = Math.Atan2(p[1] - g.C[1], p[0] - g.C[0]);
+             return g.Ccw ? Pt(-Math.Sin(phi), Math.Cos(phi)) : Pt(Math.Sin(phi), -Math.Cos(phi));
+         // Cac dinh noi hai doan khong tiep tuyen (goc nhon / goc chua bo R): lech huong > 0.5 do
+         static List<double[]> SharpCorners(List<Seg> loop)
+             var res = new List<double[]>();
+             var L = loop.Where(g => !(g.T == 'L' && Dist(g.P, g.Q) < 1e-9)).ToList(); int n = L.Count;
+             for (int i = 0; i < n; i++)
+                 Seg a = L[i], b = L[(i + 1) % n];
+                 double[] ta = TanAt(a, true), tb = TanAt(b, false);
+                 if (ta[0] * tb[0] + ta[1] * tb[1] < Math.Cos(0.5 * Math.PI / 180)) res.Add(a.Q);
+             return res;
+         {
+         }
+         {
+             // Thep SS day >= 19: moi goc (trong va ngoai, ke ca mat vat) phai duoc bo R, khong con goc nhon
+                 var A1 = Analyze(prims0);
+                 var loops1 = new List<List<Seg>> { A1.Outer };
+                 foreach (var Lh in A1.Holes) loops1.Add(Chain(Lh));
+                 foreach (var Lg in GroupLoops(prims0.Segs.Where(s => s.Col == "green").ToList())) if (!IsOpen(Lg)) loops1.Add(Chain(Lg));
+                 var sharp = new List<double[]>();
+                 foreach (var lp in loops1) sharp.AddRange(SharpCorners(lp));
+                 if (sharp.Count > 0)
+                     throw new NCError("Thép (SS) dày " + t.ToString("0.##", Inv) + " phải bo R3 cả góc trong và ngoài, nhưng DXF còn " + sharp.Count + " góc nhọn tại " +
+                         string.Join(", ", sharp.Take(6).Select(p => "(" + Fmt(p[0]) + ", " + Fmt(p[1]) + ")")) + (sharp.Count > 6 ? ", ..." : "") + ". Hãy bo R3 các góc này trong DXF.");
+ 
+             if (Curved(group, t))
+             }
+ 
+ 
+             {
+             }
+             {
+         }
```

## 11:54 - bien dich thanh cong
```diff
--- Core.cs
+                 pierceOut = 10; jointOut = 1; pierceHole = 10; return;   // SS dày trên 16 và dưới 19: làm như thép t10-12 (piercing thẳng 10, joint ngoài 1, piercing lỗ 10)
-                 throw new NCError("Thép (SS) dày " + t.ToString(Inv) + ": chưa có quy tắc cho độ dày trên 16 và dưới 19");
```

## 12:13 - bien dich thanh cong
```diff
--- Core.cs
+             if (group.ToUpperInvariant() == "SUS")
+                 if (t >= 19 - 1e-9) { pierceOut = 10; jointOut = 3; pierceHole = 10; return; }   // SUS >= 19: piercing cong 10 + 7 + R3, joint ngoai 3, piercing lo 10
+                 throw new NCError("Inox (SUS) dày " + t.ToString(Inv) + ": chưa có quy tắc cho độ dày trên 16 và dưới 19");
+             double jointHole = (group.ToUpperInvariant() == "SUS" && t >= 19 - 1e-9) ? 1.0 : jointOut;   // do rong joint lo (SUS >= 19: 1.0)
+                     if (2 * c.R >= 50 - 1e-9) { ch.HasJoint = true; ch.Joint = isSS ? 0 : jointHole; }   // lo tron phi >= 50 co joint (SS: rong 0)
+                 double jt = (hasJ && !isSS) ? jointHole : 0;
+             double[] entryPt = start;
+             else if (group.ToUpperInvariant() == "SUS" && t >= 19 - 1e-9)
+                 // inox >= 19: pierce cach canh 10 va lech 10 doc canh; thang 10 xuong diem chuan, thang 7 vao, cung R3 CCW toi diem vao (lech +3 doc canh);
+                 // duong cat dung tai diem chuan (chua joint 3)
+                 double tl = Dist(a, b2); double[] tv = Pt((b2[0] - a[0]) / tl, (b2[1] - a[1]) / tl), nv = Pt(-tv[1], tv[0]);
+                 double[] S0 = Pt(start[0] + 10 * nv[0], start[1] + 10 * nv[1]);
+                 double[] Pp0 = Pt(S0[0] + 10 * tv[0], S0[1] + 10 * tv[1]);
+                 double[] S1 = Pt(start[0] + 3 * nv[0], start[1] + 3 * nv[1]);
+                 entryPt = Pt(start[0] + 3 * tv[0], start[1] + 3 * tv[1]);
+                 end = start;
+                 pierce(Pp0, 9010);
+                 body.Add("G01G41X" + Fmt(S0[0]) + "Y" + Fmt(S0[1])); cut += Dist(Pp0, S0);
+                 body.Add("G01X" + Fmt(S1[0]) + "Y" + Fmt(S1[1])); cut += Dist(S0, S1);
+                 var lead2 = new Seg { T = 'A', C = Pt(S1[0] + 3 * tv[0], S1[1] + 3 * tv[1]), R = 3, P = S1, Q = entryPt, Sweep = Math.PI / 2, Ccw = true };
+                 emit(new List<Seg> { lead2 }, S1, false);
+             body.Add("G01X" + Fmt(b2[0]) + "Y" + Fmt(b2[1])); cut += Dist(entryPt, b2);
+             }
+             {
+             }
+             {
-                     if (2 * c.R >= 50 - 1e-9) { ch.HasJoint = true; ch.Joint = isSS ? 0 : jointOut; }   // lo tron phi >= 50 co joint (SS: rong 0)
-                 double jt = (hasJ && !isSS) ? jointOut : 0;
-             body.Add("G01X" + Fmt(b2[0]) + "Y" + Fmt(b2[1])); cut += Dist(start, b2);
```

## 12:36 - bien dich thanh cong
```diff
--- Core.cs
+                 pierceOut = 10; jointOut = 1; pierceHole = 10; return;   // SUS dày trên 16 và dưới 19: làm như inox t9-12 (piercing 10, joint ngoài 1, piercing lỗ 10, joint lỗ 1)
+         // Huong cua diem so voi tam lo: 0 trai, 1 phai, 2 duoi, 3 tren (thu tu uu tien khi chon vi tri joint)
+             if (Math.Abs(dx) >= Math.Abs(dy)) return dx > 0 ? 1 : 0;
+                     if (ch.HasJoint)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) trai, phai, duoi, tren
+                         double[] angs = { Math.PI, 0, 1.5 * Math.PI, 0.5 * Math.PI };
+                     // Thu tu uu tien vi tri joint: (1) cach cac bien dang xung quanh > 3 mm; (2) ben trai, ben phai, phia duoi, phia tren
-                 throw new NCError("Inox (SUS) dày " + t.ToString(Inv) + ": chưa có quy tắc cho độ dày trên 16 và dưới 19");
-         // Huong cua diem so voi tam lo: 0 phai, 1 trai, 2 duoi, 3 tren (thu tu uu tien khi chon vi tri joint)
-             if (Math.Abs(dx) >= Math.Abs(dy)) return dx > 0 ? 0 : 1;
-                     if (ch.HasJoint)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) phai, trai, duoi, tren
-                         double[] angs = { 0, Math.PI, 1.5 * Math.PI, 0.5 * Math.PI };
-                     // Thu tu uu tien vi tri joint: (1) cach cac bien dang xung quanh > 3 mm; (2) ben phai, ben trai, phia duoi, phia tren
```

## 12:37 - bien dich thanh cong
```diff
--- Core.cs
+                 double[] endP = hh.St;   // vi tri ket thuc (co joint thi dung truoc diem vao) de tinh chay khong
+                         endP = Pt(hh.Cx + rr * Math.Cos(ea), hh.Cy + rr * Math.Sin(ea));
+                         body.Add("G03X" + Fmt(endP[0]) + "Y" + Fmt(endP[1]) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr - hh.Joint;
+                 else endP = emit(TrimEnd(hh.Segs, hh.Joint), hh.St, false);
+                 endCut(); pos = endP;
-                         body.Add("G03X" + Fmt(hh.Cx + rr * Math.Cos(ea)) + "Y" + Fmt(hh.Cy + rr * Math.Sin(ea)) + IJ(hh.Cx - hh.St[0], hh.Cy - hh.St[1])); cut += TAU * rr - hh.Joint;
-                 else emit(TrimEnd(hh.Segs, hh.Joint), hh.St, false);
-                 endCut(); pos = hh.St;
```

## 12:50 - bien dich thanh cong
```diff
--- Core.cs
+             if (group.ToUpperInvariant() == "AL" && t >= 19 - 1e-9) { pierceOut = 10; jointOut = 0.01; pierceHole = 10; return; }   // AL >= 19: piercing cong, joint ngoai ~0 (0.01), piercing lo 10, joint lo 0
+             double jointHole = (group.ToUpperInvariant() == "SUS" && t >= 19 - 1e-9) ? 1.0 : (group.ToUpperInvariant() == "AL" && t >= 19 - 1e-9) ? 0 : jointOut;   // do rong joint lo (SUS >= 19: 1.0, AL >= 19: 0)
+             else if ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9)
+                 end = Pt(entryPt[0] - jointOut * tv[0], entryPt[1] - jointOut * tv[1]);   // dung truoc diem vao mot doan joint ngoai (SUS 3 -> diem chuan; AL 0.01)
-             double jointHole = (group.ToUpperInvariant() == "SUS" && t >= 19 - 1e-9) ? 1.0 : jointOut;   // do rong joint lo (SUS >= 19: 1.0)
-             else if (group.ToUpperInvariant() == "SUS" && t >= 19 - 1e-9)
-                 end = start;
```

## 19:05 - bien dich thanh cong
```diff
--- Core.cs
+         class Analysis { public List<Seg> Outer; public BB Bb; public List<List<Seg>> Holes, Opens; public Circ OuterCircle; }
+             // phoi hinh tron: CIRCLE by layer lon nhat (lon hon moi vong kin LINE/ARC) la bien ngoai
+             var bigC = P.Circles.Where(c => c.Col == "L").OrderByDescending(c => c.R).FirstOrDefault();
+             if (bigC != null && (closed.Count == 0 || 4 * bigC.R * bigC.R > area(closed[0])))
+                 var half = new List<Seg> {
+                     new Seg { T = 'A', C = Pt(bigC.Cx, bigC.Cy), R = bigC.R, P = Pt(bigC.Cx + bigC.R, bigC.Cy), Q = Pt(bigC.Cx - bigC.R, bigC.Cy), Sweep = Math.PI, Col = "L" },
+                     new Seg { T = 'A', C = Pt(bigC.Cx, bigC.Cy), R = bigC.R, P = Pt(bigC.Cx - bigC.R, bigC.Cy), Q = Pt(bigC.Cx + bigC.R, bigC.Cy), Sweep = Math.PI, Col = "L" } };
+                 var oc = Chain(half);
+                 return new Analysis { Outer = oc, Bb = LoopBBox(oc), Holes = closed, Opens = loops.Where(IsOpen).ToList(), OuterCircle = bigC };
+             if (closed.Count == 0) throw new NCError("Không tìm thấy biên dạng ngoài kín (LINE/ARC hoặc CIRCLE Layer 0, by layer)");
+                 if (prims0.Marks.ContainsKey("J") && prims0.Marks.ContainsKey("LJ") || An.OuterCircle != null || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
+             bool noLeft = false;
+             if (A == null)   // phoi cong / xien: khong co canh thang dung sat Oy o moi huong xoay -> giu nguyen huong, dung quy tac "canh phia duoi dinh trai nhat"
+                 Prims Pr = prims0; Analysis An = Analyze(Pr); double[] shift = new double[] { 0, 0 };
+                 if (Math.Abs(An.Bb.X0) > 1e-4 || Math.Abs(An.Bb.Y0) > 1e-4) { shift = new[] { -An.Bb.X0, -An.Bb.Y0 }; Pr = Translate(Pr, shift[0], shift[1]); An = Analyze(Pr); }
+                 P = Pr; A = An; chRot = 0; chShift = shift; noLeft = true;
+             foreach (Circ cc0 in P.Circles) if (cc0.Col != "red" && !ReferenceEquals(cc0, A.OuterCircle)) contours.Add(new Contour { Owner = cc0, IsCircle = true, C = Pt(cc0.Cx, cc0.Cy), R = cc0.R });
+                 if (ReferenceEquals(c, A.OuterCircle)) continue;
+             else if (A.OuterCircle != null)
+                 // phoi tron: duc ngoai tai goc 225 do (huong tam -> goc duoi trai), cat CW mot vong, dung truoc diem vao mot doan joint
+                 if (Curved(group, t) || ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9)) throw new NCError("Phôi tròn dày từ 19 mm: chưa có quy tắc đục piercing cong");
+                 var oc = A.OuterCircle; double ang0 = 1.25 * Math.PI, ux = Math.Cos(ang0), uy = Math.Sin(ang0);
+                 double[] E0 = Pt(oc.Cx + oc.R * ux, oc.Cy + oc.R * uy), Pc = Pt(E0[0] + pierceOut * ux, E0[1] + pierceOut * uy);
+                 pierce(Pc, 9010);
+                 body.Add("G01G41X" + Fmt(E0[0]) + "Y" + Fmt(E0[1])); cut += pierceOut;
+                 double ea = ang0 + jointOut / oc.R; double[] E1 = Pt(oc.Cx + oc.R * Math.Cos(ea), oc.Cy + oc.R * Math.Sin(ea));
+                 body.Add("G02X" + Fmt(E1[0]) + "Y" + Fmt(E1[1]) + IJ(oc.Cx - E0[0], oc.Cy - E0[1])); cut += TAU * oc.R - jointOut;
+                 pos = E1;
+                 body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
+             else if (noLeft)
+                 // phoi cong / xien: vao tai canh nam ngay phia duoi dinh trai nhat (doan truoc dinh do theo chieu CW, bo qua cung goc R nho);
+                 // canh <= 100: vao giua canh; > 100: cach dau gan dinh trai nhat 40 (do doc canh). Duc ngoai cach canh pierceOut theo phap tuyen ngoai.
+                 if (Curved(group, t) || ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9)) throw new NCError("Phôi cong/xiên không có cạnh thẳng đứng, dày từ 19 mm: chưa có quy tắc đục piercing cong");
+                 int n0 = sg.Count, ic = 0;
+                 for (int i = 0; i < n0; i++) { if (LoopBBox(new List<Seg> { sg[i] }).X0 <= bb.X0 + 1e-4) { ic = i; break; } }
+                 int ie = ic;
+                 if (sg[ic].T == 'A' && sg[ic].R <= 5 + 1e-9)   // dinh trai nhat la cung goc nho -> lay canh truoc do
+                     ie = (ic - 1 + n0) % n0;
+                     for (int gd = 0; gd < n0 && sg[ie].T == 'A' && sg[ie].R <= 5 + 1e-9; gd++) ie = (ie - 1 + n0) % n0;
+                 Seg ed = sg[ie]; double Le = ed.T == 'L' ? Dist(ed.P, ed.Q) : ed.R * ed.Sweep;
+                 double sdist = Le - (Le <= 100 + 1e-9 ? Le / 2 : 40);   // khoang cach tu dau doan (theo chieu CW)
+                 double[] Ee, nrm;
+                 if (ed.T == 'L')
+                 {
+                     double ux2 = (ed.Q[0] - ed.P[0]) / Le, uy2 = (ed.Q[1] - ed.P[1]) / Le;
+                     Ee = Pt(ed.P[0] + ux2 * sdist, ed.P[1] + uy2 * sdist); nrm = Pt(-uy2, ux2);
+                 }
+                 else
+                 {
+                     double a0e = Math.Atan2(ed.P[1] - ed.C[1], ed.P[0] - ed.C[0]), ame = a0e + (ed.Ccw ? 1 : -1) * sdist / ed.R;
+                     Ee = Pt(ed.C[0] + ed.R * Math.Cos(ame), ed.C[1] + ed.R * Math.Sin(ame));
+                     nrm = ed.Ccw ? Pt(-Math.Cos(ame), -Math.Sin(ame)) : Pt(Math.Cos(ame), Math.Sin(ame));
+                 }
+                 pierce(Pt(Ee[0] + pierceOut * nrm[0], Ee[1] + pierceOut * nrm[1]), 9010);
+                 body.Add("G01G41X" + Fmt(Ee[0]) + "Y" + Fmt(Ee[1])); cut += pierceOut;
+                 var spl = SplitAtPoint(sg, Ee);
+                 pos = emit(TrimEnd(spl.Loop, jointOut), Ee, false);
+                 body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
+                 {
+                 }
+             {
+             }
+             {
+             }
+             }
+             {
+             }
+             {
-         class Analysis { public List<Seg> Outer; public BB Bb; public List<List<Seg>> Holes, Opens; }
-             if (closed.Count == 0) throw new NCError("Không tìm thấy biên dạng ngoài kín (LINE/ARC Layer 0, by layer)");
-                 if (prims0.Marks.ContainsKey("J") && prims0.Marks.ContainsKey("LJ") || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
-             if (A == null) throw new NCError("Không có cạnh thẳng đứng sát Oy (kể cả khi xoay 90/180/270°). Kiểm tra lại hình dạng phôi.");
-             foreach (Circ cc0 in P.Circles) if (cc0.Col != "red") contours.Add(new Contour { Owner = cc0, IsCircle = true, C = Pt(cc0.Cx, cc0.Cy), R = cc0.R });
```

## 19:06 - bien dich thanh cong
```diff
--- Core.cs
+                     // canh dai > 100: 40 mm do theo duong thang (day cung) tu dau canh, khong phai theo chieu dai cung
+                     if (Le > 100 + 1e-9 && 40 < 2 * ed.R) sdist = Le - ed.R * 2 * Math.Asin(40 / (2 * ed.R));
```

## 19:20 - bien dich thanh cong
```diff
--- Core.cs
+             // Kieu dan vao cong cho day >= 19: 1 = thep SS (thang 7 + cung R3), 2 = inox/nhom (thang 10, thang 7, cung R3). Neo tai diem vao E, tiep tuyen tv, phap tuyen ngoai nv.
+             int leadStyle = Curved(group, t) ? 1 : ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9) ? 2 : 0;
+             Action<double[], double[], double[], int> emitLead = (E, tv, nv, style) =>
+                 double[] S1 = Pt(E[0] + 3 * nv[0] - 3 * tv[0], E[1] + 3 * nv[1] - 3 * tv[1]);
+                 var lead = new Seg { T = 'A', C = Pt(E[0] + 3 * nv[0], E[1] + 3 * nv[1]), R = 3, P = S1, Q = E, Sweep = Math.PI / 2, Ccw = true };
+                 if (style == 1)
+                     double[] Pp0 = Pt(E[0] + pierceOut * nv[0] - 3 * tv[0], E[1] + pierceOut * nv[1] - 3 * tv[1]);
+                     pierce(Pp0, 9010);
+                     body.Add("G01G41X" + Fmt(S1[0]) + "Y" + Fmt(S1[1])); cut += Dist(Pp0, S1);
+                     double[] S0 = Pt(E[0] + 10 * nv[0] - 3 * tv[0], E[1] + 10 * nv[1] - 3 * tv[1]);
+                     double[] Pp0 = Pt(S0[0] + 10 * tv[0], S0[1] + 10 * tv[1]);
+                     pierce(Pp0, 9010);
+                     body.Add("G01G41X" + Fmt(S0[0]) + "Y" + Fmt(S0[1])); cut += Dist(Pp0, S0);
+                     body.Add("G01X" + Fmt(S1[0]) + "Y" + Fmt(S1[1])); cut += Dist(S0, S1);
+                 emit(new List<Seg> { lead }, S1, false);
+                 // phoi tron: diem chuan tai goc 225 do (huong tam -> goc duoi trai), cat CW mot vong, dung truoc diem vao mot doan joint ngoai
+                 var oc = A.OuterCircle; double ang0 = 1.25 * Math.PI;
+                 if (leadStyle == 0)
+                     double ux = Math.Cos(ang0), uy = Math.Sin(ang0);
+                     double[] E0 = Pt(oc.Cx + oc.R * ux, oc.Cy + oc.R * uy), Pc = Pt(E0[0] + pierceOut * ux, E0[1] + pierceOut * uy);
+                     pierce(Pc, 9010);
+                     body.Add("G01G41X" + Fmt(E0[0]) + "Y" + Fmt(E0[1])); cut += pierceOut;
+                     double ea = ang0 + jointOut / oc.R; double[] E1 = Pt(oc.Cx + oc.R * Math.Cos(ea), oc.Cy + oc.R * Math.Sin(ea));
+                     body.Add("G02X" + Fmt(E1[0]) + "Y" + Fmt(E1[1]) + IJ(oc.Cx - E0[0], oc.Cy - E0[1])); cut += TAU * oc.R - jointOut;
+                     pos = E1;
+                 else
+                     // day >= 19: diem vao (kieu inox/nhom) lech 3 mm theo chieu cat so voi diem chuan; dan vao cong tiep tuyen tai diem vao
+                     double angE = leadStyle == 2 ? ang0 - 3 / oc.R : ang0;
+                     double[] Ec = Pt(oc.Cx + oc.R * Math.Cos(angE), oc.Cy + oc.R * Math.Sin(angE));
+                     emitLead(Ec, Pt(Math.Sin(angE), -Math.Cos(angE)), Pt(Math.Cos(angE), Math.Sin(angE)), leadStyle);
+                     double ea = angE + jointOut / oc.R; double[] E1 = Pt(oc.Cx + oc.R * Math.Cos(ea), oc.Cy + oc.R * Math.Sin(ea));
+                     body.Add("G02X" + Fmt(E1[0]) + "Y" + Fmt(E1[1]) + IJ(oc.Cx - Ec[0], oc.Cy - Ec[1])); cut += TAU * oc.R - jointOut;
+                     pos = E1;
+             }            else if (noLeft)
+                 // canh <= 100: vao giua canh; > 100: cach dau gan dinh trai nhat 40 (do theo duong thang). Duc ngoai theo phap tuyen ngoai cua canh.
+                 {
+                 }
+                 {
+                 }
+                 else
+                 {
+                     if (Le > 100 + 1e-9 && 40 < 2 * ed.R) sdist = Le - ed.R * 2 * Math.Asin(40 / (2 * ed.R));   // 40 mm do theo day cung
+                 }
+                 if (leadStyle == 0)
+                 {
+                     pierce(Pt(Ee[0] + pierceOut * nrm[0], Ee[1] + pierceOut * nrm[1]), 9010);
+                     body.Add("G01G41X" + Fmt(Ee[0]) + "Y" + Fmt(Ee[1])); cut += pierceOut;
+                     var spl = SplitAtPoint(sg, Ee);
+                     pos = emit(TrimEnd(spl.Loop, jointOut), Ee, false);
+                 }
+                 else
+                 {
+                     // day >= 19: kieu inox/nhom dich diem vao ra truoc 3 mm doc canh (theo chieu cat) so voi diem chuan
+                     double[] Ep = Ee;
+                     if (leadStyle == 2)
+                     {
+                         var s0 = SplitAtPoint(sg, Ee).Loop[0];
+                         if (s0.T == 'L') { double l0 = Dist(s0.P, s0.Q); Ep = Pt(Ee[0] + 3 * (s0.Q[0] - s0.P[0]) / l0, Ee[1] + 3 * (s0.Q[1] - s0.P[1]) / l0); }
+                         else { double aa = Math.Atan2(Ee[1] - s0.C[1], Ee[0] - s0.C[0]) + (s0.Ccw ? 1 : -1) * 3 / s0.R; Ep = Pt(s0.C[0] + s0.R * Math.Cos(aa), s0.C[1] + s0.R * Math.Sin(aa)); }
+                     }
+                     var splE = SplitAtPoint(sg, Ep);
+                     double[] tvE = TanAt(splE.Loop[0], false), nvE = Pt(-tvE[1], tvE[0]);
+                     emitLead(Ep, tvE, nvE, leadStyle);
+                     pos = emit(TrimEnd(splE.Loop, jointOut), Ep, false);
+                 }
+             }            else
+                 {
+                 }
+             {
+             };
-                 // phoi tron: duc ngoai tai goc 225 do (huong tam -> goc duoi trai), cat CW mot vong, dung truoc diem vao mot doan joint
-                 if (Curved(group, t) || ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9)) throw new NCError("Phôi tròn dày từ 19 mm: chưa có quy tắc đục piercing cong");
-                 var oc = A.OuterCircle; double ang0 = 1.25 * Math.PI, ux = Math.Cos(ang0), uy = Math.Sin(ang0);
-                 double[] E0 = Pt(oc.Cx + oc.R * ux, oc.Cy + oc.R * uy), Pc = Pt(E0[0] + pierceOut * ux, E0[1] + pierceOut * uy);
-                 pierce(Pc, 9010);
-                 body.Add("G01G41X" + Fmt(E0[0]) + "Y" + Fmt(E0[1])); cut += pierceOut;
-                 double ea = ang0 + jointOut / oc.R; double[] E1 = Pt(oc.Cx + oc.R * Math.Cos(ea), oc.Cy + oc.R * Math.Sin(ea));
-                 body.Add("G02X" + Fmt(E1[0]) + "Y" + Fmt(E1[1]) + IJ(oc.Cx - E0[0], oc.Cy - E0[1])); cut += TAU * oc.R - jointOut;
-                 pos = E1;
-             else if (noLeft)
-                 // canh <= 100: vao giua canh; > 100: cach dau gan dinh trai nhat 40 (do doc canh). Duc ngoai cach canh pierceOut theo phap tuyen ngoai.
-                 if (Curved(group, t) || ((group.ToUpperInvariant() == "SUS" || group.ToUpperInvariant() == "AL") && t >= 19 - 1e-9)) throw new NCError("Phôi cong/xiên không có cạnh thẳng đứng, dày từ 19 mm: chưa có quy tắc đục piercing cong");
-                     // canh dai > 100: 40 mm do theo duong thang (day cung) tu dau canh, khong phai theo chieu dai cung
-                     if (Le > 100 + 1e-9 && 40 < 2 * ed.R) sdist = Le - ed.R * 2 * Math.Asin(40 / (2 * ed.R));
-                 pierce(Pt(Ee[0] + pierceOut * nrm[0], Ee[1] + pierceOut * nrm[1]), 9010);
-                 body.Add("G01G41X" + Fmt(Ee[0]) + "Y" + Fmt(Ee[1])); cut += pierceOut;
-                 var spl = SplitAtPoint(sg, Ee);
-                 pos = emit(TrimEnd(spl.Loop, jointOut), Ee, false);
-             else
-             }
-             }
```

## 19:25 - bien dich thanh cong
```diff
--- Program.cs
+         // Sau khi luu xong: bo chon file Excel (xoa dong ten file tren thanh dau); lan sau phai chon lai. Cac file con lai trong danh sach giu nguyen ket qua da tinh.
+         void ClearExcel()
+             excelPath = null; table = null; tableError = null;
+             chip.SetText(""); LayoutHeader(); SaveSettings();
+                     ClearExcel();
+ 
+             if (saved.Count > 0) ClearExcel();
+         {
+         }
```

## 19:31 - bien dich thanh cong
```diff
--- Program.cs
+         // Vat lieu / do day rieng cua tung file khi khong lay tu Excel: gia tri luc them file, hoac do nguoi dung chinh (Override)
+         public string MGroup; public double MThick; public bool Override;
+         MatTable table; string excelPath, tableError, manGroup = "SS"; bool dark; double manThick = 3.2; bool loading = true, syncing = false;
+         // Doi vat lieu / do day: chi ap dung cho file dang duoc chon (co the chon nhieu file bang Shift/Ctrl); cac file khac giu nguyen
+             if (loading || syncing) return;
+             var sel = list.SelectedItems.Cast<FileItem>().ToList();
+             if (sel.Count == 0) return;
+             foreach (var f in sel) { f.Override = true; f.MGroup = Groups[Math.Max(0, cmbMat.SelectedIndex)]; f.MThick = (double)nudThk.Value; Convert(f); }
+         // Dua o Vat Lieu / Do Day ve thong so cua file dang chon (khong kich hoat chuyen doi lai)
+         void SyncCombos(FileItem f)
+             if (f == null || f.Row == null) return;
+             int gi = Array.IndexOf(Groups, f.Row.Group); decimal th = (decimal)f.Row.Thick;
+             syncing = true;
+             try
+                 if (gi >= 0) cmbMat.SelectedIndex = gi;
+                 nudThk.Value = Math.Max(nudThk.Minimum, Math.Min(nudThk.Maximum, th));
+             finally { syncing = false; }
+             f.Row = (!f.Override && table != null) ? table.Find(f.Base) : null;
+             if (f.Row == null)   // khong co trong Excel (hoac nguoi dung da chinh tay) -> dung vat lieu / do day rieng cua file
+                 if (f.MGroup == null) { f.MGroup = Groups[Math.Max(0, cmbMat.SelectedIndex)]; f.MThick = (double)nudThk.Value; }   // gia tri o thanh tren luc them file
+                 f.Row = new MatRow { Code = f.Base, MaterialText = "Nhập Tay", Group = f.MGroup, Thick = f.MThick, Line = 0 };
+             SyncCombos(f);
+ 
+         {
+             }
+             {
+             {
+             }
+         }
-         MatTable table; string excelPath, tableError, manGroup = "SS"; bool dark; double manThick = 3.2; bool loading = true;
-             if (loading) return;
-             foreach (var f in items) Convert(f);
-             f.Row = table != null ? table.Find(f.Base) : null;
-             if (f.Row == null)   // khong co trong Excel -> dung vat lieu / do day nhap tay o thanh tren
-                 f.Row = new MatRow { Code = f.Base, MaterialText = "Nhập Tay", Group = Groups[Math.Max(0, cmbMat.SelectedIndex)], Thick = (double)nudThk.Value, Line = 0 };
```

## 19:37 - bien dich thanh cong
```diff
--- Ui.cs
+         public WarnList() { Font = new Font("Segoe UI", 10f); DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 30; BorderStyle = BorderStyle.None; IntegralHeight = false; DoubleBuffered = true; SelectionMode = SelectionMode.None; }
-         public WarnList() { Font = new Font("Segoe UI", 11f); DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 32; BorderStyle = BorderStyle.None; IntegralHeight = false; DoubleBuffered = true; SelectionMode = SelectionMode.None; }
```

## 19:39 - bien dich thanh cong
```diff
--- Ui.cs
+                 Col(Theme.Text) + Col(Theme.Muted) + Col(Theme.Accent) + Col(Theme.Dark ? Color.FromArgb(251, 146, 60) : Color.FromArgb(194, 98, 12)) + "}\\f0\\fs24\\cf1 ");
-                 Col(Theme.Text) + Col(Theme.Muted) + Col(Theme.Accent) + Col(Theme.Dark ? Color.FromArgb(251, 146, 60) : Color.FromArgb(194, 98, 12)) + "}\\f0\\fs20\\cf1 ");
```

## 19:42 - bien dich thanh cong
```diff
--- Program.cs
+             btnXls.Text = "Chọn File Excel"; btnXls.LeadIcon = 1; btnXls.Width = 180; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
-             btnXls.Text = "Chọn File Excel"; btnXls.Width = 150; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
--- Ui.cs
+         public bool Primary; public int Glyph, LeadIcon; bool hover, down;   // LeadIcon: 0 khong, 1 bieu tuong file Excel ben trai chu   // Glyph: 0 chu, 1 mat troi, 2 mat trang
+             if (Glyph == 0 && LeadIcon == 1)
+                 var sz = g.MeasureString(Text, Font); float iw = 17, ih = 21, gap = 9, x0 = (Width - (iw + gap + sz.Width)) / 2, y0 = (Height - ih) / 2;
+                 var doc = new RectangleF(x0, y0, iw, ih);
+                 using (var path = Gfx.RR(doc, 3)) using (var gb = new SolidBrush(Enabled ? (Theme.Dark ? Color.FromArgb(52, 168, 110) : Color.FromArgb(33, 115, 70)) : Theme.Muted)) g.FillPath(gb, path);
+                 using (var pen = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
+                     float cx2 = x0 + iw / 2, cy2 = y0 + ih / 2;
+                     g.DrawLine(pen, cx2 - 4.2f, cy2 - 4.8f, cx2 + 4.2f, cy2 + 4.8f); g.DrawLine(pen, cx2 + 4.2f, cy2 - 4.8f, cx2 - 4.2f, cy2 + 4.8f);
+                 using (var br = new SolidBrush(fg)) g.DrawString(Text, Font, br, new RectangleF(x0 + iw + gap, 0, sz.Width + 6, Height), Gfx.Left);
+                 return;
+                 {
+                 }
+             {
+             }
-         public bool Primary; public int Glyph; bool hover, down;   // Glyph: 0 chu, 1 mat troi, 2 mat trang
```

## 19:46 - bien dich thanh cong
```diff
--- Program.cs
+             btnXls.Text = "Chọn File Excel"; btnXls.LeadIcon = 1; btnXls.Width = 190; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
-             btnXls.Text = "Chọn File Excel"; btnXls.LeadIcon = 1; btnXls.Width = 180; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
--- Ui.cs
+         static Image xlsxIcon; static bool xlsxTried;
+         static Image XlsxIcon
+             get
+                 if (!xlsxTried)
+                     xlsxTried = true;
+                     try { using (var s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NCTool.xlsx_icon.png")) if (s != null) xlsxIcon = Image.FromStream(s); } catch { }
+                 return xlsxIcon;
+                 // bieu tuong file XLSX (anh nhung trong exe, nen trong suot)
+                 var sz = g.MeasureString(Text, Font); float ih = Height - 12, iw = ih, gap = 7, x0 = (Width - (iw + gap + sz.Width)) / 2;
+                 Image xi = XlsxIcon;
+                 if (xi != null)
+                     var oi = g.InterpolationMode; var op = g.PixelOffsetMode; g.InterpolationMode = InterpolationMode.HighQualityBicubic; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
+                     var dst = new Rectangle((int)x0, (int)((Height - ih) / 2), (int)iw, (int)ih);
+                     if (Enabled) g.DrawImage(xi, dst);
+                     else using (var ia = new System.Drawing.Imaging.ImageAttributes())
+                         ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new float[][] { new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { .3f, .3f, .3f, 0, 0 }, new float[] { 0, 0, 0, .6f, 0 }, new float[] { 0, 0, 0, 0, 1 } }));
+                         g.DrawImage(xi, dst, 0, 0, xi.Width, xi.Height, GraphicsUnit.Pixel, ia);
+                     g.InterpolationMode = oi; g.PixelOffsetMode = op;
+             }            if (Glyph == 0) { using (var br = new SolidBrush(fg)) g.DrawString(Text, Font, br, new RectangleF(0, 0, Width, Height), Gfx.Center); return; }
+                     {
+                     }
+                 {
+                 }
+         {
+             {
+         }
-                 var sz = g.MeasureString(Text, Font); float iw = 17, ih = 21, gap = 9, x0 = (Width - (iw + gap + sz.Width)) / 2, y0 = (Height - ih) / 2;
-                 var doc = new RectangleF(x0, y0, iw, ih);
-                 using (var path = Gfx.RR(doc, 3)) using (var gb = new SolidBrush(Enabled ? (Theme.Dark ? Color.FromArgb(52, 168, 110) : Color.FromArgb(33, 115, 70)) : Theme.Muted)) g.FillPath(gb, path);
-                 using (var pen = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
-                     float cx2 = x0 + iw / 2, cy2 = y0 + ih / 2;
-                     g.DrawLine(pen, cx2 - 4.2f, cy2 - 4.8f, cx2 + 4.2f, cy2 + 4.8f); g.DrawLine(pen, cx2 + 4.2f, cy2 - 4.8f, cx2 - 4.2f, cy2 + 4.8f);
-             if (Glyph == 0) { using (var br = new SolidBrush(fg)) g.DrawString(Text, Font, br, new RectangleF(0, 0, Width, Height), Gfx.Center); return; }
```

## 19:47 - bien dich thanh cong
```diff
--- Program.cs
+             btnXls.Text = "Chọn File Excel"; btnXls.LeadIcon = 1; btnXls.Width = 200; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
-             btnXls.Text = "Chọn File Excel"; btnXls.LeadIcon = 1; btnXls.Width = 190; btnXls.Height = 42; btnTheme.Height = 42; btnXls.Font = new Font("Segoe UI", 10.5f);
--- Ui.cs
+                 var sz = g.MeasureString(Text, Font); float ih = Height - 4, iw = ih, gap = 6, x0 = (Width - (iw + gap + sz.Width)) / 2;
-                 var sz = g.MeasureString(Text, Font); float ih = Height - 12, iw = ih, gap = 7, x0 = (Width - (iw + gap + sz.Width)) / 2;
```

## 19:51 - bien dich thanh cong
```diff
--- Ui.cs
+                     if (Enabled)   // to mau bieu tuong theo mau nhan cua giao dien (Theme.Accent)
+                         Color ac = Theme.Accent;
+                         using (var ia = new System.Drawing.Imaging.ImageAttributes())
+                         {
+                             ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new float[][] { new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { ac.R / 255f, ac.G / 255f, ac.B / 255f, 0, 1 } }));
+                             g.DrawImage(xi, dst, 0, 0, xi.Width, xi.Height, GraphicsUnit.Pixel, ia);
+                         }
+                     {
+                     }
-                     if (Enabled) g.DrawImage(xi, dst);
```
