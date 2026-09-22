# CURRENT DESIGN AUTHORITY — TLTD

## Purpose
This file is the quick-reference authority for the **current** design state. It does not erase historical D documents. Historical rules remain traceable through `D1_D23_LOCKED.md`, amendment files, and `DESIGN_CHANGELOG.md`.

## Authority order
1. Original user-approved D1-D23 decisions.
2. Later explicit user-approved amendments/revisions that supersede an older rule.
3. P01+ behavior that was actually tested and explicitly accepted/locked by the user.
4. Current implementation evidence.
5. New proposals.
6. General AI assumptions.

If a conflict cannot be proven as superseded: STOP and report it. Never guess.

## Current locked amendments
### Combat resource / Bun
- Bun applies to Normal Monster Battle.
- 1 Bun = 1 Hero combat action.
- Hero and Companions both stop when Bun reaches 0 during Normal Battle.
- Boss Battle does not depend on Bun.

### Chest / Drop Level
- `Chest Level = Drop Level = Cấp Rơi`.
- One concept, one runtime authority.

### Item Level
- Item Level is based on current Hero Level.
- `abs(ItemLevel - HeroLevel) <= 5`.
- Distribution inside ±5 is TBD unless explicitly sourced.

### Rarity / Quality
- Cấp Rơi controls availability/unlock and probability of qualities.
- The nine qualities visible in the reference UI are not the maximum.
- Higher qualities may exist.
- `0%` at a lower Cấp Rơi can mean unavailable/not unlocked; it does not mean the quality does not exist.
- Rarity count, names beyond sourced data, unlock thresholds and probabilities remain data-driven/TBD unless explicitly locked.
- Never hard-code `Tối Thượng` as the final rarity.

### Equipment
- 12 wearable equipment slots are locked.
- Current concepts: Vũ khí/Kiếm, Mũ, Khăn che mặt, Áo, Quần, Giày, Găng tay, Đai lưng, Áo choàng, Dây chuyền, Nhẫn, Bùa/Ngọc.
- Do not use `SPECIAL` as a player-facing slot name.

### Attack interval
- Hero: 1.5s baseline.
- Companion: 2.0s baseline.
- Both remain configurable and independent.

### Companion combat
- Companion uses the common Combat Resolver/pipeline.
- Combo/Counter must not have a separate Companion engine.
- Companion has HP, can die and respawn; this supersedes older historical text that described Pets/satellites as having no HP.

### Recycle
- Equipment recycle returns GOLD ONLY.
- Base Auto-Recycle rule: lower item CP than the currently equipped item is eligible.
- Future Preferred Attribute/Affix protection can keep an otherwise eligible item.

### Chest upgrade
- One level at a time; no queue; no auto-chain; no cancel; no claim button.
- During upgrade, opening uses the currently completed level.
- At persisted `CurrentTime >= FinishTime`, auto-complete to the next level and enter IDLE.
- Persist timestamps; do not rely on transient timers for authority.

## UI structure baseline
- The game is a **2D mobile portrait / vertical-screen game**.
- The bottom navigation is a **global game shell**, not part of an individual system screen.
- There are **5 primary global navigation positions**.
- The **center position is the Main Hub / Main Game Frame**.
- Major systems replace the Main Content Area above the navigation; they do not create a second navigation framework.
- **Công Pháp is a dedicated system screen/module**, not another tab inside a generic all-purpose function panel.
- Công Pháp may contain its own overview, category list, and detail sub-screens while retaining the global 5-position navigation shell.
- Exact navigation labels/icons, portrait resolution, pixel layout, visual art and detailed interaction remain TBD unless separately approved.
- Full structural rules are recorded in `PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md`.

## P07.8 status
P07.8 Shield/Barrier is LOCKED according to the latest user-provided acceptance report: Automated 55/55, Play Mode 35/35, UI 5/5, Visual V01-V08 PASS, Master Regression P01-P07.8 PASS.

## P07.9 status
P07.9 Advanced Skill Casting is **LOCKED** according to the user's final acceptance. Locked behavior includes Cast/Channel progression, Rage at Cast Start/no refund, cooldown after successful completion, no normal cooldown for interruption before completion, movement lock while casting, Stun/Freeze interrupt, Root/ordinary damage no interrupt, existing authority preservation, Ultimate validation before Rage, and no second execution/interrupt authority.

