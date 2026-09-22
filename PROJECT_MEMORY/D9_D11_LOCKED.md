# D9–D11 — RECOVERED / LOCKED DESIGN CONTRACT

> Source: historical user-provided design document.
> Status: RECOVERED / LOCKED as a design contract, while unresolved/TBD details remain explicitly unresolved.
> Rule: do not invent missing values or silently promote examples/test values into permanent balance.

## D9 — Animation + VFX + Combat Feedback

### D9.1 Complete attack pipeline
A complete attack follows:
Hero standing → Attack Timer ready → Attack animation → Animation Event / Hit Event → Hit Effect / Projectile → Projectile travels → Impact → Damage Calculation → Damage Number → Crit / Miss / Lifesteal / Stun feedback → Enemy Reaction → Hero returns to combat.

Damage must NOT be calculated merely because the animation starts. Gameplay hit timing is tied to an Animation Event / Hit Event.

### D9.2 Animation state machine
Minimum Hero states:
- Idle
- Moving
- Attack
- Skill
- Ultimate
- Hit
- Stun
- Death
- Respawn

Basic flow: Idle → Moving → Attack → Hit Event → Idle.
Skill: Idle → Cast → Skill Animation → Impact → Idle.
Ultimate: Idle → Ultimate Start → Ultimate Animation → Impact → Ultimate End → Idle.

### D9.3 Animation must not decide gameplay
Architecture:
Animation → Animation Event: AttackHit → Combat System → Damage.
Animation implementation must not directly own gameplay damage logic.

### D9.4 Attack timeline
Historical example:
- 0.00s Attack Start
- 0.15s Wind-up
- 0.35s Weapon Strike
- 0.40s Hit Event / Damage
- 0.45s Impact VFX
- 0.70s Attack Animation End
- 1.50s Attack Timer Ready

1.5s is Attack Interval and does not imply animation duration must equal 1.5s.

### D9.5–D9.6 Projectile and delivery types
Projectile is an independent system with:
- Owner
- Target
- Speed
- DamagePayload
- Direction
- HitEffect
- OnHit

Flow: Hero → Attack Animation → Spawn Projectile → Projectile → Enemy → Collision/Hit → Combat Damage.

Skill delivery must be data-driven and support at least:
- Projectile
- Melee
- Area
- Fixed Position
- Targeted

Skill Data should expose `DeliveryType` rather than hard-coding one delivery mechanism.

### D9.7 Targeted Skill
Targeted skills lock the target at cast time. Projectile tracks that target until hit, while checking:
- IsTargetable?
- IsAlive?
- HasInvincible?

The source states that this type normally follows and hits the selected target and cannot be dodged; preserve this rule as source-defined behavior.

### D9.8 Fixed Position Skill
A Fixed Position skill stores its cast position and does not move with the enemy. If the enemy leaves the area, it does not take damage from that fixed area.

### D9.9 Melee Skill
Melee checks `Distance <= SkillRange` at the Animation Event / impact stage, then resolves the target without a projectile.

### D9.10 Area Skill
Area skills expose at least:
- Center
- Radius
- Shape
- Duration
- Tick

Shape must be extensible; examples include Circle, Cone, Rectangle, Line. Do not hard-code only Circle.

### D9.11–D9.12 Damage feedback / combat text
DamageEvent concept includes:
- amount
- type
- target

UI/VFX decides presentation.
Minimum combat text types:
- NORMAL_DAMAGE
- CRIT_DAMAGE
- MISS
- HEAL
- LIFESTEAL
- STUN
- DODGE
- IMMUNE

Future examples: BLOCK, PARRY, COUNTER, RESIST, ABSORB, REFLECT.

### D9.13 Crit feedback
Crit result must propagate from damage calculation to Damage Number and Crit Effect, with optional camera/screen feedback. Crit feedback should not be limited to a text color; scale, flash, particle, impact, and sound are possible presentation layers.

### D9.14 Miss / Dodge
A dodge resolves as a distinct `AttackResult = MISS`, with Damage = 0 and a `MISS` combat event/text. Do not display a fake `-0` damage number.

### D9.15 Lifesteal
Example flow: Damage 200 × Lifesteal 10% = Heal 20. Healing feedback appears on Hero side; damage appears on Enemy side. This follows the previously confirmed combat-feedback placement.

### D9.16 Stun visual and gameplay linkage
Stun status drives gameplay restrictions such as:
- Attack Timer Pause
- Movement Disabled
- Skill Disabled

Historical example duration: 0.5–1.0s depending on skill. This duration is not promoted to universal permanent balance without later explicit confirmation.

### D9.17 Hit Reaction
Hit reaction can be visually short (example 0.15s), but Combat System remains authoritative for attack timing. Hit animation must not unnecessarily lock gameplay.

