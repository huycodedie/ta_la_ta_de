using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "DispelEffect", menuName = "WuxiaGame/Combat/Effects/DispelEffect")]
    public class DispelEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Dispel Configuration")]
        [SerializeField] private StatusRemovalCategory removalCategory = StatusRemovalCategory.PositiveStatus;
        [SerializeField] private string specificBuffId = "";
        [SerializeField] private StatType specificStatType = (StatType)(-1);
        [SerializeField] private int maxRemoveCount = 1;
        [SerializeField] private int removeStacks = 0;
        [SerializeField] private StatusSelectionMode selectionMode = StatusSelectionMode.Oldest;

        public StatusRemovalCategory RemovalCategory => removalCategory;
        public string SpecificBuffId => specificBuffId;
        public StatType SpecificStatType => specificStatType;
        public int MaxRemoveCount => maxRemoveCount;
        public int RemoveStacks => removeStacks;
        public StatusSelectionMode SelectionMode => selectionMode;

        private void Reset()
        {
            effectType = SkillEffectType.Dispel;
            targetPolicy = SkillTargetPolicy.SingleTarget;
        }

        public void Initialize(
            StatusRemovalCategory category = StatusRemovalCategory.PositiveStatus,
            int maxCount = 1,
            StatusSelectionMode selectMode = StatusSelectionMode.Oldest,
            int stacksToRemove = 0,
            string specificId = "",
            StatType statType = (StatType)(-1),
            SkillTargetPolicy policy = SkillTargetPolicy.SingleTarget)
        {
            effectType = SkillEffectType.Dispel;
            targetPolicy = policy;
            removalCategory = category;
            maxRemoveCount = maxCount;
            selectionMode = selectMode;
            removeStacks = stacksToRemove;
            specificBuffId = specificId;
            specificStatType = statType;
        }

        public override SkillEffectExecutionResult Execute(
            SkillExecutionRequest request,
            Entity target,
            CombatConfigSO combatConfig)
        {
            if (target == null || !target.IsAlive || target.StatusController == null)
            {
                return SkillEffectExecutionResult.CreateFailure(
                    effectType,
                    target,
                    "Target is null, dead, or missing StatusController.",
                    request != null ? request.Source : null);
            }

            Entity source = request != null ? request.Source : target;
            var removalResult = target.StatusController.Dispel(this, source);

            return new SkillEffectExecutionResult(
                effectType,
                target,
                true,
                $"Dispel executed: {removalResult.StatusesRemovedCount} buffs dispelled, {removalResult.StacksRemovedCount} stacks removed.",
                null,
                1f,
                source);
        }
    }
}
