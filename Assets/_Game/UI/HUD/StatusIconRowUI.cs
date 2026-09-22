using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Entities;
using WuxiaGame.UI.Core;

namespace WuxiaGame.UI.HUD
{
    /// <summary>
    /// Compact, data-driven status effect indicator row for combatants.
    /// Pure presentation layer reading strictly from EntityStatusController.
    /// Does not own or mutate gameplay timers or status calculations.
    /// </summary>
    public class StatusIconRowUI : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private Entity targetEntity;
        [SerializeField] private EntityType boundEntityType = EntityType.Hero;

        [Header("Container")]
        [SerializeField] private RectTransform badgesRoot;

        private class StatusBadge
        {
            public GameObject Root;
            public Image Background;
            public TextMeshProUGUI Text;
        }

        private readonly List<StatusBadge> badgePool = new List<StatusBadge>();
        private const int MaxBadges = 8;
        private Sprite defaultWhiteSprite;

        public Entity TargetEntity => targetEntity;
        public EntityType BoundEntityType => boundEntityType;

        private void Awake()
        {
            if (badgesRoot == null)
            {
                badgesRoot = GetComponent<RectTransform>();
            }
            CreateBadgePool();
        }

        private void Start()
        {
            if (targetEntity == null)
            {
                FindAndBindInitialEntity();
            }
            RefreshStatusDisplay();
        }

        private void OnEnable()
        {
            EventBus.OnEntitySpawned += HandleEntitySpawned;
            EventBus.OnCrowdControlApplied += HandleCCChanged;
            EventBus.OnCrowdControlExpired += HandleCCExpired;
            EventBus.OnCrowdControlImmune += HandleCCImmune;
            EventBus.OnDebuffApplied += HandleDebuffChanged;
            EventBus.OnDebuffExpired += HandleDebuffExpired;
        }

        private void OnDisable()
        {
            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnCrowdControlApplied -= HandleCCChanged;
            EventBus.OnCrowdControlExpired -= HandleCCExpired;
            EventBus.OnCrowdControlImmune -= HandleCCImmune;
            EventBus.OnDebuffApplied -= HandleDebuffChanged;
            EventBus.OnDebuffExpired -= HandleDebuffExpired;
        }

        public void BindEntity(Entity entity)
        {
            targetEntity = entity;
            if (targetEntity != null)
            {
                boundEntityType = targetEntity.EntityType;
            }
            RefreshStatusDisplay();
        }

        public void FindAndBindInitialEntity()
        {
            Entity[] entities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude);
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

        private void HandleEntitySpawned(Entity entity)
        {
            if (entity != null && entity.EntityType == boundEntityType)
            {
                BindEntity(entity);
            }
        }

        private void HandleCCChanged(Entity entity, CrowdControlType type, float dur)
        {
            if (entity == targetEntity) RefreshStatusDisplay();
        }

        private void HandleCCExpired(Entity entity, CrowdControlType type)
        {
            if (entity == targetEntity) RefreshStatusDisplay();
        }

        private void HandleCCImmune(Entity entity, CrowdControlType type)
        {
            if (entity == targetEntity) RefreshStatusDisplay();
        }

        private void HandleDebuffChanged(Entity entity, string debuffId)
        {
            if (entity == targetEntity) RefreshStatusDisplay();
        }

        private void HandleDebuffExpired(Entity entity, string debuffId)
        {
            if (entity == targetEntity) RefreshStatusDisplay();
        }

        private void Update()
        {
            // Lightweight tick for remaining duration readouts
            RefreshStatusDisplay();
        }

        private void CreateBadgePool()
        {
            if (defaultWhiteSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                defaultWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }

            Transform parent = badgesRoot != null ? badgesRoot.transform : transform;

            for (int i = 0; i < MaxBadges; i++)
            {
                GameObject badgeGO = new GameObject($"Badge_{i}");
                badgeGO.transform.SetParent(parent, false);

                RectTransform rt = badgeGO.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(72f, 22f);

                Image bg = badgeGO.AddComponent<Image>();
                bg.sprite = UIProceduralTextureFactory.GetPanelSprite();
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(badgeGO.transform, false);
                RectTransform textRT = textGO.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 11;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                badgeGO.SetActive(false);

                badgePool.Add(new StatusBadge
                {
                    Root = badgeGO,
                    Background = bg,
                    Text = tmp
                });
            }
        }

