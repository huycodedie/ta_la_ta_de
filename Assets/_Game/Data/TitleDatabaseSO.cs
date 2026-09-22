using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "TitleDatabase", menuName = "WuxiaGame/Data/TitleDatabase")]
    public class TitleDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<TitleConfigSO> titles = new List<TitleConfigSO>();

        public IReadOnlyList<TitleConfigSO> Titles => titles;

        public TitleConfigSO GetTitle(int index)
        {
            if (titles == null || titles.Count == 0) return null;
            index = Mathf.Clamp(index, 0, titles.Count - 1);
            return titles[index];
        }

        public TitleConfigSO GetNextTitle(TitleConfigSO current)
        {
            if (titles == null || current == null) return null;
            int idx = titles.IndexOf(current);
            if (idx >= 0 && idx < titles.Count - 1)
            {
                return titles[idx + 1];
            }
            return null;
        }

        public void SetTitles(List<TitleConfigSO> titleList)
        {
            titles = titleList;
        }
    }
}
