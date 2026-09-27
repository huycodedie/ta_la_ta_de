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
        public BattleManager BoundBattleManager { get; private set; }
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
            BoundBattleManager = BattleManager.Instance;
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
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += HandleSceneUnloaded;

            CreatePlaceholderVisual();
        }

        private void OnEnable()
        {
            if (IsCancelled || HasImpacted || Effects == null || Effects.Count == 0 || BoundTarget == null)
            {
                return;
            }
            if (!_activeProjectiles.Contains(this))
            {
                _activeProjectiles.Add(this);
            }
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void OnDisable()
        {
            _activeProjectiles.Remove(this);
            EventBus.OnEntityDied -= HandleEntityDied;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            if (!HasImpacted && !IsCancelled)
            {
                IsCancelled = true;
                CancelReason = "Projectile controller or root GameObject disabled.";
                Effects = null;
                Request = null;
                BoundTarget = null;
                Caster = null;
                Debug.Log($"[PROJECTILE] Cancelled: {CancelReason}");
            }
        }

        private void OnDestroy()
        {
            _activeProjectiles.Remove(this);
            EventBus.OnEntityDied -= HandleEntityDied;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            if (!HasImpacted && !IsCancelled)
            {
                Cancel("Projectile destroyed.");
            }
        }

        private void HandleSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            if (scene == gameObject.scene || !gameObject.scene.IsValid() || !gameObject.scene.isLoaded)
            {
                Cancel("Scene unloaded.");
            }
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

        /// <summary>
        /// Validates that an entity is currently registered and belongs to the specified encounter.
        /// Read-only inspection using canonical BattleManager APIs.
        /// </summary>
        public static bool IsEntityInEncounter(BattleManager bm, Entity entity)
        {
            if (bm == null || entity == null) return false;
            if (!entity.gameObject.activeInHierarchy || !entity.IsAlive) return false;
            if (entity.Health != null && entity.Health.CurrentHealth <= 0f) return false;

            if (entity is Monster m)
            {
                var active = bm.ActiveMonsters;
                if (active == null) return false;
                for (int i = 0; i < active.Count; i++)
                {
                    if (active[i] == m) return true;
                }
                return false;
            }

            return bm.BelongsToEncounter(entity);
        }

        public void SimulateTick(float deltaTime)
        {
            if (HasImpacted || IsCancelled) return;

            // 1. Cancellation & Invalidation Conditions (evaluated BEFORE pause and deltaTime)

            // 1.1 Encounter / BattleManager Guard: directly check current BattleManager.Instance against bound manager
            var currentBm = BattleManager.Instance;
            if (currentBm == null || currentBm != BoundBattleManager || currentBm.EncounterIndex != BoundEncounterIndex)
            {
                Cancel($"Encounter or BattleManager changed or invalidated before arrival (BoundWave={BoundEncounterIndex}, CurrentWave={currentBm?.EncounterIndex}).");
                return;
            }

            // 1.2 Caster & Target Encounter Membership Guard (via existing read-only APIs)
            if (!IsEntityInEncounter(currentBm, Caster))
            {
                Cancel("Caster is null or no longer belongs to active encounter.");
                return;
            }

            if (!IsEntityInEncounter(currentBm, BoundTarget))
            {
                Cancel("Bound target is null or no longer registered in active encounter.");
                return;
            }

            // 1.3 Target Validity Guard: cancel if target null, destroyed, inactive, or dead
            if (BoundTarget == null || !BoundTarget.gameObject.activeInHierarchy || !BoundTarget.IsAlive ||
                (BoundTarget.Health != null && BoundTarget.Health.CurrentHealth <= 0f))
            {
                Cancel("Bound target is null, destroyed, dead, or inactive.");
                return;
            }

            // 1.4 Caster Validity Guard: cancel if caster null, destroyed, inactive, or dead
            if (Caster == null || !Caster.gameObject.activeInHierarchy || !Caster.IsAlive ||
                (Caster.Health != null && Caster.Health.CurrentHealth <= 0f))
            {
                Cancel("Caster is null, destroyed, dead, or inactive.");
                return;
            }

            // 1.5 Immediate Expiry Guard: if already expired
            if (ElapsedTime >= Lifetime)
            {
                Cancel("Projectile lifetime expired.");
                return;
            }

            // 2. Combat Pause Guard: freeze travel during combat pause
            if (!currentBm.IsBattleActive)
            {
                return;
            }

            // 3. DeltaTime check
            if (deltaTime <= 0f) return;

            // 4. Lifetime Expiry Guard
            ElapsedTime += deltaTime;
            if (ElapsedTime >= Lifetime)
            {
                Cancel("Projectile lifetime expired.");
                return;
            }

            // 5. Homing Movement
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

            // Final fail-closed check right at arrival before applying effects
            var currentBm = BattleManager.Instance;
            if (currentBm == null || currentBm != BoundBattleManager || currentBm.EncounterIndex != BoundEncounterIndex ||
                !IsEntityInEncounter(currentBm, BoundTarget) || !IsEntityInEncounter(currentBm, Caster))
            {
                Cancel("Encounter, caster, or target invalidated at impact arrival.");
                return;
            }

            HasImpacted = true;

            try
            {
                if (Effects != null && Effects.Count > 0)
                {
                    var targetResolver = new BoundTargetResolver(BoundTarget, BoundBattleManager, BoundEncounterIndex);
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
                Effects = null;
                Request = null;
                BoundTarget = null;
                Caster = null;
                DestroyCleanly();
            }
        }

        public void Cancel(string reason)
        {
            if (HasImpacted || IsCancelled) return;
            IsCancelled = true;
            CancelReason = reason;
            Effects = null;
            Request = null;
            BoundTarget = null;
            Caster = null;

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
