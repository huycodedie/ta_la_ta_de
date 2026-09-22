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
- Never silently change, reinterpret, simplify, or replace a locked D1-D23 rule.
- Never invent missing D1-D23 details.
- If a new implementation conflicts with a locked rule, STOP and report `ARCHITECTURE/DESIGN CONFLICT WITH LOCKED BASELINE`.
- Do not ask again about a decision already present in the locked baseline.
- If the original D1-D23 text is unavailable in a new chat, preserve this rule: do not reconstruct missing D1-D20 from guesswork. Obtain the original source once if a specific missing rule is required.

## 4. Architecture rules
- Audit repository before coding a new milestone.
- Find existing managers, authorities, components, ScriptableObjects, EventBus events, tests, and extension points first.
- Do not create duplicate runtime authorities.
- Prefer extending the existing system over creating a parallel system.
- Data that designers may tune should be data-driven/configurable rather than hard-coded when the architecture supports it.
- UI is presentation only and must not become the gameplay authority.
- Do not create a second damage pipeline, status pipeline, skill pipeline, or EventBus.

## 5. Core combat baseline
- Baseline combat is fixed-position 1v1 rather than free-roaming action combat.
- Player: 1 main Hero + up to 5 Pets/satellites.
- Pets act like turrets/satellites and have no HP under the current locked design.
- Hero has HP and Rage.
- Max Rage: 100.
- Hero baseline: HP 1000, ATK 100, DEF 20, Move Speed 5, Attack Interval 1.5s, Attack Range 1.8.
- Monster baseline: HP 500, ATK 50, DEF 10, Move Speed 3, Attack Interval 2.0s.
- Basic attack Rage gain: 1.
- Rage on damage: 1.
- Default Combo Rate: 1%.
- Default Counter Rate: 1%.
- Default Crit Rate: 1%.
- Baseline damage formula: ATK × multiplier × DefenseModifier.
- Do not invent or alter balance values unless explicitly approved.

## 6. Skills
- Hero skill structure has 5 skill types/slots: Normal Attack, Skill, External Skill, External Skill 2, Ultimate.
- Ultimate cannot be dodged and can crit.
- Skills are data-driven and use the existing execution/effect pipeline.
- Skill cooldown and priority rules must remain consistent with the locked design.
- Multi-effect skills must consume Rage once and apply cooldown once, not once per effect.

## 7. Banh Bao
- In the normal monster screen, 1 Banh Bao = 1 skill usage.
- If there is no Banh Bao, the Hero cannot use a skill even if its cooldown is ready.
- Banh Bao does not affect Boss fights.
- Offline/idle behavior must follow the locked design and must not be invented.

## 8. Progression
- Hero Level is used for progression/EXP and does not directly increase base Hero stats under the locked design.
- Base stats increase through `Đột phá Danh hiệu` / Title Breakthrough.
- D21 Level Cap is part of the locked progression baseline and must be preserved.

## 9. Loot / D23 baseline
- About 95% of gear comes from chests; 5% comes from stage rewards/events under the current locked baseline.
- Normal monsters share the same Drop Rate.
- Each normal monster death drops exactly 0 or 1 item.
- Chest Level is unified with Drop Level.
- Chest Level determines item rarity/tier.
- Loot rarity/tier/probability and chest progression are data-driven/configurable.
- Previously used example test state: 1,700,000 gold upgrade requirement and 974,470 current gold. Treat these as test/example values unless the original D23 source explicitly defines them as permanent balance.

## 10. Status architecture
- `EntityStatusController` is the authoritative runtime status authority.
- `EffectResolver` is the central effect resolution path.
- `SkillExecutor` is the skill execution path.
- `SkillExecutionValidator` validates skill use conditions.
- Do not create parallel DebuffManager/CCManager/CleanseManager/DispelManager/ShieldManager unless a repository audit proves the existing abstraction cannot support the feature and the user explicitly approves a redesign.

