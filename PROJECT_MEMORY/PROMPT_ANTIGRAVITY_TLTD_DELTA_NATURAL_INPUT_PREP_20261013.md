# PROMPT — F-TLTD-DELTA-NATURAL-INPUT-PREP-01

Vai trò: Antigravity thực thi source kiểm chứng; Codex Tech Lead review trước phiên chạy. Đây là file prompt riêng, không phải tài liệu giải thích hoặc acceptance.

## Mục tiêu và boundary

Chuẩn bị adapter mới để kiểm chứng hai phần còn thiếu của delta tại E:\code\TLTD: **D01 channel→loot→UI→modal drain→one advance bằng natural PlayMode**, và **D05 Công Pháp chọn slot/skill qua TMP/EventSystem thật**. Task này chỉ chuẩn bị source/fixture/context trong workspace mới, **không launch Unity hoặc chạy wrapper trong task này**. Sau source review, Codex sẽ cấp chỉ đạo chạy cụ thể; không dùng pending prompt R4 cũ làm run permission.

Đọc main canonical PROJECT_MEMORY hiện hành và báo SHA thật. Đọc TLTD_GAME_COMPLETION_PROGRESS_20261013.md, TLTD_DIRECT_BUGFIX_RESULT_20261013.md, TLTD_COMPLETION_ROADMAP_20261013.md, DECISION_BOSS_EXIT_LOSE_RETRY_GATE_20261013.md. Nếu chưa được đồng bộ local E, đọc bản canonical/đầu ra workspace; không overwrite ACTIVE_WORK_HANDOFF.md dirty của Owner.

## Nguồn và phạm vi sửa

- E chỉ đọc: HEAD expected 7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425; tám sửa R01–R04 và MindMethodUI đã áp dụng, chưa commit. MindMethodUI expected 22,210 bytes, SHA D1702857311ED4089B481E67F0CF319B544C09898771A779D31622AC6D0E3FE6. Đối chiếu actual file/hash/porcelain trước khi dùng; lệch thì báo, không reset/clean/stash/pull E.
- Chỉ tạo/sửa adapter, launcher/context và evidence tools của task mới dưới work/complete_game_next_20261013/natural_input_preparation/. Không sửa runtime/Editor/data/scene/meta/settings/helper tại E hoặc lab stage_gate cũ. Không sửa recovery journals/raw lịch sử.
- Không tự mở skill assets, thêm feature, balance/cost, đổi Tâm Pháp, sửa save schema production, mở Stage/Boss/Bun/offline/Companion. Boss-exit policy đã duyệt nhưng implementation nằm ngoài task này.
- Review source/API thực tế: các báo cáo R3 cũ có signatures bịa/sai. Không sao chép bảng declaration mà không đối chiếu file hiện hành. Chỉ tái sử dụng các block đã xác minh; không dùng entry/cleanup/parser của runner R3 đang revision-required.

## D01 — natural gameplay delta

1. Chuẩn bị fixture owned thật Hero/Monster/BM/Drop/Inventory/Progression/Modal/LootUI cùng transient config. Roster hợp lệ, guaranteed drop qua config; kill bằng Health thật để DropSystem thêm exact item và BM queue. Không fake queue/drop event/cast completion.
2. Retain SkillExecutionRequest và item identity; channel phải StartChannel thành công với API thật. Trong channel: queue còn item, pending chưa có và decision chưa mở. Đợi natural bounded finish bằng engine frames; không gọi trực tiếp CompleteChannel hoặc đổi cast state để làm PASS.
3. Sau finish: đúng item được presentation, decision open và inventory ownership hợp lệ. Quyết định chính đi qua LootDecisionUI.OnDismantleClicked hoặc OnEquipClicked thật. Tách chỉ Gold, không Material/EXP thêm/advance sớm. Sau UI đóng decision, kiểm immediate duplicate BM call trong cùng stack trước yield bị từ chối và không effect.
4. Đợi loot/modal drain thật, assert EncounterIndex tăng đúng một lần. Nếu thêm case có blocker hoặc Hero death để phủ nhánh delta, nêu rõ risk và oracle; không mở rộng thành replay toàn P08/P09. Không gọi ResetInstance/EventBus global reset hoặc private CancelLootLifecycle để che leak.

