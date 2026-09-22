# P07.9.1 — Hero Autonomous Skill Decision & Auto Combat

**Status:** LOCKED  
**Milestone:** P07.9.1  
**Lock Date:** 2026-09-15  
**Authority:** P07.9.1 Architecture Contract + verified implementation/regression evidence  

---

## Final Acceptance

P07.9.1 was explicitly LOCKED following successful implementation and verification of the autonomous combat decision layer:
- **Decision vs Execution Separation:** `HeroSkillDecisionController` acts strictly as an evaluation/request layer; `SkillExecutor` and execution pipeline remain pure execution engines.
- **Data-Driven Priority:** Skills prioritize execution based on `SkillDefinitionSO.Priority` (Ultimate = 100, Tuyệt Kỹ = 60, Ngoại Công 1 = 40, Ngoại Công 2 = 30, Basic = 0) with deterministic ordinal tie-breaker.
- **Gameplay-Authoritative Auto Battle:** `BattleManager.Instance.IsAutoBattle` is the single gameplay authority. `SkillBarUI` delegates purely to `BattleManager.Instance.SetAutoBattle` via `EventBus.RaiseAutoBattleChanged`.
- **Preemptive Ultimate:** Ultimate resets active basic attack windup timer to immediately take priority when ready.
- **Automated Tests:** 16/16 PASSED (`Prototype01PlayTestRunner_P07_9_1`).
- **Play Mode Acceptance:** 5/5 Scenarios verified in `Prototype01.unity` (Auto Normal, Auto Ultimate, Auto Toggle OFF, Auto Toggle ON, Manual Trigger).
- **Full Regressions:** 100% PASS across P07.8 (55/55), P07.9 Phase 5.3 (36/36), and P07.9 Risk 04 (18/18).

---

## Locked Authorities Preserved

- **Skill Execution:** `SkillExecutor.cs` (LOCKED)
- **Validation Gate:** `SkillExecutionValidator.cs` (LOCKED)
- **Cast/Channel State:** `SkillCastState.cs` (LOCKED)
- **Cooldown Authority:** `CooldownManager.cs` (LOCKED)
- **Rage Authority:** `RageComponent.cs` (LOCKED)
- **Status & CC Authority:** `EntityStatusController.cs` (LOCKED)
- **Damage Authority:** `DamageCalculator.cs` (LOCKED)
- **Health Authority:** `HealthComponent.cs` (LOCKED)
- **Decision Layer:** `HeroSkillDecisionController.cs` (LOCKED)
- **Auto Battle Authority:** `BattleManager.cs` (`IsAutoBattle`) (LOCKED)
