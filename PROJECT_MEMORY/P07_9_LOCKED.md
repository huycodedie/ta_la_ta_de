# P07.9 — Advanced Skill Casting

**Status:** LOCKED
**Milestone:** P07.9
**Lock Date:** 2026-09-14
**Authority:** P07.9 Design Contract + verified implementation/regression evidence

---

## Final Acceptance

P07.9 was explicitly LOCKED by the user after successful remediation and final full master regression.

### Final Regression Evidence

- P01 → P07.8: **834/834 PASS** across 34 suites.
- P07.9: **209/209 PASS** across 9 suites.
- Grand total: **1,043/1,043 PASS — 100%**.
- Phase 2.3 Dodge-RNG remediation was verified **3 consecutive runs: 13/13, 13/13, 13/13**.
- Targeted regressions: **234/234 PASS**.
- Compile errors: **0**.
- Compile warnings: **0**.
- No skipped or bypassed tests.
- Full regression was completed without gameplay-code modifications during the final gate.

## Locked P07.9 Behavior

P07.9 extends the existing skill architecture with optional non-instantaneous casting while preserving P01–P07.8 authorities.

- Instant skills remain backward-compatible and synchronous when `CastTime = 0`.
- Cast-time skills progress deterministically before effect execution.
- Channel skills support deterministic channel progression and periodic ticks.
- Rage is consumed at Cast Start; interrupted casts do not refund Rage.
- Failed validation before Cast Start consumes no Rage.
- Cast-time cooldown starts only after successful completion.
- Interrupted cast/channel before successful completion does not start normal cooldown.
- Casting/channeling locks movement.
- Stun and Freeze interrupt active cast/channel.
- Root does not interrupt casting/channeling.
- Ordinary incoming damage does not interrupt casting/channeling.
- P07.6 CC/Resistance/Anti-CC authority remains in `EntityStatusController`.
- P07.7 Cleanse/Dispel behavior remains isolated and does not falsely resume interrupted casts.
- P07.8 Shield/Damage/Health pipeline remains authoritative for channel damage.
- Ultimate validation respects `CanUseUltimate` before Rage consumption.
- Death and target-death invalid execution paths are covered by implementation tests.
- UI Cast/Channel presentation is presentation-only and does not become a gameplay authority.

## Architecture Authority

No second authority was introduced for:

- Skill execution — `SkillExecutor`
- Skill validation — `SkillExecutionValidator`
- CC/status — `EntityStatusController`
- Damage calculation — `DamageCalculator`
- HP/damage/shield entry — `HealthComponent`
- Cooldown — `CooldownManager`
- Rage — `RageComponent`

Cast lifecycle state is an internal lifecycle/state mechanism within the existing skill execution architecture, not a second skill executor.

## Remediation Record

The only failure encountered during the first final regression was P07.9 Phase 2.3 Play Mode verification. The target Monster's random Dodge caused `Final=0` and made an effect-execution assertion fail. The test fixture was hardened by setting the spawned test Monster's Dodge stat to zero for that fixture only. Production gameplay rules and the damage algorithm were not modified. The original assertion requiring Monster HP reduction was preserved.

After remediation, Phase 2.3 passed three consecutive runs and the final full regression reached 1,043/1,043 PASS.

## Lock Decision

The user explicitly issued:

> **LOCK P07.9**

Therefore P07.9 is now a **LOCKED milestone** and becomes the implementation baseline for subsequent work. Any future behavior that conflicts with this locked milestone must be treated as a potential design change and must not be silently overwritten.

**Next milestone:** TBD / awaiting next explicit design or implementation task.
