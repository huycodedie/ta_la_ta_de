using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    /// <summary>
    /// P09-A: Projectile Controller Foundation.
    /// Manages single-target homing projectile delivery through the existing EffectResolver pipeline.
    /// Strictly enforces:
    /// - Homing toward bound living target only.
    /// - Zero damage calculation / HP mutation in projectile code (delegates to EffectResolver).
    /// - Clean cancellation on target death, caster death, encounter change, or lifetime expiry.
    /// - Freezes during combat pause (BattleManager.IsBattleActive == false).
    /// - Exactly one arrival/impact execution per released projectile.
    /// - Visual rendering has no combat authority.
    /// </summary>
    [ExecuteAlways]
    public class ProjectileController : MonoBehaviour
    {
        private static readonly List<ProjectileController> _activeProjectiles = new List<ProjectileController>();
        public static IReadOnlyList<ProjectileController> ActiveProjectiles => _activeProjectiles;

        public SkillExecutionRequest Request { get; private set; }
        public Entity BoundTarget { get; private set; }
        public Entity Caster { get; private set; }
        public int BoundEncounterIndex { get; private set; }
        public float Speed { get; private set; } = 15f;
        public float Lifetime { get; private set; } = 5f;
        public float ElapsedTime { get; private set; } = 0f;

        public bool HasImpacted { get; private set; } = false;
        public bool IsCancelled { get; private set; } = false;
        public string CancelReason { get; private set; } = string.Empty;

        public IReadOnlyList<SkillEffectDefinitionSO> Effects { get; private set; }
        public GameObject VisualObject { get; private set; }

        public static void ClearAllProjectiles()
        {
            var copy = new List<ProjectileController>(_activeProjectiles);
            foreach (var p in copy)
            {
                if (p != null)
                {
                    p.Cancel("Global cleanup requested.");
                }
            }
            _activeProjectiles.Clear();
        }

        public static ProjectileController Launch(
            SkillExecutionRequest request,
            Entity boundTarget,
            float speed,
            float lifetime,
            IReadOnlyList<SkillEffectDefinitionSO> effects)
        {
            if (request == null || boundTarget == null) return null;

            string skillName = request.Skill != null ? request.Skill.SkillId : "UnknownSkill";
            string sourceName = request.Source != null ? request.Source.EntityName : "UnknownSource";
            var go = new GameObject($"Projectile_{skillName}_{sourceName}");

            Vector3 spawnPos = request.Source != null
                ? request.Source.transform.position + Vector3.up * 0.5f
                : Vector3.zero;
            go.transform.position = spawnPos;

            var controller = go.AddComponent<ProjectileController>();
            int encounterIndex = BattleManager.Instance != null ? BattleManager.Instance.EncounterIndex : 1;
            controller.Initialize(request, boundTarget, request.Source, encounterIndex, speed, lifetime, effects);

            return controller;
        }

        public void Initialize(
            SkillExecutionRequest request,
            Entity boundTarget,
            Entity caster,
            int encounterIndex,
            float speed,
            float lifetime,
            IReadOnlyList<SkillEffectDefinitionSO> effects)
        {
            Request = request;
            BoundTarget = boundTarget;
            Caster = caster;
            BoundEncounterIndex = encounterIndex;
            Speed = Mathf.Max(0.001f, speed);
            Lifetime = Mathf.Max(0.001f, lifetime);
            ElapsedTime = 0f;
            HasImpacted = false;
            IsCancelled = false;
            CancelReason = string.Empty;

            Effects = effects != null
                ? new List<SkillEffectDefinitionSO>(effects)
                : new List<SkillEffectDefinitionSO>();

            if (!_activeProjectiles.Contains(this))
            {
                _activeProjectiles.Add(this);
            }
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;

            CreatePlaceholderVisual();
        }

        private void OnEnable()
        {
            if (!_activeProjectiles.Contains(this))
            {
                _activeProjectiles.Add(this);
            }
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;
        }

        private void OnDisable()
        {
            _activeProjectiles.Remove(this);
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        private void OnDestroy()
        {
            _activeProjectiles.Remove(this);
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        private void HandleEntityDied(Entity entity)
        {
            if (HasImpacted || IsCancelled) return;

            if (entity == BoundTarget)
            {
                Cancel("Bound target died before arrival.");
            }
            else if (entity == Caster)
            {
                Cancel("Caster died before arrival.");
            }
        }

        private void Update()
        {
            SimulateTick(Time.deltaTime);
        }

        public void SimulateTick(float deltaTime)
        {
            if (HasImpacted || IsCancelled) return;
            if (deltaTime <= 0f) return;

            // 1. Combat Pause Guard: freeze travel during combat pause
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive)
            {
                return;
            }

            // 2. Encounter Replacement Guard: do not damage next wave
            if (BattleManager.Instance != null && BattleManager.Instance.EncounterIndex != BoundEncounterIndex)
            {
                Cancel($"Encounter index changed before arrival (BM={BattleManager.Instance.EncounterIndex}, Bound={BoundEncounterIndex}).");
                return;
            }

            // 3. Target Validity Guard: cancel if target dead, destroyed, or inactive
            if (BoundTarget == null || !BoundTarget.gameObject.activeInHierarchy || !BoundTarget.IsAlive)
            {
                Cancel("Bound target is dead, destroyed, or inactive.");
                return;
            }
            if (BoundTarget.Health != null && BoundTarget.Health.CurrentHealth <= 0f)
            {
                Cancel("Bound target has zero or negative health.");
                return;
            }

            // 4. Caster Validity Guard: cancel if caster dead, destroyed, or inactive
            if (Caster != null)
            {
                if (!Caster.gameObject.activeInHierarchy || !Caster.IsAlive ||
                    (Caster.Health != null && Caster.Health.CurrentHealth <= 0f))
                {
                    Cancel("Caster is dead, destroyed, or inactive.");
                    return;
                }
            }

            // 5. Lifetime Expiry Guard
            ElapsedTime += deltaTime;
            if (ElapsedTime >= Lifetime)
            {
                Cancel("Projectile lifetime expired.");
                return;
            }

            // 6. Homing Movement
            Vector3 targetCenter = BoundTarget.transform.position + Vector3.up * 0.5f;
            Vector3 diff = targetCenter - transform.position;
            float dist = diff.magnitude;
            float step = Speed * deltaTime;

            const float arrivalThreshold = 0.2f;
            if (dist <= step || dist <= arrivalThreshold)
            {
                transform.position = targetCenter;
                Impact();
            }
            else
            {
                transform.position += diff.normalized * step;
            }
        }

        private void Impact()
        {
            if (HasImpacted || IsCancelled) return;
            HasImpacted = true;

            try
            {
                if (Effects != null && Effects.Count > 0)
                {
                    var targetResolver = new BoundTargetResolver(BoundTarget);
                    EffectResolver.Instance.ProcessEffects(Request, Effects, targetResolver);
                }
                Debug.Log($"[PROJECTILE] Impacted bound target '{BoundTarget?.EntityName}' at position {transform.position}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PROJECTILE] Error executing impact effects: {ex}");
            }
            finally
            {
                DestroyCleanly();
            }
        }

        public void Cancel(string reason)
        {
            if (HasImpacted || IsCancelled) return;
            IsCancelled = true;
            CancelReason = reason;

            Debug.Log($"[PROJECTILE] Cancelled: {reason}");
            DestroyCleanly();
        }

        private void DestroyCleanly()
        {
            _activeProjectiles.Remove(this);
            if (gameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        private void CreatePlaceholderVisual()
        {
            try
            {
                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "PlaceholderVisual";
                visual.transform.SetParent(transform, false);
                visual.transform.localScale = Vector3.one * 0.25f;

                var col = visual.GetComponent<Collider>();
                if (col != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(col);
                    }
                    else
                    {
                        DestroyImmediate(col);
                    }
                }
                VisualObject = visual;
            }
            catch
            {
                // In headless or batchmode environments where primitive mesh generation may be suppressed
            }
        }
    }
}
