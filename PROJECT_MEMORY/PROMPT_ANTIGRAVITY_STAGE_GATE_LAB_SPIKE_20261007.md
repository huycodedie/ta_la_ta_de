# PROMPT ANTIGRAVITY — STAGE GATE INTEGRATION, DISPOSABLE LAB ONLY

Task: **SPIKE-STAGE-GATE-LAB-01**. Ngày07/10/2026, UTC+07.

Authority: Codex Tech Lead technical scope trong [TLTD_STAGE_GATE_R1_REVIEW_AND_LAB_SCOPE_20261007.md](TLTD_STAGE_GATE_R1_REVIEW_AND_LAB_SCOPE_20261007.md).

**AUTHORIZED LAB ONLY:** tạo một bản sao Unity dùng riêng cho lab, sửa đúng allowlist tại lab và chạy các probe có giới hạn. **Không ghi vào E:\code\TLTD, không chuyển source trở lại project chính, không push source, không bật Stage/Boss trong sản phẩm.** Stage chỉ giữ trong RAM. Production implementation/exposure và persistence NOT APPROVED.

## 0. Đọc authority, xác nhận baseline và tạo lab

Đọc review/scope trên và AI_RULES, CONTINUITY_AND_SYNC_AUTHORITY, ACTIVE_WORK_HANDOFF, CURRENT_DESIGN_AUTHORITY từ canonical main snapshot tách biệt. Giữ nguyên R1 tại `E:\code\TLTD\scratch\stage_gate_scope_spec_r1_20261007_195000` và packet đợt đầu. Addendum của Codex thay các claim fixture100%, all14routes và fullD18.23. Không sửa originals để giả toàn bộ findings đã resolved; không thêm một vòng spec revision chung.

Pre-publication refs: memoryb6d5d0ee01aca8c624a08c083253d41cc60ccb56; game main/E:7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425; source393a0487083fd1e311db28cdf07393427769a4de với hai docs commits1410/7441 kế tiếp. Resolve main lúc bắt đầu vì review/prompt có publication commits riêng. Ghi commit và file bytes/hash riêng.

Tạo directory mới ngoài `E:\code\TLTD`, ví dụ `C:\Users\conca\Documents\Codex\2026-10-05\b-n-ti-p-qu-n\work\stage_gate_lab_<runid>`. Resolve absolute path và clone tracked Assets/Packages/ProjectSettings từ exact verified game commit. So sánh 12 runtime hashes trong R1 với E: trước khi sửa lab; mismatch ảnh hưởng scenario phải báo và dừng, không copy dirty working tree rộng để làm khớp.

Không copy Library/Temp/Logs/UserSettings/save/Registry dumps của E:. Không dùng project subfolder của E: làm lab Unity writable. Không pull/stage/reset/stash/clean/checkout/push hoặc overwrite memory tại E:. Đo read-only HEAD/status và runtime/ba dirty-doc hashes trước/sau; giữ MANUAL_P09A_CHECKLIST09C343…, local ACTIVE842B4F…/6358bytes, REVIEW_RESPONSE E9143A….

## 1. Cô lập PlayerPrefs trước lần Unity launch đầu tiên

Project folder riêng vẫn dùng profile thật nếu ProjectSettings giữ `DefaultCompany/TLTD`. Prefix `TLTD_Fixture_*` chỉ cô lập Stage keys, không cô lập EXP/Gold. Existing SaveGuard còn mặc định real key và backup E:.

Trước bất kỳ Unity launch/import/compile nào tại lab:

