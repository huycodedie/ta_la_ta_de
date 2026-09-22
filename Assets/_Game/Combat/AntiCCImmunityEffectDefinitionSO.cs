using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "AntiCCImmunityEffect", menuName = "WuxiaGame/Combat/Effects/AntiCCImmunityEffect")]
    public class AntiCCImmunityEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Anti-CC Immunity Configuration")]
        [SerializeField] private string immunityId = "anti_cc_immunity";
        [SerializeField] private string immunityName = "Miễn Nhiễm Khống Chế";
        [SerializeField] private float duration = 5f;
        [SerializeField] private EffectPowerTier powerTier = EffectPowerTier.TierB;

        public string ImmunityId => immunityId;
        public string ImmunityName => immunityName;
        public float Duration => duration;
        public EffectPowerTier PowerTier => powerTier;

        private void Reset()
        {
            effectType = SkillEffectType.Buff;
            targetPolicy = SkillTargetPolicy.Self;
        }

        public void Initialize(
            string id,
            string name,
            float dur = 5f,
            EffectPowerTier tier = EffectPowerTier.TierB,
            SkillTargetPolicy policy = SkillTargetPolicy.Self)
        {
            effectType = SkillEffectType.Buff;
            targetPolicy = policy;
            immunityId = id;
            immunityName = name;
            duration = dur;
            powerTier = tier;
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
                    $"Anti-CC target '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.",
                    request?.Source);
            }

            var statusController = target.GetComponent<EntityStatusController>();
            if (statusController == null)
            {
                statusController = target.gameObject.AddComponent<EntityStatusController>();
                statusController.Initialize(target);
            }

            statusController.ApplyAntiCCImmunity(duration, request?.Source, immunityId);

            Debug.Log($"[EFFECT:ANTI_CC] Target={target.EntityName}, ID={immunityId}, Dur={duration:F1}s, Tier={powerTier}");

            return new SkillEffectExecutionResult(
                SkillEffectType.Buff,
                target,
                true,
                $"Anti-CC Immunity applied successfully for {duration:F1}s.",
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
