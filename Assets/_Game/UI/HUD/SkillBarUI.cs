using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Progression;
using WuxiaGame.UI.Core;

namespace WuxiaGame.UI.HUD
{
    /// <summary>
    /// Compact mobile Wuxia action bar containing 5 skill slots, Auto toggle, and Speed toggle.
    /// Pure presentation layer: executes skills via Hero.ExecuteSelectedSkill(slot)
    /// and reads cooldowns from CooldownManager.
    /// </summary>
    public class SkillBarUI : MonoBehaviour
    {
        [Header("Hero Binding")]
        [SerializeField] private Hero boundHero;

        [Header("Skill Buttons")]
        [SerializeField] private Button normalAttackBtn;
        [SerializeField] private Button skillBtn;
        [SerializeField] private Button externalSkill1Btn;
        [SerializeField] private Button externalSkill2Btn;
        [SerializeField] private Button ultimateBtn;

        [Header("Toggles")]
        [SerializeField] private Button autoToggleBtn;
        [SerializeField] private TextMeshProUGUI autoToggleText;
        [SerializeField] private Button speedToggleBtn;
        [SerializeField] private TextMeshProUGUI speedToggleText;

        public class SkillSlotView
        {
            public SkillSlotType SlotType;
            public Button Button;
            public Image Background;
            public Image Border;
            public Image CooldownFill;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI CooldownText;
            public GameObject ReadyPulse;
        }

        private readonly Dictionary<SkillSlotType, SkillSlotView> slotViews = new Dictionary<SkillSlotType, SkillSlotView>();
        private bool isAutoActive = false;
        private bool isDoubleSpeed = false;
        private float pulseTimer = 0f;

        public bool IsAutoActive => isAutoActive;
        public bool IsDoubleSpeed => isDoubleSpeed;
        public Hero BoundHero => boundHero;

        private void Start()
        {
            if (boundHero == null)
            {
                FindAndBindHero();
            }
            if (BattleManager.Instance != null)
            {
                isAutoActive = BattleManager.Instance.IsAutoBattle;
            }
            InitializeButtons();
            UpdateTogglesDisplay();
        }

        private void OnEnable()
        {
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnSkillSelected += HandleSkillSelected;
            EventBus.OnActiveMindMethodChanged += HandleMindMethodChanged;
            EventBus.OnRageChanged += HandleRageChanged;
            EventBus.OnAutoBattleChanged += HandleAutoBattleChanged;
        }

        private void OnDisable()
        {
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnSkillSelected -= HandleSkillSelected;
            EventBus.OnActiveMindMethodChanged -= HandleMindMethodChanged;
            EventBus.OnRageChanged -= HandleRageChanged;
            EventBus.OnAutoBattleChanged -= HandleAutoBattleChanged;
        }

        private void OnDestroy()
        {
        }

        public void BindHero(Hero hero)
        {
            boundHero = hero;
            UpdateAllSlots();
        }

        public void FindAndBindHero()
        {
            Hero h = UnityEngine.Object.FindAnyObjectByType<Hero>();
            if (h != null)
            {
                BindHero(h);
            }
        }

        private void HandleEntitySpawned(Entity entity)
        {
            if (entity is Hero hero)
            {
                BindHero(hero);
            }
        }

        private void HandleSkillSelected(SkillSlotType slot, string skillId) => UpdateSlot(slot);
        private void HandleMindMethodChanged(MindMethodDefinitionSO mm) => UpdateAllSlots();
        private void HandleRageChanged(Entity entity, float cur, float max)
        {
            if (entity == boundHero)
            {
                UpdateSlot(SkillSlotType.Ultimate);
            }
        }

