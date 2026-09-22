using System;
using System.Collections.Generic;
using WuxiaGame.Combat;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Items;
using WuxiaGame.Stats;

namespace WuxiaGame.Core
{
    public enum BattleState
    {
        None,
        InProgress,
        CombatActive = InProgress,
        Victory,
        Defeat,
        HeroDead,
        AwaitingPlayerStart,
        WaitingForPlayerCommand = AwaitingPlayerStart,
        CombatStarting,
        MonsterDead,
        EncounterTransition,
        LootPending,
        LOOT_PENDING = LootPending
    }

    public static class EventBus
    {
        // Combat Events
        public static event Action<Entity> OnEntitySpawned;
        public static event Action<Entity> OnEntityDied;
        public static event Action<Entity, DamageResult> OnEntityDamaged;
        public static event Action<Entity, float, float> OnRageChanged;
        public static event Action<BattleState> OnBattleStateChanged;
        public static event Action<bool> OnAutoBattleChanged;

        // Progression Events
        public static event Action<float, float> OnExpChanged;
        public static event Action<int> OnLevelChanged;
        public static event Action<int, int, Dictionary<StatType, float>> OnHeroLevelUp; // (newLevel, levelDelta, statDeltas)
        public static event Action<TitleConfigSO> OnTitleChanged;
        public static event Action<bool> OnBreakthroughStatusChanged;

        // Equipment / Drop / Loot Events
        public static event Action<EquipmentInstance> OnEquipmentDropped;
        public static event Action<EquipmentInstance> OnEquipmentAddedToInventory;
        public static event Action<EquipmentSlotType, EquipmentInstance> OnEquipmentEquipped;
        public static event Action<EquipmentInstance> OnLootDecisionRequested;
        public static event Action<EquipmentInstance> OnLootDecisionCompleted;

        // LootTier Progression Events
        public static event Action<LootTierConfigSO> OnLootTierChanged;
        public static event Action<int, int> OnLootTierProgressChanged; // (current, required)
        public static event Action OnLootTierUpgradeStarted;
        public static event Action<LootTierConfigSO> OnLootTierUpgradeCompleted;

        // Mind Method & Skill Events
        public static event Action<MindMethodDefinitionSO> OnActiveMindMethodChanged;
        public static event Action<string> OnMindMethodUnlocked;
        public static event Action<string, int> OnMindMethodLevelChanged; // (mindMethodId, newLevel)
        public static event Action<string> OnSkillUnlocked;
        public static event Action<SkillSlotType, string> OnSkillSelected; // (slot, skillId)
        public static event Action<SkillExecutionRequest> OnSkillExecutionRequested;
        public static event Action<SkillExecutionRequest, SkillExecutionResult> OnSkillExecutionSucceeded;
        public static event Action<SkillExecutionRequest, SkillExecutionResult> OnSkillExecutionFailed;
        public static event Action<Entity, SkillDefinitionSO, SkillCastInterruptSource> OnSkillCastInterrupted;

        // P07.6 CC & Debuff Events
        public static event Action<Entity, CrowdControlType, float> OnCrowdControlApplied;
        public static event Action<Entity, CrowdControlType> OnCrowdControlExpired;
        public static event Action<Entity, CrowdControlType> OnCrowdControlImmune;
        public static event Action<Entity, CrowdControlType> OnCrowdControlResisted;
        public static event Action<Entity, CrowdControlType> OnCrowdControlInterrupted;
        public static event Action<Entity, Entity> OnFreezeShattered;
        public static event Action<Entity, string> OnDebuffApplied;
        public static event Action<Entity, string> OnDebuffExpired;

        // P07.7 Cleanse & Dispel Events
        public static event Action<Entity, StatusRemovalCategory, string, int, Entity> OnStatusRemoved;
        public static event Action<Entity, string, Entity> OnBuffDispelled;
        public static event Action<Entity, string, Entity> OnDebuffCleansed;
        public static event Action<Entity, CrowdControlType, Entity> OnCrowdControlCleansed;
        public static event Action<Entity, string, int, int, Entity> OnStatusStacksRemoved;

        // P07.8 Shield Events
        public static event Action<Entity, string, float, float> OnShieldApplied;
        public static event Action<Entity, ShieldAbsorbResult> OnShieldAbsorbed;
        public static event Action<Entity, string> OnShieldDepleted;
        public static event Action<Entity, string> OnShieldExpired;
        public static event Action<Entity, string> OnShieldRemoved;

        public static void RaiseEntitySpawned(Entity entity)
        {
            OnEntitySpawned?.Invoke(entity);
        }

        public static void RaiseEntityDied(Entity entity)
        {
            OnEntityDied?.Invoke(entity);
        }

