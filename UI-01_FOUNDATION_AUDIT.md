# UI-01 FOUNDATION AUDIT

**Project:** `E:\code\TLTD`  
**Milestone:** UI-01 Foundation Audit + Portrait UI Shell  
**Scope:** UI / Presentation Layer Only  
**Gameplay Authority:** Locked P01 → P07.9 (ZERO gameplay changes permitted)  
**Date:** 2026-09-14  

---

## 1. Current UI Hierarchy

The current UI hierarchy in `Assets/_Game/Scenes/Prototype01.unity` (generated via `Prototype01SceneBuilder.cs`) is structured as a flat, landscape developer prototype:

```text
UI Canvas (Canvas, CanvasScaler [1920x1080 Landscape], GraphicRaycaster, UIRaycastDebugger, BattleHUD, DebugProgressionUI, EquipmentDropDebugUI, LootDecisionUI, LootTierProgressionUI, TitleBreakthroughUI, MindMethodUI)
├── TopHeader (RectTransform, TitleText, LevelText, RankTitleText, ExpBar)
├── LevelUpNotification (TextMeshProUGUI)
├── HeroUI (HeroLabel, HeroHPBar [HealthBarUI], HeroShieldBar, HeroRageBar [RageBarUI], HeroCastBar [CastBarUI, InterruptText])
├── MonsterUI (MonsterLabel, MonsterHPBar [HealthBarUI], MonsterShieldBar [HealthBarUI])
├── EquipmentStatsPanel (Bg, HeroStats, 8x Equipment Slot Text labels)
├── EquipmentDropPanel (Bg, Header, ItemName, ItemType, ItemLevel, Rarity, Status, Affixes, InvCount)
├── ComparisonPanel (Bg, ComparisonText)
├── UpgradePanel (Bg, UpgradePreviewText)
├── DeveloperDebugPanel (Bg, DebugHeader, 12 action buttons, PlayerResText)
├── ToggleDebugBtn (Button)
├── VictoryPanel (Bg, VictoryText)
├── DefeatPanel (Bg, DefeatText, StartButton)
├── LootDecisionPanel (Header, ComparisonContent [CurrentEquipmentColumn, NewEquipmentColumn], PowerChangeSection, StatChangesSection, Buttons)
├── LootTierToggleButton (Button)
├── LootTierProgressionPanel (Header, CloseButton, TiersHeader, RarityComparisonTable, UpgradeInfoText, TimerText, ProgressBar, UpgradeButton)
├── TitleBreakthroughToggleBtn (Button)
├── TitleBreakthroughPanel (Bg, CloseButton, TitleText, CurrentTitleText, NextTitleText, CurrentLevelText, LevelCapText, RequirementsText, RewardsText, StatComparisonText, BreakthroughActionButton)
├── MindMethodToggleBtn (Button)
└── MindMethodPanel (Bg, CloseButton, TitleText, ActiveTitleText, ActiveLevelText, ActiveDescText, PassiveEffectsText, RageModifiersText, MindMethodSummaryText, 5x Slot Texts, Alternatives)
```

---

## 2. Existing UI Components

Located in `Assets/_Game/UI/`:
- **`BattleHUD`**: Central combat UI coordinator. Manages Victory/Defeat panels, Start/Restart buttons, hero & monster shield UI references, hero cast bar UI references, and listens to EventBus events (`OnBattleStateChanged`, `OnShieldApplied`, `OnShieldAbsorbed`, etc.).
- **`HealthBarUI`**: Dual-mode health bar controller for Hero and Monster. Updates fill amount and HP text via `HealthComponent` events.
- **`RageBarUI`**: Hero rage bar controller. Updates fill amount and Rage text via `RageComponent` events.
- **`CastBarUI`**: P07.9 cast/channel progression bar. Binds to `SkillCastState`, updates progress fill, shows cast name, and presents interrupt feedback (`STUN`/`FREEZE`).
- **`DamagePopup`**: Floating combat text for damage numbers, crits, and dodges.
- **`DamagePopupManager`**: Spawns and pools `DamagePopup` instances in world/screen space.
- **`DebugProgressionUI`**: Handles player level, EXP bar, Title rank, level-up notifications, and EXP manipulation buttons.
- **`EquipmentDropDebugUI`**: Manages selected equipment inspector, manual drop simulation, equipment slot list, unequip/equip buttons, and upgrade buttons.
- **`LootDecisionUI`**: Two-column equipment comparison modal (Current Equipped vs New Drop) with combat power delta, stat comparison, Equip, and Dismantle buttons.
- **`LootTierProgressionUI`**: Cấp Rơi (Loot Tier) progression modal showing current/next tier, rarity probability table, upgrade timer countdown, and upgrade trigger button.
- **`TitleBreakthroughUI`**: Đột Phá Danh Hiệu modal displaying current title, next title, level cap expansion preview, breakthrough requirements, and action button.
- **`MindMethodUI`**: Tâm Pháp / Công Pháp panel displaying active mind method, passives, rage modifiers, 5 skill slots, and alternative skills.
- **`StatusDebugUI`**: Developer overlay rendering active CC, buffs, and debuffs.
- **`UIRaycastDebugger`**: Diagnostic script for tracking UI raycast blocking.

