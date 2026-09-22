using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public interface IEffectResolver
    {
        List<SkillEffectExecutionResult> ProcessEffects(
            SkillExecutionRequest request,
            IReadOnlyList<SkillEffectDefinitionSO> effects,
            ISkillTargetResolver targetResolver = null);
    }

    public class EffectResolver : IEffectResolver
    {
        private static IEffectResolver defaultInstance = new EffectResolver();
        public static IEffectResolver Instance
        {
            get => defaultInstance;
            set => defaultInstance = value ?? new EffectResolver();
        }

        public List<SkillEffectExecutionResult> ProcessEffects(
            SkillExecutionRequest request,
            IReadOnlyList<SkillEffectDefinitionSO> effects,
            ISkillTargetResolver targetResolver = null)
        {
            var results = new List<SkillEffectExecutionResult>();
            if (effects == null || effects.Count == 0)
            {
                return results;
            }

            targetResolver ??= new DefaultSkillTargetResolver();

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                List<Entity> targets = targetResolver.ResolveTargets(request, effect);
                if (targets == null || targets.Count == 0)
                {
                    results.Add(SkillEffectExecutionResult.CreateFailure(
                        effect.EffectType,
                        null,
                        "No valid target resolved for effect."));
                    continue;
                }

                foreach (var target in targets)
                {
                    var effectResult = effect.Execute(request, target, request?.CombatConfig);
                    results.Add(effectResult);
                }
            }

            return results;
        }

        public static List<SkillEffectDefinitionSO> ResolveEffectsForSkill(SkillDefinitionSO skill)
        {
            if (skill == null) return new List<SkillEffectDefinitionSO>();

            // 1. If effects list has valid elements, use them
            if (skill.Effects != null && skill.Effects.Count > 0)
            {
                var validEffects = new List<SkillEffectDefinitionSO>();
                foreach (var eff in skill.Effects)
                {
                    if (eff != null) validEffects.Add(eff);
                }
                if (validEffects.Count > 0)
                {
                    return validEffects;
                }
            }

            // 2. If explicitly marked as having zero effects
            if (skill.HasExplicitEffects && skill.DamageMultiplier <= 0f)
            {
                return new List<SkillEffectDefinitionSO>();
            }

            // 3. Fallback for skills configured with damageMultiplier > 0
            if (!skill.IsPassive && skill.DamageMultiplier > 0f)
            {
                var legacyEffect = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                legacyEffect.Initialize(skill.DamageMultiplier, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                return new List<SkillEffectDefinitionSO> { legacyEffect };
            }

            return new List<SkillEffectDefinitionSO>();
        }
    }
}
