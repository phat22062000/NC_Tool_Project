# Lịch sử làm việc ngày 17-10-2026

Dự án: DXF To NC Conversion Tool (chuyển DXF trải phôi sang NC cho CamMagic LA).

## Nội dung đã làm

1. **Yêu cầu ban đầu và quy tắc màu.** Layer 0, by layer: Line/Arc/Circle là biên ngoài và lỗ (`M98P9010`), Point là lỗ trắng (`M98P9016`). Xanh lá: khe cắt và piercing xuyên (`M98P9010`). Đỏ: kẻ dấu (`M98P9017`). Nét đứt là đường uốn, bỏ qua. Tham khảo `Quy_Tac_Laser.md`, `Template_NC.dxf`, `Template_NC.nc`.
2. **Lõi chuyển đổi.** Tra piercing, joint theo vật liệu và độ dày; thứ tự đường chạy; laser R; điểm đục biên ngoài (cạnh trái, hoặc cạnh trên Ox khi cạnh trái dưới 10 mm). Kết quả khớp từng dòng với `Template_NC.nc`.
3. **Bản web** `NC_Tool.html` và `nc_core.js`.
4. **Phần mềm Windows `NC_Tool.exe`** (C# WinForms, biên dịch bằng `csc.exe` của Windows vì máy không có Python hay .NET SDK). Kết quả khớp từng byte với `Template_NC.nc`. Tạo shortcut Desktop.
5. **Đọc vật liệu và độ dày từ Excel/CSV**, hỗ trợ mẫu CAD依頼書 `520930-09_依頼.xlsx` (cột 作業No., 材質, 板厚; tên vật liệu ở cột bên cạnh mã số). Nhận dạng nhóm SS, SUS, AL, CU từ tên vật liệu.
6. **Chức năng bổ sung:** bỏ nút tạo Excel mẫu; chọn nhiều file bằng Shift/Ctrl, phím Delete gỡ file; lưu `.nc` xong thì gỡ DXF khỏi danh sách (file trên ổ đĩa giữ nguyên); ô Vật Liệu và Độ Dày nhập tay (dùng khi mã không có trong Excel); ẩn dòng báo lỗi đường dẫn Excel.
7. **Quy trình Terminal:** `build.ps1` in phần thay đổi code (+ xanh, - đỏ) và kết quả biên dịch.
8. **Giao diện mới** theo bản phác đã duyệt: thanh tiêu đề, thẻ file, ô số liệu, xem trước thu phóng bằng chuột, mã NC tô màu, cảnh báo có màu, chế độ sáng/tối.
9. **Tinh chỉnh giao diện:** chữ đầu mỗi từ viết hoa; chú thích xem trước đổi tên (Biên Dạng Ngoài Và Lỗ, Rạch, Kẻ Dấu, Through Hole, Piercing; ẩn 9017); nút sáng/tối đổi thành biểu tượng; tên phần mềm "DXF To NC Conversion Tool"; biểu tượng đầu cắt laser; thanh tiêu đề cao 74 px; chữ ô số liệu to và đổi màu; danh sách file rộng 320 px, thẻ cao 74 px.

## Chưa xong cuối ngày

- Phôi dài hẹp có điểm J/LJ (ví dụ 26-057101): chưa hỗ trợ.
- Joint lỗ cho SUS/AL/CU, độ dày ≥ 19 mm: chưa hỗ trợ.
- Chưa thử nhiều file DXF thật ngoài file mẫu.
