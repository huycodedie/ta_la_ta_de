using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "TitleBreakthroughDatabase", menuName = "WuxiaGame/Data/TitleBreakthroughDatabase")]
    public class TitleBreakthroughDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<TitleBreakthroughConfigSO> breakthroughs = new List<TitleBreakthroughConfigSO>();

        public IReadOnlyList<TitleBreakthroughConfigSO> Breakthroughs => breakthroughs;
        public int Count => breakthroughs != null ? breakthroughs.Count : 0;

        public TitleBreakthroughConfigSO GetBreakthrough(int index)
        {
            if (breakthroughs == null || breakthroughs.Count == 0) return null;
            index = Mathf.Clamp(index, 0, breakthroughs.Count - 1);
            return breakthroughs[index];
        }

        public TitleBreakthroughConfigSO GetNextBreakthrough(TitleBreakthroughConfigSO current)
        {
            if (breakthroughs == null || current == null) return null;
            int idx = breakthroughs.IndexOf(current);
            if (idx >= 0 && idx < breakthroughs.Count - 1)
            {
                return breakthroughs[idx + 1];
            }
            return null;
        }

        public TitleBreakthroughConfigSO GetBreakthroughById(string id)
        {
            if (breakthroughs == null) return null;
            return breakthroughs.Find(b => b != null && b.BreakthroughId == id);
        }

        public void SetBreakthroughs(List<TitleBreakthroughConfigSO> list)
        {
            breakthroughs = list ?? new List<TitleBreakthroughConfigSO>();
        }

        public bool ValidateDatabase(out List<string> errors)
        {
            errors = new List<string>();
            if (breakthroughs == null || breakthroughs.Count == 0)
            {
                errors.Add("Title Breakthrough Database is empty.");
                return false;
            }

            var seenIds = new HashSet<string>();
            for (int i = 0; i < breakthroughs.Count; i++)
            {
                var b = breakthroughs[i];
                if (b == null)
                {
                    errors.Add($"Breakthrough entry at index {i} is null.");
                    continue;
                }

                if (seenIds.Contains(b.BreakthroughId))
                {
                    errors.Add($"Duplicate breakthroughId '{b.BreakthroughId}' found at index {i}.");
                }
                seenIds.Add(b.BreakthroughId);

                if (!b.ValidateConfig(out string err))
                {
                    errors.Add($"Breakthrough '{b.BreakthroughId}' validation failed: {err}");
                }

                if (i > 0)
                {
                    var prev = breakthroughs[i - 1];
                    if (prev != null && b.CurrentLevelCap != prev.NextLevelCap && prev.NextLevelCap > 0)
                    {
                        errors.Add($"Breakthrough sequence gap at index {i}: '{prev.BreakthroughId}' nextLevelCap ({prev.NextLevelCap}) does not match '{b.BreakthroughId}' currentLevelCap ({b.CurrentLevelCap}).");
                    }
                }
            }

            return errors.Count == 0;
        }
    }
}
