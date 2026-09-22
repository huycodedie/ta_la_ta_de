using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Items;
using WuxiaGame.Stats;

namespace WuxiaGame.Equipment
{
    public class EquipmentManager : MonoBehaviour
    {
        private static EquipmentManager _instance;
        public static EquipmentManager Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();
                return _instance;
            }
            private set => _instance = value;
        }

        private readonly Dictionary<EquipmentSlotType, EquipmentInstance> equippedSlots = new Dictionary<EquipmentSlotType, EquipmentInstance>();

        public event Action<EquipmentSlotType, EquipmentInstance> OnEquipmentChanged;

        public static void ResetInstance()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private Inventory.Inventory GetInventory()
        {
            if (Inventory.Inventory.Instance != null) return Inventory.Inventory.Instance;
            return UnityEngine.Object.FindAnyObjectByType<Inventory.Inventory>();
        }

        public bool Equip(EquipmentInstance item)
        {
            if (Instance == null) Instance = this;
            if (item == null) return false;
            return EquipToSlot(item.SlotType, item);
        }

        public bool EquipToSlot(EquipmentSlotType targetSlot, EquipmentInstance item)
        {
            if (item == null) return false;

            // Slot Validation: item type must match target slot
            if (item.SlotType != targetSlot)
            {
                Debug.LogWarning($"[EquipmentManager] Cannot equip {item.ItemName} ({item.SlotType}) into slot {targetSlot}!");
                return false;
            }

            // 1. If target slot already has an item, return old item to Inventory
            if (equippedSlots.TryGetValue(targetSlot, out var oldItem) && oldItem != null)
            {
                equippedSlots.Remove(targetSlot);
                var inv = GetInventory();
                if (inv != null)
                {
                    inv.AddItem(oldItem);
                }
            }

            // 2. Remove new item from Inventory
            var targetInv = GetInventory();
            if (targetInv != null)
            {
                targetInv.RemoveItem(item);
            }

            // 3. Equip new item into slot
            equippedSlots[targetSlot] = item;

            EventBus.RaiseEquipmentEquipped(targetSlot, item);
            OnEquipmentChanged?.Invoke(targetSlot, item);
            return true;
        }

        public EquipmentInstance Unequip(EquipmentSlotType slot)
        {
            if (equippedSlots.TryGetValue(slot, out var existing) && existing != null)
            {
                equippedSlots.Remove(slot);
                var inv = GetInventory();
                if (inv != null)
                {
                    inv.AddItem(existing);
                }
                EventBus.RaiseEquipmentEquipped(slot, null);
                OnEquipmentChanged?.Invoke(slot, null);
                return existing;
            }
            return null;
        }

        public EquipmentInstance GetEquippedItem(EquipmentSlotType slot)
        {
            if (equippedSlots.TryGetValue(slot, out var item))
            {
                return item;
            }
            return null;
        }

        public IReadOnlyDictionary<EquipmentSlotType, EquipmentInstance> GetAllEquippedItems()
        {
            return equippedSlots;
        }

        public Dictionary<StatType, float> GetTotalEquipmentStats()
        {
            Dictionary<StatType, float> totals = new Dictionary<StatType, float>();
            foreach (var kvp in equippedSlots)
            {
                EquipmentInstance item = kvp.Value;
                if (item != null && item.Affixes != null)
                {
                    foreach (var affix in item.Affixes)
                    {
                        if (affix == null) continue;
                        if (!totals.ContainsKey(affix.StatType))
                        {
                            totals[affix.StatType] = 0f;
                        }
                        totals[affix.StatType] += affix.Value;
                    }
                }
            }
            return totals;
        }

        public int GetEquippedItemCount()
        {
            int count = 0;
            foreach (var kvp in equippedSlots)
            {
                if (kvp.Value != null) count++;
            }
            return count;
        }

        public bool HasEquippedItemWithId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            foreach (var kvp in equippedSlots)
            {
                if (kvp.Value != null && (kvp.Value.DefinitionId == itemId || kvp.Value.ItemName == itemId))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasEquippedItemWithMinRarityOrder(int minOrder)
        {
            foreach (var kvp in equippedSlots)
            {
                if (kvp.Value != null && kvp.Value.Rarity != null && kvp.Value.Rarity.OrderIndex >= minOrder)
                {
                    return true;
                }
            }
            return false;
        }

        public void ClearAll()
        {
            equippedSlots.Clear();
            OnEquipmentChanged?.Invoke(EquipmentSlotType.Helmet, null);
        }
    }
}
