# TLTD — AI HANDOFF

## Current decision update — 2026-09-26

**P08 = ACCEPTED / LOCKED**, closed by Project Owner direct Unity acceptance and Tech Lead decision. Full contract/evidence: [P08_LOCKED.md](P08_LOCKED.md).

**Product priority: complete gameplay functionality first; complete UI and visual polish later.**
The suggestion to start typography/damage-popup UI polish next is superseded. Preserve the functional B1 UI baseline used by P08; unfinished UI phases are deferred.

Next active gameplay slice: **P09-A Projectile Foundation**, scoped in [P09_PROJECTILE_FOUNDATION_TASK.md](P09_PROJECTILE_FOUNDATION_TASK.md). Its implementation/testing/acceptance is pending; do not confuse a task assignment with a LOCK.

P08 fixes the normal-wave contract at 4-5 monsters and once-per-completed-wave MaxHealth/Attack/Defense x1.01 growth. It includes AOE, per-execution channel snapshots/natural completion and sequential loot. Gate 2 remains 135/137, exit 1, with accepted legacy single-monster assumptions in UI02_HeightFix 07/09. Do not relabel them 137/137 PASS.

This dated update supersedes older NOT STARTED / pending-review status and continuation instructions for P08 and the preserved B1 baseline. Historical entries remain evidence of their dates, not active tasks. UI-02 and P07.8/P07.9/P07.9.1 remain LOCKED.
Read ACTIVE_WORK_HANDOFF.md for the exact current action.

**Project:** Thao Thiet Long Than Dao (TLTD)

**Workspace:** `E:\code\TLTD`

**Engine:** Unity 6000.6.0f1 (64-bit)

**Purpose:** This is the entry point for a new AI chat/coding session. Read this file first, then follow the authority chain and coding protocol below. Do not start implementation from the user's prompt alone.

---

## 1. FIRST ACTION IN EVERY NEW CHAT

Before analyzing or modifying anything:

1. Read `PROJECT_MEMORY/AI_RULES.md`.
2. Read `PROJECT_MEMORY/AI_CODING_PROTOCOL.md` if present.
3. Read `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`.
4. Read `PROJECT_MEMORY/D1_D23_LOCKED.md` and `PROJECT_MEMORY/D1_D23_AMENDMENTS_LOCKED.md`.
5. Read the relevant locked Prototype/Milestone documents for the task.
6. Inspect the actual current source code, Unity scene/prefab/configuration, and relevant tests.
7. Determine the current milestone/status before proposing changes.

**Never assume that a previous chat's summary is enough when the repository memory contains the authoritative document.**

---

## 2. AUTHORITY HIERARCHY

Use this order when sources disagree:

1. Explicit current Project Owner decision.
2. `CURRENT_DESIGN_AUTHORITY.md`.
3. `D1_D23_LOCKED.md`.
4. `D1_D23_AMENDMENTS_LOCKED.md`.
5. Applicable locked Prototype/Milestone documents.
6. Later P01+ behavior explicitly tested and accepted by the Project Owner when it supersedes historical behavior.
7. Current source code.
8. Automated tests.
9. Historical documents.
10. AI assumptions.

A historical rule is not automatically the current rule. A test passing is not automatically proof that the implementation is correct.

If the Project Owner explicitly changed a behavior after testing, treat the later accepted decision as the current design and do not force the old behavior back into code.

---

## 3. CURRENT LOCKED MILESTONE BASELINE

The following milestones are locked according to the project memory:

- P07.5 — LOCKED
- P07.6 — LOCKED
- P07.7 — LOCKED
- P07.8 — LOCKED
- P07.9 — LOCKED
- P07.9.1 — LOCKED / accepted baseline

Important locked runtime authorities include:

