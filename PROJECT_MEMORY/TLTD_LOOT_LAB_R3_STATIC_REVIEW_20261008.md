# TLTD — Review STATIC-03 / R3, 08/10/2026

**Kết luận: STATIC REPAIR PARTIAL / REVISION REQUIRED / NO RUN AUTHORIZED.** Packet giao nhận đúng hash và có cải thiện source, nhưng chưa đạt điều kiện cấp phiên Unity. Unity compilation **NOT RUN**; C0/A1–A5 **NOT EXECUTED**. Các lỗi API dưới đây là đối chiếu tĩnh với source có thật, không phải compiler diagnostics từ một phiên mới.

Parent F-LOOT-CHANNEL-HANDOFF-LAB-01 giữ **LAB RUNTIME PATCH ACCEPTED WITH EVIDENCE LIMITS; OVERALL CLOSEOUT PENDING HARNESS / EVIDENCE CORRECTION**. Không dùng lỗi harness R3 để kết luận BM regression. Phase B **NOT EXECUTED / STOPPED**.

## Dữ liệu đã review và phần đạt

Codex đọc attachment STATIC-03 R3, đủ runner1329 dòng/wrapper548 dòng hiện hành, frozen API liên quan và packet `work/loot_lab_harness_output_r3_20261008_210000`. Packet17 files, manifest16/16 payloads khớp; sáu pre-R3 copies khớp R2 và sáu post-R3 copies khớp lab. Chỉ hai file được phép thay đổi trong nhóm source đã đo. Bản nhận được và source hiện tại đã được lưu riêng trong `work/loot_r3_received_review_20261008`; không sửa originals.

| File / trạng thái | Bytes | SHA256 |
| --- | ---: | --- |
| Runner R3 | 63492 | 93027F6B881793BC545FAB828A0891A1D86B168725A24177188C06EFA707884C |
| Wrapper R3 | 23905 | F4AAA5265931FBF556F09FC6778678C7AD921AF4BBA35FD303D0CA5550398BAD |
| Frozen BattleManager | 58723 | 5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6 |
| Frozen ProjectSettings | 24410 | A85CC67184A18039D0E1664D7C5625FF2A81BF550889CC1DCCCD0451AB8CC561 |
| Frozen runner meta | 242 | 6DF524CA04537093A4EB824E3C28972CCC21EDBF66DCF7BE969145C7E5B19943 |

Giữ các cải thiện: bỏ global ResetInstance, interactive CLI đầu vào refuse, include-inactive inventory, caller tạo ctx trước setup, observer đăng ký spawn, duplicate Tách ngay trước yield, A3 synchronous clear, A5 typed token delta; wrapper kiểm RunId/containment/trusted baseline trước dot-source, Backup nằm trong outer try/finally và conflict skip cả Restore/Compare. Đây là source improvements, chưa phải runtime proof.

Codex độc lập parse AST **đúng wrapper R3 hiện tại:0 syntax errors**. Hai patch độc lập `git apply --check` **2/2 exit0** trên exact pre-R3 copies trong disposable directory mới, copied baselines unchanged; không apply. Receipt: `work/loot_r3_wrapper_ast_receipt_20261008.json`, `work/loot_r3_independent_patchcheck_receipt_20261008.json`. Kiểm tra patch/AST không chứng minh C# compile hoặc gameplay PASS. Receipt dry-apply cũ thiếu command/baseline/stdout nên vẫn là EXECUTOR_REPORTED; kiểm chứng mới được ghi rõ Codex attribution.

## Findings cần sửa trong hai file hiện có

Line refs dưới đây thuộc đúng R3 hash ở trên. R=runner, W=wrapper; các path API thuộc `Assets/_Game/` trong lab.

