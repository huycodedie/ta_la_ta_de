# TLTD — Review Stage Gate Foundation spec

Ngày 07/10/2026, UTC+07. Reviewer: Codex, Tech Lead/Architect.

**Kết luận: REVIEW COMPLETE / REVISION REQUIRED. Packet chưa đủ điều kiện chốt scope implementation hoặc gửi ba lựa chọn hiện tại cho Owner. Stage/Boss implementation NOT APPROVED.** Đây là review tài liệu và source; không phải nghiệm thu gameplay mới.

Task gốc `SPEC-STAGE-GATE-READONLY-01`; executor đặt tên submission `AUDIT_STAGE_GATE_FOUNDATION_SPEC_20261007`. Cùng một nhiệm vụ đặc tả read-only, không phải audit gameplay mới. Next authorized action: **SPEC-STAGE-GATE-READONLY-01-R1**, theo [prompt revision riêng](PROMPT_ANTIGRAVITY_STAGE_GATE_SPEC_REVISION_20261007.md).

## 1. Inputs và provenance đã kiểm tra

Đã đọc attachment báo cáo và bốn file thực tại `E:\code\TLTD\scratch\stage_gate_scope_spec_20261007_005000`. Bản nhận ban đầu được giữ nguyên; bản sao byte-identical nằm trong workspace Codex `work/stage_packet_received_20261007`, không sửa originals.

| File | Bytes | SHA256 — đã đối chiếu |
|---|---:|---|
| STAGE_GATE_SCOPE_SPEC.md | 24866 | A4D1F5C589D36F1F03BAF5F475BAE2F8354517C6F710A23958C6C28FF0570185 |
| STAGE_GATE_ACCEPTANCE_MATRIX.md | 8911 | 1630CAD6F990E79A88E80FEDAC78B2DC361A2D0861A9A4F08AE07CC8829A3F8F |
| STAGE_GATE_DECISION_PACKET.md | 9191 | CD9405D2F809E263B6839299240393588F99DA53D8123576CFD52FBDC74BDA1B |
| PROVENANCE.md | 5744 | 82D71229EB716F68BC2599E41BC761879A1CD7499CF9A53B526621CC5C72D421 |

Script phụ `get_provenance_hashes.ps1`: 963 bytes / A8DC61FC10E943FAAAED245E4141A67BC3AE69D03115F3C41CA5E26C4AF3FB90, khớp manifest; không chạy script này. SG-01–SG-12 đều **PLANNED / NOT EXECUTED**. Không có kết quả Unity/Stage PASS trong packet.

Review baselines được resolve riêng: canonical memory main **0b538088423df637024abc8f4aded81079e26df1**; game main và E: HEAD **1410af40405a8270f113bd07361015e4588d5cd6**. Game commit này chỉ cập nhật memory so với source parent **393a0487083fd1e311db28cdf07393427769a4de**. Không suy diễn ai đã cập nhật E: hoặc source từ quan hệ parent.

Manifest source có đúng 10 rows: **9/10 khớp file hiện hành**, riêng `CURRENT_DESIGN_AUTHORITY.md` đã đổi từ baseline báo cáo 12442 bytes / 4C70FA… sang 12661 bytes / **07D3C2ECDD3749FAADE15E313EEE9400D32903EA02BAB857F36023D332DC6EBE**. Đây là chênh lệch memory baseline; ba runtime paths trong manifest khớp. Giữ baseline executor như lịch sử có ghi ngày; R1 phải đọc current canonical snapshot riêng, không viết đè dirty local ACTIVE.

Snapshot Codex trước review 07/10/2026 01:24:27 +07: E: HEAD1410af4, **3 tracked modifications + 530 untracked entries**. Ba dirty docs khớp bytes/hash đã ghi: MANUAL_P09A_CHECKLIST.md, PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md, REVIEW_RESPONSE.md. Historical before/after equality của executor chỉ là reported evidence vì packet không chứa raw inventories. Các phép đo trên không chứng minh toàn bộ working tree hoặc lịch sử save.

## 2. Phần giữ lại được

