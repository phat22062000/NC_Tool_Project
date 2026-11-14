# Quy tắc của DXF To NC Conversion Tool

Cập nhật lần cuối: 14-11-2026. File này được cập nhật mỗi khi có quy tắc mới.

## 1. Đầu vào DXF: đọc và phân loại

| Đối tượng (Layer 0) | Màu | Ý nghĩa | Mã NC |
|---|---|---|---|
| LINE, ARC, CIRCLE, LWPOLYLINE | by layer (hoặc trắng) | Biên dạng ngoài, lỗ | `M98P9010` |
| POINT | by layer | Lỗ trắng (không xuyên) | `M98P9016` |
| LINE, ARC (đường hở) | xanh lá (ACI 3) | Khe cắt | `M98P9010` |
| POINT | xanh lá | Piercing xuyên (đục một điểm) | `M98P9010` |
| LINE, ARC, CIRCLE | đỏ (ACI 1) | Kẻ dấu (kegaki) | `M98P9017` |
| Đường nét đứt (DASHED) | bất kỳ | Đường uốn, bỏ qua | không cắt |
| Màu khác | ngoài các màu trên | Bỏ qua, có cảnh báo | không cắt |

1. LWPOLYLINE (kể cả cung bulge) được đổi thành LINE và ARC. Khối INSERT được bung thành đối tượng thường.
2. POLYLINE (cũ) và ELLIPSE: báo lỗi, cần Explode trước. INSERT tỉ lệ không đều hoặc đối xứng: báo lỗi.
3. Các đầu mút cách nhau không quá 0.02 mm được nối thành một điểm, **chỉ giữa các đối tượng cùng màu** (từ 04-11-2026). Kegaki (đỏ) không bị nối vào biên ngoài. Ca 26-065663: đoạn kegaki kết thúc cách đỉnh biên ngoài 0.01 mm (y = 12.49) là cố ý, nên đầu đó không chạm biên, không dùng làm điểm đục, và đường cắt dừng đúng tại y = 12.49. Mỗi điểm nối tối đa 2 đầu mút: nếu gộp sẽ tạo nút có 3 đầu mút thì không gộp. Nhờ vậy khe hở joint 0.005 mm của đường dẫn vào đã vẽ sẵn (mục 6.3) không bị lấp kín.
4. Đường hở màu by layer (biên ngoài hoặc lỗ không khép kín): báo lỗi kèm tọa độ chỗ hở. Đường hở chỉ hợp lệ với màu xanh lá (khe cắt) và đỏ (kẻ dấu).
5. Biên dạng ngoài là vòng kín lớn nhất. Nếu một CIRCLE by layer lớn hơn mọi vòng kín thì đó là phôi tròn.
6. Vòng kín xanh lá được xử lý như lỗ.
7. **SPLINE** (từ 24-10-2026): chuyển thành các đoạn thẳng `G01`, xử lý như LINE cùng màu và cùng layer (biên ngoài, lỗ, khe cắt, kegaki). Cách chia, khớp với `ViDu_Spline.nc`:
   - Đọc B-spline từ bậc, knot, điểm điều khiển (có trọng số thì tính như NURBS). Lấy tham số từ đầu đến cuối.
   - Chia đôi tham số theo đệ quy. Đoạn `[a, b]` được chia đôi khi độ võng lớn nhất so với dây cung `> 0.01 mm`. Độ võng đo tại 3 điểm ở 1/4, 1/2 và 3/4 tham số của đoạn.
   - Các đoạn thẳng nối tiếp tạo thành biên dạng cùng các LINE và ARC kề bên. Đầu mút lệch dưới 0.02 mm được nối như mục 3.
   - Spline chỉ có điểm fit, không có điểm điều khiển: nối thẳng các điểm fit. Spline nét đứt thì bỏ qua như đường uốn.
   - Có cảnh báo cho biết số spline và số đoạn thẳng đã tạo.
8. **Hệ tọa độ đối tượng (từ 28-10-2026):** CIRCLE, ARC, LWPOLYLINE và POINT có vector pháp tuyến (nhóm 210/220/230) khác (0, 0, 1) được đổi sang tọa độ thế giới theo thuật toán trục tùy ý của AutoCAD. Trường hợp thường gặp là vẽ trong UCS bị lật gương (230 = −1): hoành độ đổi dấu, cung đổi chiều. Ca 26-061922 có hai CIRCLE tại x = −321.8 và −171.8, thực tế nằm tại x = 321.8 và 171.8 trong phôi.
9. **Đoạn thẳng ngắn hơn dung sai nối (từ 28-10-2026):** khi nối đầu mút (mục 3), hai đầu của cùng một đoạn không bao giờ được gộp (và đầu mút khác màu không được gộp với nhau) vào cùng một điểm. Đoạn ngắn hơn 0.02 mm (ví dụ 0.008 mm ở góc khía) được giữ nguyên và xuất thành một dòng `G01` riêng, cung đi vào hoặc đi ra đoạn đó giữ nguyên điểm đầu cuối thật của mình. Các cặp đầu mút được gộp từ gần đến xa. Nếu gộp, đoạn thành chiều dài 0, nút có bậc 4 và tool báo biên dạng hở hoặc rẽ nhánh (ca 26-063561). Đoạn có đầu và cuối trùng nhau trong DXF gốc vẫn bị bỏ.

