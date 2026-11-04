# Nhat ky thay doi ma nguon ngay 24-10-2026


## 17:40 - bien dich thanh cong
```diff
--- Core.cs
+             bool tri = false;
+                         tri = true;
+             // Lo nghieng: toan canh thang, khong co canh nao song song Ox/Oy, khong co phan nho -> vao giua canh bat dau tu dinh thap nhat (theo chieu cat nguoc kim dong ho)
+             if (!tri && caps.Count == 0 && loop.All(g => g.T == 'L' && Math.Abs(g.P[0] - g.Q[0]) > 1e-3 && Math.Abs(g.P[1] - g.Q[1]) > 1e-3))
+                 int lo = 0;
+                 for (int i = 1; i < n; i++) if (loop[i].P[1] < loop[lo].P[1] - 1e-9) lo = i;
+                 Seg g = loop[lo]; double len = Dist(g.P, g.Q);
+                 add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 11 });
+             }
+             {
```

## 17:41 - bien dich thanh cong
```diff
--- Core.cs
+                 add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 10 });
-                 add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 11 });
```

## 17:49 - bien dich thanh cong
```diff
--- Core.cs
+         // ---------- SPLINE -> duong gap khuc ----------
+         // Chia doi tham so de quy: mot doan [ua, ub] duoc chia doi khi do vong lon nhat so voi day cung (do tai 1/4, 1/2, 3/4 tham so) > SplineTol.
+         // Khop voi ViDu_Spline.nc (dung cho moi diem, 165 diem chia).
+         const double SplineTol = 0.01;
+         static double Sag(double[] a, double[] m, double[] b)
+             double dx = b[0] - a[0], dy = b[1] - a[1], L = Math.Sqrt(dx * dx + dy * dy);
+             if (L < 1e-12) return Dist(a, m);
+             return Math.Abs((m[0] - a[0]) * dy - (m[1] - a[1]) * dx) / L;
+         static List<double[]> SplinePolyline(List<double[]> cps, List<double> wts, List<double> kn, int deg, List<double[]> fit)
+             int n = cps.Count - 1;
+             if (deg < 1 || n < deg || kn.Count != cps.Count + deg + 1)
+                 if (fit.Count >= 2) return new List<double[]>(fit);   // khong du du lieu B-spline: noi thang cac diem fit
+                 throw new NCError("SPLINE thiếu dữ liệu điểm điều khiển/knot. Hãy Explode hoặc lưu lại DXF.");
+             bool rational = wts.Count == cps.Count;
+             double u0 = kn[deg], u1 = kn[n + 1];
+             Func<double, double[]> eval = u =>
+                 int k = deg;
+                 if (u >= u1) { k = n; while (k > deg && !(kn[k] < kn[k + 1])) k--; }
+                 else for (int i = deg; i <= n; i++) if (u >= kn[i] && u < kn[i + 1]) { k = i; break; }
+                 var d = new double[deg + 1][];
+                 for (int j = 0; j <= deg; j++) { var c = cps[j + k - deg]; double w = rational ? wts[j + k - deg] : 1; d[j] = new[] { c[0] * w, c[1] * w, w }; }
+                 for (int r = 1; r <= deg; r++)
+                     for (int j = deg; j >= r; j--)
+                         double den = kn[j + 1 + k - r] - kn[j + k - deg];
+                         double a = den == 0 ? 0 : (u - kn[j + k - deg]) / den;
+                         for (int t = 0; t < 3; t++) d[j][t] = (1 - a) * d[j - 1][t] + a * d[j][t];
+                 return Pt(d[deg][0] / d[deg][2], d[deg][1] / d[deg][2]);
+             Action<double, double, double[], double[], int> rec = null;
+             rec = (ua, ub, pa, pb, depth) =>
+                 double um = (ua + ub) / 2; var pm = eval(um);
+                 var q1 = eval((ua + um) / 2); var q3 = eval((um + ub) / 2);
+                 double dev = Math.Max(Sag(pa, pm, pb), Math.Max(Sag(pa, q1, pb), Sag(pa, q3, pb)));
+                 if (dev > SplineTol && depth < 16) { rec(ua, um, pa, pm, depth + 1); res.Add(pm); rec(um, ub, pm, pb, depth + 1); }
+             var s0 = eval(u0); var s1 = eval(u1);
+             res.Add(s0); rec(u0, u1, s0, s1, 0); res.Add(s1);
+             int expanded = 0, splineSegs = 0, splineCount = 0;
+                     case "SPLINE":
+                         {
+                             if (dashed) { ign("đường nét đứt (đường uốn)"); break; }
+                             if (col == "other") { ign("đối tượng màu khác (không phải by layer/xanh/đỏ)"); break; }
+                             int deg = 3; int.TryParse(Get(en, "71") ?? "3", out deg);
+                             var kn = new List<double>(); var cps = new List<double[]>(); var wts = new List<double>(); var fit = new List<double[]>();
+                             foreach (var kv in en.G)
+                             {
+                                 double dv; if (!double.TryParse(kv.Value, NumberStyles.Float, Inv, out dv)) continue;
+                                 if (kv.Key == "40") kn.Add(dv);
+                                 else if (kv.Key == "41") wts.Add(dv);
+                                 else if (kv.Key == "10") cps.Add(new[] { dv, 0 });
+                                 else if (kv.Key == "20" && cps.Count > 0) cps[cps.Count - 1][1] = dv;
+                                 else if (kv.Key == "11") fit.Add(new[] { dv, 0 });
+                                 else if (kv.Key == "21" && fit.Count > 0) fit[fit.Count - 1][1] = dv;
+                             }
+                             var poly = SplinePolyline(cps.Select(c => m.Ap(c[0], c[1])).ToList(), wts, kn, deg, fit.Select(c => m.Ap(c[0], c[1])).ToList());
+                             int segs = 0;
+                             for (int k = 0; k + 1 < poly.Count; k++)
+                                 if (Dist(poly[k], poly[k + 1]) > 1e-9) { P.Segs.Add(new Seg { T = 'L', P = poly[k], Q = poly[k + 1], Col = col }); segs++; }
+                             splineSegs += segs; splineCount++;
+                             break;
+                         }
+                     case "POLYLINE": case "ELLIPSE":
+             if (splineCount > 0) P.Warnings.Add("Đã chuyển " + splineCount + " spline thành " + splineSegs + " đoạn thẳng (sai số võng ≤ " + SplineTol.ToString("0.##", Inv) + " mm)");
+             var res = new List<double[]>();
+             return res;
+         {
+         }
+         {
+                     {
+                     }
+             {
+             };
+ 
+             {
+             }
+             {
+             };
+         }
-             int expanded = 0;
-                     case "POLYLINE": case "SPLINE": case "ELLIPSE":
```

