# TLTD — Review R2: compile blocker và điều kiện trước phiên lab tiếp theo

Ngày 08/10/2026, UTC+07. Codex Tech Lead/Architect. Prompt thực thi được tách riêng: [PROMPT_ANTIGRAVITY_LOOT_LAB_R3_STATIC_REPAIR_20261008.md](PROMPT_ANTIGRAVITY_LOOT_LAB_R3_STATIC_REPAIR_20261008.md).

## Quyết định hiện hành

**F-LOOT-LAB-HARNESS-EVIDENCE-02 (R2) = COMPILE_BLOCKED / SCENARIOS NOT_REACHED; STOP-RULE FOLLOWED; HARNESS REUSE NOT ACCEPTED.** Đã xác minh một launch R2, lỗi compile, exit nonzero và raw Restore/Compare của profile lab. Chưa nghiệm thu C0/A1–A5, cleanup fixture hoặc toàn bộ nâng cấp safety được báo cáo.

**Parent F-LOOT-CHANNEL-HANDOFF-LAB-01 giữ LAB RUNTIME PATCH ACCEPTED WITH EVIDENCE LIMITS; OVERALL CLOSEOUT PENDING HARNESS / EVIDENCE CORRECTION.** Final raw R1 năm PASS/OS0 vẫn có giá trị trong phạm vi đã review. R2 compile fail không chứng minh BM regression; cũng không bổ sung gameplay proof. BattleManager tiếp tục frozen **58723 bytes / SHA256 5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6**.

**Bước tiếp: F-LOOT-LAB-HARNESS-STATIC-03 — AUTHORIZED LAB SOURCE PREPARATION ONLY / PROMPT PREPARED, NOT DISPATCHED / NOT EXECUTED.** Sửa source cụ thể trong runner/wrapper và trả diff cho Codex review trước. **Chưa cho launch thêm Unity/wrapper/Guard, chưa đổi profile hay chạy C0/A1–A5.** Quyết định này dựa trên những lỗi safety/assertion còn thấy trong source, không chỉ hai lỗi compile. Một phiên mới chỉ được giao bằng prompt riêng sau khi source đủ điều kiện.

**Phase B NOT EXECUTED / STOPPED.** Production source transfer/publication, Stage/Boss implementation/exposure/persistence/schema vẫn NOT APPROVED. Không yêu cầu Owner chơi thử, retest phần đã nghiệm thu, recovery rerun hoặc ZIP.

## Inputs và những gì thực sự đã kiểm tra

Đã đọc attachment `C:\Users\conca\.codex\attachments\7a47cb6f-1d14-4790-b5ca-6ee94f55ed0a\Văn bản đã dán.txt`, bốn báo cáo, manifest, source copies, patches, raw Unity/wrapper/Guard, parent-shell receipt và journal trong `work/loot_lab_harness_output_r2_20261008_201500`. Review actual runner/wrapper và các runtime API liên quan, đối chiếu prompt R2, previous archive và fingerprints.

Canonical main hiện hành trước publication: memory **8c4b5dd78ddae376ce6835e0cc63e2f6619cbf28**, game mirror **1b797739966b7a18970b69c5237a21b969d6d5cc**; hai clone sạch và remote main khớp. E: snapshot **08/10 20:26:38+07**, HEAD **7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425**, **3 tracked modifications + 530 untracked entries**; toàn bộ porcelain inventory và **17/17 file đã chọn** khớp snapshot trước. Không chứng nhận byte của mọi untracked file hay lịch sử save từ phép đo này.

So với 49 đầu vào lần review trước: **46 giữ nguyên**, ba thay đổi đúng nhóm đã được authorize là runner, wrapper và identity settings. BM, các runtime/helper/meta đã đo, original19 corrective artifacts, old8 Phase A artifacts và old journal giữ nguyên. Sáu pre-R2 source copies khớp archive đã nhận trước; sáu R2 copies khớp actual lab. Archive mới tại `work/loot_r2_received_review_20261008` giữ packet27 file, 21 lab source file và một old lab journal; snapshot77 gồm 49 đầu vào hiện tại +27 file R2+attachment. Đây là phép đo khi nhận/review, không giả thành executor đã cung cấp historical before/after proof.

Codex **không chạy Unity, wrapper, test, SaveGuard hoặc truy cập live Registry/save**. Codex chỉ chạy `git apply --check` trên bốn nhóm file copy riêng, không apply patch vào lab/E:. Receipt tại `work/loot_r2_patch_readcheck_receipt_20261008.json`.

## Runtime evidence R2

| Trường | Raw outcome |
|---|---|
| Run/profile | `20261008_201500` / `TLTDLab/StageGateFixR2_20261008_201500` |
| Unity PID | **24052**, một launch trong packet |
| Thời gian raw, UTC+07 | **20:11:30→20:12:23**, **52.5784775s** |
| Process | **OS exit1**, **TimedOut=False** |
| Scenario | **NOT_REACHED**, không có raw CLI/suite/C0/A1–A5 execution markers |
| Wrapper | logged exit1; parent-shell receipt **exit1 / 55.3395684s**, 20:11:28→20:12:24 |
| Guard | CheckInterruptedJournal, Backup, Restore, Compare: cả bốn child exits0 |
| Lab recovery | OriginallyExisted=false, ValueCount0; Restore scope-deleted/VERIFIED; Compare0valuesPASS |

