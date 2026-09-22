# UI DESIGN AUTHORITY — TLTD

## Purpose
This document protects the current UI structural baseline so future UI work does not accidentally redesign or break the global navigation shell.

This is a **structural UI authority**, not a final pixel-perfect visual specification. Visual styling, exact spacing, typography, icon artwork and decorative details may evolve without changing the locked structure below.

## Status
`LOCKED — UI STRUCTURE BASELINE`

## 1. Global orientation
- The game is a **mobile portrait / vertical-screen 2D game**.
- All major UI screens must be designed for portrait usage first.
- Do not design a landscape-first layout and merely scale/crop it into portrait.
- Future UI systems must respect the portrait content area and mobile safe-area constraints.

## 2. Global Navigation Bar
The bottom navigation shown in the current reference is part of the **global game shell**, not part of the Cultivation/Công Pháp screen itself.

### Locked structure
- The game has **5 primary navigation positions** in the global bottom navigation.
- The **center position is the Main Hub / Main Game Frame**.
- The bottom navigation is intended to remain available while navigating between major game systems, unless a future explicit design decision introduces a modal/full-screen exception.
- A system screen must replace/change the **main content area**, not redesign or replace the global 5-slot navigation bar.

Conceptually:

```text
GameRoot / MainShell
├── MainContentArea
│   └── CurrentSystemScreen
└── GlobalBottomNavigation (5 positions)
    ├── Position 1
    ├── Position 2
    ├── Position 3 = Main Hub / Main Game Frame
    ├── Position 4
    └── Position 5
```

The exact player-facing names/icons of the five positions are not locked by this document unless separately sourced.

## 3. Main Hub
- The center navigation position is the **main game frame / Main Hub**.
- It is not merely an ordinary third navigation button.
- It is the primary home/context from which major systems can be accessed and to which the player can return.
- Future systems should be integrated into this shell rather than creating unrelated navigation frameworks.

## 4. Main Content Area
- The area above the global bottom navigation is the **Main Content Area**.
- Each major game system may have its own dedicated screen/module inside this area.
- A system may have multiple internal sub-screens/panels while retaining the same global navigation shell.
- Internal Back navigation must operate inside the current system without duplicating the global navigation bar.

## 5. Công Pháp / Cultivation UI
### Locked structural decision
- **Công Pháp is a dedicated system screen/module.**
- It must NOT continue using one generic shared content layout that tries to represent every game function.
- Opening Công Pháp changes the Main Content Area to the dedicated Công Pháp screen while the Global Bottom Navigation remains the game's common shell.

### Recommended screen hierarchy

```text
Công Pháp Screen
├── Header / Back / System title
├── Công Pháp Overview
│   ├── Character presentation
│   └── Total Công Pháp Buff summary
├── Công Pháp Categories
│   ├── Cơ Bản
│   ├── Chiêu Thức
│   ├── Ngoại Công / other sourced categories
│   ├── Tuyệt Kỹ
│   └── Tâm Pháp
├── Category List / Grid
└── Công Pháp Detail
    ├── Icon / identity
    ├── Current level/state
    ├── Current effects
    ├── Next-level information when applicable
    └── Upgrade / equip actions when applicable
```

The exact category list and naming must follow the current game design/data source. Do not invent new gameplay categories merely to fill UI space.

## 6. Separation of UI responsibilities
- Global Navigation owns navigation between the game's major root systems.
- A system screen owns its own internal presentation and navigation.
- Công Pháp UI must not own or redefine global navigation state.
- UI must display authoritative game data; it must not become a second authority for combat, stats, progression or Công Pháp effects.
- The visual layer must remain modular so additional Công Pháp types/categories can be added without rebuilding the entire global shell.

## 7. Unity architecture direction
The UI should conceptually be structured as:

```text
_Game/UI/
├── Core/
│   ├── MainGameShell
│   └── GlobalBottomNavigation
│
└── Cultivation/
    ├── CultivationScreen
    ├── CultivationOverview
    ├── CultivationCategoryPanel
    ├── CultivationList
    ├── CultivationDetailPanel
    ├── CultivationItemView
    └── CultivationStatSummary
```

These names are architectural examples, not a mandate to create every class immediately. Do not introduce duplicate gameplay authorities merely because a UI component needs data.

## 8. What is NOT locked yet
The following remain open unless separately approved:
- Exact five navigation labels/icons.
- Exact portrait resolution/reference device.
- Exact dimensions, spacing and anchors.
- Final fonts, colors, borders and decorative art.
- Exact Công Pháp category names beyond sourced/approved terminology.
- Exact number of Công Pháp entries.
- Exact interaction/animation details.
- Exact placement and size of the character artwork.
- Whether some system screens temporarily hide the global navigation in a future explicitly approved full-screen mode.

## 9. Change protection rule
If a future implementation request would:
- replace the 5-slot global navigation,
- move the Main Hub away from the center position,
- redesign the game as landscape-first,
- make Công Pháp depend on the generic shared-function panel again,
- or create a second unrelated navigation framework,

STOP and report the conflict with this document before changing code.

A change is allowed only after an explicit new user decision is recorded as a superseding UI decision.

## 10. Authority relationship
This document governs **UI structure**. It does not override gameplay/design contracts D1-D23 or P01+ tested gameplay authority.

Priority for UI decisions:
1. Explicit later user-approved UI revisions.
2. This locked UI structure baseline.
3. Existing tested/accepted UI behavior that does not conflict with the above.
4. Historical UI references.
5. New proposals.

Never silently reinterpret a historical screenshot as a locked gameplay rule.
