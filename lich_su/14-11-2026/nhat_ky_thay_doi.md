# Nhat ky thay doi ma nguon ngay 14-11-2026


## 12:36 - bien dich thanh cong
```diff
--- Core.cs
+ using System.Text.RegularExpressions;
+         // Khach hang dac biet (cot 得意先 trong Excel). 824: khong tao joint cho lo ben trong (moi vat lieu, ke ca SUS, AL, CU)
+         public static bool NoHoleJointCustomer(string customer) { return !string.IsNullOrEmpty(customer) && Regex.IsMatch(customer, @"^\s*0*824(\D|$)"); }
+         public static Result Generate(Prims prims0, string name, string group, double t) { return Generate(prims0, name, group, t, null); }
+         public static Result Generate(Prims prims0, string name, string group, double t, string customer)
+             if (NoHoleJointCustomer(customer)) { jointHole = 0; warnings.Add("Khách hàng 824: không tạo joint cho lỗ bên trong"); }
+ 
+ 
-         public static Result Generate(Prims prims0, string name, string group, double t)
--- Excel.cs
+         public string Code, MaterialText, Group, Customer;   // Customer: ma khach hang (cot 得意先 / Khach hang)
+             int hdr = -1, cCode = 0, cMat = 1, cThk = 2, cCus = -1;
+                 int a = -1, b = -1, c = -1, cu = -1;
+                     if (cu < 0 && Regex.IsMatch(h, @"得意先|客先|顧客|khach hang|customer")) { cu = i; continue; }
+                 if (a >= 0 && b >= 0 && c >= 0) { hdr = r; cCode = a; cMat = b; cThk = c; cCus = cu; }
+                 else T.Rows.Add(new MatRow { Code = code, MaterialText = mat, Group = g, Thick = t, Line = r + 1, Customer = cCus >= 0 ? cell(cCus) : "" });
-         public string Code, MaterialText, Group;
-             int hdr = -1, cCode = 0, cMat = 1, cThk = 2;
-                 int a = -1, b = -1, c = -1;
-                 if (a >= 0 && b >= 0 && c >= 0) { hdr = r; cCode = a; cMat = b; cThk = c; }
-                 else T.Rows.Add(new MatRow { Code = code, MaterialText = mat, Group = g, Thick = t, Line = r + 1 });
--- Program.cs
+             MatRow tr = table != null ? table.Find(f.Base) : null;   // dong Excel (van dung de lay khach hang khi nguoi dung chinh tay vat lieu / do day)
+             f.Row = !f.Override ? tr : null;
+                 f.Row = new MatRow { Code = f.Base, MaterialText = "Nhập Tay", Group = f.MGroup, Thick = f.MThick, Line = 0, Customer = tr != null ? tr.Customer : null };
+             try { f.Error = null; f.Result = Core.Generate(f.Prims, f.Base, f.Row.Group, f.Row.Thick, f.Row.Customer); }
+                     string bn = System.IO.Path.GetFileNameWithoutExtension(args[1]); string grp = args[3]; double th; string cus = args.Length >= 6 ? args[5] : null;   // tuy chon: ma khach hang (vd 824)
+                         grp = row.Group; th = row.Thick; cus = row.Customer;
+                     var r = Core.Generate(P, bn, grp, th, cus);
+                 try { var T = MatTable.Load(args[1]); File.WriteAllLines(args[2], T.Rows.Select(r => r.Line + "|" + r.Code + "|" + r.MaterialText + "|" + r.Group + "|" + r.Thick + "|" + r.Customer).Concat(T.Warnings), Encoding.UTF8); }
-             f.Row = (!f.Override && table != null) ? table.Find(f.Base) : null;
-                 f.Row = new MatRow { Code = f.Base, MaterialText = "Nhập Tay", Group = f.MGroup, Thick = f.MThick, Line = 0 };
-             try { f.Error = null; f.Result = Core.Generate(f.Prims, f.Base, f.Row.Group, f.Row.Thick); }
-                     string bn = System.IO.Path.GetFileNameWithoutExtension(args[1]); string grp = args[3]; double th;
-                         grp = row.Group; th = row.Thick;
-                     var r = Core.Generate(P, bn, grp, th);
-                 try { var T = MatTable.Load(args[1]); File.WriteAllLines(args[2], T.Rows.Select(r => r.Line + "|" + r.Code + "|" + r.MaterialText + "|" + r.Group + "|" + r.Thick).Concat(T.Warnings), Encoding.UTF8); }
```

