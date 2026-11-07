# Nhat ky thay doi ma nguon ngay 07-11-2026


## 09:10 - bien dich thanh cong
```diff
--- Core.cs
+         class Analysis { public List<Seg> Outer; public BB Bb; public List<List<Seg>> Holes, Opens; public Circ OuterCircle; public bool Drawn; }
+         // Day >= 19: DXF co the da ve san duong dan vao piercing cong (2 doan thang + cung R3) va khe ho joint -> bien ngoai la duong ho, cat dung theo duong ve
+         static bool DrawnLeadMode;
+             if (DrawnLeadMode && openLoops.Count == 1 && Endpoints(openLoops[0]).Count == 2)
+                 var open = openLoops[0]; var oe = Endpoints(open); var ob = LoopBBox(open);
+                 double ocx = (ob.X0 + ob.X1) / 2, ocy = (ob.Y0 + ob.Y1) / 2;
+                 var ostart = oe.OrderByDescending(e => Dist(e, Pt(ocx, ocy))).First();   // dau tu do xa tam nhat = dau duong dan vao
+                 var chainO = ChainOpenFrom(open, ostart);
+                 int lead = 0;   // doan dan vao: cac doan dau chuoi toi het cung nho dau tien (trong 4 doan dau)
+                 for (int i = 0; i < Math.Min(4, chainO.Count); i++) if (chainO[i].T == 'A' && chainO[i].R <= 5 + 1e-9) { lead = i + 1; break; }
+                 var partO = chainO.Skip(lead).ToList(); if (partO.Count == 0) partO = chainO;
+                 var holesO = loops.Where(L => !IsOpen(L)).OrderByDescending(L => { var b = LoopBBox(L); return (b.X1 - b.X0) * (b.Y1 - b.Y0); }).ToList();
+                 return new Analysis { Outer = chainO, Bb = LoopBBox(partO), Holes = holesO, Opens = new List<List<Seg>>(), Drawn = true };
+             DrawnLeadMode = t >= 19 - 1e-9;
+             if (Curved(group, t) && !Analyze(prims0).Drawn)
+             {   // Duong bien ngoai da ve san duong dan vao piercing cong (day >= 19): khong xoay, khong xet canh trai, cat dung theo duong ve
+                 var aD = Analyze(prims0);
+                 if (aD.Drawn)
+                     double[] shiftD = new double[] { 0, 0 }; Prims PrD = prims0;
+                     if (Math.Abs(aD.Bb.X0) > 1e-4 || Math.Abs(aD.Bb.Y0) > 1e-4) { shiftD = new[] { -aD.Bb.X0, -aD.Bb.Y0 }; PrD = Translate(prims0, shiftD[0], shiftD[1]); }
+                     P = PrD; A = Analyze(PrD); chShift = shiftD;
+                     warnings.Add("Biên ngoài là đường hở có sẵn đường dẫn vào piercing cong (dày ≥ 19): không xoay phôi, đục tại đầu tự do và cắt đúng theo đường vẽ");
+             if (A == null && !(prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ")))
+             if (A.Drawn)
+                 double[] psD = sg[0].P;   // dau tu do cua duong ve
+                 pierce(psD, 9010);
+                 pos = emit(sg, psD, true);
+             else if (split)
+                 body.Add("M121"); body.Add("M199"); body.Add("G01G40"); body.Add("M30");
+                 {
+                 }
+             }
+             }
+             {
+             }
+             {
-         class Analysis { public List<Seg> Outer; public BB Bb; public List<List<Seg>> Holes, Opens; public Circ OuterCircle; }
-             if (!(prims0.Js.Count > 0 && prims0.Marks.ContainsKey("LJ")))
-             if (split)
-             if (Curved(group, t))
```

## 09:11 - bien dich thanh cong
```diff
--- Core.cs
+                 if (sa.Count + sb.Count > 2) continue;   // moi nut chi noi toi da 2 dau mut (khe ho joint 0.005 mm cua duong dan vao piercing cong khong bi lap kin, ca ViDu_AL_t19)
```
