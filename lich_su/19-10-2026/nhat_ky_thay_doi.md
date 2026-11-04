# Nhat ky thay doi ma nguon ngay 19-10-2026


## 19:37 - bien dich thanh cong
```diff
--- Core.cs
+         public Dictionary<string, double[]> Marks = new Dictionary<string, double[]>();   // diem danh dau LJ (phoi dai hep)
+         public List<double[]> Js = new List<double[]>();   // cac diem danh dau J (co the nhieu diem)
+                             if (tx == "LJ") P.Marks[tx] = m.Ap(Num(en, "10"), Num(en, "20"));
+                             else if (tx == "J") P.Js.Add(m.Ap(Num(en, "10"), Num(en, "20")));
+             if (P.Marks.Count > 0 || P.Js.Count > 0) P.Points.RemoveAll(pt => P.Marks.Values.Concat(P.Js).Any(mk => Dist(mk, Pt(pt.X, pt.Y)) < 0.05));   // POINT trung moi diem danh dau J / LJ khong phai lo trang
+                     double k1 = Math.Round(pt[ax] * 100) / 100; int k2 = kind != 0 ? 0 : 1;
+                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 12 });
+                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 12 });
+                 if (Dist(g.P, pt) < 1e-3) return new Split { Loop = loop.Skip(i).Concat(loop.Take(i)).ToList(), St = g.P, Ax = 0 };   // trung dinh: khong cat thanh doan do dai ~0
+                     if (d * g.R > 1e-3 && (g.Sweep - d) * g.R > 1e-3)
+         // Vi tri (chieu dai cung tinh tu dau bien dang) cua diem M nam tren canh thang cua bien ngoai
+         static double PathPos(List<Seg> sg, double[] M)
+             double cum = 0;
+             foreach (Seg g in sg)
+                 if (OnSeg(g, M, 1e-3)) return cum + Dist(g.P, M);
+                 cum += g.T == 'L' ? Dist(g.P, g.Q) : g.R * g.Sweep;
+             throw new NCError("Marker J/LJ không nằm trên cạnh thẳng của biên dạng ngoài");
+             foreach (var j in P.Js) o.Js.Add(f(j));
+         {
+                     if (prims0.Js.Count == 0) miss.Add("J");
+                 if (prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ") || An.OuterCircle != null || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
+             bool split = P.Js.Count > 0 && P.Marks.ContainsKey("LJ");
+                 // sap xep LJ va cac J theo chieu CW tren bien ngoai, bat dau tu LJ; moi cap lien tiep (marker -> marker ke tiep) la 1 duong cat
+                 double total = sg.Sum(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep), pLJ = PathPos(sg, P.Marks["LJ"]);
+                 var mk = new List<double[]> { P.Marks["LJ"] };
+                 mk.AddRange(P.Js.OrderBy(j => (PathPos(sg, j) - pLJ + 2 * total) % total));
+                 var pierces = new List<double[]>(); var seqs = new List<List<Seg>>();
+                 for (int k = 0; k < mk.Count; k++)
+                     double[] pk; List<Seg> sk;
+                     SplitPath(sg, mk[k], mk[(k + 1) % mk.Count], jointOut, pierceOut, out pk, out sk);   // duong k: bat dau tai marker k, dung truoc marker ke tiep
+                     pierces.Add(pk); seqs.Add(sk);
+                 for (int pi = 0; pi < mk.Count; pi++)
+                     var sq = seqs[pi]; double[] M0 = sq[0].P;
+                     pierce(pierces[pi], 9010);
+                 double[] Ee = null, nrm = null;
+                 // phoi gan tron: doan trai nhat la cung lon (tong cung cung tam/ban kinh quet >= 180 do) va diem 225 do nam tren cung -> quy tac phoi tron
+                 if (sg[ic].T == 'A' && sg[ic].R > 5 + 1e-9)
+                     Seg la = sg[ic]; double a225 = 1.25 * Math.PI;
+                     var same = sg.Where(q => q.T == 'A' && q.Ccw == la.Ccw && Dist(q.C, la.C) < 1e-3 && Math.Abs(q.R - la.R) < 1e-3).ToList();
+                     bool on225 = same.Any(q =>
+                         double ap225 = Math.Atan2(q.P[1] - q.C[1], q.P[0] - q.C[0]), ang225 = q.Ccw ? Mod(a225 - ap225, TAU) : Mod(ap225 - a225, TAU);
+                         return ang225 <= q.Sweep + 1e-9;
+                     });
+                     if (same.Sum(q => q.Sweep) >= Math.PI - 1e-6 && on225)
+                     {
+                         Ee = Pt(la.C[0] + la.R * Math.Cos(a225), la.C[1] + la.R * Math.Sin(a225)); nrm = Pt(Math.Cos(a225), Math.Sin(a225));
+                         warnings.Add("Phôi gần tròn (cung R" + Fmt(la.R) + " quét ≥ 180°): đục tại góc 225° của cung, hướng ra ngoài");
+                 if (Ee != null) { }
+                 else if (ed.T == 'L')
+                 {
+                     {
+                     }
+                 }
+                 if (g.T == 'L' && Math.Abs(g.P[0] - xmin) < 1e-4 && Math.Abs(g.Q[0] - xmin) < 1e-4) cand.Add(new[] { Math.Abs(g.Q[1] - g.P[1]), i, Math.Min(g.P[1], g.Q[1]) });
+             // canh dung dai nhat sat Oy; bang nhau (sai so 0.001) thi lay canh thap nhat (Y nho nhat)
+             var cbest = cand.OrderByDescending(c => Math.Round(c[0], 3)).ThenBy(c => c[2]).First();
+                 {
+                 }
+ 
+             }
+             {
+         }
-         public Dictionary<string, double[]> Marks = new Dictionary<string, double[]>();   // diem danh dau J / LJ (phoi dai hep)
-                             if (tx == "J" || tx == "LJ") P.Marks[tx] = m.Ap(Num(en, "10"), Num(en, "20"));
-             if (P.Marks.Count > 0) P.Points.RemoveAll(pt => P.Marks.Values.Any(mk => Dist(mk, Pt(pt.X, pt.Y)) < 0.05));   // POINT trung diem danh dau khong phai lo trang
-                     double k1 = Math.Round(pt[ax] * 1e6) / 1e6; int k2 = kind != 0 ? 0 : 1;
-                     add(new Pick { Pt = Pt((g.P[0] + g.Q[0]) / 2, (g.P[1] + g.Q[1]) / 2), Dir = Pt(-(g.Q[1] - g.P[1]) / len, (g.Q[0] - g.P[0]) / len), R = null, Tier = 11 });
-                     add(new Pick { Pt = Pt(mx, my), Dir = g.Ccw ? Pt(-Math.Cos(am), -Math.Sin(am)) : Pt(Math.Cos(am), Math.Sin(am)), R = g.Ccw ? (double?)g.R : null, Tier = 11 });
-                 if (Dist(g.P, pt) < 1e-6) return new Split { Loop = loop.Skip(i).Concat(loop.Take(i)).ToList(), St = g.P, Ax = 0 };
-                     if (d > 1e-9 && d < g.Sweep - 1e-9)
-                     if (!prims0.Marks.ContainsKey("J")) miss.Add("J");
-                 if (prims0.Marks.ContainsKey("J") && prims0.Marks.ContainsKey("LJ") || An.OuterCircle != null || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
-             bool split = P.Marks.ContainsKey("J") && P.Marks.ContainsKey("LJ");
-                 double[] pa, pb; List<Seg> sa, sb;
-                 SplitPath(sg, P.Marks["LJ"], P.Marks["J"], jointOut, pierceOut, out pa, out sa);   // duong 1: bat dau tai LJ, dung truoc J
-                 SplitPath(sg, P.Marks["J"], P.Marks["LJ"], jointOut, pierceOut, out pb, out sb);   // duong 2: bat dau tai J, dung truoc LJ
-                 for (int pi = 0; pi < 2; pi++)
-                     var sq = pi == 0 ? sa : sb; double[] M0 = sq[0].P;
-                     pierce(pi == 0 ? pa : pb, 9010);
-                 double[] Ee, nrm;
-                 if (ed.T == 'L')
-                 if (g.T == 'L' && Math.Abs(g.P[0] - xmin) < 1e-4 && Math.Abs(g.Q[0] - xmin) < 1e-4) cand.Add(new[] { Math.Abs(g.Q[1] - g.P[1]), i });
-             var cbest = cand.OrderBy(c => c[0]).ThenBy(c => c[1]).Last();
```

## 19:37 - bien dich thanh cong
```diff
--- Core.cs
+                 else if ((ax == 0 && Math.Abs(g.P[0] - g.Q[0]) < 1e-3 && Math.Abs(g.P[1] - g.Q[1]) > 1e-3) || (ax == 1 && Math.Abs(g.P[1] - g.Q[1]) < 1e-3 && Math.Abs(g.P[0] - g.Q[0]) > 1e-3))
-                 else if ((ax == 0 && Math.Abs(g.P[0] - g.Q[0]) < 1e-9) || (ax == 1 && Math.Abs(g.P[1] - g.Q[1]) < 1e-9))
```

## 19:39 - bien dich thanh cong
```diff
--- Core.cs
+                     if (d * g.R > 1e-3 && (g.Sweep - d) * g.R > 1e-3) cand.Add(new object[] { Pt(g.C[0] + g.R * Math.Cos(target), g.C[1] + g.R * Math.Sin(target)), 1, d });
-                     if (d > 1e-9 && d < g.Sweep - 1e-9) cand.Add(new object[] { Pt(g.C[0] + g.R * Math.Cos(target), g.C[1] + g.R * Math.Sin(target)), 1, d });
```
