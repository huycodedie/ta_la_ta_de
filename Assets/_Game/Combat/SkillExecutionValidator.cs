using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Progression;

namespace WuxiaGame.Combat
{
    public static class SkillExecutionValidator
    {
        public static bool Validate(
            SkillExecutionRequest request,
            out SkillExecutionFailureReason failureReason,
            out string failureMessage)
        {
            failureReason = SkillExecutionFailureReason.None;
            failureMessage = string.Empty;

            if (request == null)
            {
                failureReason = SkillExecutionFailureReason.SourceInvalidOrDead;
                failureMessage = "Skill execution request is null.";
                return false;
            }

            // 1. Validate Source Hero / Entity
            Entity source = request.Source;
            if (source == null || !source.gameObject.activeInHierarchy || !source.IsAlive ||
                source.Health == null || source.Health.CurrentHealth <= 0f)
            {
                failureReason = SkillExecutionFailureReason.SourceInvalidOrDead;
                failureMessage = $"Source entity '{(source != null ? source.EntityName : "null")}' is invalid, null, or dead.";
                return false;
            }

            // 1.45 Validate Active Dash Action Busy Guard (P09-B) - Must be evaluated before CC permissions (F6)
            if (source.IsDashing)
            {
                failureReason = SkillExecutionFailureReason.SourceActionBusy;
                failureMessage = $"Source entity '{source.EntityName}' is busy dashing.";
                return false;
            }

            // 1.5 Validate CC Action Permission (P07.6 / P07.9 Phase 0)
            bool isUltimate = request.Slot == SkillSlotType.Ultimate || (request.Skill != null && request.Skill.SlotType == SkillSlotType.Ultimate);
            if (isUltimate)
            {
                if (!source.CanUseUltimate)
                {
                    failureReason = SkillExecutionFailureReason.SourceCrowdControlled;
                    failureMessage = $"Source entity '{source.EntityName}' is crowd controlled and cannot use ultimate.";
                    return false;
                }
            }
            else
            {
                if (!source.CanUseSkill)
                {
                    failureReason = SkillExecutionFailureReason.SourceCrowdControlled;
                    failureMessage = $"Source entity '{source.EntityName}' is crowd controlled and cannot use skills.";
                    return false;
                }
            }

            // 1.6 Validate Active Cast Re-entry Guard (P07.9 Phase 2.1)
            if (source.IsCasting)
            {
                failureReason = SkillExecutionFailureReason.SourceAlreadyCasting;
                failureMessage = $"Source entity '{source.EntityName}' is already casting.";
                return false;
            }

            // 2. Validate Skill Definition
            SkillDefinitionSO skill = request.Skill;
            if (skill == null)
            {
                failureReason = SkillExecutionFailureReason.SkillNotFound;
                failureMessage = "Skill definition is missing or null.";
                return false;
            }

            // 3. Validate MindMethodManager & Active Mind Method (Hero only)
            if (source is Hero)
            {
                MindMethodManager mmMgr = MindMethodManager.Instance;
                if (mmMgr == null)
                {
                    failureReason = SkillExecutionFailureReason.SkillNotFromActiveMindMethod;
                    failureMessage = "MindMethodManager instance is not found in scene.";
                    return false;
                }

                string activeMmId = mmMgr.ActiveMindMethodId;
                if (string.IsNullOrEmpty(activeMmId) || skill.MindMethodId != activeMmId)
                {
                    failureReason = SkillExecutionFailureReason.SkillNotFromActiveMindMethod;
                    failureMessage = $"Skill '{skill.SkillId}' belongs to Mind Method '{skill.MindMethodId}', but active Mind Method is '{activeMmId}'.";
                    return false;
                }

                // 4. Validate Skill Unlock State
                if (!mmMgr.IsSkillUnlocked(skill.SkillId))
                {
                    failureReason = SkillExecutionFailureReason.SkillLocked;
                    failureMessage = $"Skill '{skill.SkillId}' is locked.";
                    return false;
                }

                // 5. Validate Skill Selection for Slot
                string selectedSkillId = mmMgr.GetSelectedSkillIdForSlot(request.Slot);
                if (string.IsNullOrEmpty(selectedSkillId) || selectedSkillId != skill.SkillId)
                {
                    failureReason = SkillExecutionFailureReason.SkillNotSelected;
                    failureMessage = $"Skill '{skill.SkillId}' is not selected for slot {request.Slot} (currently selected: '{selectedSkillId}').";
                    return false;
                }
            }

            // 5.5 Validate Projectile Delivery Configuration (P09-A)
            var resolvedEffects = EffectResolver.ResolveEffectsForSkill(skill);
            if (skill.IsProjectile)
            {
                if (skill.IsChannel)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Skill '{skill.SkillId}' cannot combine Projectile delivery with Channel execution.";
                    return false;
                }

                if (skill.ProjectileSpeed <= 0f || float.IsNaN(skill.ProjectileSpeed) || float.IsInfinity(skill.ProjectileSpeed))
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Projectile skill '{skill.SkillId}' requires a positive finite ProjectileSpeed (current: {skill.ProjectileSpeed}).";
                    return false;
                }

