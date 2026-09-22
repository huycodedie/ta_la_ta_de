# UI-02 — COMBAT ZONE & COMBAT HUD PRE-IMPLEMENTATION AUDIT

**PROJECT:** TLTD (Thao Thiet Long Than Dao - 饕餮龙神道)  
**AUDIT MODE:** READ-ONLY PRE-IMPLEMENTATION AUDIT (GATE 0)  
**TARGET PATH:** `E:\code\TLTD`  
**OUTPUT DOCUMENT:** `E:\code\TLTD\PROJECT_MEMORY\UI-02_PRE_IMPLEMENTATION_AUDIT.md`  
**DATE / TIMESTAMP:** 2026-09-15 20:35:00 (Local Time)  
**FRAMEWORK / ENGINE:** Unity 6000.6.0f1 (64-bit)  
**STATUS:** PRE-IMPLEMENTATION AUDIT COMPLETE — AWAITING IMPLEMENTATION  

---

## 1. Executive Summary & Audit Purpose

Milestone **UI-02 (Combat Zone & HUD Polish)** upgrades the central combat viewport and battle HUD of TLTD from a functional developer/debug prototype into a polished, mobile portrait 2D wuxia/anime presentation (`1080x1920`, Safe Area compliant).

In accordance with:
- `PROJECT_MEMORY/AI_RULES.md`
- `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`
- `PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md`
- `PROJECT_MEMORY/UI_REDESIGN_SPECIFICATION.md`
- `PROJECT_MEMORY/P07_8_LOCKED.md`
- `PROJECT_MEMORY/P07_9_LOCKED.md`
- `PROJECT_MEMORY/P07_9_1_LOCKED.md`

**Core Invariant:** UI-02 is **strictly a presentation layer** enhancement. UI-02 must **NOT** introduce, modify, or duplicate any gameplay authority, combat calculation, or state mutation.

This audit evaluates the exact state of all 12 target UI systems, maps their runtime authorities, establishes strict file change boundaries, and defines the proposed UI-only scope.

---

## 2. Current Implementation Audit (12 Core UI Components)