1. Sửa **ProjectSettings của lab** chỉ companyName/productName thành identity duy nhất theo RunID, ví dụ `TLTDLab` / `StageGate_<runid>`. Không trùng profile thật hoặc lab cũ. Ghi exact key mapping và hash settings trước/sau.
2. Reuse unmodified `Tools/Verification/P09/tltd_save_guard.ps1` và helper preflight P09B phù hợp; dùng `P09_VerificationCore.ps1` làm pattern/compatible API khi phù hợp, không sửa shared modules. Mọi CheckInterruptedJournal/Backup/Restore/Compare call phải explicit `-CustomRegKey <exact matching lab key>` và `-BackupDir <fresh absolute lab backup directory>`. Kiểm tra signatures/args; không để bất kỳ nhánh fallback về default key/backup E:.
3. Preflight journals có liên quan trước launch theo existing policy và giữ raw output. Old owner journal chỉ đọc metadata nếu policy yêu cầu; không recovery/restore/delete/overwrite nó. Fresh lab backup/journal phải khớp profile lab. Không export Owner Registry/save dumps.
4. Không bypass global Unity-process conflict bằng AllowRunningUnity, không kill hoặc đóng Owner Editor. Nếu bị chặn, hoàn tất preparation không phụ thuộc rồi trả exact blocker; không launch Unity.
5. Wrapper xác nhận resolved projectPath là lab, identity và guard args trước launch. Runner ghi Application.companyName/productName/dataPath/isBatchMode và refuse mismatch trước fixture mutation. Entry chỉ batch, không menu hoặc public suite bypass; entry không được chạy tùy tiện trong PlayMode.

Existing EXP/Gold APIs có thể ghi keys hiện hành **trong profile lab**. Guard bảo vệ và khôi phục baseline lab. Diff0 là proof của lab profile, không live Owner-save comparison, historical recovery proof hay bảo đảm mọi exception. Chỉ ghi isolation verified sau khi có actual identity/args/raw cleanup evidence.

## 2. Exact changed-path allowlist — chỉ bản sao lab

Existing lab files được sửa:

- `Assets/_Game/Core/BattleManager.cs`
- `Assets/_Game/Core/GameBootstrap.cs`
- `Assets/_Game/UI/BattleHUD.cs`
- `ProjectSettings/ProjectSettings.asset` — chỉ company/product lab

New lab files:

- `Assets/_Game/Development/StageGateLabPolicy.cs` + `.meta`; `Assets/_Game/Development.meta` chỉ nếu folder mới.
- `Assets/_Game/Editor/Prototype01PlayTestRunner_StageGateLab.cs` + `.meta`.
- `Tools/Verification/StageGateLab/run_stage_gate_lab_spike.ps1`.

Tạo configs, transient scene/canvas và real LootDecisionUI/ModalCoordinator trong RAM, chỉ owned roots; không lưu scene/prefab/Stage assets. Không StageDefinitionSO, production scene/builder wiring, Packages upgrade hoặc UI polish. Generated Library/Temp/logs là lab artifacts, không source patch paths.

Không sửa ProgressionManager, ResourceManager, Inventory, EquipmentManager, DropSystem, SkillExecutor, ModalCoordinator, old runners/guards hoặc skill assets. Nếu cần path ngoài allowlist, báo exact need trước edit; không tự mở rộng.

## 3. Phase A — baseline natural channel → first loot, trước gate patch

Viết batch runner/wrapper/profile cho **SG-LAB-00**. BattleManager/GameBootstrap/BattleHUD phải còn exact baseline; chưa có gate hooks hoặc policy attached. Hash trước launch chứng minh điều này.

Static hazard cần probe:

- BM WaitForFinishingExecutionsThenProceed1289–1297 tự dequeue và raise request, không set `_isLootDecisionOpen`.
- CompleteLootDecisionAndResume1052 trong PlayMode yêu cầu cờtrue; TryPresentNextQueuedLoot887 mới set cờ. LootDecisionUI request handler268 không set cờ BM.

Setup một actual cast/channel request trên entity thuộc encounter; final legitimate Monster death qua HealthComponent/EventBus khi request còn active; ít nhất một actual drop được sinh vào queue. Dùng real owned loot view và ModalCoordinator, đợi natural frames tới first loot request, thực hiện action qua cùng functional decision flow. Quan sát result, queue/modal drain và next wave.

Không reflection-set open flag/pending item/finishing state để chữa oracle; không manual-present loot, dummy modal hoặc fake cast completion để bypass nhánh. Reflection đọc telemetry hoặc setup configs phải ghi rõ giới hạn, không đại diện cho natural-frame proof.

Capture request identity/start/end/cancel reason, actual branch, open flag/pending InstanceId/queue/modal counts, Hero/Encounter/Wave/counters trước/sau action, bool result, warnings và frame order. PASS chỉ khi scenario reached đúng và decision/drain hoạt động. NOT_REACHED, SETUP_BLOCKED, TIMEOUT hoặc compile/import failure không phải PASS.

