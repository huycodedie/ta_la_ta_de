# TLTD — kết quả sửa trực tiếp R01–R04 tại E:\code\TLTD

**Cập nhật continuity ngày13/10:** đây là snapshot kết quả trước phiên Unity. Sau đó đã thêm MindMethodUI và chạy kiểm chứng hẹp Unity EditMode33/33 trên fixture/profile riêng; channel natural, DEV UI và input/visual vẫn chưa chạy. Trạng thái mới và raw receipts ở [TLTD_GAME_COMPLETION_PROGRESS_20261013.md](TLTD_GAME_COMPLETION_PROGRESS_20261013.md). Các ghi nhận NOT_RUN bên dưới được giữ đúng thời điểm lập báo cáo, không xóa lịch sử hoặc biến toàn bộ tám sửa thành gameplay-accepted.

Ngày **13/10/2026, UTC+07**. Authority: Owner yêu cầu trực tiếp sửa các lỗi đã báo cáo; Owner xác nhận chưa chạy prompt R4 mới nhất.

**ĐÃ ÁP DỤNG 8 FILE C# TẠI E: / STANDALONE C# COMPILE PASS / OFFLINE CHECKS PASS / UNITY–PLAY MODE CHƯA CHẠY.** Đây là kết quả sửa source, chưa phải nghiệm thu gameplay hoặc đóng hồ sơ lab.

## 1. Phạm vi và thay đổi thực tế

| Nhóm | Source đã sửa | Hành vi sau sửa theo source và kiểm tra cô lập |
|---|---|---|
| R01 — finishing channel → loot | [BattleManager.cs](E:/code/TLTD/Assets/_Game/Core/BattleManager.cs:1291) | Nhánh finishing giữ item trong queue tới presentation authority hiện có. Dùng TryPresentNextQueuedLoot để mở cờ quyết định, hoặc deferred wake khi còn modal active/queued. Captured encounter/loot token và cancellation hiện có được giữ. |
| R02 — ownership | [Inventory.cs](E:/code/TLTD/Assets/_Game/Inventory/Inventory.cs:90), [EquipmentManager.cs](E:/code/TLTD/Assets/_Game/Equipment/EquipmentManager.cs:63), [EquipmentUpgradeService.cs](E:/code/TLTD/Assets/_Game/Equipment/EquipmentUpgradeService.cs:127), [EquipmentDropDebugUI.cs](E:/code/TLTD/Assets/_Game/UI/EquipmentDropDebugUI.cs:269) | Thêm lookup đúng object đang sở hữu. Equip kiểm tra ownership và Remove thành công trước khi thay slot; từ chối item đã Tách hoặc object khác trùng ID. Upgrade chấp nhận item trong túi hoặc đúng slot đang mặc trước khi chi tài nguyên. DEV UI xóa lựa chọn stale và giữ action reference qua refresh đồng bộ. |
| R03 — Cấp Rơi auto-complete | [LootTierProgressionManager.cs](E:/code/TLTD/Assets/_Game/Progression/LootTierProgressionManager.cs:79), [LootTierProgressionUI.cs](E:/code/TLTD/Assets/_Game/UI/LootTierProgressionUI.cs:382) | Update và LoadState hòa giải timestamp qua cùng một completion path, lên đúng một cấp, lưu IDLE và đồng bộ DropSystem. CompleteUpgrade khi IDLE bị từ chối. UI bỏ Claim, khóa action trong lúc upgrading và từ chối click thuộc tier cũ để tránh khởi động thêm cấp ngoài ý muốn. |
| R04 — builder tạo effect dư | [Prototype01SceneBuilder.cs](E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs:2581) | GetOrCreateSkillDef trả lại skill hiện có nguyên trạng, giữ config/effect references và cảnh báo khi metadata lệch. Chỉ tạo skill/effect khi thiếu asset; path có asset khác loại bị từ chối. |

Các bản sửa được chuẩn bị và review trong workspace, compile trước, rồi kiểm hash và áp dụng nguyên byte vào E. Bản áp dụng đã được compile lại trực tiếp từ source E. Không lấy báo cáo R3/R4 hoặc PASS lịch sử làm bằng chứng cho lần sửa này.

**Tradeoff R04:** builder trở thành create-only đối với skill đã tồn tại. Sửa constants trong builder sẽ không tự ghi đè skill data đã author; cập nhật data có chủ đích cần một task riêng. Các effect subasset dư trong asset lịch sử vẫn còn; task này ngăn tạo tiếp, chưa dọn migration.

