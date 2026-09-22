using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WuxiaGame.UI
{
    public class UIRaycastDebugger : MonoBehaviour
    {
        [SerializeField] private bool logOnPointerDown = true;
        [SerializeField] private bool logAllHits = false;

        private void Update()
        {
            if (!logOnPointerDown) return;

            bool pointerDown = false;
            Vector2 screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointerDown = true;
                screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }
            else if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                pointerDown = true;
                screenPos = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0))
            {
                pointerDown = true;
                screenPos = Input.mousePosition;
            }
#endif

            if (pointerDown)
            {
                LogRaycastAtPosition(screenPos);
            }
        }

        public static void LogRaycastAtPosition(Vector2 screenPos)
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("[UI RAYCAST DEBUG] EventSystem.current is null!");
                return;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[UI RAYCAST DEBUG]");
            sb.AppendLine($"Pointer position: {screenPos}");
            sb.AppendLine($"Raycast count: {results.Count}");

            if (results.Count > 0)
            {
                RaycastResult topHit = results[0];
                GameObject hitGO = topHit.gameObject;
                sb.AppendLine($"Hit object: {hitGO.name}");

                // Hierarchy path
                string hierarchy = GetHierarchyPath(hitGO.transform);
                sb.AppendLine($"Hit hierarchy: {hierarchy}");

                // Button detection
                Button btn = hitGO.GetComponentInParent<Button>();
                if (btn != null)
                {
                    sb.AppendLine($"Button detected: {btn.gameObject.name}");
                    sb.AppendLine($"Button interactable: {btn.interactable}");
                    sb.AppendLine($"Button enabled: {btn.enabled}");
                }
                else
                {
                    sb.AppendLine("Button detected: None");
                    sb.AppendLine("Button interactable: N/A");
                    sb.AppendLine("Button enabled: N/A");
                }

                // CanvasGroup detection
                CanvasGroup cg = hitGO.GetComponentInParent<CanvasGroup>();
                if (cg != null)
                {
                    sb.AppendLine($"CanvasGroup.interactable: {cg.interactable}");
                    sb.AppendLine($"CanvasGroup.blocksRaycasts: {cg.blocksRaycasts}");
                }
                else
                {
                    sb.AppendLine("CanvasGroup.interactable: N/A");
                    sb.AppendLine("CanvasGroup.blocksRaycasts: N/A");
                }

                if (results.Count > 1)
                {
                    sb.AppendLine("--- All Hit Objects ---");
                    for (int i = 0; i < results.Count; i++)
                    {
                        sb.AppendLine($" [{i}] {results[i].gameObject.name} (depth: {results[i].depth}, module: {results[i].module?.GetType().Name})");
                    }
                }
            }
            else
            {
                sb.AppendLine("Hit object: None (Raycast missed all UI)");
                sb.AppendLine("Hit hierarchy: None");
                sb.AppendLine("Button detected: None");
                sb.AppendLine("Button interactable: N/A");
                sb.AppendLine("Button enabled: N/A");
                sb.AppendLine("CanvasGroup.interactable: N/A");
                sb.AppendLine("CanvasGroup.blocksRaycasts: N/A");
            }

            Debug.Log(sb.ToString());
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return string.Empty;
            List<string> parts = new List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join(" / ", parts);
        }
    }
}
