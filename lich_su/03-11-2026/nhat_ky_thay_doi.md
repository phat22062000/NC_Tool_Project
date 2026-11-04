# Nhat ky thay doi ma nguon ngay 03-11-2026


## 19:49 - bien dich thanh cong
```diff
--- Core.cs
+                 // canh ngan nhat (sai so 0.01), bang nhau thi canh thap nhat (trung diem Y nho nhat) - ca 26-062754 va 26-065394
+                 int lo = 0; double bestLen = double.MaxValue, bestMidY = double.MaxValue;
+                 for (int i = 0; i < n; i++)
+                     double li = Dist(loop[i].P, loop[i].Q), my = (loop[i].P[1] + loop[i].Q[1]) / 2;
+                     if (li < bestLen - 0.01 || (Math.Abs(li - bestLen) <= 0.01 && my < bestMidY)) { lo = i; bestLen = li; bestMidY = my; }
+                         // phoi co bien trai cong (duc o canh day): lo tron thu ben PHAI truoc (ca 26-065395, 26-065394); con lai trai truoc
+                         double[] angs = splBottom ? new[] { 0, Math.PI, 1.5 * Math.PI, 0.5 * Math.PI } : new[] { Math.PI, 0, 1.5 * Math.PI, 0.5 * Math.PI };
+                         double chosen = okc != null ? okc.ang : (ch.HasJoint ? dl.OrderByDescending(x => x.d).First().ang : angs[0]);
+                 {
+                     if (g.T == 'L' && !g.Spl && Math.Abs(g.P[1] - bb.Y0) < 1e-2 && Math.Abs(g.Q[1] - bb.Y0) < 1e-2) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k, Math.Min(g.P[0], g.Q[0]) });
+                 }
+                 var bestB = cb.OrderBy(c => c[2]).ThenByDescending(c => c[0]).First();   // canh day trai nhat tren Ox (ca 26-065395), giong canh trai thap nhat
-                 int lo = 0;
-                 for (int i = 1; i < n; i++) if (loop[i].P[1] < loop[lo].P[1] - 1e-9) lo = i;
-                         double[] angs = { Math.PI, 0, 1.5 * Math.PI, 0.5 * Math.PI };
-                         double chosen = okc != null ? okc.ang : (ch.HasJoint ? dl.OrderByDescending(x => x.d).First().ang : Math.PI);
-                     if (g.T == 'L' && !g.Spl && Math.Abs(g.P[1] - bb.Y0) < 1e-2 && Math.Abs(g.Q[1] - bb.Y0) < 1e-2) cb.Add(new[] { Math.Abs(g.Q[0] - g.P[0]), k });
-                 var bestB = cb.OrderBy(c => c[0]).ThenBy(c => c[1]).Last();
```

## 19:59 - bien dich thanh cong
```diff
--- Core.cs
+                         double[] angs = { Math.PI, 0, 1.5 * Math.PI, 0.5 * Math.PI };
-                         // phoi co bien trai cong (duc o canh day): lo tron thu ben PHAI truoc (ca 26-065395, 26-065394); con lai trai truoc
-                         double[] angs = splBottom ? new[] { 0, Math.PI, 1.5 * Math.PI, 0.5 * Math.PI } : new[] { Math.PI, 0, 1.5 * Math.PI, 0.5 * Math.PI };
```
