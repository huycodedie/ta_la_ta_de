# DESIGN CHANGELOG — TLTD

## 2026-10-06 — Review Gold-only correction; isolate new test-harness risk

- EXISTING LOCKED DECISION: A10 recycle Gold-only is unchanged. Source restores that contract; current Gold formula remains preserved implementation/TBD balance, no new product decision.
- VERIFIED SOURCE/EVIDENCE: four-file source commit01c5b23 already on game main; ResourceManager minimal Gold-only change and four old assertion groups reviewed. Final targeted batch EditMode fixture5/5, OS exit0; all3 raw wrappers historical RestoreSUCCESS/Diff0. Codex read evidence, did not run Unity/live Registry compare.
- BOUNDED ACCEPTANCE: runtime reward fix ACCEPTED within source/fixture scope; total F-RECYCLE-GOLD-ONLY-01 closeout PENDING HARNESS SAFETY FIX. Unsafe interactive runner menu/persistence mutation and owned-fixture cleanup are new tooling findings, not reopened P08/P09 acceptance.
- ACTIVE SCOPED TASK: F-RECYCLE-HARNESS-SAFE-ENTRY-01; remove interactive bypass, make teardown safe, one guarded batch run after this new tooling delta. Exact prompt and limits in ACTIVE_WORK_HANDOFF/review; Antigravity executes, Codex reviews.
- EVIDENCE LIMITS: R3 API/dummy-modal/queue, R5 immediatePrefs/own listener; no new natural-frame/UI/reload claims. Metadata corrections preserve raw logs and prior failed runs. P08 Gate2, P09-A E2 and P09-B recovery evidence limits remain unchanged.
- PROPOSALS/TBD: Stage progress/Boss Gate foundation and Material acquisition gap remain deferred; no feature/skill assets rollout, reward source or balance authorization.
- DOCUMENTATION PUBLICATION: scoped review/prompt/current handoff, canonical memory plus game memory mirror. No source/asset/save changes by Codex; dirty E: preserved.

## 2026-10-05 — Publish accepted P09-B closure and restore current handoff

- LOCKED DECISION: P09-B Dash Foundation was accepted on 2026-10-04 within reviewed runtime/fixture scope. F-SAVE-P09B-01 CLOSED / RECOVERY ACCEPTED; N-META CLOSED; preflight FIX VERIFIED.
- PRIOR ACCEPTANCE PRESERVED: P09-A Projectile Foundation, P08/R1, S01–S05 and M1–M7 / GUI user verification remain in force. This publication fills the remote documentation gap; it is not a new implementation or a new Unity run.
- VERIFIED PACKAGE EVIDENCE: corrected handoff ZIP 17/17 payload hashes; original Codex decision preserved byte-for-byte. Full scope and source hashes: DECISION_P09B_ACCEPTED_LOCKED_20261004.md; new raw-log limits and provenance corrections: TECH_LEAD_HANDOFF_REVIEW_20261004.md.
- EVIDENCE LIMITS: raw Restore/execution PARTIAL; baseline Compare/preflight METADATA_ONLY. Recovery snapshot 24/24 names/kinds/truncated digests matched baseline; this is not an independent live Registry full-byte inspection. Historical PID 25360 and P09-A E2 outcomes are not rewritten.
- USER-PROVIDED EVIDENCE: Antigravity reported corrected local handoff synchronization complete on 2026-10-04 23:40 +07:00. Local E:\code\TLTD was not directly inspected by Codex during publication.
- EXPLICIT AUTHORIZATION: on 2026-10-05 the Owner requested “nếu đã chốt thì hãy gửi nó lên git”. Publish scoped acceptance/continuity documentation to canonical memory and the game memory mirror. This supersedes the previous task's no-push restriction only for this documentation publication; no runtime/assets/tests/save changes.
- SUPERSEDED CONTINUATION: older instructions treating P09-A as pending implementation or Dash as the immediate unstarted next slice are historical. Next action is roadmap/scope review, not another P09-B acceptance cycle or production skill rollout.
- Publication scope/provenance: P09B_GIT_PUBLICATION_20261005.md. Preserve all unrelated historical decisions below.

## 2026-09-26 — P08 acceptance and gameplay-first continuation

- LOCKED DECISION: the Project Owner personally tested the final conditions in Unity and confirmed they work. Tech Lead closes P08 as ACCEPTED / LOCKED.
- Scope: 4-5 simultaneous normal monsters per wave; HP/ATK/DEF x1.01 once per legitimately completed normal wave; AOE; independent channel snapshots and natural completion; encounter membership; sequential loot with safe modal decisions.
- Full contract, package SHA and provenance: P08_LOCKED.md.
- USER-PROVIDED EVIDENCE: final report gives Gate 1 50/50; Gate 2 135/137 (exit 1); Gate 3 52/52; Gate 4 three natural waves with exit 0. Final manual acceptance is Project Owner evidence, not a new Unity run by Codex.
- ACCEPTED EXCEPTIONS: UI02_HeightFix tests 07/09 retain old one-monster advancement assertions. Their raw failures remain failures; the single-monster expectation is superseded. No production workaround or silent test editing.
- LOCKED DIRECTION: finish game functionality before finished UI. This supersedes the immediate UI-polish-next suggestion. Preserve existing functional UI; do not begin final presentation work.
- NEXT TASK: P09-A Projectile Foundation. Historical combat planning placed projectile/dash work after AOE; this label avoids reopening accepted P08. Scope and technical implementation rules are in P09_PROJECTILE_FOUNDATION_TASK.md; P09 itself is not yet accepted.
- PUBLICATION AUTHORITY: the Project Owner explicitly requested committing/pushing acceptance and LOCK documentation. This permits the scoped memory publication and game-memory mirror; it is not permission to publish unseen local source changes.
- Historical September 17 pending B1/P08 statuses no longer describe current work. Update ACTIVE_WORK_HANDOFF instead of repeating those audits.
- Source reference observed: ta_la_ta_de ff4fcec7fda4f6390b1212300249554a89ef4e3d; the manual-tested local working tree is the acceptance subject. Do not assert byte identity without local hashes.

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

