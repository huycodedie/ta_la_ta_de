using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Items;
using WuxiaGame.Stats;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "AffixDefinition", menuName = "WuxiaGame/Data/AffixDefinition")]
    public class AffixDefinitionSO : ScriptableObject
    {
        [Header("Affix Info")]
        [SerializeField] private string affixId = "affix_atk";
        [SerializeField] private string displayName = "+ ATK";
        [SerializeField] private StatType statType = StatType.Attack;

        [Header("Base Value Range")]
        [SerializeField] private float baseMinValue = 10f;
        [SerializeField] private float baseMaxValue = 20f;
        [SerializeField] private float weight = 100f;

        [Header("Allowed Slots (Empty = All Slots)")]
        [SerializeField] private List<EquipmentSlotType> allowedSlotTypes = new List<EquipmentSlotType>();

        public string AffixId => affixId;
        public string DisplayName => displayName;
        public StatType StatType => statType;
        public float BaseMinValue => baseMinValue;
        public float BaseMaxValue => baseMaxValue;
        public float Weight => weight;
        public IReadOnlyList<EquipmentSlotType> AllowedSlotTypes => allowedSlotTypes;

        public bool IsAllowedForSlot(EquipmentSlotType slot)
        {
            if (allowedSlotTypes == null || allowedSlotTypes.Count == 0) return true;
            return allowedSlotTypes.Contains(slot);
        }

        public void InitializeAffix(string id, string name, StatType stat, float minVal, float maxVal, float affixWeight, List<EquipmentSlotType> allowedSlots = null)
        {
            affixId = id;
            displayName = name;
            statType = stat;
            baseMinValue = minVal;
            baseMaxValue = maxVal;
            weight = affixWeight;
            allowedSlotTypes = allowedSlots ?? new List<EquipmentSlotType>();
        }
    }
}
