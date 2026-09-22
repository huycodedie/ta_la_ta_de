using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Items;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "WuxiaGame/Data/EquipmentDatabase")]
    public class EquipmentDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<EquipmentDefinitionSO> definitions = new List<EquipmentDefinitionSO>();

        public IReadOnlyList<EquipmentDefinitionSO> Definitions => definitions;

        public EquipmentDefinitionSO GetRandomDefinition()
        {
            if (definitions == null || definitions.Count == 0) return null;
            int idx = Random.Range(0, definitions.Count);
            return definitions[idx];
        }

        public EquipmentDefinitionSO GetDefinitionBySlot(EquipmentSlotType slot)
        {
            if (definitions == null) return null;
            var matching = definitions.FindAll(d => d.SlotType == slot);
            if (matching.Count > 0)
            {
                return matching[Random.Range(0, matching.Count)];
            }
            return GetRandomDefinition();
        }

        public void SetDefinitions(List<EquipmentDefinitionSO> list)
        {
            definitions = list;
        }
    }
}
