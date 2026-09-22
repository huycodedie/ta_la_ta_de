# UI REDESIGN SPECIFICATION — TLTD

## Status
`PROPOSED / IMPLEMENTATION SPECIFICATION`

This document is the visual/presentation implementation specification derived from the supplied reference-game screenshots, the current TLTD prototype screenshot, and the already LOCKED `UI_DESIGN_AUTHORITY.md`.

It is **not a new gameplay authority** and does not override D1-D23 or P01+ locked behavior. It is the specification for the UI refactor before P08.

---

## 1. Source references

The supplied reference set contains screenshots covering:
- Main game/home screen with five bottom navigation positions.
- Normal-monster combat presentation.
- Boss combat presentation.
- Stage/Floor screen.
- Thành Lăng Tiêu city/home screen.
- Tâm Pháp / Công Pháp screen.
- Xông Pha / dungeon-like screen.
- Current TLTD prototype UI.

The reference game is used for **visual hierarchy, composition, information density, framing, icon treatment, and mobile portrait presentation**. It is not a source of TLTD gameplay rules.

---

## 2. Non-negotiable structural baseline

The existing `UI_DESIGN_AUTHORITY.md` remains authoritative:

- 2D mobile portrait / vertical-first.
- One global game shell.
- One Main Content Area.
- Five global bottom-navigation positions.
- Center position is Main Hub / Main Game Frame.
- System screens replace the Main Content Area rather than creating a second global navigation framework.
- Công Pháp is a dedicated system screen/module.
- UI is presentation only and must not become a second authority for combat, stats, progression, or Công Pháp effects.

---

## 3. Overall visual direction

### Target
Move TLTD from the current developer/debug prototype appearance toward a **mobile wuxia/anime RPG presentation** inspired by the supplied reference screenshots while keeping TLTD's own identity and data.

### Principles
- Portrait-first composition.
- Artwork/background carries most of the visual identity.
- UI panels should feel integrated with the scene instead of large plain developer rectangles.
- Use compact framed panels, stylized buttons, readable labels, icon-led controls, and layered foreground/background elements.
- Preserve strong information hierarchy: player state → combat state → actions → navigation.
- Avoid excessive text and debug information during normal gameplay.
- Developer/debug controls must not occupy the primary player-facing HUD.
- Do not copy copyrighted artwork, characters, logos, or exact visual assets from the reference game.
- Reference the **design language**, not the source game's assets.

---

## 4. Global Main Shell

Conceptual structure:

```text
GameRoot / MainGameShell
├── MainContentArea
│   └── CurrentSystemScreen
└── GlobalBottomNavigation
    ├── Position 1
    ├── Position 2
    ├── Position 3 = Main Hub
    ├── Position 4
    └── Position 5
```

### Visual requirements
- Bottom navigation should be visually separated from the content area using a decorative/top border or layered frame.
- Five positions must remain visually distinct.
- Center Main Hub should have stronger visual emphasis without changing the five-position structure.
- Active navigation state must be immediately obvious.
- Notification/badge indicators may be shown where the underlying system already has meaningful state; do not invent gameplay notifications merely for appearance.
- Safe-area aware on mobile portrait.

### Not locked by this specification
- Final five labels.
- Final icons.
- Exact pixel dimensions.
- Final artwork.
- Final fonts/colors.

These may be refined during visual implementation without changing the structural baseline.

---

## 5. Main Hub / Main Game Frame

The Main Hub is the primary player-facing screen.

### Layout direction

```text
┌─────────────────────────┐
│ Player / resources      │  Header
├─────────────────────────┤
│                         │
│     World / Hero        │
│     Main presentation   │
│                         │
│   contextual objects    │
│                         │
├─────────────────────────┤
│ contextual HUD/actions  │
├─────────────────────────┤
│ Global Navigation 5     │
└─────────────────────────┘
```

### Reference-derived goals
- Use a full-height illustrated scene/background rather than the current dark developer canvas.
- Player/hero presentation should be central and visually dominant.
- Resources and progression should sit in compact top-level HUD elements.
- Contextual actions should be represented by compact icon/button elements around the scene when appropriate.
- Avoid covering the character/world with oversized opaque panels.

