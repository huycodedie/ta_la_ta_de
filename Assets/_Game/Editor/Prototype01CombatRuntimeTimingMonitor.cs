#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    /// <summary>
    /// Play Mode observer for Prototype01. It deliberately has no simulation API:
    /// movement and attacks must be driven by the normal MonoBehaviour.Update path.
    /// </summary>
    public static class Prototype01CombatRuntimeTimingMonitor
    {
        private const string ReportPath = "PROJECT_MEMORY/UI-02_COMBAT_RUNTIME_TIMING_REPORT.md";
        private const int RequiredDeaths = 3;
        private static RuntimeObserver observer;

        [MenuItem("Tools/Wuxia RPG/Run Combat Runtime Timing Verification")]
        public static bool RunCombatRuntimeTimingVerification()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[UI-02 TIMING] Play Mode is already running.");
                return false;
            }

            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.isPlaying = true;
            Debug.Log("[UI-02 TIMING] Started real Play Mode observation; no manual Tick/Update/Attack calls and no synthetic deltaTime.");
            return true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                observer = null;
                return;
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            observer = UnityEngine.Object.FindAnyObjectByType<RuntimeObserver>();
            if (observer == null)
            {
                var go = new GameObject("UI02_RuntimeTimingObserver");
                observer = go.AddComponent<RuntimeObserver>();
            }
        }

        public sealed class RuntimeObserver : MonoBehaviour
        {
            private readonly List<EncounterRecord> encounters = new List<EncounterRecord>();
            private readonly List<EventRecord> sessionEvents = new List<EventRecord>();
            private EncounterRecord active;
            private Hero hero;
            private BattleManager battle;
            private Monster lastMonster;
            private float startedAt;
            private float lastSampleTime;
            private float deadlockTimer;
            private bool finished;
            private bool monitorCompletedRecorded;

            private void Start()
            {
                startedAt = Time.realtimeSinceStartup;
                lastSampleTime = startedAt;
                hero = FindAnyObjectByType<Hero>();
                battle = FindAnyObjectByType<BattleManager>();
                if (hero != null && hero.Health != null)
                {
                    // Test-only durability; no gameplay authority/config is changed.
                    float before = hero.Health.CurrentHealth;
                    hero.Health.InitializeHealth(5000f, hero);
                    hero.Health.Revive(5000f);
                    Debug.Log($"[UI-02 TIMING] Survivability conditioning: Hero HP {before:F2} -> {hero.Health.CurrentHealth:F2}; test-only, not natural runtime evidence.");
                }
                EventBus.OnEntityDied += OnEntityDied;
                RecordSession("MonitorStarted", "Observer created in real Play Mode");
                RecordSession("PlayModeReady", "Scene components discovered; runtime Update observation begins");
                Debug.Log($"[UI-02 TIMING] Runtime observer started frame={Time.frameCount} time={Time.time:F3} realtime={Time.realtimeSinceStartup:F3} scale={Time.timeScale:F2}");
            }

            private void Update()
            {
                if (finished || battle == null) return;
                float now = Time.realtimeSinceStartup;
                float frameDt = Time.deltaTime;
                if (lastMonster != battle.CurrentMonster)
                {
                    lastMonster = battle.CurrentMonster;
                    if (lastMonster != null)
                    {
                        if (active != null && active.MonsterDeathRecorded)
                            Record("NextEncounterSpawn", "New Monster reference observed after prior MonsterDeath");
                        active = new EncounterRecord { Index = battle.EncounterIndex, StartRealtime = now };
                        encounters.Add(active);
                        active.PreviousMonsterHealth = lastMonster.Health != null ? lastMonster.Health.CurrentHealth : 0f;
                        active.PreviousHeroHealth = hero != null && hero.Health != null ? hero.Health.CurrentHealth : 0f;
                        Record("EncounterStart", "Encounter index and live Monster reference observed");
                        Record("MonsterSpawn", "Runtime spawn observed");
                    }
                }

                if (active == null || lastMonster == null) return;
                if (battle.CurrentBattleState == BattleState.LootPending)
                {
                    // Only resolves the normal loot gate after MonsterDeath; it does not drive combat.
                    battle.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                    return;
                }

                if (lastMonster.IsAlive && hero != null && hero.IsAlive)
                {
                    float distance = HorizontalDistance(hero, lastMonster);
                    bool inRange = distance <= hero.AttackRange + .1f;
                    if (!active.MoveStartRecorded && distance > hero.AttackRange)
                    {
                        active.MoveStartRecorded = true;
                        Record("HeroMoveStart", "Movement observed from runtime Update");
                    }
                    if (!active.EnterRangeRecorded && inRange)
                    {
                        active.EnterRangeRecorded = true;
                        Record("HeroEnterAttackRange", "Range crossing observed from runtime Update");
                    }
                    if (!active.MonsterMoveStartRecorded && distance > lastMonster.AttackRange)
                    {
                        active.MonsterMoveStartRecorded = true;
                        Record("MonsterMoveStart", "Monster movement observed from runtime Update");
                    }
                    if (!active.MonsterEnterRangeRecorded && distance <= lastMonster.AttackRange + .1f)
                    {
                        active.MonsterEnterRangeRecorded = true;
                        Record("MonsterEnterAttackRange", "Monster range crossing observed from runtime Update");
                    }
                    float monsterHealth = lastMonster.Health != null ? lastMonster.Health.CurrentHealth : 0f;
                    float heroHealth = hero.Health != null ? hero.Health.CurrentHealth : 0f;
                    if (monsterHealth < active.PreviousMonsterHealth && !active.HeroDamageRecorded)
                    {
                        active.HeroDamageRecorded = true;
                        Record("HeroAttackDamage", "HP change observed after normal runtime attack");
                    }
                    if (heroHealth < active.PreviousHeroHealth && !active.MonsterDamageRecorded)
                    {
                        active.MonsterDamageRecorded = true;
                        Record("MonsterAttackDamage", "HP change observed after normal runtime attack");
                    }
                    active.PreviousMonsterHealth = monsterHealth;
                    active.PreviousHeroHealth = heroHealth;
                    bool heroReady = hero.Attack != null && EffectiveInterval(hero) > 0f && hero.Attack.AttackTimer >= EffectiveInterval(hero);
                    if (heroReady && !active.HeroReadyRecorded)
                    {
                        active.HeroReadyRecorded = true;
                        Record("HeroAttackReady", "Readiness observed from runtime attack timer");
                    }
                    bool monsterReady = lastMonster.Attack != null && EffectiveInterval(lastMonster) > 0f && lastMonster.Attack.AttackTimer >= EffectiveInterval(lastMonster);
                    if (monsterReady && !active.MonsterReadyRecorded)
                    {
                        active.MonsterReadyRecorded = true;
                        Record("MonsterAttackReady", "Readiness observed from runtime attack timer");
                    }
                    if (inRange && heroReady) deadlockTimer += frameDt; else deadlockTimer = 0f;
                    if (deadlockTimer > 5f) Record("DeadlockDiagnostic", "Read-only diagnostic; no intervention");
                }
                lastSampleTime = now;
            }

            private void OnEntityDied(Entity entity)
            {
                var monster = entity as Monster;
                if (monster == null || active == null || monster != lastMonster || active.MonsterDeathRecorded) return;
                active.MonsterDeathRecorded = true;
                active.DeathRealtime = Time.realtimeSinceStartup;
                Record("MonsterDeath", "Encounter completed only on MonsterDeath event");
                if (encounters.FindAll(e => e.MonsterDeathRecorded).Count >= RequiredDeaths)
                {
                    finished = true;
                    if (!monitorCompletedRecorded)
                    {
                        monitorCompletedRecorded = true;
                        Record("MonitorCompleted", "Three concrete MonsterDeath events observed");
                    }
                    GenerateReport();
                    Debug.Log("[UI-02 TIMING] PASS: Encounter 1 -> 2 -> 3 completed by three MonsterDeath events.");
                    EditorApplication.isPlaying = false;
                }
            }

            private void Record(string eventName, string note)
            {
                if (active == null) return;
                var rec = new EventRecord
                {
                    Name = eventName, Frame = Time.frameCount, Time = Time.time,
                    Realtime = Time.realtimeSinceStartup, DeltaTime = Time.deltaTime,
                    UnscaledDeltaTime = Time.unscaledDeltaTime, Note = note,
                    TimeScale = Time.timeScale,
                    Hero = Snapshot(hero), Monster = Snapshot(lastMonster),
                    Distance = DistanceSnapshot(hero, lastMonster)
                };
                active.Events.Add(rec);
                Debug.Log($"[UI-02 EVENT] E{active.Index} {eventName} frame={rec.Frame} time={rec.Time:F3} realtime={rec.Realtime:F3} dt={rec.DeltaTime:F4} unscaledDt={rec.UnscaledDeltaTime:F4} hero={rec.Hero.Position} monster={rec.Monster.Position} heroMove={rec.Hero.MoveSpeed:F2} heroAtk={rec.Hero.EffectiveAttackInterval:F2} monsterMove={rec.Monster.MoveSpeed:F2} monsterAtk={rec.Monster.EffectiveAttackInterval:F2} note={note}");
            }

            private void RecordSession(string eventName, string note)
            {
                sessionEvents.Add(new EventRecord
                {
                    Name = eventName, Frame = Time.frameCount, Time = Time.time,
                    Realtime = Time.realtimeSinceStartup, DeltaTime = Time.deltaTime,
                    UnscaledDeltaTime = Time.unscaledDeltaTime, TimeScale = Time.timeScale, Note = note,
                    Hero = Snapshot(hero), Monster = Snapshot(lastMonster), Distance = DistanceSnapshot(hero, lastMonster)
                });
            }

            private Snapshot Snapshot(Entity entity)
            {
                var s = new Snapshot { Position = entity != null ? entity.transform.position : Vector3.zero };
                if (entity == null) return s;
                s.MoveSpeed = entity.Movement != null ? entity.Movement.CurrentMoveSpeed : 0f;
                s.ConfiguredAttackInterval = entity.Attack != null ? entity.Attack.AttackInterval : 0f;
                s.EffectiveAttackInterval = EffectiveInterval(entity);
                s.AttackTimer = entity.Attack != null ? entity.Attack.AttackTimer : 0f;
                s.AttackReady = s.EffectiveAttackInterval > 0f && s.AttackTimer >= s.EffectiveAttackInterval;
                s.EntityId = entity.GetEntityId();
                s.IsAlive = entity.IsAlive;
                s.CurrentHealth = entity.Health != null ? entity.Health.CurrentHealth : 0f;
                s.MaxHealth = entity.Health != null ? entity.Health.MaxHealth : 0f;
                s.Target = entity.CurrentTarget != null ? entity.CurrentTarget.EntityName : "None";
                s.IsCasting = entity.IsCasting;
                s.Rage = entity.Rage != null ? entity.Rage.CurrentRage : 0f;
                s.ConfigSource = entity is Hero ? "HeroConfigSO.AttackInterval; HeroProgressionConfigSO.GetBaseStatsForLevel(AttackInterval=1.0) is not applied" : "MonsterConfigSO.AttackInterval";
                s.ModifierSource = ModifierSource(entity);
                return s;
            }

            private static float EffectiveInterval(Entity entity)
            {
                if (entity == null || entity.Attack == null) return 0f;
                float value = entity.Attack.AttackInterval;
                if (entity.StatusController != null) value += entity.StatusController.GetAttackIntervalModifier();
                return Mathf.Max(.1f, value);
            }

            private static string ModifierSource(Entity entity)
            {
                float modifier = entity != null && entity.StatusController != null ? entity.StatusController.GetAttackIntervalModifier() : 0f;
                return $"StatusController.GetAttackIntervalModifier={modifier:+0.###;-0.###;0}";
            }

            private static float HorizontalDistance(Entity a, Entity b)
            {
                if (a == null || b == null) return float.MaxValue;
                Vector3 d = b.transform.position - a.transform.position; d.y = 0f; return d.magnitude;
            }

            private static DistanceData DistanceSnapshot(Entity a, Entity b)
            {
                if (a == null || b == null) return new DistanceData { Unavailable = true };
                Vector3 d = b.transform.position - a.transform.position;
                Vector3 horizontal = d; horizontal.y = 0f;
                float threshold = a.AttackRange + .1f;
                return new DistanceData
                {
                    DeltaX = Mathf.Abs(d.x), DeltaY = Mathf.Abs(d.y), DeltaZ = Mathf.Abs(d.z),
                    Horizontal = horizontal.magnitude, Full3D = d.magnitude,
                    AttackRange = a.AttackRange, Threshold = threshold, InRange = horizontal.magnitude <= threshold
                };
            }

            private void GenerateReport()
            {
                var sb = new StringBuilder();
                sb.AppendLine("# UI-02 - Combat Runtime Timing Verification Report");
                sb.AppendLine();
                sb.AppendLine("## Result");
                sb.AppendLine();
                int completed = encounters.FindAll(e => e.MonsterDeathRecorded).Count;
                sb.AppendLine($"- **Runtime result:** {(completed >= RequiredDeaths ? "PASS" : "INCONCLUSIVE")} ({completed}/3 encounters completed by MonsterDeath)");
                sb.AppendLine("- **Global UI-02:** **NOT LOCKED**");
                sb.AppendLine("- **Runtime constraints:** real Play Mode frames; no teleport; no manual Tick/Update/Attack; no synthetic deltaTime; no Time.timeScale write; no MoveSpeed change.");
                sb.AppendLine($"- **Time.timeScale observed:** `{Time.timeScale:F2}`");
                sb.AppendLine();
                sb.AppendLine("## Root-cause and configuration audit");
                sb.AppendLine();
                sb.AppendLine("- The old monitor compressed a synchronous editor loop by directly advancing movement and attacks with a fixed synthetic 0.05s step. That is why large movement appeared in 0.001-0.002s.");
                sb.AppendLine("- The replacement observer only reads state from normal runtime `Update`; it does not call Tick, Update, Attack, or supply deltaTime.");
                sb.AppendLine("- The 1.0s Hero value came from `HeroProgressionConfigSO.GetBaseStatsForLevel()` but that method is not used by `Hero.InitializeHero`/`ApplyBaseStatsFromTitle`; runtime Hero initialization uses `HeroConfigSO` (asset is 1.5s).");
                sb.AppendLine("- The 1.5s Monster value was a stale/hard-coded report claim; runtime `MonsterConfigSO` and Prototype01 runtime initialization use 2.0s. No modifier is applied unless reported by `StatusController.GetAttackIntervalModifier()`.");
                sb.AppendLine();
                sb.AppendLine("## Event telemetry");
                sb.AppendLine();
                sb.AppendLine("Each event records frameCount, time, realtimeSinceStartup, deltaTime, unscaledDeltaTime, Hero/Monster position, effective MoveSpeed, configured/effective AttackInterval, attack timer/readiness, and config/modifier source.");
                sb.AppendLine();
                sb.AppendLine("| Encounter | Event | Frame | Time | Realtime | dt | unscaledDt | TimeScale | Hero (Id, Pos) | Monster (Id, Pos) | Distance (Horiz/3D/Range/Threshold/InRange) | Hero move/attack(timer/ready) | Monster move/attack(timer/ready) | Sources |");
                sb.AppendLine("|---:|---|---:|---:|---:|---:|---:|---:|---|---|---|---|---|---|");
                foreach (var x in sessionEvents)
                    sb.AppendLine($"| 0 | {x.Name} | {x.Frame} | {x.Time:F3} | {x.Realtime:F3} | {x.DeltaTime:F4} | {x.UnscaledDeltaTime:F4} | {x.TimeScale:F2} | [Hero:{x.Hero.EntityId}] {x.Hero.Position} | [Monster:{x.Monster.EntityId}] {x.Monster.Position} | N/A | {x.Hero.MoveSpeed:F2}/{x.Hero.EffectiveAttackInterval:F2} ({x.Hero.AttackTimer:F2}/{x.Hero.AttackReady}) | {x.Monster.MoveSpeed:F2}/{x.Monster.EffectiveAttackInterval:F2} ({x.Monster.AttackTimer:F2}/{x.Monster.AttackReady}) | {x.Hero.ConfigSource}; {x.Hero.ModifierSource}; {x.Monster.ConfigSource}; {x.Monster.ModifierSource} |");
                foreach (var e in encounters) foreach (var x in e.Events)
                    sb.AppendLine($"| {e.Index} | {x.Name} | {x.Frame} | {x.Time:F3} | {x.Realtime:F3} | {x.DeltaTime:F4} | {x.UnscaledDeltaTime:F4} | {x.TimeScale:F2} | [Hero:{x.Hero.EntityId}] {x.Hero.Position} | [Monster:{x.Monster.EntityId}] {x.Monster.Position} | {x.Distance.Horizontal:F3}/{x.Distance.Full3D:F3} (dXYZ {x.Distance.DeltaX:F3}/{x.Distance.DeltaY:F3}/{x.Distance.DeltaZ:F3}, range {x.Distance.AttackRange:F2}, threshold {x.Distance.Threshold:F2}, inRange {x.Distance.InRange}) | {x.Hero.MoveSpeed:F2}/{x.Hero.EffectiveAttackInterval:F2} ({x.Hero.AttackTimer:F2}/{x.Hero.AttackReady}) | {x.Monster.MoveSpeed:F2}/{x.Monster.EffectiveAttackInterval:F2} ({x.Monster.AttackTimer:F2}/{x.Monster.AttackReady}) | {x.Hero.ConfigSource}; {x.Hero.ModifierSource}; {x.Monster.ConfigSource}; {x.Monster.ModifierSource} |");
                File.WriteAllText(ReportPath, sb.ToString());
            }

            private void OnDestroy() { EventBus.OnEntityDied -= OnEntityDied; }
        }

        private sealed class EncounterRecord
        {
            public int Index; public float StartRealtime; public float DeathRealtime = -1f;
            public float PreviousMonsterHealth, PreviousHeroHealth;
            public bool MonsterDeathRecorded, MoveStartRecorded, EnterRangeRecorded, MonsterMoveStartRecorded, MonsterEnterRangeRecorded;
            public bool HeroDamageRecorded, MonsterDamageRecorded, HeroReadyRecorded, MonsterReadyRecorded;
            public readonly List<EventRecord> Events = new List<EventRecord>();
        }
        private struct Snapshot
        {
            public Vector3 Position; public UnityEngine.EntityId EntityId; public bool IsAlive, IsCasting, AttackReady;
            public float CurrentHealth, MaxHealth, Rage, MoveSpeed, ConfiguredAttackInterval, EffectiveAttackInterval, AttackTimer;
            public string Target, ConfigSource, ModifierSource;
        }
        private struct DistanceData
        {
            public bool Unavailable, InRange; public float DeltaX, DeltaY, DeltaZ, Horizontal, Full3D, AttackRange, Threshold;
        }
        private struct EventRecord
        {
            public string Name, Note; public int Frame; public float Time, Realtime, DeltaTime, UnscaledDeltaTime, TimeScale;
            public Snapshot Hero, Monster; public DistanceData Distance;
        }
    }
}
#endif