Unity raw lines920/922, lặp979/980 và1012/1013: **CS0619 Object.GetInstanceID obsolete, dùng GetEntityId** tại runner249; **CS0117 LootTierProgressionUI không có ResetInstance** tại393. Raw1037–1038 xác nhận compiler errors và return code1. Hai diagnostics lặp không phải nhiều launch. Không timeout hoặc bonus run được thấy trong packet; việc dừng và bàn giao sau compile fail đúng stop-rule.

Normal exit đi qua `WaitForExit`; raw global Unity check trước Restore không thấy Unity đang chạy. Đây là bounded normal-exit observation, không chứng minh toàn bộ child-process tree/all-exception/watchdog cleanup. `cleanupVerified=True` của wrapper nói về process inventory, không phải fixture cleanup chưa hề chạy. Command thực có `-batchmode`, không có `-nographics`; không gọi đây là graphics-disabled proof.

Raw lab Guard proof chỉ cho profile cô lập vốn không tồn tại. Historical Owner Restore/execution PARTIAL và Compare/preflight METADATA_ONLY không được nâng cấp. Root đọc thêm **metadata-only** launcher journal đã biết `E:\code\TLTD\scratch\.save_backup_manual_session_p09b\journal.json`: VERIFIED/24values, bytes12776/hash5ADAA5347E64AF213D4E46278C32969CC5B46E333C9ECE5AA2B6A0C6A658ACB2; không đọc/xuất raw values hoặc restore. Rollback checkpoint chủ ý BACKED_UP không phải active launcher journal cần restore.

## Source blockers trước khi cho chạy lại

Line numbers dưới đây là actual R2 runner57050 bytes và wrapper19700 bytes, không phải dòng patch.

| Vùng | Finding cụ thể | Sửa trong allowlist |
|---|---|---|
| Compile/cleanup runner249,387–393 | GetInstanceID compile error; bảy global ResetInstance calls vi phạm prompt; không chỉ riêng Tier API thiếu | Dùng supported identity API; bỏ cả block global resets. Dispose đúng owned components/root; existing OnDestroy/OnDisable tự clear instance/unregister khi đúng object. |
| Entry45–74,145–151,171 | Interactive refusal vẫn EditorApplication.Exit; suite còn public và thiếu exact path/launch context; fixed token chưa consume/clear đầy đủ | Unowned/interactive chỉ refuse/log/detach, không đóng Editor. Chỉ authorized batch context mới exit nonzero. Một controlled CLI, nonpublic suite/helpers, context/token revalidate qua reload và terminal. |
| Inventory trước NewScene105–113 | FindAnyObjectByType mặc định bỏ inactive; thiếu generic Entity và view/harness liên quan | Read-only include-inactive inventory trước scene mutation, không lazy singleton getters hoặc adopt unowned objects. |
| Setup/exception235–333,520–526 và các case tương tự | Context được cấp bên trong builder, throw làm caller mất ownership; A1–A5/suite không finally/guarded MoveNext | Caller cấp ctx trước setup; một shared valid iterator/cleanup path giữ ledger qua throw/abort/failure. |
| Ownership242–253,336–414 | Observer nhận mọi EntitySpawned, có thể nhận object không thuộc fixture; ledger bị clear trước verification; “Verified0” chỉ là log | Validate actual owned BM membership/context; capture next-wave roots trước BM disposal; kiểm tra reference thực, listener/view/coroutine counts, cleanup result bắt buộc. |
| C0461–507/config255–260 | Probe không dùng actual partial setup/observer; comment owned DropConfig nhưng vẫn LoadDatabasesIfMissing/rate | Dùng chung setup/ledger với fault sau owned objects+observer; owned transient DropConfig qua API hiện có. Không sửa authored assets/runtime. |
| Assertions | A2 vẫn yield745 trước duplicate767; các case không ghi same request natural completion, loot events/count/order/frame/token đầy đủ | Giữ các predicates mới hữu ích, thêm đúng assertions R2 đã giao; không viết lại BM hoặc gọi manual Tick/Complete/Interrupt. |
| Wrapper26–89 | Raw StartsWith không canonical containment; chỉ check reparse LabRoot; không refuse existing outputDir; RunId chưa validate/context vẫn hardcoded runner | Canonicalize các đích và ancestors trước write; segment boundary/reparse checks; fresh identity; immutable launch context truyền runner. |
| Wrapper173–277 | Dot-source helper/chạy Guard trước trusted hashes; preflight chỉ hai lab journals; chưa có revised payload matrix | Verify helper+runtime+runner/settings trước invoke; metadata-only known Owner launcher và prior R2 journal; không coi rollback checkpoint là unresolved launch. |
| Wrapper275–313,362–394 | Backup nằm ngoài try/finally ownership; exception kill không bounded confirm captured identity; Compare vẫn chạy khi Restore blocked; recovery scope lấy từ journal | Cleanup ownership từ trước Backup attempt; validate fresh owned journal path/key/context; captured-process terminal gate và conflict policy chung cho Restore/Compare; blocked thì preserve/nonzero. Actual OS exit tách wrapper reason codes. |
| Wrapper396–448 | Chỉ regex existence; không context/unique terminal receipts, không post-run hash hoặc E/history gates | Structured context-bound exact results, cleanup before PASS; pre/post matrices và preservation fail gates. OS0/!timeout/all5+C0 gate mới là cải thiện thật, nhưng chưa đủ. |