| ID | Finding và tác động | Sửa hữu hạn cần giao lại |
| --- | --- | --- |
| R3-01 | R2–17 thiếu namespaces Data/Items/UI; R575–586 gán fields/enums không có hoặc private của SkillDefinitionSO; R595 gọi sai StartChannel và R594 Reset trước nó; R662 dùng completion delegate hai tham số; R508 gọi private BM.CancelLootLifecycle. | Dùng existing declarations: Data/SkillDefinitionSO.SetChannel160; Combat/SkillExecutionTypes.SkillExecutionRequest23; Combat/SkillCastState.StartChannel121; Core/EventBus.OnLootDecisionCompleted một EquipmentInstance; bỏ private call và setup Reset, không mở runtime API. Kiểm toàn bộ symbol mới trước giao. |
| R3-02 | A2 R798–820 tạo EquipmentInstance bằng CreateInstance dù nó là plain Serializable class; gán get-only properties, đưa vào ledger SO, gọi BM.EnqueueDropReward/ProcessPendingDropRewards không tồn tại. Fixture đã thay real death/drop bằng item giả. | Khôi phục A2 hai registered owned monsters và hai legit Health deaths → actual DropSystem/BM queue, dùng baseline pre-R3 làm tham chiếu. Cấp monsterCount2 riêng A2; không chỉ đổi sang public queue API để né luồng thật. **R3 hiện tại không truy cập Monsters[1]**; nhận định ban đầu về lỗi index được sửa tại đây. |
| R3-03 | RunGuardedCase272–344 không có guaranteed finally/Dispose cho abort/nested iterator, listener không nằm trong ledger; C0 coi mọi setup exception là expected. Cleanup479–568 clear OwnedSOs trước verify, bỏ sót wave config và listener/view/coroutine/session; PASS case đếm trước cleanup, cleanup fail vẫn chạy tiếp. | Retain allocation/subscription refs tới verification; fault C0 đúng loại/vị trí đã chủ động inject; tất cả exception/timeout/abort về cleanup hữu hạn. Stop owned BM trước destroy entity/config, không gọi private API. Cleanup failure làm case/suite fail và dừng cases sau. Final driver verification phải có trước suite terminal PASS. |
| R3-04 | A1 R698–700 đòi queue1 sau pending presentation đã dequeue; channelOk692 chỉ log. A3/A4/A5 chưa chứng minh cùng request/natural end và reached prerequisites; A5 activeDeferCancelled1270 không gate. | Đo hai thời điểm A1: during-channel finishing/queue1/no-open, rồi natural completion/pending ID/open/queue0. Dùng same request + phase/recovery/interrupt + deadline làm predicate. Bổ sung các predicates còn thiếu của **chính C0+A1–A5**, event/economy/death/count/ID/defer checks; không thêm test case hay sửa BM để chiều oracle. |
| R3-05 | R58–64 tự tạo fallback old run/token khi thiếu args; postreload có thể Exit/isPlaying=false trên unowned context. W fixed R2 product, fixed RunId/predictable token; không context-bind terminal receipts. | Explicit immutable fresh context, reject missing/stale/mismatch trước scene mutation; consume one-shot task state. Prepare future profile mapping trong code; **settings hiện tại frozen**, chưa allocate session. Unowned interactive/reload chỉ detach/log/refuse. |
| R3-06 | W thiếu child Assets/ProjectSettings/Tools reparse checks; missing historical journals silently skipped; revised runner/wrapper payload receipt thiếu; E status đọc nhưng bỏ qua, HEAD-only gate; chưa có historical artifact comparisons. | Kiểm exact targets/ancestors trước side effects; require đúng known journals, giữ checkpoint classification. External expected payload receipt tránh selfhash recursion; expected/actual pre/post rows; E HEAD/full porcelain/17selected + fixed history comparisons và tool exits phải gate. |
| R3-07 | Fresh journal chưa xác thực exact ownership/profile/context trước Restore; Backup failure vẫn có recovery branch không rõ ownership. Cleanup stages có thể abort nhau, logging failures bị swallow. PID-only kill/check, catch thiếu bounded terminal confirmation; timeout124/exception1 ghi thành actual OS exit. | Track attempted/result/journal ownership; safe isolated finally stages. Captured Process/start identity + owned tree/bounded terminal proof; unknown/conflict thì preserve và skip cả Restore/Compare. Actual OS exit hoặc UNAVAILABLE tách reason code. Required evidence write failure block acceptance. |
| R3-08 | W493–501 regex chỉ cần thấy PASS/teardown text; R260 suite result trước driver destruction; thiếu exactly-one terminal row/context/cleanup gates. | Structured terminal C0+A1–A5 sau assertions+cleanup, final driver/session teardown, exactly one suite receipt cùng context. Reject duplicate/stale/conflict/intermediate/missing; future wrapper gate đủ preflight/Backup/terminal/source-preservation/OS0/no-timeout/Restore/Compare. Không tăng suite. |

Bằng chứng API không được sửa để làm test hợp lệ. Read-only reflection BM chỉ dùng các member diagnostic đã cho phép, validate exact field/type; không SetValue/invoke private BM, không public API mới. Coroutine handle nonnull là diagnostic, không độc lập chứng minh scheduler đang sống. Nếu cần Contains trên IReadOnlyList, dùng explicit owned-membership loop hoặc namespace LINQ hợp lệ; không giả nó là List.

R400–405 chưa tạo owned transient EquipmentDropConfigSO như đã giao; mới chỉ load databases và đặt fallback rate. Đây không phải bằng chứng sửa authored config, nhưng claim owned config chưa đạt. Dùng existing EquipmentDropConfigSO.InitializeConfig và DropSystem.SetDropConfig, ledger trước attach/verify sau destroy. Include-inactive entry inventory còn phải xét existing CorrectiveHarness, không tạo driver mới khi có harness chưa sở hữu.

