# Lịch sử làm việc 24-10-2026

## Ca 26-062754
Bản tool hiện tại tái hiện đúng file F khi chạy với thông số CU 6 (joint 0.7). Vì vậy F và T được tạo với thông số CU, dù tên file ghi SS-t6 (SS t6 sẽ có joint 1). Chạy lại với CU 6, file khớp T từng dòng.

Khác biệt duy nhất nằm ở lỗ chữ nhật 12 × 20 xoay 45°. Tool vào đúng đỉnh trái nhất (11.186, 25.328). T vào giữa cạnh dưới bên phải (29.571, 15.429), đục vào trong 6 mm, cắt ngược chiều kim đồng hồ.

Quy tắc thêm (`src\Core.cs`, `EntryCandidates`): lỗ toàn cạnh thẳng, không cạnh nào song song Ox/Oy, không có phần nhô và không phải tam giác thì vào giữa cạnh xuất phát từ đỉnh thấp nhất theo chiều cắt ngược chiều kim đồng hồ. Bạn đã xác nhận quy tắc này đúng ý: vào giữa cạnh xuất phát từ đỉnh thấp nhất.

Kiểm tra: 5 ca NC_Tool_Fix cũ và các Vi_Du không đổi so với trước. Đã cập nhật `Quy_Tac_NC_Tool.md` (4.3.6) và dashboard. Bộ cài chưa có thay đổi này.

## Hỗ trợ SPLINE (ViDu_Spline)
Trước đây DXF có SPLINE bị báo lỗi. Đã thêm `SplinePolyline` trong `src\Core.cs`: đọc B-spline (bậc, knot, điểm điều khiển, trọng số), chia đôi tham số theo đệ quy, chia tiếp khi độ võng lớn nhất tại 1/4, 1/2, 3/4 tham số so với dây cung vượt 0.01 mm. Tiêu chí này tách đúng toàn bộ 327 đoạn của `ViDu_Spline.nc` (các điểm nằm đúng tại phân số nhị phân của tham số). Kết quả với SS 6: 187/187 dòng, khớp từng dòng. Spline chỉ có điểm fit thì nối thẳng các điểm đó. Đã chép ví dụ vào `Vi_Du_11(SS-t6)`, cập nhật mục 1.7 trong `Quy_Tac_NC_Tool.md` và dashboard. Các ca cũ và Vi_Du khác không đổi.

## Ca ViDu_Spline_1
Spline lồi ra bên trái (x nhỏ nhất 3.537, cạnh thẳng đứng gần nhất ở x=5.432 dài 18). Bản cũ xoay 180° để lấy cạnh phải làm cạnh trái nên sai toàn bộ. Mẫu T không xoay, giữ nguyên tọa độ DXF và đục ở cạnh đáy: cạnh đáy dài 146 > 100 nên đục cách đầu trái (7.432) 40 mm, tức (47.432, -6).

Quy tắc thêm: nếu điểm trái nhất của biên ngoài nằm trên spline và có cạnh đáy thẳng trên Ox dài từ 10 mm thì không xoay, đặt gốc theo bao điểm điều khiển spline và đục ở cạnh đáy. Tiêu đề kích thước tính cả gốc (0, 0), nên rộng 155.4. Kết quả khớp T từng dòng (chỉ khác dòng tên file). Quy tắc chỉ áp dụng khi điểm trái nhất là spline, chưa mở rộng cho cung tròn (chờ bạn xác nhận). Đã chép ví dụ vào `Vi_Du_12(SS-t6)`, cập nhật mục 4.4.8 và dashboard. Các ca cũ và Vi_Du khác không đổi.

## Mở rộng quy tắc cạnh đáy cho cung tròn
Theo yêu cầu, quy tắc "đường cong lồi ra trái nhất" áp dụng cho cả cung tròn, không chỉ spline: điểm trái nhất nằm trên spline hoặc cung, không có cạnh thẳng đứng chạm điểm đó và có cạnh đáy thẳng trên Ox dài từ 10 mm thì không xoay, đục ở cạnh đáy. Chạy lại: ViDu_Spline_1 vẫn khớp, các ca NC_Tool_Fix cũ và Vi_Du_1..12 không đổi. Thử thêm DXF tự tạo (hình D, cung bán nguyệt bên trái, đáy 90 mm): đục tại (70, -6), giữa cạnh đáy, đúng quy tắc. Cập nhật mục 4.4.8 và dashboard. Bộ cài chưa có các thay đổi này.
