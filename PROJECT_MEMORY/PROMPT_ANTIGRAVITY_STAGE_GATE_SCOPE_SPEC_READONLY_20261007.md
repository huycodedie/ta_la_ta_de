# PROMPT ANTIGRAVITY — STAGE GATE SCOPE SPEC, READ ONLY

Task: **SPEC-STAGE-GATE-READONLY-01**. Ngày07/10/2026, UTC+07.

**AUTHORIZED: phân tích và artifacts tài liệu read-only. Stage/Boss implementation NOT APPROVED.** Không sửa Unity source/assets/scene/save, không chạy Unity/import/compile/test. Không lặp audit tám nhóm đã đóng.

## Mục tiêu

Sau corrective recycle closeout, lập một packet đủ cụ thể để Codex chốt lát cắt **Stage progress + Boss Gate foundation** và chỉ hỏi Owner đúng quyết định sản phẩm thực sự còn thiếu. Các phương án/spec là PROPOSAL; không tự chuyển thành thiết kế LOCKED hoặc code authorization.

Đọc [TLTD_RECYCLE_CLOSEOUT_REVIEW_20261007.md](TLTD_RECYCLE_CLOSEOUT_REVIEW_20261007.md), [TLTD_GAMEPLAY_GAP_AUDIT_REVIEW_20261005.md](TLTD_GAMEPLAY_GAP_AUDIT_REVIEW_20261005.md) và dùng matrix22rows/8groups đã review làm context. Chỉ đào sâu những integration/checkpoint/content points cần cho candidate này; không audit lại skill/Bun/Companion/equipment hoặc sửa report cũ.

## Baseline và authority

Project E:\code\TLTD. Source/main local và remote Codex đã quan sát393a0487083fd1e311db28cdf07393427769a4de, parent5d38dbbb4e6e49bf94105ba1ed38adff8b938cdd. Canonical review base d1da751bd5b0eeeab37954feff9698ec9d4288a3; publication closeout/spec có commit riêng. Resolve main hiện hành và đọc ACTIVE mới; ghi exact source/authority SHAs riêng. Không giả current source/tested local identical bằng commit name.

Giữ ba dirty docs MANUAL_P09A_CHECKLIST.md, PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md, REVIEW_RESPONSE.md và530untracked entries snapshot review. Không pull/sync overwrite vào dirty E:; read canonical snapshot tách biệt. Local ACTIVE6358bytes/hash842B4F… vẫn cũ, không dùng nó để mở lại recycle/P09.

Inventory dùng `git --no-optional-locks status --porcelain=v1 --untracked-files=all` và read-only log/rev-parse/diff; không refresh index hoặc stage files.

Đọc AI_RULES, CONTINUITY_AND_SYNC_AUTHORITY, ACTIVE_WORK_HANDOFF, CURRENT_DESIGN_AUTHORITY; D17_D18_LOCKED D18.3–.9/.16–.29, D19 authority/reopen gate; amendments precedence; P08_LOCKED và relevant P09 accepted limits. Master roadmap là proposal, không authorization.

Inputs đã khóa, không hỏi lại:

- Current normal threshold **50 legitimate normal monster deaths**, config `monstersRequired`; không5waves. Stage2/3 numbers trong ví dụ không phải production table được duyệt.
- Progress100% giữ tới Boss WIN; Boss LOSE về Normal Screen, không reset progress0 để farm lại. Exact post-loss UX/full Boss resume-vs-reset còn thuộc scope sau.
- P08 normal roster4–5; legitimate full-wave completion; MaxHealth/ATK/DEF x1.01 đúng một lần/completed wave; natural channel completion/loot/modal boundary giữ nguyên.
- Boss config và arena riêng, shared combat engine; Boss60s/Hero immediate loss là full Boss contract tham chiếu, không yêu cầu implement trong foundation này.
- Distinct PvE Stage progress; active TitleBreakthrough không có Stage blocker được chứng minh. Không dùng title index/legacy requiredStageCleared làm Stage authority hoặc tự thêm Title requirement.
- D18.23 PlayerProgress có chapter/currentStage/highestStage/unlockedStages/bossProgress; partial/gate state phải xét persistence/reopen. Không coi hai Prefs viết khi thắng là full contract.

