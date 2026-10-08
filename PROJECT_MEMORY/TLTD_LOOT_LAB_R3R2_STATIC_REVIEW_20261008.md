# TLTD — Review STATIC-03-R2, 08/10/2026 UTC+07

**Kết luận: STATIC REPAIR PARTIAL / REVISION REQUIRED / NO RUN AUTHORIZED.** Chưa chấp nhận tuyên bố “100% R1-01…R1-08 resolved”. Có sửa đúng cần giữ, nhưng runner phát sinh lỗi API/type mới và wrapper chưa sử dụng launch contract. **Unity compilation NOT RUN; C0/A1–A5 NOT EXECUTED; Phase B STOPPED.** Lỗi compile/API dưới đây là kết luận từ đối chiếu declaration thật, không phải diagnostics của một phiên compiler mới.

Bước tiếp theo duy nhất: **F-LOOT-LAB-HARNESS-STATIC-03-R3**, hoàn tất scope static đã giao trong đúng hai file harness. [Prompt riêng cho Antigravity](PROMPT_ANTIGRAVITY_LOOT_LAB_STATIC03_R3_FIX_20261008.md). Prompt đã chuẩn bị; không có evidence dispatch/thực thi R3. Không cần Owner chơi lại, ZIP, hay chạy lại các phần P08/P09 đã LOCKED.

## 1. Source/evidence đã đọc và kiểm tra

Authoritative lab: `C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/stage_gate_lab_20261007_204500`.

Packet nhận: `work/loot_lab_harness_output_r3_r2_20261008_230000`. Codex đọc source runner 1,521 dòng, wrapper 646 dòng, packet, pre/post copies, declaration runtime liên quan và continuity main. Trong tài liệu này, **R** là dòng runner, **W** là dòng wrapper của đúng bản đo dưới đây; đường dẫn `Assets/...`/`Tools/...` nằm trong authoritative lab.

| File | Bytes | SHA-256 |
|---|---:|---|
| Runner hiện tại và post-copy | 73,111 | F5035D96E0D28C1000AD5742C83441FD98411C07A324A526233D1D88B9024E45 |
| Wrapper hiện tại và post-copy | 29,096 | 92BDB6993B79849B5267C663C7EF5255B7F04FCE6DCB24D9A66BD45251EAFB6C |
| Frozen BattleManager.cs | 58,723 | 5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6 |
| Frozen BattleManager.cs.meta | 59 | 59B1AA9F84414AFC5E53F92AF5DFBE3FD4B7A2E6815F9A62980AA8D6AEA80111 |
| Frozen ProjectSettings.asset | 24,410 | A85CC67184A18039D0E1664D7C5625FF2A81BF550889CC1DCCCD0451AB8CC561 |
| Frozen runner meta | 242 | 6DF524CA04537093A4EB824E3C28972CCC21EDBF66DCF7BE969145C7E5B19943 |

Runner GUID thật: `7e5ae45bd4db4a0b874c601745a96c20`. Pre runner 71,096/680F42E1FC9FB825D0F2229286ADCA8DF7179428432079D972D3DD2F82079409; pre wrapper 25,801/D7BC7F54D20F3730D1F1748AE51ADAB82B33067C10E579F52F5CEF5E011CCE36. Sáu pre-copies khớp R3-R1; sáu post-copies khớp lab hiện tại.

Packet có **19 files; manifest 18/18 SHA match; payload receipt 17/17 size/SHA match**. Manifest không tự băm; receipt nằm trong manifest. Contract frozen 19/19 và mutable post 2/2 khớp lab. Đây là integrity của delivery, chưa là launch gate hoặc quyền chạy.

Codex thực hiện riêng lúc 23:00:50+07:

- PowerShell AST trên wrapper đúng hash 92BDB…: **0 parse errors**, không invoke script.
- `git apply --check` hai patch trên exact pre-copies trong disposable repo mới: **2/2 exit 0**, baseline copies không đổi; không apply patch vào lab/E.
- Receipts: `work/loot_r3r2_wrapper_ast_receipt_20261008.json`, `work/loot_r3r2_independent_patchcheck_receipt_20261008.json`.

Executor dry-apply receipt được giữ riêng dưới attribution EXECUTOR_REPORTED; phép kiểm tra mới không chứng thực ngược lịch sử lệnh executor. Không có C# compiler/Unity reference receipt. AST/dry-check không chứng minh C# compile hoặc gameplay PASS. Codex không chạy Unity, wrapper top-level, Guard/preflight action, Registry hoặc save.

## 2. Sửa đúng cần giữ

