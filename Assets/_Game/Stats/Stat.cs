using System;
using UnityEngine;

namespace WuxiaGame.Stats
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private StatType statType;
        [SerializeField] private float baseValue;
        private float currentValue;

        public StatType StatType => statType;
        public float BaseValue => baseValue;
        
        public float CurrentValue
        {
            get => currentValue;
            set
            {
                if (Mathf.Approximately(currentValue, value)) return;
                currentValue = value;
                OnValueChanged?.Invoke(currentValue);
            }
        }

        public event Action<float> OnValueChanged;

        public Stat(StatType type, float initialBaseValue)
        {
            statType = type;
            baseValue = initialBaseValue;
            currentValue = initialBaseValue;
        }

        public void SetBaseValue(float value)
        {
            baseValue = value;
            CurrentValue = value;
        }
    }
}
