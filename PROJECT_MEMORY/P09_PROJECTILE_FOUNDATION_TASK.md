# P09-A — Projectile Foundation: Antigravity execution task

Date: 2026-09-26.
Status: **NEXT GAMEPLAY WORKSTREAM — IMPLEMENTATION TASK; NOT ACCEPTED / NOT LOCKED**.
Tech Lead: ChatGPT / Codex. Executor: Antigravity.
Project: E:\\code\\TLTD; Unity 6000.6.0f1.

## Mission and sequencing

P08 is ACCEPTED / LOCKED. The Project Owner has chosen complete gameplay functionality before finished UI.
Start a small, usable projectile skill foundation through the existing skill/damage pipeline. Use simple placeholder projectile rendering and existing skill controls. Do not begin UI-POLISH-01, new menus, final VFX or character animation.

Historical planning placed Projectile / Dash / Movement Skill after AOE. This task uses the current label P09-A; older P07.10 AOE corresponds to completed P08. Dash, jump, projectile AOE/piercing/bounce and advanced combat integration remain later work, not implicit scope.

## Read and preserve

Read AI_RULES.md, CONTINUITY_AND_SYNC_AUTHORITY.md, ACTIVE_WORK_HANDOFF.md, CURRENT_DESIGN_AUTHORITY.md, P08_LOCKED.md, D1_D23_LOCKED.md, its amendments, D12_D14_LOCKED.md and P07_9_LOCKED.md / P07_9_1_LOCKED.md.
In the memory repository some D files are at its root; in the game copy they are under PROJECT_MEMORY. Resolve the existing file rather than inventing its contents.

Record local HEAD, status and relevant source hashes without resetting, replacing or discarding uncommitted changes. The Project Owner's tested working tree takes precedence over an older remote snapshot.

## Source findings already established

Read-only inspection of game remote ff4fcec7fda4f6390b1212300249554a89ef4e3d found:
- SkillDefinitionSO has damage, Rage, cooldown, effects, cast/channel and priority fields; no projectile delivery/speed fields.
- SkillExecutor routes instantaneous or completed casts into ExecuteEffectsAndFinalize, then the existing EffectResolver. Channels retain their own prepared snapshot resolver.
- SkillExecutionRequest holds Source, Skill, Slot, Target and CombatConfig.
- MovementComponent handles ordinary movement; it is not a projectile or dash authority.
- D13.18-19 explicitly anticipates optional projectiles; the historical speed 15 is an example, not locked balance. D14 leaves exact dash behavior open.

Reconcile these findings against the local code once. Do not repeat a general audit of the accepted P08 subsystem.

## Bounded first slice

Implement player-faction single-target projectile skills first:
- Existing skills default to the current immediate delivery mode; missing/new serialized fields must preserve existing asset behavior.
- New projectile mode is opt-in and data-driven. Speed and lifetime must be positive finite configurable values; fixture values are not permanent balance.
- Support instant skills and a normal cast-time release. Channel+projectile and unsupported mixed/area delivery combinations must fail validation before consuming resources until explicitly implemented in a later slice.
- Bind the explicit launch target, request, caster and encounter generation to one runtime projectile. Never reacquire a different target or hit a newly spawned wave.
- Homing toward the bound living target is the first implementation mode. Fixed-position shots, physics ricochet, collision with terrain and piercing are outside this slice.
- Arrival performs the effect exactly once via EffectResolver / the existing DamageEffect and HealthComponent path. Projectile code must not calculate damage, write HP or award EXP/loot itself.
- A dead/destroyed/inactive target, caster death, lifetime expiration, battle reset, scene unload or encounter replacement cancels the outstanding projectile cleanly. No replacement target and no resource refund after a valid release.
- Keep already-released projectiles from introducing actions during an actual combat pause. Ordinary caster CC after release is not a second cast interruption; pre-release cast interruptions retain P07.9 behavior.
- Release success and impact are distinct observations. Rage is charged once by SkillExecutor at the existing cast-start point. A successful release completes that skill execution and starts cooldown once through SkillExecutor; impact must not trigger a second cooldown or a second SkillExecutionSucceeded. Existing non-projectile timing stays unchanged.
- Failed pre-release validation spends no Rage and starts no cooldown.
- The targeted effect resolver used at impact must preserve the bound target and fail closed rather than using CurrentTarget fallback.
- Do not let outstanding projectiles prolong an already completed wave or leak into the next one. Use the existing encounter identity/lifecycle; avoid another BattleManager.
- Temporary renderers are presentation only. Disabling renderer/VFX must not change damage or timing.

