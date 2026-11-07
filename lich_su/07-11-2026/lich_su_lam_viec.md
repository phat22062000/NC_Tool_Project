# Lịch sử làm việc 07-11-2026

## Nhôm (AL) dày từ 19: ViDu_AL_t19
NC của AL 19 và AL 25 khớp mẫu từng dòng (chỉ khác dòng tên file).

DXF mẫu đã vẽ sẵn đường dẫn vào piercing cong: đoạn thẳng (−10, 60) đến (−10, 50), đoạn (−10, 50) đến (−3, 50), cung R3 đến (0, 53), rồi biên ngoài hình vuông 100 × 100, và cạnh trái bị tách thành hai đoạn có khe hở 0.005 mm (53 và 52.995). Vì vậy biên ngoài là đường hở, tool cũ báo "bị hở".

Hai chỗ sửa trong `src\Core.cs`:
- Dày ≥ 19: nếu có đúng một đường hở by layer với hai đầu mút thì coi là biên ngoài có sẵn đường dẫn vào. Điểm đục là đầu tự do xa tâm nhất, đường cắt đi đúng theo thứ tự đã vẽ. Không xoay, gốc tọa độ lấy theo phần biên ngoài sau cung dẫn vào.
- `SnapEndpoints`: mỗi nút nối tối đa 2 đầu mút. Trước đó đầu (0, 52.995) bị gộp vào nút (0, 53) đã có hai đoạn, tạo nút bậc 3 nên khe hở mất và hình thành hình "kẹo mút".

Thử thêm SS 19 và SUS 19 với cùng DXF cho cùng đường cắt. 5 ca NC_Tool_Fix cũ và Vi_Du_1..22 không đổi. Thêm `Vi_Du_23`. Đã cập nhật `Quy_Tac_NC_Tool.md` (mục 1.3, 6.3) và dashboard. Bộ cài chưa có thay đổi này.
