using UnityEngine;
using UnityEngine.SceneManagement;

namespace WuxiaGame.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public BattleState CurrentBattleState { get; private set; } = BattleState.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnBattleStateChanged += HandleBattleStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnBattleStateChanged -= HandleBattleStateChanged;
        }

        private void HandleBattleStateChanged(BattleState newState)
        {
            CurrentBattleState = newState;
            Debug.Log($"[GameManager] Battle state changed to: {newState}");
        }

        public void RestartBattle()
        {
            Debug.Log("[GameManager] RestartBattle requested — restarting combat encounter without resetting Hero progression.");
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.RestartBattle();
            }
            else
            {
                BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
                if (bm != null) bm.RestartBattle();
            }
        }
    }
}
