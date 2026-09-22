# D12–D14 LOCKED DESIGN RECOVERY

> Source: recovered historical design supplied by the user.
> Status: RECOVERED / LOCKED DESIGN CONTRACT, subject to later explicit superseding decisions.
> Rule: do not invent unresolved values. Historical example/test values are not automatically permanent balance unless explicitly locked later.

## D12 — DAMAGE & EFFECT ENGINE

### D12.1 Architecture
All damage goes through a common Combat Resolver / Hit Calculation / Damage Calculation / Effect Processing / Combat Events pipeline. Individual Skills must not implement their own damage formulas. The architecture must remain extensible for equipment, martial arts, mind methods, Skill Creator, buffs, debuffs, bosses and PvP.

### D12.2 AttackContext
Each damage attempt creates an AttackContext containing Attacker, Target, AttackType, SkillID, BaseDamage, Multiplier, CanCrit, CanDodge, CanCounter, CanCombo, CanLifesteal and Source.

### D12.3 Attack Types
Defined attack categories:
- BASIC_ATTACK
- SKILL
- ULTIMATE
- COMPANION_ATTACK
- COMPANION_SKILL
- COUNTER_ATTACK
- COMBO_ATTACK
- THALISMAN / SPECIAL

Do not reduce the system to a single `isSkill` flag.

### D12.4 Damage
Prototype formula:
`Damage = ATK × Multiplier × DefenseModifier`
with RawDamage = Attacker.ATK × Skill.Multiplier, followed by DefenseModifier.

### D12.5 Defense
Prototype default:
`DefenseModifier = 100 / (100 + DEF)`
This is explicitly a Prototype default, not permanently hard-coded game balance. Future formula types may include CLASSIC, PERCENT_REDUCTION and CUSTOM.

### D12.6 Dodge
Dodge is resolved before damage. A successful dodge produces MISS and zero damage. Counter does not activate on a miss under the stated rule that Counter requires the target to actually be hit.

### D12.7–D12.8 Rage
Hero Basic Attack grants Rage even when the attack misses. Therefore Basic Attack completion / attack-completed processing grants the configured basic-attack Rage before/independently of dodge/damage resolution.

Current historical defaults:
- Rage Basic Attack = 1
- Rage On Damage = 1

Hero gains Rage on damage only when Hero actually receives damage > 0.

### D12.9–D12.10 Crit
Historical defaults:
- Crit Rate = 1%
- Crit Damage = 1%

CritDamage is additive bonus over 100% base damage: `CritMultiplier = 1 + CritDamage`. Thus 1% Crit Damage = 1.01×; 50% = 1.50×.

### D12.11–D12.15 Combo
Historical default Combo Rate = 1%. Combo is triggered only from BASIC_ATTACK. A Combo attack cannot trigger another Combo (`CanTriggerCombo=false`). Combo has its own Crit roll and its own Dodge roll. Combo can Lifesteal from actual damage; a miss gives zero Lifesteal.

### D12.16–D12.18 Counter
Historical default Counter Rate = 1%. Counter requires the original attack to actually hit, then rolls Counter. Counter attacks cannot trigger another Counter and cannot trigger Combo (`CanCounter=false`, `CanCombo=false`).

### D12.19 Ultimate
Ultimate historical rules:
- Rage Cost = 80
- CanCrit = true
- CanDodge is skill-dependent
- CanCombo = false
- CanTriggerCounter = false

`CanDodge` must not be hard-coded globally.

### D12.20 Targeted Skill
Targeted Skills lock their target at cast. Under the recovered rule they cannot be normally dodged, therefore CanDodge=false unless the Skill has a special exception.

### D12.21 Fixed Position
Fixed Position Skills resolve to the cast coordinate and do not follow targets. An entity leaving the area is not damaged by that fixed-position instance.

### D12.22 Melee
Melee Skill checks attacker-target distance against Skill.Range. Out-of-range means no hit/damage.

