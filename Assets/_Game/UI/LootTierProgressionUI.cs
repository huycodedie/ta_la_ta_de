using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Progression;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.UI
{
    public class LootTierProgressionUI : MonoBehaviour, IModalView
    {
        public static LootTierProgressionUI Instance { get; private set; }

        [Header("Main Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;

        [Header("Header Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI currentTierText;
        [SerializeField] private TextMeshProUGUI nextTierText;

        [Header("Rarity Table Comparison")]
        [SerializeField] private TextMeshProUGUI rarityComparisonText;

        [Header("Progress & Timer")]
        [SerializeField] private TextMeshProUGUI upgradeInfoText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI progressNumbersText;

        [Header("Action Button")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;

        public GameObject Panel => panel;
        public Button ToggleButton => toggleButton;
        public Button CloseButton => closeButton;
        public Button UpgradeButton => upgradeButton;
        public TextMeshProUGUI RarityComparisonText => rarityComparisonText;
        public TextMeshProUGUI ProgressNumbersText => progressNumbersText;
        public TextMeshProUGUI TimerText => timerText;
        public TextMeshProUGUI CurrentTierText => currentTierText;
        public TextMeshProUGUI NextTierText => nextTierText;
        public TextMeshProUGUI UpgradeButtonText => upgradeButtonText;

        // IModalView implementation
        public string ModalId => "LootTierProgression";
        public ModalPriority DefaultPriority => ModalPriority.SystemProgression;
        public bool IsDismissable => true;
        public bool IsVisible => panel != null && panel.activeSelf;
        private ModalRequest _activeModalRequest;

        public void ShowModal(ModalRequest request = null)
        {
            _activeModalRequest = request;
            if (panel != null)
            {
                panel.SetActive(true);
                RefreshUI();
            }
        }

        public void HideModal(DismissalReason reason = DismissalReason.UserClosed)
        {
            _activeModalRequest = null;
            if (panel != null)
            {
                panel.SetActive(false);
            }
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
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(TogglePanel);
                toggleButton.onClick.AddListener(TogglePanel);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HidePanel);
                closeButton.onClick.AddListener(HidePanel);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            SubscribeToEvents();
            RefreshUI();
            ModalCoordinator.Instance?.RegisterModalView(this);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            ModalCoordinator.Instance?.RegisterModalView(this);
        }

        private void OnDisable()
        {
            ModalCoordinator.Instance?.UnregisterModalView(this);
            UnsubscribeFromEvents();
        }

        public void SubscribeToEvents()
        {
            UnsubscribeFromEvents();
            EventBus.OnLootTierChanged += HandleLootTierChanged;
            EventBus.OnLootTierProgressChanged += HandleLootTierProgressChanged;
            EventBus.OnLootTierUpgradeStarted += HandleLootTierUpgradeStarted;
            EventBus.OnLootTierUpgradeCompleted += HandleLootTierUpgradeCompleted;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= HandleResourcesChanged;
                ResourceManager.Instance.OnResourcesChanged += HandleResourcesChanged;
            }
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.OnLootTierChanged -= HandleLootTierChanged;
            EventBus.OnLootTierProgressChanged -= HandleLootTierProgressChanged;
            EventBus.OnLootTierUpgradeStarted -= HandleLootTierUpgradeStarted;
            EventBus.OnLootTierUpgradeCompleted -= HandleLootTierUpgradeCompleted;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= HandleResourcesChanged;
            }
        }

        private void HandleResourcesChanged(int gold, int mat)
        {
            RefreshUI();
        }

        private void Update()
        {
            if (panel != null && panel.activeInHierarchy)
            {
                UpdateTimerDisplay();
            }
        }

        public void SetReferences(
            GameObject pnl,
            Button toggleBtn,
            Button closeBtn,
            TextMeshProUGUI titleTmp,
            TextMeshProUGUI curTierTmp,
            TextMeshProUGUI nextTierTmp,
            TextMeshProUGUI rarityCompTmp,
            TextMeshProUGUI upgInfoTmp,
            TextMeshProUGUI timerTmp,
            Image fillImg,
            TextMeshProUGUI progNumTmp,
            Button upgBtn,
            TextMeshProUGUI upgBtnTmp)
        {
            panel = pnl;
            toggleButton = toggleBtn;
            closeButton = closeBtn;
            titleText = titleTmp;
            currentTierText = curTierTmp;
            nextTierText = nextTierTmp;
            rarityComparisonText = rarityCompTmp;
            upgradeInfoText = upgInfoTmp;
            timerText = timerTmp;
            progressBarFill = fillImg;
            progressNumbersText = progNumTmp;
            upgradeButton = upgBtn;
            upgradeButtonText = upgBtnTmp;

            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(TogglePanel);
                toggleButton.onClick.AddListener(TogglePanel);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HidePanel);
                closeButton.onClick.AddListener(HidePanel);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            SubscribeToEvents();
            RefreshUI();
        }

        public void TogglePanel()
        {
            if (panel != null && panel.activeSelf)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        public void ShowPanel()
        {
            if (ModalCoordinator.Instance != null)
            {
                var req = new ModalRequest(ModalId, DefaultPriority, isDismissable: true);
                ModalCoordinator.Instance.RequestModal(req);
            }
            else
            {
                if (panel != null)
                {
                    panel.SetActive(true);
                    RefreshUI();
                }
            }
        }

        public void HidePanel()
        {
            if (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveRequest != null && ModalCoordinator.Instance.ActiveRequest.ModalId == ModalId)
            {
                ModalCoordinator.Instance.DismissActiveModal(DismissalReason.UserClosed, _activeModalRequest);
            }
            else if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void RefreshUI()
        {
            var mgr = LootTierProgressionManager.Instance;
            if (mgr == null) return;

            var curTier = mgr.GetCurrentTier();
            var nextTier = mgr.GetNextTier();
            int curProgress = mgr.GetCurrentProgress();
            int reqProgress = mgr.GetRequiredProgress();
            bool isMax = mgr.IsMaxTier();
            bool isUpgrading = mgr.IsUpgrading;

            int ownedGold = ResourceManager.Instance != null ? ResourceManager.Instance.Gold : 0;
            int ownedMaterial = ResourceManager.Instance != null ? ResourceManager.Instance.Material : 0;
            int reqGold = curTier != null ? curTier.UpgradeCostGold : 0;
            int reqMaterial = curTier != null ? curTier.UpgradeCostMaterial : 0;

            // 1. Header Tiers
            if (currentTierText != null)
            {
                currentTierText.text = curTier != null ? $"Cấp hiện tại: <color=#00FFFF>{curTier.TierId}</color>" : "Cấp hiện tại: 1";
            }

            if (nextTierText != null)
            {
                if (isMax)
                {
                    nextTierText.text = "<color=#FFD700>CẤP CAO NHẤT</color>";
                }
                else
                {
                    nextTierText.text = nextTier != null ? $"Cấp tiếp theo: <color=#FFD700>{nextTier.TierId}</color>" : "Cấp tiếp theo: -";
                }
            }

            // 2. Rarity Probability Table Comparison
            if (rarityComparisonText != null)
            {
                rarityComparisonText.text = GenerateRarityComparisonTable(curTier, nextTier, mgr.Database);
            }

            // 3. Progress Info & Fill
            int numCurrent;
            int numRequired;
            if (reqGold > 0)
            {
                numCurrent = ownedGold;
                numRequired = reqGold;
            }
            else if (reqProgress > 0)
            {
                numCurrent = curProgress;
                numRequired = reqProgress;
            }
            else
            {
                numCurrent = curProgress;
                numRequired = 1;
            }

            if (progressNumbersText != null)
            {
                if (isMax)
                {
                    progressNumbersText.text = "MAX";
                }
                else
                {
                    string curNumStr = numCurrent.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                    string reqNumStr = numRequired.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                    progressNumbersText.text = $"{curNumStr} / {reqNumStr}";
                }
            }

            if (progressBarFill != null)
            {
                if (isMax)
                {
                    progressBarFill.fillAmount = 1f;
                }
                else
                {
                    progressBarFill.fillAmount = numRequired > 0 ? Mathf.Clamp01((float)numCurrent / numRequired) : 0f;
                }
            }

            if (upgradeInfoText != null)
            {
                if (isMax)
                {
                    upgradeInfoText.text = "Cấp nâng cấp: <color=#FFD700>ĐÃ ĐẠT TỐI ĐA</color>";
                }
                else
                {
                    string reqGoldStr = reqGold.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                    string reqMatStr = reqMaterial.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                    string ownedGoldStr = ownedGold.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                    string ownedMatStr = ownedMaterial.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"<b>UPGRADE PREVIEW:</b> Tier {curTier?.TierId} -> Tier {nextTier?.TierId}");
                    sb.AppendLine($"<b>Chi phí:</b> Gold <color=#FFD700>{reqGoldStr}</color> | Nguyên liệu <color=#00FFFF>{reqMatStr}</color>");
                    sb.AppendLine($"<b>Hiện có:</b> Gold <color=#FFD700>{ownedGoldStr}</color> | Nguyên liệu <color=#00FFFF>{ownedMatStr}</color>");
                    upgradeInfoText.text = sb.ToString().TrimEnd();
                }
            }

            // 4. Timer & Upgrade Button State
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            var mgr = LootTierProgressionManager.Instance;
            if (mgr == null) return;

            var curTier = mgr.GetCurrentTier();
            var nextTier = mgr.GetNextTier();
            bool isMax = mgr.IsMaxTier();
            bool isUpgrading = mgr.IsUpgrading;
            float remaining = mgr.GetUpgradeRemainingSeconds();

            if (isMax)
            {
                if (timerText != null) timerText.text = "Thời gian tăng cấp: --:--:--";
                if (upgradeButton != null) upgradeButton.interactable = false;
                if (upgradeButtonText != null) upgradeButtonText.text = "CẤP CAO NHẤT";
            }
            else if (isUpgrading)
            {
                if (remaining > 0f)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(remaining);
                    string timeFormatted = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                    if (timerText != null) timerText.text = $"Thời gian tăng cấp: <color=#00FFFF>{timeFormatted}</color>";
                    if (upgradeButton != null) upgradeButton.interactable = false;
                    if (upgradeButtonText != null) upgradeButtonText.text = $"ĐANG NÂNG CẤP ({timeFormatted})";
                }
                else
                {
                    if (timerText != null) timerText.text = "Thời gian tăng cấp: <color=#00FF00>00:00:00</color>";
                    if (upgradeButton != null) upgradeButton.interactable = true;
                    if (upgradeButtonText != null) upgradeButtonText.text = "HOÀN THÀNH";
                }
            }
            else
            {
                float duration = curTier != null ? curTier.UpgradeDurationSeconds : 900f;
                TimeSpan ts = TimeSpan.FromSeconds(duration);
                string timeFormatted = $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                if (timerText != null) timerText.text = $"Thời gian tăng cấp: {timeFormatted}";

                bool canUpgrade = mgr.CanUpgrade();
                if (upgradeButton != null) upgradeButton.interactable = canUpgrade;
                if (upgradeButtonText != null) upgradeButtonText.text = "TĂNG BẬC";
            }
        }

        public string GenerateRarityComparisonTable(LootTierConfigSO curTier, LootTierConfigSO nextTier, LootTierDatabaseSO db)
        {
            // Measured column widths:
            // Column 1 (Phẩm chất): min 130 reference units
            // Column 2 (Hiện tại):  min  80 reference units
            // Gap / Arrow (->):     min  40 reference units
            // Column 3 (Tiếp theo): min  80 reference units
            // Gap:                  min  20 reference units
            // Column 4 (Biến động): min  90 reference units
            // Horizontal Padding:   min  32 reference units (16 left + 16 right)
            // Total minimum required width for 4-column layout = 472 units.
            const float RequiredMultiColumnWidth = 472f;

            float availableWidth = 600f;
            if (rarityComparisonText != null && rarityComparisonText.rectTransform != null)
            {
                float w = rarityComparisonText.rectTransform.rect.width;
                if (w > 0f) availableWidth = w;
            }
            else if (panel != null)
            {
                RectTransform prt = panel.GetComponent<RectTransform>();
                if (prt != null && prt.rect.width > 0f) availableWidth = prt.rect.width - 64f;
            }

            StringBuilder sb = new StringBuilder();

            // Gather all distinct rarities from database or tiers
            List<RarityDefinitionSO> allRarities = new List<RarityDefinitionSO>();
            HashSet<string> seenIds = new HashSet<string>();

            if (curTier != null && curTier.RarityWeights != null)
            {
                foreach (var rw in curTier.RarityWeights)
                {
                    if (rw != null && rw.Rarity != null && !seenIds.Contains(rw.Rarity.RarityId))
                    {
                        allRarities.Add(rw.Rarity);
                        seenIds.Add(rw.Rarity.RarityId);
                    }
                }
            }

            if (nextTier != null && nextTier.RarityWeights != null)
            {
                foreach (var rw in nextTier.RarityWeights)
                {
                    if (rw != null && rw.Rarity != null && !seenIds.Contains(rw.Rarity.RarityId))
                    {
                        allRarities.Add(rw.Rarity);
                        seenIds.Add(rw.Rarity.RarityId);
                    }
                }
            }

            allRarities.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));

            if (allRarities.Count == 0)
            {
                sb.AppendLine("  (Chưa có dữ liệu phẩm chất)");
                return sb.ToString();
            }

            bool useStacked = availableWidth < RequiredMultiColumnWidth;

            if (useStacked)
            {
                sb.AppendLine("<b><color=#CCCCCC>TỶ LỆ RƠI THEO PHẨM CHẤT</color></b>");
                foreach (var r in allRarities)
                {
                    float curWeight = curTier != null ? curTier.GetRarityWeight(r.RarityId) : 0f;
                    float nextWeight = nextTier != null ? nextTier.GetRarityWeight(r.RarityId) : 0f;

                    string colorHex = ColorUtility.ToHtmlStringRGB(r.RarityColor);
                    string rName = $"<color=#{colorHex}><b>{r.DisplayName}</b></color>";
                    string curStr = curWeight > 0f ? $"{curWeight:F1}%" : "0.0%";
                    string nextStr = nextTier != null ? (nextWeight > 0f ? $"{nextWeight:F1}%" : "0.0%") : "-";

                    string diffStr;
                    if (nextTier == null)
                    {
                        diffStr = "-";
                    }
                    else if (curWeight <= 0f && nextWeight > 0f)
                    {
                        diffStr = "<color=#00FFFF>UNLOCK</color>";
                    }
                    else if (curWeight > 0f && nextWeight <= 0f)
                    {
                        diffStr = "<color=#888888>LOCKED</color>";
                    }
                    else if (nextWeight > curWeight)
                    {
                        diffStr = $"<color=#00FF00>+{nextWeight - curWeight:F1}%</color>";
                    }
                    else if (nextWeight < curWeight)
                    {
                        diffStr = $"<color=#FF5555>-{curWeight - nextWeight:F1}%</color>";
                    }
                    else
                    {
                        diffStr = "<color=#888888>0.0%</color>";
                    }

                    sb.AppendLine($"{rName}: {curStr} -> {nextStr} ({diffStr})");
                }
            }
            else
            {
                sb.AppendLine("<b><color=#CCCCCC>PHẨM CHẤT         HIỆN TẠI      TIẾP THEO     BIẾN ĐỘNG</color></b>");

                foreach (var r in allRarities)
                {
                    float curWeight = curTier != null ? curTier.GetRarityWeight(r.RarityId) : 0f;
                    float nextWeight = nextTier != null ? nextTier.GetRarityWeight(r.RarityId) : 0f;

                    string colorHex = ColorUtility.ToHtmlStringRGB(r.RarityColor);
                    string rName = $"<color=#{colorHex}>{r.DisplayName,-13}</color>";
                    string curStr = curWeight > 0f ? $"{curWeight,5:F1}%" : " 0.0%";
                    string nextStr = nextTier != null ? (nextWeight > 0f ? $"{nextWeight,5:F1}%" : " 0.0%") : "    -";

                    string diffStr;
                    if (nextTier == null)
                    {
                        diffStr = "  -   ";
                    }
                    else if (curWeight <= 0f && nextWeight > 0f)
                    {
                        diffStr = "<color=#00FFFF>UNLOCK</color>";
                    }
                    else if (curWeight > 0f && nextWeight <= 0f)
                    {
                        diffStr = "<color=#888888>LOCKED</color>";
                    }
                    else if (nextWeight > curWeight)
                    {
                        diffStr = $"<color=#00FF00>+{nextWeight - curWeight,4:F1}%</color>";
                    }
                    else if (nextWeight < curWeight)
                    {
                        diffStr = $"<color=#FF5555>-{curWeight - nextWeight,4:F1}%</color>";
                    }
                    else
                    {
                        diffStr = "<color=#888888>  0.0%</color>";
                    }

                    sb.AppendLine($"{rName}  {curStr}   ->   {nextStr}   {diffStr}");
                }
            }

            return sb.ToString();
        }

        private void OnUpgradeButtonClicked()
        {
            var mgr = LootTierProgressionManager.Instance;
            if (mgr == null) return;

            if (mgr.IsUpgrading)
            {
                if (mgr.GetUpgradeRemainingSeconds() <= 0f)
                {
                    mgr.CompleteUpgrade();
                }
            }
            else
            {
                if (mgr.CanUpgrade())
                {
                    mgr.StartUpgrade();
                }
            }

            RefreshUI();
        }

        private void HandleLootTierChanged(LootTierConfigSO newTier) => RefreshUI();
        private void HandleLootTierProgressChanged(int cur, int req) => RefreshUI();
        private void HandleLootTierUpgradeStarted() => RefreshUI();
        private void HandleLootTierUpgradeCompleted(LootTierConfigSO newTier) => RefreshUI();
    }
}
