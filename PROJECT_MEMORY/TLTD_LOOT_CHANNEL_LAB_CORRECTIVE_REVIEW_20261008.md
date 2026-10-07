# TLTD — Review corrective first-loot lab và phần còn thiếu

Ngày08/10/2026, UTC+07. Codex Tech Lead/Architect. Review source/evidence; không thực thi Unity. Prompt tiếp theo tách riêng tại [PROMPT_ANTIGRAVITY_LOOT_LAB_HARNESS_EVIDENCE_R2_20261008.md](PROMPT_ANTIGRAVITY_LOOT_LAB_HARNESS_EVIDENCE_R2_20261008.md).

## 1. Quyết định

**F-LOOT-CHANNEL-HANDOFF-LAB-01: LAB RUNTIME PATCH ACCEPTED WITH EVIDENCE LIMITS; OVERALL CLOSEOUT PENDING HARNESS / EVIDENCE CORRECTION.** Bản vá BM hẹp đúng hướng đã giao; final raw run xác nhận first-loot decision-open true, UI Tách đi qua transaction, Gold5000→5100, modal release được quan sát và encounter1→2. Không phủ nhận final raw **SUITE_RESULT=PASS / 5 PASS**. Tuy nhiên các PASS predicates thiếu assertions cần thiết, harness/wrapper chưa đạt safety, và report thiếu hai failed attempts. Vì vậy không nghiệm thu toàn bộ claim5/5/complete-safe-packet.

**Phase B Stage Gate integration = NOT EXECUTED / STOPPED.** Chưa cho chạy tiếp gate integration, chuyển source vào E: hoặc source publication. BM corrective được giữ nguyên tại lab; không yêu cầu revert hoặc lặp baseline lỗi Phase A.

**Next task: F-LOOT-LAB-HARNESS-EVIDENCE-02 — AUTHORIZED LAB ONLY / PROMPT PREPARED, NOT DISPATCHED / NOT EXECUTED.** Sửa đúng runner/wrapper/identity và evidence/assertions còn thiếu; BM58723/hash5B477… frozen. Một session có giới hạn sau preparation đủ an toàn để kiểm chứng các thay đổi thực này. Metadata/manifest/encoding/history corrections riêng không cần Unity rerun; missing historical logs không được tái tạo bằng run mới. Đây là concrete tooling/evidence risk, không mở lại P08/P09 acceptance.

Không Owner playtest/retest/recovery rerun/closureZIP. Không Stage/Boss product approval, persistence/schema/reopen/exactly-once durable guarantee, new economy/Material/Bun/offline/Companion/skills/UIpolish. Existing accepted/locked scopes và recovery limits giữ nguyên.

## 2. Inputs và current baselines

Đã đọc attachment `C:\Users\conca\.codex\attachments\2bf9c4cc-9d75-4754-b803-513400ff9f4b\Văn bản đã dán.txt`; tất cả19 files của `work/loot_channel_handoff_lab_output_corrective_20261007_232818`, actual lab BM/runner/wrapper/settings và relevant baseline runtime/guard sources. Đối chiếu prompt07/10, prior preserved byte copies, previous review fingerprints và original8 Phase A artifacts.

Lab root: `C:\Users\conca\Documents\Codex\2026-10-05\b-n-ti-p-qu-n\work\stage_gate_lab_20261007_204500`.

Current canonical main **66adb3b7b7b06ff5622dd4fcac649f6f8b62f630**, game main **005a6a35f94b7cce2b4a9a4201aa95b836dace26**; isolated clones pulled `--ff-only` và clean trước documentation edits. Local E: HEAD **7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425**; underlying source393a048… distinct from later docs commits.

Read-only E: snapshot08/10 **00:13:05+07**:3tracked modifications+530untracked entries, full porcelain inventory và17selected fingerprints match previous review. Current lab **11/12 original runtime paths** match E:/prior baseline; only BM has intended corrective. LootDecisionUI/core/preflight helpers và metas được đo thêm riêng. Đây là measured equality/status, không full untracked bytes hoặc historical save proof. Không sửa dirty E: memory để sync.

Codex không chạy Unity/wrapper/test/SaveGuard, không đọc live Registry/save hoặc sửa E:/lab source. Bằng chứng runtime là inspected executor logs, không Codex-executed test. Packet19files và21currentlabsource files được archive byte-identically tại `work/loot_corrective_received_20261008`; original8 Phase A artifacts vẫn hashmatch, old lab journal đo thêm riêng. Snapshot49 reviewinputs và local external manifest đã tạo, không phải executor đã giao manifest đầy đủ. Final remote refs/preservation verification ghi receipt riêng.

