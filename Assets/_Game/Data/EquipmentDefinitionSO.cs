using UnityEngine;
using WuxiaGame.Items;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "EquipmentDefinition", menuName = "WuxiaGame/Data/EquipmentDefinition")]
    public class EquipmentDefinitionSO : ScriptableObject
    {
        [Header("Equipment Definition Info")]
        [SerializeField] private string definitionId = "eq_weapon_01";
        [SerializeField] private string itemName = "Wuxia Sword";
        [SerializeField] private EquipmentSlotType slotType = EquipmentSlotType.Weapon;
        [SerializeField] private Sprite icon;

        public string DefinitionId => definitionId;
        public string ItemName => itemName;
        public EquipmentSlotType SlotType => slotType;
        public Sprite Icon => icon;

        public void InitializeDefinition(string id, string name, EquipmentSlotType slot, Sprite itemIcon = null)
        {
            definitionId = id;
            itemName = name;
            slotType = slot;
            icon = itemIcon;
        }
    }
}