---

## 3. Existing Reusable Components

- **`HealthBarUI`**: Highly reusable fill + text status bar component.
- **`RageBarUI`**: Highly reusable resource bar component.
- **`CastBarUI`**: Clean, decoupled presentation component bound strictly to `SkillCastState`.
- **`DamagePopupManager` / `DamagePopup`**: Fully functional floating number system.
- **`BattleHUD` Presentation Facades**: Query properties (`IsHeroCastBarVisible`, `CurrentHeroCastProgress`, `IsHeroShieldVisible`, `CurrentHeroShieldFill`, etc.) provide clean abstractions for test runners without leaking internals.
- **UI Builder Helper Methods**: In `Prototype01SceneBuilder.cs`: `CreateUIRect`, `CreateUIImage`, `CreateUIText`, `CreateUIButton`, `CreateUIBar`.

---

## 4. Debug UI Components

The following components represent developer/debug controls and must NOT dictate player-facing layout:
- **`DeveloperDebugPanel`**:
  - Buttons: `+10 EXP`, `+100 EXP`, `+1K EXP`, `BREAKTHROUGH`, `DROP`, `EQUIP`, `UNEQUIP`, `UPGRADE`, `+1K G`, `+5 MAT`, `SIM 1000`, `RESET PROG`.
  - Text: `PlayerResText`.
- **`ToggleDebugBtn`**: Floating button toggling `DeveloperDebugPanel`.
- **`EquipmentStatsPanel`**: Raw text display listing 8 equipment slots and aggregate stats.
- **`EquipmentDropPanel`**: Debug text readout of last selected inventory item.
- **`ComparisonPanel` & `UpgradePanel`**: Debug text previews.
- **`StatusDebugUI`**: Floating debug text listing status effects.
- **`UIRaycastDebugger`**: Raycast diagnostic script.

*Rule:* All debug controls must be preserved for testing purposes, but grouped into a collapsible developer overlay so they do not obstruct player-facing UI.

---

## 5. Player-Facing UI Components

The following components belong to the real player-facing game presentation:
- **Combat HUD**:
  - `HeroUI`: Hero label, Health bar (`HealthBarUI`), Shield bar, Rage bar (`RageBarUI`), Cast bar (`CastBarUI`).
  - `MonsterUI`: Monster label, Health bar (`HealthBarUI`), Shield bar.
  - Floating Damage Popups (`DamagePopupManager`).
- **Combat State Overlays**:
  - `VictoryPanel` (Victory announcement).
  - `DefeatPanel` (Defeat announcement + `StartButton` / "BẮT ĐẦU").
- **Core Progression Header**:
  - `TopHeader`: Hero Level, Current Title/Rank, EXP bar.
- **System Screen Modals (to be hosted in `MainContentArea`)**:
  - `MindMethodPanel` (`MindMethodUI`): Tâm Pháp / Công Pháp screen.
  - `TitleBreakthroughPanel` (`TitleBreakthroughUI`): Đột Phá Danh Hiệu modal.
  - `LootTierProgressionPanel` (`LootTierProgressionUI`): Cấp Rơi progression modal.
  - `LootDecisionPanel` (`LootDecisionUI`): Equipment comparison modal.

---

## 6. Existing Bindings

1. **EventBus Subscriptions**:
   - `BattleHUD` subscribes to: `OnBattleStateChanged`, `OnShieldApplied`, `OnShieldAbsorbed`, `OnShieldDepleted`, `OnShieldExpired`, `OnShieldRemoved`, `OnEntitySpawned`, `OnEntityDied`.
   - `HealthBarUI` subscribes to entity `Health.OnHealthChanged`, `Health.OnDied`, `Health.OnRevived`.
   - `RageBarUI` subscribes to entity `Rage.OnRageChanged`.
   - `CastBarUI` subscribes to entity `CastState.OnCastStarted`, `OnCastProgress`, `OnCastCompleted`, `OnCastInterrupted`, `OnCastReset`.
   - `LootDecisionUI` subscribes to `DropManager.OnEquipmentDropped`.