## 2. Vật liệu, độ dày và thông số

| Nhóm | Độ dày (mm) | Piercing ngoài | Joint ngoài | Piercing lỗ | Joint lỗ |
|---|---|---|---|---|---|
| SS | ≤ 3.2 | 3 | 0.8 | 6 | 0 |
| SS | 3.2 đến 9 | 6 | 1 | 6 | 0 |
| SS | 9 đến 16, và trên 16 dưới 19 | 10 | 1 | 10 | 0 |
| SS | ≥ 19 | cong (mục 6) | 3 | 10 | 0 |
| SUS | ≤ 8 | 6 | 0.7 | 6 | 0.7 |
| SUS | 8 đến 16, và trên 16 dưới 19 | 10 | 1 | 10 | 1 |
| SUS | ≥ 19 | cong (mục 6) | 3 | 10 | 1 |
| AL | ≤ 8 | 6 | 0.7 | 6 | 0.7 |
| AL | 8 đến 18 | 10 | 1 | 10 | 1 |
| AL | ≥ 19 | cong (mục 6) | 0.01 | 10 | 0 |
| CU | mọi độ dày | 6 | 0.7 | 6 | 0.7 |

1. Thông số lấy từ file Excel theo mã DXF (tên file). Tên vật liệu tự nhận nhóm: SS (SS400, SPHC, SPCC, SPHCP…), SUS (SUS, inox, 2B, HL, 304…), AL (A5052, **A1050, ＡＬその他**, nhôm…), CU (đồng và đồng thau: 銅, 黄銅, **真鍮**, シンチュウ, BRASS, COPPER, C1100, C2801, C2600, C2680, C3604). Đồng thau dùng đúng thông số của CU trong bảng trên (piercing ngoài 6, joint ngoài 0.7, piercing lỗ 6, joint lỗ 0.7, mọi độ dày). Trên giao diện, mục "CU – Đồng / Đồng thau" áp dụng cho cả hai. Chữ toàn góc (ＡＬ, ＳＵＳ…) được đổi sang chữ thường trước khi nhận dạng. Chữ `その他` đứng một mình không nhận dạng được.
2. Mã không có trong Excel dùng vật liệu và độ dày ở hai ô nhập tay, ghi "Nhập Tay". Mỗi file có thông số riêng. Đổi hai ô chỉ áp dụng cho file đang chọn.
3. Độ dày không hợp lệ hoặc nhóm không hợp lệ: báo lỗi.
4. "Joint lỗ 0" của SS nghĩa là đường cắt khép kín nhưng vẫn áp dụng quy tắc vị trí joint (mục 5).
5. **Khách hàng đặc biệt (từ 14-11-2026):** tool đọc cột khách hàng của file Excel (tiêu đề `得意先`, `客先`, `顧客`, `Khách hàng` hoặc `Customer`; ở mẫu CAD依頼書 cột G là mã số, cột H là tên). Mỗi khách có thể có quy tắc riêng, áp dụng cho mọi vật liệu và độ dày:
   - **Khách 824 (武野工業): không tạo joint cho lỗ bên trong**, kể cả SUS, AL và CU. Độ rộng joint lỗ bằng 0, đường cắt lỗ khép kín như SS; vị trí điểm vào và thứ tự thử vị trí (mục 5) giữ nguyên. Joint của biên ngoài không đổi. Có cảnh báo "Khách hàng 824: không tạo joint cho lỗ bên trong".
   - Khi bạn chỉnh tay vật liệu hoặc độ dày của một file có trong Excel, khách hàng vẫn lấy từ dòng Excel đó. File không có trong Excel (nhập tay hoàn toàn) coi như không có khách hàng đặc biệt.
   - Dùng dòng lệnh: thêm mã khách làm tham số cuối, ví dụ `NC_Tool.exe --convert in.dxf out.nc SUS 1.5 824`.

## 3. Thứ tự đường cắt

1. Lỗ trắng (`9016`).
2. Kẻ dấu (`9017`).
3. Piercing xuyên màu xanh lá.
4. Khe cắt (xanh lá).
5. Các lỗ, theo diện tích tăng dần. Cùng diện tích thì chọn lỗ gần vị trí hiện tại nhất. **Diện tích chênh nhau dưới 6% được coi là cùng diện tích** (từ 14-11-2026, ca 26-067424: lỗ dài cong 52.4 mm² và hai lỗ dài thẳng 49.6 mm² chênh 5.6%, mẫu T cắt lỗ dài cong, là lỗ gần nhất, trước).
6. Biên ngoài, cắt cuối cùng.

Trong cùng một nhóm, các đối tượng khác nhau được cắt theo thứ tự gần vị trí hiện tại nhất, bắt đầu từ gốc (0, 0).

## 4. Điểm vào và đường cắt từng loại đối tượng

