using System;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Drop;

namespace WuxiaGame.Progression
{
    public class LootTierProgressionManager : MonoBehaviour
    {
        private static LootTierProgressionManager instance;
        public static LootTierProgressionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<LootTierProgressionManager>();
                }
                return instance;
            }
            private set => instance = value;
        }

        private const string PREF_TIER_ID = "TLTD_LootTier_CurrentId";
        private const string PREF_PROGRESS = "TLTD_LootTier_Progress";
        private const string PREF_IS_UPGRADING = "TLTD_LootTier_IsUpgrading";
        private const string PREF_START_TIME = "TLTD_LootTier_StartTime";
        private const string PREF_DURATION = "TLTD_LootTier_Duration";

        [Header("Database")]
        [SerializeField] private LootTierDatabaseSO lootTierDatabase;

        [Header("Runtime State")]
        [SerializeField] private int currentTierId = 1;
        [SerializeField] private int currentProgress = 0;
        [SerializeField] private bool isUpgrading = false;
        [SerializeField] private long upgradeStartTimeUnix = 0;
        [SerializeField] private float upgradeDuration = 0f;

        public LootTierDatabaseSO Database => lootTierDatabase;
        public int CurrentTierId => currentTierId;
        public int CurrentProgress => currentProgress;
        public bool IsUpgrading => isUpgrading;
        public float UpgradeDuration => upgradeDuration;

        public static void ResetInstance()
        {
            instance = null;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;

            LoadDatabaseIfMissing();
            LoadState();
        }

        private void Start()
        {
            LoadDatabaseIfMissing();
            SyncWithDropSystem();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (isUpgrading && GetUpgradeRemainingSeconds() <= 0f)
            {
                // Upgrade countdown finished - ready to complete
            }
        }

        public void LoadDatabaseIfMissing()
        {
            if (lootTierDatabase == null)
            {
                lootTierDatabase = Resources.Load<LootTierDatabaseSO>("Data/LootTierDatabase");
#if UNITY_EDITOR
                if (lootTierDatabase == null)
                {
                    lootTierDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<LootTierDatabaseSO>("Assets/_Game/Data/LootTierDatabase.asset");
                }
#endif
            }
        }

        public void SetDatabase(LootTierDatabaseSO db)
        {
            lootTierDatabase = db;
        }

        public LootTierConfigSO GetCurrentTier()
        {
            LoadDatabaseIfMissing();
            if (lootTierDatabase == null) return null;
            return lootTierDatabase.GetTier(currentTierId);
        }

        public LootTierConfigSO GetNextTier()
        {
            LoadDatabaseIfMissing();
            if (lootTierDatabase == null) return null;
            return lootTierDatabase.GetNextTier(currentTierId);
        }

        public int GetCurrentProgress()
        {
            return currentProgress;
        }

        public int GetRequiredProgress()
        {
            var curTier = GetCurrentTier();
            if (curTier != null)
            {
                return curTier.RequiredProgress;
            }
            return 2500;
        }

        public bool IsMaxTier()
        {
            return GetNextTier() == null;
        }

        public bool CanUpgrade()
        {
            if (isUpgrading) return false;
            if (IsMaxTier()) return false;

            var curTier = GetCurrentTier();
            if (curTier == null) return false;

            if (curTier.RequiredHeroLevel > 1 && ProgressionManager.Instance != null && ProgressionManager.Instance.CurrentLevel < curTier.RequiredHeroLevel)
            {
                return false;
            }

            if (curTier.RequiredTitleIndex > 0 && ProgressionManager.Instance != null && ProgressionManager.Instance.CurrentTitleIndex < curTier.RequiredTitleIndex)
            {
                return false;
            }

            if (curTier.UpgradeCostGold > 0 || curTier.UpgradeCostMaterial > 0)
            {
                if (ResourceManager.Instance != null)
                {
                    if (ResourceManager.Instance.Gold < curTier.UpgradeCostGold ||
                        ResourceManager.Instance.Material < curTier.UpgradeCostMaterial)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (curTier.RequiredProgress > 0)
            {
                if (currentProgress < curTier.RequiredProgress) return false;
            }

            return true;
        }

        public string GetUpgradeCostDescription(LootTierConfigSO tier)
        {
            if (tier == null) return "Miễn phí";
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            if (tier.UpgradeCostGold > 0) parts.Add($"{tier.UpgradeCostGold:N0} Gold");
            if (tier.UpgradeCostMaterial > 0) parts.Add($"{tier.UpgradeCostMaterial:N0} Nguyên liệu");
            if (tier.RequiredHeroLevel > 1) parts.Add($"Lv.{tier.RequiredHeroLevel}");
            return parts.Count > 0 ? string.Join(" | ", parts) : "Miễn phí";
        }

        public void AddProgress(int amount)
        {
            if (amount <= 0) return;
            currentProgress += amount;
            SaveState();
            EventBus.RaiseLootTierProgressChanged(currentProgress, GetRequiredProgress());
        }

        public bool StartUpgrade()
        {
            if (!CanUpgrade())
            {
                Debug.LogWarning("[LOOT TIER] Cannot start upgrade: Requirements not met.");
                return false;
            }

            var curTier = GetCurrentTier();
            var nextTier = GetNextTier();
            if (curTier == null || nextTier == null)
            {
                return false;
            }

            // Consume costs
            if (curTier.UpgradeCostGold > 0 || curTier.UpgradeCostMaterial > 0)
            {
                if (ResourceManager.Instance != null)
                {
                    if (!ResourceManager.Instance.ConsumeResources(curTier.UpgradeCostGold, curTier.UpgradeCostMaterial))
                    {
                        Debug.LogWarning("[LOOT TIER] Upgrade failed: Insufficient resources.");
                        return false;
                    }
                }
            }

            if (curTier.UpgradeDurationSeconds <= 0f)
            {
                return CompleteUpgradeInternal(curTier, nextTier);
            }

            isUpgrading = true;
            upgradeDuration = curTier.UpgradeDurationSeconds;
            upgradeStartTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveState();

            EventBus.RaiseLootTierUpgradeStarted();
            Debug.Log($"[LOOT TIER] Upgrade started from Tier {curTier.TierId} to Tier {nextTier.TierId}. Duration: {upgradeDuration}s");
            return true;
        }

        public float GetUpgradeRemainingSeconds()
        {
            if (!isUpgrading) return 0f;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = now - upgradeStartTimeUnix;
            float remaining = upgradeDuration - elapsed;
            return Mathf.Max(0f, remaining);
        }

        public bool CompleteUpgrade()
        {
            var curTier = GetCurrentTier();
            var nextTier = GetNextTier();

            if (nextTier == null)
            {
                Debug.LogWarning("[LOOT TIER] Cannot complete upgrade: Already at Max Tier.");
                return false;
            }

            if (isUpgrading && GetUpgradeRemainingSeconds() > 0f)
            {
                Debug.LogWarning("[LOOT TIER] Cannot complete upgrade: Timer still running.");
                return false;
            }

            return CompleteUpgradeInternal(curTier, nextTier);
        }

        private bool CompleteUpgradeInternal(LootTierConfigSO curTier, LootTierConfigSO nextTier)
        {
            int req = curTier != null ? curTier.RequiredProgress : 2500;
            currentProgress = Mathf.Max(0, currentProgress - req);
            currentTierId = nextTier.TierId;

            isUpgrading = false;
            upgradeStartTimeUnix = 0;
            upgradeDuration = 0f;

            SaveState();
            SyncWithDropSystem();

            EventBus.RaiseLootTierUpgradeCompleted(nextTier);
            EventBus.RaiseLootTierChanged(nextTier);
            EventBus.RaiseLootTierProgressChanged(currentProgress, GetRequiredProgress());

            Debug.Log($"[LOOT TIER] Upgrade complete! Advanced to Tier {nextTier.TierId} ({nextTier.DisplayName}).");
            return true;
        }

        public void SyncWithDropSystem()
        {
            if (DropSystem.Instance != null)
            {
                DropSystem.Instance.CurrentDropLevel = currentTierId;
                DropSystem.Instance.CurrentLootTier = currentTierId;
            }
        }

        public void SetMockTier(int tierId)
        {
            currentTierId = Mathf.Max(1, tierId);
            SaveState();
            SyncWithDropSystem();
            EventBus.RaiseLootTierChanged(GetCurrentTier());
            EventBus.RaiseLootTierProgressChanged(currentProgress, GetRequiredProgress());
        }

        public void SetMockProgress(int progress)
        {
            currentProgress = Mathf.Max(0, progress);
            SaveState();
            EventBus.RaiseLootTierProgressChanged(currentProgress, GetRequiredProgress());
        }

        public void SetMockUpgradeState(bool upgrading, float remainingSeconds)
        {
            isUpgrading = upgrading;
            if (upgrading)
            {
                upgradeDuration = Mathf.Max(0f, remainingSeconds);
                upgradeStartTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else
            {
                upgradeDuration = 0f;
                upgradeStartTimeUnix = 0;
            }
            SaveState();
        }

        public void ResetProgression()
        {
            currentTierId = lootTierDatabase != null && lootTierDatabase.GetFirstTier() != null ? lootTierDatabase.GetFirstTier().TierId : 1;
            currentProgress = 0;
            isUpgrading = false;
            upgradeStartTimeUnix = 0;
            upgradeDuration = 0f;
            SaveState();
            SyncWithDropSystem();
            EventBus.RaiseLootTierChanged(GetCurrentTier());
            EventBus.RaiseLootTierProgressChanged(currentProgress, GetRequiredProgress());
        }

        public void SaveState()
        {
            PlayerPrefs.SetInt(PREF_TIER_ID, currentTierId);
            PlayerPrefs.SetInt(PREF_PROGRESS, currentProgress);
            PlayerPrefs.SetInt(PREF_IS_UPGRADING, isUpgrading ? 1 : 0);
            PlayerPrefs.SetString(PREF_START_TIME, upgradeStartTimeUnix.ToString());
            PlayerPrefs.SetFloat(PREF_DURATION, upgradeDuration);
            PlayerPrefs.Save();
        }

        public void LoadState()
        {
            if (PlayerPrefs.HasKey(PREF_TIER_ID))
            {
                currentTierId = PlayerPrefs.GetInt(PREF_TIER_ID, 1);
            }
            else if (lootTierDatabase != null && lootTierDatabase.GetFirstTier() != null)
            {
                currentTierId = lootTierDatabase.GetFirstTier().TierId;
            }

            currentProgress = PlayerPrefs.GetInt(PREF_PROGRESS, 0);
            isUpgrading = PlayerPrefs.GetInt(PREF_IS_UPGRADING, 0) == 1;

            string startStr = PlayerPrefs.GetString(PREF_START_TIME, "0");
            long.TryParse(startStr, out upgradeStartTimeUnix);
            upgradeDuration = PlayerPrefs.GetFloat(PREF_DURATION, 0f);
        }
    }
}