Source map/death membership/dedupe và nhận diện queue/channel integration có ích. Giữ các inputs đã khóa: threshold hiện tại50, code dùng `monstersRequired`; P08 roster4–5/full-wave growth; latch giữa wave là **technical proposal** cần reconcile với P08, không hủy quái còn sống để đạt cổng; hiển thị clamp100%; kết thúc current wave/channel/loot/modal trước gate stop. Boss combat/Bun/offline/Companion/skill rollout/Material source/UI polish vẫn ngoài candidate.

Packet giữ nhãn read-only/proposal và không tự chạy test. Những điểm này được ghi nhận; không đồng nghĩa state/schema/checkpoint đã được phê duyệt.

## 3. Các lỗi chặn readiness

### R1-01 — Wave checkpoint không bảo đảm exactly-once reward hoặc không mất loot

Spec §6.2 row170 và Decision3A khẳng định rủi ro thưởng lặp bằng0, loại bỏ100% mất loot/credit. Source bác bỏ bảo đảm đó:

- `Assets/_Game/Progression/ProgressionManager.cs`: death handler437–454 cấp EXP qua AddExp; SaveState được gọi251 và ghi Level/EXP/Title PlayerPrefs374–379. `HasAwardedExp` thuộc instance RAM.
- `Assets/_Game/Progression/ResourceManager.cs`: AddGold97–102 ghi SaveState; SaveState54–58 ghi Gold/Material và Save; dismantle148 gọi AddGold. Gold từ **quyết định Tách**, không mặc định mỗi normal death.
- `Assets/_Game/Drop/DropSystem.cs` tạo EquipmentInstance209 và **đã AddItem vào Inventory215 trước khi return**; `BattleManager.cs` queue940, death895–907 hủy loot lifecycle/xóa queued và displayed refs, không tự xóa item đã thêm vào Inventory. Inventory/equipment/pending loot không được serialize bởi proposal này.
- `_processedDeaths`, roster IDs, wave completion/tier hiện hành thuộc RAM; tạo monsters mới sau reopen không kế thừa dedupe của instance cũ.

Chỉ delay Stage save đến cuối wave trong khi EXP/Gold đã save trước đó sẽ để reward và checkpoint lệch nhau. Replaying wave không tự đảo ngược EXP/Gold đã lưu; equipment/pending items có thể mất vì chưa có durable representation. Đây là **static architectural finding**, không phải Codex đã chạy crash/reopen. EXP writes đã được chứng minh từ source; correct cold-start EXP restore **chưa được chứng minh**: Progression.Start gọi InitializeProgression mặc định, chưa tìm thấy production caller tự động LoadState trong trace. ResourceManager có load ở Awake. Ghi load-order gap để R1 phân tích, không tự sửa hoặc mở lại P09/recycle.

State table§4 nói persist từng death/GatePending; §6/Decision3A chỉ persist wave boundary. Phải thống nhất một policy và ghi rõ cửa sổ rollback/loss/replay. Save Guard là bảo vệ baseline khi test; Diff0 không phải transaction/recovery của gameplay. Per-death persistence cũng không bắt buộc spawn1–3 quái; đó là false dichotomy cần phân tích lại.

**Disposition:** không chốt lựa chọn3A/3B. R1 phải trace persist order, failure windows và feasibility; nếu narrow slice cần mở rộng reward/inventory persistence, báo exact dependency để Codex chốt lại scope, không tự implement.

### R1-02 — Boss LOSE bị dẫn sai và Hero retry bị thay nghĩa

Decision2A dẫn D18.19 thành BossLOSE→NormalScreen Progress0/50. D18.29 rows433–436 giữ100% đến BossWIN, không rollback normal rewards; D19.14 giữ StageN100% khi reopen và D19.20 chỉ nói BossLOSE về NormalScreen. Không hỏi Owner lại về reset0 dưới danh nghĩa lựa chọn cùng contract.

Same-session Hero Start trong `BattleManager.ExecutePlayerStartCommand`817–850 **resume surviving roster**; chỉ prepare wave khi không còn living member. `RestartBattle` là luồng riêng có thể tạo lại wave. Packet tự chọn Start tạo fresh wave khi GatePending mà không mô tả delta với source hiện hành. Tách rõ death/Start/resume, explicit Restart, reload/reopen, final-kill/channel/loot interruption; preserve baseline hoặc đánh dấu exact proposed integration change. Queue clearing là source evidence, không tự suy thành một quyết định sản phẩm về mất loot đã khóa. Boss exit/reset/resume chi tiết vẫn TBD ngoài foundation.

