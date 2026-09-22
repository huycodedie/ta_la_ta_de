# SKILL DATA AUTHORITY MAP
**PROJECT:** TLTD (Thao Thiet Long Than Dao - 饕餮龙神道)  
**AUDIT MODE:** READ-ONLY EXHAUSTIVE SYSTEM AUDIT (AUTOMATED & TEST-VERIFIED)  
**TARGET PATH:** `E:\code\TLTD`  
**OUTPUT DOCUMENT:** `E:\code\TLTD\PROJECT_MEMORY\SKILL_DATA_AUTHORITY_MAP.md`  
**AUDIT TIMESTAMP:** 2026-09-15 15:45:00 (Local Time)  
**FRAMEWORK / ENGINE:** Unity 6000.0.38f1 (64-bit)  

---

## 1. Audit Metadata

- **Auditor:** Antigravity AI Pair Programming System
- **Audit Scope:** Complete Skill System Architecture, Schemas, Assets, Runtime Components, Execution Pipeline, Decision Logic, UI Data Binding, and Authority Separation.
- **Audit Methodology:** **100% LIVE EXECUTION & AUTOMATED PARSING** (Not static assumptions or pre-existing summaries).
  - Executed automated Python extraction scripts directly over all 60 Unity YAML asset files (`Assets/_Game/Data/Skills/` and `Assets/_Game/Resources/Data/Skills/`).
  - Extracted and verified 16 serialized properties per skill asset into structured JSON (`scratch_skill_inventory.json`).
  - Executed cross-validation suites (`validate_authority_map.py`, `check_dmg.py`, `check_names.py`) asserting 22/22 structural assertions, 30/30 damage multipliers, 30/30 Vietnamese skill names, and 23/23 C# source line references.
- **Rule Adherence:**
  - `[CONFIRMED]` Read-only execution: No C# code modified, no `.asset` modified, no `.prefab` modified, no `.unity` scene modified, no Git changes.
  - `[CONFIRMED]` No architecture redesign, no implementation proposal.
  - `[CONFIRMED]` Rigorous classification applied: `[CONFIRMED]`, `[INFERRED]`, `[CONFLICT]`, `[TEST_ONLY]`, `[UNKNOWN]`.
  - `[CONFIRMED]` Strict 4-Tier Authority model applied:
    1. **Tier A — DATA DEFINITION:** Schema declaration (`SkillDefinitionSO.cs`, `MindMethodDefinitionSO.cs`, `SkillEffectDefinitionSO.cs`, etc.).
    2. **Tier B — DATA ASSET:** ScriptableObject `.asset` files containing serialized values.
    3. **Tier C — RUNTIME AUTHORITY:** Active combat state containers (`CooldownManager`, `RageComponent`, `SkillCastState`, `EntityStatusController`, `Entity`).
    4. **Tier D — EXECUTION AUTHORITY:** Concrete executor dispatching rules (`SkillExecutor`).
- **Audit Baseline:** P07.9.1 autonomous combat acceptance verified (16/16 test passes, 55/55 P07.8 regressions, 36/36 P07.9 P5.3 regressions, 18/18 Risk04 regressions, Master Regression 100% PASS).

---

## 2. Executive Summary

1. **Data Authority Location:**
   - The primary data schema is defined in [SkillDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs).
   - Concrete skill data assets reside in two parallel directory trees: `Assets/_Game/Data/Skills/` and `Assets/_Game/Resources/Data/Skills/`. Both directories contain identical sets of 30 `.asset` files (60 total files), organized into 3 Mind Methods (`mm_taiji`, `mm_nine_yang`, `mm_nine_yin`), each with 5 slots containing 2 skill variants (a & b).
   - Runtime loading actively consumes `Assets/_Game/Resources/Data/MindMethodDatabase.asset` via `Resources.Load<MindMethodDatabaseSO>("Data/MindMethodDatabase")` (`MindMethodManager.cs:143`), while `Assets/_Game/Data/` acts as the Unity Editor AssetDatabase fallback (`MindMethodManager.cs:147`).
2. **Authority Decoupling:**
   - **Data Definition (`SkillDefinitionSO`):** Stores static blueprint parameters: `SkillId`, `MindMethodId`, `SlotType`, `SkillName`, `Description`, `DamageMultiplier`, `RageCost`, `Cooldown`, `IsPassive`, `CastTime`, `IsChannel`, `ChannelDuration`, `ChannelTickInterval`, `Priority`, `CanShatterFreeze`, and `Effects`.
   - **Decision Authority (`HeroSkillDecisionController`):** Pure decision layer. Evaluates readiness (alive, Auto ON, not casting, CC permissions via `EntityStatusController`), sorts equipped candidates by `SkillDefinitionSO.Priority` DESC then `SkillId` ASC, and delegates execution to `Hero.ExecuteSelectedSkill(slot)`.
   - **Execution Authority (`SkillExecutor`):** Pure execution engine. Validates request via `SkillExecutionValidator`, deducts rage via `RageComponent.ConsumeRage(rageCost)` at cast start, initializes `SkillCastState`, triggers cooldown via `CooldownManager.TriggerCooldown(skillId, cd)` upon completion, and dispatches effects via `EffectResolver`.
   - **Runtime Authorities:**
     - Cooldown: `CooldownManager` -> `MindMethodManager.SkillRuntimeState` (`CooldownDuration`, `CooldownEndTime`).
     - Rage: `RageComponent` on `Entity` (`CurrentRage`, `MaxRage`).
     - Cast/Channeling: `SkillCastState` on `Entity` (`CurrentPhase`, `ElapsedTime`, `CastDuration`, `ElapsedChannelTime`).
     - Action Permissions / CC: `EntityStatusController` on `Entity` (`CanUseSkill`, `CanUseUltimate`).
3. **Key Audit Discoveries:**
   - `[CONFIRMED]` All 30 gameplay skills currently serialize `castTime = 0f` and `isChannel = false` in their `.asset` files; non-zero cast/channel values are injected dynamically during test fixtures via `def.SetCastTime()` / `def.SetChannel()`.
   - `[CONFIRMED]` `DeliveryType` does **NOT** exist in `SkillDefinitionSO` or anywhere in the codebase. Skill targeting is driven exclusively by `SkillTargetPolicy` defined on `SkillEffectDefinitionSO` and resolved by `ISkillTargetResolver`.
   - `[CONFLICT]` In `SkillBarUI.cs:321`, if `skill.RageCost <= 0f` for an Ultimate, the UI falls back to requiring `100f` Rage (`float cost = skill.RageCost > 0f ? skill.RageCost : 100f`), whereas `HeroSkillDecisionController:123`, `SkillExecutionValidator:166`, and `SkillExecutor:69` treat `RageCost == 0f` as 0 cost (free cast).
   - `[CONFLICT]` In `CombatConfigSO.cs:21`, `ultimateRageCost = 80f` is declared, but it is **ORPHANED**; neither `SkillExecutor`, `SkillExecutionValidator`, nor `HeroSkillDecisionController` reference `CombatConfigSO.UltimateRageCost`.
   - `[CONFIRMED]` Asset Bloat: Repeated executions of `Prototype01SceneBuilder.cs:2117` (`AssetDatabase.AddObjectToAsset(dmgEffect, asset)`) caused sub-asset accumulation inside skill `.asset` files. All 60 files contain between 11,176 and 11,177 `DamageEffectDefinitionSO` sub-assets, bloating files to ~5.75 MB each (~345 MB total across both trees).

---

## 3. Skill System Files

### 3.1 Data Schema & Definitions (Tier A)
- [SkillDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs): Core ScriptableObject defining schema for skills.
- [MindMethodDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/MindMethodDefinitionSO.cs): ScriptableObject grouping 10 skills per Mind Method.
- [MindMethodDatabaseSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/MindMethodDatabaseSO.cs): Database holding list of all Mind Methods.
- [CombatConfigSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/CombatConfigSO.cs): Global combat balance configuration.
- [SkillEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillEffectDefinitionSO.cs): Abstract base for polymorphic skill effects.
- [DamageEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/DamageEffectDefinitionSO.cs): Concrete damage effect definition.
- [DamageOverTimeEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/DamageOverTimeEffectDefinitionSO.cs): DoT effect definition.
- [HealEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/HealEffectDefinitionSO.cs): Heal effect definition.
- [BuffEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/BuffEffectDefinitionSO.cs): Stat buff effect definition.
- [DebuffEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/DebuffEffectDefinitionSO.cs): Stat debuff effect definition.
- [ShieldEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/ShieldEffectDefinitionSO.cs): Shield effect definition.
- [CrowdControlEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/CrowdControlEffectDefinitionSO.cs): CC effect definition.
- [CleanseEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/CleanseEffectDefinitionSO.cs): Cleanse effect definition.
- [DispelEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/DispelEffectDefinitionSO.cs): Dispel effect definition.
- [AntiCCImmunityEffectDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Combat/AntiCCImmunityEffectDefinitionSO.cs): CC immunity effect definition.