        public static void RaiseEntityDamaged(Entity target, DamageResult result)
        {
            OnEntityDamaged?.Invoke(target, result);
        }

        public static void RaiseRageChanged(Entity entity, float currentRage, float maxRage)
        {
            OnRageChanged?.Invoke(entity, currentRage, maxRage);
        }

        public static void RaiseBattleStateChanged(BattleState newState)
        {
            OnBattleStateChanged?.Invoke(newState);
        }

        public static void RaiseAutoBattleChanged(bool isAuto)
        {
            OnAutoBattleChanged?.Invoke(isAuto);
        }

        public static void RaiseExpChanged(float currentExp, float requiredExp)
        {
            OnExpChanged?.Invoke(currentExp, requiredExp);
        }

        public static void RaiseLevelChanged(int newLevel)
        {
            OnLevelChanged?.Invoke(newLevel);
        }

        public static void RaiseHeroLevelUp(int newLevel, int levelDelta, Dictionary<StatType, float> statDeltas)
        {
            OnHeroLevelUp?.Invoke(newLevel, levelDelta, statDeltas);
        }

        public static void RaiseTitleChanged(TitleConfigSO newTitle)
        {
            OnTitleChanged?.Invoke(newTitle);
        }

        public static void RaiseBreakthroughStatusChanged(bool canBreakthrough)
        {
            OnBreakthroughStatusChanged?.Invoke(canBreakthrough);
        }

        public static void RaiseEquipmentDropped(EquipmentInstance item)
        {
            OnEquipmentDropped?.Invoke(item);
        }

        public static void RaiseEquipmentAddedToInventory(EquipmentInstance item)
        {
            OnEquipmentAddedToInventory?.Invoke(item);
        }

        public static void RaiseEquipmentEquipped(EquipmentSlotType slot, EquipmentInstance item)
        {
            OnEquipmentEquipped?.Invoke(slot, item);
        }

        public static void RaiseLootDecisionRequested(EquipmentInstance item)
        {
            OnLootDecisionRequested?.Invoke(item);
        }

        public static void RaiseLootDecisionCompleted(EquipmentInstance item)
        {
            OnLootDecisionCompleted?.Invoke(item);
        }

        public static void RaiseLootTierChanged(LootTierConfigSO newTier)
        {
            OnLootTierChanged?.Invoke(newTier);
        }

        public static void RaiseLootTierProgressChanged(int current, int required)
        {
            OnLootTierProgressChanged?.Invoke(current, required);
        }

        public static void RaiseLootTierUpgradeStarted()
        {
            OnLootTierUpgradeStarted?.Invoke();
        }

        public static void RaiseLootTierUpgradeCompleted(LootTierConfigSO newTier)
        {
            OnLootTierUpgradeCompleted?.Invoke(newTier);
        }

        public static void RaiseActiveMindMethodChanged(MindMethodDefinitionSO newMindMethod)
        {
            OnActiveMindMethodChanged?.Invoke(newMindMethod);
        }

        public static void RaiseMindMethodUnlocked(string mindMethodId)
        {
            OnMindMethodUnlocked?.Invoke(mindMethodId);
        }

        public static void RaiseMindMethodLevelChanged(string mindMethodId, int newLevel)
        {
            OnMindMethodLevelChanged?.Invoke(mindMethodId, newLevel);
        }

        public static void RaiseSkillUnlocked(string skillId)
        {
            OnSkillUnlocked?.Invoke(skillId);
        }

        public static void RaiseSkillSelected(SkillSlotType slot, string skillId)
        {
            OnSkillSelected?.Invoke(slot, skillId);
        }

        public static void RaiseSkillExecutionRequested(SkillExecutionRequest request)
        {
            OnSkillExecutionRequested?.Invoke(request);
        }

        public static void RaiseSkillExecutionSucceeded(SkillExecutionRequest request, SkillExecutionResult result)
        {
            OnSkillExecutionSucceeded?.Invoke(request, result);
        }

        public static void RaiseSkillExecutionFailed(SkillExecutionRequest request, SkillExecutionResult result)
        {
            OnSkillExecutionFailed?.Invoke(request, result);
        }

        public static void RaiseSkillCastInterrupted(Entity source, SkillDefinitionSO skill, SkillCastInterruptSource interruptSource)
        {
            OnSkillCastInterrupted?.Invoke(source, skill, interruptSource);
        }

        public static void RaiseCrowdControlApplied(Entity target, CrowdControlType type, float duration)
        {
            OnCrowdControlApplied?.Invoke(target, type, duration);
        }

        public static void RaiseCrowdControlExpired(Entity target, CrowdControlType type)
        {
            OnCrowdControlExpired?.Invoke(target, type);
        }

