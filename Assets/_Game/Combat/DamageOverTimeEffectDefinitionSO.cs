using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "DamageOverTimeEffect", menuName = "WuxiaGame/Combat/Effects/DamageOverTimeEffect")]
    public class DamageOverTimeEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("DoT Configuration")]
        [SerializeField] private string dotId = "status_dot";
        [SerializeField] private string dotName = "Sát Thương Liên Tục";
        [SerializeField] private float damagePerTick = 10f;
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private float duration = 5f;
        [SerializeField] private int maxStacks = 3;
        [SerializeField] private StatusStackPolicy stackPolicy = StatusStackPolicy.Stack;
        [SerializeField] private DamageType damageType = DamageType.Skill;
        [SerializeField] private StatusCategory category = StatusCategory.Dot;
        [SerializeField] private StatusEffectDefinitionSO statusDefinition;

        public string DotId => dotId;
        public string DotName => dotName;
        public float DamagePerTick => damagePerTick;
        public float TickInterval => tickInterval;
        public float Duration => duration;
        public int MaxStacks => maxStacks;
        public StatusStackPolicy StackPolicy => stackPolicy;
        public DamageType DamageType => damageType;
        public StatusCategory Category => category;
        public StatusEffectDefinitionSO StatusDefinition => statusDefinition;

        private void Reset()
        {
            effectType = SkillEffectType.Debuff;
            targetPolicy = SkillTargetPolicy.SingleTarget;
        }

        public void Initialize(
            string id,
            string name,
            float dmgPerTick,
            float interval = 1f,
            float dur = 5f,
            int stacks = 3,
            StatusStackPolicy policy = StatusStackPolicy.Stack,
            SkillTargetPolicy targetPol = SkillTargetPolicy.SingleTarget,
            DamageType dmgType = DamageType.Skill,
            StatusCategory cat = StatusCategory.Dot,
            StatusEffectDefinitionSO explicitStatusDef = null)
        {
            effectType = SkillEffectType.Debuff;
            targetPolicy = targetPol;
            dotId = id;
            dotName = name;
            damagePerTick = dmgPerTick;
            tickInterval = interval;
            duration = dur;
            maxStacks = stacks;
            stackPolicy = policy;
            damageType = dmgType;
            category = cat;
            statusDefinition = explicitStatusDef;
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
                    $"DoT target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            var statusController = target.GetComponent<EntityStatusController>();
            if (statusController == null)
            {
                statusController = target.gameObject.AddComponent<EntityStatusController>();
                statusController.Initialize(target);
            }

            StatusEffectDefinitionSO def = statusDefinition;
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<StatusEffectDefinitionSO>();
                def.Initialize(
                    dotId,
                    dotName,
                    duration,
                    tickInterval,
                    maxStacks,
                    stackPolicy,
                    category,
                    damagePerTick,
                    damageType,
                    combatConfig);
            }

            statusController.ApplyStatus(def, request?.Source);

            Debug.Log($"[EFFECT:DOT] Applied to {target.EntityName}: ID={def.StatusId}, DmgPerTick={damagePerTick:F1}, Interval={tickInterval:F1}s, Dur={duration:F1}s, Stacks={maxStacks}, Policy={stackPolicy}");

            return SkillEffectExecutionResult.CreateDotSuccess(
                target,
                def.StatusId,
                damagePerTick,
                tickInterval,
                duration,
                request?.Source);
        }
    }
}
