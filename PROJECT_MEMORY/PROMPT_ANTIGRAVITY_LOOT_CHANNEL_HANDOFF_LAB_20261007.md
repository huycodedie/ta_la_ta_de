# PROMPT ANTIGRAVITY — LOOT CHANNEL HANDOFF CORRECTIVE, LAB ONLY

Task: **F-LOOT-CHANNEL-HANDOFF-LAB-01**. Ngày07/10/2026, UTC+07.

Authority/review: [TLTD_STAGE_GATE_PHASE_A_REVIEW_20261007.md](TLTD_STAGE_GATE_PHASE_A_REVIEW_20261007.md). Đọc review trước. Đây là scoped technical lab corrective của Codex, không product feature approval hoặc P08/P09 reopening.

**AUTHORIZED:** chỉ sửa lab allowlist bên dưới; sửa runner/wrapper safety trước bất kỳ launch mới, sửa first-loot handoff của lab BattleManager, rồi một bounded guarded corrective session. **Phase B gate integration vẫn STOPPED**, cần Codex review/accept packet và giao continuation. PASS corrective không tự cho phép Phase B, apply E:, push source hoặc Stage/Boss activation.

## 0. Baseline, ownership và preserved evidence

Exact lab root:

`C:\Users\conca\Documents\Codex\2026-10-05\b-n-ti-p-qu-n\work\stage_gate_lab_20261007_204500`

Original output root, chỉ đọc/giữ nguyên:

`C:\Users\conca\Documents\Codex\2026-10-05\b-n-ti-p-qu-n\work\stage_gate_lab_output_20261007_204500`

Đọc AI_RULES/CONTINUITY_AND_SYNC_AUTHORITY/ACTIVE_WORK_HANDOFF/CURRENT_DESIGN_AUTHORITY từ canonical main snapshot riêng. Resolve refs lúc bắt đầu; pre-publication refs của review memory6e8d31cb4a6e1fbbb13f88fe2f5bc1ec4c1f5310/game03a1744620236c996f22eb6d15615e51fd40f595; local E:HEAD7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425/source393a048. Prompt/review sẽ có publication commits riêng.

Trước edit, preserve byte copies BM/runner/meta/wrapper/settings và baseline source fingerprint file vào **new external evidence directory** `...\work\loot_channel_handoff_lab_output_<runid>`. Lưu baseline eight-original-artifact manifest và final historical lab journal metadata; không overwrite shared historical backup hoặc first/final raw logs. Không phục dựng raw guard đã thiếu hoặc giả journal first attempt. Không rerun lỗi baseline chỉ để lấy clean provenance. Compile12100/NOT_REACHED và final24764/FAIL là lịch sử giữ riêng.

Read-only E: HEAD/porcelain `--untracked-files=all`, 17 selected hashes trước/sau, cùng exact12 lab-runtime baseline, guard/helper fingerprints. Compare against review/R1; mismatch dừng, không copy broad dirty E: để chữa. Snapshot21:20:52+07 có3trackedmods+530untracked. Ba dirty docs giữ09C343…/842B4F…/E9143A…; không overwrite để sync. No E: pull/stage/reset/stash/clean/checkout/push/source/memory/save mutation.

Exact changed source allowlist, **chỉ tại lab root**:

- `Assets/_Game/Core/BattleManager.cs` — bounded first-loot handoff correction.
- `Assets/_Game/Editor/Prototype01PlayTestRunner_StageGateLab.cs` + existing `.meta` — guarded CLI entry/private suite, ownership/cleanup, scoped cases/telemetry. Preserve existing GUID, không meta churn.
- `Tools/Verification/StageGateLab/run_stage_gate_lab_spike.ps1` — strict safety, fresh run identity/evidence, result/cleanup exit gates.
- `ProjectSettings/ProjectSettings.asset` — chỉ company/product unique corrective lab profile trước launch.

Artifacts/patches/manifest nằm ngoài source allowlist tại fresh output. Unity generated lab cache/logs được ghi bởi Unity, không copy từ E: hoặc stage/publish. Không new Stage policy/hooks/Bootstrap/HUD, scene/prefab/assets saved; không sửa LootDecisionUI/ModalCoordinator/Resource/Inventory/Equipment/Progression/Drop/Skill sources, old runner/shared guard/core/P09B helper/Packages. Setup configs/UI/managers chỉ owned transient RAM objects. Nếu cần path khác, hoàn tất preparation hiện có và báo exact blocker cho Codex trước edit.

## 1. Safety prerequisite — wrapper trước launch

Không chạy nguyên wrapper hiện tại. Sửa các điểm sau trước Unity:

