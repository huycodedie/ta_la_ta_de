using System;
using UnityEngine;
using TMPro;
using WuxiaGame.Progression;

namespace WuxiaGame.UI.HUD
{
    /// <summary>
    /// Presentation-only HUD component binding the top-right gold display
    /// to the authoritative ResourceManager.
    /// Strictly read-only; never mutates currency; no polling or coroutines.
    /// Uses an idempotent TryBindAndRefresh() called from OnEnable and Start.
    /// </summary>
    public class HeaderGoldHUDUI : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private TextMeshProUGUI goldText;

        // Track exact subscribed instance to ensure symmetric unsubscribe
        private ResourceManager subscribedManager;
        private bool isSubscribed = false;

        public TextMeshProUGUI GoldText => goldText;
        public bool IsSubscribed => isSubscribed;
        public ResourceManager SubscribedManager => subscribedManager;

        public void SetTargetText(TextMeshProUGUI tmp)
        {
            goldText = tmp;
            if (isSubscribed && subscribedManager != null)
            {
                UpdateDisplay(subscribedManager.Gold);
            }
        }

        private void Awake()
        {
            if (goldText == null)
            {
                goldText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            TryBindAndRefresh();
        }

        private void Start()
        {
            TryBindAndRefresh();

            if (subscribedManager == null)
            {
                Debug.LogError("[HeaderGoldHUDUI] Initialization Error: Authoritative ResourceManager.Instance is absent at Start(). Cannot bind live currency!");
            }
        }

        private void OnDisable()
        {
            Unbind();
        }

        /// <summary>
        /// Idempotent subscription and refresh from authoritative ResourceManager.
        /// Subscribes at most once, stores exact instance, and refreshes immediately.
        /// </summary>
        public void TryBindAndRefresh()
        {
            ResourceManager manager = ResourceManager.Instance;
            if (manager == null)
            {
                manager = UnityEngine.Object.FindAnyObjectByType<ResourceManager>();
            }

            if (manager != null)
            {
                if (!isSubscribed || subscribedManager != manager)
                {
                    Unbind();
                    manager.OnResourcesChanged += HandleResourcesChanged;
                    subscribedManager = manager;
                    isSubscribed = true;
                }

                // Refresh immediately after binding
                UpdateDisplay(manager.Gold);
            }
        }

        public void RefreshDisplay()
        {
            TryBindAndRefresh();
        }

        public void Unbind()
        {
            if (isSubscribed && subscribedManager != null)
            {
                subscribedManager.OnResourcesChanged -= HandleResourcesChanged;
            }
            subscribedManager = null;
            isSubscribed = false;
        }

        private void HandleResourcesChanged(int currentGold, int currentMaterial)
        {
            UpdateDisplay(currentGold);
        }

        public void UpdateDisplay(int currentGold)
        {
            if (goldText != null)
            {
                goldText.text = FormatGold(currentGold);
            }
        }

        public static string FormatGold(int gold)
        {
            return gold.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " G";
        }
    }
}
