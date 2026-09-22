using UnityEngine;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public class RuntimeStatusInstance
    {
        public string StatusId { get; }
        public StatusEffectDefinitionSO Definition { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public Entity SourceEntity => Source;
        public Entity TargetEntity => Target;

        public float StartTime { get; private set; }
        public float Duration { get; private set; }
        public float ExpirationTime { get; private set; }
        public float EndTime => ExpirationTime;
        public float TickInterval { get; }
        public float NextTickTime { get; private set; }
        public int StackCount { get; private set; }
        public int MaxStacks { get; }
        public StatusStackPolicy StackPolicy { get; }
        public StatusCategory Category { get; }
        public float DamagePerTick { get; }
        public EffectPowerTier PowerTier { get; }
        public CrowdControlType CcType { get; }
        public bool CanShatterFreeze { get; }
        public int RemovalPriority { get; set; }

        public bool IsActive => !IsExpired;

        public bool IsExpired
        {
            get
            {
                if (Duration <= 0f) return false;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return now >= ExpirationTime;
            }
        }

        public float RemainingDuration
        {
            get
            {
                if (Duration <= 0f) return 0f;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return Mathf.Max(0f, ExpirationTime - now);
            }
        }

        public RuntimeStatusInstance(
            StatusEffectDefinitionSO definition,
            Entity source,
            Entity target,
            float currentTime)
        {
            Definition = definition;
            StatusId = definition != null ? definition.StatusId : string.Empty;
            Source = source;
            Target = target;
            Duration = definition != null ? definition.DefaultDuration : 0f;
            TickInterval = definition != null ? definition.TickInterval : 0f;
            MaxStacks = definition != null ? Mathf.Max(1, definition.MaxStacks) : 1;
            StackPolicy = definition != null ? definition.StackPolicy : StatusStackPolicy.RefreshDuration;
            Category = definition != null ? definition.Category : StatusCategory.Dot;
            DamagePerTick = definition != null ? definition.DamagePerTick : 0f;
            PowerTier = definition != null ? definition.PowerTier : EffectPowerTier.None;
            CcType = definition != null ? definition.CcType : CrowdControlType.None;
            CanShatterFreeze = definition != null && definition.CanShatterFreeze;

            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
            NextTickTime = TickInterval > 0f ? currentTime + TickInterval : float.MaxValue;
            StackCount = 1;
            RemovalPriority = definition != null ? definition.RemovalPriority : (int)PowerTier;
        }

        public int ReduceStacks(int count)
        {
            if (count <= 0) return StackCount;
            StackCount = Mathf.Max(0, StackCount - count);
            return StackCount;
        }

        public bool CheckTick(float currentTime)
        {
            if (TickInterval <= 0f || IsExpired) return false;

            if (currentTime >= NextTickTime)
            {
                NextTickTime += TickInterval;
                // Stall protection: prevent accumulating runaway ticks if frame stalled
                if (NextTickTime < currentTime)
                {
                    NextTickTime = currentTime + TickInterval;
                }
                return true;
            }

            return false;
        }

        public void RefreshDuration(float currentTime)
        {
            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
        }

        public void Replace(float currentTime)
        {
            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
            NextTickTime = TickInterval > 0f ? currentTime + TickInterval : float.MaxValue;
            StackCount = 1;
        }

        public void AddStack(float currentTime)
        {
            if (StackCount < MaxStacks)
            {
                StackCount++;
            }
            RefreshDuration(currentTime);
        }
    }
}
