using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Entities;

namespace WuxiaGame.UI
{
    public class StatusDebugUI : MonoBehaviour
    {
        [Header("Target Controllers")]
        [SerializeField] private EntityStatusController heroStatusController;
        [SerializeField] private EntityStatusController monsterStatusController;

        [Header("Display Settings")]
        [SerializeField] private bool showDebugUI = false;

        public void BindHero(EntityStatusController controller)
        {
            heroStatusController = controller;
        }

        public void BindMonster(EntityStatusController controller)
        {
            monsterStatusController = controller;
        }

        public string GetDebugText()
        {
            var sb = new System.Text.StringBuilder();

            if (heroStatusController != null)
            {
                sb.AppendLine($"[HERO STATUS] ({heroStatusController.ActiveStatuses.Count} active):");
                sb.AppendLine(heroStatusController.GetDebugStatusSummary());
            }

            if (monsterStatusController != null)
            {
                sb.AppendLine($"[MONSTER STATUS] ({monsterStatusController.ActiveStatuses.Count} active):");
                sb.AppendLine(monsterStatusController.GetDebugStatusSummary());
            }

            return sb.ToString();
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUI.color = Color.white;
            GUI.Box(new Rect(10, 300, 360, 200), "STATUS RUNTIME DEBUG (P07.5)");
            GUI.Label(new Rect(20, 325, 340, 165), GetDebugText());
        }
    }
}
