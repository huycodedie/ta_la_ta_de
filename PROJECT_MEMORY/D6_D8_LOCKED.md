# D6-D8 RECOVERED DESIGN CONTRACT

## Purpose
This file records the D6-D8 design source recovered from the user's historical project notes. It preserves the source terminology and explicit rules. Historical inconsistencies are recorded as unresolved/conflict items rather than silently guessed.

## Preservation / conflict rule
- Do NOT invent missing values or gameplay rules.
- Where the recovered source contains an internal inconsistency, preserve both statements and mark the item for explicit resolution.
- A test/example multiplier is not a permanent game balance value unless explicitly approved.
- Architecture must remain modular and data-driven.

---

# D6 — SKILL SYSTEM V0.1

## D6.1 — Hero action / skill structure
- Historical source defines Hero action groups including Basic Attack, Skill 1, Skill 2, Skill 3, and Ultimate.
- The source also lists Skill 1 through Skill 4 plus Ultimate, while separately describing Basic Attack. Therefore the exact number of UI/action slots is internally inconsistent in this historical source.
- Source explicitly says the architecture must NOT hard-lock slot count; V0.1 may use 4 Active + 1 Ultimate, and UI must be able to increase/decrease slots later without changing the Skill Engine.
- Historical test timings/examples: Basic Attack 1.5s; Skill 1 1s; Skill 2 5s; Skill 3 12s; Skill 4 25–40s; Ultimate Rage 80. These are preserved as source values/examples, not silently promoted beyond what the source establishes.

## D6.2 — Skills must not be hard-coded
- Do not implement `if (skillId == 1)` / `else if (skillId == 2)` gameplay branches.
- Intended pipeline:
  `SkillData -> SkillController -> SkillExecution -> Effect -> Combat System`.
- Adding a new skill should require data/configuration rather than modifying BattleManager.

## D6.3 — SkillData
- Location: `Assets/_Game/Skills/`
- Intended `SkillData.cs` structure:
  - Identity: ID, Name, Description, Icon
  - Timing: Cooldown, CastTime, Priority
  - Resource: RageCost
  - Targeting: TargetType, DeliveryType, Range, Area
  - Damage: Multiplier, CanCrit, CanDodge, CanCounter
  - Animation: AnimationClip, HitTiming
  - Effects[]

## D6.4 — SkillType
- V0.1 types:
  - Active
  - Ultimate
- Future examples mentioned but not required in V0.1: Passive, Triggered, Counter, Transformation.

## D6.5 — SkillDeliveryType
- Historical V0.1 values:
  - Targeted
  - FixedPosition
  - Melee
  - Area
- Future examples: Dash, Jump, Projectile, Self, Chain, Line, Cone.

## D6.6 — SkillTargetType
- Historical values:
  - Self
  - SingleEnemy
  - MultipleEnemies
  - Position
  - AreaAroundSelf
  - AreaAroundTarget

## D6.7 — Targeted Skill
- Flow:
  `Skill Ready -> Find Target -> Lock Target -> Start Animation -> Wait Hit Timing -> Apply Damage`.
- Once a Targeted skill locks its target, it follows that target.
- Example: Enemy A moves after lock; the skill still hits A.
- Exceptions stated by source: target becomes Untargetable or Invulnerable.

## D6.8 — Fixed Position Skill
- Player selects a world position.
- Flow: Cast -> Create Skill Area -> Area Position = Selected Position.
- Fixed-position area does not track moving targets.
- A target leaving the area is not hit by that area.

## D6.9 — Melee Skill
- Melee skill uses skill-specific range.
- Example source value: Range = 2m.
- Target outside the required range is not hit.

## D6.10 — Area Skill
- All entities inside the Area are checked with `IsValidTarget()`.
- Each valid target receives the applicable Effect separately.

## D6.11 — Cooldown
- Each skill has Cooldown and CurrentCooldown.
- On cast: CurrentCooldown = MaxCooldown.
- Time advances: CurrentCooldown -= deltaTime.
- CurrentCooldown <= 0 means Ready.

## D6.12 — Skill Priority
- AI uses skill priority to select ready skills.
- Historical example:
  - Ultimate = 100
  - Skill 4 = 80
  - Skill 3 = 60
  - Skill 2 = 40
  - Skill 1 = 20
  - Basic = 10
