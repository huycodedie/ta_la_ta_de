# UI RESPONSIVE LAYOUT AUTHORITY — TLTD

## Purpose
This document records the Project Owner's current UI direction so future implementation does not confuse **layout design**, **visual polish**, and **animation**.

This authority extends `PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md`. It governs responsive portrait layout and implementation order. It does not replace gameplay authority or the locked UI-02 combat baseline.

## Status
`LOCKED — RESPONSIVE LAYOUT DIRECTION`

Decision date: `2026-09-17`.

## 1. Current UI objective
The current task is to **design the UI layout and information structure**.

Current work includes:
- Screen composition and information hierarchy.
- Placement, anchors, relative sizing and spacing of UI regions.
- Combat viewport, top HUD, skill/action area and global bottom navigation.
- Modal header/body/footer structure, scrolling regions and action placement.
- Layouts for Equipment Comparison, Cấp Rơi, Tâm Pháp/Công Pháp, Hero, Companion/Hiệp Khách and related system screens.
- Safe Area handling and responsive behavior across portrait phones and tablets.
- Modal layer, backdrop coverage and interaction boundaries.
- Empty, locked, insufficient-resource and long-content layout states.

This stage is not merely a technical modal-coordinator patch. A reviewed responsive layout specification/wireframe must exist before broad UI implementation.

## 2. Reference resolution is not a fixed target device
`1080x1920` is the **reference coordinate system** for design and verification. It must not be treated as the only supported screen size.

Required Unity baseline:

```text
Canvas Scaler
UI Scale Mode        = Scale With Screen Size
Reference Resolution = 1080 x 1920
Screen Match Mode    = Match Width Or Height
Match                = 0 (width-driven portrait baseline)
Reference Pixels/Unit = 100
```

The implementation must not depend on absolute positions that work only at `1080x1920`.

## 3. Responsive portrait rules
- TLTD remains a portrait-first mobile game.
- Header content anchors to the top of the Safe Area.
- Global Bottom Navigation anchors to the bottom of the Safe Area.
- Combat viewport stretches between the upper HUD and lower action/navigation regions.
- Skill/action controls anchor above the global navigation.
- Modal surfaces center within the Safe Area.
- Modal bodies use flexible sizing and `ScrollRect` for content that cannot fit.
- Close controls anchor to the modal's upper-right region.
- Background art may extend outside the Safe Area, but essential text and controls may not.
- Tall screens gain useful combat/background space; modal content must not be stretched simply to consume all added height.
- Tablet layouts use bounded content width and side gutters instead of horizontally stretching every card.
- Landscape support is outside the current authority unless separately approved.

## 4. Safe Area is mandatory
The root UI layout must accommodate:
- Notches.
- Hole-punch cameras.
- Dynamic-island-style obstructions.
- Status bars.
- Gesture/home-indicator regions.
- Tablets and foldable portrait viewports.

No essential button, label, resource value or navigation control may be placed outside the computed Safe Area.

## 5. Responsive layout bands
Use responsive layout behavior rather than one uniformly scaled fixed composition:

| Layout band | Approximate height/width ratio | Required behavior |
|---|---:|---|
| Compact portrait/tablet | `1.30–1.69` | Reduce nonessential vertical gaps; constrain content width; use scrolling where necessary. |
| Standard portrait | `1.70–1.95` | Use the reference composition. |
| Tall portrait | `1.96–2.25+` | Expand combat/background space while preserving stable HUD, modal and navigation proportions. |

Exact breakpoint implementation may be refined during the layout specification, but all three behavior classes must be addressed.

## 6. Modal layout baseline
Modal geometry is relative to the Safe Area, not hard-coded to a single pixel size:

```text
Width      = approximately 88–92% of Safe Area width
Max Width  = approximately 920 reference units
Max Height = approximately 88–92% of Safe Area height
Header     = stable minimum height
Footer CTA = stable minimum height
Body       = flexible height with ScrollRect when required
```

The full-screen modal layer/backdrop must cover the physical viewport for raycast blocking, while modal content remains constrained to the Safe Area.

