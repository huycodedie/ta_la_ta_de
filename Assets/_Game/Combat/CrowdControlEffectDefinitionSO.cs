using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "CrowdControlEffect", menuName = "WuxiaGame/Combat/Effects/CrowdControlEffect")]
    public class CrowdControlEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Crowd Control Configuration")]
        [SerializeField] private string ccId = "cc_stun";
        [SerializeField] private string ccName = "Choáng";
        [SerializeField] private CrowdControlType ccType = CrowdControlType.Stun;
        [SerializeField] private float duration = 2f;
        [SerializeField] private EffectPowerTier powerTier = EffectPowerTier.TierB;
        [SerializeField] private bool canShatterFreeze = false;
        [SerializeField] private StatusStackPolicy stackingPolicy = StatusStackPolicy.RefreshDuration;
        [SerializeField] private int removalPriority = 0;

        public string CcId => ccId;
        public string CcName => ccName;
        public CrowdControlType CcType => ccType;
        public float Duration => duration;
        public EffectPowerTier PowerTier => powerTier;
        public bool CanShatterFreeze => canShatterFreeze;
        public StatusStackPolicy StackingPolicy => stackingPolicy;
        public int RemovalPriority => removalPriority > 0 ? removalPriority : (int)powerTier;

        public void SetRemovalPriority(int priority) => removalPriority = priority;

        private void Reset()
        {
            effectType = SkillEffectType.CrowdControl;
            targetPolicy = SkillTargetPolicy.SingleTarget;
        }

        public void Initialize(
            string id,
            string name,
            CrowdControlType type,
            float dur = 2f,
            EffectPowerTier tier = EffectPowerTier.TierB,
            bool shatterFreeze = false,
            StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration,
            SkillTargetPolicy policy = SkillTargetPolicy.SingleTarget)
        {
            effectType = SkillEffectType.CrowdControl;
            targetPolicy = policy;
            ccId = id;
            ccName = name;
            ccType = type;
            duration = dur;
            powerTier = tier;
            canShatterFreeze = shatterFreeze;
            stackingPolicy = stackPolicy;
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
                    SkillEffectType.CrowdControl,
                    target,
                    $"CC target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            var statusController = target.GetComponent<EntityStatusController>();
            if (statusController == null)
            {
                statusController = target.gameObject.AddComponent<EntityStatusController>();
                statusController.Initialize(target);
            }

            bool applied = statusController.ApplyCrowdControl(this, request?.Source, out string applyReason);

            Debug.Log($"[EFFECT:CC] Target={target.EntityName}, CC={ccId}, Type={ccType}, Dur={duration:F1}s, Tier={powerTier}, Applied={applied}, Reason={applyReason}");

            if (!applied)
            {
                return SkillEffectExecutionResult.CreateFailure(
                    SkillEffectType.CrowdControl,
                    target,
                    $"CC {ccType} blocked or resisted: {applyReason}",
                    request?.Source);
            }

            return new SkillEffectExecutionResult(
                SkillEffectType.CrowdControl,
                target,
                true,
                $"CC {ccType} applied successfully.",
                null,
                1f,
                request?.Source,
                0f,
                0f,
                0f,
                0f,
                null,
                duration);
        }
    }
}
