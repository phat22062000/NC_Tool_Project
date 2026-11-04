# Nhat ky thay doi ma nguon ngay 01-11-2026


## 10:21 - bien dich thanh cong
```diff
--- Core.cs
+         // Giao cua duong thang keo dai cua doan thang `line` voi doan ke `other` (thang hoac cung); chon giao diem gan `near` nhat
+         static double[] VirtualVertex(Seg line, Seg other, double[] near)
+             double dx = line.Q[0] - line.P[0], dy = line.Q[1] - line.P[1];
+             var cands = new List<double[]>();
+             if (other.T == 'L')
+                 double ex = other.Q[0] - other.P[0], ey = other.Q[1] - other.P[1], den = dx * ey - dy * ex;
+                 if (Math.Abs(den) < 1e-12) return null;
+                 double t = ((other.P[0] - line.P[0]) * ey - (other.P[1] - line.P[1]) * ex) / den;
+                 cands.Add(Pt(line.P[0] + t * dx, line.P[1] + t * dy));
+                 double fx = line.P[0] - other.C[0], fy = line.P[1] - other.C[1];
+                 double a = dx * dx + dy * dy, b = 2 * (dx * fx + dy * fy), c = fx * fx + fy * fy - other.R * other.R, disc = b * b - 4 * a * c;
+                 if (disc < 0) return null;
+                 double sq = Math.Sqrt(disc);
+                 foreach (double t in new[] { (-b - sq) / (2 * a), (-b + sq) / (2 * a) }) cands.Add(Pt(line.P[0] + t * dx, line.P[1] + t * dy));
+             return cands.OrderBy(q => Dist(q, near)).First();
+         {
+                     // Diem vao (va vi tri joint neu co): (1) cach bien dang khac > 3 mm; (2) trai, phai, duoi, tren. Ap dung cho moi lo tron (ca 26-063378)
+                         double chosen = okc != null ? okc.ang : (ch.HasJoint ? dl.OrderByDescending(x => x.d).First().ang : Math.PI);
+                         string cs = (2 * c.R).ToString("0.##", Inv), what = ch.HasJoint ? "joint" : "điểm vào";
+                         if (okc == null) { if (ch.HasJoint) warnings.Add("Joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(dl.Max(x => x.d)) + " mm (≤ 3 mm), không có vị trí tốt hơn"); }
+                         else if (dl[0].d <= 3 + 1e-9) warnings.Add("Đã xoay " + what + " lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
+                     body.Add("G01G41");   // kegaki tron: giu dong G01G41 dung rieng (mau 26-064456), kegaki thang/kin khong G41
+                     if (Le > 100 + 1e-9)
+                         // canh thang > 100: cach DINH DUOI cua canh 40 mm (theo duong thang). Dinh = giao cua canh voi canh ke ben duoi, bo qua cung bo goc R nho
+                         // (ca 26-064456: canh trai nghieng, vao cach dinh duoi 40 mm chu khong phai dau tren)
+                         bool pLower = ed.P[1] <= ed.Q[1];
+                         int nb = pLower ? (ie - 1 + n0) % n0 : (ie + 1) % n0;
+                         for (int gd = 0; gd < n0 && sg[nb].T == 'A' && sg[nb].R <= 5 + 1e-9; gd++) nb = pLower ? (nb - 1 + n0) % n0 : (nb + 1) % n0;
+                         double[] lowEnd = pLower ? ed.P : ed.Q, hiEnd = pLower ? ed.Q : ed.P;
+                         double[] vv = VirtualVertex(ed, sg[nb], lowEnd);
+                         if (vv == null || Dist(vv, lowEnd) > 3) vv = lowEnd;
+                         double ln = Dist(vv, hiEnd);
+                         Ee = Pt(vv[0] + (hiEnd[0] - vv[0]) / ln * 40, vv[1] + (hiEnd[1] - vv[1]) / ln * 40);
+                     {
+                     }
+                 var cbest = cand.OrderBy(c => c[2]).ThenByDescending(c => c[0]).First();   // canh dung thap nhat sat Oy (ca 26-063416)
+             else
+             }
+ 
+             {
+             }
+             {
+         }
-                     if (ch.HasJoint)   // vi tri joint: (1) cach bien dang khac > 3 mm; (2) trai, phai, duoi, tren
-                         double chosen = okc != null ? okc.ang : dl.OrderByDescending(x => x.d).First().ang;
-                         string cs = (2 * c.R).ToString("0.##", Inv);
-                         if (okc == null) warnings.Add("Joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") cách biên dạng khác " + Fmt(dl.Max(x => x.d)) + " mm (≤ 3 mm), không có vị trí tốt hơn");
-                         else if (dl[0].d <= 3 + 1e-9) warnings.Add("Đã xoay joint lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
-                 var cbest = cand.OrderByDescending(c => Math.Round(c[0], 3)).ThenBy(c => c[2]).First();
```

## 10:26 - bien dich thanh cong
```diff
--- Core.cs
-                     body.Add("G01G41");   // kegaki tron: giu dong G01G41 dung rieng (mau 26-064456), kegaki thang/kin khong G41
```