### Responsive portrait layout and deferred visual animation
- The Project Owner clarified that `1080x1920` is a reference coordinate system, not the only supported screen size.
- TLTD UI must support common portrait phones, tall screens, portrait tablets and Safe Area obstructions through anchors, flexible regions, bounded modal geometry and scrolling where required.
- The immediate objective is responsive screen composition and information-layout design, followed by structural implementation and runtime/regression stabilization.
- Final visual polish, decorative art, transitions, tweening and Unity `Animator` work are deferred until the game is stable and the responsive layout is accepted.
- The supplied `Giang Hồ Trong Tay` Google Drive screenshot collection is approved as a visual/layout reference only; it does not create gameplay authority.
- Phase B implementation must not begin by allowing an implementation agent to invent screen composition. A responsive layout specification/wireframe must be reviewed first.
- Full authority: `PROJECT_MEMORY/UI_RESPONSIVE_LAYOUT_AUTHORITY.md`.
- Status: `LOCKED — RESPONSIVE LAYOUT DIRECTION` as of 2026-09-17.

## 2026-09-17 — UI-POLISH-01 Phase B1 out-of-sequence implementation review
- The Project Owner supplied an Antigravity report claiming a local Phase B1 modal-coordination implementation, compile PASS, 17/17 new tests, 137/137 regressions and runtime scenarios A-F PASS.
- The implementation occurred before the required responsive-layout/wireframe review and is therefore not accepted as canonical milestone evidence.
- The source project is not a Git repository and the executor's local documentation commit `27ba4ce` was not pushed; its claims remain executor-reported rather than independently verified.
- Further Phase B edits are frozen. Preserve the local work for read-only inspection and reconciliation after responsive layout approval; do not roll it back merely to restore the earlier status wording.
- Canonical status: `Phase B1 = LOCAL IMPLEMENTATION REPORTED / NOT ACCEPTED / VERIFICATION PENDING`; UI-POLISH-01 remains not locked.
- Detailed decision and technical blockers: `PROJECT_MEMORY/UI-POLISH-01_PHASE_B1_TECH_LEAD_REVIEW.md`.

## 2026-09-17 — UI-POLISH-01 responsive layout approved and locked
- The Project Owner approved the responsive portrait layout direction and specification.
- `1080x1920` remains the reference coordinate system rather than a single-device target.
- Locked coverage includes compact, standard and tall phones, narrow screens, portrait tablets and runtime Safe Area insets.
- Locked structure includes a persistent five-position Global Bottom Navigation, flexible primary/context regions, a full-physical-screen modal backdrop, Safe Area-constrained modal content, bounded tablet width and scroll-based overflow handling.
- `Công Pháp` remains a dedicated system screen/module.
- Final visual polish, decorative assets, transitions and Unity `Animator` remain deferred until runtime stability.
- This approval does not accept the out-of-sequence local B1 implementation. B1 requires actual-source inspection, controlled reconciliation and rerun verification.
- Authority: `PROJECT_MEMORY/UI-POLISH-01_RESPONSIVE_LAYOUT_SPEC.md`.
- Status: `LOCKED — PROJECT OWNER APPROVED RESPONSIVE LAYOUT AUTHORITY` as of 2026-09-17.

### Persistent continuity and Git synchronization
- The Project Owner requires every approved project decision to be committed, pushed and verified in the memory repository so work can continue across new chats and changed workspaces without losing context.
- ChatGPT/Codex is the primary coordinator, Tech Lead/design authority and memory publisher.
- Antigravity is the secondary Unity implementation/test executor and may not independently redefine locked authority.
- GitHub `huycodedie/Ai_MEMORY_TLTD` branch `main` is the persistent canonical memory source.
- `ACTIVE_WORK_HANDOFF.md` records the exact current state and next authorized action; it must be updated whenever accepted work changes the continuation point.
- A local-only commit or unpushed conversation decision is not considered durably synchronized.
- Full policy: `PROJECT_MEMORY/CONTINUITY_AND_SYNC_AUTHORITY.md`.
- Status: `LOCKED — CONTINUITY AND GIT SYNC POLICY` as of 2026-09-17.

## Future revision rule
Every new gameplay or UI change must record: old rule -> evidence/reason -> new rule -> status -> affected milestone/code -> tests required. Never delete historical decisions to hide a conflict.
