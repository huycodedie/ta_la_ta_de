# PROMPT ANTIGRAVITY — STATIC-03-R1, sửa source rồi trả review

Task **F-LOOT-LAB-HARNESS-STATIC-03-R1**,08/10/2026 UTC+07. Đây là revision hữu hạn của STATIC-03, **không phải prompt chạy Unity**. Đọc [TLTD_LOOT_LAB_R3_STATIC_REVIEW_20261008.md](TLTD_LOOT_LAB_R3_STATIC_REVIEW_20261008.md) và latest canonical ACTIVE_WORK_HANDOFF/CURRENT_DESIGN_AUTHORITY. Review R3 = PARTIAL/REVISION REQUIRED. Giữ các cải thiện R3; sửa theo mapping R3-01…08, không wholesale rewrite hay tự mở gameplay scope.

## 1. Quyền và baseline

Lab duy nhất: `C:\Users\conca\Documents\Codex\2026-10-05\b-n-ti-p-qu-n\work\stage_gate_lab_20261007_204500`.

Chỉ hai mutable files:

1. `Assets/_Game/Editor/Prototype01PlayTestRunner_StageGateLab.cs` — current63492bytes/SHA25693027F6B881793BC545FAB828A0891A1D86B168725A24177188C06EFA707884C.
2. `Tools/Verification/StageGateLab/run_stage_gate_lab_spike.ps1` — current23905/SHA256F4AAA5265931FBF556F09FC6778678C7AD921AF4BBA35FD303D0CA5550398BAD.

Freeze BM58723/SHA2565B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6; settings24410/A85CC67184A18039D0E1664D7C5625FF2A81BF550889CC1DCCCD0451AB8CC561; runner meta242/6DF524CA04537093A4EB824E3C28972CCC21EDBF66DCF7BE969145C7E5B19943/GUID7e5ae45bd4db4a0b874c601745a96c20; BMmeta/runtime/UI/sharedhelpers/Assets khác/Packages/scenes/prefabs không đổi. Không thêm Assets scripts/public runtime API hoặc đổi access modifier.

**CẤM launch Unity kể cả compile/batch/PlayMode; cấm wrapper top-level, dot-source có side effects, Guard/preflight action/live Registry/save, kill Owner processes.** Không tự thêm bonus run sau static fix. Được AST/API review, offline compile đúng assemblies nếu sẵn có, diff/hash/encoding/git apply --check trên disposable copies. Không cài tooling để vượt boundary. Settings vẫn frozen; chỉ chuẩn bị code future context/profile, chưa cấp hoặc allocate phiên mới.

E:\code\TLTD chỉ read HEAD/full porcelain/17selected fingerprints, không source/memory/save edits hay Git mutation. Preserve all originals: PhaseA8, corrective19, R2packet27, R3packet17, received archives, journals, logs và snapshots. Fresh external output trong workspace work/, tuyệt đối không reuse output/run/backup cũ. Record exact path, không sửa report cũ.

## 2. Sửa API và khôi phục fixture thật trước

- Thêm đúng namespaces Data/Items/UI hoặc fully qualify types; rà từng symbol mới against current declaration. SkillDefinitionSO dùng existing `SetChannel(true, duration, tickInterval)`; bỏ fields/enums bịa hoặc private assignments. Channel helper theo existing API:

```csharp
var request = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
bool started = hero.CastState.StartChannel(request, skill.ChannelDuration, skill.ChannelTickInterval);
```

Retain request để chứng minh cùng cast tới natural end; không Reset/ForceComplete/interrupt để qua oracle. Kiểm declaration `Combat/SkillExecutionTypes.cs`, `Combat/SkillCastState.cs`, `Data/SkillDefinitionSO.cs`.

- `EventBus.OnLootDecisionCompleted` là `Action<EquipmentInstance>` một tham số. Đổi đúng subscribe/unsubscribe signature và giữ exact callback refs trong owned ledger. Bỏ call private BM.CancelLootLifecycle; không reflection invoke hoặc mở API. Dừng/dispose đúng owned BM rồi verify teardown qua existing lifecycle. IReadOnlyList membership dùng loop hoặc supported LINQ đúng namespace.
- **A2 khôi phục hai owned registered monsters và legit Health death → actual DropSystem/BM queue.** Có thể lấy phần real flow pre-R3 làm tham chiếu rồi giữ cleanup/duplicate timing R3. Cấp monsterCount2 riêng A2. Bỏ toàn bộ CreateInstance<EquipmentInstance>, property assignments/get-only, fake inventory inserts, nonexistent queue APIs. Không thay bằng trực tiếp EnqueuePendingLoot/TryPresentNextQueuedLoot để fake source. Current A2 không dùng Monsters[1]; chỉ khi restore hai deaths mới cần setup2.
- Giao API table mỗi changed symbol → declaration/path/signature/access/type. Không tuyên bố compilation PASS nếu chỉ review text.

