using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Affix;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Items;

namespace WuxiaGame.Drop
{
    public class DropSystem : MonoBehaviour
    {
        private static DropSystem instance;
        public static DropSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<DropSystem>();
                }
                return instance;
            }
            private set => instance = value;
        }

        [Header("Data Databases")]
        [SerializeField] private DropLevelDatabaseSO dropLevelDatabase;
        [SerializeField] private EquipmentDatabaseSO equipmentDatabase;
        [SerializeField] private AffixDatabaseSO affixDatabase;
        [SerializeField] private EquipmentDropConfigSO dropConfig;

        [Header("Drop Settings")]
        [SerializeField, Range(0, 100)] private float normalMonsterDropRate = 100f; // 100% for testing
        [SerializeField] private int currentDropLevel = 16;

        private IItemLevelProvider itemLevelProvider = new HeroLevelItemLevelProvider();

        public float NormalMonsterDropRate
        {
            get => normalMonsterDropRate;
            set => normalMonsterDropRate = Mathf.Clamp(value, 0f, 100f);
        }

        public int CurrentDropLevel
        {
            get => currentDropLevel;
            set => currentDropLevel = value;
        }

        public int CurrentLootTier
        {
            get => currentDropLevel;
            set => currentDropLevel = value;
        }

        public DropLevelDatabaseSO DropLevelDatabase => dropLevelDatabase;
        public EquipmentDatabaseSO EquipmentDatabase => equipmentDatabase;
        public AffixDatabaseSO AffixDatabase => affixDatabase;
        public EquipmentDropConfigSO DropConfig => dropConfig;

        public static void ResetInstance()
        {
            instance = null;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;

            LoadDatabasesIfMissing();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetItemLevelProvider(IItemLevelProvider provider)
        {
            itemLevelProvider = provider ?? new HeroLevelItemLevelProvider();
        }

        public void SetDropConfig(EquipmentDropConfigSO config)
        {
            dropConfig = config;
        }

        public void LoadDatabasesIfMissing()
        {
            if (Instance == null) Instance = this;
            if (dropLevelDatabase == null)
            {
                dropLevelDatabase = Resources.Load<DropLevelDatabaseSO>("Data/DropLevelDatabase");
#if UNITY_EDITOR
                if (dropLevelDatabase == null) dropLevelDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<DropLevelDatabaseSO>("Assets/_Game/Data/DropLevelDatabase.asset");
#endif
            }

            if (equipmentDatabase == null)
            {
                equipmentDatabase = Resources.Load<EquipmentDatabaseSO>("Data/EquipmentDatabase");
#if UNITY_EDITOR
                if (equipmentDatabase == null) equipmentDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentDatabaseSO>("Assets/_Game/Data/EquipmentDatabase.asset");
#endif
            }

            if (affixDatabase == null)
            {
                affixDatabase = Resources.Load<AffixDatabaseSO>("Data/AffixDatabase");
#if UNITY_EDITOR
                if (affixDatabase == null) affixDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<AffixDatabaseSO>("Assets/_Game/Data/AffixDatabase.asset");
#endif
            }
        }

        public EquipmentInstance GenerateDrop(Monster monster = null, int? overrideDropLevel = null)
        {
            LoadDatabasesIfMissing();

            // 1. Roll Shared Equipment Drop Rate
            float effectiveDropRate = dropConfig != null ? dropConfig.EquipmentDropRate : normalMonsterDropRate;
            float dropRoll = Random.Range(0f, 100f);
            bool success = dropRoll <= effectiveDropRate;

            if (!success)
            {
                Debug.Log("[DROP] Equipment Drop Roll: FAIL");
                return null;
            }

            // 2. Roll / Determine LootTier
            int lootTierId = overrideDropLevel ?? currentDropLevel;
            LootTierConfigSO lootTierConfig = null;
            Debug.Log($"[DROP DEBUG] override={overrideDropLevel}, mgrInstance={(WuxiaGame.Progression.LootTierProgressionManager.Instance != null ? WuxiaGame.Progression.LootTierProgressionManager.Instance.CurrentTierId.ToString() : "NULL")}, dropConfig={(dropConfig != null ? "YES" : "NULL")}");

            if (overrideDropLevel != null)
            {
                lootTierId = overrideDropLevel.Value;
                if (WuxiaGame.Progression.LootTierProgressionManager.Instance != null && WuxiaGame.Progression.LootTierProgressionManager.Instance.Database != null)
                {
                    lootTierConfig = WuxiaGame.Progression.LootTierProgressionManager.Instance.Database.GetTier(lootTierId);
                }
            }
            else if (WuxiaGame.Progression.LootTierProgressionManager.Instance != null)
            {
                lootTierId = WuxiaGame.Progression.LootTierProgressionManager.Instance.CurrentTierId;
                lootTierConfig = WuxiaGame.Progression.LootTierProgressionManager.Instance.GetCurrentTier();
            }
            else if (dropConfig != null)
            {
                lootTierConfig = dropConfig.RollLootTier();
                if (lootTierConfig != null)
                {
                    lootTierId = lootTierConfig.TierId;
                }
            }

            if (lootTierConfig == null && WuxiaGame.Progression.LootTierProgressionManager.Instance != null && WuxiaGame.Progression.LootTierProgressionManager.Instance.Database != null)
            {
                lootTierConfig = WuxiaGame.Progression.LootTierProgressionManager.Instance.Database.GetTier(lootTierId);
            }

            // 3. Roll Rarity using LootTier's Rarity Table
            RarityDefinitionSO rarity = null;
            if (lootTierConfig != null)
            {
                rarity = lootTierConfig.RollRarity();
            }
            else if (dropLevelDatabase != null)
            {
                DropLevelConfigSO dlConfig = dropLevelDatabase.GetDropLevelConfig(lootTierId);
                if (dlConfig != null)
                {
                    rarity = dlConfig.RollRarity();
                }
            }

            if (rarity == null)
            {
                // Fallback rarity from database if config missing
                rarity = Resources.Load<RarityDefinitionSO>("Data/Rarities/Rarity_01_Pure");
#if UNITY_EDITOR
                if (rarity == null) rarity = UnityEditor.AssetDatabase.LoadAssetAtPath<RarityDefinitionSO>("Assets/_Game/Data/Rarities/Rarity_01_Pure.asset");
#endif
            }

            // 4. Determine Item Level (Independent from LootTier)
            int itemLevel = itemLevelProvider != null ? itemLevelProvider.GetItemLevel() : 1;

            // 5. Select Equipment Slot (12 Slots) & Definition
            EquipmentDefinitionSO def = equipmentDatabase != null ? equipmentDatabase.GetRandomDefinition() : null;
            EquipmentSlotType slot = def != null ? def.SlotType : (EquipmentSlotType)Random.Range(0, 12);
            string defId = def != null ? def.DefinitionId : $"eq_{slot.ToString().ToLower()}_default";
            string itemName = def != null ? def.ItemName : $"{slot}";

            // 6. Determine Affixes (Unique, Min-Max range from Rarity)
            List<AffixInstance> affixes = AffixGenerator.GenerateAffixes(affixDatabase, slot, rarity, itemLevel);

            // 7. Create ONE Equipment Item storing LootTier metadata
            EquipmentInstance instance = new EquipmentInstance(defId, itemName, slot, itemLevel, rarity, affixes, 5, lootTierId);

            // 8. Add to Inventory
            Inventory.Inventory inv = Inventory.Inventory.Instance != null ? Inventory.Inventory.Instance : UnityEngine.Object.FindAnyObjectByType<Inventory.Inventory>();
            if (inv != null)
            {
                inv.AddItem(instance);
            }

            // 9. Emit Coherent Debug Log Group
            Debug.Log("[DROP] Equipment Drop Roll: SUCCESS");
            Debug.Log($"[DROP] LootTier: {lootTierId}");
            Debug.Log($"[DROP] Rarity: {(rarity != null ? rarity.DisplayName : "Thường")}");
            Debug.Log($"[DROP] Slot: {slot}");
            Debug.Log($"[DROP] Item Level: {itemLevel}");
            Debug.Log($"[DROP] Affix Count: {affixes.Count}");

            // 10. Trigger Pause & Comparison UI
            EventBus.RaiseEquipmentDropped(instance);
            return instance;
        }

        private void HandleEntityDied(Entity entity)
        {
            if (entity is Monster monster)
            {
                GenerateDrop(monster);
            }
        }
    }
}