### Gameplay safety
The Main Hub UI must only display already-authoritative values. It must not calculate progression, combat, rewards, stats, or loot independently.

---

## 6. Normal Battle UI

The battle screen must remain compatible with P01-P07.9 behavior.

### Recommended presentation hierarchy

```text
Top
├── Hero status
├── Level / progression information
├── Optional system status
└── Monster/Boss status

Center
├── Battle background
├── Hero
├── Companions
├── Monster/Boss
├── Projectiles/VFX
└── Damage / combat feedback

Lower combat HUD
├── Rage / resource display
├── Cast / Channel bar when active
├── Skill controls
├── Auto / Speed controls
└── Contextual battle information

Bottom
└── Global Navigation where the current screen permits it
```

### P07.8 compatibility
- Shield presentation must remain based on authoritative `EntityStatusController` state.
- Do not introduce a UI-owned shield value.
- Zero active shield should not leave stale shield UI visible.

### P07.9 compatibility
- CastBarUI remains presentation only.
- Cast/Channel progress comes from existing Entity presentation facades.
- Interrupted state should show clear visual feedback without creating a new gameplay interruption system.
- Casting/channeling movement lock remains gameplay authority, not a UI behavior.

---

## 7. Hero / Monster Status Bars

Replace the current plain developer-style horizontal rectangles with compact game HUD elements.

### Hero
Display only relevant player combat state, such as:
- Hero identity/name where appropriate.
- HP.
- Rage.
- Level where appropriate.
- Existing status indicators.

### Monster/Boss
Display:
- Name/identity.
- HP.
- Existing status indicators.
- Boss-specific presentation only where already supported by gameplay data.

### Visual rules
- HP and Rage must remain immediately distinguishable.
- Numbers should remain readable at portrait/mobile scale.
- Status icons should be compact and data-driven.
- Do not add fake stats or decorative values that look authoritative.

---

## 8. Cast / Channel UI

The P07.9 CastBarUI should be visually integrated into the new HUD instead of appearing as a developer/debug rectangle.

### Requirements
- Visible only when useful/active.
- Distinguish Cast vs Channel.
- Clear progress indication.
- Clear interrupted state.
- Compact enough not to obscure combat.
- No gameplay logic inside the presentation component.
- No duplicate timer or cast authority.

---

## 9. Skill / Action Area

The skill area should visually follow the reference game's compact action-button approach while preserving TLTD's existing skill model.

### Requirements
- Basic attack/skill/ultimate controls use clear icon-first presentation.
- Skill availability/cooldown remains sourced from the existing combat systems.
- Ultimate/Rage state remains sourced from `RageComponent`/existing authority.
- Auto/manual state remains sourced from existing gameplay state.
- Do not hard-code a new skill count in UI architecture.
- Do not make UI buttons directly manipulate combat state outside existing commands/controllers.

### Locked gameplay behavior preserved
- Existing skill priority.
- Existing Rage rules.
- Existing cooldown rules.
- P07.9 Cast/Channel lifecycle.
- Existing CC interruption semantics.

---

## 10. Companion Presentation

The reference style favors clear character/icon presentation without turning companions into separate UI systems.

### Requirements
- Companions remain visually readable in the battle scene.
- If companion status is shown in HUD, use compact portraits/status indicators.
- Companion HP/death/respawn must be sourced from existing Companion/Entity authority.
- Do not create a second companion combat state in UI.

---

## 11. Stage / Floor Screen

The reference shows a dedicated stage/floor presentation.

### Direction
- Illustrated or themed background.
- Clear stage/floor title.
- Central/vertical progression path where appropriate.
- Current progress highlighted.
- Boss Gate state should be visually distinct when the underlying battle state is at 100%.

### Safety
Do not invent additional stage progression rules. Use existing Stage/BattleManager data.

---

## 12. Thành Lăng Tiêu / World-City Screen

The supplied reference uses a large illustrated city scene with interactive locations placed over the environment.

### Direction for TLTD
- Use a full-screen illustrated wuxia environment.
- Place system entry points as visually integrated location buttons.
- Use compact labels attached to locations.
- Avoid a conventional grid of large rectangular developer buttons.
- Preserve the global five-position navigation shell.

