# D1-D23 CURRENT AMENDMENTS — LOCKED

> This file records later explicit user decisions that supersede or clarify earlier recovered historical wording. It is part of the current Design Contract. Where this file conflicts with an older D1-D23 recovery file, this later decision wins.

## A1 — Bun = 0: Companion also stops

When Normal Monster Battle reaches Bun = 0:

```text
Bun = 0
  ↓
Normal Battle STOP
  ↓
Hero STOP
Companion STOP
Monster/Normal combat STOP
```

- Companion actions do not consume Bun while Normal Battle is active.
- However, once the Hero causes Normal Battle to enter the Bun-exhausted stopped state, Companions must also stop.
- Companion must not continue fighting indefinitely while the Normal Battle is `OUT_OF_BUN`.
- Boss Battle remains independent of Bun.

This clarifies/supersedes the intentionally vague historical D19.3 wording.

## A2 — Chest Level = Drop Level = one system

The sample game uses the label `Cấp rơi`. The user confirms that **Cấp Rương and Cấp Rơi are NOT two separate systems**. The sample game's naming is considered incorrect/confusing; the project treats them as one unified progression concept.

Canonical concept:

```text
Chest Level = Drop Level = Cấp Rơi
```

Do not create separate runtime authorities or progression systems for Chest Level and Drop Level.

## A3 — Item Level is based on current Hero Level

Item Level is determined from the Hero's **current Level**.

Constraint:

```text
|ItemLevel - HeroLevel| <= 5
```

In other words, an item's level must not differ from the current Hero Level by more than 5 levels.

The exact roll/distribution inside that allowed range is not separately specified here and remains data/configurable until explicitly decided.

Important: this supersedes the older D17/D16 recovery wording that treated Chest Level as directly determining the Item Level range. Chest/Drop Level determines loot quality/rarity; Hero Level determines the Item Level basis, subject to the +/-5 constraint.

## A4 — Item quality/rarity is unlocked progressively by Chest/Drop Level

Item quality (Phẩm chất) is determined by the unified Chest/Drop Level through data-driven rarity probabilities **and availability/unlock rules**.

The supplied reference screenshot shows the following quality tiers at the displayed Cấp Rơi 2 and Cấp Rơi 3:

1. `Thô Sơ`
2. `Thường`
3. `Tốt`
4. `Hiếm`
5. `Sử Thi`
6. `Truyền Kỳ`
7. `Thần Thoại`
8. `Tinh Khiết`
9. `Tối Thượng`

**Important correction:** these 9 rows are NOT a declaration that the game has only 9 total quality tiers. The user explicitly confirms that there are higher-quality tiers beyond what is currently available/visible at the lower Chest/Drop Levels.

The correct model is:

```text
Cấp Rơi thấp
    ↓
Chỉ mở/hiển thị các phẩm chất đã đạt điều kiện
    ↓
Các phẩm chất cao hơn chưa được mở
    ↓
Có thể hiển thị 0% ở Cấp Rơi hiện tại
```

Therefore a `0%` shown for a high-quality tier at a low Cấp Rơi must **not** be interpreted as “this rarity does not exist”. It can mean that the Cấp Rơi hiện tại has not unlocked that quality yet.

As Cấp Rơi increases:

```text
Cấp Rơi ↑
   ↓
Mở thêm các phẩm chất cao cấp
   ↓
Các phẩm chất mới có thể bắt đầu có tỷ lệ > 0%
```

The exact unlock threshold for every quality tier is **not yet specified** and must remain data-driven/configurable until explicitly decided.

The screenshot also demonstrates that each Cấp Rơi can have its own probability table. Reference evidence shown in the supplied image:

| Phẩm chất | Cấp hiện tại: 2 | Cấp tiếp theo: 3 |
|---|---:|---:|
| Thô Sơ | 51.5% | 39% |
| Thường | 25% | 30% |
| Tốt | 20% | 24% |
| Hiếm | 3.2% | 6.2% |
| Sử Thi | 0.3% | 0.8% |
| Truyền Kỳ | 0% | 0% |
| Thần Thoại | 0% | 0% |
| Tinh Khiết | 0% | 0% |
| Tối Thượng | 0% | 0% |

These displayed percentages are **reference/sample data from the supplied screenshot**, not a universal formula for every Cấp Rơi. The architecture must remain data-driven.

The screenshot's 9 visible rows are therefore **reference evidence of currently displayed tiers**, not a hard maximum of 9 tiers.

## A5 — Equipment: 12 player-worn slots

The project keeps exactly **12 equipment slots**, but replaces the provisional technical names with player-facing equipment concepts based on the sample game's worn equipment and the user's examples.

Current slot vocabulary:

