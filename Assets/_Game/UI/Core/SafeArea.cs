using UnityEngine;

namespace WuxiaGame.UI.Core
{
    /// <summary>
    /// Adjusts the RectTransform anchors to conform to the hardware safe area (notches, cutouts, system bars).
    /// Portrait-first foundation for mobile devices.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool conformX = true;
        [SerializeField] private bool conformY = true;

        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        public Rect CurrentSafeArea => Screen.safeArea;
        public bool ConformX => conformX;
        public bool ConformY => conformY;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            ScreenOrientation orientation = Screen.orientation;

            if (safeArea != lastSafeArea || screenSize != lastScreenSize || orientation != lastOrientation)
            {
                lastSafeArea = safeArea;
                lastScreenSize = screenSize;
                lastOrientation = orientation;
                ApplySafeArea(safeArea);
            }
        }

        public void ApplySafeArea(Rect r)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;

            if (conformX)
            {
                anchorMin.x /= Screen.width;
                anchorMax.x /= Screen.width;
            }
            else
            {
                anchorMin.x = 0f;
                anchorMax.x = 1f;
            }

            if (conformY)
            {
                anchorMin.y /= Screen.height;
                anchorMax.y /= Screen.height;
            }
            else
            {
                anchorMin.y = 0f;
                anchorMax.y = 1f;
            }

            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
