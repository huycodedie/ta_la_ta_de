using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;

namespace WuxiaGame.Combat
{
    public enum SkillCastPhase
    {
        Ready = 0,
        Casting = 1,
        Channeling = 2,
        Recovery = 3,
        Interrupted = 4
    }

    public enum SkillCastInterruptSource
    {
        None = 0,
        CrowdControl = 1,
        CasterDeath = 2,
        ManualCancel = 3,
        Stun = 4,
        Freeze = 5
    }

    /// <summary>
    /// P07.9 Phase 1: Minimal runtime cast state foundation.
    /// Manages execution lifecycle, elapsed/remaining time, and interruption status.
    /// Does NOT duplicate CooldownManager, RageComponent, EntityStatusController, or EffectResolver.
    /// </summary>
    public class SkillCastState
    {
        public SkillCastPhase CurrentPhase { get; private set; } = SkillCastPhase.Ready;
        public SkillExecutionRequest ActiveRequest { get; private set; }

        public float CastDuration { get; private set; } = 0f;
        public float ElapsedTime { get; private set; } = 0f;
        public float StartTime { get; private set; } = 0f;

        // Channel tracking (P07.9 Phase 3)
        public bool IsChannel { get; private set; } = false;
        public float ChannelDuration { get; private set; } = 0f;
        public float ChannelTickInterval { get; private set; } = 0f;
        public float ElapsedChannelTime { get; private set; } = 0f;
        public int ChannelTicksExecuted { get; private set; } = 0;

        public bool IsInterruptible { get; private set; } = true;
        public bool IsInterrupted => CurrentPhase == SkillCastPhase.Interrupted;
        public SkillCastInterruptSource InterruptSource { get; private set; } = SkillCastInterruptSource.None;

        public bool IsActive => CurrentPhase == SkillCastPhase.Casting || CurrentPhase == SkillCastPhase.Channeling;
        public bool IsFinished => CurrentPhase == SkillCastPhase.Recovery || CurrentPhase == SkillCastPhase.Interrupted;

        public float RemainingTime
        {
            get
            {
                if (CurrentPhase == SkillCastPhase.Channeling)
                    return Mathf.Max(0f, ChannelDuration - ElapsedChannelTime);
                return Mathf.Max(0f, CastDuration - ElapsedTime);
            }
        }

        public float Progress
        {
            get
            {
                if (CurrentPhase == SkillCastPhase.Channeling)
                    return ChannelDuration > 0f ? Mathf.Clamp01(ElapsedChannelTime / ChannelDuration) : 1f;
                return CastDuration > 0f ? Mathf.Clamp01(ElapsedTime / CastDuration) : 1f;
            }
        }

        public System.Func<SkillCastState, SkillExecutionResult> OnCastCompletedCallback { get; set; }
        public System.Action<SkillCastState, int> OnChannelTickCallback { get; set; }
        public bool HasExecutedEffects { get; set; } = false;
        private bool hasCompleted = false;

        public SkillCastState()
        {
            Reset();
        }

        public void Reset()
        {
            CurrentPhase = SkillCastPhase.Ready;
            ActiveRequest = null;
            CastDuration = 0f;
            ElapsedTime = 0f;
            StartTime = 0f;
            IsChannel = false;
            ChannelDuration = 0f;
            ChannelTickInterval = 0f;
            ElapsedChannelTime = 0f;
            ChannelTicksExecuted = 0;
            IsInterruptible = true;
            InterruptSource = SkillCastInterruptSource.None;
            HasExecutedEffects = false;
            hasCompleted = false;
            OnCastCompletedCallback = null;
            OnChannelTickCallback = null;
        }

        public bool StartCast(SkillExecutionRequest request, float duration, float startTime = 0f, bool interruptible = true)
        {
            if (request == null) return false;
            if (IsActive) return false;
            Reset();
            ActiveRequest = request;
            CastDuration = Mathf.Max(0f, duration);
            StartTime = startTime;
            ElapsedTime = 0f;
            IsInterruptible = interruptible;
            InterruptSource = SkillCastInterruptSource.None;
            HasExecutedEffects = false;
            hasCompleted = false;
            CurrentPhase = SkillCastPhase.Casting;
            return true;
        }

