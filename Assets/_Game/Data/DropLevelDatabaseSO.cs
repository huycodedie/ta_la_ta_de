using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "DropLevelDatabase", menuName = "WuxiaGame/Data/DropLevelDatabase")]
    public class DropLevelDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<DropLevelConfigSO> dropLevels = new List<DropLevelConfigSO>();

        public IReadOnlyList<DropLevelConfigSO> DropLevels => dropLevels;

        public DropLevelConfigSO GetDropLevelConfig(int dropLevel)
        {
            if (dropLevels == null || dropLevels.Count == 0) return null;

            DropLevelConfigSO match = dropLevels.Find(dl => dl.DropLevel == dropLevel);
            if (match != null) return match;

            return dropLevels[0]; // fallback
        }

        public void SetDropLevels(List<DropLevelConfigSO> list)
        {
            dropLevels = list;
        }
    }
}
