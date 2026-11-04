# Lịch sử làm việc 03-11-2026

## 2 ca NC_Tool_Fix: 26-065395 và 26-065394 (SS 3.2)
`acad.err` chỉ là lỗi AutoCAD khi mở DXF 26-065394, không liên quan tool.

Lần đầu bạn gửi mẫu T có lỗ tròn vào bên phải, tôi đã đoán một quy tắc riêng (phôi đục ở cạnh đáy thì lỗ tròn thử bên phải trước). Sau đó bạn nói có nhầm lẫn và gửi lại file. Mẫu T mới có lỗ tròn vào bên trái, đúng với quy tắc cũ, nên đã bỏ quy tắc đoán thêm đó. Cả hai file giờ khớp T mới từng dòng, kể cả dòng tiêu đề.

| Chỗ lệch ban đầu | Nguyên nhân | Quy tắc |
|---|---|---|
| Điểm đục ngoài | Đáy có hai đoạn thẳng trên Ox: 8.0 mm ở góc trái và 27.5 mm sau rãnh cắt. Tool chọn đoạn dài (62.751, −3), mẫu T chọn đoạn trái nhất (4.503, −3). | Nhiều cạnh đáy thì chọn cạnh trái nhất, giống nhiều cạnh trái chọn cạnh thấp nhất. |
| Lỗ chữ nhật nghiêng (26-065394) | Mẫu T vào giữa cạnh ngắn 21 mm đi tới đỉnh thấp nhất. Quy tắc cũ chọn cạnh xuất phát từ đỉnh thấp nhất, ra cạnh dài 32 mm. | Vào giữa cạnh ngắn nhất, bằng nhau lấy cạnh thấp. Ca 26-062754 vẫn đúng. |

## Kiểm tra hồi quy
5 ca NC_Tool_Fix cũ và Vi_Du_1..19 không đổi. Thử thêm DXF tự tạo hình chữ nhật xoay của ca 26-062754: vẫn vào (29.571, 15.429). Thêm `Vi_Du_20, 21` từ mẫu T mới. Đã cập nhật `Quy_Tac_NC_Tool.md` và dashboard. Bộ cài chưa có các thay đổi này.
