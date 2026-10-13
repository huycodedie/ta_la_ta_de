# TLTD — tiến trình đi tới một game hoàn chỉnh

Ngày 13/10/2026, UTC+07. Đây là lộ trình kỹ thuật để tiếp tục công việc, **không phải toàn bộ thiết kế mới đã được phê duyệt**. Những luật đã LOCKED được giữ; balance/content/TBD không được tự điền bằng số liệu game tham chiếu.

## 1. Quyết định sản phẩm mới đã được Owner xác nhận

**Thoát game trong lúc đánh Boss → tính thua; giữ Cổng Boss ở 100% để thử lại.** Owner đã chọn phương án này trong câu trả lời ngày 13/10. Quyết định thay mục chưa chọn trong D18.25, không rollback phần thưởng quái thường, không cấp thưởng thắng Boss và không mở Stage kế tiếp.

Phạm vi được xác nhận là thoát game khi đang đánh Boss. Background/focus-loss tạm thời không tự đồng nhất với thoát game; cần quy định riêng khi làm vòng đời mobile. Lưu đúng checkpoint bền vững để nhận biết một trận Boss bị bỏ dở là yêu cầu kỹ thuật của policy mới; source Stage/Boss chưa triển khai.

## 2. Baseline thực tế

- Local E HEAD: `7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425` tại lần kiểm tra hiện tại; có tám bản sửa R01–R04 chưa commit và ba Markdown dirty có sẵn.
- Tám bản sửa đã áp dụng vào E; thêm một file MindMethodUI chọn skill đã mở. Compile standalone150inputs không lỗi; nhóm offline10/12/22/8 và UI27 đạt trong giới hạn đã ghi. Unity6000.6.0f1 EditMode fixture riêng đã đạt33/33 ownership/expired-load/builder, không natural PlayMode hoặc Unity input/visual acceptance. Phiên1 có lỗi import path trước adapter; phiên2 fixture ngắn xử lý đúng lỗi đó. Chi tiết trong TLTD_GAME_COMPLETION_PROGRESS_20261013.md.
- Main đọc qua `git ls-remote` trong lượt này: memory `208c0de611f9cbf1dc6b3705c30e2539a890c147`; source `cbfe7b547766ab32278a7f347b9f6fbe15ee5087`. Đây là mốc trước đồng bộ continuity mới, không khẳng định remote chứa các bản sửa local E.
- P08/P09-A/P09-B giữ ACCEPTED/LOCKED. R4 lab cũ chưa chạy; overall lab closeout còn pending; Stage Gate Phase B STOPPED.
- Không điều khiển lại tài khoản game tham chiếu. Khảo sát trước đó là dữ liệu tham khảo có giới hạn, không thay authority TLTD.

## 3. Thứ tự triển khai và điều kiện hoàn tất

| Chặng | Công việc | Điều kiện để chuyển tiếp |
|---|---|---|
| A — ổn định delta hiện tại | Kiểm chứng R01–R04 bằng fixture/project/profile mới cô lập; giữ riêng Unity compile, EditMode, natural PlayMode và visual evidence | Real ownership/timer/builder đúng trong scope; channel→loot→UI→modal drain→one advance được xác minh bằng natural frames trước khi dựa vào nó để mở gate |
| B — Công Pháp tương tác | Nối chọn slot và skill đã mở với SelectSkillForSlot hiện có; không đổi database, effect, flags hoặc reset HP/Rage/cooldown | Slot/alternative đúng, locked/sai slot/sai Tâm Pháp bị chặn; chọn/lưu không lặp; thao tác pointer thực tế và skill bar được kiểm trong Unity |
| C — lưu/load vòng chơi | Scope rõ item identity, túi/đồ đang mặc, level/EXP/title, Cấp Rơi và skill selections; chốt thứ tự restore trước combat | Reopen giữ đúng item/loadout/progression; không khởi tạo defaults đè state; giữ key/data legacy, không xóa save thật hoặc phát thưởng lại |
| D — Stage progress / Gate | Triển khai lát cắt nhỏ sau khi dependency kiểm chứng đạt: legitimate deaths, config50, midwave latch, wave/channel/loot/modal drain, giữ gate và checkpoint | Không đếm duplicate/ngoài roster; không trim wave sống; growth P08 once/wave; không spawn normal wave sau gate; reopen giữ partial/gate hợp lệ |
| E — Boss playable loop | Boss config/arena riêng, shared combat engine, timer60s, Hero death immediate lose, kết quả và reward/advance có checkpoint | Win cấp đúng reward một lần, unlock stage hợp lệ; Lose/exit giữ gate100% và không cấp reward thắng; input/transition không tạo trận lặp |
| F — progression/content vận hành | Nối unlock/nâng Công Pháp và các điều kiện progression theo config được duyệt; hoàn thiện nguồn tài nguyên liên quan | Người chơi đạt điều kiện bằng gameplay thật; không bị treo do nguồn Material chưa có; không tự tặng tài nguyên, giảm giá hoặc mở mọi skill để che gap |
| G — Bun/offline/Companion | Tách từng slice theo D19 và authority hiện hành; dùng chung combat/health/effect pipeline | Bun đúng Hero action và dừng Normal khi0; Boss độc lập Bun; offline dừng tại gate và giới hạn được duyệt; Companion death/respawn không tạo engine thứ hai |
| H — hoàn thiện bản chơi | Các màn chức năng cần thiết, hướng dẫn và nội dung đầu game; responsive portrait; build/checkpoint/recovery validation trên bản sao | Có một tuyến chơi tự hiểu được từ bắt đầu→chiến đấu→trang bị→nâng cấp→Boss→ải kế→thoát/mở lại; các trạng thái lỗi/thua/hết tài nguyên có đường trở lại rõ ràng |
| I — polish/phát hành | Art/audio/animation/performance/mobile build và release checks sau gameplay ổn định | Thiết bị mục tiêu và content được duyệt; không dùng compile/offline PASS thay visual/runtime acceptance |

