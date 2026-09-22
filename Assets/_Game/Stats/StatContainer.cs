using System;
using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Stats
{
    public class StatContainer
    {
        private readonly Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

        public event Action<StatType, float> OnStatChanged;

        public Stat GetStat(StatType type)
        {
            if (stats.TryGetValue(type, out var stat))
            {
                return stat;
            }
            return null;
        }

        public float GetValue(StatType type, float defaultValue = 0f)
        {
            if (stats.TryGetValue(type, out var stat))
            {
                return stat.CurrentValue;
            }
            return defaultValue;
        }

        public void SetStat(StatType type, float baseValue)
        {
            if (stats.TryGetValue(type, out var stat))
            {
                stat.SetBaseValue(baseValue);
            }
            else
            {
                stat = new Stat(type, baseValue);
                stat.OnValueChanged += (val) => OnStatChanged?.Invoke(type, val);
                stats[type] = stat;
            }
        }

        public void ModifyValue(StatType type, float delta)
        {
            if (stats.TryGetValue(type, out var stat))
            {
                stat.CurrentValue += delta;
            }
        }

        public bool HasStat(StatType type)
        {
            return stats.ContainsKey(type);
        }
    }
}
