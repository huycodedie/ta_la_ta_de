# P07.9 — Advanced Skill Casting Design Contract

**Status:** LOCKED — Design Contract Approved
**Milestone:** P07.9
**Scope:** Cast Time / Channel / Interrupt
**Authority:** P01+ tested/locked behavior + this P07.9 contract
**Date:** 2026-09-11

---

## 1. Purpose

P07.9 extends the existing skill system from fully synchronous, one-frame execution to optional non-instantaneous skill casting while preserving all existing P01–P07.8 authorities and backward compatibility.

The goal is an extensible execution lifecycle for:

- Instant skills (`CastTime = 0`)
- Cast-time skills
- Channel skills
- Hard-CC interruption

P07.9 must NOT create a second Skill Executor, second CC authority, second damage pipeline, second cooldown authority, or second resource authority.

---

## 2. Locked Design Decisions

### P07.9-D01 — Official gameplay durations

No official gameplay cast/channel duration is locked by P07.9 itself.

- Production/balance values remain **TBD / data-driven**.
- Automated tests may use explicit test fixtures with concrete durations/tick intervals.
- Test fixture values must not silently become gameplay defaults.

This prevents test numbers from becoming accidental game-balance rules.

### P07.9-D02 — Rage consumption timing and interruption

For a skill that requires Rage:

1. Rage is consumed at **Cast Start**.
2. If the cast is interrupted, the consumed Rage is **not refunded**.
3. If the cast completes successfully, no additional Rage is consumed.
4. Failed validation before Cast Start consumes no Rage.

This preserves the principle that a successfully accepted cast reserves/consumes its resource immediately.

### P07.9-D03 — Cooldown timing

For cast-time skills:

1. Cooldown starts only after **Cast Complete / successful execution**, preserving the existing successful-execution cooldown principle.
2. If a cast is interrupted before completion, the skill does **not** start its normal cooldown.
3. A failed validation before Cast Start does not start cooldown.

For channel skills:

- The channel begins after the cast/start phase reaches the channel state.
- The normal successful cooldown begins only after the channel completes successfully.
- An interrupted channel before successful completion does not start the normal cooldown.

No alternative cooldown authority is introduced.

### P07.9-D04 — Movement while casting/channeling

Casting and channeling **lock movement**.

- The active caster cannot voluntarily move while in the casting/channeling lifecycle.
- Root does not interrupt the cast because Root is not a hard interrupt under this contract.
- Stun/Freeze interrupt according to P07.9-D05.
- Existing movement authority remains responsible for movement permission; P07.9 must not create a parallel movement controller.

The exact presentation/animation of movement lock is implementation detail, not a new gameplay rule.

### P07.9-D05 — What interrupts a skill

Only the currently locked **Hard CC types Stun and Freeze** interrupt an active cast/channel.

- Stun: interrupts.
- Freeze: interrupts.
- Root: does **not** interrupt.
- Ordinary incoming damage: does **not** interrupt.
- Other future CC/effects do not interrupt unless a later explicit design decision adds them.
- Anti-CC prevents applicable Stun/Freeze application according to the existing P07.6 authority.

P07.9 must use the existing `EntityStatusController` CC authority rather than implementing another CC system.

---

## 3. Backward Compatibility

### Instant skills

`CastTime = 0` must retain the current synchronous behavior.

An instant skill must not require a multi-frame coroutine or asynchronous Unity Animator dependency merely because P07.9 exists.

Existing P01–P07.8 tests and behavior must remain valid.

### Existing authorities remain authoritative

- `SkillExecutor` remains the sole skill execution authority.
- `SkillExecutionValidator` remains the validation authority.
- `EntityStatusController` remains the sole CC/status authority.
- `DamageCalculator` remains the damage calculation authority.
- `HealthComponent` remains the HP/damage/shield entry pipeline.
- `CooldownManager` remains the cooldown authority.
- `RageComponent` remains the Rage authority.

A helper such as `SkillCastController`/cast-state object may be introduced only as an internal lifecycle/state helper. It must not become a second independent skill execution system.

---

## 4. P07.9 Skill Lifecycle

The implementation must support a deterministic lifecycle conceptually equivalent to:

`READY → CASTING → CHANNELING (optional) → COMPLETE → READY`

and interruption:

`CASTING → INTERRUPTED`

`CHANNELING → INTERRUPTED`

The exact runtime class/type names are implementation details and are not locked by this document.

