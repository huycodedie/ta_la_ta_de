using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Drop;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Items;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.UI
{
    public class EquipmentDropDebugUI : MonoBehaviour
    {
        [Header("Selected Item Display Texts")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI itemLevelText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI affixesText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI comparisonText;
        [SerializeField] private TextMeshProUGUI upgradePreviewText;
        [SerializeField] private TextMeshProUGUI playerResourcesText;

        [Header("Action & Debug Buttons")]
        [SerializeField] private Button testDropButton;
        [SerializeField] private Button test1000DropsButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button addGoldButton;
        [SerializeField] private Button addMaterialButton;

        [Header("Equipment Slot Texts")]
        [SerializeField] private TextMeshProUGUI helmetSlotText;
        [SerializeField] private TextMeshProUGUI weaponSlotText;
        [SerializeField] private TextMeshProUGUI armorSlotText;
        [SerializeField] private TextMeshProUGUI glovesSlotText;
        [SerializeField] private TextMeshProUGUI bootsSlotText;
        [SerializeField] private TextMeshProUGUI ringSlotText;
        [SerializeField] private TextMeshProUGUI necklaceSlotText;
        [SerializeField] private TextMeshProUGUI accessorySlotText;

        [Header("Hero Stats Display Text")]
        [SerializeField] private TextMeshProUGUI heroStatsText;

        private EquipmentInstance selectedItem;
        private bool isSelectedEquipped;
        private EquipmentSlotType selectedSlot;

        public EquipmentInstance SelectedItem => selectedItem;
        public bool IsSelectedEquipped => isSelectedEquipped;
        public EquipmentSlotType SelectedSlot => selectedSlot;
        public Button EquipButton => equipButton;
        public Button UnequipButton => unequipButton;
        public TextMeshProUGUI PlayerResourcesText => playerResourcesText;

        private void Awake()
        {
            FindReferencesIfMissing();
        }

        private void Start()
        {
            FindReferencesIfMissing();
            BindButtons();
            RefreshUI();
        }

        private void BindButtons()
        {
            if (testDropButton != null)
            {
                testDropButton.onClick.RemoveListener(OnTestDropClicked);
                testDropButton.onClick.AddListener(OnTestDropClicked);
            }
            if (test1000DropsButton != null)
            {
                test1000DropsButton.onClick.RemoveListener(OnTest1000DropsClicked);
                test1000DropsButton.onClick.AddListener(OnTest1000DropsClicked);
            }
            if (equipButton != null)
            {
                equipButton.onClick.RemoveListener(EquipSelected);
                equipButton.onClick.AddListener(EquipSelected);
            }
            if (unequipButton != null)
            {
                unequipButton.onClick.RemoveListener(UnequipSelected);
                unequipButton.onClick.AddListener(UnequipSelected);
            }
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }
            if (addGoldButton != null)
            {
                addGoldButton.onClick.RemoveListener(OnAddGoldClicked);
                addGoldButton.onClick.AddListener(OnAddGoldClicked);
            }
            if (addMaterialButton != null)
            {
                addMaterialButton.onClick.RemoveListener(OnAddMaterialClicked);
                addMaterialButton.onClick.AddListener(OnAddMaterialClicked);
            }
        }

        public void FindReferencesIfMissing()
        {
            Transform root = transform.root;
            if (root == null) root = transform;

            Button[] allButtons = root.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                if (b == null) continue;
                string bName = b.gameObject.name;
                if (equipButton == null && bName == "EquipBtn") equipButton = b;
                else if (unequipButton == null && bName == "UnequipBtn") unequipButton = b;
                else if (testDropButton == null && bName == "TestDropBtn") testDropButton = b;
                else if (test1000DropsButton == null && bName == "Test1000DropsBtn") test1000DropsButton = b;
                else if (upgradeButton == null && bName == "UpgradeBtn") upgradeButton = b;
                else if (addGoldButton == null && bName == "AddGoldBtn") addGoldButton = b;
                else if (addMaterialButton == null && bName == "AddMatBtn") addMaterialButton = b;
            }

            TextMeshProUGUI[] allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t == null) continue;
                string tName = t.gameObject.name;
                if (itemNameText == null && tName == "ItemName") itemNameText = t;
                else if (itemTypeText == null && tName == "ItemType") itemTypeText = t;
                else if (itemLevelText == null && tName == "ItemLevel") itemLevelText = t;
                else if (rarityText == null && tName == "Rarity") rarityText = t;
                else if (statusText == null && tName == "Status") statusText = t;
                else if (affixesText == null && tName == "Affixes") affixesText = t;
                else if (inventoryCountText == null && tName == "InvCount") inventoryCountText = t;
                else if (comparisonText == null && tName == "ComparisonText") comparisonText = t;
                else if (upgradePreviewText == null && tName == "UpgradePreviewText") upgradePreviewText = t;
                else if (playerResourcesText == null && tName == "PlayerResText") playerResourcesText = t;
                else if (heroStatsText == null && tName == "HeroStats") heroStatsText = t;
                else if (helmetSlotText == null && tName == "HelmetSlot") helmetSlotText = t;
                else if (weaponSlotText == null && tName == "WeaponSlot") weaponSlotText = t;
                else if (armorSlotText == null && tName == "ArmorSlot") armorSlotText = t;
                else if (glovesSlotText == null && tName == "GlovesSlot") glovesSlotText = t;
                else if (bootsSlotText == null && tName == "BootsSlot") bootsSlotText = t;
                else if (ringSlotText == null && tName == "RingSlot") ringSlotText = t;
                else if (necklaceSlotText == null && tName == "NecklaceSlot") necklaceSlotText = t;
                else if (accessorySlotText == null && tName == "AccessorySlot") accessorySlotText = t;
            }
        }

        private void OnEnable()
        {
            EventBus.OnEquipmentDropped += HandleEquipmentDropped;
            EventBus.OnEquipmentEquipped += HandleEquipmentEquipped;
            if (Inventory.Inventory.Instance != null)
            {
                Inventory.Inventory.Instance.OnInventoryUpdated += RefreshUI;
            }
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged += HandleResourcesChanged;
            }
        }

        private void OnDisable()
        {
            EventBus.OnEquipmentDropped -= HandleEquipmentDropped;
            EventBus.OnEquipmentEquipped -= HandleEquipmentEquipped;
            if (Inventory.Inventory.Instance != null)
            {
                Inventory.Inventory.Instance.OnInventoryUpdated -= RefreshUI;
            }
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= HandleResourcesChanged;
            }
        }

        public void SetReferences(
            TextMeshProUGUI nameTmp,
            TextMeshProUGUI typeTmp,
            TextMeshProUGUI levelTmp,
            TextMeshProUGUI rarityTmp,
            TextMeshProUGUI affixesTmp,
            TextMeshProUGUI countTmp,
            Button dropBtn,
            Button drop1000Btn,
            Button eqBtn = null,
            Button uneqBtn = null,
            TextMeshProUGUI heroStatsTmp = null,
            TextMeshProUGUI helmetTmp = null,
            TextMeshProUGUI weaponTmp = null,
            TextMeshProUGUI armorTmp = null,
            TextMeshProUGUI glovesTmp = null,
            TextMeshProUGUI bootsTmp = null,
            TextMeshProUGUI ringTmp = null,
            TextMeshProUGUI necklaceTmp = null,
            TextMeshProUGUI accTmp = null,
            TextMeshProUGUI statusTmp = null,
            TextMeshProUGUI comparisonTmp = null,
            TextMeshProUGUI upgradePreviewTmp = null,
            TextMeshProUGUI playerResTmp = null,
            Button upgBtn = null,
            Button addGBtn = null,
            Button addMBtn = null)
        {
            itemNameText = nameTmp;
            itemTypeText = typeTmp;
            itemLevelText = levelTmp;
            rarityText = rarityTmp;
            affixesText = affixesTmp;
            inventoryCountText = countTmp;
            testDropButton = dropBtn;
            test1000DropsButton = drop1000Btn;
            equipButton = eqBtn;
            unequipButton = uneqBtn;
            heroStatsText = heroStatsTmp;
            helmetSlotText = helmetTmp;
            weaponSlotText = weaponTmp;
            armorSlotText = armorTmp;
            glovesSlotText = glovesTmp;
            bootsSlotText = bootsTmp;
            ringSlotText = ringTmp;
            necklaceSlotText = necklaceTmp;
            accessorySlotText = accTmp;
            statusText = statusTmp;
            comparisonText = comparisonTmp;
            upgradePreviewText = upgradePreviewTmp;
            playerResourcesText = playerResTmp;
            upgradeButton = upgBtn;
            addGoldButton = addGBtn;
            addMaterialButton = addMBtn;

            BindButtons();
            OnEnable();
        }

        public void SelectItem(EquipmentInstance item, bool isEquipped = false, EquipmentSlotType slot = EquipmentSlotType.Weapon)
        {
            selectedItem = item;
            isSelectedEquipped = isEquipped;
            selectedSlot = slot;

            DisplayEquipment(selectedItem);
            UpdateComparison(selectedItem, isEquipped);
            UpdateUpgradePreview();
            UpdateButtonsState();
        }

        public void SelectSlot(EquipmentSlotType slot)
        {
            EquipmentInstance item = EquipmentManager.Instance != null ? EquipmentManager.Instance.GetEquippedItem(slot) : null;
            SelectItem(item, true, slot);
        }

        public void EquipSelected()
        {
            if (selectedItem != null && !isSelectedEquipped)
            {
                var bm = BattleManager.Instance != null ? BattleManager.Instance : UnityEngine.Object.FindAnyObjectByType<BattleManager>();
                if (bm != null && bm.CurrentBattleState == BattleState.LootPending && bm.PendingLootItem == selectedItem)
                {
                    EquipmentSlotType targetSlot = selectedItem.SlotType;
                    bm.CompleteLootDecisionAndResume(equip: true, dismantle: false);
                    SelectSlot(targetSlot);
                    Debug.Log($"[DEBUG UI] Button action executed via BattleManager: EQUIP ({selectedItem.ItemName})");
                    return;
                }

                EquipmentManager eqMgr = EquipmentManager.Instance != null ? EquipmentManager.Instance : UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();
                if (eqMgr != null)
                {
                    EquipmentSlotType targetSlot = selectedItem.SlotType;
                    eqMgr.Equip(selectedItem);
                    SelectSlot(targetSlot);
                    Debug.Log($"[DEBUG UI] Button action executed: EQUIP ({selectedItem.ItemName})");
                }
                else
                {
                    Debug.LogWarning("[DEBUG UI] Button action failed: EQUIP (EquipmentManager null)");
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: EQUIP (No unequipped item selected)");
            }
        }

        public void UnequipSelected()
        {
            if (isSelectedEquipped)
            {
                EquipmentManager eqMgr = EquipmentManager.Instance != null ? EquipmentManager.Instance : UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();
                if (eqMgr != null)
                {
                    EquipmentInstance unequipped = eqMgr.Unequip(selectedSlot);
                    SelectItem(unequipped, false, selectedSlot);
                    Debug.Log($"[DEBUG UI] Button action executed: UNEQUIP ({selectedSlot})");
                }
                else
                {
                    Debug.LogWarning("[DEBUG UI] Button action failed: UNEQUIP (EquipmentManager null)");
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: UNEQUIP (No equipped item selected)");
            }
        }

        public void OnUpgradeClicked()
        {
            if (selectedItem != null)
            {
                EquipmentUpgradeService upgradeSvc = EquipmentUpgradeService.Instance != null
                    ? EquipmentUpgradeService.Instance
                    : UnityEngine.Object.FindAnyObjectByType<EquipmentUpgradeService>();

                if (upgradeSvc != null && upgradeSvc.UpgradeEquipment(selectedItem))
                {
                    RefreshUI();
                    Debug.Log($"[DEBUG UI] Button action executed: UPGRADE ({selectedItem.ItemName} Lv.{selectedItem.EquipmentLevel})");
                }
                else
                {
                    Debug.LogWarning("[DEBUG UI] Button action failed: UPGRADE (Insufficient resources or max level)");
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: UPGRADE (No item selected)");
            }
        }

        public void OnAddGoldClicked()
        {
            ResourceManager rm = ResourceManager.Instance != null ? ResourceManager.Instance : UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
            if (rm != null)
            {
                rm.AddGold(1000);
                Debug.Log("[DEBUG UI] Button action executed: +1K G");
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: +1K G (ResourceManager null)");
            }
        }

        public void OnAddMaterialClicked()
        {
            ResourceManager rm = ResourceManager.Instance != null ? ResourceManager.Instance : UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
            if (rm != null)
            {
                rm.AddMaterial(5);
                Debug.Log("[DEBUG UI] Button action executed: +5 MAT");
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: +5 MAT (ResourceManager null)");
            }
        }

        private void HandleEquipmentDropped(EquipmentInstance item)
        {
            SelectItem(item, false, item.SlotType);
            RefreshUI();
        }

        private void HandleEquipmentEquipped(EquipmentSlotType slot, EquipmentInstance item)
        {
            RefreshUI();
        }

        private void HandleResourcesChanged(int gold, int material)
        {
            RefreshUI();
        }

        public void DisplayEquipment(EquipmentInstance item)
        {
            if (item == null)
            {
                if (itemNameText != null) itemNameText.text = "Item Name: (None)";
                if (itemTypeText != null) itemTypeText.text = "Equipment Type: -";
                if (itemLevelText != null) itemLevelText.text = "Equipment Level: -";
                if (rarityText != null)
                {
                    rarityText.text = "Rarity: -";
                    rarityText.color = Color.white;
                }
                if (statusText != null) statusText.text = "Status: -";
                if (affixesText != null) affixesText.text = "Affixes:\n  (None)";
                return;
            }

            if (itemNameText != null) itemNameText.text = $"Item Name: {item.ItemName}";
            if (itemTypeText != null) itemTypeText.text = $"Equipment Type: {item.SlotType}";
            if (itemLevelText != null) itemLevelText.text = $"Equipment Level: {item.EquipmentLevel} / {item.MaxLevel}";

            if (rarityText != null)
            {
                string rName = item.Rarity != null ? item.Rarity.DisplayName : "Common";
                Color rColor = item.Rarity != null ? item.Rarity.RarityColor : Color.white;
                rarityText.text = $"Rarity: {rName}";
                rarityText.color = rColor;
            }

            if (statusText != null)
            {
                statusText.text = isSelectedEquipped ? "Status: EQUIPPED" : "Status: NOT EQUIPPED";
                statusText.color = isSelectedEquipped ? Color.green : Color.yellow;
            }

            if (affixesText != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Affixes:");
                foreach (var a in item.Affixes)
                {
                    string formattedVal = a.Value % 1 == 0 ? $"{a.Value:F0}" : $"{a.Value:F1}%";
                    sb.AppendLine($"  {a.DisplayName,-15} {formattedVal}");
                }
                affixesText.text = sb.ToString();
            }
        }

        public void UpdateComparison(EquipmentInstance newItem, bool isEquipped)
        {
            if (comparisonText == null) return;

            if (newItem == null || isEquipped || EquipmentManager.Instance == null)
            {
                comparisonText.text = "ITEM COMPARISON:\nSelect an unequipped item to compare.";
                return;
            }

            EquipmentInstance currentItem = EquipmentManager.Instance.GetEquippedItem(newItem.SlotType);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ITEM COMPARISON ({newItem.SlotType})");
            sb.AppendLine($"CURRENT: {(currentItem != null ? currentItem.ItemName : "[Empty]")}");
            sb.AppendLine($"NEW    : {newItem.ItemName}");
            sb.AppendLine("STAT CHANGES:");

            Dictionary<StatType, float> currentStats = GetItemStats(currentItem);
            Dictionary<StatType, float> newStats = GetItemStats(newItem);

            HashSet<StatType> allTypes = new HashSet<StatType>(currentStats.Keys);
            allTypes.UnionWith(newStats.Keys);

            foreach (StatType type in allTypes)
            {
                float curVal = currentStats.TryGetValue(type, out var c) ? c : 0f;
                float newVal = newStats.TryGetValue(type, out var n) ? n : 0f;
                float diff = newVal - curVal;

                if (diff != 0f)
                {
                    string prefix = diff > 0 ? "+" : "";
                    string formattedDiff = diff % 1 == 0 ? $"{diff:F0}" : $"{diff:F1}%";
                    sb.AppendLine($"  {type,-14}: {prefix}{formattedDiff}");
                }
            }

            comparisonText.text = sb.ToString();
        }

        public void UpdateUpgradePreview()
        {
            if (upgradePreviewText == null) return;

            if (selectedItem == null)
            {
                upgradePreviewText.text = "UPGRADE PREVIEW:\nSelect an item to view upgrade costs.";
                return;
            }

            EquipmentUpgradeService upgradeSvc = EquipmentUpgradeService.Instance != null
                ? EquipmentUpgradeService.Instance
                : UnityEngine.Object.FindAnyObjectByType<EquipmentUpgradeService>();

            if (upgradeSvc == null)
            {
                upgradePreviewText.text = "UPGRADE PREVIEW:\nUpgrade service not initialized.";
                return;
            }

            EquipmentUpgradePreviewData preview = upgradeSvc.GetUpgradePreview(selectedItem);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UPGRADE PREVIEW");

            if (preview.IsMaxLevel)
            {
                sb.AppendLine($"Level: {preview.CurrentLevel} / {preview.MaxLevel} (MAX LEVEL)");
                sb.AppendLine("Item is at maximum upgrade level.");
            }
            else
            {
                sb.AppendLine($"Next Level: Level {preview.NextLevel} (Max: {preview.MaxLevel})");
                sb.AppendLine("Stat Changes:");
                foreach (var diff in preview.AffixDiffs)
                {
                    string curFormatted = diff.CurrentValue % 1 == 0 ? $"{diff.CurrentValue:F0}" : $"{diff.CurrentValue:F1}%";
                    string nextFormatted = diff.NextValue % 1 == 0 ? $"{diff.NextValue:F0}" : $"{diff.NextValue:F1}%";
                    sb.AppendLine($"  {diff.AffixName}: {curFormatted} -> {nextFormatted}");
                }

                sb.AppendLine($"Cost: Gold {preview.GoldCost} | Material {preview.MaterialCost}");

                ResourceManager resMgr = ResourceManager.Instance != null ? ResourceManager.Instance : UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
                if (resMgr != null)
                {
                    sb.AppendLine($"Owned: Gold {resMgr.Gold} | Material {resMgr.Material}");
                }

                if (!preview.HasEnoughResources)
                {
                    sb.AppendLine("[INSUFFICIENT RESOURCES]");
                }
            }

            upgradePreviewText.text = sb.ToString();
        }

        private Dictionary<StatType, float> GetItemStats(EquipmentInstance item)
        {
            Dictionary<StatType, float> map = new Dictionary<StatType, float>();
            if (item != null && item.Affixes != null)
            {
                foreach (var a in item.Affixes)
                {
                    if (a == null) continue;
                    if (!map.ContainsKey(a.StatType)) map[a.StatType] = 0f;
                    map[a.StatType] += a.Value;
                }
            }
            return map;
        }

        public void RefreshUI()
        {
            UpdateInventoryCount();
            UpdateEquipmentSlots();
            UpdateHeroStats();
            UpdatePlayerResources();
            UpdateButtonsState();
            if (selectedItem != null)
            {
                DisplayEquipment(selectedItem);
                UpdateComparison(selectedItem, isSelectedEquipped);
                UpdateUpgradePreview();
            }
        }

        private void UpdatePlayerResources()
        {
            if (playerResourcesText == null) return;
            ResourceManager resMgr = ResourceManager.Instance != null ? ResourceManager.Instance : UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
            if (resMgr != null)
            {
                playerResourcesText.text = $"Gold: {resMgr.Gold:N0} | Material: {resMgr.Material}";
            }
        }

        private void UpdateInventoryCount()
        {
            if (inventoryCountText != null)
            {
                int count = Inventory.Inventory.Instance != null ? Inventory.Inventory.Instance.Count : 0;
                inventoryCountText.text = $"Inventory: {count} item(s)";
            }
        }

        private void UpdateEquipmentSlots()
        {
            if (EquipmentManager.Instance == null) return;

            SetSlotText(helmetSlotText, EquipmentSlotType.Helmet);
            SetSlotText(weaponSlotText, EquipmentSlotType.Weapon);
            SetSlotText(armorSlotText, EquipmentSlotType.Armor);
            SetSlotText(glovesSlotText, EquipmentSlotType.Gloves);
            SetSlotText(bootsSlotText, EquipmentSlotType.Boots);
            SetSlotText(ringSlotText, EquipmentSlotType.Ring);
            SetSlotText(necklaceSlotText, EquipmentSlotType.Necklace);
            SetSlotText(accessorySlotText, EquipmentSlotType.Accessory);
        }

        private void SetSlotText(TextMeshProUGUI tmp, EquipmentSlotType slot)
        {
            if (tmp == null) return;
            EquipmentInstance item = EquipmentManager.Instance.GetEquippedItem(slot);
            if (item != null)
            {
                tmp.text = $"{slot}: {item.ItemName} Lv.{item.EquipmentLevel} ({item.Rarity?.DisplayName})";
                tmp.color = item.Rarity != null ? item.Rarity.RarityColor : Color.yellow;
            }
            else
            {
                tmp.text = $"{slot}: [Empty]";
                tmp.color = Color.gray;
            }
        }

        private void UpdateHeroStats()
        {
            if (heroStatsText == null) return;

            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            if (hero == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HERO TOTAL STATS");
            sb.AppendLine($"HP : {hero.Stats.GetValue(StatType.MaxHealth):F0}");
            sb.AppendLine($"ATK: {hero.Stats.GetValue(StatType.Attack):F0}");
            sb.AppendLine($"DEF: {hero.Stats.GetValue(StatType.Defense):F0}");
            sb.AppendLine($"Crit Rate  : {hero.Stats.GetValue(StatType.CritRate):F1}%");
            sb.AppendLine($"Crit Damage: {hero.Stats.GetValue(StatType.CritDamage):F1}%");
            sb.AppendLine($"Dodge      : {hero.Stats.GetValue(StatType.Dodge):F1}%");
            heroStatsText.text = sb.ToString();
        }

        private void UpdateButtonsState()
        {
            if (equipButton != null)
            {
                equipButton.interactable = (selectedItem != null && !isSelectedEquipped);
            }
            if (unequipButton != null)
            {
                unequipButton.interactable = isSelectedEquipped;
            }
            if (upgradeButton != null)
            {
                EquipmentUpgradeService upgradeSvc = EquipmentUpgradeService.Instance != null
                    ? EquipmentUpgradeService.Instance
                    : UnityEngine.Object.FindAnyObjectByType<EquipmentUpgradeService>();

                if (selectedItem != null && upgradeSvc != null)
                {
                    EquipmentUpgradePreviewData preview = upgradeSvc.GetUpgradePreview(selectedItem);
                    upgradeButton.interactable = preview.CanUpgrade;
                }
                else
                {
                    upgradeButton.interactable = false;
                }
            }
        }

        public void OnTestDropClicked()
        {
            DropSystem ds = DropSystem.Instance != null ? DropSystem.Instance : UnityEngine.Object.FindAnyObjectByType<DropSystem>();
            if (ds != null)
            {
                EquipmentInstance item = ds.GenerateDrop();
                if (item != null)
                {
                    SelectItem(item, false, item.SlotType);
                    Debug.Log($"[DEBUG UI] Button action executed: DROP ({item.ItemName})");
                }
                else
                {
                    Debug.LogWarning("[DEBUG UI] Button action failed: DROP (No item generated)");
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: DROP (DropSystem null)");
            }
        }

        public void OnTest1000DropsClicked()
        {
            ItemGenerationTester.Run1000DropSimulation();
            Debug.Log("[DEBUG UI] Button action executed: SIM 1000");
        }
    }
}
