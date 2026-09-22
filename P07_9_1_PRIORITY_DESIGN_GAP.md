# TLTD — P07.9.1 SKILL PRIORITY & AUTO BATTLE DESIGN GAP REPORT

**Project:** TLTD (Wuxia Mobile Idle/AFK RPG)  
**Engine:** Unity 6.x (6000.6.0f1)  
**Milestone:** P07.9.1 Hero Autonomous Skill Decision & Auto Combat  
**Date:** 2026-09-15  
**Status:** **DESIGN GAP IDENTIFIED — STOPPING BEFORE GAMEPLAY IMPLEMENTATION PER SECTION 4**  

---

## 1. Purpose of This Report

Per Section 4 and Section 0.2 of the **P07.9.1 Directive**, the agent is strictly prohibited from:
1. Hard-coding arbitrary slot priority (e.g. `Slot 1 > Slot 2 > Slot 3` or `Skill > ExternalSkill1 > ExternalSkill2`).
2. Inventing a rogue gameplay authority for Auto Battle inside UI scripts (`SkillBarUI`).
3. Guessing or hardcoding skill execution order without an authoritative data-driven model.

An exhaustive audit of the codebase and project memory was conducted. This document records the **existing skill model**, identifies the **exact architectural gaps in Priority and Auto Battle**, and presents a **data-driven design proposal** for user authorization before any gameplay implementation begins.

---

## 2. Existing Skill Data Model Audit

### 2.1 `SkillDefinitionSO` ([SkillDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs))
The current production skill asset definition contains the following serialized fields:

| Field Name | Type | Purpose | Authority |
|---|---|---|---|
| `skillId` | `string` | Unique identifier (e.g. `"skill_taiji_2_a"`) | Metadata / Identity |
| `mindMethodId` | `string` | Parent Mind Method reference | Progression Link |
| `slotType` | `SkillSlotType` | Slotted category enum | Slot Category |
| `skillName` | `string` | Display name (e.g. `"Bát Quái Chưởng"`) | Presentation |
| `description` | `string` | Tooltip description | Presentation |
| `icon` | `Sprite` | UI icon | Presentation |
| `unlockConditions`| `List<SkillUnlockRequirement>` | Unlock prerequisites (HeroLevel, etc.) | P07.2 Unlock Authority |
| `damageMultiplier`| `float` | Base attack multiplier | Damage Calculation |
| `rageCost` | `float` | Rage required and consumed at Cast Start | P07.9 Rage Authority |
| `cooldown` | `float` | Skill cooldown duration in seconds | Cooldown Authority |
| `isPassive` | `bool` | Passive vs Active flag | Execution Category |
| `effects` | `List<SkillEffectDefinitionSO>` | Explicit effect definitions | P07.3 Effect Foundation |
| `canShatterFreeze`| `bool` | Freeze-shattering physical flag | P07.6 CC Authority |
| `castTime` | `float` | Non-instant cast duration | P07.9 Cast Authority |
| `isChannel` | `bool` | Periodic channeling flag | P07.9 Channel Authority |
| `channelDuration`| `float` | Channel duration in seconds | P07.9 Channel Authority |
| `channelTickInterval`| `float` | Tick interval in seconds | P07.9 Channel Authority |

> [!CRITICAL]
> **GAP IDENTIFIED:** `SkillDefinitionSO` has **NO `priority` field or property**. There is currently no data field on the skill asset specifying its execution priority relative to other skills.

---

## 3. Existing Skill Slots & Selection

### 3.1 `SkillSlotType` Enum ([SkillDefinitionSO.cs:L8-L15](file:///E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs#L8-L15))
```csharp
public enum SkillSlotType
{
    NormalAttack = 1,   // Slot 1: Đánh thường (Basic Attack)
    Skill = 2,          // Slot 2: Tuyệt kỹ (Primary Mind Method Active Skill)
    ExternalSkill1 = 3, // Slot 3: Ngoại công 1 (External / Secondary Skill 1)
    ExternalSkill2 = 4, // Slot 4: Ngoại công 2 (External / Secondary Skill 2)
    Ultimate = 5        // Slot 5: Thần công / Bí kỹ (Ultimate Technique)
}
```

### 3.2 Selection Authority in `MindMethodManager` ([MindMethodManager.cs](file:///E:/code/TLTD/Assets/_Game/Progression/MindMethodManager.cs))
- Slotted skills are stored in `ActiveMindMethodState.SelectedSkillPerSlot`:
  - A `Dictionary<SkillSlotType, string>` mapping each slot type to a `SkillId`.
