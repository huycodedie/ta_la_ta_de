using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public interface ISkillTargetResolver
    {
        List<Entity> ResolveTargets(SkillExecutionRequest request, SkillEffectDefinitionSO effect);
    }

    /// <summary>
    /// Shared canonical target-query helper for skill targeting and validation.
    /// Strictly enforces:
    /// - Source is active, alive Hero or Companion.
    /// - Target is active, alive Monster belonging to BattleManager.ActiveMonsters for the current encounter.
    /// </summary>
    public static class CombatTargetQuery
    {
        public static bool IsValidPlayerSource(Entity source)
        {
            if (source == null || !source.gameObject.activeInHierarchy || !source.IsAlive)
                return false;
            if (source.Health == null || source.Health.CurrentHealth <= 0f)
                return false;
            return source.EntityType == EntityType.Hero || source.EntityType == EntityType.Companion;
        }

        public static bool IsValidEncounterMonster(Entity source, Entity target)
        {
            if (!IsValidPlayerSource(source))
                return false;
            if (target == null || !(target is Monster monster))
                return false;
            if (!target.gameObject.activeInHierarchy || !target.IsAlive)
                return false;
            if (target.Health == null || target.Health.CurrentHealth <= 0f)
                return false;
            if (BattleManager.Instance == null)
                return false;
            if (BattleManager.Instance.ActiveMonsters == null || BattleManager.Instance.ActiveMonsters.Count == 0)
                return false;

            var active = BattleManager.Instance.ActiveMonsters;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i] == monster) return true;
            }
            return false;
        }

        public static bool TryGetValidAreaAnchor(SkillExecutionRequest request, out Monster anchor)
        {
            anchor = null;
            if (request == null || !IsValidPlayerSource(request.Source))
                return false;

            // 1. Explicit request target
            if (request.Target is Monster m1 && IsValidEncounterMonster(request.Source, m1))
            {
                anchor = m1;
                return true;
            }

            // 2. Source current target
            if (request.Source.CurrentTarget is Monster m2 && IsValidEncounterMonster(request.Source, m2))
            {
                anchor = m2;
                return true;
            }

            // 3. BattleManager currentMonster fallback
            if (BattleManager.Instance != null && BattleManager.Instance.CurrentMonster is Monster m3 &&
                IsValidEncounterMonster(request.Source, m3))
            {
                anchor = m3;
                return true;
            }

            // 4. BattleManager GetNextLivingMonster fallback
            if (BattleManager.Instance != null)
            {
                Monster m4 = BattleManager.Instance.GetNextLivingMonster();
                if (m4 != null && IsValidEncounterMonster(request.Source, m4))
                {
                    anchor = m4;
                    return true;
                }
            }

            return false;
        }

        public static bool HasLivingRegisteredMonster(Entity source)
        {
            if (!IsValidPlayerSource(source)) return false;
            if (BattleManager.Instance != null && BattleManager.Instance.ActiveMonsters != null && BattleManager.Instance.ActiveMonsters.Count > 0)
            {
                var list = BattleManager.Instance.ActiveMonsters;
                for (int i = 0; i < list.Count; i++)
                {
                    if (IsValidEncounterMonster(source, list[i])) return true;
                }
                return false;
            }
            if (BattleManager.Instance != null && BattleManager.Instance.CurrentMonster != null)
            {
                return IsValidEncounterMonster(source, BattleManager.Instance.CurrentMonster);
            }
            return false;
        }

        public static List<Entity> ResolveAreaTargets(SkillExecutionRequest request, SkillEffectDefinitionSO effect, Monster anchor)
        {
            var targets = new List<Entity>();
            if (request == null || effect == null || anchor == null) return targets;
            if (effect.TargetRadius <= 0f) return targets;

            // Anchor is always ordered first
            targets.Add(anchor);

            if (BattleManager.Instance == null || BattleManager.Instance.ActiveMonsters == null)
                return targets;

            var activeMonsters = BattleManager.Instance.ActiveMonsters;
            var remainingCandidates = new List<Monster>();

            float anchorX = anchor.transform.position.x;
            float anchorZ = anchor.transform.position.z;
            float radiusSq = effect.TargetRadius * effect.TargetRadius;

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m == null || m == anchor) continue;
                if (!IsValidEncounterMonster(request.Source, m)) continue;

                float dx = m.transform.position.x - anchorX;
                float dz = m.transform.position.z - anchorZ;
                float distSq = dx * dx + dz * dz;

                if (distSq <= radiusSq)
                {
                    remainingCandidates.Add(m);
                }
            }

            // Deterministic ordering: horizontal distance ascending, tie-break by registration order ascending
            remainingCandidates.Sort((a, b) =>
            {
                float dxA = a.transform.position.x - anchorX;
                float dzA = a.transform.position.z - anchorZ;
                float distA = dxA * dxA + dzA * dzA;

                float dxB = b.transform.position.x - anchorX;
                float dzB = b.transform.position.z - anchorZ;
                float distB = dxB * dxB + dzB * dzB;

                int distCompare = distA.CompareTo(distB);
                if (distCompare != 0) return distCompare;

                int orderA = BattleManager.Instance.GetRegistrationOrder(a);
                int orderB = BattleManager.Instance.GetRegistrationOrder(b);
                return orderA.CompareTo(orderB);
            });

            // Append remaining and deduplicate
            for (int i = 0; i < remainingCandidates.Count; i++)
            {
                var c = remainingCandidates[i];
                if (!targets.Contains(c))
                {
                    targets.Add(c);
                }
            }

            // MaxTargetCount limit
            if (effect.MaxTargetCount > 0 && targets.Count > effect.MaxTargetCount)
            {
                targets = targets.GetRange(0, effect.MaxTargetCount);
            }

            return targets;
        }

        public static List<Entity> ResolveAllEnemiesTargets(SkillExecutionRequest request, SkillEffectDefinitionSO effect)
        {
            var targets = new List<Entity>();
            if (request == null || effect == null) return targets;
            if (BattleManager.Instance == null || BattleManager.Instance.ActiveMonsters == null) return targets;

            var activeMonsters = BattleManager.Instance.ActiveMonsters;
            var validMonsters = new List<Monster>();

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (IsValidEncounterMonster(request.Source, m))
                {
                    validMonsters.Add(m);
                }
            }

            if (validMonsters.Count == 0) return targets;

            // Determine primary target: request.Target or source.CurrentTarget or BattleManager.CurrentMonster or first living
            Monster primary = null;
            if (request.Target is Monster m1 && validMonsters.Contains(m1)) primary = m1;
            else if (request.Source.CurrentTarget is Monster m2 && validMonsters.Contains(m2)) primary = m2;
            else if (BattleManager.Instance.CurrentMonster is Monster m3 && validMonsters.Contains(m3)) primary = m3;
            else primary = validMonsters[0];

            targets.Add(primary);

            var remaining = new List<Monster>();
            float refX = primary.transform.position.x;
            float refZ = primary.transform.position.z;

            for (int i = 0; i < validMonsters.Count; i++)
            {
                var m = validMonsters[i];
                if (m != primary) remaining.Add(m);
            }

            remaining.Sort((a, b) =>
            {
                float dxA = a.transform.position.x - refX;
                float dzA = a.transform.position.z - refZ;
                float distA = dxA * dxA + dzA * dzA;

                float dxB = b.transform.position.x - refX;
                float dzB = b.transform.position.z - refZ;
                float distB = dxB * dxB + dzB * dzB;

                int distCompare = distA.CompareTo(distB);
                if (distCompare != 0) return distCompare;

                int orderA = BattleManager.Instance.GetRegistrationOrder(a);
                int orderB = BattleManager.Instance.GetRegistrationOrder(b);
                return orderA.CompareTo(orderB);
            });

            for (int i = 0; i < remaining.Count; i++)
            {
                var c = remaining[i];
                if (!targets.Contains(c))
                {
                    targets.Add(c);
                }
            }

            if (effect.MaxTargetCount > 0 && targets.Count > effect.MaxTargetCount)
            {
                targets = targets.GetRange(0, effect.MaxTargetCount);
            }

            return targets;
        }
    }

    public class DefaultSkillTargetResolver : ISkillTargetResolver
    {
        public List<Entity> ResolveTargets(SkillExecutionRequest request, SkillEffectDefinitionSO effect)
        {
            var targets = new List<Entity>();
            if (request == null || request.Source == null) return targets;

            SkillTargetPolicy policy = effect != null ? effect.TargetPolicy : SkillTargetPolicy.SingleTarget;

            switch (policy)
            {
                case SkillTargetPolicy.Self:
                    if (request.Source.IsAlive)
                    {
                        targets.Add(request.Source);
                    }
                    break;

                case SkillTargetPolicy.Area:
                    if (CombatTargetQuery.TryGetValidAreaAnchor(request, out var anchor))
                    {
                        targets = CombatTargetQuery.ResolveAreaTargets(request, effect, anchor);
                    }
                    break;

                case SkillTargetPolicy.AllEnemies:
                    if (CombatTargetQuery.HasLivingRegisteredMonster(request.Source))
                    {
                        targets = CombatTargetQuery.ResolveAllEnemiesTargets(request, effect);
                    }
                    break;

                case SkillTargetPolicy.SingleTarget:
                default:
                    Entity resolved = null;
                    if (request.Target != null && request.Target.gameObject.activeInHierarchy && request.Target.IsAlive)
                    {
                        resolved = request.Target;
                    }
                    else if (effect != null && (effect.EffectType == SkillEffectType.Heal || effect.EffectType == SkillEffectType.Buff || effect.EffectType == SkillEffectType.Cleanse || effect.EffectType == SkillEffectType.Shield))
                    {
                        if (request.Source != null && request.Source.gameObject.activeInHierarchy && request.Source.IsAlive)
                        {
                            resolved = request.Source;
                        }
                    }
                    else if (request.Source.CurrentTarget != null && request.Source.CurrentTarget.gameObject.activeInHierarchy && request.Source.CurrentTarget.IsAlive)
                    {
                        resolved = request.Source.CurrentTarget;
                    }
                    else
                    {
                        resolved = new NearestEnemyTargetResolver().ResolveTarget(request.Source);
                        if (resolved != null && request.Source is Hero hero)
                        {
                            hero.SetCurrentTarget(resolved);
                        }
                    }

                    if (resolved != null && resolved.IsAlive)
                    {
                        targets.Add(resolved);
                    }
                    break;
            }

            return targets;
        }
    }

    /// <summary>
    /// Preserves the original target snapshot captured at skill execution start.
    /// For Channel skills:
    /// - Snapshot is eagerly populated before channel start.
    /// - Each channel pulse reads the existing snapshot only; never re-resolves or appends new monsters.
    /// - Dead or inactive preserved targets are skipped dynamically on each pulse.
    /// </summary>
    public class PreservedTargetResolver : ISkillTargetResolver
    {
        private readonly Dictionary<SkillEffectDefinitionSO, List<Entity>> _snapshots = new();

        public PreservedTargetResolver(SkillExecutionRequest request, List<SkillEffectDefinitionSO> effects = null)
        {
            if (request == null) return;
            var effectsToCapture = effects ?? (request.Skill != null ? EffectResolver.ResolveEffectsForSkill(request.Skill) : null);
            if (effectsToCapture == null) return;

            var defaultResolver = new DefaultSkillTargetResolver();
            foreach (var eff in effectsToCapture)
            {
                if (eff == null) continue;
                var resolved = defaultResolver.ResolveTargets(request, eff);
                _snapshots[eff] = new List<Entity>(resolved ?? new List<Entity>());
            }
        }

        public PreservedTargetResolver(Dictionary<SkillEffectDefinitionSO, List<Entity>> initialSnapshots)
        {
            if (initialSnapshots != null)
            {
                foreach (var kvp in initialSnapshots)
                {
                    _snapshots[kvp.Key] = new List<Entity>(kvp.Value);
                }
            }
        }

        public IReadOnlyDictionary<SkillEffectDefinitionSO, List<Entity>> Snapshots => _snapshots;

        public List<Entity> ResolveTargets(SkillExecutionRequest request, SkillEffectDefinitionSO effect)
        {
            var targets = new List<Entity>();
            if (request == null || request.Source == null) return targets;

            SkillTargetPolicy policy = effect != null ? effect.TargetPolicy : SkillTargetPolicy.SingleTarget;

            if (policy == SkillTargetPolicy.Self)
            {
                if (request.Source.IsAlive)
                {
                    targets.Add(request.Source);
                }
                return targets;
            }

            // If snapshot exists for this effect, consume preserved snapshot:
            // Each channel pulse uses preserved order; skip dead or inactive members; do not append new spawns
            if (effect != null && _snapshots.TryGetValue(effect, out var snapshotList))
            {
                for (int i = 0; i < snapshotList.Count; i++)
                {
                    var t = snapshotList[i];
                    if (t != null && t.gameObject.activeInHierarchy && t.IsAlive &&
                        t.Health != null && t.Health.CurrentHealth > 0f)
                    {
                        targets.Add(t);
                    }
                }
                return targets;
            }

            // Fallback for SingleTarget if no pre-captured snapshot exists
            if (policy == SkillTargetPolicy.SingleTarget)
            {
                if (request.Target != null && request.Target.gameObject.activeInHierarchy && request.Target.IsAlive &&
                    request.Target.Health != null && request.Target.Health.CurrentHealth > 0f)
                {
                    targets.Add(request.Target);
                }
                return targets;
            }

            return targets;
        }
    }
}