1. `Vũ khí / Kiếm`
2. `Mũ`
3. `Khăn che mặt`
4. `Áo`
5. `Quần`
6. `Giày`
7. `Găng tay`
8. `Đai lưng`
9. `Áo choàng`
10. `Dây chuyền`
11. `Nhẫn`
12. `Bùa / Ngọc`

The exact final display wording may be polished later if the original game's UI supplies a more exact name, but the 12-slot structure and these equipment concepts are the current locked design direction. Do not keep `SPECIAL` as a player-facing name.

## A6 — Companion Attack Interval

The historical Companion Basic Attack interval of **2 seconds** is accepted as the current baseline/configurable value.

Hero Basic Attack interval of **1.5 seconds** remains the current baseline/configurable value.

They are intentionally independent values:

```text
Hero Attack Interval      = 1.5s baseline
Companion Attack Interval = 2.0s baseline
```

Do not merge them into one global hard-coded attack interval.

## A7 — Companion Combo / Counter

Companion may use the common Combo/Counter systems.

- Use the same common Combat Resolver/rules.
- No separate Companion Combo/Counter engine.
- Existing anti-recursion rules remain mandatory.
- Companion-specific rates/availability remain data-driven.

## A8 — Status/CC architecture after P01+

From the beginning of P01+ implementation and subsequent testing/fixes, **later tested/explicitly locked implementation decisions take precedence over earlier historical D12-D16 descriptions where they differ**.

In particular:

- Use the existing P07.6+ shared `EntityStatusController` authority.
- Use the existing `EffectResolver` / shared effect pipeline.
- Do not create a second Companion status controller.
- Later P07.x decisions about CC, Cleanse, Dispel, Shield, permissions and damage/status authority are the implementation authority when more specific.

The historical D12 processing-order text is therefore a recovery reference, not a command to revert P01+ through P07.x architecture.

## A9 — Chest upgrade during upgrade + automatic completion

The D22 upgrade behavior is confirmed as useful and must be tested as a real state machine:

```text
Lv N
 ↓
Start Upgrade Lv N+1
 ↓
UPGRADING
 ↓
(opening chest still uses old/current completed level)
 ↓
CurrentTime >= FinishTime
 ↓
AUTO COMPLETE
 ↓
ChestLevel = N+1
Status = IDLE
```

- No Claim button.
- No automatic chain to N+2.
- No queue.
- The player must manually start the next level after the current upgrade is complete.
- Upgrade progress must survive closing/reopening the game using persisted timestamps.

## A10 — Recycle equipment: Gold only

Equipment recycling returns **Gold only**.

- No Cultivation EXP reward is part of the current locked recycle result.
- Exact Gold amount/formula remains data/configurable until separately locked.
- Do not implement mandatory recycle EXP from the older D16/D17 historical wording.

## A11 — Auto Recycle precedence

The earlier D23 rule remains the base rule:

> Equipment with CP lower than the equipped item is eligible for automatic recycling.

Later, the automatic-management system may add player-selected conditions such as preferred attributes/affixes so that a lower-CP item can be **kept instead of recycled** when it contains a desired attribute.

Therefore the intended evolution is:

```text
Base rule:
Lower CP than equipped → Auto Recycle eligible

Extended rule:
Lower CP + user-selected keep/preferred attribute → Keep / protect from recycle
```

Preferred attributes are an exception/extension to the base Auto Recycle rule, not a replacement of the D23 base condition.

Auto Recycle must therefore evaluate eligibility and keep-protection rules before destroying/recycling an item.

## A12 — Current design precedence rule

For this project, design decisions evolve through implementation and testing:

```text
Initial Design
    ↓
P01+ Implementation
    ↓
Play Mode / Automated Tests
    ↓
Discovery / Bug / Better Behavior
    ↓
Explicit User Decision
    ↓
New Locked Decision
```

A historical D1-D23 statement is not automatically more authoritative than a later explicitly approved and tested P01+ behavior. When the later behavior is intentionally accepted by the user, it supersedes the older rule and must be recorded as such.

However, **do not infer a supersession merely because code differs**. Only an explicit user decision or clearly locked later milestone decision establishes the new rule.

## A13 — No silent redesign

These amendments do not authorize inventing any still-unknown values, including:
- exact item-level roll distribution within HeroLevel +/- 5;
- exact rarity probability tables beyond screenshot reference values;
- exact rarity unlock thresholds;
- exact total number of quality tiers beyond what has been explicitly established;
- exact Chest/Drop Level cost/time progression;
- exact affix values;
- exact CP weights;
- exact recycle Gold formula;
- other values explicitly marked TBD in earlier locked files.

## Status

This amendment file is the current authoritative addendum to the D1-D23 recovery files and must be consulted together with `D1_D23_LOCKED.md`, `D22_LOCKED.md`, `D15_D16_LOCKED.md`, and the P07.x locked implementation baselines.
