using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "BuffEffect", menuName = "WuxiaGame/Combat/Effects/BuffEffect")]
    public class BuffEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Buff Configuration")]
        [SerializeField] private string buffId = "buff_atk";
        [SerializeField] private string buffName = "Tăng Công Kích";
        [SerializeField] private StatType statType = StatType.Attack;
        [SerializeField] private BuffModifierMode modifierMode = BuffModifierMode.Additive;
        [SerializeField] private float modifierValue = 20f;
        [SerializeField] private float duration = 10f;
        [SerializeField] private BuffStackingPolicy stackingPolicy = BuffStackingPolicy.RefreshDuration;
        [SerializeField] private int maxStacks = 5;
        [SerializeField] private int removalPriority = 0;

        public string BuffId => buffId;
        public string BuffName => buffName;
        public StatType StatType => statType;
        public BuffModifierMode ModifierMode => modifierMode;
        public float ModifierValue => modifierValue;
        public float Duration => duration;
        public BuffStackingPolicy StackingPolicy => stackingPolicy;
        public int MaxStacks => maxStacks;
        public int RemovalPriority => removalPriority;

        public void SetRemovalPriority(int priority) => removalPriority = priority;

        private void Reset()
        {
            effectType = SkillEffectType.Buff;
            targetPolicy = SkillTargetPolicy.Self;
        }

        public void Initialize(
            string id,
            string name,
            StatType stat,
            float value,
            float dur = 10f,
            BuffStackingPolicy stackPolicy = BuffStackingPolicy.RefreshDuration,
            SkillTargetPolicy policy = SkillTargetPolicy.Self,
            int maxStackCount = 5)
        {
            effectType = SkillEffectType.Buff;
            targetPolicy = policy;
            buffId = id;
            buffName = name;
            statType = stat;
            modifierValue = value;
            duration = dur;
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
                    SkillEffectType.Buff,
                    target,
                    $"Buff target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            var statusController = target.GetComponent<EntityStatusController>();
            if (statusController == null)
            {
                statusController = target.gameObject.AddComponent<EntityStatusController>();
                statusController.Initialize(target);
            }

            statusController.ApplyBuff(this, request?.Source);

            Debug.Log($"[EFFECT:BUFF] Target={target.EntityName}, Buff={buffId}, Stat={statType}, Value=+{modifierValue:F1}, Dur={duration:F1}s");

            return SkillEffectExecutionResult.CreateBuffSuccess(
                target,
                statType,
                modifierValue,
                duration,
                request?.Source);
        }
    }
}
