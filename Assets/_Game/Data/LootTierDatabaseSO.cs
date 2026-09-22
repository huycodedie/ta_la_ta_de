using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "LootTierDatabase", menuName = "WuxiaGame/Data/LootTierDatabase")]
    public class LootTierDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<LootTierConfigSO> tiers = new List<LootTierConfigSO>();

        public IReadOnlyList<LootTierConfigSO> Tiers => tiers;

        public void SetTiers(List<LootTierConfigSO> list)
        {
            tiers = list ?? new List<LootTierConfigSO>();
        }

        public LootTierConfigSO GetTier(int tierId)
        {
            if (tiers == null || tiers.Count == 0) return null;
            LootTierConfigSO match = tiers.Find(t => t != null && t.TierId == tierId);
            if (match != null) return match;
            return tiers[0];
        }

        public LootTierConfigSO GetFirstTier()
        {
            if (tiers == null || tiers.Count == 0) return null;
            return tiers[0];
        }

        public LootTierConfigSO GetNextTier(int currentTierId)
        {
            if (tiers == null || tiers.Count == 0) return null;

            int currentIndex = tiers.FindIndex(t => t != null && t.TierId == currentTierId);
            if (currentIndex >= 0 && currentIndex + 1 < tiers.Count)
            {
                return tiers[currentIndex + 1];
            }

            return null; // Max tier or not found
        }

        public bool IsMaxTier(int tierId)
        {
            return GetNextTier(tierId) == null;
        }

        public bool Validate(out string errorMessage)
        {
            if (tiers == null || tiers.Count == 0)
            {
                errorMessage = "LootTierDatabase contains no tiers.";
                return false;
            }

            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < tiers.Count; i++)
            {
                var tier = tiers[i];
                if (tier == null)
                {
                    errorMessage = $"LootTierDatabase contains null tier entry at index {i}.";
                    return false;
                }

                if (!tier.Validate(out string tierError))
                {
                    errorMessage = $"LootTierDatabase tier {tier.TierId} validation failed: {tierError}";
                    return false;
                }

                if (seenIds.Contains(tier.TierId))
                {
                    errorMessage = $"LootTierDatabase contains duplicate TierId {tier.TierId}.";
                    return false;
                }
                seenIds.Add(tier.TierId);
            }

            errorMessage = null;
            return true;
        }
    }
}
