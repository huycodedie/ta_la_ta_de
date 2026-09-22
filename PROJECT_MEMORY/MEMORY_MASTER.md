# TLTD AI MEMORY MASTER

## 1. Project identity
- Project: TLTD / Giang Ho Trong Tay
- Engine: Unity + C#
- Development environment: Windows, Google Antigravity IDE
- Local repository: `E:\\code\\TLTD`
- Architecture goal: modular, data-driven, extensible, testable, deterministic where required.

## 2. Mandatory response/audit behavior
- Respond in Vietnamese unless the user requests another language.
- Be direct, technical, structured, and evidence-driven.
- Preferred structure: conclusion -> passed -> failed/missing -> risks/audit -> next action -> prompt when needed.
- Never fabricate PASS, execution, visual verification, regression, or LOCKED status.
- Distinguish: IMPLEMENTED, EXECUTED, PASS, VISUAL VERIFIED, REGRESSION PASS, LOCKED.
- Automated test PASS is not the same as Unity Play Mode PASS.
- Play Mode PASS is not automatically visual verification.
- A report claiming PASS is evidence to audit, not a substitute for actual execution evidence.
- If a required test was not executed, say `NOT EXECUTED`.
- If implemented but not verified, say `IMPLEMENTED — NOT VERIFIED`.
- If visual evidence is missing, say `VISUAL NOT VERIFIED`.
- Only call a milestone LOCKED when all required evidence and regression conditions are actually satisfied.

## 3. Locked-design protection
- D1-D23 are the game's locked design contract.
- The original D1-D23 document supplied by the user is the highest-priority source of truth.
- Later explicit user-approved amendments/revisions supersede older rules when the superseding relationship is proven.
- P01+ behavior that was actually tested and explicitly accepted can supersede older historical implementation descriptions.
- Never silently change, reinterpret, simplify, or replace a locked rule.
- Never invent missing D1-D23 details.
- If a new implementation conflicts with a locked rule and no superseding decision is proven, STOP and report `ARCHITECTURE/DESIGN CONFLICT WITH LOCKED BASELINE`.
- Do not ask again about a decision already present in the locked baseline.
- If the original D1-D23 text is unavailable in a new chat, preserve this rule: do not reconstruct missing D1-D20 from guesswork.

## 4. Architecture rules
- Audit repository before coding a new milestone.
- Find existing managers, authorities, components, ScriptableObjects, EventBus events, tests, and extension points first.
- Do not create duplicate runtime authorities.
- Prefer extending the existing system over creating a parallel system.
- Data that designers may tune should be data-driven/configurable rather than hard-coded when the architecture supports it.
- UI is presentation only and must not become the gameplay authority.
- Do not create a second damage pipeline, status pipeline, skill pipeline, or EventBus.

## 5. Core combat baseline
- Player: 1 main Hero + up to 5 Companions.
- Companion is a CombatEntity with HP, can be attacked, can die, and can respawn under the locked respawn rule. This is the current rule and supersedes older no-HP Pet/satellite wording.
- Hero has HP and Rage.
- Max Rage: 100.
- Hero baseline: HP 1000, ATK 100, DEF 20, Move Speed 5, Attack Interval 1.5s, Attack Range 1.8.
- Monster baseline: HP 500, ATK 50, DEF 10, Move Speed 3, Attack Interval 2.0s.
- Companion baseline attack interval: 2.0s.
- Basic attack Rage gain: 1.
- Rage on damage: 1.
- Default Combo Rate: 1%.
- Default Counter Rate: 1%.
- Default Crit Rate: 1%.
- Baseline damage formula: ATK × multiplier × DefenseModifier.
- Do not invent or alter balance values unless explicitly approved.

## 6. Skills
- Historical D6/D13 material describes Hero action/skill slots; exact historical counting wording must not override later P01+ tested implementation.
- Skills are data-driven and use the existing execution/effect pipeline.
- Skill cooldown and priority rules must remain consistent with the current locked design and tested P01+ behavior.
- Multi-effect skills must consume Rage once and apply cooldown once, not once per effect.
- Ultimate behavior, including its Rage requirement and targeting/dodge rules, must follow the current locked/tested implementation rather than an older prototype example when they differ.

## 7. Banh Bao
- In Normal Monster Battle, 1 Bun/Banh Bao = 1 Hero combat action, including Basic Attack, Skill actions and Ultimate.
- Companion actions do not consume Bun.
- When Bun reaches 0 during Normal Battle: Hero stops, Companions stop, and Normal combat stops.
- Boss fights do not depend on Bun.
- Offline/idle Bun behavior follows the locked D19 rules; do not invent additional behavior.

## 8. Progression
- Hero Level is used for progression/EXP and does not directly increase base Hero stats under the locked design.
- Base stats increase through `Đột phá Danh hiệu` / Title Breakthrough.
- D21 Level Cap is part of the locked progression baseline and must be preserved.
- If Hero reaches a Title Level Cap and cannot Breakthrough, additional EXP is retained/accumulated rather than lost; this supersedes an older historical reset-to-zero answer.

