# Lịch sử làm việc 01-11-2026

## 3 ca NC_Tool_Fix mới
Cả 3 file khớp T từng dòng (bỏ qua dòng tên file).

| Ca | Nguyên nhân | Quy tắc thêm |
|---|---|---|
| 26-063378 (SS 3.2) | Lỗ φ6 tâm (5, 11) có điểm trái nhất cách mép phôi 2 mm. Mẫu T vào bên phải (8, 11). | Lỗ tròn thử vào trái, phải, dưới, trên và lấy vị trí đầu tiên cách biên dạng khác hơn 3 mm, cho mọi lỗ tròn (trước đây chỉ lỗ có joint, Ø ≥ 50). |
| 26-063416 (SS 1.6) | Cạnh trái bị hai rãnh chia thành ba đoạn 4.5, 70 và 4.5 mm. Tool chọn đoạn dài nhất 70 mm, mẫu T chọn đoạn thấp nhất 4.5 mm rồi đục ở cạnh đáy (cạnh trái dưới 10 mm). | Nhiều cạnh trái thì chọn cạnh thấp nhất, không chọn cạnh dài nhất. Ca 26-061080 (hai cạnh bằng nhau chọn cạnh thấp) vẫn đúng. |
| 26-064456 (SS 2.3) | (1) Kegaki tròn trong mẫu T có dòng `G01G41`, nhưng bạn xác nhận đây là nhầm, đúng là không có G41 (giữ nguyên hành vi hiện tại, đã bỏ dòng này trong `Vi_Du_19.nc`). (2) Cạnh trái nghiêng dài 199 mm: tool vào cách đầu trên 40 mm, mẫu vào cách đỉnh dưới 40 mm (đỉnh là giao với cung đáy, bỏ qua cung bo R0.5). Kegaki chạm cung đáy đã đúng nhờ quy tắc cung bo góc lần trước. | Cạnh thẳng dài hơn 100 mm của phôi cong vào cách đỉnh dưới 40 mm. |

## Kiểm tra hồi quy
5 ca NC_Tool_Fix cũ và Vi_Du_1..16 không đổi. `Vi_Du_2.nc` giữ nguyên không có G41 ở kegaki tròn (số dòng khác biệt vẫn 150 như trước). Thêm `Vi_Du_17..19` từ ba file T. Đã cập nhật `Quy_Tac_NC_Tool.md` và dashboard. Bộ cài chưa có các thay đổi này.
