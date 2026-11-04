# Nhat ky thay doi ma nguon ngay 28-10-2026


## 18:34 - bien dich thanh cong
```diff
--- Core.cs
+                 // He toa do doi tuong (OCS) theo vector phap tuyen 210/220/230 (thuat toan truc tuy y cua AutoCAD): CIRCLE/ARC/LWPOLYLINE/POINT ve
+                 // trong UCS bi lat guong (230 = -1) co hoanh do dao dau
+                 double nx = 0, ny = 0, nz = 1;
+                 if (Get(en, "230") != null) { double.TryParse(Get(en, "210") ?? "0", NumberStyles.Float, Inv, out nx); double.TryParse(Get(en, "220") ?? "0", NumberStyles.Float, Inv, out ny); double.TryParse(Get(en, "230"), NumberStyles.Float, Inv, out nz); }
+                 double nl = Math.Sqrt(nx * nx + ny * ny + nz * nz); if (nl < 1e-12) { nx = 0; ny = 0; nz = 1; nl = 1; }
+                 nx /= nl; ny /= nl; nz /= nl;
+                 double[] axv, ayv;
+                 if (Math.Abs(nx) < 1.0 / 64 && Math.Abs(ny) < 1.0 / 64) { axv = new[] { 1 * nz - 0 * ny, 0 * nx - 0 * nz, 0 * ny - 1 * nx }; }   // Wy x N
+                 else { axv = new[] { 0 * nz - 1 * ny, 1 * nx - 0 * nz, 0 }; }   // Wz x N
+                 double axl = Math.Sqrt(axv[0] * axv[0] + axv[1] * axv[1] + axv[2] * axv[2]); axv[0] /= axl; axv[1] /= axl; axv[2] /= axl;
+                 ayv = new[] { ny * axv[2] - nz * axv[1], nz * axv[0] - nx * axv[2], nx * axv[1] - ny * axv[0] };   // N x Ax
+                 bool ocsId = Math.Abs(nx) < 1e-9 && Math.Abs(ny) < 1e-9 && nz > 0;
+                 double ocsDet = axv[0] * ayv[1] - axv[1] * ayv[0];
+                 Func<double, double, double[]> wxy = (x, y) => ocsId ? new[] { x, y } : new[] { x * axv[0] + y * ayv[0], x * axv[1] + y * ayv[1] };
+                             var wc = wxy(Num(en, "10"), Num(en, "20")); double[] cc = m.Ap(wc[0], wc[1]); double rr = Num(en, "40") * m.S;
+                             double a0 = Num(en, "50") * Math.PI / 180, a1 = Num(en, "51") * Math.PI / 180;
+                             if (!ocsId)
+                                 var d0 = wxy(Math.Cos(a0), Math.Sin(a0)); var d1 = wxy(Math.Cos(a1), Math.Sin(a1));
+                                 a0 = Math.Atan2(d0[1], d0[0]); a1 = Math.Atan2(d1[1], d1[0]);
+                                 if (ocsDet < 0) { double tmpA = a0; a0 = a1; a1 = tmpA; }   // lat guong: doi chieu cung
+                             a0 += rotDeg * Math.PI / 180; a1 += rotDeg * Math.PI / 180;
+                             var wc = wxy(Num(en, "10"), Num(en, "20")); double[] cc = m.Ap(wc[0], wc[1]);
+                                 var wa = wxy(va[0], va[1]); var wb = wxy(vb[0], vb[1]);
+                                 pushBulge(m.Ap(wa[0], wa[1]), m.Ap(wb[0], wb[1]), ocsDet < 0 ? -va[2] : va[2], col);
+                         { var wp = wxy(Num(en, "10"), Num(en, "20")); var pp = m.Ap(wp[0], wp[1]); P.Points.Add(new Pnt { X = pp[0], Y = pp[1], Col = col }); }
+                             {
+                             }
+             foreach (Seg g in outer)   // dau chay tren cung bo goc cua bien ngoai cung tinh la cham bien
+                 if (g.T == 'A' && DistToSeg(g, e) < 1e-3)
+                     double ax = e[0] - g.C[0], ay = e[1] - g.C[1], L = Math.Sqrt(ax * ax + ay * ay);
+                     return L < 1e-9 ? e : Pt(e[0] + ax / L * d, e[1] + ay / L * d);
+         // Lo dai cong (rang cong): hai dau bo tron (cung ~180 do, cung ban kinh) noi bang hai canh bat ky. Chieu dai truc = trung binh do dai hai canh
+         // (lo dai thang: bang tam-tam). Dung de xet joint lo: truc >= 10
+         static bool SlotLength(List<Seg> loop, out double len)
+             len = 0; int n = loop.Count; if (n < 4) return false;
+             Func<Seg, Seg, bool> cont = (a, b) => a.T == 'A' && b.T == 'A' && a.Ccw == b.Ccw && Dist(a.C, b.C) < 1e-3 && Math.Abs(a.R - b.R) < 1e-3;
+             int start = -1;
+             for (int i = 0; i < n; i++) if (!cont(loop[(i + n - 1) % n], loop[i])) { start = i; break; }
+             if (start < 0) return false;
+             var groups = new List<List<Seg>>();
+             for (int k = 0; k < n; k++)
+                 Seg g = loop[(start + k) % n];
+                 if (groups.Count > 0 && cont(groups[groups.Count - 1][groups[groups.Count - 1].Count - 1], g)) groups[groups.Count - 1].Add(g);
+                 else groups.Add(new List<Seg> { g });
+             if (groups.Count != 4) return false;
+             Func<List<Seg>, bool> isCap = gr => { double sw = gr.Where(x => x.T == 'A').Sum(x => x.Sweep); return gr[0].T == 'A' && gr[0].Ccw && sw >= 150 * Math.PI / 180 && sw <= 210 * Math.PI / 180; };
+             Func<List<Seg>, double> glen = gr => gr.Sum(x => x.T == 'L' ? Dist(x.P, x.Q) : x.R * x.Sweep);
+             int c0 = (isCap(groups[0]) && isCap(groups[2])) ? 0 : ((isCap(groups[1]) && isCap(groups[3])) ? 1 : -1);
+             if (c0 < 0) return false;
+             if (Math.Abs(groups[c0][0].R - groups[c0 + 2][0].R) > 0.01) return false;
+             len = (glen(groups[(c0 + 1) % 4]) + glen(groups[(c0 + 3) % 4])) / 2;
+             return true;
+         {
+                     double slotLen;
+                     else if (SlotLength(ccw, out slotLen)) hasJ = slotLen >= 10 - 1e-9;   // lo dai cong (ca 26-063771)
+             takeAll(slits, s => { pierce(s.Pp, 9010); pos = emit(s.Segs, s.Pp, false); endCut(); });   // khe cat khong G41 (theo ca 26-061922)
+                 {
+                 }
+ 
+             }
+             {
+         }
-                             double[] cc = m.Ap(Num(en, "10"), Num(en, "20")); double rr = Num(en, "40") * m.S;
-                             double a0 = (Num(en, "50") + rotDeg) * Math.PI / 180, a1 = (Num(en, "51") + rotDeg) * Math.PI / 180;
-                             double[] cc = m.Ap(Num(en, "10"), Num(en, "20"));
-                                 pushBulge(m.Ap(va[0], va[1]), m.Ap(vb[0], vb[1]), va[2], col);
-                         { var pp = m.Ap(Num(en, "10"), Num(en, "20")); P.Points.Add(new Pnt { X = pp[0], Y = pp[1], Col = col }); }
-             takeAll(slits, s => { pierce(s.Pp, 9010); pos = emit(s.Segs, s.Pp, true); endCut(); });
```