## 3. Hoàn tất existing lifecycle/oracles, không thêm cases

Giữ **C0+A1–A5** theo contract STATIC-03 trước; các predicates cũ còn binding. Không tăng suite hay yêu cầu Owner test lại. Sửa cụ thể:

- Caller ctx/ledger có trước allocation; record ngay objects/SOs/config/subscriptions/views. Guarded execution phải catch/route nested iterator failure, dispose và bảo đảm cleanup khi setup/case/callback/timeout/abort/early return. C# iterator try/catch/finally phải hợp lệ; không yield trong vùng cấm. Cleanup stages bounded và failure-recorded.
- C0 chỉ expected khi đúng controlled fault type/site/flag đã inject và owned listener/config/root cleanup được verify. Unexpected fault không được đổi nhãn expected. Case terminal/PASS chỉ sau assertions+cleanup; cleanup fail dừng cases còn lại và suite fail.
- Cleanup guarantee chỉ áp dụng đường abort/timeout còn trong process và owned driver/iterator. Sau force-kill Unity không thể hứa C# finally/frame/driver receipt; thiếu cleanup phải FAIL/MISSING, không giả PASS. Future guarded wrapper recovery là bước riêng, chưa được chạy trong task này.
- Retain references trước destroy và tới sau verification, không clear ledger rồi iterate list rỗng. Capture owned ActiveWaveMonsterConfig cùng next-wave BM members/config khi BM còn sống. Stop owned BM lifecycle trước entity/config destroy. Verify owned roots/entities/BM/Hero/Coordinator/views/SOs/listeners/coroutines/session state thực tế; không dùng unconditional zero. Never adopt/destroy arbitrary global objects. Final driver/session teardown phải có verification riêng trước final suite receipt.
- Tạo owned transient EquipmentDropConfigSO qua existing InitializeConfig/DropSystem.SetDropConfig; record trước attach, retain/verify sau destroy. Reuse read-only database references; fallback rate alone chưa đạt owned-config contract. Include-inactive refusal inventory phải có CorrectiveHarness trước tạo driver.
- A1 đo **during channel** finishing+queuedID/queue1/no-pending/no-open/no-request rồi **sau natural end** same request/no-interrupt/recovery-finished/deadline + pendingID/open/queue0. Gate channel result; không đòi queue1 sau dequeue. Assert requested/completed/effect/count và one advance đúng boundary.
- A2 giữ actual two deaths/drops, FIFO IDs, first/second request/completed/economy deltas. Duplicate Tách của item đầu phải ngay cùng stack/windowclosed trước yield; assert no second effect/completion. Không chỉ log. Gold-only contract: lấy snapshots sau legitimate death rewards, ngay trước mỗi decision/duplicate; Tách/duplicate không đổi Material/EXP so với đúng boundary đó, không phủ nhận EXP hợp lệ do death.
- A3 actual blockers A rồi B; queued/pending/open/request/counter trước clear; synchronous active+queued clear trước acquire, bounded natural wake/requestonce/drain/oneadvance và exact IDs/counts. Không reflection mutate view khi public reference setter đã đủ.
- A4 assert blocker thật và finishing/no-loot defer reached, actualdrop0/queue0/no-pending/no-open/request0; no advance trước clear/exactlyone sau. Same request natural end và no interrupt phải gate; observer luôn finally unsubscribe. Không retry random outcome để chọn PASS.
- A5 assert finishing+same natural request/aliveHero/blocker/LootPending/validqueuedID/no-pending/no-open/request0/activeowneddefer trước death; thiếu=NOT_REACHED. Legit Hero Health death, actual death event + typed token invalidation; release blocker, bounded frames: AwaitingPlayerStart/noqueue/pending/open/newrequest/completed/economyeffect/encounteradvance và defer cancellation **phải gate**, không chỉ log.

BM read-only reflection chỉ các diagnostic members đã cho phép, exact type check; không SetValue/private invocation/new runtime API. Nonnull coroutine handle không tự chứng minh scheduler sống.

