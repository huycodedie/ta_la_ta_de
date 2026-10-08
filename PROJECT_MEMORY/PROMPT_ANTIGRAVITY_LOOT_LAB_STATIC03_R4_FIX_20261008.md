# PROMPT ANTIGRAVITY — F-LOOT-LAB-HARNESS-STATIC-03-R4

**Source preparation only**, hoàn tất prompt ngày **09/10/2026 UTC+07** cho packet R3-R3 ngày08/10; giữ filename08/10 để nối task history. Authority [STATIC03-R3 review](TLTD_LOOT_LAB_R3R3_STATIC_REVIEW_20261008.md), latest ACTIVE_WORK_HANDOFF/CURRENT_DESIGN_AUTHORITY. R3-R3 = PARTIAL WITH REGRESSIONS/REVISION REQUIRED/NO RUN AUTHORIZED. Hoàn tất cùng **R1-01…08, C0+A1–A5**, không case/feature mới.

## Boundary và cách sửa bắt buộc

Lab duy nhất: `C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/stage_gate_lab_20261007_204500`.

Chỉ mutable:

1. `Assets/_Game/Editor/Prototype01PlayTestRunner_StageGateLab.cs`61,102/SHA256**D0E78357E02512D999A819579E9F683C8EC641372B11E783B9C74C3B06A53505**.
2. `Tools/Verification/StageGateLab/run_stage_gate_lab_spike.ps1`24,737/**881200E23795285EC5A2A696A567E03CD4D0B0D5C77BD89EBE882D0E0D4284F9**.

Frozen BM58,723/5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6; BMmeta59/59B1AA9F84414AFC5E53F92AF5DFBE3FD4B7A2E6815F9A62980AA8D6AEA80111; settings24,410/A85CC67184A18039D0E1664D7C5625FF2A81BF550889CC1DCCCD0451AB8CC561; runnermeta242/6DF524CA04537093A4EB824E3C28972CCC21EDBF66DCF7BE969145C7E5B19943/GUID7e5ae45bd4db4a0b874c601745a96c20. All runtime/UI/helpers/meta/assets khác frozen. Mismatch: preserve+report exact bytes/hash, không rollback/recreate.

**Không Unity GUI/batchmode/compilation qua Editor, PlayMode, wrapper top-level, Guard/preflight action, live Registry/save.** Không allocate RunId/session/profile/nonce hoặc sửa settings. Offline reads/API/AST/extracted pure predicates/drycheck disposablecopies được phép; unavailable standalone C#refs => STATIC_API_REVIEW_ONLY/UNITY_COMPILATION_NOT_RUN. E:/code/TLTD read-only HEAD7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425/full workingtree/save unchanged; no pull/stage/commit/push/stash/reset/clean/source transfer.

**Không viết lại suite/framework theo APIs tưởng tượng.** Trước sửa, lập hunk plan trong fresh output: current finding → exact actual declaration → block nguồn đã đúng → intended minimal diff. Source tham chiếu là **pre-R3R3 copies** trong `work/loot_lab_harness_output_r3_r3_20261008_233000/source_copies_pre_r3_r3` (runner73,111/F5035…; wrapper29,096/92BDB…). Lấy lại các khối đã valid: imports, real BuildFixture/config/database/entity/BM init, cast helper, typed diagnostics, UI actions, PlayMode entry/reload. Pre-copy có findings riêng R3R2review, không copy lại invented wave/modal setter calls. Giữ current correct named setters/ModalRequest/direct wave capture/retained refs/final logflag check và các cải thiện hợp lệ. Sửa từng block và tự kiểm symbol/dataflow trước block kế tiếp; không blanket revert runtime hay packet.

## R1-01 — Actual API toàn bộ changed sites

Khôi phục namespaces Combat/Drop/Items/Equipment/Progression/Entities.Components đã có trong pre-copy; dùng `WuxiaGame.Inventory.Inventory` fully qualified, bỏ WuxiaGame.Systems. API table exact declarations, returntypes/defaults/types/path/line/sourcehash, không paraphrase. Bảng sửa finite:

| Sai trong R3R3 | Dùng actual frozen source |
|---|---|
| DropConfigSO/DropRate | EquipmentDropConfigSO + InitializeConfig(rate,weights); **rate percent0–100: standard100/A4zero0**, không standard1.0; DropSys.SetDropConfig(EquipmentDropConfigSO), LoadDatabasesIfMissing |
| BM.SetChannel/InitializeBattleManager | Restore real fixture registrations/config initialization/StartBattle; không có hai APIs này |
| BM.SkillCast | **ctx.Hero.CastState**, SkillExecutionRequest từ Combat |
| SkillSlotType.Primary/InterruptSource.None | **SkillSlotType.Skill / SkillCastInterruptSource.None** |
| QueuedLootDecisionsCount/PendingLootDecisionId/IsLootPending/BattleState | **PendingLootQueueCount / PendingLootItem?.InstanceId / CurrentBattleState==BattleState.LootPending / CurrentBattleState** |
| Hero.CurrentExp (int) | **owned ProgressionManager.CurrentExp (float)** |
| completion Action<string> | **Action<EquipmentInstance>**, ID lấy từ item thật |
| UnregisterModalView(string) | **UnregisterModalView(IModalView)**, truyền retained view |
| Coord.PendingRequestsCount | **TotalQueuedCount** |
| BattleState.NotStarted | actual **AwaitingPlayerStart** scope, không invented enum |

ActiveWaveMonsterConfig là **property** returning MonsterConfigSO, không type; giữ direct capture. Title12required+1optional/Tier13required/Loot9required+5optional named setter calls hiện đúng. RequestModal(ModalRequest), ActiveRequest, DismissActiveModal(reason,expectedrequest) đúng. Health.SetCurrentHealth(float hp), Resource.SetMockResources(int,int), Inventory.HasItem(string) tồn tại; không đổi các APIs frozen này. Không tạo accessor mới/privateinvoke/SetValue/globalreset.

## R1-02 — Real fixture + cleanup + exact C0 fault

Restore initialized owned HeroConfig/MonsterConfig/stats/Progression/Drop DB/transient EquipmentDropConfig, BM.RegisterHero/RegisterMonster/StartBattle và legit real death/drop path từ pre-copy. Hero/Monster **đã RequireComponent Health**: dùng `hero.Health`/`monster.Health`, không AddComponent Health thứ hai và kill secondcomponent. Register allocation/SO/subscription ngay ledger, callerctx trước setup. Spawn observer chỉ ghi actual members thuộc ownedBM/root; no ambient adoption/destroy.

C0 fault occurs during iterator MoveNext: recognize **exact injected InvalidOperationException/site/message** bằng explicit expected-fault flag, không generic catch đánh CaseFailed cho expectedfault. C0 PASS only exactexpectedfault+required root/observercleanup; unrelatedfault/missingfault/failure =>FAIL. C0 chưa allocate config, không claim configcleanup. Giữ nestedstack finally unwind nhưng **Dispose exception phải recorded CleanupFailed**, không swallow. Guarantee fixture cleanup trên valid in-process setup/MoveNext/Dispose/earlyreturn/abort/timeout paths; trailing teardown sau finally không đủ. C# yield/catch/finally hợp lệ, bounded cleanup stages failure-recorded. Forcekill thiếu cleanup=>FAIL/MISSING, không promisefinally/frame.

Retain actual core/managers/views/entities/SOs/waveclone/nextwave/listener/handle refs tới verified removal sau appropriate frame, sau đó clear bookkeeping. BM cleanup trước entity/config, captured BM.ActiveWaveMonsterConfig before destroy và afterframe verify, không double destroy. Unregister view truyền IModalView; removal exceptions ngănPASS. Stage exception không skip mọi cleanupstage sau. No global singleton/EventBus reset; typed diagnostics failclosed on missing owned instance/type/state, không silentqueue0.

## R1-03 — Driver và one terminal outcome

Single aggregate per case; PASS only reachedrequired/assertionsPASS/cleanupverified/noCaseFailed. Cleanup failure explicit kể cả NOT_REACHED/SETUP_BLOCKED. Stop after firstmandatoryfailure, missing case receipts truthful.

Giữ named post-destruction observer, hoàn tất verified immutable ownercontext/static task-state/watchers/globalledgers+retaineddriver reference. Common success/failure driver path: gameplayFAIL không tự gán driverFAIL; measure actualcleanup separately. Detach observer/clear ownedstaticstate boundedly trước terminal suite. Callback không Exit/isPlaying mutation chỉ vì batch=true; validate exactownedcontext. Cả driver/suite có agreedschema/proof fields. No extra script/asset/runtime API.

Rows fixed schema/context(run,nonce,contract/receiptidentity)/case/reached/assertions/cleanup/outcome; exactly one per C0/A1…A5/driver/suite. Intermediate PASS prose không terminalevidence.

## R1-04 — Restore actual UI path và existing oracles

Tách/Equip primary decisions **gọi ctx.LootUI.OnDismantleClicked()/OnEquipClicked()** như pre-copy. Public callbacks returnvoid: verify result bằng same item/request/completed/economy/modal ledger, không giả returnbool. UI completes captured active modal; directBM call không làm bước này. Primary transaction không tự gọi BM thayUI. Chỉ **immediate duplicate** gọi BM supported API trong alreadyclosedwindow để assertfalse/noeffects.

Shared owned ordered ledger run/case/frame/time/castrequest/dropqueuependingmodalitem IDs/counters/actualindex. Same Hero.CastState request, successful StartHeroChannel return, SkillDef.SetChannel, duringfinishing membership, bounded natural Recovery/IsFinished/!active/None/elapsed+ticks/deadline; timeout không naturalPASS. Actual EncounterIndex observedincrements sau modaldrain exactlyone, không token/stateevent count hay immediateendpoint assertion.

- **A1:** legit kill during finishing, same real drop/queue1/nonemptyID/pendingnull/openfalse; naturalfinish→same pendingID/queue0/requestonce/open. Inventory contains before UI Tách; goldonly/material+EXP unchanged from postdeath baseline/completedIDonce/itemremoved/modaldrain/boundedoneadvance. No OR IDchain.
- **A2:** restore established order: **first legitimate non-final monster death → queue1/retained firstID → successful retained Hero channel before second/final monster death → queue2/FIFO/natural finish**. Không đổi thành channel trước cả hai deaths. Gate firstrequest+secondblocked chain. First UI Tách; **ngay sau firstTách** pendingnull/openfalse, duplicateBMcall cùng stack trước yield returnsfalse; no extra request/completion/Gold/Material/EXP/inventory/encounter effects. Đừng chuyển duplicate sau secondEquip. Second request/ID/order, UI Equip/slot/ID/membership/economy; finaldrain/oneadvance. Snapshots sau legitimate deaths, không fakeitem/queue/event.
- **A3:** namedsetter/retained A/Brequests/panels; strict successful naturalcast trước defer boundary; same queueditem/no request/indexhold/economy. Dismiss A thenB bằng exactrequests; synchronous active+queueclear before yield/lootacquire; exactlootrequest/completedonce/UI Tách/economy/drain/advance.
- **A4:** actual zero-drop transientconfig và **OnEquipmentDropped observer**, không chỉ lootrequestcounter; retain tới terminal. Strictnatural/no-loot deferredboundary/blockeridentity/visibility, queue0/pendingnull/openfalse/request0, indexheld beforeclear/exactoneafter. NoRNGretry.
- **A5:** bounded strictnaturalfinish before preconditions; aliveHero/exactblocker/samevalidqueueditem/LootPending/pendingnull/openfalse/request0/retaineddeferhandle observed. Missing=>NOT_REACHED. Legit Hero.Health death/actualdeath-event, typed token invalidation+owneddefercancel; restorethese gates lostR3R3. Gold/Material/floatEXP/inventory baseline aftermonsterdeath immediatelybeforeHeroDeath; no newrequest/completion/effect/advance afterrelease, exactAwaitingPlayerStart/clearedqueuepending. Nonnullhandle diagnostic notschedulerproof.

No fullSkillExecutor/dash/projectile/Stage/newcase scope.

## R1-05 — Enforce unauthorized descriptor; owned PlayMode entry/reload

Current descriptor **RunAuthorized=false** và sourcepreparationonly. Wrapper phải **refuse nó trước GUID/output/Backup/Unity**, không loaderrootmatch là đủ và không fallbackauthorize. Prepare strict loader/interface for **future separate approved run mapping**: canonical lab/data/output/backup/profile, explicitRunId/freshness, expectedcontract/payloadidentity/editor6000.6.0f1 và noncepolicy. No autoRunId/unrevieweddefaultoldprofile/UNSPECIFIEDhash/nonemptycontext acceptance. Futurewrapper nonce generateonce chỉ sau permissiongate theo explicitpolicy, bind receipt **trước Backup**, extend with measuredjournalidentity; settings/currentprofile frozenbâygiờ.

Restore owned static executeMethod entry → validatedbatchcontext/sceneinventory → ownedPlayModetransition → EnteredPlayMode/reloadvalidated immutable receipt/CLI/SessionState → drivercoroutine. Không automatic InitializeOnLoad entry hoặc menu path tự setup; missing/stale/unowned/duplicate/partialcontext detach namedhooks/clearonlytaskstate/refuse without scene/Exit/PlayModemutation. Gate at initial/reload/suite/finalcallback, not batch/company only. Retain finite include-inactive relevantobjects+harness inventory acrossloadedscenes and playing/transition refusal, no nameownership/lazysingletonadoption. No session/settingsallocation now; code only.

## R1-06 — Correct actual journals và prewrite paths

Restore reviewed output boundary **ngoài lab trong workspace**, futureexplicitfreshoutput/run/backup mapping; rejectreuse/existingdirs, no fixed nestedlab/work packetlikeoutput. Canonical/kind/existence/reparse checks finite lab/6targets/contract/output/backup/journals và all actualancestors **trước write/import/invoke**. Settings unique scalar company/product matches reviewedmapping; editorbinary existence/version6000.6.0f1 preparedgate, no runnow.

Historicaljournal FILES lấy canonical actualcontract/reviewpaths: PhaseA lab/scratch journal, corrective **workspace/work/loot_channel…** journal, R2 **workspace/work/loot_lab_harness_output_r2…** journal; không ghép hai paths latter dướilab. Ba status hiện tại **VERIFIED**, không BACKED_UP. Restore unchanged trusted P09Bpreflight semantics: unresolvedBACKED_UP/RESTORE_IN_PROGRESS/RESTORE_FAILED BLOCK, completedVERIFIED permit. Known Ownerlauncher file mustexist/parseable/VERIFIED metadata; missing/parse/statusnotPASSwarning. Intentional BACKED_UP rollbackcheckpoint riêng, không recovery/clear/reclassify.

Freshjournal validator dùng **actual GuardG306–315 fields**: Status/RegSubKey/FullRegPath/OriginallyExisted/ValueCount/Snapshot/BackupFile/BackupHash/Timestamp. **Không có Action hay SchemaVersion field** trong currentjournal; không sửa Guard/schema để khớp inventedfields. Validate exact ownedcanonicalfreshpath/type/status/coherentexisting-orabsentbaseline, actualHKEY_CURRENT_USER scopeANDsubkey, existingbackupfilefreshownedhash. Bind measuredjournalidentity in externalreceipt, không inventjournalcontextfields. BeforeUnity and immediatelybeforerecovery; partial/unowned/mismatchpreserve+block. No liveRegistry/Ownerrecoverytest.

## R1-07 — Captured tree, successful empty inspection, isolated closeout

Giữ capturedrootProcess/PID/StartTimeonce; retain/revalidate finiteowneddescendants PID/start/parent/path trước stop và boundedterminalconfirm. No PIDonlykill/ambientUnitykill/suppressCIMunknown. Successful empty processenumeration phải khác inspectionfailure; **Get-Process -Name Unity -ErrorAction Stop** coi no-match làerror, **SilentlyContinue** có thể hideunknown. Use successful enumeration +filter/explicitfailure record ở prelaunch/pre-postrecovery. Unknown/nonterminal/conflict=>preservejournal/skipbothRestoreCompare/nonzero. No AllowRunningUnity/Ownerkill.

Protect independently terminalinspection/recovery/Compare/metadata/evidence/postpreservation/finalreceipt stages; recordallfailures không exception mấtcloseout. Compare chỉ eligible sau successfulRestore, không fabricateComparePASS khiRestorefailed. ActualOSexit orUNAVAILABLE riêng wrapperoutcome. Forcekillkhôngruntimecleanupguarantee.

Requiredprelaunchevidencefailure blockdependentlaunch; laterfailureblockPASS while safecleanupstillreached. **Giữ W558final-logflag recheck đã đúng**, không reintroduce finalLogthenexit0bug. No retry/overwriteearlyraw.

## R1-08 — Actual 21-row trust/pre-post/preservation/allrecords

Requiredexternalreviewed exact21mapping, uniqueallowlistedrelativepaths/canonicalcontainment/types/bytesSHA; forbidmissing/invalidcontractfallback và stale selfSHA4CBF…. Persist expected+actualpre/postmatrix; any helper/payload/context/evidence mismatch **stop before Backup/launch**, not finalPASSonly. Wrapperactualhash externalnonrecursive, no selfapproval.

Read-only E approvedHEAD/fullporcelain--untracked-files=all/17fingerprints và fixedhistoricalPhaseA/corrective/R2/R3/R3R1/R3R2/R3R3sets expected/pre/post comparison; checkalltoolexits/missing/mismatch andpersist receipts used inPASS. SuccessfulGitexit/HEADonlykhôngpreservation. No wholeuntracked/saveclaim.

NormalizeCRLF, parse **all** terminalrows with strictschema/literalRunId/nonce/contract/receiptmatch, totalexactlyone per expectedID, requiredreached/assertions/cleanup/status/order. Rejectunknown/malformed/stale/duplicate/surplus/contradictoryrecords, no successprefixcount-only. Validatedsuite derives ScenarioResult, no firstunboundmatch. Driververified beforeonesuite. PASS only all context/trust/preflight/Backup/preservation/requiredcases+cleanup/driver/ownedterminal/actualOS0/!timeout/RestoreCompare/required evidence passed. No bonusruntimeattempt.

## Return static packet và STOP

Fresh output ngoàilab: hunkplan, pre/post6copies, exactlytwoUTF8relativepatches, reviewedcontextdescriptor **RunAuthorized=false** (no session allocation), exactsymbolreceipt/sourceflowmap, checklistR1-01…08 per actualchangedlines+namedproof, receipts+manifest. Preserve allold108packetfiles/raw/journals/archives byte-identical, noZIP. Unresolved ghiOPEN, không claim100%fromcomments/logstrings.

OfflineAST exactwrapperpath/bytesSHA/command/errors; drycheck exactdisposablecopies/Gitversion/command/separateoutputs/exit/unchangedpostcheckhash. Safeextractedpurecontext/journal/resultpredicates with exactsource/hash and existingnegative inputs unauthorizedfalse/missingcontext/nonce mismatch/unresolvedhistorical/wrongsubkey/malformed-stale-PASS+FAIL/surplus rows; no wrappertop-level/sharedhelpers/Unity. Nếu correctstandalonecompilerrefs available thì actualcommand/refs/diagnostics/exit; otherwise compilationNOTRUN, no fabricatedPASS. Source-wide changedsymbol audit actualreturn/types/enum/delegates beforehandoff.

Giữ baseline15table/contract21fingerprints đã đúng; sửa actualreport7wrongAPIrows/copied-exactpathsmetadata/overclaims, UTF8mojibake/C1controls bằng readable newaddendum, perfileBOM/CR/control scope truth. Không normalizefrozenfiles, không inventCRLFcause/oldraw/executorproof. Allmetadata mustrefer actualpacketversion.

**STOP sau source/diff/static bàn giao cho Codex.** Unity compilation NOT RUN / C0+A1–A5 NOT EXECUTED / Phase B STOPPED. Parent BM accepted with limits / overall pending; P08/P09-A/P09-B ACCEPTED / LOCKED, recycle CLOSED / FIX VERIFIED, recovery CLOSED / RECOVERY ACCEPTED và raw limits giữ đúng phạm vi cũ. No StageBossproduction/exposure/persistence/schema/skillactivation/sourcepush/productapproval, Ownerplay/retest/recoveryrerun/ZIP. Futureexecution chỉ sau sourceacceptance và **prompt phiên riêng**.