- HP → `HealthComponent`
- Shield/status/CC → `EntityStatusController`
- Damage → `DamageCalculator`
- Basic attack execution → `BasicAttackProcessor`
- Rage resource → `RageComponent`
- Skill validation → `SkillExecutionValidator`
- Skill execution → `SkillExecutor`
- Cast/channel runtime state → `SkillCastState`
- Cooldown → `CooldownManager`
- Battle lifecycle/spawn → `BattleManager`
- Movement → `MovementComponent`
- Autonomous Hero skill decisions → `HeroSkillDecisionController`

Do not create a second authority for any of these behaviors.

---

## 4. CORE DESIGN FACTS THAT MUST NOT BE LOST

### Combat

- Hero automatically approaches the target when outside attack range and stops at attack range; D1 is locked.
- Normal targeting uses nearest target; equal-distance tie-break gives Hero priority over Companion.
- Hero death causes immediate defeat; companions stop/disappear from combat.
- Hero basic attack interval is `1.5s`.
- Companion basic attack interval is `2.0s`.
- Rage range is `0–100`.
- Rage generation and Ultimate costs remain data-driven; do not hard-code new gameplay values.
- Ultimate/skill behavior is governed by the locked P07.x/P07.9 architecture.
- P07.9: Rage is consumed at Cast Start; interrupted cast loses Rage with no refund; CastTime cooldown starts at successful completion; Channel cooldown starts when channel ends; Stun/Freeze interrupt; Root and ordinary damage do not interrupt.
- P07.9.1: autonomous Hero skill decision is data-driven; normal-skill priority is not hard-coded to fixed slots; Ultimate uses `SkillDefinitionSO.RageCost`.

### Bun / resource

- Bun is consumed by Hero actions in normal-monster combat.
- Bun = 0: Hero stops, Companion stops, and Monster stops; normal combat stops.
- Boss combat does not depend on Bun.
- Bun regeneration is data-driven; historical locked baseline is approximately 3/minute.
- Offline progression is timestamp/simulation based and stops at the Boss Gate; no offline Boss combat.

### Progression

- No fixed hard stage/level cap is assumed; progression is data-driven.
- Hero level does not directly scale HP/ATK/DEF.
- Title breakthrough increases Hero base stats.
- Level/Title Gate conditions are AND conditions where specified.
- Excess EXP at a Title Level Cap is retained when breakthrough cannot yet occur; an older reset-to-zero rule is superseded.

### Loot / equipment

- Chest Level = Drop Level. Treat them as one unified system.
- Chest upgrade uses Gold + Time.
- Upgrade deducts Gold immediately; no cancellation; no auto-chain/queue.
- During upgrade, opening uses the current/completed level; when `CurrentTime >= FinishTime`, upgrade auto-completes and the new level becomes active.
- Offline elapsed real time counts for chest upgrade.
- Item Level is determined from current Hero Level and must remain within ±5 levels of Hero Level; exact distribution is data-driven/TBD unless separately locked.
- Rarity/quality is determined by Chest/Drop Level. `0%` at a lower level means that quality is unavailable/unlocked at that level; it does **not** establish a hard maximum rarity.
- Higher rarity tiers may exist beyond the visible sample. Never hard-code a maximum rarity count from the sample UI.
- Recycle returns Gold only; no EXP.
- Base Auto Recycle rule: equipment with CP lower than the equipped item is eligible. Future Preferred Attribute/Affix protection can expand this without deleting the base rule.
- 12 equipment slots are locked in count: Vũ khí/Kiếm, Mũ, Khăn che mặt, Áo, Quần, Giày, Găng tay, Đai lưng, Áo choàng, Dây chuyền, Nhẫn, Bùa/Ngọc. Exact presentation naming may be refined only from an authoritative later decision.

---

## 5. IMPORTANT HISTORICAL-DESIGN RULE

D6–D18 contain early design material. Some historical details were intentionally left unresolved or were later superseded by P01+ testing and explicit Project Owner decisions.

Do not reconstruct missing D rules from memory.
Do not invent values to fill historical gaps.
Do not overwrite later accepted behavior with an older document.

