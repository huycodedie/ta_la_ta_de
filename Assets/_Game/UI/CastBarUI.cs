using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.UI
{
    /// <summary>
    /// P07.9 Phase 5.3: Cast & Channel Progress Bar and Interrupt Feedback UI.
    /// Pure presentation layer. Reads directly from runtime authority (Entity.CastState / SkillCastState).
    /// Does NOT mutate gameplay state, does NOT create timers, coroutines, or secondary authorities.
    /// </summary>
    public class CastBarUI : MonoBehaviour
    {
        [Header("Target Binding")]
        [SerializeField] private Entity targetEntity;
        [SerializeField] private EntityType boundEntityType = EntityType.Hero;

        [Header("Visual Components")]
        [SerializeField] private GameObject rootObject;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI castText;
        [SerializeField] private TextMeshProUGUI interruptText;

        [Header("Colors")]
        [SerializeField] private Color castColor = new Color(0.2f, 0.8f, 1f, 1f);       // Cyan for Cast
        [SerializeField] private Color channelColor = new Color(0.85f, 0.45f, 1f, 1f);  // Purple for Channel
        [SerializeField] private Color interruptColor = new Color(1f, 0.3f, 0.3f, 1f);  // Red for Interrupt

        [Header("Feedback Settings")]
        [SerializeField] private float interruptFeedbackDuration = 2.0f;

        private float interruptFeedbackTimer = 0f;
        private bool isShowingInterruptFeedback = false;
        private SkillCastInterruptSource lastInterruptSource = SkillCastInterruptSource.None;

        public Entity TargetEntity => targetEntity;
        public EntityType BoundEntityType => boundEntityType;
        public GameObject RootObject => rootObject != null ? rootObject : gameObject;
        public Image FillImage => fillImage;
        public TextMeshProUGUI CastText => castText;
        public TextMeshProUGUI InterruptText => interruptText;

        public bool IsVisible => RootObject != null && RootObject.activeSelf && !isShowingInterruptFeedback;
        public float CurrentFill => fillImage != null ? fillImage.fillAmount : 0f;
        public string CurrentText => castText != null ? castText.text : string.Empty;
        public string CurrentInterruptText => interruptText != null ? interruptText.text : string.Empty;
        public bool IsShowingInterruptFeedback => isShowingInterruptFeedback;
        public SkillCastInterruptSource LastInterruptSource => lastInterruptSource;

        private void Awake()
        {
            if (rootObject == null) rootObject = gameObject;
        }

        private void Start()
        {
            if (targetEntity == null)
            {
                FindAndBindInitialEntity();
            }
            UpdateDisplay();
        }

        public void SetReferences(
            GameObject root,
            Image fill,
            TextMeshProUGUI cText,
            TextMeshProUGUI iText,
            Entity entity = null,
            EntityType entityType = EntityType.Hero)
        {
            rootObject = root != null ? root : gameObject;
            fillImage = fill;
            castText = cText;
            interruptText = iText;
            targetEntity = entity;
            boundEntityType = entityType;
            RegisterEvents();
        }

        public void BindEntity(Entity entity)
        {
            targetEntity = entity;
            if (entity != null) boundEntityType = entity.EntityType;
            ClearInterruptFeedback();
            UpdateDisplay();
        }

        public void FindAndBindInitialEntity()
        {
            Entity[] entities = UnityEngine.Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude);
            if (entities != null)
            {
                foreach (var e in entities)
                {
                    if (e != null && e.EntityType == boundEntityType)
                    {
                        BindEntity(e);
                        return;
                    }
                }
            }
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        public void RegisterEvents()
        {
            UnregisterEvents();
            EventBus.OnSkillCastInterrupted += HandleSkillCastInterrupted;
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnEntityDied += HandleEntityDied;
            EventBus.OnBattleStateChanged += HandleBattleStateChanged;
        }

        public void UnregisterEvents()
        {
            EventBus.OnSkillCastInterrupted -= HandleSkillCastInterrupted;
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnBattleStateChanged -= HandleBattleStateChanged;
        }

        private void HandleSkillCastInterrupted(Entity source, SkillDefinitionSO skill, SkillCastInterruptSource sourceReason)
        {
            if (this == null || !gameObject) return;
            if (source == null) return;
            if (targetEntity == null && source.EntityType == boundEntityType)
            {
                targetEntity = source;
            }

            if (source != targetEntity) return;

            if (sourceReason == SkillCastInterruptSource.Stun)
            {
                ShowInterruptFeedback("SKILL CAST INTERRUPTED (STUN)", sourceReason);
            }
            else if (sourceReason == SkillCastInterruptSource.Freeze)
            {
                ShowInterruptFeedback("SKILL CAST INTERRUPTED (FREEZE)", sourceReason);
            }
            else if (sourceReason == SkillCastInterruptSource.CrowdControl)
            {
                if (source.IsStunned)
                {
                    ShowInterruptFeedback("SKILL CAST INTERRUPTED (STUN)", SkillCastInterruptSource.Stun);
                }
                else if (source.IsFrozen)
                {
                    ShowInterruptFeedback("SKILL CAST INTERRUPTED (FREEZE)", SkillCastInterruptSource.Freeze);
                }
            }
            else if (sourceReason == SkillCastInterruptSource.CasterDeath)
            {
                ClearInterruptFeedback();
                HideCastBar();
            }
        }

        private void HandleEntitySpawned(Entity entity)
        {
            if (this == null || !gameObject) return;
            if (entity != null && entity.EntityType == boundEntityType)
            {
                BindEntity(entity);
            }
        }

        private void HandleEntityDied(Entity entity)
        {
            if (this == null || !gameObject) return;
            if (entity != null && entity == targetEntity)
            {
                ClearInterruptFeedback();
                HideCastBar();
            }
        }

        private void HandleBattleStateChanged(BattleState newState)
        {
            if (this == null || !gameObject) return;
            if (newState == BattleState.AwaitingPlayerStart || newState == BattleState.Defeat || newState == BattleState.Victory)
            {
                ClearInterruptFeedback();
                HideCastBar();
            }
        }

        public void ShowInterruptFeedback(string message, SkillCastInterruptSource source = SkillCastInterruptSource.None)
        {
            lastInterruptSource = source;
            isShowingInterruptFeedback = true;
            interruptFeedbackTimer = interruptFeedbackDuration;

            if (RootObject != null) RootObject.SetActive(true);
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                fillImage.gameObject.SetActive(false);
            }
            if (castText != null)
            {
                castText.text = string.Empty;
                castText.gameObject.SetActive(false);
            }
            if (interruptText != null)
            {
                interruptText.gameObject.SetActive(true);
                interruptText.text = message;
                interruptText.color = interruptColor;
            }
        }

        public void ClearInterruptFeedback()
        {
            isShowingInterruptFeedback = false;
            interruptFeedbackTimer = 0f;
            lastInterruptSource = SkillCastInterruptSource.None;

            if (interruptText != null)
            {
                interruptText.text = string.Empty;
                interruptText.gameObject.SetActive(false);
            }
        }

        public void HideCastBar()
        {
            if (RootObject != null && !isShowingInterruptFeedback)
            {
                if (RootObject != gameObject)
                {
                    RootObject.SetActive(false);
                }
                else
                {
                    if (fillImage != null) fillImage.gameObject.SetActive(false);
                    if (castText != null) castText.gameObject.SetActive(false);
                }
            }
            if (fillImage != null) fillImage.fillAmount = 0f;
            if (castText != null) castText.text = string.Empty;
        }

        private void Update()
        {
            float dt = Mathf.Clamp(Time.deltaTime, 0f, 0.1f);
            UpdateDisplay(dt);
        }

        public void ManualUpdate(float deltaTime = 0f)
        {
            UpdateDisplay(deltaTime);
        }

        public void UpdateDisplay(float deltaTime = 0f)
        {
            float dt = deltaTime;

            // Handle timer for interrupt feedback
            if (isShowingInterruptFeedback && dt > 0f)
            {
                interruptFeedbackTimer -= dt;
                if (interruptFeedbackTimer <= 0f)
                {
                    ClearInterruptFeedback();
                }
            }

            if (targetEntity == null)
            {
                FindAndBindInitialEntity();
            }

            if (targetEntity == null || !targetEntity.IsAlive)
            {
                ClearInterruptFeedback();
                HideCastBar();
                return;
            }

            SkillCastState castState = targetEntity.CastState;
            if (castState == null || !castState.IsActive)
            {
                // Not actively casting
                if (!isShowingInterruptFeedback)
                {
                    HideCastBar();
                }
                return;
            }

            // Actively casting or channeling
            // A new active cast immediately clears any old interrupt feedback
            if (isShowingInterruptFeedback)
            {
                ClearInterruptFeedback();
            }

            if (RootObject != null && !RootObject.activeSelf)
            {
                RootObject.SetActive(true);
            }

            if (fillImage != null)
            {
                if (!fillImage.gameObject.activeSelf) fillImage.gameObject.SetActive(true);
                fillImage.fillAmount = castState.Progress;
            }

            if (castText != null)
            {
                if (!castText.gameObject.activeSelf) castText.gameObject.SetActive(true);
            }

            if (interruptText != null && interruptText.gameObject.activeSelf)
            {
                interruptText.gameObject.SetActive(false);
            }

            string skillName = (castState.ActiveRequest != null && castState.ActiveRequest.Skill != null)
                ? castState.ActiveRequest.Skill.SkillName
                : "Skill";

            if (castState.CurrentPhase == SkillCastPhase.Channeling)
            {
                if (fillImage != null) fillImage.color = channelColor;
                if (castText != null)
                {
                    castText.text = $"CHANNEL: {skillName} ({castState.ElapsedChannelTime:F1}s / {castState.ChannelDuration:F1}s)";
                }
            }
            else // Casting
            {
                if (fillImage != null) fillImage.color = castColor;
                if (castText != null)
                {
                    castText.text = $"CAST: {skillName} ({castState.ElapsedTime:F1}s / {castState.CastDuration:F1}s)";
                }
            }
        }
    }
}
