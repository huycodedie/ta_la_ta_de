using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Progression
{
    public enum ProgressionState
    {
        NormalProgression,
        LevelCapReached,
        BreakthroughAvailable,
        BreakthroughExecuting
    }

    public class ProgressionManager : MonoBehaviour
    {
        private static ProgressionManager _instance;
        public static ProgressionManager Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Data Configs")]
        [SerializeField] private ExpConfigSO expConfig;
        [SerializeField] private HeroProgressionConfigSO progressionConfig;
        [SerializeField] private TitleDatabaseSO titleDatabase;

        [Header("Runtime State")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentExp = 0f;
        [SerializeField] private int currentTitleIndex = 0;
        [SerializeField] private int currentStageCleared = 0;
        [SerializeField] private int currentDropLevel = 1;

        public int CurrentLevel => currentLevel;
        public float CurrentExp => currentExp;
        public int CurrentTitleIndex => currentTitleIndex;
        public int CurrentStageCleared => currentStageCleared;
        public int CurrentDropLevel => currentDropLevel;
        public int MaxLevel => progressionConfig != null ? progressionConfig.MaxLevel : (expConfig != null ? expConfig.MaxPlayerLevel : 100);

        public ProgressionState CurrentProgressionState
        {
            get
            {
                int cap = GetEffectiveLevelCap();
                if (currentLevel < cap) return ProgressionState.NormalProgression;
                if (TitleBreakthroughManager.Instance != null)
                {
                    if (TitleBreakthroughManager.Instance.IsExecutingBreakthrough) return ProgressionState.BreakthroughExecuting;
                    if (TitleBreakthroughManager.Instance.CanBreakthrough()) return ProgressionState.BreakthroughAvailable;
                }
                else if (CanBreakthrough())
                {
                    return ProgressionState.BreakthroughAvailable;
                }
                return ProgressionState.LevelCapReached;
            }
        }

        public ExpConfigSO ExpConfig => expConfig;
        public HeroProgressionConfigSO ProgressionConfig => progressionConfig;
        public TitleDatabaseSO TitleDatabase => titleDatabase;
        public TitleConfigSO CurrentTitle => titleDatabase != null ? titleDatabase.GetTitle(currentTitleIndex) : null;

        public static void ResetInstance()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            _instance = this;

            LoadConfigsIfMissing();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        private void Start()
        {
            InitializeProgression();
        }

        private void OnEnable()
        {
            EventBus.OnEntityDied += HandleEntityDied;
        }

        private void OnDisable()
        {
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        public void LoadConfigsIfMissing()
        {
            if (expConfig == null)
            {
                expConfig = Resources.Load<ExpConfigSO>("Data/ExpConfig");
#if UNITY_EDITOR
                if (expConfig == null) expConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<ExpConfigSO>("Assets/_Game/Data/ExpConfig.asset");
#endif
            }

            if (progressionConfig == null)
            {
                progressionConfig = Resources.Load<HeroProgressionConfigSO>("Data/HeroProgressionConfig");
#if UNITY_EDITOR
                if (progressionConfig == null) progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroProgressionConfigSO>("Assets/_Game/Data/HeroProgressionConfig.asset");
#endif
            }

            if (titleDatabase == null)
            {
                titleDatabase = Resources.Load<TitleDatabaseSO>("Data/TitleDatabase");
#if UNITY_EDITOR
                if (titleDatabase == null) titleDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<TitleDatabaseSO>("Assets/_Game/Data/TitleDatabase.asset");
#endif
            }
        }

        public void InitializeProgression(int level = 1, float exp = 0f, int titleIndex = 0)
        {
            if (Instance == null) Instance = this;
            LoadConfigsIfMissing();
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;

            currentLevel = Mathf.Max(1, level);
            currentExp = Mathf.Max(0f, exp);
            currentTitleIndex = titleIndex;

            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero != null)
            {
                hero.ApplyBaseStatsFromTitle(CurrentTitle, false);
            }

            float reqExp = GetRequiredExpForLevel(currentLevel);
            EventBus.RaiseExpChanged(currentExp, reqExp);
            EventBus.RaiseLevelChanged(currentLevel);
            EventBus.RaiseTitleChanged(CurrentTitle);
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());

            int bkCount = TitleBreakthroughManager.Instance != null ? TitleBreakthroughManager.Instance.BreakthroughCount : 0;
            string titleName = TitleBreakthroughManager.Instance != null ? TitleBreakthroughManager.Instance.CurrentTitleName : (CurrentTitle != null ? CurrentTitle.TitleName : "Novice Disciple");
            int cap = GetEffectiveLevelCap();
            Debug.Log($"[PROGRESSION STATE]\nBreakthroughCount = {bkCount}\nCurrentTitle = {titleName}\nCurrentLevel = {currentLevel}\nCurrentLevelCap = {cap}");
        }

        private const string PREF_HERO_LEVEL = "TLTD_Hero_Level";
        private const string PREF_HERO_EXP = "TLTD_Hero_Exp";
        private const string PREF_HERO_TITLE = "TLTD_Hero_TitleIndex";

        public float GetRequiredExpForLevel(int level)
        {
            if (expConfig != null)
            {
                return expConfig.GetRequiredExpForLevel(level);
            }
            return 100f + (Mathf.Max(1, level) - 1) * 50f;
        }

        public int GetEffectiveLevelCap()
        {
            if (TitleBreakthroughManager.Instance != null)
            {
                return TitleBreakthroughManager.Instance.CurrentLevelCap;
            }

            TitleConfigSO title = CurrentTitle;
            if (title != null && title.MaxLevelCap > 0)
            {
                return title.MaxLevelCap;
            }
            return 5;
        }

        public bool AddExperience(int amount)
        {
            return AddExp(amount);
        }

        public bool AddExp(float expAmount)
        {
            if (expAmount <= 0f)
            {
                if (expAmount < 0f)
                {
                    Debug.LogWarning("[ProgressionManager] Negative EXP rejected!");
                }
                return false;
            }

            LoadConfigsIfMissing();

            int levelCap = GetEffectiveLevelCap();
            currentExp += expAmount;

            while (currentLevel < levelCap)
            {
                float reqExp = GetRequiredExpForLevel(currentLevel);
                if (currentExp >= reqExp)
                {
                    currentExp -= reqExp;
                    currentLevel++;

                    EventBus.RaiseLevelChanged(currentLevel);
                    EventBus.RaiseHeroLevelUp(currentLevel, 1, new Dictionary<StatType, float>());

                    Hero hero = Object.FindAnyObjectByType<Hero>();
                    if (hero != null)
                    {
                        hero.RecalculateStats(true);
                    }
                }
                else
                {
                    break;
                }
            }

            if (currentLevel >= levelCap)
            {
                Debug.Log($"[LEVEL CAP] Hero Level: {currentLevel} | Level Cap: {levelCap} | EXP Gained: +{expAmount} | Stored EXP: {currentExp} | Level Up BLOCKED | Breakthrough Required");
            }

            float finalReqExp = GetRequiredExpForLevel(currentLevel);
            EventBus.RaiseExpChanged(currentExp, finalReqExp);
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
            SaveState();
            return true;
        }

        public void ProcessStoredExp()
        {
            LoadConfigsIfMissing();
            int levelCap = GetEffectiveLevelCap();
            Hero hero = Object.FindAnyObjectByType<Hero>();

            while (currentLevel < levelCap)
            {
                float reqExp = GetRequiredExpForLevel(currentLevel);
                if (currentExp >= reqExp)
                {
                    currentExp -= reqExp;
                    currentLevel++;

                    EventBus.RaiseLevelChanged(currentLevel);
                    EventBus.RaiseHeroLevelUp(currentLevel, 1, new Dictionary<StatType, float>());

                    if (hero != null)
                    {
                        hero.RecalculateStats(true);
                    }
                }
                else
                {
                    break;
                }
            }

            float finalReqExp = GetRequiredExpForLevel(currentLevel);
            EventBus.RaiseExpChanged(currentExp, finalReqExp);
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
            SaveState();
        }

        public bool CanBreakthrough()
        {
            if (TitleBreakthroughManager.Instance != null && TitleBreakthroughManager.Instance.Database != null)
            {
                return TitleBreakthroughManager.Instance.CanBreakthrough();
            }

            TitleConfigSO currentTitle = CurrentTitle;
            if (currentTitle == null || titleDatabase == null) return false;

            TitleConfigSO nextTitle = titleDatabase.GetNextTitle(currentTitle);
            if (nextTitle == null) return false;

            if (currentTitle.Requirements == null || currentTitle.Requirements.Count == 0) return true;

            foreach (var req in currentTitle.Requirements)
            {
                switch (req.Type)
                {
                    case RequirementType.PlayerLevel:
                        if (currentLevel < req.RequiredValue) return false;
                        break;
                    case RequirementType.ClearStage:
                        if (currentStageCleared < req.RequiredValue) return false;
                        break;
                    case RequirementType.DropLevel:
                        if (currentDropLevel < req.RequiredValue) return false;
                        break;
                }
            }

            return true;
        }

        public bool TryBreakthrough()
        {
            if (TitleBreakthroughManager.Instance != null && TitleBreakthroughManager.Instance.Database != null)
            {
                return TitleBreakthroughManager.Instance.TryPerformBreakthrough();
            }

            if (!CanBreakthrough()) return false;

            currentTitleIndex++;
            TitleConfigSO newTitle = CurrentTitle;

            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero != null)
            {
                hero.ApplyTitleStats(newTitle);
            }

            EventBus.RaiseTitleChanged(newTitle);

            // Process stored EXP immediately against the newly unlocked Level Cap
            int newLevelCap = GetEffectiveLevelCap();
            while (currentLevel < newLevelCap)
            {
                float reqExp = GetRequiredExpForLevel(currentLevel);
                if (currentExp >= reqExp)
                {
                    currentExp -= reqExp;
                    currentLevel++;

                    EventBus.RaiseLevelChanged(currentLevel);
                    EventBus.RaiseHeroLevelUp(currentLevel, 1, new Dictionary<StatType, float>());

                    if (hero != null)
                    {
                        hero.RecalculateStats(true);
                    }
                }
                else
                {
                    break;
                }
            }

            float finalReqExp = GetRequiredExpForLevel(currentLevel);
            EventBus.RaiseExpChanged(currentExp, finalReqExp);
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
            SaveState();
            return true;
        }

        public void SaveState()
        {
            PlayerPrefs.SetInt(PREF_HERO_LEVEL, currentLevel);
            PlayerPrefs.SetFloat(PREF_HERO_EXP, currentExp);
            PlayerPrefs.SetInt(PREF_HERO_TITLE, currentTitleIndex);
            PlayerPrefs.Save();
        }

        public void LoadState()
        {
            if (PlayerPrefs.HasKey(PREF_HERO_LEVEL))
            {
                currentLevel = PlayerPrefs.GetInt(PREF_HERO_LEVEL, 1);
            }
            if (PlayerPrefs.HasKey(PREF_HERO_EXP))
            {
                currentExp = PlayerPrefs.GetFloat(PREF_HERO_EXP, 0f);
            }
            if (PlayerPrefs.HasKey(PREF_HERO_TITLE))
            {
                currentTitleIndex = PlayerPrefs.GetInt(PREF_HERO_TITLE, 0);
            }
        }

        public void ResetPersistence()
        {
            PlayerPrefs.DeleteKey(PREF_HERO_LEVEL);
            PlayerPrefs.DeleteKey(PREF_HERO_EXP);
            PlayerPrefs.DeleteKey(PREF_HERO_TITLE);
            PlayerPrefs.Save();
            currentLevel = 1;
            currentExp = 0f;
            currentTitleIndex = 0;
        }

        public void ResetAllProgression()
        {
            ResetPersistence();
            if (TitleBreakthroughManager.Instance != null)
            {
                TitleBreakthroughManager.Instance.ResetPersistence();
            }
            InitializeProgression(1, 0f, 0);
        }

        public void SetMockLevel(int level)
        {
            currentLevel = Mathf.Max(1, level);
            EventBus.RaiseLevelChanged(currentLevel);
        }

        public void SetMockStageCleared(int stage)
        {
            currentStageCleared = stage;
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
        }

        public void SetMockDropLevel(int dropLevel)
        {
            currentDropLevel = dropLevel;
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
        }

        public void HandleEntityDied(Entity entity)
        {
            if (entity is Monster monster)
            {
                if (monster.HasAwardedExp) return;
                monster.HasAwardedExp = true;

                float reward = monster.MonsterConfig != null
                    ? monster.MonsterConfig.ExpReward
                    : (expConfig != null ? expConfig.ExpPerMonsterKill : 10f);

                if (reward <= 0f) reward = 10f;

                Debug.Log($"[REWARD] EXP +{reward:F0}");
                Debug.Log($"[PROGRESSION] {monster.EntityName} EXP REWARD = {reward:F0}");
                AddExp(reward);
                Debug.Log($"[PROGRESSION] Hero EXP = {currentExp:F0} / {GetRequiredExpForLevel(currentLevel):F0}");
                Debug.Log($"[PROGRESSION] Hero Level: {currentLevel}");
            }
        }
    }
}
