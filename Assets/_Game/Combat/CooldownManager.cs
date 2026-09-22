using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Progression;

namespace WuxiaGame.Combat
{
    public interface ITimeProvider
    {
        float CurrentTime { get; }
    }

    public class UnityTimeProvider : ITimeProvider
    {
        public float CurrentTime => Time.time;
    }

    public class TestTimeProvider : ITimeProvider
    {
        public float CurrentTime { get; set; }

        public TestTimeProvider(float initialTime = 0f)
        {
            CurrentTime = initialTime;
        }

        public void Advance(float seconds)
        {
            CurrentTime += seconds;
        }

        public void SetTime(float time)
        {
            CurrentTime = time;
        }
    }

    public static class CooldownManager
    {
        private static ITimeProvider timeProvider = new UnityTimeProvider();
        private static readonly Dictionary<string, float> standaloneCooldownEndTimes = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> standaloneCooldownDurations = new Dictionary<string, float>();

        public static ITimeProvider TimeProvider
        {
            get => timeProvider ?? (timeProvider = new UnityTimeProvider());
            set => timeProvider = value ?? new UnityTimeProvider();
        }

        public static float CurrentTime => TimeProvider.CurrentTime;

        public static bool IsReady(string skillId)
        {
            return !IsOnCooldown(skillId, out _);
        }

        public static bool IsOnCooldown(string skillId, out float remainingTime)
        {
            remainingTime = 0f;
            if (string.IsNullOrEmpty(skillId)) return false;

            float now = CurrentTime;

            var mmMgr = MindMethodManager.Instance;
            if (mmMgr != null)
            {
                var state = mmMgr.GetSkillState(skillId);
                if (state != null)
                {
                    if (state.IsOnCooldown(now))
                    {
                        remainingTime = state.GetRemainingCooldown(now);
                        return true;
                    }
                    return false;
                }
            }

            // Fallback for isolated standalone tests without MindMethodManager
            if (standaloneCooldownEndTimes.TryGetValue(skillId, out float endTime))
            {
                if (now < endTime)
                {
                    remainingTime = endTime - now;
                    return true;
                }
                else
                {
                    standaloneCooldownEndTimes.Remove(skillId);
                    standaloneCooldownDurations.Remove(skillId);
                }
            }

            return false;
        }

        public static float GetRemainingCooldown(string skillId)
        {
            IsOnCooldown(skillId, out float remaining);
            return remaining;
        }

        public static float GetCooldownDuration(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return 0f;

            var mmMgr = MindMethodManager.Instance;
            if (mmMgr != null)
            {
                var state = mmMgr.GetSkillState(skillId);
                if (state != null) return state.CooldownDuration;
            }

            if (standaloneCooldownDurations.TryGetValue(skillId, out float duration))
            {
                return duration;
            }

            return 0f;
        }

        public static void TriggerCooldown(string skillId, float duration)
        {
            if (string.IsNullOrEmpty(skillId) || duration <= 0f) return;

            float now = CurrentTime;

            var mmMgr = MindMethodManager.Instance;
            if (mmMgr != null)
            {
                var state = mmMgr.GetOrCreateSkillState(skillId);
                if (state != null)
                {
                    state.TriggerCooldown(duration, now);
                    return;
                }
            }

            // Fallback standalone tracking
            standaloneCooldownEndTimes[skillId] = now + duration;
            standaloneCooldownDurations[skillId] = duration;
        }

        public static void ResetCooldown(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return;

            var mmMgr = MindMethodManager.Instance;
            if (mmMgr != null)
            {
                var state = mmMgr.GetSkillState(skillId);
                if (state != null)
                {
                    state.ResetCooldown();
                }
            }

            standaloneCooldownEndTimes.Remove(skillId);
            standaloneCooldownDurations.Remove(skillId);
        }

        public static void ResetAllCooldowns()
        {
            var mmMgr = MindMethodManager.Instance;
            if (mmMgr != null)
            {
                mmMgr.ResetAllSkillCooldowns();
            }

            standaloneCooldownEndTimes.Clear();
            standaloneCooldownDurations.Clear();
        }

        public static void ResetForTesting()
        {
            timeProvider = new UnityTimeProvider();
            ResetAllCooldowns();
        }
    }
}
