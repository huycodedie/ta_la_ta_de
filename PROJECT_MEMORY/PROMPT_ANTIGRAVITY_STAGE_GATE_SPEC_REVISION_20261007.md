# PROMPT ANTIGRAVITY — STAGE GATE SPEC REVISION, READ ONLY

Task: **SPEC-STAGE-GATE-READONLY-01-R1**. Ngày07/10/2026, UTC+07.

**AUTHORIZED: đọc source/authority và tạo packet revision tài liệu mới. Stage/Boss implementation NOT APPROVED.** Không code/assets/meta/scene/save writes; không Unity/import/compile/test/SaveGuard/recovery; không audit lại tám nhóm gameplay đã đóng.

## Đọc trước và giữ baseline

Đọc [TLTD_STAGE_GATE_SPEC_REVIEW_20261007.md](TLTD_STAGE_GATE_SPEC_REVIEW_20261007.md), task gốc [PROMPT_ANTIGRAVITY_STAGE_GATE_SCOPE_SPEC_READONLY_20261007.md](PROMPT_ANTIGRAVITY_STAGE_GATE_SCOPE_SPEC_READONLY_20261007.md), AI_RULES, CONTINUITY_AND_SYNC_AUTHORITY, ACTIVE_WORK_HANDOFF và CURRENT_DESIGN_AUTHORITY current canonical main. Packet cũ task alias `AUDIT_STAGE_GATE_FOUNDATION_SPEC_20261007` nằm tại `E:\code\TLTD\scratch\stage_gate_scope_spec_20261007_005000`; **giữ originals và hashes**, sửa bằng version mới có changelist R1-01…R1-05.

Review pre-publication baselines: memory main0b538088423df637024abc8f4aded81079e26df1; game main/E:HEAD1410af40405a8270f113bd07361015e4588d5cd6; source parent393a0487083fd1e311db28cdf07393427769a4de unchanged by docs commit. Resolve main hiện hành lúc bắt đầu; phân biệt canonical authority SHA, source HEAD và file bytes. Review/prompt này được publish bằng commits riêng: không coi baseline0b538/1410 là SHA cuối cùng mãi mãi.

Không pull/overwrite vào E: dirty memory. Đọc canonical snapshot tách biệt. Snapshot Codex01:24:27+07:3tracked dirty +530untracked; giữ MANUAL_P09A_CHECKLIST.md16177/hash09C343…; local ACTIVE6358/hash842B4F…; REVIEW_RESPONSE16925/hashE9143A…. Inventory read-only dùng `git --no-optional-locks status --porcelain=v1 --untracked-files=all`, read-only rev-parse/log/diff; không refresh/stage/commit/push/reset/stash/clean/checkout tại E:.

Inputs không hỏi lại:50 normal deaths hiện tại/config monstersRequired; gate100% giữ đến BossWIN; BossLOSE không reset normal progress/rewards; P08 roster4–5, growthMaxHealth/ATK/DEFx1.01 once/completed-wave, natural finishing/channel/loot/modal; A10 Gold-only và current preserved formula/TBD balance. D18.23 PlayerProgress fields; D18.24 normal exit không tự Lose; D19.13/.14/.18/.20. D18.25 Boss exit/reset/resume vẫn TBD/outside. Stage khác Title; không invent Title blocker.

## 1. Sửa authority và retry contract — không hỏi Owner vì lỗi citation

- Bỏ BossLOSE Progress0/50 và citation sai D18.19; cite D18.29/D19 đúng. Không đưa reset0/free-farm xuyên gate thành compliant options. Threshold/gate giữ100% không phải câu hỏi cần Owner chọn lại.
- Trace exact same-session Hero death → Start/resume surviving roster, explicit Restart, process reopen, final kill/channel/loot interruption. `ExecutePlayerStartCommand`817–850 không mặc định fresh wave khi survivors còn sống. Làm table trigger/guard/current behavior/proposed delta/side effects/RAM-durable; mỗi delta phải được đánh dấu proposal và giải thích cần thiết.
- Hero death đang clear pending/displayed loot là current source behavior/risk; giữ accepted authorities, không tự coi hậu quả mất queue là quyết định sản phẩm đã khóa. Không claim receipt tồn tại hoặc tự sửa policy trong task này. Normal exit không tự coi Lose. Không chốt Boss retry/resume hoặc offline behavior ngoài scope.

