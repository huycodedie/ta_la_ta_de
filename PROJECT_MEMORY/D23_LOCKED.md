# D23 — LOOT / EQUIPMENT — CURRENT LOCKED BASELINE

## Core loot
- Approximately 95% of gear comes from chests; approximately 5% from stage/event rewards under the current locked baseline.
- Normal monsters share the same drop-rate rule.
- Current baseline selected by the user: each normal monster death drops exactly 1 item (not 0/1).
- Item Level is based on current Hero Level with `abs(ItemLevel - HeroLevel) <= 5`.
- Cấp Rơi determines available quality/rarity and probability.
- Higher rarities may exist beyond those visible at low Cấp Rơi; 0% can mean not unlocked/available.

## Chest / Drop Level
- `Chest Level = Drop Level = Cấp Rơi`.
- One system and one runtime authority.
- Chest upgrade uses Gold + Time and follows D22 locked flow.

## Equipment slots
There are 12 wearable slots:
1. Vũ khí/Kiếm
2. Mũ
3. Khăn che mặt
4. Áo
5. Quần
6. Giày
7. Găng tay
8. Đai lưng
9. Áo choàng
10. Dây chuyền
11. Nhẫn
12. Bùa/Ngọc

Do not use `SPECIAL` as a player-facing slot name.

## Recycle / Auto-Recycle
- Equipment with CP lower than the currently equipped item is eligible for Auto-Recycle under the base D23 rule.
- Recycle reward is GOLD ONLY; no EXP.
- Future Preferred Attribute/Affix protection can keep an otherwise eligible item. This extends the base rule rather than replacing it.
- Final decision flow: eligibility -> protection/keep conditions -> recycle.

## Rarity / quality
- Rarity is data-driven and extensible.
- The nine qualities visible in the reference UI are not the maximum.
- Never hard-code a maximum rarity or unlock table without explicit approval.

## TBD
Exact rarity probabilities, complete rarity table/unlock thresholds, chest costs/timers beyond D22, open cost, affix ranges, CP weights, loot pool details and Item Level distribution within ±5 remain TBD unless separately locked.
