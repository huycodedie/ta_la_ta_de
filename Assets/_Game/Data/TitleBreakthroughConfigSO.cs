using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Equipment;
using WuxiaGame.Stats;

namespace WuxiaGame.Data
{
    public enum BreakthroughRequirementType
    {
        HeroLevel = 0,
        Gold = 1,
        Material = 2,
        LootTier = 3,
        EquipmentCount = 4,
        SpecificEquipment = 5,
        SpecificRarity = 6,
        Custom = 7,
        MindMethodLevel = 8
    }

    [Serializable]
    public class BreakthroughRequirement
    {
        [SerializeField] private BreakthroughRequirementType type = BreakthroughRequirementType.HeroLevel;
        [SerializeField] private int requiredValue = 0;
        [SerializeField] private string targetId = "";
        [SerializeField] private int targetRarityOrder = 0;
        [SerializeField] private string customDescription = "";

        public BreakthroughRequirementType Type { get => type; set => type = value; }
        public int RequiredValue { get => requiredValue; set => requiredValue = value; }
        public string TargetId { get => targetId; set => targetId = value; }
        public int TargetRarityOrder { get => targetRarityOrder; set => targetRarityOrder = value; }
        public string CustomDescription { get => customDescription; set => customDescription = value; }

        public BreakthroughRequirement() { }

        public BreakthroughRequirement(BreakthroughRequirementType reqType, int value, string id = "", int rarityOrder = 0, string desc = "")
        {
            type = reqType;
            requiredValue = value;
            targetId = id;
            targetRarityOrder = rarityOrder;
            customDescription = desc;
        }

        public BreakthroughRequirement Clone()
        {
            return new BreakthroughRequirement(type, requiredValue, targetId, targetRarityOrder, customDescription);
        }
    }

    [Serializable]
    public struct TitleStatModifier
    {
        public StatType statType;
        public float value;

        public TitleStatModifier(StatType type, float val)
        {
            statType = type;
            value = val;
        }
    }

    [CreateAssetMenu(fileName = "TitleBreakthroughConfig", menuName = "WuxiaGame/Data/TitleBreakthroughConfig")]
    public class TitleBreakthroughConfigSO : ScriptableObject
    {
        [Header("Breakthrough Meta")]
        [SerializeField] private string breakthroughId = "title_00";
        [SerializeField] private string titleName = "No Title";
        [SerializeField] private int displayOrder = 0;
        [SerializeField] private string description = "";

        [Header("Level Caps & Progression")]
        [SerializeField] private int currentLevelCap = 5;
        [SerializeField] private int nextLevelCap = 10;

        [Header("Data-Driven Requirements (Flexible & Extensible)")]
        [SerializeField] private List<BreakthroughRequirement> requirements = new List<BreakthroughRequirement>();

        [Header("Legacy Requirements (Kept for fallback/direct access)")]
        [SerializeField] private int requiredHeroLevel = 5;
        [SerializeField] private int requiredGold = 5000;
        [SerializeField] private int requiredMaterial = 30;
        [SerializeField] private int requiredStageCleared = 0;
        [SerializeField] private string requiredPreviousTitleId = "";

        [Header("Base Combat Stats")]
        [SerializeField] private float baseMaxHealth = 1000f;
        [SerializeField] private float baseAttack = 100f;
        [SerializeField] private float baseDefense = 20f;
        [SerializeField] private float bonusCritRate = 0f;
        [SerializeField] private float bonusCritDamage = 0f;
        [SerializeField] private float bonusDodge = 0f;

        [Header("Custom Modifiers (Optional Future Expansion)")]
        [SerializeField] private List<TitleStatModifier> customModifiers = new List<TitleStatModifier>();

        public string BreakthroughId => breakthroughId;
        public string TitleName => titleName;
        public int DisplayOrder => displayOrder;
        public string Description => description;
        public int CurrentLevelCap => currentLevelCap;
        public int NextLevelCap => nextLevelCap;
        public int RequiredHeroLevel => requiredHeroLevel;
        public int RequiredGold => requiredGold;
        public int RequiredMaterial => requiredMaterial;
        public int RequiredStageCleared => requiredStageCleared;
        public string RequiredPreviousTitleId => requiredPreviousTitleId;