## P07.9.1 status
P07.9.1 Hero Autonomous Skill Decision & Auto Combat is **LOCKED** according to the user's final audit/execution report. Locked behavior includes data-driven normal-skill priority, Auto ON autonomous normal skills, Auto OFF disabling autonomous skill/ultimate decisions while Basic Attack remains active, Ultimate RageCost read from `SkillDefinitionSO.RageCost`, Rage mutation remaining in the existing authority, and Basic Attack blocked during active Cast/Channel. Reported validation: dedicated 16/16, P07.8 55/55, P07.9 Phase5.3 36/36, P07.9 Risk04 18/18, historical Master P01-P07.8 PASS, compile 0 errors/0 warnings, runtime scenarios A-E PASS. Full baseline is in `PROJECT_MEMORY/P07_9_1_LOCKED.md`.

## UI-02 status
Global UI-02 is **LOCKED** as of 2026-09-17 (Project Owner acceptance + Tech Lead approval).

### UI-02 runtime combat height remediation & visual acceptance
The runtime deadlock caused by Y-axis discrepancy ($Y = -1.2\text{m}$ vs $Y = -0.3\text{m}$) and 3D Euclidean distance evaluation in AttackComponent has been completely remediated, verified by telemetry, regression tested (137/137 PASS), and visually accepted:
- Monster spawn Y = -0.3.
- `Prototype01SceneBuilder` serializes the same spawn Y into `Assets/_Game/Scenes/Prototype01.unity`.
- `AttackComponent` uses the same horizontal combat-plane distance model as `MovementComponent` (`delta.y = 0`, then magnitude).
- Spawned monsters reuse `UIProceduralTextureFactory.GetMonsterStandeeSprite()`, scale `(1.1,1.1,1)`, sorting order 10, `flipX=true`, with legacy 3D `MONSTER` label and unstyled white boxes removed.
- Dedicated validation: 12/12 PASS.
- Play Mode real runtime timing verification: 3/3 consecutive encounters completed by `MonsterDeath` with zero deadlock observed.
- Full regression: 137/137 PASS across all 5 test suites (UI-02 Height Fix 12, P07.8 55, P07.9 Phase5.3 36, P07.9 Risk04 18, P07.9.1 16).
- Visual Acceptance: 4/4 mandatory 1080x1920 reference portrait screenshots VERIFIED PASS (`UI02_VISUAL_01_Encounter1Combat.png` to `UI02_VISUAL_04_Encounter3Combat.png`).
- Compile: PASS — 0 errors / 8 warnings (4 unique pre-existing, non-blocking warnings in legacy test runners).
- Locked P07.8/P07.9/P07.9.1 and D1-D23 gameplay authorities remain untouched.
- Non-blocking presentation debt (modal coordination, damage popup styling, font scaling) is explicitly deferred to `UI-POLISH-01`.

*Status: Milestone UI-02 is LOCKED.*

## UI-POLISH-01 status
UI-POLISH-01 Phase A (Architecture Audit) is **AUDIT COMPLETE**.
Implementation has **NOT STARTED**.
- Scope audited: Modal coordination, full-screen input blocker / backdrop, mobile readability (1080x1920 reference portrait), floating combat text lifecycle, and listener lifecycle safety.
- Root causes: 100% verified from code and visual evidence.
- Proposed architecture: Non-intrusive `ModalCoordinator` with priority queuing, single exclusive blocking modal invariant, and full-screen `ModalBackdrop`. Zero modification to combat formulas, item stats, or `Time.timeScale`.
- Implementation plan: Phased plan defined (Phase B1: Modal Exclusivity & Queue; Phase B2: Pending Payload Safety; Phase B3: Typography & Touch Targets; Phase B4: Damage Popup Lifecycle & Pooling).
- Test matrix: 15 dedicated verification scenarios established.
- Audit report: Complete documentation recorded in `PROJECT_MEMORY/UI-POLISH-01_ARCHITECTURE_AUDIT.md`.
- Status rule: UI-POLISH-01 is NOT locked and NOT declared PASS. Implementation will begin in Phase B.

*Milestone status: UI-02 remains LOCKED. UI-POLISH-01 = AUDIT COMPLETE / IMPLEMENTATION NOT STARTED. P08 remains NOT STARTED.*

## Implementation rule
P01+ tested-and-accepted behavior may supersede an older historical design rule. Record the change; do not silently overwrite history.

## Evidence qualifier
All execution/visual/regression numbers stated above come from user-provided project reports unless independently executed in the current chat. Never convert a report claim into independent verification.

## Continuation handoff
For the full current-chat package, read `PROJECT_MEMORY/CHAT_HANDOFF_2026-09-16.md`.