### 4.1 Kẻ dấu (đỏ)
1. Vòng kín: đục tại đỉnh gần gốc (0, 0) nhất, cắt **ngược** chiều kim đồng hồ, **không có `G41`** (đục ngay đỉnh nên không có đường dẫn vào). Quy tắc này theo ca 26-061939 và áp dụng cho mọi vật liệu, mọi độ dày (đã sửa `Vi_Du_1.nc` cho khớp ngày 21-10-2026).
2. Đường tròn đỏ: đục tại góc 225° (góc dưới trái), cắt `G02` một vòng ngay sau `M98P9017`, không có dòng `G01G41`. (Mẫu 26-064456T có dòng `G01G41` do nhầm, bạn đã xác nhận đúng là không có.)
3. Đường hở: đục tại đầu chạm biên ngoài, kéo dài 3 mm **ngược chiều đoạn đầu của đường kegaki** (không theo pháp tuyến của biên; ca 26-062203 chạm cạnh vát, đoạn đầu nằm ngang nên đục tại (−2, 4)). Nếu kegaki vuông góc biên thì hai cách cho cùng kết quả. Đầu nằm trên cung bo góc của biên ngoài (cách cung dưới 0.001 mm) cũng tính là chạm biên (ca 26-063986: kegaki bắt đầu tại (0.268, 1) trên cung R2, đục tại (−2.732, 1)). Nếu không đầu nào chạm biên thì đục tại đầu gần gốc nhất. Dẫn vào bằng `G01` thường, không `G41`.
4. **Mọi kegaki, gồm đường thẳng, cung, vòng kín và đường tròn (từ 22-10-2026):** sau khi bật tia đánh dấu (`M98P9017`) không dùng bù dao `G41`. Dòng đầu tiên sau `M98P9017` là `G01` hoặc `G02` thường. Đã sửa `Vi_Du_1.nc` (1 khối) và `Vi_Du_2.nc` (3 khối).

### 4.2 Khe cắt (xanh lá)
1. Đục tại đầu nằm ngoài chi tiết, cắt qua khe tới đầu kia, **không có `G41`** (từ 28-10-2026, theo ca 26-061922: sau `M98P9010` là `G01` thường). Nhiều khe thì chọn khe gần vị trí hiện tại nhất.

### 4.3 Lỗ
1. **Điểm vào ưu tiên:** phần nhô nhỏ của lỗ, tức nửa tròn hoặc mũi nhọn lồi (tổng góc ≥ 150°), bán kính nhỏ nhất. Điểm vào là đỉnh của phần nhô.
2. Nhiều phần nhô cùng bán kính: lỗ nằm ngang vào đầu trái. Lỗ nằm đứng: **lỗ không có joint** (rãnh ngắn, mục 5.1) vào đầu **trên** (ca 26-067424: rãnh đứng tâm-tâm 6 mm vào đầu trên, đục tại tâm cung trên); **lỗ có joint** vào đầu **thấp** (Vi_Du_1). Lỗ nằm ngang khi bề rộng ≥ bề cao, tính theo kích thước thật. Thứ tự các phần nhô theo bán kính rồi theo trục như trên, **không** theo hướng so với tâm lỗ (hướng trái, phải, dưới, trên chỉ dùng khi phải đổi vị trí do cách biên dạng khác < 3 mm).
3. Lỗ hình tam giác (có 3 cạnh thẳng dài) vào giữa cạnh thẳng dài nhất.
4. **Lỗ không có phần nhô (chữ nhật, chữ nhật bo góc, vuông) xét theo trục trước:** lỗ ngang hoặc vuông (rộng ≥ cao) vào giữa cạnh trái; lỗ đứng (cao > rộng) vào giữa cạnh đáy. Thứ tự trái, phải, đáy, đỉnh ở mục 5 chỉ dùng để chọn vị trí khác khi vị trí theo trục cách biên dạng khác ≤ 3 mm.
5. **Điểm vào không bao giờ trùng đỉnh hoặc đầu đoạn:** nếu trùng (kể cả lệch dưới 0.001 mm do DXF) thì dời sang giữa cạnh. Không xuất đoạn cung dài 0. Sai số vị trí trái nhất hoặc thấp nhất khi so điểm vào là 0.01 mm.
6. **Lỗ nghiêng (ca 26-062754, 26-065394):** lỗ toàn cạnh thẳng, không cạnh nào song song Ox hoặc Oy, không có phần nhô nhỏ và không phải tam giác (ví dụ hình chữ nhật xoay). Vào giữa **cạnh ngắn nhất** (sai số 0.01 mm); các cạnh ngắn bằng nhau thì lấy cạnh thấp nhất (trung điểm có Y nhỏ nhất). Đục vào trong lỗ vuông góc với cạnh đó. Trước 03-11-2026 quy tắc là "cạnh xuất phát từ đỉnh thấp nhất", trùng với quy tắc này ở ca 26-062754 nhưng sai ở ca 26-065394.
8. **Điểm đục trong lỗ:** nếu bán kính của phần nhô nhỏ hơn piercing lỗ thì đục ở tâm cung đó. Ngược lại đục cách điểm vào bằng nhỏ hơn giữa piercing lỗ và nửa bề rộng vuông góc với hướng đục. Hướng đục đi vào trong lỗ.
7. **Lỗ tròn:** điểm vào thử lần lượt bên trái, bên phải, phía dưới, phía trên và lấy vị trí đầu tiên cách biên dạng khác (biên ngoài và lỗ khác) **từ 3 mm trở lên** (từ 14-11-2026 tính cả đúng 3 mm; ca 26-067249: lỗ cách mép đúng 3 mm vẫn vào bên trái). Mọi lỗ tròn đều áp dụng, kể cả lỗ nhỏ không có joint. Nếu cả bốn vị trí đều ≤ 3 mm thì vào bên trái (lỗ có joint thì chọn vị trí xa nhất). Ca 26-063378: lỗ φ6 tâm (5, 11) cách mép trái 2 mm nên vào bên phải tại (8, 11), piercing tại tâm.
9. Lỗ cắt ngược chiều kim đồng hồ, `G01G41` tới điểm vào. Lỗ tròn cắt `G03`.

