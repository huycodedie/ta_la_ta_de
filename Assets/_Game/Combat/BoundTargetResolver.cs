using System.Collections.Generic;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    /// <summary>
    /// P09-A: Strict fail-closed target resolver for projectile impact.
    /// Preserves the explicitly bound target and fails closed (returns empty list)
    /// if the target is null, dead, inactive, destroyed, or no longer registered in the active encounter.
    /// Evaluated fresh before each effect.
    /// NEVER falls back to CurrentTarget or acquires a replacement target.
    /// </summary>
    public class BoundTargetResolver : ISkillTargetResolver
    {
        private readonly Entity _boundTarget;
        private readonly BattleManager _boundBattleManager;
        private readonly int _boundEncounterIndex;

        public BoundTargetResolver(Entity boundTarget, BattleManager boundBattleManager = null, int boundEncounterIndex = -1)
        {
            _boundTarget = boundTarget;
            _boundBattleManager = boundBattleManager;
            _boundEncounterIndex = boundEncounterIndex;
        }

        public List<Entity> ResolveTargets(SkillExecutionRequest request, SkillEffectDefinitionSO effect)
        {
            if (effect == null || effect.TargetPolicy != SkillTargetPolicy.SingleTarget)
            {
                return new List<Entity>();
            }

            if (_boundTarget == null || !_boundTarget.gameObject.activeInHierarchy || !_boundTarget.IsAlive)
            {
                return new List<Entity>();
            }

            if (_boundTarget.Health == null || _boundTarget.Health.CurrentHealth <= 0f)
            {
                return new List<Entity>();
            }

            var currentBm = BattleManager.Instance;
            if (currentBm == null)
            {
                return new List<Entity>();
            }

            if (_boundBattleManager != null && (currentBm != _boundBattleManager || (_boundEncounterIndex >= 0 && currentBm.EncounterIndex != _boundEncounterIndex)))
            {
                return new List<Entity>();
            }

            if (!ProjectileController.IsEntityInEncounter(currentBm, _boundTarget))
            {
                return new List<Entity>();
            }

            if (request != null && request.Source != null && !ProjectileController.IsEntityInEncounter(currentBm, request.Source))
            {
                return new List<Entity>();
            }

            return new List<Entity> { _boundTarget };
        }
    }
}
