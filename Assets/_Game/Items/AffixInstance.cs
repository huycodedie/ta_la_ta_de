using System;
using UnityEngine;
using WuxiaGame.Stats;

namespace WuxiaGame.Items
{
    [Serializable]
    public class AffixInstance
    {
        [SerializeField] private string affixId;
        [SerializeField] private string displayName;
        [SerializeField] private StatType statType;
        [SerializeField] private float baseValue;
        [SerializeField] private float value;

        public string AffixId => affixId;
        public string DisplayName => displayName;
        public StatType StatType => statType;
        public float BaseValue => baseValue;
        public float Value => value;

        public AffixInstance(string id, string name, StatType stat, float val, float baseVal = 0f)
        {
            affixId = id;
            displayName = name;
            statType = stat;
            value = val;
            baseValue = baseVal > 0f ? baseVal : val;
        }

        public void SetValue(float val)
        {
            value = val;
        }
    }
}