        public void RefreshStatusDisplay()
        {
            if (badgePool.Count == 0)
            {
                CreateBadgePool();
            }

            if (targetEntity == null || !targetEntity.IsAlive || targetEntity.StatusController == null)
            {
                HideAllBadges();
                return;
            }

            var sc = targetEntity.StatusController;
            int activeCount = 0;

            // 1. Crowd Control: Stun
            if (sc.IsStunned && activeCount < MaxBadges)
            {
                SetBadge(activeCount++, "CHOÁNG", UIStyleConfig.StatusStun, Color.black);
            }

            // 2. Crowd Control: Freeze
            if (sc.IsFrozen && activeCount < MaxBadges)
            {
                SetBadge(activeCount++, "BĂNG", UIStyleConfig.StatusFreeze, Color.black);
            }

            // 3. Crowd Control: Root
            if (sc.IsRooted && activeCount < MaxBadges)
            {
                SetBadge(activeCount++, "TRÓI", UIStyleConfig.StatusRoot, Color.black);
            }

            // 4. Immunity: Anti-CC
            if (sc.HasAntiCCImmunity && activeCount < MaxBadges)
            {
                float rem = sc.AntiCCRemainingDuration;
                string label = rem > 0.1f ? $"MIỄN {rem:F1}s" : "MIỄN";
                SetBadge(activeCount++, label, UIStyleConfig.StatusAntiCC, Color.white);
            }

            // 5. Active Buffs
            if (sc.ActiveBuffs != null)
            {
                foreach (var kvp in sc.ActiveBuffs)
                {
                    if (activeCount >= MaxBadges) break;
                    if (kvp.Value != null && !kvp.Value.IsExpired)
                    {
                        string buffName = kvp.Key.Length > 6 ? kvp.Key.Substring(0, 6).ToUpper() : kvp.Key.ToUpper();
                        SetBadge(activeCount++, buffName, UIStyleConfig.StatusBuff, Color.black);
                    }
                }
            }

            // 6. Active Debuffs
            if (sc.ActiveDebuffs != null)
            {
                foreach (var kvp in sc.ActiveDebuffs)
                {
                    if (activeCount >= MaxBadges) break;
                    if (kvp.Value != null && !kvp.Value.IsExpired)
                    {
                        string debuffName = kvp.Key.Length > 6 ? kvp.Key.Substring(0, 6).ToUpper() : kvp.Key.ToUpper();
                        SetBadge(activeCount++, debuffName, UIStyleConfig.StatusDebuff, Color.white);
                    }
                }
            }

            // Deactivate remaining badges
            for (int i = activeCount; i < badgePool.Count; i++)
            {
                if (badgePool[i].Root != null && badgePool[i].Root.activeSelf)
                {
                    badgePool[i].Root.SetActive(false);
                }
            }
        }

        private void SetBadge(int index, string text, Color bgColor, Color textColor)
        {
            if (index < 0 || index >= badgePool.Count) return;

            StatusBadge badge = badgePool[index];
            if (badge.Root != null)
            {
                if (!badge.Root.activeSelf) badge.Root.SetActive(true);
                if (badge.Background != null) badge.Background.color = bgColor;
                if (badge.Text != null)
                {
                    badge.Text.text = text;
                    badge.Text.color = textColor;
                }
            }
        }

        private void HideAllBadges()
        {
            for (int i = 0; i < badgePool.Count; i++)
            {
                if (badgePool[i].Root != null && badgePool[i].Root.activeSelf)
                {
                    badgePool[i].Root.SetActive(false);
                }
            }
        }
    }
}