        public static void RaiseCrowdControlImmune(Entity target, CrowdControlType type)
        {
            OnCrowdControlImmune?.Invoke(target, type);
        }

        public static void RaiseCrowdControlResisted(Entity target, CrowdControlType type)
        {
            OnCrowdControlResisted?.Invoke(target, type);
        }

        public static void RaiseCrowdControlInterrupted(Entity target, CrowdControlType type)
        {
            OnCrowdControlInterrupted?.Invoke(target, type);
        }

        public static void RaiseFreezeShattered(Entity target, Entity attacker)
        {
            OnFreezeShattered?.Invoke(target, attacker);
        }

        public static void RaiseDebuffApplied(Entity target, string debuffId)
        {
            OnDebuffApplied?.Invoke(target, debuffId);
        }

        public static void RaiseDebuffExpired(Entity target, string debuffId)
        {
            OnDebuffExpired?.Invoke(target, debuffId);
        }

        public static void RaiseStatusRemoved(Entity target, StatusRemovalCategory category, string statusId, int stacksRemoved, Entity source)
        {
            OnStatusRemoved?.Invoke(target, category, statusId, stacksRemoved, source);
        }

        public static void RaiseBuffDispelled(Entity target, string buffId, Entity source)
        {
            OnBuffDispelled?.Invoke(target, buffId, source);
        }

        public static void RaiseDebuffCleansed(Entity target, string debuffId, Entity source)
        {
            OnDebuffCleansed?.Invoke(target, debuffId, source);
        }

        public static void RaiseCrowdControlCleansed(Entity target, CrowdControlType ccType, Entity source)
        {
            OnCrowdControlCleansed?.Invoke(target, ccType, source);
        }

        public static void RaiseStatusStacksRemoved(Entity target, string statusId, int stacksRemoved, int remainingStacks, Entity source)
        {
            OnStatusStacksRemoved?.Invoke(target, statusId, stacksRemoved, remainingStacks, source);
        }

        public static void RaiseShieldApplied(Entity target, string shieldId, float amount, float maxAmount)
        {
            OnShieldApplied?.Invoke(target, shieldId, amount, maxAmount);
        }

        public static void RaiseShieldAbsorbed(Entity target, ShieldAbsorbResult result)
        {
            OnShieldAbsorbed?.Invoke(target, result);
        }

        public static void RaiseShieldDepleted(Entity target, string shieldId)
        {
            OnShieldDepleted?.Invoke(target, shieldId);
        }

        public static void RaiseShieldExpired(Entity target, string shieldId)
        {
            OnShieldExpired?.Invoke(target, shieldId);
        }

        public static void RaiseShieldRemoved(Entity target, string shieldId)
        {
            OnShieldRemoved?.Invoke(target, shieldId);
        }

        public static void ClearAllListeners()
        {
            OnEntitySpawned = null;
            OnEntityDied = null;
            OnEntityDamaged = null;
            OnRageChanged = null;
            OnBattleStateChanged = null;
            OnAutoBattleChanged = null;
            OnExpChanged = null;
            OnLevelChanged = null;
            OnHeroLevelUp = null;
            OnTitleChanged = null;
            OnBreakthroughStatusChanged = null;
            OnEquipmentDropped = null;
            OnEquipmentAddedToInventory = null;
            OnEquipmentEquipped = null;
            OnLootDecisionRequested = null;
            OnLootDecisionCompleted = null;
            OnLootTierChanged = null;
            OnLootTierProgressChanged = null;
            OnLootTierUpgradeStarted = null;
            OnLootTierUpgradeCompleted = null;
            OnActiveMindMethodChanged = null;
            OnMindMethodUnlocked = null;
            OnMindMethodLevelChanged = null;
            OnSkillUnlocked = null;
            OnSkillSelected = null;
            OnSkillExecutionRequested = null;
            OnSkillExecutionSucceeded = null;
            OnSkillExecutionFailed = null;
            OnSkillCastInterrupted = null;
            OnCrowdControlApplied = null;
            OnCrowdControlExpired = null;
            OnCrowdControlImmune = null;
            OnCrowdControlResisted = null;
            OnCrowdControlInterrupted = null;
            OnFreezeShattered = null;
            OnDebuffApplied = null;
            OnDebuffExpired = null;
            OnStatusRemoved = null;
            OnBuffDispelled = null;
            OnDebuffCleansed = null;
            OnCrowdControlCleansed = null;
            OnStatusStacksRemoved = null;
            OnShieldApplied = null;
            OnShieldAbsorbed = null;
            OnShieldDepleted = null;
            OnShieldExpired = null;
            OnShieldRemoved = null;
        }
    }
}