### D12.23 Area
Area Skills resolve a center/shape and entities inside the area. Planned shapes include Circle, Cone, Rectangle and Line. This is intended to be reusable by Skill Creator.

### D12.24 Lifesteal
Historical default Lifesteal = 0%. Lifesteal uses actual damage dealt, not raw damage. If only 100 damage is actually dealt from a 1000 raw result and Lifesteal is 10%, heal is 10, not 100.

### D12.25–D12.26 Combat Feedback
Combat emits DamageEvent data; UI renders NORMAL/CRIT/MISS/HEAL etc. Lifesteal numbers are displayed on the Hero side; damage numbers appear on the target side. Damage colors are selected by UI from DamageType rather than hard-coded in Combat Engine. Historical examples include NORMAL, CRIT, TRUE_DAMAGE, REFLECT, POISON.

### D12.27–D12.28 Stun
Historical prototype Stun duration is 0.5–1s depending on Skill. While stunned: attack timer paused, skill cast blocked, movement blocked. On the first frame Stun ends, check Rage; if Rage >= 80, Ultimate is prioritized, otherwise resume attack timer.

> Later P07.x status architecture provides the more specific implementation authority for CC/status behavior; this recovered D12 rule records the design intent and does not duplicate that runtime authority.

### D12.29–D12.31 Buff/Debuff and Stats
Use a generic StatusEffect rather than separate hard-coded systems for each buff type. StatusEffect includes Type, Value, Duration, Stack, Source and Modifier. Stats are queried through a central StatSystem (e.g. GetFinalStat) rather than direct fields. Current listed stats: ATK, DEF, HP, AttackSpeed, CritRate, CritDamage, ComboRate, CounterRate, Dodge, Lifesteal. Stat contributions remain distinguishable as Base, Permanent, Equipment and Temporary for debugging/extensibility.

Example historical calculation: Base Crit 1% + Equipment 10% + Skill Buff 20% = 31%.

### D12.32 Events
Recovered event concepts:
- AttackStarted
- AttackCompleted
- AttackHit
- AttackMissed
- DamageCalculated
- DamageApplied
- CriticalHit
- ComboTriggered
- CounterTriggered
- HealApplied
- StunApplied
- StunEnded
- RageChanged
- EntityDied
- EntityRespawned
- SkillStarted
- SkillCompleted

### D12.33 Processing Order
The historical source proposes this Prototype Combat Engine order:
1. Attack Start
2. Animation
3. Attack Completed
4. On-Attack effects
5. Target Resolution
6. Dodge Check
7. Damage Calculation
8. Crit Check
9. Apply Damage
10. Lifesteal
11. Target Rage
12. Counter Check
13. Combo Check
14. Status Effects
15. Death Check

Combo and Counter must never recurse infinitely.

> Implementation note: later P07.x architecture has become more specific for status/shield/damage authority. This list is preserved as recovered D12 sequencing intent; later explicit locked runtime decisions supersede where they are more specific.

### D12.34–D12.36
The recovered design aims for a generic Martial Arts Idle Combat Engine. The complete prototype scenario includes Basic → Animation → Hit → Damage → Crit → Lifesteal → Combo → Enemy Counter → Enemy Damage → Enemy Rage → Stun → Hero Timer Pause → Rage >= 80 → Ultimate → Enemy Death → Victory. Skill-specific hard-coded combat logic is prohibited.

### D12 unresolved/open areas
The source intentionally leaves exact formulas/implementation for individual effects, Knockback, DOT/HOT, Shield, Immunity, Invincible, Skill upgrades, Skill levels/tree and martial-arts interaction open.

---

## D13 — SKILL SYSTEM + SKILL CREATOR

### D13.1 Architecture
Skill is DATA; Combat Engine is EXECUTION.
`Skill Definition → Skill Runtime → Target Resolver → Effect Resolver → Combat Engine`
No `if skillId == ...` hard-coded skill implementations.

