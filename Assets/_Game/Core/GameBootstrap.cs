using UnityEngine;
using WuxiaGame.Data;

namespace WuxiaGame.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private CombatConfigSO globalCombatConfig;
        [SerializeField] private bool autoStartBattle = true;

        public CombatConfigSO GlobalCombatConfig => globalCombatConfig;

        private void Awake()
        {
            if (globalCombatConfig == null)
            {
                globalCombatConfig = Resources.Load<CombatConfigSO>("Data/CombatConfig");
#if UNITY_EDITOR
                if (globalCombatConfig == null)
                {
                    globalCombatConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CombatConfigSO>("Assets/_Game/Data/CombatConfig.asset");
                }
#endif
            }
        }

        private void Start()
        {
            if (globalCombatConfig == null)
            {
                Debug.LogWarning("[GameBootstrap] CombatConfigSO is missing on GameBootstrap!");
            }

            if (autoStartBattle && BattleManager.Instance != null)
            {
                BattleManager.Instance.StartBattle();
            }
        }
    }
}
