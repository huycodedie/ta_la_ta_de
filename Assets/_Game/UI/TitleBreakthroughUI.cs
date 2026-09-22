using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.Items;
using WuxiaGame.Progression;
using WuxiaGame.Stats;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.UI
{
    public class TitleBreakthroughUI : MonoBehaviour, IModalView
    {
        public static TitleBreakthroughUI Instance { get; private set; }

        [Header("UI Panels & Root")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;

        [Header("Header Text Elements")]
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [SerializeField] private TextMeshProUGUI currentTitleText;
        [SerializeField] private TextMeshProUGUI nextTitleText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI levelCapText;

        [Header("Content Text Elements")]
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private TextMeshProUGUI statComparisonText;

        [Header("Action Buttons")]
        [SerializeField] private Button breakthroughButton;
        [SerializeField] private TextMeshProUGUI breakthroughButtonText;

        public GameObject Panel => panel;
        public Button ToggleButton => toggleButton;
        public Button CloseButton => closeButton;
        public Button BreakthroughButton => breakthroughButton;
        public TextMeshProUGUI RequirementsText => requirementsText;
        public TextMeshProUGUI RewardsText => rewardsText;
        public TextMeshProUGUI StatComparisonText => statComparisonText;

        // IModalView implementation
        public string ModalId => "TitleBreakthrough";
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

        public static void ResetInstance()
        {
            Instance = null;
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
            BindButtons();
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
            EventBus.OnLevelChanged += HandleLevelChanged;
            EventBus.OnExpChanged += HandleExpChanged;
            EventBus.OnTitleChanged += HandleTitleChanged;
            EventBus.OnBreakthroughStatusChanged += HandleBreakthroughStatusChanged;
            EventBus.OnLootTierChanged += HandleLootTierChanged;
            EventBus.OnEquipmentEquipped += HandleEquipmentEquipped;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= HandleResourcesChanged;
                ResourceManager.Instance.OnResourcesChanged += HandleResourcesChanged;
            }
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.OnLevelChanged -= HandleLevelChanged;
            EventBus.OnExpChanged -= HandleExpChanged;
            EventBus.OnTitleChanged -= HandleTitleChanged;
            EventBus.OnBreakthroughStatusChanged -= HandleBreakthroughStatusChanged;
            EventBus.OnLootTierChanged -= HandleLootTierChanged;
            EventBus.OnEquipmentEquipped -= HandleEquipmentEquipped;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= HandleResourcesChanged;
            }
        }

        private void HandleLevelChanged(int lvl) => RefreshUI();
        private void HandleExpChanged(float cur, float req) => RefreshUI();
        private void HandleTitleChanged(TitleConfigSO t) => RefreshUI();
        private void HandleBreakthroughStatusChanged(bool status) => RefreshUI();
        private void HandleResourcesChanged(int g, int m) => RefreshUI();
        private void HandleLootTierChanged(LootTierConfigSO tier) => RefreshUI();
        private void HandleEquipmentEquipped(EquipmentSlotType slot, EquipmentInstance item) => RefreshUI();

        private void BindButtons()
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

            if (breakthroughButton != null)
            {
                breakthroughButton.onClick.RemoveListener(OnBreakthroughButtonClicked);
                breakthroughButton.onClick.AddListener(OnBreakthroughButtonClicked);
            }
        }

        public void SetReferences(
            GameObject pnl,
            Button toggleBtn,
            Button closeBtn,
            TextMeshProUGUI headerTmp,
            TextMeshProUGUI curTitleTmp,
            TextMeshProUGUI nextTitleTmp,
            TextMeshProUGUI curLevelTmp,
            TextMeshProUGUI levelCapTmp,
            TextMeshProUGUI reqTmp,
            TextMeshProUGUI rewardTmp,
            TextMeshProUGUI statCompTmp,
            Button bkBtn,
            TextMeshProUGUI bkBtnTmp = null)
        {
            panel = pnl;
            toggleButton = toggleBtn;
            closeButton = closeBtn;
            headerTitleText = headerTmp;
            currentTitleText = curTitleTmp;
            nextTitleText = nextTitleTmp;
            currentLevelText = curLevelTmp;
            levelCapText = levelCapTmp;
            requirementsText = reqTmp;
            rewardsText = rewardTmp;
            statComparisonText = statCompTmp;
            breakthroughButton = bkBtn;
            breakthroughButtonText = bkBtnTmp;
            Instance = this;

            BindButtons();
            SubscribeToEvents();
            RefreshUI();
        }

        public void TogglePanel()
        {
            if (panel != null)
            {
                bool nextActive = !panel.activeSelf;
                if (nextActive) ShowPanel();
                else HidePanel();
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
                    Debug.Log("[TITLE BREAKTHROUGH] Panel Opened");
                    Debug.Log("[TITLE BREAKTHROUGH] Combat state preserved");
                    Debug.Log("[TITLE BREAKTHROUGH] Combat continues");
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

        public void OnBreakthroughButtonClicked()
        {
            var mgr = TitleBreakthroughManager.Instance;
            if (mgr != null)
            {
                bool success = mgr.TryPerformBreakthrough();
                if (success)
                {
                    RefreshUI();
                    Debug.Log("[TitleBreakthroughUI] Breakthrough executed successfully!");
                }
                else
                {
                    RefreshUI();
                    Debug.LogWarning("[TitleBreakthroughUI] Breakthrough execution failed.");
                }
            }
        }

        public void RefreshUI()
        {
            var mgr = TitleBreakthroughManager.Instance;
            if (mgr == null) return;

            var curStage = mgr.CurrentBreakthrough;
            var nextStage = mgr.NextBreakthrough;
            bool isMax = mgr.IsMaxStage;
            BreakthroughEvaluationResult eval = mgr.EvaluateBreakthrough();

            int curLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;

            // 1. Header Information
            if (headerTitleText != null)
            {
                headerTitleText.text = "ĐỘT PHÁ DANH HIỆU";
            }

            if (currentTitleText != null)
            {
                currentTitleText.text = $"Đột phá hiện tại: <color=#00FFFF>#{mgr.CurrentBreakthroughCount} [{mgr.CurrentTitleName}]</color>";
            }

            if (nextTitleText != null)
            {
                nextTitleText.text = isMax
                    ? "Đột phá lần kế tiếp: <color=#FFD700>[ ĐÃ ĐẠT TỐI ĐA ]</color>"
                    : $"Đột phá lần kế tiếp: <color=#FFD700>#{mgr.NextBreakthroughNumber} [{nextStage?.TitleName ?? "-"}]</color>";
            }

            if (currentLevelText != null)
            {
                currentLevelText.text = $"Cấp hiện tại: <color=#FFFFFF>Lv. {curLevel}</color>";
            }

            if (levelCapText != null)
            {
                levelCapText.text = isMax
                    ? $"Cấp tối đa: <color=#FFD700>{mgr.CurrentLevelCap} (MAX)</color>"
                    : $"Cấp tối đa: <color=#00FFFF>{mgr.CurrentLevelCap}</color> → <color=#FFD700>{mgr.NextLevelCap}</color>";
            }

            // 2. Requirements Section (ĐIỀU KIỆN)
            if (requirementsText != null)
            {
                if (isMax)
                {
                    requirementsText.text = "<color=#FFD700>ĐÃ ĐẠT CẤP ĐỘT PHÁ CAO NHẤT</color>";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<b><color=#FFD700>ĐIỀU KIỆN ĐỘT PHÁ</color></b>");
                    sb.AppendLine($"Đột phá lần: <color=#00FFFF>#{mgr.NextBreakthroughNumber}</color>\n");

                    if (eval.RequirementDetails != null && eval.RequirementDetails.Count > 0)
                    {
                        foreach (var req in eval.RequirementDetails)
                        {
                            string statusColor = req.IsSatisfied ? "#00FF66" : "#FF4444";
                            string statusIcon = req.IsSatisfied ? "<color=#00FF66>✓</color>" : "<color=#FF4444>✗</color>";
                            sb.AppendLine($"{req.DisplayText}  {statusIcon}");
                        }
                    }
                    else
                    {
                        // Fallback display
                        string lvlStatus = eval.LevelSatisfied ? "<color=#00FF66>✓</color>" : "<color=#FF4444>✗</color>";
                        sb.AppendLine($"Cấp Hero: {eval.CurrentLevel} / {eval.RequiredLevel}  {lvlStatus}");
                    }

                    sb.AppendLine();
                    string statusStr = eval.CanBreakthrough ? "<color=#00FF66>ĐỦ ĐIỀU KIỆN</color>" : "<color=#FF4444>CHƯA ĐỦ ĐIỀU KIỆN</color>";
                    sb.AppendLine($"Trạng thái: {statusStr}");

                    if (curLevel >= mgr.CurrentLevelCap)
                    {
                        sb.AppendLine("\n<b><color=#FFAA00>[ LEVEL CAP REACHED — BREAKTHROUGH REQUIRED ]</color></b>");
                    }

                    requirementsText.text = sb.ToString().TrimEnd();
                }
            }

            // 3. Rewards Section (THƯỞNG ĐỘT PHÁ)
            if (rewardsText != null)
            {
                if (isMax || nextStage == null)
                {
                    rewardsText.text = "Không có thưởng tiếp theo.";
                }
                else
                {
                    float deltaHp = nextStage.BaseMaxHealth - (curStage != null ? curStage.BaseMaxHealth : 1000f);
                    float deltaAtk = nextStage.BaseAttack - (curStage != null ? curStage.BaseAttack : 100f);
                    float deltaDef = nextStage.BaseDefense - (curStage != null ? curStage.BaseDefense : 20f);
                    float deltaCrit = nextStage.BonusCritRate - (curStage != null ? curStage.BonusCritRate : 0f);
                    float deltaCritDmg = nextStage.BonusCritDamage - (curStage != null ? curStage.BonusCritDamage : 0f);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<b><color=#FFD700>THƯỞNG ĐỘT PHÁ</color></b>");
                    if (deltaHp > 0) sb.AppendLine($"HP: <color=#00FF66>+{deltaHp:F0}</color>");
                    if (deltaAtk > 0) sb.AppendLine($"ATK: <color=#00FF66>+{deltaAtk:F0}</color>");
                    if (deltaDef > 0) sb.AppendLine($"DEF: <color=#00FF66>+{deltaDef:F0}</color>");
                    if (deltaCrit > 0) sb.AppendLine($"Crit Rate: <color=#00FF66>+{deltaCrit:F1}%</color>");
                    if (deltaCritDmg > 0) sb.AppendLine($"Crit Damage: <color=#00FF66>+{deltaCritDmg:F1}%</color>");

                    rewardsText.text = sb.ToString().TrimEnd();
                }
            }

            // 4. Stat Comparison Preview (SO SÁNH THUỘC TÍNH)
            if (statComparisonText != null)
            {
                statComparisonText.text = GenerateStatComparisonPreview(curStage, nextStage);
            }

            // 5. Action Button State
            if (breakthroughButton != null)
            {
                breakthroughButton.interactable = eval.CanBreakthrough;
            }

            if (breakthroughButtonText != null)
            {
                if (isMax)
                {
                    breakthroughButtonText.text = "ĐẠT TỐI ĐA";
                }
                else if (eval.CanBreakthrough)
                {
                    breakthroughButtonText.text = "TĂNG BẬC";
                }
                else
                {
                    breakthroughButtonText.text = "CHƯA ĐỦ ĐIỀU KIỆN";
                }
            }
        }

        private string GenerateStatComparisonPreview(TitleBreakthroughConfigSO curStage, TitleBreakthroughConfigSO nextStage)
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            float curHp = hero != null ? hero.Stats.GetValue(StatType.MaxHealth) : (curStage != null ? curStage.BaseMaxHealth : 1000f);
            float curAtk = hero != null ? hero.Stats.GetValue(StatType.Attack) : (curStage != null ? curStage.BaseAttack : 100f);
            float curDef = hero != null ? hero.Stats.GetValue(StatType.Defense) : (curStage != null ? curStage.BaseDefense : 20f);

            if (nextStage == null)
            {
                return $"<b><color=#00FFFF>THUỘC TÍNH HIỆN TẠI</color></b>\nHP: {curHp:F0}\nATK: {curAtk:F0}\nDEF: {curDef:F0}";
            }

            float deltaHp = nextStage.BaseMaxHealth - (curStage != null ? curStage.BaseMaxHealth : 1000f);
            float deltaAtk = nextStage.BaseAttack - (curStage != null ? curStage.BaseAttack : 100f);
            float deltaDef = nextStage.BaseDefense - (curStage != null ? curStage.BaseDefense : 20f);

            float nextHp = curHp + deltaHp;
            float nextAtk = curAtk + deltaAtk;
            float nextDef = curDef + deltaDef;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b><color=#00FFFF>SO SÁNH THUỘC TÍNH (PREVIEW)</color></b>");
            sb.AppendLine($"HP:   {curHp,6:F0}  →  <color=#00FF66>{nextHp,6:F0}  (+{deltaHp:F0})</color>");
            sb.AppendLine($"ATK:  {curAtk,6:F0}  →  <color=#00FF66>{nextAtk,6:F0}  (+{deltaAtk:F0})</color>");
            sb.AppendLine($"DEF:  {curDef,6:F0}  →  <color=#00FF66>{nextDef,6:F0}  (+{deltaDef:F0})</color>");

            return sb.ToString().TrimEnd();
        }
    }
}
