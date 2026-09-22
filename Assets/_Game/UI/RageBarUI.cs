using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.UI
{
    public class RageBarUI : MonoBehaviour
    {
        [SerializeField] private Entity targetEntity;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI rageText;
        [SerializeField] private Image glowImage;

        private static readonly Color NormalRageColor = new Color(1.00f, 0.55f, 0.10f, 1f);
        private static readonly Color FullRageColor = new Color(1.00f, 0.78f, 0.20f, 1f);
        private bool isRageFull = false;

        public Entity TargetEntity => targetEntity;
        public Image FillImage => fillImage;
        public TextMeshProUGUI RageText => rageText;
        public Image GlowImage => glowImage;
        public float CurrentDisplayedFill => fillImage != null ? fillImage.fillAmount : 0f;
        public string CurrentDisplayedText => rageText != null ? rageText.text : string.Empty;

        public void SetReferences(Image fill, TextMeshProUGUI text)
        {
            fillImage = fill;
            rageText = text;
        }

        public void SetReferences(Image fill, TextMeshProUGUI text, Image glow)
        {
            fillImage = fill;
            rageText = text;
            glowImage = glow;
        }

        private void Start()
        {
            if (targetEntity == null)
            {
                FindAndBindHero();
            }
            else
            {
                BindEntity(targetEntity);
            }
        }

        private void OnEnable()
        {
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnRageChanged += HandleRageChanged;

            if (targetEntity != null && targetEntity.Rage != null)
            {
                targetEntity.Rage.OnRageChanged -= UpdateRageBar;
                targetEntity.Rage.OnRageChanged += UpdateRageBar;
                UpdateRageBar(targetEntity.Rage.CurrentRage, targetEntity.Rage.MaxRage);
            }
        }

        private void OnDisable()
        {
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnRageChanged -= HandleRageChanged;

            if (targetEntity != null && targetEntity.Rage != null)
            {
                targetEntity.Rage.OnRageChanged -= UpdateRageBar;
            }
        }

        private void OnDestroy()
        {
            if (targetEntity != null && targetEntity.Rage != null)
            {
                targetEntity.Rage.OnRageChanged -= UpdateRageBar;
            }
        }

        public void FindAndBindHero()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero != null)
            {
                BindEntity(hero);
            }
        }

        private void HandleEntitySpawned(Entity entity)
        {
            if (entity is Hero)
            {
                BindEntity(entity);
            }
        }

        private void HandleRageChanged(Entity entity, float current, float max)
        {
            if (targetEntity == null || entity == targetEntity)
            {
                UpdateRageBar(current, max);
            }
        }

        public void BindEntity(Entity entity)
        {
            if (targetEntity != null && targetEntity.Rage != null)
            {
                targetEntity.Rage.OnRageChanged -= UpdateRageBar;
            }

            targetEntity = entity;

            if (targetEntity != null && targetEntity.Rage != null)
            {
                targetEntity.Rage.OnRageChanged += UpdateRageBar;
                UpdateRageBar(targetEntity.Rage.CurrentRage, targetEntity.Rage.MaxRage);
            }
        }

        private void Update()
        {
            if (isRageFull && fillImage != null)
            {
                fillImage.color = Color.Lerp(NormalRageColor, FullRageColor, 0.5f + 0.5f * Mathf.Sin(Time.time * 5f));
            }
        }

        public void UpdateRageBar(float current, float max)
        {
            isRageFull = (max > 0f && current >= max - 0.01f);
            if (fillImage != null)
            {
                fillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
                if (!isRageFull)
                {
                    fillImage.color = NormalRageColor;
                }
            }

            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(isRageFull);
            }

            if (rageText != null)
            {
                rageText.text = $"Rage: {Mathf.FloorToInt(current)} / {Mathf.FloorToInt(max)}";
            }
        }
    }
}
