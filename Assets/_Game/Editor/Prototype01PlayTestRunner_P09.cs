#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Progression;

namespace WuxiaGame.Editor
{
    public static class Prototype01PlayTestRunner_P09
    {
        private static string _savedActiveMmId;

        [MenuItem("Tools/Wuxia RPG/Run P09-A Projectile Foundation Tests (T01 - T21)")]
        public static bool RunAllP09Tests()
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "P09-A Safe Execution Notice",
                    "Running P09-A foundation tests directly in the interactive Editor mutates PlayerPrefs.\n\n" +
                    "To safely run these tests with full Save Guard isolation (pre-run backup, journal check, and exact restore), please execute the verification wrapper:\n\n" +
                    "  Tools\\Verification\\P09\\run_gate1_p09.ps1\n\n" +
                    "Direct Editor execution is blocked to protect your workspace persistence.",
                    "OK"
                );
                return false;
            }

            return RunAllP09TestsInternal();
        }

        public static bool RunAllP09TestsInternal()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-A PROJECTILE FOUNDATION AUTOMATED SUITE (T01 -> T21)");
            Debug.Log("================================================================================");

            int passed = 0;
            int total = 21;

            bool t01 = T01_DefaultImmediateSkillRetainsDamageRageCooldownAndTiming();
            if (t01) passed++;

            bool t02 = T02_ProjectileReleaseProducesZeroEarlyDamage_NaturalArrivalProducesExactDamage();
            if (t02) passed++;

            bool t03 = T03_MovingTargetRemainsBound_HeroCurrentTargetChangeDoesNotRedirect();
            if (t03) passed++;

            bool t04 = T04_TargetDeathBeforeArrival_CancelsWithoutHit();
            if (t04) passed++;

            bool t05 = T05_CasterDeathBeforeArrival_CancelsWithoutHit();
            if (t05) passed++;

            bool t06 = T06_MultipleSimultaneousProjectilesIndependent_NoDoubleHit();
            if (t06) passed++;

            bool t07 = T07_EncounterAdvancementCancelsOutstandingProjectiles();
            if (t07) passed++;

            bool t08 = T08_InvalidDeliveryConfigurationsRejectedBeforeRageAndCooldown();
            if (t08) passed++;

            bool t09 = T09_CastTimeProjectileChargesRageAtStart_ReleasesAtCastEnd_StartsCooldownOnce();
            if (t09) passed++;

            bool t10 = T10_PauseFreezesTravel_ResumeCompletesArrival();
            if (t10) passed++;

            bool t11 = T11_DisablingVisualRendererHasZeroCombatAuthority();
            if (t11) passed++;

            bool t12 = T12_LifetimeExpiry_CancelsWithoutHit();
            if (t12) passed++;

            bool t13 = T13_CancellationDuringCombatPause();
            if (t13) passed++;

            bool t14 = T14_CasterDestroyedBeforeArrival_CancelsWithoutHit();
            if (t14) passed++;

            bool t15 = T15_MixedPolicySkill_RejectedBeforeRageAndCooldown();
            if (t15) passed++;

            bool t16 = T16_ResetInstanceWhileManagerAndTargetExist_CancelsWithoutHit();
            if (t16) passed++;

            bool t17 = T17_ReplaceInstanceWhileOldManagerAlive_CancelsWithoutHit();
            if (t17) passed++;

            bool t18 = T18_LivingTargetUnregisteredFromEncounter_CancelsWithoutHit();
            if (t18) passed++;

            bool t19 = T19_EncounterIndexChangedDuringPause_CancelsBeforeUnpause();
            if (t19) passed++;

            bool t20 = T20_DisableControllerAndReenable_DoesNotResurrectPayload();
            if (t20) passed++;

            bool t21 = T21_SceneUnload_CancelsProjectilesWithoutDamage();
            if (t21) passed++;

            bool allPass = (passed == total);
            Debug.Log("================================================================================");
            Debug.Log($"   [P09 AUTOMATED TESTS RESULT]: {passed}/{total} PASSED | ALL_PASS={allPass}");
            Debug.Log("================================================================================");

            return allPass;
        }

        public static void RunGate1_P09_CLI()
        {
            try
            {
                bool pass = RunAllP09TestsInternal();
                EditorApplication.Exit(pass ? 0 : 1);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[P09 CLI FATAL] Exception: {ex}");
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Wuxia RPG/Run P09-A Play Mode Scenario (Natural Frames)")]
        public static void RunP09PlayModeScenarioMenu()
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "P09-A Safe Execution Notice",
                    "Running this Play Mode verification scenario directly in the interactive Editor will replace your active scene and mutate PlayerPrefs.\n\n" +
                    "To safely run this test with full Save Guard isolation (pre-run backup, journal check, and exact restore), please execute the verification wrapper:\n\n" +
                    "  Tools\\Verification\\P09\\run_gate2_playmode_p09.ps1\n\n" +
                    "Direct Editor execution is blocked to protect your workspace persistence.",
                    "OK"
                );
                return;
            }

            if (EditorApplication.isPlaying)
            {
                CreateHarnessIfMissing();
            }
            else
            {
                SessionState.SetBool("RunP09PlayModeScenario", true);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        private static void CreateHarnessIfMissing()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<P09PlayModeHarness>();
            if (existing != null) return;

            var go = new GameObject("P09_PlayMode_Harness_Runner");
            var harness = go.AddComponent<P09PlayModeHarness>();
            harness.StartCoroutine(harness.RunScenarioCoroutine());
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeWatcher()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("RunP09PlayModeScenario", false))
                {
                    SessionState.SetBool("RunP09PlayModeScenario", false);
                    CreateHarnessIfMissing();
                }
            };

            EditorApplication.update += () =>
            {
                if (EditorApplication.isPlaying && SessionState.GetBool("RunP09PlayModeScenario", false))
                {
                    SessionState.SetBool("RunP09PlayModeScenario", false);
                    CreateHarnessIfMissing();
                }
            };
        }

        #region Helpers

        private static void SetupEncounter(
            out GameObject heroGO, out Hero hero,
            out GameObject bmGO, out BattleManager bm,
            out GameObject mmMgrGO, out MindMethodManager mmMgr)
        {
            ProjectileController.ClearAllProjectiles();
            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();
            EventBus.ClearAllListeners();

            // Snapshot existing active MindMethod preference to guarantee in-process restore
            _savedActiveMmId = PlayerPrefs.HasKey("TLTD_MM_ActiveId") ? PlayerPrefs.GetString("TLTD_MM_ActiveId") : null;

            // Services
            GameObject servicesGO = new GameObject("TestServices");
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            servicesGO.AddComponent<ProgressionManager>();

            mmMgrGO = new GameObject("TestMindMethodManager");
            mmMgr = mmMgrGO.AddComponent<MindMethodManager>();

            // Transient in-memory MindMethod database
            var tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
            var tempMmDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
            tempMmDef.InitializeMindMethod("mm_taiji", "Taiji", "Test Taiji", 10, true, new MindMethodPassiveData());
            tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { tempMmDef });
            mmMgr.SetDatabase(tempDb);
            mmMgr.SetActiveMindMethod("mm_taiji");

            // Hero
            heroGO = new GameObject("TestHero");
            hero = heroGO.AddComponent<Hero>();
            hero.InitializeHero();
            heroGO.transform.position = new Vector3(-3f, -0.3f, 0f);

            // BattleManager
            bmGO = new GameObject("TestBM");
            bm = bmGO.AddComponent<BattleManager>();
            bm.RegisterHero(hero);
            bm.StartBattle();
        }

        private static void TeardownEncounter(GameObject heroGO, GameObject bmGO, GameObject mmMgrGO)
        {
            ProjectileController.ClearAllProjectiles();

            if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
            if (bmGO != null)
            {
                var bm = bmGO.GetComponent<BattleManager>();
                if (bm != null)
                {
                    if (bm.ActiveMonsters != null)
                    {
                        var copy = new List<Monster>(bm.ActiveMonsters);
                        foreach (var m in copy)
                        {
                            if (m != null) UnityEngine.Object.DestroyImmediate(m.gameObject);
                        }
                    }
                }
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            if (mmMgrGO != null)
            {
                var mmMgr = mmMgrGO.GetComponent<MindMethodManager>();
                if (mmMgr != null && mmMgr.Database != null)
                {
                    if (mmMgr.Database.MindMethods != null)
                    {
                        foreach (var mm in mmMgr.Database.MindMethods)
                        {
                            if (mm != null) UnityEngine.Object.DestroyImmediate(mm);
                        }
                    }
                    UnityEngine.Object.DestroyImmediate(mmMgr.Database);
                }
                UnityEngine.Object.DestroyImmediate(mmMgrGO);
            }

            GameObject servicesGO = GameObject.Find("TestServices");
            if (servicesGO != null) UnityEngine.Object.DestroyImmediate(servicesGO);

            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();
            EventBus.ClearAllListeners();

            // Revert PlayerPrefs mutations caused by MindMethodManager in tests
            if (_savedActiveMmId != null)
            {
                PlayerPrefs.SetString("TLTD_MM_ActiveId", _savedActiveMmId);
            }
            else
            {
                PlayerPrefs.DeleteKey("TLTD_MM_ActiveId");
            }
            PlayerPrefs.DeleteKey("TLTD_MM_Unlocked_mm_taiji");
            PlayerPrefs.DeleteKey("TLTD_MM_Level_mm_taiji");
            for (int i = 1; i <= 5; i++)
            {
                PlayerPrefs.DeleteKey($"TLTD_MM_Slot_mm_taiji_{i}");
            }
            PlayerPrefs.Save();
        }

        private static SkillDefinitionSO CreateTestSkill(
            string skillId,
            string name,
            float dmgMultiplier,
            float rageCost,
            float cd,
            bool isProjectile,
            float speed = 15f,
            float lifetime = 5f,
            float castTime = 0f,
            bool isChannel = false,
            SkillTargetPolicy targetPolicy = SkillTargetPolicy.SingleTarget,
            SkillSlotType slotType = SkillSlotType.Skill)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            var effect = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            effect.Initialize(dmgMultiplier, targetPolicy, DamageType.Skill);

            skill.InitializeSkill(
                id: skillId,
                mmId: "mm_taiji",
                slot: slotType,
                name: name,
                desc: "P09 Test Skill",
                conditions: null,
                dmgMultiplier: dmgMultiplier,
                costRage: rageCost,
                cd: cd,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { effect },
                shatterFreeze: false,
                skillCastTime: castTime,
                channel: isChannel,
                chDuration: 0f,
                chTickInterval: 0f,
                skillPriority: 50,
                projectile: isProjectile,
                projSpeed: speed,
                projLifetime: lifetime
            );
            return skill;
        }

        private static SkillDefinitionSO CreateMultiEffectTestSkill(
            string skillId,
            string name,
            float rageCost,
            float cd,
            bool isProjectile,
            float speed,
            float lifetime,
            List<SkillEffectDefinitionSO> effects)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            skill.InitializeSkill(
                id: skillId,
                mmId: "mm_taiji",
                slot: SkillSlotType.Skill,
                name: name,
                desc: "Multi Effect Test Skill",
                conditions: null,
                dmgMultiplier: 1.0f,
                costRage: rageCost,
                cd: cd,
                passive: false,
                skillEffects: effects,
                shatterFreeze: false,
                skillCastTime: 0f,
                channel: false,
                chDuration: 0f,
                chTickInterval: 0f,
                skillPriority: 50,
                projectile: isProjectile,
                projSpeed: speed,
                projLifetime: lifetime
            );
            return skill;
        }

        private static void RegisterAndSelectSkill(MindMethodManager mmMgr, SkillDefinitionSO skill)
        {
            if (mmMgr == null || skill == null) return;
            mmMgr.SetActiveMindMethod(skill.MindMethodId);

            var activeDef = mmMgr.ActiveMindMethodDefinition;
            if (activeDef != null)
            {
                var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = skillsField != null ? skillsField.GetValue(activeDef) as List<SkillDefinitionSO> : null;
                if (list != null && !list.Contains(skill))
                {
                    list.Add(skill);
                }
            }

            var activeState = mmMgr.ActiveMindMethodState;
            if (activeState != null)
            {
                activeState.SkillStates[skill.SkillId] = new SkillRuntimeState(skill.SkillId, true, 1);
                activeState.SelectedSkillPerSlot[skill.SlotType] = skill.SkillId;
            }
        }

        #endregion

        #region Tests

        /// <summary>
        /// T01: Existing immediate skills retain current immediate damage, Rage, cooldown, and timing.
        /// </summary>
        private static bool T01_DefaultImmediateSkillRetainsDamageRageCooldownAndTiming()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t01_imm", "Thái Cực Chưởng", 2.0f, 20f, 3.0f, isProjectile: false);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(2f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                var res = SkillExecutor.Execute(req);

                bool success = res.Success;
                bool hpReducedImmediately = target.Health.CurrentHealth < hpBefore;
                bool rageCharged = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool cooldownStarted = CooldownManager.IsOnCooldown(skill.SkillId, out float cdRemain) && cdRemain > 0f;
                bool noProjectilesSpawned = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = success && hpReducedImmediately && rageCharged && cooldownStarted && noProjectilesSpawned;
                Debug.Log($"[T01] Default Immediate Skill: Success={success}, ImmediateDmg={hpReducedImmediately}, RageCharged={rageCharged}, Cooldown={cooldownStarted}, ProjCount={ProjectileController.ActiveProjectiles.Count} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T02: Projectile release produces zero early damage; natural arrival produces exactly one matching damage event.
        /// </summary>
        private static bool T02_ProjectileReleaseProducesZeroEarlyDamage_NaturalArrivalProducesExactDamage()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;
            Action<Entity, DamageResult> damageListener = null;
            ITimeProvider origTimeProvider = CooldownManager.TimeProvider;
            TestTimeProvider testTime = new TestTimeProvider(100f);
            CooldownManager.TimeProvider = testTime;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t02_proj", "Phi Kiếm Quyết", 2.0f, 25f, 4.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(10f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // Track exact damage events and correlation without synthetic skill id injection
                int damageEventCount = 0;
                Entity damagedTarget = null;
                Entity damageSource = null;
                DamageResult recordedDmgRes = default;

                damageListener = (ent, dmgRes) =>
                {
                    damageEventCount++;
                    damagedTarget = ent;
                    damageSource = dmgRes.Attacker;
                    recordedDmgRes = dmgRes;
                };
                EventBus.OnEntityDamaged += damageListener;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                var res = SkillExecutor.Execute(req);

                // Step 1: Verification at Release
                bool success = res.Success;
                bool zeroEarlyDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool rageChargedAtRelease = Mathf.Approximately(hero.Rage.CurrentRage, 75f);
                bool cdStartedAtRelease = CooldownManager.IsOnCooldown(skill.SkillId, out float cdInitial) && Mathf.Approximately(cdInitial, 4.0f);
                bool projectileActive = ProjectileController.ActiveProjectiles.Count == 1;
                bool zeroEventsAtRelease = damageEventCount == 0;

                var proj = projectileActive ? ProjectileController.ActiveProjectiles[0] : null;

                // Step 2: Verification at Halfway Travel (0.5s at 10 speed = 5 units) with test clock advance
                testTime.Advance(0.5f);
                if (proj != null)
                {
                    proj.SimulateTick(0.5f);
                }
                bool zeroMidwayDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool stillFlying = proj != null && !proj.HasImpacted && !proj.IsCancelled;
                bool zeroEventsMidway = damageEventCount == 0;
                bool cdDecreasedMidway = CooldownManager.IsOnCooldown(skill.SkillId, out float cdMid) && Mathf.Abs(cdMid - (cdInitial - 0.5f)) < 0.05f;

                // Step 3: Verification at Arrival (remaining 5 units at 10 speed = 0.5s) with test clock advance
                testTime.Advance(0.5f);
                if (proj != null)
                {
                    proj.SimulateTick(0.5f);
                }
                bool damageDealtOnArrival = target.Health.CurrentHealth < hpBefore;
                bool projCompleted = proj == null || proj.HasImpacted;

                // Cooldown verification after arrival: clock advanced by 1.0s, remaining must be exactly cdInitial - 1.0s and NOT reset back to 4.0s
                bool cdAfterArrivalOk = CooldownManager.IsOnCooldown(skill.SkillId, out float cdAfter) && Mathf.Abs(cdAfter - (cdInitial - 1.0f)) < 0.05f;
                bool cdNotResetAtImpact = cdAfterArrivalOk && (cdAfter < cdInitial);

                // Assert specific target, specific request source, damage type, exactly one event
                bool exactlyOneDamageEvent = damageEventCount == 1;
                bool correctTargetMatched = damagedTarget == target && recordedDmgRes.Target == target;
                bool correctSourceMatched = damageSource == hero && recordedDmgRes.Attacker == hero;
                bool correctDamageType = recordedDmgRes.DamageType == DamageType.Skill;
                bool rageNotChargedAgainAtImpact = Mathf.Approximately(hero.Rage.CurrentRage, 75f);

                // Step 4: Subsequent tick after impact must NOT deal duplicate damage
                testTime.Advance(0.5f);
                if (proj != null)
                {
                    proj.SimulateTick(0.5f);
                }
                bool noDuplicateDamageAfterImpact = damageEventCount == 1;

                bool pass = success && zeroEarlyDamage && rageChargedAtRelease && cdStartedAtRelease && projectileActive &&
                            zeroEventsAtRelease && zeroMidwayDamage && stillFlying && zeroEventsMidway && cdDecreasedMidway &&
                            damageDealtOnArrival && projCompleted && exactlyOneDamageEvent && correctTargetMatched &&
                            correctSourceMatched && correctDamageType && rageNotChargedAgainAtImpact &&
                            cdNotResetAtImpact && noDuplicateDamageAfterImpact;

                Debug.Log($"[T02] Projectile Release & Arrival: ReleaseOk={success}, ZeroEarlyDmg={zeroEarlyDamage}, DmgEvents={damageEventCount}, TargetMatch={correctTargetMatched}, RageKept={rageNotChargedAgainAtImpact}, CdMidOk={cdDecreasedMidway}, CdNoReset={cdNotResetAtImpact}, NoDup={noDuplicateDamageAfterImpact} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                CooldownManager.TimeProvider = origTimeProvider;
                if (damageListener != null) EventBus.OnEntityDamaged -= damageListener;
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T03: Target movement keeps homing bound to original target; hero changing CurrentTarget does NOT redirect projectile.
        /// </summary>
        private static bool T03_MovingTargetRemainsBound_HeroCurrentTargetChangeDoesNotRedirect()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t03_homing", "Hư Không Kiếm", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster targetA = bm.ActiveMonsters[0];
                Monster targetB = bm.ActiveMonsters[1];
                targetA.transform.position = new Vector3(5f, -0.3f, 0f);
                targetB.transform.position = new Vector3(-1f, -0.3f, 0f);

                float aHpBefore = targetA.Health.CurrentHealth;
                float bHpBefore = targetB.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, targetA);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Redirect hero's current target to targetB
                hero.SetCurrentTarget(targetB);

                // Move targetA dynamically
                targetA.transform.position = new Vector3(7f, -0.3f, 0f);

                // Tick projectile travel to arrival
                if (proj != null)
                {
                    proj.SimulateTick(1.0f);
                }

                bool targetAHit = targetA.Health.CurrentHealth < aHpBefore;
                bool targetBUntouched = Mathf.Approximately(targetB.Health.CurrentHealth, bHpBefore);

                bool pass = targetAHit && targetBUntouched;
                Debug.Log($"[T03] Moving Target & Hero Retarget: TargetAHit={targetAHit}, TargetBUntouched={targetBUntouched} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T04: Target death before arrival cancels projectile cleanly without dealing damage to any replacement target.
        /// </summary>
        private static bool T04_TargetDeathBeforeArrival_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t04_death", "Tàn Ảnh Kiếm", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster targetA = bm.ActiveMonsters[0];
                Monster targetB = bm.ActiveMonsters[1];
                targetA.transform.position = new Vector3(8f, -0.3f, 0f);
                targetB.transform.position = new Vector3(8f, -0.3f, 0f);

                float bHpBefore = targetB.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, targetA);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Advance 0.2s mid-flight
                if (proj != null)
                {
                    proj.SimulateTick(0.2f);
                }

                // Simulate targetA death via legitimate death flow
                EventBus.RaiseEntityDied(targetA);

                // Projectile should be cancelled cleanly
                bool cancelledOnDeath = proj == null || proj.IsCancelled;

                // Tick after death to verify no damage applied to targetB
                if (proj != null)
                {
                    proj.SimulateTick(1.0f);
                }

                bool targetBUntouched = Mathf.Approximately(targetB.Health.CurrentHealth, bHpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = cancelledOnDeath && targetBUntouched && cleanedUp;
                Debug.Log($"[T04] Target Death Before Arrival: Cancelled={cancelledOnDeath}, BUntouched={targetBUntouched}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T05: Caster death before arrival cancels projectile cleanly without dealing damage to target.
        /// </summary>
        private static bool T05_CasterDeathBeforeArrival_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t05_caster_death", "Tuyệt Mệnh Kiếm", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(8f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Advance 0.2s mid-flight
                if (proj != null)
                {
                    proj.SimulateTick(0.2f);
                }

                // Caster dies
                EventBus.RaiseEntityDied(hero);

                bool cancelledOnCasterDeath = proj == null || proj.IsCancelled;

                if (proj != null)
                {
                    proj.SimulateTick(1.0f);
                }

                bool targetUntouched = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = cancelledOnCasterDeath && targetUntouched && cleanedUp;
                Debug.Log($"[T05] Caster Death Before Arrival: Cancelled={cancelledOnCasterDeath}, TargetUntouched={targetUntouched} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T06: Multiple simultaneous projectiles from same or different skills execute independently without double hits.
        /// </summary>
        private static bool T06_MultipleSimultaneousProjectilesIndependent_NoDoubleHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill1 = null;
            SkillDefinitionSO skill2 = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill1 = CreateTestSkill("p09_t06_s1", "Song Kiếm 1", 1.5f, 10f, 0f, isProjectile: true, speed: 10f, lifetime: 5f, slotType: SkillSlotType.Skill);
                skill2 = CreateTestSkill("p09_t06_s2", "Song Kiếm 2", 1.5f, 10f, 0f, isProjectile: true, speed: 20f, lifetime: 5f, slotType: SkillSlotType.Ultimate);
                RegisterAndSelectSkill(mmMgr, skill1);
                RegisterAndSelectSkill(mmMgr, skill2);

                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(10f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                hero.Rage.ResetRage(100f);

                // Launch projectile 1 (slow, 10 speed)
                var req1 = new SkillExecutionRequest(hero, skill1, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req1);

                // Launch projectile 2 (fast, 20 speed)
                var req2 = new SkillExecutionRequest(hero, skill2, SkillSlotType.Ultimate, target);
                SkillExecutor.Execute(req2);

                bool twoProjectilesActive = ProjectileController.ActiveProjectiles.Count == 2;
                var p1 = ProjectileController.ActiveProjectiles[0];
                var p2 = ProjectileController.ActiveProjectiles[1];

                // Advance 0.6s -> p2 travels 12 units (impacts at 10 units), p1 travels 6 units (mid-flight)
                p2.SimulateTick(0.6f);
                p1.SimulateTick(0.6f);

                bool p2Impacted = ((object)p2 != null && p2.HasImpacted) || p2 == null;
                bool p1StillFlying = (object)p1 != null && !p1.HasImpacted && !p1.IsCancelled && p1 != null;
                float hpAfterFirstImpact = target.Health.CurrentHealth;
                bool firstDamageDealt = hpAfterFirstImpact < hpBefore;

                // Advance another 0.6s -> p1 completes remaining distance and impacts
                p1.SimulateTick(0.6f);

                bool p1Impacted = ((object)p1 != null && p1.HasImpacted) || p1 == null;
                float hpAfterSecondImpact = target.Health.CurrentHealth;
                bool secondDamageDealt = hpAfterSecondImpact < hpAfterFirstImpact;

                bool allCleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = twoProjectilesActive && p2Impacted && p1StillFlying && firstDamageDealt &&
                            p1Impacted && secondDamageDealt && allCleanedUp;

                Debug.Log($"[T06] Multiple Simultaneous Projectiles: TwoActive={twoProjectilesActive}, BothImpacted={(p1Impacted && p2Impacted)}, CleanedUp={allCleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill1 != null) UnityEngine.Object.DestroyImmediate(skill1);
                if (skill2 != null) UnityEngine.Object.DestroyImmediate(skill2);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T07: Encounter advancement / reset cancels outstanding projectiles; no damage leaked into subsequent wave.
        /// </summary>
        private static bool T07_EncounterAdvancementCancelsOutstandingProjectiles()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t07_wave", "Lạc Lôi Quyết", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 10f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(20f, -0.3f, 0f);

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;
                int initialEncounter = bm.EncounterIndex;

                // Legitimate wave defeat / encounter advancement: increment EncounterIndex
                var encField = typeof(BattleManager).GetProperty("EncounterIndex");
                if (encField != null)
                {
                    encField.SetValue(bm, initialEncounter + 1);
                }

                // Simulate tick after encounter incremented
                if (proj != null)
                {
                    proj.SimulateTick(0.1f);
                }

                bool cancelled = proj == null || proj.IsCancelled;
                bool noActiveProjectiles = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = cancelled && noActiveProjectiles;
                Debug.Log($"[T07] Encounter Advancement Projectile Cancellation: Cancelled={cancelled}, NoActive={noActiveProjectiles} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T08: Invalid delivery configurations rejected before Rage and Cooldown.
        /// </summary>
        private static bool T08_InvalidDeliveryConfigurationsRejectedBeforeRageAndCooldown()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO chanProj = null, areaProj = null, badSpeed = null, badLife = null, mixedProj = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // Subtest 8A: Channel + Projectile
                chanProj = CreateTestSkill("p09_sub8a", "Channel Proj", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 0f, isChannel: true);
                RegisterAndSelectSkill(mmMgr, chanProj);
                var res8A = SkillExecutor.Execute(new SkillExecutionRequest(hero, chanProj, SkillSlotType.Skill, target));
                bool sub8AOk = !res8A.Success && res8A.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                               Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(chanProj.SkillId, out _);

                // Subtest 8B: Area + Projectile
                areaProj = CreateTestSkill("p09_sub8b", "Area Proj", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 0f, isChannel: false, targetPolicy: SkillTargetPolicy.Area);
                RegisterAndSelectSkill(mmMgr, areaProj);
                var res8B = SkillExecutor.Execute(new SkillExecutionRequest(hero, areaProj, SkillSlotType.Skill, target));
                bool sub8BOk = !res8B.Success && res8B.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                               Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(areaProj.SkillId, out _);

                // Subtest 8C: Non-positive speed
                badSpeed = CreateTestSkill("p09_sub8c", "Bad Speed", 2.0f, 20f, 3.0f, isProjectile: true, speed: 0f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, badSpeed);
                var res8C = SkillExecutor.Execute(new SkillExecutionRequest(hero, badSpeed, SkillSlotType.Skill, target));
                bool sub8COk = !res8C.Success && res8C.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                               Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(badSpeed.SkillId, out _);

                // Subtest 8D: Non-positive lifetime
                badLife = CreateTestSkill("p09_sub8d", "Bad Life", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 0f);
                RegisterAndSelectSkill(mmMgr, badLife);
                var res8D = SkillExecutor.Execute(new SkillExecutionRequest(hero, badLife, SkillSlotType.Skill, target));
                bool sub8DOk = !res8D.Success && res8D.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                               Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(badLife.SkillId, out _);

                // Subtest 8E: Mixed Policies: SingleTarget Damage + Self Heal
                var dmgEff = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dmgEff.Initialize(2.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                var healEff = ScriptableObject.CreateInstance<HealEffectDefinitionSO>();
                healEff.Initialize(50f, HealAmountMode.FlatValue, SkillTargetPolicy.Self);
                mixedProj = CreateMultiEffectTestSkill("p09_sub8e", "Mixed Proj", 20f, 3.0f, isProjectile: true, 10f, 5f, new List<SkillEffectDefinitionSO> { dmgEff, healEff });
                RegisterAndSelectSkill(mmMgr, mixedProj);
                var res8E = SkillExecutor.Execute(new SkillExecutionRequest(hero, mixedProj, SkillSlotType.Skill, target));
                bool sub8EOk = !res8E.Success && res8E.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                               Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(mixedProj.SkillId, out _);

                bool pass = sub8AOk && sub8BOk && sub8COk && sub8DOk && sub8EOk;
                Debug.Log($"[T08] Invalid Delivery Rejection: ChannelProj={sub8AOk}, AreaProj={sub8BOk}, BadSpeed={sub8COk}, BadLife={sub8DOk}, MixedPolicy={sub8EOk} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (chanProj != null) UnityEngine.Object.DestroyImmediate(chanProj);
                if (areaProj != null) UnityEngine.Object.DestroyImmediate(areaProj);
                if (badSpeed != null) UnityEngine.Object.DestroyImmediate(badSpeed);
                if (badLife != null) UnityEngine.Object.DestroyImmediate(badLife);
                if (mixedProj != null) UnityEngine.Object.DestroyImmediate(mixedProj);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T09: Cast-time projectile charges Rage at start, releases at cast completion, starts cooldown once.
        /// </summary>
        private static bool T09_CastTimeProjectileChargesRageAtStart_ReleasesAtCastEnd_StartsCooldownOnce()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;
            ITimeProvider origTimeProvider = CooldownManager.TimeProvider;
            TestTimeProvider testTime = new TestTimeProvider(100f);
            CooldownManager.TimeProvider = testTime;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t09_cast", "Ngưng Khí Phi Kiếm", 2.5f, 30f, 5.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 1.0f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                var startRes = SkillExecutor.Execute(req);

                // Phase 1: Cast Start Checks (Rage deducted at cast start, NO projectile and NO cooldown yet)
                bool startSuccess = startRes.Success;
                bool rageChargedAtStart = Mathf.Approximately(hero.Rage.CurrentRage, 70f);
                bool isCastingAtStart = hero.IsCasting;
                bool noProjectileYet = ProjectileController.ActiveProjectiles.Count == 0;
                bool noCooldownYet = !CooldownManager.IsOnCooldown(skill.SkillId, out _);

                // Phase 2: Advance Cast State (0.5s elapsed, cast duration = 1.0s)
                testTime.Advance(0.5f);
                hero.CastState.Tick(0.5f);
                bool stillCasting = hero.IsCasting;
                bool stillNoProjectile = ProjectileController.ActiveProjectiles.Count == 0;
                bool stillNoCooldown = !CooldownManager.IsOnCooldown(skill.SkillId, out _);

                // Phase 3: Complete Cast State (remaining 0.5s) -> Projectile releases, cooldown starts ONCE at release
                testTime.Advance(0.5f);
                hero.CastState.Tick(0.5f);
                bool castFinished = !hero.IsCasting;
                bool projectileReleased = ProjectileController.ActiveProjectiles.Count == 1;
                bool cdStartedAtRelease = CooldownManager.IsOnCooldown(skill.SkillId, out float cdInitial) && Mathf.Approximately(cdInitial, 5.0f);
                bool stillZeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);

                var proj = projectileReleased ? ProjectileController.ActiveProjectiles[0] : null;

                // Phase 4: Advance Projectile to Arrival (hero at -3, target at 5 -> 8 units at speed 10 takes 0.8s)
                testTime.Advance(0.8f);
                if (proj != null)
                {
                    proj.SimulateTick(0.8f);
                }
                bool damageAppliedAtArrival = target.Health.CurrentHealth < hpBefore;
                bool projGone = ProjectileController.ActiveProjectiles.Count == 0;
                bool rageNotChargedAgain = Mathf.Approximately(hero.Rage.CurrentRage, 70f);

                // Cooldown verification at arrival: clock advanced by 0.8s, cooldown remaining must be 5.0 - 0.8 = 4.2s, NOT reset to 5.0s
                bool isOnCooldown = CooldownManager.IsOnCooldown(skill.SkillId, out float cdValAfter);
                bool cdDecreasedByFlight = isOnCooldown && Mathf.Abs(cdValAfter - (cdInitial - 0.8f)) < 0.05f;
                bool cdNotResetAtArrival = cdDecreasedByFlight && (cdValAfter < cdInitial);
                bool cdStartedOnce = cdStartedAtRelease && cdNotResetAtArrival;

                bool pass = startSuccess && rageChargedAtStart && isCastingAtStart && noProjectileYet && noCooldownYet &&
                            stillCasting && stillNoProjectile && stillNoCooldown && castFinished && projectileReleased && cdStartedOnce &&
                            stillZeroDamage && damageAppliedAtArrival && projGone && rageNotChargedAgain;

                Debug.Log($"[T09] Cast-Time Projectile: StartOk={startSuccess}, RageAtStart={rageChargedAtStart}, ReleasedAtEnd={projectileReleased}, CdStartedOnce={cdStartedOnce}, DmgAtArrival={damageAppliedAtArrival}, RageKept={rageNotChargedAgain} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                CooldownManager.TimeProvider = origTimeProvider;
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T10: Combat pause freezes projectile travel; resume continues to arrival.
        /// </summary>
        private static bool T10_PauseFreezesTravel_ResumeCompletesArrival()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t10_pause", "Định Thân Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(10f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Pause combat
                var activeProp = typeof(BattleManager).GetProperty("IsBattleActive");
                var stateProp = typeof(BattleManager).GetProperty("CurrentBattleState");
                if (activeProp != null) activeProp.SetValue(bm, false);
                if (stateProp != null) stateProp.SetValue(bm, BattleState.LootPending);

                Vector3 posBeforePause = proj != null ? proj.transform.position : Vector3.zero;

                // Tick while paused
                if (proj != null)
                {
                    proj.SimulateTick(0.5f);
                }

                Vector3 posDuringPause = proj != null ? proj.transform.position : Vector3.zero;
                bool travelFrozen = (posBeforePause == posDuringPause) && Mathf.Approximately(target.Health.CurrentHealth, hpBefore);

                // Resume combat
                if (activeProp != null) activeProp.SetValue(bm, true);
                if (stateProp != null) stateProp.SetValue(bm, BattleState.InProgress);

                // Tick while active
                if (proj != null)
                {
                    proj.SimulateTick(1.2f);
                }

                bool arrivedAfterResume = target.Health.CurrentHealth < hpBefore;
                bool pass = travelFrozen && arrivedAfterResume;
                Debug.Log($"[T10] Combat Pause & Resume: TravelFrozen={travelFrozen}, ArrivedAfterResume={arrivedAfterResume} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T11: Disabling visual renderer has zero combat authority; travel and damage timing are identical.
        /// </summary>
        private static bool T11_DisablingVisualRendererHasZeroCombatAuthority()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t11_visual", "Vô Ảnh Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Disable visual placeholder
                if (proj != null && proj.VisualObject != null)
                {
                    proj.VisualObject.SetActive(false);
                }

                // Tick travel
                if (proj != null)
                {
                    proj.SimulateTick(0.6f);
                }

                bool damageAppliedWithoutVisual = target.Health.CurrentHealth < hpBefore;
                bool pass = damageAppliedWithoutVisual;
                Debug.Log($"[T11] Disabled Visual Has Zero Combat Authority: DamageApplied={damageAppliedWithoutVisual} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T12: Lifetime expiry cancels projectile cleanly without dealing damage to target.
        /// </summary>
        private static bool T12_LifetimeExpiry_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                // Speed = 2, Lifetime = 0.5s. In 0.5s it can only travel 1 unit. Target is 8 units away.
                skill = CreateTestSkill("p09_t12_expiry", "Tàn Lửa Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 2f, lifetime: 0.5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Tick 0.6s (exceeds lifetime 0.5s)
                if (proj != null)
                {
                    proj.SimulateTick(0.6f);
                }

                bool isCancelled = proj == null || proj.IsCancelled;
                bool expiredReason = ((object)proj != null && !string.IsNullOrEmpty(proj.CancelReason) && proj.CancelReason.Contains("lifetime expired")) || (proj == null);
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelled && expiredReason && zeroDamage && cleanedUp;
                Debug.Log($"[T12] Lifetime Expiry: Cancelled={isCancelled}, ExpiredReason={expiredReason}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T13: Target death or encounter invalidation during combat pause cancels projectile immediately without dealing damage.
        /// </summary>
        private static bool T13_CancellationDuringCombatPause()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t13_pause_cancel", "Tĩnh Chỉ Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null)
                {
                    proj.SimulateTick(0.1f);
                }

                // Pause combat
                var activeProp = typeof(BattleManager).GetProperty("IsBattleActive");
                if (activeProp != null) activeProp.SetValue(bm, false);

                // Branch 1: While paused: kill target
                EventBus.RaiseEntityDied(target);

                // Tick while paused
                if (proj != null)
                {
                    proj.SimulateTick(0.1f);
                }

                bool isCancelledDeath = proj == null || proj.IsCancelled;
                bool cleanedUpDeath = ProjectileController.ActiveProjectiles.Count == 0;

                // Branch 2: While paused: encounter/wave changes before projectile arrives
                // Spawn a new monster and launch another projectile
                Monster target2 = bm.SpawnMonster();
                target2.transform.position = new Vector3(5f, -0.3f, 0f);
                float hp2Before = target2.Health.CurrentHealth;

                // Unpause briefly to launch
                if (activeProp != null) activeProp.SetValue(bm, true);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                var req2 = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target2);
                SkillExecutor.Execute(req2);
                var proj2 = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Tick 0.1s in flight
                if (proj2 != null) proj2.SimulateTick(0.1f);

                // Pause combat
                if (activeProp != null) activeProp.SetValue(bm, false);

                // While paused: advance encounter index
                var encProp = typeof(BattleManager).GetProperty("EncounterIndex");
                if (encProp != null) encProp.SetValue(bm, bm.EncounterIndex + 1);

                // Tick while still paused
                if (proj2 != null) proj2.SimulateTick(0f);

                bool isCancelledWave = proj2 == null || proj2.IsCancelled;
                bool zeroDamageWave = Mathf.Approximately(target2.Health.CurrentHealth, hp2Before);
                bool cleanedUpWave = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelledDeath && cleanedUpDeath && isCancelledWave && zeroDamageWave && cleanedUpWave;
                Debug.Log($"[T13] Cancellation During Combat Pause: DeathCancel={isCancelledDeath}, WaveCancel={isCancelledWave}, ZeroDmg={zeroDamageWave}, CleanedUp={cleanedUpWave} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T14: Caster destroyed before arrival cancels projectile without dealing damage.
        /// </summary>
        private static bool T14_CasterDestroyedBeforeArrival_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                skill = CreateTestSkill("p09_t14_caster_destroy", "Tuyệt Diệt Kiếm", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null)
                {
                    proj.SimulateTick(0.1f);
                }

                // Destroy caster gameobject completely
                UnityEngine.Object.DestroyImmediate(heroGO);
                heroGO = null;

                // Tick travel
                if (proj != null)
                {
                    proj.SimulateTick(0.1f);
                }

                bool isCancelled = proj == null || proj.IsCancelled;
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelled && zeroDamage && cleanedUp;
                Debug.Log($"[T14] Caster Destroyed Before Arrival: Cancelled={isCancelled}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T15: Mixed policy projectile skill (SingleTarget Damage + Self Heal) rejected before consuming Rage or Cooldown.
        /// </summary>
        private static bool T15_MixedPolicySkill_RejectedBeforeRageAndCooldown()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                var dmgEff = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dmgEff.Initialize(2.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);

                var healEff = ScriptableObject.CreateInstance<HealEffectDefinitionSO>();
                healEff.Initialize(50f, HealAmountMode.FlatValue, SkillTargetPolicy.Self);

                skill = CreateMultiEffectTestSkill("p09_t15_mixed", "Âm Dương Tiễn", 25f, 4.0f, isProjectile: true, 10f, 5f, new List<SkillEffectDefinitionSO> { dmgEff, healEff });
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                float initialHeroRage = hero.Rage.CurrentRage;
                float initialMonsterHp = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                var res = SkillExecutor.Execute(req);

                bool rejected = !res.Success && res.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration;
                bool zeroRageConsumed = Mathf.Approximately(hero.Rage.CurrentRage, initialHeroRage);
                bool zeroCooldown = !CooldownManager.IsOnCooldown(skill.SkillId, out _);
                bool zeroProjectiles = ProjectileController.ActiveProjectiles.Count == 0;
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, initialMonsterHp);

                bool pass = rejected && zeroRageConsumed && zeroCooldown && zeroProjectiles && zeroDamage;
                Debug.Log($"[T15] Mixed Policy Rejection: Rejected={rejected}, ZeroRage={zeroRageConsumed}, ZeroCD={zeroCooldown}, ZeroProj={zeroProjectiles}, ZeroDmg={zeroDamage} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T16: Resetting BattleManager.Instance while old manager and target still exist cancels projectile without dealing damage.
        /// </summary>
        private static bool T16_ResetInstanceWhileManagerAndTargetExist_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t16_reset_bm", "Thiên Cơ Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null) proj.SimulateTick(0.1f);

                // Reset BattleManager.Instance (old manager and target GameObjects remain in hierarchy)
                BattleManager.ResetInstance();

                // Next tick must detect current Instance is null and cancel immediately
                if (proj != null) proj.SimulateTick(0.1f);

                bool isCancelled = proj == null || proj.IsCancelled;
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelled && zeroDamage && cleanedUp;
                Debug.Log($"[T16] Reset Instance In Flight: Cancelled={isCancelled}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T17: Replacing BattleManager.Instance with a new manager while old manager is still alive cancels projectile without hit.
        /// </summary>
        private static bool T17_ReplaceInstanceWhileOldManagerAlive_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            GameObject bm2GO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t17_replace_bm", "Hoán Đổi Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s bound to bm
                if (proj != null) proj.SimulateTick(0.1f);

                // Instantiate new BattleManager bm2 and set as Instance without destroying bm
                bm2GO = new GameObject("TestBM2");
                var bm2 = bm2GO.AddComponent<BattleManager>();
                bm2.SetAsInstance();

                // Next tick must detect current Instance != BoundBattleManager and cancel immediately
                if (proj != null) proj.SimulateTick(0.1f);

                bool isCancelled = proj == null || proj.IsCancelled;
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelled && zeroDamage && cleanedUp;
                Debug.Log($"[T17] Replace Instance In Flight: Cancelled={isCancelled}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (bm2GO != null) UnityEngine.Object.DestroyImmediate(bm2GO);
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T18: Target unregistered from active encounter while still alive cancels projectile without hit.
        /// </summary>
        private static bool T18_LivingTargetUnregisteredFromEncounter_CancelsWithoutHit()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t18_unreg_target", "Phá Giới Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null) proj.SimulateTick(0.1f);

                // Target is still alive, active in hierarchy, but removed from encounter activeMonsters
                bm.UnregisterMonster(target);

                bool targetStillAlive = target.IsAlive && target.gameObject.activeInHierarchy;

                // Next tick must detect target is no longer in active encounter monsters and cancel
                if (proj != null) proj.SimulateTick(0.1f);

                bool isCancelled = proj == null || proj.IsCancelled;
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = targetStillAlive && isCancelled && zeroDamage && cleanedUp;
                Debug.Log($"[T18] Living Target Unregistered: TargetAlive={targetStillAlive}, Cancelled={isCancelled}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T19: Encounter index changed during combat pause cancels projectile immediately before unpause.
        /// </summary>
        private static bool T19_EncounterIndexChangedDuringPause_CancelsBeforeUnpause()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t19_wave_pause", "Thời Không Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null) proj.SimulateTick(0.1f);

                // Pause combat
                var activeProp = typeof(BattleManager).GetProperty("IsBattleActive");
                if (activeProp != null) activeProp.SetValue(bm, false);

                // While paused: advance encounter index
                var encProp = typeof(BattleManager).GetProperty("EncounterIndex");
                if (encProp != null) encProp.SetValue(bm, bm.EncounterIndex + 1);

                // Tick while paused (deltaTime=0f) - must cancel before unpause and before travel
                if (proj != null) proj.SimulateTick(0f);

                bool isCancelledBeforeUnpause = proj == null || proj.IsCancelled;
                bool zeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelledBeforeUnpause && zeroDamage && cleanedUp;
                Debug.Log($"[T19] Wave Change During Pause: CancelledBeforeUnpause={isCancelledBeforeUnpause}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T20: Disabling ProjectileController cleans up payload; re-enabling does NOT resurrect old payload.
        /// </summary>
        private static bool T20_DisableControllerAndReenable_DoesNotResurrectPayload()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;

            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t20_disable_reenable", "Phong Ma Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null) proj.SimulateTick(0.1f);

                // Disable component (simulates disable controller / root)
                if (proj != null) proj.enabled = false;

                bool isCancelledOnDisable = proj == null || proj.IsCancelled;
                bool payloadNulled = proj != null && proj.Effects == null && proj.Request == null;
                bool activeListEmpty = ProjectileController.ActiveProjectiles.Count == 0;

                // Re-enable component
                if (proj != null) proj.enabled = true;

                bool notReRegistered = ProjectileController.ActiveProjectiles.Count == 0;

                // Simulate tick after re-enable
                if (proj != null) proj.SimulateTick(1.0f);

                bool zeroDamageDealt = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);

                bool pass = isCancelledOnDisable && payloadNulled && activeListEmpty && notReRegistered && zeroDamageDealt;
                Debug.Log($"[T20] Disable & Re-enable Safe: Cancelled={isCancelledOnDisable}, Nulled={payloadNulled}, NotResurrected={notReRegistered}, ZeroDamage={zeroDamageDealt} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T21: Scene unload cleans up active projectile without delivering damage.
        /// Uses native Unity scene management to unload an active additive scene fixture.
        /// </summary>
        private static bool T21_SceneUnload_CancelsProjectilesWithoutDamage()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            SkillDefinitionSO skill = null;
            UnityEngine.SceneManagement.Scene initialScene = default;
            UnityEngine.SceneManagement.Scene tempScene = default;

            try
            {
                initialScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (!initialScene.IsValid() || string.IsNullOrEmpty(initialScene.path))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
                    initialScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                }
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(initialScene);

                // Setup encounter in base scene so monster and hero persist to verify target damage is zero
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);

                skill = CreateTestSkill("p09_t21_scene_unload", "Tịch Diệt Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(-3f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                // Create a temporary additive scene fixture and set as active so projectile is owned by tempScene
                tempScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                    UnityEditor.SceneManagement.NewSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(tempScene);

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                SkillExecutor.Execute(req);

                var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

                // Fly 0.1s
                if (proj != null) proj.SimulateTick(0.1f);

                // Close scene fixture via native Unity EditorSceneManager API
                // Unity dispatches SceneManager.sceneUnloaded naturally without reflection or synthetic calls
                bool sceneClosed = UnityEditor.SceneManagement.EditorSceneManager.CloseScene(tempScene, true);

                bool isCancelled = proj == null || proj.IsCancelled;
                bool zeroDamage = target != null && Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = sceneClosed && isCancelled && zeroDamage && cleanedUp;
                Debug.Log($"[T21] Real Scene Unload Cleanup: Closed={sceneClosed}, Cancelled={isCancelled}, ZeroDamage={zeroDamage}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (tempScene.IsValid() && tempScene.isLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(tempScene, true);
                }
                if (initialScene.IsValid() && initialScene.isLoaded)
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(initialScene);
                }
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        #endregion
    }

    /// <summary>
    /// Play Mode scenario harness testing projectile delivery via Hero.ExecuteSelectedSkill with natural frames.
    /// </summary>
    public class P09PlayModeHarness : MonoBehaviour
    {
        public bool IsRunning { get; private set; }
        public bool Passed { get; private set; }
        public string ErrorMessage { get; private set; }

        public IEnumerator RunScenarioCoroutine()
        {
            IsRunning = true;
            Passed = false;
            ErrorMessage = string.Empty;
            Debug.Log("[P09 PLAY MODE] Starting Enhanced Natural Frames Scenario via Hero.ExecuteSelectedSkill...");

            GameObject heroGO = null;
            GameObject monsterGO = null;
            GameObject bmGO = null;
            GameObject servicesGO = null;
            SkillDefinitionSO instantSkill = null;
            SkillDefinitionSO castSkill = null;
            MindMethodDatabaseSO tempDb = null;
            MindMethodDefinitionSO tempMmDef = null;

            int damageEventCount = 0;
            Entity lastDamagedEntity = null;
            Entity lastDamageSource = null;
            DamageType lastDamageType = DamageType.BasicAttack;
            float lastDamageAmount = 0f;
            bool targetDiedNaturally = false;

            Action<Entity, DamageResult> onDamagedHandler = (ent, dmgRes) =>
            {
                damageEventCount++;
                lastDamagedEntity = ent;
                lastDamageSource = dmgRes.Attacker;
                lastDamageType = dmgRes.DamageType;
                lastDamageAmount = dmgRes.FinalDamage;
            };

            Action<Entity> onDiedHandler = (ent) =>
            {
                if (ent != null && ent.name == "Monster_PlayMode_Target")
                {
                    targetDiedNaturally = true;
                }
            };

            try
            {
                ProjectileController.ClearAllProjectiles();
                MindMethodManager.ResetInstance();
                BattleManager.ResetInstance();
                EquipmentManager.ResetInstance();
                Inventory.Inventory.ResetInstance();
                ResourceManager.ResetInstance();
                ProgressionManager.ResetInstance();
                EventBus.ClearAllListeners();

                EventBus.OnEntityDamaged += onDamagedHandler;
                EventBus.OnEntityDied += onDiedHandler;

                servicesGO = new GameObject("PlayModeServices");
                servicesGO.AddComponent<EquipmentManager>();
                servicesGO.AddComponent<Inventory.Inventory>();
                servicesGO.AddComponent<ResourceManager>();
                servicesGO.AddComponent<ProgressionManager>();

                var mmMgr = servicesGO.AddComponent<MindMethodManager>();

                tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
                tempMmDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
                tempMmDef.InitializeMindMethod("mm_taiji", "Taiji", "Test Taiji", 10, true, new MindMethodPassiveData());
                tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { tempMmDef });
                mmMgr.SetDatabase(tempDb);
                mmMgr.SetActiveMindMethod("mm_taiji");

                heroGO = new GameObject("PlayModeHero");
                var hero = heroGO.AddComponent<Hero>();
                hero.InitializeHero();
                heroGO.transform.position = new Vector3(-3f, -0.3f, 0f);

                bmGO = new GameObject("PlayModeBM");
                var bm = bmGO.AddComponent<BattleManager>();
                bm.RegisterHero(hero);
                bm.StartBattle();

                // Create a standalone target monster with exactly 300 HP
                monsterGO = new GameObject("Monster_PlayMode_Target");
                monsterGO.transform.position = new Vector3(5f, -0.3f, 0f); // 8 units away
                var monsterSr = monsterGO.AddComponent<SpriteRenderer>();
                monsterSr.color = Color.white;
                var monster = monsterGO.AddComponent<Monster>();
                var mCfg = ScriptableObject.CreateInstance<MonsterConfigSO>();
                mCfg.InitializeMonsterConfig("PlayMode Target", 300f, 10f, 0f, 2f, 2f, 2f, 50);
                var cCfg = ScriptableObject.CreateInstance<CombatConfigSO>();
                monster.InitializeMonster(mCfg, cCfg);
                bm.RegisterMonster(monster);

                // Skill 1: Instant cast projectile (deals ~180 damage, leaving ~120 HP)
                instantSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                var dmgEffect1 = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dmgEffect1.Initialize(1.8f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                instantSkill.InitializeSkill(
                    id: "p09_pm_instant",
                    mmId: "mm_taiji",
                    slot: SkillSlotType.Skill,
                    name: "PlayMode Instant Proj",
                    desc: "Instant Projectile Natural Frames",
                    conditions: null,
                    dmgMultiplier: 1.8f,
                    costRage: 20f,
                    cd: 3.0f,
                    passive: false,
                    skillEffects: new List<SkillEffectDefinitionSO> { dmgEffect1 },
                    shatterFreeze: false,
                    skillCastTime: 0f,
                    channel: false,
                    chDuration: 0f,
                    chTickInterval: 0f,
                    skillPriority: 50,
                    projectile: true,
                    projSpeed: 10f, // 10 units/s -> 8 units takes 0.8s
                    projLifetime: 5f
                );

                // Skill 2: Cast-time projectile (0.3s cast time, deals ~200 damage to finish monster)
                castSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                var dmgEffect2 = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dmgEffect2.Initialize(2.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                castSkill.InitializeSkill(
                    id: "p09_pm_cast",
                    mmId: "mm_taiji",
                    slot: SkillSlotType.Skill,
                    name: "PlayMode Cast Proj",
                    desc: "Cast Time Projectile Natural Frames",
                    conditions: null,
                    dmgMultiplier: 2.0f,
                    costRage: 20f,
                    cd: 3.0f,
                    passive: false,
                    skillEffects: new List<SkillEffectDefinitionSO> { dmgEffect2 },
                    shatterFreeze: false,
                    skillCastTime: 0.3f,
                    channel: false,
                    chDuration: 0f,
                    chTickInterval: 0f,
                    skillPriority: 50,
                    projectile: true,
                    projSpeed: 10f,
                    projLifetime: 5f
                );

                var activeDef = mmMgr.ActiveMindMethodDefinition;
                var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = skillsField != null ? skillsField.GetValue(activeDef) as List<SkillDefinitionSO> : null;
                if (list != null)
                {
                    list.Add(instantSkill);
                    list.Add(castSkill);
                }

                var activeState = mmMgr.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[instantSkill.SkillId] = new SkillRuntimeState(instantSkill.SkillId, true, 1);
                    activeState.SkillStates[castSkill.SkillId] = new SkillRuntimeState(castSkill.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = instantSkill.SkillId;
                }

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                hero.SetCurrentTarget(monster);

                float initialMonsterHp = monster.Health.CurrentHealth;
                float initialHeroRage = hero.Rage.CurrentRage;

                // =========================================================================
                // SEGMENT 1: Instant Cast Projectile
                // =========================================================================
                Debug.Log($"[P09 PLAY MODE] --- Segment 1: Instant Projectile Launch (Frame={Time.frameCount}, Time={Time.time:F3}) ---");
                var result1 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);

                if (!result1.Success)
                {
                    ErrorMessage = $"Segment 1 ExecuteSelectedSkill failed: {result1.FailureReason} - {result1.ReasonDescription}";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Verify release frame
                int releaseFrame = Time.frameCount;
                float releaseTime = Time.time;
                bool rageDeducted1 = Mathf.Approximately(hero.Rage.CurrentRage, initialHeroRage - 20f);
                bool cooldownTriggered1 = CooldownManager.IsOnCooldown(instantSkill.SkillId, out _);
                bool projectileSpawned1 = ProjectileController.ActiveProjectiles.Count == 1;
                bool zeroEarlyDamage1 = Mathf.Approximately(monster.Health.CurrentHealth, initialMonsterHp);

                Debug.Log($"[P09 PLAY MODE] Release Frame {releaseFrame} (t={releaseTime:F3}): RageDeducted={rageDeducted1}, CDTriggered={cooldownTriggered1}, ProjCount={ProjectileController.ActiveProjectiles.Count}, HP={monster.Health.CurrentHealth}/{initialMonsterHp}");

                if (!rageDeducted1 || !cooldownTriggered1 || !projectileSpawned1 || !zeroEarlyDamage1)
                {
                    ErrorMessage = "Segment 1 release frame assertions failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // In-flight monitoring across natural frames
                float waitTimeout = 2.5f;
                float elapsed = 0f;
                bool zeroMidwayDamageConfirmed = true;
                bool impactConfirmed = false;

                while (elapsed < waitTimeout)
                {
                    yield return null; // NATURAL FRAME
                    elapsed += Time.deltaTime;

                    if (ProjectileController.ActiveProjectiles.Count > 0)
                    {
                        if (monster.Health.CurrentHealth < initialMonsterHp)
                        {
                            zeroMidwayDamageConfirmed = false;
                            ErrorMessage = "Early damage detected mid-flight!";
                            Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                            yield break;
                        }
                    }
                    else
                    {
                        impactConfirmed = true;
                        break;
                    }
                }

                if (!impactConfirmed)
                {
                    ErrorMessage = "Segment 1 projectile did not impact within timeout!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                int arrivalFrame = Time.frameCount;
                float arrivalTime = Time.time;
                bool damageApplied1 = monster.Health.CurrentHealth < initialMonsterHp;
                bool eventCorrelated1 = damageEventCount == 1 && lastDamagedEntity == monster && lastDamageSource == hero && lastDamageType == DamageType.Skill;
                bool projCleanedUp1 = ProjectileController.ActiveProjectiles.Count == 0;

                Debug.Log($"[P09 PLAY MODE] Arrival Frame {arrivalFrame} (t={arrivalTime:F3}): DmgApplied={damageApplied1}, EventCorrelated={eventCorrelated1}, RemainingHP={monster.Health.CurrentHealth:F1}/{initialMonsterHp}, CleanedUp={projCleanedUp1}");

                if (!damageApplied1 || !eventCorrelated1 || !projCleanedUp1 || !zeroMidwayDamageConfirmed)
                {
                    ErrorMessage = "Segment 1 post-impact verification failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // =========================================================================
                // SEGMENT 2: Cast-Time Projectile Finishing Target
                // =========================================================================
                Debug.Log($"[P09 PLAY MODE] --- Segment 2: Cast-Time Projectile (Frame={Time.frameCount}, Time={Time.time:F3}) ---");
                activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = castSkill.SkillId;
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                float hpBeforeSegment2 = monster.Health.CurrentHealth;
                float rageBeforeSegment2 = hero.Rage.CurrentRage;

                var result2 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                if (!result2.Success)
                {
                    ErrorMessage = $"Segment 2 ExecuteSelectedSkill failed: {result2.FailureReason} - {result2.ReasonDescription}";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Cast start assertions
                int castStartFrame = Time.frameCount;
                float castStartTime = Time.time;
                bool isCastingAtStart = hero.IsCasting;
                bool rageDeductedAtCastStart = Mathf.Approximately(hero.Rage.CurrentRage, rageBeforeSegment2 - 20f);
                bool noProjAtCastStart = ProjectileController.ActiveProjectiles.Count == 0;

                Debug.Log($"[P09 PLAY MODE] Cast Start Frame {castStartFrame} (t={castStartTime:F3}): IsCasting={isCastingAtStart}, RageDeducted={rageDeductedAtCastStart}, ProjCount={ProjectileController.ActiveProjectiles.Count}");

                if (!isCastingAtStart || !rageDeductedAtCastStart || !noProjAtCastStart)
                {
                    ErrorMessage = "Segment 2 cast start assertions failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Cycle natural frames while casting
                float castWaitElapsed = 0f;
                bool castCompletedNaturally = false;
                while (castWaitElapsed < 1.0f)
                {
                    yield return null; // NATURAL FRAME
                    castWaitElapsed += Time.deltaTime;

                    if (!hero.IsCasting)
                    {
                        castCompletedNaturally = true;
                        break;
                    }
                }

                if (!castCompletedNaturally)
                {
                    ErrorMessage = "Segment 2 cast did not finish within timeout!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                int castEndFrame = Time.frameCount;
                float castEndTime = Time.time;
                bool projReleasedAtCastEnd = ProjectileController.ActiveProjectiles.Count == 1;
                bool cdTriggeredAtCastEnd = CooldownManager.IsOnCooldown(castSkill.SkillId, out _);

                Debug.Log($"[P09 PLAY MODE] Cast End / Release Frame {castEndFrame} (t={castEndTime:F3}): ProjReleased={projReleasedAtCastEnd}, CDTriggered={cdTriggeredAtCastEnd}");

                if (!projReleasedAtCastEnd || !cdTriggeredAtCastEnd)
                {
                    ErrorMessage = "Segment 2 release assertions failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Cycle natural frames for flight and natural death
                float flightWaitElapsed = 0f;
                bool segment2Impacted = false;
                while (flightWaitElapsed < 2.5f)
                {
                    yield return null; // NATURAL FRAME
                    flightWaitElapsed += Time.deltaTime;

                    if (ProjectileController.ActiveProjectiles.Count == 0)
                    {
                        segment2Impacted = true;
                        break;
                    }
                }

                if (!segment2Impacted)
                {
                    ErrorMessage = "Segment 2 projectile did not impact within timeout!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                int finalFrame = Time.frameCount;
                float finalTime = Time.time;
                bool monsterDeadNaturally = !monster.IsAlive || (monster.Health != null && monster.Health.CurrentHealth <= 0f);
                bool deathEventFired = targetDiedNaturally;
                bool allCleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                Debug.Log($"[P09 PLAY MODE] Final Frame {finalFrame} (t={finalTime:F3}): MonsterDead={monsterDeadNaturally}, DeathEventFired={deathEventFired}, FinalHP={monster.Health?.CurrentHealth}, CleanedUp={allCleanedUp}");

                if (monsterDeadNaturally && deathEventFired && allCleanedUp)
                {
                    Passed = true;
                    Debug.Log("================================================================================");
                    Debug.Log("   [P09 PLAY MODE SCENARIO RESULT]: PASSED (Natural Frames Verified)");
                    Debug.Log("================================================================================");
                }
                else
                {
                    ErrorMessage = "Segment 2 natural target death verification failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                }
            }
            finally
            {
                IsRunning = false;
                EventBus.OnEntityDamaged -= onDamagedHandler;
                EventBus.OnEntityDied -= onDiedHandler;

                ProjectileController.ClearAllProjectiles();
                if (instantSkill != null) UnityEngine.Object.DestroyImmediate(instantSkill);
                if (castSkill != null) UnityEngine.Object.DestroyImmediate(castSkill);
                if (tempMmDef != null) UnityEngine.Object.DestroyImmediate(tempMmDef);
                if (tempDb != null) UnityEngine.Object.DestroyImmediate(tempDb);
                if (heroGO != null) UnityEngine.Object.Destroy(heroGO);
                if (monsterGO != null) UnityEngine.Object.Destroy(monsterGO);
                if (bmGO != null) UnityEngine.Object.Destroy(bmGO);
                if (servicesGO != null) UnityEngine.Object.Destroy(servicesGO);
                MindMethodManager.ResetInstance();
                BattleManager.ResetInstance();
                EventBus.ClearAllListeners();

                if (Application.isBatchMode)
                {
                    EditorApplication.isPlaying = false;
                    EditorApplication.update += () =>
                    {
                        EditorApplication.Exit(Passed ? 0 : 1);
                    };
                    EditorApplication.Exit(Passed ? 0 : 1);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
#endif