## 2. Checkpoint feasibility từ source thực — trọng tâm revision

Trace tối thiểu BattleManager, Monster death/EventBus producers/subscriber registration, ProgressionManager.AddExp/SaveState/HandleEntityDied, ResourceManager.AddGold/Dismantle/SaveState, DropSystem, Inventory, EquipmentManager và loot decision. Đọc đúng paths/lines và hash các files thật đã dùng.

Trả **commit/failure-window table** cho normal death, threshold death, final-wave kill, finishing channel, sequential loot decision (equip/keep/dismantle), Hero death, Restart, exit/reopen:

- State/item/receipt ID, owner, write target (RAM/PlayerPrefs), actual save order, durable step vs notify, dedupe lifetime, producer/subscriber ordering và known unknowns.
- Logical stage credit khác EXP reward, dropped item/decision, Gold-on-dismantle, completed-wave growth. DropSystem đã AddItem trước return/enqueue; Hero death xóa queue không tự xóa Inventory item. Queue append không phải durable reward commit. EXP listener ordering không mặc định sau BattleManager hook. Không invent Gold per normal death. Không đổi existing reward authority hoặc relabel P08 completion.
- Phân biệt durable write với actual load caller: Progression.Start hiện gọi InitializeProgression defaults, chưa tìm thấy production caller tự động LoadState trong review; ResourceManager load ở Awake. Xác minh source load order/limits, ghi dependency nếu cần; không tự sửa existing cold-start progression hoặc claim runtime reopen đã kiểm thử.
- Crash/abort giữa từng write gây gì: committed EXP/Gold với Stage checkpoint cũ; missing inventory/queue serialization; save/growth tier mismatches; re-created monster identities/dedupe. Static failure model chỉ là static finding, chưa runtime crash proof.

**Bỏ bảo đảm zero duplicate/lost loot/100% safe của wave-only checkpoint.** SaveGuardDiff0 chỉ bảo vệ test baseline. So sánh có căn cứ các phương án sau, không tạo false dichotomy per-death→partial-spawn:

1. **Ephemeral isolated gate fixture:** explicit session/store/build isolation, no persistence/exactly-once claim, no production progression/reset control. Nêu hạn chế vì chưa đáp ứng full player foundation.
2. **Stage-only checkpoint:** một policy RAM/durable thống nhất trong toàn packet; ghi loss/replay/divergence window và contract nào không đạt. Không gọi compliant crash-safe nếu thiếu proof/design.
3. **Minimal durable stage/encounter/receipt proposal:** stable wave/member/transaction identity, recorded kills/growth/loot decision relationship, publication/idempotency/load ordering cần có. Nêu exact coupling/delta/files và failure guarantees thực sự đạt; không giả independent PlayerPrefs.Save calls là atomic multi-authority transaction. Không tự thêm full inventory serialization, deferEXP/Gold, frameworkSaveManager hoặc offline.

Chọn technical recommendation dựa trên feasibility. Nếu đáp ứng full contract đòi persistence work vượt candidate, trả exact blocker/dependency + một bounded alternative có thể review, không mở scope tự động. Chỉ đưa Owner decision thật sự cần sau khi technical options đúng; không yêu cầu Owner chọn giữa assurances sai. Không implement bất kỳ phương án nào.

## 3. Completion ownership, hook và toàn bộ entry paths