### D13.2 Skill Definition
Skill data contains identity, description/icon, animation reference, Skill Type, Target Type, Range, Area, Damage, Effects, Cost, Cooldown, Cast Time, Priority and Conditions.

### D13.3 Hero action slots
Hero currently has up to six combat action categories:
- Basic Attack
- Skill 1
- Skill 2
- Skill 3
- Skill 4
- Ultimate

This resolves the action-count intent in the recovered D13 source more clearly than the older D6 wording that had an internal “5 groups” inconsistency. The D13 source explicitly states six Hero action categories.

### D13.4–D13.5 Target Types
Supported target concepts:
- TARGETED: locks target; no normal Dodge.
- FIXED_POSITION: cast at X/Y coordinate; does not follow target.
- MELEE: close-range requirement.
- AREA: affects an area.

Target concepts may be combinable, e.g. Targeted + Area, Fixed Position + Area, Melee + Area.

### D13.6–D13.7 Multi-hit Damage
A Skill may contain multiple damage hits, each with its own delay and multiplier. Skill System specifies damage events; Combat Engine calculates final damage.

### D13.8–D13.10 Effects and Buff timing
A Skill may apply multiple effects including Damage, Stun, Slow, Heal, Buff and Debuff. Generic Effect types in the recovered design include DAMAGE, HEAL, STUN, SLOW, BUFF, DEBUFF, KNOCKBACK, DOT, HOT, SHIELD and CUSTOM, with future examples such as POISON, BLEED, BURN, SILENCE, IMMUNITY, INVINCIBLE and ROOT.

Applying a timed Buff does not block the next Skill until the Buff expires. Skill completion and Buff duration are separate concepts.

### D13.11–D13.12 Cooldown
Historical example CD values:
- Skill 1 = 1s
- Skill 2 = 5s
- Skill 3 = 12s
- Skill 4 = 25–40s
- Ultimate uses Rage rather than ordinary cooldown.

Each runtime Skill tracks CurrentCooldown and MaxCooldown; Ready when CurrentCooldown <= 0.

These values remain historical examples unless later explicitly promoted to locked balance.

### D13.13–D13.15 Priority and Auto/Manual
Skill Priority is data-driven. Historical example ordering: Ultimate 100, Skill 4 80, Skill 3 60, Skill 2 40, Skill 1 20, Basic 0. Auto mode finds available actions, checks conditions/cooldown/rage, sorts by Priority and executes the highest-priority valid action. Manual mode disables automatic Skill selection; player commands are still validated for cooldown, Rage, range, target and status.

### D13.16 Skill State
Skill states:
READY → CASTING → EXECUTING → COMPLETED, with CANCELLED as an alternative terminal state.

### D13.17 Animation
Skill Definition stores an AnimationId/reference rather than embedding animation execution code. One Skill may later support multiple animations.

### D13.18–D13.19 Projectile
Projectile is optional. Default historical design is Projectile=false for typical martial-arts skills, while the architecture supports Projectile=true with ProjectileSpeed for future ranged skills such as sword energy. The historical example speed 15 is not locked balance.

### D13.20–D13.21 Conditions
Skill Conditions may inspect Rage, HP, Target Distance, Target Status and Self Buffs. A Skill may alter behavior based on conditions, e.g. higher damage below an HP threshold. Exact condition vocabulary/evaluation is intentionally extensible.

### D13.22–D13.25 Skill Creator
Skill Creator is an Editor/data authoring tool, not combat logic. Prototype workflow:
Create → Save → Load → Test.
It should expose Skill ID/name/description, Skill Type, Target Type, cooldown, Rage Cost, Priority, Animation, damage hits, effects, conditions, and actions such as TEST/SAVE/DUPLICATE. Effects can define chance/duration/value. Multiple damage events can define Delay, Multiplier and Damage Type. Preview should initially remain simple: Hero vs Dummy with ATK, Damage, Crit, Effect and Cooldown visible.