### D9.18–D9.19 Death and Companion Respawn presentation
Hero: HP <= 0 → Death Animation → Battle End.
Companion: HP <= 0 → Death Animation → Disappear → Respawn Timer 25s.
Respawn presentation may show a countdown. 25s is consistent with the recovered locked Companion respawn rule and remains configurable.

### D9.20 Battle speed
Animation/VFX must synchronize with `BattleTimeScale` rather than using independent unsynchronized timers.
Historical examples:
- x1 = 1
- x2 = 2
- x3 = 3

Attack Timer, Skill CD, Stun, Buff, Projectile, Respawn and Battle Timer all use Battle Time Scale.

### D9.21 Camera
Prototype camera architecture:
`CameraController` with FollowBattle, Shake, Zoom, FocusTarget.
Keep cinematic camera complexity out of this prototype. Fixed battle camera is sufficient; Ultimate may trigger a light zoom/effect and return.

### D9.22 Combat FX Architecture
Create `CombatFXManager` listening to combat events such as:
- AttackEvent
- DamageEvent
- CritEvent
- MissEvent
- HealEvent
- StunEvent
- DeathEvent
- SkillEvent
- UltimateEvent

It may spawn Particle/VFX/Text/Sound/Camera Shake. Presentation can change without modifying DamageSystem.

### D9.23 Sound System
Actions may expose AttackSound, HitSound, CritSound, SkillSound, UltimateSound, StunSound, DeathSound, HealSound. Audio is event-driven (`OnCrit` → AudioManager.Play(CritSound)) and must not be directly embedded in DamageSystem.

### D9.24 Complete attack architecture
Player attack flows through animation → projectile spawn (when applicable) → target → dodge check → MISS or HIT → damage calculation → normal/crit/lifesteal branches → damage number → hit animation.

### D9.25 D9 required tests
The historical source requires testing at least:
1. Basic Attack: Animation → Hit → Damage
2. Projectile: travels to correct target
3. Targeted Skill: follows correct target
4. Fixed Skill: remains at cast position
5. Melee Skill: only hits in range
6. Area Skill: can hit multiple targets
7. Crit: increased damage + separate FX
8. Dodge: no damage
9. Lifesteal: heal appears on Hero side
10. Stun: Timer/Movement/Skill locked

D10 Combat UI is to follow after these D9 tests are stable.

## D10 — Combat UI / Battle HUD

### D10.1 Battle screen
Prototype HUD includes conceptually:
- Stage/area identifier
- Battle timer
- Player and enemy identity/level
- Hero/Enemy HP
- Rage
- Hero + Companion presentation
- Damage feedback
- Auto control
- x1/x2/x3 speed controls
- Skill 1–4 and Ultimate controls

### D10.2 HP Bar
UI consumes Combat Entity state such as CurrentHP, MaxHP, HPPercent and listens to `HPChanged`. UI must not independently calculate authoritative HP.

### D10.3 Rage Bar
Max Rage is 100 in the recovered source. Historical current Ultimate cost is 80. UI should show current/max Rage and `ULT READY` when Rage >= required cost. Rage should not be artificially capped at the Ultimate cost merely because current cost is 80.

### D10.4 Skill Bar
Hero has Basic Attack + Skill 1–4 + Ultimate in the source. Basic Attack is not treated as an ordinary skill cooldown button; it is controlled by AttackTimer/combat logic.

### D10.5–D10.8 Skill cooldown examples
Historical examples:
- Skill 1: 1s
- Skill 2: 5s
- Skill 3: 12s
- Skill 4: configurable, example range 25–40s; examples 30/40

These are source examples/current historical values and must not be silently generalized beyond later locked decisions.

### D10.9 Ultimate
Historical current value: Rage Cost = 80. UI dark/disabled when insufficient and shows READY when Rage >= 80.

### D10.10 Auto
AUTO ON: AI decides Basic Attack, Skills, Ultimate and Movement.
AUTO OFF: player can manually press Skill 1–4 and Ultimate. Basic Attack remains governed by its own combat logic.

### D10.11 Skill Priority
Confirmed priority behavior:
- A skill that becomes ready earlier is used first.
- If multiple skills are ready together, higher-level/higher-priority skill is preferred.
Historical example ordering: Skill 4 → Skill 3 → Skill 2 → Skill 1.

### D10.12 Buff skills do not block the next skill
If a skill only applies a stat buff, casting completion does not require waiting for the buff duration to expire before another skill can cast.

### D10.13 Skill Queue
`SkillPriorityController` should return `NextSkill` based on readiness and priority. This prevents ambiguous ordering when multiple skills are ready.

