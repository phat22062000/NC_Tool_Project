# Lịch sử làm việc 19-10-2026

## Xử lý 5 ca lỗi trong NC_Tool_Fix
Bản tool cũ tái hiện đúng lỗi (file F) ở cả 5 ca. Đã thêm quy tắc và sửa `src\Core.cs`. Kết quả: cả 5 file khớp từng dòng với file T (trừ dòng tiêu đề).

| Ca | Quy tắc thêm |
|---|---|
| 26-060780 (SS t6) | Lỗ không có phần nhô xét theo trục trước: lỗ đứng vào giữa cạnh đáy, lỗ ngang hoặc vuông vào giữa cạnh trái. Thứ tự trái, phải, đáy, đỉnh chỉ để tránh vị trí cách biên khác ≤ 3 mm. |
| 26-060781 (SS t6) | Điểm vào không trùng đỉnh hoặc đầu đoạn (sai số 0.001 mm); không xuất cung dài 0. Nguyên nhân: DXF lệch 1e-5 làm điểm cuối cung thắng giữa cạnh. |
| 26-061080 (SS t2.3) | Nhiều cạnh trái dài bằng nhau: chọn cạnh thấp nhất. |
| 26-061298 (SS t6) | Nhiều điểm J: sắp theo chiều kim đồng hồ từ LJ, mỗi cặp liên tiếp là một đường cắt, xóa mọi POINT trùng J hoặc LJ. |
| 26-061341 (SS t4.5) | Đoạn trái nhất là cung lớn quét ≥ 180° có điểm 225°: áp dụng quy tắc phôi tròn (225°, hướng tâm ra ngoài, G02). |

## Kiểm tra hồi quy
Vi_Du_1, 3, 4, 5, 6, 7, 8, 9, 10 khớp hoàn toàn. Vi_Du_2 vẫn 17/22 khối giống, năm khối lệch là các điểm vào đã được chấp nhận trước đây.

## Tài liệu
Đã cập nhật `Quy_Tac_NC_Tool.md` (mục 4.3.4, 4.3.5, 4.4.1, 4.4.6, 4.5, 5.6, bảng ví dụ, lịch sử) và tạo lại dashboard.

## Còn lại
- Bộ cài `NC_Tool_Setup.exe` chưa build lại (chờ bạn yêu cầu).
- Tác vụ định kỳ `luu-lich-su-nc-tool-cuoi-ngay` vẫn trỏ thư mục cũ `NC_Tool`, cần đổi sang `NC_Tool_Project`.
