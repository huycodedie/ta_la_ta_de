# D22 — CẤP RƯƠNG / CHEST LEVEL — LOCKED

> Recovered directly from the user's supplied historical decision table. These 10 rules are locked design decisions. Do not invent additional rules or silently change them. Current implementation/Play Mode status is separate and is not asserted by this document.

## D22-1 — Mối quan hệ cấp độ
- **Cấp Rương = Cấp rơi**.
- `Chest Level` is unified with `Drop Level`.
- Treat these as one concept unless a later explicit user decision supersedes it.

## D22-2 — Tài nguyên nâng cấp
- Chest Level upgrade uses **Gold + Time**.

## D22-3 — Mở rương trong khi đang nâng cấp
- A chest may still be opened while an upgrade is in progress.
- Loot uses the **old/current completed Chest Level** until the upgrade finishes.
- The new level is not applied before completion.

## D22-4 — Cost + Time per level
- Each level has its own **Cost + Time**.
- Values are data-driven/configurable.
- Do not infer a universal upgrade formula unless separately approved.

## D22-5 — Gold deduction timing
- The full Gold cost is deducted **immediately when the upgrade is started**.
- No deferred payment at completion.

## D22-6 — Offline time calculation
- Upgrade time continues to progress using **real elapsed time while offline**.
- Reopening the game must evaluate the real-time progression.

## D22-7 — Upgrade progression model
- **A — Manual one-level-at-a-time upgrade.**
- No automatic chained upgrades.
- No upgrade queue.
- The player must initiate each next level separately after the previous upgrade completes.

## D22-8 — Cancellation
- **A — Upgrade cannot be cancelled.**
- Once started, the upgrade continues until completion.

## D22-9 — Completion behavior
- **A — Automatically complete.**
- When `CurrentTime >= FinishTime`, Chest Level immediately changes to the new level.
- Upgrade status becomes `IDLE`.
- No manual "Claim/Receive" action is required.

## D22-10 — Insufficient Gold
- **B — The player may press the upgrade action, then receives an insufficient-Gold notification.**
- Gold is **not** deducted when insufficient.
- No alternative resource is substituted.

## Canonical Upgrade State Model
The locked rules imply this conceptual flow:

```text
IDLE
  │
  │ Start Upgrade (enough Gold)
  ▼
UPGRADING
  │
  ├── Chest Open → use old/current Chest Level
  │
  ├── Offline elapsed time still counts
  │
  └── CurrentTime >= FinishTime
              ▼
           AUTO COMPLETE
              ▼
         ChestLevel = NextLevel
         Status = IDLE
```

Insufficient Gold:

```text
Player presses Upgrade
        ↓
Check Gold
        ↓
Insufficient
        ↓
Notify player
        ↓
No Gold deduction
No alternative resource
No upgrade started
```

## Locked / Do Not Invent
The following are **not specified by D22 itself** and must not be invented:
- Exact Gold cost of each level.
- Exact upgrade time of each level.
- Exact maximum Chest Level.
- Exact rarity probabilities.
- Exact item/drop-level formulas.
- Exact chest opening resource/cost.

Those values must come from other locked design evidence, actual game data, or a later explicit user decision.

## Precedence
- This file records D22 exactly from the supplied historical source.
- Later explicit user decisions may supersede a D22 rule; if so, record the supersession rather than silently rewriting history.
- D22 design status does not imply implementation, automated-test, Play Mode, or visual acceptance status.