- Exposed APIs:
  - `GetSelectedSkillIdForSlot(SkillSlotType slot)`
  - `GetSelectedSkillForSlot(SkillSlotType slot)`
  - `SelectSkillForSlot(SkillSlotType slot, string skillId)`
- **Role:** Tracks *which* skill is equipped in *which* slot. It contains **zero priority sorting, ranking, or combat decision logic**.

---

## 4. Existing Priority-Related Fields Across Codebase

Priority exists in two other systems in TLTD, but **not** in the Skill System:

1. **P07.8 Shield Absorption Priority:**
   - `EntityStatusController.ApplyShield(..., int priority)`
   - Governed by explicit rule: higher numerical priority absorbs damage first (`P07_8_18`).
2. **P07.7 Cleanse / Dispel Selection Priority:**
   - `StatusSelectionMode.HighestPriority` and `LowestPriority`
   - Governed by explicit rule: cleanses higher/lower priority debuffs first.
3. **Skill Priority:**
   - Referenced conceptually in historical design docs (`D5`, `D6.12`, `D10.11`, `D13.13`).
   - **Never implemented in C# code.** Neither `SkillDefinitionSO`, `MindMethodManager`, `SkillExecutor`, nor `Hero` has any priority field or comparator.

---

## 5. Why the Current Architecture Cannot Decide Priority

If two skills (e.g. Slot 2 `Skill` and Slot 3 `ExternalSkill1`) are simultaneously equipped, unlocked, off cooldown, and valid:
1. **The enum values `2` and `3` represent UI/equipment slot indices, not combat value.**
   - In wuxia RPG design, a player might equip a high-tier crowd-control External Skill that should execute before a standard damage Skill, or vice-versa.
   - Hardcoding `Slot 2 > Slot 3` violates locked rule `D13.13`: *"Skill Priority is data-driven... Auto mode finds available actions, sorts by Priority and executes highest-priority valid action."*
2. **Skill names or IDs cannot determine priority** (alphabetical ordering is arbitrary).
3. **Skill cooldown duration cannot determine priority** (a short-cooldown utility buff might have higher priority than a long-cooldown nuke, or vice versa).
4. **Conclusion:** Without a dedicated, authoritative `Priority` property on `SkillDefinitionSO`, autonomous skill selection cannot be deterministically resolved without violating D5/D13.13.

---

## 6. Existing Auto Battle Authority Audit

### Audit Findings
- **`BattleManager.cs`:**
  - Owns `IsBattleActive`, `EncounterIndex`, monster spawning, and entity death events.
  - Has **NO `Update()` loop** and **NO `IsAutoBattle` property**.
- **`Hero.cs`:**
  - Has **NO `Update()` loop** and **NO `IsAutoBattle` reference**.
- **`SkillBarUI.cs`:**
  - Contains `private bool isAutoActive = false;` purely for button label toggling (`"AUTO: BẬT"` / `"AUTO: TẮT"`).
  - Per Gate B and Gate D, **UI is strictly presentation and MUST NOT own gameplay authority**.

### Conclusion
**Auto Battle currently has NO gameplay authority in the TLTD codebase.**

---

## 7. Proposed Data-Driven Architecture

To resolve both gaps cleanly without violating any locked P01–P07.9 rules, the following architecture is proposed:

### 7.1 Auto Battle Authority in `BattleManager`
Introduce authoritative Auto Battle state on the existing combat manager:
```csharp
// Assets/_Game/Core/BattleManager.cs
[SerializeField] private bool isAutoBattle = true; // Default ON for idle RPG

public bool IsAutoBattle => isAutoBattle;

public void SetAutoBattle(bool enabled)
{
    if (isAutoBattle != enabled)
    {
        isAutoBattle = enabled;
        EventBus.RaiseAutoBattleChanged(isAutoBattle);
    }
}
```
`SkillBarUI` will simply delegate:
```csharp
public void ToggleAuto()
{
    if (BattleManager.Instance != null)
    {
        BattleManager.Instance.SetAutoBattle(!BattleManager.Instance.IsAutoBattle);
    }
}
```

### 7.2 Data-Driven Priority on `SkillDefinitionSO`
Per `D13.13` and `D6.12`, add a serialized `priority` field to [SkillDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs):
```csharp
[Header("Priority Foundation (D5 / D13.13)")]
[Tooltip("Higher value = higher priority. Evaluated during Auto combat when multiple skills are ready.")]
[SerializeField] private int priority = 50;

public int Priority => priority;

public void SetPriority(int prio)
{
    priority = prio;
}
```