`TerminateSuiteEarly` đã có `yield break` (R294). Guard đã xử lý nested iterator ở nhánh bình thường, aggregate counter/outcome một lần và dừng suite sau case thất bại (R334–410). A1/A3/A4/A5 có ledger callback; A4 giữ drop observer; A5 bổ sung death/request/completed observer và flag NOT_REACHED. A2 vẫn có hai quái đăng ký thật, Health death thật, DropSystem thật; Material/EXP của cả Tách và Equip nay được gate sau death rewards (R1028–1033/R1066–1070).

Giữ typed diagnostics, transient drop config, `SetChannel`, `SkillExecutionRequest`/`StartChannel`, EventBus signature đã đúng, bỏ global reset/private BM invocation và membership iteration. `RunAllTests` khớp wrapper executeMethod; việc đổi tên không tạo entry mismatch.

Wrapper cải thiện finite LiteralPath/reparse checks, yêu cầu journal.json thật, GUID nonce, captured HasExited/bounded catch wait, conjunction RegSubKey+FullRegPath, evidence-write failure flag và recognized failure-row rejection. Thiếu args/nonbatch ở entry không còn Exit/PlayMode mutation. Các cải thiện này chưa đóng toàn bộ checkpoint.

## 3. Lỗi API mới phải sửa trước

| Finding | Source hiện tại | Declaration thật và sửa tối thiểu |
|---|---|---|
| R2-01: wave config type/property | R583–584 phản chiếu `ActiveWaveConfig`, cast `ActiveWaveMonsterConfig`; field type R1511 cùng tên không tồn tại | `Core/BattleManager.cs:94` là `public MonsterConfigSO ActiveWaveMonsterConfig => _activeWaveMonsterConfig;`. Dùng **MonsterConfigSO** và capture trực tiếp property này trước BM destruction; bỏ invented reflection/property/type và swallowed capture error. |
| R2-02: setter arity | Title R1117/1274/1390 có 11 args; Tier R1131 có 11 args | Title `UI/TitleBreakthroughUI.cs:184–197`: **12 required + 1 optional**. Tier `UI/LootTierProgressionUI.cs:178–191`: **13 required**. Khôi phục call đúng từ PRE_R3R2 runner; không tạo overload mới trong frozen UI. |
| R2-03: modal API | R1145/1146/1278/1394 truyền view vào RequestModal; R1149/1188 đọc CurrentActiveModal; R1185/1198/1349/1466 gọi DismissModal | `UI/Modal/ModalCoordinator.cs`: `ActiveRequest` L45; `RequestModal(ModalRequest request)` L173; `DismissActiveModal(DismissalReason reason = UserClosed, ModalRequest expectedRequest = null)` L359. Retain request A/B thật, compare ActiveRequest và panel visibility riêng, dismiss bằng expected request. |

Đây là regression so với modal/setter calls từng đúng ở R3-R1. Sửa từ declaration thật trước khi nâng điều kiện kiểm tra; không rewrite scenario theo signature tưởng tượng trong báo cáo.

## 4. Các checkpoint còn mở trong scope đã giao

| Checkpoint | Đã tiến bộ | Còn thiếu, exact source anchors |
|---|---|---|
| R1-01 — API/return | Helper yield break, nhiều API cũ đúng | R2-01…03 ở trên; report “exact declaration” vẫn sai UI setters/constructor parameter names. |
| R1-02 — owned cleanup | Nested pump thường, listener ledger, BM trước entity | `stack.Clear` khi MoveNext throw R350 bỏ Dispose của outstanding iterators; Dispose R356 ngoài catch; không outer finally/in-process abort cleanup. Unregister exception R559 chỉ warning rồi clear ledger. BM/Hero/Coord/Root bị gán null R601/611/612/626 trước survivor check R634. Captured wave config chưa có trong predicate. Retain actual references và verified removal trước bookkeeping clear. |
| R1-03 — terminal/driver | Một aggregation, assertion+cleanup gate ở PASS | Driver PASS R266/suite R278 trước DestroyImmediate(driver) R280, chỉ dựa list counts R263. Thiếu verified post-destruction driver/session observation. SetupBlocked/NotReached cần giữ reached classification nhưng cleanup failure phải explicit. Rows thiếu nonce/contract/reached/assertion/cleanup. |
| R1-04 — C0+A1–A5 | Real A2 economy, A4 observer, A5 actual death/cancel | Oracle gaps và regressions bên dưới; source chưa hoàn tất shared ordered observation ledger/natural finishing proof. |
| R1-05 — explicit context | Thiếu args/nonbatch refuse; random GUID wrapper | Contract không được wrapper đọc. R53–75 chấp nhận bất kỳ nonempty pair; SessionState R161–187 không compare actual CLI/context receipt; suite R215–220 có unowned mutation/Exit. R85–107 thay finite include-inactive inventory bằng active-root name heuristic; managers/views trong hierarchy/scene khác lọt. Profile R33/194/W228 vẫn fixed R2. |
| R1-06 — path/journal | Sáu target và ancestors; historical journal files; AND thay OR | Kind/existence phải gate trước output. Fresh Backup W400–403 chưa validate journal trước Unity. W499–510 thiếu schema/status/exact fresh path/baseline coherence/receipt binding; nhận thêm `HKCU:\...` dù Guard thật emit `HKEY_CURRENT_USER\...`. |
| R1-07 — process/finally/evidence | Captured HasExited, bounded wait, log failure flag | W181–198 đọc root StartTime ở stop time, immediate children kill theo PID chưa revalidate captured identity/descendants. W487/535 suppress query error thành apparent absence. Restore/Compare/journal W513–532 chưa có independent exception isolation. WrapperPass W629–638 rồi final Log W641 có thể fail nhưng W642 vẫn exit0. |
| R1-08 — contract/results/preservation | Delivery hashes; one RunId PASS row per known case; recognized FAIL rejection | Wrapper không có contract loader/parameter, chỉ frozen19 ở W244–273/W543–558. E status W561 unused, W563 HEAD-only/no Git exits. Terminal parser W581–601 bỏ qua stale/unknown/malformed/surplus rows; thiếu nonce/schema/order/reached/cleanup và CRLF normalization. W457 lấy first unbound suite row. |

