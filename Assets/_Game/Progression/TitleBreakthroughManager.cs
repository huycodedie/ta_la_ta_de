using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.UI;

namespace WuxiaGame.Progression
{
    [Serializable]
    public struct RequirementEvaluationDetail
    {
        public BreakthroughRequirement Requirement;
        public int CurrentValue;
        public int RequiredValue;
        public bool IsSatisfied;
        public string DisplayText;
        public string StatusText;
    }

    public struct BreakthroughEvaluationResult
    {
        public bool CanBreakthrough;
        public bool IsMaxStage;
        public string Reason;
        public List<RequirementEvaluationDetail> RequirementDetails;

        // Direct access fields
        public int RequiredLevel;
        public int CurrentLevel;
        public bool LevelSatisfied;
        public int RequiredGold;
        public int CurrentGold;
        public bool GoldSatisfied;
        public int RequiredMaterial;
        public int CurrentMaterial;
        public bool MaterialSatisfied;
        public int RequiredLootTier;
        public int CurrentLootTier;
        public bool LootTierSatisfied;

        public override string ToString()
        {
            return $"CanBreakthrough: {CanBreakthrough}, Lv: {CurrentLevel}/{RequiredLevel} ({LevelSatisfied}), LootTier: {CurrentLootTier}/{RequiredLootTier} ({LootTierSatisfied}), Gold: {CurrentGold}/{RequiredGold} ({GoldSatisfied}), Mat: {CurrentMaterial}/{RequiredMaterial} ({MaterialSatisfied}), Reason: {Reason}";
        }
    }