## D05 — TMP/input delta

1. Dùng final MindMethodUI và manager/data source thật. Tạo transient database/config với các skill cùng slot đã được mở trong fixture; không author asset production hoặc bật toàn bộ flags. Đăng ký tất cả key fixture có thể ghi trước lifecycle; profile mới riêng TLTDDeltaLab/product unique.
2. Tạo Canvas/TMP/EventSystem owned; dùng font được copy nguyên bản. ForceMeshUpdate để lấy geometry thật, dispatch qua ExecuteEvents/ancestor đúng hành vi InputSystem. Ghi rõ synthetic input khác với physical/manual visual check.
3. Kiểm slot→alternatives, chọn unlocked đúng slot, event/save đúng một lần, reselect no-op, locked/sai slot/foreign method không chọn; không reset HP/Rage/revive/cooldown. Kiểm panel hidden/wrong tab/disabled/right pointer/unrelated text bị chặn.
4. Bắt buộc negative press/release: decorative→alternative; slot→alternative; link A→link B cùng TMP; non-link→link. Chỉ press/release cùng TMP/cùng link mới chọn. Không mô phỏng FindIntersectingLink bằng doubles trong Unity evidence.
5. Một điểm manual delta sau chạy được chuẩn bị rõ: scene/profile test, vị trí Công Pháp, thao tác và kỳ vọng; không xin Owner chạy lại P08/P09 hoặc dùng tài khoản game tham chiếu.

## Launcher, save và evidence

Clone đầy đủ/đại diện cần được fingerprint; không gọi source-fixture ít assets là full game. Audit builder InitializeOnLoadMethod trước first launch: giữ scene gốc trong clone để không auto-author lúc import, không adopt ambient loaded scene. Omit legacy Editor auto runners. Clone company/product phải khác DefaultCompany/TLTD trước launch; entry kiểm batch/dataPath/company/product/runId/token/context hash và owned source input receipt trước mọi fixture/prefs access. Kiểm file-save paths nếu có; không chạm save thật/live Registry/SaveGuard production.

Cấp context riêng duy nhất cho phiên sau domain reload; PlayMode entry/exit phải owned, không interactive Exit hoặc auto-run. Ledger allocation/subscription đăng ký ngay; BM hủy trước entities/config, dispose nested coroutines khi abort, verify retained refs và driver sau frame. Failure/cleanup/evidence write failure phải chặn PASS. Known prefs phải baseline absent trong profile fresh, chỉ xóa exact owned keys; không DeleteAll hoặc giả định clone path tự cô lập prefs.

Launcher sẽ capture Process object/PID/StartTime và timeout; không kill ambient Unity hoặc PID-only. Parse đúng một terminal row per case/driver/suite gắn context nonce; fail/not-reached/surplus không được bỏ qua. Giữ actual OS exit khác scenario outcome; không dùng -quit để cắt natural PlayMode. Trả nguyên raw log trong session sau, không ZIP.

## Đầu ra và điểm dừng

SOURCE_PREPARED_NOT_RUN: source adapter/wrapper, exact API table, allowlist, pre/post fingerprints, context template runAuthorized=false, preservation receipt E, static compiler/AST receipts, readable report và manifest SHA256. Static compiler direct csc dùng refs thật; không SDK restore/init hoặc launch Unity. Mọi chưa thực hiện ghi NOT_RUN/NOT_REACHED.

**STOP FOR CODEX SOURCE REVIEW.** Task chuẩn bị không được tự chạy, không công bố Unity PASS, không đóng lab R4 cũ hoặc Phase B. Không có approval mới nào cho code production từ prompt này.