When a conflict is found, classify it as one of:

- LOCKED
- SUPERSEDED
- IMPLEMENTED
- TEST DISCOVERY
- CONFLICT
- UNKNOWN

If the conflict cannot be resolved from project memory, stop and request a Project Owner decision.

---

## 6. UI AUTHORITY

`UI_DESIGN_AUTHORITY.md` and `UI_REDESIGN_SPECIFICATION.md` govern UI structure/presentation.

UI is a presentation layer, not a gameplay authority.

UI must read/delegate to existing runtime authorities and must not create duplicate:

- HP state
- Shield state
- Rage state
- CC/status state
- Cast/channel state
- Cooldown state
- Skill execution state
- Damage calculation
- Companion state
- Battle speed authority

The Global Bottom Navigation is the game shell, not owned by Công Pháp.

UI-02 is LOCKED following its later visual acceptance. Do not apply the historical pending-acceptance wording as an active blocker.

---

## 7. RECENT RUNTIME BUG / FIX BASELINE

A confirmed UI-02 runtime combat deadlock was caused by two related defects:

1. Initial Hero/Monster combat plane was `Y = -0.3`, while legacy spawned Monster position used `Y = -1.2`.
2. `MovementComponent` measured horizontal combat distance by zeroing Y, while `AttackComponent` previously used full `Vector3.Distance`.

The accepted remediation aligned spawned Monster Y to `-0.3` and aligned attack distance with the same horizontal combat plane. The spawned Monster visual pipeline was also unified with the existing standee pipeline.

Reported validation:

- Dedicated height-fix suite: 12/12 PASS.
- Multi-encounter Play Mode scenarios A–L: PASS.
- P07.8: 55/55 PASS.
- P07.9 Phase 5.3: 36/36 PASS.
- P07.9 Risk 04: 18/18 PASS.
- P07.9.1: 16/16 PASS.
- Cumulative targeted regression: 125/125 PASS.
- Compile: 0 CS errors, 0 CS warnings.

These are reported execution results; a new AI session must still inspect the current repository state and not blindly assume the report is sufficient proof for a new change.

---

## 8. BUG-ANALYSIS METHOD

For every bug, follow:

```text
USER SYMPTOM
    ↓
REPRODUCTION
    ↓
EXPECTED BEHAVIOR
    ↓
LOCKED DESIGN
    ↓
ACTUAL CODE
    ↓
RUNTIME CODE PATH
    ↓
AUTHORITY
    ↓
ROOT CAUSE
    ↓
MINIMAL FIX
    ↓
REGRESSION RISK
    ↓
TARGETED TEST
    ↓
PLAY MODE
    ↓
REGRESSION
    ↓
REPORT
```

Never jump directly from symptom to code modification.

For spatial/combat bugs, measure actual:

- world/local position
- X/Y/Z deltas
- horizontal distance
- 2D/3D distance
- stopping distance
- attack threshold
- timers/cooldowns
- target
- HP
- Rage
- runtime state

Do not guess when a value can be measured.

---

## 9. TEST MASKING RULE

Always inspect test initialization for artificial conditions that production runtime does not guarantee.

Examples:

- forced transform positions
- forced Y coordinates
- forced stats
- forced RNG
- forced Rage
- forced cooldown
- forced targets
- hidden initialization

If tests pass because they force a condition that production does not have, classify the test as potentially **MASKING A PRODUCTION BUG**.

Automated PASS does not equal Play Mode PASS.
Play Mode PASS does not automatically equal Visual PASS.

---

## 10. IMPLEMENTATION GATE

Before modifying production code, the AI must be able to answer:

```text
[ ] Memory read
[ ] Relevant authority identified
[ ] Current code inspected
[ ] Relevant Unity scene/config inspected
[ ] Relevant tests inspected
[ ] Runtime path traced
[ ] Root cause established
[ ] Design conflict checked
[ ] Minimal fix identified
[ ] Regression impact identified
[ ] Validation plan defined
```

