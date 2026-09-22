using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "ExpConfig", menuName = "WuxiaGame/Data/ExpConfig")]
    public class ExpConfigSO : ScriptableObject
    {
        [Header("EXP Curve Settings")]
        [SerializeField] private float baseExp = 100f;
        [SerializeField] private float incrementPerLevel = 50f;
        [SerializeField] private int tierSize = 50;
        [SerializeField] private float tierMultiplier = 2f;
        [SerializeField] private int maxPlayerLevel = 999999;

        [Header("Monster Rewards")]
        [SerializeField] private float expPerMonsterKill = 10f;

        public float BaseExp => baseExp;
        public float IncrementPerLevel => incrementPerLevel;
        public int TierSize => tierSize;
        public float TierMultiplier => tierMultiplier;
        public int MaxPlayerLevel => maxPlayerLevel;
        public float ExpPerMonsterKill => expPerMonsterKill;

        public float GetIncrementForLevel(int level)
        {
            if (level <= 0) return incrementPerLevel;
            int band = (level - 1) / tierSize;
            return incrementPerLevel * Mathf.Pow(tierMultiplier, band);
        }

        public float GetRequiredExpForLevel(int level)
        {
            if (level <= 1) return baseExp;
            if (level >= maxPlayerLevel) return float.MaxValue;

            int band = (level - 1) / tierSize;
            // Cumulative base across all preceding full bands:
            // Full band k contribution = ((tierSize - 1) * inc(k) + inc(k+1)) = (49 * 50 + 100) * 2^k = 2550 * 2^k
            // Sum of 2^k from k=0 to band-1 = 2^band - 1
            float cumulativeFullBands = 2550f * (Mathf.Pow(tierMultiplier, band) - 1f);

            // Steps within the current band:
            int levelInBand = (level - 1) % tierSize;
            float currentBandIncrement = incrementPerLevel * Mathf.Pow(tierMultiplier, band);
            float currentBandSteps = levelInBand * currentBandIncrement;

            return baseExp + cumulativeFullBands + currentBandSteps;
        }
    }
}
