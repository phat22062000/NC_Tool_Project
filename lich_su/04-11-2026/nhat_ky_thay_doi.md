# Nhat ky thay doi ma nguon ngay 04-11-2026


## 18:27 - bien dich thanh cong
```diff
--- Core.cs
+                     // chi noi dau mut giua cac doan cung mau (kegaki khong bi dinh vao bien ngoai khi cach 0.01 mm, ca 26-065663)
+                     if (dx * dx + dy * dy <= tol * tol && P.Segs[(int)list[ord[a]][2] / 2].Col == P.Segs[(int)list[ord[b]][2] / 2].Col) pairs.Add(new double[] { dx * dx + dy * dy, ord[a], ord[b] });
-                     if (dx * dx + dy * dy <= tol * tol) pairs.Add(new double[] { dx * dx + dy * dy, ord[a], ord[b] });
```
