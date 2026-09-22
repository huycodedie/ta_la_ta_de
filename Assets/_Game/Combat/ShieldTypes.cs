using System;
using UnityEngine;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public enum ShieldStackPolicy
    {
        Additive = 1,
        RefreshDuration = 2,
        Replace = 3,
        Independent = 4,
        Ignore = 5
    }

    public struct ShieldAbsorbResult
    {
        public Entity Target;
        public Entity Source;
        public string ShieldId;
        public float IncomingDamage;
        public float AbsorbedAmount;
        public float RemainingDamage;
        public float RemainingShield;

        public ShieldAbsorbResult(
            Entity target,
            Entity source,
            string shieldId,
            float incomingDamage,
            float absorbedAmount,
            float remainingDamage,
            float remainingShield)
        {
            Target = target;
            Source = source;
            ShieldId = shieldId ?? string.Empty;
            IncomingDamage = Mathf.Max(0f, incomingDamage);
            AbsorbedAmount = Mathf.Max(0f, absorbedAmount);
            RemainingDamage = Mathf.Max(0f, remainingDamage);
            RemainingShield = Mathf.Max(0f, remainingShield);
        }
    }

    public class RuntimeShieldInstance
    {
        public string ShieldId { get; }
        public ShieldEffectDefinitionSO Definition { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public float CurrentAmount { get; private set; }
        public float MaxAmount { get; private set; }
        public float Duration { get; private set; }
        public float StartTime { get; private set; }
        public float ExpirationTime { get; private set; }
        public int Priority { get; set; }
        public ShieldStackPolicy StackingPolicy { get; }
        public int StackCount { get; private set; }
        public int MaxStacks { get; }

        public bool IsExpired
        {
            get
            {
                if (Duration <= 0f) return false;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return now >= ExpirationTime;
            }
        }

        public bool IsDepleted => CurrentAmount <= 0f;

        public float RemainingDuration
        {
            get
            {
                if (Duration <= 0f) return float.MaxValue;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return Mathf.Max(0f, ExpirationTime - now);
            }
        }

        public float MaxAmountCap { get; private set; }

        public RuntimeShieldInstance(
            string shieldId,
            float amount,
            float duration,
            int priority,
            ShieldStackPolicy stackPolicy,
            Entity source,
            Entity target,
            float currentTime,
            ShieldEffectDefinitionSO definition = null,
            int maxStacks = 1,
            float maxAmount = -1f)
        {
            ShieldId = shieldId ?? "shield_default";
            Definition = definition;
            Source = source;
            Target = target;
            Duration = Mathf.Max(0f, duration);
            Priority = priority;
            StackingPolicy = stackPolicy;
            StackCount = 1;
            MaxStacks = Mathf.Max(1, maxStacks);

            MaxAmountCap = maxAmount > 0f ? maxAmount : -1f;
            float initialAmount = Mathf.Max(0f, amount);
            MaxAmount = MaxAmountCap > 0f ? MaxAmountCap : initialAmount;
            CurrentAmount = MaxAmountCap > 0f ? Mathf.Min(initialAmount, MaxAmountCap) : initialAmount;

            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
        }

        public float Absorb(float damage)
        {
            if (damage <= 0f || CurrentAmount <= 0f) return 0f;
            float absorbed = Mathf.Min(damage, CurrentAmount);
            CurrentAmount = Mathf.Max(0f, CurrentAmount - absorbed);
            return absorbed;
        }

        public void AddAmount(float amount, float newMax = -1f)
        {
            if (amount <= 0f) return;
            if (newMax > 0f)
            {
                MaxAmountCap = newMax;
                MaxAmount = newMax;
            }
            else if (MaxAmountCap > 0f)
            {
                MaxAmount = MaxAmountCap;
            }
            else
            {
                MaxAmount += amount;
            }
            CurrentAmount = Mathf.Clamp(CurrentAmount + amount, 0f, MaxAmount);
        }

        public void RefreshDuration(float currentTime)
        {
            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
        }

        public void ResetAmount(float amount, float newMax = -1f)
        {
            float sanitized = Mathf.Max(0f, amount);
            if (newMax > 0f)
            {
                MaxAmountCap = newMax;
                MaxAmount = newMax;
            }
            else if (MaxAmountCap > 0f)
            {
                MaxAmount = MaxAmountCap;
            }
            else
            {
                MaxAmount = Mathf.Max(MaxAmount, sanitized);
            }
            CurrentAmount = Mathf.Clamp(sanitized, 0f, MaxAmount);
        }

        public void AddStack(float amount, float currentTime, float newMax = -1f)
        {
            if (StackCount < MaxStacks)
            {
                StackCount++;
                AddAmount(amount, newMax);
            }
            RefreshDuration(currentTime);
        }

        public void Replace(float amount, float duration, float currentTime, int priority)
        {
            float sanitized = Mathf.Max(0f, amount);
            Duration = Mathf.Max(0f, duration);
            Priority = priority;
            MaxAmount = sanitized;
            CurrentAmount = sanitized;
            StackCount = 1;
            StartTime = currentTime;
            ExpirationTime = Duration > 0f ? currentTime + Duration : float.MaxValue;
        }
    }
}