The exact city locations remain data/design decisions unless separately approved. This specification does not invent new gameplay systems.

---

## 13. Xông Pha / Dungeon-like Screen

The supplied reference shows a dedicated progression/combat-entry presentation.

### Direction
- Dedicated system screen inside Main Content Area.
- Strong title/header.
- Themed background.
- Clear current challenge/progression.
- Primary action visually prominent.
- Rewards/cost/status shown only when backed by existing data.

No new Xông Pha gameplay mechanics are introduced by this UI specification.

---

## 14. Công Pháp / Tâm Pháp Screen

This screen must follow the already LOCKED dedicated-system structure.

### Structure

```text
Công Pháp Screen
├── Header / Back / Title
├── Character / overview presentation
├── Total Công Pháp Buff summary
├── Category selector
├── List/Grid
└── Detail panel
    ├── Icon / identity
    ├── Current level/state
    ├── Current effects
    ├── Next-level information
    └── Upgrade/equip action when applicable
```

### Visual direction
- Use a dedicated wuxia-themed panel system.
- Category tabs/buttons should be compact and visually distinct.
- Item/skill/cultivation cards should support future data-driven expansion.
- Detail panel should emphasize the selected Công Pháp.
- Do not hard-code category count.

---

## 15. Equipment / Loot Presentation

The current TLTD prototype contains developer-style equipment panels. These should be replaced by a mobile RPG inventory/equipment presentation.

### Locked equipment count
12 player-facing equipment concepts:
1. Vũ khí/Kiếm
2. Mũ
3. Khăn che mặt
4. Áo
5. Quần
6. Giày
7. Găng tay
8. Đai lưng
9. Áo choàng
10. Dây chuyền
11. Nhẫn
12. Bùa/Ngọc

### Visual direction
- Character-centered equipment layout where appropriate.
- Equipment slots use icons/artwork rather than text-only rectangles.
- Rarity frame/background should be data-driven.
- Item Level and relevant stats should remain readable.
- Selected item gets a strong selection frame.
- Comparison UI should be compact and readable.
- Auto-Recycle remains gameplay/data authority; UI only displays eligibility/result.

### Safety
Do not introduce a new maximum rarity. The number of rarity tiers remains data-driven.

---

## 16. Loot / Rarity Visual System

The supplied reference uses strongly differentiated item-quality presentation.

### TLTD requirements
- Rarity/quality visual treatment must be data-driven.
- Higher rarities may exist beyond the nine qualities visible in the supplied reference.
- A `0%` quality at a lower Cấp Rơi may represent unavailable/not unlocked.
- Never use `Tối Thượng` as a hard-coded final rarity.
- UI should iterate over configured rarity definitions rather than hard-coded rows.

---

## 17. Debug UI separation

The current prototype exposes developer controls such as EXP additions, breakthrough, drop, equip, upgrade, simulation and reset buttons.

### New rule
Developer/debug controls must be separated from the player-facing HUD.

Preferred implementation:
- Debug panel remains available for development/testing.
- It is hidden or collapsed during normal player-facing presentation.
- It must not affect production UI layout.
- Existing test/debug functionality must not be deleted merely for visual cleanup unless separately requested.

---

## 18. Visual component architecture

Suggested presentation components:

```text
_Game/UI/
├── Core/
│   ├── MainGameShell
│   ├── GlobalBottomNavigation
│   ├── UITheme / visual tokens (if useful)
│   └── SafeArea / portrait layout support
│
├── HUD/
│   ├── PlayerStatusHUD
│   ├── TargetStatusHUD
│   ├── SkillBarUI
│   ├── CastBarUI
│   ├── StatusIconRowUI
│   └── CombatFeedbackUI
│
├── Battle/
│   ├── BattleHUD
│   └── BattlePresentation
│
├── Cultivation/
│   ├── CultivationScreen
│   ├── CultivationOverview
│   ├── CultivationCategoryPanel
│   ├── CultivationList
│   ├── CultivationDetailPanel
│   └── CultivationItemView
│
├── Equipment/
│   ├── EquipmentScreen
│   ├── EquipmentSlotView
│   ├── ItemCardView
│   └── ItemComparisonView
│
└── World/
    ├── MainHubScreen
    ├── StageScreen
    ├── CityScreen
    └── ExpeditionScreen
```

