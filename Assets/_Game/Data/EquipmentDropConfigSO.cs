using System;
using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [Serializable]
    public class LootTierWeightData
    {
        [SerializeField] private LootTierConfigSO lootTier;
        [SerializeField] private float weightPercentage;

        public LootTierConfigSO LootTier => lootTier;
        public float WeightPercentage => weightPercentage;

        public LootTierWeightData(LootTierConfigSO tier, float weight)
        {
            lootTier = tier;
            weightPercentage = weight;
        }
    }

    [CreateAssetMenu(fileName = "EquipmentDropConfig", menuName = "WuxiaGame/Data/EquipmentDropConfig")]
    public class EquipmentDropConfigSO : ScriptableObject
    {
        [Header("Drop Settings")]
        [SerializeField, Range(0, 100)] private float equipmentDropRate = 100f;
        [SerializeField] private List<LootTierWeightData> lootTierWeights = new List<LootTierWeightData>();

        public float EquipmentDropRate
        {
            get => equipmentDropRate;
            set => equipmentDropRate = Mathf.Clamp(value, 0f, 100f);
        }

        public IReadOnlyList<LootTierWeightData> LootTierWeights => lootTierWeights;

        public void InitializeConfig(float dropRate, List<LootTierWeightData> weights)
        {
            equipmentDropRate = dropRate;
            lootTierWeights = weights ?? new List<LootTierWeightData>();
        }

        public LootTierConfigSO RollLootTier()
        {
            if (lootTierWeights == null || lootTierWeights.Count == 0) return null;

            float totalWeight = 0f;
            foreach (var tw in lootTierWeights)
            {
                if (tw != null && tw.LootTier != null && tw.WeightPercentage > 0f)
                {
                    totalWeight += tw.WeightPercentage;
                }
            }

            if (totalWeight <= 0f) return lootTierWeights[0].LootTier;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var tw in lootTierWeights)
            {
                if (tw == null || tw.LootTier == null || tw.WeightPercentage <= 0f) continue;
                cumulative += tw.WeightPercentage;
                if (roll <= cumulative)
                {
                    return tw.LootTier;
                }
            }

            return lootTierWeights[lootTierWeights.Count - 1].LootTier;
        }

        public bool Validate(out string errorMessage)
        {
            if (equipmentDropRate < 0f || equipmentDropRate > 100f)
            {
                errorMessage = $"Invalid drop rate {equipmentDropRate}. Must be between 0 and 100.";
                return false;
            }

            if (lootTierWeights == null || lootTierWeights.Count == 0)
            {
                errorMessage = "No LootTier weights defined in drop config.";
                return false;
            }

            float total = 0f;
            HashSet<int> seenTiers = new HashSet<int>();

            foreach (var tw in lootTierWeights)
            {
                if (tw == null || tw.LootTier == null)
                {
                    errorMessage = "Contains a null LootTier entry.";
                    return false;
                }

                if (tw.WeightPercentage < 0f)
                {
                    errorMessage = $"Tier {tw.LootTier.TierId} has negative weight {tw.WeightPercentage}.";
                    return false;
                }

                if (seenTiers.Contains(tw.LootTier.TierId))
                {
                    errorMessage = $"Duplicate tier entry for Tier {tw.LootTier.TierId}.";
                    return false;
                }
                seenTiers.Add(tw.LootTier.TierId);
                total += tw.WeightPercentage;

                if (!tw.LootTier.Validate(out string tierErr))
                {
                    errorMessage = $"Tier {tw.LootTier.TierId} validation failed: {tierErr}";
                    return false;
                }
            }

            if (total <= 0f)
            {
                errorMessage = "Total LootTier weight is zero or negative.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