**Nếu Phase A không PASS:** preserve raw outputs, cleanup/guard trong finally, trả packet và **dừng trước gate patch**. Không tự sửa loot/open flag/accepted source; không yêu cầu Owner lặp P08. Codex sẽ quyết định corrective riêng từ evidence. Một probe có giới hạn, không retest loop hoặc broad P08/P09 suites.

Nếu probePASS và profile/cleanup verified thì tiếp Phase B. Probe không chứng minh mọi channel path hay thay acceptance cũ.

## 4. Phase B — policy RAM và integration trên source thật

Một owned policy được runner inject vào đúng BattleManager lab, default absent/off; một owner ghi count/state. Threshold test-configurable valid>0, current test50. Không global autoload Stage singleton, Stage PlayerPrefs, save schema/migration/BossProgress/unlockedStages hoặc production content.

Hook sau membership916/dedupe919/member mark924, **ngoài optional drop branch đóng942**, trước GetNextLivingMonster945. Quái không rơi đồ vẫn count. Giữ existing EXP listener/drop/Inventory/Gold-on-dismantle/wave growth; BM guard không được coi là global EXP membership guard.

NormalActive→GatePending tại threshold giữa roster4–5; displayed count cap threshold, current survivors/rewards tiếp tục. Full-wave growth đúng một lần, độc lập kill count. GateReady chỉ sau legitimate completed wave, zero living, finishing requests theo lifecycle tự nhiên, queue/displayed item/decision/modal đã drain và Hero/pause/context hợp lệ.

Một finalization owner trả Ready/Wait/Reject. False predicate phải chặn spawn/advance và có wait/re-evaluation owner, không strand gate. Ghi wakepoints: final kill/channel done/last loot/modal release/unpause. Predicate null-safe. Captured encounter/wave/policy identity và token đúng lifecycle: encounterTransitionCounter khác lootTransactionCounter. Threshold token có thể đổi ở final-death CancelEncounterTransition976; capture đúng handoff context, không so current counter với chính nó.

Finishing caster Count0 cũng có thể do Hero death hoặc cancellation; cần request/frame evidence để claim natural drain. Không thêm coroutine-handle==null: channel→loot branch có thể giữ handle đã hoàn tất. Không cancel caster để ép Ready.

Pending Hero death với survivors: Start resume same roster và giữ count. Explicit Restart với survivors có thể giữ existing full-wave replacement behavior tại lab, không tăng completed-wave tier giả/reset count. Pending đã zero living nhưng chưa drain không được tự spawn replacement wave. Hero death sau final kill trước drain: capture observed blocked/TBD edge nếu cần policy ngoài bounds; không fabricate Ready/refill loot/fresh wave và claim đầy đủ.

Guard **trước side effects** tạo/đăng ký/start/enable/retarget/advance. Lập actual caller/coverage rows, tối thiểu:

- BootstrapStart/auto; BMInitializeBattle/Establish; PrepareAndStartNormalWave; private SpawnAndStartNormalWaveInternal.
- StartBattle; SpawnMonster; RegisterMonster/Monster.Start; relevant RegisterHero side effects.
- CanStartCombat; ExecutePlayerStartCommand/StartCombatAfterHeroDeath; RestartBattle; ResumeCombat; HUD start/auto/pause/unpause.
- AdvanceEncounterAfterLoot; EndEncounterAndStartNext; DeferEncounterAdvanceAfterFinalModal; DeferEncounterAdvanceWithoutLoot; DeferNextLootDecisionRequest; WaitForFinishingExecutionsThenProceed.

Không claim “all14” bằng prose. Private spawner guard trước cancel/destroy/instantiate, Restart trước clear/revive, Ready reject trước enable/retarget/index increment. Registration existing member khác fresh/foreign/late object; Pending không admit một fresh monster bằng retry không hợp lệ. Direct Instantiate có thể tạo object trước Register: proof là không join/auto combat, không phải Unity constructor bị chặn. Không destroy unowned object.

HUD binding chỉ fixture-owned: threshold label/fill/state, Boss unavailable/disabled. Không production Replay/reset, Challenge Normal redirect, fake WIN, Stage advance/unlock/new rewards hoặc finished UI.

## 5. Targeted proof và runner safety