### 4.4 Biên ngoài (cắt chiều kim đồng hồ, bù `G41`)
Piercing ngoài nằm ngoài biên, cách đúng bằng piercing ngoài, dẫn vào vuông góc với biên.

1. **Cạnh trái thẳng đứng (dọc Oy, sát Oy), không tính bo R và vát C:**
   - Dài ≤ 100 mm: đục giữa cạnh.
   - Dài > 100 mm: đục cách đầu dưới của cạnh 40 mm.
   - Có nhiều cạnh thẳng đứng sát Oy: chọn cạnh **thấp nhất** (Y nhỏ nhất), không chọn cạnh dài nhất. Nếu cạnh thấp nhất ngắn hơn 10 mm thì áp dụng mục 4.4.2 (cạnh đáy). Ca 26-063416: cạnh trái bị hai rãnh cắt làm ba đoạn 4.5, 70 và 4.5 mm; đoạn thấp nhất 4.5 mm nên đục ở cạnh đáy dài 160.8 mm, cách đầu trái 40 mm.
   - Có nhiều cạnh đáy thẳng nằm trên Ox: chọn cạnh **trái nhất** (không chọn cạnh dài nhất), giống cách chọn cạnh trái thấp nhất ở mục 4.4.1. Ca 26-065395: đáy có đoạn 8.0 mm ở góc trái và đoạn 27.5 mm sau rãnh cắt; đục ở đoạn 8.0 mm, tại giữa cạnh (4.503, −3).
2. **Cạnh trái dưới 10 mm:** đục ở cạnh dưới nằm trên trục Ox, nằm ngoài biên, cách đoạn thẳng đó một khoảng bằng piercing ngoài:
   - Cạnh dưới ≤ 100 mm: đục giữa cạnh dưới.
   - Cạnh dưới > 100 mm: đục cách đầu trái 40 mm.
3. Đường cắt dừng trước điểm vào một đoạn bằng joint ngoài.
4. **Không tự xoay hình (từ 14-11-2026):** DXF để hướng nào thì giữ nguyên hướng đó, không thử xoay 90°, 180°, 270° để tìm cạnh thẳng đứng như trước. Nếu không có cạnh thẳng đứng sát Oy thì áp dụng quy tắc phôi cong hoặc xiên (mục 7 bên dưới). Phôi vẫn được dời về gốc (0, 0), có cảnh báo khi dời.
   - **Dung sai:** cạnh được coi là thẳng đứng sát Oy (hoặc nằm ngang sát Ox) khi lệch không quá 0.01 mm. DXF hay có nhiễu cỡ 0.0001 mm làm cạnh hơi nghiêng; trước đây dung sai 0.0001 mm khiến tool xoay nhầm phôi (ca 26-062063). Cảnh báo dời gốc chỉ hiện khi dời trên 0.01 mm. (Dung sai này vẫn dùng để nhận cạnh thẳng đứng, nhưng không còn dẫn đến việc xoay.)
5. **Phôi tròn:** đục tại góc 225°, cắt một vòng `G02`, dừng trước điểm vào một đoạn joint ngoài.
6. **Phôi gần tròn:** nếu đoạn trái nhất của biên ngoài là cung lớn (các cung cùng tâm, cùng bán kính quét tổng ≥ 180°) và điểm 225° của cung nằm trên cung, áp dụng quy tắc phôi tròn của mục 4.4.5: đục tại góc 225° của cung, cách ngoài theo hướng tâm, cắt `G02`, dừng trước điểm vào một đoạn joint ngoài. Có cảnh báo.
7. **Phôi cong hoặc xiên (không xoay ra cạnh thẳng đứng nào, không thuộc mục 6):** lấy cạnh nằm ngay phía dưới đỉnh trái nhất (đoạn đi trước đỉnh đó theo chiều kim đồng hồ, bỏ qua cung bo R ≤ 5). Cạnh dài ≤ 100 mm: vào giữa cạnh. Cạnh dài > 100 mm: vào cách đầu gần đỉnh trái 40 mm, đo theo đường thẳng. Đục ngoài theo pháp tuyến ngoài của cạnh. Riêng cạnh **thẳng** dài hơn 100 mm: vào cách **đỉnh dưới** của cạnh 40 mm (đỉnh là giao của cạnh với cạnh kề phía dưới, bỏ qua cung bo góc R ≤ 5), không tính từ đầu trên. Ca 26-064456: cạnh trái nghiêng dài 199 mm, vào tại (36.1, 38.867), cách đỉnh dưới 40 mm.
8. **Biên không có cạnh thẳng đứng chạm điểm trái nhất (đường cong, hoặc cạnh trái xiên; ca ViDu_Spline_1, 26-068005, 26-068007):** nếu không có cạnh thẳng đứng nào chạm điểm trái nhất của biên ngoài (điểm đó nằm trên spline, cung hoặc cạnh xiên), đồng thời có cạnh đáy thẳng nằm trên Ox dài từ 10 mm trở lên, thì không xoay phôi và đục ở cạnh đáy theo quy tắc cạnh đáy ở mục 4.4.2: đáy ≤ 100 mm đục giữa cạnh, đáy > 100 mm đục cách đầu trái 40 mm, đục ngoài biên theo piercing ngoài, đường cắt đi sang trái (chiều kim đồng hồ) và dừng trước điểm vào một đoạn joint ngoài. Gốc tọa độ: có spline thì lấy theo bao của các điểm điều khiển spline cùng các đường khác (ở ca ViDu_Spline_1 gốc không đổi); chỉ có cung thì dời theo bao thông thường. Có cảnh báo. Kích thước ở tiêu đề tính cả gốc (0, 0). Quy tắc này đứng trước quy tắc phôi cong ở mục 4.4.7, nhưng không áp dụng khi không có cạnh đáy thẳng trên Ox. Mở rộng cho cung theo yêu cầu ngày 24-10-2026, mở rộng cho cạnh trái xiên ngày 14-11-2026: ca 26-068005 (SS 3.2, đáy 40.229 → vào giữa cạnh đáy tại (80.229, −3) theo mục 4.4.2, vì không còn xoay phôi) và 26-068007 (SS 6, vào tại (33.347, −6)).