## 3. Ba attempts, không phải một session

Appended wrapper.log giữ ba lần launch cùng RunID/profile/output/backup. `unity.log` hiện chỉ có final run. Giữ raw outcomes; không gộp failed attempts thành successful preflight.

| PID | Start→End, UTC+07 07/10 | Elapsed raw | OS / wrapper | Scenario |
|---:|---|---:|---|---|
| 6792 | 23:50:17→23:50:51 | 34.7338972s | 1 /1 | NOT_REACHED |
| 2684 | 23:52:05→23:52:55 | 49.5357603s | 1 /1 | FAIL |
| 16808 | 23:57:30→23:58:19 | 49.2020002s | 0 /0 | PASS, raw5/5 |

Wrapper lines71/72/125,220/221/274,369/370/423 establish these outcomes; wrapper-exit column là logged markers consistent với source, không parent-shell exit receipt độc lập. No watchdog timeout in any attempt. Reports describe only PID16808/49.20s. Prompt allowed one bounded session and stop after failed attempt before Codex review. Reused output overwritten earlier Unity logs/hash matrix and final journal; retained wrapper/guard text still records earlier journal metadata. Không có standalone first-two Unity logs/intermediate source versions hoặc journals đầy đủ để diagnose their scenario failures independently. Không invent reason/compile state từ NOT_REACHED alone. Có thể đọc archive còn tồn tại để tìm originals; nếu không có, ghi MISSING/OVERWRITTEN, không rerun cho mục đích lịch sử.

## 4. Bản vá runtime được giữ và giới hạn

BM58723bytes/hash **5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6**, baseline58100/hash3A021DE… . Semantic change chỉ first queued-loot branch của `WaitForFinishingExecutionsThenProceed`1291–1307: enterLootPending, captureencounter, **++lootTransactionCounter**, canonical TryPresent khi active/queued modals clear hoặc existing DeferNextLootDecisionRequest nếu bận. Queue không directdequeue tại branch nữa. Existing channel checks/transition token/no-loot/complete-beforeeffects/reward/sequential/growth bodies nguyên trạng; current file thêm UTF8BOM.

Captured token dùng đúng **lootTransactionCounter**, không encounterTransitionCounter. Có một extra first-handoff increment so với baseline nonfinishing pattern capture-current; không claim numerical token sequence identical. Final case1 rawlootTx **2**, không1 như execution report. Increment này không mở một reward/save authority khác trong inspected branch; preserve current byte baseline cho R2, không refactor BM để sửa harness.

Natural evidence là direct real SkillCastState.StartChannel request, Entity.Update/yieldframes và legitimate Health death/EventBus; không manualTick/Complete/Interrupt/reflectionstate repair. Wait0.484s là accumulated waiting delta, không full end-to-end skill duration/request telemetry. Không SkillExecutor/Rage/CD/projectile/dash hoặc production4–5 wave/Owner touch/visual acceptance. Runtime instantiated1/2registered monsters; spawned following waves dùng actual BM path. Final log có UnityEditor.Search indexing exception646–655; không claim zero errors hoặc clean headless import. Wrapper command không `-nographics`; provenance claim đó sai.

## 5. Actual observations khác với PASS coverage

