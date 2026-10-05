# UI-POLISH-01 PHASE B1 — ACTUAL-SOURCE REVIEW TASK

## Authorization
`AUTHORIZED — READ-ONLY SOURCE AUDIT AND EVIDENCE EXPORT`

Authorization date: `2026-09-17`.

Executor: Google Antigravity, secondary implementation/test executor.
Reviewer and acceptance authority: ChatGPT/Codex, primary Tech Lead and memory publisher.

## Objective
Inspect the actual local Phase B1 implementation in `E:\code\TLTD` and return sufficient source and evidence for Tech Lead review against the locked responsive layout.

This task does not authorize any production, test, scene, prefab, asset, configuration or memory-document edit.

## Mandatory synchronization
Before inspection:

1. Synchronize the local memory copy from `huycodedie/Ai_MEMORY_TLTD`, branch `main`, using fast-forward-only behavior.
2. Confirm remote authority includes commit `220a89ed1cfbad26ba2f565f8a04162495d62a44`.
3. Read:
   - `PROJECT_MEMORY/AI_RULES.md`
   - `PROJECT_MEMORY/CONTINUITY_AND_SYNC_AUTHORITY.md`
   - `PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md`
   - `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`
   - `PROJECT_MEMORY/UI-POLISH-01_ARCHITECTURE_AUDIT.md`
   - `PROJECT_MEMORY/UI-POLISH-01_PHASE_B1_TECH_LEAD_REVIEW.md`
   - `PROJECT_MEMORY/UI-POLISH-01_RESPONSIVE_LAYOUT_SPEC.md`
   - this task file.

Stop if the memory copy cannot be synchronized or the locked responsive layout is missing.

## Mode and prohibitions
- Read-only source inspection.
- Do not edit or format any `.cs` file.
- Do not regenerate or save `Prototype01.unity`.
- Do not open/save scenes, prefabs or assets in a way that changes serialization.
- Do not run SceneBuilder.
- Do not run compile, automated tests or Play Mode in this task.
- Do not apply the local documentation patch/commit `27ba4ce`.
- Do not commit or push anything.
- Do not run `git init` in `E:\code\TLTD`.
- Do not copy Unity production source into `Ai_MEMORY_TLTD`.
- Do not start Phase B2, B3 or B4.
- Do not fix any defect discovered during audit.

If inspection itself changes a file timestamp/content or Unity creates incidental artifacts, stop and report the exact path. Do not delete or revert automatically.

## Source inventory to inspect
Resolve and report the exact absolute path for every item. Do not rely on filenames alone.

### Reported created files
- `IModalView.cs`
- `ModalRequest.cs`
- `ModalBackdrop.cs`
- `ModalCoordinator.cs`
- `Prototype01PlayTestRunner_UI01_PhaseB1.cs`

### Reported modified files
- `LootDecisionUI.cs`
- `LootTierProgressionUI.cs`
- `TitleBreakthroughUI.cs`
- `MindMethodUI.cs`
- `Prototype01SceneBuilder.cs`
- `Assets/_Game/Scenes/Prototype01.unity`

Also identify relevant `.asmdef` files and any additional file that references `ModalCoordinator`, `IModalView`, `ModalRequest`, `ModalBackdrop`, `EquipmentComparison`, `MindMethod`, `LootTierProgression` or `TitleBreakthrough`.

## Required file evidence
For every inspected file return:

| Absolute Path | Exists | Bytes | LastWriteTime | SHA-256 | Reported Created/Modified | Actual Role |
|---|---:|---:|---|---|---|---|

Classify created/modified status as `EXECUTOR-REPORTED` if no independent baseline exists. Do not fabricate a before/after diff.

## Required code excerpts
Return verbatim, line-numbered excerpts containing complete method bodies and enough class context for review. Do not omit branches.

### `ModalCoordinator`
- singleton initialization/reset;
- view registration/unregistration;
- `RequestModal`;
- duplicate detection;
- priority/FIFO queue selection;
- preemption;
- internal show;
- dismiss;
- complete;
- queue draining;
- backdrop/Escape handling;
- scene unload / `OnDestroy` cleanup;
- callback invocation order.

### `LootDecisionUI`
- event subscription lifecycle;
- request construction and payload ownership;
- show/hide;
- Equip flow;
- Tách/recycle flow;
- exact ordering between `CompleteModal`, `BattleManager` mutation, pending-item clearing and encounter continuation;
- legacy fallback path.

### Other modal views
For `LootTierProgressionUI`, `TitleBreakthroughUI` and `MindMethodUI`:

- listener binding/unbinding;
- request construction;
- toggle/open/close path;
- show/hide behavior;
- dismissability;
- payload/callback usage;
- direct `SetActive` calls that bypass the coordinator.

