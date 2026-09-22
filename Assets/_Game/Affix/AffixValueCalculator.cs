using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Stats;

namespace WuxiaGame.Affix
{
    public static class AffixValueCalculator
    {
        public static float CalculateAffixValue(AffixDefinitionSO affix, int equipmentLevel, RarityDefinitionSO rarity)
        {
            if (affix == null) return 0f;

            float rawRoll = Random.Range(affix.BaseMinValue, affix.BaseMaxValue);
            float levelFactor = 1.0f + (Mathf.Max(1, equipmentLevel) - 1) * 0.1f;
            float rarityFactor = rarity != null ? rarity.StatMultiplier : 1.0f;

            float finalVal = rawRoll * levelFactor * rarityFactor;

            return RoundStatValue(finalVal, affix.StatType);
        }

        public static float CalculateUpgradeValue(float baseValueAtLv1, int targetLevel, StatType statType, float scalePerLevel = 0.20f)
        {
            float factor = 1.0f + (Mathf.Max(1, targetLevel) - 1) * scalePerLevel;
            float finalVal = baseValueAtLv1 * factor;
            return RoundStatValue(finalVal, statType);
        }

        private static float RoundStatValue(float val, StatType statType)
        {
            switch (statType)
            {
                case StatType.CritRate:
                case StatType.CritDamage:
                case StatType.Dodge:
                case StatType.Lifesteal:
                case StatType.ComboRate:
                case StatType.CounterRate:
                    return Mathf.Round(val * 10f) / 10f;
                default:
                    return Mathf.Round(val);
            }
        }
    }
}
