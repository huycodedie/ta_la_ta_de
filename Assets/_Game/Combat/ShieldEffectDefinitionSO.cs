using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    public enum ShieldAmountMode
    {
        FlatValue = 1,
        MaxHpPercentage = 2,
        AttackMultiplier = 3
    }

    [CreateAssetMenu(fileName = "ShieldEffect", menuName = "WuxiaGame/Combat/Effects/ShieldEffect")]
    public class ShieldEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Shield Configuration")]
        [SerializeField] private string shieldId = "shield_default";
        [SerializeField] private ShieldAmountMode amountMode = ShieldAmountMode.FlatValue;
        [SerializeField] private float shieldValue = 50f;
        [SerializeField] private float duration = 5f;
        [SerializeField] private int priority = 0;
        [SerializeField] private ShieldStackPolicy stackPolicy = ShieldStackPolicy.RefreshDuration;
        [SerializeField] private int maxStacks = 1;
        [SerializeField] private float maxAmount = -1f;

        public string ShieldId => shieldId;
        public ShieldAmountMode AmountMode => amountMode;
        public float ShieldValue => shieldValue;
        public float Duration => duration;
        public int Priority => priority;
        public ShieldStackPolicy StackingPolicy => stackPolicy;
        public int MaxStacks => maxStacks;
        public float MaxAmount => maxAmount;

        private void Reset()
        {
            effectType = SkillEffectType.Shield;
            targetPolicy = SkillTargetPolicy.Self;
        }

        public void Initialize(
            string id,
            float value,
            float dur = 5f,
            int prio = 0,
            ShieldStackPolicy policy = ShieldStackPolicy.RefreshDuration,
            ShieldAmountMode mode = ShieldAmountMode.FlatValue,
            SkillTargetPolicy targetPol = SkillTargetPolicy.Self,
            int maxStk = 1,
            float maxCap = -1f)
        {
            effectType = SkillEffectType.Shield;
            targetPolicy = targetPol;
            shieldId = id;
            shieldValue = value;
            duration = dur;
            priority = prio;
            stackPolicy = policy;
            amountMode = mode;
            maxStacks = maxStk;
            maxAmount = maxCap;
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
                    SkillEffectType.Shield,
                    target,
                    $"Shield target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            if (target.StatusController == null)
            {
                return SkillEffectExecutionResult.CreateFailure(
                    SkillEffectType.Shield,
                    target,
                    $"Target '{target.EntityName}' has no StatusController.",
                    request?.Source);
            }

            float calculatedAmount = 0f;
            switch (amountMode)
            {
                case ShieldAmountMode.FlatValue:
                    calculatedAmount = Mathf.Max(0f, shieldValue);
                    break;

                case ShieldAmountMode.MaxHpPercentage:
                    calculatedAmount = Mathf.Max(0f, (shieldValue / 100f) * target.Health.MaxHealth);
                    break;

                case ShieldAmountMode.AttackMultiplier:
                    float atk = request?.Source != null ? request.Source.Stats.GetValue(StatType.Attack, 10f) : 10f;
                    calculatedAmount = Mathf.Max(0f, shieldValue * atk);
                    break;
            }

            bool applied = target.StatusController.ApplyShield(
                shieldId,
                calculatedAmount,
                duration,
                priority,
                stackPolicy,
                request?.Source,
                maxStacks,
                maxAmount,
                this);

            if (applied)
            {
                Debug.Log($"[EFFECT:SHIELD] Applied: ID={shieldId}, Target={target.EntityName}, Amount={calculatedAmount:F1}, Dur={duration:F1}s, Priority={priority}");
                return SkillEffectExecutionResult.CreateShieldSuccess(
                    target,
                    shieldId,
                    calculatedAmount,
                    duration,
                    request?.Source);
            }
            else
            {
                return SkillEffectExecutionResult.CreateFailure(
                    SkillEffectType.Shield,
                    target,
                    $"Failed to apply shield '{shieldId}' to '{target.EntityName}'.",
                    request?.Source);
            }
        }
    }
}