    public class TitleBreakthroughManager : MonoBehaviour
    {
        private static TitleBreakthroughManager instance;
        public static TitleBreakthroughManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<TitleBreakthroughManager>();
                }
                return instance;
            }
            private set => instance = value;
        }

        private const string PREF_STAGE_INDEX = "TLTD_Title_StageIndex";

        [Header("Database")]
        [SerializeField] private TitleBreakthroughDatabaseSO breakthroughDatabase;

        [Header("Runtime State")]
        [SerializeField] private int currentStageIndex = 0;
        private bool isExecutingBreakthrough = false;

        [Header("Progression Rules")]
        [SerializeField] private int baseBreakthroughLevelIncrement = 5;

        public TitleBreakthroughDatabaseSO Database
        {
            get
            {
                Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
                if (hero != null && hero.GetBreakthroughProfile() != null)
                {
                    return hero.GetBreakthroughProfile();
                }
                return breakthroughDatabase;
            }
        }

        public void SetMockDatabase(TitleBreakthroughDatabaseSO db)
        {
            breakthroughDatabase = db;
        }

        public int CurrentStageIndex => currentStageIndex;
        public int CurrentBreakthroughCount => currentStageIndex;
        public int BreakthroughCount => currentStageIndex;
        public int NextBreakthroughNumber => CurrentBreakthroughCount + 1;
        public int BaseBreakthroughLevelIncrement => baseBreakthroughLevelIncrement;
        public bool IsExecutingBreakthrough => isExecutingBreakthrough;

        public ProgressionState CurrentProgressionState
        {
            get
            {
                if (isExecutingBreakthrough) return ProgressionState.BreakthroughExecuting;
                int curLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;
                int cap = CurrentLevelCap;
                if (curLevel < cap) return ProgressionState.NormalProgression;
                if (CanBreakthrough()) return ProgressionState.BreakthroughAvailable;
                return ProgressionState.LevelCapReached;
            }
        }

        public TitleBreakthroughConfigSO CurrentBreakthrough =>
            Database != null ? Database.GetBreakthrough(currentStageIndex) : null;

        public TitleBreakthroughConfigSO NextBreakthrough =>
            Database != null && CurrentBreakthrough != null ? Database.GetNextBreakthrough(CurrentBreakthrough) : null;

        public string CurrentTitleName => CurrentBreakthrough != null ? CurrentBreakthrough.TitleName : "Novice Disciple";
        public int CurrentLevelCap => GetLevelCapForBreakthroughCount(CurrentBreakthroughCount);
        public int NextLevelCap => GetLevelCapForBreakthroughCount(CurrentBreakthroughCount + 1);
        public bool IsMaxStage => NextBreakthrough == null;

        public int GetRequiredBreakthroughLevel()
        {
            return GetRequiredBreakthroughLevel(NextBreakthroughNumber);
        }

        public int GetLevelCapForBreakthroughCount(int count)
        {
            if (count <= 0) return 5;
            int cap = 5;
            for (int k = 1; k <= count; k++)
            {
                cap += baseBreakthroughLevelIncrement * k;
            }
            return cap;
        }

        public int GetRequiredBreakthroughLevel(int nextBreakthroughNumber)
        {
            if (nextBreakthroughNumber <= 1)
            {
                return 5; // Breakthrough #1 requires Level 5
            }

            return GetLevelCapForBreakthroughCount(nextBreakthroughNumber - 1);
        }

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
            ApplyCurrentTitleToHero();
            LogAuthoritativeState("Start");
            ValidateProgressionState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LogAuthoritativeState(string callerContext)
        {
            int curLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;
            Debug.Log($"[PROGRESSION STATE] ({callerContext})\nBreakthroughCount = {CurrentBreakthroughCount}\nCurrentTitle = {CurrentTitleName}\nCurrentLevel = {curLevel}\nCurrentLevelCap = {CurrentLevelCap}");
        }

        public bool ValidateProgressionState()
        {
            bool isValid = true;
            int count = currentStageIndex;
            int cap = CurrentLevelCap;
            int curLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;

            if (count < 0)
            {
                Debug.LogError($"[INVALID PROGRESSION STATE] BreakthroughCount is negative: {count}");
                isValid = false;
            }

            int expectedCap = GetLevelCapForBreakthroughCount(count);
            if (cap != expectedCap)
            {
                Debug.LogError($"[INVALID PROGRESSION STATE] LevelCap mismatch: CurrentLevelCap={cap}, Expected={expectedCap} for BreakthroughCount={count}");
                isValid = false;
            }

            if (count == 0)
            {
                if (CurrentTitleName != "Novice Disciple")
                {
                    Debug.LogError($"[INVALID PROGRESSION STATE] Initial Title mismatch: Title='{CurrentTitleName}', Expected='Novice Disciple' for BreakthroughCount=0");
                    isValid = false;
                }
                if (cap != 5)
                {
                    Debug.LogError($"[INVALID PROGRESSION STATE] Initial Cap mismatch: CurrentLevelCap={cap}, Expected=5 for BreakthroughCount=0");
                    isValid = false;
                }
            }
            else if (count == 1)
            {
                if (cap != 10)
                {
                    Debug.LogError($"[INVALID PROGRESSION STATE] Cap mismatch for Breakthrough #1: CurrentLevelCap={cap}, Expected=10");
                    isValid = false;
                }
            }
            else if (count == 2)
            {
                if (cap != 20)
                {
                    Debug.LogError($"[INVALID PROGRESSION STATE] Cap mismatch for Breakthrough #2: CurrentLevelCap={cap}, Expected=20");
                    isValid = false;
                }
            }
            else if (count == 3)
            {
                if (cap != 35)
                {
                    Debug.LogError($"[INVALID PROGRESSION STATE] Cap mismatch for Breakthrough #3: CurrentLevelCap={cap}, Expected=35");
                    isValid = false;
                }
            }

            return isValid;
        }

        private void SetBreakthroughCountInternal(int newCount, string caller, string reason)
        {
            int oldCount = currentStageIndex;
            int oldCap = CurrentLevelCap;
            string oldTitle = CurrentTitleName;

            currentStageIndex = Mathf.Max(0, newCount);

            if (oldCount != currentStageIndex)
            {
                Debug.Log($"[BREAKTHROUGH COUNT MUTATION]\nOLD = {oldCount}\nNEW = {currentStageIndex}\nCALLER = {caller}\nREASON = {reason}");
            }

            int newCap = CurrentLevelCap;
            if (oldCap != newCap)
            {
                Debug.Log($"[LEVEL CAP MUTATION]\nOLD = {oldCap}\nNEW = {newCap}\nBREAKTHROUGH COUNT = {currentStageIndex}\nCALLER = {caller}\nREASON = {reason}");
            }

            string newTitle = CurrentTitleName;
            if (oldTitle != newTitle)
            {
                Debug.Log($"[TITLE MUTATION]\nOLD = {oldTitle}\nNEW = {newTitle}\nBREAKTHROUGH COUNT = {currentStageIndex}\nCALLER = {caller}\nREASON = {reason}");
            }

            ValidateProgressionState();
        }

        public void LoadDatabaseIfMissing()
        {
            if (breakthroughDatabase == null)
            {
                breakthroughDatabase = Resources.Load<TitleBreakthroughDatabaseSO>("Data/TitleBreakthroughDatabase");
#if UNITY_EDITOR
                if (breakthroughDatabase == null)
                {
                    breakthroughDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<TitleBreakthroughDatabaseSO>("Assets/_Game/Data/TitleBreakthroughDatabase.asset");
                }
#endif
            }
        }

        public void SetDatabase(TitleBreakthroughDatabaseSO db)
        {
            breakthroughDatabase = db;
        }

        public void ApplyCurrentTitleToHero(bool preserveHpDelta = false)
        {
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            if (hero != null && CurrentBreakthrough != null)
            {
                hero.ApplyBaseStatsFromBreakthrough(CurrentBreakthrough, preserveHpDelta);
            }
        }

        public BreakthroughEvaluationResult EvaluateBreakthrough()
        {
            LoadDatabaseIfMissing();

            BreakthroughEvaluationResult res = new BreakthroughEvaluationResult();
            res.RequirementDetails = new List<RequirementEvaluationDetail>();

            var cur = CurrentBreakthrough;
            var next = NextBreakthrough;
            var db = Database;

            if (cur == null || db == null)
            {
                res.CanBreakthrough = false;
                res.Reason = "Breakthrough database or current stage missing.";
                return res;
            }

            if (next == null)
            {
                res.IsMaxStage = true;
                res.CanBreakthrough = false;
                res.Reason = "Already reached maximum title breakthrough stage.";
                return res;
            }

            int curLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;
            int curGold = ResourceManager.Instance != null ? ResourceManager.Instance.Gold : 0;
            int curMat = ResourceManager.Instance != null ? ResourceManager.Instance.Material : 0;
            int curLootTier = LootTierProgressionManager.Instance != null ? LootTierProgressionManager.Instance.CurrentTierId : 1;

            int equipCount = 0;
            if (EquipmentManager.Instance != null)
            {
                equipCount = EquipmentManager.Instance.GetEquippedItemCount();
            }

            var reqList = cur.GetRequirements();
            bool allSatisfied = true;
            string failReason = "";

            Debug.Log($"[TITLE BREAKTHROUGH] Evaluate");

            foreach (var req in reqList)
            {
                RequirementEvaluationDetail detail = new RequirementEvaluationDetail();
                detail.Requirement = req;
                detail.RequiredValue = req.RequiredValue;

                switch (req.Type)
                {
                    case BreakthroughRequirementType.HeroLevel:
                        detail.CurrentValue = curLevel;
                        detail.IsSatisfied = (curLevel >= req.RequiredValue);
                        detail.DisplayText = $"Cấp Hero: {curLevel} / {req.RequiredValue}";
                        res.RequiredLevel = req.RequiredValue;
                        res.CurrentLevel = curLevel;
                        res.LevelSatisfied = detail.IsSatisfied;
                        Debug.Log($"[TITLE BREAKTHROUGH] HeroLevel: {curLevel}/{req.RequiredValue} {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.LootTier:
                        detail.CurrentValue = curLootTier;
                        detail.IsSatisfied = (curLootTier >= req.RequiredValue);
                        detail.DisplayText = $"Cấp Rơi: {curLootTier} / {req.RequiredValue}";
                        res.RequiredLootTier = req.RequiredValue;
                        res.CurrentLootTier = curLootTier;
                        res.LootTierSatisfied = detail.IsSatisfied;
                        Debug.Log($"[TITLE BREAKTHROUGH] LootTier: {curLootTier}/{req.RequiredValue} {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.Gold:
                        detail.CurrentValue = curGold;
                        detail.IsSatisfied = (curGold >= req.RequiredValue);
                        detail.DisplayText = $"Gold: {curGold:N0} / {req.RequiredValue:N0}";
                        res.RequiredGold = req.RequiredValue;
                        res.CurrentGold = curGold;
                        res.GoldSatisfied = detail.IsSatisfied;
                        Debug.Log($"[TITLE BREAKTHROUGH] Gold: {curGold}/{req.RequiredValue} {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.Material:
                        detail.CurrentValue = curMat;
                        detail.IsSatisfied = (curMat >= req.RequiredValue);
                        detail.DisplayText = $"Nguyên liệu: {curMat:N0} / {req.RequiredValue:N0}";
                        res.RequiredMaterial = req.RequiredValue;
                        res.CurrentMaterial = curMat;
                        res.MaterialSatisfied = detail.IsSatisfied;
                        Debug.Log($"[TITLE BREAKTHROUGH] Material: {curMat}/{req.RequiredValue} {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.EquipmentCount:
                        detail.CurrentValue = equipCount;
                        detail.IsSatisfied = (equipCount >= req.RequiredValue);
                        detail.DisplayText = $"Số trang bị: {equipCount} / {req.RequiredValue}";
                        Debug.Log($"[TITLE BREAKTHROUGH] EquipmentCount: {equipCount}/{req.RequiredValue} {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.SpecificEquipment:
                        bool hasSpecific = EquipmentManager.Instance != null && EquipmentManager.Instance.HasEquippedItemWithId(req.TargetId);
                        detail.CurrentValue = hasSpecific ? 1 : 0;
                        detail.IsSatisfied = hasSpecific;
                        detail.DisplayText = $"Trang bị '{req.TargetId}': {(hasSpecific ? "Đã có" : "Chưa có")}";
                        Debug.Log($"[TITLE BREAKTHROUGH] SpecificEquipment '{req.TargetId}': {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.SpecificRarity:
                        bool hasRarity = EquipmentManager.Instance != null && EquipmentManager.Instance.HasEquippedItemWithMinRarityOrder(req.TargetRarityOrder);
                        detail.CurrentValue = hasRarity ? 1 : 0;
                        detail.IsSatisfied = hasRarity;
                        detail.DisplayText = $"Phẩm chất tối thiểu [Bậc {req.TargetRarityOrder}]: {(hasRarity ? "Đạt" : "Chưa đạt")}";
                        Debug.Log($"[TITLE BREAKTHROUGH] SpecificRarity Order [{req.TargetRarityOrder}]: {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.Custom:
                        detail.CurrentValue = 1;
                        detail.IsSatisfied = true;
                        detail.DisplayText = string.IsNullOrEmpty(req.CustomDescription) ? "Yêu cầu đặc biệt: Đạt" : $"{req.CustomDescription}: Đạt";
                        Debug.Log($"[TITLE BREAKTHROUGH] Custom: {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;

                    case BreakthroughRequirementType.MindMethodLevel:
                        int mmLvl = MindMethodManager.Instance != null ? MindMethodManager.Instance.GetActiveMindMethodLevel() : 1;
                        if (!string.IsNullOrEmpty(req.TargetId) && MindMethodManager.Instance != null)
                        {
                            mmLvl = MindMethodManager.Instance.GetMindMethodLevel(req.TargetId);
                        }
                        detail.CurrentValue = mmLvl;
                        detail.IsSatisfied = (mmLvl >= req.RequiredValue);
                        detail.DisplayText = $"Cấp Tâm Pháp: {mmLvl} / {req.RequiredValue}";
                        Debug.Log($"[TITLE BREAKTHROUGH] MindMethodLevel: {mmLvl}/{req.RequiredValue} {(detail.IsSatisfied ? "PASS" : "FAIL")}");
                        break;
                }

                detail.StatusText = detail.IsSatisfied ? "✓" : "✗";
                res.RequirementDetails.Add(detail);

                if (!detail.IsSatisfied)
                {
                    allSatisfied = false;
                    if (string.IsNullOrEmpty(failReason))
                    {
                        failReason = detail.DisplayText;
                    }
                }
            }

            res.CanBreakthrough = allSatisfied;
            res.Reason = allSatisfied ? "All requirements satisfied." : $"Requirements not met: {failReason}";
            Debug.Log($"[TITLE BREAKTHROUGH] CanBreakthrough: {(res.CanBreakthrough ? "TRUE" : "FALSE")}");

            return res;
        }

        public bool CanBreakthrough()
        {
            return EvaluateBreakthrough().CanBreakthrough;
        }

        public bool TryPerformBreakthrough()
        {
            Debug.Log("[TITLE BREAKTHROUGH] Player requested breakthrough");
            BreakthroughEvaluationResult eval = EvaluateBreakthrough();
            Debug.Log("[TITLE BREAKTHROUGH] Requirements revalidated");
            if (!eval.CanBreakthrough)
            {
                Debug.LogWarning($"[TITLE BREAKTHROUGH] Breakthrough validation failed: {eval.Reason}");
                return false;
            }

            var curStage = CurrentBreakthrough;
            var nextStage = NextBreakthrough;

            // 1. Deduct required consumable resources (Gold, Material) atomically based on data
            int goldToConsume = 0;
            int matToConsume = 0;
            foreach (var req in curStage.GetRequirements())
            {
                if (req.Type == BreakthroughRequirementType.Gold) goldToConsume += req.RequiredValue;
                if (req.Type == BreakthroughRequirementType.Material) matToConsume += req.RequiredValue;
            }

            if (ResourceManager.Instance != null && (goldToConsume > 0 || matToConsume > 0))
            {
                bool consumed = ResourceManager.Instance.ConsumeResources(goldToConsume, matToConsume);
                if (!consumed)
                {
                    Debug.LogWarning("[TITLE BREAKTHROUGH] Failed to consume resources for breakthrough!");
                    return false;
                }
            }

            int oldCount = currentStageIndex;

            // 2. Increase Title stage (Breakthrough Count)
            SetBreakthroughCountInternal(currentStageIndex + 1, "TryPerformBreakthrough", "Player confirmed breakthrough transaction");
            var newStage = CurrentBreakthrough;

            // 3. Apply Base Stats to Hero
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            if (hero != null && newStage != null)
            {
                hero.ApplyBaseStatsFromBreakthrough(newStage, true);
            }

            // 4. Save state
            SaveState();

            // 5. Fire Events
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());

            // 6. Process accumulated EXP immediately against new Level Cap
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.ProcessStoredExp();
            }

            Debug.Log($"[TITLE BREAKTHROUGH] Transaction SUCCESS\n[TITLE BREAKTHROUGH] BreakthroughCount: {oldCount} -> {currentStageIndex}\n[TITLE BREAKTHROUGH] LevelCap: {GetLevelCapForBreakthroughCount(oldCount)} -> {CurrentLevelCap}");
            return true;
        }

        public void SetMockBreakthroughCount(int count)
        {
            SetMockStage(count);
        }

        public void SetMockStage(int stageIndex)
        {
            SetBreakthroughCountInternal(stageIndex, "SetMockStage", "Unit Test / Debug mock setup");
            ApplyCurrentTitleToHero();
            // NOTE: Do NOT call SaveState() here to avoid polluting persistent PlayerPrefs during tests!
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.ProcessStoredExp();
            }
            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
        }

        public void ResetProgression()
        {
            ResetPersistence();
            ApplyCurrentTitleToHero();

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.ResetPersistence();
                ProgressionManager.Instance.InitializeProgression(1, 0f, 0);
            }

            EventBus.RaiseBreakthroughStatusChanged(CanBreakthrough());
            if (TitleBreakthroughUI.Instance != null)
            {
                TitleBreakthroughUI.Instance.RefreshUI();
            }
            Debug.Log("[PROGRESSION] Full reset executed -> BreakthroughCount=0, Level=1, Cap=5, Title='Novice Disciple'");
        }

        public void SaveState()
        {
            PlayerPrefs.SetInt(PREF_STAGE_INDEX, currentStageIndex);
            PlayerPrefs.Save();
        }

        public void LoadState()
        {
            if (PlayerPrefs.HasKey(PREF_STAGE_INDEX))
            {
                int loaded = PlayerPrefs.GetInt(PREF_STAGE_INDEX, 0);
                SetBreakthroughCountInternal(loaded, "LoadState", "PlayerPrefs loaded");
            }
            else
            {
                SetBreakthroughCountInternal(0, "LoadState", "Default initial state (no saved key)");
            }
        }

        public void ResetPersistence()
        {
            PlayerPrefs.DeleteKey(PREF_STAGE_INDEX);
            PlayerPrefs.Save();
            SetBreakthroughCountInternal(0, "ResetPersistence", "PlayerPrefs cleared");
        }
    }
}
