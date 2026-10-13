# TLTD — Codex trực tiếp triển khai; mục tiêu Windows và Android

Ngày13/10/2026, UTC+07. **OWNER-APPROVED WORKFLOW / PLATFORM DECISION.**

Owner yêu cầu Codex trực tiếp code TLTD thành game hoàn chỉnh, “không cần dùng antigravity nữa”; sau đó chọn **cả Windows và Android** cho bản chơi đầu tiên. Codex nhận vai trò Tech Lead/Architect kiêm executor code/test/build; không tiếp tục giao prompt để Owner chuyển cho Antigravity. Owner vẫn quyết định sản phẩm/balance/content chưa chốt và kiểm trải nghiệm khi cần bằng thao tác cụ thể.

Quyết định này supersede yêu cầu Antigravity là executor bắt buộc trong AI_RULES/CONTINUITY_AND_SYNC_AUTHORITY và latest handoff. Các prompt Antigravity PREP-01/R1 là hồ sơ lịch sử; không execute packet bị review reject hoặc dùng false context cũ làm run authority. Codex có quyền thực hiện source changes cần thiết trong E:/code/TLTD và tự compile/test/build trong clone/profile mới cô lập, với review source/guard trước actual Unity launch. Không yêu cầu Owner chạy prompt Antigravity.

Phạm vi triển khai theo luật gameplay đã được duyệt. Roadmap là thứ tự kỹ thuật; số balance, cost, reward, feature behavior còn TBD không tự thành LOCKED. Đợt trực tiếp đầu tiên: equipment/inventory identity/loadout persistence và restore, authored Công Pháp unlock/selection guards/UI, natural channel→loot→decision→drain verification; chuẩn bị player build Windows/Android dựa Unity6000.6.0f1 hiện có và kiểm module thật. Đợt này không tự mở toàn skill assets hoặc rollout content chưa duyệt.

Bảo toàn source dirty của Owner trước khi apply: HEAD7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425,9C#+3Markdown dirty,530untracked file rows. Backup/compare originals, exact allowlist/diff và evidence mỗi batch; không reset/clean/stash/pull hoặc overwrite dirty docs/save. Source E không tự stage/commit/push vì thay vai trò; publication continuity vẫn documentation-only trong checkout riêng đã xác minh.

P08/P09-A/P09-B ACCEPTED/LOCKED và raw/recovery limits giữ nguyên. Old frozen lab/Phase B trạng thái lịch sử không được gọi đã chạy/đóng; workflow mới không ép replay/ZIP hoặc recovery rerun. Boss-exit LOSE/gate100% retry đã chốt vẫn giữ; focus/background và balance/content chưa có quyết định không được bịa. Tài khoản game mẫu không điều khiển tự động nữa; khảo sát mẫu là partial reference evidence, không phải authority cho mọi cơ chế.

Tiêu chí hoàn chỉnh: có vòng chơi playable theo contract, lưu/mở lại đúng, paths lỗi/thua/quay lại rõ, kiểm Unity/player thật và build cho hai nền tảng. Compile/test slice PASS không bằng game hoàn chỉnh. Nếu build module hoặc thiết bị thiếu, báo đúng prerequisite/evidence còn thiếu; không tuyên bố đã có APK/Windows executable khi chưa build và xác minh.
