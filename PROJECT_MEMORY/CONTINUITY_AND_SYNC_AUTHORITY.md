# CONTINUITY AND SYNC AUTHORITY — TLTD

## Purpose
This document ensures that approved project decisions, current progress and unfinished work survive chat changes, context loss, tool changes and handoff between ChatGPT/Codex and Antigravity.

Conversation history is temporary working context. The `main` branch of `huycodedie/Ai_MEMORY_TLTD` is the persistent project-memory source of truth.

## Status
`LOCKED — CONTINUITY AND GIT SYNC POLICY`

Decision date: `2026-09-17`.

## 1. Roles

### ChatGPT/Codex — primary coordinator
- Acts as Tech Lead, Architect, design-governance authority and memory publisher.
- Consolidates Project Owner decisions.
- Distinguishes approved decisions from proposals and assumptions.
- Reviews implementation reports, diffs and evidence.
- Updates and publishes the memory repository after approved decisions or accepted milestones.

### Antigravity — secondary implementation executor
- Reads the synchronized local memory package before implementation.
- Modifies the Unity project only within an approved scope.
- Runs compile, automated tests, Play Mode checks and evidence capture.
- Reports exact files, diffs, test results, runtime results and blockers.
- Must not independently redefine locked design authority.
- Updates the memory repository only when an explicit task authorizes exact documentation paths and commit/push behavior.

### GitHub `main` — persistent source of truth
- Repository: `huycodedie/Ai_MEMORY_TLTD`.
- Branch: `main`.
- GitHub `main`, not either agent's private conversation memory, is the canonical continuation source.
- Unity production source must never be copied into the memory repository.

## 2. Mandatory decision-sync rule
When the Project Owner explicitly approves, locks, supersedes or materially clarifies a project decision:

1. Identify the affected authority and status files.
2. Record the decision with date, scope, constraints and superseded rule when applicable.
3. Update `CURRENT_DESIGN_AUTHORITY.md` when the decision affects the current state.
4. Update `DESIGN_CHANGELOG.md` when the decision changes, supersedes or clarifies prior authority.
5. Update `ACTIVE_WORK_HANDOFF.md` with the completed action and exact next step.
6. Review `git diff --check`, staged paths and staged diff.
7. Commit with a scoped, descriptive message.
8. Push normally to `origin/main`.
9. Verify the remote commit and remote file contents.
10. Return the commit SHA and direct GitHub links.

A decision is not considered durably synchronized until remote verification succeeds.

## 3. Push failure policy
If authentication, permission, network or remote-state failure prevents push:

- Preserve the local commit or export a patch.
- Report the exact error.
- Mark synchronization as `PENDING — NOT ON REMOTE`.
- Do not claim the decision is safely stored in GitHub.
- Do not use credential workarounds, another repository, force-push, rebase or history rewriting.
- Resolve the synchronization blocker before starting implementation that depends on the unsynchronized decision.

## 4. New-chat and resumed-work protocol
At the start of a new chat, changed workspace or resumed task, read in this order:

1. `PROJECT_MEMORY/AI_RULES.md`
2. `PROJECT_MEMORY/CONTINUITY_AND_SYNC_AUTHORITY.md`
3. `PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md`
4. `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`
5. `PROJECT_MEMORY/DESIGN_CHANGELOG.md`
6. Relevant locked D/P/UI authority files named by the active handoff.
7. Relevant implementation/audit/test reports.

Then verify:
- repository identity;
- branch;
- current remote HEAD;
- clean or understood working-tree state;
- last completed milestone;
- active milestone;
- implementation status;
- exact next authorized action;
- explicit prohibitions and stop conditions.

Do not restart, skip ahead or reinterpret the project from generic assumptions when these records exist.

## 5. Active handoff requirements
`PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md` is the short current-state pointer and must record:

- Last synchronization date and remote commit.
- Current locked milestone state.
- Active workstream and phase.
- Work completed.
- Work not started.
- Exact next action.
- Files/reports that must be read next.
- Current Project Owner constraints.
- Known blockers or unresolved questions.

It is intentionally concise. Detailed evidence remains in milestone-specific reports.

## 6. Classification discipline
Every persisted statement must be identifiable as one of:

- `LOCKED DECISION` — explicitly approved by the Project Owner.
- `VERIFIED EVIDENCE` — independently inspected or executed evidence.
- `USER-PROVIDED EVIDENCE` — reported result not independently rerun in the current environment.
- `PROPOSAL` — not approved and must not be implemented as authority.
- `TBD` — unresolved and must not be invented.
- `SUPERSEDED/HISTORICAL` — retained for traceability but not current authority.

Do not convert a proposal, inference or historical statement into current authority.

## 7. Git safety
- Pull with `--ff-only` before editing when working from a local clone.
- Stage exact paths; never use broad staging for mixed worktrees.
- Do not amend published commits.
- Do not rebase or squash shared history without explicit Project Owner authorization.
- Never force-push or use `--force-with-lease` for the memory repository.
- Stop on unrelated changes, conflicts or unexpected remote advancement.
- Do not delete or revert another actor's work merely to obtain a clean tree.

## 8. Implementation synchronization
Before Antigravity implements a task:

1. Synchronize the local `PROJECT_MEMORY` copy from GitHub `main`.
2. Read the mandatory continuation files.
3. Confirm the expected authority commit.
4. Audit current Unity source rather than assuming it matches memory.
5. Stop if source and authority conflict.

After implementation:

1. Antigravity returns source diff and execution evidence.
2. ChatGPT/Codex reviews the evidence as primary coordinator.
3. Only accepted results update milestone authority/status.
4. The accepted status is committed, pushed and remotely verified.

## 9. Current project relationship
This policy does not change gameplay design or milestone acceptance by itself.

Current state when this policy was established:
- `UI-02 = LOCKED` as of `2026-09-17`.
- `UI-POLISH-01 Phase A = AUDIT COMPLETE`.
- Responsive layout design is required before UI-POLISH-01 implementation.
- UI-POLISH-01 implementation has not started and is not locked.
- `P08 = NOT STARTED`.
