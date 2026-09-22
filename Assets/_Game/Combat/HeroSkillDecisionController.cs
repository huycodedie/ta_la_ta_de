using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Progression;

namespace WuxiaGame.Combat
{
    /// <summary>
    /// Autonomous decision layer for Hero skill usage in combat (P07.9.1).
    /// Pure decision authority: evaluates combat state, cooldowns, rage, and priorities,
    /// and delegates execution to Hero.ExecuteSelectedSkill(slot).
    /// DECISION != EXECUTION. Does NOT mutate Rage, HP, Cooldowns, or Damage.
    /// </summary>
    public class HeroSkillDecisionController : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private Hero hero;

        [Header("Telemetry")]
        [SerializeField] private bool enableTelemetry = true;

        public Hero BoundHero => hero != null ? hero : (hero = GetComponent<Hero>());

        private void Awake()
        {
            if (hero == null)
            {
                hero = GetComponent<Hero>();
            }
        }

        public void Initialize(Hero boundHero)
        {
            hero = boundHero;
        }

        private void Update()
        {
            TickDecision(Time.deltaTime);
        }

        /// <summary>
        /// Evaluates combat decision tree each tick.
        /// Exposed for both runtime Update() and deterministic test execution.
        /// </summary>
        public bool TickDecision(float deltaTime = 0f)
        {
            // STEP 1: Hero alive?
            if (BoundHero == null || !BoundHero.gameObject.activeInHierarchy || !BoundHero.IsAlive)
            {
                return false;
            }

            // STEP 2: Battle active?
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive)
            {
                return false;
            }

            // STEP 3: Auto Battle ON?
            bool isAuto = BattleManager.Instance != null ? BattleManager.Instance.IsAutoBattle : true;
            if (!isAuto)
            {
                return false;
            }

            // STEP 4: Hero currently Casting or Channeling?
            if (BoundHero.IsCasting)
            {
                return false;
            }

            // Target resolution for combat checks
            Entity target = (BoundHero.CurrentTarget != null && BoundHero.CurrentTarget.IsAlive)
                ? BoundHero.CurrentTarget
                : new NearestEnemyTargetResolver().ResolveTarget(BoundHero);

            var mmMgr = MindMethodManager.Instance;
            if (mmMgr == null)
            {
                return false;
            }

            // STEP 5 / 6: ULTIMATE BRANCH (Preemptive priority per D5)
            if (TryEvaluateUltimate(mmMgr, target))
            {
                return true;
            }

            // STEP 7: NORMAL SKILL BRANCH (Priority DESC + deterministic tie-break)
            if (TryEvaluateNormalSkills(mmMgr, target))
            {
                return true;
            }

