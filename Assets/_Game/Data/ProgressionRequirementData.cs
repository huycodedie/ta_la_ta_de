using System;
using UnityEngine;

namespace WuxiaGame.Data
{
    [Serializable]
    public class ProgressionRequirementData
    {
        [SerializeField] private RequirementType type = RequirementType.PlayerLevel;
        [SerializeField] private int requiredValue = 1;
        [SerializeField] private string description = "Reach Required Level";
        [SerializeField] private bool isPermanent = true;

        public RequirementType Type => type;
        public int RequiredValue => requiredValue;
        public string Description => description;
        public bool IsPermanent => isPermanent;

        public ProgressionRequirementData(RequirementType reqType, int value, string desc, bool permanent = true)
        {
            type = reqType;
            requiredValue = value;
            description = desc;
            isPermanent = permanent;
        }
    }
}