### 2.1 BattleHUD
- **Source File:** [BattleHUD.cs](file:///E:/code/TLTD/Assets/_Game/UI/BattleHUD.cs)
- **Hierarchy Location:** Component attached to root `Canvas` in `Prototype01.unity` (instantiated and wired via [Prototype01SceneBuilder.cs:473](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L473)).
- **Existing References:**
  - `victoryPanel`, `defeatPanel`, `restartButton`, `startButton`
  - `statusText`, `defeatText`
  - Shield references: `heroShieldRoot`, `heroShieldFill`, `heroShieldText`, `monsterShieldRoot`, `monsterShieldFill`, `monsterShieldText`
  - CastBar references: `heroCastBarUI`, `heroCastBarRoot`, `heroCastBarFill`, `heroCastBarText`, `heroInterruptText`
- **Runtime Authority:**
  - Battle Lifecycle: [BattleManager.cs](file:///E:/code/TLTD/Assets/_Game/Combat/BattleManager.cs) via `EventBus.OnBattleStateChanged`.
  - Shield Presentation: [EntityStatusController.cs](file:///E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs) via `EventBus.OnShieldApplied`, `OnShieldAbsorbed`, `OnShieldDepleted`, `OnShieldExpired`, `OnShieldRemoved`.
  - Entity Lifecycle: `EventBus.OnEntitySpawned`, `OnEntityDied`.
- **Current Behavior:** Pure presentation. Displays victory/defeat overlays and drives shield bars when shield amount > 0. Provides public properties (`CurrentHeroShieldFill`, `IsHeroCastBarVisible`, etc.) utilized by automated test runners.

### 2.2 HealthBarUI
- **Source File:** [HealthBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HealthBarUI.cs)
- **Hierarchy Location:** Attached to `HeroHPBar` (`HeroUI`) and `MonsterHPBar` (`MonsterUI`).
- **Existing References:** `fillImage` (Image), `hpText` (TextMeshProUGUI), `targetEntity` (Entity), `boundEntityType` (EntityType).
- **Runtime Authority:**
  - Primary: [HealthComponent.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Components/HealthComponent.cs) on `Entity` (`CurrentHealth`, `MaxHealth`, `OnHealthChanged`).
  - Secondary Event Listeners: `EventBus.OnEntitySpawned`, `EventBus.OnEntityDamaged`.
- **Current Behavior:** Updates fill amount (`Mathf.Clamp01(current / max)`) and text readout (`{current:F2} / {max:F2}`). Does not calculate or store independent HP values.

### 2.3 RageBarUI
- **Source File:** [RageBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/RageBarUI.cs)
- **Hierarchy Location:** Attached to `HeroRageBar` under `HeroUI`.
- **Existing References:** `fillImage` (Image), `rageText` (TextMeshProUGUI), `glowImage` (Image), `targetEntity` (Entity).
- **Runtime Authority:**
  - Primary: [RageComponent.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Components/RageComponent.cs) on `Entity` (`CurrentRage`, `MaxRage`, `OnRageChanged`).
  - Secondary Event Listeners: `EventBus.OnEntitySpawned`, `EventBus.OnRageChanged`.
- **Current Behavior:** Updates fill amount and numeric text. Executes fiery color lerp and activates glow when `currentRage >= maxRage - 0.01f`. Owns zero gameplay rage authority.

### 2.4 CastBarUI
- **Source File:** [CastBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/CastBarUI.cs)
- **Hierarchy Location:** Attached to `HeroUI`, driving `HeroCastBar` child panel.
- **Existing References:** `rootObject`, `fillImage`, `castText`, `interruptText`, `targetEntity`, `boundEntityType`.
- **Runtime Authority:**
  - Primary: `Entity.CastState` ([SkillCastState.cs](file:///E:/code/TLTD/Assets/_Game/Combat/SkillCastState.cs)) polled per frame (`CurrentPhase`, `ElapsedTime`, `CastDuration`, `ElapsedChannelTime`, `ChannelDuration`).
  - Event Listeners: `EventBus.OnSkillCastInterrupted`, `OnEntitySpawned`, `OnEntityDied`, `OnBattleStateChanged`.
- **Current Behavior:** Automatically hides when no cast/channel is active (`rootObject.SetActive(false)`). Changes color dynamically (Cyan for Cast, Purple for Channel). Upon CC interrupt, displays red interrupt text for 2.0 seconds. Does not own or trigger cast completion or interruption.

### 2.5 SkillBarUI
- **Source File:** [SkillBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/SkillBarUI.cs)
- **Hierarchy Location:** Attached to `SkillBarUI` under `MainContentArea`.
- **Existing References:**
  - 5 Skill Slots: `Slot1_NormalAttack`, `Slot2_Skill`, `Slot3_External1`, `Slot4_External2`, `Slot5_Ultimate` (each with Button, Background Image, Border Image, CooldownFill Image, Name Text, Cooldown Text, ReadyPulse GameObject).
  - Toggles: `autoToggleBtn`, `autoToggleText`, `speedToggleBtn`, `speedToggleText`.
  - Hero Binding: `boundHero` ([Hero.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Hero.cs)).
- **Runtime Authority:**
  - Equipped Skills: [MindMethodManager.cs](file:///E:/code/TLTD/Assets/_Game/Progression/MindMethodManager.cs) via `GetSelectedSkillForSlot(slot)`.
  - Cooldown: [CooldownManager.cs](file:///E:/code/TLTD/Assets/_Game/Combat/CooldownManager.cs) via `IsOnCooldown(skillId, out remaining)` and `GetCooldownDuration(skillId)`.
  - Action Permission: `Hero.CanUseSkill`, `Hero.CanUseUltimate`.
  - Execution Command: `boundHero.ExecuteSelectedSkill(slot)`.
  - Auto Battle: [BattleManager.cs](file:///E:/code/TLTD/Assets/_Game/Combat/BattleManager.cs) via `SetAutoBattle(bool)` and `EventBus.OnAutoBattleChanged`.
- **Current Behavior:** Data-driven button bar. Renders cooldown radial overlay and text countdown. Pulses Ultimate button when ready.
- **Existing Conflict Noted (Gate 4):** In line 321, `float cost = skill.RageCost > 0f ? skill.RageCost : 100f;` falls back to 100 for Ultimate if asset has 0. Per Gate 4 rules, this is NOT modified in UI-02 to preserve gameplay consistency.
- **Existing Speed State (Gate 9):** Toggle switches visual text between 1X and 2X, but does NOT modify `Time.timeScale`.

### 2.6 DamagePopup & DamagePopupManager
- **Source Files:** [DamagePopup.cs](file:///E:/code/TLTD/Assets/_Game/UI/DamagePopup.cs) & [DamagePopupManager.cs](file:///E:/code/TLTD/Assets/_Game/UI/DamagePopupManager.cs)
- **Hierarchy Location:** Scene manager listening to global events and instantiating world-space floating text.
- **Runtime Authority:**
  - `EventBus.OnEntityDamaged` delivering authoritative `DamageResult` ([DamageCalculator.cs](file:///E:/code/TLTD/Assets/_Game/Combat/DamageCalculator.cs)).
  - `EventBus.OnShieldAbsorbed` delivering authoritative `ShieldAbsorbResult` ([EntityStatusController.cs](file:///E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs)).
- **Current Behavior:** Displays distinct floating callouts: Normal (White), Skill (Cyan), Ultimate (Orange), Crit (`★ BẠO KÍCH! ★` Gold), Dodge (`NÉ ĐÒN` Gray), and Shield Absorb (`HỘ THỂ -{absorbed}` Qi Blue).

### 2.7 Status UI (StatusIconRowUI)
- **Source File:** [StatusIconRowUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/StatusIconRowUI.cs)
- **Hierarchy Location:** Attached to `MonsterStatusRow` in `MonsterUI` and `HeroStatusRow` in `HeroUI`.
- **Runtime Authority:**
  - [EntityStatusController.cs](file:///E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs) via `ActiveCrowdControls`, `ActiveDebuffs`, `ActiveBuffs`.
  - Event Listeners: `EventBus.OnCrowdControlApplied`, `OnCrowdControlExpired`, `OnCrowdControlImmune`, `OnDebuffApplied`, `OnDebuffExpired`.
- **Current Behavior:** Dynamically renders up to 8 compact status badges (Stun, Root, Freeze, Anti-CC, Stat Buffs, Stat Debuffs) with countdown timers. Reusable component for both Hero and Monster.

### 2.8 Companion UI (CompanionHUDUI)
- **Source File:** [CompanionHUDUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/CompanionHUDUI.cs)
- **Hierarchy Location:** Attached to `CompanionHUD` in `MainContentArea` (upper-left of combat viewport).
- **Runtime Authority:**
  - Real companion entity ([Entity.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Entity.cs)) with `EntityType == EntityType.Companion`.
  - [HealthComponent.cs](file:///E:/code/TLTD/Assets/_Game/Entities/Components/HealthComponent.cs) for companion HP.
- **Current Behavior:** Displays companion name, HP bar, and status ("CHIẾN ĐẤU" or "TRỌNG THƯƠNG"). Automatically hides when no companion entity exists in the encounter. Does not invent respawn timers or fake state.

### 2.9 TopHeader
- **Source File:** Constructed in [Prototype01SceneBuilder.cs:532-550](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L532-L550).
- **Hierarchy Location:** Top of `MainContentArea` (`anchoredPosition = (0, -55)`, size `1020x96`).
- **Runtime Authority:** [PlayerProgressionManager.cs](file:///E:/code/TLTD/Assets/_Game/Progression/PlayerProgressionManager.cs) via [DebugProgressionUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/DebugProgressionUI.cs).
- **Current Behavior:** Displays Title ("PROTOTYPE 05"), Level ("HERO LV. 1"), Title Rank ("Novice Disciple"), EXP bar, Gold, and Material badges.

### 2.10 MainGameShell
- **Source File:** [MainGameShell.cs](file:///E:/code/TLTD/Assets/_Game/UI/Core/MainGameShell.cs)
- **Hierarchy Location:** Child of `SafeAreaRoot`, enclosing `MainContentArea` and `GlobalBottomNavigation`.
- **Runtime Authority:** Pure UI shell coordination. Listens to `GlobalBottomNavigation.OnNavigationSelected`.
- **Current Behavior:** Coordinates the portrait screen layout and notifies registered listeners when tabs switch.

### 2.11 MainContentArea
- **Source File:** Constructed in [Prototype01SceneBuilder.cs:496-502](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L496-L502).
- **Hierarchy Location:** RectTransform anchored to full screen with `offsetMin.y = 160f` (reserving bottom 160px for navigation).
- **Current Behavior:** Container for all combat and screen elements (`TopHeader`, `MonsterUI`, `CompanionHUD`, `HeroUI`, `SkillBarUI`, `EquipmentViewContainer`, `DeveloperDebugPanel`).

### 2.12 GlobalBottomNavigation
- **Source File:** [GlobalBottomNavigation.cs](file:///E:/code/TLTD/Assets/_Game/UI/Core/GlobalBottomNavigation.cs)
- **Hierarchy Location:** Bottom 160px of `MainGameShell`.
- **Runtime Authority:** Pure UI navigation authority.
- **Current Behavior:** Exactly 5 slots (Slot 0: Túi Đồ, Slot 1: Công Pháp, Slot 2: Đại Điện/Combat Hub, Slot 3: Luyện Khí, Slot 4: Bang Hội). Center position 2 is emphasized. Switching between Tab 0 and Tab 2 toggles `EquipmentViewContainer` to keep combat screen clean.

---

## 3. Existing Runtime Authorities & Binding Map

| Presentation Element | Primary Authoritative Component | Authority Tier | Authoritative Fields / Events Read | Invariant / Boundary |
|:---|:---|:---:|:---|:---|
| **Hero HP** | `HealthComponent` | Tier C (Runtime) | `CurrentHealth`, `MaxHealth`, `OnHealthChanged` | Read-only. Zero UI calculation. |
| **Monster HP** | `HealthComponent` | Tier C (Runtime) | `CurrentHealth`, `MaxHealth`, `OnHealthChanged` | Read-only. Zero UI calculation. |
| **Shield (Hero & Monster)** | `EntityStatusController` | Tier C (Runtime) | `TotalShieldAmount`, `HasActiveShield`, `ActiveShields`, shield EventBus events | Aggregated display. Hides when shield <= 0. Zero UI shield timers. |
| **Hero Rage** | `RageComponent` | Tier C (Runtime) | `CurrentRage`, `MaxRage`, `OnRageChanged` | Read-only. Visual excitation at max rage. |
| **Cast / Channel** | `Entity.CastState` (`SkillCastState`) | Tier C (Runtime) | `CurrentPhase`, `ElapsedTime`, `CastDuration`, `ElapsedChannelTime`, `ChannelDuration` | Read-only polling. Hides when inactive. No UI timers. |
| **Cast Interrupt** | `EventBus` | Tier C (Runtime) | `OnSkillCastInterrupted` (`SkillCastInterruptSource`) | Presentation callout for 2.0s. No gameplay effect. |
| **Status Effects** | `EntityStatusController` | Tier C (Runtime) | `ActiveCrowdControls`, `ActiveDebuffs`, `ActiveBuffs`, CC events | Data-driven badges. No status logic in UI. |
| **Skill Slot Blueprint** | `MindMethodManager` / `SkillDefinitionSO` | Tier A/B (Data) | `GetSelectedSkillForSlot(slot)`, `SkillId`, `SkillName`, `RageCost`, `Cooldown`, `Priority` | Read-only slot mapping. |
| **Cooldown Duration & State** | `CooldownManager` | Tier C (Runtime) | `IsOnCooldown(skillId, out remaining)`, `GetCooldownDuration(skillId)` | Read-only radial sweep and countdown. |
| **Skill Execution** | `Hero` -> `SkillExecutor` | Tier D (Execution) | `boundHero.ExecuteSelectedSkill(slot)` | UI delegates execution directly to Hero. |
| **Auto Battle State** | `BattleManager` | Tier C (Runtime) | `IsAutoBattle`, `SetAutoBattle(bool)`, `OnAutoBattleChanged` | Toggles existing manager. Decision is `HeroSkillDecisionController`. |
| **Damage Popups** | `EventBus` | Tier C (Runtime) | `OnEntityDamaged` (`DamageResult`), `OnShieldAbsorbed` (`ShieldAbsorbResult`) | World-space visual feedback. |
| **Companion State** | `Entity` / `HealthComponent` | Tier C (Runtime) | `companion.Health.CurrentHealth`, `MaxHealth`, `IsAlive` | Auto-hides when absent. No fake timers. |

---

## 4. File Boundary & Modification Rules

### 4.1 Files Expected to Change / Polish (Presentation Layer ONLY)
- [Prototype01SceneBuilder.cs](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs): Layout hierarchy adjustments, spacing, anchors, panel dimensions, visual framing, camera framing in portrait 9:16.
- [BattleHUD.cs](file:///E:/code/TLTD/Assets/_Game/UI/BattleHUD.cs): Ensure smooth integration of polish, references, and overlay transitions.
- [HealthBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HealthBarUI.cs): Formatting / visual fill smoothing.
- [RageBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/RageBarUI.cs): Excitation animation and threshold glow.
- [CastBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/CastBarUI.cs): Framing, progress sweep, interrupt banner layout.
- [SkillBarUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/SkillBarUI.cs): Action cluster framing, radial fill presentation, auto/speed toggle layout.
- [StatusIconRowUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/StatusIconRowUI.cs): Visual badge icons and framing.
- [CompanionHUDUI.cs](file:///E:/code/TLTD/Assets/_Game/UI/HUD/CompanionHUDUI.cs): Compact card framing.
- [DamagePopup.cs](file:///E:/code/TLTD/Assets/_Game/UI/DamagePopup.cs) & [DamagePopupManager.cs](file:///E:/code/TLTD/Assets/_Game/UI/DamagePopupManager.cs): Typography, movement curves, sorting orders.
- [UIStyleConfig.cs](file:///E:/code/TLTD/Assets/_Game/UI/Core/UIStyleConfig.cs): Wuxia color palettes, gold/bronze borders, ink backgrounds.

### 4.2 Files That MUST NOT Change (Locked Gameplay Authority)
The following core gameplay files **MUST NOT BE MODIFIED**:
- `Assets/_Game/Combat/SkillExecutor.cs` (Locked Execution Authority)
- `Assets/_Game/Combat/SkillExecutionValidator.cs` (Locked Validation Authority)
- `Assets/_Game/Combat/SkillCastState.cs` (Locked Cast/Channel Runtime Authority)
- `Assets/_Game/Combat/CooldownManager.cs` (Locked Cooldown Authority)
- `Assets/_Game/Entities/Components/RageComponent.cs` (Locked Rage Runtime Authority)
- `Assets/_Game/Combat/DamageCalculator.cs` (Locked Damage Algorithm)
- `Assets/_Game/Entities/Components/HealthComponent.cs` (Locked Health Authority)
- `Assets/_Game/Combat/EntityStatusController.cs` (Locked CC & Shield Authority)
- `Assets/_Game/Combat/HeroSkillDecisionController.cs` (Locked Auto Decision Authority)
- `Assets/_Game/Combat/BattleManager.cs` (Locked Battle State Authority)
- `Assets/_Game/Progression/MindMethodManager.cs` (Locked Mind Method Authority)

---

## 5. Deficiencies Identified & Missing Presentation Pieces

1. **Combat Viewport Framing:**
   - In portrait 9:16 (`1080x1920`), the camera orthographic size must comfortably capture Hero at `X = -4.0` and Monster at `X = +4.0` in the central Combat Zone (Y: 700 to 1500) without characters being clipped by `MonsterUI` or `HeroUI`.
2. **Boss / Monster Presentation:**
   - The monster card currently displays a flat label. Boss encounters require a clear framing distinction with segmented HP or elite badge while maintaining single-source HP data binding.
3. **Qi Shield Visual Integration:**
   - The shield bar is positioned underneath HP. It should feel like an integrated Qi barrier (layered cyan/blue sheen overlay) over the combatant's health rather than a detached prototype bar.
4. **Hero Combat Card Hierarchy:**
   - The Hero card stacks HP, Shield, Rage, and Cast bars vertically. Spacing, typography, and visual grouping must be optimized for readability at portrait mobile scale.
5. **Skill Action Cluster:**
   - The 5 skill slots must maintain clear distinction between Basic Attack (Slot 1), Active Skills (Slots 2-4), and Ultimate (Slot 5), with responsive readiness indicators and cooldown sweeps.
6. **Debug Separation:**
   - The 12 developer buttons must remain strictly isolated inside the collapsible drawer so normal gameplay presentation is clean and immersive.

---

## 6. Risks, Edge Cases & Authority Gaps

### 6.1 SPEED AUTHORITY GAP (Gate 9)
- **Finding:** No gameplay `TimeScaleController` or battle speed authority currently exists in the TLTD gameplay runtime.
- **Rule:** UI must **NOT** write `Time.timeScale = 2f` or own gameplay simulation speed.
- **Action:** Speed button in `SkillBarUI` only toggles a visual 1X/2X display state. It delegates nowhere until a gameplay-layer speed controller is formally introduced into the engine roadmap.

### 6.2 COMPANION PRESENTATION API GAP (Gate 10)
- **Finding:** The runtime exposes `EntityType.Companion`, `HealthComponent`, and `IsAlive`. However, there is **no companion respawn timer or respawn lifecycle API** exposed in the engine.
- **Rule:** UI must **NOT** invent a fake `CompanionRespawnTimer` or independent countdown.
- **Action:** When dead, `CompanionHUDUI` displays `TRỌNG THƯƠNG` without inventing fake timers. When no companion exists, it hides automatically.

### 6.3 EXISTING RAGE COST FALLBACK (Gate 4)
- **Finding:** In [SkillBarUI.cs:321](file:///E:/code/TLTD/Assets/_Game/UI/HUD/SkillBarUI.cs#L321), `float cost = skill.RageCost > 0f ? skill.RageCost : 100f;` falls back to 100 for an Ultimate if the asset has 0.
- **Rule:** Per Gate 4: "Nếu code hiện tại có fallback này: KHÔNG tự sửa trong UI-02. Ghi nhận vào report là existing authority conflict. Không thay đổi gameplay trong UI-02."
- **Action:** Preserve existing code behavior in UI-02 and document the conflict.

---

## 7. Proposed UI-Only Implementation Scope

The implementation will execute cleanly across the following sequential phases:

1. **Phase 1 — Scene Layout & Viewport Composition:**
   - Verify `SafeAreaRoot` -> `MainGameShell` -> `MainContentArea` -> `GlobalBottomNavigation`.
   - Ensure central Combat Zone (Y: 700 to 1540) has unimpeded visual depth.
   - Verify camera setup (orthographic size 8.5, positioned at `(0, 0.5, -10)`) frames combatants.
2. **Phase 2 — Monster / Boss Card Polish:**
   - Refine `MonsterUI` wuxia framing, crimson/gold header, segmented HP bar, and status badge alignment.
3. **Phase 3 — Hero Combat Card Polish:**
   - Refine `HeroUI` wuxia framing, jade HP bar, Qi shield overlay, energetic Rage bar with ultimate-ready pulse, and `CastBarUI`.
4. **Phase 4 — Skill Action Cluster & Controls:**
   - Polish `SkillBarUI` 5-slot radial cooldown overlays, ready pulse for Ultimate, and Auto/Speed toggles.
5. **Phase 5 — Status, Companion & Feedback Polish:**
   - Refine `StatusIconRowUI` badge spacing and remaining-time format.
   - Ensure `CompanionHUDUI` auto-binding and graceful hide.
   - Verify `DamagePopupManager` sorting orders and font sizes.
6. **Phase 6 — Validation & Regression Execution:**
   - Run Unity batchmode compile check.
   - Run P07.8 full regression suite (`RunAllPrototype07_8Tests`).
   - Run P07.9 Phase 5.3 regression suite (`RunAllPrototype07_9_Phase5_3_Tests`).
   - Run P07.9 Risk 04 regression suite (`RunAllPrototype07_9_Risk04_Tests`).
   - Run P07.9.1 autonomous combat regression suite (`RunAllPrototype07_9_1_Tests`).
   - Verify V01-V15 visual acceptance in Unity Play Mode.

---

## 8. Gate 0 Audit Sign-off

- [x] All 12 target UI components audited against actual source code.
- [x] Runtime authorities strictly identified and mapped.
- [x] Zero-gameplay-change boundary established.
- [x] Authority gaps (Speed, Companion Respawn, RageCost fallback) formally recorded.
- [x] Compile check verified (Exit Code 0, 0 errors).
- [x] Audit document saved to `PROJECT_MEMORY/UI-02_PRE_IMPLEMENTATION_AUDIT.md`.

**GATE 0 COMPLETE — READY FOR IMPLEMENTATION PHASE.**
