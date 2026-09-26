# TLTD PROJECT_MEMORY

Memory publication: [8ff7f79](https://github.com/huycodedie/Ai_MEMORY_TLTD/commit/8ff7f79fcdcbb966a88e9a74fd41bd97c5c4a9c2).

## Current decision update — 2026-09-26

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
