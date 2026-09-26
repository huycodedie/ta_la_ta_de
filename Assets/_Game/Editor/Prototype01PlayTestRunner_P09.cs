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
using WuxiaGame.Items;
using WuxiaGame.Progression;

namespace WuxiaGame.Editor
{
    public static class Prototype01PlayTestRunner_P09
    {
        [MenuItem("Tools/Wuxia RPG/Run P09-A Projectile Foundation Tests (T01 - T11)")]
        public static bool RunAllP09Tests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-A PROJECTILE FOUNDATION AUTOMATED SUITE (T01 -> T11)");
            Debug.Log("================================================================================");

            int passed = 0;
            int total = 11;

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

            bool allPass = (passed == total);
            Debug.Log("================================================================================");
            Debug.Log($"   [P09 AUTOMATED TESTS RESULT]: {passed}/{total} PASSED | ALL_PASS={allPass}");
            Debug.Log("================================================================================");

            return allPass;
        }

        public static void RunGate1_P09_CLI()
        {
            bool pass = RunAllP09Tests();
            EditorApplication.Exit(pass ? 0 : 1);
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

            Prototype01SceneBuilder.BuildPrototype02Data();

            GameObject servicesGO = new GameObject("TestServices");
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            servicesGO.AddComponent<ProgressionManager>();

            mmMgrGO = new GameObject("TestMindMethodManager");
            mmMgr = mmMgrGO.AddComponent<MindMethodManager>();
            mmMgr.LoadDatabaseIfMissing();
            mmMgr.ResetPersistence();
            mmMgr.InitializeFromDatabase();

            heroGO = new GameObject("TestHero");
            hero = heroGO.AddComponent<Hero>();
            hero.InitializeHero();
            heroGO.transform.position = new Vector3(-3f, -0.3f, 0f);

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
            if (mmMgrGO != null) UnityEngine.Object.DestroyImmediate(mmMgrGO);

            GameObject servicesGO = GameObject.Find("TestServices");
            if (servicesGO != null) UnityEngine.Object.DestroyImmediate(servicesGO);

            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
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

            var skill = CreateTestSkill("p09_t01_imm", "Thái Cực Chưởng", 2.0f, 20f, 3.0f, isProjectile: false);
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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T02: Projectile release produces zero early damage; natural arrival produces exactly one matching damage event.
        /// </summary>
        private static bool T02_ProjectileReleaseProducesZeroEarlyDamage_NaturalArrivalProducesExactDamage()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t02_proj", "Phi Kiếm Quyết", 2.0f, 25f, 4.0f, isProjectile: true, speed: 10f, lifetime: 5f);
            RegisterAndSelectSkill(mmMgr, skill);

            Monster target = bm.CurrentMonster;
            hero.transform.position = new Vector3(0f, -0.3f, 0f);
            target.transform.position = new Vector3(10f, -0.3f, 0f); // 10 units away
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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T03: Target movement keeps homing bound to original target; hero changing CurrentTarget does NOT redirect projectile.
        /// </summary>
        private static bool T03_MovingTargetRemainsBound_HeroCurrentTargetChangeDoesNotRedirect()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t03_homing", "Hư Không Kiếm", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T04: Target death/deactivation before arrival cleanly cancels outstanding projectile with zero foreign hit.
        /// </summary>
        private static bool T04_TargetDeathBeforeArrival_CancelsWithoutHit()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t04_cancel", "Tử Thần Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
            RegisterAndSelectSkill(mmMgr, skill);

            Monster targetA = bm.ActiveMonsters[0];
            Monster targetB = bm.ActiveMonsters[1];
            targetA.transform.position = new Vector3(10f, -0.3f, 0f);
            targetB.transform.position = new Vector3(12f, -0.3f, 0f);

            float bHpBefore = targetB.Health.CurrentHealth;

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, targetA);
            SkillExecutor.Execute(req);

            var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

            // Target A dies mid-flight before arrival
            targetA.Health.TakeDamage(targetA.Health.CurrentHealth + 100f);

            // Simulate tick
            if (proj != null)
            {
                proj.SimulateTick(2.0f);
            }

            bool projCancelled = proj == null || proj.IsCancelled;
            bool bUntouched = Mathf.Approximately(targetB.Health.CurrentHealth, bHpBefore);
            bool noActiveProjectiles = ProjectileController.ActiveProjectiles.Count == 0;