**Giới hạn R02:** một số fixture Editor cũ tạo item rồi gọi Equip/Upgrade mà chưa AddItem sẽ cần setup ownership hợp lệ khi dùng lại. Đã rà caller runtime: DropSystem thêm item vào Inventory trước drop event. Chưa chạy các fixture cũ, không báo chúng FAIL và không hủy acceptance lịch sử.

## 2. Kiểm chứng đã thực hiện

### Compiler standalone với DLL tham chiếu Unity

Chạy trực tiếp Roslyn C# compiler qua .NET SDK 9.0.200, sử dụng DLL và defines từ Bee response files của Unity **6000.6.0f1**. Đầu ra DLL/log nằm trong workspace Codex. Không launch Unity Editor hoặc chạy code game trong Unity.

| Assembly | Source inputs | Baseline trước sửa | Source E sau áp dụng |
|---|---:|---|---|
| Assembly-CSharp | 126 | exit 0; 0 errors; 1 warning | exit 0; 0 errors; 1 warning |
| Assembly-CSharp-Editor | 24 | exit 0; 0 errors; 30 warnings | exit 0; 0 errors; 30 warnings |

Tập diagnostic cảnh báo không đổi sau khi bỏ số dòng/cột để đối chiếu. Tổng **150 source inputs, 0 lỗi, 31 cảnh báo có sẵn**. Analyzer và additionalfile của pipeline Editor được bỏ khỏi lệnh standalone; kết quả này không thay thế Unity import/build/Play Mode.

Lần chuẩn bị đầu dùng Bee1900b0aE response file cũ thiếu Modal sources và bị 22 lỗi thiếu type. Đã giữ receipt/log thất bại đó, kiểm inventory hiện tại và dùng Bee1300b0aE đủ 126 runtime + 24 Editor. Đây là lỗi danh sách đầu vào của kiểm tra đầu, không sửa thêm code ngoài allowlist để che lỗi.

Receipts: [baseline](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/compile_baseline_current/receipt.json), [staged](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/compile_staged_current/receipt.json), [source E đã áp dụng](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/compile_applied_current/receipt.json).

### Kiểm tra hành vi offline

| Nhóm | Kết quả chạy | Phạm vi bằng chứng |
|---|---|---|
| R01 | **10/10**, exit 0; baseline negative control 3/10, exit 1 | Ba method thật trích nguyên từ BattleManager; scheduler/entity/modal/event doubles trong RAM. Bao phủ first decision, giữ queue khi blocker active/queued, wake/frame boundary mô phỏng, FIFO, duplicate presentation, stale token, Hero death và no-loot. Không chứng minh natural Unity frames hoặc real UI transaction. |
| R02 | **12/12**, exit 0 | Bốn class sửa thật và bảy domain file không sửa, Unity/resource dịch vụ giả. Chặn stale/same-ID clone, giữ gear cũ khi bị từ chối, valid Equip/Upgrade, equipped upgrade và DEV refresh đồng bộ. |
| R03 | **22/22 assertions**, exit 0 | Actual manager + UI với PlayerPrefs trong RAM. Deadline/load, panel-independent completion, idempotence/events, cost, zero duration, stale click, next explicit start và missing database. Không kiểm real save/cold-start trong Unity. |
| R04 | **8/8**, exit 0; baseline negative control 2/8, exit 1 | Exact builder method với AssetDatabase doubles. Repeated call, authored effect/config preservation, orphan preservation, missing skill và wrong-type path. Không chạy builder/import/serialization thật. |

Evidence: [R01 receipt](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/r01/behavior_receipt.json), [R02 output](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/r02/verification/behavior_output.txt), [R03 output](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/r03/offline_tests/result.log), [R04 receipt](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/r04/verification/behavior_receipt.json).

Review chéo read-only R01/R03 và R02 không tìm thấy blocker mới cụ thể trong delta. Review/compile/offline PASS không được đổi nhãn thành Unity gameplay hoặc visual PASS.

## 3. Bảo toàn working tree

| Hạng mục | Kết quả trước–sau |
|---|---|
| HEAD | Giữ **7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425** |
| File đo fingerprint | 786 trước / 786 sau; **8 source đổi đúng allowlist, 778 file còn lại không đổi** |
| Ba Markdown dirty có sẵn | MANUAL_P09A_CHECKLIST.md, PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md, REVIEW_RESPONSE.md giữ nguyên hash |
| Git status | 3 tracked modified → 11 tracked modified; cùng 530 untracked entries |
| Git index / HEAD file | Hash không đổi; không stage/commit/push/reset/stash/clean |
| Assets/metas/scenes/settings/packages trong tập đo | Không đổi ngoài 8 source C# đã liệt kê |
| Diff whitespace | Source Assets/_Game **exit 0**; whole-tree **exit 2** vì trailing whitespace đã có trong hai Markdown dirty, giữ nguyên |

