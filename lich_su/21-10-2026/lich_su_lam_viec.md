# Lịch sử làm việc 21-10-2026

## Xử lý 3 ca NC_Tool_Fix mới
Đã sửa `src\Core.cs`. Cả 3 file khớp từng dòng với T (trừ dòng tiêu đề).

| Ca | Quy tắc |
|---|---|
| 26-062063 (SS t1.6) | Dung sai cạnh thẳng đứng sát Oy (và nằm ngang sát Ox) tăng từ 0.0001 lên 0.01 mm. Cạnh trái trong DXF lệch 0.00014 mm khiến tool xoay nhầm 90°. |
| 26-062203 (SS t6) | Kegaki hở chạm biên: đục kéo dài 3 mm ngược chiều đoạn đầu của đường kegaki, không theo pháp tuyến của biên. |
| 26-061939 (SS t19) | Kegaki vòng kín: ngược chiều kim đồng hồ, không G41. SS dày ≥ 19: joint ngoài 3 (đường cắt dừng cách điểm vào 3 mm). |

## Kiểm tra hồi quy
- 5 ca NC_Tool_Fix của ngày 19-10 cho kết quả giống hệt bản exe cũ.
- Vi_Du_1: kegaki vòng kín cũ là CW kèm `G01G41`, mâu thuẫn với T của 26-061939. Bạn quyết định áp dụng quy tắc mới cho mọi trường hợp, nên đã sửa file `.nc` (2 dòng) và hiện khớp hoàn toàn.
- Vi_Du_5 đã sửa file `.nc` theo joint 3 (dòng cuối Y47, độ dài cắt 403.6).
- Các ví dụ còn lại giữ nguyên kết quả. Vi_Du_2 vẫn 17/22 khối giống như trước.

## Đồng thau 真鍮
Thêm 真鍮 (và シンチュウ, C2600, C2680, C3604) vào nhóm CU trong `src\Excel.cs`, thông số giống đồng. Đổi nhãn ô vật liệu thành "CU – Đồng / Đồng thau" và mở rộng ô cho vừa chữ (`src\Program.cs`). Thử với `521021-15_依頼.xlsx`: 26-062208 và 26-062209 nhận CU (dày 2 và 1.6), 26-062279 và 26-062280 vẫn là SUS.

## Lưu ý khác
- Thư mục bộ cài đã đổi tên thành `Desktop\SetupToolNC` (build_installer.ps1 vẫn mặc định ghi vào `Desktop\NC-Tool`). Bộ cài chưa build lại với các thay đổi hôm nay.
- Thư mục NC_Tool_Fix đã dọn các ca cũ, chỉ còn 3 ca mới và thư mục `521021-15(Fix CU thanh Brass)` chưa xử lý.
