using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Data;

namespace WuxiaGame.Items
{
    [Serializable]
    public class EquipmentInstance
    {
        [SerializeField] private string instanceId;
        [SerializeField] private string definitionId;
        [SerializeField] private string itemName;
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private int equipmentLevel;
        [SerializeField] private int maxLevel = 5;
        [SerializeField] private RarityDefinitionSO rarity;
        [SerializeField] private List<AffixInstance> affixes = new List<AffixInstance>();
        [SerializeField] private int lootTier = 1;

        public string InstanceId => instanceId;
        public string DefinitionId => definitionId;
        public string ItemName => itemName;
        public EquipmentSlotType SlotType => slotType;
        public int EquipmentLevel => equipmentLevel;
        public int MaxLevel => maxLevel;
        public bool CanUpgrade => equipmentLevel < maxLevel;
        public RarityDefinitionSO Rarity => rarity;
        public IReadOnlyList<AffixInstance> Affixes => affixes;
        public int LootTier => lootTier;

        public EquipmentInstance(string defId, string name, EquipmentSlotType slot, int level, RarityDefinitionSO itemRarity, List<AffixInstance> generatedAffixes, int maxLvl = 5, int tier = 1)
        {
            instanceId = Guid.NewGuid().ToString();
            definitionId = defId;
            itemName = name;
            slotType = slot;
            equipmentLevel = level;
            maxLevel = maxLvl > 0 ? maxLvl : 5;
            rarity = itemRarity;
            affixes = generatedAffixes ?? new List<AffixInstance>();
            lootTier = tier > 0 ? tier : 1;
        }

        public void SetLootTier(int tier)
        {
            lootTier = tier > 0 ? tier : 1;
        }

        public void SetLevel(int level)
        {
            equipmentLevel = level;
        }

        public void SetMaxLevel(int max)
        {
            maxLevel = max;
        }
    }
}
