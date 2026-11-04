# Lịch sử làm việc 28-10-2026

## 3 ca NC_Tool_Fix mới
Cả 3 file khớp T từng dòng, kể cả dòng tiêu đề.

| Ca | Nguyên nhân | Quy tắc thêm |
|---|---|---|
| 26-061922 (SS 3.2) | Hai CIRCLE trong DXF có vector pháp tuyến (0, 0, −1), tức vẽ trong UCS lật gương, nên tọa độ lưu là −321.8 và −171.8 trong khi vị trí thật là 321.8 và 171.8. Ngoài ra khe cắt ở mẫu T không có G41. | Đổi OCS sang WCS theo thuật toán trục tùy ý cho CIRCLE, ARC, LWPOLYLINE, POINT. Khe cắt bỏ G41. |
| 26-063986 (SS 6) | Hai kegaki hở bắt đầu tại (0.268, 1) và (0.268, 31), nằm đúng trên cung bo góc R2 của biên ngoài. Tool chỉ nhận cạnh thẳng nên không coi là chạm biên. T đục lùi 3 mm theo hướng đường kegaki tại (−2.732, 1). | Đầu nằm trên cung bo góc của biên ngoài cũng tính là chạm biên. |
| 26-063771 (SUS 4) | Hai lỗ dài cong (đầu R2.5, trục khoảng 10.06) có kích thước ngoài 14.98, nhỏ hơn 15 một chút nên tool không gắn joint. T có joint 0.7. | Lỗ dài cong: hai đầu bo tròn cùng bán kính nối bằng hai cạnh cung; có joint khi chiều dài trục (trung bình độ dài hai cạnh) ≥ 10. |

## Kiểm tra hồi quy
5 ca NC_Tool_Fix cũ không đổi so với bản trước. Vi_Du_2 vẫn lệch như trước. Khe cắt không còn G41 nên đã sửa 4 dòng trong `Vi_Du_1.nc`. Thêm ba ví dụ `Vi_Du_13..15` từ file T. Thử thêm DXF tự tạo có ARC lật gương (hai nửa cung tạo vòng tròn tại −50 thành lỗ tại x = 50): đúng.

Đã cập nhật `Quy_Tac_NC_Tool.md` (1.8, 4.1.3, 4.2, 5.1) và dashboard. Bộ cài chưa có các thay đổi này.

## Ca 26-063561 (SUS 1.5): tool báo biên dạng hở
Lỗi: "Biên dạng bị hở hoặc rẽ nhánh tại (1225.36, 875.36)". Bản vẽ phôi 1257 × 907 có 4 khía góc. Mỗi khía có 2 đoạn thẳng dài chỉ 0.008 mm (ví dụ từ (1225.360, 882.720) đến (1225.368, 882.720)). Bước nối đầu mút với dung sai 0.02 mm gộp hai đầu thành một điểm, đoạn này thành đoạn dài 0 tự nối chính nó, nên tám nút có bậc 4 và vòng kín không còn là vòng đơn.

Cách sửa đầu tiên (bỏ đoạn dài 0) làm tool chạy được nhưng NC không khớp mẫu T của bạn. Mẫu T giữ cả đoạn 0.008 mm (`G01X25.Y875.36`) và cung đi tới điểm cuối thật (`G02X25.Y875.368I0.212J-0.212`). Sửa lại (`SnapEndpoints` trong `src\Core.cs`): gộp các cặp đầu mút từ gần đến xa và không gộp nếu nhóm mới chứa cả hai đầu của cùng một đoạn. Kết quả với SUS 1.5: 54/54 dòng, khớp `26-063561T.nc` từng dòng kể cả dòng tiêu đề. 5 ca NC_Tool_Fix cũ và Vi_Du_1..15 không đổi. Đã chép ví dụ thành `Vi_Du_16(SUS-t1.5)`, sửa mục 1.9 và dashboard. Bộ cài chưa có thay đổi này.
