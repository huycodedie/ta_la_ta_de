# TLTD — Review STATIC03-R3, packet 08/10/2026 UTC+07

Review hoàn tất và continuity tiếp tục ngày **09/10/2026 UTC+07**. Giữ tên file ngày08/10 để nối đúng packet/task; các phép đo ngày08/10 bên dưới giữ timestamp gốc.

**Verdict: STATIC REPAIR PARTIAL WITH REGRESSIONS / REVISION REQUIRED / NO RUN AUTHORIZED.** Chưa nhận “100% R1-01…08 resolved”. Source R3-R3 sửa được một số điểm, nhưng rewrite runner làm mất flow đã đúng và đưa nhiều API không tồn tại vào code. Wrapper có loader thật nhưng chưa thực thi quyền của contract, đồng thời phát sinh journal/path/process blockers. **Unity compilation NOT RUN; C0/A1–A5 NOT EXECUTED; Phase B STOPPED.** Những lỗi API và ordering bên dưới là static source findings, không phải compiler/runtime diagnostics mới.

Next duy nhất: **F-LOOT-LAB-HARNESS-STATIC-03-R4**, cùng hai file/sáu cases/tám checkpoint đã giao. [Prompt riêng cho Antigravity](PROMPT_ANTIGRAVITY_LOOT_LAB_STATIC03_R4_FIX_20261008.md): phục hồi các khối API/fixture/entry/UI-flow đã đúng từ pre-R3R3 copy, giữ sửa đúng hiện tại, rồi đóng phần còn thiếu bằng diff có điểm neo. Prompt PREPARED/NO DISPATCH EVIDENCE/NOT EXECUTED. Không có Owner data nào chặn source preparation; không cần Owner chơi lại/ZIP/recovery rerun.

## 1. Identity và independent evidence

Authoritative lab: `C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/stage_gate_lab_20261007_204500`. Packet: `work/loot_lab_harness_output_r3_r3_20261008_233000`. Codex đọc full runner **1,297lines**, wrapper **562lines**, packet/pre/post và relevant frozen declarations. **R/W** là dòng hai source hiện hành; runtime/helper paths tương đối trong lab.

| File | Bytes | SHA256 |
|---|---:|---|
| Runner/post-copy | 61,102 | D0E78357E02512D999A819579E9F683C8EC641372B11E783B9C74C3B06A53505 |
| Wrapper/post-copy | 24,737 | 881200E23795285EC5A2A696A567E03CD4D0B0D5C77BD89EBE882D0E0D4284F9 |
| Pre runner | 73,111 | F5035D96E0D28C1000AD5742C83441FD98411C07A324A526233D1D88B9024E45 |
| Pre wrapper | 29,096 | 92BDB6993B79849B5267C663C7EF5255B7F04FCE6DCB24D9A66BD45251EAFB6C |
| Frozen BM | 58,723 | 5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6 |
| Frozen BMmeta | 59 | 59B1AA9F84414AFC5E53F92AF5DFBE3FD4B7A2E6815F9A62980AA8D6AEA80111 |
| Frozen settings | 24,410 | A85CC67184A18039D0E1664D7C5625FF2A81BF550889CC1DCCCD0451AB8CC561 |
| Frozen runnermeta | 242 | 6DF524CA04537093A4EB824E3C28972CCC21EDBF66DCF7BE969145C7E5B19943 |

Actual runnerGUID7e5ae45bd4db4a0b874c601745a96c20. Packet19 files/manifest18unique18SHA matches/payload17size+SHA matches; pre6=priorR3R2post6, post6=currentlab6. Actual baseline15table **15/15correct**; contract **21/21 unique bytes+SHA matches**. Decoded JSON paths hợp lệ, không có lỗi overescaping. Contract có **RunAuthorized=false / SOURCE_PREPARATION_ONLY / SEPARATE_RUN_PROMPT_REQUIRED**.