## 18:01 - bien dich thanh cong
```diff
--- Core.cs
+         public bool Spl;             // doan thang sinh ra tu SPLINE
+         public List<double[]> Hull = new List<double[]>();   // diem dieu khien cua SPLINE (dung de dat goc toa do)
+                             var cpsT = cps.Select(c => m.Ap(c[0], c[1])).ToList();
+                             var poly = SplinePolyline(cpsT, wts, kn, deg, fit.Select(c => m.Ap(c[0], c[1])).ToList());
+                             P.Hull.AddRange(cpsT);
+                                 if (Dist(poly[k], poly[k + 1]) > 1e-9) { P.Segs.Add(new Seg { T = 'L', P = poly[k], Q = poly[k + 1], Col = col, Spl = true }); segs++; }
+             foreach (var h in P.Hull) o.Hull.Add(f(h));
+             // Bien ngoai co SPLINE loi ra trai nhat (khong co canh thang dung sat diem trai nhat) va co canh day nam tren Ox:
+             // khong xoay, dat goc theo bao diem dieu khien, duc o canh day (ca ViDu_Spline_1)
+             bool splBottom = false;
+             if (prims0.Hull.Count > 0 && !(prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ")))
+                 var a0 = Analyze(prims0);
+                 if (a0.OuterCircle == null)
+                     double x0 = a0.Bb.X0, y0 = a0.Bb.Y0;
+                     var atLeft = a0.Outer.Where(g => LoopBBox(new List<Seg> { g }).X0 <= x0 + 1e-2).ToList();
+                     bool hasBottom = a0.Outer.Any(g => !g.Spl && g.T == 'L' && Math.Abs(g.P[1] - y0) < 1e-2 && Math.Abs(g.Q[1] - y0) < 1e-2 && Math.Abs(g.P[0] - g.Q[0]) >= 10);
+                     if (atLeft.Count > 0 && atLeft.All(g => g.Spl) && hasBottom)
+                         double hx = x0, hy = y0;
+                         foreach (var h in prims0.Hull) { hx = Math.Min(hx, h[0]); hy = Math.Min(hy, h[1]); }
+                         double[] shift = new double[] { 0, 0 }; Prims Pr = prims0;
+                         if (Math.Abs(hx) > 1e-4 || Math.Abs(hy) > 1e-4) { shift = new[] { -hx, -hy }; Pr = Translate(prims0, shift[0], shift[1]); }
+                         P = Pr; A = Analyze(Pr); chShift = shift; splBottom = true;
+                         warnings.Add("Điểm trái nhất của biên ngoài là spline: không xoay phôi, đục ở cạnh đáy nằm trên Ox");
+             var xs = new List<double> { bb.X0, bb.X1, 0 }; var ys = new List<double> { bb.Y0, bb.Y1, 0 };   // goc (0,0) cung tinh vao kich thuoc tieu de
+                     {
+                     }
+             double Lr = 0; int i0 = 0;
+             if (!splBottom)
+                 var cbest = cand.OrderByDescending(c => Math.Round(c[0], 3)).ThenBy(c => c[2]).First();
+                 Lr = cbest[0]; i0 = (int)cbest[1];
+             double[] a = sg[i0].P, b2 = sg[i0].Q, start, end, pp2;   // splBottom: Lr = 0 nen vao nhanh canh day
+                 {
+                     if (g.T == 'L' && !g.Spl && Math.Abs(g.P[1] - bb.Y0) < 1e-2 && Math.Abs(g.Q[1] - bb.Y0) < 1e-2) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k });
+                 }
+             }
+             {
+             }
+             {
-                             var poly = SplinePolyline(cps.Select(c => m.Ap(c[0], c[1])).ToList(), wts, kn, deg, fit.Select(c => m.Ap(c[0], c[1])).ToList());
-                                 if (Dist(poly[k], poly[k + 1]) > 1e-9) { P.Segs.Add(new Seg { T = 'L', P = poly[k], Q = poly[k + 1], Col = col }); segs++; }
-             var xs = new List<double> { bb.X0, bb.X1 }; var ys = new List<double> { bb.Y0, bb.Y1 };
-             var cbest = cand.OrderByDescending(c => Math.Round(c[0], 3)).ThenBy(c => c[2]).First();
-             double Lr = cbest[0]; int i0 = (int)cbest[1];
-             double[] a = sg[i0].P, b2 = sg[i0].Q, start, end, pp2;
-                     if (g.T == 'L' && Math.Abs(g.P[1] - bb.Y0) < 1e-2 && Math.Abs(g.Q[1] - bb.Y0) < 1e-2) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k });
```

## 18:05 - bien dich thanh cong
```diff
--- Core.cs
+             // Bien ngoai co SPLINE hoac CUNG loi ra trai nhat (khong co canh thang dung sat diem trai nhat) va co canh day nam tren Ox:
+             if (!(prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ")))
+                     if (atLeft.Count > 0 && atLeft.All(g => g.Spl || g.T == 'A') && hasBottom)
+                         warnings.Add("Điểm trái nhất của biên ngoài là đường cong (spline/cung), không có cạnh thẳng đứng chạm điểm đó: không xoay phôi, đục ở cạnh đáy nằm trên Ox");
-             // Bien ngoai co SPLINE loi ra trai nhat (khong co canh thang dung sat diem trai nhat) va co canh day nam tren Ox:
-             if (prims0.Hull.Count > 0 && !(prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ")))
-                     if (atLeft.Count > 0 && atLeft.All(g => g.Spl) && hasBottom)
-                         warnings.Add("Điểm trái nhất của biên ngoài là spline: không xoay phôi, đục ở cạnh đáy nằm trên Ox");
```
