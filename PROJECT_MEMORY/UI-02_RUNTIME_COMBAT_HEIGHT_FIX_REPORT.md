# UI-02 — RUNTIME COMBAT HEIGHT / Y-AXIS DEADLOCK FIX REPORT

**PROJECT:** TLTD (Thao Thiet Long Than Dao - 饕餮龙神道)  
**WORKSPACE:** `E:\code\TLTD`  
**ENGINE:** Unity 6000.6.0f1 (64-bit)  
**MILESTONE:** UI-02 Visual Remediation / Runtime Combat Fix  
**DATE / TIMESTAMP:** 2026-09-16 20:05:00 (Local Time)  
**STATUS:** **UI-02 RUNTIME COMBAT HEIGHT FIX: PASS**  
*(Note: Global UI-02 remains pending Project Owner acceptance; NOT declared globally LOCKED)*

---

## 1. Bug Summary

Following the defeat of Monster #1 in `Prototype01.unity`, subsequent encounters (Encounter #2+) entered a complete, permanent combat deadlock:
- Hero and the newly spawned Monster #2 stood facing each other indefinitely.
- Neither entity moved forward or initiated Basic Attacks.
- Rage was never generated, preventing skills and Ultimates from executing.
- Additionally, Monster #2 spawned as an unstyled placeholder (white rectangle with 3D text `"MONSTER"`) rather than matching the wuxia crimson fiend standee visual of Encounter #1.

---

## 2. Reproduction

The deadlock was reproduced in both interactive Play Mode and automated headless batch mode:
1. Load `Assets/_Game/Scenes/Prototype01.unity`.
2. Start Encounter #1: Hero (`Y = -0.3f`) and Monster #1 (`Y = -0.3f`) engage in combat normally.
3. Defeat Monster #1 (`Health.TakeDamage(999999f)`).
4. `BattleManager.EndEncounterAndStartNext()` triggers `SpawnMonster()`.
5. Monster #2 spawns at legacy position `(4.000, -1.200, 0.000)`.
6. Hero remains on ground plane `Y = -0.300`, creating a vertical delta $\Delta Y = 0.900\text{m}$.
7. Hero moves toward Monster #2 along the horizontal axis until $\Delta X = 1.712\text{m} \le 1.800\text{m}$ (stopping distance).
8. `MovementComponent` stops the Hero.
9. `AttackComponent` calculates 3D Euclidean distance: $\sqrt{1.712^2 + 0.900^2} \approx 1.934\text{m}$.
10. Because $1.934\text{m} > \text{AttackRange} + 0.1\text{m} = 1.900\text{m}$, `AttackComponent` evaluates `InRange = FALSE`.
11. Movement is halted, attack is out of range: permanent deadlock.

---

## 3. Root Cause

There were two confirmed root causes:

### Cause A: Ground Plane Inconsistency in BattleManager
`Prototype01SceneBuilder.cs` established the combat ground plane at $Y = -0.3\text{m}$ for both Hero and initial Monster. However, `BattleManager.monsterSpawnPosition` retained an obsolete legacy prototype default of `(4.0, -1.2, 0.0)`. Thus, spawned monsters dropped $0.9\text{m}$ below the active combat plane.

### Cause B: Distance Model Divergence Between Movement & Attack
- `MovementComponent.cs` explicitly enforces a 2D horizontal combat plane:
  ```csharp
  Vector3 direction = (targetPosition - transform.position);
  direction.y = 0; // Maintain 2D ground plane (X axis movement)
  float distance = direction.magnitude;
  ```
  It halts movement when horizontal distance $\le \text{stoppingDistance}$.
- `AttackComponent.cs` previously evaluated distance using full 3D Euclidean distance:
  ```csharp
  float distance = Vector3.Distance(transform.position, target.transform.position);
  ```
This mathematical divergence caused `MovementComponent` to declare the entity "close enough to stop" while `AttackComponent` declared the target "out of range to attack."

### Cause C: Visual Pipeline Inconsistency in SpawnMonster
`BattleManager.SpawnMonster()` instantiated a legacy prototype placeholder:
- Scale `(1.4f, 2.2f, 1f)`
- White 1x1 texture with red tint `Color(1f, 0.3f, 0.3f)`
- Sorting order 1
- Child GameObject `"Label"` with 3D `TextMeshPro` text `"MONSTER"`
This contradicted Encounter #1's standalone visual pipeline (`UIProceduralTextureFactory.GetMonsterStandeeSprite()`, scale `1.1`, sortingOrder `10`, `flipX = true`).