Codex lúc23:47:41+07 đo E HEAD/status/17files; parse exact wrapper881200… **AST0errors**. Independent `git apply --check` hai exact patches trên disposable pre-copies **2/2exit0**, copies unchanged/noapplication. Receipts `work/loot_r3r3_wrapper_ast_receipt_20261008.json`, `work/loot_r3r3_independent_patchcheck_receipt_20261008.json`. Không invoke wrapper/Guard/preflight/Unity/compiler/Registry/save. AST/drycheck không phải C#compile/gameplayPASS. Executor dryreceipt được giữ riêng EXECUTOR_REPORTED; kiểm mới không xác thực ngược lịch sử lệnh executor.

## 2. Sửa đúng cần giữ

- `MonsterConfigSO` + direct BM.ActiveWaveMonsterConfig capture R598–603; retained core refs/wave config được check sau frame trước clear R642–681. Unsubscribe exception đã fail cleanup R570–575.
- Named Title/Tier/Loot setters hiện khớp frozen signatures; retained ModalRequest + ActiveRequest + DismissActiveModal là API thật. Finite include-inactive manager/view/entity scan R111–132 thay root-name heuristic; missing args/nonbatch/unowned suite refuse không Exit.
- Stack unwind finally R412–422 có Dispose calls; terminal case rows có nonce/contract/reached/assertions/cleanup. Normal-success driver callback quan sát destroyed GameObject sau DestroyImmediate R278–318. Các điểm này vẫn chưa là cleanup/context acceptance đầy đủ.
- A1 có inventory-before oracle; A3/A5 có bounded wait trước boundary; A5 đã khôi phục Gold/Material/EXP snapshots. Đây là source intent, còn phụ thuộc API/fixture/natural proof đúng.
- Wrapper thật sự đọc JSON và consumes payload hashes W76–83/195–199, capture root StartTime once W347–350, exact HKEY_CURRENT_USER conjunction, CRLF normalization, Git exit-code capture. **Final log-failure exit bug đã sửa**: W558 kiểm flag lại sau final Log; không giữ finding cũ này là OPEN.

## 3. Runner regressions chặn source readiness

### API map phải sửa bằng actual frozen declarations

| Current source | Actual API / correction |
|---|---|
| `using WuxiaGame.Systems` R14 và thiếu namespaces | Dùng namespaces đã đúng ở pre-R3R3: Combat/Drop/Items/Equipment/Progression; Inventory type fully qualified `WuxiaGame.Inventory.Inventory`. |
| `DropConfigSO`/`.DropRate` R541–547/R1049–1055 | `Data/EquipmentDropConfigSO`, `InitializeConfig(dropRate, weights)`; `Drop/DropSystem.cs92 SetDropConfig(EquipmentDropConfigSO config)`. Rate là percent0–100: standard deterministic cases dùng **100**, A4 dùng **0**, không đổi 1.0 thành 100%. Restore real database/config path từ pre-copy. |
| BM.SetChannel/InitializeBattleManager R553–554 | Không tồn tại. Restore `RegisterHero`, real fixture initialization/registration và `StartBattle`; cast thuộc **Hero.CastState**, không BM.SkillCast. |
| BM.SkillCast/QueuedLootDecisionsCount/PendingLootDecisionId/IsLootPending/BattleState ở cases | Hero.CastState (`Entities/Entity.cs62`); BM.PendingLootQueueCount L290; BM.PendingLootItem L859, lấy `.InstanceId` từ item thật; BM.CurrentBattleState L740. Typed read-only diagnostic fields cho token/defer/finishing như pre-copy, không thêm accessor. |
| SkillSlotType.Primary R734/840/998/1113/1214; InterruptSource.None R758/1128; BattleState.NotStarted R1274 | Slot **Skill** và enum **SkillCastInterruptSource.None**; current BM state AwaitingPlayerStart đúng existing scope, không invented enum value. |
| `Hero.CurrentExp` R775/786/866/872/1250/1270 | EXP của owned **ProgressionManager.CurrentExp**, kiểu **float**, L44; không thêm EXP API vào Hero. |
| completion handlers Action<string> R721/822/1159 | `Core/EventBus.cs50`: **Action<EquipmentInstance>**; lấy ID trong handler của retained item. |
| UnregisterModalView(.ModalId) R588–590 | `UI/Modal/ModalCoordinator.cs153`: **UnregisterModalView(IModalView view)**. Truyền view thật. |
| Coord.PendingRequestsCount R982/1028 | Existing **TotalQueuedCount** L53; active identity/panel riêng. |

