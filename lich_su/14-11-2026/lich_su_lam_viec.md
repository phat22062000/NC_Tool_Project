# Lịch sử làm việc 14-11-2026

## Khách hàng đặc biệt 824
Yêu cầu: khách 824 (cột khách hàng trong Excel) không tạo joint cho lỗ bên trong, kể cả SUS, AL, CU.

Cột khách hàng trong mẫu CAD依頼書 có tiêu đề `得意先`: cột G là mã số, cột H là tên. Mã 824 là `武野工業`, có trong `521110-12_依頼.xlsx` (dòng 26-067058, SUS 304-2B dày 1.5).

Đã sửa:
- `src\Excel.cs`: nhận cột khách hàng (`得意先`, `客先`, `顧客`, `khach hang`, `customer`), lưu mã vào `MatRow.Customer`. Lệnh `--list` in thêm cột khách hàng.
- `src\Core.cs`: `Generate` có thêm tham số `customer`. Với khách 824, độ rộng joint lỗ bằng 0 (giống SS), có cảnh báo. Joint biên ngoài không đổi.
- `src\Program.cs`: truyền khách hàng từ dòng Excel vào `Generate`. Khi chỉnh tay vật liệu hoặc độ dày của file có trong Excel thì vẫn giữ khách hàng. Dòng lệnh nhận thêm mã khách làm tham số cuối.

Kiểm tra: đọc đúng mã 824 của file 26-067058. Cùng một DXF SUS, có khách 824 khác bản thường 53 dòng (Vi_Du_2) và 4 dòng (Vi_Du_15), nghĩa là các lỗ có joint nay khép kín. AL dày 19 không đổi vì không có lỗ cần joint. Không có khách hàng thì toàn bộ Vi_Du giữ nguyên kết quả. Đã cập nhật `Quy_Tac_NC_Tool.md` (mục 2.5) và dashboard. Chưa build lại bộ cài, chưa đẩy GitHub.

## Vật liệu nhôm A1050, ＡＬその他
`GroupOf` (`src\Excel.cs`) nhận thêm `A1050` vào nhóm AL. Chữ toàn góc được chuẩn hóa (NFKC) trước khi nhận dạng, nên `ＡＬその他` thành `ALその他` và vào nhóm AL; `その他` đứng một mình vẫn không nhận dạng. Cách chuẩn hóa này cũng giúp nhận `ＳＵＳ304` và các chữ toàn góc khác. Kiểm tra bằng phản xạ: `A1050`, `a1050`, `ＡＬその他`, `アルミ`, `A5052` đều thành AL.

## Không tự xoay hình
Vòng thử xoay 90°, 180°, 270° trong `Generate` (`src\Core.cs`) chỉ còn xét hướng gốc. Nếu không có cạnh thẳng đứng sát Oy thì dùng quy tắc phôi cong hoặc xiên như các phôi đã có. Phôi vẫn được dời về gốc (0, 0), đây là chỗ tôi hiểu "để im" chỉ nói về hướng. Bạn đã xác nhận giữ việc dời gốc.

Thử với tam giác chỉ có cạnh dọc bên phải: bản cũ xoay 180° và dời gốc, bản mới giữ nguyên hướng và đục theo quy tắc phôi xiên. Các ví dụ Vi_Du và 5 ca NC_Tool_Fix cũ không đổi vì không ca nào cần xoay. Đã cập nhật `Quy_Tac_NC_Tool.md` và dashboard. Chưa build lại bộ cài, chưa đẩy GitHub.

## 4 ca Fix_Update (26-068005, 26-068007, 26-067249, 26-067424)
Cả 4 ca nay khớp từng dòng với file T (trừ dòng tên file). Đã thêm làm Vi_Du_24..27; Vi_Du_1..27 vẫn khớp (Vi_Du_2 vẫn là ca lệch đã biết).

- **26-068005 (SS 3.2) và 26-068007 (SS 6):** file F do bản cũ tự xoay phôi. Không còn xoay, cạnh trái xiên (không có cạnh thẳng đứng chạm điểm trái nhất) nên đục ở cạnh đáy trên Ox: giữa cạnh nếu ≤ 100, cách đầu trái 40 mm nếu > 100 (mục 4.4.8, mở rộng điều kiện `splBottom` từ "trái là đường cong" sang "không có cạnh thẳng đứng chạm điểm trái nhất").
- **26-067249 (SS 1):** lỗ tròn cách biên đúng 3 mm vẫn được chọn vào bên trái. Ngưỡng đổi từ > 3 thành ≥ 3 mm (4.3.7, 5.3).
- **26-067424 (SUS 3):** (1) rãnh đứng ngắn (tâm-tâm 6, không joint) vào đầu trên, rãnh có joint (Vi_Du_1) vẫn vào đầu thấp; (2) các phần nhô chỉ xếp theo bán kính rồi theo trục, không theo hướng so với tâm lỗ; (3) lỗ dài cong 52.4 mm² và hai rãnh thẳng 49.6 mm² chênh 5.6%, T cắt lỗ dài cong (gần nhất) trước, nên diện tích chênh < 6% coi là bằng nhau khi xếp thứ tự lỗ (3.5). File F của ca này dùng thông số SS nên không dùng để so; T dùng SUS 3 (piercing 6, joint 0.7).

Code: `src\Core.cs` (`EntryCandidates(loop, vertTop)`, dung sai diện tích 6%, ngưỡng ≥ 3, `splBottom`). Đã cập nhật `Quy_Tac_NC_Tool.md` và dashboard. Chưa build lại bộ cài, chưa đẩy GitHub.