## Nội dung phân tích bắt buộc

### 1. State ownership và đúng death source

Trace legitimate normal death membership/dedupe/reward commit trong BattleManager.HandleEntityDied. Chọn một extension point đề xuất sau accepted membership/dedupe; không thêm death/reward resolver cạnh tranh hoặc global event listener tự cấp progress lần hai.

Đề xuất transition table NormalActive → GatePending → BossGateReady (tên là kỹ thuật đề xuất, chưa lớp/enum đã duyệt). Mỗi row ghi trigger, guard, owner, state persisted/RAM, side effects, signal consumer và invariant.

Reconcile D18 threshold với P08: đề xuất latch GatePending khi death50 nằm giữa wave, hoàn tất roster hiện hành/channel/loot/modal hợp lệ rồi gate; không trim/destroy monster sống hoặc count một death như wave complete. Phân biệt displayed progress clamp100% với remaining legitimate kills/rewards của wave. Nêu cách xử lý normal Hero death/interrupt trước drain; nếu thiếu authority, đánh dấu proposal/TBD và option cụ thể, không tự reset count/gate hoặc suppress existing reward.

### 2. Mọi đường spawn/start/retry/advance

Rà đúng các entry hiện có, xác minh tên/line/current callers:

- GameBootstrap.Start/autoStartBattle.
- BattleManager.EstablishCanonicalEncounterMonsters, PrepareAndStartNormalWave và private wave spawn.
- StartBattle, EndEncounterAndStartNext, RestartBattle.
- CanStartCombat, ExecutePlayerStartCommand.
- AdvanceEncounterAfterLoot và DeferEncounterAdvanceWithoutLoot, delayed callbacks/coroutines/tokens.
- BattleHUD start/auto/pause/death interactions liên quan.

Lập call graph/integration map và guard table; chỉ chặn một loot advance branch chưa đủ. Đề xuất nơi bảo vệ loaded/pending/ready gate để không bypass qua auto start, retry, UI hoặc stale coroutine. Giữ authority encounter/channel/loot/modal của P08/P09; không tự refactor engine.

### 3. Checkpoint, retry và reopen

Đọc source save/load thực; không dựa trên tên SaveManager hoặc giả đã có inventory serialization. Trace RAM `_processedDeaths`, wave roster/IDs, pending loot queue/item, normal wave growth tier và hero retry.

Giải quyết rủi ro cụ thể: lưu progress mỗi death nhưng replay partial wave sau retry/reopen có thể đếm credit/reward lặp trong khi RAM loot queue mất. So sánh các phương án checkpoint tối thiểu, chẳng hạn safe-wave/loot boundary và minimal durable identity/state; ghi cái nào phù hợp contract, cái nào cần scope/decision mới. Không tự chọn chỉ lưu tại wave-end nếu làm mất per-death progress đã khóa, không lén nhận full inventory/loot/crash recovery vào foundation.

Phân biệt logical kill credit, reward commit, loot decision và completed-wave growth; không dùng killcount làm wave tier. Nêu yêu cầu save/load partial/gate state, load-before-bootstrap auto spawn và idempotent init; missing/invalid/future-version handling là proposal có giới hạn, không silently reset data hoặc migrate save thật.

Nếu không thể đáp ứng narrow scope mà không thêm persistence work, báo exact coupling/blocker và một phương án nhỏ có thể review. Không mở rộng tự động hoặc hỏi Owner khi còn có thể phân tích options từ source.

### 4. Data/content và allowlist dự kiến

Inventory đúng Stage/Chapter/Boss/Arena definitions/config/assets hoặc sự vắng mặt trong relevant scope. Ghi actual paths/IDs/load source; không tạo asset hoặc đọc hàng MB skill YAML không liên quan.

Đề xuất minimal fields/schema/version/store owner, PlayerProgress fields và stable identity mapping. Phân biệt existing types với new proposed types. S1/config50 dùng làm development fixture candidate, không tự invent production chapter/stage/boss table hoặc formula.