Batch-only runner, transient owned scene, refuse existing unowned singleton/manager; setup catch và teardown finally cleanup partial owned roots/coroutines/listeners. Không global EventBus.ResetAll/shutdown/unowned destruction. Tests observe actual BM routes/results/frame order; không dùng model mirror để thay source integration.

Phase B cases, báo executed/mode/limit từng row:

1. 48→49→50 giữa5-member wave; threshold death không drop;3 survivors còn act; Pending count/HUD đúng.
2. Remaining deaths nhận EXP/drop như baseline, không auto Gold; duplicate/out-of-roster Stage/BM drop guards. Tách nếu được test dùng approved Gold-only flow, không oracle global EXP sai.
3. Hero death với survivors→Start resume cùng wave; explicit Restart giữ full count/tier/Stage cap; premature public advance và Pending zero-living/chưa finishing bị chặn.
4. Natural channel + sequential loot ít nhất2items + real modal/pause; Ready đợi release thật. Separate natural no-loot branch. EditMode state asserts không thay PlayMode proof.
5. Ready start/creation/registration/enable/retarget/deferred routes và stale encounter/token, no new normal wave/no index advance. Direct-instantiation participation proof ghi scope đúng.
6. Deterministic4+4+5 dưới threshold50: kills13/completed3; wave3 current1.01^2, next1.01^3; growth không theo kills hoặc tăng hai lần.
7. Non50 threshold/HUD denominator và policy-absent bounded normal advance; không Boss functional claim.

SG10 durable reopen và SG11 versioned Stage corruption/D18.23 **DEFERRED/NOT APPLICABLE** cho RAM lab. Không exactly-once/product persistence/recovery guarantee.

Một guarded Phase A session, một Phase B session chỉ nếu A PASS. Cap mỗi Unity session300s gồm import/compile; capture PID/elapsed/raw OS exit. Process do wrapper tạo bằng Start-Process phải có WindowStyle Hidden; không mở cửa sổ tương tác. Không dùng quit làm natural PlayMode kết thúc trước scenario; wrapper quản lý đúng PID/terminal exit/teardown. Timeout giữ scenario outcome riêng với wrapper outcome, preserve raw logs và restore/compare fresh lab profile. Không rerun vô ích hoặc fake exit0. Không auto-kill/close Owner Unity.

## 6. Deliverables và stop

Artifacts trong lab output directory ngoài E:, không ZIP:

- `LAB_SPIKE_REPORT.md`: Phase A/B status riêng, observed behavior, failed/non-reached/deferred routes/limits, integration go/no-go. Không Stage feature ACCEPTED/LOCKED.
- `SOURCE_BASELINE_AND_DIFF.md` + UTF-8 patches tách Phase A runner/profile/wrapper và Phase B gate integration. Exact files/metas/bytes/hash, E:/lab baseline comparison, wiring và allowlist. Review only, không apply E:.
- Case results/call coverage/frame telemetry/raw Unity+wrapper logs, exact PID/command/OS exit/times; executed khác planned.
- Guard preflight/backup/restore/compare/final journal trong lab scope; exact CustomRegKey/BackupDir/actual Application identity. Không Owner Registry dump hoặc backup reg payload vào report/memory repo.
- `PROVENANCE.md` và external `MANIFEST_SHA256.txt`: exact refs/read paths/bytes/hash, raw before/after E: inventories và measured source/dirty hashes, lab path/settings/company/product/version/RunID, unchanged reused guard hashes, limitations. Không self-hash recursion.

Không commit/push source/patch/assets vào main/canonical repo; không copy memory/Unityfiles trở lại E:; giữ lab/evidence để review, không tự delete/archive. Nếu bị chặn, hoàn tất preparation có thể review rồi trả exact blocker. Codex review trước source transfer/product activation; Antigravity không gửi Owner questions/playtest hoặc tự corrective P08/P09 source.

Giữ P08/P09-A/P09-B ACCEPTED/LOCKED và recycle corrective CLOSED theo scope cũ. Preserve P08Gate2 135/137exit1, P09-AE2SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2, oldPID25360USER_REPORTED_PASS/RESTORE_UNRESOLVED, historical Restore/executionPARTIAL và Compare/preflightMETADATA_ONLY/recovery snapshot limits. Không old acceptance/recovery rerun, closureZIP, skill rollout, Boss/Bun/offline/Companion/Material/balance changes.
