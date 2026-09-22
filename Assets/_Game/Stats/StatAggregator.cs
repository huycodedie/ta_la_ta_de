using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Equipment;

namespace WuxiaGame.Stats
{
    public static class StatAggregator
    {
        public static Dictionary<StatType, float> CalculateFinalStats(
            Dictionary<StatType, float> baseStats,
            EquipmentManager equipmentManager,
            Dictionary<StatType, float> mindMethodPassiveStats = null,
            Dictionary<StatType, float> buffStats = null,
            Dictionary<StatType, float> debuffStats = null)
        {
            Dictionary<StatType, float> finalStats = new Dictionary<StatType, float>();

            if (baseStats != null)
            {
                foreach (var kvp in baseStats)
                {
                    finalStats[kvp.Key] = kvp.Value;
                }
            }

            if (equipmentManager != null)
            {
                Dictionary<StatType, float> equipStats = equipmentManager.GetTotalEquipmentStats();
                foreach (var kvp in equipStats)
                {
                    StatType type = kvp.Key;
                    float bonus = kvp.Value;

                    if (!finalStats.ContainsKey(type))
                    {
                        finalStats[type] = 0f;
                    }
                    finalStats[type] += bonus;
                }
            }

            if (mindMethodPassiveStats != null)
            {
                foreach (var kvp in mindMethodPassiveStats)
                {
                    StatType type = kvp.Key;
                    float bonus = kvp.Value;

                    if (!finalStats.ContainsKey(type))
                    {
                        finalStats[type] = 0f;
                    }
                    finalStats[type] += bonus;
                }
            }

            if (buffStats != null)
            {
                foreach (var kvp in buffStats)
                {
                    StatType type = kvp.Key;
                    float bonus = kvp.Value;

                    if (!finalStats.ContainsKey(type))
                    {
                        finalStats[type] = 0f;
                    }
                    finalStats[type] += bonus;
                }
            }

            if (debuffStats != null)
            {
                foreach (var kvp in debuffStats)
                {
                    StatType type = kvp.Key;
                    float penalty = kvp.Value;

                    if (!finalStats.ContainsKey(type))
                    {
                        finalStats[type] = 0f;
                    }
                    finalStats[type] -= penalty;

                    if (type == StatType.MoveSpeed)
                    {
                        finalStats[type] = Mathf.Max(0.1f, finalStats[type]);
                    }
                    else if (type != StatType.Health && type != StatType.MaxHealth)
                    {
                        finalStats[type] = Mathf.Max(0f, finalStats[type]);
                    }
                }
            }

            return finalStats;
        }
    }
}
