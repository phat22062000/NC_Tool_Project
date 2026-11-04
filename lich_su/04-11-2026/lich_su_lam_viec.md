# Lịch sử làm việc 04-11-2026

## Ca 26-065663 (SS 6)
File khớp T từng dòng, kể cả dòng tiêu đề.

Kegaki chữ L có hai đầu chạm biên ngoài: đầu phải (27, 6.5) nằm trên cạnh x = 27, đầu trên (15, 12.49) cách đỉnh biên ngoài (15, 12.5) đúng 0.01 mm trong DXF. Bước nối đầu mút (dung sai 0.02 mm) của tool kéo đầu 12.49 lên thành 12.5, dính vào biên, nên tool coi cả hai đầu đều chạm biên và chọn đầu gần gốc nhất: đục tại (15, 15.5) và đường cắt đi đến y = 12.5. Mẫu T đục tại (30, 6.5), cắt ngược về (15, 6.5) rồi lên đến y = 12.49.

Sửa (`SnapEndpoints` trong `src\Core.cs`): chỉ nối đầu mút giữa các đoạn cùng màu. Kegaki không còn bị dính vào biên ngoài, đầu 12.49 giữ nguyên, tool chọn đầu duy nhất chạm biên. 5 ca NC_Tool_Fix cũ và Vi_Du_1..21 không đổi. Thêm `Vi_Du_22`. Đã cập nhật `Quy_Tac_NC_Tool.md` (mục 1.3) và dashboard. Bộ cài chưa có thay đổi này.