- Priority ordering is data-driven; the numeric example is not a permanent balance lock unless separately approved.

## D6.13 — CanUseSkill
- Ready alone is insufficient.
- `CanUseSkill()` must consider conditions including:
  - Cooldown
  - Rage
  - Target
  - Range
  - State
  - Stun
  - Dead
  - Silence
  - Animation
  - Position
- Exact advanced condition behavior must follow later locked systems where applicable.

## D6.14 — Skill State
- Historical states:
  - Ready
  - Casting
  - Executing
  - Recovery
- Flow:
  `READY -> CASTING -> EXECUTING -> RECOVERY -> READY`.
- After skill completion/recovery, search for the next action.

## D6.15 — Buff skill execution vs Buff duration
- A buff skill does not block the next skill until the buff expires.
- Skill Execution Time and Buff Duration are separate concepts.
- Example: Cast Time 0.5s, Buff Duration 10s -> after buff is applied and skill completes, another skill may be cast without waiting 10s.

## D6.16 — Buff System
- Buff has its own duration and effects.
- Example: Frenzy, Duration 5s, +20% AttackSpeed, +10% CritRate.
- Skill Controller should not need to know how long the buff lasts.
- Actual balance values remain data/configuration.

## D6.17 — Rage Skill / Ultimate
- Historical V0.1 Ultimate requirement: Rage >= 80.
- On cast: Rage -= 80.
- Source states Ultimate does not use cooldown.

## D6.18 — Rage Interrupt
- If Rage >= 80 but Hero is Stunned, Ultimate waits until Stun ends.
- On the frame Stun ends, if Rage >= 80, Ultimate may fire immediately rather than waiting for the Basic Attack timer.
- This is consistent with the recovered D5 interrupt/priority concept; later explicit P07.x interrupt rules supersede only where they are more specific.

## D6.19 — Ultimate is a SkillData
- Ultimate is not a separate engine.
- Historical configuration:
  - SkillType = Ultimate
  - RageCost = 80
  - Cooldown = 0
  - Priority = 100
- Skill Engine handles Ultimate through the same skill pipeline.

## D6.20 — Skill Effects
- A skill may contain multiple Effects, e.g. Damage, Stun, Heal, Buff, Debuff, Knockback.
- Example historical skill: Damage 250%, 30% Stun 1s, +10% Lifesteal 5s.
- Future effect examples: RageEffect, TeleportEffect, DashEffect, SummonEffect, ExecuteEffect, ShieldEffect.

## D6.21 — SkillEffect interface
- Intended abstraction: `SkillEffect` with `Execute()`.
- Example implementations: DamageEffect, StunEffect, HealEffect, BuffEffect, DebuffEffect.
- Effects must remain extensible without changing the Skill System core.

## D6.22 — Complete skill example
- Historical example:
  - ID: `SKILL_001`
  - Name: Huyết Sát Chưởng
  - Type: Active
  - CD: 5s
  - Priority: 40
  - Delivery: Targeted
  - Target: SingleEnemy
  - Damage: 250%
  - Crit: Yes
  - Dodge: No
  - Counter: No
  - Effect: Stun 1s / 30%
- Flow: Ready -> Find Target -> Lock Target -> Animation -> Hit -> Damage -> Stun roll -> Cooldown -> Next Action.
- These values are a historical example, not universal balance.

## D6.23 — Auto Mode
- AUTO ON: AI finds the highest-priority ready skill.
- AUTO OFF: Player manually presses skill buttons.
- `CanUseSkill()` still validates manual input.
- Example: pressing Ultimate at Rage 50 must not cast.

## D6.24 — Skill Button
- Skill button displays icon and cooldown/readiness state.
- Ultimate displays Rage requirement/state.
- Exact visual style/colors are not locked here.

## D6.25 — Battle Speed
- Battle speed values in source: x1 = 1.0, x2 = 2.0, x3 = 3.0.
- Systems use shared Battle Time:
  `deltaTime × BattleSpeed`.
- Skill CD, Attack Timer, Stun, Buff Duration and Battle Timer must advance consistently with Battle Speed.

