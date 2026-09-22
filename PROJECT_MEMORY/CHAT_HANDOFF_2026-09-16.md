# TLTD — CHAT HANDOFF / SESSION PACKAGE

Date: 2026-09-16
Project: TLTD / Thao Thiet Long Than Dao (饕餮龙神道)
Workspace: `E:\code\TLTD`
Engine: Unity 6000.6.0f1 (64-bit)
Repository: `huycodedie/Ai_MEMORY_TLTD`

## 0. PURPOSE

This file packages the important decisions, verified/reported implementation state, audit findings, constraints, and immediate next steps from the current conversation so a new ChatGPT conversation can continue without reconstructing history from memory or guessing.

READ THIS FILE TOGETHER WITH:
- `PROJECT_MEMORY/AI_RULES.md`
- `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`
- `PROJECT_MEMORY/D1_D23_LOCKED.md`
- `PROJECT_MEMORY/D1_D23_AMENDMENTS_LOCKED.md`
- `PROJECT_MEMORY/DESIGN_CHANGELOG.md`
- `PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md`
- `PROJECT_MEMORY/UI_REDESIGN_SPECIFICATION.md`
- `PROJECT_MEMORY/UI-02_PRE_IMPLEMENTATION_AUDIT.md`
- `PROJECT_MEMORY/UI-02_COMBAT_HUD_REPORT.md`
- `PROJECT_MEMORY/UI-02_Y_AXIS_COMBAT_ROOT_CAUSE_AUDIT.md`

## 1. NON-NEGOTIABLE OPERATING RULES

- Respond in Vietnamese unless the user asks for another language.
- Be direct, technical, structured, and evidence-driven.
- Never fabricate execution, PASS, visual verification, regression, or LOCKED status.
- Distinguish IMPLEMENTED / EXECUTED / PASS / VISUAL VERIFIED / REGRESSION PASS / LOCKED.
- Automated PASS is not automatically Unity Play Mode PASS.
- Play Mode PASS is not automatically visual verification.
- User-provided reports are evidence to audit, not independent execution by the assistant.
- D1-D23 are the locked design contract; later explicit user-approved amendments or P01+ tested-and-accepted behavior may supersede older historical descriptions when the superseding relationship is proven.
- Never invent missing design decisions.
- If a conflict with locked design cannot be proven superseded: STOP and report.
- UI is presentation only; never create duplicate gameplay authorities.
- Audit existing architecture before coding.

## 2. PROJECT / MEMORY ARCHITECTURE

Long-term memory/backup:
- GitHub repository `huycodedie/Ai_MEMORY_TLTD`.

