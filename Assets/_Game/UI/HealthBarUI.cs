using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Entity targetEntity;
        [SerializeField] private EntityType boundEntityType = EntityType.Hero;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI hpText;

        public Entity TargetEntity => targetEntity;
        public EntityType BoundEntityType => boundEntityType;
        public Image FillImage => fillImage;
        public TextMeshProUGUI HpText => hpText;
        public float CurrentDisplayedFill => fillImage != null ? fillImage.fillAmount : 0f;
        public string CurrentDisplayedText => hpText != null ? hpText.text : string.Empty;

        public void SetReferences(Image fill, TextMeshProUGUI text, EntityType entityType = EntityType.Hero)
        {
            fillImage = fill;
            hpText = text;
            boundEntityType = entityType;
        }

        private void Start()
        {
            if (targetEntity == null)
            {
                FindAndBindInitialEntity();
            }
        }

        public void FindAndBindInitialEntity()
        {
            Entity[] entities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude);
            foreach (var e in entities)
            {
                if (e != null && e.EntityType == boundEntityType)
                {
                    BindEntity(e);
                    break;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnEntityDamaged += HandleEntityDamaged;
        }

        private void OnDisable()
        {
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnEntityDamaged -= HandleEntityDamaged;

            if (targetEntity != null && targetEntity.Health != null)
            {
                targetEntity.Health.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void OnDestroy()
        {
            if (targetEntity != null && targetEntity.Health != null)
            {
                targetEntity.Health.OnHealthChanged -= UpdateHealthBar;
            }
        }

        public void BindEntity(Entity entity)
        {
            if (targetEntity != null && targetEntity.Health != null)
            {
                targetEntity.Health.OnHealthChanged -= UpdateHealthBar;
            }

            targetEntity = entity;
            if (targetEntity != null)
            {
                boundEntityType = targetEntity.EntityType;
            }

            if (targetEntity != null && targetEntity.Health != null)
            {
                targetEntity.Health.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(targetEntity.Health.CurrentHealth, targetEntity.Health.MaxHealth);
            }
        }

        private void HandleEntitySpawned(Entity entity)
        {
            if (entity != null && entity.EntityType == boundEntityType)
            {
                BindEntity(entity);
            }
        }

        private void HandleEntityDamaged(Entity entity, DamageResult result)
        {
            if (entity == targetEntity && targetEntity != null && targetEntity.Health != null)
            {
                UpdateHealthBar(targetEntity.Health.CurrentHealth, targetEntity.Health.MaxHealth);
            }
        }

        public void UpdateHealthBar(float current, float max)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            }

            if (hpText != null)
            {
                hpText.text = $"{current:F2} / {max:F2}";
            }
        }
    }
}