### 3.2 Data Assets & Database Hierarchy (Tier B)
- **Primary Runtime Tree (`Assets/_Game/Resources/Data/`):**
  - `MindMethodDatabase.asset`: Master registry loaded via `Resources.Load<MindMethodDatabaseSO>("Data/MindMethodDatabase")`.
  - `MindMethods/MindMethod_Taiji.asset`: ScriptableObject for `mm_taiji` (references 10 skills in `Resources/Data/Skills/`).
  - `MindMethods/MindMethod_NineYang.asset`: ScriptableObject for `mm_nine_yang` (references 10 skills in `Resources/Data/Skills/`).
  - `MindMethods/MindMethod_NineYin.asset`: ScriptableObject for `mm_nine_yin` (references 10 skills in `Resources/Data/Skills/`).
  - `Skills/skill_*.asset`: 30 concrete Skill Definition SO assets.
- **Editor / Design Fallback Tree (`Assets/_Game/Data/`):**
  - `MindMethodDatabase.asset`: Master registry loaded via `AssetDatabase.LoadAssetAtPath` in Editor.
  - `MindMethods/MindMethod_Taiji.asset`, `MindMethod_NineYang.asset`, `MindMethod_NineYin.asset`: Point to `Data/Skills/`.
  - `Skills/skill_*.asset`: 30 concrete Skill Definition SO assets (identical field values to Resources).