## 11. P07.5 locked
- Debuff / DoT / Status Tick.
- Reported automated: 46/46 PASS.
- Unity Play Mode: T01-T30 PASS.
- Master regression P01-P07.5: PASS.
- P07.5 is LOCKED.

## 12. P07.6 locked baseline
- Crowd control types: Stun, Root, Freeze.
- Effect Power Tier: A, B, C.
- `StatType.CcResistance`, clamped 0..1.
- Effective CC duration follows the existing resistance rule: base duration × (1 - resistance).
- 100% resistance blocks CC.
- Anti-CC immunity is checked before CC resistance.
- Freeze behaves like Stun for control purposes.
- Freeze Shatter occurs only when the applicable damage/effect has `CanShatterFreeze=true`.
- Root permits skills while restricting movement according to the established permission rules.
- Action permissions include `CanMove`, `CanBasicAttack`, `CanUseSkill`, `CanUseUltimate`, `CanDash`.
- Status flags include `IsStunned`, `IsRooted`, `IsFrozen`, `IsAntiCCImmune`.
- Movement and attack components enforce status permissions; skill validation enforces skill permission.
- Status modifiers integrate with Rage Gain, Healing Received, Damage Dealt, and Attack Interval.
- Do not duplicate this system.

## 13. P07.7 baseline
- Generic status removal lives in `EntityStatusController`.
- Cleanse removes negative statuses (Debuff, DoT, CC) according to target/category/selection rules.
- Dispel removes positive Buffs according to target/category/selection rules.
- Supports specific status, category, count, stacks, all negative/all positive as defined.
- Selection modes: Oldest, Newest, Random, HighestPriority, LowestPriority, All.
- Random test selection uses injected deterministic `RandomRangeProvider`; production randomness must not be weakened.
- Partial stack removal is supported; zero stacks are cleaned up.
- Preserve P07.5 stack/refresh/replace/max-stack semantics.
- Cleansing CC immediately restores permissions.
- Stun/Freeze interrupt behavior remains unchanged; Root does not gain an interrupt side effect.
- Cleansing Freeze must NOT trigger Freeze Shatter.
- Ordinary Cleanse/Dispel must NOT remove Anti-CC immunity unless an explicitly defined effect says so.
- CC Resistance remains unchanged by Cleanse/Dispel.
- EventBus includes status removal/cleanse/dispel/CC-cleanse events as implemented.
- Reported automated: 55/55 PASS; Play Mode: 35/35 PASS; Master Regression: PASS; 670+ total tests reported.
- Treat visual status as verified only if actual visual evidence exists; do not infer it from backend test counts.

## 14. P07.8 Shield / Barrier — LOCKED
- Shield authority: `EntityStatusController`.
- Damage calculation authority: `DamageCalculator`.
- HP/damage application authority: `HealthComponent`.
- Shield interception is integrated into the existing `HealthComponent.TakeDamage` pipeline after damage calculation/modifiers and before HP decrement, as audited/reported for P07.8.
- Multiple shield ordering is deterministic: Priority descending -> StartTime ascending/FIFO -> ShieldId ordinal.
- No production random ordering.
- `ShieldTypes.cs`, `ShieldStackPolicy`, `ShieldAbsorbResult`, `RuntimeShieldInstance`, and `ShieldEffectDefinitionSO` are part of the P07.8 implementation.
- Stacking policies: Additive, RefreshDuration, Replace, Independent, Ignore.
- Zero shields are cleaned up.
- No per-shield Update/coroutine/GameObject architecture.
- Shield is safe for zero/negative damage, expired/removed shields, dead/destroyed targets, and multi-effect skills.
- Shield interacts through the existing damage/effect pipeline and does not create a second damage system.
- Shield is represented in status-removal categorization as `StatusRemovalCategory.Shield = 9`, while Cleanse/Dispel isolation remains intact.
- Shield events are integrated into EventBus.
- P07.8 Automated: 55/55 PASS.
- P07.8 Play Mode: 35/35 PASS.
- P07.8 UI Tests: 5/5 PASS.
- P07.8 Visual Acceptance V01-V08: PASS.
- Required regression suites: PASS.
- Master Regression P01-P07.8: 100% PASS, including P07.8 55/55.
- Latest user-provided acceptance report explicitly states the Shield & Barrier system is 100% complete and integrated with no regression.
- P07.8 is now LOCKED based on the latest reported acceptance evidence.