Scope fingerprint: Assets/_Game, Assets/Scenes, ProjectSettings, Packages, ba Markdown dirty, .git/index và .git/HEAD. Không suy rộng thành hash của toàn bộ untracked/Library hoặc xác nhận save thật.

Backup nguyên byte trước sửa: [applied_backups](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/applied_backups). [Apply receipt](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/apply_receipt.json) chứa 8 before/post SHA-256. [Preservation receipt](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/preservation_receipt.json) xác nhận tập đổi và hash bản compile/staged khớp E. [Patch](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/TLTD_DIRECT_FIXES_R01_R04_20261013.patch), SHA-256 **D1925A0D52B69D6E4E47A80E57B6FB9CE1B24482CE1BC7D7E3651E25FBDADB21**. Không ZIP.

Quyền filesystem đã được cấp cho các source liên quan. Lần apply trong sandbox bị từ chối ở file đầu, applied count 0; receipt thất bại được giữ. Sau khi verify backups/hash, lệnh nâng quyền đã áp dụng đủ 8 file. Không đổi ACL/ownership để né lỗi.

## 4. Ngoại lệ công cụ phải ghi rõ

Khi chuẩn bị test R03, lần chạy SDK đầu thông báo **Installed an ASP.NET Core HTTPS development certificate**, rồi fail do NuGet fallback folder cấu hình không tồn tại. Không chủ động gọi lệnh certificate; chưa kiểm tra chứng chỉ thực tế, nên không tuyên bố host hoàn toàn không có side effect. Lần tiếp theo tắt certificate generation, dùng cấu hình nguồn/fallback trống và đã chạy xong test; các compiler/check khác dùng direct csc.

Chi tiết và trích xuất output: [R03 notes](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/r03/NOTES_R03.md), [SDK output excerpt](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/direct_fixes_20261013/r03/offline_tests/sdk_first_run_relevant_output.txt). Không sửa certificate/OS security để xử lý phát sinh này. Không gọi SaveGuard, live game PlayerPrefs, Registry commands hoặc save game thật; các bài test thay PlayerPrefs hoàn toàn trong RAM. Giới hạn này khác với thông báo khởi tạo host của SDK.

## 5. Continuity và bước tiếp theo

- **Owner authorization mới** thay boundary read-only cho đúng task sửa R01–R04 tại E. Không mở Stage/Boss, Bun, persistence framework, skill rollout hoặc status R05 conditional backlog.
- **Prompt F-LOOT-LAB-HARNESS-STATIC-03-R4 chưa chạy.** Lab runner/wrapper/runtime/meta/settings không đổi trong task này. Direct source fixes không đồng nghĩa R4 resolved, C0/A1–A5 PASS hoặc overall lab closeout accepted.
- **Stage Gate Phase B vẫn STOPPED.** P08/P09-A/P09-B giữ ACCEPTED/LOCKED trong phạm vi và lịch sử evidence cũ. Delta 8 file này có verification riêng, chưa gameplay-accepted; không thay Gate2 135/137, P09-A E2 timeout, PID25360 hoặc recovery metadata/raw limits.
- **Còn thiếu kiểm chứng Unity hẹp cho delta:** real channel→loot→UI Tách/Equip→modal drain→one advance; stale DEV selection bị chặn nhưng đồ hợp lệ dùng được; Cấp Rơi hết hạn khi panel đóng và reopen; builder chạy lặp trong bản sao, không tăng subassets/đổi references. Chưa yêu cầu Owner chơi lại toàn P08/P09 hoặc ZIP cũ.
- Bước kỹ thuật tiếp theo là chuẩn bị/review phiên kiểm chứng delta trong project/profile cô lập, với harness đủ readiness và chỉ đạo chạy riêng. Không dùng nguyên harness lab hiện bị review-required hoặc thử trên save thật. Test nào không tới được phải ghi NOT_REACHED, không suy từ offline PASS.
- Continuity trong báo cáo này được lưu **LOCAL ONLY / CANONICAL SYNC PENDING**. Không có commit/push hoặc remote-main verification trong task sửa trực tiếp này. Giữ ba tài liệu dirty tại E; không overwrite chúng để đồng bộ.

Kết quả giao: source đã sửa trực tiếp và compile/offline kiểm tra được, backup và patch đã lưu; Unity/runtime/visual acceptance còn giới hạn nêu trên.