- Một gate state authority/writer và một complete-safe predicate; BattleManager/Stage owner/consumer rõ. Hook đề xuất sau membership/dedupe/member mark, **ngoài** `if(droppedItem != null)` đóng942, trước GetNextLivingMonster945. Include legitimate no-drop death; không thêm duplicate resolver/global progress award listener.
- NormalActive→GatePending→BossGateReady vẫn tên/architecture PROPOSAL. Table mỗi transition ghi trigger, guard, owner, persisted/RAM, token/read model/notifications và invariant. Không vừa per-death persist ở§4 vừa wave-only ở§6/SG10.
- Pending cho phép survivors của current wave tiếp tục; chặn different-wave spawn khi chưa drain, except explicit checkpoint/restart policy đã mô tả. Ready chỉ sau legitimate full wave + natural finishing requests + queue/displayed item/modal/pause release + valid encounter token. Public EndEncounterAndStartNext không tự chứng minh đã đủ predicate. Không hủy caster coroutine để ép gate; stale callback cancellation khác finishing execution.
- Call graph đúng actual callers và guard coverage: GameBootstrap/auto start; EstablishCanonicalEncounterMonsters; PrepareAndStartNormalWave; private SpawnAndStartNormalWaveInternal; StartBattle; EndEncounterAndStartNext; RestartBattle; CanStartCombat; ExecutePlayerStartCommand/StartCombatAfterHeroDeath; public SpawnMonster/RegisterMonster/Monster.Start và direct instantiation; AdvanceEncounterAfterLoot; DeferEncounterAdvanceWithoutLoot/AfterFinalModal; WaitForFinishingExecutionsThenProceed; BattleHUD start/auto/pause/death interactions.
- Tách creation/registration/command/deferred callback categories. Mỗi route ghi existing caller, current behavior, proposed guard ởNormalActive/Pending/Ready/loadedInvalid, test oracle và ảnh hưởng accepted lifecycle. Không chỉ chặn loot branch/StageManager presence hoặc coroutine while Ready.

## 4. Data, exposure và exact future allowlist

- D18.23 minimum chapter/currentStage/highestStage/unlockedStages/bossProgress phải có mapping/version/ownership; phân biệt persisted reserved/deferred fields với feature implementation. Clarify highest semantics/stable IDs/current gate/normal growth tier/checkpoint transaction relationship. Kill cap theo config, không hardcode50 vào schema.
- Source inventory chỉ relevant Stage/Chapter/Arena/Boss content; stage/chapter/name examples label DEVELOPMENT FIXTURE / PROPOSAL. Không production content table mới, balance, skills, Material source hoặc Boss implementation.
- Idempotent init/load trước bootstrap; missing/corrupt/future-version/unknown-field handling preservative có bounds. SavedReady/config missing không được silently endless farm/reset. Tách legacy no-stage test fixture fallback khỏi missing manager trong player session có durable gate.
- Dev-only phải có exact isolation/activation/config discovery path. Bỏ production ReplayStage1/reset progress control. Gate-only endpoint truthful locked/unavailable Boss; không fake Challenge/Normal redirect/WIN. Giữ dev fixture recommendation và production exposure tradeoff là proposal, không tự bật main scene.
- Exact proposed allowlist gồm code/new .meta/asset/scene-or-builder/HUD path thực sự cần. Không vừa asset outsideResources vừa không loading/wiring. Correct3existing+4newcount; không invent event enum đã có. Nếu gate-only fixture khác production integration thì allowlist tách hai alternatives rõ.
- Rollback code vs feature disable vs retained data riêng; không claim reverting patch cleanly restores saves/assets/scene. No blanket revert/reset dirty baseline; preserve unknown saved keys. Nêu dependency/defer boundaries rõ.

## 5. Acceptance matrix PLANNED — không chạy

Sửa SG-01–SG-12 theo review và thêm rows khi cần để cover exact route/failure windows, không đòi mở broad suites:

