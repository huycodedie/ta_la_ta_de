# D15–D16 LOCKED — COMPANION / EQUIPMENT & LOOT SYSTEM

> Recovered from the user's historical design source. This document preserves the source decisions and explicitly keeps unresolved balance/configuration values open. It is a design contract, not proof of current implementation.

## D15 — COMPANION / TÙY TÙNG SYSTEM

### D15.1 Model
Companion is an independent Combat Entity. Confirmed properties:
- Has HP.
- Can be attacked.
- Can die.
- Has respawn time.
- Has movement.
- Has animation.
- Has Basic Attack.
- Has Skill/Effect.
- Does not use Hero Rage.
- Each Companion has its own Runtime.

Architecture:
```text
CombatEntity
├── Hero
└── Companion
```

Both share Stats, Health, Movement, Attack, Skill, Status, Target, Animation. Hero additionally has Rage/Ultimate/PlayerControl; Companion has Respawn/Owner/CompanionAI.

### D15.2 Count
Current maximum is 5 Companions, but Combat Engine must not hard-code 5. Use configurable `BattleFormation.maxCompanions` or equivalent data-driven configuration. Example unlock levels in the source (1/20/30/40/50) are examples only and are NOT locked.

### D15.3 Definition / Runtime
`CompanionDefinition` separates data from Runtime and may contain id, name, description, model, icon, baseStats, attack, skills, movement, effects, rarity, tags.

`CompanionRuntime` contains runtime state such as CurrentHP, MaxHP, Position, State, Target, AttackTimer, Cooldowns, ActiveEffects, RespawnTimer.

Combat Engine must not branch on specific companion IDs.

### D15.4 HP / Death / Respawn
Companion has MaxHP and CurrentHP. At HP <= 0 it dies.

Death flow:
`ALIVE → DYING → DEAD`

On DEAD: stop attack, skill, movement, and animation; start configurable RespawnTimer. Current confirmed respawn duration is **25 seconds**.

Respawn flow:
`DEAD → RESPawn Timer → RESPAWNING → restore HP → ALIVE`

Respawn HP percentage is configurable; 100% is an example/default from the source, not a general immutable balance rule.

### D15.5 Hero Death
Hero HP <= 0 causes immediate Battle LOST. All Companions stop immediately; Companion combat does not continue after Hero death.

### D15.6 Movement / Formation
Companions can move. Formation defines a desired position around the Hero, not a hard-locked position. Movement Controller may move from Current Position toward Target Position.

Possible states include IDLE, MOVING, ATTACKING, CASTING, STUNNED, DEAD, RESPAWNING.

### D15.7 Targeting
Companion TargetResolver follows the confirmed targeting rule: nearest target; equal distance resolves to Hero priority. Example: nearer Enemy Companion is selected over farther Enemy Hero; at equal distance Enemy Hero wins.

### D15.8 Basic Attack
Historical source specifies Companion Attack Interval = 2s. This is a configurable value and must not be hard-coded in Companion Controller. Companion attack timer is independent from Hero attack timer.

### D15.9 Rage
Companion attacks do not grant Rage to the owning Hero. Companion receiving damage also does not grant Rage to the owning Hero. Hero Rage sources remain those explicitly defined by the Hero/Rage system; future special effects may define additional sources.

### D15.10 Shared Combat Systems
Companions use the common Damage Engine; no separate Companion damage formula. Prototype formula preserved from source:
`Damage = ATK × Multiplier × DefenseModifier`

Companions may independently use CritRate/CritDamage, ComboRate, CounterRate, and Lifesteal when defined by their Definition/configuration. Combo/Counter must obey the existing anti-recursion rule. Lifesteal heals the Companion itself unless an explicit effect such as `HEAL_OWNER` defines another target.

### D15.11 Companion Skills / Effects
Companions may have their own skills without a hard-coded skill count. Skill data can include SkillID, Cooldown, CastTime, TargetType, Damage, Effects. Example cooldowns (5s/12s/25s) are schema examples, not locked balance.

Effects route through the common Effect System and can include damage, crit, CC/debuffs, heal, shield, buffs/debuffs, etc. No duplicate effect engine should be created.

### D15.12 AI / State Machine
Use `CompanionAIController` rather than hard-coded enemy-condition branches. Source flow:
`Update → Check Alive → Check Stun → Check Target → Check Skill → Check Attack → Check Movement`

Example priority:
`DEAD → STUNNED → CASTING → SKILL READY → ATTACK READY → MOVE TO TARGET → IDLE`

Skill priority remains configurable per Companion.

State flow supports combat states and death/respawn transitions. Death can transition from any state to DEAD, then Respawn Timer, RESPAWNING, and back to IDLE/ALIVE.

### D15.13 Stun
Companions can be Stunned unless immune. While Stunned: Movement blocked, Attack Timer paused, Skill Cast blocked. After Stun, resume.