2. **Direct Manager References**:
   - `DebugProgressionUI` references `ProgressionManager`.
   - `EquipmentDropDebugUI` references `InventoryManager`, `EquipmentManager`, `ProgressionManager`.
   - `LootTierProgressionUI` references `DropLevelManager`.
   - `TitleBreakthroughUI` references `TitleBreakthroughManager`.
   - `MindMethodUI` references `MindMethodManager`.

*Rule:* All existing bindings must remain functional and unmodified.

---

## 7. Current Canvas Setup

- **GameObject:** `"UI Canvas"` in `Prototype01.unity`.
- **Canvas Component:** `renderMode = RenderMode.ScreenSpaceOverlay`.
- **CanvasScaler Component:**
  - `uiScaleMode = ScaleWithScreenSize`.
  - `referenceResolution = Vector2(1920, 1080)` (Landscape 16:9).
  - `screenMatchMode = MatchWidthOrHeight`.
  - `matchWidthOrHeight = 0` (Match width).
- **Raycasting:** `GraphicRaycaster` + `UIRaycastDebugger`.
- **EventSystem:** `EventSystem` + `InputSystemUIInputModule`.

---

## 8. Current Resolution Assumptions

- **Problem:** The entire current UI was constructed with a fixed landscape coordinate system (1920 wide by 1080 high).
- Panels are placed with hardcoded horizontal offsets:
  - `HeroUI` at `Vector2(220, -115)` (left side).
  - `MonsterUI` at `Vector2(-220, -115)` (right side).
  - `DeveloperDebugPanel` at `Vector2(560, 75)` with width `1120`.
- On a mobile portrait screen (e.g. 1080x1920 or 9:19.5), this layout completely breaks: horizontal elements overflow, center elements overlap, and vertical space is wasted.

---

## 9. Safe-Area Status

- **Status:** **NOT IMPLEMENTED / NON-EXISTENT**.
- There is currently no `SafeAreaRoot` in `UI Canvas`.
- UI panels anchor directly to canvas edges, which will cause clipping behind notches, punch holes, and system home bars on mobile devices.
- **Required Solution:** Introduce a dedicated `SafeArea` component and `SafeAreaRoot` RectTransform directly beneath Canvas.

---

## 10. Current Navigation Status

- **Status:** **NON-EXISTENT**.
- There is currently no `GlobalBottomNavigation` bar in `Prototype01.unity`.
- Modal open/close buttons are scattered across the screen:
  - `LootTierToggleButton` at `Vector2(400, 205)`
  - `TitleBreakthroughToggleBtn` at `Vector2(330, 480)`
  - `MindMethodToggleBtn` at `Vector2(330, 430)`
  - `ToggleDebugBtn` at `Vector2(90, 20)`
- **Required Solution:** Implement the locked `GlobalBottomNavigation` structure with exactly 5 positions: `[Position 1]`, `[Position 2]`, `[Position 3 = Main Hub]`, `[Position 4]`, `[Position 5]`, with Position 3 visually prominent.

---

## 11. Components That Should Be Reused

1. **`BattleHUD`**: Preserved completely. All serialized fields, events, and public facades must remain intact.
2. **`HealthBarUI`**, **`RageBarUI`**, **`CastBarUI`**: Reused directly for portrait status bars.
3. **`DamagePopupManager`**: Reused directly for combat feedback.
4. **Modal UI Scripts** (`LootDecisionUI`, `LootTierProgressionUI`, `TitleBreakthroughUI`, `MindMethodUI`): Reused directly, docked inside `MainContentArea`.
5. **Debug UI Scripts** (`DebugProgressionUI`, `EquipmentDropDebugUI`): Reused inside a collapsible debug container.

---

## 12. Components That Need Refactor / Relayout

1. **`CanvasScaler`**:
   - Change `referenceResolution` from `Vector2(1920, 1080)` to **`Vector2(1080, 1920)`** (Portrait 9:16).
   - Set `matchWidthOrHeight = 0f` (Width-priority for portrait devices).
2. **Hierarchy Restructuring**:
   - Wrap all UI inside `SafeAreaRoot` -> `MainGameShell`.
   - Divide `MainGameShell` into `MainContentArea` (upper/middle vertical region) and `GlobalBottomNavigation` (bottom bar).
3. **HUD Layout in Portrait**:
   - `TopHeader`: Positioned at top of `SafeAreaRoot`.
   - `MonsterUI`: Positioned below header (representing the opponent in combat).
   - `HeroUI`: Positioned in middle/lower section above actions.
   - `DeveloperDebugPanel`: Placed in a collapsible drawer anchored to bottom-left with a clean toggle button.
4. **`GlobalBottomNavigation`**:
   - 5 dedicated slots anchored to bottom of `SafeAreaRoot`.
   - Position 3 (Center) styled with larger frame / prominence as Main Hub.

---

## 13. Components That Should NOT Be Touched

