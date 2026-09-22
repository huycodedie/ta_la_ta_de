# Ai_MEMORY_TLTD

Persistent AI memory / project decision repository for the TLTD Unity game project.

## Purpose

This repository is the persistent source of project decisions, locked design contracts, architecture constraints, milestone status, validation rules, and handoff context used by AI assistants working on TLTD.

## Source-of-truth hierarchy

1. Original D1-D23 design document supplied by the user — highest priority.
2. Later explicitly LOCKED user decisions and P01+ tested/locked implementation decisions where they supersede older historical wording.
3. Verified repository/runtime evidence and test logs.
4. New proposals — proposals are NOT locked until explicitly approved.
5. General AI/game-development knowledge — must never override the above.

## Critical rule

If a new implementation conflicts with a locked decision, STOP and report the conflict. Do not silently redesign, reinterpret, or invent missing rules.

## Current state

- Project: TLTD / Giang Ho Trong Tay
- Unity + C#
- Repository: E:\\code\\TLTD
- IDE: Google Antigravity
- P07.8 Shield/Barrier: LOCKED according to the latest user-provided acceptance report.
- P07.8 reported Automated: 55/55; Play Mode: 35/35; UI: 5/5; Visual V01-V08: PASS; Master Regression P01-P07.8: PASS.
- Current D1-D23 amendment addendum: `D1_D23_AMENDMENTS_LOCKED.md`.
- Do not start P07.9 without first auditing the existing repository/architecture and consulting the current D1-D23 contract plus amendments.

## Important current design amendments

- Bun = 0 stops Hero, Companion, and Normal combat together; Companion does not continue fighting indefinitely. Boss remains independent of Bun.
- Chest Level = Drop Level = Cấp Rơi; these are one system, not two.
- Item Level is based on current Hero Level and must not differ from Hero Level by more than 5 levels.
- Current item quality reference has 9 tiers: Thô Sơ, Thường, Tốt, Hiếm, Sử Thi, Truyền Kỳ, Thần Thoại, Tinh Khiết, Tối Thượng.
- Equipment has 12 player-worn slots; `SPECIAL` is no longer a player-facing slot name.
- Companion attack interval baseline is 2s; Hero baseline is 1.5s; values remain configurable and independent.
- Equipment recycle returns Gold only.
- D23 lower-CP auto-recycle remains the base rule; preferred attributes can protect a lower-CP item from recycling.
- P01+ through P07.x explicitly locked/tested architecture takes precedence over older historical implementation descriptions when the later decision is more specific.

See `MEMORY_MASTER.md` for consolidated handoff, `D1_D23_LOCKED.md` for the recovered D1-D23 contract, and `D1_D23_AMENDMENTS_LOCKED.md` for later superseding/clarifying decisions.