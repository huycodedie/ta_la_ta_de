using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public interface ISkillExecutor
    {
        SkillExecutionResult ExecuteSkill(SkillExecutionRequest request);
        SkillExecutionResult ExecuteCastComplete(SkillCastState castState);
    }

    public class SkillExecutor : ISkillExecutor
    {
        public static bool EnableCooldown { get; set; } = true;
        public static bool EnableRageCost { get; set; } = true;

        private static ISkillExecutor defaultInstance = new SkillExecutor();
        public static ISkillExecutor Instance
        {
            get => defaultInstance;
            set => defaultInstance = value ?? new SkillExecutor();
        }

        public static SkillExecutionResult Execute(SkillExecutionRequest request)
        {
            return Instance.ExecuteSkill(request);
        }

        public static SkillExecutionResult CompleteCast(SkillCastState castState)
        {
            return Instance.ExecuteCastComplete(castState);
        }

        public SkillExecutionResult ExecuteSkill(SkillExecutionRequest request)
        {
            if (request == null)
            {
                var nullResult = SkillExecutionResult.CreateFailure(
                    null,
                    SkillExecutionFailureReason.SourceInvalidOrDead,
                    "Execution request is null.");
                EventBus.RaiseSkillExecutionFailed(null, nullResult);
                return nullResult;
            }

            Debug.Log($"[SKILL] Request: Source={(request.Source != null ? request.Source.EntityName : "null")}, Skill={(request.Skill != null ? request.Skill.SkillId : "null")}, Slot={request.Slot}");

            var source = request.Source;
            var rageComp = source is Hero hero ? hero.Rage : (source != null ? source.GetComponent<WuxiaGame.Entities.Components.RageComponent>() : null);
            float curRage = rageComp != null ? rageComp.CurrentRage : 0f;
            float curCdRemain = CooldownManager.GetRemainingCooldown(request.Skill?.SkillId);

            // 1. Validation
            if (!SkillExecutionValidator.Validate(request, out SkillExecutionFailureReason reason, out string message))
            {
                Debug.LogWarning($"[SKILL] Validation FAIL: Reason={reason}, Message={message}");
                var failResult = SkillExecutionResult.CreateFailure(request, reason, message, curRage, curCdRemain);
                EventBus.RaiseSkillExecutionFailed(request, failResult);
                return failResult;
            }

            Debug.Log($"[SKILL] Validation PASS: Skill={request.Skill.SkillId}, Target={(request.Target != null ? request.Target.EntityName : "None")}");

            // 2. Resolve ordered per-effect snapshots for this execution (if channel) and verify required offensive targets
            PreservedTargetResolver preparedChannelResolver = null;
            if (request.Skill.IsChannel)
            {
                var effectsToProcess = EffectResolver.ResolveEffectsForSkill(request.Skill);
                preparedChannelResolver = new PreservedTargetResolver(request, effectsToProcess);

                if (effectsToProcess != null && effectsToProcess.Count > 0)
                {
                    foreach (var eff in effectsToProcess)
                    {
                        if (eff == null) continue;
                        bool isOffensive = eff.EffectType == SkillEffectType.Damage ||
                                           eff.EffectType == SkillEffectType.Debuff ||
                                           eff.EffectType == SkillEffectType.CrowdControl ||
                                           eff.EffectType == SkillEffectType.Dispel;

                        if (isOffensive)
                        {
                            var resolvedTargets = preparedChannelResolver.ResolveTargets(request, eff);
                            if (resolvedTargets == null || resolvedTargets.Count == 0)
                            {
                                Debug.LogWarning($"[SKILL] Channel snapshot preparation FAIL: Skill={request.Skill.SkillId}, Effect={eff.name} resolved 0 targets.");
                                var failResult = SkillExecutionResult.CreateFailure(
                                    request,
                                    SkillExecutionFailureReason.TargetInvalidOrDead,
                                    $"Channel skill '{request.Skill.SkillId}' effect '{eff.name}' resolved 0 valid targets during snapshot preparation.",
                                    curRage,
                                    curCdRemain);
                                EventBus.RaiseSkillExecutionFailed(request, failResult);
                                return failResult;
                            }
                        }
                    }
                }
            }

            EventBus.RaiseSkillExecutionRequested(request);

            // 3. Safe Rage Consumption (consumed at Cast Start after validation and snapshot preparation)
            float rageBefore = curRage;
            float rageCost = EnableRageCost ? request.Skill.RageCost : 0f;
            bool rageConsumed = false;

            if (rageCost > 0f && rageComp != null)
            {
                rageConsumed = rageComp.ConsumeRage(rageCost);
                if (!rageConsumed)
                {
                    var failResult = SkillExecutionResult.CreateFailure(
                        request,
                        SkillExecutionFailureReason.InsufficientRage,
                        $"Failed to consume {rageCost} Rage.",
                        rageBefore,
                        curCdRemain);
                    EventBus.RaiseSkillExecutionFailed(request, failResult);
                    return failResult;
                }
            }
            float rageAfter = rageComp != null ? rageComp.CurrentRage : rageBefore;

            // 4. Cast/Channel branch (P07.9 Phase 2 & Phase 3)
            if (request.Skill.IsChannel)
            {
                if (source != null && source.CastState != null)
                {
                    source.CastState.StartChannel(
                        request,
                        request.Skill.ChannelDuration,
                        request.Skill.ChannelTickInterval,
                        request.Skill.CastTime,
                        CooldownManager.CurrentTime);

                    source.CastState.OnChannelTickCallback = (state, tickIndex) =>
                    {
                        ExecuteChannelTick(state, tickIndex, preparedChannelResolver);
                    };
                    source.CastState.OnCastCompletedCallback = (state) => ExecuteCastComplete(state);
                }

                var channelStartedResult = SkillExecutionResult.CreateSuccess(
                    request,
                    null,
                    "Skill channel started.",
                    rageBefore,
                    rageCost,
                    rageAfter,
                    0f,
                    0f);
                return channelStartedResult;
            }
            else if (request.Skill.CastTime > 0f)
            {
                if (source != null && source.CastState != null)
                {
                    source.CastState.StartCast(request, request.Skill.CastTime, CooldownManager.CurrentTime);
                    source.CastState.OnCastCompletedCallback = (state) => ExecuteCastComplete(state);
                }

                var castStartedResult = SkillExecutionResult.CreateSuccess(
                    request,
                    null,
                    "Skill cast started.",
                    rageBefore,
                    rageCost,
                    rageAfter,
                    0f,
                    0f);
                return castStartedResult;
            }

            // 3. Execution Actions (Effect Pipeline via EffectResolver) - for Instant Skills
            if (request.Skill.IsProjectile)
            {
                return ReleaseProjectileAndFinalize(request, rageBefore, rageCost, rageAfter, rageConsumed);
            }

            return ExecuteEffectsAndFinalize(request, rageBefore, rageCost, rageAfter, rageConsumed);
        }

        public SkillExecutionResult ExecuteCastComplete(SkillCastState castState)
        {
            if (castState == null || castState.ActiveRequest == null) return null;
            if (castState.HasExecutedEffects) return null;
            castState.HasExecutedEffects = true;

            var request = castState.ActiveRequest;
            var source = request.Source;
            var rageComp = source is Hero hero ? hero.Rage : (source != null ? source.GetComponent<WuxiaGame.Entities.Components.RageComponent>() : null);
            float curRage = rageComp != null ? rageComp.CurrentRage : 0f;

            if (castState.IsChannel)
            {
                return FinalizeChannelSuccess(request, curRage);
            }

            if (request.Skill.IsProjectile)
            {
                return ReleaseProjectileAndFinalize(request, curRage, 0f, curRage, false);
            }

            return ExecuteEffectsAndFinalize(request, curRage, 0f, curRage, false);
        }

        private SkillExecutionResult ReleaseProjectileAndFinalize(
            SkillExecutionRequest request,
            float rageBefore,
            float rageCost,
            float rageAfter,
            bool rageConsumed)
        {
            var source = request.Source;
            var rageComp = source is Hero hero ? hero.Rage : (source != null ? source.GetComponent<WuxiaGame.Entities.Components.RageComponent>() : null);
            float curCdRemain = CooldownManager.GetRemainingCooldown(request.Skill?.SkillId);

            try
            {
                var effectsToProcess = EffectResolver.ResolveEffectsForSkill(request.Skill);
                var proj = ProjectileController.Launch(
                    request,
                    request.Target,
                    request.Skill.ProjectileSpeed,
                    request.Skill.ProjectileLifetime,
                    effectsToProcess);

                if (proj == null)
                {
                    throw new System.Exception("Failed to launch projectile instance.");
                }
            }
            catch (System.Exception ex)
            {
                if (rageConsumed && rageComp != null)
                {
                    rageComp.AddRage(rageCost);
                }
                Debug.LogError($"[SKILL:PROJECTILE] Release Error: {ex}");
                var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.None, ex.Message, rageBefore, curCdRemain);
                EventBus.RaiseSkillExecutionFailed(request, failResult);
                return failResult;
            }

            // Start Cooldown Only After Successful Release
            float cdDuration = EnableCooldown ? request.Skill.Cooldown : 0f;
            if (cdDuration > 0f)
            {
                CooldownManager.TriggerCooldown(request.Skill.SkillId, cdDuration);
            }
            float cdRemainAfter = CooldownManager.GetRemainingCooldown(request.Skill.SkillId);

            // Create Success Result & Raise Event for Release
            var successResult = SkillExecutionResult.CreateSuccess(
                request,
                null,
                "Projectile released successfully.",
                rageBefore,
                rageCost,
                rageAfter,
                cdDuration,
                cdRemainAfter,
                null);

            Debug.Log($"[SKILL:PROJECTILE] Result: SUCCESS (Released), Skill={request.Skill.SkillId}, Target={request.Target?.EntityName}, Speed={request.Skill.ProjectileSpeed}, Cooldown={cdDuration:F1}s");
            EventBus.RaiseSkillExecutionSucceeded(request, successResult);
            return successResult;
        }

        private void ExecuteChannelTick(SkillCastState castState, int tickIndex, ISkillTargetResolver targetResolver)
        {
            if (castState == null || castState.ActiveRequest == null) return;
            var request = castState.ActiveRequest;
            if (request.Source != null && !request.Source.IsAlive) return;

            try
            {
                var effectsToProcess = EffectResolver.ResolveEffectsForSkill(request.Skill);
                if (effectsToProcess != null && effectsToProcess.Count > 0)
                {
                    EffectResolver.Instance.ProcessEffects(request, effectsToProcess, targetResolver);
                }
                Debug.Log($"[CHANNEL_TICK] Tick #{tickIndex} executed for Skill={request.Skill.SkillId}, Source={request.Source?.EntityName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CHANNEL_TICK] Error on tick #{tickIndex}: {ex}");
            }
        }

        private SkillExecutionResult FinalizeChannelSuccess(SkillExecutionRequest request, float curRage)
        {
            float cdDuration = EnableCooldown ? request.Skill.Cooldown : 0f;
            if (cdDuration > 0f)
            {
                CooldownManager.TriggerCooldown(request.Skill.SkillId, cdDuration);
            }
            float cdRemainAfter = CooldownManager.GetRemainingCooldown(request.Skill.SkillId);

            var successResult = SkillExecutionResult.CreateSuccess(
                request,
                null,
                "Channel skill completed successfully.",
                curRage,
                0f,
                curRage,
                cdDuration,
                cdRemainAfter);

            Debug.Log($"[SKILL:CHANNEL] Result: SUCCESS, Skill={request.Skill.SkillId}, Cooldown={cdDuration:F1}s");
            EventBus.RaiseSkillExecutionSucceeded(request, successResult);
            return successResult;
        }

        private SkillExecutionResult ExecuteEffectsAndFinalize(
            SkillExecutionRequest request,
            float rageBefore,
            float rageCost,
            float rageAfter,
            bool rageConsumed)
        {
            var source = request.Source;
            var rageComp = source is Hero hero ? hero.Rage : (source != null ? source.GetComponent<WuxiaGame.Entities.Components.RageComponent>() : null);
            float curCdRemain = CooldownManager.GetRemainingCooldown(request.Skill?.SkillId);

            System.Collections.Generic.List<SkillEffectExecutionResult> effectResults = null;
            DamageResult? primaryDamageResult = null;
            try
            {
                var effectsToProcess = EffectResolver.ResolveEffectsForSkill(request.Skill);
                if (effectsToProcess != null && effectsToProcess.Count > 0)
                {
                    effectResults = EffectResolver.Instance.ProcessEffects(request, effectsToProcess);

                    if (effectResults != null)
                    {
                        foreach (var effRes in effectResults)
                        {
                            if (effRes.DamageResult.HasValue && !primaryDamageResult.HasValue)
                            {
                                primaryDamageResult = effRes.DamageResult;
                            }
                        }
                    }
                }
                else
                {
                    effectResults = new System.Collections.Generic.List<SkillEffectExecutionResult>();
                }
            }
            catch (System.Exception ex)
            {
                // Invariant safety: refund consumed Rage on unexpected execution failure
                if (rageConsumed && rageComp != null)
                {
                    rageComp.AddRage(rageCost);
                }
                Debug.LogError($"[SKILL] Execution Error: {ex}");
                var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.None, ex.Message, rageBefore, curCdRemain);
                EventBus.RaiseSkillExecutionFailed(request, failResult);
                return failResult;
            }

            // 4. Start Cooldown Only After Successful Execution
            float cdDuration = EnableCooldown ? request.Skill.Cooldown : 0f;
            if (cdDuration > 0f)
            {
                CooldownManager.TriggerCooldown(request.Skill.SkillId, cdDuration);
            }
            float cdRemainAfter = CooldownManager.GetRemainingCooldown(request.Skill.SkillId);

            // 5. Create Success Result & Raise Event
            var successResult = SkillExecutionResult.CreateSuccess(
                request,
                primaryDamageResult,
                "Skill executed successfully.",
                rageBefore,
                rageCost,
                rageAfter,
                cdDuration,
                cdRemainAfter,
                effectResults);
            Debug.Log($"[SKILL] Result: SUCCESS, Skill={request.Skill.SkillId}, Effects={effectResults?.Count ?? 0}, Rage: {rageBefore:F0}->{rageAfter:F0} (-{rageCost:F0}), Cooldown={cdDuration:F1}s");
            EventBus.RaiseSkillExecutionSucceeded(request, successResult);

            return successResult;
        }
    }
}