## 7. Combat viewport protection
- Backgrounds use aspect-preserving crop/fill; never non-uniform stretch.
- Hero, Monster and important combat feedback stay inside a protected central composition zone.
- UI must not cover the primary combat interaction/readability zone.
- Additional height on tall screens expands presentation space without altering combat distance, ground-plane authority or gameplay calculations.
- Wider tablet viewports may use side gutters or controlled viewport expansion; they must not change gameplay behavior.

## 8. Required layout verification matrix
At minimum, verify the layout at:

- `720x1280` — compact 16:9 phone.
- `1080x1920` — reference portrait.
- `1080x2160` — 18:9 phone.
- `1080x2340` — 19.5:9 phone.
- `1080x2400` — 20:9 phone.
- `1440x3200` — high-resolution tall phone.
- `1536x2048` — 4:3 portrait tablet.
- At least one simulated notched device.
- At least one simulated bottom gesture/home-indicator device.
- At least one larger-text/accessibility configuration.

Acceptance requires:
- No clipped essential text.
- No essential control outside the Safe Area.
- No modal outside the usable viewport.
- Long content remains reachable by scrolling.
- Global navigation remains reachable when no blocking modal is active.
- A blocking backdrop covers the full physical viewport.
- No click-through while a blocking modal is active.
- Combat HUD does not obscure the protected combat area.

## 9. Visual reference authority
Project Owner supplied the following Google Drive folder as the visual-direction reference for TLTD:

`https://drive.google.com/drive/folders/1EtN2qgqELrRvmt5JAnacgRKz4TMQV8jY`

The collection includes reference screens for general gameplay, first-time account flow, equipment, Hero/Companion statistics and related progression interfaces from the sample game `Giang Hồ Trong Tay`.

The reference may guide:
- Screen composition.
- Relative allocation of combat and system areas.
- Card and modal structure.
- Equipment comparison layout.
- Cấp Rơi and Danh Hiệu information layout.
- Hero/Companion presentation.
- Progress bars, states and navigation placement.

The screenshots are **visual/layout references only**. They do not create or supersede gameplay rules, formulas, item authority, progression authority or locked TLTD behavior. Android screenshot-editor overlays, age-warning overlays and unrelated capture UI are not part of the desired game interface.

## 10. Required implementation order
The approved order is:

1. Design responsive layout/wireframes.
2. Review and approve the layout.
3. Implement the structural layout in Unity.
4. Stabilize modal lifecycle, data flow, responsive behavior and regression safety.
5. Verify the game runs reliably without relevant compile, runtime or regression failures.
6. Apply final visual polish using the sample-game direction.
7. Add Animator, transitions, tweening and decorative effects only after stability and visual-layout acceptance.

No implementation agent may invent the layout while coding without an approved layout specification.

## 11. Deferred work
The following are deliberately deferred until after structural and runtime stability:
- Final colors and decorative frames.
- Final typography treatment.
- Final icon artwork.
- High-fidelity visual polish.
- Modal opening/closing animation.
- Button animation and transition effects.
- Damage-popup animation polish.
- Decorative VFX and screen transitions.
- Unity `Animator` integration for presentation-only effects.

Functional scrolling, visibility, input blocking and responsive positioning are structural requirements and are not deferred.

## 12. Milestone relationship
- `UI-02` remains **LOCKED** as of `2026-09-17`.
- `UI-POLISH-01` Phase A architecture audit remains complete.
- Responsive layout design is required before Phase B implementation.
- UI-POLISH-01 implementation has not started and the milestone is not locked.
- `P08` remains **NOT STARTED**.

## 13. Change protection rule
STOP and report a conflict before implementation if a request would:
- Treat `1080x1920` as the only supported device size.
- Use fixed absolute layout positions without responsive anchors.
- Ignore Safe Area constraints.
- Begin final polish or Animator work before structural/runtime stability.
- Copy sample-game visuals as gameplay authority.
- Alter locked UI-02 combat behavior while adjusting layout.
- Allow an implementation agent to choose a new unapproved screen composition during coding.