Baseline APIs đủ để sửa trong runner/wrapper: EventBus OnEquipmentDropped/OnLootDecisionRequested/Completed/OnEntityDied/OnBattleStateChanged; SkillCastState request/phase/IsFinished/InterruptSource/ticks; owned DropConfig và public view references. Không có EncounterAdvanced event: quan sát owned BM index/state/frame/member-spawn để đếm advance thực; không bịa API hoặc coi EncounterTransition event là advance đã xong. BM register member trước RaiseEntitySpawned nên filter membership khả thi. BM OnDestroy có deferred Destroy config trong PlayMode; cleanup phải tính actual frame boundary, không clear ledger để chứng minh zero.

Loot/encounter counters và lifecycle coroutine handle là private fields, không public typed API. Prompt R3 cho phép diagnostic reflection chỉ đọc/type-checked trên existing frozen fields; handle observation phải kết hợp state và bounded behavior, không suy thành scheduler proof hoặc yêu cầu runtime accessor mới.

A1 mới có Gold/Material/EXP/ID/modal predicates; A2 có actual slot/ID; A3 dùng actual Title/Tier components; A4 giữ encounter khi no-loot+blocker; A5 kiểm tra queue/state/resource/noadvance. **Tất cả hiện là written source, NOT_EXECUTED**, chưa phải PASS. Thiếu event/order/same-request assertions vẫn là requirements của R2, không feature mới.

## Integrity và report addendum

- Packet **27 files**, manifest **26 unique rows**, bao phủ tất cả file trừ chính manifest; **26/26 SHA khớp**. Artifact/patch hashes trong attachment khớp. Bốn patch nay là valid UTF-8 relative a/b paths, không UTF-16/mojibake; Codex dry-check trên exact baseline copies **4/4 exit0**, bytes baseline không đổi. Không cần rerun Unity để sửa metadata/encoding.
- Ma trận **18/18** thực sự gồm original12runtime +LootUI +3guard/helpers +2metas, hashes hiện tại khớp. Nhưng source chỉ đo prelaunch; không có post-run matrix trong packet. Root current measurement không thay thế historical executor post-run gate.
- Bốn báo cáo có NUL/BEL/backspace thực, escaped paths hỏng và mojibake; dirty-doc hash bị mất ký tự đầu khi ghi. Actual runner meta **GUID7e5ae45bd4db4a0b874c601745a96c20**; `6DF524CA…` là SHA256, không phải GUID. Watcher tên actual OnPlayModeStateChanged, không phải OnPlayModeStateChangedGuarded. Parent receipt có escaped path metadata cần chuẩn hóa trong addendum, không thay raw receipt.
- Historical attempt6792/OS1/NOT_REACHED và2684/OS1/FAIL vẫn thiếu original Unity logs. R2 tự ghi exact CS0117/fixture-duplicate causes mà không giao raw mới; ghi **UNVERIFIED EXECUTOR EXPLANATION**, không đưa thành verified root cause. Final16808/OS0/raw5PASS được giữ. Original Phase A compile12100/OS1/UNKNOWN và final24764/OS1/FAIL vẫn riêng biệt.
- Packet thiếu E before/after full inventories/17fingerprints và original19+8 comparison artifacts như đã giao. Codex đã làm phần current comparison hữu ích; không yêu cầu Owner upload lại hoặc làm giả pre-run measurements. Chỉ phát hành addendum/manifest mới, giữ originals.

## Authority được bảo toàn

P08/P09-A/P09-B ACCEPTED/LOCKED và USER_VERIFIED giữ đúng phạm vi. P08 Gate2 **135/137 exit1/legacy07,09**; P09-A E2 **SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2**; PID25360 **USER_REPORTED_PASS/RESTORE_UNRESOLVED**. F-SAVE-P09B-01 CLOSED/RECOVERY ACCEPTED theo snapshot đã review, historical Restore/execution PARTIAL và Compare/preflight METADATA_ONLY giữ nguyên. Recycle closeouts/A10 Gold-only LOCKED, formula preserved implementation/TBD balance, historical61/current30+30skills không rollout.

Không có roadmap proposal nào được nâng thành product approval. Dependency hiện hành là source preparation/harness review, không cần lặp gameplay gap audit đã đóng. Publication lần này chỉ đúng năm tài liệu continuity trong hai isolated clones; refs cuối và preservation receipt sẽ ghi riêng.
