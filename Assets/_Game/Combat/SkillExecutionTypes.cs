using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public enum SkillExecutionFailureReason
    {
        None = 0,
        SourceInvalidOrDead = 1,
        SkillNotFound = 2,
        SkillNotFromActiveMindMethod = 3,
        SkillLocked = 4,
        SkillNotSelected = 5,
        TargetInvalidOrDead = 6,
        CooldownNotReady = 7,     // Reserved for P07.2
        InsufficientRage = 8,     // Reserved for P07.2
        SourceCrowdControlled = 9, // P07.6
        SourceAlreadyCasting = 10  // P07.9 Phase 2.1
    }

    public class SkillExecutionRequest
    {
        public Entity Source { get; }
        public SkillDefinitionSO Skill { get; }
        public SkillSlotType Slot { get; }
        public Entity Target { get; }
        public CombatConfigSO CombatConfig { get; }

        public SkillExecutionRequest(
            Entity source,
            SkillDefinitionSO skill,
            SkillSlotType slot,
            Entity target,
            CombatConfigSO combatConfig = null)
        {
            Source = source;
            Skill = skill;
            Slot = slot;
            Target = target;
            CombatConfig = combatConfig;
        }
    }

    public class SkillEffectExecutionResult
    {
        public SkillEffectType EffectType { get; }
        public Entity Target { get; }
        public Entity Source { get; }
        public bool Success { get; }
        public string Description { get; }
        public DamageResult? DamageResult { get; }
        public float Multiplier { get; }
        public float RequestedAmount { get; }
        public float ActualAmount { get; }
        public float PreviousHP { get; }
        public float CurrentHP { get; }
        public WuxiaGame.Stats.StatType? StatType { get; }
        public float Duration { get; }

        public SkillEffectExecutionResult(
            SkillEffectType effectType,
            Entity target,
            bool success,
            string description,
            DamageResult? damageResult = null,
            float multiplier = 1f,
            Entity source = null,
            float requestedAmount = 0f,
            float actualAmount = 0f,
            float previousHp = 0f,
            float currentHp = 0f,
            WuxiaGame.Stats.StatType? statType = null,
            float duration = 0f)
        {
            EffectType = effectType;
            Target = target;
            Source = source;
            Success = success;
            Description = description ?? string.Empty;
            DamageResult = damageResult;
            Multiplier = multiplier;
            RequestedAmount = requestedAmount;
            ActualAmount = actualAmount;
            PreviousHP = previousHp;
            CurrentHP = currentHp;
            StatType = statType;
            Duration = duration;
        }

        public static SkillEffectExecutionResult CreateDamageSuccess(
            Entity target,
            DamageResult damageResult,
            float multiplier = 1f,
            string desc = "Damage effect executed successfully.",
            Entity source = null)
        {
            return new SkillEffectExecutionResult(
                SkillEffectType.Damage,
                target,
                true,
                desc,
                damageResult,
                multiplier,
                source);
        }

        public static SkillEffectExecutionResult CreateHealSuccess(
            Entity target,
            float requestedAmount,
            float actualAmount,
            float previousHp,
            float currentHp,
            Entity source = null,
            string desc = "Heal effect executed successfully.")
        {
            return new SkillEffectExecutionResult(
                SkillEffectType.Heal,
                target,
                true,
                desc,
                null,
                1f,
                source,
                requestedAmount,
                actualAmount,
                previousHp,
                currentHp);
        }

        public static SkillEffectExecutionResult CreateBuffSuccess(
            Entity target,
            WuxiaGame.Stats.StatType statType,
            float modifierValue,
            float duration,
            Entity source = null,
            string desc = "Buff effect applied successfully.")
        {
            return new SkillEffectExecutionResult(
                SkillEffectType.Buff,
                target,
                true,
                desc,
                null,
                1f,
                source,
                modifierValue,
                modifierValue,
                0f,
                0f,
                statType,
                duration);
        }

        public static SkillEffectExecutionResult CreateStatusSuccess(
            Entity target,
            string statusId,
            float duration,
            Entity source = null,
            string desc = "Status effect applied successfully.")
        {
            return new SkillEffectExecutionResult(
                SkillEffectType.SpecialEffect,
                target,
                true,
                desc,
                null,
                1f,
                source,
                0f,
                0f,
                0f,
                0f,
                null,
                duration);
        }

        public static SkillEffectExecutionResult CreateDotSuccess(
            Entity target,
            string statusId,
            float damagePerTick,
            float tickInterval,
            float duration,
            Entity source = null,
            string desc = "Damage-over-time effect applied successfully.")
        {
            return new SkillEffectExecutionResult(
                SkillEffectType.Debuff,
                target,
                true,
                desc,
                null,
                1f,
                source,
                damagePerTick,
                damagePerTick,
                0f,
                0f,
                null,
                duration);
        }

        public static SkillEffectExecutionResult CreateShieldSuccess(
            Entity target,
            string shieldId,
            float shieldAmount,
            float duration,
            Entity source = null,
            string desc = "Shield effect applied successfully.")
        {
            return new SkillEffectExecutionResult(
                SkillEffectType.Shield,
                target,
                true,
                desc,
                null,
                1f,
                source,
                shieldAmount,
                shieldAmount,
                0f,
                0f,
                null,
                duration);
        }

        public static SkillEffectExecutionResult CreateFailure(
            SkillEffectType effectType,
            Entity target,
            string description,
            Entity source = null)
        {
            return new SkillEffectExecutionResult(
                effectType,
                target,
                false,
                description,
                null,
                0f,
                source);
        }
    }

    public class SkillExecutionResult
    {
        public bool Success { get; }
        public string SkillId { get; }
        public SkillSlotType Slot { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public SkillExecutionFailureReason FailureReason { get; }
        public string ReasonDescription { get; }
        public DamageResult? DamageResult { get; }
        public float RageBefore { get; }
        public float RageCost { get; }
        public float RageAfter { get; }
        public float CooldownDuration { get; }
        public float CooldownRemaining { get; }
        public System.Collections.Generic.IReadOnlyList<SkillEffectExecutionResult> EffectResults { get; }

        public SkillExecutionResult(
            bool success,
            string skillId,
            SkillSlotType slot,
            Entity source,
            Entity target,
            SkillExecutionFailureReason failureReason,
            string reasonDescription,
            DamageResult? damageResult = null,
            float rageBefore = 0f,
            float rageCost = 0f,
            float rageAfter = 0f,
            float cooldownDuration = 0f,
            float cooldownRemaining = 0f,
            System.Collections.Generic.List<SkillEffectExecutionResult> effectResults = null)
        {
            Success = success;
            SkillId = skillId ?? string.Empty;
            Slot = slot;
            Source = source;
            Target = target;
            FailureReason = failureReason;
            ReasonDescription = reasonDescription ?? string.Empty;
            DamageResult = damageResult;
            RageBefore = rageBefore;
            RageCost = rageCost;
            RageAfter = rageAfter;
            CooldownDuration = cooldownDuration;
            CooldownRemaining = cooldownRemaining;
            EffectResults = effectResults != null
                ? new System.Collections.Generic.List<SkillEffectExecutionResult>(effectResults)
                : new System.Collections.Generic.List<SkillEffectExecutionResult>();

            if (!DamageResult.HasValue && EffectResults.Count > 0)
            {
                foreach (var eff in EffectResults)
                {
                    if (eff.DamageResult.HasValue)
                    {
                        DamageResult = eff.DamageResult;
                        break;
                    }
                }
            }
        }

        public static SkillExecutionResult CreateSuccess(
            SkillExecutionRequest request,
            DamageResult? damageResult = null,
            string description = "Skill executed successfully.",
            float rageBefore = 0f,
            float rageCost = 0f,
            float rageAfter = 0f,
            float cooldownDuration = 0f,
            float cooldownRemaining = 0f,
            System.Collections.Generic.List<SkillEffectExecutionResult> effectResults = null)
        {
            return new SkillExecutionResult(
                true,
                request?.Skill != null ? request.Skill.SkillId : string.Empty,
                request != null ? request.Slot : SkillSlotType.NormalAttack,
                request?.Source,
                request?.Target,
                SkillExecutionFailureReason.None,
                description,
                damageResult,
                rageBefore,
                rageCost,
                rageAfter,
                cooldownDuration,
                cooldownRemaining,
                effectResults);
        }

        public static SkillExecutionResult CreateFailure(
            SkillExecutionRequest request,
            SkillExecutionFailureReason reason,
            string description,
            float currentRage = 0f,
            float cooldownRemaining = 0f,
            System.Collections.Generic.List<SkillEffectExecutionResult> effectResults = null)
        {
            return new SkillExecutionResult(
                false,
                request?.Skill != null ? request.Skill.SkillId : string.Empty,
                request != null ? request.Slot : SkillSlotType.NormalAttack,
                request?.Source,
                request?.Target,
                reason,
                description,
                null,
                currentRage,
                0f,
                currentRage,
                0f,
                cooldownRemaining,
                effectResults);
        }
    }
}
