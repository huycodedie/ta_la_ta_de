using UnityEngine;
using WuxiaGame.Progression;

namespace WuxiaGame.Drop
{
    public class HeroLevelItemLevelProvider : IItemLevelProvider
    {
        public int GetItemLevel()
        {
            if (ProgressionManager.Instance != null)
            {
                return Mathf.Max(1, ProgressionManager.Instance.CurrentLevel);
            }
            return 1;
        }
    }
}