| Case | Raw observation được giữ | Thiếu hoặc oracle chưa bắt buộc |
|---|---|---|
| 1 firstloot | raw968finishing1/queue1;1032LootPending/openTrue/itemID;1126Gold5000→5100;1138modalDrainedTrue;1276enc1→2 | Gold/open/advance gated. Inventory snapshot0→0 là trướcdeath/afteraction và chỉlogged; không assert item insertion/removal theoID. No ProgressionManager nên EXP không đo; Material khôngassert. modalDrained khôngthamgiaFAIL; queued/pending/request/completion count/frame order chưađủ. |
| 2 sequential | raw1917item1/queue1;2012dupFalse;2064differentitem2;2257Gold+100/enc1→2 | UniqueIDs/duplicatebool/advance gated; không equippedslot/ID, Inventoryremove, effect/completion/event counts, noeffectduplicate hoặc modal ordering assertions. Bool-only API không nhận callerID; không suy thành stale-ID protection sau next item. |
| 3 active/queued | raw2357active1/queued1;2632openFalse;2685secondblockeractive/openFalse;2751openTrue/item;2974advance | Source486–493 chỉ openFalse, không queue/pending/request0 assertions dù logs nói no-dequeue. Registered StubModalView `_vis` chỉ contracttest với actual Coordinator, không real runtime UI view. AfterB raw2739active1 là lootmodal; không report active0/queued0 snapshot riêng hoặc one-frame wake. |
| 4 no-loot | raw3292drops0/queue0/openFalse,3316noLootTrue/advance | Existing zero-drop/noPending/advance predicates hữu ích; no blockingmodal control, request end reason/frame history chưađủ. Rate0 không deterministic guarantee vì source roll<=rate; actualzero observation vẫn cógiátrị. |
| 5 cancellation | raw3678deferredTrue;3780stateAwaitingStart/openFalse/enc1→1;3792noStaleAdvanceTrue | DeferredActive/noStaleAdvance chỉlogged; FAIL629 chỉ staleBlocked. Có thểPASS khi chưareachdeferredboundary hoặc wrongadvance. Queue/pending/requestevent invalidation chưaassert. |

SUITE condition line141 cho `casesPassed>=4`; required5cases không buộc all5execute. Không convert raw5PASS thành FAIL, nhưng cũng không convert logged predicates thành verified fail-sensitive assertions. R2 phải ghi reached và assert đúng effect/order/cleanup thay vì thêm “100%” wording.

## 6. Safety blockers thực tế

Runner:

- CLI52 dataPathContains thay exactcanonicalpath;62 NewSceneSingle trước refuse unowned relevant managers. Postreload80–84 chỉprofile/batch, thiếu exactpath/ownedlaunchcontext. Named watcher códetachvalidbranch, nhưng vẫn InitializeOnLoad và exceptional/stalebranches chưacleanup đầyđủ; report “removed hiddenwatchers” không chínhxác.
- Public `CorrectiveHarness.RunAllCases`109 vẫn bypass controlledentry khi attachcomponent/contextprofileđúng. Suite/case bodies khôngfinally; BuildFixture exception mấtpartialctx, yieldbreak khôngdisposeallocatedroot/SOs/listeners.
- Teardown212–227 chỉDone, khôngactualownedcount/subscription/lifecycleassertion. **BMnext-wave monsters sinh tại scene-root**, khôngparentunderfixture; BM.OnDestroy324–338 khôngdestroylist. Successfulcases sinh4+4+5+4next-waveobjects theo raw, survive rootteardown và laterBM.Awake có thểdeactivateambient leftovers. Đây là concrete ownership leak giữa cases, không chỉ hypotheticalexception.
- Dropfixture gọiLoadDatabasesIfMissing vàsetrate, không ownedDropConfig; missingEXPmanager/effect snapshots. Không broad production config/source fix được authorize vì thế.

Wrapper:

- RunID12 hardcoded; run/output/backup dirs reused; logsappend/unityoverwrite. CreatesOutputBase path before containmentvalidation. LabRoot93 substring, ProjectSettings regex prefix; noexact/reparse/output/backup bounds; actualobservedprofilecorrect không chứngnhậnsafereuse.
- Matrix12rows thực **9runtime+guard+2metas**, không original12runtime. Missing Progression/Inventory/Equipment/SkillExecutor; reusedcore/P09Bpreflighthelper khôngvalidate/use; chỉCheckInterruptedJournal ở currentreuseddir, khôngstrictoldrelevantjournalpolicy.
- RemovedAllowRunningUnity tốt và actualguardno-bypass; nhưng timeout/catchkill khôngguaranteeownedterminal trướcfinally. OtherUnitychecks264/cleanup excludeownedPID, nên `cleanupVerified=True` khôngchứngminhownedPIDgone. Guard hiệncó processconflict protection nhưng wrapper khôngđạtassignedexplicitterminalcontract.
- WrapperPass309 omitsOSexit/timedOut/ownedterminal vàharnesscleanuprecord. Có thể scenarioPASSmarker+OSfailure/timeout vẫnwrapperPASSnếuguardok. Source/identity/postpreservation/cleanup markers cũngchưagatetogether.

R2 cần khắc phục trước launch mới. Không đóng/kill Owner Unity, bỏguard hoặc resetglobalmanager để làm sạch fixture.

## 7. Guard evidence và history limits

