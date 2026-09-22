# D17-D18 LOCKED DESIGN RECOVERY

> Recovered from the user's historical design source. This file preserves the D17/D18 decisions without inventing values that the source explicitly marked TBD/unlocked.

## D17 — CHEST / LOOT ECONOMY

### D17.1 — Core Gameplay Loop
- Main progression loop:
  `Idle/Game → Receive Resources → Open Chest → Equipment/Resources → Compare Item → Keep or Recycle → Equip → Hero CP → Stage Progress → More Loot → Open Chest`.
- Chest/equipment progression is part of the core gameplay loop.

### D17.2 — Chest Level
- Each player has a `ChestState`.
- `ChestState` includes at least:
  - `chestLevel`
  - `upgradeCost`
  - `upgradeProgress`
  - `isUpgrading`
- Hero Level and Chest Level are independent systems.
- Chest Lv.10 does NOT mean Hero Lv.10.

### D17.3 — What Chest Level affects
Chest Level is allowed to affect:
1. Item Level → Item Level Range
2. Rarity → Rarity Weight
3. Affix → Affix Value Range
4. Loot Pool → Available Items

### D17.4 — Chest Definition
Data-driven `ChestDefinition` may contain:
- `id`
- `level`
- `openCost`
- `itemLevelMin`
- `itemLevelMax`
- `rarityTableId`
- `affixTableId`
- `lootTableId`
- `upgradeCost`
- `upgradeDuration`

Unconfirmed values remain configurable/TBD.

### D17.5 — Chest Upgrade
- Chest upgrade costs Gold + time.
- Upgrade timing must be persistent.
- Persist `upgradeStartTime` and `upgradeEndTime`.
- Do NOT use `setTimeout()` as the authoritative source of upgrade state.
- On reopening the game, compare current time against persisted `upgradeEndTime` to determine completion.

### D17.6 — Chest Open Resource
- Opening a Chest requires a resource.
- Abstract this as `ChestOpenCurrency`.
- Do not hard-code a specific resource name into the Chest Engine.
- Possible future names include Chest Ticket, Chest Key, Treasure Token, etc., but these are examples only, not locked values.

### D17.7 — Open Chest Flow
Official flow:
`Click Open → Check Chest Currency → Consume Currency → Roll Loot → Generate Item → Calculate CP → Inventory → Result UI`

- Never generate the Item before validating and consuming the required Chest currency.
- This prevents the exploit where an empty currency balance could still produce loot.

### D17.8 — Loot Roll Pipeline
Loot Engine has four layers:
1. Roll Rarity
2. Select Item Definition
3. Generate Base Stats
4. Generate Affixes

### D17.9 — Rarity Table
- Use a data-driven `RarityTable`.
- The source lists example rarity keys such as Common, Uncommon, Rare, Epic, Legendary, Mythic, Divine.
- Actual rarity weights were NOT confirmed and must remain configurable/TBD.
- Do not use earlier example probabilities as production game data.

### D17.10 — Rarity Weight by Chest Level
- Chest Levels may reference different Rarity Tables.
- Example architecture:
  - Chest Lv.1 → `RarityTable_01`
  - Chest Lv.2 → `RarityTable_02`
  - Chest Lv.3 → `RarityTable_03`
- Designer must be able to rebalance rarity without changing code.

### D17.11 — Item Level Table
- Chest Level determines an Item Level Range through data.
- Example structure contains `chestLevel`, `itemLevelMin`, `itemLevelMax`.
- Actual values are TBD.

### D17.12 — Loot Pool
Each Chest Level may have a `LootPool` containing equipment slots such as:
- Weapon
- Helmet
- Armor
- Boots
- Gloves
- Belt
- Necklace
- Ring
- etc.

- Slot probabilities may be configurable.

### D17.13 — Duplicate Items
- No rule limiting the player to one item of a given type.
- Each chest opening may create a separate `ItemInstance` with different stats.