### D10.14 UI must not control Combat directly
Architecture:
UI → Command → Combat System.
Example: Ultimate button → `BattleCommand.UseUltimate()` → HeroSkillController → RageController → SkillSystem → Combat.
UI must not directly mutate gameplay values such as HP.

### D10.15 Battle Speed
UI exposes x1/x2/x3 with one active selection. Historical values: 1.0, 2.0, 3.0. Actual battle timing is controlled by BattleSpeed/BattleTimeScale.

### D10.16 Battle Timer
Boss/PvP historical timer: 60s countdown. Normal monsters: no finite countdown / effectively unlimited. At 00:00, Boss loss if Boss is not dead.

### D10.17 Normal Monster Progress
Historical confirmed example: 50 normal monsters = 100%; each death +1. UI can show count and percentage. Code must use configurable `stage.monstersRequired`, not hard-code 50.

### D10.18 Boss Transition
Normal monsters → kill required count → 100% → Boss Transition → Boss Arena → Boss Battle. Boss battle uses `BattleMode = Boss`, `MaxTime = 60s` in the source.

### D10.19 Boss Arena
Boss has a separate battle environment from NormalMap. Use an extensible `BattleArenaConfig` concept so Normal, Boss, PvP and Event arenas can differ.

### D10.20 Pause
Pause freezes Battle Simulation, not merely animation. Attack Timer, Skill CD, Stun, Projectile and Battle Timer all stop while paused.

### D10.21–D10.22 Battle Result
Victory/Defeat screens display the result and actions. `BattleManager` is authoritative and produces a `BattleResult` consumed by `BattleResultUI`.
Possible reasons in source:
- WIN / BOSS_DEAD
- LOSE / HERO_DEAD
- LOSE / TIMEOUT

UI must not calculate the battle result itself.

### D10.23 Data Model for UI
UI should consume ViewModels, e.g.:
`HeroBattleViewModel`: Name, Level, CurrentHP, MaxHP, Rage, MaxRage, Skills[], AttackProgress, StatusEffects[].
`SkillViewModel`: SkillID, Icon, Cooldown, RemainingCooldown, IsReady, CanCast, RageCost.

### D10.24 Status Effect UI
UI may show active Stun, Buff and Debuff with remaining duration and effect value. This is the presentation foundation for future cultivation/equipment systems.

### D10.25 Separation rule
From D10 onward, Gameplay Data and Presentation must remain separated.
Gameplay: Hero, Skill, Equipment, Buff, Rage, HP, Stats.
Presentation: UI, Animation, VFX, Sound, Camera.
Architecture:
Battle Core → UI / VFX / Audio.
Do not create direct UI→Combat, VFX→Combat, or Audio→Combat gameplay coupling.

### D10.26 D10 completion checklist
The source requires testing:
- Hero and Enemy HP realtime updates
- Rage 0–100
- Ultimate ready at required Rage
- Skill cooldowns
- Skill priority
- Buff skill does not block next skill
- AUTO ON/OFF
- Manual skill use
- x1/x2/x3
- Boss timer 60s
- Normal monster unlimited time
- Required monster count reaches 100%
- Transition to Boss Arena
- Victory / Defeat screens
- Pause
- Correct Damage/Crit/Miss/Lifesteal placement

## D11 — Companion System

### D11.1 Team composition
One side can have:
- 1 Hero
- up to 5 Companions

It is not mandatory to fill all five slots.

### D11.2 Companion is a real Combat Entity
Each Companion has its own:
- HP / MaxHP
- ATK
- DEF
- AttackSpeed
- CritRate
- CritDamage
- Dodge
- Lifesteal
- Skill
- RespawnTimer

Companion does not use the Hero's Rage system.

### D11.3 Targeting
Enemies can target Hero or any active Companion. Target selection uses the shared Target System:
- nearest target first
- if distance is equal, Hero has priority

Do not create a separate incompatible targeting system for each Companion.

### D11.4 Companion Attack Timer
Historical current example: Companion Attack Interval = 2s. Each Companion has its own timer and need not attack on the same frame.

### D11.5 Companion animation/attack
Companion attack follows:
Attack Timer → Attack Start → Attack Animation → Projectile/Melee → Hit → Damage → Reset Timer.

### D11.6–D11.7 Companion death and respawn
When HP <= 0, Companion remains represented in the system but transitions ALIVE → DEAD. It becomes non-attacking and non-targetable, plays Death animation, then starts RespawnTimer.
Historical locked respawn: 25s. On respawn, HP returns to MaxHP by default; respawn HP percentage can be data-driven later.

### D11.8 Hero death
Hero death immediately produces BattleResult = LOSE and stops the Battle. All Companions immediately stop attack, animation and AI. They cannot continue attacking and win after Hero death.