These are the bounded technical rules for the new slice, not a declaration of P09 acceptance or final balance.

## Allowed implementation boundary

First trace the exact local call path from Hero.ExecuteSelectedSkill through validation, Rage, cast completion/release, projectile arrival, EffectResolver and natural death.
Write a short scope note with the concrete insertion points, then proceed within this task; routine implementation/test actions do not require another permission round.

Candidate production files:
- Assets/_Game/Data/SkillDefinitionSO.cs: backward-compatible opt-in delivery data.
- Assets/_Game/Combat/SkillExecutionValidator.cs: delivery configuration and unsupported-combination checks.
- Assets/_Game/Combat/SkillExecutor.cs: smallest release branch using existing resource/finalization ownership.
- New narrowly scoped projectile runtime/definition files under Assets/_Game/Combat/.
- SkillExecutionTypes.cs only if existing result semantics cannot honestly distinguish release from hit; explain the additive change.

Tests/infrastructure:
- New dedicated P09 runner and its .meta.
- Minimal P09 verification wrapper if existing tools cannot invoke the new runner.
- An isolated fixture/sample skill for the new mode, with cleanup and no rewrite of existing skill assets.

Protected for this task: P08 BattleManager and wave/loot rules; SkillCastState; DamageCalculator; HealthComponent; RageComponent; CooldownManager; BasicAttackProcessor; existing target resolver policies; EntityStatusController; locked UI and Prototype01 scene. If a necessary change crosses this boundary, give the exact call flow and minimal proposed diff before editing that file. Do not work around the boundary with reflection in production or a second authority.

No mass asset migration, no scene rebuild, no rebalance, no reset/rebase, no source commit/push under this task. Documentation publication by the Tech Lead does not authorize an unrelated source push.

## Focused verification

1. Immediate/default skills retain their current damage, Rage, cooldown and cast/channel behavior.
2. Projectile release produces zero early damage; natural arrival produces exactly one matching damage event for the bound target.
3. A moving target remains the same bound target; a changed Hero.CurrentTarget does not redirect the projectile.
4. Target death/deactivation/destruction before arrival, expiry and caster death cause zero replacement/foreign hits.
5. Multiple simultaneous executions keep independent payloads; duplicate arrival cannot double-hit.
6. Encounter change/reset/scene cleanup leaves zero outstanding projectiles that can damage a later wave.
7. Invalid delivery data and unsupported channel/area combinations abort before Rage/cooldown; valid instant and cast-time release charge once and finalize once.
8. Pause/resume and pre-release CC follow the existing battle/cast contract; rendering has no combat authority.

Use unit tests for edge cases and one bounded real Play Mode scenario for release/travel/impact and natural target death. In Play Mode, use real frames, Time.timeScale=1 and the canonical skill entry point; no manual Tick/Update, direct ProcessEffects as a pretend cast, forced damage/death or synthetic death events. Initial isolated fixture configuration is permitted and must be restored.

Run the existing P08 and relevant P07.9/P07.9.1 tests because the common skill entry point is extended. Broaden testing only for a concrete changed dependency. If the 137 suite is run, report actual results and preserve the known 07/09 single-monster exceptions; do not present exit 1 as full PASS.
No automatic repetition of all old UI gates when no UI dependency changed.

Reuse bounded process startup/scenario/exit timeouts, actual OS exit codes, real error listeners and save isolation. Do not let an unavailable environment run for hours.
If Unity automation is blocked, finish the code/scope evidence, report NOT EXECUTED for the blocked checks, and provide an exact short Unity manual procedure and observer log. The Project Owner can verify that path directly; do not fabricate PASS.

## Deliverable and stop rule

Return only:
- changed paths and minimal diff;
- concrete runtime call flow and any boundary exception;
- compile/test results with actual counts, errors and exit codes;
- one real projectile demonstration or exact manual instructions if automation was blocked;
- untouched P08 contract confirmation and remaining limits.

Stop for a proven unresolved design conflict or necessary protected-file change, with the exact choice/diff required. Do not stop for decisions already stated here. Do not add speculative blockers.
Final status: P09-A IMPLEMENTED / TECH LEAD REVIEW REQUIRED, or a precise BLOCKED / NOT EXECUTED statement.