1. `ErrorActionPreference=Stop`, explicit validation/error cleanup. Resolve lab root, Assets, output/backup absolute paths; so exact expected root và containment. Reject E: hoặc arbitrary/nested/symlink target sai scope. Validate parsed ProjectSettings company/product và expected key **trước Unity import/compile**. Runtime runner recheck không thay wrapper preflight.
2. Dùng company `TLTDLab`, product mới duy nhất `StageGateFix_<runid>` tại exact lab; preserve old settings fingerprint. Exact `HKCU\Software\Unity\UnityEditor\TLTDLab\StageGateFix_<runid>` phải khớp actual company/product. Cùng một expected context truyền vào wrapper/runner; không hardcode key cũ, prefix/substring acceptance hoặc fallback defaults. Không dùng Stage-key prefix như save isolation.
3. **Bỏ mọi AllowRunningUnity và mọi branch bypass.** Refuse global other-Unity conflict, không đóng/kill Owner editor. Reuse unchanged P09 Guard và strict P09B unresolved-journal preflight policy theo actual API; old relevant journal đọc metadata/output only, không restore/delete/overwrite. Every CheckInterruptedJournal/Backup/Restore/Compare invocation explicit **CustomRegKey exact + BackupDir fresh absolute per attempt**; signatures audited, không default Owner key/E:backup.
4. Preserve full stdout/stderr, command args, exit/result cho từng preflight/Backup/Restore/Compare child call vào fresh raw guard logs; success code phải kiểm tra, không chỉ tự viết “SUCCESS”. Save journal metadata trước/sau, không publish Registry payload. Nếu old relevant journal unresolved/conflict, stop trước launch; trả exact reason, không override/recover Owner state.
5. So sánh **12 runtime baseline expected hashes**, reused guard/core/P09B helper hashes và current exact allowlist. Before patch12match review; prelaunch BM phải khớp reviewed local corrective payload,11 runtime còn lại nguyên baseline. Prefix/name-only/log3hash không phải verification. Settings ngoài company/product unchanged. Record expected/actual matrix và fail mismatch.
6. One owned hidden Unity process, cap **300s gồm import/compile**; actual PID/UTC+07 start/end/elapsed numeric/OS exit riêng. Nếu Start-Process dùng WindowStyleHidden. Watchdog chỉ kill captured owned PID tree, **WaitForExit/verify terminal process trước restore**, không broadkill. Không `-quit` làm scenario kết thúc trước proof.
7. Guard cleanup trong finally sau owned process terminal. Check safe conflict trước restore; nếu otherUnity xuất hiện, **không bypass**, giữ unresolved fresh journal/raw và báo RECOVERY_BLOCKED; không gọi đó là PASS hoặc tự sửa Owner recovery. Restore/Compare failure phải làm wrapper nonzero, dù scenarioPASS/Unityexit0. Identity/source/preservation mismatch, scenario NOT_REACHED/TIMEOUT/FAIL và missing harness cleanup cũng chặn success. Log scenario, process, wrapper, guard/cleanup riêng; không fake exit0.

Không cần launch safety-negative sessions hoặc full guard suite. Static safety review + actual successful-path evidence và declared negative/exception limits đủ cho packet; nếu execution bị chặn, trả complete diff/preparation và blocker.

## 2. Safety prerequisite — runner/fixture ownership

Một **public static CLI method** cho Unity `-executeMethod`, guard batch/context trước mutation theo safe runner pattern hiện có; suite/setup/helpers/coroutines private và không có MenuItem/public suite/coroutine bypass. Reject interactive, `isPlayingOrWillChangePlaymode`, wrong exact company/product/dataPath và any unowned relevant manager/singleton **trước NewScene/session mutation**. Controlled post-domain-reload/EnteredPlayMode continuation phải revalidate batch, exact expected context và owner token; detach watcher/event subscriptions thuộc task khi complete/fail. SessionState không tự cho phép chạy một arbitrary stale session. Không generic public component coroutine có thể chạy fixture ngoài entry.

Một transient lab scene và owned roots/configs/UI/managers; validate setup order để BM.Awake không disable một monster rồi fixture giết inactive object. Monster active/registered member, Hero alive và request active là assertions. Instantiate đủ actual owned Inventory/Resource/Equipment/Progression khi case cần real effect, từ baseline APIs; refuse unowned instance, không reuse hoặc destroy external object. Gold/save effects chỉ được phép trong profile lab đã guard.

**Không ModalCoordinator.ResetInstance, EventBus.ResetAll/global shutdown hoặc FindAll destroy.** Setup catch + teardown finally phải quản lý partial objects ngay sau tạo, unsubscribe own observers/callbacks, stop owned coroutines, cancel/clear fixture lifecycle bằng existing owned APIs khi cần teardown, release only owned modals, dispose ScriptableObject configs, destroy owned roots. Assert remaining owned counts/subscriptions và log cleanup before leaving PlayMode/EditorExit. Engine exit/destruction không là explicit ownership proof. Exceptions/timeouts vẫn vào controlled teardown; nếu watchdog kill mất proof, ghi cleanup NOT_VERIFIED/FAIL, không all-exception claim.

