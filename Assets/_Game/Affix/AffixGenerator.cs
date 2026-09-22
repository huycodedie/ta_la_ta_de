using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Items;

namespace WuxiaGame.Affix
{
    public static class AffixGenerator
    {
        public static List<AffixInstance> GenerateAffixes(AffixDatabaseSO database, EquipmentSlotType slot, RarityDefinitionSO rarity, int equipmentLevel)
        {
            List<AffixInstance> result = new List<AffixInstance>();
            if (database == null || database.Affixes == null || database.Affixes.Count == 0 || rarity == null)
            {
                return result;
            }

            int requestedCount = Random.Range(rarity.MinAffixCount, rarity.MaxAffixCount + 1);

            // Filter allowed affixes for slot
            List<AffixDefinitionSO> pool = new List<AffixDefinitionSO>();
            foreach (var aff in database.Affixes)
            {
                if (aff != null && aff.IsAllowedForSlot(slot))
                {
                    pool.Add(aff);
                }
            }

            if (pool.Count == 0) pool = new List<AffixDefinitionSO>(database.Affixes);

            int targetCount = Mathf.Min(requestedCount, pool.Count);

            for (int i = 0; i < targetCount; i++)
            {
                if (pool.Count == 0) break;

                // Weighted random selection
                float totalWeight = 0f;
                foreach (var a in pool) totalWeight += a.Weight;

                AffixDefinitionSO selected = null;
                if (totalWeight > 0f)
                {
                    float roll = Random.Range(0f, totalWeight);
                    float cumulative = 0f;
                    foreach (var a in pool)
                    {
                        cumulative += a.Weight;
                        if (roll <= cumulative)
                        {
                            selected = a;
                            break;
                        }
                    }
                }

                if (selected == null)
                {
                    selected = pool[Random.Range(0, pool.Count)];
                }

                // REMOVE from pool to guarantee NO DUPLICATES
                pool.Remove(selected);

                float val = AffixValueCalculator.CalculateAffixValue(selected, equipmentLevel, rarity);
                result.Add(new AffixInstance(selected.AffixId, selected.DisplayName, selected.StatType, val));
            }

            return result;
        }
    }
}