Local memory intended for Antigravity:
- `E:\code\TLTD\PROJECT_MEMORY\`

The local project memory should mirror the important Git memory files so Antigravity can read the same baseline without depending on GitHub access.

## 3. LOCKED CORE DESIGN AMENDMENTS

### Combat / Companion
- Player has 1 Hero and up to 5 Companions.
- Companion is a CombatEntity with HP, can be attacked, can die, and respawns after 25s under the locked configurable rule.
- Hero death = immediate defeat; companions stop/disappear.
- Hero baseline attack interval = 1.5s.
- Companion baseline attack interval = 2.0s.
- Common Combat Resolver is used; Companion does not get a separate Combo/Counter engine.

### Bun / Banh Bao
- Normal Monster Battle consumes 1 Bun per Hero combat action.
- Companion actions do not consume Bun.
- Bun = 0 during Normal Battle: Hero STOP + Companion STOP + Monster/Normal combat STOP.
- Boss does not depend on Bun.
- Offline Bun behavior follows locked D19; do not invent additional behavior.

### Progression
- Hero Level does not directly scale base HP/ATK/DEF.
- Stats increase through Title Breakthrough.
- D21 Level Cap behavior: excess EXP is retained/accumulated when breakthrough cannot yet happen; this supersedes an older historical reset-to-zero statement.
- Breakthrough costs no Gold; conditions are AND and data-driven.

### Loot / Equipment
- `Chest Level = Drop Level = Cấp Rơi` — one concept/runtime authority.
- Item Level is based on current Hero Level and must satisfy `abs(ItemLevel - HeroLevel) <= 5`; exact distribution within ±5 remains TBD.
- Cấp Rơi determines quality/rarity availability and probability.
- The nine visible qualities in the reference are NOT a hard maximum. Higher qualities may exist. A 0% row can mean unavailable/not unlocked at the current Cấp Rơi.
- Never hard-code `Tối Thượng` as the final rarity.
- Equipment has 12 wearable slots: Vũ khí/Kiếm, Mũ, Khăn che mặt, Áo, Quần, Giày, Găng tay, Đai lưng, Áo choàng, Dây chuyền, Nhẫn, Bùa/Ngọc.
- Do not use `SPECIAL` as a player-facing slot name.
- Equipment recycle returns GOLD ONLY; no EXP.
- Base Auto-Recycle: lower CP than equipped item is eligible. Future Preferred Attribute/Affix protection can keep an otherwise eligible item.

### Chest Upgrade
- One level at a time.
- No queue, no auto-chain, no cancel, no claim button.
- While upgrading, chest opening uses the currently completed level.
- At persisted `CurrentTime >= FinishTime`, auto-complete to the new level and enter IDLE.
- Gold is deducted immediately when upgrade starts.
- Exact costs/times remain data-driven/TBD unless explicitly locked.

## 4. LOCKED P07 MILESTONES

### P07.5
- Debuff / DoT / Status Tick.
- Reported Automated 46/46, Play Mode 30/30, Master Regression through P07.5 PASS.
- LOCKED based on supplied execution evidence.

### P07.6
- Stun / Root / Freeze.
- CC Resistance 0..1; effective duration = baseDuration × (1 - resistance).
- 100% resistance blocks CC.
- Anti-CC checked before CC resistance/application.
- Freeze behaves like Stun; shatter only when `CanShatterFreeze=true`.
- Action permissions include CanMove, CanBasicAttack, CanUseSkill, CanUseUltimate, CanDash.
- `EntityStatusController` is sole status authority.
- LOCKED based on supplied execution evidence.

### P07.7
- Generic Cleanse / Dispel / Status Removal in `EntityStatusController`.
- Cleanse negative; Dispel positive.
- Supports configured category/status/count/stacks/selection behavior.
- CC cleansing restores permissions.
- Cleanse Freeze does not Shatter.
- Ordinary Cleanse/Dispel does not remove Anti-CC immunity unless explicitly targeted.
- Shield isolation preserved.
- Reported Automated 55/55, Play Mode 35/35, Master Regression PASS.
- LOCKED based on supplied execution evidence.

### P07.8
- Shield authority remains `EntityStatusController`.
- Damage authority remains `DamageCalculator`.
- HP/damage application remains `HealthComponent`.
- Shield interception is inside the existing damage pipeline before HP decrement.
- Deterministic shield ordering: Priority desc -> StartTime asc/FIFO -> ShieldId ordinal.
- Stacking policies: Additive, RefreshDuration, Replace, Independent, Ignore.
- No per-frame shield loops or duplicate shield authority.
- Initial visual blocker was remediated.
- Latest supplied acceptance: Automated 55/55, Play Mode 35/35, UI 5/5, Visual V01-V08 PASS, Master Regression P01-P07.8 PASS.
- P07.8 is LOCKED.

## 5. P07.9 — LOCKED

P07.9 = Advanced Skill Casting (Cast Time / Channel / Interrupt).

Approved/locked behavior:
- Instant CastTime=0 executes synchronously.
- Cast progression is runtime-driven.
- Channel progression is runtime-driven and deterministic.
- Rage is consumed at Cast Start.
- Interrupted cast gets no Rage refund.
- Cast-time cooldown starts at Cast Complete.
- Channel cooldown starts when channel ends.
- Interrupted before completion does not receive normal cooldown.
- Casting locks movement.
- Stun and Freeze interrupt active Cast/Channel.
- Root does not interrupt.
- Ordinary damage does not interrupt.
- Existing `EntityStatusController` remains sole CC authority.
- Existing `Entity.InterruptCurrentAction` is integration point.
- No second skill execution authority, global loop, coroutine/async authority, or Animator-dependent timing authority.
- Ultimate validation must occur before Rage deduction.
- Death/target-death invalid paths remain protected.
- UI is presentation only.

Reported final evidence:
- Phase 2.3 initially produced 1 failure out of 1043 due to Dodge RNG dependency in a test.
- Test-only remediation set monster Dodge to 0 in the affected test after battle start; no gameplay code change.
- Three consecutive Phase 2.3 runs 13/13.
- Targeted regressions 234/234.
- Full Master Regression 1043/1043.
- Compile 0 errors / 0 warnings.
- P07.9 was explicitly LOCKED by the user.

## 6. P07.9.1 — LOCKED

P07.9.1 = Hero Autonomous Skill Decision & Auto Combat.

Runtime scope:
- Auto ON: basic attack + autonomous normal skills.
- Priority is data-driven: Priority DESC, SkillId ordinal ASC tie-break.
- Ultimate can preempt/interrupt basic windup according to existing locked combat behavior.
- Auto OFF stops autonomous skill/ultimate decisions while basic attack continues according to the tested behavior.
- Manual skill still uses existing validator/executor path.

Audit gates:
- Removed colloquial `Silence` terminology; no gameplay support added.
- `HeroSkillDecisionController` only reads Rage; it does not mutate Rage.
- Rage deduction remains in `SkillExecutor` / `RageComponent` authority.
- No hard-coded Ultimate RageCost; controller uses `ultDef.RageCost`.
- Current Prototype01 Ultimate RageCost is 100 as runtime/configuration evidence.
- Basic attack is blocked while `Entity.IsCasting` is true; `SkillCastState.IsActive` covers Casting and Channeling.
- Compile reported 0 errors / 0 warnings.
- Dedicated P07.9.1 suite 16/16 PASS.
- P07.8 55/55 PASS.
- P07.9 Phase5.3 36/36 PASS.
- P07.9 Risk04 18/18 PASS.
- Historical Master P01-P07.8 PASS.
- Actual Prototype01 runtime scenarios A-E were reported PASS.
- User-approved final audit recommended LOCK and the user subsequently treated P07.9.1 as LOCKED.

## 7. UI DESIGN BASELINE

`PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md` is the structural UI baseline.

- 2D mobile portrait / vertical-first.
- Global bottom navigation is the game shell, not owned by Công Pháp.
- 5 primary navigation positions.
- Center position = Main Hub / Main Game Frame.
- Main Content Area changes for major systems.
- No second navigation framework.
- Công Pháp is a dedicated system screen/module.
- Exact labels/icons, artwork, spacing, font, colors, detailed interaction remain open unless explicitly approved.

`PROJECT_MEMORY/UI_REDESIGN_SPECIFICATION.md` is the approved implementation specification, not itself a gameplay lock.

## 8. UI-01 STATUS

UI-01 Foundation was reported PASS:
- Portrait 1080x1920.
- Hierarchy `SafeAreaRoot -> MainGameShell -> MainContentArea + GlobalBottomNavigation`.
- Safe Area uses `Screen.safeArea`.
- Debug controls moved to drawer.
- Compile 0 errors/warnings.
- P07.8 55/55, P07.9 Phase5.3 36/36, Risk04 18/18, targeted regression 109/109 reported PASS.

Current navigation placeholder labels were reported as `Túi Đồ`, `Tâm Pháp`, `Đại Điện`, `Bang Hội`, `Thiết Lập`. These are implementation placeholders, NOT newly locked gameplay/design names unless separately approved.

## 9. UI-02 STATUS BEFORE CURRENT SESSION

UI-02 pre-implementation audit found:
- TopHeader too basic.
- Monster UI too flat.
- Hero UI too flat.
- No dedicated on-screen skill action bar.
- CastBarUI functional but visually basic.
- Shield authoritative but visually basic.
- Equipment/inventory panels obscure battle area.
- Companion HUD absent.
- DamagePopup minimalist.

Authoritative bindings:
- HP -> HealthComponent.
- Shield -> EntityStatusController shield state/events.
- Rage -> RageComponent.
- Cast/Channel -> Entity.CastState / SkillCastState.
- Cast interrupt -> EventBus.OnSkillCastInterrupted.
- Skill execution -> existing Hero/MindMethodManager/CooldownManager/SkillExecutor path.
- UI must delegate/read only.

## 10. CURRENT CRITICAL DISCOVERY — COMBAT HEIGHT DEADLOCK

User suspected that Hero/Monster height difference caused combat deadlock. A dedicated audit confirmed this.

### Reproduction
Encounter #1:
- Hero = `(-2.200, -0.300, 0.000)`
- Monster = `(2.200, -0.300, 0.000)`
- DeltaY = 0
- Combat works.

Encounter #2 before fix:
- Hero approximately `(0.605, -0.300, 0.000)`
- Monster spawn = `(4.000, -1.200, 0.000)`
- Monster after movement approximately `(2.317, -1.200, 0.000)`
- DeltaY = 0.900
- Horizontal distance = 1.712
- Full 3D distance = 1.9342
- Movement stopping distance = 1.800
- Attack threshold = 1.900

Result:
- Movement sees horizontal distance <= 1.8 and stops.
- Attack sees 3D distance 1.9342 > 1.9 and refuses attack.
- Both can remain ready but no attack executes.
- Rage never increases from basic hits.
- Permanent deadlock.

### Root causes
1. `BattleManager.monsterSpawnPosition` retained legacy Y = -1.2 while Prototype01 SceneBuilder established combat plane Y = -0.3.
2. `MovementComponent.MoveTowardTarget` zeroes `direction.y` and therefore uses horizontal combat-plane distance.
3. `AttackComponent.TryExecuteAttack` previously used `Vector3.Distance` (full 3D distance), creating a divergent distance model.
4. SpawnMonster also instantiated a legacy white rectangle + 3D `MONSTER` label instead of the standard wuxia standee pipeline.

The audit classified this as multiple causes: spawn-plane discrepancy + movement/attack distance model inconsistency + visual spawn pipeline inconsistency.

## 11. UI-02 RUNTIME COMBAT HEIGHT FIX — CURRENT BASELINE

The user supplied a final remediation report dated 2026-09-16 20:05.

Production files modified:
- `Assets/_Game/Entities/Components/AttackComponent.cs`
- `Assets/_Game/Core/BattleManager.cs`
- `Assets/_Game/Editor/Prototype01SceneBuilder.cs`
- `Assets/_Game/Scenes/Prototype01.unity`

Test file added:
- `Assets/_Game/Editor/Prototype01PlayTestRunner_UI02_CombatHeightFix.cs`

### Exact fix
A. BattleManager monster spawn Y:
- `new Vector3(4f, -1.2f, 0f)` -> `new Vector3(4f, -0.3f, 0f)`.
- SceneBuilder now serializes `monsterSpawnPosition = (4, -0.3, 0)`.

B. Attack distance:
- Replaced `Vector3.Distance(...)` with the same horizontal-plane model as Movement:
  - `delta = target.position - transform.position`
  - `delta.y = 0`
  - `distance = delta.magnitude`

C. Monster visual spawn:
- Removed legacy white placeholder and 3D `MONSTER` label.
- Uses `UIProceduralTextureFactory.GetMonsterStandeeSprite()`.
- Scale `(1.1,1.1,1)`.
- Sorting order 10.
- `flipX = true`.

### Supplied validation
Dedicated suite:
- 12/12 PASS.

Important tests:
- Same combat Y.
- Spawned monster Y = -0.3.
- Artificial DeltaY = 0.9 with horizontal distance inside range still attacks.
- Horizontal distance remains authoritative.
- Hero approaches runtime spawn without deadlock.
- Monster can attack Hero.
- Encounter 1 -> 2 works.
- Monster 2 damaged.
- Encounter 2 -> 3 works.
- Monster 3 damaged.
- Visual pipeline consistent.
- No physics dependency introduced.

Play Mode scenarios A-L were reported PASS, including continuous Encounter 1 -> 2 -> 3 without manual repositioning.

Regression reported:
- P07.8: 55/55.
- P07.9 Phase5.3: 36/36.
- P07.9 Risk04: 18/18.
- P07.9.1: 16/16.
- Cumulative: 125/125.

Compile reported:
- 0 errors.
- 0 warnings.
- Exit code 0.

Locked boundaries reported untouched:
- `SkillExecutor.cs`
- `SkillExecutionValidator.cs`
- `SkillCastState.cs`
- `CooldownManager.cs`
- `RageComponent.cs`
- `EntityStatusController.cs`
- `DamageCalculator.cs`
- `HealthComponent.cs`
- `BasicAttackProcessor.cs`
- `HeroSkillDecisionController.cs`

### Status rule
Treat this as:
- `UI-02 Runtime Combat Height Fix = PASS / REMEDIATED` based on user-provided execution evidence.
- It is NOT the same as globally locking UI-02.
- Global UI-02 remains pending owner acceptance.
- Do not revert this Y-axis fix.

## 12. NEXT IMMEDIATE TASK — COMBAT RUNTIME TIMING MONITOR

The user asked for a way to check actual runtime duration/timing after the height fix.

A prompt was prepared for Antigravity to create a test-only instrumentation layer:
- Suggested file: `Assets/_Game/Editor/Prototype01CombatRuntimeTimingMonitor.cs`.
- Suggested report: `PROJECT_MEMORY/UI-02_COMBAT_RUNTIME_TIMING_REPORT.md`.

Purpose:
- Measure wall-clock and Unity runtime timing.
- Record Encounter #1 -> #2 -> #3 (and #4 if possible).
- Record MonsterSpawn, HeroMoveStart, HeroEnterAttackRange, HeroFirstAttack, MonsterFirstAttack, FirstDamage, MonsterDeath, NextEncounterSpawn.
- Record Hero/Monster position, alive state, Rage where available, attack timer/interval where available, AttackRange, target state.
- Record DeltaX, DeltaY, horizontal distance, full 3D distance, attack threshold.
- Detect diagnostic deadlock only after a clear realtime timeout; proposed diagnostic timeout 5s is NOT a gameplay rule.
- Do not fake timing with Sleep/WaitForSeconds and call it attack timing.
- Do not teleport or manually reposition entities.
- Do not manually call attack to make the test pass.
- Do not alter gameplay authority just to expose timing.
- If an API is unavailable, report `[UNAVAILABLE_FROM_CURRENT_API]` rather than changing production architecture.

Minimum timing acceptance:
- Encounter 1 -> 2 -> 3 observed in actual runtime.
- No manual reposition.
- No manual attack.
- No manual Y correction.
- No manual Time.timeScale manipulation.
- Actual attack and damage events observed.
- No deadlock during the observation window.
- Timing report created.

Do NOT declare UI-02 LOCKED from this timing monitor alone.

## 13. IMPORTANT LESSON FROM THE HEIGHT BUG

Previous automated tests forced Hero and Monster to matching Y values in some test runners, which masked the real scene/spawn discrepancy.

Therefore future verification must include:
- Actual Prototype01 scene runtime.
- Multi-encounter transitions.
- Spawned entities, not only hand-positioned test fixtures.
- No hidden fixture that repairs the condition being tested.
- Visual evidence when visual behavior matters.

## 14. WHAT MUST NOT BE DONE NEXT

Do not:
- Rewrite P07.8/P07.9/P07.9.1 backend systems.
- Create another combat engine.
- Create another distance authority.
- Create another skill authority.
- Create another Rage authority.
- Create another cooldown authority.
- Add physics/NavMesh merely to solve this issue.
- Reintroduce 3D distance for attack while movement remains horizontal.
- Hard-code a fake AttackRange tolerance to hide the mismatch.
- Lock global UI-02 before its required visual/runtime acceptance is complete.

## 15. CONTINUATION ORDER FOR NEW CHAT

When continuing in a new chat:

1. Read `PROJECT_MEMORY/AI_RULES.md`.
2. Read `PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md`.
3. Read `PROJECT_MEMORY/D1_D23_LOCKED.md` and amendments.
4. Read this `CHAT_HANDOFF_2026-09-16.md`.
5. Read the UI-02 audit/report files if working on UI-02.
6. Treat P07.8, P07.9 and P07.9.1 as LOCKED.
7. Treat the Y-axis combat fix as the current runtime baseline and do not revert it.
8. If a new report says PASS, audit its evidence instead of blindly trusting the label.
9. Continue with runtime timing verification, then UI-02 Combat HUD presentation work.
10. Only after UI-02 visual/runtime acceptance is complete should global UI-02 be considered for LOCKED status.

## 16. CURRENT STATE IN ONE SCREEN

```text
D1-D23                     = LOCKED + amendments recorded
P07.5                     = LOCKED
P07.6                     = LOCKED
P07.7                     = LOCKED
P07.8                     = LOCKED
P07.9                     = LOCKED
P07.9.1                   = LOCKED

UI structural baseline    = LOCKED
UI-01 Foundation         = PASS / implemented
UI-02 global             = LOCKED (as of 2026-09-17)

Combat Y-axis deadlock   = ROOT CAUSE CONFIRMED
Y-axis remediation       = PASS / current baseline
12 dedicated tests       = PASS (reported)
Multi-encounter PlayMode = PASS (reported)
125 regressions          = PASS (reported)
Compile                  = PASS — 0 errors / 8 warnings
          (4 unique pre-existing, non-blocking warnings)

NEXT:
Combat Runtime Timing Monitor (COMPLETED)
        ↓
UI-02 Combat HUD visual work (COMPLETED)
        ↓
UI-02 visual/runtime acceptance (VERIFIED PASS)
        ↓
UI-02 LOCK (COMPLETED 2026-09-17)
        ↓
P08 / UI-POLISH-01 (Not started)
```

## 17. EVIDENCE QUALIFIER

All execution numbers and Play Mode results in this handoff are explicitly marked as user-provided/reported evidence unless independently executed in the current chat. This distinction must remain intact in future conversations.

END OF HANDOFF
