# P07.9.1 — LOCKED

## Status
LOCKED based on the user's final audit/execution report.

## Scope
Hero Autonomous Skill Decision & Auto Combat.

## Locked behavior
- Auto ON: Hero autonomously evaluates and executes normal skills when eligible.
- Normal-skill priority is data-driven: Priority DESC, SkillId ordinal ASC as deterministic tie-break.
- Ultimate can preempt/interupt Basic Attack windup when its conditions are met.
- Auto OFF: autonomous normal-skill/ultimate decision is disabled; Basic Attack behavior remains active.
- Manual skill activation still passes through the existing validation/execution pipeline.
- HeroSkillDecisionController only reads Rage; it does not own Rage mutation.
- Ultimate RageCost is read from SkillDefinitionSO.RageCost; no hard-coded gameplay cost.
- Casting/channeling keeps IsCasting active and blocks Basic Attack while active.
- Existing P07.8/P07.9 authorities remain authoritative: SkillExecutor, SkillExecutionValidator, SkillCastState, CooldownManager, EntityStatusController, RageComponent, DamageCalculator, HealthComponent, BasicAttackProcessor, AttackComponent.
- No second authority for skill execution, Rage, cooldown, cast/channel state, CC, damage, or health was introduced.

## Validation evidence supplied by user
- P07.9.1 dedicated suite: 16/16 PASS.
- P07.8 regression: 55/55 PASS.
- P07.9 Phase5.3: 36/36 PASS.
- P07.9 Risk04: 18/18 PASS.
- Historical Master P01-P07.8: 100% PASS.
- Compile check: 0 CS errors, 0 CS warnings, exit code 0.
- Runtime Play Mode scenarios A-E: PASS.

## Modified implementation files reported by user
- SkillDefinitionSO.cs
- EventBus.cs
- BattleManager.cs
- SkillBarUI.cs
- Entity.cs
- Hero.cs
- HeroSkillDecisionController.cs (new)
- Prototype01SceneBuilder.cs
- Prototype01PlayTestRunner_P07_9_1.cs (new)

## Locked boundaries
- Do not modify P07.9.1 behavior without an explicit design change/supersede decision.
- Do not reintroduce hard-coded skill-slot counts, Rage costs, cooldowns, or skill priorities into runtime logic.
- Do not create a second gameplay authority in UI or autonomous-decision code.

## Important distinction
This LOCK records the user's supplied audit and execution evidence; it is not an independently executed Unity verification by the assistant.

## Skill data authority note
Skill gameplay parameters belong to the data-driven SkillDefinitionSO/skill data layer, while runtime systems consume those definitions. Exact project asset paths and every serialized field should be verified against the current Unity project before changing them; do not invent a new data authority.
