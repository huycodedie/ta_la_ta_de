# PROMPT ANTIGRAVITY — F-LOOT-LAB-HARNESS-STATIC-03-R3

Task ngày08/10/2026 UTC+07: **SOURCE PREPARATION ONLY**. Authority: [review STATIC03-R2](TLTD_LOOT_LAB_R3R2_STATIC_REVIEW_20261008.md) và latest canonical ACTIVE_WORK_HANDOFF/CURRENT_DESIGN_AUTHORITY. R3-R2 = PARTIAL/REVISION REQUIRED/NO RUN AUTHORIZED. Hoàn tất đúng R1-01…R1-08 còn mở; giữ fixes đã đạt. Không mở case/feature mới.

## Boundary và pre-edit identity

Authoritative lab duy nhất:
`C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/stage_gate_lab_20261007_204500`.

Chỉ hai files mutable:

1. `Assets/_Game/Editor/Prototype01PlayTestRunner_StageGateLab.cs` — 73,111 bytes/SHA256 **F5035D96E0D28C1000AD5742C83441FD98411C07A324A526233D1D88B9024E45**.
2. `Tools/Verification/StageGateLab/run_stage_gate_lab_spike.ps1` — 29,096/**92BDB6993B79849B5267C663C7EF5255B7F04FCE6DCB24D9A66BD45251EAFB6C**.

Frozen: BM58,723/5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6; BMmeta59/59B1AA9F84414AFC5E53F92AF5DFBE3FD4B7A2E6815F9A62980AA8D6AEA80111; settings24,410/A85CC67184A18039D0E1664D7C5625FF2A81BF550889CC1DCCCD0451AB8CC561; runnermeta242/6DF524CA04537093A4EB824E3C28972CCC21EDBF66DCF7BE969145C7E5B19943/GUID7e5ae45bd4db4a0b874c601745a96c20. Tất cả runtime/UI/shared helpers/meta/assets khác frozen. Nếu baseline không khớp: giữ nguyên, báo exact mismatch, không tự rollback/recreate lab.

**Cấm launch Unity GUI/batchmode/compiler qua Editor, PlayMode, wrapper top-level, Guard/preflight action, live Registry/save.** Không allocate future session/profile/nonce hoặc sửa settings trong task này. Offline read/API/AST/dry-check trên disposable copies được phép. Không sửa E:/code/TLTD; HEAD7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425 và toàn working tree/save giữ nguyên; không pull/stage/commit/push/stash/reset/clean E hoặc đưa lab source sang production.

Lập fresh pre/post artifacts ngoài lab; giữ packet/report/raw/journals/archives cũ nguyên byte, không ZIP. Chỉ actual missing static points; không gọi Owner test/đóng nghiệm thu P08/P09/recovery.

## R1-01 — Khôi phục API đúng trước, không rewrite theo báo cáo

Giữ `TerminateSuiteEarly` có yield break và existing public RunAllTests matching wrapper. Đối chiếu actual declarations trước mọi call; API table phải copied exact multiline declaration/defaults/path/line+source hash, không paraphrase từ trí nhớ.

Sửa wave config R583–584/R1511:

```csharp
// Field in fixture uses the existing WuxiaGame.Data.MonsterConfigSO type.
public MonsterConfigSO CapturedWaveConfig;
// Capture while owned BM is still alive; no reflection/property guessing.
ctx.CapturedWaveConfig = ctx.BM.ActiveWaveMonsterConfig;
```

BM public property thật `public MonsterConfigSO ActiveWaveMonsterConfig => _activeWaveMonsterConfig;` tại Core/BattleManager.cs94. Bỏ type `ActiveWaveMonsterConfig`, property `ActiveWaveConfig` và swallowed capture failure. Capture expected owned clone trước BM destruction; after frame verify đúng retained reference bị BM cleanup, không destroy hai lần.

Khôi phục SetReferences từ valid PRE_R3R2 source, hoặc named args đúng actual frozen signatures:

```csharp
titleUI.SetReferences(
    pnl: titlePanel, toggleBtn: null, closeBtn: closeButton,
    headerTmp: null, curTitleTmp: null, nextTitleTmp: null,
    curLevelTmp: null, levelCapTmp: null, reqTmp: null,
    rewardTmp: null, statCompTmp: null, bkBtn: null);

tierUI.SetReferences(
    pnl: tierPanel, toggleBtn: null, closeBtn: tierCloseButton,
    titleTmp: null, curTierTmp: null, nextTierTmp: null,
    rarityCompTmp: null, upgInfoTmp: null, timerTmp: null,
    fillImg: null, progNumTmp: null, upgBtn: null, upgBtnTmp: null);
```

Snippets minh họa signature; bind tên biến thực tế và ownership fixture. Title12required+optional bkBtnTmp; Tier13required. Restore cả ba Title sites R1117/1274/1390 và Tier R1131. Không thêm overload vào runtime, không đổi TMP thành Text/Slider/Image giả.

Modal flow dùng existing ModalRequest/ActiveRequest/DismissActiveModal:

```csharp
var reqA = new ModalRequest(titleUI.ModalId, titleUI.DefaultPriority,
                            isDismissable: true);
bool accepted = ctx.Coord.RequestModal(reqA);
bool active = ctx.Coord.ActiveRequest == reqA && titlePanel.activeSelf;
ctx.Coord.DismissActiveModal(DismissalReason.UserClosed, reqA);
```

Retain A/B exact requests, panel refs riêng. Sửa tất cả RequestModal(view), CurrentActiveModal và DismissModal(view). A3 giữ synchronous clear check ngay sau B dismissal trước yield/loot acquire. Đừng reset ModalRequest sequence/global singleton/EventBus hoặc sửa access modifiers/private BM functions.

## R1-02 — Cleanup owned references phải được chứng minh

Giữ caller-created ctx trước setup, register allocation/subscription ngay vào ledger; setup partial không mất ctx. Guard nested stack phải dispose mọi outstanding IEnumerator trên MoveNext/Dispose exception, early return, valid in-process abort/timeout, ghi mọi failure. Bỏ `stack.Clear` làm mất cleanup; C# yield/try/catch/finally phải hợp lệ. Không promise C# finally sau force-kill; cleanup record khi đó FAIL/MISSING.

Unsubscribe exception phải fail cleanup; giữ callback/removal result đến verify xong. Không clear rồi kết luận ledger empty=removed. Capture owned BM wave clone và next-wave members trước BM teardown; BM lifecycle hủy trước entity/config. **Không gán null các retained BM/Hero/Coord/Root/manager/UI refs trước Unity destroyed-reference checks.** Verify after appropriate frame, gồm allocated managers/views/entities/SOs/wave clone/listeners/owned handles/registered views; remove bookkeeping sau proof. Không destroy ambient objects hoặc global reset. C0 chỉ root/observer đã allocate tại injected fault; không claim config cleanup khi config chưa allocate.

Typed read-only diagnostics chỉ exact existing BM fields và expected types; reject missing owned BM/field/type/null unexpected state, không fallback queue0/-1/false thành thành công. Read-only reflection không SetValue/private invoke; nonnull coroutine chỉ handle diagnostic.

## R1-03 — Một outcome, cleanup và driver thật

Giữ single counter aggregation/suite stop đã sửa. PASS chỉ khi required assertions PASS AND cleanup verified AND không CaseFailed. SetupBlocked/NotReached có reached classification riêng nhưng cleanup false/unknown phải explicit và ngăn PASS. Exact injected C0 fault+site/type flag AND owned cleanup mới PASS probe.

Driver/session PASS phải **sau** actual destruction/verified task context and watcher removal, không dựa list counts trước DestroyImmediate. Implement named task-owned static editor callback trong cùng runner (hoặc valid equivalent post-destruction observer), retained driver ref+immutable owned context, bounded observation/detach tất cả terminal paths; emit final suite chỉ sau driver result. Không thêm asset/runtime script. Gameplay FAIL không tự đồng nghĩa driver teardown FAIL: measure riêng, aggregate đúng.

Structured terminal schema cố định: context identity/RunId/nonce/contract hash, case ID, reached/assertions/cleanup/outcome; một row per C0/A1…A5, driver, suite. Missing proof explicit. `_activeToken` phải thực sự bound/use, không stored-unused. Intermediate prose/PASS không là terminal evidence.

## R1-04 — Hoàn tất đúng sáu cases đã giao

Shared owned observation ledger: run/case/frame/time/retained cast request/queued/drop/pending/modal/equipment IDs, relevant events/counters và ordered EncounterIndex values. Subscribe/unsubscribe owned ledger, filter đúng fixture và identity. Channel A1–A5: gate StartHeroChannel success, during finishing state/membership; bounded await strict **same ActiveRequest + CurrentPhase.Recovery + IsFinished + !IsActive + InterruptSource.None + expected channel elapsed/ticks within deadline**. Timeout không thành natural finish. C0 không claim channel chưa reached. Không Reset/ForceComplete.

“Exactly one advance” là một lần EncounterIndex thực tế tăng sau modal drain, không đếm EncounterTransition state events/token increments. BM emits transition trước actual advance. Giữ ordered boundary observations, không endpoint-only PASS.

- **A1:** during channel gate one real drop/queue1/same nonempty ID/finishing caster; post natural queue0/pending same ID/open/request1. Khôi phục **Inventory contains đúng pending ID trước Tách**; sau Tách removed/completed same ID/gold-only/material+EXP unchanged từ post-death baseline; strict ID chain phải AND/equal, không OR. Modal/drain/one actual advance.
- **A2:** giữ hai registered monsters, Health deaths/real DropSystem, current economy gates. Enforce queued IDs/FIFO/distinctness/request+completed order và second retained cast natural finish. Ngay trước immediate duplicate gate pendingnull/openfalse; call ngay cùng stack trước yield, reject/no request/completion/economy/inventory/encounter effects. First Tách và second Equip snapshots sau legitimate death rewards; gate second equipped slot/ID/membership/economy, final queue/pending/open/modal drain/one actual advance. Không fake SO/item/queue/event.
- **A3:** restore valid APIs A/B requests và capture successful cast; không năm frames thay natural completion. Exact active/queued request A/B và panels; held same real drop ID/request0/economy/EncounterIndex; clear active+queued synchronously trước loot acquire. Loot ID/request/completed once/drain/one advance.
- **A4:** giữ retained zero-drop observer tới terminal; config drop0 thật. Gate exact blocker visible/active identity, strict natural finishing/no-loot defer reached, queue0/pendingnull/openfalse/request0; hold trước clear và exactlyone actual advance sau clear. Không RNG retry/chọn attempt.
- **A5:** **đợi natural completion bằng bounded state gate trước LootPending/defer precondition**, không 1.5s channel rồi chỉ ba frames. Require Hero alive, exact blocker+panel, same valid queued item, LootPending/pendingnull/openfalse/request0, verified retained defer handle observation trước Hero death; thiếu là NOT_REACHED, không skip rồi PASS. Giữ actual Health death observer/token increase/defer cancellation đã gate; khôi phục post-death-reward/pre-Hero-death Gold/Material/EXP/inventory baseline và no-effects/no-advance/no request/completed sau blocker release; AwaitingPlayerStart/clear. Không đòi full SkillExecutor/dash/projectile/Stage test.

## R1-05 — Wire explicit immutable launch context, restore scene refusal

`expected_launch_contract.json` hiện chỉ descriptor, wrapper chưa đọc. Implement actual contract path/loader/interface trong wrapper và runner: canonical lab/data/output/backup/profile, explicit reviewed RunId, unique nonce policy, expected payload map/identity. Không auto default RunId và không chấp nhận arbitrary nonempty pair. Descriptor source-preparation có thể chưa có session allocation; code phải refuse run cho đến separate reviewed run prompt cung cấp actual approved mapping. Nonce generate once ở owned wrapper launch stage được phép trong future run nếu policy explicit; bind vào immutable launch receipt trước Backup/Unity, CLI/SessionState/terminal phải exact-match receipt. Không allocate nonce/session/settings bây giờ, không thêm general auth framework.

Exact same immutable context validate initial/reload/suite, không chỉ profile/nonempty SessionState. Stale/missing/duplicate/partial/unowned context: log/refuse/detach named task hooks/clear only task state, không NewScene/Exit/isPlaying mutation. Suite ownership fail cũng phải obey. Owned batch termination chỉ sau verified context. Settings vẫn frozen; code chuẩn bị future fresh unique profile binding từ external expectation thay fixed R2; actual settings change cần separate run prompt.

Khôi phục finite include-inactive inventory của existing relevant managers/views/entities/harness across loaded scenes, already-playing/transition refusal trước NewScene. Name prefix không là ownership và transform-only root không chứng minh empty; không adopt/destroy ambient objects hoặc lazy-create singleton. Entry/scene transition/reload exceptions luôn detach task hooks/state an toàn.

## R1-06 — Actual target/journal schema và prelaunch ownership

Giữ finite six actual runner/wrapper/settings/Guard/Core/preflight targets và ancestors reparse checks. Validate canonical containment, exact expected kind/existence trước output/import/invoke bằng LiteralPath; missing không “no reparse PASS”. YAML identity parse unique scalar entries, không first-match mơ hồ.

Require known historical journal.json chính file và Owner launcher metadata VERIFIED; intentional BACKED_UP rollback checkpoint giữ riêng, không broad recovery sweep. Fresh journal sau future successful Backup phải validate **trước Unity** và revalidate ngay trước recovery: actual Guard schema/types/status, canonical fresh task-owned path, coherent OriginallyExisted/Snapshot/BackupFile/BackupHash và **RegSubKey AND FullRegPath exact HKEY_CURRENT_USER form Guard thật emit**. Bỏ invented HKCU-provider accepted alias. Existing-profile BackupFile nếu dùng phải owned fresh backup với verified Guard hash. Bind measured journal identity/status to external receipt, không đổi Guard/journal schema.

Missing/partial/mismatched/unowned: preserve+block, không claim recovery PASS; no live Registry action trong static task. Không khôi phục Owner save hoặc old recovery journals để “test validator”.

## R1-07 — Process identity, independent finally, required evidence

Capture root Process/start identity **một lần tại launch**, không mới đọc làm baseline lúc kill. Retain finite observed owned descendants, verify captured PID/start/parent/path identity lại trước stop, bounded terminal confirmation root+owned descendants. Không kill PID trần/ambient Unity/Owner; uncertain inspection fail closed, không suppress thành “absence”.

Unknown/nonterminal/conflict: preserve journal, skip both Restore/Compare, nonzero. Isolate every closeout stage (terminal confirmation, recovery, metadata, evidence/finalreceipt) with exception recording; lỗi một stage không mất failures/receipts sau đó. Safe recovery chỉ khi verified owned terminal, no conflict và exact owned journal; Compare không bù Restore exception thành PASS. Giữ actual OS exit/UNAVAILABLE riêng wrapper reason/exit.

Required prelaunch evidence failure block launch; required later evidence failure block PASS nhưng safe cleanup vẫn thực hiện. **Final required receipt/acceptance write phải checked trước selecting exit code**; không wrapperPass=true rồi Log fail vẫn exit0 như W641–642. Không xóa/overwrite thất bại hoặc retry bonus.

## R1-08 — Contract payload/preservation/all-terminal acceptance

Wrapper phải consume reviewed expected contract thật, canonical decoded paths, externally expected actual runner/wrapper/settings + frozen18 => **21 selected rows** pre/post with actual bytes/SHA/expected/match/status persisted. Avoid selfhash recursion; delivery receipt không thay trust context. Current exact code payload expectations nằm external descriptor, không selfapprove nguồn mới.

Gate assigned finite E HEAD/full porcelain/17fingerprints và fixed historical PhaseA/corrective/R2/R3/R3R1/R3R2 packet sets pre/post, check every Git/tool exit/missing/mismatch; persist actual rows. E read-only, không toàn untracked/save byte claim. Contract/preservation check phải used in result, không retrieve rồi bỏ status.

Normalize CRLF rồi parse **all** terminal rows với schema/context/nonce exact; total đúng một per expected case/driver/suite. Reject malformed/unknown/stale/duplicate/contradictory/surplus terminal rows, not count success-only. Gate reached/assertions/cleanup/status/order; driver verified trước suite. ScenarioResult lấy validated current suite, không first unbound match. Final wrapper PASS only all preflight/Backup/context/source/preservation/required cases+cleanup/driver/owned terminal/actual OS0/!timeout/Restore/Compare/evidence gates passed. Không auto retry.

## Offline verification và packet mới

Hoàn tất source rồi tự đối chiếu diff với actual declarations và **known-valid PRE_R3R2 calls**, tránh regression khác. Deliver R1-01…08 checklist mỗi row beforefinding → exact changed source line → actual declaration/dataflow → named static proof; unresolved ghi OPEN. Không “100% completed” chỉ vì có code/log string.

Packet mới chứa pre/post6copies, two exact UTF-8 relative patches, expected contract/interface descriptor, payload receipt+manifest nonrecursive, copied-exact API declarations, static source map và receipts. Manifest cover every payload; giữ old packets byte-identical. AST receipt exact parsed path/bytes/hash/command/errors. Drycheck exact disposable cwd/Gitversion/command/pre+post bytesSHA/patchSHA/separate stdout+stderr/exit+unchanged-copies proof. Không apply disposable patch vào runtime.

C# compile chỉ nếu available correct references ngoài Unity với exact command/refs/diagnostics/exit; không có thì **STATIC_API_REVIEW_ONLY / UNITY_COMPILATION_NOT_RUN**, không launch Editor để kiếm refs. Safe extracted pure decision helpers của context/result/journal được kiểm offline với exact source/hash và negative inputs missing/stale/context mismatch/PASS+FAIL/wrongRegSubKey despite matchingCustomRegKey; không evaluate wrapper top-level hoặc trusted helpers. Đây là predicates của harness, không gameplay cases mới. Không ghi PASS cho receipt chưa chạy.

Giữ bảng15paths/bytes/SHA **đã đúng trong actual R3R2 packet**. Addendum sửa still-wrong exact UI signatures/constructor names, overclaims21gate/Eexits/contractwired, UTF8 mojibake và chat-inline discrepancy; copied frozenfiles giữ format nguyên. Report own per-file BOM/CR/control metadata, không normalize frozenfiles hoặc tự giải thích CRLF/historical causes.

**STOP sau source/diff/static handoff cho Codex.** No Unity compilation/session, no C0/A1–A5 execution. Phase B stopped; parent BM runtime patch accepted with evidence limits/overallpending; P08/P09A/P09B/recycle/recoveryLOCKS và raw limits giữ nguyên. Không Stage/Boss production/exposure/persistence/schema/skill activation/source push/product approval. Future run chỉ sau Codex source acceptance và prompt phiên riêng.
