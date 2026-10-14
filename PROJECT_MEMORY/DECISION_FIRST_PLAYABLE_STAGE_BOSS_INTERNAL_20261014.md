# TLTD — Owner chọn cấu hình thử nội bộ Ải1 → Boss1 → Ải2

Ngày14/10/2026, UTC+07. **OWNER-APPROVED INTERNAL PLAYTEST CONFIGURATION.**

Owner trả lời “Dùng bộ đề xuất cho bản thử nội bộ” cho câu hỏi gồm đầy đủ các thông số dưới đây. Quyết định cho phép Codex trực tiếp triển khai và kiểm chứng lát cắt này. Đây là cấu hình thử nội bộ, chưa phải balance phát hành hoặc nghiệm thu trải nghiệm.

| Phần | Cấu hình được chọn |
|---|---|
| Ải1 | 50 quái thường hợp lệ mở cổng Boss; normal profile hiện có |
| Boss1 | Entity/config riêng, 2.500 HP, 50 Công, 10 Thủ, timer60s |
| Kỹ năng Boss | Đơn mục tiêu1,2× Công, cooldown6s; Tuyệt kỹ2× Công, Nộ80; dùng pipeline chiến đấu chung |
| Thưởng Boss1 thắng lần đầu | 1.000 Vàng +100 EXP; không Nguyên Liệu; không tự thêm item/currency |
| Vào/thử lại Boss | Hồi đầy HP; giữ Nộ/cooldown; làm mới quyền hồi sinh một lần cho lần thử mới |
| Ải2 | Yêu cầu cấp4, tiến độ ban đầu0/50; không tạo hard-cap2ải |

Các luật đã chốt giữ nguyên: timer60s; Hero chết là thua ngay; thua hoặc thoát giữa Boss giữ cổng100% để thử lại; không rollback thưởng quái thường; không thưởng thắng/mở ải khi thua hoặc thoát; Boss độc lập Bun; Tách chỉ trả Vàng. Normal kill10EXP giữ theo authority hiện hành.

Đề xuất kiến trúc/triển khai trong TLTD_FIRST_PLAYABLE_SLICE_PROPOSAL_20261014.md vẫn cần review kỹ thuật. Câu trả lời này không tự duyệt mọi roadmap, economy, Bun/offline/Companion, đổi/nâng Tâm Pháp hoặc title-cost discrepancy còn mở. Không rollout toàn bộ skill assets.

Triển khai phải giữ working tree/save, tách source staging và backup, kiểm natural frames + cold restart + player thực trong clone/profile riêng. Source Git E không tự commit/push; P08/P09-A/P09-B, raw-log limits và recovery history vẫn ACCEPTED/LOCKED đúng scope. Không yêu cầu chơi lại/ZIP phần đã xong nếu không có thay đổi hoặc rủi ro mới cụ thể.
