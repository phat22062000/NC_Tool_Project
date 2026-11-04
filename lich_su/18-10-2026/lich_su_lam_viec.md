# Lịch sử làm việc ngày 18-10-2026

Dự án: DXF To NC Conversion Tool (chuyển DXF trải phôi sang NC cho CamMagic LA).

## Nội dung đã làm

1. **Tăng cỡ chữ** các vùng trạng thái: tiêu đề "Danh Sách File" (12), vùng thả file (11), chữ giữa khung xem trước (13), dòng thông báo "Chưa có file" (11; khung cảnh báo cao 32 px).
2. **Phóng to hai nút lưu:** "Lưu File .nc" 180 × 46, "Lưu Tất Cả" 160 × 46, chữ 11.5; chân cửa sổ cao 72 px.
3. **Thẻ file:** dòng phụ (SS · Nhập Tay · t3.2) chữ 11; tên file chữ 14 đậm.
4. **Shortcut Desktop:** tạo shortcut "DXF To NC Conversion Tool" với biểu tượng giống giao diện. Icon bị Windows lưu đệm nên tạo file icon nhiều kích thước (16 đến 256 px), đặt tên mới, xóa bộ nhớ đệm icon bằng `ie4uinit`.
5. **Báo lỗi biên dạng hở:** LINE/ARC Layer 0 by layer không khép kín (biên ngoài hoặc lỗ) thì báo lỗi kèm tọa độ chỗ hở, chặn tạo NC. Đã thử bằng hai file DXF bị xóa một đoạn (một ở biên ngoài, một ở lỗ vuông). Khe cắt xanh lá và kẻ dấu đỏ vẫn được phép hở.
6. **Biểu tượng lỗi:** thẻ file lỗi hiện dấu X trong vòng tròn đỏ (thay dấu "!").
7. **Biểu tượng tiêu đề:** hộp 48 px, nền xanh dương, đầu cắt trắng, tia vàng cam để tương phản với nền; icon của file exe và shortcut đổi cùng màu (`DXF_To_NC_v2.ico`).
8. **Đổi tên ô số liệu:** Kích Thước Phôi, Độ Dài Cắt, Chạy Không, Số Lần Cắt.
9. **Dòng thông tin Excel** đổi sang chữ và nền xanh lá (cảnh báo vẫn vàng, lỗi vẫn đỏ).
10. **Hộp thoại sau khi lưu:** "Đã Chuyển Đổi Thành Công" sau Lưu File .nc hoặc Lưu Tất Cả; thiết kế riêng cùng tông màu giao diện (có bản thành công màu xanh và bản lỗi màu đỏ, hỗ trợ chế độ sáng và tối, rộng 640 px, đóng bằng nút Đóng, Enter hoặc Esc).
11. **Lưu lịch sử làm việc:** tạo `lich_su_lam_viec.md` (tổng hợp) rồi chia lịch sử theo từng ngày vào thư mục `lich_su\dd-MM-yyyy`.

## Chưa xong cuối ngày

- Chưa bấm lưu thật rồi xem hộp thoại thành công trong phần mềm (đã dựng thử và chụp hình hộp thoại).
- Phôi dài hẹp J/LJ, joint lỗ SUS/AL/CU, độ dày ≥ 19 mm vẫn chưa hỗ trợ.
- Chưa thử nhiều file DXF thật ngoài file mẫu.
