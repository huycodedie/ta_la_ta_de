using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    [CreateAssetMenu(fileName = "CleanseEffect", menuName = "WuxiaGame/Combat/Effects/CleanseEffect")]
    public class CleanseEffectDefinitionSO : SkillEffectDefinitionSO
    {
        [Header("Cleanse Configuration")]
        [SerializeField] private StatusRemovalCategory removalCategory = StatusRemovalCategory.NegativeStatus;
        [SerializeField] private string specificStatusId = "";
        [SerializeField] private CrowdControlType specificCcType = CrowdControlType.None;
        [SerializeField] private DebuffType specificDebuffType = (DebuffType)(-1);
        [SerializeField] private int maxRemoveCount = 1;
        [SerializeField] private int removeStacks = 0;
        [SerializeField] private StatusSelectionMode selectionMode = StatusSelectionMode.Oldest;

        public StatusRemovalCategory RemovalCategory => removalCategory;
        public string SpecificStatusId => specificStatusId;
        public CrowdControlType SpecificCcType => specificCcType;
        public DebuffType SpecificDebuffType => specificDebuffType;
        public int MaxRemoveCount => maxRemoveCount;
        public int RemoveStacks => removeStacks;
        public StatusSelectionMode SelectionMode => selectionMode;

        private void Reset()
        {
            effectType = SkillEffectType.Cleanse;
            targetPolicy = SkillTargetPolicy.Self;
        }

        public void Initialize(
            StatusRemovalCategory category = StatusRemovalCategory.NegativeStatus,
            int maxCount = 1,
            StatusSelectionMode selectMode = StatusSelectionMode.Oldest,
            int stacksToRemove = 0,
            string specificId = "",
            CrowdControlType ccType = CrowdControlType.None,
            DebuffType debuffType = (DebuffType)(-1),
            SkillTargetPolicy policy = SkillTargetPolicy.Self)
        {
            effectType = SkillEffectType.Cleanse;
            targetPolicy = policy;
            removalCategory = category;
            maxRemoveCount = maxCount;
            selectionMode = selectMode;
            removeStacks = stacksToRemove;
            specificStatusId = specificId;
            specificCcType = ccType;
            specificDebuffType = debuffType;
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
            var removalResult = target.StatusController.Cleanse(this, source);

            return new SkillEffectExecutionResult(
                effectType,
                target,
                true,
                $"Cleanse executed: {removalResult.StatusesRemovedCount} statuses removed, {removalResult.StacksRemovedCount} stacks removed.",
                null,
                1f,
                source);
        }
    }
}
