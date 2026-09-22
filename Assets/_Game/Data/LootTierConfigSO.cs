using System;
using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "LootTierConfig", menuName = "WuxiaGame/Data/LootTierConfig")]
    public class LootTierConfigSO : ScriptableObject
    {
        [Header("Loot Tier Info")]
        [SerializeField] private int tierId = 1;
        [SerializeField] private string displayName = "Cấp Rơi 1";
        [SerializeField] private List<RarityWeightData> rarityWeights = new List<RarityWeightData>();

        [Header("Progression & Upgrade Requirements")]
        [SerializeField] private int requiredProgress = 2500;
        [SerializeField] private float upgradeDurationSeconds = 900f; // 15 mins default
        [SerializeField] private int upgradeCostGold = 0;
        [SerializeField] private int upgradeCostMaterial = 0;
        [SerializeField] private int requiredHeroLevel = 1;
        [SerializeField] private int requiredTitleIndex = 0;

        public int TierId => tierId;
        public string DisplayName => displayName;
        public IReadOnlyList<RarityWeightData> RarityWeights => rarityWeights;
        public int RequiredProgress => requiredProgress;
        public float UpgradeDurationSeconds => upgradeDurationSeconds;
        public int UpgradeCostGold => upgradeCostGold;
        public int UpgradeCostMaterial => upgradeCostMaterial;
        public int RequiredHeroLevel => requiredHeroLevel;
        public int RequiredTitleIndex => requiredTitleIndex;

        public void InitializeTier(int id, string name, List<RarityWeightData> weights, int reqProgress = 2500, float durationSeconds = 900f, int costGold = 0, int costMaterial = 0, int reqHeroLevel = 1, int reqTitle = 0)
        {
            tierId = id;
            displayName = name;
            rarityWeights = weights ?? new List<RarityWeightData>();
            requiredProgress = Mathf.Max(1, reqProgress);
            upgradeDurationSeconds = Mathf.Max(0f, durationSeconds);
            upgradeCostGold = Mathf.Max(0, costGold);
            upgradeCostMaterial = Mathf.Max(0, costMaterial);
            requiredHeroLevel = Mathf.Max(1, reqHeroLevel);
            requiredTitleIndex = Mathf.Max(0, reqTitle);
        }

        public float GetRarityWeight(string rarityId)
        {
            if (string.IsNullOrEmpty(rarityId) || rarityWeights == null) return 0f;
            foreach (var rw in rarityWeights)
            {
                if (rw != null && rw.Rarity != null && rw.Rarity.RarityId == rarityId)
                {
                    return rw.WeightPercentage;
                }
            }
            return 0f;
        }

        public bool IsRarityUnlocked(string rarityId)
        {
            return GetRarityWeight(rarityId) > 0f;
        }

        public List<RarityDefinitionSO> GetUnlockedRarities()
        {
            List<RarityDefinitionSO> list = new List<RarityDefinitionSO>();
            if (rarityWeights != null)
            {
                foreach (var rw in rarityWeights)
                {
                    if (rw != null && rw.Rarity != null && rw.WeightPercentage > 0f)
                    {
                        list.Add(rw.Rarity);
                    }
                }
            }
            return list;
        }

        public RarityDefinitionSO RollRarity()
        {
            if (rarityWeights == null || rarityWeights.Count == 0) return null;

            float totalWeight = 0f;
            foreach (var rw in rarityWeights)
            {
                if (rw != null && rw.Rarity != null && rw.WeightPercentage > 0f)
                {
                    totalWeight += rw.WeightPercentage;
                }
            }

            if (totalWeight <= 0f) return rarityWeights[0].Rarity;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var rw in rarityWeights)
            {
                if (rw == null || rw.Rarity == null || rw.WeightPercentage <= 0f) continue;
                cumulative += rw.WeightPercentage;
                if (roll <= cumulative)
                {
                    return rw.Rarity;
                }
            }

            return rarityWeights[rarityWeights.Count - 1].Rarity;
        }

        public bool Validate(out string errorMessage)
        {
            if (tierId <= 0)
            {
                errorMessage = $"Invalid TierId {tierId}. Must be positive.";
                return false;
            }

            if (rarityWeights == null || rarityWeights.Count == 0)
            {
                errorMessage = $"Tier {tierId} has no rarity weights defined.";
                return false;
            }

            float total = 0f;
            HashSet<string> seenRarities = new HashSet<string>();

            foreach (var rw in rarityWeights)
            {
                if (rw == null || rw.Rarity == null)
                {
                    errorMessage = $"Tier {tierId} contains a null rarity entry.";
                    return false;
                }

                if (rw.WeightPercentage < 0f)
                {
                    errorMessage = $"Tier {tierId} rarity {rw.Rarity.DisplayName} has negative weight {rw.WeightPercentage}.";
                    return false;
                }

                if (seenRarities.Contains(rw.Rarity.RarityId))
                {
                    errorMessage = $"Tier {tierId} contains duplicate rarity entry: {rw.Rarity.DisplayName}.";
                    return false;
                }
                seenRarities.Add(rw.Rarity.RarityId);
                total += rw.WeightPercentage;
            }

            if (total <= 0f)
            {
                errorMessage = $"Tier {tierId} has zero total rarity weight.";
                return false;
            }

            if (requiredProgress <= 0)
            {
                errorMessage = $"Tier {tierId} has invalid RequiredProgress {requiredProgress}. Must be positive.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
