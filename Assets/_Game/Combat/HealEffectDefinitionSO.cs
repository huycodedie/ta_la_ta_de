using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    public enum HealAmountMode
    {
        FlatValue = 1,
        MaxHpPercentage = 2,
        AttackMultiplier = 3
    }

    [CreateAssetMenu(fileName = "HealEffect", menuName = "WuxiaGame/Combat/Effects/HealEffect")]
    public class HealEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Heal Configuration")]
        [SerializeField] private HealAmountMode healMode = HealAmountMode.FlatValue;
        [SerializeField] private float healValue = 50f;

        public HealAmountMode HealMode => healMode;
        public float HealValue => healValue;

        private void Reset()
        {
            effectType = SkillEffectType.Heal;
            targetPolicy = SkillTargetPolicy.Self;
        }

        public void Initialize(
            float value,
            HealAmountMode mode = HealAmountMode.FlatValue,
            SkillTargetPolicy policy = SkillTargetPolicy.Self)
        {
            effectType = SkillEffectType.Heal;
            targetPolicy = policy;
            healValue = value;
            healMode = mode;
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
                    SkillEffectType.Heal,
                    target,
                    $"Heal target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            float requestedAmount = 0f;
            switch (healMode)
            {
                case HealAmountMode.FlatValue:
                    requestedAmount = Mathf.Max(0f, healValue);
                    break;

                case HealAmountMode.MaxHpPercentage:
                    requestedAmount = Mathf.Max(0f, (healValue / 100f) * target.Health.MaxHealth);
                    break;

                case HealAmountMode.AttackMultiplier:
                    float atk = request?.Source != null ? request.Source.Stats.GetValue(StatType.Attack, 10f) : 10f;
                    requestedAmount = Mathf.Max(0f, healValue * atk);
                    break;
            }

            float prevHp = target.Health.CurrentHealth;
            float actualHealed = target.Health.Heal(requestedAmount);
            float curHp = target.Health.CurrentHealth;

            Debug.Log($"[EFFECT:HEAL] Target={target.EntityName}, Requested={requestedAmount:F1}, Actual={actualHealed:F1}, HP: {prevHp:F1}->{curHp:F1}/{target.Health.MaxHealth:F1}");

            return SkillEffectExecutionResult.CreateHealSuccess(
                target,
                requestedAmount,
                actualHealed,
                prevHp,
                curHp,
                request?.Source);
        }
    }
}
