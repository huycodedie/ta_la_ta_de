# TLTD PROJECT_MEMORY

Canonical P09-B publication: [3240a49](https://github.com/huycodedie/Ai_MEMORY_TLTD/commit/3240a49788bc097acff6b530b95db39876646550).

## Current decision update — 2026-10-05

**P09-B DASH FOUNDATION = ACCEPTED / LOCKED** (accepted 2026-10-04). **F-SAVE-P09B-01 = CLOSED / RECOVERY ACCEPTED**; N-META CLOSED; journal preflight FIX VERIFIED. Scope and runtime hashes: [DECISION_P09B_ACCEPTED_LOCKED_20261004.md](DECISION_P09B_ACCEPTED_LOCKED_20261004.md); evidence limits: [TECH_LEAD_HANDOFF_REVIEW_20261004.md](TECH_LEAD_HANDOFF_REVIEW_20261004.md).

**P09-A Projectile Foundation remains ACCEPTED / LOCKED**, with F-MANUAL-AUTONOMY closed and N1/N2 maintenance verified; [DECISION_P09A_ACCEPTED_LOCKED_20260927.md](DECISION_P09A_ACCEPTED_LOCKED_20260927.md). P08/R1 and prior locked gameplay remain preserved. Owner manual results S01–S05 and M1–M7 / GUI controls remain USER_VERIFIED.

Recovery acceptance is based on the reviewed snapshot/metadata; raw Restore/execution logs remain PARTIAL and baseline Compare/preflight METADATA_ONLY. Preserve old PID 25360 as USER_REPORTED_PASS / RESTORE_UNRESOLVED and P09-A E2 as SCENARIO_PASS_EXIT_TIMEOUT / exit 2. Do not rewrite historical outcomes.

The Owner explicitly authorized Git publication on 2026-10-05; exact documentation-only boundary: [P09B_GIT_PUBLICATION_20261005.md](P09B_GIT_PUBLICATION_20261005.md). This supersedes the earlier handoff task's local no-push restriction for this publication only.

Next action: consult the current roadmap and propose the next bounded gameplay scope. Do not restart accepted P09-A/P09-B tests or enable production skill assets without a separately approved scope. Gameplay functionality remains the priority; finished UI/polish remains deferred. Read [ACTIVE_WORK_HANDOFF.md](ACTIVE_WORK_HANDOFF.md).

The dated sections below are historical where their milestone status or next-action wording conflicts with this update; preserve their evidence, including P08 Gate 2's accepted legacy exceptions.

Historical memory publication (2026-09-26): [8ff7f79](https://github.com/huycodedie/Ai_MEMORY_TLTD/commit/8ff7f79fcdcbb966a88e9a74fd41bd97c5c4a9c2).

## Historical decision update — 2026-09-26

**P08 = ACCEPTED / LOCKED**, closed by Project Owner direct Unity acceptance and Tech Lead decision. Full contract/evidence: [P08_LOCKED.md](P08_LOCKED.md).

**Product priority: complete gameplay functionality first; complete UI and visual polish later.**
The suggestion to start typography/damage-popup UI polish next is superseded. Preserve the functional B1 UI baseline used by P08; unfinished UI phases are deferred.

Next active gameplay slice: **P09-A Projectile Foundation**, scoped in [P09_PROJECTILE_FOUNDATION_TASK.md](P09_PROJECTILE_FOUNDATION_TASK.md). Its implementation/testing/acceptance is pending; do not confuse a task assignment with a LOCK.

P08 fixes the normal-wave contract at 4-5 monsters and once-per-completed-wave MaxHealth/Attack/Defense x1.01 growth. It includes AOE, per-execution channel snapshots/natural completion and sequential loot. Gate 2 remains 135/137, exit 1, with accepted legacy single-monster assumptions in UI02_HeightFix 07/09. Do not relabel them 137/137 PASS.

This dated update supersedes older NOT STARTED / pending-review status and continuation instructions for P08 and the preserved B1 baseline. Historical entries remain evidence of their dates, not active tasks. UI-02 and P07.8/P07.9/P07.9.1 remain LOCKED.
Read ACTIVE_WORK_HANDOFF.md for the exact current action.

## Purpose

This directory is the AI-readable local memory package for the Unity project `E:\code\TLTD`.

The repository `huycodedie/Ai_MEMORY_TLTD` is the long-term backup. Google Antigravity should consume a LOCAL copy inside the TLTD project.

## First-time installation into TLTD

If `E:\code\TLTD\PROJECT_MEMORY` does not exist, obtain this repository and copy its contents into that directory.

If the user does not want the whole memory repository nested inside the game repository, copy only the Markdown files from this directory plus the referenced D/P documents into `E:\code\TLTD\PROJECT_MEMORY`.

## Operating rule

After a new design decision is explicitly LOCKED:

1. Update the memory source in GitHub.
2. Sync the local `PROJECT_MEMORY` copy.
3. Antigravity reads local files before implementation.
4. Code and tests are then updated.
5. Acceptance evidence is recorded before the milestone is marked LOCKED.

## Do not treat this folder as gameplay code

This directory is a contract/memory layer. It does not itself prove that the Unity implementation is correct.

Implementation evidence must come from the actual project, tests, Unity Play Mode, regression logs, and visual checks where applicable.
