# D21 — HERO LEVEL / EXP / TITLE BREAKTHROUGH — LOCKED

## EXP
- Lv1 -> Lv2: 100 EXP.
- Lv2 -> Lv3: 150 EXP.
- Requirement increases +50 per level through the Lv1-50 range.
- Then +100 per level through Lv51-100.
- Then +200 per level through Lv101-150.
- Do not derive values beyond the explicitly stated range.
- Normal monster kill: 10 EXP fixed.

## Level vs stats
- Hero Level does NOT directly increase HP/ATK/DEF.
- Hero Level primarily participates in progression/Title Breakthrough conditions.
- Title rank breakthrough immediately updates Hero base stats.
- Title-rank stat upgrades are non-stacking according to the locked design.

## Level Gate / excess EXP
- If Hero reaches the configured Title Level Cap and cannot Breakthrough, additional EXP is retained/accumulated rather than lost.
- This supersedes an older historical answer that said excess EXP resets to zero.

## Breakthrough
- Breakthrough does not consume Gold/Silver under the recovered locked rule.
- It checks the required Chest/Drop Level condition.
- All configured breakthrough conditions use AND semantics: every required condition must be satisfied.
- Title ranks/tiers are flexible and data-driven via ScriptableObject configuration.
- Breakthrough UI follows the established reference design; battle continues while the panel is open.

## Unknown / TBD
- Exact title tables, exact stat values, future level ranges beyond sourced values, and other unstated formulas remain TBD.
