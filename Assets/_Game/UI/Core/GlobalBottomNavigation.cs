using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WuxiaGame.UI;

namespace WuxiaGame.UI.Core
{
    /// <summary>
    /// Global Bottom Navigation Bar (5 positions).
    /// Center position (index 2) is the Main Hub with enhanced visual prominence.
    /// Strictly presentation layer; owns no gameplay logic.
    /// </summary>
    public class GlobalBottomNavigation : MonoBehaviour
    {
        public const int SlotCount = 5;
        public const int MainHubIndex = 2; // Center position (0-indexed)

        [Header("Slots (Exactly 5 Positions)")]
        [SerializeField] private Button[] navButtons = new Button[SlotCount];
        [SerializeField] private Image[] navIcons = new Image[SlotCount];
        [SerializeField] private TextMeshProUGUI[] navLabels = new TextMeshProUGUI[SlotCount];
        [SerializeField] private GameObject[] navActiveIndicators = new GameObject[SlotCount];

        [Header("Content Routing Containers")]
        [SerializeField] private GameObject equipViewContainer;
        [SerializeField] private GameObject congPhapContainer;

        [Header("State")]
        [SerializeField] private int currentSelectedIndex = MainHubIndex;

        private static GlobalBottomNavigation _instance;
        public static GlobalBottomNavigation Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<GlobalBottomNavigation>();
                return _instance;
            }
            private set => _instance = value;
        }

        public event Action<int> OnNavigationSelected;

        public int CurrentSelectedIndex => currentSelectedIndex;
        public Button[] NavButtons => navButtons;
        public Image[] NavIcons => navIcons;
        public TextMeshProUGUI[] NavLabels => navLabels;
        public GameObject[] NavActiveIndicators => navActiveIndicators;

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
            InitializeButtons();
            UpdateVisualState();
            RouteContent(currentSelectedIndex);
        }

        public void SetRoutingContainers(GameObject equipView, GameObject congPhap)
        {
            equipViewContainer = equipView;
            congPhapContainer = congPhap;
        }

        public void SetReferences(
            Button[] buttons,
            Image[] icons = null,
            TextMeshProUGUI[] labels = null,
            GameObject[] activeIndicators = null)
        {
            if (buttons != null && buttons.Length == SlotCount)
            {
                navButtons = buttons;
            }
            if (icons != null && icons.Length == SlotCount)
            {
                navIcons = icons;
            }
            if (labels != null && labels.Length == SlotCount)
            {
                navLabels = labels;
            }
            if (activeIndicators != null && activeIndicators.Length == SlotCount)
            {
                navActiveIndicators = activeIndicators;
            }

            InitializeButtons();
            UpdateVisualState();
        }

        private void InitializeButtons()
        {
            if (navButtons == null) return;

            for (int i = 0; i < navButtons.Length; i++)
            {
                if (navButtons[i] != null)
                {
                    int index = i;
                    navButtons[i].onClick.RemoveAllListeners();
                    navButtons[i].onClick.AddListener(() => OnButtonClicked(index));
                }
            }
        }

        private void OnButtonClicked(int index)
        {
            SelectPosition(index);
        }

        public void SelectPosition(int index)
        {
            if (index < 0 || index >= SlotCount) return;

            currentSelectedIndex = index;
            UpdateVisualState();
            RouteContent(index);
            OnNavigationSelected?.Invoke(index);
        }

        public void RouteContent(int index)
        {
            if (congPhapContainer != null)
            {
                if (index == 1)
                {
                    congPhapContainer.transform.SetAsLastSibling();
                    congPhapContainer.SetActive(true);
                }
                else
                {
                    congPhapContainer.SetActive(false);
                }
            }
            if (equipViewContainer != null)
            {
                equipViewContainer.SetActive(index == 0);
            }

            var mmUI = MindMethodUI.Instance != null ? MindMethodUI.Instance : UnityEngine.Object.FindAnyObjectByType<MindMethodUI>();
            if (mmUI != null)
            {
                if (index == 1)
                {
                    mmUI.ShowPanel();
                }
                else if (mmUI.Panel != null && mmUI.Panel.activeSelf)
                {
                    mmUI.Panel.SetActive(false);
                }
            }
        }

        public void UpdateVisualState()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                bool isSelected = (i == currentSelectedIndex);

                if (navActiveIndicators != null && i < navActiveIndicators.Length && navActiveIndicators[i] != null)
                {
                    navActiveIndicators[i].SetActive(isSelected);
                }

                if (navLabels != null && i < navLabels.Length && navLabels[i] != null)
                {
                    if (isSelected)
                    {
                        navLabels[i].color = (i == MainHubIndex) ? UIStyleConfig.GoldAccent : UIStyleConfig.TextActive;
                        navLabels[i].fontStyle = FontStyles.Bold;
                    }
                    else
                    {
                        navLabels[i].color = UIStyleConfig.TextMuted;
                        navLabels[i].fontStyle = FontStyles.Normal;
                    }
                }

                if (navIcons != null && i < navIcons.Length && navIcons[i] != null)
                {
                    navIcons[i].color = isSelected ? Color.white : UIStyleConfig.IconMuted;
                }

                if (navButtons != null && i < navButtons.Length && navButtons[i] != null)
                {
                    Transform t = navButtons[i].transform;
                    float targetScale = (i == MainHubIndex) ? (isSelected ? 1.12f : 1.05f) : (isSelected ? 1.05f : 1.0f);
                    t.localScale = Vector3.one * targetScale;
                }
            }
        }
    }
}
