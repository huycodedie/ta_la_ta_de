# UI-POLISH-01 — RESPONSIVE PORTRAIT LAYOUT SPECIFICATION

## Status
`LOCKED — PROJECT OWNER APPROVED RESPONSIVE LAYOUT AUTHORITY`

Draft date: `2026-09-17`.
Approval/lock date: `2026-09-17`.

This specification defines structure, responsive behavior and input-safe layout only. Final color, decorative frames, icon art, animation, tweening, transitions and Unity `Animator` work remain deferred until runtime stability.

## 1. Design basis

- TLTD is a portrait-first mobile game.
- `1080x1920` is the reference coordinate system, not a fixed device requirement.
- The five-position Global Bottom Navigation is persistent shell UI.
- System screens replace the Main Content Area; they do not create another bottom navigation.
- The Project Owner's `Giang Hồ Trong Tay` screenshots are layout/composition references only.
- The reference screens consistently use a clear vertical hierarchy: status/header at top, primary gameplay or system content in the center, contextual actions above a persistent bottom navigation, and blocking modal content over a dimmed full-screen backdrop.
- TLTD gameplay, item, progression and combat authorities are not derived from the reference images.

## 2. Root hierarchy

| Layer | Parent | Bounds | Responsibility |
|---|---|---|---|
| `RootCanvas` | Screen | Full physical screen | One authoritative Screen Space Overlay canvas and scaler. |
| `ScreenBackdrop` | `RootCanvas` | Full physical screen | Background art/color; may extend under cutouts. No critical content. |
| `SafeAreaRoot` | `RootCanvas` | Runtime device Safe Area | All interactive shell and screen content. |
| `GlobalShell` | `SafeAreaRoot` | Full Safe Area | Top status, Main Content Area and Global Bottom Navigation. |
| `ModalLayer` | `RootCanvas` | Full physical screen | Blocking-modal backdrop and modal host. Must render above `GlobalShell`. |
| `ModalSafeContent` | `ModalLayer` | Runtime Safe Area | Modal cards, text, controls and sticky actions. |
| `TransientOverlay` | `RootCanvas` | Full physical screen | Non-blocking toast/floating text; must not own blocking-modal state. |

The modal backdrop covers the physical screen, including cutout/gutter regions. Modal content remains inside `ModalSafeContent`.

## 3. Canvas scaling

- Reference resolution: `1080 x 1920`.
- Scale mode: `Scale With Screen Size`.
- Screen Match Mode: `Match Width Or Height`.
- Default match: `0.5`.
- Runtime layout is driven by anchors, layout groups, safe-area bounds and min/max constraints; it must not rely only on the scaler.
- Do not position critical UI with device-specific absolute coordinates.
- Background art may crop using aspect fill. Interactive UI must not crop.

## 4. Responsive classes

Classification uses the Safe Area dimensions, not raw display dimensions.

| Class | Rule | Layout response |
|---|---|---|
| Compact phone | `safeHeight / safeWidth < 1.78` and not tablet | Reduce flexible combat/content height; keep touch targets and sticky controls. Scroll overflowing secondary content. |
| Standard phone | `1.78–2.10` | Reference composition. `1080x1920` belongs here. |
| Tall phone | `> 2.10` | Give surplus height to combat viewport or system content, not to fixed HUD/nav controls. |
| Portrait tablet | `safeWidth / safeHeight >= 0.62` | Center a bounded content column; use side gutters instead of stretching cards and text across the entire width. |

Required validation targets:

| Target | Example resolution | Purpose |
|---|---:|---|
| Compact phone | `1080x1920` with reduced Safe Area height | Short usable height and gesture inset. |
| Reference phone | `1080x1920` | Reference composition and evidence capture. |
| Tall phone | `1080x2400` | Extra vertical space. |
| Narrow phone | `720x1600` | Text wrapping and touch-target pressure. |
| Portrait tablet | `1536x2048` | Width bounding and centered composition. |
| Cutout simulation | Any target plus top/bottom insets | Notch and home-indicator protection. |

## 5. Global shell zones

Percentages refer to the usable Safe Area height. Fixed elements use clamped reference sizes; flexible zones consume the remainder.

| Order | Zone | Standard reference | Compact response | Tall/tablet response |
|---:|---|---:|---|---|
| 1 | Top Status/Header | `6–9%` | Preserve minimum readable/tappable height. | Do not grow beyond maximum. |
| 2 | Context/Enemy Header | `7–10%` | Merge low-priority labels or use one-line wrapping. | Preserve size. |
| 3 | Primary Content / Combat Viewport | `34–42%` | May shrink to its defined minimum. Never overlap HUD/actions. | Receives most surplus height. Tablet content is width-bounded. |
| 4 | Hero HUD + Skill/Action Region | `12–16%` | Preserve controls; reduce gaps before control size. | Preserve maximum height. |
| 5 | Context Tray / System Content | `20–31%` | Becomes scrollable; may collapse optional summaries. | Receives remaining height after primary viewport. |
| 6 | Global Bottom Navigation | `8–10%` | Fixed/sticky above bottom Safe Area inset. | Height is capped; tablet width is bounded. |

No zone may push Global Bottom Navigation off screen. The primary content viewport and context tray are the flexible regions.

## 6. Width, padding and touch rules

- Phone horizontal safe padding: `max(24 reference units, 3% of Safe Area width)`.
- Tablet content maximum width: `960 reference units`; center it with side gutters.
- Standard card gap: `16–24 reference units`.
- Effective touch target: at least `48dp`; at `1080x1920` reference, author buttons/icons with a minimum interactive box of `88 x 88` reference units.
- Visible icon art may be smaller than its interactive box.
- No action button may depend on a screen edge outside Safe Area.
- Text/content containers must wrap or scroll; do not solve overflow by shrinking text below the approved readability floor in Phase B3.

