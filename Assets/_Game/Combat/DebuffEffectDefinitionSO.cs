using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "DebuffEffect", menuName = "WuxiaGame/Combat/Effects/DebuffEffect")]
    public class DebuffEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Debuff Configuration")]
        [SerializeField] private string debuffId = "debuff_atk_down";
        [SerializeField] private string debuffName = "Giảm Công Kích";
        [SerializeField] private DebuffType debuffType = DebuffType.Attack;
        [SerializeField] private DebuffModifierMode modifierMode = DebuffModifierMode.Flat;
        [SerializeField] private float modifierValue = 20f;
        [SerializeField] private float duration = 5f;
        [SerializeField] private EffectPowerTier powerTier = EffectPowerTier.TierB;
        [SerializeField] private StatusStackPolicy stackingPolicy = StatusStackPolicy.RefreshDuration;
        [SerializeField] private int maxStacks = 5;
        [SerializeField] private int removalPriority = 0;

        public string DebuffId => debuffId;
        public string DebuffName => debuffName;
        public DebuffType DebuffType => debuffType;
        public DebuffModifierMode ModifierMode => modifierMode;
        public float ModifierValue => modifierValue;
        public float Duration => duration;
        public EffectPowerTier PowerTier => powerTier;
        public StatusStackPolicy StackingPolicy => stackingPolicy;
        public int MaxStacks => maxStacks;
        public int RemovalPriority => removalPriority > 0 ? removalPriority : (int)powerTier;

        public void SetRemovalPriority(int priority) => removalPriority = priority;

        private void Reset()
        {
            effectType = SkillEffectType.Debuff;
            targetPolicy = SkillTargetPolicy.SingleTarget;
        }

        public void Initialize(
            string id,
            string name,
            DebuffType type,
            float value,
            DebuffModifierMode mode = DebuffModifierMode.Flat,
            float dur = 5f,
            EffectPowerTier tier = EffectPowerTier.TierB,
            StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration,
            SkillTargetPolicy policy = SkillTargetPolicy.SingleTarget,
            int maxStackCount = 5)
        {
            effectType = SkillEffectType.Debuff;
            targetPolicy = policy;
            debuffId = id;
            debuffName = name;
            debuffType = type;
            modifierValue = value;
            modifierMode = mode;
            duration = dur;
            powerTier = tier;
            stackingPolicy = stackPolicy;
            maxStacks = maxStackCount;
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
                    SkillEffectType.Debuff,
                    target,
                    $"Debuff target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            var statusController = target.GetComponent<EntityStatusController>();
            if (statusController == null)
            {
                statusController = target.gameObject.AddComponent<EntityStatusController>();
                statusController.Initialize(target);
            }

            statusController.ApplyDebuff(this, request?.Source);

            Debug.Log($"[EFFECT:DEBUFF] Target={target.EntityName}, Debuff={debuffId}, Type={debuffType}, Value=-{modifierValue:F1} ({modifierMode}), Dur={duration:F1}s, Tier={powerTier}");

            return new SkillEffectExecutionResult(
                SkillEffectType.Debuff,
                target,
                true,
                $"Debuff {debuffId} applied successfully.",
                null,
                1f,
                request?.Source,
                modifierValue,
                modifierValue,
                0f,
                0f,
                null,
                duration);
        }
    }
}