### D11.9 Companion death does not lose battle
If Hero remains alive, one or more dead Companions do not by themselves cause defeat. Battle continues with remaining entities.

### D11.10 Shared Companion Targeting
Companions use the same Target System and nearest-target/equal-distance-Hero-priority rules.

### D11.11 Companion does not generate Hero Rage
Companion attacks do not add Rage to the Hero. Hero Rage sources include Hero Basic Attack, Hero taking damage, and explicitly defined special effects from later Skills/Items.

### D11.12 Companion damage
Enemy attacks against a Companion reduce Companion HP and do not add Rage to Hero merely because the Companion was damaged.

### D11.13 Companion Skill structure
Companion can support a data-driven array such as `CompanionSkill[]`, with Basic Attack, Skill 1, Passive 1/2, and later expandable Skill 2/Ultimate as needed. Do not hard-limit the system to one skill type.

### D11.14 Companion Skill Rage independence
Companion skills use their own cooldown/system and do not depend on Hero Rage unless a future explicitly designed special interaction says otherwise.

### D11.15 Companion Skill Effects
Possible data-driven effects include Damage, Heal, Buff, Debuff, Stun, Slow, Crit Buff, Attack Speed Buff, Defense Buff, Dodge Buff and Lifesteal Buff. Historical examples such as 150% ATK, Slow 30%, Duration 3s are examples, not universal balance constants.

### D11.16 Companion can buff Hero
Companion buffs to Hero must go through the shared BuffSystem rather than directly mutating Hero stats.

### D11.17 Companion Position
Companion positions should be relative to Hero position through a Formation Offset, not fixed absolute coordinates. When Hero moves, the formation moves with Hero.

### D11.18 Companion Movement
Companions can move similarly to Hero. Movement gameplay and movement animation remain separate. Possible states include Idle, Walk, Run, Attack, Hit, Stun, Death, Respawn, Skill.

### D11.19 Formation Controller
Use `CompanionFormationController`:
Hero Position → Formation Calculation → Companion Position.
Formation should be extensible beyond ARC, e.g. LINE, V, CUSTOM.

### D11.20 Companion State Machine
Suggested states:
- IDLE
- ATTACKING
- RECOVER
- STUNNED
- DEAD
- RESPAWNING
- BATTLE_STOPPED

Hero death forces any Companion state into BATTLE_STOPPED.

### D11.21 State priority
Historical priority:
DEAD → BATTLE_STOPPED → STUN → SKILL → ATTACK → IDLE.
If stunned while attacking, attack is cancelled, stun timer runs, then Companion resumes according to normal rules after stun.

### D11.22 Companion Data
Use data-driven `CompanionData`, including:
- id
- name
- level
- rarity
- baseHP
- baseATK
- baseDEF
- attackInterval
- skills[]
- respawnTime
- formationPosition

Adding a Companion should not require changing Combat Engine code.

### D11.23 Companion UI
Battle HUD may display Companion slots, HP, DEAD/RESPAWN state and respawn countdown, then a respawn effect when alive again.

### D11.24 Debug Mode
Prototype should provide a Companion Debug Panel showing, per Companion:
- HP / MaxHP
- State
- Target
- Attack Timer
- Respawn countdown
- Skill readiness

### D11.25 D11 completion checklist
Test that:
- max 5 Companions
- own HP
- own ATK/DEF
- own Attack Timer
- historical 2s attack example
- animation
- can be attacked
- can die
- death → 25s respawn
- respawn → returns to combat
- Hero death → Companions stop immediately
- Companion does not generate Hero Rage
- shared Target System
- Companion Skills
- Companion Effects
- Formation
- movement with Hero
- State Machine

Full prototype target:
Hero + up to 5 Companions VS Enemy Hero + up to 5 Enemy Companions in one Battle Simulation.

## Important source-status notes

1. This file is a recovered historical design contract. It preserves source terminology and architecture rather than rewriting the design from general knowledge.
2. Numeric values appearing as examples/current historical values (such as skill cooldowns, 0.5–1.0s stun, 2s Companion attack interval, 60s Boss timer, 50 monsters, x1/x2/x3) must not be promoted to immutable balance constants unless a later explicit decision does so.
3. D9–D11 must remain compatible with later, more specific locked architecture decisions. In particular, later P07.x unified status/damage/effect authorities take precedence over older prototype implementation patterns without erasing the D9–D11 design intent.
4. Companion HP is explicitly supported by this recovered source and aligns with the later recovered D2/C7–C9 rule; any older conflicting memory saying Companions/Pets have no HP is superseded.
5. No missing D12+ content is inferred here.
