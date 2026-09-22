using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    public class EntityStatusController : MonoBehaviour
    {
        private Entity ownerEntity;
        public Entity OwnerEntity => ownerEntity != null ? ownerEntity : (ownerEntity = GetComponent<Entity>());

        private readonly Dictionary<string, RuntimeBuffInstance> activeBuffs = new Dictionary<string, RuntimeBuffInstance>();
        private readonly Dictionary<string, RuntimeStatusInstance> activeStatuses = new Dictionary<string, RuntimeStatusInstance>();
        private readonly Dictionary<string, RuntimeDebuffInstance> activeDebuffs = new Dictionary<string, RuntimeDebuffInstance>();
        private readonly Dictionary<string, RuntimeCrowdControlInstance> activeCrowdControls = new Dictionary<string, RuntimeCrowdControlInstance>();
        private readonly Dictionary<string, RuntimeShieldInstance> activeShields = new Dictionary<string, RuntimeShieldInstance>();
        private int independentShieldCounter = 0;

        public IReadOnlyDictionary<string, RuntimeBuffInstance> ActiveBuffs => activeBuffs;
        public IReadOnlyDictionary<string, RuntimeStatusInstance> ActiveStatuses => activeStatuses;
        public IReadOnlyDictionary<string, RuntimeDebuffInstance> ActiveDebuffs => activeDebuffs;
        public IReadOnlyDictionary<string, RuntimeCrowdControlInstance> ActiveCrowdControls => activeCrowdControls;
        public IReadOnlyDictionary<string, RuntimeShieldInstance> ActiveShields => activeShields;

        public bool HasActiveShield
        {
            get
            {
                if (activeShields.Count == 0) return false;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                foreach (var kvp in activeShields)
                {
                    if (kvp.Value != null && !kvp.Value.IsDepleted && !kvp.Value.IsExpired)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public float TotalShieldAmount
        {
            get
            {
                float total = 0f;
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                foreach (var kvp in activeShields)
                {
                    if (kvp.Value != null && !kvp.Value.IsDepleted && !kvp.Value.IsExpired)
                    {
                        total += kvp.Value.CurrentAmount;
                    }
                }
                return total;
            }
        }

        // Existing events
        public event Action<RuntimeBuffInstance> OnBuffApplied;
        public event Action<RuntimeBuffInstance> OnBuffExpired;
        public event Action<RuntimeStatusInstance> OnStatusApplied;
        public event Action<RuntimeStatusInstance> OnStatusExpired;
        public event Action<RuntimeStatusInstance, DamageResult> OnStatusTick;

        // P07.6 CC & Debuff events
        public event Action<RuntimeDebuffInstance> OnDebuffApplied;
        public event Action<RuntimeDebuffInstance> OnDebuffExpired;
        public event Action<RuntimeCrowdControlInstance> OnCrowdControlApplied;
        public event Action<RuntimeCrowdControlInstance> OnCrowdControlExpired;
        public event Action<CrowdControlType> OnCrowdControlImmune;
        public event Action<CrowdControlType> OnCrowdControlResisted;
        public static event Action<Entity, Entity> OnFreezeShatterTriggered;

        // P07.8 Shield events
        public event Action<RuntimeShieldInstance> OnShieldApplied;
        public event Action<RuntimeShieldInstance, ShieldAbsorbResult> OnShieldAbsorbed;
        public event Action<RuntimeShieldInstance> OnShieldDepleted;
        public event Action<RuntimeShieldInstance> OnShieldExpired;
        public event Action<RuntimeShieldInstance> OnShieldRemoved;

        // Anti-CC Immunity
        private float antiCCImmunityEndTime = 0f;
        public bool HasAntiCCImmunity => (CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time) < antiCCImmunityEndTime;
        public float AntiCCRemainingDuration
        {
            get
            {
                float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
                return Mathf.Max(0f, antiCCImmunityEndTime - now);
            }
        }

        // Action permissions
        public bool CanMove => !IsStunned && !IsRooted && !IsFrozen;
        public bool CanBasicAttack => !IsStunned && !IsFrozen;
        public bool CanUseSkill => !IsStunned && !IsFrozen;
        public bool CanUseUltimate => !IsStunned && !IsFrozen;
        public bool CanDash => !IsStunned && !IsRooted && !IsFrozen;

        public bool IsStunned => HasActiveCrowdControl(CrowdControlType.Stun);
        public bool IsRooted => HasActiveCrowdControl(CrowdControlType.Root);
        public bool IsFrozen => HasActiveCrowdControl(CrowdControlType.Freeze);

        public bool HasActiveCrowdControl(CrowdControlType type)
        {
            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
            foreach (var kvp in activeCrowdControls)
            {
                if (kvp.Value != null && !kvp.Value.IsExpired && !kvp.Value.IsExpiredAt(now) && kvp.Value.CcType == type)
                {
                    return true;
                }
            }
            return false;
        }

        private void Awake()
        {
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<Entity>();
            }
        }

        public void Initialize(Entity owner)
        {
            ownerEntity = owner;
        }

        // --- ANTI-CC IMMUNITY ---
        public void ApplyAntiCCImmunity(float duration, Entity source = null, string id = "anti_cc")
        {
            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
            antiCCImmunityEndTime = Mathf.Max(antiCCImmunityEndTime, now + Mathf.Max(0f, duration));
            Debug.Log($"[ANTI_CC:APPLIED] Target={OwnerEntity?.EntityName}, Dur={duration:F1}s, EndTime={antiCCImmunityEndTime:F1}s");
        }

        public void RemoveAntiCCImmunity()
        {
            antiCCImmunityEndTime = 0f;
        }

        // --- CROWD CONTROL ---
        public float GetCcResistance()
        {
            if (OwnerEntity == null || OwnerEntity.Stats == null) return 0f;
            float res = OwnerEntity.Stats.GetValue(StatType.CcResistance, 0f);
            if (res > 1f) res /= 100f;
            return Mathf.Clamp(res, 0f, 1f);
        }

        public bool ApplyCrowdControl(CrowdControlEffectDefinitionSO ccDef, Entity source, out string applyReason)
        {
            if (ccDef == null || string.IsNullOrEmpty(ccDef.CcId))
            {
                applyReason = "Invalid CC Definition";
                return false;
            }

            return ApplyCrowdControl(
                ccDef.CcId,
                ccDef.CcType,
                ccDef.Duration,
                ccDef.PowerTier,
                source,
                ccDef.StackingPolicy,
                out applyReason);
        }

        public bool ApplyCrowdControl(
            string ccId,
            CrowdControlType ccType,
            float baseDuration,
            EffectPowerTier powerTier = EffectPowerTier.TierB,
            Entity source = null,
            StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration)
        {
            return ApplyCrowdControl(ccId, ccType, baseDuration, powerTier, source, stackPolicy, out _);
        }

        public RuntimeCrowdControlInstance GetCrowdControl(string ccId)
        {
            if (string.IsNullOrEmpty(ccId)) return null;
            activeCrowdControls.TryGetValue(ccId, out var cc);
            return cc;
        }

        public bool ApplyCrowdControl(
            string ccId,
            CrowdControlType ccType,
            float baseDuration,
            EffectPowerTier powerTier,
            Entity source,
            StatusStackPolicy stackPolicy,
            out string applyReason)
        {
            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;

            if (OwnerEntity == null || !OwnerEntity.IsAlive)
            {
                applyReason = "Target is dead or invalid";
                return false;
            }

            // 1. Active Anti-CC Protection Check (Checked BEFORE CC Application!)
            if (HasAntiCCImmunity)
            {
                applyReason = "Target is Immune to CC";
                EventBus.RaiseCrowdControlImmune(OwnerEntity, ccType);
                OnCrowdControlImmune?.Invoke(ccType);
                Debug.Log($"[CC:IMMUNE] Target={OwnerEntity.EntityName} is immune to {ccType}!");
                return false;
            }

            // 2. CC Resistance Duration Reduction: ActualDuration = BaseDuration * (1 - Resistance)
            float resistance = GetCcResistance();
            float effectiveDuration = baseDuration * (1.0f - resistance);

            if (effectiveDuration <= 0f)
            {
                applyReason = $"Resisted 100% (Resistance: {resistance * 100f:F0}%)";
                EventBus.RaiseCrowdControlResisted(OwnerEntity, ccType);
                OnCrowdControlResisted?.Invoke(ccType);
                Debug.Log($"[CC:RESISTED] Target={OwnerEntity.EntityName} resisted {ccType} (100% resistance)");
                return false;
            }

            // 3. Register Active Crowd Control
            if (activeCrowdControls.TryGetValue(ccId, out var existing))
            {
                existing.RefreshDuration(now, effectiveDuration);
                OnCrowdControlApplied?.Invoke(existing);
            }
            else
            {
                var newCc = new RuntimeCrowdControlInstance(
                    ccId,
                    ccType,
                    baseDuration,
                    effectiveDuration,
                    powerTier,
                    source,
                    OwnerEntity,
                    now,
                    stackPolicy);
                activeCrowdControls[ccId] = newCc;
                OnCrowdControlApplied?.Invoke(newCc);
            }

            // 4. Interruption (Stun and Freeze immediately interrupt active skills)
            if (ccType == CrowdControlType.Stun)
            {
                if (OwnerEntity != null)
                {
                    OwnerEntity.InterruptCurrentAction(SkillCastInterruptSource.Stun);
                    EventBus.RaiseCrowdControlInterrupted(OwnerEntity, ccType);
                }
            }
            else if (ccType == CrowdControlType.Freeze)
            {
                if (OwnerEntity != null)
                {
                    OwnerEntity.InterruptCurrentAction(SkillCastInterruptSource.Freeze);
                    EventBus.RaiseCrowdControlInterrupted(OwnerEntity, ccType);
                }
            }

            EventBus.RaiseCrowdControlApplied(OwnerEntity, ccType, effectiveDuration);
            Debug.Log($"[CC:APPLIED] Target={OwnerEntity.EntityName}, Type={ccType}, BaseDur={baseDuration:F1}s, EffDur={effectiveDuration:F1}s (Res={resistance * 100f:F0}%), Tier={powerTier}");

            applyReason = "Applied";
            return true;
        }

        public bool RemoveCrowdControl(string ccId)
        {
            if (activeCrowdControls.TryGetValue(ccId, out var cc))
            {
                activeCrowdControls.Remove(ccId);
                OnCrowdControlExpired?.Invoke(cc);
                EventBus.RaiseCrowdControlExpired(OwnerEntity, cc.CcType);
                return true;
            }
            return false;
        }

        public void ClearAllCrowdControl()
        {
            if (activeCrowdControls.Count > 0)
            {
                var keys = new List<string>(activeCrowdControls.Keys);
                foreach (var k in keys)
                {
                    RemoveCrowdControl(k);
                }
            }
        }

        // --- FREEZE SHATTER EXTENSION ---
        public bool TryTriggerFreezeShatter(Entity attacker)
        {
            if (!IsFrozen) return false;

            EventBus.RaiseFreezeShattered(OwnerEntity, attacker);
            OnFreezeShatterTriggered?.Invoke(OwnerEntity, attacker);
            Debug.Log($"[FREEZE_SHATTER] Target {OwnerEntity?.EntityName} shattered by {attacker?.EntityName}!");
            return true;
        }

        // --- NUMERICAL DEBUFFS ---
        public void ApplyDebuff(DebuffEffectDefinitionSO debuffDef, Entity source)
        {
            if (debuffDef == null || string.IsNullOrEmpty(debuffDef.DebuffId)) return;
            ApplyDebuff(
                debuffDef.DebuffId,
                debuffDef.DebuffType,
                debuffDef.ModifierMode,
                debuffDef.ModifierValue,
                debuffDef.Duration,
                debuffDef.PowerTier,
                source,
                debuffDef.StackingPolicy,
                debuffDef.MaxStacks);
        }

        public void ApplyDebuff(
            string debuffId,
            DebuffType debuffType,
            DebuffModifierMode modifierMode,
            float modifierValue,
            float duration,
            EffectPowerTier powerTier,
            Entity source,
            StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration,
            int maxStacks = 5)
        {
            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;

            if (activeDebuffs.TryGetValue(debuffId, out var existing))
            {
                switch (stackPolicy)
                {
                    case StatusStackPolicy.Replace:
                        existing.Replace(now);
                        break;
                    case StatusStackPolicy.RefreshDuration:
                        existing.RefreshDuration(now);
                        break;
                    case StatusStackPolicy.Stack:
                        existing.AddStack(now);
                        break;
                }
                OnDebuffApplied?.Invoke(existing);
                EventBus.RaiseDebuffApplied(OwnerEntity, debuffId);
            }
            else
            {
                var newDebuff = new RuntimeDebuffInstance(
                    debuffId,
                    debuffType,
                    modifierMode,
                    modifierValue,
                    duration,
                    powerTier,
                    source,
                    OwnerEntity,
                    now,
                    stackPolicy,
                    maxStacks);
                activeDebuffs[debuffId] = newDebuff;
                OnDebuffApplied?.Invoke(newDebuff);
                EventBus.RaiseDebuffApplied(OwnerEntity, debuffId);
            }

            NotifyStatsChanged();
        }

        public bool HasDebuff(string debuffId)
        {
            if (string.IsNullOrEmpty(debuffId)) return false;
            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
            return activeDebuffs.TryGetValue(debuffId, out var debuff) && !debuff.IsExpired && !debuff.IsExpiredAt(now);
        }

        public RuntimeDebuffInstance GetDebuff(string debuffId)
        {
            if (string.IsNullOrEmpty(debuffId)) return null;
            activeDebuffs.TryGetValue(debuffId, out var debuff);
            return debuff;
        }

        public bool RemoveDebuff(string debuffId)
        {
            if (activeDebuffs.TryGetValue(debuffId, out var debuff))
            {
                activeDebuffs.Remove(debuffId);
                OnDebuffExpired?.Invoke(debuff);
                EventBus.RaiseDebuffExpired(OwnerEntity, debuffId);
                NotifyStatsChanged();
                return true;
            }
            return false;
        }

        public Dictionary<StatType, float> GetActiveDebuffModifiers()
        {
            var modifiers = new Dictionary<StatType, float>();

            foreach (var kvp in activeDebuffs)
            {
                var debuff = kvp.Value;
                if (debuff != null && !debuff.IsExpired)
                {
                    StatType? statType = MapDebuffToStatType(debuff.DebuffType);
                    if (statType.HasValue)
                    {
                        if (!modifiers.ContainsKey(statType.Value))
                        {
                            modifiers[statType.Value] = 0f;
                        }

                        if (debuff.ModifierMode == DebuffModifierMode.Flat)
                        {
                            modifiers[statType.Value] += debuff.TotalValue;
                        }
                        else if (debuff.ModifierMode == DebuffModifierMode.Percentage)
                        {
                            float baseVal = OwnerEntity != null && OwnerEntity.Stats != null ? OwnerEntity.Stats.GetValue(statType.Value, 0f) : 0f;
                            modifiers[statType.Value] += baseVal * (debuff.TotalValue / 100f);
                        }
                    }
                }
            }

            return modifiers;
        }

        private static StatType? MapDebuffToStatType(DebuffType debuffType)
        {
            switch (debuffType)
            {
                case DebuffType.Attack: return StatType.Attack;
                case DebuffType.Defense: return StatType.Defense;
                case DebuffType.AttackSpeed: return null; // Handled directly in AttackComponent via GetAttackIntervalModifier()
                case DebuffType.MoveSpeed: return StatType.MoveSpeed;
                case DebuffType.CritRate: return StatType.CritRate;
                case DebuffType.DodgeRate: return StatType.Dodge;
                default: return null;
            }
        }

        public float GetRageGainMultiplier()
        {
            float multiplier = 1f;
            foreach (var kvp in activeDebuffs)
            {
                var debuff = kvp.Value;
                if (debuff != null && !debuff.IsExpired && debuff.DebuffType == DebuffType.RageGain)
                {
                    if (debuff.ModifierMode == DebuffModifierMode.Flat)
                        multiplier -= debuff.TotalValue;
                    else
                        multiplier -= (debuff.TotalValue / 100f);
                }
            }
            return Mathf.Max(0f, multiplier);
        }

        public float GetHealingReceivedMultiplier()
        {
            float multiplier = 1f;
            foreach (var kvp in activeDebuffs)
            {
                var debuff = kvp.Value;
                if (debuff != null && !debuff.IsExpired && debuff.DebuffType == DebuffType.HealingReceived)
                {
                    if (debuff.ModifierMode == DebuffModifierMode.Flat)
                        multiplier -= debuff.TotalValue;
                    else
                        multiplier -= (debuff.TotalValue / 100f);
                }
            }
            return Mathf.Max(0f, multiplier);
        }

        public float GetDamageDealtMultiplier()
        {
            float multiplier = 1f;
            foreach (var kvp in activeDebuffs)
            {
                var debuff = kvp.Value;
                if (debuff != null && !debuff.IsExpired && debuff.DebuffType == DebuffType.DamageDealt)
                {
                    if (debuff.ModifierMode == DebuffModifierMode.Flat)
                        multiplier -= debuff.TotalValue;
                    else
                        multiplier -= (debuff.TotalValue / 100f);
                }
            }
            return Mathf.Max(0f, multiplier);
        }

        public float GetAttackIntervalModifier()
        {
            float penalty = 0f;
            foreach (var kvp in activeDebuffs)
            {
                var debuff = kvp.Value;
                if (debuff != null && !debuff.IsExpired && debuff.DebuffType == DebuffType.AttackSpeed)
                {
                    if (debuff.ModifierMode == DebuffModifierMode.Flat)
                    {
                        penalty += debuff.TotalValue;
                    }
                    else
                    {
                        float baseInterval = OwnerEntity != null && OwnerEntity.Attack != null ? OwnerEntity.Attack.AttackInterval : 1.5f;
                        penalty += baseInterval * (debuff.TotalValue / 100f);
                    }
                }
            }
            return penalty;
        }

        // --- BUFFS (P07.4) ---
        public void ApplyBuff(BuffEffectDefinitionSO buffDef, Entity source)
        {
            if (buffDef == null || string.IsNullOrEmpty(buffDef.BuffId)) return;

            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;

            if (activeBuffs.TryGetValue(buffDef.BuffId, out RuntimeBuffInstance existing))
            {
                switch (buffDef.StackingPolicy)
                {
                    case BuffStackingPolicy.Replace:
                        existing.Replace(now);
                        break;
                    case BuffStackingPolicy.RefreshDuration:
                        existing.RefreshDuration(now);
                        break;
                    case BuffStackingPolicy.Stack:
                        existing.AddStack(now);
                        break;
                }
                OnBuffApplied?.Invoke(existing);
            }
            else
            {
                var newBuff = new RuntimeBuffInstance(buffDef, source, OwnerEntity, now);
                activeBuffs[buffDef.BuffId] = newBuff;
                OnBuffApplied?.Invoke(newBuff);
            }

            NotifyStatsChanged();
        }

        public bool HasBuff(string buffId)
        {
            if (string.IsNullOrEmpty(buffId)) return false;
            return activeBuffs.TryGetValue(buffId, out var buff) && !buff.IsExpired;
        }

        public RuntimeBuffInstance GetBuff(string buffId)
        {
            if (string.IsNullOrEmpty(buffId)) return null;
            activeBuffs.TryGetValue(buffId, out var buff);
            return buff;
        }

        public bool RemoveBuff(string buffId)
        {
            if (activeBuffs.TryGetValue(buffId, out var buff))
            {
                activeBuffs.Remove(buffId);
                OnBuffExpired?.Invoke(buff);
                NotifyStatsChanged();
                return true;
            }
            return false;
        }

        public Dictionary<StatType, float> GetActiveBuffModifiers()
        {
            var modifiers = new Dictionary<StatType, float>();

            foreach (var kvp in activeBuffs)
            {
                var buff = kvp.Value;
                if (buff != null && !buff.IsExpired)
                {
                    if (!modifiers.ContainsKey(buff.StatType))
                    {
                        modifiers[buff.StatType] = 0f;
                    }
                    modifiers[buff.StatType] += buff.TotalValue;
                }
            }

            return modifiers;
        }

        // --- STATUSES / DoT (P07.5) ---
        public void ApplyStatus(StatusEffectDefinitionSO statusDef, Entity source)
        {
            if (statusDef == null || string.IsNullOrEmpty(statusDef.StatusId)) return;

            float now = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;

            if (activeStatuses.TryGetValue(statusDef.StatusId, out RuntimeStatusInstance existing))
            {
                switch (statusDef.StackPolicy)
                {
                    case StatusStackPolicy.Replace:
                        existing.Replace(now);
                        break;
                    case StatusStackPolicy.RefreshDuration:
                        existing.RefreshDuration(now);
                        break;
                    case StatusStackPolicy.Stack:
                        existing.AddStack(now);
                        break;
                }
                OnStatusApplied?.Invoke(existing);
                Debug.Log($"[STATUS_STACK] Status={statusDef.StatusId} on {OwnerEntity?.EntityName}: Policy={statusDef.StackPolicy}, Stacks={existing.StackCount}/{existing.MaxStacks}, Remaining={existing.RemainingDuration:F1}s");
            }
            else
            {
                var newStatus = new RuntimeStatusInstance(statusDef, source, OwnerEntity, now);
                activeStatuses[statusDef.StatusId] = newStatus;
                OnStatusApplied?.Invoke(newStatus);
                Debug.Log($"[STATUS] Applied={statusDef.StatusId} to {OwnerEntity?.EntityName}: Category={statusDef.Category}, Dur={statusDef.DefaultDuration:F1}s, Interval={statusDef.TickInterval:F1}s, Stacks=1/{newStatus.MaxStacks}");
            }
        }

        public bool HasStatus(string statusId)
        {
            if (string.IsNullOrEmpty(statusId)) return false;
            return activeStatuses.TryGetValue(statusId, out var status) && !status.IsExpired;
        }

        public RuntimeStatusInstance GetStatus(string statusId)
        {
            if (string.IsNullOrEmpty(statusId)) return null;
            activeStatuses.TryGetValue(statusId, out var status);
            return status;
        }

        public bool RemoveStatus(string statusId)
        {
            if (activeStatuses.TryGetValue(statusId, out var status))
            {
                activeStatuses.Remove(statusId);
                OnStatusExpired?.Invoke(status);
                return true;
            }
            return false;
        }

        // =========================================================================
        // P07.8 SHIELD / BARRIER SYSTEM
        // =========================================================================

        public bool ApplyShield(ShieldEffectDefinitionSO def, Entity source, out string applyReason)
        {
            if (def == null || string.IsNullOrEmpty(def.ShieldId))
            {
                applyReason = "Invalid Shield Definition";
                return false;
            }

            float calculatedAmount = def.ShieldValue;
            if (def.AmountMode == ShieldAmountMode.MaxHpPercentage && OwnerEntity != null && OwnerEntity.Health != null)
            {
                calculatedAmount = (def.ShieldValue / 100f) * OwnerEntity.Health.MaxHealth;
            }
            else if (def.AmountMode == ShieldAmountMode.AttackMultiplier && source != null && source.Stats != null)
            {
                calculatedAmount = def.ShieldValue * source.Stats.GetValue(StatType.Attack, 10f);
            }

            bool success = ApplyShield(
                def.ShieldId,
                calculatedAmount,
                def.Duration,
                def.Priority,
                def.StackingPolicy,
                source,
                def.MaxStacks,
                def.MaxAmount,
                def);

            applyReason = success ? "Success" : "Failed to apply";
            return success;
        }

        public bool ApplyShield(
            string shieldId,
            float amount,
            float duration = 0f,
            int priority = 0,
            ShieldStackPolicy stackPolicy = ShieldStackPolicy.RefreshDuration,
            Entity source = null,
            int maxStacks = 1,
            float maxAmount = -1f,
            ShieldEffectDefinitionSO definition = null)
        {
            if (OwnerEntity == null || !OwnerEntity.gameObject.activeInHierarchy || !OwnerEntity.IsAlive || amount <= 0f)
            {
                return false;
            }

            float currentTime = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;
            string key = string.IsNullOrEmpty(shieldId) ? "shield_default" : shieldId;

            if (stackPolicy == ShieldStackPolicy.Independent)
            {
                key = $"{key}_{++independentShieldCounter}";
            }

            if (activeShields.TryGetValue(key, out var existing))
            {
                if (existing.IsExpired || existing.IsDepleted)
                {
                    activeShields.Remove(key);
                    var newInst = new RuntimeShieldInstance(key, amount, duration, priority, stackPolicy, source, OwnerEntity, currentTime, definition, maxStacks, maxAmount);
                    activeShields[key] = newInst;
                    OnShieldApplied?.Invoke(newInst);
                    EventBus.RaiseShieldApplied(OwnerEntity, key, newInst.CurrentAmount, newInst.MaxAmount);
                    Debug.Log($"[SHIELD:APPLIED] Target={OwnerEntity?.EntityName}, ShieldId={key}, Amount={amount:F1}, Dur={duration:F1}s");
                    return true;
                }

                switch (stackPolicy)
                {
                    case ShieldStackPolicy.Additive:
                        existing.AddStack(amount, currentTime, maxAmount);
                        break;

                    case ShieldStackPolicy.RefreshDuration:
                        existing.ResetAmount(Mathf.Max(existing.CurrentAmount, amount), maxAmount);
                        existing.RefreshDuration(currentTime);
                        break;

                    case ShieldStackPolicy.Replace:
                        existing.Replace(amount, duration, currentTime, priority);
                        break;

                    case ShieldStackPolicy.Ignore:
                        return false;

                    case ShieldStackPolicy.Independent:
                    default:
                        break;
                }

                OnShieldApplied?.Invoke(existing);
                EventBus.RaiseShieldApplied(OwnerEntity, key, existing.CurrentAmount, existing.MaxAmount);
                Debug.Log($"[SHIELD:UPDATED] Target={OwnerEntity?.EntityName}, ShieldId={key}, Policy={stackPolicy}, Current={existing.CurrentAmount:F1}, Max={existing.MaxAmount:F1}");
                return true;
            }
            else
            {
                var newInst = new RuntimeShieldInstance(key, amount, duration, priority, stackPolicy, source, OwnerEntity, currentTime, definition, maxStacks, maxAmount);
                activeShields[key] = newInst;
                OnShieldApplied?.Invoke(newInst);
                EventBus.RaiseShieldApplied(OwnerEntity, key, newInst.CurrentAmount, newInst.MaxAmount);
                Debug.Log($"[SHIELD:APPLIED] Target={OwnerEntity?.EntityName}, ShieldId={key}, Amount={amount:F1}, Dur={duration:F1}s");
                return true;
            }
        }

        public RuntimeShieldInstance GetShield(string shieldId)
        {
            if (string.IsNullOrEmpty(shieldId)) return null;
            activeShields.TryGetValue(shieldId, out var shield);
            return shield;
        }

        public bool RemoveShield(string shieldId)
        {
            if (string.IsNullOrEmpty(shieldId)) return false;
            if (activeShields.TryGetValue(shieldId, out var shield))
            {
                activeShields.Remove(shieldId);
                OnShieldRemoved?.Invoke(shield);
                EventBus.RaiseShieldRemoved(OwnerEntity, shieldId);
                Debug.Log($"[SHIELD:REMOVED] Shield={shieldId} removed on {OwnerEntity?.EntityName}");
                return true;
            }
            return false;
        }

        public float AbsorbDamage(
            float incomingDamage,
            out float absorbedAmount,
            out float remainingDamage,
            Entity attacker = null,
            DamageType damageType = DamageType.BasicAttack)
        {
            absorbedAmount = 0f;
            remainingDamage = Mathf.Max(0f, incomingDamage);

            if (incomingDamage <= 0f || activeShields.Count == 0 || OwnerEntity == null || !OwnerEntity.IsAlive)
            {
                return 0f;
            }

            float currentTime = CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time;

            // 1. Purge expired shields before absorbing
            List<string> expiredKeys = null;
            foreach (var kvp in activeShields)
            {
                if (kvp.Value.IsExpired || (kvp.Value.Duration > 0f && currentTime >= kvp.Value.ExpirationTime))
                {
                    expiredKeys ??= new List<string>();
                    expiredKeys.Add(kvp.Key);
                }
            }
            if (expiredKeys != null)
            {
                foreach (var k in expiredKeys)
                {
                    if (activeShields.TryGetValue(k, out var s))
                    {
                        activeShields.Remove(k);
                        OnShieldExpired?.Invoke(s);
                        OnShieldRemoved?.Invoke(s);
                        EventBus.RaiseShieldExpired(OwnerEntity, k);
                        EventBus.RaiseShieldRemoved(OwnerEntity, k);
                    }
                }
            }

            if (activeShields.Count == 0)
            {
                return 0f;
            }

            // 2. Deterministic sorting: Priority DESC -> StartTime ASC (Oldest) -> ShieldId ASC (Ordinal)
            var sortedShields = new List<RuntimeShieldInstance>(activeShields.Values);
            sortedShields.Sort((a, b) =>
            {
                int pCmp = b.Priority.CompareTo(a.Priority);
                if (pCmp != 0) return pCmp;
                int tCmp = a.StartTime.CompareTo(b.StartTime);
                if (tCmp != 0) return tCmp;
                return string.Compare(a.ShieldId, b.ShieldId, StringComparison.Ordinal);
            });

            // 3. Sequentially absorb damage
            float remDmg = incomingDamage;
            float totalAbsorbed = 0f;
            List<string> depletedKeys = null;

            foreach (var shield in sortedShields)
            {
                if (remDmg <= 0f) break;
                if (shield.IsDepleted) continue;

                float absorbed = shield.Absorb(remDmg);
                if (absorbed > 0f)
                {
                    remDmg -= absorbed;
                    totalAbsorbed += absorbed;

                    var absorbResult = new ShieldAbsorbResult(
                        OwnerEntity,
                        shield.Source ?? attacker,
                        shield.ShieldId,
                        incomingDamage,
                        absorbed,
                        remDmg,
                        shield.CurrentAmount);

                    OnShieldAbsorbed?.Invoke(shield, absorbResult);
                    EventBus.RaiseShieldAbsorbed(OwnerEntity, absorbResult);

                    Debug.Log($"[SHIELD:ABSORB] Target={OwnerEntity?.EntityName}, Shield={shield.ShieldId}, Absorbed={absorbed:F1}, RemDmg={remDmg:F1}, RemShield={shield.CurrentAmount:F1}");
                }

                if (shield.IsDepleted)
                {
                    depletedKeys ??= new List<string>();
                    depletedKeys.Add(shield.ShieldId);
                }
            }

            // 4. Clean up depleted shields
            if (depletedKeys != null)
            {
                foreach (var id in depletedKeys)
                {
                    if (activeShields.TryGetValue(id, out var dep))
                    {
                        activeShields.Remove(id);
                        OnShieldDepleted?.Invoke(dep);
                        OnShieldRemoved?.Invoke(dep);
                        EventBus.RaiseShieldDepleted(OwnerEntity, id);
                        EventBus.RaiseShieldRemoved(OwnerEntity, id);
                        Debug.Log($"[SHIELD:DEPLETED] Shield={id} depleted on {OwnerEntity?.EntityName}");
                    }
                }
            }

            absorbedAmount = totalAbsorbed;
            remainingDamage = Mathf.Max(0f, remDmg);
            return totalAbsorbed;
        }

        // =========================================================================
        // P07.7 CLEANSE & DISPEL REMOVAL ENGINE
        // =========================================================================

        public static Func<int, int> RandomRangeProvider { get; set; } = null;

        private class RemovalCandidate
        {
            public StatusRemovalCategory Category;
            public string Id;
            public float StartTime;
            public int Priority;
            public int CurrentStacks;
            public Action<int, Entity> RemoveCallback;
        }

        public StatusRemovalResult Cleanse(CleanseEffectDefinitionSO cleanseDef, Entity source = null)
        {
            if (cleanseDef == null) return StatusRemovalResult.None;
            return Cleanse(
                cleanseDef.RemovalCategory,
                cleanseDef.MaxRemoveCount,
                cleanseDef.SelectionMode,
                cleanseDef.RemoveStacks,
                cleanseDef.SpecificStatusId,
                cleanseDef.SpecificCcType,
                cleanseDef.SpecificDebuffType,
                source);
        }

        public StatusRemovalResult Cleanse(
            StatusRemovalCategory category = StatusRemovalCategory.NegativeStatus,
            int maxCount = 1,
            StatusSelectionMode selectionMode = StatusSelectionMode.Oldest,
            int stacksToRemove = 0,
            string specificId = "",
            CrowdControlType specificCcType = CrowdControlType.None,
            DebuffType specificDebuffType = (DebuffType)(-1),
            Entity source = null)
        {
            if (OwnerEntity == null || !OwnerEntity.IsAlive) return StatusRemovalResult.None;

            var candidates = new List<RemovalCandidate>();

            // 1. Check Debuffs
            bool includeDebuffs = category == StatusRemovalCategory.NegativeStatus ||
                                  category == StatusRemovalCategory.Debuff ||
                                  (category == StatusRemovalCategory.SpecificStatus && !string.IsNullOrEmpty(specificId)) ||
                                  (category == StatusRemovalCategory.SpecificStatusType && specificDebuffType != (DebuffType)(-1));

            if (includeDebuffs)
            {
                foreach (var kvp in activeDebuffs)
                {
                    var debuff = kvp.Value;
                    if (debuff.IsExpired) continue;

                    if (!string.IsNullOrEmpty(specificId) && debuff.DebuffId != specificId) continue;
                    if (specificDebuffType != (DebuffType)(-1) && debuff.DebuffType != specificDebuffType) continue;

                    string id = debuff.DebuffId;
                    candidates.Add(new RemovalCandidate
                    {
                        Category = StatusRemovalCategory.Debuff,
                        Id = id,
                        StartTime = debuff.StartTime,
                        Priority = debuff.RemovalPriority,
                        CurrentStacks = debuff.StackCount,
                        RemoveCallback = (count, src) =>
                        {
                            if (!activeDebuffs.TryGetValue(id, out var inst)) return;
                            if (count > 0 && inst.StackCount > count)
                            {
                                int rem = inst.ReduceStacks(count);
                                EventBus.RaiseStatusStacksRemoved(OwnerEntity, id, count, rem, src);
                                NotifyStatsChanged();
                            }
                            else
                            {
                                activeDebuffs.Remove(id);
                                OnDebuffExpired?.Invoke(inst);
                                EventBus.RaiseDebuffExpired(OwnerEntity, id);
                                EventBus.RaiseDebuffCleansed(OwnerEntity, id, src);
                                EventBus.RaiseStatusRemoved(OwnerEntity, StatusRemovalCategory.Debuff, id, inst.StackCount, src);
                                NotifyStatsChanged();
                            }
                        }
                    });
                }
            }

            // 2. Check Crowd Controls (Anti-CC is protected; Freeze removal does NOT trigger shatter)
            bool includeCc = category == StatusRemovalCategory.NegativeStatus ||
                             category == StatusRemovalCategory.CrowdControl ||
                             (category == StatusRemovalCategory.SpecificStatus && !string.IsNullOrEmpty(specificId)) ||
                             (category == StatusRemovalCategory.SpecificStatusType && specificCcType != CrowdControlType.None);

            if (includeCc)
            {
                foreach (var kvp in activeCrowdControls)
                {
                    var cc = kvp.Value;
                    if (cc.IsExpired) continue;

                    if (!string.IsNullOrEmpty(specificId) && cc.CcId != specificId) continue;
                    if (specificCcType != CrowdControlType.None && cc.CcType != specificCcType) continue;

                    string id = cc.CcId;
                    CrowdControlType ccType = cc.CcType;
                    candidates.Add(new RemovalCandidate
                    {
                        Category = StatusRemovalCategory.CrowdControl,
                        Id = id,
                        StartTime = cc.StartTime,
                        Priority = cc.RemovalPriority,
                        CurrentStacks = 1,
                        RemoveCallback = (count, src) =>
                        {
                            if (!activeCrowdControls.TryGetValue(id, out var inst)) return;
                            activeCrowdControls.Remove(id);
                            OnCrowdControlExpired?.Invoke(inst);
                            EventBus.RaiseCrowdControlExpired(OwnerEntity, ccType);
                            EventBus.RaiseCrowdControlCleansed(OwnerEntity, ccType, src);
                            EventBus.RaiseStatusRemoved(OwnerEntity, StatusRemovalCategory.CrowdControl, id, 1, src);
                        }
                    });
                }
            }

            // 3. Check DoT / Statuses
            bool includeStatuses = (category == StatusRemovalCategory.NegativeStatus && specificCcType == CrowdControlType.None && specificDebuffType == (DebuffType)(-1)) ||
                                   category == StatusRemovalCategory.DoT ||
                                   (category == StatusRemovalCategory.SpecificStatus && !string.IsNullOrEmpty(specificId));

            if (includeStatuses)
            {
                foreach (var kvp in activeStatuses)
                {
                    var status = kvp.Value;
                    if (status.IsExpired) continue;

                    if (!string.IsNullOrEmpty(specificId) && status.StatusId != specificId) continue;
                    if (category == StatusRemovalCategory.DoT && status.Category != StatusCategory.Dot) continue;

                    string id = status.StatusId;
                    var cat = status.Category == StatusCategory.Dot ? StatusRemovalCategory.DoT : StatusRemovalCategory.NegativeStatus;
                    candidates.Add(new RemovalCandidate
                    {
                        Category = cat,
                        Id = id,
                        StartTime = status.StartTime,
                        Priority = status.RemovalPriority,
                        CurrentStacks = status.StackCount,
                        RemoveCallback = (count, src) =>
                        {
                            if (!activeStatuses.TryGetValue(id, out var inst)) return;
                            if (count > 0 && inst.StackCount > count)
                            {
                                int rem = inst.ReduceStacks(count);
                                EventBus.RaiseStatusStacksRemoved(OwnerEntity, id, count, rem, src);
                            }
                            else
                            {
                                activeStatuses.Remove(id);
                                OnStatusExpired?.Invoke(inst);
                                EventBus.RaiseStatusRemoved(OwnerEntity, cat, id, inst.StackCount, src);
                            }
                        }
                    });
                }
            }

            return ExecuteRemoval(candidates, maxCount, selectionMode, stacksToRemove, source);
        }

        public StatusRemovalResult Dispel(DispelEffectDefinitionSO dispelDef, Entity source = null)
        {
            if (dispelDef == null) return StatusRemovalResult.None;
            return Dispel(
                dispelDef.RemovalCategory,
                dispelDef.MaxRemoveCount,
                dispelDef.SelectionMode,
                dispelDef.RemoveStacks,
                dispelDef.SpecificBuffId,
                dispelDef.SpecificStatType,
                source);
        }

        public StatusRemovalResult Dispel(
            StatusRemovalCategory category = StatusRemovalCategory.PositiveStatus,
            int maxCount = 1,
            StatusSelectionMode selectionMode = StatusSelectionMode.Oldest,
            int stacksToRemove = 0,
            string specificId = "",
            StatType specificStatType = (StatType)(-1),
            Entity source = null)
        {
            if (OwnerEntity == null || !OwnerEntity.IsAlive) return StatusRemovalResult.None;

            var candidates = new List<RemovalCandidate>();
            bool includeBuffs = (category == StatusRemovalCategory.PositiveStatus || category == StatusRemovalCategory.Buff || category == StatusRemovalCategory.SpecificStatus || category == StatusRemovalCategory.SpecificStatusType);
            if (includeBuffs)
            {
                foreach (var kvp in activeBuffs)
                {
                    var buff = kvp.Value;
                    if (buff.IsExpired) continue;

                    if (!string.IsNullOrEmpty(specificId) && buff.BuffId != specificId) continue;
                    if (specificStatType != (StatType)(-1) && buff.StatType != specificStatType) continue;

                    string id = buff.BuffId;
                    candidates.Add(new RemovalCandidate
                    {
                        Category = StatusRemovalCategory.Buff,
                        Id = id,
                        StartTime = buff.StartTime,
                        Priority = buff.RemovalPriority,
                        CurrentStacks = buff.StackCount,
                        RemoveCallback = (count, src) =>
                        {
                            if (!activeBuffs.TryGetValue(id, out var inst)) return;
                            if (count > 0 && inst.StackCount > count)
                            {
                                int rem = inst.ReduceStacks(count);
                                EventBus.RaiseStatusStacksRemoved(OwnerEntity, id, count, rem, src);
                                NotifyStatsChanged();
                            }
                            else
                            {
                                activeBuffs.Remove(id);
                                OnBuffExpired?.Invoke(inst);
                                EventBus.RaiseBuffDispelled(OwnerEntity, id, src);
                                EventBus.RaiseStatusRemoved(OwnerEntity, StatusRemovalCategory.Buff, id, inst.StackCount, src);
                                NotifyStatsChanged();
                            }
                        }
                    });
                }
            }

            bool includeShields = (category == StatusRemovalCategory.Shield || (category == StatusRemovalCategory.SpecificStatus && !string.IsNullOrEmpty(specificId) && activeShields.ContainsKey(specificId)));
            if (includeShields)
            {
                foreach (var kvp in activeShields)
                {
                    var shield = kvp.Value;
                    if (shield.IsExpired) continue;

                    if (!string.IsNullOrEmpty(specificId) && shield.ShieldId != specificId) continue;

                    string id = shield.ShieldId;
                    candidates.Add(new RemovalCandidate
                    {
                        Category = StatusRemovalCategory.Shield,
                        Id = id,
                        StartTime = shield.StartTime,
                        Priority = shield.Priority,
                        CurrentStacks = shield.StackCount,
                        RemoveCallback = (count, src) =>
                        {
                            if (!activeShields.TryGetValue(id, out var inst)) return;
                            activeShields.Remove(id);
                            OnShieldRemoved?.Invoke(inst);
                            EventBus.RaiseShieldRemoved(OwnerEntity, id);
                            EventBus.RaiseStatusRemoved(OwnerEntity, StatusRemovalCategory.Shield, id, 1, src);
                        }
                    });
                }
            }

            return ExecuteRemoval(candidates, maxCount, selectionMode, stacksToRemove, source);
        }

        private StatusRemovalResult ExecuteRemoval(
            List<RemovalCandidate> candidates,
            int maxCount,
            StatusSelectionMode selectionMode,
            int stacksToRemove,
            Entity source)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return StatusRemovalResult.None;
            }

            // Selection sorting
            switch (selectionMode)
            {
                case StatusSelectionMode.Oldest:
                    candidates.Sort((a, b) =>
                    {
                        int cmp = a.StartTime.CompareTo(b.StartTime);
                        return cmp != 0 ? cmp : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
                    });
                    break;
                case StatusSelectionMode.Newest:
                    candidates.Sort((a, b) =>
                    {
                        int cmp = b.StartTime.CompareTo(a.StartTime);
                        return cmp != 0 ? cmp : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
                    });
                    break;
                case StatusSelectionMode.HighestPriority:
                    candidates.Sort((a, b) =>
                    {
                        int cmp = b.Priority.CompareTo(a.Priority);
                        if (cmp != 0) return cmp;
                        cmp = a.StartTime.CompareTo(b.StartTime);
                        return cmp != 0 ? cmp : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
                    });
                    break;
                case StatusSelectionMode.LowestPriority:
                    candidates.Sort((a, b) =>
                    {
                        int cmp = a.Priority.CompareTo(b.Priority);
                        if (cmp != 0) return cmp;
                        cmp = a.StartTime.CompareTo(b.StartTime);
                        return cmp != 0 ? cmp : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
                    });
                    break;
                case StatusSelectionMode.Random:
                    for (int i = candidates.Count - 1; i > 0; i--)
                    {
                        int randIdx = RandomRangeProvider != null 
                            ? RandomRangeProvider(i + 1) 
                            : UnityEngine.Random.Range(0, i + 1);
                        randIdx = Mathf.Clamp(randIdx, 0, i);
                        var temp = candidates[i];
                        candidates[i] = candidates[randIdx];
                        candidates[randIdx] = temp;
                    }
                    break;
                case StatusSelectionMode.All:
                default:
                    break;
            }

            int countToTake = (selectionMode == StatusSelectionMode.All || maxCount <= 0) 
                ? candidates.Count 
                : Mathf.Min(maxCount, candidates.Count);

            var result = new StatusRemovalResult
            {
                Success = true,
                StatusesRemovedCount = 0,
                StacksRemovedCount = 0,
                RemovedStatusIds = new List<string>()
            };

            for (int i = 0; i < countToTake; i++)
            {
                var cand = candidates[i];
                int previousStacks = cand.CurrentStacks;
                cand.RemoveCallback(stacksToRemove, source);

                result.RemovedStatusIds.Add(cand.Id);
                if (stacksToRemove > 0 && previousStacks > stacksToRemove)
                {
                    result.StacksRemovedCount += stacksToRemove;
                }
                else
                {
                    result.StatusesRemovedCount++;
                    result.StacksRemovedCount += previousStacks;
                }
            }

            result.Message = $"Successfully removed {result.StatusesRemovedCount} statuses ({result.StacksRemovedCount} stacks).";
            return result;
        }

        public StatusRemovalResult CleanseAllNegativeStatuses(Entity source = null)
        {
            return Cleanse(StatusRemovalCategory.NegativeStatus, 0, StatusSelectionMode.All, 0, null, CrowdControlType.None, (DebuffType)(-1), source);
        }

        public StatusRemovalResult CleanseCrowdControl(Entity source = null)
        {
            return Cleanse(StatusRemovalCategory.CrowdControl, 0, StatusSelectionMode.All, 0, null, CrowdControlType.None, (DebuffType)(-1), source);
        }

        public StatusRemovalResult CleanseDebuffs(Entity source = null)
        {
            return Cleanse(StatusRemovalCategory.Debuff, 0, StatusSelectionMode.All, 0, null, CrowdControlType.None, (DebuffType)(-1), source);
        }

        public StatusRemovalResult DispelAllBuffs(Entity source = null)
        {
            return Dispel(StatusRemovalCategory.PositiveStatus, 0, StatusSelectionMode.All, 0, null, (StatType)(-1), source);
        }

        public void ClearAll()
        {
            activeBuffs.Clear();
            activeDebuffs.Clear();
            activeCrowdControls.Clear();
            activeStatuses.Clear();
            activeShields.Clear();
            antiCCImmunityEndTime = 0f;
            NotifyStatsChanged();
        }

        private void Update()
        {
            Tick(CooldownManager.TimeProvider != null ? CooldownManager.TimeProvider.CurrentTime : Time.time);
        }

        public void Tick(float currentTime)
        {
            // 1. Process expired buffs
            List<string> expiredBuffs = null;
            foreach (var kvp in activeBuffs)
            {
                if (kvp.Value.IsExpired || (kvp.Value.Duration > 0f && currentTime >= kvp.Value.ExpirationTime))
                {
                    expiredBuffs ??= new List<string>();
                    expiredBuffs.Add(kvp.Key);
                }
            }

            if (expiredBuffs != null)
            {
                foreach (var id in expiredBuffs)
                {
                    var buff = activeBuffs[id];
                    activeBuffs.Remove(id);
                    OnBuffExpired?.Invoke(buff);
                }
                NotifyStatsChanged();
            }

            // 2. Process expired debuffs
            List<string> expiredDebuffs = null;
            foreach (var kvp in activeDebuffs)
            {
                if (kvp.Value.IsExpired || kvp.Value.IsExpiredAt(currentTime))
                {
                    expiredDebuffs ??= new List<string>();
                    expiredDebuffs.Add(kvp.Key);
                }
            }

            if (expiredDebuffs != null)
            {
                foreach (var id in expiredDebuffs)
                {
                    var debuff = activeDebuffs[id];
                    activeDebuffs.Remove(id);
                    OnDebuffExpired?.Invoke(debuff);
                    EventBus.RaiseDebuffExpired(OwnerEntity, id);
                }
                NotifyStatsChanged();
            }

            // 3. Process expired crowd controls
            List<string> expiredCcs = null;
            foreach (var kvp in activeCrowdControls)
            {
                if (kvp.Value.IsExpired || kvp.Value.IsExpiredAt(currentTime))
                {
                    expiredCcs ??= new List<string>();
                    expiredCcs.Add(kvp.Key);
                }
            }

            if (expiredCcs != null)
            {
                foreach (var id in expiredCcs)
                {
                    var cc = activeCrowdControls[id];
                    activeCrowdControls.Remove(id);
                    OnCrowdControlExpired?.Invoke(cc);
                    EventBus.RaiseCrowdControlExpired(OwnerEntity, cc.CcType);
                }
            }

            // 4. Process active statuses (ticking & expiration)
            List<string> expiredStatuses = null;

            bool isTargetInvalidOrDead = OwnerEntity == null || !OwnerEntity.gameObject.activeInHierarchy || !OwnerEntity.IsAlive ||
                                         OwnerEntity.Health == null || OwnerEntity.Health.CurrentHealth <= 0f;

            if (isTargetInvalidOrDead)
            {
                // Dead entity safety: if owner is dead or invalid, stop all ticks and expire all statuses
                if (activeStatuses.Count > 0)
                {
                    expiredStatuses = new List<string>(activeStatuses.Keys);
                }
                if (activeDebuffs.Count > 0)
                {
                    activeDebuffs.Clear();
                }
                if (activeCrowdControls.Count > 0)
                {
                    activeCrowdControls.Clear();
                }
            }
            else
            {
                var statusKeys = new List<string>(activeStatuses.Keys);
                foreach (var id in statusKeys)
                {
                    if (!activeStatuses.TryGetValue(id, out var status) || status == null)
                    {
                        continue;
                    }

                    if (status.IsExpired || (status.Duration > 0f && currentTime >= status.ExpirationTime))
                    {
                        expiredStatuses ??= new List<string>();
                        expiredStatuses.Add(id);
                        continue;
                    }

                    // Check if tick is ready
                    if (status.CheckTick(currentTime))
                    {
                        ExecuteStatusTick(status);

                        // If owner died from the tick, stop further ticks and expire all
                        if (!OwnerEntity.IsAlive || OwnerEntity.Health.CurrentHealth <= 0f)
                        {
                            expiredStatuses = new List<string>(activeStatuses.Keys);
                            break;
                        }
                    }

                    // Re-check expiration after tick
                    if (status.IsExpired || (status.Duration > 0f && currentTime >= status.ExpirationTime))
                    {
                        expiredStatuses ??= new List<string>();
                        if (!expiredStatuses.Contains(id))
                        {
                            expiredStatuses.Add(id);
                        }
                    }
                }
            }

            if (expiredStatuses != null)
            {
                foreach (var id in expiredStatuses)
                {
                    if (activeStatuses.TryGetValue(id, out var status))
                    {
                        activeStatuses.Remove(id);
                        OnStatusExpired?.Invoke(status);
                        Debug.Log($"[STATUS_EXPIRE] Status={id} expired on {OwnerEntity?.EntityName}");
                    }
                }
            }

            // 5. Process expired shields
            List<string> expiredShields = null;
            foreach (var kvp in activeShields)
            {
                if (kvp.Value.IsExpired || (kvp.Value.Duration > 0f && currentTime >= kvp.Value.ExpirationTime))
                {
                    expiredShields ??= new List<string>();
                    expiredShields.Add(kvp.Key);
                }
            }

            if (expiredShields != null)
            {
                foreach (var id in expiredShields)
                {
                    if (activeShields.TryGetValue(id, out var shield))
                    {
                        activeShields.Remove(id);
                        OnShieldExpired?.Invoke(shield);
                        OnShieldRemoved?.Invoke(shield);
                        EventBus.RaiseShieldExpired(OwnerEntity, id);
                        EventBus.RaiseShieldRemoved(OwnerEntity, id);
                        Debug.Log($"[SHIELD:EXPIRED] Shield={id} expired on {OwnerEntity?.EntityName}");
                    }
                }
            }
        }

        private void ExecuteStatusTick(RuntimeStatusInstance status)
        {
            if (status == null || OwnerEntity == null || !OwnerEntity.IsAlive || OwnerEntity.Health == null || OwnerEntity.Health.CurrentHealth <= 0f)
            {
                return;
            }

            if (status.DamagePerTick > 0f || status.Category == StatusCategory.Dot)
            {
                DamageResult result = DamageCalculator.CalculateDotDamage(
                    status.Source,
                    OwnerEntity,
                    status.DamagePerTick,
                    status.StackCount,
                    status.Definition != null ? status.Definition.CombatConfig : null,
                    status.Definition != null ? status.Definition.DamageType : DamageType.Skill);

                OwnerEntity.Health.TakeDamage(result);
                EventBus.RaiseEntityDamaged(OwnerEntity, result);
                OnStatusTick?.Invoke(status, result);

                Debug.Log($"[STATUS_TICK] Tick: Status={status.StatusId} (x{status.StackCount}), Target={OwnerEntity.EntityName}, Dmg={result.FinalDamage:F1}, HP={OwnerEntity.Health.CurrentHealth:F1}/{OwnerEntity.Health.MaxHealth:F1}");
            }
        }

        public string GetDebugStatusSummary()
        {
            if (activeStatuses.Count == 0 && activeDebuffs.Count == 0 && activeCrowdControls.Count == 0) return "No active statuses.";
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in activeStatuses)
            {
                var s = kvp.Value;
                sb.AppendLine($"[Status:{s.StatusId}] Stacks: {s.StackCount}/{s.MaxStacks}, Rem: {s.RemainingDuration:F1}s, Dmg/Tick: {s.DamagePerTick * s.StackCount:F1}");
            }
            foreach (var kvp in activeDebuffs)
            {
                var d = kvp.Value;
                sb.AppendLine($"[Debuff:{d.DebuffId}] Type: {d.DebuffType}, Val: -{d.TotalValue:F1} ({d.ModifierMode}), Rem: {d.RemainingDuration:F1}s");
            }
            foreach (var kvp in activeCrowdControls)
            {
                var c = kvp.Value;
                sb.AppendLine($"[CC:{c.CcId}] Type: {c.CcType}, Rem: {c.RemainingDuration:F1}s, Tier: {c.PowerTier}");
            }
            return sb.ToString();
        }

        public void NotifyStatsChanged()
        {
            if (OwnerEntity != null)
            {
                OwnerEntity.RecalculateStats(true);
            }
        }
    }
}