These are architectural directions, not a requirement to create every class. Reuse existing components where possible.

---

## 19. UI data-flow rule

```text
Gameplay Authority
       ↓
Existing runtime state / ViewModel / command facade
       ↓
UI Presentation
       ↓
Visual feedback
```

Never:

```text
UI
 ↓
new gameplay calculation
 ↓
new gameplay authority
```

No duplicate authority for:
- Combat.
- Damage.
- HP.
- Shield.
- Rage.
- Cooldown.
- CC.
- Cast/Channel.
- Loot/Rarity.
- Progression.

---

## 20. Responsive / mobile rules

- Portrait-first.
- Safe-area aware.
- Do not rely on fixed desktop resolution assumptions.
- UI should remain usable on narrow portrait screens.
- Text must not overlap icons or bars.
- Important controls must remain reachable by thumb-friendly placement.
- Avoid tiny debug-style text in player-facing UI.
- Use scalable layout groups/anchors where appropriate rather than absolute desktop coordinates.

---

## 21. Implementation scope before P08

### Phase UI-01 — Foundation
- Audit existing UI hierarchy and reusable components.
- Preserve existing gameplay/UI bindings.
- Establish portrait shell and safe area.
- Establish visual panel/button/icon conventions.

### Phase UI-02 — Global Shell
- Refactor MainGameShell.
- Implement/clean five-position GlobalBottomNavigation.
- Make center Main Hub visually dominant.

### Phase UI-03 — Main Hub + Battle HUD
- Replace developer-style battle presentation.
- Rebuild player/target status presentation.
- Integrate existing skill area.
- Integrate P07.8 shield display.
- Integrate P07.9 CastBarUI.
- Keep combat logic unchanged.

### Phase UI-04 — System Screens
- Stage/Floor.
- Thành Lăng Tiêu / world-city.
- Xông Pha.
- Công Pháp.
- Equipment/Loot presentation where current systems already exist.

### Phase UI-05 — Debug Separation
- Move developer controls into a separate development-only/collapsible presentation.
- Do not remove testing capability.

### Phase UI-06 — Acceptance
- Compile check.
- Existing P07.8 regression.
- Existing P07.9 regression.
- Play Mode visual acceptance.
- Portrait/resolution checks.
- No gameplay regression.

---

## 22. Explicit non-goals

This UI pass must NOT:
- redesign D1-D23 gameplay rules;
- change P01-P07.9 locked combat behavior;
- add new skills/effects/resources;
- change Bun rules;
- change Chest/Drop Level rules;
- change Item Level ±5 rule;
- change rarity availability logic;
- change equipment slot count;
- change recycle rewards;
- change cast/channel/interrupt rules;
- replace existing gameplay authorities;
- introduce a second navigation framework;
- turn the reference game's content into TLTD gameplay.

---

## 23. Acceptance target

The UI refactor is accepted only when:

1. TLTD is visibly closer to a polished mobile wuxia/anime RPG presentation than the current developer prototype.
2. The five-slot global navigation structure remains intact.
3. Main Hub remains the center navigation position.
4. System screens use the shared Main Content Area.
5. Công Pháp remains a dedicated system screen.
6. Existing P07.8 Shield behavior remains correct.
7. Existing P07.9 Cast/Channel behavior remains correct.
8. Debug controls are separated from player-facing presentation.
9. No gameplay authority is duplicated.
10. Existing automated/regression tests remain passing.
11. Actual Unity Play Mode visual inspection confirms the new presentation.
12. Any new visual decision that changes locked UI structure is explicitly approved before implementation.

---

## 24. Authority

This document is a **visual implementation specification** based on the supplied screenshots and the locked structural UI baseline.

Authority order remains:
1. Explicit later user-approved UI revision.
2. `UI_DESIGN_AUTHORITY.md` locked UI structure.
3. P01+ tested/accepted gameplay authority.
4. This specification for visual implementation details.
5. Historical reference screenshots.
6. New implementation assumptions.

If implementation discovers a conflict with locked gameplay or UI structure: STOP, report the conflict, and do not silently redesign the authority.