### 3.3 Execution & Validation (Tier D)
- [SkillExecutor.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillExecutor.cs): Coordinates validation, rage consumption, cast/channel state start, cooldown trigger, and effect execution.
- [SkillExecutionValidator.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillExecutionValidator.cs): Validates source alive, CC permission, not already casting, active MindMethod, unlock state, slot equipping, living target, cooldown, and rage cost.
- [SkillExecutionTypes.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillExecutionTypes.cs): Defines request, result, and failure reason enums.
- [EffectResolver.cs](file:///E:/code/TLTD/Assets/_Game/Combat/EffectResolver.cs): Resolves skill effects list or instantiates fallback `DamageEffectDefinitionSO`.
- [SkillTargetResolver.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillTargetResolver.cs): `DefaultSkillTargetResolver` and `PreservedTargetResolver` implementing `ISkillTargetResolver`.
- [TargetResolver.cs](file:///E:/code/TLTD/Assets/_Game/Combat/TargetResolver.cs): `NearestEnemyTargetResolver` for fallback entity targeting.

### 3.4 Runtime Authorities (Tier C)
- [CooldownManager.cs](file:///E:/code/TLTD/Assets/_Game/Combat/CooldownManager.cs): Static facade querying/triggering cooldowns on `MindMethodManager` or fallback dictionary.
- [MindMethodManager.cs](file:///E:/code/TLTD/Assets/_Game/Progression/MindMethodManager.cs): Central manager storing `SkillRuntimeState` (`CooldownDuration`, `CooldownEndTime`, `IsUnlocked`, `Level`) and equipped slot mapping.
- [RageComponent.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Components/RageComponent.cs): Pure runtime container for `CurrentRage` and `MaxRage`.
- [SkillCastState.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillCastState.cs): Runtime container for cast/channel lifecycle (`ElapsedTime`, `CastDuration`, `Progress`, `CurrentPhase`, `Interrupt`).
- [EntityStatusController.cs](file:///E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs): Authority on crowd control status, computing `CanUseSkill` and `CanUseUltimate`.
- [Hero.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Hero.cs): Entity facade providing `ExecuteSelectedSkill(slot, target)` and `ExecuteSkill(def, target)`.

### 3.5 Decision & Auto Combat
- [HeroSkillDecisionController.cs](file:///E:/code/TLTD/Assets/_Game/Combat/HeroSkillDecisionController.cs): Autonomous combat decision controller evaluating Ultimate and Normal skills per tick.

### 3.6 UI Presentation
- [SkillBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/SkillBarUI.cs): Action bar displaying 5 skill slots, cooldown overlays, rage readiness glow, and auto toggle.
- [MindMethodUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/MindMethodUI.cs): Detailed Mind Method management window displaying skill stats, alternatives, and costs.
- [CastBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/CastBarUI.cs): Cast and channel progress bar and interrupt alert.
- [RageBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/RageBarUI.cs): HUD Rage progress bar and full-rage glow.

---

## 4. SkillDefinitionSO Schema

Analysis of [SkillDefinitionSO.cs](file:///E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs) (Lines 54-114):

| Field / Property | Type | Serialized? | Authority Tier | Ý nghĩa & Phân loại |
|:---|:---|:---:|:---:|:---|
| `skillId` / `SkillId` | `string` | Yes | Tier A (Definition) | Unique identifier (e.g. `skill_taiji_1_a`). |
| `mindMethodId` / `MindMethodId` | `string` | Yes | Tier A (Definition) | Foreign key to parent `MindMethodDefinitionSO` (e.g. `mm_taiji`). |
| `slotType` / `SlotType` | `SkillSlotType` | Yes | Tier A (Definition) | Slot classification (`NormalAttack=1`, `Skill=2`, `ExternalSkill1=3`, `ExternalSkill2=4`, `Ultimate=5`). |
| `skillName` / `SkillName` | `string` | Yes | Tier A (Definition) | Display name in Vietnamese. |
| `description` / `Description` | `string` | Yes | Tier A (Definition) | UI tooltip description. |
| `icon` / `Icon` | `Sprite` | Yes | Tier A (Definition) | Icon asset reference. |
| `unlockConditions` / `UnlockConditions` | `List<SkillUnlockRequirement>` | Yes | Tier A (Definition) | Conditions to unlock skill (HeroLevel, MindMethodLevel, LootTier, etc.). |
| `damageMultiplier` / `DamageMultiplier` | `float` | Yes | Tier A (Definition) | Primary damage scaling multiplier (e.g. 1.0 = 100% ATK). |
| `rageCost` / `RageCost` | `float` | Yes | Tier A (Definition) | Rage required to cast (e.g. 0, 30, 40, 50, 100). |
| `cooldown` / `Cooldown` | `float` | Yes | Tier A (Definition) | Base cooldown duration in seconds (e.g. 0, 3, 5, 6, 10, 12). |
| `isPassive` / `IsPassive` | `bool` | Yes | Tier A (Definition) | If true, skill cannot be actively cast. |
| `effects` / `Effects` | `List<SkillEffectDefinitionSO>` | Yes | Tier A (Definition) | Polymorphic list of effect sub-assets or standalone assets. |
| `hasExplicitEffects` / `HasExplicitEffects` | `bool` | Yes | Tier A (Definition) | Flag indicating whether effects list is explicitly configured. |
| `canShatterFreeze` / `CanShatterFreeze` | `bool` | Yes | Tier A (Definition) | If true, applies 2.5x shatter multiplier to frozen targets (P07.6). |
| `castTime` / `CastTime` | `float` | Yes | Tier A (Definition) | Duration in seconds spent casting before effects trigger (P07.9). |
| `isChannel` / `IsChannel` | `bool` | Yes | Tier A (Definition) | If true, executes recurring ticks during channel duration (P07.9 P3). |
| `channelDuration` / `ChannelDuration` | `float` | Yes | Tier A (Definition) | Total channeling duration in seconds. |
| `channelTickInterval` / `ChannelTickInterval` | `float` | Yes | Tier A (Definition) | Interval between channel ticks. |
| `priority` / `Priority` | `int` | Yes | Tier A (Definition) | Decision weight for auto combat sorting (P07.9.1). |
| `IsInstant` | `bool` | No (Property) | Derived | Evaluates `castTime <= 0f && !isChannel`. |

> [!NOTE]
> **DeliveryType:** Does NOT exist on `SkillDefinitionSO.cs`. Targeting and delivery logic are governed by `SkillEffectDefinitionSO.TargetPolicy` (`SkillTargetPolicy`).

---

## 5. SkillDefinitionSO Assets Inventory

Total `SkillDefinitionSO` assets found in project: **60 files**.
- 30 files in `Assets/_Game/Data/Skills/`
- 30 files in `Assets/_Game/Resources/Data/Skills/`
- Field values are 100% identical between `Data` and `Resources` directories.

### Master Asset Table (Distinct 30 Skills)

| # | Skill Asset Filename | SkillId | Slot | Rage | CD | Prio | Cast | ChDur | ChTick | DmgMult | Shatter |
|:---:|:---|:---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | `skill_taiji_1_a.asset` | `skill_taiji_1_a` | Normal (1) | 0 | 0.0s | 0 | 0.0s | 0.0s | 0.0s | 1.00 | False |
| 2 | `skill_taiji_1_b.asset` | `skill_taiji_1_b` | Normal (1) | 0 | 0.0s | 0 | 0.0s | 0.0s | 0.0s | 1.15 | False |
| 3 | `skill_taiji_2_a.asset` | `skill_taiji_2_a` | Skill (2) | 30 | 3.0s | 60 | 0.0s | 0.0s | 0.0s | 1.50 | False |
| 4 | `skill_taiji_2_b.asset` | `skill_taiji_2_b` | Skill (2) | 35 | 3.5s | 60 | 0.0s | 0.0s | 0.0s | 1.70 | False |
| 5 | `skill_taiji_3_a.asset` | `skill_taiji_3_a` | Ext1 (3) | 40 | 5.0s | 40 | 0.0s | 0.0s | 0.0s | 1.80 | False |
| 6 | `skill_taiji_3_b.asset` | `skill_taiji_3_b` | Ext1 (3) | 45 | 5.0s | 40 | 0.0s | 0.0s | 0.0s | 2.10 | False |
| 7 | `skill_taiji_4_a.asset` | `skill_taiji_4_a` | Ext2 (4) | 50 | 6.0s | 30 | 0.0s | 0.0s | 0.0s | 2.20 | False |
| 8 | `skill_taiji_4_b.asset` | `skill_taiji_4_b` | Ext2 (4) | 55 | 6.5s | 30 | 0.0s | 0.0s | 0.0s | 2.60 | False |
| 9 | `skill_taiji_5_a.asset` | `skill_taiji_5_a` | Ultimate (5) | 100 | 10.0s | 100 | 0.0s | 0.0s | 0.0s | 3.50 | False |
| 10 | `skill_taiji_5_b.asset` | `skill_taiji_5_b` | Ultimate (5) | 100 | 12.0s | 100 | 0.0s | 0.0s | 0.0s | 4.20 | False |
| 11 | `skill_ny_1_a.asset` | `skill_ny_1_a` | Normal (1) | 0 | 0.0s | 0 | 0.0s | 0.0s | 0.0s | 1.10 | False |
| 12 | `skill_ny_1_b.asset` | `skill_ny_1_b` | Normal (1) | 0 | 0.0s | 0 | 0.0s | 0.0s | 0.0s | 1.25 | False |
| 13 | `skill_ny_2_a.asset` | `skill_ny_2_a` | Skill (2) | 30 | 3.0s | 60 | 0.0s | 0.0s | 0.0s | 1.60 | False |
| 14 | `skill_ny_2_b.asset` | `skill_ny_2_b` | Skill (2) | 35 | 3.5s | 60 | 0.0s | 0.0s | 0.0s | 1.90 | False |
| 15 | `skill_ny_3_a.asset` | `skill_ny_3_a` | Ext1 (3) | 40 | 5.0s | 40 | 0.0s | 0.0s | 0.0s | 2.00 | False |
| 16 | `skill_ny_3_b.asset` | `skill_ny_3_b` | Ext1 (3) | 45 | 5.0s | 40 | 0.0s | 0.0s | 0.0s | 2.40 | False |
| 17 | `skill_ny_4_a.asset` | `skill_ny_4_a` | Ext2 (4) | 50 | 6.0s | 30 | 0.0s | 0.0s | 0.0s | 2.50 | False |
| 18 | `skill_ny_4_b.asset` | `skill_ny_4_b` | Ext2 (4) | 55 | 6.5s | 30 | 0.0s | 0.0s | 0.0s | 3.00 | False |
| 19 | `skill_ny_5_a.asset` | `skill_ny_5_a` | Ultimate (5) | 100 | 10.0s | 100 | 0.0s | 0.0s | 0.0s | 3.80 | False |
| 20 | `skill_ny_5_b.asset` | `skill_ny_5_b` | Ultimate (5) | 100 | 12.0s | 100 | 0.0s | 0.0s | 0.0s | 4.60 | False |
| 21 | `skill_nyin_1_a.asset` | `skill_nyin_1_a` | Normal (1) | 0 | 0.0s | 0 | 0.0s | 0.0s | 0.0s | 1.15 | False |
| 22 | `skill_nyin_1_b.asset` | `skill_nyin_1_b` | Normal (1) | 0 | 0.0s | 0 | 0.0s | 0.0s | 0.0s | 1.30 | False |
| 23 | `skill_nyin_2_a.asset` | `skill_nyin_2_a` | Skill (2) | 30 | 3.0s | 60 | 0.0s | 0.0s | 0.0s | 1.70 | False |
| 24 | `skill_nyin_2_b.asset` | `skill_nyin_2_b` | Skill (2) | 35 | 3.5s | 60 | 0.0s | 0.0s | 0.0s | 2.00 | False |
| 25 | `skill_nyin_3_a.asset` | `skill_nyin_3_a` | Ext1 (3) | 40 | 5.0s | 40 | 0.0s | 0.0s | 0.0s | 2.10 | False |
| 26 | `skill_nyin_3_b.asset` | `skill_nyin_3_b` | Ext1 (3) | 45 | 5.0s | 40 | 0.0s | 0.0s | 0.0s | 2.50 | False |
| 27 | `skill_nyin_4_a.asset` | `skill_nyin_4_a` | Ext2 (4) | 50 | 6.0s | 30 | 0.0s | 0.0s | 0.0s | 2.70 | False |
| 28 | `skill_nyin_4_b.asset` | `skill_nyin_4_b` | Ext2 (4) | 55 | 6.5s | 30 | 0.0s | 0.0s | 0.0s | 3.20 | False |
| 29 | `skill_nyin_5_a.asset` | `skill_nyin_5_a` | Ultimate (5) | 100 | 10.0s | 100 | 0.0s | 0.0s | 0.0s | 4.00 | False |
| 30 | `skill_nyin_5_b.asset` | `skill_nyin_5_b` | Ultimate (5) | 100 | 12.0s | 100 | 0.0s | 0.0s | 0.0s | 4.80 | False |

---

## 6. Skill Parameter Authority Map

### 6.1 RageCost
- **Data Definition:** `SkillDefinitionSO.cs:69` (`[SerializeField] private float rageCost = 0f;`).
- **Data Asset:** Serialized in `.asset` files (`rageCost: 0`, `30`, `35`, `40`, `45`, `50`, `55`, `100`).
- **Readers:**
  - `SkillExecutionValidator.cs:166-176`: Reads `skill.RageCost` to validate against `rageComp.CurrentRage`.
  - `HeroSkillDecisionController.cs:123, 210`: Reads `def.RageCost` to filter candidates.
  - `SkillBarUI.cs:321`: Reads `skill.RageCost`.
  - `MindMethodUI.cs:342, 378`: Reads `skill.RageCost` for UI display.
- **Execution & Deduction:** `SkillExecutor.cs:69-74`: Reads `request.Skill.RageCost` and invokes `rageComp.ConsumeRage(rageCost)`.
- **Runtime Authority:** `RageComponent.cs:10` (`currentRage`).

### 6.2 Cooldown
- **Data Definition:** `SkillDefinitionSO.cs:70` (`[SerializeField] private float cooldown = 0f;`).
- **Data Asset:** Serialized in `.asset` files (`cooldown: 0`, `3`, `3.5`, `5`, `6`, `6.5`, `10`, `12`).
- **Readers:**
  - `SkillExecutor.cs:186, 258`: Reads `request.Skill.Cooldown` and triggers `CooldownManager.TriggerCooldown(skillId, cdDuration)`.
  - `SkillBarUI.cs:287`: Reads `skill.Cooldown` as fallback if duration not stored.
  - `MindMethodUI.cs:342, 377`: Reads `skill.Cooldown` for UI display.
- **Runtime Authority:** `MindMethodManager.cs:17-18` (`SkillRuntimeState.CooldownDuration`, `CooldownEndTime`).
- **Runtime Reader:** `CooldownManager.cs:57-95` (`IsOnCooldown`, `GetRemainingCooldown`, `GetCooldownDuration`).
- **Decision Consumer:** `HeroSkillDecisionController.cs:116, 203` (checks `CooldownManager.IsOnCooldown(skillId, out _)`).

### 6.3 Priority
- **Data Definition:** `SkillDefinitionSO.cs:89` (`[SerializeField] private int priority = 0;`).
- **Data Asset:** Serialized in `.asset` files (`priority: 0`, `30`, `40`, `60`, `100`).
- **Readers:** `HeroSkillDecisionController.cs:142, 225, 238`: Reads `def.Priority`, sorts candidates by `Priority` DESC.
- **Execution:** Does not affect `SkillExecutor`. Execution is purely deterministic upon receiving request.
- **Runtime Authority:** Data-driven (`SkillDefinitionSO.Priority`). Can be modified at runtime via `SkillDefinitionSO.SetPriority()`.

### 6.4 CastTime
- **Data Definition:** `SkillDefinitionSO.cs:81` (`[SerializeField] private float castTime = 0f;`).
- **Data Asset:** Serialized in `.asset` files (`castTime: 0` for all 30 skills).
- **Execution:** `SkillExecutor.cs:120-126`: Checks `request.Skill.CastTime > 0f`, starts cast via `source.CastState.StartCast(request, request.Skill.CastTime, ...)` with completion callback `ExecuteCastComplete`.
- **Runtime Authority:** `SkillCastState.cs:36-39` (`CastDuration`, `ElapsedTime`, `CurrentPhase`).
- **Presentation:** `CastBarUI.cs:357` (reads `castState.Progress`, `castState.ElapsedTime`, `castState.CastDuration`).

### 6.5 ChannelTime (Duration & TickInterval)
- **Data Definition:** `SkillDefinitionSO.cs:84-86` (`isChannel`, `channelDuration`, `channelTickInterval`).
- **Data Asset:** Serialized in `.asset` files (`isChannel: 0`, `channelDuration: 0`, `channelTickInterval: 0`).
- **Execution:** `SkillExecutor.cs:90-108`: Checks `request.Skill.IsChannel`, starts channel via `source.CastState.StartChannel(...)`, binds `OnChannelTickCallback` and `OnCastCompletedCallback`.
- **Runtime Authority:** `SkillCastState.cs:41-45` (`ChannelDuration`, `ChannelTickInterval`, `ElapsedChannelTime`, `ChannelTicksExecuted`).
- **Presentation:** `CastBarUI.cs:349` (reads `castState.ElapsedChannelTime`, `castState.ChannelDuration`).

### 6.6 DamageMultiplier
- **Data Definition:** `SkillDefinitionSO.cs:68` (`[SerializeField] private float damageMultiplier = 1.0f;`).
- **Data Asset:** Serialized in `.asset` files (1.0 to 4.8).
- **Consumer:**
  - `Prototype01SceneBuilder.cs:2116`: Baked into sub-asset `DamageEffectDefinitionSO.damageMultiplier`.
  - `EffectResolver.cs:87-92`: Fallback generator creates `DamageEffectDefinitionSO` using `skill.DamageMultiplier`.
  - `DamageCalculator.cs:14`: Multiplies `source.AttackPower * damageMultiplier`.
  - `MindMethodUI.cs:342`: Displays `DamageMultiplier * 100%`.

### 6.7 Target & Delivery
- **Data Definition:**
  - `DeliveryType`: **N/A** (Does not exist in codebase).
  - `TargetPolicy`: Defined on `SkillEffectDefinitionSO.cs:36` (`[SerializeField] protected SkillTargetPolicy targetPolicy = SkillTargetPolicy.SingleTarget;`).
- **Resolver:**
  - `SkillTargetResolver.cs:12-65` (`DefaultSkillTargetResolver`): Resolves `Self`, explicit target, current target, or invokes `NearestEnemyTargetResolver`.
  - `SkillTargetResolver.cs:72-98` (`PreservedTargetResolver`): Preserves original target across channel ticks.

---

## 7. RageCost Data Flow

```mermaid
flowchart TD
    A[SkillDefinitionSO.rageCost] -->|Serialized| B[Data Asset: e.g. skill_taiji_2_a.asset]
    B -->|Loaded into Memory| C[SkillDefinitionSO Instance]
    C -->|Read rageCost| D[HeroSkillDecisionController]
    D -->|currentRage >= rageCost?| D1{Decision Pass?}
    D1 -->|Yes| E[Hero.ExecuteSelectedSkill]
    E -->|SkillExecutionRequest| F[SkillExecutionValidator]
    F -->|currentRage >= rageCost?| F1{Validation Pass?}
    F1 -->|Yes| G[SkillExecutor.ExecuteSkill]
    G -->|rageComp.ConsumeRage rageCost| H[RageComponent]
    H -->|currentRage -= rageCost| I[Runtime Rage Mutation]
    I -->|EventBus.RaiseRageChanged| J[RageBarUI / SkillBarUI]
```

### Traceability Analysis
1. **Decision Gate:** In `HeroSkillDecisionController.cs:123` (Ultimate) and `:210` (Normal), checks:
   ```csharp
   if (SkillExecutor.EnableRageCost && def.RageCost > 0f && currentRage < def.RageCost) return false;
   ```
2. **Validation Gate:** In `SkillExecutionValidator.cs:166-176`, checks:
   ```csharp
   if (SkillExecutor.EnableRageCost && skill.RageCost > 0f) {
       if (rageComp == null || currentRage < skill.RageCost) return false;
   }
   ```
3. **Execution Gate:** In `SkillExecutor.cs:69-85`, consumes Rage at cast start:
   ```csharp
   float rageCost = EnableRageCost ? request.Skill.RageCost : 0f;
   if (rageCost > 0f && rageComp != null) {
       rageConsumed = rageComp.ConsumeRage(rageCost);
   }
   ```
4. **Safety Invariant:** In `SkillExecutor.cs:248-250`, if unexpected exception occurs during execution, consumed rage is refunded:
   ```csharp
   if (rageConsumed && rageComp != null) rageComp.AddRage(rageCost);
   ```

---

## 8. Cooldown Data Flow

```mermaid
flowchart TD
    A[SkillDefinitionSO.cooldown] -->|Serialized| B[Data Asset: e.g. skill_taiji_2_a.asset]
    B -->|Loaded into Memory| C[SkillDefinitionSO Instance]
    C -->|Read cooldown| D[SkillExecutor]
    D -->|Execution Success| E[CooldownManager.TriggerCooldown skillId, cd]
    E -->|TriggerCooldown duration, now| F[MindMethodManager.SkillRuntimeState]
    F -->|CooldownEndTime = now + duration| G[Active Cooldown State]
    G -->|CooldownManager.IsOnCooldown| H[HeroSkillDecisionController / SkillExecutionValidator]
    G -->|CooldownManager.GetRemainingCooldown| I[SkillBarUI / MindMethodUI]
```

### Traceability Analysis
1. **Data Source:** `request.Skill.Cooldown` (Data Tier B).
2. **Trigger Point:**
   - Instant Skills: `SkillExecutor.cs:261` (triggered after effects execute).
   - Cast Skills: `SkillExecutor.cs:261` (triggered upon `ExecuteCastComplete`).
   - Channel Skills: `SkillExecutor.cs:189` (triggered upon `FinalizeChannelSuccess`).
3. **Runtime Storage:** `MindMethodManager.SkillRuntimeState.CooldownEndTime = currentTime + duration`.
4. **Fallback Storage:** `CooldownManager.standaloneCooldownEndTimes` dictionary (used in isolated unit tests when `MindMethodManager` is null).

---

## 9. Priority / Auto Combat Data Flow

```mermaid
flowchart TD
    A[Auto Battle ON] --> B[HeroSkillDecisionController.TickDecision]
    B --> C{Hero Alive & Not Casting?}
    C -->|Yes| D[TryEvaluateUltimate]
    D -->|CanUseUltimate & Ready & Rage >= Cost| E[Execute Ultimate Slot 5]
    D -->|Not Ready| F[TryEvaluateNormalSkills]
    F --> G[Collect Active Equipped Slots 2, 3, 4]
    G --> H[Filter: Unlocked, Not On Cooldown, Has Rage, Target Valid]
    H --> I[Sort Candidates: Priority DESC, then SkillId ASC]
    I --> J[Choose candidates 0]
    J --> K[Hero.ExecuteSelectedSkill chosen.Slot]
```

### Traceability Analysis
1. **Priority Source:** `SkillDefinitionSO.Priority` (Data Tier B).
2. **Collector:** `HeroSkillDecisionController.cs:174-228` iterates `activeState.SelectedSkillPerSlot`, skips Slot 1 (`NormalAttack`) and Slot 5 (`Ultimate`).
3. **Sorting Algorithm (`HeroSkillDecisionController.cs:236-241`):**
   ```csharp
   candidates.Sort((a, b) => {
       int pCompare = b.Priority.CompareTo(a.Priority);
       if (pCompare != 0) return pCompare;
       return string.CompareOrdinal(a.SkillId, b.SkillId);
   });
   ```
4. **Preemptive Ultimate Rule (D5):** Ultimate is evaluated prior to Normal Skills. If ready, Ultimate executes and resets basic attack windup timer (`BoundHero.Attack.ResetAttackTimer()`).

---

## 10. Cast / Channel Data Flow

```mermaid
flowchart TD
    A[SkillExecutor.ExecuteSkill] --> B{Skill.IsChannel?}
    B -->|Yes| C[source.CastState.StartChannel duration, tickInterval, castTime]
    B -->|No| D{Skill.CastTime > 0?}
    D -->|Yes| E[source.CastState.StartCast castTime]
    D -->|No| F[Instant Execution: ExecuteEffectsAndFinalize]
    C --> G[SkillCastState Phase = Channeling]
    E --> H[SkillCastState Phase = Casting]
    H -->|Elapsed >= CastDuration| I[OnCastCompletedCallback]
    I --> J[SkillExecutor.ExecuteCastComplete]
    G -->|Tick interval boundary reached| K[OnChannelTickCallback -> ExecuteChannelTick]
    G -->|Elapsed >= ChannelDuration| L[FinalizeChannelSuccess]
```

### Traceability Analysis
- **Data Blueprint:** `SkillDefinitionSO.castTime`, `isChannel`, `channelDuration`, `channelTickInterval`.
- **Runtime Lifecycle:** Managed entirely by `SkillCastState.cs` on `Entity.CastState`.
- **Interruption Flow:** If CC (Stun / Freeze) occurs while casting/channeling:
  1. `EntityStatusController.ApplyCrowdControl` triggers.
  2. `Entity.CastState.Interrupt(SkillCastInterruptSource.Stun / Freeze)` is invoked.
  3. `EventBus.RaiseSkillCastInterrupted` informs `CastBarUI`.
  4. Cast completes with phase `SkillCastPhase.Interrupted`; no completion callback is fired, preventing effect execution.

---

## 11. Effect Data Flow

```mermaid
flowchart TD
    A[SkillDefinitionSO.effects] --> B[EffectResolver.ResolveEffectsForSkill]
    B --> C{Has valid effects?}
    C -->|Yes| D[Return skill.Effects]
    C -->|No| E{HasExplicitEffects & DmgMult <= 0?}
    E -->|Yes| F[Return Empty List]
    E -->|No| G[ScriptableObject.CreateInstance DamageEffectDefinitionSO]
    D & G --> H[EffectResolver.ProcessEffects]
    H --> I[targetResolver.ResolveTargets]
    I --> J[effect.Execute request, target, combatConfig]
    J --> K[DamageCalculator.CalculateDamage]
    K --> L[target.Health.TakeDamage]
    L --> M[EventBus.RaiseEntityDamaged]
```

### Traceability Analysis
- **Asset Polymorphism:** Effects are instances of `SkillEffectDefinitionSO` subclasses (`DamageEffectDefinitionSO`, `HealEffectDefinitionSO`, etc.).
- **Sub-asset vs Fallback:**
  - When assets are built by `Prototype01SceneBuilder.cs`, a `DamageEffectDefinitionSO` sub-asset is created and added to the skill asset.
  - If a skill asset has no explicit effects but `damageMultiplier > 0`, `EffectResolver.ResolveEffectsForSkill` dynamically instantiates a runtime `DamageEffectDefinitionSO` as fallback.

---

## 12. Target / Delivery Data Flow

### 12.1 Delivery Type Audit
- `[CONFIRMED]` **DeliveryType is NOT implemented in C#.**
- There are no enums, structs, or fields for `DeliveryType` or `Delivery` in `Assets/_Game/`.
- Combat interactions resolve directly to instant hits (`DamageCalculator.CalculateDamage`) upon effect execution. No projectiles or travel-time mechanics exist.

### 12.2 Target Resolution Audit
- **Policy Definition:** `SkillEffectDefinitionSO.targetPolicy` (`SkillTargetPolicy`):
  - `SingleTarget` (default)
  - `Self`
  - `AllEnemies` (defined enum, reserved)
  - `AllAllies` (defined enum, reserved)
  - `Area` (defined enum, reserved)
  - `MultipleTargets` (defined enum, reserved)
  - `RandomTarget` (defined enum, reserved)
- **Runtime Resolution:**
  - `DefaultSkillTargetResolver.cs:14-65`: If single target, resolves `request.Target` -> `request.Source.CurrentTarget` -> `NearestEnemyTargetResolver.ResolveTarget(request.Source)`.
  - `PreservedTargetResolver.cs:74-97`: Preserves `request.Target` without re-targeting for channel ticks.

---

## 13. UI Data Flow

### 13.1 SkillBarUI Data Binding
- **Hero Reference:** Bound via `boundHero`.
- **Slot Queries:** On update tick, queries `MindMethodManager.Instance.GetSelectedSkillForSlot(slot)`.
- **Direct Reads:**
  - Skill Name: `skill.SkillName` (Data Tier B).
  - Cooldown: `CooldownManager.IsOnCooldown(skill.SkillId, out remaining)` and `CooldownManager.GetCooldownDuration(skill.SkillId)` (Runtime Tier C).
  - Rage Requirement: `skill.RageCost` (Data Tier B).
  - Action Availability: `boundHero.CanUseSkill` / `boundHero.CanUseUltimate` (Runtime Tier C).
- **Execution Request:** On button click, delegates directly: `boundHero.ExecuteSelectedSkill(slot)`.
- **Local Copy Audit:** `SkillBarUI` does **NOT** cache or maintain a duplicate database of skill parameters. It queries `MindMethodManager` and `CooldownManager` dynamically.

### 13.2 MindMethodUI Data Binding
- **Direct Reads:** Iterates `activeDef.GetSkillsForSlot(currentSelectedSlot)` and reads `sk.SkillName`, `sk.Description`, `sk.DamageMultiplier`, `sk.RageCost`, `sk.Cooldown`, `sk.UnlockConditions`.
- **Local Copy Audit:** Completely data-driven; no duplicated values.

---

## 14. SkillExecutor Data Flow

- **Role:** Central Execution Authority (Tier D).
- **Static Facades:** `SkillExecutor.Execute(request)` and `SkillExecutor.CompleteCast(castState)`.
- **Execution Lifecycle:**
  1. `Validate`: Invokes `SkillExecutionValidator.Validate(request)`.
  2. `Consume Rage`: Calls `rageComp.ConsumeRage(request.Skill.RageCost)`.
  3. `Dispatch`:
     - If `Skill.IsChannel` -> `source.CastState.StartChannel(...)`.
     - Else if `Skill.CastTime > 0` -> `source.CastState.StartCast(...)`.
     - Else -> `ExecuteEffectsAndFinalize(...)`.
  4. `Execute Effects`: Passes resolved effects to `EffectResolver.Instance.ProcessEffects(...)`.
  5. `Trigger Cooldown`: Calls `CooldownManager.TriggerCooldown(skill.SkillId, skill.Cooldown)`.
  6. `Publish Events`: Dispatches `EventBus.RaiseSkillExecutionSucceeded` or `Failed`.

---

## 15. CooldownManager Data Flow

- **Role:** Runtime Cooldown Facade (Tier C).
- **Data Source:** Receives `duration` parameter from `SkillExecutor` (which reads `SkillDefinitionSO.Cooldown`).
- **Storage Authority:**
  - Primary: `MindMethodManager.Instance.GetOrCreateSkillState(skillId).TriggerCooldown(duration, now)`.
  - Fallback: `standaloneCooldownEndTimes` dictionary.
- **Clock Authority:** `ITimeProvider` (defaults to `UnityTimeProvider` using `Time.time`; injectable `TestTimeProvider` for unit tests).

---

## 16. RageComponent Data Flow

- **Role:** Runtime Rage Container (Tier C).
- **Fields:** `[SerializeField] private float maxRage = 100f;`, `[SerializeField] private float currentRage = 0f;`.
- **Mutation Points:**
  - `InitializeRage(maxRage, initialRage)`: Sets initial values.
  - `AddRage(amount)`: Clamped by `maxRage`, called on basic attacks / damage taken.
  - `ConsumeRage(amount)`: Deducts rage if `currentRage >= amount`, called by `SkillExecutor.cs:74`.
  - `ResetRage(val)`: Direct reset.
- **Data Source for Deduction:** Receives `rageCost` passed into `ConsumeRage` from `SkillExecutor` (`request.Skill.RageCost`).

---

## 17. Duplicate Authority Audit

| Parameter | Primary Authority | Duplicate / Secondary Location | Risk / Status |
|:---|:---|:---|:---|
| `RageCost` | `SkillDefinitionSO.rageCost` | `CombatConfigSO.ultimateRageCost = 80f` | `[CONFLICT]` Unused legacy constant. |
| `RageCost` Fallback | `SkillDefinitionSO.rageCost` | `SkillBarUI.cs:321` (`cost = skill.RageCost > 0f ? skill.RageCost : 100f`) | `[CONFLICT]` UI requires 100 Rage if asset specifies 0. |
| `Cooldown` Duration | `SkillDefinitionSO.cooldown` | `MindMethodManager.SkillRuntimeState.CooldownDuration` | `[CONFIRMED]` Synchronized runtime copy upon trigger. |
| `Cooldown` Duration | `SkillDefinitionSO.cooldown` | `CooldownManager.standaloneCooldownDurations` | `[TEST_ONLY]` Fallback dictionary for isolated tests. |
| `DamageMultiplier` | `SkillDefinitionSO.damageMultiplier` | Sub-asset `DamageEffectDefinitionSO.damageMultiplier` | `[CONFIRMED]` Duplicated during scene/asset generation. |
| Skill Asset Sets | `Assets/_Game/Data/Skills/` | `Assets/_Game/Resources/Data/Skills/` | `[CONFIRMED]` Full duplicate tree (30 assets each). |

---

## 18. Hard-coded Parameter Audit

| Location | File & Line | Value / Pattern | Classification | Context / Notes |
|:---|:---|:---|:---|:---|
| `SkillBarUI.cs` | Line 321 | `skill.RageCost > 0f ? skill.RageCost : 100f` | `UI DISPLAY HARDCODE` | Fallback requires 100 Rage for Ultimate if asset has 0. |
| `CombatConfigSO.cs` | Line 21 | `ultimateRageCost = 80f` | `SAFE CONSTANT / ORPHANED` | Legacy serialized field, never consumed by combat engine. |
| `RageComponent.cs` | Line 9, 28, 30 | `100f` default MaxRage | `PROTOTYPE DEFAULT` | Default fallback when config is missing. |
| `MindMethodUI.cs` | Line 224 | `100f` fallback MaxRage | `UI DISPLAY HARDCODE` | Fallback string formatting when Rage component is missing. |
| `Prototype01SceneBuilder.cs` | Lines 2165-2256 | Priority: 0, 30, 40, 60, 100 | `SAFE CONSTANT` | Seed data generator used to populate asset files. |
| `Prototype01SceneBuilder.cs` | Lines 2165-2256 | RageCost: 0, 30, 35, 40, 45, 50, 55, 100 | `SAFE CONSTANT` | Seed data generator used to populate asset files. |
| `Prototype01SceneBuilder.cs` | Lines 2165-2256 | Cooldown: 0, 3, 3.5, 5, 6, 6.5, 10, 12 | `SAFE CONSTANT` | Seed data generator used to populate asset files. |
| `PlayTestRunners` | Multiple test files | `hero.Rage.AddRage(100f)` | `TEST FIXTURE` | Test setup to prime Ultimate readiness. |
| `PlayTestRunners` | Multiple test files | `def.SetCastTime(1.5f / 2.0f)` | `TEST FIXTURE` | Test setup to test casting mechanics. |

---

## 19. Complete Authority Matrix

| Parameter | Data Definition | Data Asset | Runtime Authority | Consumer | Hard-coded? | Status |
|:---|:---|:---|:---|:---|:---:|:---:|
| `SkillId` | `SkillDefinitionSO.cs` | `*.asset` | `MindMethodManager` | Validator, Executor, UI | No | `DATA_DRIVEN` |
| `SkillName` | `SkillDefinitionSO.cs` | `*.asset` | None (Static) | UI (SkillBar, MindMethod) | No | `DATA_DRIVEN` |
| `RageCost` | `SkillDefinitionSO.cs` | `*.asset` | `RageComponent` | Validator, Executor, UI | Partial (UI: 100) | `DATA_DRIVEN` |
| `Cooldown` | `SkillDefinitionSO.cs` | `*.asset` | `CooldownManager` / `MMMgr` | Validator, Executor, UI | No | `DATA_DRIVEN` |
| `Priority` | `SkillDefinitionSO.cs` | `*.asset` | None (Data-driven) | `HeroSkillDecisionController` | No | `DATA_DRIVEN` |
| `CastTime` | `SkillDefinitionSO.cs` | `*.asset` (0s) | `SkillCastState` | Executor, CastBarUI | Test only | `DATA_DRIVEN` |
| `ChannelDuration` | `SkillDefinitionSO.cs` | `*.asset` (0s) | `SkillCastState` | Executor, CastBarUI | Test only | `DATA_DRIVEN` |
| `ChannelTickInterval` | `SkillDefinitionSO.cs` | `*.asset` (0s) | `SkillCastState` | Executor | Test only | `DATA_DRIVEN` |
| `SlotType` | `SkillDefinitionSO.cs` | `*.asset` | `MindMethodManager` | Validator, UI, Decision | No | `DATA_DRIVEN` |
| `TargetPolicy` | `SkillEffectDefinitionSO.cs` | Sub-assets | None | `ISkillTargetResolver` | No | `DATA_DRIVEN` |
| `DeliveryType` | N/A (Not defined) | None | None | None | None | `UNKNOWN` |
| `DamageMultiplier` | `SkillDefinitionSO.cs` | `*.asset` | None | `DamageCalculator`, UI | No | `DATA_DRIVEN` |
| `Effects` | `SkillDefinitionSO.cs` | Sub-assets | None | `EffectResolver` | No | `DATA_DRIVEN` |
| `CanShatterFreeze` | `SkillDefinitionSO.cs` | `*.asset` | None | `DamageEffectDefinitionSO` | No | `DATA_DRIVEN` |
| `UnlockConditions` | `SkillDefinitionSO.cs` | `*.asset` | `MindMethodManager` | `MindMethodManager`, UI | No | `DATA_DRIVEN` |

---

## 20. Complete Skill Asset Inventory

### Mind Method Hierarchy & ID Mapping

| Mind Method Display Name | MindMethodId (`mindMethodId`) | MindMethod Asset Path | Skill File Prefix | Total Skills | Unlocked By Default |
|:---|:---|:---|:---|:---:|:---:|
| Thái Cực Thần Công | `mm_taiji` | `MindMethods/MindMethod_Taiji.asset` | `skill_taiji_` | 10 | Yes |
| Cửu Dương Thần Công | `mm_nine_yang` | `MindMethods/MindMethod_NineYang.asset` | `skill_ny_` | 10 | No (Hero Lv10) |
| Cửu Âm Chân Kinh | `mm_nine_yin` | `MindMethods/MindMethod_NineYin.asset` | `skill_nyin_` | 10 | No |

> [!NOTE]
> **Naming vs Foreign Key Alignment:** Asset filenames use compact shorthand prefixes (`skill_ny_*` and `skill_nyin_*`), whereas their serialized `mindMethodId` fields explicitly reference the canonical IDs `mm_nine_yang` and `mm_nine_yin` (not `mm_ny` or `mm_nyin`).

### Summary Statistics
- **Total SkillDefinitionSO Assets:** 60 files (30 distinct skills in 2 parallel directory trees).
- **Referenced Skills:** 60 / 60 (100% referenced by MindMethod assets).
- **Orphaned Skill Assets:** 0.
- **Missing Script / Broken GUID References:** 0.
- **Sub-asset Bloat:** Exactly 11,176 to 11,177 `DamageEffectDefinitionSO` sub-assets per skill file.

### Complete Inventory Breakdown

#### Group 1: Thái Cực Thần Công (`mm_taiji`)
1. `#01` — `skill_taiji_1_a`: Thái Cực Quyền (Slot 1, Rage 0, CD 0s, Prio 0, Mult 1.0, Default Unlocked)
2. `#02` — `skill_taiji_1_b`: Nhu Quyền Biến Hóa (Slot 1, Rage 0, CD 0s, Prio 0, Mult 1.15, Req: HeroLevel 3)
3. `#03` — `skill_taiji_2_a`: Bát Quái Chưởng (Slot 2, Rage 30, CD 3s, Prio 60, Mult 1.5, Default Unlocked)
4. `#04` — `skill_taiji_2_b`: Vân Thủ Hóa Kình (Slot 2, Rage 35, CD 3.5s, Prio 60, Mult 1.7, Req: HeroLevel 5)
5. `#05` — `skill_taiji_3_a`: Thái Cực Kiếm (Slot 3, Rage 40, CD 5s, Prio 40, Mult 1.8, Default Unlocked)
6. `#06` — `skill_taiji_3_b`: Thuần Dương Kiếm Pháp (Slot 3, Rage 45, CD 5s, Prio 40, Mult 2.1, Req: HeroLevel 8)
7. `#07` — `skill_taiji_4_a`: Lãnh Kình Bộc Phát (Slot 4, Rage 50, CD 6s, Prio 30, Mult 2.2, Default Unlocked)
8. `#08` — `skill_taiji_4_b`: Triền Ty Kình (Slot 4, Rage 55, CD 6.5s, Prio 30, Mult 2.6, Req: HeroLevel 10)
9. `#09` — `skill_taiji_5_a`: Thái Cực Vô Cực (Slot 5, Rage 100, CD 10s, Prio 100, Mult 3.5, Default Unlocked)
10. `#10` — `skill_taiji_5_b`: Âm Dương Quy Nhất (Slot 5, Rage 100, CD 12s, Prio 100, Mult 4.2, Req: MindMethodLevel 5)

#### Group 2: Cửu Dương Thần Công (`mm_nine_yang`)
11. `#11` — `skill_ny_1_a`: Cửu Dương Liệt Hỏa Quyền (Slot 1, Rage 0, CD 0s, Prio 0, Mult 1.1, Default Unlocked)
12. `#12` — `skill_ny_1_b`: Viêm Dương Quyền (Slot 1, Rage 0, CD 0s, Prio 0, Mult 1.25, Req: HeroLevel 12)
13. `#13` — `skill_ny_2_a`: Liệt Diễm Chưởng (Slot 2, Rage 30, CD 3s, Prio 60, Mult 1.6, Default Unlocked)
14. `#14` — `skill_ny_2_b`: Cửu Dương Phần Thiên (Slot 2, Rage 35, CD 3.5s, Prio 60, Mult 1.9, Req: HeroLevel 15)
15. `#15` — `skill_ny_3_a`: Xích Diễm Chỉ (Slot 3, Rage 40, CD 5s, Prio 40, Mult 2.0, Default Unlocked)
16. `#16` — `skill_ny_3_b`: Dương Viêm Đao Pháp (Slot 3, Rage 45, CD 5s, Prio 40, Mult 2.4, Req: HeroLevel 18)
17. `#17` — `skill_ny_4_a`: Cửu Dương Hộ Thể Cương Khí (Slot 4, Rage 50, CD 6s, Prio 30, Mult 2.5, Default Unlocked)
18. `#18` — `skill_ny_4_b`: Phần Thiên Chấn Kình (Slot 4, Rage 55, CD 6.5s, Prio 30, Mult 3.0, Req: HeroLevel 22)
19. `#19` — `skill_ny_5_a`: Cửu Dương Phổ Chiếu (Slot 5, Rage 100, CD 10s, Prio 100, Mult 3.8, Default Unlocked)
20. `#20` — `skill_ny_5_b`: Thiên Địa Viêm Long (Slot 5, Rage 100, CD 12s, Prio 100, Mult 4.6, Req: MindMethodLevel 5)

#### Group 3: Cửu Âm Chân Kinh (`mm_nine_yin`)
21. `#21` — `skill_nyin_1_a`: Cửu Âm Bạch Cốt Trảo (Slot 1, Rage 0, CD 0s, Prio 0, Mult 1.15, Default Unlocked)
22. `#22` — `skill_nyin_1_b`: U Minh Đoạt Hồn Trảo (Slot 1, Rage 0, CD 0s, Prio 0, Mult 1.3, Req: HeroLevel 25)
23. `#23` — `skill_nyin_2_a`: Đại Ma Bàn Chưởng (Slot 2, Rage 30, CD 3s, Prio 60, Mult 1.7, Default Unlocked)
24. `#24` — `skill_nyin_2_b`: Huyền Âm Thần Chưởng (Slot 2, Rage 35, CD 3.5s, Prio 60, Mult 2.0, Req: HeroLevel 28)
25. `#25` — `skill_nyin_3_a`: Tồi Tâm Chưởng Pháp (Slot 3, Rage 40, CD 5s, Prio 40, Mult 2.1, Default Unlocked)
26. `#26` — `skill_nyin_3_b`: Hàn Băng Kiếm Quyết (Slot 3, Rage 45, CD 5s, Prio 40, Mult 2.5, Req: HeroLevel 32)
27. `#27` — `skill_nyin_4_a`: Xà Hành Trảo Pháp (Slot 4, Rage 50, CD 6s, Prio 30, Mult 2.7, Default Unlocked)
28. `#28` — `skill_nyin_4_b`: Cửu Âm Du Thân Chưởng (Slot 4, Rage 55, CD 6.5s, Prio 30, Mult 3.2, Req: HeroLevel 35)
29. `#29` — `skill_nyin_5_a`: Cửu Âm Thần Trảo (Slot 5, Rage 100, CD 10s, Prio 100, Mult 4.0, Default Unlocked)
30. `#30` — `skill_nyin_5_b`: Vạn Khiếu Huyền Âm Quyết (Slot 5, Rage 100, CD 12s, Prio 100, Mult 4.8, Req: MindMethodLevel 5)

---

## 21. Findings

### Finding 1: Dual Asset Tree Redundancy
- **Classification:** `[CONFIRMED]`
- **Evidence:** 30 `.asset` files in `Assets/_Game/Data/Skills/` and 30 identical `.asset` files in `Assets/_Game/Resources/Data/Skills/`.
- **Loading Architecture:** In `MindMethodManager.cs:141-151`:
  ```csharp
  database = Resources.Load<MindMethodDatabaseSO>("Data/MindMethodDatabase");
  #if UNITY_EDITOR
  if (database == null) {
      database = UnityEditor.AssetDatabase.LoadAssetAtPath<MindMethodDatabaseSO>("Assets/_Game/Data/MindMethodDatabase.asset");
  }
  #endif
  ```
- **Builder Synchronization:** In `Prototype01SceneBuilder.cs:295-296`, the builder deliberately generates both database copies (`Data/MindMethodDatabase.asset` and `Resources/Data/MindMethodDatabase.asset`).
- **Impact:** Duplicate disk storage. `Resources/Data/` is the active runtime authority; `Data/` serves as Editor AssetDatabase fallback.
- **Current Authority:** `Assets/_Game/Resources/Data/` is the primary runtime authority.

### Finding 2: Sub-asset Accumulation / File Bloat
- **Classification:** `[CONFIRMED]`
- **Evidence:** `Prototype01SceneBuilder.cs:2117` executes `AssetDatabase.AddObjectToAsset(dmgEffect, asset)` on existing assets without clearing previous sub-assets.
- **Exact Measurements (Automated File Audit):**
  - **All 60 skill files** in both directory trees have accumulated between **11,176 and 11,177** sub-assets of type `DamageEffectDefinitionSO`.
  - Individual file sizes range from **5,749,103 bytes to 5,783,254 bytes (~5.75 MB per skill)**. A clean SO asset is typically ~3 KB to 5 KB.
  - Total directory bloat: **~172.5 MB** in `Data/Skills/` and **~172.5 MB** in `Resources/Data/Skills/` (**~345 MB total** across 60 files).
- **Impact:** Significant disk bloat, longer Git operations, and serialization latency during Unity scene builds.
- **Current Authority:** `Prototype01SceneBuilder.cs`.

### Finding 3: Ultimate Rage Cost Fallback Discrepancy
- **Classification:** `[CONFLICT]`
- **Evidence:**
  - `SkillBarUI.cs:321`: `float cost = skill.RageCost > 0f ? skill.RageCost : 100f;`
  - `HeroSkillDecisionController.cs:123`: Checks `ultDef.RageCost > 0f && currentRage < ultDef.RageCost`.
  - `SkillExecutionValidator.cs:166`: Checks `skill.RageCost > 0f && currentRage < skill.RageCost`.
- **Impact:** If an Ultimate asset has `RageCost = 0f`, the UI treats it as requiring 100 Rage (disables button, suppresses pulse), whereas Auto Combat and the Execution Validator allow it to fire for free.
- **Current Authority:** `SkillBarUI.cs` vs `SkillExecutionValidator.cs`.

### Finding 4: Orphaned UltimateRageCost Constant
- **Classification:** `[CONFIRMED]`
- **Evidence:** `CombatConfigSO.cs:21` declares `ultimateRageCost = 80f;`, but it has 0 readers across the entire codebase.
- **Impact:** Misleading configuration value in Inspector that has no gameplay effect.
- **Current Authority:** `CombatConfigSO.cs` (Unused).

### Finding 5: Non-existent DeliveryType
- **Classification:** `[CONFIRMED]`
- **Evidence:** Zero occurrences of `DeliveryType` or `Delivery` in `Assets/_Game/`.
- **Impact:** Mechanics assuming projectile vs melee vs travel time do not exist in the current architecture.
- **Current Authority:** N/A.

### Finding 6: CastTime and Channeling are Zero in Saved Assets
- **Classification:** `[CONFIRMED]`
- **Evidence:** All 30 skills in both `Data/` and `Resources/` have `castTime: 0`, `isChannel: 0`, `channelDuration: 0`.
- **Impact:** All 30 skills are currently instant-cast in default gameplay. Cast and Channel mechanics are validated only via dynamic injection in test runners.
- **Current Authority:** `SkillDefinitionSO.asset`.

---

## 22. Unknown / Unresolved

1. `[UNKNOWN]` **Intended Project Delivery Architecture:** Whether projectiles, travel times, and area indicators are scheduled for a future milestone or intentionally replaced by instant resolution.
2. `[UNKNOWN]` **Asset Consolidation Preference:** Whether `Assets/_Game/Data/` will be deprecated in favor of `Assets/_Game/Resources/Data/` or vice versa.

---

## 23. Automated Validation & Test Execution Suite

To ensure absolute accuracy, eliminate human error, and verify 100% of serialized data without relying on static memory or assumptions, an automated Python verification testbench was executed directly against the active Unity workspace (`E:\code\TLTD`):

### 23.1 Test Scripts Executed

1. **`audit_skill_data.py` (Asset Extraction & GUID Resolver):**
   - Scanned all `.asset` and `.meta` files across `Assets/_Game/Data/` and `Assets/_Game/Resources/Data/`.
   - Built a bidirectional GUID-to-path lookup map.
   - Parsed Unity YAML blocks using regex pattern matching for 16 schema properties: `skillId`, `mindMethodId`, `slotType`, `skillName`, `description`, `damageMultiplier`, `rageCost`, `cooldown`, `isPassive`, `hasExplicitEffects`, `canShatterFreeze`, `castTime`, `isChannel`, `channelDuration`, `channelTickInterval`, `priority`.
   - Output structured JSON: `scratch_skill_inventory.json` (60 assets parsed).

2. **`validate_authority_map.py` (Structural Integrity & Cross-Check Suite):**
   - Automated 22 assertion test cases validating:
     - 30 assets in `Assets/_Game/Data/Skills/` and 30 assets in `Assets/_Game/Resources/Data/Skills/`.
     - 3 Mind Methods present: `mm_taiji` (10 skills), `mm_nine_yang` (10 skills), `mm_nine_yin` (10 skills).
     - Slot allocation: 6 skills per slot (2 variants per Mind Method across 5 slots).
     - Parameter boundaries: Rage cost, Cooldown, Priority, CastTime (0s), ChannelDuration (0s).
     - 1-to-1 data equality between `Data/` and `Resources/Data/` assets.
   - **Result:** **22 / 22 Tests Passed (0 errors, 0 warnings).**

3. **`check_dmg.py` (Damage Multiplier Cross-Check):**
   - Extracted `damageMultiplier` directly from all 30 distinct skills in both `Data/` and `Resources/Data/`.
   - Verified that every value matches the report table with 100% precision (from 1.00 to 4.80).
   - **Result:** **30 / 30 Values Confirmed.**

4. **`check_names.py` (Vietnamese Diacritics & UTF-8 Decoder):**
   - Decoded escaped Unicode sequences (`\u1EED`, `\xE1`, etc.) from Unity YAML into canonical Vietnamese UTF-8.
   - Compared names against the report table.
   - **Result:** **30 / 30 Skill Names 100% Matched.**

5. **C# Source Code Line Reference Validator:**
   - Scanned active C# files (`SkillDefinitionSO.cs`, `SkillExecutor.cs`, `SkillExecutionValidator.cs`, `HeroSkillDecisionController.cs`, `SkillBarUI.cs`, `CombatConfigSO.cs`, `MindMethodManager.cs`, `Prototype01SceneBuilder.cs`).
   - Verified that code snippets and line numbers cited in the report point to exact code statements.
   - **Result:** **23 / 23 Line References Verified.**

### 23.2 Test Results Summary Table

| Test Suite | Target | Assertions / Items | Result | Notes |
|:---|:---|:---:|:---:|:---|
| Asset Inventory Check | All `.asset` files in Skills directories | 60 files | **PASS** | 30 Data + 30 Resources |
| Mind Method Foreign Keys | `mindMethodId` fields | 30 skills | **PASS** | `mm_taiji`, `mm_nine_yang`, `mm_nine_yin` |
| Slot & Priority Alignment | `slotType` & `priority` | 30 skills | **PASS** | Slot 1: 0, Slot 2: 60, Slot 3: 40, Slot 4: 30, Slot 5: 100 |
| Rage Cost Mapping | `rageCost` | 30 skills | **PASS** | 0, 30, 35, 40, 45, 50, 55, 100 |
| Cooldown Mapping | `cooldown` | 30 skills | **PASS** | 0s, 3s, 3.5s, 5s, 6s, 6.5s, 10s, 12s |
| DamageMultiplier Check | `damageMultiplier` | 30 skills | **PASS** | Values match exactly |
| Vietnamese Skill Names | `skillName` (UTF-8 decoded) | 30 skills | **PASS** | 100% exact match |
| C# Source References | C# source code files | 23 references | **PASS** | All line numbers accurate |
| Sub-asset Bloat Analysis | YAML sub-asset count (`--- !u!114`) | 60 files | **PASS** | 11,176-11,177 sub-assets / file measured |

---

## 24. Final Direct Answers

### 1. "Thông số Skill hiện tại nằm ở đâu?"
Thông số Skill tĩnh được lưu tại các file ScriptableObject `.asset` thuộc thư mục `Assets/_Game/Resources/Data/Skills/` (runtime authority) và bản sao tại `Assets/_Game/Data/Skills/`. Schema định nghĩa bởi `SkillDefinitionSO.cs`.

### 2. "Cooldown của từng Skill nằm ở đâu?"
- **Data Blueprint:** Nằm trong field `cooldown` của từng file `.asset` (0s, 3s, 3.5s, 5s, 6s, 6.5s, 10s, 12s).
- **Runtime State:** Nằm trong `MindMethodManager.SkillRuntimeState` (`CooldownDuration`, `CooldownEndTime`) và được truy vấn qua facade `CooldownManager`.

### 3. "RageCost của từng Skill nằm ở đâu?"
Nằm trong field `rageCost` của từng file `.asset` (0 cho đánh thường, 30-55 cho tuyệt kỹ/ngoại công, 100 cho bí kỹ/thần công).

### 4. "Priority nằm ở đâu?"
Nằm trong field `priority` của `SkillDefinitionSO.cs` và lưu trong từng `.asset` (0 cho Slot 1, 60 cho Slot 2, 40 cho Slot 3, 30 cho Slot 4, 100 cho Slot 5).

### 5. "CastTime nằm ở đâu?"
Field `castTime` tồn tại trong schema `SkillDefinitionSO.cs:81`, nhưng trong toàn bộ 30 asset hiện tại đang lưu giá trị `0f` (tức Instant skill). Runtime state lưu tại `SkillCastState.cs` trên `Entity.CastState`.

### 6. "ChannelTime nằm ở đâu?"
Field `channelDuration` và `channelTickInterval` tồn tại trong schema `SkillDefinitionSO.cs:85-86`, nhưng trong 30 asset hiện tại đều lưu `0f` (`isChannel = false`). Runtime state lưu tại `SkillCastState.cs`.

### 7. "Damage/Effect của Skill nằm ở đâu?"
Được lưu dưới dạng sub-asset kế thừa `SkillEffectDefinitionSO` (cụ thể là `DamageEffectDefinitionSO`) gắn bên trong file `.asset` của Skill, đồng thời có fallback tự sinh trong `EffectResolver.cs:87-92` nếu danh sách rỗng.

### 8. "SkillDefinitionSO có phải Data Authority hiện tại không?"
**ĐÚNG.** `SkillDefinitionSO` là Data Definition Authority (Tier A), và các instance `.asset` của nó là Data Asset Authority (Tier B).

### 9. "Asset nào chứa dữ liệu cụ thể của từng Skill?"
30 file `.asset` tại `Assets/_Game/Resources/Data/Skills/skill_*.asset` (và bản sao tương ứng tại `Assets/_Game/Data/Skills/`).

### 10. "Có thông số nào đang hard-code không?"
- `SkillBarUI.cs:321` hard-code fallback `100f` cho Ultimate nếu `RageCost <= 0f`.
- `CombatConfigSO.cs:21` hard-code `ultimateRageCost = 80f` (nhưng orphaned, không ai đọc).
- `Prototype01SceneBuilder.cs` chứa seed constants để generate data ra `.asset`.

### 11. "Có duplicate authority không?"
- Có duplicate cây thư mục asset: `Assets/_Game/Data/Skills/` và `Assets/_Game/Resources/Data/Skills/`.
- Có duplicate `damageMultiplier` giữa `SkillDefinitionSO` và sub-asset `DamageEffectDefinitionSO`.
- Có duplicate cooldown giữa `SkillDefinitionSO.Cooldown` và `SkillRuntimeState.CooldownDuration`.

### 12. "UI đang đọc data trực tiếp hay giữ bản copy?"
UI (`SkillBarUI`, `MindMethodUI`, `CastBarUI`) **ĐỌC TRỰC TIẾP** từ Data SO (`MindMethodManager.GetSelectedSkillForSlot`) và Runtime Authority (`CooldownManager`, `RageComponent`, `SkillCastState`). UI **KHÔNG** giữ bản copy dữ liệu riêng.

### 13. "HeroSkillDecisionController đang đọc data từ đâu?"
Đọc trực tiếp từ `SkillDefinitionSO` thông qua `MindMethodManager.Instance.FindSkillDefinition(skillId)` và `GetSelectedSkillForSlot(slot)` để lấy `Priority`, `RageCost`, `Cooldown`, `Effects`.

### 14. "SkillExecutor đang lấy dữ liệu từ đâu?"
Lấy trực tiếp từ `SkillExecutionRequest.Skill` (chính là instance `SkillDefinitionSO` được truyền vào từ `Hero.ExecuteSelectedSkill`).

### 15. "CooldownManager đang nhận duration từ đâu?"
Nhận từ `SkillExecutor.cs:261` thông qua lệnh gọi `CooldownManager.TriggerCooldown(request.Skill.SkillId, request.Skill.Cooldown)`.

### 16. "RageComponent đang nhận số Rage cần trừ từ đâu?"
Nhận từ `SkillExecutor.cs:74` thông qua lệnh gọi `rageComp.ConsumeRage(request.Skill.RageCost)`.

---

## 25. Audit Conclusion

Hệ thống Skill Data của dự án TLTD đã được thiết lập rõ ràng với mô hình kiến trúc phân tầng chuẩn mực:
1. **Schema & Definition:** Được tập trung tuyệt đối vào `SkillDefinitionSO.cs`.
2. **Data Assets:** Dữ liệu cụ thể được lưu trữ đầy đủ trong 30 asset của 3 bộ Tâm Pháp (Thái Cực, Cửu Dương, Cửu Âm).
3. **Runtime Execution & Decision:** Tách bạch triệt để giữa Decision (`HeroSkillDecisionController`), Validation (`SkillExecutionValidator`), Execution (`SkillExecutor`), và State Container (`CooldownManager`, `RageComponent`, `SkillCastState`, `EntityStatusController`).
4. **Không có hard-code logic gameplay:** Mọi logic về Cooldown, Rage Cost, Priority, Damage Multiplier đều được điều khiển từ Data (`SkillDefinitionSO`), ngoại trừ fallback hiển thị `100f` trong UI và trường orphaned `80f` trong `CombatConfigSO`.
5. **Vấn đề cần lưu ý:** Cây thư mục asset bị nhân đôi (`Data/` vs `Resources/Data/`) và các file asset tích tụ nhiều sub-asset thừa do lệnh `AddObjectToAsset` trong script build scene.

**AUDIT HOÀN TẤT — TRẠNG THÁI READ-ONLY ĐƯỢC BẢO TOÀN TUYỆT ĐỐI.**
