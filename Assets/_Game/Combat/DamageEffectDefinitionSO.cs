using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "WuxiaGame/Combat/Effects/DamageEffect")]
    public class DamageEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Damage Configuration")]
        [SerializeField] private float damageMultiplier = 1.0f;
        [SerializeField] private DamageType damageType = DamageType.Skill;

        public float DamageMultiplier => damageMultiplier;
        public DamageType DamageType => damageType;

        private void Reset()
        {
            effectType = SkillEffectType.Damage;
            targetPolicy = SkillTargetPolicy.SingleTarget;
        }

        public void Initialize(
            float multiplier,
            SkillTargetPolicy policy = SkillTargetPolicy.SingleTarget,
            DamageType dmgType = DamageType.Skill)
        {
            effectType = SkillEffectType.Damage;
            targetPolicy = policy;
            damageMultiplier = multiplier;
            damageType = dmgType;
        }

        public override SkillEffectExecutionResult Execute(
            SkillExecutionRequest request,
            Entity target,
            CombatConfigSO combatConfig)
        {
            if (target == null || !target.gameObject.activeInHierarchy || !target.IsAlive ||
                target.Health == null || target.Health.CurrentHealth <= 0f)
            {
                return SkillEffectExecutionResult.CreateFailure(
                    SkillEffectType.Damage,
                    target,
                    $"Target entity '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.");
            }

            bool shatter = (request != null && request.Skill != null && request.Skill.CanShatterFreeze);

            // Calls authoritative DamageCalculator
            DamageResult damageResult = DamageCalculator.CalculateDamage(
                request.Source,
                target,
                damageMultiplier,
                combatConfig,
                damageType,
                shatter);

            // Applies to authoritative HealthComponent
            target.Health.TakeDamage(damageResult);

            // Raises existing event for visual feedback, UI popups, etc.
            EventBus.RaiseEntityDamaged(target, damageResult);

            Debug.Log($"[EFFECT:DAMAGE] Execute: Skill={request.Skill?.SkillName ?? "Unknown"}, Target={target.EntityName}, Mult={damageMultiplier:F2}, Raw={damageResult.RawDamage:F1}, Final={damageResult.FinalDamage:F1}, Crit={damageResult.IsCrit}");

            return SkillEffectExecutionResult.CreateDamageSuccess(target, damageResult, damageMultiplier);
        }
    }
}