### R1-03 — GateReady phải có một completion authority và đủ guard routes

Guard `EndEncounterAndStartNext` thấy Pending→Ready không chứng minh legitimately complete wave: method public không tự bảo đảm tất cả members chết/channel/loot đã drain. Định nghĩa một completion predicate/owner: wave completion hợp lệ, zero living members, finishing requests hoàn tất theo accepted lifecycle, displayed item/queue/modal/pause release và đúng encounter token. Không hủy coroutine channel chỉ để ép Ready.

Death hook count nằm sau membership916/dedupe919/member-mark924. Queue940 ở trong `if(droppedItem != null)` kết thúc942; hook phải **ngoài nhánh optional drop**, trước GetNextLivingMonster945, để death không rơi đồ vẫn count. Queue append không chứng minh EXP đã commit: EXP là authority/listener riêng cần trace ordering.

Map hiện tại thiếu private `SpawnAndStartNormalWaveInternal`, public `SpawnMonster`/`RegisterMonster` và `CanStartCombat` như các guard/integration paths riêng. `Monster.Start` registration và direct instantiation phải được phân loại; sửa hướng calls StartBattle/StartCombatAfterHeroDeath. Guard creation ở Pending/Ready, resume current members và stale deferred transitions phải nhất quán; không dùng chỉ presence StageManager làm release gate.

### R1-04 — Product packet có lựa chọn ngoài scope và attribution sai

Decision1A gọi dev-only nhưng thêm ReplayStage1/reset tiến độ cho normal player, không định nghĩa build/session/store isolation. Không đưa production reset control vào foundation. Dev fixture phải được mô tả thành scene/store/session riêng, real save không bị tác động; activation/exposure còn proposal. Free-farm sau Ready là deviation D18/D19, không phải phương án tương đương compliant.

Chưa có Boss implementation thì HUD phải báo endpoint trung thực/không khả dụng. Không fake Challenge, redirect sang Normal để giả Boss hoặc fabricated WIN. Các recommendations đang ghi “ANTIGRAVITY / TECH LEAD” phải sửa thành đề xuất executor chờ review. Codex chưa endorse checkpoint/exposure đó.

### R1-05 — Schema, wiring và rollback chưa reviewable

D18.23 yêu cầu PlayerProgress tối thiểu chapter/currentStage/highestStage/unlockedStages/bossProgress. Bốn keys độc lập hiện tại không đủ: cần mapping/stable IDs, version, highest semantics, wave growth/checkpoint identity và relationship với pending/reward state theo policy đề xuất. Không đòi implement Boss/unlock chỉ để có schema; fields/deferred responsibilities phải rõ. Stage/chapter/name/boss examples chỉ là fixture proposals, không production content đã duyệt.

Load phải trước bootstrap spawn, idempotent và giữ unknown/versioned saved data; manager/config missing không được silently farm xuyên savedReady hoặc reset100%. No-stage legacy fixture fallback phải phân biệt với corrupted/ready player record.

Allowlist thực tế **3 existing + 4 new**, không3+3; thiếu `.meta`, asset discovery/loading/wiring, HUD exposure/scene/builder choices. Cần exact proposed paths/dependencies sau khi chốt kiến trúc, không lén mở scenes. Revert code không tự xóa persistent keys/asset links: rollback code, feature disable và data preservation là ba việc khác nhau; không reset dirty tree hoặc restore Registry để rollback feature.

## 4. Sửa matrix mà không chạy lại nghiệm thu đã đóng