This historical behavior must be interpreted through the later P07.6 shared `EntityStatusController` authority where applicable, rather than creating a second status controller.

### D15.14 Animation / Hit Timing / Projectile
Combat Logic is separated from Animation. Combat may issue commands such as `PlayAnimation("Attack")`; Animation System executes them.

Attack timing can be:
`Attack Command → Attack Animation → Attack Event → Projectile/Hit → Damage`

Example timing in the source is illustrative only.

Companion delivery can be Melee, Projectile, Area, Fixed Position, or Targeted. It must use the common delivery/effect/combat architecture.

### D15.15 Battle Integration
Battle contains:
- PlayerHero
- EnemyHero
- PlayerCompanions[]
- EnemyCompanions[]

Battle tick updates Heroes and both Companion collections, then damage/effects/death/battle-end processing. Companion is not a Hero subclass; both are CombatEntity variants.

---

## D16 — EQUIPMENT & LOOT SYSTEM

### D16.1 Architecture
Loot System consists conceptually of:
- Chest System: Chest Level, Upgrade, Cost, Open.
- Item Generator: Item Level, Slot, Rarity, Base Stats, Random Affixes.
- Equipment System: 12 slots, Equip, Unequip, Compare.
- CP System: Combat Power.
- Auto Management: Auto Equip and Auto Recycle.

All should be data-driven and extensible without modifying Combat Engine for new items/affixes.

### D16.2 Equipment Slots
Current design has exactly 12 technical slots:
`WEAPON, HELMET, ARMOR, BOOTS, GLOVES, BELT, NECKLACE, RING_LEFT, RING_RIGHT, JADE, TALISMAN, SPECIAL`.

`SPECIAL` is explicitly a temporary technical slot name; the final UI display name remains subject to later confirmation against the real game UI.

### D16.3 Rarity
Seven rarity tiers:
`COMMON, UNCOMMON, RARE, EPIC, LEGENDARY, MYTHIC, DIVINE`.

UI colors are associated in the historical source as White/Green/Blue/Purple/Orange/Red/Yellow-Pink, but colors belong to `RarityDefinition`, not hard-coded Item logic.

### D16.4 Item Level / No Stars
Equipment has ItemLevel. Item Level influences stat ranges through data/configuration.

There is no Item Star/upgrade-star system in the current design. Do not add +1/+2/5★ item progression unless separately approved; if added later it should be an independent module.

### D16.5 Definition vs Instance
`ItemDefinition` is the reusable template; `ItemInstance` is the concrete generated item. ItemInstance contains a unique instanceId, definitionId, generated stats/affixes, combat power, createdAt or equivalent provenance. Multiple instances may share one Definition while having different generated rolls.

### D16.6 Affix Generation
Affixes are generated immediately when the chest is opened. There is no separate Identify stage.

Flow:
`Open Chest → Generate Item → Generate Base Stats → Generate Affixes → Calculate CP → Item Complete`

`AffixDefinition` must be data-driven rather than hard-coded string checks. Example affixes in source include Crit Rate, Crit Damage, Combo Rate, Counter Rate, Dodge, Lifesteal, Stun Resist; future affixes remain extensible.

### D16.7 Affix Count / Range
Affix count is controlled by `RarityDefinition` (`minAffixes`, `maxAffixes`), not hard-coded in the Generator.

The historical example says White/Green/Blue/Purple have Base Stats and Orange/Red/Divine have 1–2 Special Affixes. This is preserved as a source example/design direction, but the Generator must remain data-driven.

Each AffixDefinition has minValue/maxValue or equivalent data-driven range. The source example Crit Rate 1.0%–3.5% is illustrative, not locked balance.

### D16.8 Base Stats
Items can provide HP, ATK, DEF and may later provide Attack Speed, Movement Speed, or other stats if separately designed.

### D16.9 Build-Aware Item Evaluation
Item evaluation must not reduce entirely to CP. A lower-CP item may be more useful for a Crit/Combo/Lifesteal build.

Auto Recycle must therefore support CP + Rarity + Affix + Player Filter, rather than blindly deleting every lower-CP item.

### D16.10 Auto Equip / Compare
Basic Auto Equip rule: `NewItem.CP > EquippedItem.CP` suggests replacement. It is not a full Build Advisor unless additional advanced conditions are enabled.

Equipment comparison displays per-stat deltas and CP delta; UI remains separate from Item Generator.

### D16.11 Preferred Affixes
Player can configure Preferred Affixes such as Crit, Combo, Lifesteal. If an item has a preferred affix, Auto Recycle may preserve it even when CP is lower, according to the configured filter rules.

### D16.12 Auto Recycle / Rule Engine
Auto Recycle supports conditions over Rarity, CP, Affix, Slot, Item Level and player preferences.