The existing `SkillRuntimeState` may be extended or complemented with a dedicated runtime cast instance/state, provided there is only one authoritative lifecycle for the active skill execution.

---

## 5. Data-Driven Skill Configuration

`SkillDefinitionSO` may be extended with optional fields for P07.9, including concepts equivalent to:

- Cast Time
- Is Channel
- Channel Duration
- Channel Tick Interval
- Is Interruptible
- Interrupt Window
- Cancel/Interrupt Refund Policy, where needed by the implementation

Defaults for existing skills must preserve instant behavior.

No hard-coded gameplay duration, tick interval, or skill count may be inserted into the Combat Engine.

If a field is not required by the final implementation, it should not be added merely for symmetry.

---

## 6. Deterministic Time Model

P07.9 must be testable without relying on Unity coroutines, frame timing, or Animator Events.

The preferred model is deterministic time progression through the existing/injectable `ITimeProvider` or equivalent time abstraction.

Tests must be able to:

- Start a cast.
- Advance time deterministically.
- Observe cast progress/state.
- Trigger channel ticks deterministically.
- Apply Stun/Freeze deterministically.
- Verify interruption.
- Verify Rage and cooldown results.
- Verify death/target-death behavior.

Unity real-time Play Mode may validate presentation and integration, but the core lifecycle must remain headless-testable.

---

## 7. Channel Damage and P07.8 Shield Compatibility

Every channel damage tick must use the existing combat pipeline.

Required route:

`Channel Tick → existing damage calculation/effect execution → HealthComponent.TakeDamage → existing Shield interception → HP result`

P07.9 must not directly subtract HP and must not create a second shield/damage path.

All P07.8 shield ordering and authority rules remain unchanged.

---

## 8. CC Compatibility

P07.9 must preserve P07.6 rules:

- Stun blocks all applicable actions and interrupts active action/cast.
- Freeze blocks all applicable actions and interrupts active action/cast.
- Root blocks movement/dash but permits Basic/Skill/Ultimate and does not interrupt the cast.
- Anti-CC is checked before CC/resistance according to existing authority.
- CC resistance remains owned by `EntityStatusController`.

P07.9 must extend the existing interruption linkage rather than replacing it.

---

## 9. P07.7 Cleanse / Dispel Compatibility

Cleanse and Dispel continue to use the existing status-removal authority.

- Cleanse of Stun/Freeze immediately restores permissions according to P07.7.
- Cleanse itself does not create a false interrupt event.
- Dispel does not interrupt an active cast/channel merely because a positive status is removed.
- A cast already interrupted remains interrupted; Cleanse does not retroactively resume it.

No new status-removal system is permitted.

---

## 10. Ultimate Validation Regression Fix

P07.9 implementation must verify the existing validation gap identified during the pre-implementation audit:

- When the selected action is an Ultimate, `SkillExecutionValidator` must respect `CanUseUltimate` from `EntityStatusController`.

This is a compatibility/regression correction and must not create a second CC validation authority.

---

## 11. Death During Cast / Target Death During Cast

These cases must be explicitly defined by implementation tests before P07.9 is considered complete.

Required invariant:

- A dead caster cannot complete or continue an invalid active cast/channel.
- A dead target cannot be damaged by a later channel tick.
- No post-death damage, Rage, cooldown, or effect application may occur through an invalid execution path.

The exact target reacquisition behavior for a surviving channel is not locked by this contract unless already defined by existing P01+ behavior. Do not invent a new targeting rule without an explicit decision.

---

## 12. Interruptibility Field

If a skill is configured as non-interruptible, P07.9 must respect that configuration.

However, this field must not be used to bypass fundamental entity death/state validity.

The exact semantics of `InterruptWindow` remain implementation-scoped unless a later design decision explicitly requires a gameplay window.

No assumed interrupt-window duration is locked here.

---

## 13. Events

P07.9 may extend `EventBus` with lifecycle events required for deterministic tests and UI/debug presentation, such as conceptual events for:

- Cast Started
- Cast Progressed, if needed
- Channel Started
- Channel Tick
- Cast Completed
- Cast Interrupted

Event names and payload types are implementation details.

Events are notifications, not alternate authorities.

---

## 14. UI / Visual Verification

P07.9 Play Mode verification should demonstrate, at minimum:

1. Instant skill still behaves immediately.
2. Cast-time skill visibly progresses before its effect.
3. Channel skill visibly progresses/ticks.
4. Stun interrupts casting.
5. Freeze interrupts casting/channeling.
6. Root does not interrupt casting.
7. Ordinary damage does not interrupt casting.
8. Cast/channel movement lock is observable.
9. Interrupted feedback is observable where the prototype UI supports it.
10. Channel damage still passes through P07.8 Shield/HP pipeline.