## 18:43 - bien dich thanh cong
```diff
--- Core.cs
+             // doan thang ngan hon dung sai noi dau mut (vd 0.008 mm o goc khia) bi gop thanh do dai 0 -> bo di de khong tao nut bac 4
+             int tiny = P.Segs.RemoveAll(sg0 => sg0.T == 'L' && Dist(sg0.P, sg0.Q) < 1e-6);
+             if (tiny > 0) P.Warnings.Add("Đã bỏ " + tiny + " đoạn thẳng quá ngắn (nhỏ hơn dung sai nối đầu mút 0.02 mm)");
```

## 18:47 - bien dich thanh cong
```diff
--- Core.cs
+             // Cac cap dau mut cach nhau <= tol, gop tu gan den xa. Khong gop neu nhom moi chua ca hai dau cua cung mot doan
+             // (doan ngan hon dung sai, vd 0.008 mm o goc khia, phai giu nguyen khong bi thanh do dai 0)
+             var pairs = new List<double[]>();
+                     if (dx * dx + dy * dy <= tol * tol) pairs.Add(new double[] { dx * dx + dy * dy, ord[a], ord[b] });
+             var members = new Dictionary<int, HashSet<int>>();
+             for (int i = 0; i < list.Count; i++) members[i] = new HashSet<int> { (int)list[i][2] / 2 };
+             foreach (var pr in pairs.OrderBy(q => q[0]))
+                 int ra = find((int)pr[1]), rb = find((int)pr[2]);
+                 if (ra == rb) continue;
+                 var sa = members[ra]; var sb = members[rb];
+                 if (sa.Overlaps(sb)) continue;
+                 par[ra] = rb; sb.UnionWith(sa);
+             }
+             {
-                     if (dx * dx + dy * dy <= tol * tol) par[find(ord[a])] = find(ord[b]);
```