### D17.14 — Inventory Overflow
- Inventory-full handling must not silently destroy an Item.
- Exact final overflow policy was not locked in this source and remains to be decided.

### D17.15 — Recycle / Economy
- Keep vs Recycle is part of the Chest/Economy loop.
- Recycle returns Gold + EXP in the design flow.
- Equipped Items must not be recycled.
- Locked Items must not be Auto-Recycled.
- Auto-Recycle may process eligible Items.
- The exact Gold/EXP recycle values remain TBD.

### D17.16 — Economy Exploit Prevention
- Economy must be balanced across:
  - Average Loot Value
  - Chest Open Cost
  - Recycle Value
  - Chest Upgrade Cost
- Avoid a loop where `Recycle Value > Open Cost` creates infinite Gold.
- Expected recycle value can be evaluated against Chest Open Cost when real data exists.

### D17.17 — Server/Client Architecture
If multiplayer is implemented later:
`Client → Request Open Chest → Server validates Currency → Consume Currency → Generate Loot → Save → Return Result`

- Client must not authoritatively decide loot.
- For offline demo/prototype, Game Logic + Local Save is sufficient.

### D17.18 — Random Provider
- Loot Engine must use a `RandomProvider` abstraction rather than direct random calls throughout code.
- Intended implementations can include ProductionRandom, DebugRandom, ReplayRandom, TestRandom.
- This supports deterministic testing and large-scale probability testing.

### D17.19 — Developer Debug Chest
Developer Mode may expose:
- Open Chest
- Force Rarity
- Force Item
- Give Gold
- Give Currency
- Set Chest Level

- Debug controls are Development Build only and must not exist in Production.

### D17.20 — Chest State Machine
Chest states:
- `IDLE`
- `UPGRADING`
- `READY`
- `OPENING`

- If the design permits only one upgrade at a time, `UPGRADING` must not simultaneously allow another `UPGRADE AGAIN` operation.

### D17.21 — Locked vs TBD
LOCKED:
- Chest has Level.
- Chest Level affects Loot quality.
- Chest Upgrade uses Gold + time.
- Opening Chest consumes a resource.
- Loot is generated when opened.
- Rarity is rolled first.
- Item is a separate Item Instance.
- Affixes are generated at Drop.
- Auto-Recycle can process eligible Items.
- Equipped Item cannot be recycled.
- Locked Item cannot be Auto-Recycled.
- Random is abstracted through RandomProvider.
- Chest/Item/Loot are data-driven.
- Unconfirmed Loot rates must not be hard-coded.

NOT LOCKED / TBD in the recovered source:
- Inventory maximum slots.
- Exact Chest Open cost.
- Single-open vs 10/100-open batch size.
- Free opening rules.
- Whether Chest Upgrade can queue multiple levels.
- Whether Gold can skip upgrade time.
- Inventory-full behavior.
- Exact Gold/EXP from Recycle.
- Actual Rarity probabilities.
- Actual Loot Pool.

---

## D18 — STAGE / NORMAL MONSTER / BOSS / PROGRESSION

### D18.1 — PvE Structure
Two primary PvE battle types:
- Normal Monster
  - No time limit.
  - Continuous combat.
  - Increases Progress %.
- Boss
  - Separate Boss Arena.
  - Max Time = 60s.
  - Uses a dedicated Boss Entity/configuration.
  - Win → progress to next Stage.

### D18.2 — Normal Monster Flow
`Stage → Normal Battle → Spawn Monster → Kill Monster → Progress + → Spawn Next Monster → ... → 100% → Boss Gate/Transition`

- Normal Monster combat has no 60-second limit.

### D18.3 — Progress Meter
- Current locked example: **50 Normal Monsters = 100%**.
- Each monster death increments progress by 1.
- Current percentage model: killed monster count / required monster count.

