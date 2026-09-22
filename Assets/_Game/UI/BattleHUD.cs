using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.UI
{
    public class BattleHUD : MonoBehaviour
    {
        [Header("Overlay Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button startButton;

        [Header("Status Text")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI defeatText;

        [Header("Shield UI (P07.8 Prototype / Debug)")]
        [SerializeField] private GameObject heroShieldRoot;
        [SerializeField] private Image heroShieldFill;
        [SerializeField] private TextMeshProUGUI heroShieldText;

        [SerializeField] private GameObject monsterShieldRoot;
        [SerializeField] private Image monsterShieldFill;
        [SerializeField] private TextMeshProUGUI monsterShieldText;

        [Header("Cast Bar UI (P07.9 Presentation)")]
        [SerializeField] private CastBarUI heroCastBarUI;
        [SerializeField] private GameObject heroCastBarRoot;
        [SerializeField] private Image heroCastBarFill;
        [SerializeField] private TextMeshProUGUI heroCastBarText;
        [SerializeField] private TextMeshProUGUI heroInterruptText;

        public GameObject DefeatPanel => defeatPanel;
        public Button StartButton => startButton != null ? startButton : restartButton;

        public GameObject HeroShieldRoot => heroShieldRoot;
        public Image HeroShieldFill => heroShieldFill;
        public TextMeshProUGUI HeroShieldText => heroShieldText;
        public GameObject MonsterShieldRoot => monsterShieldRoot;
        public Image MonsterShieldFill => monsterShieldFill;
        public TextMeshProUGUI MonsterShieldText => monsterShieldText;

        public CastBarUI HeroCastBarUI => heroCastBarUI;
        public GameObject HeroCastBarRoot => heroCastBarRoot;
        public Image HeroCastBarFill => heroCastBarFill;
        public TextMeshProUGUI HeroCastBarText => heroCastBarText;
        public TextMeshProUGUI HeroInterruptText => heroInterruptText;

        public bool IsHeroCastBarVisible => heroCastBarUI != null ? heroCastBarUI.IsVisible : (heroCastBarRoot != null && heroCastBarRoot.activeSelf);
        public float CurrentHeroCastProgress => heroCastBarUI != null ? heroCastBarUI.CurrentFill : (heroCastBarFill != null ? heroCastBarFill.fillAmount : 0f);
        public string CurrentHeroCastText => heroCastBarUI != null ? heroCastBarUI.CurrentText : (heroCastBarText != null ? heroCastBarText.text : string.Empty);
        public string CurrentHeroInterruptText => heroCastBarUI != null ? heroCastBarUI.CurrentInterruptText : (heroInterruptText != null ? heroInterruptText.text : string.Empty);
        public bool IsHeroShowingInterruptFeedback => heroCastBarUI != null ? heroCastBarUI.IsShowingInterruptFeedback : (!string.IsNullOrEmpty(CurrentHeroInterruptText) && heroInterruptText != null && heroInterruptText.gameObject.activeSelf);

        public bool IsHeroShieldVisible => heroShieldRoot != null && heroShieldRoot.activeSelf;
        public float CurrentDisplayedHeroShield => GetDisplayedShieldAmount(heroShieldText);
        public float CurrentHeroShieldFill => heroShieldFill != null ? heroShieldFill.fillAmount : 0f;

        public bool IsMonsterShieldVisible => monsterShieldRoot != null && monsterShieldRoot.activeSelf;
        public float CurrentDisplayedMonsterShield => GetDisplayedShieldAmount(monsterShieldText);
        public float CurrentMonsterShieldFill => monsterShieldFill != null ? monsterShieldFill.fillAmount : 0f;

        private void Start()
        {
            HideOverlays();
            UpdateAllShields();

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnStartClicked);
            }

            if (startButton != null && startButton != restartButton)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        public void RegisterEvents()
        {
            UnregisterEvents();
            EventBus.OnBattleStateChanged += HandleBattleStateChanged;
            EventBus.OnShieldApplied += HandleShieldApplied;
            EventBus.OnShieldAbsorbed += HandleShieldAbsorbed;
            EventBus.OnShieldDepleted += HandleShieldDepleted;
            EventBus.OnShieldExpired += HandleShieldExpired;
            EventBus.OnShieldRemoved += HandleShieldRemoved;
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnEntityDied += HandleEntityDied;
        }

        public void UnregisterEvents()
        {
            EventBus.OnBattleStateChanged -= HandleBattleStateChanged;
            EventBus.OnShieldApplied -= HandleShieldApplied;
            EventBus.OnShieldAbsorbed -= HandleShieldAbsorbed;
            EventBus.OnShieldDepleted -= HandleShieldDepleted;
            EventBus.OnShieldExpired -= HandleShieldExpired;
            EventBus.OnShieldRemoved -= HandleShieldRemoved;
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        public void SetShieldReferences(
            GameObject heroRoot, Image heroFill, TextMeshProUGUI heroTxt,
            GameObject monsterRoot = null, Image monsterFill = null, TextMeshProUGUI monsterTxt = null)
        {
            heroShieldRoot = heroRoot;
            heroShieldFill = heroFill;
            heroShieldText = heroTxt;
            monsterShieldRoot = monsterRoot;
            monsterShieldFill = monsterFill;
            monsterShieldText = monsterTxt;

            RegisterEvents();
            UpdateAllShields();
        }

        public void SetCastBarReferences(
            CastBarUI castUI,
            GameObject root = null,
            Image fill = null,
            TextMeshProUGUI cText = null,
            TextMeshProUGUI iText = null)
        {
            heroCastBarUI = castUI;
            heroCastBarRoot = root != null ? root : (castUI != null ? castUI.RootObject : null);
            heroCastBarFill = fill != null ? fill : (castUI != null ? castUI.FillImage : null);
            heroCastBarText = cText != null ? cText : (castUI != null ? castUI.CastText : null);
            heroInterruptText = iText != null ? iText : (castUI != null ? castUI.InterruptText : null);
        }

        private void HandleShieldApplied(Entity entity, string shieldId, float cur, float max) => UpdateShieldDisplay(entity);
        private void HandleShieldAbsorbed(Entity entity, ShieldAbsorbResult result) => UpdateShieldDisplay(entity);
        private void HandleShieldDepleted(Entity entity, string shieldId) => UpdateShieldDisplay(entity);
        private void HandleShieldExpired(Entity entity, string shieldId) => UpdateShieldDisplay(entity);
        private void HandleShieldRemoved(Entity entity, string shieldId) => UpdateShieldDisplay(entity);
        private void HandleEntitySpawned(Entity entity) => UpdateShieldDisplay(entity);
        private void HandleEntityDied(Entity entity) => UpdateShieldDisplay(entity);

        public void UpdateShieldDisplay(Entity entity = null)
        {
            if (entity == null)
            {
                UpdateAllShields();
                return;
            }

            if (entity.EntityType == EntityType.Hero)
            {
                UpdateEntityShieldUI(entity, heroShieldRoot, heroShieldFill, heroShieldText);
            }
            else if (entity.EntityType == EntityType.Monster)
            {
                UpdateEntityShieldUI(entity, monsterShieldRoot, monsterShieldFill, monsterShieldText);
            }
        }

        public void UpdateAllShields()
        {
            Entity[] entities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude);
            if (entities != null)
            {
                foreach (var e in entities)
                {
                    if (e != null) UpdateShieldDisplay(e);
                }
            }
        }

        private void UpdateEntityShieldUI(Entity entity, GameObject root, Image fill, TextMeshProUGUI text)
        {
            if (root == null && fill == null && text == null) return;

            if (entity == null || !entity.IsAlive || entity.StatusController == null)
            {
                if (root != null) root.SetActive(false);
                if (fill != null) fill.fillAmount = 0f;
                if (text != null) text.text = string.Empty;
                return;
            }

            float totalShield = entity.StatusController.TotalShieldAmount;
            bool hasActiveShield = entity.StatusController.HasActiveShield && totalShield > 0.001f;

            if (!hasActiveShield)
            {
                if (root != null) root.SetActive(false);
                if (fill != null) fill.fillAmount = 0f;
                if (text != null) text.text = string.Empty;
            }
            else
            {
                if (root != null) root.SetActive(true);
                if (text != null)
                {
                    text.text = $"SHIELD: {totalShield:F0}";
                }
                if (fill != null)
                {
                    float maxRef = 0f;
                    if (entity.StatusController.ActiveShields != null)
                    {
                        foreach (var s in entity.StatusController.ActiveShields.Values)
                        {
                            if (s != null) maxRef += s.MaxAmount;
                        }
                    }
                    if (maxRef <= 0f && entity.Health != null)
                    {
                        maxRef = entity.Health.MaxHealth;
                    }
                    fill.fillAmount = maxRef > 0f ? Mathf.Clamp01(totalShield / maxRef) : 1f;
                }
            }
        }

        public float GetDisplayedShieldAmount(TextMeshProUGUI text)
        {
            if (text == null || string.IsNullOrEmpty(text.text)) return 0f;
            string raw = text.text.Replace("SHIELD:", "").Trim();
            if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float val))
            {
                return val;
            }
            if (float.TryParse(raw, out float val2))
            {
                return val2;
            }
            return 0f;
        }

        public void SetReferences(GameObject vicPanel, GameObject defPanel, Button startBtn, TextMeshProUGUI statusTmp, TextMeshProUGUI defTmp = null)
        {
            victoryPanel = vicPanel;
            defeatPanel = defPanel;
            startButton = startBtn;
            restartButton = startBtn;
            statusText = statusTmp;
            defeatText = defTmp;

            EventBus.OnBattleStateChanged -= HandleBattleStateChanged;
            EventBus.OnBattleStateChanged += HandleBattleStateChanged;

            if (startBtn != null)
            {
                startBtn.onClick.RemoveAllListeners();
                startBtn.onClick.AddListener(OnStartClicked);
            }
        }

        public void HideOverlays()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(false);
            if (startButton != null) startButton.gameObject.SetActive(false);
        }

        public void HandleBattleStateChanged(BattleState newState)
        {
            HideOverlays();

            switch (newState)
            {
                case BattleState.Victory:
                    if (statusText != null) statusText.text = "MONSTER DEFEATED!";
                    break;
                case BattleState.Defeat:
                case BattleState.HeroDead:
                case BattleState.AwaitingPlayerStart:
                    if (defeatPanel != null) defeatPanel.SetActive(true);
                    if (defeatText != null) defeatText.text = "ANH HÙNG ĐÃ GỤC";
                    if (startButton != null) startButton.gameObject.SetActive(true);
                    else if (restartButton != null) restartButton.gameObject.SetActive(true);
                    if (statusText != null) statusText.text = "ANH HÙNG ĐÃ GỤC";
                    Debug.Log("[DEATH UI] Hero death panel shown");
                    break;
                case BattleState.InProgress:
                    if (statusText != null) statusText.text = "PROTOTYPE 05";
                    break;
            }
        }

        public void OnStartClicked()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.StartCombatAfterHeroDeath();
            }
            else
            {
                BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
                if (bm != null)
                {
                    bm.StartCombatAfterHeroDeath();
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartBattle();
                }
            }
        }
    }
}
