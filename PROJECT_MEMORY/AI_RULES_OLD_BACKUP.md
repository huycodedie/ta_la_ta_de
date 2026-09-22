# AI_RULES — TLTD PROJECT MEMORY AUTHORITY

> Local-copy contract for Google Antigravity / Unity development.
> GitHub is the long-term memory backup. Antigravity must read the LOCAL copy under `E:\code\TLTD\PROJECT_MEMORY\`, not assume it can read GitHub directly.

## 1. Purpose

This folder is the local, AI-readable Design Contract for project `E:\code\TLTD`.

Before implementing or modifying gameplay code, AI must read:

1. `AI_RULES.md`
2. `D1_D23_LOCKED.md`
3. `D1_D23_AMENDMENTS_LOCKED.md`
4. Relevant D-section documents (`D6_D8_LOCKED.md`, `D9_D11_LOCKED.md`, `D12_D14_LOCKED.md`, `D15_D16_LOCKED.md`, `D17_D18_LOCKED.md`, `D22_LOCKED.md`)
5. Relevant P07.x locked/acceptance documents available in this memory repository
6. Current project code and tests

## 2. Authority hierarchy

When rules differ, use this order:

1. Original user D1-D23 decisions
2. Later explicit locked amendments
3. P01+ behavior that was actually tested and explicitly accepted/locked by the user
4. Current implementation evidence
5. New proposal
6. General AI assumptions

If a later P01+ decision explicitly supersedes an older historical rule, the later decision wins.

If a conflict cannot be proven as superseded, STOP and report it. Do not silently redesign.

## 3. Current locked amendments

### Bun
- Bun is consumed by Hero actions in Normal Battle.
- Companion actions do not consume Bun.
- When Bun reaches 0 during Normal Battle: Hero stops, Companions stop, and Normal combat stops.
- Boss Battle does not depend on Bun.

### Chest / Drop Level
- `Chest Level = Drop Level = Cấp Rơi`.
- These are one concept and must have one runtime authority.
- Do not create separate ChestLevel and DropLevel systems.

### Item Level
- Item Level is based on the Hero's current Level.
- Constraint: `abs(ItemLevel - HeroLevel) <= 5`.
- The exact distribution/roll inside the ±5 range is NOT locked unless another source explicitly provides it. Do not invent one.

### Rarity / Quality
- Cấp Rơi determines which qualities/rarities can appear and their probabilities.
- The screenshot showing `Thô Sơ, Thường, Tốt, Hiếm, Sử Thi, Truyền Kỳ, Thần Thoại, Tinh Khiết, Tối Thượng` is NOT a maximum-rarity list.
- Higher qualities may exist beyond what is visible in the screenshot.
- A quality may show `0%` because the current Cấp Rơi has not unlocked/allowed it yet; `0%` does NOT mean the quality does not exist.
- Rarity count, future rarity names, unlock thresholds, and probabilities not explicitly locked remain data-driven/TBD.
- NEVER hard-code `Tối Thượng` as the final maximum rarity.

### Equipment slots
- There are 12 equipment slots.
- Current player-facing concepts: Vũ khí/Kiếm, Mũ, Khăn che mặt, Áo, Quần, Giày, Găng tay, Đai lưng, Áo choàng, Dây chuyền, Nhẫn, Bùa/Ngọc.
- Do not use `SPECIAL` as a player-facing slot name.
- Slot count is locked; exact UI wording may be refined only with evidence.

### Attack interval
- Hero baseline: 1.5 seconds.
- Companion baseline: 2.0 seconds.
- Keep these independent and configurable.

### Companion combat
- Companion uses the common Combat Resolver / common combat pipeline.
- Do not create a separate Companion Combo/Counter engine.
- Preserve anti-recursion rules.

### Status / CC
- P07.6+ architecture is authoritative over older historical implementation descriptions.
- Use the shared `EntityStatusController`, `EffectResolver`, and shared status/combat pipeline.
- Do not create duplicate Hero/Companion status authorities.

### Chest upgrade
- Upgrade is one level at a time.
- Start `N -> N+1` enters `UPGRADING`.
- While upgrading, opening a chest still uses the currently completed level.
- When persisted `CurrentTime >= FinishTime`, auto-complete immediately: level becomes `N+1`, status becomes `IDLE`.
- No Claim button.
- No Cancel.
- No queue.
- No auto-chain to N+2.
- State must survive closing/reopening the game; authoritative state must use persisted timestamps, not a transient `setTimeout`.

### Recycle
- Equipment recycle reward is GOLD ONLY.
- No Cultivation EXP from recycle.
- Base Auto-Recycle rule: item CP lower than the currently equipped item is eligible for Auto-Recycle.
- Future Keep/Protect rules such as Preferred Attribute/Affix may protect an otherwise eligible low-CP item.
- Evaluate eligibility first, then protection/keep conditions, then make the final recycle decision.

## 4. P01+ implementation rule

P01+ marks the point where the project entered implementation and iterative testing.

Historical D12-D16 material must NOT be blindly forced onto later code if a later behavior was tested and explicitly accepted.

Lifecycle:

`Initial Design -> Implementation -> Play Mode/Test -> Discovery -> User Decision -> New Implementation -> LOCKED`

## 5. Test discipline

Never claim:

- Compile PASS = Gameplay PASS
- Automated PASS = Visual PASS
- Test definition = Test execution

For a milestone to be called LOCKED, use actual evidence appropriate to the milestone: automated tests, Unity Play Mode, regression, and visual verification where applicable.

## 6. No invention rule

Do not invent:

- gameplay values
- rarity maximum
- rarity unlock levels
- probability tables not sourced
- Item Level distribution formula
- CP weights
- loot formulas
- costs/timers not locked
- new status/effect behavior
- new authority classes

If unknown, mark it `TBD` or ask the user.

## 7. Conflict protocol

If code and Design Contract differ, report:

- [CONFLICT]
- Historical rule
- Current implementation
- Later locked rule, if any
- Evidence
- Recommended action
- Need user decision: YES/NO

Do not silently alter either side.

## 8. Required workflow for every new milestone

1. Read this file.
2. Read relevant D/P locked documents.
3. Audit current code before editing.
4. Identify dependencies and authority ownership.
5. Identify conflicts and unresolved TBD values.
6. Propose implementation scope.
7. Only then modify code.
8. Add/extend tests.
9. Run actual Unity/Play Mode validation where required.
10. Update milestone evidence and memory only after acceptance.

## 9. Important current architecture principle

Extend existing authoritative systems. Do not duplicate:

- Combat Resolver
- Damage Calculator
- Health/Damage pipeline
- EntityStatusController
- EffectResolver
- Shield authority
- event authority

P07.8 is LOCKED and must not be regressed while implementing later milestones.