### 4.5 Phôi dài hẹp (cạnh ngắn ≤ 100, cạnh dài ≥ 1000)
1. Bắt buộc có một điểm đánh dấu `LJ` và ít nhất một điểm `J` (TEXT kèm POINT, nằm trên cạnh dài). Thiếu một hoặc cả hai: báo lỗi nêu rõ tên điểm thiếu.
2. Có thể có nhiều điểm `J`. Xếp `LJ` và các `J` theo chiều kim đồng hồ trên biên ngoài, bắt đầu từ `LJ`. Mỗi cặp điểm liên tiếp là một đường cắt, nên số đường cắt bằng số điểm đánh dấu (1 LJ + n J = n + 1 đường). Đều chạy chiều kim đồng hồ.
3. Đường bắt đầu tại `LJ`: đục ngoài, dẫn vuông góc vào, cắt quanh biên, dừng cách điểm đánh dấu kế tiếp một đoạn joint ngoài.
4. Đường bắt đầu tại một `J`: chạy dọc cạnh, dừng cách điểm đánh dấu kế tiếp một đoạn joint ngoài. Đường của `J` cuối cùng dừng cách `LJ` một đoạn joint ngoài.
5. Mọi POINT trùng vị trí `J` hoặc `LJ` (kể cả nhiều J) không bị hiểu là lỗ trắng.

## 5. Joint lỗ và vị trí joint

1. **Lỗ có joint khi:**
   - Lỗ tròn Ø ≥ 50.
   - Lỗ dài (hai nửa tròn và hai cạnh thẳng): tâm đến tâm ≥ 10.
   - Lỗ dài cong (hai đầu bo tròn cùng bán kính, hai cạnh là cung; ca 26-063771): chiều dài trục ≥ 10, tính bằng trung bình độ dài hai cạnh. Với lỗ dài thẳng chiều dài trục chính là tâm đến tâm.
   - Lỗ vuông hoặc lỗ hình khác: cạnh lớn nhất ≥ 15.
2. Độ rộng joint lỗ theo bảng mục 2. Với SS bằng 0 nhưng vẫn áp dụng quy tắc vị trí.
3. **Thứ tự ưu tiên vị trí joint (cao xuống thấp):**
   1. Cách các biên dạng xung quanh (biên ngoài và lỗ khác) hơn 3 mm.
   2. Bên trái.
   3. Bên phải.
   4. Phía dưới.
   5. Phía trên.
4. Vị trí cách các biên dạng khác 3 mm trở xuống bị loại trước. Nếu mọi vị trí đều gần thì chọn vị trí xa nhất và ghi cảnh báo.
5. Với lỗ tròn, thử lần lượt trái, phải, dưới, trên.
6. Với lỗ có phần nhô nhỏ, vẫn ưu tiên phần nhô trước, rồi mới áp dụng thứ tự trái, phải, dưới, trên. Lỗ không có phần nhô thì ưu tiên vị trí theo trục (mục 4.3.4) trước, rồi mới đến trung điểm các cạnh và cung theo thứ tự trái, phải, dưới, trên.
7. Đường cắt lỗ dừng trước điểm vào một đoạn bằng joint lỗ.

## 6. Dày từ 19 mm: đục piercing cong

Áp dụng cho biên ngoài thẳng, phôi tròn và phôi cong. Vị trí đặt piercing theo mục 4.4, chỉ khác kiểu dẫn vào.