## 7. Main combat layout

| Region | Anchor behavior | Protected content |
|---|---|---|
| Top Header | Top stretch inside `SafeAreaRoot` | Player identity/resources and essential status only. |
| Monster Header | Below top header, horizontal stretch | Monster name/HP and encounter status. |
| Combat Stage | Stretch between headers and Hero HUD | Hero, companion and monster standees; damage text stays inside a protected combat rectangle. |
| Hero HUD | Above skills, horizontal stretch | Hero HP/Rage and essential companion state. |
| Skill Row | Above context tray/nav | Skills, Auto and speed controls; never covered by bottom navigation. |
| Context Tray | Above bottom navigation | Equipment/loot/contextual content; scroll when compact. |
| Bottom Navigation | Bottom stretch in Safe Area | Five equal navigation hit regions; center slot remains Main Hub. |

The protected combat rectangle excludes Top Header, Monster Header, Hero HUD, Skill Row and Bottom Navigation. Standee pivots and floating text must be clamped to this rectangle.

## 8. System screens

- A selected primary system replaces the combat Main Content Area while Global Bottom Navigation remains visible.
- `Công Pháp` is a dedicated system screen/module, not a blocking modal and not a tab inside a generic shared function panel.
- If `Tâm Pháp` is a distinct detail surface, its exact relationship to `Công Pháp` must be mapped before B1 integration; do not assume the current `MindMethodUI` blocking-modal classification is final authority.
- Long lists use a single authoritative `ScrollRect` with fixed header and, where necessary, fixed action footer.
- Hero and Companion/Hiệp Khách presentation uses the same bounded content column and Safe Area rules.

## 9. Blocking modal geometry

| Property | Rule |
|---|---|
| Backdrop | Full physical screen, raycast target enabled, owned only by `ModalCoordinator`. |
| Content bounds | Inside `ModalSafeContent`; never under notch/home indicator. |
| Phone width | `min(92% of Safe Area width, 840 reference units)`. |
| Tablet width | Maximum `840 reference units`, centered. |
| Height | Maximum `82% of Safe Area height`; overflowing body scrolls. |
| Header | Fixed title and close affordance when dismissable. |
| Body | Flexible/scrollable. |
| Footer | Sticky actions; never scroll Equip/Tách/Confirm actions off screen. |
| Queue | At most one blocking modal visible. No visual stacking. |

### Equipment Comparison

- `CriticalGameplay`, non-dismissable by backdrop/Escape.
- Preserve pending-item ownership in `BattleManager`.
- Comparison body scrolls if necessary.
- `[EQUIP]` and `[TÁCH]` remain sticky and mutually exclusive.
- Coordinator completion must not expose the next queued modal until the item transaction has completed and pending state is safe.

### Cấp Rơi

- Header and current/next-level summary remain visible.
- Probability table/body scrolls independently.
- Upgrade requirement and primary action remain in a sticky footer.
- A queued Cấp Rơi request must not cover an unresolved Equipment Comparison.

## 10. Compact, tall and tablet behavior

### Compact phone
- Preserve top header, Hero HUD, skill controls and bottom navigation.
- Reduce decorative spacing first.
- Then reduce flexible Combat Stage/Context Tray height to their minimums.
- Scroll contextual/system/modal bodies instead of shrinking interactive controls.

### Tall phone
- Keep headers, actions and navigation capped.
- Allocate extra height primarily to Combat Stage; system screens may allocate it to scroll viewport height.
- Do not stretch modal cards to fill height.

### Portrait tablet
- Center the Global Shell content column with a maximum width.
- Background/backdrop may fill the display.
- Do not spread five navigation items across the full tablet width; bind navigation to the same centered maximum-width column.
- Modal card width remains capped; increase gutters rather than line length.

## 11. B1 reconciliation requirements

The reported local B1 implementation may be reused only after source review and these structural corrections are satisfied:

1. `ModalLayer` backdrop remains full-screen, while modal cards move under a Safe Area-constrained content host.
2. Replace fixed modal-size assumptions with bounded width/height plus scrolling.
3. Confirm `MindMethodUI` represents an approved blocking detail surface and is not incorrectly replacing the dedicated `Công Pháp` screen architecture.
4. Delay queue draining until Equip/Tách completion is transaction-safe.
5. Verify raycast isolation against every Canvas/GraphicRaycaster and `overrideSorting` surface.
6. Validate every required responsive target before B1 acceptance.

## 12. Acceptance gate for layout approval

The layout can be locked only when:

- all required screens have a documented hierarchy;
- no critical content leaves Safe Area;
- all five navigation hit regions remain reachable;
- modal backdrop blocks the whole physical screen;
- modal content remains usable without clipping;
- compact layouts scroll rather than overlap;
- tall layouts allocate surplus space predictably;
- tablet layouts use bounded centered content;
- `Công Pháp` remains a dedicated system screen;
- final visual polish and Animator remain excluded;
- the Project Owner approves the wireframe direction.

## 13. Current decision boundary

This responsive layout specification is locked and must govern subsequent UI structural work. Its approval does not accept the existing local Phase B1 implementation, authorize Phase B2/B3/B4, or mark UI-POLISH-01 as PASS/LOCKED. Phase B1 must first undergo actual-source inspection and controlled reconciliation against this authority.
