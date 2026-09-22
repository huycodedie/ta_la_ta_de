using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.Items;

namespace WuxiaGame.Stats
{
    public static class CombatPowerCalculator
    {
        // Deterministic Stat Weights for authoritative combat power
        public const float WeightHp = 1.0f;
        public const float WeightAtk = 5.0f;
        public const float WeightDef = 5.0f;
        public const float WeightCritRate = 20.0f;
        public const float WeightCritDamage = 10.0f;
        public const float WeightDodge = 20.0f;
        public const float WeightLifesteal = 20.0f;
        public const float WeightComboRate = 20.0f;
        public const float WeightCounterRate = 20.0f;

        /// <summary>
        /// Pure calculation of Combat Power from a stats dictionary.
        /// </summary>
        public static int CalculatePower(Dictionary<StatType, float> stats)
        {
            if (stats == null) return 0;
            float total = 0f;

            float hp = 0f;
            if (stats.TryGetValue(StatType.MaxHealth, out float maxH)) hp = maxH;
            else if (stats.TryGetValue(StatType.Health, out float curH)) hp = curH;

            float atk = stats.TryGetValue(StatType.Attack, out float a) ? a : 0f;
            float def = stats.TryGetValue(StatType.Defense, out float d) ? d : 0f;
            float critRate = stats.TryGetValue(StatType.CritRate, out float cr) ? cr : 0f;
            float critDmg = stats.TryGetValue(StatType.CritDamage, out float cd) ? cd : 0f;
            float dodge = stats.TryGetValue(StatType.Dodge, out float dg) ? dg : 0f;
            float lifesteal = stats.TryGetValue(StatType.Lifesteal, out float ls) ? ls : 0f;
            float combo = stats.TryGetValue(StatType.ComboRate, out float cb) ? cb : 0f;
            float counter = stats.TryGetValue(StatType.CounterRate, out float ct) ? ct : 0f;

            total += hp * WeightHp;
            total += atk * WeightAtk;
            total += def * WeightDef;
            total += critRate * WeightCritRate;
            total += critDmg * WeightCritDamage;
            total += dodge * WeightDodge;
            total += lifesteal * WeightLifesteal;
            total += combo * WeightComboRate;
            total += counter * WeightCounterRate;

            return Mathf.RoundToInt(total);
        }

        /// <summary>
        /// Pure calculation of Combat Power for an active Hero.
        /// </summary>
        public static int CalculatePower(Hero hero)
        {
            if (hero == null) return 0;
            Dictionary<StatType, float> currentStats = new Dictionary<StatType, float>();
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                currentStats[type] = hero.Stats.GetValue(type);
            }
            return CalculatePower(currentStats);
        }

        /// <summary>
        /// Pure calculation of Combat Power contribution for an individual item.
        /// </summary>
        public static int CalculateItemPower(EquipmentInstance item)
        {
            if (item == null || item.Affixes == null) return 0;
            Dictionary<StatType, float> itemStats = new Dictionary<StatType, float>();
            foreach (var affix in item.Affixes)
            {
                if (affix == null) continue;
                if (!itemStats.ContainsKey(affix.StatType)) itemStats[affix.StatType] = 0f;
                itemStats[affix.StatType] += affix.Value;
            }
            return CalculatePower(itemStats);
        }

        /// <summary>
        /// Pure, non-mutating comparison calculation.
        /// Returns current stats, projected stats, current power, projected power, and power diff.
        /// </summary>
        public static ComparisonResult CalculateComparison(Hero hero, EquipmentManager eqMgr, EquipmentInstance newItem)
        {
            Dictionary<StatType, float> currentStats = new Dictionary<StatType, float>();
            Dictionary<StatType, float> projectedStats = new Dictionary<StatType, float>();

            if (hero != null)
            {
                foreach (StatType type in Enum.GetValues(typeof(StatType)))
                {
                    float val = hero.Stats.GetValue(type);
                    currentStats[type] = val;
                    projectedStats[type] = val;
                }
            }
            else
            {
                // Fallback default base stats if Hero is null in unit tests
                currentStats[StatType.MaxHealth] = 1000f;
                currentStats[StatType.Attack] = 100f;
                currentStats[StatType.Defense] = 20f;
                currentStats[StatType.CritRate] = 5f;
                currentStats[StatType.CritDamage] = 150f;
                currentStats[StatType.Dodge] = 0f;
                currentStats[StatType.Lifesteal] = 0f;

                foreach (var kvp in currentStats)
                {
                    projectedStats[kvp.Key] = kvp.Value;
                }
            }

            EquipmentInstance currentEquipped = null;
            if (newItem != null && eqMgr != null)
            {
                currentEquipped = eqMgr.GetEquippedItem(newItem.SlotType);
            }

            // 1. Subtract current equipped item contribution if any
            if (currentEquipped != null && currentEquipped.Affixes != null)
            {
                foreach (var affix in currentEquipped.Affixes)
                {
                    if (affix == null) continue;
                    if (!projectedStats.ContainsKey(affix.StatType)) projectedStats[affix.StatType] = 0f;
                    projectedStats[affix.StatType] -= affix.Value;
                }
            }

            // 2. Add new item contribution if any
            if (newItem != null && newItem.Affixes != null)
            {
                foreach (var affix in newItem.Affixes)
                {
                    if (affix == null) continue;
                    if (!projectedStats.ContainsKey(affix.StatType)) projectedStats[affix.StatType] = 0f;
                    projectedStats[affix.StatType] += affix.Value;
                }
            }

            int currentPower = CalculatePower(currentStats);
            int projectedPower = CalculatePower(projectedStats);
            int powerDiff = projectedPower - currentPower;
            int currentItemPower = CalculateItemPower(currentEquipped);
            int newItemPower = CalculateItemPower(newItem);

            return new ComparisonResult
            {
                CurrentEquippedItem = currentEquipped,
                NewItem = newItem,
                CurrentStats = currentStats,
                ProjectedStats = projectedStats,
                CurrentPower = currentPower,
                ProjectedPower = projectedPower,
                PowerDifference = powerDiff,
                CurrentItemPower = currentItemPower,
                NewItemPower = newItemPower
            };
        }
    }

    public class ComparisonResult
    {
        public EquipmentInstance CurrentEquippedItem;
        public EquipmentInstance NewItem;
        public Dictionary<StatType, float> CurrentStats;
        public Dictionary<StatType, float> ProjectedStats;
        public int CurrentPower;
        public int ProjectedPower;
        public int PowerDifference;
        public int CurrentItemPower;
        public int NewItemPower;

        public float GetStatDelta(StatType stat)
        {
            float cur = (CurrentStats != null && CurrentStats.TryGetValue(stat, out float c)) ? c : 0f;
            float proj = (ProjectedStats != null && ProjectedStats.TryGetValue(stat, out float p)) ? p : 0f;
            return proj - cur;
        }

        public (float current, float projected, float delta) GetStatComparison(StatType stat)
        {
            float cur = (CurrentStats != null && CurrentStats.TryGetValue(stat, out float c)) ? c : 0f;
            float proj = (ProjectedStats != null && ProjectedStats.TryGetValue(stat, out float p)) ? p : 0f;
            return (cur, proj, proj - cur);
        }
    }
}
