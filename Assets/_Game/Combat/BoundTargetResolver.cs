using System.Collections.Generic;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    /// <summary>
    /// P09-A: Strict fail-closed target resolver for projectile impact.
    /// Preserves the explicitly bound target and fails closed (returns empty list)
    /// if the target is null, dead, inactive, or destroyed.
    /// NEVER falls back to CurrentTarget or acquires a replacement target.
    /// </summary>
    public class BoundTargetResolver : ISkillTargetResolver
    {
        private readonly Entity _boundTarget;

        public BoundTargetResolver(Entity boundTarget)
        {
            _boundTarget = boundTarget;
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

            return new List<Entity> { _boundTarget };
        }
    }
}