| Nhóm | Dẫn vào | Joint ngoài |
|---|---|---|
| SS | Đục cách biên 10 mm. Thẳng 7 mm rồi cung R3 ngược chiều kim đồng hồ, vào tiếp tuyến tại điểm vào. | 3: đường cắt dừng cách điểm vào 3 mm dọc cạnh |
| SUS | Đục cách biên 10 mm và lệch 10 mm dọc biên. Thẳng 10 mm về điểm chuẩn, thẳng 7 mm vào, cung R3. Điểm vào lệch +3 mm theo chiều cắt so với điểm chuẩn. | 3 |
| AL | Giống SUS. | 0.01 |

1. **SS dày từ 19:** DXF phải bo hết các góc nhọn (trong và ngoài, kể cả mặt vát). Còn góc nhọn thì báo lỗi kèm tọa độ. SUS và AL không có kiểm tra này.
2. Piercing lỗ là 10 mm cho cả ba nhóm. Joint lỗ: SS 0, SUS 1, AL 0.
3. **Đường dẫn vào đã vẽ sẵn trong DXF (từ 07-11-2026, ca ViDu_AL_t19):** DXF có thể đã vẽ sẵn đường dẫn vào piercing cong (hai đoạn thẳng nối với một cung R3) cùng khe hở joint ngay trên biên ngoài. Khi đó biên ngoài là một đường hở, có đúng hai đầu mút, và tool không báo lỗi hở mà cắt đúng theo đường vẽ:
   - Điểm đục là đầu tự do xa tâm nhất của đường hở (đầu nằm ngoài phôi, ví dụ (−10, 60)). Đường cắt bắt đầu bằng `G01G41` đi theo từng đoạn đã vẽ và kết thúc tại đầu kia (ví dụ (0, 52.995)). Độ rộng joint chính là khe hở đã vẽ (0.005 mm ở ca mẫu), không tra theo bảng mục 2.
   - Không xoay phôi, không xét cạnh trái. Gốc tọa độ lấy theo phần biên ngoài sau đường dẫn vào, nên đường dẫn có thể nằm ngoài gốc (x âm). Kích thước ở tiêu đề tính cả điểm đục.
   - Áp dụng cho dày ≥ 19 của mọi nhóm (SS, SUS, AL). Bỏ kiểm tra góc nhọn của SS dày ≥ 19 với đường hở kiểu này. Các lỗ và đường cắt khác xử lý như bình thường.

## 7. Cấu trúc file NC

1. Định dạng: ASCII, xuống dòng CRLF, dòng cuối không có CRLF. Số bỏ 0 thừa, số nguyên vẫn có dấu chấm (`21.`, `0.`). Không có dấu cách giữa các từ lệnh.
2. Tiêu đề: `(TÊN.DXF / rộng X cao)`. Kích thước tính cả điểm đục ngoài.
3. Đầu file: `#501=106`, `M66`, `G90`, `G92X0.Y0.`.
4. Mỗi đường cắt: `G00…G40`, mã cắt, `G01G41…`, các đoạn cắt, `M121`, `M199`. Cuối file: `G01G40`, `M30`.
5. Cuối file: `CUTTING_LENGTH`, `RAPID_LENGTH` (từ gốc qua các điểm đục, không tính quay về), `TOTAL_LENGTH`, `BEAM_ON` (số lần đục).

## 8. Giao diện và quy trình

1. Kéo thả hoặc chọn file DXF. Chọn nhiều file bằng Shift hoặc Ctrl. File có lỗi hiện dấu X đỏ kèm lý do.
2. Chọn một file Excel danh sách vật liệu (3 cột: Mã DXF, Vật liệu, Độ dày, thêm cột Khách hàng nếu có; đọc được mẫu CAD依頼書). Excel tự đọc lại khi bạn sửa file.
3. Bấm **Lưu File .nc** hoặc **Lưu Tất Cả**: lưu file NC, gỡ file đã lưu khỏi danh sách (file DXF trên ổ đĩa giữ nguyên), bỏ chọn file Excel, rồi hiện hộp thoại thành công.
4. Hai ô Vật Liệu và Độ Dày tự hiện đúng thông số của file đang chọn. Đổi hai ô chỉ áp dụng cho file đang chọn.
5. Chế độ sáng hoặc tối được nhớ cho lần mở sau.

## 9. Ví dụ đối chiếu

NC của phần mềm khớp từng dòng với các ví dụ sau (trừ dòng tên file). Thư mục `Vi_Du`.