        public IReadOnlyList<BreakthroughRequirement> Requirements => GetRequirements();

        public float BaseMaxHealth => baseMaxHealth;
        public float BaseAttack => baseAttack;
        public float BaseDefense => baseDefense;
        public float BonusCritRate => bonusCritRate;
        public float BonusCritDamage => bonusCritDamage;
        public float BonusDodge => bonusDodge;
        public IReadOnlyList<TitleStatModifier> CustomModifiers => customModifiers;

        public List<BreakthroughRequirement> GetRequirements()
        {
            if (requirements == null) requirements = new List<BreakthroughRequirement>();
            if (requirements.Count == 0)
            {
                // Fallback / default population from fields if empty
                var list = new List<BreakthroughRequirement>();
                if (requiredHeroLevel > 0) list.Add(new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, requiredHeroLevel));
                if (requiredGold > 0) list.Add(new BreakthroughRequirement(BreakthroughRequirementType.Gold, requiredGold));
                if (requiredMaterial > 0) list.Add(new BreakthroughRequirement(BreakthroughRequirementType.Material, requiredMaterial));
                return list;
            }
            return requirements;
        }

        public void SetRequirements(List<BreakthroughRequirement> reqs)
        {
            requirements = reqs != null ? new List<BreakthroughRequirement>(reqs) : new List<BreakthroughRequirement>();
            // Also sync legacy convenience fields
            foreach (var r in requirements)
            {
                if (r.Type == BreakthroughRequirementType.HeroLevel) requiredHeroLevel = r.RequiredValue;
                else if (r.Type == BreakthroughRequirementType.Gold) requiredGold = r.RequiredValue;
                else if (r.Type == BreakthroughRequirementType.Material) requiredMaterial = r.RequiredValue;
            }
        }

        public void InitializeBreakthrough(
            string id,
            string name,
            int order,
            int curCap,
            int nextCap,
            int reqLevel,
            int reqGold,
            int reqMat,
            float hp,
            float atk,
            float def,
            float critRate = 0f,
            float critDmg = 0f,
            float dodge = 0f,
            string desc = "",
            int reqStage = 0,
            string reqPrevTitle = "",
            List<BreakthroughRequirement> customReqs = null)
        {
            breakthroughId = id;
            titleName = name;
            displayOrder = order;
            currentLevelCap = curCap;
            nextLevelCap = nextCap;
            requiredHeroLevel = reqLevel;
            requiredGold = reqGold;
            requiredMaterial = reqMat;
            baseMaxHealth = hp;
            baseAttack = atk;
            baseDefense = def;
            bonusCritRate = critRate;
            bonusCritDamage = critDmg;
            bonusDodge = dodge;
            description = desc;
            requiredStageCleared = reqStage;
            requiredPreviousTitleId = reqPrevTitle;
            if (customModifiers == null) customModifiers = new List<TitleStatModifier>();

            if (customReqs != null && customReqs.Count > 0)
            {
                requirements = new List<BreakthroughRequirement>(customReqs);
            }
            else
            {
                requirements = new List<BreakthroughRequirement>();
                if (reqLevel > 0) requirements.Add(new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, reqLevel));
                if (reqGold > 0) requirements.Add(new BreakthroughRequirement(BreakthroughRequirementType.Gold, reqGold));
                if (reqMat > 0) requirements.Add(new BreakthroughRequirement(BreakthroughRequirementType.Material, reqMat));
            }
        }

        public bool ValidateConfig(out string error)
        {
            if (string.IsNullOrEmpty(breakthroughId))
            {
                error = "Breakthrough ID cannot be null or empty.";
                return false;
            }

            if (string.IsNullOrEmpty(titleName))
            {
                error = $"Title Name for '{breakthroughId}' cannot be null or empty.";
                return false;
            }

            if (nextLevelCap <= currentLevelCap && nextLevelCap > 0)
            {
                error = $"Title '{breakthroughId}' nextLevelCap ({nextLevelCap}) must be greater than currentLevelCap ({currentLevelCap}).";
                return false;
            }

            if (baseMaxHealth <= 0f || baseAttack <= 0f)
            {
                error = $"Title '{breakthroughId}' base stats (HP: {baseMaxHealth}, ATK: {baseAttack}) must be positive.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