## D6.26 — Animation and Skill impact
- Skill must not directly call damage immediately merely because the skill was selected.
- Intended flow:
  `SkillController -> AnimationController -> Animation Event -> Skill Impact -> Effect Executor`.
- Hit timing belongs to animation/impact timing so combat does not visually desync from damage.

## D6.27 — V0.1 test skills
- Skill 1: CD 1s, Damage 100%, Targeted.
- Skill 2: CD 5s, Damage 200%, Targeted.
- Skill 3: CD 12s, Damage 300%, Melee / Area.
- Skill 4: CD 30s, Damage 500%, Fixed Position.
- Ultimate: Rage 80, Damage 800%, Targeted.
- Source explicitly states these multipliers are TEST values, not official game values.

## D6.28 — Required V0.1 skill test observations
- Skill 1: CD, animation, damage.
- Skill 2: CD, damage.
- Skill 3: Area, damage.
- Skill 4: Fixed Position, Area Damage.
- Ultimate: Rage 80, highest priority, damage.
- AUTO ON -> AI uses skills.
- AUTO OFF -> Player manually uses skills.

## D6.29 — Skill architecture after Phase 6
- BattleManager
  - Targeting
  - Movement
  - Combat
    - Attack
    - Skill
    - Damage
      - Targeting
      - Effects
      - Cooldown
        - Damage
        - Buff
        - Debuff
        - Status
- Architecture intent: skill system remains modular/data-driven and independent from hard-coded skill IDs.

---

# D7 — AI COMBAT + MOVEMENT + TARGETING V0.1

## D7.1 — Combat loop goal
- Battle must autonomously run:
  `Battle Start -> Hero/Companions appear -> AI target selection -> Hero movement -> range -> attack -> skill when ready -> Ultimate when Rage sufficient -> retarget on enemy death -> Hero death = Lose`.
- Source explicitly states:
  - Hero can move left/right.
  - Hero can approach target.
  - Skills may jump/dash where applicable.
  - Companions can move.
  - Target is not always the enemy Hero.
  - Nearest target first; equal distance -> Hero > Companion.
  - Hero death -> immediate loss.
  - Companion death -> respawn after 25s.
  - Hero death -> companions immediately stop animation/combat and disappear/stop fighting.
  - Normal monsters have no time limit.
  - Boss/PvP have a 60s time limit.

## D7.2 — AI architecture
- Do not place full AI logic directly in Hero.
- Intended components:
  - AIController
  - TargetSelector
  - MovementController
  - CombatDecision
  - SkillDecision
  - StateController
- Base flow: AIController -> Target / Decision / Movement -> Skill or Attack.

## D7.3 — Combat State Machine
- Historical states:
  - Idle
  - SearchingTarget
  - Moving
  - Attacking
  - Casting
  - Stunned
  - Dead
  - Victory
  - Defeat
- Future examples mentioned: Knockback, Dashing, Jumping, Invulnerable, Silenced, Frozen.
- Do not hard-code special states into individual skills.

## D7.4 — AI loop priority
- Every frame, source proposes checking:
  Dead -> Stun -> Battle End -> Target -> Ultimate -> Skill -> Basic Attack -> Movement.
- Effective action priority is explicitly:
  `DEAD? -> STUN? -> ULT READY? -> SKILL READY? -> BASIC ATTACK READY? -> MOVEMENT`.
- Skill/attack decision occurs before movement in the action selection sequence.

## D7.5 — Target Selection
- Step 1: collect living and targetable entities (`IsAlive` AND `IsTargetable`).
- Step 2: distance = `abs(attacker.x - target.x)`.
- Step 3: select smallest distance.
- Step 4: if distances are effectively equal, Hero > Companion.

## D7.6 — Targeting example
- If Enemy Pet is closer than Enemy Hero, Hero targets Enemy Pet.
- If Enemy Pet and Enemy Hero are equal distance, Enemy Hero is selected.

## D7.7 — CurrentTarget vs SkillTarget
- `CurrentTarget` is not the same concept as `SkillTarget`.
- A Fixed Position skill may not require a target.
- Area skills may affect multiple entities.
- Keep general AI targeting separate from per-skill targeting.

