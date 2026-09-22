# DESIGN CHANGELOG — TLTD

Purpose: preserve why locked decisions changed without destroying historical source material.

## Status vocabulary
- `LOCKED`: current approved rule.
- `SUPERSEDED`: older rule replaced by a later approved rule.
- `IMPLEMENTED`: code exists; acceptance may still be pending.
- `TEST DISCOVERY`: behavior learned during testing; not automatically a design change.
- `TBD`: intentionally unresolved.
- `CONFLICT`: two sources disagree and no superseding decision has been proven.

## Recorded changes

### Companion HP model
- Historical descriptions in some early material treated Pets/satellites as turret-like/no HP.
- Later D11/D15 and recovered user decisions explicitly establish Companion as a CombatEntity with HP, damage/death and 25s configurable respawn.
- Status: `SUPERSEDED` for the old no-HP description; Companion-with-HP is current.

### Bun = 0 behavior
- Earlier baseline wording could imply only Hero/normal action stopping.
- Later explicit user decision: Hero STOP + Companion STOP + Monster/Normal combat STOP when Bun reaches 0.
- Boss remains independent of Bun.
- Status: `LOCKED` current rule.

### Chest Level / Drop Level
- Earlier documentation used separate terminology and caused ambiguity.
- User explicitly unified them: `Chest Level = Drop Level = Cấp Rơi`.
- Status: `LOCKED`; one runtime concept/authority.

### Item Level range
- Current rule: Item Level is derived from current Hero Level and may differ by at most 5 levels.
- Exact probability/distribution inside the ±5 range is not locked.
- Status: range `LOCKED`; distribution `TBD`.

### Rarity ceiling
- Older historical material listed a finite set of visible rarity tiers.
- User clarified the visible nine qualities are not the maximum; higher qualities may exist and can be unavailable at lower Cấp Rơi. A displayed 0% may mean locked/unavailable.
- Status: any fixed maximum-rarity interpretation is `SUPERSEDED`; rarity system is extensible/data-driven; exact total/unlock/probabilities remain `TBD` unless sourced.

### Equipment slots
- Historical D16 used provisional names including `SPECIAL`.
- Current locked concept is 12 wearable slots: Vũ khí/Kiếm, Mũ, Khăn che mặt, Áo, Quần, Giày, Găng tay, Đai lưng, Áo choàng, Dây chuyền, Nhẫn, Bùa/Ngọc.
- Status: count `LOCKED`; exact player-facing wording may be refined only with evidence.

### Recycle reward
- Historical D16 left EXP reward unresolved.
- Later explicit decision: equipment recycle returns Gold only.
- Status: `LOCKED`; no EXP.

### P01+ precedence
- Historical D12-D16 are recovery documents, not a reason to overwrite later tested behavior.
- P01+ behavior that was tested and explicitly accepted can supersede older historical descriptions.
- Status: `LOCKED operating rule`.

### P07.8 Shield visual blocker
- Initial P07.8 acceptance had a Shield UI/visual gap.
- A remediation was performed using the existing HUD/status architecture.
- Latest user-provided acceptance report states V01-V08 PASS, UI 5/5, Play Mode 35/35, Automated 55/55 and Master Regression PASS.
- Status: P07.8 `LOCKED` based on reported acceptance evidence.

### Global portrait UI shell / Công Pháp separation
- Reference UI review established that the game is a **mobile portrait / vertical-screen game**.
- The bottom navigation is part of the **global game shell**, not a local panel belonging to Công Pháp.
- The global shell has **5 primary navigation positions**.
- The **center position is the Main Hub / Main Game Frame**.
- Individual systems replace the Main Content Area above this navigation.
- **Công Pháp is a dedicated system screen/module** and must not return to the old generic shared-function panel structure.
- The structural baseline is recorded in `PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md`.
- Exact labels/icons, visual styling, spacing and detailed screen layout remain open unless separately approved.
- Status: `LOCKED — UI STRUCTURE BASELINE`.

### P07.9 Advanced Skill Casting
- Cast Time / Channel / Interrupt milestone was explicitly locked by the user after the final regression and audit.
- Locked behavior includes Rage at Cast Start/no refund, completion-based cooldown, no normal cooldown on pre-completion interruption, movement lock while casting, Stun/Freeze interrupt, Root/ordinary damage no interrupt, and preservation of existing execution/status authorities.
- Status: `LOCKED`.

### P07.9.1 Hero Autonomous Skill Decision & Auto Combat
- P07.9.1 was explicitly accepted/locked after audit of autonomous normal-skill decisions, data-driven priority, Auto ON/OFF behavior, Ultimate RageCost from `SkillDefinitionSO.RageCost`, Rage authority preservation, and Cast/Channel interaction.
- Reported validation: dedicated 16/16, P07.8 55/55, P07.9 Phase5.3 36/36, P07.9 Risk04 18/18, historical Master P01-P07.8 PASS, compile 0 errors/0 warnings, runtime scenarios A-E PASS.
- Full baseline: `PROJECT_MEMORY/P07_9_1_LOCKED.md`.
- Status: `LOCKED` based on user-provided execution evidence.

### UI-02 Runtime Combat Height / Y-axis deadlock remediation
- Runtime investigation reproduced a permanent multi-encounter combat deadlock after Monster #1 death.
- Root causes: legacy `BattleManager.monsterSpawnPosition` Y=-1.2 versus the active Prototype01 combat plane Y=-0.3, plus Movement using horizontal distance while Attack used full 3D `Vector3.Distance`.
- The user-approved remediation set spawned Monster Y to -0.3, serialized the same spawn plane in `Prototype01SceneBuilder`, and aligned Attack distance with the Movement horizontal-plane model (`delta.y=0`).
- Spawned Monster visual initialization was also unified with the standard `UIProceduralTextureFactory.GetMonsterStandeeSprite()` pipeline and the legacy 3D `MONSTER` label was removed.
- Reported validation: dedicated 12/12, Play Mode scenarios A-L, cumulative regressions 125/125, compile 0 errors/0 warnings.
- Real Play Mode runtime timing verification confirmed 3/3 consecutive encounters with zero deadlock. Full regression: 137/137 PASS. Visual Acceptance: 4/4 mandatory screenshots (1080x1920) VERIFIED PASS.
- Status: `LOCKED` as of 2026-09-17 (Project Owner acceptance + Tech Lead approval).
- Final UI-02 timing verification compile result: PASS with 0 errors and 8 warnings (4 unique pre-existing warnings).
- This corrects the “0 warnings” summary accidentally written in commit 2ca6169 metadata/current milestone summary.
- Warning correction does not affect the UI-02 LOCK decision.

## Future revision rule
Every new gameplay or UI change must record: old rule -> evidence/reason -> new rule -> status -> affected milestone/code -> tests required. Never delete historical decisions to hide a conflict.