## 9. Loot / D23 baseline
- About 95% of gear comes from chests; 5% comes from stage rewards/events under the current locked baseline.
- Normal monsters share the same Drop Rate.
- Current locked baseline: each normal monster death drops exactly 1 item.
- `Chest Level = Drop Level = Cấp Rơi`; one concept and one runtime authority.
- Item Level is based on current Hero Level with `abs(ItemLevel - HeroLevel) <= 5`.
- Cấp Rơi determines rarity/quality availability and probability.
- The visible nine qualities in the reference UI are NOT the maximum rarity list. Higher qualities may exist.
- A quality at 0% may simply be unavailable/not unlocked at the current Cấp Rơi; it does not mean the quality does not exist.
- Rarity count, higher-rarity names, unlock thresholds, probabilities, and Item Level distribution inside ±5 remain data-driven/TBD unless explicitly locked.
- Equipment has 12 wearable slots: Vũ khí/Kiếm, Mũ, Khăn che mặt, Áo, Quần, Giày, Găng tay, Đai lưng, Áo choàng, Dây chuyền, Nhẫn, Bùa/Ngọc.
- Do not use `SPECIAL` as a player-facing slot name.
- Equipment recycle returns GOLD ONLY; no EXP.
- Base Auto-Recycle: item CP lower than equipped item is eligible; future Preferred Attribute/Affix protection may keep it.
- Previously used example test state: 1,700,000 gold upgrade requirement and 974,470 current gold. Treat these as test/example values unless a source explicitly defines them as permanent balance.

## 10. Status architecture
- `EntityStatusController` is the authoritative runtime status authority.
- `EffectResolver` is the central effect resolution path.
- `SkillExecutor` is the skill execution path.
- `SkillExecutionValidator` validates skill use conditions.
- Shield authority is also integrated into `EntityStatusController` under P07.8.
- Do not create parallel DebuffManager/CCManager/CleanseManager/DispelManager/ShieldManager unless a repository audit proves the existing abstraction cannot support the feature and the user explicitly approves a redesign.

## 11. P07.5 locked
- Debuff / DoT / Status Tick.
- Reported automated: 46/46 PASS.
- Unity Play Mode: 30/30 PASS.
- Master regression through P07.5: PASS.
- P07.5 is LOCKED based on supplied execution evidence.

## 12. P07.6 locked baseline
- Crowd control types: Stun, Root, Freeze.
- Effect Power Tier: A, B, C.
- `StatType.CcResistance`, clamped 0..1.
- Effective CC duration follows: base duration × (1 - resistance).
- 100% resistance blocks CC.
- Anti-CC immunity is checked before CC resistance/application.
- Freeze behaves like Stun for control purposes.
- Freeze Shatter occurs only when the applicable effect has `CanShatterFreeze=true`.
- Action permissions include `CanMove`, `CanBasicAttack`, `CanUseSkill`, `CanUseUltimate`, `CanDash`.
- Status flags include `IsStunned`, `IsRooted`, `IsFrozen`, `IsAntiCCImmune`.
- Movement and attack components enforce status permissions; skill validation enforces skill permission.
- Status modifiers integrate with Rage Gain, Healing Received, Damage Dealt, and Attack Interval.
- Do not duplicate this system.

## 13. P07.7 baseline
- Generic status removal lives in `EntityStatusController`.
- Cleanse removes negative statuses according to target/category/selection rules.
- Dispel removes positive Buffs according to target/category/selection rules.
- Supports specific status, category, count, stacks and configured all-negative/all-positive operations.
- Selection modes include Oldest, Newest, Random, HighestPriority, LowestPriority and All where configured.
- Random test selection uses injected deterministic `RandomRangeProvider`; production randomness must not be weakened.
- Partial stack removal is supported; zero stacks are cleaned up.
- Preserve P07.5 stack/refresh/replace/max-stack semantics.
- Cleansing CC immediately restores permissions.
- Stun/Freeze interrupt behavior remains unchanged; Root does not gain an interrupt side effect.
- Cleansing Freeze must NOT trigger Freeze Shatter.
- Ordinary Cleanse/Dispel must NOT remove Anti-CC immunity unless explicitly defined to target it.
- CC Resistance remains unchanged by Cleanse/Dispel.
- Shield isolation remains intact.
- Reported automated: 55/55 PASS; Play Mode: 35/35 PASS; Master Regression: PASS; 670+ total tests reported.
- Visual status is not inferred from backend test counts.