| Ví dụ | Nội dung |
|---|---|
| Vi_Du_1, 4 | SS dày 3.2, biên có lỗ (Vi_Du_1 đã sửa: 21-10-2026 kegaki vòng kín ngược chiều kim đồng hồ không G41, 28-10-2026 khe cắt không G41) |
| Vi_Du_3 | SUS dày 6, phôi dài hẹp có điểm J và LJ |
| Vi_Du_5 | SS dày 19, đục cong (đã sửa 19-10-2026: đường cắt dừng ở Y47, joint 3) |
| Vi_Du_6 | SUS dày 19, đục cong |
| Vi_Du_7 | AL dày 19, đục cong |
| Vi_Du_8 | Phôi tròn SS dày 6 |
| Vi_Du_9, 10 | Phôi cong SS dày 6 |
| Vi_Du_2 | SUS dày 6, nhiều lỗ: 17 trong 22 đoạn giống hệt. Các điểm vào khác (lỗ dài đứng, lỗ hình hoa) đã được bạn chấp nhận. |
| NC_Tool_Fix: 26-060780, 26-060781 | SS dày 6, lỗ chữ nhật bo góc dọc và ngang (mục 4.3.4, 4.3.5) |
| NC_Tool_Fix: 26-061080 | SS dày 2.3, hai cạnh trái bằng nhau chọn cạnh thấp (mục 4.4.1) |
| NC_Tool_Fix: 26-061298 | SS dày 6, dải dài hẹp có LJ và hai J (mục 4.5) |
| NC_Tool_Fix: 26-061341 | SS dày 4.5, phôi gần tròn (mục 4.4.6) |
| NC_Tool_Fix: 26-061939 | SS dày 19, kegaki vòng kín không G41 (mục 4.1.1), joint ngoài 3 (mục 2, 6) |
| NC_Tool_Fix: 26-062063 | SS dày 1.6, cạnh trái hơi nghiêng 0.0001 mm không được xoay (mục 4.4.4) |
| Vi_Du_11 (ViDu_Spline) | SS dày 6, biên ngoài có một spline, chia thành 164 đoạn thẳng (mục 1.7) |
| Vi_Du_12 (ViDu_Spline_1) | SS dày 6, spline lồi ra trái nhất, đục ở cạnh đáy 40 mm từ đầu trái (mục 4.4.8) |
| Vi_Du_13 (26-063986) | SS dày 6, kegaki hở chạm cung bo góc, đục kéo dài 3 mm (mục 4.1.3) |
| Vi_Du_14 (26-061922) | SS dày 3.2, CIRCLE vẽ trong UCS lật gương (mục 1.8), khe cắt không G41 (mục 4.2) |
| Vi_Du_15 (26-063771) | SUS dày 4, lỗ dài cong có joint 0.7 (mục 5.1) |
| Vi_Du_16 (26-063561) | SUS dày 1.5, phôi 1257 × 907 có 4 khía góc chứa đoạn 0.008 mm (mục 1.9) |
| Vi_Du_17 (26-063378) | SS dày 3.2, lỗ tròn φ6 sát mép trái vào bên phải (mục 4.3.7) |
| Vi_Du_18 (26-063416) | SS dày 1.6, cạnh trái bị rãnh cắt, chọn cạnh thấp nhất 4.5 mm rồi đục ở cạnh đáy (mục 4.4.1) |
| Vi_Du_19 (26-064456) | SS dày 2.3, phôi cong 1330 × 270, kegaki chạm cung, kegaki tròn không G41, vào cạnh trái nghiêng 40 mm từ đỉnh dưới (mục 4.1, 4.4.7) |
| Vi_Du_20 (26-065395) | SS dày 3.2, phôi cong đục ở cạnh đáy trái nhất, lỗ tròn vào bên trái (mục 4.4.2, 4.3.7) |
| Vi_Du_21 (26-065394) | SS dày 3.2, như trên, thêm lỗ chữ nhật nghiêng vào giữa cạnh ngắn nhất (mục 4.3.6) |
| Vi_Du_22 (26-065663) | SS dày 6, kegaki chữ L có một đầu cách biên 0.01 mm; chọn đúng đầu chạm biên để đục (mục 1.3, 4.1.3) |
| Vi_Du_23 (ViDu_AL_t19) | AL dày 19, DXF đã vẽ sẵn đường dẫn vào piercing cong và khe joint 0.005 mm, cắt đúng theo đường vẽ (mục 6.3) |
| Vi_Du_24 (26-068005) | SS dày 3.2, phôi cạnh trái xiên, không xoay, đục ở cạnh đáy trên Ox (mục 4.4.8), lỗ tròn φ32 vào bên trái |
| Vi_Du_25 (26-068007) | SS dày 6, phôi cạnh trái xiên + cung lớn, đục ở cạnh đáy 40 mm từ đầu trái (mục 4.4.8) |
| Vi_Du_26 (26-067249) | SS dày 1, lỗ tròn cách biên đúng 3 mm vẫn vào bên trái (mục 4.3.7) |
| Vi_Du_27 (26-067424) | SUS dày 3, rãnh đứng ngắn không joint vào đầu trên (4.3.2); lỗ dài cong cắt trước hai rãnh thẳng vì diện tích chênh < 6% (mục 3) |
| NC_Tool_Fix: 26-062754 | Thông số CU (6, 0.7), lỗ chữ nhật xoay 45° vào giữa cạnh từ đỉnh thấp nhất (mục 4.3.6) |
| NC_Tool_Fix: 26-062203 | SS dày 6, kegaki hở chạm cạnh vát, kéo dài theo hướng đường (mục 4.1.3) |

## 10. Chưa hỗ trợ hoặc cần lưu ý

1. Phôi tròn: điểm vào cố định tại góc 225°, chưa có quy tắc đổi hướng khi gần biên khác.
2. Đồng (CU) không phân biệt độ dày, kể cả từ 19 mm trở lên (không đục cong).
3. Bo R tự động: phần mềm chỉ kiểm tra góc nhọn với SS dày từ 19, không tự bo.
4. Lỗ nhỏ hơn ngưỡng ở mục 5 không có joint (theo quy tắc), nhưng vẫn theo quy tắc điểm vào ở mục 4.3.
## Lịch sử cập nhật

