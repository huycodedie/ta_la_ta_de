# TLTD — lát cắt Stage → Boss tiếp theo

14/10/2026, UTC+07. **READ_ONLY_SCOPE_PROPOSAL / PROPOSED_NOT_APPROVED.** Không sửa E, không chạy Unity, không đổi dữ liệu sản phẩm. Source/build đang được root kiểm chứng; tài liệu này không nâng trạng thái các phép thử đó.

## Luật có sẵn: dùng trực tiếp, không hỏi lại

- Normal liên tục, không timer; hiện 50 legitimate normal deaths = 100%, nhưng đọc `stage.monstersRequired`. Boss có Entity/config và arena riêng, dùng chung combat engine; timer60s, Hero chết là thua ngay. Gate100% giữ đến Boss WIN; thua quay về Normal Screen, không rollback normal rewards. D18: `work/recycle_remote_memory_20261006/D17_D18_LOCKED.md:194,211,251,263,268,397,423`.
- Bấm `Khiêu chiến Boss` mới vào Boss; offline không đánh Boss/không vượt gate. Boss WIN mở Stage tiếp; Stage không có hard-cap cố định. D19: `D1_D23_LOCKED.md:104,115,157`; `PROJECT_MEMORY/D20_LOCKED.md:4`.
- **Normal kill = 10 EXP đã LOCKED**, không phải một reward còn TBD: `D1_D23_LOCKED.md:215`, `PROJECT_MEMORY/D21_LOCKED.md:10`. Current baseline mỗi normal death1item: `PROJECT_MEMORY/D23_LOCKED.md:6`; giữ P08 roster4–5, full-wave growth×1.01, finishing channel/loot/modal contract. Không thay bằng “50 quái = 5 wave”.
- Recycle Gold-only (`D1_D23_AMENDMENTS_LOCKED.md:201`); Boss độc lập Bun. Normal1Bun/1HeroAction, Bun0 cả Hero/Companion/Normal dừng, regen1/20s, tự resume. Companion hồi25s/configurable: `D1_D23_LOCKED.md:43,49,69,73`; D18:277.
- Exit đang Boss = LOSE, gate100% để retry, không win reward/advance: `outputs/DECISION_BOSS_EXIT_LOSE_RETRY_GATE_20261013.md:5`. Background/focus tạm thời chưa được coi là exit (:13).

## Hiện trạng source và dependency thật

`E:/code/TLTD/Assets/_Game/Core/BattleManager.cs:915` đã có roster/dedupe/death/queue và whole-wave completion, nhưng chưa Stage count/gate/Boss mode. `ProgressionManager.cs:41,435` có `currentStageCleared`/mock setter, không phải D18.23 PlayerProgress. Scan file và source chưa thấy runtime Stage/Boss/Bun/offline implementation hoặc Stage/Boss content configs. Equipment snapshot mới chỉ bag/equipment/Gold/Material, không được gọi là checkpoint chung của cả Stage/EXP/reward.

`MonsterConfig.asset:16` đang là normal500HP/50ATK/10DEF, interval2s; `HeroConfig.asset:16`1000HP/100ATK/20DEF, interval1.5s. Đây là dữ liệu source hiện có, không phải Boss balance đã duyệt. Boss không thể chỉ rename Monster: `Monster.Start` tự register normal và `ProgressionManager.HandleEntityDied` có normal EXP authority; phải tách role và routes trước khi gắn Boss reward.

## Implementation có thể làm độc lập với balance