### D18.4 — Required Monster Count must be Data-Driven
- Although the current design uses 50 monsters, code must not scatter hard-coded `if (killCount >= 50)` checks.
- Use `stage.monstersRequired`.
- Example: Stage 1 → 50, Stage 2 → 50, Stage 3 → 60.
- Example values beyond the currently confirmed value are configuration examples, not universal production values.

### D18.5 — Monster Death → Progress → Spawn
On Normal Monster death:
`Monster Death → Reward → Progress++ → Check Progress → <100% ? Spawn Next Monster : Boss Transition`

### D18.6 — Normal Monster Definition
Normal Monster may contain:
- HP
- ATK
- DEF
- Attack Speed
- Movement Speed
- Skills
- Rewards

Boss must have its own configuration and must not simply be a renamed Monster object.

### D18.7 — Movement / Targeting
- Normal Battle may include movement; Hero and Monster are not required to remain stationary.
- Combat Movement System must remain independent from Damage System.
- Targeting uses the previously confirmed rule:
  1. Nearest Target
  2. Hero priority when distances are equal

### D18.8 — Boss Transition
Current intended flow after the required Normal Monster count is reached:
`Monster final death → Progress = 100% → Stop Normal Spawn → Clear Normal Battle → Boss Transition → Boss Arena`

- Do not spawn the next normal monster after reaching 100%.

### D18.9 — Boss Arena
- Boss has a separate combat space from Normal Monster combat.
- `NormalArena` and `BossArena` are separate configurations.
- Suggested data-driven `ArenaDefinition` fields include:
  - id
  - scene
  - bounds
  - spawnPoints
  - camera
  - background
  - movementRules

### D18.10 — Boss Battle Timer
- Current locked Boss Max Time = **60 seconds**.
- Flow:
  `Boss Start → Timer 60 → Combat → Boss HP <= 0 ? WIN : Timer <= 0 ? LOSE : Continue`

### D18.11 — Hero Death
- Hero death means immediate loss.
- `Hero.HP <= 0 → BattleResult.LOSE` immediately.
- Do not wait for animation, Companion, or timer completion.

### D18.12 — Companion on Hero Death
- When Hero dies, Companion immediately stops/vanishes/stops animation and AI.
- Battle ends as Lose.

### D18.13 — Companion Death
- Companion death does NOT cause battle loss.
- Companion flow:
  `Alive → Dead → Respawn Timer 25s → Respawn`.
- Respawn value = **25s**, configurable.

### D18.14 — Boss Stats
Boss may have:
- HP
- ATK
- DEF
- Attack Speed
- Movement Speed
- Skills
- Rage
- optionally Crit, Dodge, Counter, Combo, Lifesteal, Stun Resistance when configured.

- These values must be data-driven.

### D18.15 — Boss Level
- Do not hard-code Boss Level as a formula such as `PlayerLevel × 10`.
- Use `BossDefinition` and `BossStatProfile` for configuration.

### D18.16 — Stage Definition
`StageDefinition` may contain:
- `stageId`
- `chapterId`
- `stageLevel`
- `normalMonsterPool`
- `monstersRequired`
- `bossId`
- `bossArenaId`
- `normalRewards`
- `bossRewards`
- `unlockRequirement`

### D18.17 — Stage Flow
`Stage N → Normal Monsters → required kills → Boss → WIN? → Stage N+1 / Retry`

### D18.18 — Boss Win
When `Boss.HP <= 0`:
`Battle Result → WIN → Calculate Rewards → Save Progress → Unlock Next Stage → Show Result`

### D18.19 — Boss Lose
Boss Lose occurs when:
- Timer reaches 60s, or
- Hero HP <= 0.

Result = LOSE.
The exact post-loss behavior was not fully locked in this historical source.

### D18.20 — Normal Monster Rewards vs Boss Rewards
Separate `MonsterReward` from `BossReward`.

Normal Monster may provide:
- Gold
- EXP
- Progress

Boss may provide:
- Gold
- EXP
- Chest Currency
- Items
- Special Rewards