### D13.26 Data persistence
Skill Creator exports independent Skill data. Historical examples use JSON files such as `skill_001.json`, but database storage is also acceptable later. Skill must not be hard-coded in UI.

### D13.27 Versioning
Skill data should include `schemaVersion`, with future migration support (e.g. schemaVersion 1 → 2).

### D13.28 Schema
Conceptual SkillDefinition structure includes identity, classification, timing, resource, targeting, animation/projectile, damage[], effects[], conditions[], priority and tags[].

### D13.29 Tags
Historical tags include PHYSICAL, MAGIC, MELEE, AREA, CONTROL, BURST, HEAL, BUFF, DEBUFF, ULTIMATE. Tags are intended to let future systems such as martial arts modify groups of Skills without depending on Skill IDs.

### D13.30 Design goal
Skill Creator must remain generic enough for Basic Attack, sword energy, palm techniques, poison, burn, stun, slow, lifesteal, buffs, debuffs, Area, Projectile, Melee, Fixed Position, Targeted, Multi-hit, DOT, Heal, Shield, Immunity and Invincible without rewriting Combat Engine.

### D13 locked vs open
Locked/recovered design: six Hero action categories; Skill 1–4 cooldown concept; Ultimate uses Rage 80; Priority; Auto/Manual; Targeted/Fixed Position/Melee/Area; multi-hit; generic Buff/Debuff; Stun; optional Projectile; Conditions; Skill Creator; no Skill hard-coding in Combat Engine.

Open by explicit source: actual animation implementation, formulas for individual Effects, Knockback, DOT/HOT, Shield, Immunity, Invincible, Skill upgrade/level/tree and martial-arts effects on Skills.

---

## D14 — HERO / CHARACTER SYSTEM

### D14.1 Architecture
Hero is a combat entity assembled from modular components rather than a monolithic class. Conceptual modules:
Identity, Base Stats, Runtime Stats, Resources (HP/Rage), Movement, Attack Controller, Skill Controller, Status Controller, Target Controller, Animation Controller and Combat State.

### D14.2 Identity
Hero data includes HeroID, Name, Description, Avatar, Model and AnimationSet. Hero names must not be hard-coded into Combat.

### D14.3 Stats
Base stats include HP, ATK, DEF, Attack Speed and Movement Speed. Combat stats include Crit Rate, Crit Damage, Combo Rate, Counter Rate, Dodge and Lifesteal.

### D14.4 Runtime Stats
Final stats are assembled from Base + Equipment + Permanent Modifier + Temporary Modifier + other future systems. Hero should query final stats through the shared stat architecture.

### D14.5–D14.8 HP and Rage
Hero death occurs at CurrentHP <= 0 and means immediate defeat in ordinary combat. Hero does not respawn during normal combat.

Rage:
- MaxRage = 100
- Ultimate threshold/cost = 80 Rage
- Basic Attack Rage = 1
- Rage On Damage = 1
- Companion attacks do not grant Hero Rage
- Rage is clamped to MaxRage

Historical source states Combo does not automatically add extra Rage absent a separate rule.

### D14.9–D14.10 Attack Speed
Historical Base Attack Interval = 1.5s, explicitly treated as a base/configurable value rather than hard-coded balance. Attack Speed should be represented as a stat; exact AttackSpeed→AttackInterval formula remains open and should be data/config driven.

### D14.11 Basic Attack flow
Attack Timer → Ready → Target Check → Animation → Attack Completed → Rage +1 → Hit Resolution → Damage → Lifesteal → Counter Check → Combo Check → Reset Timer.

### D14.12 Basic Attack options
Hero supports a BasicAttackSlot and may select between two Basic Attack options. Historical examples: Basic A 1.5s, Basic B 1.0s. These are examples unless later locked as balance.