            return false;
        }

        private bool TryEvaluateUltimate(MindMethodManager mmMgr, Entity target)
        {
            if (!BoundHero.CanUseUltimate)
            {
                return false;
            }

            SkillDefinitionSO ultDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            if (ultDef == null || ultDef.IsPassive)
            {
                return false;
            }

            // Cooldown check
            if (SkillExecutor.EnableCooldown && CooldownManager.IsOnCooldown(ultDef.SkillId, out float cdRemaining))
            {
                return false;
            }

            // Rage check
            float currentRage = BoundHero.Rage != null ? BoundHero.Rage.CurrentRage : 0f;
            if (SkillExecutor.EnableRageCost && ultDef.RageCost > 0f && currentRage < ultDef.RageCost)
            {
                return false;
            }

            // Target check if offensive
            if (RequiresLivingTarget(ultDef) && (target == null || !target.IsAlive))
            {
                return false;
            }

            // Ultimate interrupts Basic Attack windup per D5 / P07.9
            if (BoundHero.Attack != null && BoundHero.Attack.AttackTimer > 0f)
            {
                BoundHero.Attack.ResetAttackTimer();
            }

            if (enableTelemetry)
            {
                Debug.Log($"[SKILL-AI] Auto=True State=ExecuteUltimate Rage={currentRage:F0}/{ultDef.RageCost:F0} Skill={ultDef.SkillId} Priority={ultDef.Priority}");
            }

            // Pure request delegation - Rage consumption & Cooldown mutation are owned by execution pipeline
            var result = BoundHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, target);
            return result.Success;
        }

        private struct SkillCandidate
        {
            public SkillSlotType Slot;
            public SkillDefinitionSO Definition;
            public int Priority;
            public string SkillId;
        }

        private bool TryEvaluateNormalSkills(MindMethodManager mmMgr, Entity target)
        {
            if (!BoundHero.CanUseSkill)
            {
                return false;
            }

            var activeState = mmMgr.ActiveMindMethodState;
            if (activeState == null || activeState.SelectedSkillPerSlot == null)
            {
                return false;
            }

            List<SkillCandidate> candidates = new List<SkillCandidate>();

            // Collect all equipped normal active skills (no hardcoded slot limit)
            foreach (var kvp in activeState.SelectedSkillPerSlot)
            {
                SkillSlotType slot = kvp.Key;
                string skillId = kvp.Value;

                // Exclude NormalAttack (Slot 1 basic attack) and Ultimate (handled in Step 6)
                if (slot == SkillSlotType.NormalAttack || slot == SkillSlotType.Ultimate)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(skillId))
                {
                    continue;
                }

                SkillDefinitionSO def = mmMgr.FindSkillDefinition(skillId);
                if (def == null || def.IsPassive)
                {
                    continue;
                }

                // Unlock check
                if (!mmMgr.IsSkillUnlocked(skillId))
                {
                    continue;
                }

                // Cooldown check
                if (SkillExecutor.EnableCooldown && CooldownManager.IsOnCooldown(def.SkillId, out _))
                {
                    continue;
                }

                // Rage check
                float currentRage = BoundHero.Rage != null ? BoundHero.Rage.CurrentRage : 0f;
                if (SkillExecutor.EnableRageCost && def.RageCost > 0f && currentRage < def.RageCost)
                {
                    continue;
                }

                // Target check
                if (RequiresLivingTarget(def) && (target == null || !target.IsAlive))
                {
                    continue;
                }

                candidates.Add(new SkillCandidate
                {
                    Slot = slot,
                    Definition = def,
                    Priority = def.Priority,
                    SkillId = def.SkillId
                });
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            // Deterministic sort: Priority DESC, then SkillId ordinal ASC
            candidates.Sort((a, b) =>
            {
                int pCompare = b.Priority.CompareTo(a.Priority);
                if (pCompare != 0) return pCompare;
                return string.CompareOrdinal(a.SkillId, b.SkillId);
            });

            SkillCandidate chosen = candidates[0];

            if (enableTelemetry)
            {
                Debug.Log($"[SKILL-AI] Auto=True State=ExecuteNormal CandidateCount={candidates.Count} SelectedSkill={chosen.SkillId} Priority={chosen.Priority} Slot={chosen.Slot}");
            }

            // Pure request delegation
            var result = BoundHero.ExecuteSelectedSkill(chosen.Slot, target);
            return result.Success;
        }

        private bool RequiresLivingTarget(SkillDefinitionSO def)
        {
            if (def == null) return false;

            var resolvedEffects = EffectResolver.ResolveEffectsForSkill(def);
            if (resolvedEffects != null && resolvedEffects.Count > 0)
            {
                foreach (var eff in resolvedEffects)
                {
                    if (eff != null && eff.TargetPolicy == SkillTargetPolicy.SingleTarget &&
                        (eff.EffectType == SkillEffectType.Damage ||
                         eff.EffectType == SkillEffectType.Debuff ||
                         eff.EffectType == SkillEffectType.CrowdControl ||
                         eff.EffectType == SkillEffectType.Dispel))
                    {
                        return true;
                    }
                }
            }
            else if (!def.IsPassive && def.DamageMultiplier > 0f)
            {
                return true;
            }

            return false;
        }
    }
}