1. StageDefinition + StageRuntime/PlayerProgress có identities `chapter/currentStage/highestStage/unlockedStages/bossProgress`, schema/version/validation; thêm completed-wave/growth checkpoint cần thiết để không reset hoặc nhân growth giả. SavedReady/corrupt/future không được fallback thành endless normal farming. Chọn và review checkpoint/receipt cụ thể, không mặc định phải viết global SaveManager.
2. Count ở sau membership/dedupe và ngoài optional drop. Technical proposal: latch khi đạt50 giữa wave; giữ sống phần còn lại của wave, clamp progress100%; chốt Ready sau legitimate whole-wave, natural finishing và loot/modal drain. Không carry số mạng vượt threshold sang Stage mới, không trim survivors, giữ thưởng hợp lệ/P08 growth. Đây là cách reconcile kỹ thuật đề xuất, chưa thành product LOCK.
3. Guard spawner/start/restart/revive/registration và deferred advance trước side effects; GateReady chỉ expose Challenge, không giả nút Boss nhưng chạy normal. Checkpoint BossInProgress ghi thành công trước combat; cold-start interrupted Boss thành Lose/gate100%; Win chỉ commit reward/unlock một lần qua receipt được review. Không dùng inventory GUID để tự nhận exactly-once toàn save.
4. Boss role/arena/BattleRules tách riêng nhưng gọi Health/CombatResolver/SkillExecutor hiện có, timer và một result owner. Normal EXP/drop không chạy cho Boss; Boss không tăng normal kill count hoặc P08 completed-normal-wave growth. Boss single-target skill/Ultimate data đi qua pipeline chung; không rollout AOE/projectile/dash trên toàn skill assets.
5. Functional HUD progress, Challenge, timer, result/retry và state restore trong shell có sẵn; rồi kiểm delta natural frames/cold restart. Không yêu cầu nghiệm thu lại P08/P09 hoặc ZIP cũ.

Có thể code schema/gates/rules/validation/config editors trên các luật đã có mà chưa chọn reward/stat. Config chưa đủ thì fail-closed/không cho bắt đầu Boss, kèm lý do rõ; trạng thái này không được gọi “Boss playable hoàn chỉnh”.

## First playable slice đề xuất: Stage1 → Boss1 → mở Stage2

**Tất cả số mới dưới đây chỉ là bảng review cho playtest, chưa được apply/approve.** Chỉ1Boss và2Stage records; không tạo hard-cap2Stage. Slice hoàn tất khi Boss1Win mở Stage2, tiến độ0/50 và normal bắt đầu; Boss2/content tiếp theo thuộc đợt sau.

| Record | Giá trị đề xuất để review |
|---|---|
| Stage1 | required50; normal profile hiện có; Stage1 vào game; Boss1; nextStage2 |
| Boss1 | riêng config/Entity; HP2500 (=5×normal baseline), ATK50, DEF10, interval2s, speed3, range1.8; timer60s đã locked |
| Boss1 kit | Basic +1single-target skill +1Ultimate qua pipeline chung; draft skill1.2×ATK/CD6s, Ultimate2×ATK/Rage80; không CC/AOE/projectile/dash. Đây là content mới đề xuất, không bật assets Hero đại trà; không nhận Basic-only là final D19 Boss kit. |
| Boss1 rewards | draft Gold1000 +EXP100, Material0, không item/chest currency mới; chỉ một lần cho Boss1Win. Không dùng con số này để suy ra nguồn Material đã giải quyết. |
| Entry/retry state | đề xuất HP đầy mỗi lần chủ động Challenge/Retry; Rage/cooldowns còn lại giữ; revive-once reset theo battle attempt mới. Cần Owner chọn rõ vì chưa có rule Boss-entry này. |
| Arena | một BossArenaDefinition/owned combat root riêng, cùng scene được phép; layout/visual dùng baseline, không đợi art mới. Geometry/camera thuộc implementation review, không cần bắt Owner chọn kiến trúc scene. |
| Stage2 | required50; normal profile tham chiếu được cấu hình, bắt đầu0/50; đề xuất unlockHeroLevel4 (sau50kill×10EXP đủ cấp4 theo EXP hiện hành); threshold level này chưa locked. Không hard-code number2 là cuối game. |

