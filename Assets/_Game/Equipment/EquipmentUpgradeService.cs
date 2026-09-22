using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Affix;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Items;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Equipment
{
    public struct EquipmentUpgradePreviewData
    {
        public bool IsValid;
        public bool CanUpgrade;
        public bool IsMaxLevel;
        public bool HasEnoughResources;
        public int CurrentLevel;
        public int NextLevel;
        public int MaxLevel;
        public int GoldCost;
        public int MaterialCost;
        public List<(string AffixName, StatType StatType, float CurrentValue, float NextValue, float Diff)> AffixDiffs;
    }

    public class EquipmentUpgradeService : MonoBehaviour
    {
        public static EquipmentUpgradeService Instance { get; private set; }

        [SerializeField] private EquipmentUpgradeConfigSO upgradeConfig;

        public EquipmentUpgradeConfigSO UpgradeConfig => upgradeConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadConfigIfMissing();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadConfigIfMissing()
        {
            if (upgradeConfig == null)
            {
                upgradeConfig = Resources.Load<EquipmentUpgradeConfigSO>("Data/EquipmentUpgradeConfig");
#if UNITY_EDITOR
                if (upgradeConfig == null)
                {
                    upgradeConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentUpgradeConfigSO>("Assets/_Game/Data/EquipmentUpgradeConfig.asset");
                }
#endif
            }
        }

        public EquipmentUpgradePreviewData GetUpgradePreview(EquipmentInstance item)
        {
            LoadConfigIfMissing();

            EquipmentUpgradePreviewData preview = new EquipmentUpgradePreviewData
            {
                IsValid = false,
                CanUpgrade = false,
                IsMaxLevel = false,
                HasEnoughResources = false,
                AffixDiffs = new List<(string, StatType, float, float, float)>()
            };

            if (item == null) return preview;

            preview.IsValid = true;
            preview.CurrentLevel = item.EquipmentLevel;
            preview.MaxLevel = item.MaxLevel;

            if (item.EquipmentLevel >= item.MaxLevel)
            {
                preview.IsMaxLevel = true;
                preview.CanUpgrade = false;
                return preview;
            }

            int nextLevel = item.EquipmentLevel + 1;
            preview.NextLevel = nextLevel;

            EquipmentUpgradeCostData cost = upgradeConfig != null
                ? upgradeConfig.GetCostForLevel(item.EquipmentLevel)
                : new EquipmentUpgradeCostData(item.EquipmentLevel, nextLevel, item.EquipmentLevel * 1000, item.EquipmentLevel * 2);

            preview.GoldCost = cost.GoldCost;
            preview.MaterialCost = cost.MaterialCost;

            ResourceManager resMgr = ResourceManager.Instance != null ? ResourceManager.Instance : UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
            preview.HasEnoughResources = resMgr != null && resMgr.HasResources(cost.GoldCost, cost.MaterialCost);
            preview.CanUpgrade = preview.HasEnoughResources;

            float scale = upgradeConfig != null ? upgradeConfig.StatScalePerLevel : 0.20f;

            if (item.Affixes != null)
            {
                foreach (var affix in item.Affixes)
                {
                    if (affix == null) continue;
                    float curVal = affix.Value;
                    float nextVal = AffixValueCalculator.CalculateUpgradeValue(affix.BaseValue, nextLevel, affix.StatType, scale);
                    float diff = nextVal - curVal;
                    preview.AffixDiffs.Add((affix.DisplayName, affix.StatType, curVal, nextVal, diff));
                }
            }

            return preview;
        }

        public bool UpgradeEquipment(EquipmentInstance item)
        {
            if (item == null) return false;

            LoadConfigIfMissing();

            if (!item.CanUpgrade || item.EquipmentLevel >= item.MaxLevel)
            {
                return false;
            }

            EquipmentUpgradeCostData cost = upgradeConfig != null
                ? upgradeConfig.GetCostForLevel(item.EquipmentLevel)
                : new EquipmentUpgradeCostData(item.EquipmentLevel, item.EquipmentLevel + 1, item.EquipmentLevel * 1000, item.EquipmentLevel * 2);

            ResourceManager resMgr = ResourceManager.Instance != null ? ResourceManager.Instance : UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
            if (resMgr == null || !resMgr.ConsumeResources(cost.GoldCost, cost.MaterialCost))
            {
                return false;
            }

            // Exactly 1 level increase
            int newLevel = item.EquipmentLevel + 1;
            item.SetLevel(newLevel);

            float scale = upgradeConfig != null ? upgradeConfig.StatScalePerLevel : 0.20f;

            // Recalculate affixes in-place without changing identity
            if (item.Affixes != null)
            {
                foreach (var affix in item.Affixes)
                {
                    if (affix == null) continue;
                    float newVal = AffixValueCalculator.CalculateUpgradeValue(affix.BaseValue, newLevel, affix.StatType, scale);
                    affix.SetValue(newVal);
                }
            }

            // If item is currently equipped, refresh Hero stats immediately
            EquipmentManager eqMgr = EquipmentManager.Instance != null ? EquipmentManager.Instance : UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();
            if (eqMgr != null && eqMgr.GetEquippedItem(item.SlotType) == item)
            {
                Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
                if (hero != null)
                {
                    hero.RecalculateStats();
                }
            }

            return true;
        }
    }
}
