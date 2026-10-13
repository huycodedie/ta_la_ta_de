# TLTD — tiến độ triển khai tiếp theo

Ngày 13/10/2026, UTC+07. Yêu cầu hiện hành: tiếp tục phát triển game; áp dụng đúng những scope đã phân tích, giữ working tree/save và báo bằng chứng thật.

**F-CONGPHAP-SLOT-SELECT-01 = APPLIED / STANDALONE COMPILE PASS / OFFLINE 27/27 PASS; UNITY INPUT & VISUAL NOT RUN.** Chưa gọi game hoàn chỉnh hoặc milestone gameplay ACCEPTED.

## 1. Source đã áp dụng

Chỉ sửa thêm `E:\code\TLTD\Assets\_Game\UI\MindMethodUI.cs` trong bước này. Dùng TMP links trên năm slot và danh sách thay thế hiện có, giữ hierarchy/scene/data/metas. Chọn skill đã mở của Tâm Pháp active đi qua `MindMethodManager.SelectSkillForSlot`, giữ API validation/event/save hiện hành.

Input chỉ nhận panel đang active, component enabled, tab Công Pháp khi navigation tồn tại, left/touch và exact TMP được bind. Press và release phải ở cùng TMP, cùng link; nhấn nơi khác hoặc trượt sang skill khác không chọn skill tại điểm nhả. Locked/current skill không có selection link, cùng ID không save/event lại. UI không gọi SetActiveMindMethod, không reset HP/Rage/revive/cooldown và không mở skill assets.

Đã bỏ câu “BẤM ĐỔI” sai chức năng ở danh sách Tâm Pháp khác; hiển thị “ĐÃ MỞ”. `OnDestroy` gỡ EventBus listeners của UI. Bố cục/chạm/skill bar bằng Unity thật chưa được nghiệm thu.

Review độc lập phát hiện P2 press/release trong bản chuẩn bị; bản đó chưa được áp dụng. Bản sửa cuối được root review source/API, compile trước apply và compile lại từ E. Bản sửa input hiện tại: **22.210 bytes / SHA256 D1702857311ED4089B481E67F0CF319B544C09898771A779D31622AC6D0E3FE6**. Bản gốc 18.416 bytes / SHA256 CD709B39972B6AF925ABF8DD12E93B0371FC9FC29C1F775CF5DF47F90AD56738, backup nguyên byte giữ trong `work/complete_game_next_20261013/applied_backups/MindMethodUI.cs`.

## 2. Kiểm chứng source và preservation

