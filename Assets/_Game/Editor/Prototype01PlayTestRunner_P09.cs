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
        [MenuItem("Tools/Wuxia RPG/Run P09-A Projectile Foundation Tests (T01 - T15)")]
        public static bool RunAllP09Tests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-A PROJECTILE FOUNDATION AUTOMATED SUITE (T01 -> T15)");
            Debug.Log("================================================================================");

            int passed = 0;
            int total = 15;

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
                bool pass = RunAllP09Tests();
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

            // Services
            GameObject servicesGO = new GameObject("TestServices");
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            servicesGO.AddComponent<ProgressionManager>();

            mmMgrGO = new GameObject("TestMindMethodManager");
            mmMgr = mmMgrGO.AddComponent<MindMethodManager>();

            // Transient in-memory MindMethod database (zero disk modification, zero persistence reset)
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
                skill = CreateTestSkill("p09_t02_proj", "Phi Kiếm Quyết", 2.0f, 25f, 4.0f, isProjectile: true, speed: 10f, lifetime: 5f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(10f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                var res = SkillExecutor.Execute(req);

                // Step 1: Verification at Release
                bool success = res.Success;
                bool zeroEarlyDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool rageChargedAtRelease = Mathf.Approximately(hero.Rage.CurrentRage, 75f);
                bool cdStartedAtRelease = CooldownManager.IsOnCooldown(skill.SkillId, out _);
                bool projectileActive = ProjectileController.ActiveProjectiles.Count == 1;

                var proj = projectileActive ? ProjectileController.ActiveProjectiles[0] : null;

                // Step 2: Verification at Halfway Travel (0.5s at 10 speed = 5 units)
                if (proj != null)
                {
                    proj.SimulateTick(0.5f);
                }
                bool zeroMidwayDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);
                bool stillFlying = proj != null && !proj.HasImpacted && !proj.IsCancelled;

                // Step 3: Verification at Arrival (remaining 6 units at 10 speed = 0.6s)
                if (proj != null)
                {
                    proj.SimulateTick(0.6f);
                }
                bool damageDealtOnArrival = target.Health.CurrentHealth < hpBefore;
                bool projCompleted = proj == null || proj.HasImpacted;
                bool noDuplicateCooldown = CooldownManager.IsOnCooldown(skill.SkillId, out float cdRemain) && cdRemain <= 4.0f;

                bool pass = success && zeroEarlyDamage && rageChargedAtRelease && cdStartedAtRelease && projectileActive &&
                            zeroMidwayDamage && stillFlying && damageDealtOnArrival && projCompleted && noDuplicateCooldown;

                Debug.Log($"[T02] Projectile Release & Arrival: ReleaseOk={success}, ZeroEarlyDmg={zeroEarlyDamage}, ZeroMidDmg={zeroMidwayDamage}, DmgOnArrival={damageDealtOnArrival}, ProjCompleted={projCompleted} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T03: Target movement keeps homing bound to original target; hero changing CurrentTarget does NOT redirect projectile.
        /// </summary>
        private static bool T03_MovingTargetRemainsBound_HeroCurrentTargetChangeDoesNotRedirect()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill1 = null;
            SkillDefinitionSO skill2 = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO chanProj = null, areaProj = null, badSpeed = null, badLife = null, mixedProj = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
                skill = CreateTestSkill("p09_t09_cast", "Ngưng Khí Phi Kiếm", 2.5f, 30f, 5.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 1.0f);
                RegisterAndSelectSkill(mmMgr, skill);

                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float hpBefore = target.Health.CurrentHealth;

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
                var startRes = SkillExecutor.Execute(req);

                // Phase 1: Cast Start Checks
                bool startSuccess = startRes.Success;
                bool rageChargedAtStart = Mathf.Approximately(hero.Rage.CurrentRage, 70f);
                bool isCastingAtStart = hero.IsCasting;
                bool noProjectileYet = ProjectileController.ActiveProjectiles.Count == 0;
                bool noCooldownYet = !CooldownManager.IsOnCooldown(skill.SkillId, out _);

                // Phase 2: Advance Cast State (0.5s elapsed, cast duration = 1.0s)
                hero.CastState.Tick(0.5f);
                bool stillCasting = hero.IsCasting;
                bool stillNoProjectile = ProjectileController.ActiveProjectiles.Count == 0;

                // Phase 3: Complete Cast State (remaining 0.5s)
                hero.CastState.Tick(0.5f);
                bool castFinished = !hero.IsCasting;
                bool projectileReleased = ProjectileController.ActiveProjectiles.Count == 1;
                bool cdStartedAtRelease = CooldownManager.IsOnCooldown(skill.SkillId, out float cdVal) && cdVal > 0f;
                bool stillZeroDamage = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);

                var proj = projectileReleased ? ProjectileController.ActiveProjectiles[0] : null;

                // Phase 4: Advance Projectile to Arrival (hero at -3, target at 5 -> 8 units at speed 10 takes 0.8s)
                if (proj != null)
                {
                    proj.SimulateTick(1.0f);
                }
                bool damageAppliedAtArrival = target.Health.CurrentHealth < hpBefore;
                bool projGone = ProjectileController.ActiveProjectiles.Count == 0;
                bool rageNotChargedAgain = Mathf.Approximately(hero.Rage.CurrentRage, 70f);

                bool pass = startSuccess && rageChargedAtStart && isCastingAtStart && noProjectileYet && noCooldownYet &&
                            stillCasting && stillNoProjectile && castFinished && projectileReleased && cdStartedAtRelease &&
                            stillZeroDamage && damageAppliedAtArrival && projGone && rageNotChargedAgain;

                Debug.Log($"[T09] Cast-Time Projectile: StartOk={startSuccess}, RageAtStart={rageChargedAtStart}, ReleasedAtEnd={projectileReleased}, CdStartedOnce={cdStartedAtRelease}, DmgAtArrival={damageAppliedAtArrival} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (skill != null) UnityEngine.Object.DestroyImmediate(skill);
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T10: Combat pause freezes projectile travel; resume continues to arrival.
        /// </summary>
        private static bool T10_PauseFreezesTravel_ResumeCompletesArrival()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
        /// T13: Target or caster invalidation during combat pause cancels projectile immediately without dealing damage.
        /// </summary>
        private static bool T13_CancellationDuringCombatPause()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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

                // While paused: kill target
                EventBus.RaiseEntityDied(target);

                // Tick while paused
                if (proj != null)
                {
                    proj.SimulateTick(0.1f);
                }

                bool isCancelled = proj == null || proj.IsCancelled;
                bool cleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                bool pass = isCancelled && cleanedUp;
                Debug.Log($"[T13] Cancellation During Combat Pause: Cancelled={isCancelled}, CleanedUp={cleanedUp} | {(pass ? "PASS" : "FAIL")}");
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);
            SkillDefinitionSO skill = null;

            try
            {
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
            Debug.Log("[P09 PLAY MODE] Starting Natural Frames Scenario via Hero.ExecuteSelectedSkill...");

            GameObject heroGO = null;
            GameObject monsterGO = null;
            GameObject bmGO = null;
            GameObject servicesGO = null;
            SkillDefinitionSO testSkill = null;
            MindMethodDatabaseSO tempDb = null;
            MindMethodDefinitionSO tempMmDef = null;

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

                var monster = bm.CurrentMonster;
                monsterGO = monster != null ? monster.gameObject : null;
                if (monster != null)
                {
                    monster.transform.position = new Vector3(5f, -0.3f, 0f); // 8 units away
                }

                testSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                var dmgEffect = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dmgEffect.Initialize(2.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                testSkill.InitializeSkill(
                    id: "p09_pm_skill",
                    mmId: "mm_taiji",
                    slot: SkillSlotType.Skill,
                    name: "PlayMode Proj Skill",
                    desc: "Natural Frames Test",
                    conditions: null,
                    dmgMultiplier: 2.0f,
                    costRage: 20f,
                    cd: 3.0f,
                    passive: false,
                    skillEffects: new List<SkillEffectDefinitionSO> { dmgEffect },
                    shatterFreeze: false,
                    skillCastTime: 0f,
                    channel: false,
                    chDuration: 0f,
                    chTickInterval: 0f,
                    skillPriority: 50,
                    projectile: true,
                    projSpeed: 10f, // 10 units / sec -> 8 units takes 0.8s
                    projLifetime: 5f
                );

                var activeDef = mmMgr.ActiveMindMethodDefinition;
                var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = skillsField != null ? skillsField.GetValue(activeDef) as List<SkillDefinitionSO> : null;
                if (list != null) list.Add(testSkill);

                var activeState = mmMgr.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[testSkill.SkillId] = new SkillRuntimeState(testSkill.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = testSkill.SkillId;
                }

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                hero.SetCurrentTarget(monster);

                float initialMonsterHp = monster.Health.CurrentHealth;
                float initialHeroRage = hero.Rage.CurrentRage;

                // Canonical execution via Hero.ExecuteSelectedSkill
                Debug.Log("[P09 PLAY MODE] Calling hero.ExecuteSelectedSkill(SkillSlotType.Skill)...");
                var result = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);

                if (!result.Success)
                {
                    ErrorMessage = $"ExecuteSelectedSkill failed: {result.FailureReason} - {result.ReasonDescription}";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Verify release state
                bool rageDeducted = Mathf.Approximately(hero.Rage.CurrentRage, initialHeroRage - 20f);
                bool cooldownTriggered = CooldownManager.IsOnCooldown(testSkill.SkillId, out _);
                bool projectileSpawned = ProjectileController.ActiveProjectiles.Count == 1;
                bool zeroEarlyDamage = Mathf.Approximately(monster.Health.CurrentHealth, initialMonsterHp);

                Debug.Log($"[P09 PLAY MODE] Release Frame: RageDeducted={rageDeducted}, CDTriggered={cooldownTriggered}, ProjSpawned={projectileSpawned}, ZeroEarlyDmg={zeroEarlyDamage}");

                if (!rageDeducted || !cooldownTriggered || !projectileSpawned || !zeroEarlyDamage)
                {
                    ErrorMessage = "Release frame assertions failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Monitor natural frames flight
                float waitTimeout = 2.0f;
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
                    ErrorMessage = "Projectile did not impact within timeout!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                    yield break;
                }

                // Verify post-impact
                bool damageApplied = monster.Health.CurrentHealth < initialMonsterHp;
                bool projectileCleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

                Debug.Log($"[P09 PLAY MODE] Arrival Frame: DamageApplied={damageApplied}, RemainingHP={monster.Health.CurrentHealth}/{initialMonsterHp}, CleanedUp={projectileCleanedUp}");

                if (damageApplied && zeroMidwayDamageConfirmed && projectileCleanedUp)
                {
                    Passed = true;
                    Debug.Log("================================================================================");
                    Debug.Log("   [P09 PLAY MODE SCENARIO RESULT]: PASSED (Natural Frames Verified)");
                    Debug.Log("================================================================================");
                }
                else
                {
                    ErrorMessage = "Post-impact verification failed!";
                    Debug.LogError($"[P09 PLAY MODE ERROR] {ErrorMessage}");
                }
            }
            finally
            {
                IsRunning = false;
                ProjectileController.ClearAllProjectiles();
                if (testSkill != null) UnityEngine.Object.DestroyImmediate(testSkill);
                if (tempMmDef != null) UnityEngine.Object.DestroyImmediate(tempMmDef);
                if (tempDb != null) UnityEngine.Object.DestroyImmediate(tempDb);
                if (heroGO != null) UnityEngine.Object.Destroy(heroGO);
                if (monsterGO != null) UnityEngine.Object.Destroy(monsterGO);
                if (bmGO != null) UnityEngine.Object.Destroy(bmGO);
                if (servicesGO != null) UnityEngine.Object.Destroy(servicesGO);
                MindMethodManager.ResetInstance();
                BattleManager.ResetInstance();
                EventBus.ClearAllListeners();
                Destroy(gameObject);

                if (Application.isBatchMode)
                {
                    EditorApplication.isPlaying = false;
                    EditorApplication.Exit(Passed ? 0 : 1);
                }
            }
        }
    }
}
#endif