Đây là thứ tự đề xuất để quản lý dependency, không phải cam kết đã hoàn thành hoặc tự duyệt từng feature. Không cần audit lại tám nhóm từ đầu: dùng audit đã review, cập nhật đúng những đường source/content thay đổi.

## 4. Lát cắt đang làm ngay

**F-CONGPHAP-SLOT-SELECT-01:** một file `Assets/_Game/UI/MindMethodUI.cs`, trên bố cục và text TMP hiện có. Chỉ chọn skill đã mở của Tâm Pháp đang active, qua manager hiện hữu. Không bật projectile/dash cho assets, không thêm chi phí/công thức, không chọn lại Tâm Pháp để hồi HP/revive.

Source review đã tìm lỗi pointer press/release trong bản chuẩn bị. Đã chặn cross-target/cross-link, compile/offline27PASS rồi áp dụng đúng một UI file; bản trước không apply và evidence giữ riêng. Physical/Unity input và skill bar chưa chạy.

**DELTA-VERIFY-20261013:** đã chạy adapter độc lập Unity EditMode R02/R03/R04 trong project/profile mới,33/33/cleanupPASS. Không đổi tên hoặc nghiệm thu C0/A1–A5 của lab cũ. R01 natural PlayMode và Công Pháp input vẫn cần evidence riêng; prompt F-TLTD-DELTA-NATURAL-INPUT-PREP-01 chuẩn bị source/context đã tách riêng, chưa dispatch/chưa chạy.

## 5. Các khoảng trống cần xử lý bằng dữ liệu/quyết định cụ thể

1. **Boss/stat/reward/content:** Boss và arena phải có config riêng; bảng HP/ATK/DEF, reward/rates, unlock/level conditions và production Stage dataset chưa đủ để tự chốt balance. Chuẩn bị bảng đề xuất có thể review trước khi hỏi Owner; không lấy boss lực chiến/giá/thưởng vừa thấy ở game khác làm luật TLTD.
2. **Unlock Công Pháp:** có API CanUnlockSkill/UnlockSkill nhưng chưa có production caller đã trace. Slice chọn skill không tự mở khóa. Cần chốt/nối UX unlock với điều kiện đã author, bảo vệ validation và kiểm chính sách tài nguyên nếu có; không suy ra mở miễn phí toàn bộ content.
3. **Đổi Tâm Pháp:** SetActiveMindMethod hiện có đường reset ReviveOnceAvailable và RecalculateStats(true), kể cả cùng ID. Cần scope guard/HP/revive policy trước khi thêm nút đổi; không mở thao tác có thể hồi tài nguyên ngoài ý định.
4. **Save/checkpoint:** không bắt buộc một SaveManager toàn cục chỉ vì chưa có class đó. Cần thiết kế identity/schema/restore/receipt tối thiểu đủ cho vòng chơi, bằng chứng cold-start và xử lý corrupted/future state trước production Stage reward.
5. **Material economy:** Tách chỉ trả Gold là LOCKED và đã sửa. Nguồn Material production phải được review riêng với requirement/cost thật; không sửa ngược contract recycle.
6. **Mobile lifecycle:** exit-Boss policy đã chốt; app background/focus loss, force close và checkpoint bền vững cần phân biệt khi tích hợp thực tế.

Những luật đã biết không hỏi lại: 50 quái hiện hành/configurable; Boss60s; giữ gate100% tới Win; regen1Bun/20s; Companion respawn25s/configurable; Gold-only; no Claim/queue/auto-chain.

## 6. Continuity và giới hạn

Tài liệu này phải đi cùng báo cáo kết quả source/evidence có ngày. Cập nhật trạng thái theo việc đã thực sự áp dụng/chạy; giữ rõ PREPARED/APPLIED/COMPILED/RUNTIME_VERIFIED/ACCEPTED.

Các raw/recovery limits, Gate2 P08 135/137, P09-A E2 timeout và PID25360 giữ nguyên. Không yêu cầu Owner thử lại hoặc ZIP phần đã xong để làm đẹp hồ sơ. Task mới chỉ test delta và dependency có rủi ro cụ thể.

Canonical publication của quyết định mới và tiến trình sẽ có receipt/commit riêng; trước khi remote readback thành công, không gọi chúng đã đồng bộ bền vững. Không stage/push source E cùng tài liệu continuity và không overwrite ba Markdown dirty của Owner.
