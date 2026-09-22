using UnityEngine;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public class RuntimeCrowdControlInstance
    {
        public string CcId { get; }
        public CrowdControlType CcType { get; }
        public float BaseDuration { get; }
        public float EffectiveDuration { get; private set; }
        public EffectPowerTier PowerTier { get; }
        public Entity Source { get; }
        public Entity Target { get; }
        public float StartTime { get; private set; }
        public float ExpirationTime { get; private set; }
        public float EndTime => ExpirationTime;
        public StatusStackPolicy StackPolicy { get; }
        public int RemovalPriority { get; set; }

        public bool IsExpired
        {
            get
            {
                if (EffectiveDuration <= 0f) return true;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return now >= ExpirationTime;
            }
        }

        public bool IsExpiredAt(float time) => EffectiveDuration <= 0f || time >= ExpirationTime;

        public float RemainingDuration
        {
            get
            {
                if (EffectiveDuration <= 0f) return 0f;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return Mathf.Max(0f, ExpirationTime - now);
            }
        }

        public RuntimeCrowdControlInstance(
            string ccId,
            CrowdControlType ccType,
            float baseDuration,
            float effectiveDuration,
            EffectPowerTier powerTier,
            Entity source,
            Entity target,
            float currentTime,
            StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration)
        {
            CcId = ccId ?? string.Empty;
            CcType = ccType;
            BaseDuration = baseDuration;
            EffectiveDuration = effectiveDuration;
            PowerTier = powerTier;
            Source = source;
            Target = target;
            StartTime = currentTime;
            ExpirationTime = effectiveDuration > 0f ? currentTime + effectiveDuration : currentTime;
            StackPolicy = stackPolicy;
            RemovalPriority = (int)powerTier;
        }

        public void RefreshDuration(float currentTime, float newEffectiveDuration)
        {
            StartTime = currentTime;
            EffectiveDuration = newEffectiveDuration;
            ExpirationTime = currentTime + newEffectiveDuration;
        }
    }
}