| Ngày | Nội dung |
|---|---|
| 18-10-2026 | Lập file tổng hợp từ toàn bộ quy tắc hiện có |
| 19-10-2026 | Thêm 5 quy tắc từ các ca NC_Tool_Fix: lỗ không có phần nhô xét theo trục trước (4.3.4); điểm vào không trùng đỉnh (4.3.5); nhiều cạnh trái bằng nhau chọn cạnh thấp nhất (4.4.1); phôi gần tròn dùng quy tắc 225° (4.4.6); nhiều điểm J (4.5) |
| 21-10-2026 | Thêm đồng thau 真鍮 vào nhóm CU (cùng thông số với đồng); trước đó cột vật liệu 真鍮 trong Excel không được nhận ra |
| 21-10-2026 | Thêm 4 quy tắc từ 3 ca NC_Tool_Fix: dung sai cạnh thẳng đứng 0.01 mm (4.4.4); kegaki hở kéo dài theo hướng đường (4.1.3); kegaki vòng kín ngược chiều kim đồng hồ, không G41 (4.1.1); SS dày ≥ 19 joint ngoài 3 (mục 2, 6), sửa Vi_Du_5 và Vi_Du_1 (kegaki vòng kín) cho khớp |
| 22-10-2026 | Mọi kegaki (hở, kín, tròn) bỏ `G41` sau khi bật tia đánh dấu (4.1.4); sửa Vi_Du_1 và Vi_Du_2 |
| 24-10-2026 | Thêm quy tắc lỗ nghiêng: vào giữa cạnh xuất phát từ đỉnh thấp nhất (4.3.6), theo ca 26-062754 |
| 24-10-2026 | Hỗ trợ SPLINE: chia thành đoạn thẳng bằng cách chia đôi tham số, võng ≤ 0.01 mm (mục 1.7), theo ViDu_Spline; thêm Vi_Du_11 |
| 24-10-2026 | Biên có đường cong (spline hoặc cung) lồi ra trái nhất: không xoay, đục ở cạnh đáy (4.4.8), theo ViDu_Spline_1; thêm Vi_Du_12 |
| 28-10-2026 | Thêm 4 quy tắc từ 3 ca NC_Tool_Fix: OCS/UCS lật gương cho CIRCLE, ARC, LWPOLYLINE, POINT (1.8); đầu kegaki trên cung bo góc tính là chạm biên (4.1.3); khe cắt bỏ G41 (4.2); lỗ dài cong có joint khi trục ≥ 10 (5.1). Sửa Vi_Du_1, thêm Vi_Du_13..15 |
| 28-10-2026 | Giữ đoạn ngắn hơn dung sai nối đầu mút, không gộp hai đầu của cùng một đoạn (1.9), sửa lỗi "biên dạng bị hở" ở ca 26-063561, thêm Vi_Du_16 |
| 01-11-2026 | Lỗ tròn thử vào trái, phải, dưới, trên theo khoảng cách > 3 mm (4.3.7); nhiều cạnh trái chọn cạnh thấp nhất thay vì dài nhất (4.4.1); cạnh thẳng > 100 mm của phôi cong vào 40 mm từ đỉnh dưới (4.4.7). Kegaki tròn vẫn không G41 (mẫu 26-064456T có dòng G01G41 do nhầm, đã xác nhận). Thêm Vi_Du_17..19 |
| 03-11-2026 | Lỗ nghiêng vào cạnh ngắn nhất, bằng nhau lấy cạnh thấp (4.3.6); nhiều cạnh đáy thì chọn cạnh trái nhất (4.4.2). Thêm Vi_Du_20, 21. (Mẫu T lần đầu của hai ca này có lỗ tròn vào bên phải do nhầm, mẫu gửi lại vào bên trái; không thêm quy tắc riêng cho lỗ tròn)
| 04-11-2026 | Nối đầu mút chỉ giữa các đối tượng cùng màu, kegaki không dính vào biên ngoài (1.3), theo ca 26-065663. Thêm Vi_Du_22 |
| 07-11-2026 | Dày ≥ 19: biên ngoài hở có sẵn đường dẫn vào piercing cong thì cắt theo đường vẽ (6.3); mỗi nút nối tối đa 2 đầu mút (1.3). Thêm Vi_Du_23 |
| 14-11-2026 | Thêm khách hàng đặc biệt đọc từ cột 得意先 của Excel: khách 824 không tạo joint cho lỗ bên trong, mọi vật liệu (mục 2.5) |
| 14-11-2026 | Vật liệu A1050 và ＡＬその他 tính như nhôm (mục 2.1); không tự xoay hình, giữ nguyên hướng DXF (mục 4.4.4) |
| 14-11-2026 | Theo 4 ca Fix_Update: biên không có cạnh thẳng đứng chạm điểm trái nhất (cả cạnh xiên) đục ở cạnh đáy (4.4.8); lỗ tròn cách biên đúng 3 mm vẫn dùng (4.3.7); lỗ rãnh đứng không joint vào đầu trên, có joint vào đầu dưới, phần nhô không xếp theo hướng tâm (4.3.2); diện tích chênh < 6% coi là bằng nhau khi xếp thứ tự lỗ (3.5). Thêm Vi_Du_24..27 |