## 15. P07.8 visual acceptance evidence
- V01 Shield Application: PASS.
- V02 Full Absorption: PASS.
- V03 Partial Absorption: PASS.
- V04 Shield Depletion: PASS.
- V05 Multiple Shields: PASS.
- V06 Real Combat: PASS.
- V07 Expiration/Removal: PASS.
- V08 BattleHUD: PASS.
- Visual verification: PASS.
- UI tests: PASS (5/5).
- Do not reinterpret the above as independent execution by the assistant; it is the latest user-provided official execution report.

## 16. Master Regression — latest official report
The latest user-provided Unity execution log reports 100% PASS across P01 through P07.8. Reported suite results include:
- Prototype 02: 13/13
- Prototype 03: 15/15
- Prototype 04: 19/19
- Prototype 05.0: 50/50
- Prototype 05.1: 40/40
- Prototype 05.2: 27/27
- Prototype 05.3: 8/8
- Prototype 05.4: 33/33
- Prototype 05.5: 12/12
- Prototype 05.6: 43/43
- Prototype 05.7: 31/31
- Prototype 05.7.1: 22/22
- Prototype 05.7.2: 28/28
- Prototype 05.7.3: 20/20
- Prototype 05.7.4: 28/28
- Prototype 05.8: 25/25
- Prototype 05.9: 20/20
- Prototype 05.9.1: 15/15
- Prototype 05.9.1 Hotfix: 14/14
- Prototype 05 / D24: 25/25
- Prototype 05 / TBREQ: 20/20
- Prototype 05 / HOTFIX: 11/11
- Prototype 05 / CAP_HOTFIX: 25/25
- Prototype 05 / COUNT_FIX: 14/14
- Prototype 05 / STATE_FIX: 20/20
- Prototype 06: 20/20
- Prototype 07.1: 16/16
- Prototype 07.2: 22/22
- Prototype 07.3: 25/25
- Prototype 07.4: 45/45
- Prototype 07.5: 46/46
- Prototype 07.6: 55/55
- Prototype 07.7: 55/55
- Prototype 07.8: 55/55

Reported regression cleanup fixes:
- Added `CleanTestEnvironment()` to remove leaked singleton/GameObject/EventBus/PlayerPrefs state and load a clean scene before suites and before master regression.
- P05.7.1.14 `MonsterAttackResumes` was made robust against random Dodge by allowing up to 5 ticks, matching the nearby stable test strategy, removing accidental RNG dependence.

## 17. Milestone history
- P06: Play Mode Acceptance 14/14 PASS; visual verification YES; LOCKED.
- P07.1: Automated 16/16; Play Mode 22/22; LOCKED.
- P07.2: Automated 22/22; Play Mode 39/39; LOCKED.
- P07.3: Automated 25/25; Play Mode 25/25; LOCKED.
- P07.4: Automated 45/45; Play Mode 36/36; Master 32/32 suites PASS; LOCKED.
- P07.5: Automated 46/46; Play Mode T01-T30 PASS; P06 14/14 PASS; Master P01-P07.5 100%; LOCKED.
- P07.6: Reported Automated 55/55; Play Mode 35/35; P06 14/14; P07.5 46 automated / 30 Play Mode; Master 100%; reported LOCKED.
- P07.7: Reported Automated 55/55; Play Mode 35/35; Master 100%; visual status must only be considered verified when evidence exists.
- P07.8: Automated 55/55; Play Mode 35/35; UI 5/5; Visual V01-V08 PASS; Master Regression 100%; LOCKED.

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
If evidence and a report disagree, trust the concrete evidence. If design and implementation disagree, preserve the locked design and report the conflict. If something is unknown, say it is unknown rather than inventing it.