Đây là các phần còn thiếu của R1-01…08 đã giao; không bổ sung gameplay case, feature, runtime accessor hay security framework.

### Existing case predicates

- **C0:** giữ đúng injected exception/type/site; ledger root/observer thật. C0 fault xảy ra trước config allocation, không gán proof cleanup cho config chưa tồn tại. Outstanding iterators/listener/root phải được cleanup thật trên valid in-process paths. Force-kill không thể chứng minh finally/frame; thiếu proof là FAIL/MISSING.
- **A1:** restoring pre-action inventory membership là bắt buộc: R859–867 đã bỏ oracle này, `!HasItem` sau Tách có thể PASS với item chưa từng có. During channel R813 chưa gate queued/drop ID chain/finishing caster; natural R832 cho phép ActiveRequest null; R848 dùng OR thay vì nonempty same ID chain. Cần strict retained request + Recovery/IsFinished + None + ticks/deadline, item/effect/event identity, actual ordered one EncounterIndex advance sau drain.
- **A2:** queueMid/firstDropId R941–943, remaining R979 và req2 mới log/chưa gate FIFO. Thiếu exact request/completion IDs/order, closed window pendingnull/openfalse trước immediate duplicate R1006, no extra event và final drain/one actual advance. Giữ economy đã sửa và snapshots sau legitimate death rewards.
- **A3:** R1165 bỏ StartHeroChannel return/request, R1167 chỉ đợi năm frames trước blocker release. Khôi phục valid request API rồi bắt buộc successful retained channel/natural finish trước kiểm deferred loot. Gate exact A/B/loot IDs, panel visibility, active+queued clear đồng thời trước loot acquire, completion/economy/drain/advance.
- **A4:** retained zero-drop observer đúng; R1318 !active/None chưa đủ strict natural/defer reached. Gate blocker identity/visibility, actual no-loot deferred boundary, queue0/pendingnull/openfalse/request0, hold trước clear/one advance sau clear. Không RNG retry chọn PASS.
- **A5:** bắt đầu channel 1.5s R1412, chỉ đợi ba frames R1422 rồi đòi LootPending R1433. Với ordinary short frames, chưa tới natural completion/defer boundary; đây là source ordering risk, không phải kết quả runtime đo mới. Phải bounded-await strict natural completion trước precondition. `deferRunningBeforeDeath` R1429 chỉ log; alive/blocker/pending/open/request0 bị mất gate. Gold/Material/EXP no-effects oracle từ R1 cũng bị bỏ. Giữ actual death/token-invalidated/defer-cancelled gates mới, khôi phục economy baseline sau death rewards và ngay trước Hero death.

“Exactly one advance” đo số lần **EncounterIndex thực tế tăng** sau modal drain, không phải số BattleState.EncounterTransition events hoặc token increments. BM phát transition state trước index increment. Nonnull coroutine chỉ là handle diagnostic. Typed diagnostic helpers vẫn phải từ chối missing owned instance/type/state, không mặc định thành queue0/false thành công. Không SetValue/private invocation.

## 5. Report/packet khác phần dán trong chat

**15-row baseline table trong packet thật đã đúng 15/15**, với original paths và hashes hiện hành. Contract 19 frozen + 2 mutable cũng khớp. Phần inline user report sử dụng nhiều paths/types/signatures khác và 15 SHA không khớp bảng trong packet; không dùng nó làm source authority và không đánh giá nhầm bảng packet là chưa sửa. Giữ cả hai theo provenance; correction addendum chỉ rõ khác biệt.

