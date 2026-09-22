using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Progression
{
    [Serializable]
    public class SkillRuntimeState
    {
        public string SkillId;
        public bool IsUnlocked;
        public int SkillLevel = 1; // Future extensible
        public float CooldownDuration = 0f;
        public float CooldownEndTime = 0f;

        public SkillRuntimeState(string id, bool unlocked = false, int lvl = 1)
        {
            SkillId = id;
            IsUnlocked = unlocked;
            SkillLevel = lvl;
            CooldownDuration = 0f;
            CooldownEndTime = 0f;
        }

        public bool IsOnCooldown(float currentTime)
        {
            return currentTime < CooldownEndTime;
        }

        public float GetRemainingCooldown(float currentTime)
        {
            return Mathf.Max(0f, CooldownEndTime - currentTime);
        }

        public void TriggerCooldown(float duration, float currentTime)
        {
            if (duration <= 0f) return;
            CooldownDuration = duration;
            CooldownEndTime = currentTime + duration;
        }

        public void ResetCooldown()
        {
            CooldownDuration = 0f;
            CooldownEndTime = 0f;
        }
    }

    [Serializable]
    public class MindMethodRuntimeState
    {
        public string MindMethodId;
        public bool IsUnlocked;
        public int Level = 1;
        public Dictionary<SkillSlotType, string> SelectedSkillPerSlot = new Dictionary<SkillSlotType, string>();
        public Dictionary<string, SkillRuntimeState> SkillStates = new Dictionary<string, SkillRuntimeState>();
        public bool ReviveOnceAvailable = true;

        public MindMethodRuntimeState(string id, bool unlocked = false, int lvl = 1)
        {
            MindMethodId = id;
            IsUnlocked = unlocked;
            Level = Mathf.Max(1, lvl);
            ReviveOnceAvailable = true;
        }
    }

    public class MindMethodManager : MonoBehaviour
    {
        private static MindMethodManager instance;
        public static MindMethodManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<MindMethodManager>();
                }
                return instance;
            }
            private set => instance = value;
        }

        private const string PREF_ACTIVE_ID = "TLTD_MM_ActiveId";
        private const string PREF_UNLOCKED_PREFIX = "TLTD_MM_Unlocked_";
        private const string PREF_LEVEL_PREFIX = "TLTD_MM_Level_";
        private const string PREF_SLOT_PREFIX = "TLTD_MM_Slot_";
        private const string PREF_SKILL_UNLOCKED_PREFIX = "TLTD_Skill_Unlocked_";

        [Header("Database")]
        [SerializeField] private MindMethodDatabaseSO database;

        [Header("Runtime State")]
        [SerializeField] private string activeMindMethodId = "";
        private Dictionary<string, MindMethodRuntimeState> runtimeStates = new Dictionary<string, MindMethodRuntimeState>();

        public MindMethodDatabaseSO Database => database;
        public string ActiveMindMethodId => activeMindMethodId;

        public MindMethodDefinitionSO ActiveMindMethodDefinition
        {
            get
            {
                LoadDatabaseIfMissing();
                if (database == null) return null;
                return database.GetMindMethod(activeMindMethodId);
            }
        }

        public MindMethodRuntimeState ActiveMindMethodState
        {
            get
            {
                if (string.IsNullOrEmpty(activeMindMethodId)) return null;
                if (!runtimeStates.ContainsKey(activeMindMethodId))
                {
                    InitializeRuntimeStateForMethod(activeMindMethodId);
                }
                return runtimeStates[activeMindMethodId];
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            LoadDatabaseIfMissing();
            LoadState();
        }

        public void LoadDatabaseIfMissing()
        {
            if (database == null)
            {
                database = Resources.Load<MindMethodDatabaseSO>("Data/MindMethodDatabase");
#if UNITY_EDITOR
                if (database == null)
                {
                    database = UnityEditor.AssetDatabase.LoadAssetAtPath<MindMethodDatabaseSO>("Assets/_Game/Data/MindMethodDatabase.asset");
                }
#endif
            }
        }

        public void SetDatabase(MindMethodDatabaseSO db)
        {
            database = db;
            InitializeFromDatabase();
        }

        public void InitializeFromDatabase()
        {
            LoadDatabaseIfMissing();
            if (database == null || database.MindMethods == null) return;

            foreach (var mm in database.MindMethods)
            {
                if (mm == null) continue;
                if (!runtimeStates.ContainsKey(mm.MindMethodId))
                {
                    InitializeRuntimeStateForMethod(mm.MindMethodId);
                }
            }

            if (string.IsNullOrEmpty(activeMindMethodId) && database.DefaultMindMethod != null)
            {
                activeMindMethodId = database.DefaultMindMethod.MindMethodId;
            }
        }

        private MindMethodRuntimeState InitializeRuntimeStateForMethod(string id)
        {
            LoadDatabaseIfMissing();
            MindMethodDefinitionSO def = database != null ? database.GetMindMethod(id) : null;
            bool defaultUnlocked = def != null ? def.IsUnlockedByDefault : false;

            MindMethodRuntimeState state = new MindMethodRuntimeState(id, defaultUnlocked, 1);

            // Populate default skills for all 5 slots
            if (def != null && def.Skills != null)
            {
                foreach (var skill in def.Skills)
                {
                    if (skill == null) continue;
                    // First skill in slot is unlocked by default if skill has 0 conditions
                    bool skillUnlocked = (skill.UnlockConditions == null || skill.UnlockConditions.Count == 0);
                    state.SkillStates[skill.SkillId] = new SkillRuntimeState(skill.SkillId, skillUnlocked, 1);
                }

                for (int slotIdx = 1; slotIdx <= 5; slotIdx++)
                {
                    SkillSlotType slot = (SkillSlotType)slotIdx;
                    var defaultSkill = def.GetDefaultSkillForSlot(slot);
                    if (defaultSkill != null)
                    {
                        state.SelectedSkillPerSlot[slot] = defaultSkill.SkillId;
                        if (state.SkillStates.ContainsKey(defaultSkill.SkillId))
                        {
                            state.SkillStates[defaultSkill.SkillId].IsUnlocked = true;
                        }
                    }
                }
            }

            runtimeStates[id] = state;
            return state;
        }

        public MindMethodRuntimeState GetMindMethodState(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!runtimeStates.ContainsKey(id))
            {
                return InitializeRuntimeStateForMethod(id);
            }
            return runtimeStates[id];
        }

        public bool IsMindMethodUnlocked(string id)
        {
            var state = GetMindMethodState(id);
            return state != null && state.IsUnlocked;
        }

        public bool CanUnlockMindMethod(string id)
        {
            if (IsMindMethodUnlocked(id)) return false;
            LoadDatabaseIfMissing();
            MindMethodDefinitionSO def = database != null ? database.GetMindMethod(id) : null;
            if (def == null) return false;

            if (def.UnlockConditions == null || def.UnlockConditions.Count == 0)
            {
                return true;
            }

            int curHeroLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;
            int curBreakthroughCount = TitleBreakthroughManager.Instance != null ? TitleBreakthroughManager.Instance.CurrentBreakthroughCount : 0;
            int curLootTier = LootTierProgressionManager.Instance != null ? LootTierProgressionManager.Instance.CurrentTierId : 1;
            int curGold = ResourceManager.Instance != null ? ResourceManager.Instance.Gold : 0;
            int curMaterial = ResourceManager.Instance != null ? ResourceManager.Instance.Material : 0;

            foreach (var req in def.UnlockConditions)
            {
                switch (req.Type)
                {
                    case BreakthroughRequirementType.HeroLevel:
                        if (curHeroLevel < req.RequiredValue) return false;
                        break;
                    case BreakthroughRequirementType.LootTier:
                        if (curLootTier < req.RequiredValue) return false;
                        break;
                    case BreakthroughRequirementType.Gold:
                        if (curGold < req.RequiredValue) return false;
                        break;
                    case BreakthroughRequirementType.Material:
                        if (curMaterial < req.RequiredValue) return false;
                        break;
                    case BreakthroughRequirementType.MindMethodLevel:
                        int activeLvl = GetActiveMindMethodLevel();
                        if (activeLvl < req.RequiredValue) return false;
                        break;
                }
            }

            return true;
        }

        public bool UnlockMindMethod(string id)
        {
            var state = GetMindMethodState(id);
            if (state == null) return false;

            if (!state.IsUnlocked)
            {
                state.IsUnlocked = true;
                Debug.Log($"[MIND METHOD] Unlocked: {id}");
                EventBus.RaiseMindMethodUnlocked(id);
                SaveState();
                return true;
            }
            return false;
        }

        public bool SetActiveMindMethod(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            var state = GetMindMethodState(id);
            if (state == null || !state.IsUnlocked)
            {
                Debug.LogWarning($"[MIND METHOD] Cannot activate locked or unknown Mind Method '{id}'!");
                return false;
            }

            string oldId = activeMindMethodId;
            activeMindMethodId = id;
            state.ReviveOnceAvailable = true;

            Debug.Log($"[MIND METHOD] Active changed from '{oldId}' to '{activeMindMethodId}' (Level: {state.Level})");

            LoadDatabaseIfMissing();
            MindMethodDefinitionSO def = database != null ? database.GetMindMethod(activeMindMethodId) : null;
            EventBus.RaiseActiveMindMethodChanged(def);

            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            if (hero != null)
            {
                hero.RecalculateStats(true);
            }

            SaveState();
            return true;
        }

        public int GetMindMethodLevel(string id)
        {
            var state = GetMindMethodState(id);
            return state != null ? state.Level : 1;
        }

        public int GetActiveMindMethodLevel()
        {
            return GetMindMethodLevel(activeMindMethodId);
        }

        public void SetMindMethodLevel(string id, int level)
        {
            var state = GetMindMethodState(id);
            if (state == null) return;

            LoadDatabaseIfMissing();
            MindMethodDefinitionSO def = database != null ? database.GetMindMethod(id) : null;
            int maxLvl = def != null ? def.MaxLevel : 10;

            int oldLvl = state.Level;
            state.Level = Mathf.Clamp(level, 1, maxLvl);

            if (state.Level != oldLvl)
            {
                Debug.Log($"[MIND METHOD] Level changed for '{id}': {oldLvl} -> {state.Level}");
                EventBus.RaiseMindMethodLevelChanged(id, state.Level);

                if (id == activeMindMethodId)
                {
                    Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
                    if (hero != null) hero.RecalculateStats(true);
                }

                SaveState();
            }
        }

        public void AddMindMethodLevel(string id, int delta = 1)
        {
            SetMindMethodLevel(id, GetMindMethodLevel(id) + delta);
        }

        #region Skill Queries & Management

        public SkillRuntimeState GetSkillState(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;
            foreach (var kvp in runtimeStates)
            {
                if (kvp.Value.SkillStates.ContainsKey(skillId))
                {
                    return kvp.Value.SkillStates[skillId];
                }
            }
            return null;
        }

        public SkillRuntimeState GetOrCreateSkillState(string skillId, string mmId = null)
        {
            var state = GetSkillState(skillId);
            if (state != null) return state;

            string targetMmId = !string.IsNullOrEmpty(mmId) ? mmId : activeMindMethodId;
            if (string.IsNullOrEmpty(targetMmId))
            {
                var def = FindSkillDefinition(skillId);
                if (def != null) targetMmId = def.MindMethodId;
            }

            if (!string.IsNullOrEmpty(targetMmId))
            {
                var mmState = GetMindMethodState(targetMmId);
                if (mmState != null)
                {
                    state = new SkillRuntimeState(skillId, false, 1);
                    mmState.SkillStates[skillId] = state;
                    return state;
                }
            }
            return null;
        }

        public void ResetAllSkillCooldowns()
        {
            foreach (var kvp in runtimeStates)
            {
                if (kvp.Value != null && kvp.Value.SkillStates != null)
                {
                    foreach (var skKvp in kvp.Value.SkillStates)
                    {
                        skKvp.Value?.ResetCooldown();
                    }
                }
            }
        }

        public bool IsSkillUnlocked(string skillId)
        {
            var state = GetSkillState(skillId);
            return state != null && state.IsUnlocked;
        }

        public bool CanUnlockSkill(string skillId)
        {
            if (IsSkillUnlocked(skillId)) return false;
            LoadDatabaseIfMissing();
            SkillDefinitionSO skillDef = FindSkillDefinition(skillId);
            if (skillDef == null) return false;

            if (skillDef.UnlockConditions == null || skillDef.UnlockConditions.Count == 0)
            {
                return true;
            }

            int curHeroLevel = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentLevel : 1;
            int mmLevel = GetMindMethodLevel(skillDef.MindMethodId);
            int curLootTier = LootTierProgressionManager.Instance != null ? LootTierProgressionManager.Instance.CurrentTierId : 1;

            foreach (var req in skillDef.UnlockConditions)
            {
                switch (req.Type)
                {
                    case SkillRequirementType.HeroLevel:
                        if (curHeroLevel < req.RequiredValue) return false;
                        break;
                    case SkillRequirementType.MindMethodLevel:
                        if (mmLevel < req.RequiredValue) return false;
                        break;
                    case SkillRequirementType.LootTier:
                        if (curLootTier < req.RequiredValue) return false;
                        break;
                }
            }

            return true;
        }

        public bool UnlockSkill(string skillId)
        {
            var state = GetSkillState(skillId);
            if (state == null)
            {
                // Create runtime state if missing
                SkillDefinitionSO def = FindSkillDefinition(skillId);
                if (def != null && runtimeStates.ContainsKey(def.MindMethodId))
                {
                    state = new SkillRuntimeState(skillId, true, 1);
                    runtimeStates[def.MindMethodId].SkillStates[skillId] = state;
                }
            }

            if (state != null && !state.IsUnlocked)
            {
                state.IsUnlocked = true;
                Debug.Log($"[SKILL] Unlocked skill: '{skillId}'");
                EventBus.RaiseSkillUnlocked(skillId);
                SaveState();
                return true;
            }
            return false;
        }

        public bool SelectSkillForSlot(SkillSlotType slot, string skillId)
        {
            var activeState = ActiveMindMethodState;
            if (activeState == null) return false;

            SkillDefinitionSO skillDef = FindSkillDefinition(skillId);
            if (skillDef == null)
            {
                Debug.LogWarning($"[SKILL] Cannot select unknown skill '{skillId}'!");
                return false;
            }

            if (skillDef.MindMethodId != activeMindMethodId)
            {
                Debug.LogWarning($"[SKILL] Skill '{skillId}' does not belong to active Mind Method '{activeMindMethodId}'!");
                return false;
            }

            if (skillDef.SlotType != slot)
            {
                Debug.LogWarning($"[SKILL] Skill '{skillId}' (Slot {skillDef.SlotType}) does not match requested slot {slot}!");
                return false;
            }

            if (!IsSkillUnlocked(skillId))
            {
                Debug.LogWarning($"[SKILL] Skill '{skillId}' is LOCKED!");
                return false;
            }

            activeState.SelectedSkillPerSlot[slot] = skillId;
            Debug.Log($"[SKILL] Slot {slot} selected skill: '{skillId}' ({skillDef.SkillName})");
            EventBus.RaiseSkillSelected(slot, skillId);
            SaveState();
            return true;
        }

        public string GetSelectedSkillIdForSlot(SkillSlotType slot)
        {
            var activeState = ActiveMindMethodState;
            if (activeState != null && activeState.SelectedSkillPerSlot.ContainsKey(slot))
            {
                return activeState.SelectedSkillPerSlot[slot];
            }
            return "";
        }

        public SkillDefinitionSO GetSelectedSkillForSlot(SkillSlotType slot)
        {
            string id = GetSelectedSkillIdForSlot(slot);
            return FindSkillDefinition(id);
        }

        public SkillDefinitionSO FindSkillDefinition(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;
            LoadDatabaseIfMissing();
            if (database == null || database.MindMethods == null) return null;

            foreach (var mm in database.MindMethods)
            {
                if (mm == null || mm.Skills == null) continue;
                foreach (var s in mm.Skills)
                {
                    if (s != null && s.SkillId == skillId) return s;
                }
            }
            return null;
        }

        #endregion

        #region Passive & Combat Queries

        public Dictionary<StatType, float> GetActivePassiveStats()
        {
            var def = ActiveMindMethodDefinition;
            var state = ActiveMindMethodState;
            if (def == null || state == null || def.PassiveData == null)
            {
                return new Dictionary<StatType, float>();
            }
            return def.PassiveData.GetStatModifiers(state.Level);
        }

        public float GetActiveAttackRageBonus()
        {
            var def = ActiveMindMethodDefinition;
            if (def == null || def.PassiveData == null) return 0f;
            return def.PassiveData.BasicAttackRageModifier;
        }

        public float GetActiveDamageRageBonus()
        {
            var def = ActiveMindMethodDefinition;
            if (def == null || def.PassiveData == null) return 0f;
            return def.PassiveData.DamageTakenRageModifier;
        }

        public bool TryTriggerReviveOnce(out float reviveHealth)
        {
            reviveHealth = 0f;
            var def = ActiveMindMethodDefinition;
            var state = ActiveMindMethodState;
            if (def == null || state == null || def.PassiveData == null) return false;

            if (def.PassiveData.HasReviveOnce && state.ReviveOnceAvailable)
            {
                state.ReviveOnceAvailable = false;
                Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
                float maxHp = (hero != null && hero.Health != null) ? hero.Health.MaxHealth : 1000f;
                reviveHealth = maxHp * (def.PassiveData.ReviveHealthPercent / 100f);
                return true;
            }
            return false;
        }

        public void ResetReviveOnce()
        {
            var state = ActiveMindMethodState;
            if (state != null)
            {
                state.ReviveOnceAvailable = true;
            }
        }

        #endregion

        #region Persistence & Test Reset

        public void SaveState()
        {
            PlayerPrefs.SetString(PREF_ACTIVE_ID, activeMindMethodId);

            foreach (var kvp in runtimeStates)
            {
                string id = kvp.Key;
                var state = kvp.Value;
                PlayerPrefs.SetInt(PREF_UNLOCKED_PREFIX + id, state.IsUnlocked ? 1 : 0);
                PlayerPrefs.SetInt(PREF_LEVEL_PREFIX + id, state.Level);

                for (int i = 1; i <= 5; i++)
                {
                    SkillSlotType slot = (SkillSlotType)i;
                    if (state.SelectedSkillPerSlot.ContainsKey(slot))
                    {
                        PlayerPrefs.SetString($"{PREF_SLOT_PREFIX}{id}_{i}", state.SelectedSkillPerSlot[slot]);
                    }
                }

                foreach (var sk in state.SkillStates)
                {
                    PlayerPrefs.SetInt(PREF_SKILL_UNLOCKED_PREFIX + sk.Key, sk.Value.IsUnlocked ? 1 : 0);
                }
            }

            PlayerPrefs.Save();
        }

        public void LoadState()
        {
            InitializeFromDatabase();

            if (PlayerPrefs.HasKey(PREF_ACTIVE_ID))
            {
                string savedActive = PlayerPrefs.GetString(PREF_ACTIVE_ID);
                if (!string.IsNullOrEmpty(savedActive) && runtimeStates.ContainsKey(savedActive))
                {
                    activeMindMethodId = savedActive;
                }
            }

            foreach (var kvp in runtimeStates)
            {
                string id = kvp.Key;
                var state = kvp.Value;

                if (PlayerPrefs.HasKey(PREF_UNLOCKED_PREFIX + id))
                {
                    state.IsUnlocked = PlayerPrefs.GetInt(PREF_UNLOCKED_PREFIX + id) == 1;
                }

                if (PlayerPrefs.HasKey(PREF_LEVEL_PREFIX + id))
                {
                    state.Level = Mathf.Max(1, PlayerPrefs.GetInt(PREF_LEVEL_PREFIX + id));
                }

                for (int i = 1; i <= 5; i++)
                {
                    SkillSlotType slot = (SkillSlotType)i;
                    string key = $"{PREF_SLOT_PREFIX}{id}_{i}";
                    if (PlayerPrefs.HasKey(key))
                    {
                        string skId = PlayerPrefs.GetString(key);
                        if (!string.IsNullOrEmpty(skId))
                        {
                            state.SelectedSkillPerSlot[slot] = skId;
                        }
                    }
                }

                foreach (var sk in state.SkillStates)
                {
                    if (PlayerPrefs.HasKey(PREF_SKILL_UNLOCKED_PREFIX + sk.Key))
                    {
                        sk.Value.IsUnlocked = PlayerPrefs.GetInt(PREF_SKILL_UNLOCKED_PREFIX + sk.Key) == 1;
                    }
                }
            }
        }

        public void ResetPersistence()
        {
            PlayerPrefs.DeleteKey(PREF_ACTIVE_ID);
            if (database != null && database.MindMethods != null)
            {
                foreach (var mm in database.MindMethods)
                {
                    if (mm == null) continue;
                    PlayerPrefs.DeleteKey(PREF_UNLOCKED_PREFIX + mm.MindMethodId);
                    PlayerPrefs.DeleteKey(PREF_LEVEL_PREFIX + mm.MindMethodId);
                    for (int i = 1; i <= 5; i++)
                    {
                        PlayerPrefs.DeleteKey($"{PREF_SLOT_PREFIX}{mm.MindMethodId}_{i}");
                    }
                    if (mm.Skills != null)
                    {
                        foreach (var s in mm.Skills)
                        {
                            if (s != null) PlayerPrefs.DeleteKey(PREF_SKILL_UNLOCKED_PREFIX + s.SkillId);
                        }
                    }
                }
            }
            PlayerPrefs.Save();
            runtimeStates.Clear();
            activeMindMethodId = "";
            InitializeFromDatabase();
        }

        public static void ResetInstance()
        {
            if (instance != null)
            {
                instance.runtimeStates.Clear();
                instance.activeMindMethodId = "";
            }
            instance = null;
        }

        #endregion
    }
}