        public void RegisterSlot(SkillSlotType slot, Button btn, Image bg, Image border, Image cdFill, TextMeshProUGUI nameTxt, TextMeshProUGUI cdTxt, GameObject pulse = null)
        {
            var view = new SkillSlotView
            {
                SlotType = slot,
                Button = btn,
                Background = bg,
                Border = border,
                CooldownFill = cdFill,
                NameText = nameTxt,
                CooldownText = cdTxt,
                ReadyPulse = pulse
            };

            slotViews[slot] = view;

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSkillClicked(slot));
            }
        }

        public void SetToggleButtons(Button autoBtn, TextMeshProUGUI autoTxt, Button spdBtn, TextMeshProUGUI spdTxt)
        {
            autoToggleBtn = autoBtn;
            autoToggleText = autoTxt;
            speedToggleBtn = spdBtn;
            speedToggleText = spdTxt;

            if (autoToggleBtn != null)
            {
                autoToggleBtn.onClick.RemoveAllListeners();
                autoToggleBtn.onClick.AddListener(ToggleAuto);
            }

            if (speedToggleBtn != null)
            {
                speedToggleBtn.onClick.RemoveAllListeners();
                speedToggleBtn.onClick.AddListener(ToggleSpeed);
            }

            UpdateTogglesDisplay();
        }

        private void InitializeButtons()
        {
            if (autoToggleBtn != null)
            {
                autoToggleBtn.onClick.RemoveAllListeners();
                autoToggleBtn.onClick.AddListener(ToggleAuto);
            }

            if (speedToggleBtn != null)
            {
                speedToggleBtn.onClick.RemoveAllListeners();
                speedToggleBtn.onClick.AddListener(ToggleSpeed);
            }
        }

        public void ToggleAuto()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.SetAutoBattle(!BattleManager.Instance.IsAutoBattle);
            }
            else
            {
                isAutoActive = !isAutoActive;
                UpdateTogglesDisplay();
            }
        }

        private void HandleAutoBattleChanged(bool enabled)
        {
            isAutoActive = enabled;
            UpdateTogglesDisplay();
        }

        public void ToggleSpeed()
        {
            // GATE B: Speed button does NOT own Time.timeScale authority.
            // When an authoritative SpeedController exists, delegate here.
            isDoubleSpeed = !isDoubleSpeed;
            UpdateTogglesDisplay();
        }

        private void UpdateTogglesDisplay()
        {
            if (autoToggleText != null)
            {
                autoToggleText.text = isAutoActive ? "AUTO: BẬT" : "AUTO: TẮT";
                autoToggleText.color = isAutoActive ? UIStyleConfig.TextActive : UIStyleConfig.TextMuted;
            }

            if (speedToggleText != null)
            {
                speedToggleText.text = isDoubleSpeed ? "2X" : "1X";
                speedToggleText.color = isDoubleSpeed ? UIStyleConfig.CyanHighlight : UIStyleConfig.TextPrimary;
            }
        }

        private void OnSkillClicked(SkillSlotType slot)
        {
            if (boundHero == null || !boundHero.IsAlive) return;

            // GATE A: Pure delegation to authoritative Hero API
            boundHero.ExecuteSelectedSkill(slot);
        }

        private void Update()
        {
            pulseTimer += Time.deltaTime * 3.5f;

            if (boundHero == null)
            {
                FindAndBindHero();
            }

            UpdateAllSlots();
        }

        public void UpdateAllSlots()
        {
            foreach (var kvp in slotViews)
            {
                UpdateSlot(kvp.Key);
            }
        }

        public void UpdateSlot(SkillSlotType slot)
        {
            if (!slotViews.TryGetValue(slot, out SkillSlotView view) || view == null) return;

            var mmMgr = MindMethodManager.Instance;
            SkillDefinitionSO skill = mmMgr != null ? mmMgr.GetSelectedSkillForSlot(slot) : null;

            bool isHeroReady = boundHero != null && boundHero.IsAlive;
            bool canAction = (slot == SkillSlotType.Ultimate) ? (isHeroReady && boundHero.CanUseUltimate) : (isHeroReady && boundHero.CanUseSkill);

            if (skill == null)
            {
                // Slot has no skill selected or unlocked
                if (view.NameText != null) view.NameText.text = GetSlotFallbackName(slot);
                if (view.CooldownFill != null) view.CooldownFill.fillAmount = 0f;
                if (view.CooldownText != null) view.CooldownText.text = string.Empty;
                if (view.ReadyPulse != null) view.ReadyPulse.SetActive(false);
                if (view.Button != null) view.Button.interactable = (slot == SkillSlotType.NormalAttack && canAction);
                return;
            }

            // Slot has an authoritative skill
            if (view.NameText != null)
            {
                view.NameText.text = skill.SkillName;
            }

            bool onCd = CooldownManager.IsOnCooldown(skill.SkillId, out float remaining);
            float totalCd = CooldownManager.GetCooldownDuration(skill.SkillId);
            if (totalCd <= 0f) totalCd = skill.Cooldown;

            if (onCd && remaining > 0.05f)
            {
                if (view.CooldownFill != null)
                {
                    view.CooldownFill.gameObject.SetActive(true);
                    view.CooldownFill.fillAmount = totalCd > 0f ? Mathf.Clamp01(remaining / totalCd) : 0f;
                }
                if (view.CooldownText != null)
                {
                    view.CooldownText.gameObject.SetActive(true);
                    view.CooldownText.text = $"{remaining:F1}s";
                }
                if (view.ReadyPulse != null) view.ReadyPulse.SetActive(false);
                if (view.Button != null) view.Button.interactable = false;
            }
            else
            {
                if (view.CooldownFill != null)
                {
                    view.CooldownFill.fillAmount = 0f;
                    view.CooldownFill.gameObject.SetActive(false);
                }
                if (view.CooldownText != null)
                {
                    view.CooldownText.text = string.Empty;
                    view.CooldownText.gameObject.SetActive(false);
                }

                // Check Ultimate Rage Availability
                bool hasResource = true;
                if (slot == SkillSlotType.Ultimate)
                {
                    float cost = skill.RageCost > 0f ? skill.RageCost : 100f;
                    hasResource = (boundHero != null && boundHero.Rage != null && boundHero.Rage.CurrentRage >= cost);
                }

                bool isAvailable = canAction && hasResource;
                if (view.Button != null) view.Button.interactable = isAvailable;

                // Visual excitation on ready Ultimate
                if (slot == SkillSlotType.Ultimate && isAvailable)
                {
                    if (view.ReadyPulse != null)
                    {
                        view.ReadyPulse.SetActive(true);
                        float scale = 1f + 0.08f * Mathf.Sin(pulseTimer);
                        view.ReadyPulse.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                    if (view.Border != null)
                    {
                        view.Border.color = Color.Lerp(UIStyleConfig.BorderGold, UIStyleConfig.GoldAccent, 0.5f + 0.5f * Mathf.Sin(pulseTimer));
                    }
                }
                else
                {
                    if (view.ReadyPulse != null) view.ReadyPulse.SetActive(false);
                    if (view.Border != null) view.Border.color = UIStyleConfig.BorderBronze;
                }
            }
        }

        private string GetSlotFallbackName(SkillSlotType slot)
        {
            switch (slot)
            {
                case SkillSlotType.NormalAttack: return "Đánh Thường";
                case SkillSlotType.Skill: return "Tuyệt Kỹ";
                case SkillSlotType.ExternalSkill1: return "Ngoại Công 1";
                case SkillSlotType.ExternalSkill2: return "Ngoại Công 2";
                case SkillSlotType.Ultimate: return "Thần Công";
                default: return "Kỹ Năng";
            }
        }
    }
}
