using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Items;

namespace WuxiaGame.Inventory
{
    public class Inventory : MonoBehaviour
    {
        private static Inventory _instance;
        public static Inventory Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<Inventory>();
                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private List<EquipmentInstance> items = new List<EquipmentInstance>();

        public IReadOnlyList<EquipmentInstance> Items => items;
        public int Count => items.Count;

        public event Action OnInventoryUpdated;

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

        public void AddItem(EquipmentInstance item)
        {
            if (Instance == null) Instance = this;
            if (item == null) return;
            // Prevent duplicate reference insertion
            if (items.Contains(item)) return;

            items.Add(item);
            EventBus.RaiseEquipmentAddedToInventory(item);
            OnInventoryUpdated?.Invoke();
        }

        public bool RemoveItem(EquipmentInstance item)
        {
            if (item == null) return false;
            bool removed = items.Remove(item);
            if (!removed)
            {
                int idx = items.FindIndex(i => i != null && (i == item || i.InstanceId == item.InstanceId));
                if (idx >= 0)
                {
                    items.RemoveAt(idx);
                    removed = true;
                }
            }
            if (removed)
            {
                OnInventoryUpdated?.Invoke();
            }
            return removed;
        }

        public bool HasItem(EquipmentInstance item)
        {
            if (item == null) return false;
            return items.Contains(item) || items.Exists(i => i != null && i.InstanceId == item.InstanceId);
        }

        public bool HasItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;
            return items.Exists(i => i != null && i.InstanceId == instanceId);
        }

        public EquipmentInstance GetItem(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return null;
            return items.Find(i => i != null && i.InstanceId == instanceId);
        }

        public IReadOnlyList<EquipmentInstance> GetAllItems()
        {
            return items;
        }

        public void Clear()
        {
            items.Clear();
            OnInventoryUpdated?.Invoke();
        }
    }
}