- No-drop legitimate death; duplicate/out-of-roster/Hero guards, current EXP/drop authority riêng; Gold chỉ theoTách; mỗi producer/handler proof đúng scope.
- Hero survivor resume vs Restart vs reopen; Pending interrupt/final channel+loot phase; no silent reset/suppression.
- Natural channel/modal frame proof không dùng EditMode+dummy để đại diện. Split API/state EditMode, bounded natural-frame PlayMode và isolated persist→teardown→load/recreate evidence. Tất cả **planned/not executed** lúc này.
- Dead roster giữ indices: zero living/new spawn là oracle, không bắt activeMonsters.Count0.
- All routes/auto/start/retry/load/stale callbacks no bypass; missing/invalid/future version preserve saved record/readiness; safe-load before spawn.
- Deterministic waves4+4+5: kills13, completed3, NextMultiplier1.01^3; current wave3 multiplier1.01^2 trước wave4. Once-per-wave growth độc lập count.
- Checkpoint/reopen assertions bao gồm saved Stage/EXP/Gold/items/queue/growth/identity theo actual proposed policy; failure injection window/idempotency nếu policy có guarantee. Diff0 chỉ teardown owner baseline, không proof behavior.
- HUD denominator config, Pending/Ready truthful; SG11 error injection/no-write proof không screenshot; SG12 values/frame/visual scopes riêng.
- Future runner/test plan phải giữ batch-only entry, owned/transient fixture, no unsafe menu/global resets/persistence migration, preflight prior journal và new backup, bounded wrapper/cleanup. Đây chỉ plan, không tạo runner/chạy Guard.

## Deliverables, provenance và stop

Chỉ artifacts mới `scratch/stage_gate_scope_spec_r1_<runid>`:

1. `STAGE_GATE_SCOPE_SPEC_R1.md`: coherent revised spec, state/guard/load/schema/allowlists, checkpoint failure/feasibility tables và exact deferred dependencies.
2. `STAGE_GATE_ACCEPTANCE_MATRIX_R1.md`: all planned/unexecuted, eachrow precise mode/setup/action/assertion/evidence/scope limit.
3. `STAGE_GATE_DECISION_PACKET_R1.md`: LOCKED INPUT / ACCEPTED BASELINE / SOURCE EVIDENCE / TECHNICAL PROPOSAL / PRODUCT TBD. Recommendations label **Antigravity proposal pending Codex review**, không joint TechLead approval. Chỉ questions genuinely unresolved; nếu none needed yet ghi none. Không gửi Owner hỏi/chơi game.
4. `PROVENANCE_R1.md`: exact source+canonical authority SHAs/read paths+bytes/SHA, old artifact hashes, current before/after inventories và dirty-doc hashes; static/not executed limitations. Thêm `MANIFEST_SHA256.txt` cho artifacts/provenance, không self-hash recursion. Giữ submitted historical inventory claim riêng khỏi new measured baseline.
5. `REVISION_MAP_R1.md`: mỗi findingR1-01…R1-05/matrix correction → revised section/source citations/resolved or concrete blocker. Có recommendation + explicit contract notmet/scope dependencies để Codex có thể quyết định, không chỉ đổi lời bảo đảm.

Không overwrite/reportfix logs cũ, không ZIP. Không Git mutation/push/memory sync overwrite E:, code/asset/meta/scene/save writes, Unity/import/compile/test/Guard/Registry dump. Không fullBoss/Bun/offline/Companion/skill rollout/Material source/balance/UI polish/replay reset.

P08/P09-A/P09-B ACCEPTED/LOCKED và recycle corrective CLOSED giữ nguyên. Preserve P08Gate2 135/137exit1, P09-AE2scenario-pass/wrapperexit2, oldPID25360restore-unresolved, rawRestore/executionPARTIAL và Compare/preflightMETADATA_ONLY/recovery snapshot limits. Không Owner retest/recovery rerun/closureZIP.

Làm hết useful trace/options trước; nếu thiếu dữ liệu ghi exact missing source/value/decision và vì sao chặn phần nào. Trả packet cho Codex rồi dừng. Read-only task này không tạo implementation authorization hoặc gameplay LOCK.
