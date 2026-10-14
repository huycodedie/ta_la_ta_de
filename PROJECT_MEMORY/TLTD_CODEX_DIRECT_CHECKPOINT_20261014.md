# TLTD — checkpoint Codex trực tiếp triển khai, 14/10/2026

Đây là checkpoint công việc đang chạy, chưa phải nghiệm thu game hoàn chỉnh. Owner đã chọn Windows và Android; Codex trực tiếp code/test/build, không giao việc qua Antigravity. Quyết định workflow đã được lưu ngày13/10; không chạy packet Antigravity bị review từ chối.

## Source thực đã áp dụng tại E:\code\TLTD

- Công Pháp: mở theo điều kiện authored, kiểm lại ở manager, UI có link MỞ, chọn lại không phát event/save lặp. Hai file đã áp dụng và kiểm hash. Offline doubles42/42 PASS; đây chưa phải Unity input/visual evidence.
- BattleManager: Mặc/Tách thất bại giữ nguyên quyết định và popup; không hoàn tất hoặc chuyển trận. Giữ cửa sổ chặn duplicate trong lúc thực hiện.
- ProgressionManager: nạp ba key cấp/EXP/title trong Awake, Start sử dụng dữ liệu đã nạp; chặn ghi đè dữ liệu âm/nonfinite bị từ chối.
- Inventory/Equipment/Resource: thêm snapshot `TLTD_EquipmentState_v1`, lưu identity/affix/loadout cùng Gold/Material; legacy resource keys là mirror. Có readiness guard, trì hoãn save/observer tới khi thao tác hoàn tất, bảo toàn record corrupt/future/unresolved. Tách chỉ trả Gold. Tám file (sáu sửa, store/meta mới) đã áp dụng và kiểm hash.
- Prototype01.unity: sửa đúng9 serialized references tới HeroConfig/MonsterConfig/CombatConfig hiện có. Bản đảo9 dòng khớp nguyên bản. Builder đã gán các reference này nên không sửa builder thêm.

Receipts/backups thực nằm tại `work/direct_completion_20261013/applied_initial`, `progression_restore`, `applied_equipment_20261014`, và `build_preparation/scene_fix`. Không stage/commit/push source E; HEAD vẫn7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425 tại kiểm tra resume. Ba Markdown dirty của Owner giữ nguyên.

Kiểm tra resume786 fingerprints khớp baseline cộng bốn file đã áp dụng trước interruption: `work/direct_completion_20261013/resumption_integrity_20261014.json`. Sau đó mới áp dụng scene và equipment payload có receipt riêng. Không suy từ snapshot này thành bằng chứng mọi file/save trên máy.

## Kiểm chứng đang thực hiện

Standalone compile source hiện hành127 runtime: exit0, errors0, một warning có sẵn. Compiler receipts và response files được giữ cùng từng slice. Không gọi kết quả compiler này là Unity/runtime PASS.

Cold persistence attempt1 tại `C:/Users/conca/Documents/Codex/TLTDDirect20261013/cold01/write`: Unity PID396, exit1, không timeout. Bị chặn trước test entry bởi các Tests assembly của UGUI/InputSystem thiếu NUnit/Test Framework trong clone tối thiểu. Không có result PASS; giữ raw `unity.log` và `launch_receipt.json`. Đang bổ sung dependency trong clone và chuẩn bị output/context R1 riêng; không sửa source game để che lỗi fixture.

D01 natural gameplay: source/launcher đã review, có real Entity.Update, death/drop, UI Tách, duplicate cùng stack, modal drain và encounter advance. Lượt đầu đã launch Unity PID15120, start2026-10-14T11:50:15Z, profile Natural_DirectD01_fe5b9a881aa8. Sau interruption không còn tiến trình, receipt vẫn PENDING, không result và không ghi nhận exit: **INTERRUPTED_EXIT_NOT_OBSERVED**, chưa runtime PASS/FAIL. Giữ d01o01; chuẩn bị d01o02/context/profile mới để tiếp tục. Đây là UI action tổng hợp, không physical mouse/touch hoặc full-game acceptance. Bản preparation bị ngắt trước fingerprint và bản sửa frame cap giữ riêng originals/receipts.

D05: chuẩn bị22 checks dùng TMP geometry thật, EventSystem synthetic pointer và Hero/native prefs thật. Không dùng StandaloneInputModule với project new Input System. Chưa chạy Unity; clone tối thiểu cũng cần bổ sung test dependencies trước launch.

Build: `TLTDDirect20261013/p01` là clone toàn nội dung ngoài Editor,127 runtime cùng16 local packages, profile riêng TLTDDirectLab/Build_8573bbe1104b; bỏ legacy Editor autorunners/dev packages và thêm adapter build/probe chỉ trong clone. Windows đã được cho phép riêng và đang chạy Unity PID2952, start2026-10-14T12:01:46Z; chưa có build verdict/executable tại checkpoint này. Android context còn false, chưa APK. Development player probe chỉ chạy khi có đủ CLI context; chưa chạy player hoặc chụp ảnh.

Owner sau đó chọn “Dùng bộ đề xuất cho bản thử nội bộ” cho cấu hình Ải1→Boss1→Ải2. Quyết định cụ thể tại DECISION_FIRST_PLAYABLE_STAGE_BOSS_INTERNAL_20261014.md; thông số chỉ áp dụng cho internal playtest, chưa balance phát hành. Đang chuẩn bị Stage policy/config/checkpoint source ở workspace; chưa apply hoặc chạy Boss trong E. Các mục ngoài lát cắt này vẫn TBD theo authority.

## Tiếp tục từ đây

1. D01 chạy thực rồi đọc result/terminal/exit/cleanup; sửa đúng blocker mới nếu có, không nhận báo cáo làm PASS.
2. Cold R1 Write PASS mới lấy expected SHA và cho phép Read bằng tiến trình Unity mới; kiểm identity, affix, resources, cấp/EXP và cleanup.
3. Chạy D05 delta trong profile riêng sau review context.
4. Build Windows, chạy player tự nhiên khoảng15s với probe dormant được cho phép riêng, xem ảnh thật; build Android APK và báo rõ chưa thử thiết bị nếu không có thiết bị.
5. Lưu kết quả và continuity thực. Stage/Boss/Bun/offline/Companion/content/balance chưa hoàn tất; roadmap đề xuất và lab cũ không tự trở thành thiết kế production đã duyệt.

Giữ P08/P09-A/P09-B ACCEPTED/LOCKED theo scope, raw-log limits và recovery history. Không yêu cầu Owner thử lại hoặc đóng ZIP các phần đã xong. Không tự động điều khiển game mẫu nữa. Các test mới dùng profile/clone riêng; không dùng save sản xuất hay gọi recovery journals cũ.
