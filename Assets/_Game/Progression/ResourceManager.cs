using System;
using UnityEngine;

namespace WuxiaGame.Progression
{
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager _instance;
        public static ResourceManager Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private int gold = 5000;
        [SerializeField] private int material = 10;

        public int Gold => gold;
        public int Material => material;

        public event Action<int, int> OnResourcesChanged;

        public static void ResetInstance()
        {
            _instance = null;
        }

        private const string PREF_GOLD = "TLTD_Gold";
        private const string PREF_MATERIAL = "TLTD_Material";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SaveState()
        {
            PlayerPrefs.SetInt(PREF_GOLD, gold);
            PlayerPrefs.SetInt(PREF_MATERIAL, material);
            PlayerPrefs.Save();
        }

        public void LoadState()
        {
            if (PlayerPrefs.HasKey(PREF_GOLD))
            {
                gold = PlayerPrefs.GetInt(PREF_GOLD, 5000);
            }
            if (PlayerPrefs.HasKey(PREF_MATERIAL))
            {
                material = PlayerPrefs.GetInt(PREF_MATERIAL, 10);
            }
        }

        public void ResetPersistence()
        {
            PlayerPrefs.DeleteKey(PREF_GOLD);
            PlayerPrefs.DeleteKey(PREF_MATERIAL);
            PlayerPrefs.Save();
        }

        public void SetResources(int goldAmount, int materialAmount)
        {
            if (Instance == null) Instance = this;
            gold = Mathf.Max(0, goldAmount);
            material = Mathf.Max(0, materialAmount);
            SaveState();
            OnResourcesChanged?.Invoke(gold, material);
        }

        public void SetMockResources(int goldAmount, int materialAmount)
        {
            if (Instance == null) Instance = this;
            gold = Mathf.Max(0, goldAmount);
            material = Mathf.Max(0, materialAmount);
            OnResourcesChanged?.Invoke(gold, material);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            SaveState();
            OnResourcesChanged?.Invoke(gold, material);
        }

        public void AddMaterial(int amount)
        {
            if (amount <= 0) return;
            material += amount;
            SaveState();
            OnResourcesChanged?.Invoke(gold, material);
        }

        public bool HasResources(int requiredGold, int requiredMaterial)
        {
            return gold >= requiredGold && material >= requiredMaterial;
        }

        public bool ConsumeResources(int requiredGold, int requiredMaterial)
        {
            if (!HasResources(requiredGold, requiredMaterial))
            {
                return false;
            }

            gold -= requiredGold;
            material -= requiredMaterial;
            SaveState();
            OnResourcesChanged?.Invoke(gold, material);
            return true;
        }

        public (int goldGain, int matGain) DismantleEquipment(WuxiaGame.Items.EquipmentInstance item)
        {
            if (item == null) return (0, 0);

            // 1. Remove from Inventory if present
            var inv = WuxiaGame.Inventory.Inventory.Instance != null ? WuxiaGame.Inventory.Inventory.Instance : UnityEngine.Object.FindAnyObjectByType<WuxiaGame.Inventory.Inventory>();
            if (inv != null && inv.HasItem(item))
            {
                inv.RemoveItem(item);
            }

            // 2. Compute resource gain: Gold only per D1_D23_AMENDMENTS_LOCKED A10 & CURRENT_DESIGN_AUTHORITY
            // Preserved baseline implementation formula (TBD balance): Mathf.Max(50, item.EquipmentLevel * 100)
            int goldGain = Mathf.Max(50, item.EquipmentLevel * 100);
            int matGain = 0; // Contract locked: Recycle awards GOLD ONLY, no Material, no EXP

            AddGold(goldGain);

            Debug.Log($"[DISMANTLE] Dismantled {item.ItemName} -> +{goldGain} Gold (Gold only)");
            return (goldGain, matGain);
        }
    }
}
