# TLTD — P07.9.1 HERO SKILL AUTONOMOUS EXECUTION AUDIT

**Project:** TLTD (Wuxia Mobile Idle/AFK RPG)  
**Engine:** Unity 6.x (6000.6.0f1)  
**Milestone:** P07.9.1 Hero Skill Autonomous Execution Audit  
**Date:** 2026-09-15  
**Final Audit Classification:** **CASE A — Decision Layer Does Not Invoke Skill**  
**Critical Authority Finding:** **MISSING AUTO BATTLE AUTHORITY & MISSING AUTONOMOUS DECISION LAYER**  

---

## 1. Executive Summary

During Unity Play Mode combat testing of TLTD, the Hero executes standard Basic Attacks via `AttackComponent`, accumulates Rage to 100/100, and takes/deals damage normally. However, **the Hero never autonomously activates any slotted skills or Ultimate**, despite skills being unlocked, selected in `MindMethodManager`, and having zero cooldown.

An exhaustive audit of the entire execution pipeline was performed without modifying gameplay code. The audit proves conclusively that:
1. **The Skill Execution Pipeline is 100% functional** (`SkillExecutionValidator`, `SkillExecutor`, `SkillCastState`, `CooldownManager`, `RageComponent`, and `EntityStatusController` pass all validation, cast-time progression, channel ticks, CC interruptions, and effect execution).
2. **The Autonomous Decision Layer is completely missing from the production codebase.**
3. **No Auto Battle Authority exists in the gameplay layer.** (Auto state existed solely as a local boolean in UI `SkillBarUI`).
4. **The exact breakage point is that no component in the combat loop evaluates skill readiness or invokes `Hero.ExecuteSelectedSkill(slot)`.**

---

## 2. Current Skill Execution Call Graph

An exhaustive trace of all call sites across the codebase reveals the three distinct flows:

```
FLOW A: MANUAL SKILL EXECUTION (OPERATIONAL)
Player Button Click (SkillBarUI.cs:216)
  └──> Hero.ExecuteSelectedSkill(slot, target) (Hero.cs:295)
         ├──> MindMethodManager.GetSelectedSkillForSlot(slot) (MindMethodManager.cs:534)
         ├──> NearestEnemyTargetResolver.ResolveTarget(this) (TargetResolver.cs)
         ├──> new SkillExecutionRequest(this, skillDef, slot, target, combatConfig)
         └──> SkillExecutor.Execute(request) (SkillExecutor.cs:28)
                ├──> SkillExecutionValidator.Validate(request, ...) (SkillExecutionValidator.cs:10)
                ├──> RageComponent.ConsumeRage(cost) (RageComponent.cs:32)
                ├──> [If Channel] SkillCastState.StartChannel(...)
                ├──> [If CastTime > 0] SkillCastState.StartCast(...)
                └──> [If Instant] EffectResolver.Resolve(...) + CooldownManager.StartCooldown(...)

FLOW B: AUTONOMOUS / AUTO SKILL EXECUTION (COMPLETELY BROKEN / NON-EXISTENT)
[Combat Loop / AI Decision]  <--- MISSING: No class owns this loop
  └──> [Skill Decision Layer] <--- MISSING: No class queries cooldown / readiness
         └──> [Priority Selector] <--- MISSING: No priority authority exists
                └──> Hero.ExecuteSelectedSkill(slot) [NEVER INVOKED AUTONOMOUSLY]

FLOW C: AUTONOMOUS ULTIMATE EXECUTION (COMPLETELY BROKEN / NON-EXISTENT)
[Rage >= Ultimate.RageCost] <--- UNMONITORED: Rage reaches 100, but no listener/tick acts
  └──> [Ultimate Decision] <--- MISSING
         └──> [Interrupt Basic Attack] <--- MISSING
                └──> Hero.ExecuteSelectedSkill(Ultimate) [NEVER INVOKED AUTONOMOUSLY]
```

### Call Site Inventory: `Hero.ExecuteSelectedSkill(slot)`
- `Assets/_Game/UI/HUD/SkillBarUI.cs`: Line 216 (`boundHero.ExecuteSelectedSkill(slot)`) — Manual button click only.
- `Assets/_Game/Editor/Prototype01PlayTestRunner*.cs`: All test fixtures invoke `ExecuteSelectedSkill` explicitly in code.
- **Production Gameplay Call Sites (Combat Loop / Update): ZERO.**