| Case | Correction cần làm trong R1 |
|---|---|
| SG01–03 | Thêm no-drop count; EXP/drop và Gold-on-dismantle đúng authority; duplicate rejection không bắt buộc log mới; không đồng nhất silent guard với global EXP dedupe. |
| SG04 | Separate same-session survivor resume, explicit Restart, reopen, interruption after final death; không tự fresh-wave/reset. |
| SG05 | EditMode/manual API/dummy modal chỉ chứng minh states đã assert. Natural cast/frame/modal drain cần bounded PlayMode evidence **sau khi** implementation được duyệt, chưa chạy bây giờ. |
| SG06 | Dead members được giữ trong registry; `activeMonsters.Count==0` không đúng oracle bắt buộc. Assert zero living/new spawn, completion/gate state và no duplicate growth. |
| SG07–08 | Cover toàn bộ routes/callback tokens, load-before-spawn; no available Boss/redirect promise ngoài scope. |
| SG09 | Fixture deterministic4+4+5 mới assert13. Completed3→**NextWaveStatMultiplier=1.01^3**; current wave3 vẫn1.01^2 tới khi wave4 spawn. Count kill không thay wave tier. |
| SG10 | Thực sự persist→destroy session state→load/recreate trong store cô lập; assert Stage/EXP/Gold/item/queue/growth theo policy và failure windows. SaveGuardDiff0/null/dedupe không tự chứng minh reopen consistency. |
| SG11 | Missing/invalid/future version cần error injection + preserved bytes/no destructive writes, không screenshot thay thế. |
| SG12 | Data-driven denominator `monstersRequired`, truthful Pending/Ready; values telemetry và natural visual/frame evidence phân biệt. |

Đây là sửa **planned checks**. Không đổi kết quả raw cũ, không đòi full P08/P09 suites hay Owner lặp S01–S05/M1–M7/GUI. Sau approval/code delta, chỉ integration risks mới sẽ quyết định tests nào cần.

## 5. Scope của bước tiếp theo

**SPEC-STAGE-GATE-READONLY-01-R1 = AUTHORIZED ANALYSIS / PROMPT PREPARED.** Antigravity tạo packet mới, không overwrite submission hiện tại. Phải trả một recommendation có căn cứ cùng alternatives thực sự khác nhau:

1. Fixture gate ephemeral/store cô lập: chứng minh state/guard integration nhưng không claim durable player progression/full D18.23.
2. Stage-only checkpoint: nêu reward/progress divergence và RAM loot loss; không gọi crash-safe/exactly-once hoặc full contract nếu không đạt.
3. Bounded durable encounter/stage/receipt design: liệt kê exact additions/coupling với existing EXP/Gold/loot/growth, transaction/failure model; nếu vượt narrow slice, trả blocker/dependency và fallback candidate nhỏ hơn để Codex chọn, không mở feature mới.

Không bắt xây full SaveManager hoặc chọn framework vì thiếu class name. Không tự thay nguồn Gold/Material, defer existing EXP đến cuối wave, implement offline, replay-reset controls hoặc full inventory persistence. Technical feasibility được giải trước; chỉ sau packet coherent mới gửi Owner lựa chọn sản phẩm thật sự còn chặn. Không thiếu dữ liệu Owner để sửa các lỗi đã chứng minh này.

## 6. Continuity và ranh giới còn nguyên

P08/P09-A/P09-B **ACCEPTED / LOCKED** theo phạm vi cũ; F-RECYCLE-GOLD-ONLY-01 **CLOSED / FIX VERIFIED WITH EVIDENCE LIMITS**, F-RECYCLE-HARNESS-SAFE-ENTRY-01 **CLOSED / FIX VERIFIED**. Review này không mở lại recycle hoặc balance.

P08 Gate2 **135/137, exit1**, accepted legacy exceptions07/09; P09-A E2 **SCENARIO_PASS_EXIT_TIMEOUT / wrapper exit2**; S01–S05/M1–M7/GUI USER_VERIFIED vẫn nguyên. Recovery REC-P09B-20261004-151227 là snapshot04/10 15:14:04+07,24/24 names/kinds/truncated digests; raw Restore/execution **PARTIAL**, Compare/preflight **METADATA_ONLY**. PID25360 **USER_REPORTED_PASS / RESTORE_UNRESOLVED** không đổi. Không tạo chứng minh live Registry/full bytes bằng lời nói.

Codex không chạy Unity/tests/Save Guard, không đọc Registry/save sống, không sửa E: source/assets/scenes/docs hoặc stage/pull/reset/stash/clean tại E:. Chỉ docs review/prompt/status được publish ở canonical memory và game memory mirror qua checkout tách biệt; remote verification có receipt riêng. Không source/Unity assets/raw evidence vào memory repo. Không playtest/retest/recovery rerun/ZIP cho revision read-only này.
