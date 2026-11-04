# Nhat ky thay doi ma nguon ngay 21-10-2026


## 19:30 - bien dich thanh cong
```diff
--- Core.cs
+                 if (t >= 19 - 1e-9) { pierceOut = 10; jointOut = 3; pierceHole = 10; return; }   // SS >= 19: piercing cong, joint ngoai 3 (theo bang goc va ca 26-061939), piercing lo 10
+                 if (prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ") || An.OuterCircle != null || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-2 && Math.Abs(g.Q[0]) < 1e-2 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
+             if (Math.Abs(chShift[0]) > 1e-2 || Math.Abs(chShift[1]) > 1e-2)
+                     var seq = ReverseLoop(Chain(L));   // kegaki kin: nguoc chieu kim dong ho, duc ngay tai dinh nen khong co G41
+                     kegaki.Add(new Op { Pp = seq[0].P, Segs = seq, Code = 9017, Closed = true, G41 = false });
+                     // dau ccham bien ngoai: duc keo dai 3 mm nguoc chieu doan dau cua duong kegaki (khong theo phap tuyen cua bien)
+                     var cands = ends.Select(e =>
+                         double[] exDir = null;
+                         if (ExtendOut(e, sg, 3) != null) { var tg = TanAt(ChainOpenFrom(L, e)[0], false); exDir = Pt(e[0] - 3 * tg[0], e[1] - 3 * tg[1]); }
+                         return new { e, ext = exDir };
+                     }).ToList();
+                 else pos = emit(k.Segs, k.Pp, k.G41);
+                     {
+                 if (g.T == 'L' && Math.Abs(g.P[0] - xmin) < 1e-2 && Math.Abs(g.Q[0] - xmin) < 1e-2) cand.Add(new[] { Math.Abs(g.Q[1] - g.P[1]), i, Math.Min(g.P[1], g.Q[1]) });
+                     if (g.T == 'L' && Math.Abs(g.P[1] - bb.Y0) < 1e-2 && Math.Abs(g.Q[1] - bb.Y0) < 1e-2) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k });
-                 if (t >= 19 - 1e-9) { pierceOut = 10; jointOut = 0; pierceHole = 10; return; }   // SS >= 19: piercing cong, joint ngoai 0 (theo vi du), piercing lo 10
-                 if (prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ") || An.OuterCircle != null || An.Outer.Any(g => g.T == 'L' && Math.Abs(g.P[0]) < 1e-4 && Math.Abs(g.Q[0]) < 1e-4 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6))
-             if (Math.Abs(chShift[0]) > 1e-4 || Math.Abs(chShift[1]) > 1e-4)
-                     var seq = Chain(L);
-                     kegaki.Add(new Op { Pp = seq[0].P, Segs = seq, Code = 9017, Closed = true, G41 = true });
-                     var cands = ends.Select(e => new { e, ext = ExtendOut(e, sg, 3) }).ToList();
-                 else pos = emit(k.Segs, k.Pp, true);
-                 if (g.T == 'L' && Math.Abs(g.P[0] - xmin) < 1e-4 && Math.Abs(g.Q[0] - xmin) < 1e-4) cand.Add(new[] { Math.Abs(g.Q[1] - g.P[1]), i, Math.Min(g.P[1], g.Q[1]) });
-                     if (g.T == 'L' && Math.Abs(g.P[1] - bb.Y0) < 1e-4 && Math.Abs(g.Q[1] - bb.Y0) < 1e-4) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k });
```

## 19:43 - bien dich thanh cong
```diff
--- Excel.cs
+             string[] cu = { "CU", "DONG", "THAU", "C1100", "C2801", "BRASS", "COPPER", "銅", "黄銅", "真鍮", "シンチュウ", "C2600", "C2680", "C3604" };   // CU gom ca dong thau (真鍮): thong so giong dong
-             string[] cu = { "CU", "DONG", "THAU", "C1100", "C2801", "BRASS", "COPPER", "銅", "黄銅" };
--- Program.cs
+             cmbMat.DropDownStyle = ComboBoxStyle.DropDownList; cmbMat.FlatStyle = FlatStyle.Flat; cmbMat.Width = 190;
+             cmbMat.Items.AddRange(new object[] { "SS – Thép", "SUS – Inox", "AL – Nhôm", "CU – Đồng / Đồng thau" });
-             cmbMat.DropDownStyle = ComboBoxStyle.DropDownList; cmbMat.FlatStyle = FlatStyle.Flat; cmbMat.Width = 150;
-             cmbMat.Items.AddRange(new object[] { "SS – Thép", "SUS – Inox", "AL – Nhôm", "CU – Đồng" });
```