        public bool StartChannel(
            SkillExecutionRequest request,
            float duration,
            float tickInterval,
            float initialCastTime = 0f,
            float startTime = 0f,
            bool interruptible = true)
        {
            if (request == null) return false;
            if (IsActive) return false;
            Reset();
            ActiveRequest = request;
            IsChannel = true;
            ChannelDuration = Mathf.Max(0f, duration);
            ChannelTickInterval = Mathf.Max(0f, tickInterval);
            ElapsedChannelTime = 0f;
            ChannelTicksExecuted = 0;
            StartTime = startTime;
            IsInterruptible = interruptible;
            InterruptSource = SkillCastInterruptSource.None;
            HasExecutedEffects = false;
            hasCompleted = false;

            if (initialCastTime > 0f)
            {
                CastDuration = initialCastTime;
                ElapsedTime = 0f;
                CurrentPhase = SkillCastPhase.Casting;
            }
            else
            {
                CastDuration = ChannelDuration;
                ElapsedTime = 0f;
                CurrentPhase = SkillCastPhase.Channeling;
            }
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive) return;
            if (deltaTime <= 0f) return;

            // Invariant: dead caster cannot complete or continue casting/channeling
            if (ActiveRequest != null && ActiveRequest.Source != null && !ActiveRequest.Source.IsAlive)
            {
                Interrupt(SkillCastInterruptSource.CasterDeath);
                return;
            }

            if (CurrentPhase == SkillCastPhase.Casting)
            {
                ElapsedTime += deltaTime;

                if (ElapsedTime >= CastDuration && !hasCompleted)
                {
                    if (IsChannel)
                    {
                        TransitionToChanneling();
                        float residualDelta = ElapsedTime - CastDuration;
                        if (residualDelta > 0f)
                        {
                            TickChannel(residualDelta);
                        }
                    }
                    else
                    {
                        hasCompleted = true;
                        Complete();
                        OnCastCompletedCallback?.Invoke(this);
                    }
                }
            }
            else if (CurrentPhase == SkillCastPhase.Channeling)
            {
                TickChannel(deltaTime);
            }
        }

        private void TickChannel(float deltaTime)
        {
            if (CurrentPhase != SkillCastPhase.Channeling) return;
            if (hasCompleted) return;

            ElapsedChannelTime += deltaTime;

            // Process any scheduled channel ticks whose boundaries have been reached/crossed
            if (ChannelTickInterval > 0f)
            {
                while (!hasCompleted && CurrentPhase == SkillCastPhase.Channeling)
                {
                    float nextTickTime = (ChannelTicksExecuted + 1) * ChannelTickInterval;
                    if (nextTickTime <= ChannelDuration + 0.0001f && ElapsedChannelTime + 0.0001f >= nextTickTime)
                    {
                        ChannelTicksExecuted++;
                        OnChannelTickCallback?.Invoke(this, ChannelTicksExecuted);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Check Channel Completion
            if (ElapsedChannelTime + 0.0001f >= ChannelDuration && !hasCompleted && CurrentPhase == SkillCastPhase.Channeling)
            {
                hasCompleted = true;
                Complete();
                OnCastCompletedCallback?.Invoke(this);
            }
        }

        public void Interrupt(SkillCastInterruptSource source = SkillCastInterruptSource.CrowdControl)
        {
            if (!IsActive) return;
            if (!IsInterruptible && source != SkillCastInterruptSource.CasterDeath) return;

            CurrentPhase = SkillCastPhase.Interrupted;
            InterruptSource = source;

            if (ActiveRequest != null && ActiveRequest.Source != null)
            {
                EventBus.RaiseSkillCastInterrupted(ActiveRequest.Source, ActiveRequest.Skill, source);
            }
        }

        public void TransitionToChanneling()
        {
            if (CurrentPhase == SkillCastPhase.Casting)
            {
                CurrentPhase = SkillCastPhase.Channeling;
            }
        }

        public void Complete()
        {
            if (IsActive)
            {
                hasCompleted = true;
                CurrentPhase = SkillCastPhase.Recovery;
            }
        }
    }
}
