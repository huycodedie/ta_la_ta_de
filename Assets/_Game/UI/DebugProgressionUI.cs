using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.UI
{
    public class DebugProgressionUI : MonoBehaviour
    {
        [Header("Text Displays")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI levelUpNotificationText;

        [Header("EXP Bar Fill")]
        [SerializeField] private Image expFillImage;

        [Header("Debug Buttons")]
        [SerializeField] private Button add10ExpBtn;
        [SerializeField] private Button add100ExpBtn;
        [SerializeField] private Button add1000ExpBtn;
        [SerializeField] private Button breakthroughBtn;
        [SerializeField] private Button resetProgressionBtn;

        private Coroutine notificationCoroutine;

        public TextMeshProUGUI LevelText => levelText;
        public TextMeshProUGUI ExpText => expText;
        public Image ExpFillImage => expFillImage;
        public TextMeshProUGUI LevelUpNotificationText => levelUpNotificationText;
        public Button ResetProgressionBtn => resetProgressionBtn;

        private void Awake()
        {
            FindReferencesIfMissing();
        }

        private void Start()
        {
            FindReferencesIfMissing();
            BindButtons();

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.InitializeProgression();
            }
        }

        private void BindButtons()
        {
            if (add10ExpBtn != null)
            {
                add10ExpBtn.onClick.RemoveListener(OnAdd10ExpClicked);
                add10ExpBtn.onClick.AddListener(OnAdd10ExpClicked);
            }
            if (add100ExpBtn != null)
            {
                add100ExpBtn.onClick.RemoveListener(OnAdd100ExpClicked);
                add100ExpBtn.onClick.AddListener(OnAdd100ExpClicked);
            }
            if (add1000ExpBtn != null)
            {
                add1000ExpBtn.onClick.RemoveListener(OnAdd1000ExpClicked);
                add1000ExpBtn.onClick.AddListener(OnAdd1000ExpClicked);
            }
            if (breakthroughBtn != null)
            {
                breakthroughBtn.onClick.RemoveListener(OnBreakthroughClicked);
                breakthroughBtn.onClick.AddListener(OnBreakthroughClicked);
            }
            if (resetProgressionBtn != null)
            {
                resetProgressionBtn.onClick.RemoveListener(OnResetProgressionClicked);
                resetProgressionBtn.onClick.AddListener(OnResetProgressionClicked);
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
                if (add10ExpBtn == null && bName == "Add10ExpBtn") add10ExpBtn = b;
                else if (add100ExpBtn == null && bName == "Add100ExpBtn") add100ExpBtn = b;
                else if (add1000ExpBtn == null && bName == "Add1000ExpBtn") add1000ExpBtn = b;
                else if (breakthroughBtn == null && bName == "BreakthroughBtn") breakthroughBtn = b;
                else if (resetProgressionBtn == null && (bName == "ResetProgressionBtn" || bName == "ResetProgBtn" || bName == "ResetBtn")) resetProgressionBtn = b;
            }

            TextMeshProUGUI[] allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t == null) continue;
                string tName = t.gameObject.name;
                if (levelText == null && tName == "LevelText") levelText = t;
                else if (expText == null && tName == "ExpText") expText = t;
                else if (titleText == null && (tName == "RankTitleText" || tName == "TitleText")) titleText = t;
                else if (levelUpNotificationText == null && tName == "LevelUpNotification") levelUpNotificationText = t;
            }
        }

        private void OnEnable()
        {
            EventBus.OnExpChanged += HandleExpChanged;
            EventBus.OnLevelChanged += HandleLevelChanged;
            EventBus.OnHeroLevelUp += HandleHeroLevelUp;
            EventBus.OnTitleChanged += HandleTitleChanged;
            EventBus.OnBreakthroughStatusChanged += HandleBreakthroughStatusChanged;
        }

        private void OnDisable()
        {
            EventBus.OnExpChanged -= HandleExpChanged;
            EventBus.OnLevelChanged -= HandleLevelChanged;
            EventBus.OnHeroLevelUp -= HandleHeroLevelUp;
            EventBus.OnTitleChanged -= HandleTitleChanged;
            EventBus.OnBreakthroughStatusChanged -= HandleBreakthroughStatusChanged;
        }

        public void SetReferences(
            TextMeshProUGUI levelTmp,
            TextMeshProUGUI expTmp,
            TextMeshProUGUI titleTmp,
            Image fillImg,
            Button add10Btn,
            Button add100Btn,
            Button bkBtn,
            Button add1000Btn = null,
            TextMeshProUGUI notifTmp = null,
            Button resetBtn = null)
        {
            levelText = levelTmp;
            expText = expTmp;
            titleText = titleTmp;
            expFillImage = fillImg;
            add10ExpBtn = add10Btn;
            add100ExpBtn = add100Btn;
            breakthroughBtn = bkBtn;
            add1000ExpBtn = add1000Btn;
            levelUpNotificationText = notifTmp;
            resetProgressionBtn = resetBtn;

            BindButtons();
            OnEnable();
        }

        public void HandleExpChanged(float currentExp, float requiredExp)
        {
            int curLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;
            int cap = ProgressionManager.Instance != null ? ProgressionManager.Instance.GetEffectiveLevelCap() : 5;

            if (expText != null)
            {
                if (curLevel >= cap)
                {
                    expText.text = $"EXP: {currentExp:F0} / {requiredExp:F0} (CAP REACHED)";
                }
                else
                {
                    expText.text = $"EXP: {currentExp:F0} / {requiredExp:F0}";
                }
            }

            if (expFillImage != null && requiredExp > 0)
            {
                expFillImage.fillAmount = Mathf.Clamp01(currentExp / requiredExp);
            }
        }

        public void HandleLevelChanged(int newLevel)
        {
            if (levelText != null)
            {
                levelText.text = $"HERO LV. {newLevel}";
            }
        }

        public void HandleHeroLevelUp(int newLevel, int levelDelta, Dictionary<StatType, float> statDeltas)
        {
            if (levelUpNotificationText != null)
            {
                string msg = $"LEVEL UP! LV. {newLevel - levelDelta} -> LV. {newLevel}";
                levelUpNotificationText.text = msg;
                levelUpNotificationText.gameObject.SetActive(true);

                if (Application.isPlaying)
                {
                    if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
                    notificationCoroutine = StartCoroutine(HideNotificationAfterDelay(3.5f));
                }
            }
        }

        private IEnumerator HideNotificationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (levelUpNotificationText != null)
            {
                levelUpNotificationText.gameObject.SetActive(false);
            }
        }

        private void HandleTitleChanged(TitleConfigSO newTitle)
        {
            if (titleText != null)
            {
                if (TitleBreakthroughManager.Instance != null && TitleBreakthroughManager.Instance.Database != null)
                {
                    titleText.text = TitleBreakthroughManager.Instance.CurrentTitleName;
                }
                else
                {
                    titleText.text = newTitle != null ? newTitle.TitleName : "No Title";
                }
            }
        }

        private void HandleBreakthroughStatusChanged(bool canBreakthrough)
        {
            if (breakthroughBtn != null)
            {
                breakthroughBtn.interactable = true; // Main entry button must ALWAYS remain clickable to inspect requirements!
            }
            if (titleText != null && TitleBreakthroughManager.Instance != null && TitleBreakthroughManager.Instance.Database != null)
            {
                titleText.text = TitleBreakthroughManager.Instance.CurrentTitleName;
            }
        }

        public void OnAdd10ExpClicked()
        {
            ProgressionManager pm = ProgressionManager.Instance != null ? ProgressionManager.Instance : Object.FindAnyObjectByType<ProgressionManager>();
            if (pm != null)
            {
                pm.AddExp(10f);
                Debug.Log("[DEBUG UI] Button action executed: +10 EXP");
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: +10 EXP (ProgressionManager null)");
            }
        }

        public void OnAdd100ExpClicked()
        {
            ProgressionManager pm = ProgressionManager.Instance != null ? ProgressionManager.Instance : Object.FindAnyObjectByType<ProgressionManager>();
            if (pm != null)
            {
                pm.AddExp(100f);
                Debug.Log("[DEBUG UI] Button action executed: +100 EXP");
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: +100 EXP (ProgressionManager null)");
            }
        }

        public void OnAdd1000ExpClicked()
        {
            ProgressionManager pm = ProgressionManager.Instance != null ? ProgressionManager.Instance : Object.FindAnyObjectByType<ProgressionManager>();
            if (pm != null)
            {
                pm.AddExp(1000f);
                Debug.Log("[DEBUG UI] Button action executed: +1K EXP");
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: +1K EXP (ProgressionManager null)");
            }
        }

        public void OnBreakthroughClicked()
        {
            if (TitleBreakthroughUI.Instance != null && TitleBreakthroughUI.Instance.Panel != null)
            {
                TitleBreakthroughUI.Instance.ShowPanel();
                Debug.Log("[DEBUG UI] Button action executed: OPEN BREAKTHROUGH PANEL");
                return;
            }

            ProgressionManager pm = ProgressionManager.Instance != null ? ProgressionManager.Instance : Object.FindAnyObjectByType<ProgressionManager>();
            if (pm != null)
            {
                bool success = pm.TryBreakthrough();
                if (success)
                {
                    Debug.Log("[DEBUG UI] Button action executed: BREAKTHROUGH");
                }
                else
                {
                    Debug.LogWarning("[DEBUG UI] Button action failed: BREAKTHROUGH (Requirements not met)");
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG UI] Button action failed: BREAKTHROUGH (ProgressionManager null)");
            }
        }

        public void OnResetProgressionClicked()
        {
            if (TitleBreakthroughManager.Instance != null)
            {
                TitleBreakthroughManager.Instance.ResetProgression();
            }
            else if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.ResetAllProgression();
            }
            Debug.Log("[DEBUG UI] Button action executed: RESET PROGRESSION");
        }
    }
}
