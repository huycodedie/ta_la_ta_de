using UnityEngine;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    public enum BuffModifierMode
    {
        Additive = 1
    }

    public enum BuffStackingPolicy
    {
        Replace = 1,
        RefreshDuration = 2,
        Stack = 3
    }

    public class RuntimeBuffInstance
    {
        public string BuffId { get; }
        public BuffEffectDefinitionSO Definition { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public StatType StatType { get; }
        public BuffModifierMode ModifierMode { get; }
        public float ModifierValue { get; }
        public float Duration { get; private set; }
        public float StartTime { get; private set; }
        public float ExpirationTime { get; private set; }
        public int StackCount { get; private set; }
        public int MaxStacks { get; }
        public BuffStackingPolicy StackingPolicy { get; }
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

        public float RemainingDuration
        {
            get
            {
                if (Duration <= 0f) return 0f;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return Mathf.Max(0f, ExpirationTime - now);
            }
        }

        public RuntimeBuffInstance(
            BuffEffectDefinitionSO definition,
            Entity source,
            Entity target,
            float currentTime)
        {
            Definition = definition;
            BuffId = definition != null ? definition.BuffId : string.Empty;
            Source = source;
            Target = target;
            StatType = definition != null ? definition.StatType : StatType.Attack;
            ModifierMode = definition != null ? definition.ModifierMode : BuffModifierMode.Additive;
            ModifierValue = definition != null ? definition.ModifierValue : 0f;
            Duration = definition != null ? definition.Duration : 0f;
            MaxStacks = definition != null ? Mathf.Max(1, definition.MaxStacks) : 1;
            StackingPolicy = definition != null ? definition.StackingPolicy : BuffStackingPolicy.RefreshDuration;

            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
            StackCount = 1;
            RemovalPriority = definition != null ? definition.RemovalPriority : 0;
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
