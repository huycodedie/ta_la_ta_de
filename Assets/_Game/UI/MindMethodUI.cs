using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Progression;
using WuxiaGame.Stats;
using WuxiaGame.UI.Core;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.UI
{
    public class MindMethodUI : MonoBehaviour
    {
        public static MindMethodUI Instance { get; private set; }

        [Header("Root & Buttons")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;

        [Header("Mind Method Info UI")]
        [SerializeField] private TextMeshProUGUI activeTitleText;
        [SerializeField] private TextMeshProUGUI activeLevelText;
        [SerializeField] private TextMeshProUGUI activeDescText;
        [SerializeField] private TextMeshProUGUI passiveEffectsText;
        [SerializeField] private TextMeshProUGUI rageModifiersText;

        [Header("Mind Method List Container")]
        [SerializeField] private Transform mindMethodListContent;
        [SerializeField] private TextMeshProUGUI mindMethodSummaryText;

        [Header("5 Skill Slots UI")]
        [SerializeField] private TextMeshProUGUI slot1Text;
        [SerializeField] private TextMeshProUGUI slot2Text;
        [SerializeField] private TextMeshProUGUI slot3Text;
        [SerializeField] private TextMeshProUGUI slot4Text;
        [SerializeField] private TextMeshProUGUI slot5Text;

        [Header("Selected Slot Alternatives UI")]
        [SerializeField] private TextMeshProUGUI alternativesHeaderText;
        [SerializeField] private TextMeshProUGUI alternativesListText;

        [Header("Selected Slot Tracker")]
        [SerializeField] private SkillSlotType currentSelectedSlot = SkillSlotType.NormalAttack;

        public GameObject Panel => panel;
        public Button ToggleButton => toggleButton;
        public Button CloseButton => closeButton;
        public SkillSlotType CurrentSelectedSlot => currentSelectedSlot;

        public static void ResetInstance()
        {
            Instance = null;
        }

        public void SetReferences(
            GameObject pnl,
            Button toggleBtn,
            Button closeBtn,
            TextMeshProUGUI titleTmp,
            TextMeshProUGUI lvlTmp,
            TextMeshProUGUI descTmp,
            TextMeshProUGUI passiveTmp,
            TextMeshProUGUI rageTmp,
            TextMeshProUGUI summaryTmp,
            TextMeshProUGUI s1Tmp,
            TextMeshProUGUI s2Tmp,
            TextMeshProUGUI s3Tmp,
            TextMeshProUGUI s4Tmp,
            TextMeshProUGUI s5Tmp,
            TextMeshProUGUI altHeaderTmp,
            TextMeshProUGUI altListTmp)
        {
            panel = pnl;
            toggleButton = toggleBtn;
            closeButton = closeBtn;
            activeTitleText = titleTmp;
            activeLevelText = lvlTmp;
            activeDescText = descTmp;
            passiveEffectsText = passiveTmp;
            rageModifiersText = rageTmp;
            mindMethodSummaryText = summaryTmp;
            slot1Text = s1Tmp;
            slot2Text = s2Tmp;
            slot3Text = s3Tmp;
            slot4Text = s4Tmp;
            slot5Text = s5Tmp;
            alternativesHeaderText = altHeaderTmp;
            alternativesListText = altListTmp;
            Instance = this;

            BindButtons();
            SubscribeToEvents();
            RefreshUI();
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
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        public void SubscribeToEvents()
        {
            UnsubscribeFromEvents();
            EventBus.OnActiveMindMethodChanged += HandleActiveMindMethodChanged;
            EventBus.OnMindMethodUnlocked += HandleMindMethodUnlocked;
            EventBus.OnMindMethodLevelChanged += HandleMindMethodLevelChanged;
            EventBus.OnSkillUnlocked += HandleSkillUnlocked;
            EventBus.OnSkillSelected += HandleSkillSelected;
            EventBus.OnLevelChanged += HandleProgressionChanged;
            EventBus.OnLootTierChanged += HandleLootTierChanged;
            EventBus.OnRageChanged += HandleRageChanged;
            EventBus.OnSkillExecutionSucceeded += HandleSkillExecution;
            EventBus.OnSkillExecutionFailed += HandleSkillExecution;
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.OnActiveMindMethodChanged -= HandleActiveMindMethodChanged;
            EventBus.OnMindMethodUnlocked -= HandleMindMethodUnlocked;
            EventBus.OnMindMethodLevelChanged -= HandleMindMethodLevelChanged;
            EventBus.OnSkillUnlocked -= HandleSkillUnlocked;
            EventBus.OnSkillSelected -= HandleSkillSelected;
            EventBus.OnLevelChanged -= HandleProgressionChanged;
            EventBus.OnLootTierChanged -= HandleLootTierChanged;
            EventBus.OnRageChanged -= HandleRageChanged;
            EventBus.OnSkillExecutionSucceeded -= HandleSkillExecution;
            EventBus.OnSkillExecutionFailed -= HandleSkillExecution;
        }

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
        }

        public void TogglePanel()
        {
            if (panel == null) return;
            if (panel.activeSelf) HidePanel();
            else ShowPanel();
        }

        public void ShowPanel()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            Debug.Log("[MIND METHOD UI] Công Pháp Panel Opened");
            Debug.Log("[MIND METHOD UI] Combat state preserved (Combat continues in background)");
            RefreshUI();
        }

        public void HidePanel()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
            Debug.Log("[MIND METHOD UI] Công Pháp Panel Closed");
            Debug.Log("[MIND METHOD UI] Combat state preserved");

            var nav = GlobalBottomNavigation.Instance != null ? GlobalBottomNavigation.Instance : UnityEngine.Object.FindAnyObjectByType<GlobalBottomNavigation>();
            if (nav != null && nav.CurrentSelectedIndex == 1)
            {
                nav.SelectPosition(GlobalBottomNavigation.MainHubIndex);
            }
        }

        public void SelectSlot(SkillSlotType slot)
        {
            currentSelectedSlot = slot;
            RefreshUI();
        }

        public void RefreshUI()
        {
            var mgr = MindMethodManager.Instance;
            if (mgr == null) return;

            var activeDef = mgr.ActiveMindMethodDefinition;
            var activeState = mgr.ActiveMindMethodState;
            var db = mgr.Database;

            // 1. Active Mind Method Info
            if (activeDef != null && activeState != null)
            {
                if (activeTitleText != null)
                {
                    Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
                    float curRage = hero != null && hero.Rage != null ? hero.Rage.CurrentRage : 0f;
                    float maxRage = hero != null && hero.Rage != null ? hero.Rage.MaxRage : 100f;
                    activeTitleText.text = $"TÂM PHÁP ĐANG DÙNG: <color=#FFD700>{activeDef.MindMethodName}</color> | <color=#FF9900>Nộ: {curRage:F0}/{maxRage:F0}</color>";
                }

                if (activeLevelText != null)
                {
                    activeLevelText.text = $"Cấp độ: <color=#00FFFF>Lv.{activeState.Level} / {activeDef.MaxLevel}</color>";
                }

                if (activeDescText != null)
                {
                    activeDescText.text = activeDef.Description;
                }

                if (passiveEffectsText != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<color=#FFD700><b>HIỆU ỨNG NỘI TẠI (PASSIVE):</b></color>");
                    var stats = activeDef.PassiveData.GetStatModifiers(activeState.Level);
                    if (stats.Count == 0)
                    {
                        sb.AppendLine("- Không có thuộc tính cộng thêm");
                    }
                    else
                    {
                        foreach (var kvp in stats)
                        {
                            string sign = kvp.Value >= 0 ? "+" : "";
                            sb.AppendLine($"• {kvp.Key}: <color=#00FF66>{sign}{kvp.Value}</color>");
                        }
                    }

                    if (activeDef.PassiveData.HasReviveOnce)
                    {
                        string revStatus = activeState.ReviveOnceAvailable ? "<color=#00FF66>Sẵn sàng</color>" : "<color=#888888>Đã sử dụng</color>";
                        sb.AppendLine($"• <b>Quy Nhất Hồi Sinh</b>: Hồi sinh {activeDef.PassiveData.ReviveHealthPercent}% HP khi chịu sát thương chí mạng ({revStatus})");
                    }

                    passiveEffectsText.text = sb.ToString();
                }

                if (rageModifiersText != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<color=#FF9900><b>TÙY CHỈNH NỘ KHÍ (RAGE):</b></color>");
                    float atkRage = activeDef.PassiveData.BasicAttackRageModifier;
                    float dmgRage = activeDef.PassiveData.DamageTakenRageModifier;
                    string signAtk = atkRage >= 0 ? "+" : "";
                    string signDmg = dmgRage >= 0 ? "+" : "";
                    sb.AppendLine($"• Nộ khi Đánh thường: <color=#00FF66>{signAtk}{atkRage:F0}</color>");
                    sb.AppendLine($"• Nộ khi Chịu đòn: <color=#00FF66>{signDmg}{dmgRage:F0}</color>");
                    rageModifiersText.text = sb.ToString();
                }
            }

            // 2. Mind Method List Summary
            if (mindMethodSummaryText != null && db != null && db.MindMethods != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<color=#FFD700><b>DANH SÁCH TÂM PHÁP SỞ HỮU:</b></color>");

                foreach (var mm in db.MindMethods)
                {
                    if (mm == null) continue;
                    bool isUnlocked = mgr.IsMindMethodUnlocked(mm.MindMethodId);
                    bool isActive = (mm.MindMethodId == mgr.ActiveMindMethodId);
                    int lvl = mgr.GetMindMethodLevel(mm.MindMethodId);

                    string statusStr;
                    if (isActive) statusStr = "<color=#00FF66>[ĐANG DÙNG]</color>";
                    else if (isUnlocked) statusStr = "<color=#00FFFF>[ĐÃ MỞ - BẤM ĐỔI]</color>";
                    else statusStr = "<color=#FF4444>[CHƯA MỞ]</color>";

                    sb.AppendLine($"• <b>{mm.MindMethodName}</b> (Lv.{lvl}) — {statusStr}");
                }
                mindMethodSummaryText.text = sb.ToString();
            }

            // 3. Five Skill Slots Display
            UpdateSkillSlotText(slot1Text, SkillSlotType.NormalAttack, "Slot 1 (Đánh thường)", activeDef, mgr);
            UpdateSkillSlotText(slot2Text, SkillSlotType.Skill, "Slot 2 (Tuyệt kỹ)", activeDef, mgr);
            UpdateSkillSlotText(slot3Text, SkillSlotType.ExternalSkill1, "Slot 3 (Ngoại công 1)", activeDef, mgr);
            UpdateSkillSlotText(slot4Text, SkillSlotType.ExternalSkill2, "Slot 4 (Ngoại công 2)", activeDef, mgr);
            UpdateSkillSlotText(slot5Text, SkillSlotType.Ultimate, "Slot 5 (Bí kỹ / Thần công)", activeDef, mgr);

            // 4. Alternatives for Current Selected Slot
            if (alternativesHeaderText != null)
            {
                alternativesHeaderText.text = $"CÔNG PHÁP THAY THẾ CHO: <color=#FFD700>{GetSlotDisplayName(currentSelectedSlot)}</color>";
            }

            if (alternativesListText != null && activeDef != null)
            {
                var skillsInSlot = activeDef.GetSkillsForSlot(currentSelectedSlot);
                string selectedId = mgr.GetSelectedSkillIdForSlot(currentSelectedSlot);

                StringBuilder sb = new StringBuilder();
                if (skillsInSlot.Count == 0)
                {
                    sb.AppendLine("Không có công pháp cho vị trí này.");
                }
                else
                {
                    for (int i = 0; i < skillsInSlot.Count; i++)
                    {
                        var sk = skillsInSlot[i];
                        if (sk == null) continue;

                        bool isUnlocked = mgr.IsSkillUnlocked(sk.SkillId);
                        bool isSelected = (sk.SkillId == selectedId);

                        string stateBadge;
                        if (isSelected) stateBadge = "<color=#00FF66>[ĐANG TRANG BỊ]</color>";
                        else if (isUnlocked) stateBadge = "<color=#00FFFF>[ĐÃ MỞ - CÓ THỂ CHỌN]</color>";
                        else stateBadge = "<color=#FF4444>[CHƯA MỞ]</color>";

                        sb.AppendLine($"<b>{i + 1}. {sk.SkillName}</b> {stateBadge}");
                        sb.AppendLine($"   <i>{sk.Description}</i>");
                        sb.AppendLine($"   Hệ số ST: {sk.DamageMultiplier * 100f:F0}% | Tiêu hao Nộ: {sk.RageCost:F0} | Hồi chiêu: {sk.Cooldown:F1}s");

                        if (!isUnlocked && sk.UnlockConditions != null && sk.UnlockConditions.Count > 0)
                        {
                            sb.Append("   Yêu cầu: ");
                            for (int c = 0; c < sk.UnlockConditions.Count; c++)
                            {
                                var cond = sk.UnlockConditions[c];
                                sb.Append($"[{cond.Type}: {cond.RequiredValue}] ");
                            }
                            sb.AppendLine();
                        }
                        sb.AppendLine();
                    }
                }
                alternativesListText.text = sb.ToString();
            }
        }

        private void UpdateSkillSlotText(TextMeshProUGUI textComp, SkillSlotType slot, string slotTitle, MindMethodDefinitionSO activeDef, MindMethodManager mgr)
        {
            if (textComp == null) return;
            if (activeDef == null || mgr == null)
            {
                textComp.text = $"{slotTitle}: (Chưa có)";
                return;
            }

            var skill = mgr.GetSelectedSkillForSlot(slot);
            bool isCurrentSelected = (slot == currentSelectedSlot);
            string highlight = isCurrentSelected ? "<color=#FFD700>► </color>" : "";

            if (skill != null)
            {
                float cdRemain = WuxiaGame.Combat.CooldownManager.GetRemainingCooldown(skill.SkillId);
                string cdStr = cdRemain > 0f ? $"<color=#FF4444>[Hồi: {cdRemain:F1}s/{skill.Cooldown:F1}s]</color>" : "<color=#00FF66>[Sẵn sàng]</color>";
                string costStr = skill.RageCost > 0f ? $"<color=#FF9900>Nộ: {skill.RageCost:F0}</color>" : "<color=#888888>0 Nộ</color>";
                textComp.text = $"{highlight}<b>{slotTitle}</b>: <color=#00FFFF>{skill.SkillName}</color> ({costStr} | {cdStr})";
            }
            else
            {
                textComp.text = $"{highlight}<b>{slotTitle}</b>: <color=#888888>(Trống)</color>";
            }
        }

        private string GetSlotDisplayName(SkillSlotType slot)
        {
            switch (slot)
            {
                case SkillSlotType.NormalAttack: return "Slot 1 - Đánh thường";
                case SkillSlotType.Skill: return "Slot 2 - Tuyệt kỹ";
                case SkillSlotType.ExternalSkill1: return "Slot 3 - Ngoại công 1";
                case SkillSlotType.ExternalSkill2: return "Slot 4 - Ngoại công 2";
                case SkillSlotType.Ultimate: return "Slot 5 - Thần công / Bí kỹ";
                default: return slot.ToString();
            }
        }

        #region Event Handlers

        private void HandleActiveMindMethodChanged(MindMethodDefinitionSO newMM)
        {
            RefreshUI();
        }

        private void HandleMindMethodUnlocked(string id)
        {
            RefreshUI();
        }

        private void HandleMindMethodLevelChanged(string id, int lvl)
        {
            RefreshUI();
        }

        private void HandleSkillUnlocked(string id)
        {
            RefreshUI();
        }

        private void HandleSkillSelected(SkillSlotType slot, string id)
        {
            RefreshUI();
        }

        private void HandleProgressionChanged(int lvl)
        {
            RefreshUI();
        }

        private void HandleLootTierChanged(LootTierConfigSO tier)
        {
            RefreshUI();
        }

        private void HandleRageChanged(Entity entity, float cur, float max)
        {
            if (entity is Hero) RefreshUI();
        }

        private void HandleSkillExecution(WuxiaGame.Combat.SkillExecutionRequest req, WuxiaGame.Combat.SkillExecutionResult res)
        {
            RefreshUI();
        }

        #endregion
    }
}