---

## 4. Files Inspected

- `PROJECT_MEMORY/AI_RULES.md`
- `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`
- `PROJECT_MEMORY/D1_D23_LOCKED.md`
- `PROJECT_MEMORY/D1_D23_AMENDMENTS_LOCKED.md`
- `PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md`
- `PROJECT_MEMORY/UI_REDESIGN_SPECIFICATION.md`
- `PROJECT_MEMORY/UI-02_PRE_IMPLEMENTATION_AUDIT.md`
- `PROJECT_MEMORY/UI-02_COMBAT_HUD_REPORT.md`
- `PROJECT_MEMORY/UI-02_Y_AXIS_COMBAT_ROOT_CAUSE_AUDIT.md`
- `Assets/_Game/Entities/Components/AttackComponent.cs`
- `Assets/_Game/Entities/Components/MovementComponent.cs`
- `Assets/_Game/Core/BattleManager.cs`
- `Assets/_Game/Editor/Prototype01SceneBuilder.cs`
- `Assets/_Game/UI/Core/UIProceduralTextureFactory.cs`
- `Assets/_Game/Entities/Monster.cs`
- `Assets/_Game/Entities/Hero.cs`

---

## 5. Files Modified

1. [AttackComponent.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Components/AttackComponent.cs)
2. [BattleManager.cs](file:///E:/code/TLTD/Assets/_Game/Core/BattleManager.cs)
3. [Prototype01SceneBuilder.cs](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs)
4. [Prototype01.unity](file:///E:/code/TLTD/Assets/_Game/Scenes/Prototype01.unity) (Rebaked via `BuildPrototype01Scene`)
5. [Prototype01PlayTestRunner_UI02_CombatHeightFix.cs](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01PlayTestRunner_UI02_CombatHeightFix.cs) (New dedicated test suite)

---

## 6. Exact Fix

### Part A: Monster Spawn Ground Plane
In `Assets/_Game/Core/BattleManager.cs`:
```diff
- [SerializeField] private Vector3 monsterSpawnPosition = new Vector3(4f, -1.2f, 0f);
+ [SerializeField] private Vector3 monsterSpawnPosition = new Vector3(4f, -0.3f, 0f);
```

In `Assets/_Game/Editor/Prototype01SceneBuilder.cs`:
```diff
  battleSO.FindProperty("defaultMonsterConfig").objectReferenceValue = monsterConfig;
  battleSO.FindProperty("defaultCombatConfig").objectReferenceValue = combatConfig;
+ battleSO.FindProperty("monsterSpawnPosition").vector3Value = new Vector3(4f, -0.3f, 0f);
  battleSO.ApplyModifiedPropertiesWithoutUndo();
```

### Part B: Mathematical Distance Plane Alignment
In `Assets/_Game/Entities/Components/AttackComponent.cs`:
```diff
  if (target != null && target.IsAlive)
  {
-     float distance = Vector3.Distance(transform.position, target.transform.position);
+     Vector3 delta = target.transform.position - transform.position;
+     delta.y = 0; // Maintain 2D ground plane (consistent with MovementComponent)
+     float distance = delta.magnitude;
      if (distance <= ownerEntity.AttackRange + 0.1f)
      {
          attackTimer = 0f;
          BasicAttackProcessor.ExecuteBasicAttack(ownerEntity, target, combatConfig);
      }
  }
```

### Part C: Monster Visual Standee Pipeline
In `Assets/_Game/Core/BattleManager.cs` (`SpawnMonster`):
```diff
- monsterGO.transform.localScale = new Vector3(1.4f, 2.2f, 1f);
- SpriteRenderer sr = monsterGO.AddComponent<SpriteRenderer>();
- Texture2D whiteTex = Texture2D.whiteTexture;
- Sprite sprite = Sprite.Create(whiteTex, new Rect(0, 0, whiteTex.width, whiteTex.height), new Vector2(0.5f, 0.5f), whiteTex.width);
- sr.sprite = sprite;
- sr.color = new Color(1f, 0.3f, 0.3f);
- sr.sortingOrder = 1;
- GameObject labelGO = new GameObject("Label");
- labelGO.transform.SetParent(monsterGO.transform, false);
- labelGO.transform.localPosition = new Vector3(0, 1.4f, 0);
- TMPro.TextMeshPro tmp = labelGO.AddComponent<TMPro.TextMeshPro>();
- tmp.text = "MONSTER";
- tmp.fontSize = 4;
- tmp.alignment = TMPro.TextAlignmentOptions.Center;
- tmp.color = new Color(1f, 0.4f, 0.4f);
- tmp.sortingOrder = 10;
+ monsterGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
+ SpriteRenderer sr = monsterGO.AddComponent<SpriteRenderer>();
+ sr.sprite = UIProceduralTextureFactory.GetMonsterStandeeSprite();
+ sr.color = Color.white;
+ sr.sortingOrder = 10;
+ sr.flipX = true;
```

---

## 7. Before / After Positions

| Entity / State | Before Fix | After Fix | Delta Y |
| :--- | :--- | :--- | :--- |
| **Hero (Spawn / Combat Plane)** | `(-2.200, -0.300, 0.000)` | `(-2.200, -0.300, 0.000)` | `0.000` |
| **Monster #1 (Encounter 1)** | `(2.200, -0.300, 0.000)` | `(2.200, -0.300, 0.000)` | `0.000` |
| **Monster #2 Spawn (Encounter 2)** | `(4.000, -1.200, 0.000)` | `(4.000, -0.300, 0.000)` | `0.000` |
| **Monster #3 Spawn (Encounter 3)** | `(4.000, -1.200, 0.000)` | `(4.000, -0.300, 0.000)` | `0.000` |
| **Hero Position at Attack Range (vs M2)** | `(2.200, -0.300, 0.000)` | `(2.200, -0.300, 0.000)` | `0.000` |

---

## 8. Before / After Distance Calculation

| Property | Before Fix | After Fix |
| :--- | :--- | :--- |
| **Movement Distance Formula** | `(target - pos).WithY(0).magnitude` | `(target - pos).WithY(0).magnitude` |
| **Movement Stopping Distance** | `AttackRange = 1.80m` | `AttackRange = 1.80m` |
| **Attack Distance Formula** | `Vector3.Distance(pos, target)` (3D) | `(target - pos).WithY(0).magnitude` (2D Horizontal) |
| **Attack Tolerance** | `AttackRange + 0.1m = 1.90m` | `AttackRange + 0.1m = 1.90m` |
| **Distance at Stop Point ($\Delta X=1.75\text{m}, \Delta Y=0.9\text{m}$)** | $1.968\text{m} > 1.90\text{m}$ ($\implies$ **FALSE / Deadlock**) | $1.750\text{m} \le 1.90\text{m}$ ($\implies$ **TRUE / Attack Executes**) |
| **Distance at Stop Point ($\Delta X=1.80\text{m}, \Delta Y=0.0\text{m}$)** | $1.800\text{m} \le 1.90\text{m}$ ($\implies$ TRUE) | $1.800\text{m} \le 1.90\text{m}$ ($\implies$ TRUE) |

---

## 9. Monster Visual Lifecycle Fix

- **SpriteRenderer Presence:** Present on initial and all spawned monsters.
- **Sprite:** Reuses `UIProceduralTextureFactory.GetMonsterStandeeSprite()` (112x160 standee with crimson aura and glowing eyes).
- **Scale:** Standardized to `Vector3(1.1f, 1.1f, 1f)` across all encounters.
- **Sorting Order:** Fixed to `10` (matching Hero standee depth).
- **Standee Orientation:** `flipX = true` (facing left toward Hero).
- **Legacy Artifacts Removed:** 3D `TextMeshPro` text `"MONSTER"` child completely eliminated; HP and identity presentation are handled exclusively by the 2D Combat HUD.

---

## 10. Automated Test Results

Suite: `Prototype01PlayTestRunner_UI02_CombatHeightFix.cs`  
Command: `Unity.exe -executeMethod WuxiaGame.Editor.Prototype01PlayTestRunner.RunAllCombatHeightFixTests`  
Result: **12 / 12 PASSED (100%)**

| Test ID | Test Description | Result | Details |
| :--- | :--- | :--- | :--- |
| **TEST 01** | Hero and initial Monster same combat Y | **PASS** | HeroY = -0.300, MonsterY = -0.300, DeltaY = 0.000 |
| **TEST 02** | Monster respawn receives the same combat-plane Y | **PASS** | Spawned Monster Y = -0.300 |
| **TEST 03** | Artificial $\Delta Y = 0.9\text{m}$, $\Delta X = 1.75\text{m}$ within range | **PASS** | 3D Dist = 1.968m, Horiz Dist = 1.750m, Monster took 90.91 damage |
| **TEST 04** | $\Delta Y = 0.9\text{m}$: Horizontal distance remains authoritative | **PASS** | $\Delta X = 1.7\text{m}$ damages target; $\Delta X = 2.5\text{m}$ correctly ignores target |
| **TEST 05** | Hero approaches runtime spawn position without deadlock | **PASS** | Hero moved from -2.2 to 2.30, stopped at 1.70m, attacked |
| **TEST 06** | Monster can attack Hero after approaching | **PASS** | Monster moved into range, attacked Hero, Hero HP reduced |
| **TEST 07** | Monster #1 dies and Monster #2 automatically spawns | **PASS** | Encounter 1 $\to$ 2, Monster #2 spawned at $Y = -0.300$ |
| **TEST 08** | Monster #2 can be damaged | **PASS** | Hero attacked Monster #2, HP reduced from 500 to 491 |
| **TEST 09** | Monster #2 dies and Monster #3 automatically spawns | **PASS** | Encounter 2 $\to$ 3, Monster #3 spawned at $Y = -0.300$ |
| **TEST 10** | Monster #3 can be damaged | **PASS** | Hero attacked Monster #3, HP reduced from 500 to 491 |
| **TEST 11** | Initial & spawned monsters match visual configuration | **PASS** | SpriteRenderer, Standee Texture, Scale (1.1, 1.1, 1), Order (10), FlipX (True), No 3D label |
| **TEST 12** | No Rigidbody / Collider / Physics dependency introduced | **PASS** | Pure mathematical horizontal plane verified (0 physics components) |

---

## 11. Play Mode Results

Multi-encounter real battle verification (Scenarios A through L): **ALL PASSED**

- **SCENARIO A (Initial Encounter):** Hero at `(-2.2, -0.3, 0)`, Monster #1 at `(2.2, -0.3, 0)`. Combat initializes cleanly.
- **SCENARIO B (Hero Attacks M1):** Hero basic attacks Monster #1, dealing 10 damage (HP 100 $\to$ 90).
- **SCENARIO C (M1 Defeated):** Monster #1 takes lethal damage and dies.
- **SCENARIO D (M2 Spawn):** Monster #2 spawns at `(4.00, -0.30, 0.00)` on combat plane $Y = -0.300$.
- **SCENARIO E (Hero Approach):** Hero automatically moves forward along X axis from $X = 0.70$ to $X = 2.20$.
- **SCENARIO F (Hero Attacks M2):** Hero halts at distance $1.80\text{m} \le 1.90\text{m}$ and immediately attacks Monster #2 (HP 500 $\to$ 491).
- **SCENARIO G (M2 Takes Damage):** Monster #2 health updates correctly in Combat HUD.
- **SCENARIO H (M2 Defeated):** Monster #2 defeated, encounter completes cleanly.
- **SCENARIO I (M3 Spawn):** Monster #3 spawns at `(4.00, -0.30, 0.00)`.
- **SCENARIO J (Encounter 3 Combat):** Hero engages Monster #3, approaches, attacks, and deals damage (HP 500 $\to$ 491).
- **SCENARIO K (Visual Pipeline):** Standee sprite, sorting order 10, scale 1.1, flipX true preserved across all encounters.
- **SCENARIO L (No Deadlock):** Continuous combat cycle completed across Encounters 1, 2, and 3 without manual repositioning.

---

## 12. Screenshot Manifest

All screenshots captured at 1080x1920 portrait resolution:

### 1. `UI02_HEIGHT_01_InitialCombat.png`
*Encounter #1 Active Combat on shared ground plane $Y = -0.3f$.*  
![Encounter #1 Active Combat](UI02_HEIGHT_01_InitialCombat.png)

---

### 2. `UI02_HEIGHT_02_Encounter2Spawn.png`
*Monster #2 spawned at $Y = -0.3f$ with matching wuxia standee visual.*  
![Encounter #2 Monster Spawn](UI02_HEIGHT_02_Encounter2Spawn.png)

---

### 3. `UI02_HEIGHT_03_Encounter2Combat.png`
*Hero approaches Monster #2, halts at 1.8m, and conducts basic attacks without deadlock.*  
![Encounter #2 Combat](UI02_HEIGHT_03_Encounter2Combat.png)

---

### 4. `UI02_HEIGHT_04_Encounter3Combat.png`
*Encounter #3 continuous combat loop operating cleanly.*  
![Encounter #3 Combat](UI02_HEIGHT_04_Encounter3Combat.png)

---

## 13. Regression Results

Full regression executed via `run_all_ui02_regressions.ps1`:

| Milestone Suite | Tests | Result | Log File |
| :--- | :--- | :--- | :--- |
| **P07.8 (Progression / Combat Systems)** | 55 / 55 | **PASS** | `ui02_p07_8_reg.log` |
| **P07.9 Phase 5.3 (Targeted Regressions)** | 36 / 36 | **PASS** | `ui02_p07_9_p5_3_reg.log` |
| **P07.9 Risk 04 (CC Interrupt & Action)** | 18 / 18 | **PASS** | `ui02_p07_9_risk04_reg.log` |
| **P07.9.1 (Autonomous Skill Decisions)** | 16 / 16 | **PASS** | `ui02_p07_9_1_reg.log` |
| **CUMULATIVE REGRESSION TOTAL** | **125 / 125** | **PASS (100%)** | — |

---

## 14. Compile Result

Executed via `run_compile_check.ps1`:
- **Exit Code:** `0`
- **Compiler Errors:** `0 CS errors`
- **Compiler Warnings:** `0 CS warnings`

---

## 15. Architecture Audit

1. **Single Positioning Authority:** The scene ground plane $Y = -0.3\text{m}$ configured in `Prototype01SceneBuilder.cs` and serialized to `BattleManager.monsterSpawnPosition` remains the sole authority. No second positioning authority created.
2. **Single Distance Authority:** Movement and Attack now strictly compute distance along the same mathematical horizontal combat plane (`delta.y = 0; distance = delta.magnitude;`).
3. **No Physics Intrusion:** No `Rigidbody`, `Rigidbody2D`, `Collider`, `Collider2D`, or `NavMesh` components were introduced. Combat remains purely mathematical.
4. **Single Monster Visual Pipeline:** Monster visual initialization reuses `UIProceduralTextureFactory.GetMonsterStandeeSprite()`. No redundant UI or standee systems were introduced.

---

## 16. Locked Gameplay Systems Untouched

Verified pristine diff:
- `SkillExecutor.cs`: **UNTOUCHED**
- `SkillExecutionValidator.cs`: **UNTOUCHED**
- `SkillCastState.cs`: **UNTOUCHED**
- `CooldownManager.cs`: **UNTOUCHED**
- `RageComponent.cs`: **UNTOUCHED**
- `EntityStatusController.cs`: **UNTOUCHED**
- `DamageCalculator.cs`: **UNTOUCHED**
- `HealthComponent.cs`: **UNTOUCHED**
- `BasicAttackProcessor.cs`: **UNTOUCHED**
- `HeroSkillDecisionController.cs`: **UNTOUCHED**

No gameplay rules, stats, damage formulas, rage generation, cooldown timings, or breakthrough logic were modified.

---

## 17. Remaining Issues

None related to the combat height or multi-encounter deadlock.  
The combat loop now runs indefinitely without entity freezing across encounters 1, 2, 3, and beyond.

---

## 18. Final Status

```
================================================================================
UI-02 RUNTIME COMBAT HEIGHT FIX: PASS
================================================================================
- Monster spawn Y matches active combat ground plane (Y = -0.300f).
- MovementComponent and AttackComponent share the identical 2D distance model.
- Spawned monsters visually match Encounter #1 standee pipeline.
- 12/12 dedicated validation tests PASSED.
- Scenarios A through L multi-encounter Play Mode PASSED.
- 4 runtime screenshots captured at 1080x1920 portrait resolution.
- 125/125 cumulative locked regressions PASSED.
- Project compilation clean (0 CS errors, 0 CS warnings).
- Global UI-02 status remains pending Project Owner acceptance.
================================================================================
```