## D7.8 — Movement System
- AI must not directly modify `transform.position` each frame.
- Use `MovementController` with `MoveTo(target)` and related movement APIs.
- Intended flow: `AI -> MovementController -> Transform`.

## D7.9 — Skill combat range
- Each skill has `RequiredRange`.
- Historical examples: Basic 2, Skill 1 = 8, Skill 2 = 5, Skill 3 = 3.
- A skill can only be used when `distance <= skill.RequiredRange`.

## D7.10 — Hero outside attack range
- If distance > required range, Hero moves toward enemy.
- When distance <= required range, Hero stops and attacks/casts as applicable.

## D7.11 — Ranged skill without approaching
- If a skill's range is sufficient, Hero does not need to close to Basic Attack range.
- Example: Skill 1 range 8, enemy distance 6 -> cast Skill 1 without further movement.

## D7.12 — Movement Decision
- AI evaluates `BestAvailableAction`.
- Example: Ultimate is ready but its range is 15 and enemy is 20 away -> move until within 15, then cast Ultimate.

## D7.13 — Basic Attack Timer
- Hero historical baseline Attack Interval = 1.5s.
- AttackTimer increases with battle delta time.
- When AttackTimer >= AttackInterval -> Basic Attack.
- After attack -> AttackTimer = 0.

## D7.14 — Attack Speed
- Do not overwrite base AttackInterval when buffs change attack speed.
- Intended calculation:
  `FinalAttackInterval = BaseAttackInterval / AttackSpeedMultiplier`.
- Historical example: 1.5 / 1.2 = 1.25s.

## D7.15 — Companion AI
- Companion has Attack Interval 2s in the historical baseline and can move.
- Companion can use TargetSelector, MovementController, CombatDecision.
- Avoid copying all Hero-specific AI; use:
  `BaseCombatAI -> HeroAI / CompanionAI`.

## D7.16 — Companion Death
- On HP <= 0:
  - State = Dead
  - stop Attack
  - stop Skill
  - stop Movement
  - stop Animation
  - start RespawnTimer = 25s

## D7.17 — Companion Respawn
- RespawnTimer decreases with battle delta time.
- At <= 0:
  - HP = MaxHP
  - State = Active
  - entity appears again.
- RespawnTime = 25 is configurable/data-driven; historical examples include 10/20/30/45 as possible tuning values.

## D7.18 — Hero Death
- Hero HP <= 0 -> BattleResult = LOSE immediately.
- Do not wait for Companion respawn, animation, or timer.
- All companions StopCombat() and StopAnimation().

## D7.19 — Battle End
- Normal monsters: no time limit; battle ends when enemy team is dead or Hero is dead.
- Boss: MaxTime = 60s; Boss dead -> Win; Hero dead -> Lose; timer reaches 60 -> Lose.

## D7.20 — Normal Monster -> Boss
- Historical source example: 50 monsters = 100%; each kill +2%.
- Source also states implementation should use `stage.monstersRequired`, not hard-code 50.
- At 100% normal area -> Boss -> separate arena.

## D7.21 — Boss Arena
- Boss uses a separate arena from the Normal Monster area.
- Boss Arena MaxTime = 60s.

## D7.22 — Auto Battle
- AUTO ON: AI controls Movement, Target, Basic, Skill, Ultimate.
- AUTO OFF: AI still handles Movement, Target and Basic Attack; Player controls skills.
- Source proposes future independent toggles: Auto Movement, Auto Basic, Auto Skill, Auto Ultimate. These are proposals/future options, not locked implementation requirements unless separately approved.

## D7.23 — Player Manual Skill
- AUTO OFF flow:
  `Player -> Click Skill -> CanUseSkill() -> Execute`.
- Targeted -> CurrentTarget.
- Fixed Position -> Player selects position.
- Area -> Player selects position.

## D7.24 — AI and Animation
- AI must not apply Damage directly.
- Intended flow:
  `AI -> Decision -> Start Attack -> Animation -> Animation Event -> Hit -> Damage`.

## D7.25 — Projectile
- ProjectileController exists as an extensible system.
- Intended flow:
  `Attack -> Animation -> Spawn Projectile -> Projectile Move -> Collision/Hit -> Damage`.
- Projectile is not mandatory for every martial-arts skill.
- Historical delivery examples: Instant, Melee, Projectile, TargetedFollow, FixedArea.