Exact future changed-path allowlist theo source integration; nếu đề xuất file mới thì ghi NEW/PROPOSAL + trách nhiệm. Không bắt tạo SaveManager/framework mới chỉ vì thiếu tên class. Giữ Resource/Title existing keys/costs/authority; không bổ sung nguồn Material để giải economy gap.

### 5. Trải nghiệm gate-only trung thực

Chỉ functional progress/pending/ready visibility và start controls trong candidate; không finished UI/polish. Chưa có Boss fight thì không có nút Challenge giả spawn Normal, fabricated Boss WIN hoặc reward.

Một gate-only foundation có thể dừng free farm nhưng chưa có đường chơi tiếp. Đưa decision packet về exposure: giữ development-only tới Boss slice, hoặc làm một bounded playable slice khác nếu Owner muốn. Nêu option recommended và tác động sản phẩm, không tự bật main scene/feature flag hoặc viết Boss implementation.

Bun/offline/Companion vẫn là deferred dependencies/gaps. Không claim full Stage loop/idle economy complete nếu chưa triển khai chúng. Boss exit/background/resume, full boss/reward/unlock content và costs/balance là scope sau; không hỏi ngay về các values đã khóa50/60s/regen1Bun20s.

## Deliverables và tiêu chí review

Chỉ tạo artifacts mới `scratch/stage_gate_scope_spec_<runid>`:

1. `STAGE_GATE_SCOPE_SPEC.md`: problem/player result, phạm vi inclusion/exclusion, authority labels, state/transition table, source call graph/guard map, checkpoint/data/load order, content gaps, exact future allowlist và dependency/rollback risks. Một recommendation và alternatives chỉ khi có tradeoff thực.
2. `STAGE_GATE_ACCEPTANCE_MATRIX.md`: planned checks, chưa chạy. Ít nhất49→50/midwave; duplicate/out-of-roster/Hero death; channel+loot/no-loot drain; every start/restart/load bypass; partial/gate reopen/credit identity; once-per-wave growth; missing/versioned data; functional HUD. Mỗi row chỉ đúng evidence type cần sau này, không yêu cầu lặp toàn P08/P09.
3. `STAGE_GATE_DECISION_PACKET.md`: các câu hỏi Owner thật sự cần cho candidate, options/impact/recommendation và cái nào chặn implementation. Label LOCKED INPUT / ACCEPTED BASELINE / SOURCE EVIDENCE / TECHNICAL PROPOSAL / PRODUCT TBD. Không hỏi lại Gold-only/50/Boss60s hoặc active Title blocker không có bằng chứng.
4. `PROVENANCE.md`: source/authority SHAs, exact read paths+bytes/SHA256, before/after Git inventory và original dirty-doc hashes; limitations/unknowns. Hash artifacts trong manifest tách khỏi self-hash document. Không export save dump, snapshot Registry hoặc test histories.

Codex sẽ review packet và chốt scope trước mọi implementation prompt. Đây là spec readiness, không nghiệm thu gameplay mới. Không sửa báo cáo/audit/logs cũ để khớp đề xuất và không tạo ZIP nếu files local đọc được.

## Hard boundaries và điểm dừng

- Không code/assets/meta/scene/save writes, Unity launch/import/compile/tests/Save Guard/recovery. Chỉ file reads và artifacts tài liệu mới.
- Không git pull vào E:, stage/commit/push/stash/reset/clean/checkout overwrite/rebase; không đồng bộ đè dirty local PROJECT_MEMORY.
- Không Boss combat/timer/results/rewards/stage advancement/title changes, Material source, balance, skill rollout, Bun/offline/Companion implementation hoặc UI polish.
- P08/P09-A/P09-B ACCEPTED/LOCKED giữ nguyên; recycle tasks CLOSED/FIX VERIFIED theo closeout mới. Giữ old failures và raw recovery limits, không yêu cầu Owner lặp S01–S05/M1–M7/GUI/P08 hay ZIP.
- Nếu thiếu dữ liệu: hoàn tất mọi trace/options độc lập, ghi exact missing path/value/decision và vì sao chặn; không biến unknown thành DONE. Trả packet cho Codex rồi dừng. Không tự phong LOCKED/implementation approved và không tự yêu cầu Owner chơi thử cho task read-only này.
