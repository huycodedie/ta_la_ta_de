using UnityEngine;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public class RuntimeDebuffInstance
    {
        public string DebuffId { get; }
        public DebuffType DebuffType { get; }
        public DebuffModifierMode ModifierMode { get; }
        public float ModifierValue { get; }
        public EffectPowerTier PowerTier { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public float StartTime { get; private set; }
        public float Duration { get; private set; }
        public float ExpirationTime { get; private set; }
        public float EndTime => ExpirationTime;
        public int StackCount { get; private set; }
        public int MaxStacks { get; }
        public StatusStackPolicy StackPolicy { get; }
        public int RemovalPriority { get; set; }

        public float TotalValue => ModifierValue * StackCount;

        public bool IsExpired
        {
            get
            {
                if (Duration <= 0f) return false;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return now >= ExpirationTime;
            }
        }

        public bool IsExpiredAt(float time) => Duration > 0f && time >= ExpirationTime;

        public float RemainingDuration
        {
            get
            {
                if (Duration <= 0f) return 0f;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return Mathf.Max(0f, ExpirationTime - now);
            }
        }

        public RuntimeDebuffInstance(
            string debuffId,
            DebuffType debuffType,
            DebuffModifierMode modifierMode,
            float modifierValue,
            float duration,
            EffectPowerTier powerTier,
            Entity source,
            Entity target,
            float currentTime,
            StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration,
            int maxStacks = 5)
        {
            DebuffId = debuffId ?? string.Empty;
            DebuffType = debuffType;
            ModifierMode = modifierMode;
            ModifierValue = modifierValue;
            Duration = duration;
            PowerTier = powerTier;
            Source = source;
            Target = target;
            StartTime = currentTime;
            ExpirationTime = duration > 0f ? currentTime + duration : float.MaxValue;
            StackPolicy = stackPolicy;
            MaxStacks = Mathf.Max(1, maxStacks);
            StackCount = 1;
            RemovalPriority = (int)powerTier;
        }

        public int ReduceStacks(int count)
        {
            if (count <= 0) return StackCount;
            StackCount = Mathf.Max(0, StackCount - count);
            return StackCount;
        }

        public void RefreshDuration(float currentTime)
        {
            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
        }

        public void AddStack(float currentTime)
        {
            if (StackCount < MaxStacks)
            {
                StackCount++;
            }
            RefreshDuration(currentTime);
        }

        public void Replace(float currentTime)
        {
            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
            StackCount = 1;
        }
    }
}