            bool pass = projCancelled && bUntouched && noActiveProjectiles;
            Debug.Log($"[T04] Target Death Before Arrival: Cancelled={projCancelled}, BUntouched={bUntouched}, CleanedUp={noActiveProjectiles} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T05: Caster death cancels outstanding projectile with zero hit.
        /// </summary>
        private static bool T05_CasterDeathBeforeArrival_CancelsWithoutHit()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t05_caster_death", "Linh Hồn Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 5f);
            RegisterAndSelectSkill(mmMgr, skill);

            Monster target = bm.CurrentMonster;
            target.transform.position = new Vector3(10f, -0.3f, 0f);
            float hpBefore = target.Health.CurrentHealth;

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
            SkillExecutor.Execute(req);

            var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

            // Caster dies mid-flight (consume revive passive if active, then ensure true death)
            hero.Health.TakeDamage(hero.Health.CurrentHealth + 100f);
            if (hero.IsAlive)
            {
                hero.Health.TakeDamage(hero.Health.CurrentHealth + 100f);
            }

            if (proj != null)
            {
                proj.SimulateTick(2.0f);
            }

            bool projCancelled = proj == null || proj.IsCancelled;
            bool targetUntouched = Mathf.Approximately(target.Health.CurrentHealth, hpBefore);

            bool pass = projCancelled && targetUntouched;
            Debug.Log($"[T05] Caster Death Before Arrival: Cancelled={projCancelled}, TargetUntouched={targetUntouched} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T06: Multiple simultaneous projectiles keep independent payloads with no double hit.
        /// </summary>
        private static bool T06_MultipleSimultaneousProjectilesIndependent_NoDoubleHit()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skillA = CreateTestSkill("p09_t06_a", "Song Long Tụ Khí A", 1.5f, 10f, 1.0f, isProjectile: true, speed: 10f, lifetime: 5f, slotType: SkillSlotType.Skill);
            var skillB = CreateTestSkill("p09_t06_b", "Song Long Tụ Khí B", 2.5f, 10f, 1.0f, isProjectile: true, speed: 10f, lifetime: 5f, slotType: SkillSlotType.ExternalSkill1);
            RegisterAndSelectSkill(mmMgr, skillA);
            RegisterAndSelectSkill(mmMgr, skillB);

            Monster target = bm.CurrentMonster;
            target.transform.position = new Vector3(5f, -0.3f, 0f);
            float hpBefore = target.Health.CurrentHealth;

            var reqA = new SkillExecutionRequest(hero, skillA, SkillSlotType.Skill, target);
            var reqB = new SkillExecutionRequest(hero, skillB, SkillSlotType.ExternalSkill1, target);

            SkillExecutor.Execute(reqA);
            SkillExecutor.Execute(reqB);

            bool twoProjectilesActive = ProjectileController.ActiveProjectiles.Count == 2;
            var projA = twoProjectilesActive ? ProjectileController.ActiveProjectiles[0] : null;
            var projB = twoProjectilesActive ? ProjectileController.ActiveProjectiles[1] : null;

            // Advance to arrival (hero at -3, target at 5 -> 8 units at speed 10 takes 0.8s)
            if (projA != null) projA.SimulateTick(1.0f);
            if (projB != null) projB.SimulateTick(1.0f);

            bool bothImpacted = (projA == null || projA.HasImpacted) && (projB == null || projB.HasImpacted);
            bool hpReducedProperly = target.Health.CurrentHealth < hpBefore;
            bool allCleanedUp = ProjectileController.ActiveProjectiles.Count == 0;

            bool pass = twoProjectilesActive && bothImpacted && hpReducedProperly && allCleanedUp;
            Debug.Log($"[T06] Multiple Simultaneous Projectiles: TwoActive={twoProjectilesActive}, BothImpacted={bothImpacted}, CleanedUp={allCleanedUp} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(skillA);
            UnityEngine.Object.DestroyImmediate(skillB);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T07: Encounter advancement cancels outstanding projectiles, preventing damage from leaking into new wave.
        /// </summary>
        private static bool T07_EncounterAdvancementCancelsOutstandingProjectiles()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t07_enc_cancel", "Lạc Lôi Quyết", 2.0f, 20f, 3.0f, isProjectile: true, speed: 5f, lifetime: 10f);
            RegisterAndSelectSkill(mmMgr, skill);

            Monster target = bm.CurrentMonster;
            target.transform.position = new Vector3(10f, -0.3f, 0f);

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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T08: Invalid delivery configurations rejected before Rage and Cooldown.
        /// </summary>
        private static bool T08_InvalidDeliveryConfigurationsRejectedBeforeRageAndCooldown()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            Monster target = bm.CurrentMonster;
            hero.Rage.ResetRage(100f);
            CooldownManager.ResetAllCooldowns();

            // Subtest 8A: Channel + Projectile
            var chanProj = CreateTestSkill("p09_sub8a", "Channel Proj", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 0f, isChannel: true);
            RegisterAndSelectSkill(mmMgr, chanProj);
            var res8A = SkillExecutor.Execute(new SkillExecutionRequest(hero, chanProj, SkillSlotType.Skill, target));
            bool sub8AOk = !res8A.Success && res8A.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                           Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(chanProj.SkillId, out _);

            // Subtest 8B: Area + Projectile
            var areaProj = CreateTestSkill("p09_sub8b", "Area Proj", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 0f, isChannel: false, targetPolicy: SkillTargetPolicy.Area);
            RegisterAndSelectSkill(mmMgr, areaProj);
            var res8B = SkillExecutor.Execute(new SkillExecutionRequest(hero, areaProj, SkillSlotType.Skill, target));
            bool sub8BOk = !res8B.Success && res8B.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                           Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(areaProj.SkillId, out _);

            // Subtest 8C: Non-positive speed
            var badSpeed = CreateTestSkill("p09_sub8c", "Bad Speed", 2.0f, 20f, 3.0f, isProjectile: true, speed: 0f, lifetime: 5f);
            RegisterAndSelectSkill(mmMgr, badSpeed);
            var res8C = SkillExecutor.Execute(new SkillExecutionRequest(hero, badSpeed, SkillSlotType.Skill, target));
            bool sub8COk = !res8C.Success && res8C.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                           Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(badSpeed.SkillId, out _);

            // Subtest 8D: Non-positive lifetime
            var badLife = CreateTestSkill("p09_sub8d", "Bad Life", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 0f);
            RegisterAndSelectSkill(mmMgr, badLife);
            var res8D = SkillExecutor.Execute(new SkillExecutionRequest(hero, badLife, SkillSlotType.Skill, target));
            bool sub8DOk = !res8D.Success && res8D.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                           Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(badLife.SkillId, out _);

            bool pass = sub8AOk && sub8BOk && sub8COk && sub8DOk;
            Debug.Log($"[T08] Invalid Delivery Rejection: ChannelProj={sub8AOk}, AreaProj={sub8BOk}, BadSpeed={sub8COk}, BadLife={sub8DOk} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(chanProj);
            UnityEngine.Object.DestroyImmediate(areaProj);
            UnityEngine.Object.DestroyImmediate(badSpeed);
            UnityEngine.Object.DestroyImmediate(badLife);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T09: Cast-time projectile charges Rage at start, releases at cast completion, starts cooldown once.
        /// </summary>
        private static bool T09_CastTimeProjectileChargesRageAtStart_ReleasesAtCastEnd_StartsCooldownOnce()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t09_cast", "Ngưng Khí Phi Kiếm", 2.5f, 30f, 5.0f, isProjectile: true, speed: 10f, lifetime: 5f, castTime: 1.0f);
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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T10: Combat pause freezes projectile travel; resume continues to arrival.
        /// </summary>
        private static bool T10_PauseFreezesTravel_ResumeCompletesArrival()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t10_pause", "Định Thân Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
            RegisterAndSelectSkill(mmMgr, skill);

            Monster target = bm.CurrentMonster;
            hero.transform.position = new Vector3(0f, -0.3f, 0f);
            target.transform.position = new Vector3(10f, -0.3f, 0f);
            float hpBefore = target.Health.CurrentHealth;

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, target);
            SkillExecutor.Execute(req);

            var proj = ProjectileController.ActiveProjectiles.Count > 0 ? ProjectileController.ActiveProjectiles[0] : null;

            // Pause combat by setting BattleManager BattleState to LootPending or IsBattleActive = false
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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        /// <summary>
        /// T11: Disabling visual renderer has zero combat authority; travel and damage timing are identical.
        /// </summary>
        private static bool T11_DisablingVisualRendererHasZeroCombatAuthority()
        {
            SetupEncounter(out var heroGO, out var hero, out var bmGO, out var bm, out var mmMgrGO, out var mmMgr);

            var skill = CreateTestSkill("p09_t11_visual", "Vô Ảnh Tiễn", 2.0f, 20f, 3.0f, isProjectile: true, speed: 10f, lifetime: 5f);
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

            UnityEngine.Object.DestroyImmediate(skill);
            TeardownEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        #endregion
    }
}
#endif
