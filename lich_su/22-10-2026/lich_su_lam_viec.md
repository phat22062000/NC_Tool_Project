# Lịch sử làm việc 22-10-2026

## Kegaki bỏ G41
Theo yêu cầu: sau khi bật tia đánh dấu (`M98P9017`) không dùng `G41` cho mọi kegaki.
- Kegaki hở: `G01G41X…` thành `G01X…` (`src\Core.cs`).
- Kegaki tròn: bỏ dòng `G01G41` đứng riêng, `G02` đi ngay sau `M98P9017`.
- Kegaki vòng kín đã không có G41 từ 21-10-2026.

Đã sửa file ví dụ cho khớp: `Vi_Du_1.nc` (1 khối kegaki), `Vi_Du_2.nc` (3 khối). Chạy lại: Vi_Du_1 khớp hoàn toàn, Vi_Du_2 giữ nguyên các khác biệt cũ (150 dòng lệch, cùng số như trước), các ví dụ còn lại và 5 ca NC_Tool_Fix cũ không đổi.

Đã cập nhật `Quy_Tac_NC_Tool.md` (mục 4.1) và tạo lại dashboard. Bộ cài `SetupToolNC\NC_Tool_Setup.exe` chưa có thay đổi này.