## 4. Wrapper và runner context — chuẩn bị code, không invoke

- Require explicit RunId/per-launch nonce/immutable context, không default R2/R3 known strings hoặc fallback khi thiếu CLI. Initial/reload validate same exact context/profile/canonical dataPath/batch và one-shot task state trước scene mutation/Exit. Interactive/unowned/stale chỉ refuse/detach/log; không isPlaying=false/NewScene/Exit. Prepare future fresh profile mapping; giữ settings bytes hiện tại.
- Validate exact Assets/ProjectSettings/Tools/helper/script targets và ancestors, reject reparse escape trước tạo output/import/helper calls. Giữ containment/trust-order fixes R3. Require từng known PhaseA/corrective/R2 journal file, không silently skip missing; known Owner launcher metadata must be VERIFIED. Intentional BACKED_UP rollback checkpoint không phải active launcher; không recovery hay broad scan trong task này.
- External reviewed payload receipt cho revised runner/wrapper/settings + trusted18baseline, expected/actual pre/post rows, no selfhash recursion. Future wrapper chụp và gate E HEAD/full porcelain/17selected cùng fixed historical packet comparisons; check tool exits. Không HEAD-only hoặc chứng nhận toàn bộ untracked/save bytes.
- Track Backup attempt/result và validate exact fresh journal path/profile/RegSubKey/FullRegPath/launch ownership trước recovery vì Guard chọn journal scope. Partial/missing/unowned phải preserve/block theo safe policy. Isolate finally terminal/recovery/metadata/evidence stages; required logging failure makes acceptance false, không nuốt lỗi.
- Captured Process/start identity và owned child tree; bounded wait/terminal confirmation cả timeout/exception. Unknown/nonterminal/otherUnity conflict ⇒ preserve journal, **skip cả Restore/Compare**, nonzero. Không PID-only kill/reuse, Ownerkill hoặc AllowRunningUnity. Actual OS exit hoặc UNAVAILABLE tách wrapper reason124/1.
- Emit one structured terminal row **sau assertions+cleanup** mỗi C0+A1–A5, context-bound final driver/session teardown rồi exactly one suite terminal. Wrapper reject missing/duplicate/stale/conflicting/intermediate rows; no substring PASS. Future PASS cần required reached cases+cleanup+driver+OS0/!timeout/ownedterminal+strictpreflight/Backup/Restore/Compare+identity/source/preservation. Giữ no-auto-retry/per-attempt immutable raw evidence/actual parent exit.

## 5. Giao source/evidence rồi dừng

Return one fresh packet, manifest mọi payload trừ manifest itself; pre/post copies sáu files (runner/wrapper/BM/settings/two metas), UTF-8 relative patches chỉ hai mutable files, source mapping R3-01…08 → changed lines → exact static evidence. Include PowerShell AST exact bytes/hash/errors receipt; API table; nếu compiler không có, ghi **STATIC_API_REVIEW_ONLY / UNITY_COMPILATION_NOT_RUN**. Không regex thành compile PASS.

Nếu drycheck: exact commands/cwd/Git version/baseline bytes+SHA/stdout+stderr/exit/copied-baseline unchanged. Pure-helper offline tests chỉ AST-select/extract no-side-effect functions, record extracted hash/limits; nếu không tách an toàn thì ghi not run. Không invoke top-level để test helper.

Readable UTF-8 addendum sửa15 baseline byte counts theo review và mojibake; phân biệt18baseline/19withsettings/21currentselected. Giữ oldreport/oldattempt outcomes và giới hạn raw. Chụp read-only actual before/after E status+17 và historical/frozen sources, ghi reviewer/executor attribution; thiếu historical artifact ghi MISSING, không tạo ngược proof. Correct prior matrixprelaunch-only, OSlabel, batchargs/parentescaping và Codex patchcheck attribution theo previous reviews.

**STOP sau giao source cho Codex.** Unity compilation NOT RUN, C0/A1–A5 NOT EXECUTED cho task này; PhaseB NOT EXECUTED/STOPPED. Parent BM runtime patch accepted with evidence limits/overall pending; P08/P09-A/P09-B/recycle/recovery locks và raw limits không đổi. Không Owner playtest/ZIP/recovery rerun/skill activation/new gameplay/Stage/Boss/persistence/schema/source Git publication. Codex sẽ review lại source rồi mới quyết định prompt session riêng.