The historical architecture proposes `FilterCondition` and `FilterGroup` with AND/OR composition, e.g. `CP < EquippedCP AND Rarity <= EPIC`, or OR conditions for preferred affix thresholds. This is an extensibility requirement, not permission to invent additional gameplay conditions.

### D16.13 Recycle Reward
Recycling returns Gold. The source also describes Cultivation EXP as a possible recycle result, but the exact reward formula/amounts are **not locked**. Do not invent them.

### D16.14 Chest System
Chest is the central Loot Loop:
`Resources → Chest → Open → Random Item → Equip / Keep / Recycle`

Chest has ChestLevel. Chest Level affects Loot, including Rarity Probability, Item Level, and Stat Range through data-driven tables.

### D16.15 Chest Upgrade
Chest upgrade consumes Gold and has an upgrade cost. The source records one observed runtime value: **1,700,000 Gold** upgrade cost while current Gold was **974,470**, so that observed state was insufficient to upgrade. This is evidence of a runtime/config value, not a universal hard-coded cost or formula.

Upgrade Cost, Time, and future level progression belong in `ChestLevelDefinition` or equivalent configuration. Do not infer a cost formula.

### D16.16 Chest Open Flow
Required conceptual flow:
`Check Resource → Consume Resource → Roll Rarity → Roll Item Definition → Generate Item Level → Generate Base Stats → Generate Affixes → Calculate CP → Auto Management → Display Result`

Resource consumption must occur before loot generation to avoid free-loot duplication/exploit behavior.

### D16.17 Rarity / Weighted Random / Loot Table
Rarity uses a weighted table rather than `random(1,7)`. Chest Level may select a different rarity-weight table.

`LootTable → Rarity → Weight → Item Pool` is the intended structure. Example rarity percentages in the source are explicitly architecture examples, not real game rates.

### D16.18 UI Separation
Item Generator creates ItemInstance only. UI decides icon, rarity presentation, animation, popup, and comparison window. Loot/Item generation must not depend on UI code.

### D16.19 CP
Item CP and Hero CP are distinct.

General Item CP model:
`CP = Σ(Stat × CPWeight)`

CP weights are data-driven via a `CPWeightDefinition` or equivalent. Do not hard-code formulas such as `ATK*5 + HP*0.1`.

Hero CP is based on final Hero Stats and is separate from an individual Item's CP.

### D16.20 Equipment Apply / Unequip
Equip flow:
`ItemInstance → EquipmentSlot → StatModifier → Hero Runtime Stats → Recalculate CP`

Do not directly mutate `Hero.ATK += Item.ATK`; use an equipment/stat modifier provider and recalculate.

Unequip removes the modifier, recalculates stats/CP, and keeps the ItemInstance.

### D16.21 Inventory
Inventory contains ItemInstance collection and supports Add, Remove, Equip, Unequip, Recycle, Sort, Filter, Compare.

Sort/filter examples include Rarity, CP, Item Level, Slot, Affix, Newest. These are UI/data operations, not reasons to hard-code item-specific logic.

### D16.22 Persistence
Persistent player data includes concepts such as Equipment, Inventory, Gold, Chest, Settings. Runtime-only combat state such as AttackTimer and CurrentTarget should not be treated as ordinary persistent save data.

### D16.23 Locked vs TBD
Confirmed design direction:
- 12 equipment slots (with `SPECIAL` technical name still provisional for final UI naming).
- 7 rarity tiers.
- Item Level.
- No Item Star system currently.
- Affixes generated at chest opening.
- High-rarity items can have special affixes.
- CP uses data-driven stat weights.
- Basic Auto Equip is CP comparison.
- Preferred Affixes can protect useful build items from Auto Recycle.
- Auto Recycle is rule/filter based.
- Recycle returns Gold.
- Chest has Level and affects Loot.
- Chest Upgrade uses Gold and has cost/time configuration.
- Inventory stores ItemInstances.

Explicitly unresolved and must remain open until sourced/approved:
1. Exact rarity drop rates.
2. Chest upgrade cost progression/formula.
3. Chest upgrade time.
4. Chest opening cost(s).
5. Exact Gold recycle reward.
6. Exact recycle EXP reward, if used.
7. Final UI names for all 12 slots, especially SPECIAL.
8. Exact min/max for each affix.
9. Exact CP weights.
10. Item Level → stat formula/ranges.
11. Loot Pool per Chest Level.

## Precedence / Later Decisions
This recovery is a historical design contract. Later explicitly LOCKED decisions from P07.x and later user-approved changes take precedence when they are more specific. In particular, Companion status/control behavior should use the shared P07.6+ `EntityStatusController`/EffectResolver architecture rather than creating duplicate systems.

The document does not claim that all D15/D16 details are currently implemented or visually verified in Unity.