using UnityEngine;
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

            // 6. Validate Target (for combat/offensive skills)
            bool requiresSingleEnemyTarget = skill.IsProjectile;
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
