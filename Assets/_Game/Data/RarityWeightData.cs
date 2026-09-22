using System;
using UnityEngine;

namespace WuxiaGame.Data
{
    [Serializable]
    public class RarityWeightData
    {
        [SerializeField] private RarityDefinitionSO rarity;
        [SerializeField] private float weightPercentage; // 0 to 100%

        public RarityDefinitionSO Rarity => rarity;
        public float WeightPercentage => weightPercentage;

        public RarityWeightData(RarityDefinitionSO itemRarity, float weight)
        {
            rarity = itemRarity;
            weightPercentage = weight;
        }
    }
}
