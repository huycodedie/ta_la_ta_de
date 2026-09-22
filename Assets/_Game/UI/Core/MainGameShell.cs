using System;
using UnityEngine;

namespace WuxiaGame.UI.Core
{
    /// <summary>
    /// Root game shell coordinating the Safe Area, Main Content Area, and Global Bottom Navigation.
    /// Acts as the single top-level container for all player-facing portrait screens.
    /// Pure presentation layer; owns no gameplay logic.
    /// </summary>
    public class MainGameShell : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private SafeArea safeArea;
        [SerializeField] private RectTransform mainContentArea;
        [SerializeField] private GlobalBottomNavigation navigation;

        public SafeArea SafeArea => safeArea;
        public RectTransform MainContentArea => mainContentArea;
        public GlobalBottomNavigation Navigation => navigation;

        public event Action<int> OnSystemScreenRequested;

        private void Awake()
        {
            if (safeArea == null) safeArea = GetComponentInChildren<SafeArea>(true);
            if (navigation == null) navigation = GetComponentInChildren<GlobalBottomNavigation>(true);
        }

        private void OnEnable()
        {
            if (navigation != null)
            {
                navigation.OnNavigationSelected += HandleNavigationSelected;
            }
        }

        private void OnDisable()
        {
            if (navigation != null)
            {
                navigation.OnNavigationSelected -= HandleNavigationSelected;
            }
        }

        public void SetReferences(SafeArea sa, RectTransform contentArea, GlobalBottomNavigation nav)
        {
            safeArea = sa;
            mainContentArea = contentArea;
            navigation = nav;

            if (navigation != null)
            {
                navigation.OnNavigationSelected -= HandleNavigationSelected;
                navigation.OnNavigationSelected += HandleNavigationSelected;
            }
        }

        private void HandleNavigationSelected(int index)
        {
            // Position 2 is Main Hub (Center)
            OnSystemScreenRequested?.Invoke(index);
        }
    }
}