Use deterministic **owned in-memory config** để real DropSystem generate desired item counts/no-drop, không sửa/load-mutate authored assets. Không giả loot queue/pending/flag, direct event death, fake modal booleans hoặc cancel caster để ép PASS. Reflection đọc telemetry/setup-owned private references được ghi rõ; không reflection-write state oracle.

## 3. Runtime correction — one canonical first-loot handoff

Baseline BM58100/hash3A021DE…; narrow failure already confirmed. Actual branch `WaitForFinishingExecutionsThenProceed`1289–1297 bypasses `TryPresentNextQueuedLoot`874–890; PlayMode completion1052 requires openflag. **Không nới completion/pending-validity guards**. Existing API chỉ nhận equip/dismantle bool, không nhận caller item/InstanceId; không thêm API để sửa một stale-ID contract ngoài scope.

Trong finishing-complete branch có queued loot:

- Giữ existing finishing request cleanup/valid expected encounter/transition/Hero checks, IsBattleActivefalse, enter/notify LootPending.
- **Bỏ direct dequeue/direct RaiseLootDecisionRequested** ở branch này. Dùng existing canonical presentation để pending/openflag/request cùng invariant.
- Khi active blocking hoặc queued real modals còn, giữ item trong queue và schedule existing `DeferNextLootDecisionRequest` với **correct captured EncounterIndex + lootTransactionCounter** theo existing first/next-loot pattern. Khi modal clear mới canonical TryPresent. Có wait/wake owner và token revalidation; chỉ TryPresentfalse rồi return không đạt.
- Không reset counters; **không dùng encounterTransitionCounter thay lootTransactionCounter**; không manual-openflag hoặc fake modal clearing. Không change cancellation/natural channel snapshots, no-loot branch, completionclose-beforeeffects, reward/formula, sequential decision lifecycle hoặc once-per-completed-wave growth.

Ưu tiên bounded branch patch/reuse pattern; không broad helper/framework/state-machine refactor. Nếu factor nhỏ để chia sẻ first-loot handoff, giới hạn đúng hai related branches và bắt buộc nonfinishing control. No Stage policy/hook/persistence/production exposure.

## 4. Một targeted guarded natural-frame session

Chỉ chạy sau preparation/safety/allowlist assertions đạt. Một fresh session cap300s; tất cả cases trong cùng scoped run, mỗi case cleanup rồi next case. Stop trước Phase B. Không rerun master P08/P09 hoặc baseline failure chỉ để closure. Compile/import/SETUP_BLOCKED/NOT_REACHED/TIMEOUT không PASS; failed attempt raw/journal phải preserve, không tự loop launch. Nếu cần lần tiếp theo, trả exact fix/blocker cho Codex review trước.

Cases phải thật sự executed; tên/expected không thay actual assertions:

1. **First finishing loot:** active Hero channel request còn sống tại legitimate final registered monster death; actual drop queued; natural frames tới successful completion. Owned Inventory/Resource và các managers cần đo phải tồn tại trước Drop generation. Before first request valid InstanceId/openflagtrue/LootPending; real functional UI Tách accepted; actual Resource Gold increment đúng existing formula **một lần**, inventory remove đúng item, no EXP/Material recycle effect. Snapshot EXP/Material **sau death rewards, ngay trước action** để đo riêng recycle delta, không nhầm EXP do death là EXP do Tách. Actual modal release/drain và exactly one observed next encounter advance. Direct handler là functional UI proof; không claim Owner pointer/visual test.
2. **Sequential two drops:** cùng legitimate wave tạo ít nhất2actualdistinctinstances; final death có finishing request. Capture request/completed-event/effect counts và correlate InstanceIds. First decision Tách, second Equip hoặc Tách qua real handler; second request chỉ sau actual first-modal release, no advance khi displayed/queued item chưa drain; cuối cùng one advance. Trong **closed inter-item window trước next request**, một immediate duplicate `CompleteLootDecisionAndResume` attempt phải trả false/no second effect; ghi đây là API guard check riêng. Sau next item presented, API hiện có targets current item, nên không coi second valid action là old-ID duplicate và không claim stale-ID protection. Không gọi direct ResourceManager.DismantleEquipment(oldItem) làm oracle chống duplicate hoặc sửa Resource API. Close-window-beforeeffects và Gold-only preserved.
3. **Active/queued modal handoff:** dùng actual owned ModalCoordinator/modal registrations đúng runtime API, blockers có real active/queued lifecycle và **khác ModalId** để không coalesce thành một request. Có thể dùng transient existing TitleBreakthroughUI/LootTierProgressionUI APIs, không sửa source UI hoặc bắt buộc tạo progression product feature. Channel complete khi blocker còn: no dequeue/open/present first loot trước release. Release từng owned modal theo real completion flow/natural frames; request wake đúng thứ tự sau totalactive/queuedclear, transaction succeeds. No dummy booleans/modal coordinator reflection repair.
4. **Natural no-loot control:** no-dropconfig, final legitimate death với request stillactive; **assert actual generated-drop events0 và queue/pending0**, không coi rate0 tự động chứng minh no-drop vì baseline dùng roll<=rate. Seeded fixture nếu dùng phải ghi scope/state và actual outcome, không đổi Drop source hoặc retry loop để chọn PASS. Natural completion; existing modal delay respected và exactly one advance, no fake loot. Nếu shared first-loot helper touched, thêm bounded **nonfinishing first-loot** control trong cùng session.
5. **Deferred cancellation boundary:** khi first-loot wait còn active, legitimate Hero death và existing encounter/lifecycle cancellation route phải chặn stale request/advance sau modal release. Dùng public/actual lifecycle APIs; no reflection counterwrites. Nếu separate stale token route không thể reached trong bounds, ghi SOURCE_REVIEWED/NOT_EXECUTED rõ, không fabricate PASS. Không thiết kế StagePending/Hero-final/retry policy mới.