HealthComponent.SetCurrentHealth(float hp), ResourceManager.SetMockResources(int,int) và Inventory.HasItem(string) **có thật**. Không báo nhầm các APIs này là absent. BlockerA/BlockerB/SpawnedNextWaveEntities fields cũng được khai báo thật trong FixtureCtx.

### Fixture, entry, C0 và UI-flow

**Entry mất owned PlayMode transition.** R104 từ chối already-playing; R135–139 tạo scene/driver và gọi MonoBehaviour.StartCoroutine ngay trong EditMode. Không còn isPlaying=true, EnteredPlayMode watcher/SessionState/reload binding. Thêm InitializeOnLoad automatic entry R20–39 và MenuItem R42–45 không tạo được lifecycle PlayMode hợp lệ. Restore previous explicit owned entry/transition/reload, gate actual approved immutable context trước scene mutation; không auto-run từ load/interactive và không sửa runtime/editor settings lúc static preparation.

**Fixture bỏ initialization và dùng second HealthComponent.** Hero/Monster RequireComponent Health ở frozen Hero13/Monster9. R483–485/R711–713/806–808/813–815/987–989/1102–1104/1203–1205 add entity rồi add Health thứ hai; kill mới-added component không chứng minh chính Entity.Health/IsAlive của quái chết. Restore owned configs/stats/Progression/Drop database init, `monster.Health`/`hero.Health`, BM.RegisterHero/registered monsters/StartBattle theo pre-copy. Không fake deaths/queue hoặc invented BM.InitializeBattleManager.

**C0 hiện luôn thất bại tại expected injected path.** Fault R698–699 xảy ra trong MoveNext; generic catch R389–394 set CaseFailed dù isProbe. R436–439 => CLEANUP_PROBE_FAIL, R245 stop suite. Catch quanh iterator creation không catch deferred fault. Restore exact injected type/site/message flag trong guarded pump; chỉ fault đúng+cleanup verified mới PASS; unrelated exception vẫn FAIL. Đây là source control-flow conclusion, không C0 runtime result.

**Tách/Equip bỏ UI completion.** A1R778/A2R868,882/A3R1036 gọi BM trực tiếp. Frozen LootUI.OnDismantleClicked/OnEquipClicked gọi BM rồi **CompleteActiveModal(capturedRequest)** (UI500–566). Gọi BM riêng không đóng active loot modal; BM final drop defer L1146–1150 chờ modal complete. R791/895/1038 đòi index+1 ngay sau decision là không đúng asynchronous boundary. Restore real UI button callbacks, observe success/economy/IDs qua ledger; await bounded modal drain/actual index advance. Chỉ direct duplicate call dùng supported BM API trong verified closed window, ngay cùng stack sau **first Tách**, trước yield; R3 đổi duplicate sang sau second Equip là regression của existing A2 oracle.

## 4. Remaining eight checkpoints, same scope