### D14.13–D14.17 Movement
Hero supports Position, Movement Speed, Direction and Movement State. Auto Movement moves toward target when target is too far, then stops/adjusts at attack range. Manual Movement should route through Input → Movement Controller → Movement Command → Character rather than direct Transform manipulation.

Movement architecture must support Move, Jump and Dash as abilities, but not every Hero must have Jump/Dash. Exact Jump/Dash behavior remains open.

### D14.18 Target Controller
Current target is managed separately from target selection. TargetResolver architecture supports future rules such as Nearest, Hero Priority, Lowest HP, Highest Threat and Boss. Current recovered rule: nearest target, tie-break Hero priority.

### D14.19–D14.22 Skill Controller and Auto/Manual
SkillController manages Skill 1–4 and Ultimate but does not calculate damage. It checks readiness and requests casts; Combat Engine executes the result. Priority is data-driven. Auto mode finds valid actions and executes highest priority. Manual mode stops automatic Skill selection; Basic Attack may still operate under the combat loop.

### D14.23–D14.26 Combat State and Ultimate
Hero states include IDLE, MOVING, ATTACKING, CASTING, STUNNED, DASHING, JUMPING and DEAD. Higher-priority states prevent incompatible lower-priority actions; DEAD cannot Move/Attack/Cast.

While Stunned: Attack Timer paused, Movement blocked, Skill Cast blocked. On Stun end, if Rage >= 80, Ultimate is prioritized; otherwise resume.

Ultimate can interrupt a Basic Attack when Rage >= 80 and Hero is not Stunned, then spends 80 Rage. Historical D14 states Ultimate casting has CC Immunity.

> Later P07.x Anti-CC/CC implementation is the more specific runtime authority; this D14 record preserves the recovered Hero-level design intent.

### D14.27–D14.28 Death and Companion interaction
When Hero HP reaches zero: Hero becomes DEAD, stops Attack/Skill/Movement, Battle becomes LOSE, and Companions stop immediately. Companion attacks must not continue after Hero death.

### D14.29–D14.31 Hero Definition vs Runtime
HeroDefinition includes id, name, model, baseStats, basicAttackOptions, skills, ultimate, movement and tags. HeroRuntime contains CurrentHP, CurrentRage, Position, CurrentTarget, CurrentState, Cooldowns and AttackTimer. Definition is immutable design data during runtime; Runtime contains mutable battle state.

### D14.32 Architecture summary
HERO → STATS / RESOURCES / MOVEMENT → ATTACK SYSTEM + SKILL SYSTEM → COMBAT ENGINE → DAMAGE/EFFECT.

### D14.33 Locked vs open
Recovered/locked design:
- Hero has HP and Rage.
- Max Rage 100; Ultimate uses 80.
- Hero death = immediate loss.
- Historical Base Attack Interval 1.5s as configurable base value.
- Basic Attack can be selected from alternatives.
- Auto and manual movement architecture.
- Move/Jump/Dash architecture.
- Skill 1–4 + Ultimate.
- Auto/Manual Skill.
- Data-driven Skill Priority.
- Stun pauses attack timer and blocks movement/cast; Stun end checks Ultimate priority.
- Historical Ultimate CC Immunity intent.
- Hero death stops Companions.

Explicitly open/unresolved in the source:
- Number/types of Heroes.
- Class/system/element.
- Hero Level/EXP within this D14 contract.
- Star upgrade.
- Skins.
- Additional base stats beyond those listed.
- Movement Speed progression.
- Exact AttackSpeed→AttackInterval formula.
- Exact Jump/Dash behavior.
- Knockback behavior.

## Cross-milestone precedence
This file records recovered D12–D14 source. Later explicitly locked P07.x architecture is more specific for implementation authorities such as unified status handling, damage pipeline integration, shields and CC/resistance. It supersedes older implementation mechanics where explicitly stated, without erasing the underlying D12–D14 design intent.