- **All Gameplay Logic:**
  - `SkillExecutor`, `SkillCastState`, `SkillExecutionValidator`
  - `EntityStatusController`, `DamageCalculator`
  - `HealthComponent`, `RageComponent`, `CooldownManager`, `MovementComponent`
  - `BattleManager`, `MonsterManager`, `ProgressionManager`, `EquipmentManager`, `DropManager`, `MindMethodManager`, `TitleBreakthroughManager`
- **Locked Project Memory:**
  - `AI_RULES.md`, `CURRENT_DESIGN_AUTHORITY.md`, `D1-D23`, `P07_8_LOCKED.md`, `P07_9_LOCKED.md`
- **P07.8 & P07.9 Test Runners:**
  - All automated assertion logic in `Prototype01PlayTestRunner_*.cs`.

---

## 14. Potential Risks & Mitigation

| Risk | Impact | Mitigation Strategy |
|---|---|---|
| **Test Runner Broken References** | High | Test runners find components via `FindAnyObjectByType<T>()` and query `BattleHUD` facades (`IsHeroCastBarVisible`, `CurrentHeroShieldFill`, etc.). Keep `BattleHUD` on the root Canvas and wire all references identical to before. |
| **Resolution Mismatch** | Medium | Changing CanvasScaler to 1080x1920 might cause UI elements with old fixed offsets to render off-screen. Re-anchor all elements with portrait-friendly relative anchors in `Prototype01SceneBuilder.cs`. |
| **Safe Area In Editor** | Low | In Unity Editor Game view, `Screen.safeArea` returns full resolution. Ensure `SafeArea` script gracefully handles default rect when notches are absent. |
| **Debug Functionality Loss** | High | Regression and manual tests require EXP/Breakthrough/Drop debug buttons. Preserve all 12 buttons and their serialized bindings inside a collapsible developer panel. |

---

## 15. Proposed Minimal Implementation Plan

### Step 1: Create Core Portrait Shell Components
1. **`Assets/_Game/UI/Core/SafeArea.cs`**:
   - Applies `Screen.safeArea` to its RectTransform anchors dynamically.
2. **`Assets/_Game/UI/Core/GlobalBottomNavigation.cs`**:
   - Implements the 5-slot navigation shell: `[1] [2] [MAIN HUB] [4] [5]`.
   - Visual emphasis on Position 3 (Main Hub).
   - Exposes public navigation events/callbacks without hardcoding gameplay actions.
3. **`Assets/_Game/UI/Core/MainGameShell.cs`**:
   - Manages `MainContentArea` and coordinates top-level view switching.

### Step 2: Establish Reusable Presentation Conventions
1. **`Assets/_Game/UI/Core/UIStyleConfig.cs`**:
   - Reusable color tokens, panel backgrounds, framed panel borders, button styling, and bar colors matching mobile wuxia/anime RPG palette (dark ink, jade, gold, bronze).

### Step 3: Update `Prototype01SceneBuilder.cs`
1. Configure `CanvasScaler` to `1080x1920` (Portrait, `matchWidthOrHeight = 0f`).
2. Construct hierarchy:
   ```text
   UI Canvas (Canvas, CanvasScaler [1080x1920], GraphicRaycaster, BattleHUD, managers)
   └── SafeAreaRoot (SafeArea)
       └── MainGameShell (MainGameShell)
           ├── MainContentArea (RectTransform)
           │   ├── TopHeader (Level, Title, EXP bar)
           │   ├── MonsterUI (Monster HP, Shield bar)
           │   ├── HeroUI (Hero HP, Shield, Rage, CastBarUI)
           │   ├── SystemScreenModals (MindMethod, TitleBreakthrough, LootTier, LootDecision)
           │   └── Overlays (VictoryPanel, DefeatPanel, StartButton)
           ├── GlobalBottomNavigation (5 positions, Main Hub center)
           └── DebugPanelContainer (Collapsible DeveloperDebugPanel + ToggleButton)
   ```
3. Wire all serialized references for `BattleHUD`, `DebugProgressionUI`, `EquipmentDropDebugUI`, `LootDecisionUI`, `LootTierProgressionUI`, `TitleBreakthroughUI`, and `MindMethodUI`.

### Step 4: Verification
1. Unity batch mode compile check (0 errors).
2. Open `Prototype01.unity` in Editor.
3. Verify Play Mode:
   - Portrait layout renders correctly.
   - HP, Rage, Shield, and Cast Bar update during combat.
   - Start / Restart buttons function.
   - Debug buttons remain accessible via toggle.
4. Run targeted P07.8 regression (`RunAllPrototype07_8Tests`: 55/55).
5. Run targeted P07.9 regression suites (Phases 2..5.3, RISK-04).