| Checkpoint | Remaining bounded work |
|---|---|
| R1-01 | API/type/delegate table trên; report seven rows không exact. Restore valid blocks trước enhanced assertions; không rewrite framework/scenarios. |
| R1-02 | Dispose exceptions R399/419 swallowed; fixture teardown R426 nằm sau finally nên abort/disposal có thể skip. Teardown stage exception có thể bỏ phần sau. Verify retained managers/views/listeners/handles/next-wave refs và failure-recorded removal, không chỉ root/core/SOs; reintroduce owned spawn observer cho cases thực. C0 expected fault logic sửa riêng. |
| R1-03 | Normal driver null observation mới đúng, nhưng thiếu immutable owner context/task-state/ledger proof; callback chỉ batch-check khi Exit, static refs chưa cleared. Failure path R330–336 vẫn gán driverFAIL vô điều kiện. Emit measured driver outcome riêng, common cleanup path và exact structured schema cả driver/suite. |
| R1-04 | Natural proof thiếu same request/Recovery/None/ticks/deadline ở A2/A3/A5; SetChannel config/start bool/finishing membership bị bỏ. A1 request/drop/pending/completed counts/ID chain; A2 first-kill flow/FIFO/secondEquip/economy/firstduplicate gates; A3 completed/economy/IDs; A4 actual drop observer/no-loot defer; A5 valid queuedID/token/defer/death-event/inventory/strictAwaiting gates chưa đủ. Ordered one real index advance sau drain, không endpoint-only. |
| R1-05 | Wrapper RunAuthorized ignored, fallback/default context/output/profile; runner nonempty token/nonce fallback/UNSPECIFIED contract, no receipt/reload comparison. Loader không đồng nghĩa approved gate. Future editor6000.6.0f1 + frozen settings YAML mapping phải gate; current descriptor unauthorized phải refuse trước side effects. |
| R1-06 | Historical paths/status policy/Owner optional mới sai; fictional journal.Action; schema/baseline/hash/owned fresh path/receipt binding thiếu; reparse above-lab/output/contract ancestors và file kind phải check trước write. |
| R1-07 | Captured root time đúng, nhưng immediate child PID-only stop/inspection unknown/independent finally thiếu. No-match Get-Process bị coi inspection error, post-query lại suppress. Prelaunch required evidence failure chưa chặn launch; safe cleanup vẫn cần khi later evidence fail. Final-log exit bug đã FIXED. |
| R1-08 | 21-row exact set/count/length/trust chưa gate/persist/post-check; fallback stale wrapperSHA4CBF…; E status chỉ captured, không compared/17fingerprints/history; all-terminal nonce/schema/order/surplus parsing thiếu. |

Same6cases, no fullSkillExecutor/Stage/new feature. Nonnull coroutine chỉ handle diagnostic, không scheduling proof. EncounterIndex actual observed increments sau drain mới là advance count; token/state-event không thay nó. Forcekill không chứng minh C# finally/frame/drivercleanup; missing=FAIL/MISSING.

## 5. Wrapper concrete incompatible gates

Contract **RunAuthorized=false** bị bỏ qua W81–82; missing/invalid contract dùng fallbackW200–225. AutoRunId/GUID W31–35 và fixed reused output nằm **lab/work/** W38, writesW45–50 trước path gate. ReceiptW319–328 sau Backup và thiếu canonical output/backup/journal mapping. Future runner vẫn chấp nhận arbitrary nonempty args/defaults R85–93. Restore required explicit approved mapping/receipt before Backup; descriptor hiện tại phải refuse. W6 defaults Unity2022.3.21f1, frozen ProjectVersion là **6000.6.0f1**.

Historical journals W149–154 ghép corrective/R2 paths vào lab thay vì workspace paths thật trong contract. W161 bắt **BACKED_UP**, trong khi ba known journals thật **VERIFIED**. P09B preflight frozen blocks unresolved BACKED_UP/RESTORE_IN_PROGRESS/RESTORE_FAILED, permits VERIFIED. Owner missing/parse/status chỉ optional/log W173–180. Intentional BACKED_UP rollback checkpoint riêng không được reclassify thành launcher hay recovery work.

Fresh validator W312 đòi **Action=Backup**; actual Guard journal schema G306–315 không emit Action. Honest Backup bị từ chối trước Unity; không sửa Guard để khớp field bịa. Dùng actual Status/RegSubKey/FullRegPath/OriginallyExisted/ValueCount/Snapshot/BackupFile/BackupHash, coherent owned fresh path/hash và external receipt binding. RecoveryW432 chỉ hai strings chưa đủ.

W411 `Get-Process -Name Unity -ErrorAction Stop` đẩy ordinary no-match vào catch/queryError, chặn recovery trên empty normal result. W458 SilentlyContinue lại biến unknown thành absence. Dùng successful enumeration phân biệt empty/error, captured root+descendants và conflict gate prelaunch/pre-postrecovery. KillchildW100–105 vẫn PID-only/immediate/suppress. Restore/CompareW442–450 chưa independent protection. No safe recovery claims mới.

Payload mismatchW227–250 chỉ ảnh hưởng final PASS, chưa chặn Backup/launch. Matrix chỉ memory/pre/SHA, không required length/exact21/persist/post. E pre/post successfulcommands W470–473 chưa compare full porcelain/17selected/history. ParserW492–511 chỉ RunId success-prefix + known failures, ignore Nonce/Contract/reached/cleanup/stale/malformed/surplus/order; ScenarioResultW380 first unboundrow. Required evidence trước launch thiếu; final evidence flag gateW558 nay đúng.

## 6. Packet/report provenance và preservation

Bảng baseline15 nay khớp cả inline và packet. Nhưng **actual report7/14API rows** còn sai copied declaration/path/line: StartChannel void/Core; BM Complete void; DropConfigSO; Unregisterstring; SkillExecutionTypesCorepath; Health method parameter names/line metadata. Title/Tier/Loot setters và ModalRequest nay đúng. Report control/encoding claim cần đúng phạm vi: all19 strictUTF8/LF/zeroCR, namedNUL/BEL/BS/VT0; listed json/patch/md/txt BOM-free đúng; frozen BM/settings copies giữ originalBOM. Report vẫn mojibake và30C1Unicodecontrols; viết readable newaddendum, không normalize/sửa packet cũ. Không giữ finding cũ “frozenCRLF/allBOMclaimwrong” khi actual measurement đã đổi.

Không có executor AST receipt file trong 19 files của packet, không C#compile/Unity/run/preservation raw. Payload receipt có 17 rows. Dryreceipt metadata exact nhưng thiếu disposable postcheck unchangedhash/separate rawstreams. Codex AST/patch/E/history receipts riêng không bù executor history. Báo cáo100%resolved/guard schemahash/21persist/statuspreservation vượt source.

Codex E **23:47:41+07** HEAD7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425/fullporcelain **3tracked modified+530untracked**/17selected unchanged. Prior132 = **130same+2allowed lab edits**; original108historical packetfiles và retained archive/journal/attachments unchanged. New **151input snapshot**, archivepacket19+22source/journal. Measured scope only, không wholeuntracked/save/globalprocesshistoryproof.

