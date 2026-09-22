using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "EquipmentUpgradeConfig", menuName = "Wuxia/Equipment Upgrade Config")]
    public class EquipmentUpgradeConfigSO : ScriptableObject
    {
        [SerializeField] private int defaultMaxLevel = 5;
        [SerializeField] private float statScalePerLevel = 0.20f; // +20% per level over base
        [SerializeField] private List<EquipmentUpgradeCostData> upgradeCosts = new List<EquipmentUpgradeCostData>();

        public int DefaultMaxLevel => defaultMaxLevel;
        public float StatScalePerLevel => statScalePerLevel;
        public IReadOnlyList<EquipmentUpgradeCostData> UpgradeCosts => upgradeCosts;

        public void InitializeConfig(int maxLevel, float scalePerLevel, List<EquipmentUpgradeCostData> costs)
        {
            defaultMaxLevel = maxLevel;
            statScalePerLevel = scalePerLevel;
            upgradeCosts = costs ?? new List<EquipmentUpgradeCostData>();
        }

        public EquipmentUpgradeCostData GetCostForLevel(int currentLevel)
        {
            foreach (var cost in upgradeCosts)
            {
                if (cost.FromLevel == currentLevel)
                {
                    return cost;
                }
            }

            // Fallback default formula: Gold = currentLevel * 1000, Material = currentLevel * 2
            return new EquipmentUpgradeCostData(currentLevel, currentLevel + 1, currentLevel * 1000, currentLevel * 2);
        }
    }
}
