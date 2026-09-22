using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Entities;
using WuxiaGame.UI.Core;

namespace WuxiaGame.UI.HUD
{
    /// <summary>
    /// Presentation component for Companion entities (EntityType.Companion).
    /// Pure presentation layer: reflects authoritative companion entity health and alive status.
    /// Automatically hides when no companions are present in the encounter.
    /// </summary>
    public class CompanionHUDUI : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private Entity boundCompanion;

        [Header("Visual Components")]
        [SerializeField] private GameObject cardRoot;
        [SerializeField] private Image companionAvatar;
        [SerializeField] private TextMeshProUGUI companionNameText;
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI statusText;

        public Entity BoundCompanion => boundCompanion;

        private void Awake()
        {
            if (cardRoot == null) cardRoot = gameObject;
        }

        public void SetReferences(GameObject root, TextMeshProUGUI nameTxt, Image fill, TextMeshProUGUI hpTxt, TextMeshProUGUI statusTxt, Image avatar = null)
        {
            cardRoot = root;
            companionNameText = nameTxt;
            hpFill = fill;
            hpText = hpTxt;
            statusText = statusTxt;
            companionAvatar = avatar;
        }

        private void Start()
        {
            if (boundCompanion == null)
            {
                FindCompanion();
            }
            UpdateDisplay();
        }

        private void OnEnable()
        {
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnEntityDied += HandleEntityDied;
        }

        private void OnDisable()
        {
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnEntityDied -= HandleEntityDied;

            if (boundCompanion != null && boundCompanion.Health != null)
            {
                boundCompanion.Health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        public void BindCompanion(Entity companion)
        {
            if (boundCompanion != null && boundCompanion.Health != null)
            {
                boundCompanion.Health.OnHealthChanged -= HandleHealthChanged;
            }

            boundCompanion = companion;

            if (boundCompanion != null && boundCompanion.Health != null)
            {
                boundCompanion.Health.OnHealthChanged += HandleHealthChanged;
            }

            UpdateDisplay();
        }

        private void FindCompanion()
        {
            Entity[] entities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude);
            if (entities != null)
            {
                foreach (var e in entities)
                {
                    if (e != null && e.EntityType == EntityType.Companion)
                    {
                        BindCompanion(e);
                        return;
                    }
                }
            }

            // No companion found
            if (cardRoot != null) cardRoot.SetActive(false);
        }

        private void HandleEntitySpawned(Entity entity)
        {
            if (entity != null && entity.EntityType == EntityType.Companion)
            {
                BindCompanion(entity);
            }
        }

        private void HandleEntityDied(Entity entity)
        {
            if (entity != null && entity == boundCompanion)
            {
                UpdateDisplay();
            }
        }

        private void HandleHealthChanged(float cur, float max)
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (boundCompanion == null)
            {
                if (cardRoot != null) cardRoot.SetActive(false);
                return;
            }

            if (cardRoot != null && !cardRoot.activeSelf)
            {
                cardRoot.SetActive(true);
            }

            if (companionNameText != null)
            {
                companionNameText.text = !string.IsNullOrEmpty(boundCompanion.EntityName) ? boundCompanion.EntityName : "ĐỒNG ĐỘI";
            }

            if (boundCompanion.Health != null)
            {
                float cur = boundCompanion.Health.CurrentHealth;
                float max = boundCompanion.Health.MaxHealth;

                if (hpFill != null)
                {
                    hpFill.fillAmount = max > 0f ? Mathf.Clamp01(cur / max) : 0f;
                }

                if (hpText != null)
                {
                    hpText.text = $"{cur:F0}/{max:F0}";
                }
            }

            if (statusText != null)
            {
                if (!boundCompanion.IsAlive)
                {
                    statusText.text = "TRỌNG THƯƠNG";
                    statusText.color = UIStyleConfig.ButtonDanger;
                }
                else
                {
                    statusText.text = "CHIẾN ĐẤU";
                    statusText.color = UIStyleConfig.StatusBuff;
                }
            }
        }
    }
}
