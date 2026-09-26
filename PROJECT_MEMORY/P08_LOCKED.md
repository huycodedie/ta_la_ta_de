# P08 — ACCEPTED / LOCKED

Decision date: 2026-09-26 (Asia/Ho_Chi_Minh).
Project Owner: accepted after direct interactive Unity testing.
Acceptance authority: ChatGPT / Codex.
Implementation executor: Antigravity.
Status: **ACCEPTED / LOCKED — agreed P08 functional scope**.

## Acceptance decision

The Project Owner confirmed: "tôi đã mở unity lên và test game và các điều kiện để chốt đều hoạt động".
This closes P08, including the final channel-finishing-with-loot / Equip / Dismantle / modal-release / next-wave path covered by the manual acceptance discussion.

Do not reopen P08 because older memory says NOT STARTED, or demand another complete evidence cycle merely to rewrite documentation. A newly reproduced bug is a separate scoped bug ticket, not automatic revocation of the milestone.

## Locked functional contract

- Normal waves contain 4 or 5 simultaneous monsters, chosen once per wave. Initial and subsequent normal waves use the same production spawning rules.
- Initialize stats, register the complete wave, and assign targets before combat becomes active. Preserve the accepted ground plane Y = -0.30.
- After a complete normal wave is legitimately defeated, increment CompletedNormalWaveCount once.
- Next-wave base MaxHealth, Attack and Defense multiply by 1.01. Equivalently: OriginalBaseStat * 1.01^CompletedNormalWaveCount.
- Do not multiply per monster death, registration, stat recalculation, loot decision or death-event replay. Retrying an incomplete wave preserves its growth tier.
- Other stats, reward values and loot rules are not implicitly scaled by this change. Runtime scaled configs do not mutate disk assets; recalculation and buff expiration retain the scaled wave base.
- Wave completion uses the encounter's legitimate-death roster. Removing an entity from the targeting registry, deactivating it or destroying it outside the death path does not count as victory.
- AOE supports Hero -> Monster and Companion -> Monster through the shared pipeline. Enemy AOE is outside the accepted P08 scope. Basic attacks stay single-target.
- Area resolution uses XZ distance, valid primary/anchor first, remaining distance ascending, runtime registration order for equal-distance ties, deduplication, living/active/registered filtering and the configured max count. AllEnemies uses the registered encounter population.
- Do not use GetInstanceID or cast Unity EntityId to int as an ordering workaround.
- Validation is side-effect-free and occurs before resource consumption. Rage is consumed once per skill execution; cooldown and cast/channel completion remain owned by the existing authorities.
- Channel target snapshots are per execution, captured at channel start, preserve order, skip dead/inactive targets and exclude later registrations. No static snapshot bridge.
- Killing the last monster does not spoof CrowdControl or complete a channel early. Accepted finishing executions continue through their natural duration; next-wave/loot progression respects completion and modal ownership.
- EXP is owned by ProgressionManager; BattleManager does not award it a second time. Loot generation is at most once per encounter monster; sequential decisions preserve each item's identity.
- Normal combat continues while wave members remain alive. Pending loot is presented and resolved through the existing modal lifecycle; duplicate/premature decisions cannot consume an undisplayed item.
- External Hero/caster/death events cannot take ownership of the active encounter or trigger its defeat.
- Reuse SkillExecutor, SkillExecutionValidator, EffectResolver, DamageCalculator, HealthComponent, EntityStatusController, RageComponent and CooldownManager. Later features may extend these only within their separately approved scope.

## Evidence and honest regression accounting

The following are the final supplied executor results, reviewed during the P08 thread. They are not Unity runs performed by Codex during this publication task.

| Evidence | Result | Qualification |
|---|---|---|
| Gate 1 | 50/50; exit 0 | Reported P08 T01-T49 plus T21_B |
| Gate 2 | 135/137; exit 1 | Two known legacy single-monster conflicts; NOT 137/137 PASS |
| Gate 3 | 52/52; exit 0 | Reported B1 modal suite |
| Gate 4 | Three natural waves; exit 0 | Reported AOE, retarget, loot, channel completion, modal race, growth and restored save |
| Final interactive check | PASS | Project Owner confirmation in Unity on 2026-09-26 |

Gate 2 exceptions accepted for this milestone: UI02_CombatHeightFix tests 07 and 09 expect encounter advancement after one monster dies. That expectation is superseded by the 4-5-monster full-wave contract. Preserve the raw failures/exit 1 and their explanation. Do not weaken production or silently relabel these tests PASS. A later test-contract maintenance change requires its own explicit scope.

## Artifact and source provenance

- Latest submitted package: review_package_p08_normal_wave_stat_growth.zip.
- Package SHA-256: AE1206706062450E08C3669843B6670593B95E941BCA930008BBE2234D976B55.
- Report baseline: 0805f9df5f81fa619ad1771d625ae92705029077.
- Report pre-fix working commit: d3aac338d70ce332dc477f95e1d7e32e01450b3c.
- Source repository remote observed during this publication: huycodedie/ta_la_ta_de at ff4fcec7fda4f6390b1212300249554a89ef4e3d.
- Acceptance applies to the local build the Project Owner tested in E:\\code\\TLTD. The remote observation is a provenance reference, not an assertion that every local post-package change is byte-identical to that commit.
- At the next local handoff, record actual git HEAD/status and hashes of the tested working source. This is bookkeeping; do not rerun P08 solely to obtain hashes.
- Publication in this task changes documentation only. It does not publish or certify an unseen Unity source patch.

## Next direction

Project Owner decision, 2026-09-26: complete game functionality first; finished UI/presentation work comes later.
UI-POLISH-01 typography, decorative assets, final layout polish, animation and popup styling are deferred.
Only minimal existing controls/debug presentation needed to exercise new gameplay belong in the current workstream.

Next gameplay task: P09-A Projectile Foundation, as scoped in P09_PROJECTILE_FOUNDATION_TASK.md.
P09 is a new implementation workstream, not an already accepted milestone.
