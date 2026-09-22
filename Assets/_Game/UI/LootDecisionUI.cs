using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.Items;
using WuxiaGame.Stats;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.UI
{
    public class LootDecisionUI : MonoBehaviour, IModalView
    {
        private static LootDecisionUI _instance;
        public static LootDecisionUI Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<LootDecisionUI>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Panel & Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemSlotText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI itemLevelText;
        [SerializeField] private TextMeshProUGUI affixesText;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Comparison Elements")]
        [SerializeField] private TextMeshProUGUI currentEquippedText;
        [SerializeField] private TextMeshProUGUI newDropText;
        [SerializeField] private TextMeshProUGUI powerChangeText;
        [SerializeField] private TextMeshProUGUI statChangesText;

        [Header("Action Buttons")]
        [SerializeField] private Button equipButton;
        [SerializeField] private Button dismantleButton; // [TÁCH]

        private EquipmentInstance displayedItem;
        private ComparisonResult latestComparison;
        private bool isProcessingTransaction = false;

        public GameObject Panel => panel;
        public EquipmentInstance DisplayedItem => displayedItem;
        public ComparisonResult LatestComparison => latestComparison;
        public TextMeshProUGUI HeaderText => headerText;
        public TextMeshProUGUI ItemNameText => itemNameText;
        public TextMeshProUGUI ItemSlotText => itemSlotText;
        public TextMeshProUGUI RarityText => rarityText;
        public TextMeshProUGUI ItemLevelText => itemLevelText;
        public TextMeshProUGUI AffixesText => affixesText;
        public TextMeshProUGUI StatsText => statsText;
        public TextMeshProUGUI CurrentEquippedText => currentEquippedText;
        public TextMeshProUGUI NewDropText => newDropText;
        public TextMeshProUGUI PowerChangeText => powerChangeText;
        public TextMeshProUGUI StatChangesText => statChangesText;
        public Button EquipButton => equipButton;
        public Button DismantleButton => dismantleButton;

        // IModalView implementation
        public string ModalId => "EquipmentComparison";
        public ModalPriority DefaultPriority => ModalPriority.CriticalGameplay;
        public bool IsDismissable => false;
        public bool IsVisible => panel != null && panel.activeSelf;

        private ModalRequest _activeModalRequest;

        public void ShowModal(ModalRequest request = null)
        {
            _activeModalRequest = request;
            isProcessingTransaction = false;
            if (request != null && request.Payload is EquipmentInstance item)
            {
                displayedItem = item;
            }

            if (displayedItem != null)
            {
                DisplayComparison(displayedItem);
            }
            else if (panel != null)
            {
                panel.SetActive(true);
            }
            if (equipButton != null) equipButton.interactable = true;
            if (dismantleButton != null) dismantleButton.interactable = true;
        }

        public void HideModal(DismissalReason reason = DismissalReason.UserClosed)
        {
            _activeModalRequest = null;
            isProcessingTransaction = false;
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static void ResetInstance()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying)
                    Destroy(this);
                else
                    DestroyImmediate(this);
                return;
            }
            Instance = this;
            FindReferencesIfMissing();
            BindButtons();
        }

        private void Start()
        {
            FindReferencesIfMissing();
            BindButtons();
            ModalCoordinator.Instance?.RegisterModalView(this);
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void FindReferencesIfMissing()
        {
            Transform root = transform.root;
            if (root == null) root = transform;

            if (panel == null)
            {
                Transform p = root.Find("LootDecisionPanel");
                if (p == null)
                {
                    var allT = root.GetComponentsInChildren<Transform>(true);
                    foreach (var t in allT)
                    {
                        if (t != null && t.name == "LootDecisionPanel") { p = t; break; }
                    }
                }
                if (p != null) panel = p.gameObject;
            }

            Transform searchRoot = panel != null ? panel.transform : root;

            Button[] allButtons = searchRoot.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                if (b == null) continue;
                string bName = b.gameObject.name;
                if (equipButton == null && (bName == "LootEquipButton" || bName == "EquipButton")) equipButton = b;
                else if (dismantleButton == null && (bName == "LootDismantleButton" || bName == "DismantleButton")) dismantleButton = b;
            }

            TextMeshProUGUI[] allTexts = searchRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t == null) continue;
                string tName = t.gameObject.name;
                if (headerText == null && (tName == "Header" || tName == "ComparisonHeader")) headerText = t;
                else if (itemNameText == null && tName == "ItemName") itemNameText = t;
                else if (itemSlotText == null && (tName == "ItemSlot" || tName == "Slot")) itemSlotText = t;
                else if (rarityText == null && tName == "Rarity") rarityText = t;
                else if (itemLevelText == null && (tName == "Level" || tName == "ItemLevel")) itemLevelText = t;
                else if (affixesText == null && tName == "Affixes") affixesText = t;
                else if (statsText == null && tName == "Stats") statsText = t;
                else if (currentEquippedText == null && (tName == "CurrentEquipped" || tName == "CurrentEquippedText")) currentEquippedText = t;
                else if (newDropText == null && (tName == "NewDrop" || tName == "NewDropText")) newDropText = t;
                else if (powerChangeText == null && (tName == "PowerChange" || tName == "PowerChangeText")) powerChangeText = t;
                else if (statChangesText == null && (tName == "StatChanges" || tName == "StatChangesText")) statChangesText = t;
            }
        }

        private void OnEnable()
        {
            FindReferencesIfMissing();
            BindButtons();
            ModalCoordinator.Instance?.RegisterModalView(this);
            EventBus.OnLootDecisionRequested += HandleLootDecisionRequested;
            EventBus.OnBattleStateChanged += HandleBattleStateChanged;
            EventBus.OnLootDecisionCompleted += HandleLootDecisionCompleted;
        }

        private void OnDisable()
        {
            ModalCoordinator.Instance?.UnregisterModalView(this);
            EventBus.OnLootDecisionRequested -= HandleLootDecisionRequested;
            EventBus.OnBattleStateChanged -= HandleBattleStateChanged;
            EventBus.OnLootDecisionCompleted -= HandleLootDecisionCompleted;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetReferences(
            GameObject panelGO,
            TextMeshProUGUI nameTmp,
            TextMeshProUGUI slotTmp,
            TextMeshProUGUI rarityTmp,
            TextMeshProUGUI levelTmp,
            TextMeshProUGUI affixesTmp,
            TextMeshProUGUI statsTmp,
            Button eqBtn,
            Button disBtn,
            TextMeshProUGUI curEqTmp = null,
            TextMeshProUGUI newDrTmp = null,
            TextMeshProUGUI pwrChgTmp = null,
            TextMeshProUGUI statChgTmp = null,
            TextMeshProUGUI hdrTmp = null)
        {
            panel = panelGO;
            itemNameText = nameTmp;
            itemSlotText = slotTmp;
            rarityText = rarityTmp;
            itemLevelText = levelTmp;
            affixesText = affixesTmp;
            statsText = statsTmp;
            equipButton = eqBtn;
            dismantleButton = disBtn;
            currentEquippedText = curEqTmp;
            newDropText = newDrTmp;
            powerChangeText = pwrChgTmp;
            statChangesText = statChgTmp;
            headerText = hdrTmp;

            BindButtons();
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void BindButtons()
        {
            if (equipButton != null)
            {
                equipButton.onClick.RemoveListener(OnEquipClicked);
                equipButton.onClick.AddListener(OnEquipClicked);
            }

            if (dismantleButton != null)
            {
                dismantleButton.onClick.RemoveListener(OnDismantleClicked);
                dismantleButton.onClick.AddListener(OnDismantleClicked);
            }
        }

        public void HandleLootDecisionRequested(EquipmentInstance item)
        {
            if (item == null) return;
            isProcessingTransaction = false;

            if (ModalCoordinator.Instance != null)
            {
                var req = new ModalRequest(ModalId, DefaultPriority, isDismissable: false, payload: item);
                ModalCoordinator.Instance.RequestModal(req);
            }
            else
            {
                displayedItem = item;
                DisplayComparison(item);
                if (panel != null)
                {
                    panel.SetActive(true);
                }
                if (equipButton != null) equipButton.interactable = true;
                if (dismantleButton != null) dismantleButton.interactable = true;
            }

            Debug.Log($"[LOOT UI] Displaying equipment power comparison: {item.ItemName} ({item.SlotType})");
        }

        private void HandleBattleStateChanged(BattleState newState)
        {
            // Intermediate BattleState changes must not complete or drain the modal.
            // Transaction completion is strictly driven by CompleteLootDecisionAndResume.
        }

        private void HandleLootDecisionCompleted(EquipmentInstance item)
        {
            // OnLootDecisionCompleted is raised mid-transaction by BattleManager.
            // Do NOT complete or drain modal here; completion must occur only after
            // CompleteLootDecisionAndResume returns true in OnEquipClicked/OnDismantleClicked.
        }

        public void DisplayComparison(EquipmentInstance newItem)
        {
            if (newItem == null) return;
            isProcessingTransaction = false;
            if (panel != null)
            {
                panel.SetActive(true);
            }
            if (equipButton != null) equipButton.interactable = true;
            if (dismantleButton != null) dismantleButton.interactable = true;

            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            EquipmentManager eqMgr = EquipmentManager.Instance != null ? EquipmentManager.Instance : UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();

            // Perform pure, non-mutating comparison calculation
            latestComparison = CombatPowerCalculator.CalculateComparison(hero, eqMgr, newItem);

            EquipmentInstance curItem = latestComparison.CurrentEquippedItem;
            int curPower = latestComparison.CurrentPower;
            int projPower = latestComparison.ProjectedPower;
            int diff = latestComparison.PowerDifference;

            // 1. Header
            if (headerText != null)
            {
                headerText.text = "SO SÁNH TRANG BỊ";
            }

            // 2. Currently Equipped Item
            if (currentEquippedText != null)
            {
                if (curItem != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<color=#FFD700>ĐANG TRANG BỊ</color>");
                    sb.AppendLine($"<b>{curItem.ItemName}</b>");
                    sb.AppendLine($"Loại: {curItem.SlotType} | Cấp Rơi: {curItem.LootTier}");
                    sb.AppendLine($"Cấp: Lv {curItem.EquipmentLevel} / {curItem.MaxLevel}");
                    sb.AppendLine($"Lực chiến: {latestComparison.CurrentItemPower:N0}");
                    currentEquippedText.text = sb.ToString().TrimEnd();
                }
                else
                {
                    currentEquippedText.text = "<color=#FFD700>ĐANG TRANG BỊ</color>\n(TRỐNG)";
                }
            }

            // 3. New Drop Item
            if (newDropText != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<color=#00FFFF>VẬT PHẨM MỚI</color>");
                sb.AppendLine($"<b>{newItem.ItemName}</b>");
                sb.AppendLine($"Loại: {newItem.SlotType} | Cấp Rơi: {newItem.LootTier}");
                sb.AppendLine($"Cấp: Lv {newItem.EquipmentLevel} / {newItem.MaxLevel}");
                sb.AppendLine($"Phẩm chất: {(newItem.Rarity != null ? newItem.Rarity.DisplayName : "-")}");
                sb.AppendLine($"Lực chiến: {latestComparison.NewItemPower:N0}");
                newDropText.text = sb.ToString().TrimEnd();
            }

            // 4. Power Change
            if (powerChangeText != null)
            {
                string sign = diff >= 0 ? $"+{diff:N0}" : $"{diff:N0}";
                string colorHex = diff >= 0 ? "#00FF66" : "#FF4444";
                string prefix = diff >= 0 ? "TĂNG LỰC CHIẾN" : "GIẢM LỰC CHIẾN";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Lực chiến hiện tại: {curPower:N0}  ->  Sau trang bị: {projPower:N0}");
                sb.AppendLine($"Biến động: <color={colorHex}><b>{sign} ({prefix})</b></color>");
                powerChangeText.text = sb.ToString().TrimEnd();
            }

            // 5. Individual Stat Changes Breakdown
            if (statChangesText != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<color=#FFCC00>THỐNG KÊ THUỘC TÍNH</color>");

                AppendStatLine(sb, "HP", StatType.MaxHealth, false);
                AppendStatLine(sb, "ATK", StatType.Attack, false);
                AppendStatLine(sb, "DEF", StatType.Defense, false);
                AppendStatLine(sb, "Crit Rate", StatType.CritRate, true);
                AppendStatLine(sb, "Crit DMG", StatType.CritDamage, true);
                AppendStatLine(sb, "Dodge", StatType.Dodge, true);
                AppendStatLine(sb, "Lifesteal", StatType.Lifesteal, true);

                // Check other non-zero affixes
                if (latestComparison.CurrentStats.TryGetValue(StatType.ComboRate, out float comboCur) || latestComparison.ProjectedStats.TryGetValue(StatType.ComboRate, out float comboProj))
                {
                    if (comboCur > 0 || (latestComparison.ProjectedStats.ContainsKey(StatType.ComboRate) && latestComparison.ProjectedStats[StatType.ComboRate] > 0))
                    {
                        AppendStatLine(sb, "Combo", StatType.ComboRate, true);
                    }
                }
                if (latestComparison.CurrentStats.TryGetValue(StatType.CounterRate, out float counterCur) || latestComparison.ProjectedStats.TryGetValue(StatType.CounterRate, out float counterProj))
                {
                    if (counterCur > 0 || (latestComparison.ProjectedStats.ContainsKey(StatType.CounterRate) && latestComparison.ProjectedStats[StatType.CounterRate] > 0))
                    {
                        AppendStatLine(sb, "Counter", StatType.CounterRate, true);
                    }
                }

                statChangesText.text = sb.ToString().TrimEnd();
            }

            // Backwards compatibility for existing legacy text fields
            if (itemNameText != null) itemNameText.text = newItem.ItemName;
            if (itemSlotText != null) itemSlotText.text = $"Slot: {newItem.SlotType}";
            if (rarityText != null)
            {
                rarityText.text = $"Rarity: {(newItem.Rarity != null ? newItem.Rarity.DisplayName : "-")}";
                if (newItem.Rarity != null) rarityText.color = newItem.Rarity.RarityColor;
            }
            if (itemLevelText != null) itemLevelText.text = $"Level: {newItem.EquipmentLevel} / {newItem.MaxLevel}";

            if (affixesText != null)
            {
                if (newItem.Affixes != null && newItem.Affixes.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Affixes:");
                    foreach (var affix in newItem.Affixes)
                    {
                        if (affix == null) continue;
                        sb.AppendLine($" - {affix.DisplayName}: +{affix.Value:F0}");
                    }
                    affixesText.text = sb.ToString().TrimEnd();
                }
                else
                {
                    affixesText.text = "Affixes: (None)";
                }
            }

            if (statsText != null)
            {
                string pwrSign = diff >= 0 ? $"+{diff}" : $"{diff}";
                statsText.text = $"Power: {curPower} -> {projPower} ({pwrSign})";
            }
        }

        private void AppendStatLine(StringBuilder sb, string label, StatType stat, bool isPercent)
        {
            var (cur, proj, delta) = latestComparison.GetStatComparison(stat);

            string deltaStr;
            string colorHex;

            if (isPercent)
            {
                if (Mathf.Abs(delta) < 0.001f)
                {
                    deltaStr = "0%";
                    colorHex = "#AAAAAA";
                }
                else if (delta > 0)
                {
                    deltaStr = $"+{delta:F1}%";
                    colorHex = "#00FF66";
                }
                else
                {
                    deltaStr = $"{delta:F1}%";
                    colorHex = "#FF4444";
                }
                sb.AppendLine($"{label,-10}: {cur:F1}% -> {proj:F1}%   <color={colorHex}>{deltaStr}</color>");
            }
            else
            {
                if (Mathf.Abs(delta) < 0.001f)
                {
                    deltaStr = "0";
                    colorHex = "#AAAAAA";
                }
                else if (delta > 0)
                {
                    deltaStr = $"+{delta:F0}";
                    colorHex = "#00FF66";
                }
                else
                {
                    deltaStr = $"{delta:F0}";
                    colorHex = "#FF4444";
                }
                sb.AppendLine($"{label,-10}: {cur:F0} -> {proj:F0}   <color={colorHex}>{deltaStr}</color>");
            }
        }

        public void DisplayItem(EquipmentInstance item)
        {
            DisplayComparison(item);
        }

        public void OnEquipClicked()
        {
            if (isProcessingTransaction) return;
            isProcessingTransaction = true;

            if (equipButton != null) equipButton.interactable = false;
            if (dismantleButton != null) dismantleButton.interactable = false;

            Debug.Log("[LOOT UI] [EQUIP] button clicked");
            var capturedRequest = _activeModalRequest;
            var bm = BattleManager.Instance != null ? BattleManager.Instance : UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            bool success = false;
            if (bm != null)
            {
                success = bm.CompleteLootDecisionAndResume(equip: true, dismantle: false);
            }

            if (success)
            {
                displayedItem = null;
                latestComparison = null;
                isProcessingTransaction = false;
                if (ModalCoordinator.Instance != null && capturedRequest != null)
                {
                    ModalCoordinator.Instance.CompleteActiveModal(capturedRequest);
                }
                else if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
            else
            {
                // Transaction returned false or no BattleManager exists:
                // retain the Equipment Comparison, retain its payload, re-enable Equip/Tách and do not drain the queue
                isProcessingTransaction = false;
                if (equipButton != null) equipButton.interactable = true;
                if (dismantleButton != null) dismantleButton.interactable = true;
                Debug.LogWarning("[LOOT UI] Equip transaction failed or no BattleManager; retaining modal.");
            }
        }

        public void OnDismantleClicked()
        {
            if (isProcessingTransaction) return;
            isProcessingTransaction = true;

            if (equipButton != null) equipButton.interactable = false;
            if (dismantleButton != null) dismantleButton.interactable = false;

            Debug.Log("[LOOT UI] [TÁCH] button clicked");
            var capturedRequest = _activeModalRequest;
            var bm = BattleManager.Instance != null ? BattleManager.Instance : UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            bool success = false;
            if (bm != null)
            {
                success = bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
            }

            if (success)
            {
                displayedItem = null;
                latestComparison = null;
                isProcessingTransaction = false;
                if (ModalCoordinator.Instance != null && capturedRequest != null)
                {
                    ModalCoordinator.Instance.CompleteActiveModal(capturedRequest);
                }
                else if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
            else
            {
                // Transaction returned false or no BattleManager exists:
                // retain the Equipment Comparison, retain its payload, re-enable Equip/Tách and do not drain the queue
                isProcessingTransaction = false;
                if (equipButton != null) equipButton.interactable = true;
                if (dismantleButton != null) dismantleButton.interactable = true;
                Debug.LogWarning("[LOOT UI] Dismantle transaction failed or no BattleManager; retaining modal.");
            }
        }
    }
}