Log request stable identity/start frame/time, phase/end/interrupt reason/ticks, finishing count, active Hero/member checks, death/drop/request/complete/modal events, expected/current encounter/wave và **hai token loại riêng**, queue/pending/openflag/active+queuedmodal, Gold/EXP/inventory/equip effect before/after, action result hoặc exact observable inference, advance count/frame ordering. Inventory/equip assertions theo InstanceId/slot; nếu swap thì old item được trả vào Inventory theo baseline, không dùng total count giảm làm oracle sai. Channel có thể setup qua real StartChannel rồi Entity.Update, nhưng declare **channel-state lifecycle**, không full SkillExecutor/Rage/CD/Dash/projectile proof. Never manual Tick/Complete/Interrupt để giả natural success.

PASS cần reached paths + actual effects/order/counts + explicit teardown + raw guard final scope verified. UI hidden/pendingnull/logmarker một mình không đủ. Không “100% mọi caster”, forever/crash, exactly-once durable save, full live Owner recovery hoặc absolute preservation claims.

## 5. Deliverables và stop

Fresh output, không ZIP:

- `CORRECTIVE_REPORT.md`: safety/runtime status riêng, case actual outcome/mode/limits, failed/nonreached routes, no PhaseB, go/no-go cho Codex review. Correct report PID/hash/ref metadata bằng addendum; original reports/logs không chỉnh.
- `SOURCE_BASELINE_AND_DIFF.md` + UTF-8 patches tách safety runner/wrapper/profile và bounded BM correction; exact changed files/metas/GUID/bytes/SHA, expected vs actual12runtime/guards/settings, pristine byte copies. No E: patch apply/source push.
- `CASE_RESULTS.md` và frame/event telemetry, raw Unity/wrapper/each guard/preflight stdout+stderr, exact PID/commands/time/elapsed/OS+wrapperexit/scenario, explicit cleanup record.
- Fresh lab journal metadata/guard args/key/profile before/after; **không Owner Registry payload/save export**. Historical shared journal/raw remain unchanged; new attempt fresh directory, không overwrite.
- `PROVENANCE.md`, E: before/after inventories/17fingerprints, preserved eight-artifact comparison, runtime identity and dependencies; external `MANIFEST_SHA256.txt` bao các artifacts/patches/source copies, không self-hash recursion. Separate measured/source-reviewed/reported/missing proof.

Không commit/push source/patch/assets hoặc tự sync dirty E: memory. Codex tiếp nhận packet, review rồi mới giao next action. Nếu safety/source mismatch/Unity conflict, hoàn tất preparation không bị chặn và chỉ báo exact blocker; không hỏi Owner lặp old acceptance/test/ZIP.

P08/P09-A/P09-B ACCEPTED/LOCKED, recycle corrective closeouts, P08Gate2 135/137exit1, P09-AE2SCENARIO_PASS_EXIT_TIMEOUT/exit2, PID25360USER_REPORTED_PASS/RESTORE_UNRESOLVED, old recovery rawRestore/executionPARTIAL/Compare-preflightMETADATA_ONLY giữ nguyên. No Boss/Stageproduction/persistence/Material/Bun/offline/Companion/skills rollout/balance/UIpolish.