## 12:44 - bien dich thanh cong
```diff
--- Core.cs
+             // Khong tu xoay hinh (tu 14-11-2026): DXF the nao thi giu nguyen huong the do. Chi thu huong goc (rot = 0);
+             // khong co canh thang dung sat Oy thi dung quy tac phoi cong / xien ben duoi (noLeft)
+             for (int rot = 0; rot < 1 && A == null; rot++)
-             for (int rot = 0; rot < 4 && A == null; rot++)
--- Excel.cs
+             mat = (mat ?? "").Normalize(NormalizationForm.FormKC);   // chu toan goc (ＡＬ, ＳＵＳ...) -> chu thuong
+             string m = mat.Trim().ToUpperInvariant().Replace(" ", "");
+             string[] al = { "A5052", "A505", "A2017", "A1050", "AL", "NHOM", "アルミ" };   // A1050, ＡＬその他 (AL sonota) tinh nhu nhom
-             string m = (mat ?? "").Trim().ToUpperInvariant().Replace(" ", "");
-             string[] al = { "A5052", "A505", "A2017", "AL", "NHOM", "アルミ" };
```

## 12:56 - bien dich thanh cong
```diff
--- Core.cs
+                                 : caps.OrderBy(c => Math.Round(c.r, 3)).ThenByDescending(c => Math.Round(c.y, 4)).ThenBy(c => c.x);   // lo dung: dau tren truoc (ca 26-067424)
+                     if (atLeft.Count > 0 && !atLeft.Any(g => g.T == 'L' && !g.Spl && Math.Abs(g.P[0] - x0) < 1e-2 && Math.Abs(g.Q[0] - x0) < 1e-2 && Math.Abs(g.P[1] - g.Q[1]) > 1e-6) && hasBottom)
+                         warnings.Add("Biên ngoài không có cạnh thẳng đứng chạm điểm trái nhất (trái là đường cong hoặc cạnh xiên): không xoay phôi, đục ở cạnh đáy nằm trên Ox");
+                         var okc = dl.FirstOrDefault(x => x.d >= 3 - 1e-6);
+                         else if (dl[0].d < 3 - 1e-6) warnings.Add("Đã xoay " + what + " lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
+                     var okList = info.Where(x => x.d >= 3 - 1e-6).OrderBy(x => x.cd.Tier).ThenBy(x => x.cd.Tier < 10 ? 0 : x.rank).ToList();   // phan nho (cap): theo thu tu truc, khong theo huong tam
+                     var wish = info.OrderBy(x => x.cd.Tier).ThenBy(x => x.cd.Tier < 10 ? 0 : x.rank).First();   // vi tri mong muon neu khong xet khoang cach
+                         if (wish.d < 3 - 1e-6 && wish.cd != sel) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
-                                 : caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.y, 4)).ThenBy(c => c.x);
-                     if (atLeft.Count > 0 && atLeft.All(g => g.Spl || g.T == 'A') && hasBottom)
-                         warnings.Add("Điểm trái nhất của biên ngoài là đường cong (spline/cung), không có cạnh thẳng đứng chạm điểm đó: không xoay phôi, đục ở cạnh đáy nằm trên Ox");
-                         var okc = dl.FirstOrDefault(x => x.d > 3 + 1e-9);
-                         else if (dl[0].d <= 3 + 1e-9) warnings.Add("Đã xoay " + what + " lỗ tròn φ" + cs + " tại (" + Fmt(c.Cx) + ", " + Fmt(c.Cy) + ") sang vị trí khác vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
-                     var okList = info.Where(x => x.d > 3 + 1e-9).OrderBy(x => x.cd.Tier).ThenBy(x => x.rank).ToList();
-                     var wish = info.OrderBy(x => x.cd.Tier).ThenBy(x => x.rank).First();   // vi tri mong muon neu khong xet khoang cach
-                         if (wish.d <= 3 + 1e-9 && wish.cd != sel) warnings.Add("Đã xoay joint lỗ về điểm (" + Fmt(sel.Pt[0]) + ", " + Fmt(sel.Pt[1]) + ") vì vị trí ưu tiên cách biên dạng khác ≤ 3 mm");
```

## 12:58 - bien dich thanh cong
```diff
--- Core.cs
+                 var grp = remH.Where(h => Math.Abs(h.Area - remH[0].Area) < Math.Max(1e-3, 0.06 * remH[0].Area)).ToList();
-                 var grp = remH.Where(h => Math.Abs(h.Area - remH[0].Area) < 1e-3).ToList();
```

## 12:59 - bien dich thanh cong
```diff
--- Core.cs
+         static List<Pick> EntryCandidates(List<Seg> loop, bool vertTop)
+                                 : (vertTop ? caps.OrderBy(c => Math.Round(c.r, 3)).ThenByDescending(c => Math.Round(c.y, 4)).ThenBy(c => c.x)   // lo dung khong joint: dau tren truoc (ca 26-067424)
+                                            : caps.OrderBy(c => Math.Round(c.r, 3)).ThenBy(c => Math.Round(c.y, 4)).ThenBy(c => c.x));   // lo dung co joint: dau duoi truoc (Vi_Du_1)
+                 var cands = EntryCandidates(ccw, !hasJ);
-         static List<Pick> EntryCandidates(List<Seg> loop)
-                                 : caps.OrderBy(c => Math.Round(c.r, 3)).ThenByDescending(c => Math.Round(c.y, 4)).ThenBy(c => c.x);   // lo dung: dau tren truoc (ca 26-067424)
-                 var cands = EntryCandidates(ccw);
```
