using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "DropLevelConfig", menuName = "WuxiaGame/Data/DropLevelConfig")]
    public class DropLevelConfigSO : ScriptableObject
    {
        [Header("Drop Level Info")]
        [SerializeField] private int dropLevel = 16;
        [SerializeField] private List<RarityWeightData> rarityWeights = new List<RarityWeightData>();

        public int DropLevel => dropLevel;
        public string DisplayName => $"Cấp Rơi {dropLevel}";
        public IReadOnlyList<RarityWeightData> RarityWeights => rarityWeights;

        public void InitializeDropLevel(int level, List<RarityWeightData> weights)
        {
            dropLevel = level;
            rarityWeights = weights ?? new List<RarityWeightData>();
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

            float roll = Random.Range(0f, totalWeight);
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
            if (dropLevel <= 0)
            {
                errorMessage = $"Invalid DropLevel {dropLevel}. Must be positive.";
                return false;
            }

            if (rarityWeights == null || rarityWeights.Count == 0)
            {
                errorMessage = $"DropLevel {dropLevel} has no rarity weights defined.";
                return false;
            }

            float total = 0f;
            HashSet<string> seenRarities = new HashSet<string>();

            foreach (var rw in rarityWeights)
            {
                if (rw == null || rw.Rarity == null)
                {
                    errorMessage = $"DropLevel {dropLevel} contains a null rarity entry.";
                    return false;
                }

                if (rw.WeightPercentage < 0f)
                {
                    errorMessage = $"DropLevel {dropLevel} rarity {rw.Rarity.DisplayName} has negative weight {rw.WeightPercentage}.";
                    return false;
                }

                if (seenRarities.Contains(rw.Rarity.RarityId))
                {
                    errorMessage = $"DropLevel {dropLevel} contains duplicate rarity entry: {rw.Rarity.DisplayName}.";
                    return false;
                }
                seenRarities.Add(rw.Rarity.RarityId);
                total += rw.WeightPercentage;
            }

            if (total <= 0f)
            {
                errorMessage = $"DropLevel {dropLevel} has zero total rarity weight.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