## 14. P07.8 Shield / Barrier — LOCKED
- Shield authority: `EntityStatusController`.
- Damage calculation authority: `DamageCalculator`.
- HP/damage application authority: `HealthComponent`.
- Shield interception is integrated into the existing `HealthComponent.TakeDamage` pipeline after damage calculation/modifiers and before HP decrement.
- Multiple shield ordering: Priority descending -> StartTime ascending/FIFO -> ShieldId ordinal.
- No production random ordering.
- Stacking policies: Additive, RefreshDuration, Replace, Independent, Ignore.
- Zero shields are cleaned up.
- No per-shield Update/coroutine/GameObject architecture.
- Shield is integrated through the existing damage/effect pipeline and does not create a second damage system.
- `StatusRemovalCategory.Shield = 9`; ordinary Cleanse/Dispel isolation remains intact.
- Shield events are integrated into EventBus.
- Automated: 55/55 PASS.
- Play Mode: 35/35 PASS.
- UI Tests: 5/5 PASS.
- Visual Acceptance V01-V08: PASS.
- Required regression suites: PASS.
- Master Regression P01-P07.8: 100% PASS according to the latest user-provided report.
- P07.8 is LOCKED based on that acceptance evidence.

## 15. P07.8 visual acceptance evidence
- V01 Shield Application: PASS.
- V02 Full Absorption: PASS.
- V03 Partial Absorption: PASS.
- V04 Shield Depletion: PASS.
- V05 Multiple Shields: PASS.
- V06 Real Combat: PASS.
- V07 Expiration/Removal: PASS.
- V08 BattleHUD: PASS.
- UI tests: PASS (5/5).
- Visual verification: PASS.
- These are user-provided acceptance results, not independent execution by the assistant.

## 16. Master Regression — latest official report
The latest user-provided Unity execution log reports 100% PASS across P01 through P07.8, including P07.8 55/55. Reported suites include P02 13/13, P03 15/15, P04 19/19, P05.0 50/50, P05.1 40/40, P05.2 27/27, P05.3 8/8, P05.4 33/33, P05.5 12/12, P05.6 43/43, P05.7 31/31, P05.7.1 22/22, P05.7.2 28/28, P05.7.3 20/20, P05.7.4 28/28, P05.8 25/25, P05.9 20/20, P05.9.1 15/15, P05.9.1 Hotfix 14/14, P05/D24 25/25, TBREQ 20/20, HOTFIX 11/11, CAP_HOTFIX 25/25, COUNT_FIX 14/14, STATE_FIX 20/20, P06 20/20, P07.1 16/16, P07.2 22/22, P07.3 25/25, P07.4 45/45, P07.5 46/46, P07.6 55/55, P07.7 55/55, P07.8 55/55.

Reported regression cleanup fixes:
- Added `CleanTestEnvironment()` to remove leaked singleton/GameObject/EventBus/PlayerPrefs state and load a clean scene before suites and master regression.
- P05.7.1.14 `MonsterAttackResumes` was hardened against random Dodge by allowing repeated ticks according to the reported fix.

## 17. Milestone history
- P06: Play Mode Acceptance 14/14 PASS; visual verification YES; LOCKED.
- P07.1: Automated 16/16; Play Mode 22/22; LOCKED.
- P07.2: Automated 22/22; Play Mode 39/39; LOCKED.
- P07.3: Automated 25/25; Play Mode 25/25; LOCKED.
- P07.4: Automated 45/45; Play Mode 36/36; Master 32/32 suites PASS; LOCKED.
- P07.5: Automated 46/46; Play Mode 30/30; Master P01-P07.5 PASS; LOCKED.
- P07.6: Automated 55/55; Play Mode 35/35; Master PASS; LOCKED.
- P07.7: Automated 55/55; Play Mode 35/35; Master PASS; LOCKED.
- P07.8: Automated 55/55; Play Mode 35/35; UI 5/5; Visual V01-V08 PASS; Master Regression PASS; LOCKED.

## 18. Current project state
- P07.8 is LOCKED.
- The previous P07.8 blocker (missing Shield visual UI) is resolved according to the latest user-provided remediation/acceptance report.
- Do not perform unnecessary P07.8 backend rewrites.
- The next milestone may be P07.9, but it must begin with repository/architecture audit.

## 19. Roadmap
P07.4 Heal + Buff -> P07.5 Debuff + DoT + Status Tick -> P07.6 Debuff + CC + Resistance + AntiCC -> P07.7 Cleanse + Dispel + Status Removal -> P07.8 Shield/Barrier -> P07.9 Advanced Skill Casting (Cast Time/Channel/Interrupt) -> P07.10 AOE/Multi Target -> P07.11 Projectile/Dash/Movement Skill -> P07.12 Advanced Combat Integration.
The roadmap is provisional beyond already-locked milestones; detailed future gameplay is not locked until explicitly approved.

## 20. P07.9 rule
P07.9 may now be prepared because P07.8 is LOCKED. Before implementation, audit SkillExecutor, SkillExecutionValidator, EffectResolver, SkillDefinitionSO, current CC interrupt behavior, Stun/Freeze/Root, EventBus, cooldown, Rage, and multi-effect execution. Extend existing systems; do not create a second interrupt system. Do not invent cast/channel durations, interruption rules, costs, or UI behavior that are not explicitly approved.

## 21. Final operating rule
If evidence and a report disagree, trust concrete evidence. If design and implementation disagree, preserve the latest proven locked design and report the conflict. If a later explicit user decision supersedes an older rule, record it in `DESIGN_CHANGELOG.md`. If something is unknown, say it is unknown rather than inventing it.