If any critical item is unresolved, STOP rather than guessing.

---

## 11. STOP CONDITIONS

Stop and report instead of coding when:

- a locked design conflict is discovered
- authority is unclear
- required API/state does not exist
- implementation would create a second authority
- a gameplay value is unknown
- tests mask the actual production behavior
- a fix requires unrelated refactoring
- a locked gameplay rule would change without Project Owner approval
- current scene/configuration contradicts source assumptions in a way that has not been resolved

A STOP report must contain:

1. STOP REASON
2. EVIDENCE
3. CONFLICT / UNKNOWN
4. AFFECTED FILES
5. REQUIRED PROJECT OWNER DECISION

---

## 12. PROMPT-GENERATION RULE

When generating a prompt for an AI coding agent, the prompt must contain:

1. Mission
2. Context
3. Authority documents
4. Current behavior
5. Expected behavior
6. Required pre-implementation audit
7. Files to inspect
8. Runtime path to trace
9. Authority constraints
10. Root-cause requirement
11. Implementation constraints
12. Stop conditions
13. Exact tests
14. Play Mode scenarios
15. Regression requirements
16. Evidence requirements
17. Final report format

A coding prompt is an execution protocol, not merely a list of files to edit.

---

## 13. TWO-PHASE CODING WORKFLOW

### PHASE 1 — AUDIT

The AI should not modify production code.

It must:

- read memory
- inspect code
- inspect scene/config
- inspect tests
- trace runtime path
- identify authority
- reproduce/measure the problem
- identify root cause
- propose the minimal fix
- identify regression risks

Then report:

`AUDIT COMPLETE — READY FOR IMPLEMENTATION`

or:

`AUDIT BLOCKED — STOP`

### PHASE 2 — IMPLEMENTATION

Only after the audit is ready:

```text
MODIFY
 ↓
COMPILE
 ↓
TARGETED TESTS
 ↓
UNITY PLAY MODE
 ↓
REGRESSION
 ↓
GIT DIFF AUDIT
 ↓
FINAL REPORT
```

---

## 14. FINAL REPORT REQUIREMENTS

Every implementation report must separate:

**VERIFIED** — directly observed/tested in the current execution.

**REPORTED** — supplied by an existing report.

**INFERRED** — deduced from code/data but not directly executed.

**UNKNOWN** — not established.

Never turn `REPORTED` or `INFERRED` into `VERIFIED`.

Never claim visual verification without visual evidence.

Never claim a milestone is LOCKED merely because code compiles or automated tests pass.

---

## 15. NEW CHAT STARTER

A new chat can begin with:

```text
This is the TLTD project.

Workspace:
E:\code\TLTD

Before doing anything:

1. Read PROJECT_MEMORY/AI_HANDOFF.md
2. Read PROJECT_MEMORY/AI_RULES.md
3. Read PROJECT_MEMORY/AI_CODING_PROTOCOL.md
4. Read PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md
5. Read the relevant locked D1-D23 and Prototype/Milestone documents.
6. Inspect the actual current code, Unity scene/configuration and relevant tests.

Do NOT modify production code yet.

First perform a PRE-IMPLEMENTATION AUDIT and report:
- current architecture
- relevant authority
- actual runtime path
- current behavior
- expected behavior
- root cause
- design conflicts
- files that would need modification
- regression risks
- exact validation plan

Only after the audit is ready should implementation begin.
```

---

## 16. CURRENT PROJECT OPERATING PRINCIPLE

The project is developed iteratively.

The objective is not to wait until the entire game is complete before correcting structural problems.

When testing reveals a better behavior, the Project Owner may explicitly change the design. The AI must then:

1. record the new decision,
2. mark the previous rule as superseded where appropriate,
3. update the authority documents,
4. implement the change,
5. run regression,
6. preserve the change for future chats.

The AI must protect against accidental regression while allowing intentional design evolution.

---

**END OF AI HANDOFF**