Packet API table đúng hơn bản inline ở StartChannel/CompleteLootDecision/Drop/Health, nhưng Title/Tier/LootDecision setters vẫn không phải copied exact declarations; constructor names cũng paraphrased. Report còn overclaim contract wiring, 21-row gate và E porcelain/tool exits. 18 baseline không phải 18 runtime C# files: original12 runtime + LootUI + 3 helpers + 2 metas; 19 thêm settings; 21 thêm hai mutable files.

Report UTF-8 vẫn có mojibake; NUL/BEL/BS/VT đo được bằng 0. Claim toàn bộ files LF/BOM-free cần ghi per-file: frozen copies giữ original byte format, không normalize để làm claim đúng. JSON paths R3R2 decode đúng; vẫn cần executable canonical-path validation khi wire contract. Executor packet thiếu AST receipt file/hash và executor measured E/history pre/post proof; Codex measurement riêng không lấp lịch sử đó. Không tự giải thích CRLF cause, reconstruct missing raw hoặc sửa packet cũ.

## 6. Preservation, main và giới hạn continuity

Codex E snapshot **23:00:49+07**: HEAD `7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425`, full porcelain **3 tracked modified + 530 untracked**. Tracked modified vẫn MANUAL_P09A_CHECKLIST.md, PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md, REVIEW_RESPONSE.md. **17 selected fingerprints unchanged** so với snapshot trước; đây không phải whole untracked/save byte proof.

Prior113 inputs = **111 unchanged + đúng hai allowlisted lab edits**. Original corrective19 + PhaseA8 + R2packet27 + R3packet17 + R3R1packet18 = **89 historical files unchanged**; retained archives/journal/attachments giữ nguyên. Snapshot mới **132 inputs** gồm prior113 current + packet19, archive nhận mới giữ packet19 và22 source/journal references. Receipts `work/loot_r3r2_e_preservation_before_20261008.json`, `work/loot_r3r2_previous_input_comparison_20261008.json`, `work/loot_r3r2_review_inputs_before_20261008.json`; archive `work/loot_r3r2_received_review_20261008`.

Hai main trước publication docs đã read-only verify:

- Ai_MEMORY_TLTD: `86ec801c2aa987e6cf6dd5ed0760bd1ab1ee1ba9`.
- ta_la_ta_de: `c2ca08cca3fbe4cbedf79715762de55188b38694`.

Continuity publication chỉ exact review/prompt + ACTIVE_WORK_HANDOFF/CURRENT_DESIGN_AUTHORITY/DESIGN_CHANGELOG trong hai isolated docs checkouts theo authority Owner đã có. Final remote/blob/readback/preservation receipts riêng sau publication. Không thay E/lab source, không push source/assets/save.

Parent **F-LOOT-CHANNEL-HANDOFF-LAB-01** vẫn **LAB RUNTIME PATCH ACCEPTED WITH EVIDENCE LIMITS; OVERALL CLOSEOUT PENDING HARNESS / EVIDENCE CORRECTION**. Source harness gaps không chứng minh BM regression. Phase A 12100/OS1/UNKNOWN và24764/OS1/FAIL; corrective6792/OS1/NOT_REACHED,2684/OS1/FAIL,16808/49.2020002s/OS0/raw5PASS; early raw mất/causes UNVERIFIED. R2 PID24052/52.5784775s/OS1/no-timeout COMPILE_BLOCKED/NOT_REACHED, parentwrapperexit1/55.3395684s; fresh absent lab-profile Guard0valuesPASS/journalVERIFIED giữ scope, không upgrade Owner recovery.

P08/P09-A/P09-B ACCEPTED/LOCKED/USER_VERIFIED không đổi. P08Gate2 **135/137 exit1**, legacy07/09; P09-AE2 **SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2**; PID25360 **USER_REPORTED_PASS/RESTORE_UNRESOLVED**. Recovery raw Restore/execution **PARTIAL**, Compare/preflight **METADATA_ONLY**, snapshot/digest limits giữ nguyên. KnownOwnerlauncher VERIFIED24-values metadata riêng, intentional BACKED_UP rollback checkpoint riêng; không invoke recovery lại. Recycle Gold-only/harness closeouts giữ nguyên. StageR1 analysis only, Stage/Boss proposal chưa product approval; Phase B stopped; production transfer/exposure/persistence/schema/skill activation chưa được mở.

Chưa có dữ liệu Owner nào cần bổ sung cho bước sửa source này. Antigravity trả exact source/diff/static receipts trước Codex review và một run prompt riêng; không tự chạy để bù thiếu static proof.