Before docs publication main verified: memory **c18d9c9216bce57d80fd7f1499a7d9009a290c1b**, game **d06d4b794c95a35a682f071708663ff871f0bd03**, isolated checkouts clean. Continuity only review/prompt+ACTIVE_WORK_HANDOFF/CURRENT_DESIGN_AUTHORITY/DESIGN_CHANGELOG, exact5docs mỗi repo theo authority Owner đã có; final normalpush/remote/blob/preservationreceipt riêng. Không E/lab/source/assets/save publication.

Parent **LAB RUNTIME PATCH ACCEPTED WITH EVIDENCE LIMITS; OVERALL CLOSEOUT PENDING HARNESS / EVIDENCE CORRECTION** unchanged; harness regressions không chứng minh BM regression. PhaseA12100OS1UNKNOWN/24764OS1FAIL, corrective6792OS1NOT_REACHED/2684OS1FAIL/16808OS0raw5PASS, earlyrawmissing/causesUNVERIFIED; R2PID24052/52.5784775s/OS1/no-timeout/COMPILE_BLOCKED/NOT_REACHED/parentwrapperexit1/55.3395684s, absent-lab-profileGuard0valuesPASS/journalVERIFIED giữ scope. **P08/P09-A/P09-B ACCEPTED / LOCKED**; recycle CLOSED / FIX VERIFIED trong evidence limits đã ghi; F-SAVE-P09B-01 CLOSED / RECOVERY ACCEPTED theo snapshot/metadata. **P08 Gate2: 135/137, exit1, legacy07/09**; P09AE2SCENARIO_PASS_EXIT_TIMEOUT/wrapper2; PID25360USER_REPORTED_PASS/RESTORE_UNRESOLVED; recoveryRestore/executionPARTIAL/Compare-preflightMETADATA_ONLY/snapshotdigestlimits. KnownOwnerlauncherVERIFIED24metadata riêng/intentionalBACKED_UPcheckpoint riêng. Stage analysis/proposal/PhaseBstopped; productionStageBoss/exposure/persistence/schema/skills/product approval chưa mở.
