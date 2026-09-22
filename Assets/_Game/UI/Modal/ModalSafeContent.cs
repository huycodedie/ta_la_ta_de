using UnityEngine;

namespace WuxiaGame.UI.Modal
{
    /// <summary>
    /// Constrains modal content inside the runtime hardware safe area (notches, home indicators).
    /// Enforces responsive modal bounds across compact phones, standard portrait, tall displays, and tablets:
    /// - Phone width: min(92% of Safe Area width, 840 reference units).
    /// - Tablet width: maximum 840 reference units, centered.
    /// - Modal height: maximum 82% of Safe Area height.
    /// Pure structural component; adds no presentation animation or decorative styling.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class ModalSafeContent : MonoBehaviour
    {
        public const float MaxModalWidth = 840f;
        public const float MaxWidthRatio = 0.92f;
        public const float MaxHeightRatio = 0.82f;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        public Rect CurrentSafeArea => Screen.safeArea;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            ScreenOrientation orientation = Screen.orientation;

            if (safeArea != _lastSafeArea || screenSize != _lastScreenSize || orientation != _lastOrientation)
            {
                _lastSafeArea = safeArea;
                _lastScreenSize = screenSize;
                _lastOrientation = orientation;
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            ApplySafeArea(Screen.safeArea, new Vector2Int(Screen.width, Screen.height));
        }

        public void ApplySafeArea(Rect r, Vector2Int screenSize)
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (screenSize.x <= 0 || screenSize.y <= 0) return;

            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;

            anchorMin.x /= screenSize.x;
            anchorMax.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.y /= screenSize.y;

            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);

            ApplyChildCardBounds();
        }

        /// <summary>
        /// Adjusts child modal cards so they never exceed maximum responsive bounds:
        /// Width: min(92% Safe Area width, 840)
        /// Height: max 82% Safe Area height
        /// </summary>
        public void ApplyChildCardBounds()
        {
            if (_rectTransform == null) return;

            float safeWidth = _rectTransform.rect.width;
            float safeHeight = _rectTransform.rect.height;

            if (safeWidth <= 0f || safeHeight <= 0f)
            {
                // In Editor / unrendered state, fallback to 1080x1920 reference
                safeWidth = 1080f;
                safeHeight = 1920f;
            }

            float targetWidth = Mathf.Min(safeWidth * MaxWidthRatio, MaxModalWidth);
            float maxAllowedHeight = safeHeight * MaxHeightRatio;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                RectTransform rt = child as RectTransform;
                if (rt != null)
                {
                    float currentH = rt.sizeDelta.y > 0 ? rt.sizeDelta.y : 800f;
                    float clampedH = Mathf.Min(currentH, maxAllowedHeight);
                    rt.sizeDelta = new Vector2(targetWidth, clampedH);
                }
            }
        }
    }
}