## D7.26 — Targeted does not imply Projectile
- A Targeted skill may lock a target and apply a sword/slash effect at impact without a projectile object.

## D7.27 — AI Decision Score
- Future concept, not V0.1 requirement:
  `DecisionScore = Priority + TargetDistanceScore + HPConditionScore + RageScore + BuffConditionScore`.
- V0.1 uses Priority -> CanUse -> Range -> Target.

## D7.28 — Debug Overlay
- Recommended prototype debug overlay should show state, target, distance, timers, Rage, skill readiness, Companion HP/state/respawn/target.
- This is a debugging recommendation, not a permanent gameplay UI lock.

## D7.29 — Architecture after Step 7
- BattleManager:
  - BattleTimer
  - BattleSpeed
  - BattleResult
  - TeamManager
  - Hero / Companions
  - TargetSystem
  - MovementSystem
  - SkillSystem
  - DamageSystem
  - StatusSystem
  - AnimationSystem
- Hero includes Stats, Rage, HP, AIController, MovementController, AttackController, SkillController.
- Companions include Stats, HP, RespawnController, AIController, AttackController.

## D7.30 — Required Step 7 result
- Prototype must run a basic battle through Target System -> AI Decision -> Movement/Attack -> Skill -> Damage -> HP/Death/Respawn -> Win/Lose.

---

# D8 — BATTLE SCENE + ENTITY SYSTEM + MOVEMENT PROTOTYPE

## D8.1 — Prototype goal
- Build a functional sample battle, not merely a UI mockup.
- Initial priority is correct combat; visual polish comes later.

## D8.2 — Battle Scene structure
- `BattleScene`
  - `BattleRoot`
  - `BattleManager`
  - `Environment`
    - Background
    - Ground
    - Boundaries
  - `PlayerTeam`
    - Hero
    - Companion_01 ... Companion_05
  - `EnemyTeam`
    - EnemyHero
    - Companion_01 ... Companion_05
  - SkillEffects
  - Projectiles
  - DamageNumbers
  - UI

## D8.3 — Entity System
- Do not create separate core logic for Hero, Enemy, Companion and Boss.
- Create base `BattleEntity` with:
  - Identity
  - Stats
  - HP
  - Movement
  - Combat
  - Target
  - Status
  - Animation
- Derived examples: HeroEntity, CompanionEntity, BossEntity.

## D8.4 — BattleEntity minimum data
- ID
- Name
- Team
- EntityType
- Position
- HP
- MaxHP
- State
- Target
- Historical examples: Hero_001 HP 1000/1000; Boss_001 HP 5000/5000. These are prototype/example values, not permanent universal balance unless separately locked.

## D8.5 — EntityType
- V0.1 values:
  - Hero
  - Companion
  - Boss
- Future examples: Monster, NPC, Summon, Object.

## D8.6 — Team
- `TeamType` values:
  - Player
  - Enemy
- Prefer unified Team data over scattered `isPlayer = true/false` checks.

## D8.7 — Entity State
- Use the state system from Step 7: Idle, Moving, Attacking, Casting, Stunned, Dead.
- Example: Moving -> Target in range -> Attacking -> Animation -> Hit -> Idle/Attack.

## D8.8 — Position System
- Prototype uses horizontal X and vertical Y.
- Historical example: Player Hero X=-6, Enemy Hero X=+6.
- Hero moves through positions until reaching combat range.

## D8.9 — Battle Bounds
- Historical prototype example: MinX=-10, MaxX=+10.
- Entity position is clamped to bounds.
- Future `MapConfig` can define MinX, MaxX, GroundY and SpawnPoints.

## D8.10 — Spawn System
- Do not hard-code Hero at -6 and Enemy at +6.
- Use SpawnPoint data:
  - PlayerHeroSpawn
  - PlayerCompanionSpawn[]
  - EnemyHeroSpawn
  - EnemyCompanionSpawn[]
- Formation positions must remain configurable.

## D8.11 — MovementController
- API examples:
  - MoveTo(position)
  - Stop()
  - MoveLeft()
  - MoveRight()
  - SetSpeed()
- AI delegates movement to MovementController.