A cast bar/channel bar is a recommended prototype presentation aid, not a new gameplay authority.

Visual PASS must be based on actual Unity Play Mode observation, not automated test output alone.

---

## 15. Required Automated Test Groups

At minimum, P07.9 must add deterministic tests covering:

### A. Instant compatibility
- CastTime=0 executes with existing synchronous behavior.
- Existing P01–P07.8 regression remains green.

### B. Cast progression
- Cast starts.
- Time advances partially.
- Effect has not executed before completion.
- Completion executes exactly once.

### C. Channel
- Channel starts at the correct lifecycle point.
- Tick timing is deterministic.
- Tick count is deterministic.
- Final completion occurs exactly once.

### D. Hard CC interruption
- Stun interrupts cast.
- Freeze interrupts cast/channel.
- Interrupted skill does not execute its delayed effect.

### E. Root isolation
- Root does not interrupt cast/channel.
- Root movement restriction remains owned by `EntityStatusController`/movement authority.

### F. Damage isolation
- Ordinary incoming damage does not interrupt cast.

### G. Rage
- Rage is consumed at Cast Start.
- Interrupted cast does not refund Rage.
- Failed validation before Cast Start does not consume Rage.

### H. Cooldown
- Successful cast starts cooldown after successful completion.
- Interrupted cast before completion does not start normal cooldown.
- Existing instant-skill cooldown behavior remains unchanged.

### I. Death
- Caster death during cast prevents completion.
- Target death does not allow later invalid damage to be applied.

### J. P07.8 shield pipeline
- Channel tick damage is intercepted by the existing shield authority before HP loss.

### K. Ultimate validation
- Ultimate execution is blocked when `CanUseUltimate` is false.

---

## 16. Regression Requirements

Before P07.9 can be LOCKED as an implementation milestone:

- P01–P07.8 Master Regression must remain PASS.
- P07.9 automated tests must PASS.
- P07.9 Unity Play Mode tests must PASS.
- Required visual checks must PASS where applicable.
- No duplicate authority is introduced.
- No P07.8 shield regression is introduced.
- No P07.7 Cleanse/Dispel regression is introduced.
- No P07.6 CC/resistance/Anti-CC regression is introduced.

A report claiming PASS is not sufficient by itself if the actual execution evidence is missing.

---

## 17. Explicit Non-Goals / Not Locked

The following are intentionally NOT fixed by this design contract:

- Official production cast durations.
- Official production channel durations.
- Official production tick intervals.
- Official skill balance values.
- Exact UI art/animation.
- Exact Animator integration.
- Exact animation-event implementation.
- Future CC types beyond current locked Stun/Root/Freeze behavior.
- Ordinary-damage interruption.
- New interrupt/refund policies beyond the locked Rage rule.
- New targeting/reacquisition behavior not already established by P01+.
- Any automatic skill queue beyond the existing skill-selection/priority architecture.

Do not silently turn any of these into permanent gameplay rules during implementation.

---

## 18. Implementation Order

Recommended implementation order:

1. Apply Ultimate `CanUseUltimate` validation regression fix.
2. Add optional data-driven cast/channel configuration fields with backward-compatible defaults.
3. Extend the existing Entity interruption linkage.
4. Implement one deterministic active-cast lifecycle within the existing SkillExecutor authority.
5. Integrate deterministic time progression.
6. Integrate Rage consumption at Cast Start.
7. Integrate successful-completion cooldown timing.
8. Integrate Stun/Freeze interruption and Root isolation.
9. Integrate channel ticks through existing Effect/Damage/Health/Shield authorities.
10. Add EventBus lifecycle notifications if needed.
11. Add automated tests.
12. Run P07.9 Play Mode tests.
13. Perform visual verification.
14. Run full P01–P07.9 Master Regression.
15. Only then evaluate implementation LOCK status.

---

## 19. Authority Rule

This file is the **approved P07.9 design contract** for the decisions explicitly listed above.

It does not override higher-priority P01+ locked runtime behavior unless this document explicitly extends it for P07.9.

If implementation reveals a better behavior, do not silently change this contract. Record the discovery, present the conflict/alternative, obtain an explicit decision, then update the authority/changelog before treating the new behavior as locked.

**Final rule:** P07.9 must extend the existing architecture, not replace it.