New `guard_raw.log` có args/stdout/exit cho4actions của mỗi3attempts; BackupOriginallyExistedFalse/ValueCount0, Restore scope-deleted/baselineverified, Compare0valuesPASS vàexit0. Đây là cải thiện raw proof **happy-path absent lab profile**; current final888-byte journalVERIFIED cùngtimestamp23:57:30/restored23:58:19. Previousjournal metadata cótrongappendedtext, fileđãoverwritten; khôngclaimfullperattemptartifact.

CheckInterruptedJournal stdoutblank/exit0 khôngthaystrictP09B unresolvedstatuspolicy hoặc chứngminh oldrelevantjournalsđãcheck. Actualcompany/productfinalmatchkey; noevidenceOwnerprofilewritten, nhưng nofullOwnerliveRegistry/saveinspection. Không promote newlabRestore/Compare thành fullhistoricalrecovery/all-exception guarantee; originalP09B rawRestore/executionPARTIAL vàCompare/preflightMETADATA_ONLY còn nguyên.

## 8. Integrity/provenance addendum

19filespresent; fourcorrectedsourcebytes/SHA vàfourreportedpatchSHA match. Fivebaselinecopies so vớipriorreviewmatch, BMmeta match59/59B1…, runnermetaGUID/hashgiữ242/6DF524… . Packetkhôngexternalmanifest/Ebeforeafter17fingerprints/original8manifest; provenance5rawartifactrows có placeholderthaySHA. Root đã làm phần đo/archive hữuích; khôngcầnyêuOwneruploadlại.

Allfourpatches **UTF16LE BOM FF FE**, absolutea/b paths vàfirstaddedline chứa literal `ï»¿` mojibake thay properUFEFF củaactualsource. Không nhận đây là UTF8review/apply delivery. Giữoriginals, phát hành newUTF8relativepathpatches vàcheck trongdisposablefilecopy; khôngsửaBMbytes hoặc applyE:. Metadata fixes không cầnexecution.

Source report còn ghi sai một-parameter finishing signature và field `_currentPendingLootItem`: actual method nhận expectedEncounter/transitionId, field vẫn `pendingLootItem`. Correct bằng addendum, không tạo field/API mới cho khớp report.

Actualcurrentmodifiedsource:

- BM58723 / **5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6**.
- Settings24419 / **B23C308E988D947AA477451C210AFE6D2E8F6BBD7FDF24C8FAD753E0C2377996**.
- Runner36755 / **E271A57BB9C0E8A2105EED2F0F72CCA573567DFB2C568C012A00EE50CDDD66F9**.
- Wrapper16866 / **1157A233BA354FC9EFB9CA307FE37CE100B6B063608C79F554286D691D528E51**.

Do notclaimwholeEunchanged1byte: rootcurrentlyverifiedEHEAD/status+17selectedfiles00:13:05; original8PhaseAhashes8/8preserved, oldjournalmetadata846/AF0E…preserved. No sourceapply/push observedfromthisCodexturn; no fullhistoricactor/build/savecertification.

## 9. Next concrete boundary

R2 retainsactualBMcorrective, fixesexactknownharness/wrappergaps vàmakesrequiredassertionsfail-sensitive inonefreshguardedsession. Mandatoryfivecases plusoneownedpartialsetupcleanupfaultcheck withinthesameboundedprocess; counts/mode/reacheds explicit, no >=4/5 success. Owned full-wave objects/sessioncontexts cleanup and finalterminal/guard gates required. No rerun oldPhaseAbaselinefailure/P08/P09/recovery or unchangedacceptedbroadskills.

AfterpacketCodexreview, parentcorrective mayclosewithinboundedproof; onlythenconsiderseparateLABONLYPhaseBcontinuation. No automaticStageimplementationbecauseproposalorlabapproval. Completeproductionroadmap/exposure/persistence stillnotapproved; currenttechnicaldependencyisthissafety/evidencefix, notanothergeneral8-groupaudit.

P08/P09-A/P09-B ACCEPTED/LOCKED vàUSER_VERIFIED giữscope; P08Gate2 **135/137exit1**, P09-AE2 **SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2**, PID25360 **USER_REPORTED_PASS/RESTORE_UNRESOLVED**, recovery04/10snapshot24metadata/Restoreexecution**PARTIAL**/Comparepreflight**METADATA_ONLY**. Recyclecloseouts/A10Gold-onlyLOCKED vàGoldformula preservedimplementation/TBDbalance khôngđổi; historical61/current30+30skillsno-rollout. No Owneractionrequirednow.