## D8.12 — Movement Speed
- Historical prototype value: MovementSpeed = 100.
- Treat as Base Movement Speed, not immutable hard-code.
- Intended:
  `FinalMovementSpeed = BaseMovementSpeed × SpeedMultiplier`.
- Historical example: Base 100, +20% -> 120; after Slow -30% -> 84.

## D8.13 — Movement to target
- Compute `distance = abs(Self.X - Target.X)`.
- If distance > RequiredRange -> move toward target.
- If distance <= RequiredRange -> stop.

## D8.14 — Do not run through entities
- Prototype must address collision/stop distance so Hero does not pass through Enemy.
- Historical example: AttackRange = 2, StopDistance = 1.8.
- Do not silently convert this example into a permanent universal balance rule.

## D8.15 — Companion Formation
- Companions can have formation slots relative to Hero.
- Example slots: Companion 1 through 5.
- Formation may move with Hero.

## D8.16 — Companion still has independent AI
- FormationController must not force Companions to remain stationary.
- During combat, Companion AI can independently choose Target, Movement and Attack.

## D8.17 — Companion Target
- Follow D3 targeting rule: nearest target.
- Equal distance -> Hero > Companion.

## D8.18 — AttackController
- Responsible for AttackTimer, AttackInterval, StartAttack(), OnAttackComplete().
- Historical baseline: Hero 1.5s; Companion 2.0s.

## D8.19 — Basic Attack flow
- AttackTimer Ready -> Find Target -> Target Valid -> Check Range -> Play Attack Animation -> Animation Event -> Hit -> Damage Calculation -> Apply Damage -> Rage -> Crit/Dodge/Lifesteal -> Reset Attack Timer.
- The later P07.x unified damage/status architecture must be respected; no duplicate damage pipeline.

## D8.20 — Rage
- Max Rage = 100.
- Ultimate cost historical baseline = 80.
- Basic Attack Rage gain = 1.
- Taking damage Rage gain = 1.
- Combo does not grant additional Rage for secondary hits under the stated rule.

## D8.21 — RageController
- Intended API:
  - AddRage(amount)
  - RemoveRage(amount)
  - CanUse(amount)
  - GetPercent()
  - Reset()
- SkillController should request Rage validation rather than directly mutating the Rage variable.

## D8.22 — Damage System
- Baseline formula:
  `Damage = ATK × Multiplier × DefenseModifier`.
- Historical example: ATK 100 × multiplier 2.0 × DefenseModifier 0.8 = 160.

## D8.23 — Crit
- Base Crit Rate = 1% in the historical baseline.
- Historical source gives Crit Damage examples: Base Damage 100%, Crit +1%, therefore 101%; then separately uses a 50% Crit Rate example with ×1.5. These examples are not fully normalized into one permanent formula and must not be silently treated as final balance.
- Crit behavior must remain configurable rather than hard-coded.

## D8.24 — Dodge
- Base Dodge = 1% in historical baseline.
- Successful Dodge -> Damage = 0.
- Source states target does not receive Rage On Damage on a dodge.

## D8.25 — Lifesteal
- Base Lifesteal = 0%.
- Example: 10% lifesteal on 200 actual damage -> Heal 20.
- Damage Number and Heal Number are separate presentation events.

## D8.26 — Combat Events
- Suggested/required event concepts include:
  - OnAttackStarted
  - OnAttackHit
  - OnDamage
  - OnCrit
  - OnMiss
  - OnHeal
  - OnStun
  - OnDeath
  - OnRespawn
  - OnSkillCast
  - OnUltimate
- Combat core should not depend on UI presentation.

## D8.27 — Damage Number presentation
- Normal damage, Crit and Miss use distinct visual categories/colors.
- Lifesteal numbers appear on Hero side.
- Other damage numbers appear on enemy side.
- Exact colors were explicitly not locked and should be decided with UI/FX.

## D8.28 — Death
- HP <= 0 -> State = Dead.
- Stop AI, Movement, Attack, Skill, Animation.
- Companion -> start Respawn Timer.
- Hero -> Battle Lose immediately.
- Enemy Hero/Boss -> Battle Win.
- BattleManager remains responsible for final battle result authority.