Bảng chọn HP để Boss baseline dài hơn normal, không lấy số game tham chiếu. Chưa có playtest chứng minh pacing/balance; gear/drop có thể khiến cần chỉnh config. Không gọi đây là production balance.

## Product TBD thật sự và thời điểm cần hỏi

| Mục | Căn cứ / tác động | Chặn lúc nào |
|---|---|---|
| Boss stat/skill/reward + Stage unlock thresholds | D18:283–358 để data-driven/exact rewardsTBD; D20 locked có level condition nhưng không threshold. Bảng trên chỉ proposal. | Trước bật Boss1 playable và phát thưởng sản phẩm; không chặn viết gate/schema. |
| HP/Rage/cooldown/revive-once khi vào/retry Boss | Không tìm thấy quyết định Boss-entry cụ thể trong D18/D19/amendments/decision13-10; normal Restart hồi HP không tự thành policy Boss. | Trước mở Challenge/Retry thật. Chỉ hỏi lựa chọn trải nghiệm, không hỏi lại60s/gate/exit. |
| Bun initial/max và maxOfflineTime, mô hình hiệu suất offline | MaxBun và capoffline explicitTBD: `D1_D23_LOCKED.md:79,97`. InitialBun chưa được nguồn chốt; ví dụ20Bun/max100 (:154) không phải giá trị mặc định. | Trước bật Bun/offline; không chặn Stage/gate/Boss foundation. Không cấp vô hạn Bun để gọi fullgame. |
| Material production source | Runtime chỉ thấy `AddMaterial` từ debug `EquipmentDropDebugUI.cs:377`; recycle Gold-only. Tier1 hiện đòi5Material (`Resources/Data/LootTiers/LootTier_01.asset:25`), initialResource10 chỉ là source baseline. | Vòng nâng cấp liên tục/fullgame; không tự thêm Boss/wave reward hoặc giảm cost. Cần một economy slice và ngân sách source/sink được duyệt. |
| Tâm Pháp nâng level/cost/đổi hệ | `MindMethodManager.cs:335,362` chỉ Set/AddLevel, không production upgrade caller/cost config. `MindMethodDefinitionSO.cs:114` cóMaxLevel vàpassive, không cost table. D12-D14:147,240 còn mở skill/level/tree/martial interaction. | Trước expose upgrade/method switch; không chặn authored skill-unlock hiện đã nối. Không coi SetLevel miễn phí là gameplay được duyệt. |
| Mobile background/focus | Quyết định13-10:13 phân biệt với exit. | Trước lifecycle nghiệm thu Android; build APK không chứng minh pause/resume. |

**Authority discrepancy cần giữ riêng, không tự sửa/chặn build hiện tại:** `PROJECT_MEMORY/D21_LOCKED.md:23` và handoff16-09:71 ghi Title breakthrough không tốn Gold; actual `TitleBreakthrough_00.asset:29,36,42` có Gold5000/Material30 và `TitleBreakthroughManager.cs:483` consume configured resources. Đã search current authority, amendments, changelog/new decisions, P08/P09 records và E memory; chưa thấy later explicit Gold-cost approval cụ thể. Những review sau vẫn yêu cầu giữ existing Title costs trong task hẹp, không tự biến chúng thành product LOCK. Gắn **AUTHORITY_DISCREPANCY_UNRESOLVED** để làm rõ trong economy/Title scope; không mở lại accepted milestone, không tự giảm giá, không lấy30Material làm bắt buộc cho Boss reward mới.

**Khối Owner cần quyết định gần nhất:** duyệt/sửa đúng bảng First playable slice, tập trung Boss1 profile/kit/reward, entry/retry resource policy và Stage2 unlock threshold. Bun/offline/Tâm Pháp upgrade/Material roadmap không cần hỏi chung ngay khi chưa triển khai tới chúng. Root tiếp tục gate/checkpoint code độc lập; P08/P09/recovery/raw limits giữ nguyên.