Exact quantities remain TBD.

### D18.21 — Normal Monster Equipment Drop Isolation
- Normal Monster may have direct equipment drops only through a separate Loot Table.
- Direct Monster equipment drops must not be mixed into the Chest System.
- The main progression loop remains Combat → Resources → Chest → Equipment.

### D18.22 — Boss Reward Table
Use data-driven `BossRewardTable`, potentially containing:
- Gold
- EXP
- ChestCurrency
- Item
- Special

- Exact rates and quantities remain TBD.

### D18.23 — Stage Progress Save
Do not save only a single `currentStage` value.
Use a `PlayerProgress` structure containing at least:
- chapter
- currentStage
- highestStage
- unlockedStages
- bossProgress

### D18.24 — Exit During Normal Battle
- Exiting during Normal Battle is NOT automatically treated as Battle Lose.
- Offline Progress is responsible for handling this later.
- The detailed Offline behavior was explicitly left for the Idle System and is therefore governed by D19 once locked.

### D18.25 — Exit During Boss Battle
- The historical source explicitly left this unresolved.
- Two candidate approaches were recorded:
  - A: Boss resets / exit counts as Boss Battle Lost.
  - B: Save Boss Battle state and resume on reopen.
- A/B was NOT selected in D18 and must not be treated as a locked rule from this document.

### D18.26 — Battle Instance
Each battle should have a `BattleInstance` containing:
- battleId
- stageId
- battleType
- arenaId
- elapsedTime
- entities
- result
- rewards

Battle types:
- NORMAL
- BOSS
- PVP

### D18.27 — Battle Type Architecture
Architecture:
- `NormalBattle`
- `BossBattle`
- `PvPBattle`

Use one shared `BattleEngine` rather than copying the entire Combat Engine.
Differences are represented by `BattleRules`.

### D18.28 — Battle Rules
Example architecture:
- `NormalBattleRules`
  - maxTime = Infinity
  - monsterProgress = enabled
  - boss = disabled
- `BossBattleRules`
  - maxTime = 60
  - monsterProgress = disabled
  - boss = enabled
- `PvPRules`
  - maxTime = TBD
  - matchmaking = TBD/implementation-specific
  - rewards = TBD

The `Infinity`/60 values above follow the recovered D18 design; PvP values remain TBD.

### D18.29 — D18 Locked Parameters
- Normal Monster required: **50 currently locked**, but code must use configurable `stage.monstersRequired`.
- Progress: **100% / 50 monsters currently locked**.
- Boss Max Time: **60s**.
- Hero death: **Immediate Lose**.
- Companion death: **Respawn**, not battle loss.
- Companion Respawn: **25s**, configurable.
- Normal Monster Time Limit: **None**.
- Boss Arena: **Separate**.
- Target: **Nearest → Hero when equal distance**.
- At Progress 100%: show/enter Boss Gate flow; current recovered design keeps Progress at 100% until Boss is won.
- Boss loss does NOT roll back Normal Monster rewards.
- Boss win gives Boss Rewards and advances to next Stage.
- Next Stage starts at Progress 0/50 in the recovered example flow.

### D18.30 — Important Unresolved Items
Do NOT invent these from D18:
- Exact post-loss flow beyond the recorded Retry/return options.
- Whether Boss state persists when exiting during Boss Battle.
- Exact Normal Monster reward quantities.
- Exact Boss reward quantities/rates.
- PvP rules.

## Reconciliation with D19
D18 originally left Offline handling for the later Idle System. D19 subsequently defines Offline Progress, Bun economy, Boss Gate behavior, and Offline Boss restrictions. Therefore, where D18 says Offline was not yet locked, D19 is the later authoritative decision.

## Current status
- D17: **RECOVERED / LOCKED with explicit TBD list**.
- D18: **RECOVERED / LOCKED with explicit unresolved items**.
- D17/D18 do not authorize inventing values marked TBD or unresolved.