### Backdrop and scene construction
- `ModalBackdrop` pointer handling;
- root Canvas and `GraphicRaycaster` creation;
- `SafeAreaRoot` bounds;
- `ModalLayer` and modal content parenting;
- sibling order, Canvas components, `overrideSorting`, sorting order and raycast settings;
- fixed modal dimensions, anchors, pivots, layout groups and scroll components;
- scene YAML objects/components proving the serialized hierarchy.

### Tests and assemblies
- exact path of the Phase B1 test runner;
- its namespace and assembly membership;
- relevant `.asmdef` include/exclude platforms;
- proof it cannot be included in a production player build;
- source of each claimed test count and whether runtime scenarios A-F are automated assertions or manual/scripted checks.

## Required search audit
Search the whole Unity project read-only and report every relevant occurrence of:

- `ModalCoordinator`
- `IModalView`
- `ModalRequest`
- `ModalLayer`
- `CompleteModal(`
- `DismissModal(`
- `RequestModal(`
- `AddListener`
- `RemoveListener`
- `SetActive(`
- `overrideSorting`
- `sortingOrder`
- `GraphicRaycaster`
- `blocksRaycasts`
- `pendingLootItem`
- `LootDecisionRequested`
- `Time.timeScale`

Classify every bypass or duplicate authority as verified, possible or absent.

## Mandatory technical determinations
Answer each item with `VERIFIED`, `NOT VERIFIED`, `DEFECT`, or `UNKNOWN`, followed by source evidence:

1. Only one blocking modal can be visible.
2. Every blocking-modal entry point routes through the coordinator.
3. Backdrop blocks all underlying Canvas/raycaster combinations.
4. Non-dismissable Equipment Comparison cannot be closed by backdrop/Escape.
5. Duplicate critical payload cannot replace or discard `pendingLootItem`.
6. Equip/Tách transaction completes before the next queued modal becomes visible.
7. Preempted dismissable modal state is intentionally discarded or safely preserved.
8. Singleton/view/callback state is clean after scene unload.
9. Event/listener lifecycle cannot multiply after 20 open/close cycles.
10. `MindMethodUI` does not incorrectly replace the locked dedicated `Công Pháp` screen architecture.
11. Modal backdrop is full physical screen while modal content is Safe Area constrained.
12. Modal body can scroll and sticky actions remain visible on compact screens.
13. Tablet modal/content width is bounded.
14. No UI code changes `Time.timeScale` or gameplay authority.
15. Phase B1 tests are Editor/test-only and excluded from production builds.

## Existing evidence review
Inspect without rerunning, if present:

- `E:\code\TLTD\b1_tests_final.log`
- `E:\code\TLTD\reg_height.log`
- `E:\code\TLTD\reg_p07_8.log`
- `E:\code\TLTD\reg_p07_9_p5_3.log`
- `E:\code\TLTD\reg_p07_9_risk04.log`
- `E:\code\TLTD\reg_p07_9_1.log`

For each log return existence, size, SHA-256, timestamp, command/suite identity, pass/fail summary and whether the content supports the reported count. Do not call old logs a fresh rerun.

## Evidence delivery boundary
Return the audit report to the Project Owner for transfer to ChatGPT/Codex.

- The report may contain the required targeted code excerpts.
- If full source files are needed, place copies only in an explicitly identified temporary review package outside `Ai_MEMORY_TLTD`; do not alter the originals and do not publish the package to GitHub.
- Include the temporary package path and SHA-256 if created.
- Do not include credentials, Unity Library cache, generated binaries or unrelated source.

## Required final report
Return:

1. Authority commit confirmed.
2. Exact source inventory and hashes.
3. Additional relevant files discovered.
4. Complete required code excerpts.
5. Search-audit results.
6. Fifteen technical determinations with evidence.
7. Existing-log integrity table.
8. Differences between actual source and the earlier B1 report.
9. Proposed correction list, classified `BLOCKING`, `REQUIRED RESPONSIVE RECONCILIATION`, or `NON-BLOCKING`; proposals only, no edits.
10. Confirmation: source changes `0`, Unity files saved `0`, tests rerun `0`, commits `0`, pushes `0`.
11. Final audit result: `READY FOR TECH LEAD CORRECTION PLAN` or `BLOCKED — INSUFFICIENT SOURCE/EVIDENCE`.

## Canonical status during this task
- `UI-02 = LOCKED`.
- `UI-POLISH-01 Responsive Layout = LOCKED`.
- `UI-POLISH-01 Phase B1 = NOT ACCEPTED / SOURCE REVIEW IN PROGRESS`.
- `UI-POLISH-01 overall = NOT LOCKED`.
- `Phase B2/B3/B4 = NOT STARTED`.
- `P08 = NOT STARTED`.
