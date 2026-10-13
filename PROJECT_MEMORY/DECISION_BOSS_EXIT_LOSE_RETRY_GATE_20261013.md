# TLTD — quyết định thoát game khi đánh Boss

Ngày 13/10/2026, UTC+07. Phân loại: **LOCKED DECISION — OWNER APPROVED**.

Owner trả lời câu hỏi về phần D18.25 còn chưa chốt: **“Tính thua, giữ cổng 100% để thử lại (đề xuất)”**. Việc chọn phương án này là phê duyệt policy sau:

- Thoát game khi đang đánh Boss được tính là thua trận Boss đó.
- Giữ cổng Boss ở 100%; người chơi có thể thử lại.
- Không cấp thưởng thắng Boss, không mở Stage kế tiếp và không rollback thưởng quái thường đã nhận hợp lệ.

Đây là quyết định sản phẩm cho phạm vi thoát game trong Boss. Nó supersede đúng phần exit/reset/resume chưa chọn ở D18.25; không tự phê duyệt toàn bộ Stage Gate spec, Phase B, balance Boss, Bun, offline hoặc Companion.

Background/focus-loss tạm thời chưa được đồng nhất với thoát game. Khi làm lifecycle mobile phải phân biệt chúng. Yêu cầu kỹ thuật: checkpoint bền vững phải ghi nhận trận Boss đang diễn ra trước khi combat bắt đầu; một trận đã ghi đang đánh nhưng bị bỏ dở qua cold start không được sinh win reward hoặc advance. Schema, thứ tự ghi/restore và xử lý lỗi lưu sẽ được review trong slice persistence; chưa có implementation hoặc test được nghiệm thu từ quyết định này.

P08/P09-A/P09-B giữ ACCEPTED/LOCKED; Phase B cũ vẫn STOPPED. Không cần Owner chơi lại, đóng ZIP hoặc chạy lại recovery vì quyết định này.

Nguồn: câu trả lời trực tiếp của Owner trong phiên tiếp tục dự án. Tài liệu được lưu local trước; chỉ được gọi đã đồng bộ canonical sau commit/push và remote readback có receipt.