#### Historical Reference Values (from D13.13 & D6.12):
- **Ultimate (Thần Công / Bí Kỹ):** Priority `100` (Always evaluated first; can interrupt Basic Attack when Rage >= RageCost per D5).
- **Tuyệt Kỹ (Slot 2):** Priority `60` (Primary Mind Method active technique).
- **Ngoại Công 1 (Slot 3):** Priority `40` (External Skill 1).
- **Ngoại Công 2 (Slot 4):** Priority `30` (External Skill 2).
- **Đánh Thường (Basic Attack):** Priority `0` (Fallback when no skill is ready).

*All values remain 100% data-driven and configurable per ScriptableObject asset without code changes.*

### 7.3 Priority Resolution Algorithm (`SkillPriorityResolver`)
When Auto Battle is active and Hero is ready to cast:
1. **Query all equipped active skills:**
   - Iterate active slots: `SkillSlotType.Skill`, `ExternalSkill1`, `ExternalSkill2`.
   - Retrieve `SkillDefinitionSO` via `MindMethodManager.GetSelectedSkillForSlot(slot)`.
2. **Filter candidates by readiness:**
   - Skill is non-null and not passive.
   - Skill is not on cooldown (`!CooldownManager.IsOnCooldown(skill.SkillId)`).
   - Hero has permission to cast (`Hero.CanUseSkill`).
   - Hero has target in range / alive.
3. **Sort candidates by Priority descending:**
   - `Order = Priority (descending) -> SkillId (ordinal string tie-breaker)`.
4. **Select top candidate:**
   - Delegate request to `Hero.ExecuteSelectedSkill(candidate.Slot)`.

### 7.4 Ultimate Decision Flow (Independent & Preemptive per D5)
Evaluated before normal skills:
1. Check slotted Ultimate (`SkillSlotType.Ultimate`).
2. If `Hero.Rage.CurrentRage >= ultimateDef.RageCost` AND `!CooldownManager.IsOnCooldown(ultimateDef.SkillId)` AND `Hero.CanUseUltimate`:
   - If Hero is currently winding up a Basic Attack, interrupt it (`Hero.InterruptCurrentAction()`).
   - Invoke `Hero.ExecuteSelectedSkill(SkillSlotType.Ultimate)`.
   - Return immediately (Ultimate preempts all normal skills).

### 7.5 Autonomous Combat Decision Component (`HeroSkillDecisionController`)
Attached to `Hero` GameObject (or integrated into `Hero.TickSkillDecision` during `Update()`):
- Ticked each frame if `BattleManager.Instance.IsBattleActive && Hero.IsAlive && BattleManager.Instance.IsAutoBattle`.
- **Re-entry Guard:** If `Hero.IsCasting` is true, yields immediately (waits for cast/channel to complete or be interrupted).
- Respects:
  - DECISION (`HeroSkillDecisionController`)
  - REQUEST (`Hero.ExecuteSelectedSkill(slot)`)
  - VALIDATION (`SkillExecutionValidator.Validate(request)`)
  - EXECUTION (`SkillExecutor.Execute(request)`)
- Zero bypass of existing authorities.

---

## 8. Summary & Next Steps for User Alignment

Because **no Skill Priority Authority currently exists in code**, Section 4 requires:
> *"STOP trước implementation gameplay và tạo: P07_9_1_PRIORITY_DESIGN_GAP.md... CHỈ tiếp tục implementation nếu priority đã có authority hoặc được xác định rõ bằng thiết kế hiện hành."*

### Proposed Action Items Upon User Approval:
1. **Add `bool isAutoBattle` to `BattleManager.cs`** (establishing gameplay authority for Auto).
2. **Add `int priority` to `SkillDefinitionSO.cs`** (establishing data-driven priority authority per D13.13).
3. **Implement `HeroSkillDecisionController.cs`** (implementing autonomous decision layer conforming to D5, P07.9, and the call pipeline).
4. **Execute all 12 test scenarios (TEST 01 to TEST 12)** in automated test runner and Unity Play Mode.
5. **Run full regressions (P07.8 55/55, P07.9 36/36, Risk04 18/18, Master Regression).**
6. **Deliver `P07_9_1_HERO_SKILL_AUTONOMOUS_REPORT.md`.**

*Awaiting user approval of this design gap resolution before making any code modifications.*
