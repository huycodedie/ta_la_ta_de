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

            // Pre-compute pure data DashExecutionPlan for Dash skill BEFORE callbacks and resource mutation
            WuxiaGame.Entities.Components.DashExecutionPlan preparedDashPlan = null;
            if (request.Skill.IsDash)
            {
                float diffX = request.Target.transform.position.x - source.transform.position.x;
                Vector3 direction = new Vector3(Mathf.Sign(diffX), 0f, 0f);
                float distToTargetX = Mathf.Abs(diffX);
                float stoppingDistance = source.AttackRange;
                float usefulDistance = Mathf.Min(request.Skill.DashDistance, distToTargetX - stoppingDistance);
                int encounterIndex = BattleManager.Instance != null ? BattleManager.Instance.EncounterIndex : 0;
                Vector3 startPos = source.transform.position;
                float endpointX = startPos.x + direction.x * usefulDistance;
                preparedDashPlan = new WuxiaGame.Entities.Components.DashExecutionPlan(
                    source,
                    request.Target,
                    direction,
                    usefulDistance,
                    request.Skill.DashSpeed,
                    stoppingDistance,
                    encounterIndex,
                    BattleManager.Instance,
                    startPos,
                    endpointX);
            }

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

            if (request.Skill.IsDash)
            {
                if (source == null || source.Movement == null)
                {
                    var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceInvalidOrDead, "Source or MovementComponent missing for Dash.", curRage, curCdRemain);
                    EventBus.RaiseSkillExecutionFailed(request, failResult);
                    return failResult;
                }

                if (!source.Movement.TryReserveDashStartup(preparedDashPlan))
                {
                    var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceActionBusy, "Source entity has an ongoing dash reservation or is already busy.", curRage, curCdRemain);
                    EventBus.RaiseSkillExecutionFailed(request, failResult);
                    return failResult;
                }
            }

            try
            {
                EventBus.RaiseSkillExecutionRequested(request);

                // POST-REQUEST CALLBACK RE-VALIDATION (R1 requirement: synchronous listeners could mutate state)
                if (request.Skill.IsDash)
                {
                    if (source == null || !source.gameObject.activeInHierarchy || !source.IsAlive || (source.Health != null && source.Health.CurrentHealth <= 0f))
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceInvalidOrDead, "Source died or deactivated during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }
                    if (source.Movement == null || !source.Movement.enabled || !source.Movement.isActiveAndEnabled || !source.Movement.IsMovementEnabled)
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceInvalidOrDead, "MovementComponent disabled during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }
                    if (source.Movement.IsDashing)
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceActionBusy, "Source entity became busy dashing during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }
                    if (!source.CanDash)
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceCrowdControlled, "Source crowd controlled during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }
                    var boundTarget = preparedDashPlan.BoundTarget;
                    if (boundTarget == null || (boundTarget is UnityEngine.Object ubt && ubt == null) ||
                        !boundTarget.gameObject.activeInHierarchy || !boundTarget.IsAlive || (boundTarget.Health != null && boundTarget.Health.CurrentHealth <= 0f))
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.TargetInvalidOrDead, "Target died or deactivated during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }

                    var boundBM = preparedDashPlan.BoundBattleManager;
                    if (boundBM == null || (boundBM is UnityEngine.Object ubbm && ubbm == null))
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.InvalidDeliveryConfiguration, "BattleManager destroyed during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }

                    var currBM = BattleManager.Instance;
                    if (currBM == null || (currBM is UnityEngine.Object ucbm && ucbm == null) ||
                        currBM != boundBM ||
                        currBM.EncounterIndex != preparedDashPlan.BoundEncounterIndex ||
                        !currBM.IsBattleActive ||
                        currBM.IsCombatPausedByUI ||
                        currBM.CurrentBattleState != BattleState.InProgress)
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.InvalidDeliveryConfiguration, "BattleManager state or encounter changed during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }

                    // Check encounter membership
                    if (!IsEntityInEncounterStatic(currBM, boundTarget))
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.TargetInvalidOrDead, "Target removed from encounter during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }

                    if (!IsEntityInEncounterStatic(currBM, source))
                    {
                        var failResult = SkillExecutionResult.CreateFailure(request, SkillExecutionFailureReason.SourceInvalidOrDead, "Source removed from encounter during execution request callbacks.", curRage, curCdRemain);
                        EventBus.RaiseSkillExecutionFailed(request, failResult);
                        return failResult;
                    }
                }

                // 3. Safe Rage Consumption (consumed at Cast Start after validation and snapshot preparation)
                float rageBefore = rageComp != null ? rageComp.CurrentRage : curRage;
                float rageCost = EnableRageCost ? request.Skill.RageCost : 0f;
                bool rageConsumed = false;

                if (rageCost > 0f && rageComp != null)
                {
                    bool notificationException = false;
                    try
                    {
                        rageConsumed = rageComp.ConsumeRage(rageCost);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[SKILL] Rage consumption notification threw exception: {ex}");
                        notificationException = true;
                        // In RageComponent, currentRage -= amount executes before NotifyRageChanged().
                        // Any exception from ConsumeRage guarantees debit already occurred.
                        rageConsumed = true;
                    }

                    if (notificationException)
                    {
                        // Rollback strictly THIS transaction's debit by adding back rageCost
                        try
                        {
                            rageComp.AddRage(rageCost);
                        }
                        catch (System.Exception refundEx)
                        {
                            Debug.LogError($"[SKILL] Exception during rage refund: {refundEx}");
                        }
                        rageConsumed = false;

                        float currentRageAfterRefund = rageComp.CurrentRage;
                        var failResult = SkillExecutionResult.CreateFailure(
                            request,
                            SkillExecutionFailureReason.InvalidDeliveryConfiguration,
                            "Notification listener threw exception during rage debit; debited rage refunded.",
                            currentRageAfterRefund,
                            curCdRemain);
                        try
                        {
                            EventBus.RaiseSkillExecutionFailed(request, failResult);
                        }
                        catch (System.Exception busEx)
                        {
                            Debug.LogError($"[SKILL] Exception raising SkillExecutionFailed: {busEx}");
                        }
                        return failResult;
                    }

                    if (!rageConsumed)
                    {
                        var failResult = SkillExecutionResult.CreateFailure(
                            request,
                            SkillExecutionFailureReason.InsufficientRage,
                            $"Failed to consume {rageCost} Rage.",
                            rageComp.CurrentRage,
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

                if (request.Skill.IsDash)
                {
                    return ExecuteDashAndFinalize(request, preparedDashPlan, rageBefore, rageCost, rageAfter, rageConsumed, rageComp, curCdRemain);
                }

                return ExecuteEffectsAndFinalize(request, rageBefore, rageCost, rageAfter, rageConsumed);
            }
            finally
            {
                if (request.Skill.IsDash && source != null && source.Movement != null && preparedDashPlan != null)
                {
                    source.Movement.ReleaseDashReservation(preparedDashPlan.TransactionId);
                }
            }
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

        private SkillExecutionResult ExecuteDashAndFinalize(
            SkillExecutionRequest request,
            WuxiaGame.Entities.Components.DashExecutionPlan plan,
            float rageBefore,
            float rageCost,
            float rageAfter,
            bool rageConsumed,
            WuxiaGame.Entities.Components.RageComponent rageComp,
            float curCdRemain)
        {
            var source = request.Source;
            bool isCommitted = false;
            float cdDuration = EnableCooldown ? request.Skill.Cooldown : 0f;

            System.Action commitAction = () =>
            {
                // ATOMIC COMMIT POINT:
                // Trigger cooldown and reset attack timers at commit BEFORE observer events fire
                if (cdDuration > 0f)
                {
                    CooldownManager.TriggerCooldown(request.Skill.SkillId, cdDuration);
                }
                if (source != null && source.Attack != null)
                {
                    source.Attack.ResetAttackTimer();
                }
                isCommitted = true;
            };

            try
            {
                if (source == null || source.Movement == null)
                {
                    throw new System.Exception("MovementComponent is missing on source entity.");
                }

                bool started = source.Movement.TryStartDash(plan, null, null, commitAction);
                if (!started)
                {
                    throw new System.Exception("MovementComponent rejected dash startup.");
                }
            }
            catch (System.Exception ex)
            {
                // Rollback on startup failure: strictly isolated to pre-commit failure
                if (!isCommitted)
                {
                    try
                    {
                        if (rageConsumed && rageComp != null)
                        {
                            rageComp.AddRage(rageCost);
                            rageConsumed = false;
                        }
                    }
                    catch (System.Exception refundEx)
                    {
                        Debug.LogError($"[SKILL:DASH] Exception during rage refund: {refundEx}");
                    }
                    finally
                    {
                        if (source != null && source.Movement != null)
                        {
                            source.Movement.ReleaseDashReservation(plan.TransactionId);
                            source.Movement.AbortDash(plan.TransactionId);
                        }
                    }

                    Debug.LogError($"[SKILL:DASH] Startup Error: {ex}");
                    SkillExecutionFailureReason reason = (source != null && source.Movement != null && source.Movement.IsDashing)
                        ? SkillExecutionFailureReason.SourceActionBusy
                        : SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    var failResult = SkillExecutionResult.CreateFailure(request, reason, ex.Message, rageBefore, curCdRemain);
                    EventBus.RaiseSkillExecutionFailed(request, failResult);
                    return failResult;
                }
                else
                {
                    Debug.LogError($"[SKILL:DASH] Post-commit exception (abort does not refund): {ex}");
                }
            }

            // At this point, startup committed successfully. Cooldown was triggered in commitAction.
            float cdRemainAfter = CooldownManager.GetRemainingCooldown(request.Skill.SkillId);

            // Create Success Result & Raise Event ONCE
            var successResult = SkillExecutionResult.CreateSuccess(
                request,
                null,
                "Dash started successfully.",
                rageBefore,
                rageCost,
                rageAfter,
                cdDuration,
                cdRemainAfter,
                null);

            Debug.Log($"[SKILL:DASH] Result: SUCCESS (Committed), Skill={request.Skill.SkillId}, Target={request.Target?.EntityName}, Distance={plan.TotalPlannedDistance:F2}m, Speed={plan.Speed}m/s, Cooldown={cdDuration:F1}s");
            try
            {
                EventBus.RaiseSkillExecutionSucceeded(request, successResult);
            }
            catch (System.Exception obsEx)
            {
                Debug.LogError($"[SKILL:DASH] Observer exception on succeeded event: {obsEx}");
                // An observer throwing post-commit MUST NOT rollback cooldown or refund rage!
            }
            return successResult;
        }

        private static bool IsEntityInEncounterStatic(BattleManager bm, Entity entity)
        {
            if (bm == null || entity == null) return false;
            if (entity is UnityEngine.Object uEnt && uEnt == null) return false;

            if (entity is Hero hero)
            {
                if (bm.CurrentHero == hero) return true;
                return bm.BelongsToEncounter(hero);
            }

            if (entity is Monster monster)
            {
                if (bm.ActiveMonsters != null)
                {
                    var activeList = bm.ActiveMonsters;
                    for (int i = 0; i < activeList.Count; i++)
                    {
                        if (activeList[i] == monster) return true;
                    }
                }
                if (bm.CurrentMonster == monster) return true;
                return bm.BelongsToEncounter(monster);
            }

            return bm.BelongsToEncounter(entity);
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