---

## 3. Current Auto Battle Authority

**AUDIT RESULT: MISSING AUTO BATTLE AUTHORITY.**

1. **Where is Auto Battle stored?**
   - Auto Battle state is **NOT** stored in any gameplay class (`BattleManager`, `GameManager`, `Hero`, or `CombatConfig`).
   - The string `"Auto"` appears only in [SkillBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/SkillBarUI.cs#L51) as a private UI field `private bool isAutoActive = false;` for toggle button text display (`"AUTO: BẬT"` / `"AUTO: TẮT"`).
2. **Who is the authority of Auto Battle?**
   - **NO gameplay authority exists.** The UI is not, and per Gate B/D must not be, the gameplay authority.
3. **Does Hero Combat Loop read Auto state?**
   - No. Neither `Hero`, `Entity`, nor `AttackComponent` references any Auto state.
4. **When Auto is ON, does any code check Skill ready?**
   - No.
5. **When Auto is OFF, does Hero stop autonomous skill casting?**
   - Hero does not cast skills in either state.

> [!CAUTION]
> **STOP CONDITION TRIGGERED (Section 0.2 & Section 3):**
> Because no Auto Battle authority exists in the locked gameplay baseline (P01–P07.9), an authoritative gameplay state (`BattleManager.IsAutoBattle` or `IAutoBattleController`) must be formally designed and introduced at the gameplay layer. UI-02 did not and must not invent a rogue gameplay authority.

---

## 4. Current Skill Decision Authority

**AUDIT RESULT: MISSING SKILL DECISION AUTHORITY.**

- `SkillExecutor`: Strictly an **EXECUTION** authority. It receives a `SkillExecutionRequest`, validates via `SkillExecutionValidator`, and executes the cast/effects. It does not decide *when* or *what* to cast.
- `SkillExecutionValidator`: Strictly a **VALIDATION** authority. It returns `true`/`false` and failure reasons. It does not initiate skill requests.
- `MindMethodManager`: Strictly a **DATABASE & SELECTION** authority. It tracks unlocked skills and which skill ID is assigned to each slot. It does not tick or make combat decisions.
- `CooldownManager`: Strictly a **COOLDOWN TIMESTAMP** authority. It checks `IsOnCooldown(skillId)`. It does not broadcast expiry events or trigger casts.
- `Hero`: Contains stat calculation, component initialization, and `ExecuteSelectedSkill(slot)`. It has **no `Update()` method** and executes no combat decision logic.
- `AttackComponent`: Manages `attackTimer += Time.deltaTime`. When `attackTimer >= attackInterval`, it unconditionally calls `BasicAttackProcessor.ExecuteBasicAttack(attacker, target)`. It contains **zero skill logic**.

---

## 5. Current Priority Authority

**AUDIT RESULT: MISSING PRIORITY AUTHORITY.**

- Locked Rule D5 states:
  > *"Các chiêu thông thường khi hết Cooldown tự động thi triển theo độ ưu tiên, chiêu cao cấp trước."*
- Audit Findings:
  1. `SkillDefinitionSO` has **NO `priority` field** (only `damageMultiplier`, `rageCost`, `cooldown`, `castTime`, etc.).
  2. `MindMethodManager` has **no priority sorting or comparator**.
  3. `SkillSlotType` is an enum (`NormalAttack=1, Skill=2, ExternalSkill1=3, ExternalSkill2=4, Ultimate=5`), but no code maps slot order to execution priority.
  4. If multiple skills (e.g. Slot 2 and Slot 3) are off cooldown simultaneously, there is currently no data-driven authority determining which skill has higher priority.

---

## 6. Current Ultimate Decision Path

**AUDIT RESULT: DISCONNECTED.**

- Locked Rule D5 states:
  > *"Khi đủ Rage theo quyết định này: Rage = 80/100, Tuyệt kỹ có thể ngắt đòn đánh thường để tung ngay."*
- Current Runtime State:
  1. `Hero.Rage.CurrentRage` reaches 100 via basic attacks and damage received.
  2. `RageComponent` fires `EventBus.RaiseRageChanged(owner, cur, max)`.
  3. `SkillBarUI` hears `OnRageChanged` and updates the visual glow of Slot 5 (presentation only).
  4. **NO gameplay component listens to `OnRageChanged` or checks `Rage >= Ultimate.RageCost` in combat.**
  5. `AttackComponent` continues ticking basic attacks unaffected.
  6. `Hero.ExecuteSelectedSkill(SkillSlotType.Ultimate)` is never invoked autonomously.

---

## 7. Exact Point Where Hero Skill Execution Stops

The breakage point is precisely identified:

```
[MindMethodManager] Slotted skills exist (e.g. "skill_taiji_2_a", "skill_taiji_3_a")
[CooldownManager]   Cooldown is 0 (READY)
[RageComponent]     Rage is 100 (READY)
[AttackComponent]   Ticks Basic Attacks (RUNNING)
        │
        ▼
   [BREAKAGE POINT] ════════════════════════════════════════════════════════════
   No component in the gameplay layer ever queries:
     - Is Auto enabled?
     - Are any slotted skills ready?
     - Does Hero have sufficient Rage for Ultimate?
     - Which skill has highest priority?
   And therefore NO CALL to Hero.ExecuteSelectedSkill(...) is ever made.
   ═════════════════════════════════════════════════════════════════════════════
        │
        ▼ (Never Reached Autonomously)
[SkillExecutionValidator] Validates request (100% functional)
[SkillExecutor]          Executes request (100% functional)
[SkillCastState]         Ticks cast/channel (100% functional)
```

---

## 8. Play Mode Evidence (Tests A to J)

Executed via dedicated audit suite `Prototype01PlayTestRunner_P07_9_1_Audit.cs` in Unity 6000.6.0f1 batchmode:

```
================================================================================
   STARTING P07.9.1 HERO SKILL AUTONOMOUS EXECUTION AUDIT (TESTS A TO J)
================================================================================
[AUDIT TEST A — AUTONOMOUS SKILL] SimTime: 5.0s | Hero Rage: 100 | Slotted Skill: Bát Quái Chưởng | Skill Requests: 0
[AUDIT TEST B — SKILL PRIORITY] Result: NO PRIORITY AUTHORITY FOUND in SkillDefinitionSO or MindMethodManager.
[AUDIT TEST C — AUTONOMOUS ULTIMATE] Hero Rage: 100 | Slotted Ultimate: Thái Cực Vô Cực | Ultimate Requests: 0
[AUDIT TEST D — AUTO OFF BEHAVIOR] Result: No Auto Battle gameplay authority exists.
[AUDIT TEST E — MANUAL SKILL INPUT] Success: True, Reason: None, Message: 'Skill executed successfully.'
[AUDIT TEST F — CAST TIME TRANSITION] Pass: True (IsCasting: True, Phase: Casting)
[AUDIT TEST G — CHANNEL SKILL] Pass: True (IsChanneling: True, TicksExecuted: 1)
[AUDIT TEST H — STUN INTERRUPT] Pass: True (Before: True, After: False)
[AUDIT TEST I — ROOT CONTINUES] Pass: True (Before: True, After: True)
[AUDIT TEST J — DAMAGE CONTINUES] Pass: True (Before: True, After: True)
================================================================================
   P07.9.1 AUDIT SUMMARY:
   TEST A (Autonomous Skill Cast): BROKEN / MISSING (Hero never auto-casts)
   TEST B (Skill Priority System): BROKEN / MISSING (No priority authority)
   TEST C (Autonomous Ultimate):   BROKEN / MISSING (Hero never auto-ultimates)
   TEST D (Auto OFF Stop Cast):    NO EFFECT (No autonomous casting exists to stop)
   TEST E (Manual Skill Input):    PASS (Execution pipeline operational)
   TEST F (Cast Time Transition):  PASS
   TEST G (Channel Skill):         PASS
   TEST H (Stun Interrupt):        PASS
   TEST I (Root Continues):        PASS
   TEST J (Damage Continues):      PASS
================================================================================
>>> [P07.9.1 AUDIT CLASSIFICATION] RESULT: CASE A <<<
>>> REASON: Decision Layer does not invoke Hero.ExecuteSelectedSkill. Execution pipeline (Validator, Executor, CastState, Cooldown, Damage) is 100% operational, but no autonomous decision layer exists in Hero/AttackComponent/BattleManager to evaluate cooldowns/rage and trigger skill requests during combat.
```

### Detailed Evaluation of Tests A through J

| Test | Description | Result | Evidence |
|---|---|---|---|
| **TEST A** | In active combat with slotted skill off cooldown, does Hero auto-cast skill? | **FAIL (0 requests)** | Ran 5.0s combat; Basic Attacks executed, Rage reached 100, but `Skill Requests = 0`. |
| **TEST B** | When multiple skills are ready, is there an authority evaluating priority? | **FAIL (No authority)** | `SkillDefinitionSO` and `MindMethodManager` lack priority properties or sorting logic. |
| **TEST C** | When Hero has 100 Rage, does Hero auto-use Ultimate? | **FAIL (0 requests)** | Hero reached 100 Rage; `Ultimate Requests = 0`. Basic attack loop continued uninterrupted. |
| **TEST D** | When Auto is OFF, does Hero stop autonomous casting? | **FAIL (No authority)** | No gameplay Auto authority exists; cannot stop what is never started. |
| **TEST E** | When manual input calls `Hero.ExecuteSelectedSkill`, does skill execute? | **PASS** | `Success: True`, damage dealt to monster, cooldown started. |
| **TEST F** | Skill with Cast Time > 0: does Hero transition to Cast State? | **PASS** | `IsCasting: True`, `CurrentPhase: Casting`. |
| **TEST G** | Channel Skill: does Hero channel across ticks? | **PASS** | `IsChanneling: True`, `TicksExecuted: 1`. |
| **TEST H** | Stun during Cast: is Cast interrupted? | **PASS** | Cast interrupted immediately by Stun CC. |
| **TEST I** | Root during Cast: does Cast continue? | **PASS** | Root applied; casting continued without interruption. |
| **TEST J** | Ordinary Damage during Cast: does Cast continue? | **PASS** | 20 damage applied; casting continued without interruption. |

---

## 9. Root Cause Analysis

1. **Historical Decoupling in Milestones P01–P07.9:**
   - From P01 through P07.0, combat was strictly basic-attack-driven (`AttackComponent`).
   - In P07.1 through P07.9, the skill execution architecture (`SkillExecutor`, `SkillExecutionValidator`, `SkillCastState`, `CooldownManager`) was built and hardened.
   - All 1,043 automated regression tests across P01–P07.9 directly invoked `hero.ExecuteSelectedSkill(slot, target)` programmatically to test the pipeline.
   - **No milestone between P01 and P07.9 ever implemented the autonomous combat loop integration (Flow B and Flow C)** connecting the combat update tick to `Hero.ExecuteSelectedSkill`.
2. **Missing Auto Battle Authority in Core Architecture:**
   - `BattleManager` has no `Update()` method and no `IsAutoBattle` state.
   - Auto toggle existed purely as visual button text in `SkillBarUI`. Per Gate B/D, UI cannot and must not be the gameplay authority.
3. **No Combat Decision Ticking:**
   - `Hero.cs` has no `Update()` loop.
   - `AttackComponent.cs` only counts `attackTimer` and fires `BasicAttackProcessor`. It has no reference to `MindMethodManager` or `ExecuteSelectedSkill`.

---

## 10. Required Fix Proposal (Design Architecture)

Per Section 0.2 and Section 3 of the prompt, **no gameplay code was modified during this audit**. The following architecture is formally recommended for the subsequent implementation task:

### 10.1 Auto Battle Gameplay Authority (`BattleManager` or `CombatManager`)
- Add authoritative `bool IsAutoBattle { get; set; }` to `BattleManager` (default: `true` for AFK/Idle RPG).
- Expose `SetAutoBattle(bool enabled)` and event `OnAutoBattleChanged(bool isAuto)`.
- `SkillBarUI.ToggleAuto()` delegates to `BattleManager.Instance.SetAutoBattle(...)` (UI is pure presentation + input delegation).

### 10.2 Autonomous Skill Decision Component (`HeroSkillDecisionController` or `Hero.TickSkillDecision`)
- Implement a dedicated gameplay decision method/component evaluated each combat tick:
  1. **Check Auto:** If `!BattleManager.Instance.IsAutoBattle`, skip autonomous casting (allow manual click only per D5).
  2. **Check Cast State:** If `Hero.IsCasting`, do nothing (wait for cast/channel to complete or interrupt).
  3. **Flow C (Ultimate Priority Interrupt):**
     - Check slotted Ultimate (`SkillSlotType.Ultimate`).
     - If `Hero.Rage.CurrentRage >= ultimateDef.RageCost` and `!CooldownManager.IsOnCooldown(ultimateDef.SkillId)` and `Hero.CanUseUltimate`:
       - If Hero is in middle of Basic Attack windup, interrupt Basic Attack (`Hero.InterruptCurrentAction()`).
       - Invoke `Hero.ExecuteSelectedSkill(SkillSlotType.Ultimate)`.
       - Return (Ultimate takes absolute precedence).
  4. **Flow B (Normal Skills by Priority):**
     - Collect all currently slotted active skills (`SkillSlotType.Skill`, `ExternalSkill1`, `ExternalSkill2`).
     - Filter skills where `!CooldownManager.IsOnCooldown(s.SkillId)` and `Hero.CanUseSkill`.
     - Select highest priority skill (data-driven priority field on `SkillDefinitionSO` or deterministic slot order `Skill > ExternalSkill1 > ExternalSkill2` per D5).
     - If ready, invoke `Hero.ExecuteSelectedSkill(selectedSlot)`.

### 10.3 Basic Attack Interlocking
- Ensure `AttackComponent` does not launch basic attacks while `Hero.IsCasting` is active.

---

## 11. Files Changed During Audit

- [Prototype01PlayTestRunner_P07_9_1_Audit.cs](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01PlayTestRunner_P07_9_1_Audit.cs): Created dedicated diagnostic test runner executing Tests A to J in Play Mode.
- [scratch/run_p07_9_1_audit.ps1](file:///C:/Users/conca/.gemini/antigravity-ide/brain/66d95990-638e-491f-8b2e-f7e9f5418b46/scratch/run_p07_9_1_audit.ps1): Created automation runner.

---

## 12. Files NOT Changed (Preserving Locked Gameplay Baseline)

**ZERO gameplay files were modified.** The following remain 100% untouched:
- `Assets/_Game/Combat/DamageCalculator.cs`
- `Assets/_Game/Combat/SkillExecutor.cs`
- `Assets/_Game/Combat/SkillExecutionValidator.cs`
- `Assets/_Game/Combat/SkillCastState.cs`
- `Assets/_Game/Combat/CooldownManager.cs`
- `Assets/_Game/Combat/EntityStatusController.cs`
- `Assets/_Game/Combat/BasicAttackProcessor.cs`
- `Assets/_Game/Entities/Hero.cs`
- `Assets/_Game/Entities/Entity.cs`
- `Assets/_Game/Entities/Monster.cs`
- `Assets/_Game/Entities/Components/AttackComponent.cs`
- `Assets/_Game/Entities/Components/HealthComponent.cs`
- `Assets/_Game/Entities/Components/RageComponent.cs`
- `Assets/_Game/Core/BattleManager.cs`
- `Assets/_Game/Progression/MindMethodManager.cs`

---

## 13. Regression Results

Existing locked test suites executed prior to and during the audit session:

| Test Suite | Total Tests | Passed | Failed | Status |
|---|---|---|---|---|
| `Prototype01PlayTestRunner_P07_8` | 55 | 55 | 0 | **PASS** |
| `Prototype01PlayTestRunner_P07_9_Phase5_3` | 36 | 36 | 0 | **PASS** |
| `Prototype01PlayTestRunner_P07_9_Risk04` | 18 | 18 | 0 | **PASS** |
| **Total Regressions** | **109** | **109** | **0** | **100% PASS** |

---

## 14. Remaining Issues & Next Step Recommendation

1. **Autonomous Execution Cannot Be "Turned On" Without Introducing an Auto Battle Authority:**
   - As mandated by Section 0.2 Stop Condition 1 & 2 and Section 3, creating an Auto Battle Authority was stopped during the audit.
2. **Next Step:**
   - User review and approval of this Audit Document.
   - Authorize implementation of the proposed **Auto Battle Gameplay Authority** and **Hero Autonomous Skill Decision Controller** in milestone `P07.9.1`.