## D8.29 — BattleManager
- Responsibilities:
  - StartBattle()
  - PauseBattle()
  - ResumeBattle()
  - EndBattle()
  - CheckBattleEnd()
- Hero should not directly set global BattleResult; it emits death/state information and BattleManager decides the result.

## D8.30 — Battle Timer
- Normal Monster: MaxTime = Infinity.
- Boss/PvP: MaxTime = 60s in the historical source.
- BattleManager evaluates BossDead -> WIN, PlayerHeroDead -> LOSE, and timed battle expiration -> LOSE.

## D8.31 — Battle Speed
- UI examples: x1/x2/x3.
- Internal multipliers: 1.0/2.0/3.0.
- All systems use shared `BattleDeltaTime = RealDeltaTime × BattleSpeed`.
- Attack, Skill CD, Stun, Buff, Respawn and Battle Timer must advance consistently.

## D8.32 — Prototype progression
- Initial test: Player Hero vs Enemy Hero.
- Then Player Hero + 2 Companions vs Enemy Hero + 2 Companions.
- Then scale to 5 Companions per side.
- This staged rollout is a prototype/testing strategy, not a permanent team-size restriction.

## D8.33 — Test Scenario 01: Hero vs Hero
- Both Heroes approach each other.
- They stop at attack range.
- They attack every 1.5s under the historical baseline.

## D8.34 — Test Scenario 02: Skill
- Example Skill 1: CD 1s, Range 5.
- Enemy distance 4 -> Hero should cast without unnecessary additional movement.

## D8.35 — Test Scenario 03: Ultimate
- Rage = 80.
- Hero preparing Basic -> Rage reaches 80 -> interrupt Basic -> Ultimate -> Rage becomes 0 under historical example.

## D8.36 — Test Scenario 04: Stun
- Example Attack Timer 1.3/1.5s.
- Apply Stun 1s.
- During Stun, timer remains at 1.3/1.5.
- After Stun, 0.2s remains to attack.
- If Rage >= 80 at Stun end, Ultimate fires immediately.

## D8.37 — Test Scenario 05: Companion Death
- Companion HP = 0 -> Death -> Animation Stop -> AI Stop -> RespawnTimer 25s -> Respawn.

## D8.38 — Test Scenario 06: Hero Death
- Hero HP = 0 -> Hero Death -> BattleManager -> LOSE -> all Companions Stop.
- Companions must not continue attacking after Hero death.

## D8.39 — Test Scenario 07: Boss
- Boss Battle MaxTime 60s.
- Boss alive at 60s -> Lose.
- Boss dies at an earlier time (historical example 37.2s) -> Win.

## D8.40 — Explicitly not included at Step 8
The source explicitly postpones:
- Bách Bảo chest
- Chest Level
- 12 equipment slots
- Rarity
- Random Affix
- Auto Recycle
- Công pháp
- Companion progression
- Boss progression
- Quest
- Idle rewards
- Complete UI menu
- Gacha
- Shop
- PvP matchmaking

Reason: stabilize Battle Core first.

## D8.41 — Historical development order
- Step 8: Battle Scene, Entity, Movement, Basic Combat.
- Step 9: Animation + VFX + Damage Numbers.
- Step 10: Combat UI (HP/Rage/Skill/Auto/x1/x2/x3).
- Step 11: Companion System complete.
- Step 12: Equipment + 12 Slots.
- Step 13: Chest System.
- Step 14: Random Affix + CP.
- Step 15: Công pháp / Buff / Synergy.
- Step 16: Normal Monster -> Boss progression.
- Step 17: Idle / AFK / Rewards.
- Step 18: Main UI + Account + Save.

---

# D6-D8 STATUS

- D6: RECOVERED from historical source; architectural rules preserved. Internal slot-count inconsistency explicitly recorded; do not invent a resolution.
- D7: RECOVERED from historical source; movement/targeting/combat rules recorded.
- D8: RECOVERED from historical source; Battle Scene/entity/movement prototype contract recorded.
- Numerical values explicitly described as examples/test values remain examples/test values unless a later explicit locked decision promotes them.
- Later P07.x locked architecture supersedes older implementation approaches where the later decision is more specific (for example, unified damage/status/effect authorities), without erasing the D6-D8 design intent.