                if (skill.ProjectileLifetime <= 0f || float.IsNaN(skill.ProjectileLifetime) || float.IsInfinity(skill.ProjectileLifetime))
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Projectile skill '{skill.SkillId}' requires a positive finite ProjectileLifetime (current: {skill.ProjectileLifetime}).";
                    return false;
                }

                if (resolvedEffects != null && resolvedEffects.Count > 0)
                {
                    foreach (var eff in resolvedEffects)
                    {
                        if (eff != null && eff.TargetPolicy != SkillTargetPolicy.SingleTarget)
                        {
                            failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                            failureMessage = $"Projectile skill '{skill.SkillId}' only supports SingleTarget effects in P09-A (found: {eff.TargetPolicy}).";
                            return false;
                        }
                    }
                }
            }

            // 5.6 Validate Dash Delivery Configuration (P09-B)
            if (skill.IsDash)
            {
                if (!(source is Hero))
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Dash skill '{skill.SkillId}' is only supported for Hero in P09-B slice.";
                    return false;
                }

                if (request.Slot != SkillSlotType.Skill &&
                    request.Slot != SkillSlotType.ExternalSkill1 &&
                    request.Slot != SkillSlotType.ExternalSkill2)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Dash skill '{skill.SkillId}' cannot be assigned to slot {request.Slot}.";
                    return false;
                }

                if (request.Slot != skill.SlotType)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Request slot '{request.Slot}' does not match skill slot '{skill.SlotType}'.";
                    return false;
                }

                if (skill.IsProjectile)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Skill '{skill.SkillId}' cannot combine Dash delivery with Projectile delivery.";
                    return false;
                }

                if (skill.IsChannel)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Skill '{skill.SkillId}' cannot combine Dash delivery with Channel execution.";
                    return false;
                }

                if (skill.IsPassive)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Dash skill '{skill.SkillId}' cannot be passive.";
                    return false;
                }

                if (float.IsNaN(skill.CastTime) || float.IsInfinity(skill.CastTime) || skill.CastTime != 0f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Skill '{skill.SkillId}' cannot combine Dash delivery with CastTime (instant dash requires CastTime == 0).";
                    return false;
                }

                if (float.IsNaN(skill.DamageMultiplier) || float.IsInfinity(skill.DamageMultiplier) || skill.DamageMultiplier != 0f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Move-only Dash skill '{skill.SkillId}' cannot have DamageMultiplier != 0 (found: {skill.DamageMultiplier}).";
                    return false;
                }

                if (skill.Effects != null && skill.Effects.Count > 0)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Move-only Dash skill '{skill.SkillId}' cannot have explicit effects in P09-B slice.";
                    return false;
                }

                if (float.IsNaN(skill.DashDistance) || float.IsInfinity(skill.DashDistance) || skill.DashDistance <= 0f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Dash skill '{skill.SkillId}' requires a positive finite DashDistance (current: {skill.DashDistance}).";
                    return false;
                }

                if (float.IsNaN(skill.DashSpeed) || float.IsInfinity(skill.DashSpeed) || skill.DashSpeed <= 0f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Dash skill '{skill.SkillId}' requires a positive finite DashSpeed (current: {skill.DashSpeed}).";
                    return false;
                }

                if (!source.gameObject.activeInHierarchy || !source.IsAlive || (source.Health != null && source.Health.CurrentHealth <= 0f))
                {
                    failureReason = SkillExecutionFailureReason.SourceInvalidOrDead;
                    failureMessage = $"Source entity '{source.EntityName}' is inactive or dead.";
                    return false;
                }

                if (!source.CanDash)
                {
                    failureReason = SkillExecutionFailureReason.SourceCrowdControlled;
                    failureMessage = $"Source entity '{source.EntityName}' cannot dash (crowd controlled or casting).";
                    return false;
                }

                if (source.Movement == null || !source.Movement.enabled || !source.Movement.isActiveAndEnabled || !source.Movement.IsMovementEnabled)
                {
                    failureReason = SkillExecutionFailureReason.SourceInvalidOrDead;
                    failureMessage = $"Source entity '{source.EntityName}' has missing, disabled or inactive MovementComponent.";
                    return false;
                }

                if (source.Movement.IsDashing)
                {
                    failureReason = SkillExecutionFailureReason.SourceActionBusy;
                    failureMessage = $"Source entity '{source.EntityName}' is already executing a dash.";
                    return false;
                }

                if (BattleManager.Instance != null)
                {
                    if (BattleManager.Instance.IsCombatPausedByUI || !BattleManager.Instance.IsBattleActive)
                    {
                        failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                        failureMessage = "Cannot initiate dash while combat is paused or inactive.";
                        return false;
                    }

                    var bState = BattleManager.Instance.CurrentBattleState;
                    if (bState == BattleState.AwaitingPlayerStart ||
                        bState == BattleState.MonsterDead ||
                        bState == BattleState.HeroDead ||
                        bState == BattleState.Victory ||
                        bState == BattleState.Defeat ||
                        bState == BattleState.EncounterTransition ||
                        bState == BattleState.LootPending)
                    {
                        failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                        failureMessage = $"Cannot initiate dash during battle state {bState}.";
                        return false;
                    }

                    bool isSourceInEncounter = false;
                    if (source is Hero hero && BattleManager.Instance.CurrentHero == hero)
                    {
                        isSourceInEncounter = true;
                    }
                    else if (BattleManager.Instance.BelongsToEncounter(source))
                    {
                        isSourceInEncounter = true;
                    }

                    if (!isSourceInEncounter)
                    {
                        failureReason = SkillExecutionFailureReason.SourceInvalidOrDead;
                        failureMessage = $"Source '{source.EntityName}' does not belong to active encounter.";
                        return false;
                    }
                }

                Entity target = request.Target;
                if (target == null || !target.gameObject.activeInHierarchy || !target.IsAlive || (target.Health != null && target.Health.CurrentHealth <= 0f))
                {
                    failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                    failureMessage = $"Dash toward target requires a living active target (target: '{(target != null ? target.EntityName : "null")}').";
                    return false;
                }

                if (BattleManager.Instance != null)
                {
                    bool isEncounterTarget = false;
                    if (target is Monster monster && BattleManager.Instance.ActiveMonsters != null)
                    {
                        var activeList = BattleManager.Instance.ActiveMonsters;
                        for (int i = 0; i < activeList.Count; i++)
                        {
                            if (activeList[i] == monster)
                            {
                                isEncounterTarget = true;
                                break;
                            }
                        }
                    }
                    else if (BattleManager.Instance.BelongsToEncounter(target))
                    {
                        isEncounterTarget = true;
                    }

                    if (!isEncounterTarget)
                    {
                        failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                        failureMessage = $"Target '{target.EntityName}' does not belong to active encounter.";
                        return false;
                    }
                }

                Vector3 sourcePos = source.transform.position;
                Vector3 targetPos = target.transform.position;
                if (float.IsNaN(sourcePos.x) || float.IsInfinity(sourcePos.x) ||
                    float.IsNaN(sourcePos.y) || float.IsInfinity(sourcePos.y) ||
                    float.IsNaN(sourcePos.z) || float.IsInfinity(sourcePos.z) ||
                    float.IsNaN(targetPos.x) || float.IsInfinity(targetPos.x) ||
                    float.IsNaN(targetPos.y) || float.IsInfinity(targetPos.y) ||
                    float.IsNaN(targetPos.z) || float.IsInfinity(targetPos.z))
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = "Source or target transform position contains NaN or Infinity.";
                    return false;
                }

                float diffX = targetPos.x - sourcePos.x;
                float distToTargetX = Mathf.Abs(diffX);
                if (float.IsNaN(distToTargetX) || float.IsInfinity(distToTargetX) || distToTargetX < 0.001f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = "Target is at the same X coordinate or invalid distance.";
                    return false;
                }

                float stoppingDistance = source.AttackRange;
                if (float.IsNaN(stoppingDistance) || float.IsInfinity(stoppingDistance) || stoppingDistance < 0f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Source entity '{source.EntityName}' has invalid AttackRange ({stoppingDistance}).";
                    return false;
                }

                float usefulDistance = Mathf.Min(skill.DashDistance, distToTargetX - stoppingDistance);
                if (usefulDistance <= 0.01f)
                {
                    failureReason = SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                    failureMessage = $"Target is already within attack range ({distToTargetX:F2} <= {stoppingDistance:F2}) or useful dash distance is zero ({usefulDistance:F2}).";
                    return false;
                }
            }

            // 6. Validate Target (for combat/offensive skills)
            bool requiresSingleEnemyTarget = skill.IsProjectile || skill.IsDash;
            if (resolvedEffects != null && resolvedEffects.Count > 0)
            {
                foreach (var eff in resolvedEffects)
                {
                    if (eff == null) continue;

                    bool isOffensive = eff.EffectType == SkillEffectType.Damage ||
                                       eff.EffectType == SkillEffectType.Debuff ||
                                       eff.EffectType == SkillEffectType.CrowdControl ||
                                       eff.EffectType == SkillEffectType.Dispel;

                    if (!isOffensive) continue;

                    if (eff.TargetPolicy == SkillTargetPolicy.SingleTarget)
                    {
                        requiresSingleEnemyTarget = true;
                    }
                    else if (eff.TargetPolicy == SkillTargetPolicy.Area)
                    {
                        if (eff.TargetRadius <= 0f)
                        {
                            failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                            failureMessage = $"Area skill '{skill.SkillId}' effect requires a positive TargetRadius (current: {eff.TargetRadius}).";
                            return false;
                        }

                        if (!CombatTargetQuery.TryGetValidAreaAnchor(request, out _))
                        {
                            failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                            failureMessage = $"Area skill '{skill.SkillId}' requires a valid living registered encounter monster as anchor.";
                            return false;
                        }
                    }
                    else if (eff.TargetPolicy == SkillTargetPolicy.AllEnemies)
                    {
                        if (!CombatTargetQuery.HasLivingRegisteredMonster(source))
                        {
                            failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                            failureMessage = $"AllEnemies skill '{skill.SkillId}' requires at least one living registered encounter monster.";
                            return false;
                        }
                    }
                }
            }
            else if (!skill.IsPassive && skill.DamageMultiplier > 0f)
            {
                requiresSingleEnemyTarget = true;
            }

            if (requiresSingleEnemyTarget)
            {
                Entity target = request.Target;
                if (target == null || !target.gameObject.activeInHierarchy || !target.IsAlive ||
                    target.Health == null || target.Health.CurrentHealth <= 0f)
                {
                    failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                    failureMessage = $"Target entity '{(target != null ? target.EntityName : "null")}' is invalid, null, or dead.";
                    return false;
                }

                bool isOpposing = (source is Hero && target is Monster) ||
                                  (source is Monster && target is Hero) ||
                                  (source.EntityType != target.EntityType);

                if (!isOpposing)
                {
                    failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                    failureMessage = $"Target entity '{target.EntityName}' is on the same team as source '{source.EntityName}'.";
                    return false;
                }

                if ((skill.IsProjectile || skill.IsDash) && source is Hero && target is Monster mTarget)
                {
                    if (!CombatTargetQuery.IsValidEncounterMonster(source, mTarget))
                    {
                        failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                        failureMessage = $"Target monster '{mTarget.EntityName}' is not a valid living registered encounter monster.";
                        return false;
                    }
                }
            }
            else if (request.Target != null && (!request.Target.gameObject.activeInHierarchy || !request.Target.IsAlive))
            {
                failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                failureMessage = $"Explicit target '{(request.Target != null ? request.Target.EntityName : "null")}' is invalid, null, or dead.";
                return false;
            }

            // Validation for Channel skills (side-effect-free: verifies target validity without storing snapshot state)
            if (skill.IsChannel && resolvedEffects != null && resolvedEffects.Count > 0)
            {
                var defaultResolver = new DefaultSkillTargetResolver();
                foreach (var eff in resolvedEffects)
                {
                    if (eff == null) continue;

                    bool isOffensive = eff.EffectType == SkillEffectType.Damage ||
                                       eff.EffectType == SkillEffectType.Debuff ||
                                       eff.EffectType == SkillEffectType.CrowdControl ||
                                       eff.EffectType == SkillEffectType.Dispel;

                    if (isOffensive)
                    {
                        var resolved = defaultResolver.ResolveTargets(request, eff);
                        if (resolved == null || resolved.Count == 0)
                        {
                            failureReason = SkillExecutionFailureReason.TargetInvalidOrDead;
                            failureMessage = $"Channel skill '{skill.SkillId}' effect '{eff.name}' resolved 0 valid targets.";
                            return false;
                        }
                    }
                }
            }

            // 7. Validate Cooldown
            if (SkillExecutor.EnableCooldown && CooldownManager.IsOnCooldown(skill.SkillId, out float remainingTime))
            {
                failureReason = SkillExecutionFailureReason.CooldownNotReady;
                failureMessage = $"Skill '{skill.SkillId}' is on cooldown ({remainingTime:F2}s remaining).";
                return false;
            }

            // 8. Validate Rage Cost
            if (SkillExecutor.EnableRageCost && skill.RageCost > 0f)
            {
                var rageComp = source is Hero hero ? hero.Rage : source.GetComponent<WuxiaGame.Entities.Components.RageComponent>();
                float currentRage = rageComp != null ? rageComp.CurrentRage : 0f;
                if (rageComp == null || currentRage < skill.RageCost)
                {
                    failureReason = SkillExecutionFailureReason.InsufficientRage;
                    failureMessage = $"Insufficient Rage for skill '{skill.SkillId}'. Required: {skill.RageCost:F0}, Current: {currentRage:F0}.";
                    return false;
                }
            }

            return true;
        }
    }
}