## Metadata/provenance cần chỉnh bằng addendum

STATIC_REPAIR_REPORT.md vẫn có **mojibake trong decoded UTF-8 thực tế**; hash đúng không chứng minh text đọc được. Có15 baseline byte counts sai dù18 hashes đúng:

| File | Reported → measured bytes |
| --- | --- |
| GameBootstrap.cs | 13858 → 1162 |
| BattleHUD.cs | 16929 → 13568 |
| ProgressionManager.cs | 16913 → 15966 |
| ResourceManager.cs | 9996 → 4729 |
| DropSystem.cs | 18337 → 9390 |
| Inventory.cs | 18171 → 3128 |
| EquipmentManager.cs | 13010 → 5999 |
| Monster.cs | 9235 → 6258 |
| Hero.cs | 13222 → 16275 |
| SkillExecutor.cs | 27672 → 33688 |
| ModalCoordinator.cs | 9219 → 18096 |
| LootDecisionUI.cs | 11872 → 23946 |
| tltd_save_guard.ps1 | 12058 → 35548 |
| P09_VerificationCore.ps1 | 13296 → 15668 |
| P09B_PreflightGuard.ps1 | 10757 → 5271 |

Không có delivered AST receipt/E before-after/history comparison artifacts trong17 files; prose không thay receipt. Codex current measurements đóng factual equality với attribution riêng, không tạo ngược executor historical proof. Giữ R2 prelaunch-only matrix, OS Core-vs-Enterprise correction, batch không có -nographics, parent-shell escaping correction và Codex independent patchcheck attribution như review trước. Giữ các nguyên nhân old PID6792/2684 **UNVERIFIED EXECUTOR EXPLANATION**, không phục dựng raw đã mất.

## Bảo toàn và continuity

Codex snapshot08/10 **21:21:57+07**: E HEAD7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425,3tracked+530untracked; full porcelain và17selected fingerprints khớp snapshot trước. Prior77inputs:75unchanged+chính2authorizedrunner/wrapper edits. Original corrective19+PhaseA8+R2packet27 preserved; received R1/R2 archives retained. New95inputs snapshot gồm prior77 hiện tại + R3packet17 + attachment1. Measured scope không chứng nhận mọi untracked byte/save. Receipt final sau publication sẽ xác minh lại scope này.

Current main trước docs publication: memory373738fc9e4391aa47cd732ef2649c1fe9e1d66f / game63342bf6efd59f8eede1776e9ee5e37cced27c1a, remote refs khớp, isolated clones clean. Docs-only publication theo quyền Owner05/10; final refs/readback ghi receipt riêng. Không source push hay E: edits.

Giữ R1attempts6792/OS1/NOT_REACHED,2684/OS1/FAIL,16808/49.2020002s/OS0/raw5PASS; old PhaseA12100/OS1/UNKNOWN và24764/OS1/FAIL. R2PID24052/52.5784775s/OS1/no-timeout/COMPILE_BLOCKED/SCENARIOS NOT_REACHED; parent-wrapperexit1/55.3395684s; absent lab profile Guard0valuesPASS/journalVERIFIED là bounded raw evidence, không Owner recovery upgrade.

P08/P09-A/P09-B ACCEPTED/LOCKED, S01–S05/M1–M7/GUI USER_VERIFIED và recycle closeouts giữ nguyên. P08Gate2 **135/137exit1/legacy07,09**; P09-AE2 **SCENARIO_PASS_EXIT_TIMEOUT / wrapperexit2**; PID25360 **USER_REPORTED_PASS / RESTORE_UNRESOLVED**. F-SAVE recovery accepted theo snapshot/metadata; historical Restore/execution **PARTIAL**, Compare/preflight **METADATA_ONLY**. Known Owner launcher VERIFIED24values chỉ metadata inspection; intentional BACKED_UP rollback checkpoint không phải unresolved active launcher. Không rerun recovery hay nâng raw status.

**Bước tiếp: F-LOOT-LAB-HARNESS-STATIC-03-R1**, bản sửa hữu hạn trong cùng static task; prompt riêng [PROMPT_ANTIGRAVITY_LOOT_LAB_STATIC03_R1_FIX_20261008.md](PROMPT_ANTIGRAVITY_LOOT_LAB_STATIC03_R1_FIX_20261008.md). Prompt prepared, chưa có bằng chứng dispatch/execution. Giữ BM/settings/meta/runtime/shared helpers frozen. Không Unity/wrapper top-level/Guard/preflight action/Registry/save/PlayMode cho tới source approval và prompt chạy riêng. Không Owner playtest/ZIP/broad retest, Stage/Boss implementation/exposure/persistence/schema, product LOCK mới hoặc skill rollout. Codex chỉ đọc source/evidence, parse AST và drycheck copies; không chạy Unity/wrapper/Guard/live Registry.
