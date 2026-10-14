# TLTD — Android: tạm dừng Boss khi chuyển ứng dụng/khóa màn hình

Ngày 14/10/2026, UTC+07. **OWNER-APPROVED PRODUCT DECISION.**

Owner chọn: “Tạm dừng trận và timer; quay lại thì tiếp tục” cho tình huống đang đánh Boss trên Android rồi chuyển sang ứng dụng khác hoặc khóa màn hình.

- App bị đưa xuống nền/khóa màn hình: tạm dừng trận và timer Boss. Trở lại cùng tiến trình: tiếp tục lần thử hiện tại; không tự tính thua, cấp thưởng, hồi HP/Nộ hoặc làm mới quyền hồi sinh.
- Đóng game hoặc tiến trình bị kết thúc trong lúc Boss đang chạy: quyết định thoát Boss đã chốt vẫn áp dụng — tính thua, giữ cổng 100% để thử lại; không thưởng thắng hoặc mở ải.
- Không đồng nhất `OnApplicationFocus(false)` do bàn phím/UI với việc app thực sự xuống nền. Implementation phải dùng lifecycle phù hợp và kiểm chứng trên Android; APK build thành công không thay bằng chứng pause/resume trên thiết bị.

Quyết định này bổ sung cấu hình thử nội bộ ngày 14/10, không mở offline Boss hoặc tự duyệt lifecycle Normal/Bun/offline. Runtime integration và device verification chưa hoàn tất tại thời điểm ghi quyết định.