- Direct Roslyn csc dùng DLL/defines Bee Unity6000.6.0f1: 126 runtime + 24 Editor, **0 errors / 31 warnings có sẵn**, trước và sau apply đều exit0. Không dùng SDK restore/build/init.
- Offline dùng actual UI/manager/data và Unity/TMP/PlayerPrefs doubles: **27/27**, exit0. Root đã chạy lại executable và giữ log `verification/root_repeat_behavior.log`. Không gọi đây là raycast/input/save thật.
- Snapshot trước/sau bước UI: **786 fingerprint, 1 file đổi đúng scope, 785 không đổi**; HEAD vẫn7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425. Status chỉ thêm một tracked modified UI; tổng12 tracked modified (9 C# + 3 Markdown cũ), giữ530 untracked. Index/HEAD và ba Markdown dirty không đổi hash.
- Tám sửa R01–R04 trước đó vẫn nguyên trạng. Chi tiết/giới hạn và SDK-first-use exception giữ trong `TLTD_DIRECT_BUGFIX_RESULT_20261013.md`; task mới không xóa hoặc nâng nhãn evidence cũ.
- Không stage/commit/push source E, không sửa scene/settings/packages/meta production, không thao tác tài khoản game tham chiếu hoặc SaveGuard/live Registry production.

Receipts local: `work/complete_game_next_20261013/skill_selection_apply_receipt.json`, `preservation_receipt.json`, `compile_staged_current/receipt.json`, `compile_applied_current/receipt.json`; source/patch/notes/offline logs trong `skill_selection/`. Đây là footprint đã đo, không chứng nhận toàn bộ Library/untracked hoặc save thật.

## 3. Unity delta riêng

Task mới **DELTA-VERIFY-20261013** dùng source fixture/profile mới; không dùng runner/wrapper R3/R4 cũ. Frozen lab cũ và Phase B vẫn giữ status cũ. Adapter là synchronous EditMode với các method/component thật, dispatch Awake tường minh; không natural Update/PlayMode.

**ĐÃ CHẠY UNITY THẬT — 33/33 EDITMODE CHECKS PASS / OWNED CLEANUP PASS.** Chỉ có ba case D02 ownership14, D03 expired timestamp LoadState11 và D04 builder reuse8; D01 channel/loot natural và Công Pháp input/visual không nằm trong phiên này. Direct-method EditMode không chứng minh natural Update, cold restart, DEV UI, raycast/chạm, combat hoặc một build game hoàn chỉnh.

| Phiên | Actual process / elapsed / OS | Kết quả và giới hạn |
|---|---|---|
| Lần1 — Delta20261013_70bda4f56297 | PID23592 / 133,31s / exit0 / no timeout | Ba case33/33 và cleanup PASS. Có DirectoryNotFoundException/asset import failure trước adapter tại path265chars; file thực tế tồn tại. Giữ nguyên log và result; không gọi toàn bộ import sạch. |
| Lần2 — Delta20261013_a40ff2f9611b | PID13204 / 84,11s / exit0 / no timeout | Profile/nonce/context mới, fixture ngắn tại C:\Users\conca\Documents\Codex\TLTDDelta20261013\p01. Ba case33/33 và cleanup PASS, đúng một terminal suite; log không còn DirectoryNotFoundException/asset import failure/compiler CS error đã gate. Licensing/access-token và cloud Curl7 diagnostics vẫn giữ trong raw, không tuyên bố mọi dịch vụ Editor đều thành công. |

Lần2 giữ nguyên runtime/builder, adapter chỉ đổi AllowedRoot sang workspace ngắn; root so sánh exact text ngoài constant và standalone compile runtime126 + adapter/builder đạt0errors. Không sửa OS long-path setting hoặc source production để xử lý fixture. Việc lỗi path biến mất khi đổi đường dẫn củng cố nguyên nhân compatibility path; chưa kiểm tra OS policy để gọi đó là nguyên nhân cấu hình đã chứng minh.

Profile lần2 TLTDDeltaLab/Delta_Delta20261013_a40ff2f9611b; context SHA2541AD428E8BF0C188DF8C47E96E4745103E0A8F1889FFA635F3B7834782E890, input receipt SHA7FEA7BE10127FD847DB91C3225DA0F01542F0E471FAFABCFEF3084A28E2649E4, adapter SHAFFD6814310D048EF64DBA7E87022BFA08EA7925AD6D784666555E31BA957B5BB. D02/D03 dùng PlayerPrefs thật trong profile này, baseline7key absent trước fixture; cleanup xóa exact7key và kiểm HasKey false. Không DeleteAll, không Registry recovery/compare; profile container hoặc Unity settings có thể còn. Không suy thành save production được test/restored.

Raw/result/launch/input/context receipts lần2: `C:\Users\conca\Documents\Codex\TLTDDelta20261013\o01\`. Lần1: `work/complete_game_next_20261013/unity_output_Delta20261013_70bda4f56297/`; review `unity_attempt_01_review.md`. Không overwrite hoặc ZIP lịch sử. E fingerprint đo lại sau Unity giữ786/786 và status/HEAD nguyên như sau UI; source whitespace check exit0.

Fixture `work/complete_game_next_20261013/unity_fixture_Delta20261013_70bda4f56297`: 126 runtime C#, exact builder, scene file để ngăn AutoBuildIfMissing, TMP assets và local package graph; thiếu phần lớn production assets/data/Editor khác. **MINIMAL_SOURCE_FIXTURE, không full project/game build.** Profile riêng TLTDDeltaLab/Delta_Delta20261013_70bda4f56297; adapter kiểm exact context/dataPath/company/product trước prefs/fixture. 16.959 input fingerprint được chốt trước launch.

Một lần chuẩn bị hash quá chậm đã dừng đúng process owned PID3516, chưa launch Unity/chưa tạo result; script/receipt attempt được giữ. Sau tối ưu cache kiểm tra ancestors, preparation hoàn tất. Không dùng lần dừng này làm bằng chứng cleanup của Unity.

## 4. Quyết định mới và bước tiếp theo

Owner đã phê duyệt **thoát khi đánh Boss → tính thua, giữ cổng100% để thử lại**. Chi tiết/superseded scope ở `DECISION_BOSS_EXIT_LOSE_RETRY_GATE_20261013.md`; background/focus loss chưa tự coi là exit. Chưa triển khai Stage/Boss/checkpoint từ quyết định này.

Lộ trình đề xuất có thứ tự và tiêu chí chuyển chặng trong `TLTD_COMPLETION_ROADMAP_20261013.md`, không biến roadmap thành design LOCKED.

Bước kiểm chứng cụ thể còn thiếu: D01 real channel→loot→UI→drain→one advance và D05 TMP/EventSystem. Prompt riêng **PROMPT_ANTIGRAVITY_TLTD_DELTA_NATURAL_INPUT_PREP_20261013.md** chỉ giao chuẩn bị source/context để review, chưa cho phép launch. Prompt mới chưa dispatch/chưa thực thi; prompt R4 cũ cũng chưa chạy.

Khoảng trống tiếp theo sau evidence: unlock Công Pháp chưa có production caller đã trace; fresh save có thể vẫn chỉ dùng default skill. Chọn skill đã mở không giải quyết toàn bộ unlock progression. Đổi Tâm Pháp còn nguy cơ reset HP/revive; save/load item/loadout, Stage/Boss content/balance, nguồn Material và mobile lifecycle cần slice riêng. Không mở tất cả skill hoặc tự điền các TBD để che khoảng trống.

P08/P09-A/P09-B giữ ACCEPTED/LOCKED; recovery/raw history không nâng nhãn. Không yêu cầu Owner thử lại/ZIP phần cũ. Chỉ đề nghị manual delta khi đã có bản riêng và thao tác/kết quả cụ thể đủ để quan sát.

Continuity: sẽ được publish docs-only ở isolated memory/game checkouts và xác minh remote. Trước receipt publication thành công không gọi source E hoặc quyết định đã lên Git.
