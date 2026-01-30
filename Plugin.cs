using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ValheimFallGuard
{
    [BepInPlugin("com.yourname.valheimfallguard", "Valheim Fall Guard", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        private float _lastUpdateLog;

        public static Dictionary<long, PlayerFallState> States =
            new Dictionary<long, PlayerFallState>();

        // Config (portable tuning)
        public static ConfigEntry<float> FallTime;
        public static ConfigEntry<float> DamageCooldown;
        public static ConfigEntry<float> FallVelocityThreshold;
        public static ConfigEntry<float> TeleportCooldown;

        private void Awake()
        {
            Instance = this;

            FallTime = Config.Bind(
                "General",
                "FallTime",
                5f,
                "Seconds falling before teleport");

            DamageCooldown = Config.Bind(
                "General",
                "DamageCooldown",
                8f,
                "Seconds after damage before teleport allowed");

            FallVelocityThreshold = Config.Bind(
                "General",
                "FallVelocityThreshold",
                -5f,
                "Vertical velocity (m/s) at or below which a player is considered falling, even if grounded is true");

            TeleportCooldown = Config.Bind(
                "General",
                "TeleportCooldown",
                2f,
                "Seconds to ignore fall detection after teleport to let physics settle");

            var harmony = new Harmony("com.yourname.valheimfallguard");
            
            Logger.LogInfo("Attempting to patch methods...");
            harmony.PatchAll();

            TryPatchOptional(harmony, typeof(Player), "Update");
            TryPatchOptional(harmony, typeof(Player), "FixedUpdate");
            TryPatchOptional(harmony, typeof(Character), "Update");
            TryPatchOptional(harmony, typeof(Character), "FixedUpdate");
            
            var patchedCount = harmony.GetPatchedMethods().Count();
            Logger.LogInfo($"Valheim Fall Guard loaded - Applied {patchedCount} patches");
            
            foreach (var method in harmony.GetPatchedMethods())
            {
                Logger.LogInfo($"Patched: {method.DeclaringType?.Name}.{method.Name}");
            }

            DumpCandidateMethods(typeof(Character));
            DumpCandidateMethods(typeof(Player));
        }

        private void Start()
        {
            Logger.LogInfo("[Fall Guard] Plugin Start");
            InvokeRepeating(nameof(LogHeartbeat), 5f, 5f);
        }

        private void OnEnable()
        {
            Logger.LogInfo("[Fall Guard] Plugin OnEnable");
        }

        private void LogHeartbeat()
        {
            var players = GetPlayers();
            Logger.LogInfo($"[Fall Guard] Heartbeat, players={players.Count}");
        }

        private void Update()
        {
            // Fallback: drive fall processing from the plugin update loop
            // in case UpdateGroundContact is not called on the server.
            var players = GetPlayers();

            if (Time.time - _lastUpdateLog > 5f)
            {
                _lastUpdateLog = Time.time;
                Logger.LogInfo($"[Fall Guard] PluginUpdate tick, players={players.Count}");
            }

            foreach (var player in players)
            {
                FallPatch.Process(player, "PluginUpdate");
            }
        }

        private static List<Player> GetPlayers()
        {
            var players = new List<Player>();

            var method = typeof(Player).GetMethod("GetAllPlayers", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                try
                {
                    if (method.Invoke(null, null) is IEnumerable enumerable)
                    {
                        foreach (var obj in enumerable)
                        {
                            if (obj is Player p)
                                players.Add(p);
                        }
                    }
                }
                catch
                {
                    // Fall back to Unity object search below
                }
            }

            if (players.Count == 0)
            {
                players.AddRange(Object.FindObjectsByType<Player>(FindObjectsSortMode.None));
            }

            return players;
        }

        private void DumpCandidateMethods(System.Type type)
        {
            var names = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(m => m.Name)
                .Where(n => n.Contains("Update") || n.Contains("Ground") || n.Contains("Fixed") || n.Contains("Land"))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            Logger.LogInfo($"[Fall Guard] Candidate methods on {type.Name}: {string.Join(", ", names)}");
        }

        private void TryPatchOptional(Harmony harmony, System.Type type, string methodName)
        {
            var method = AccessTools.Method(type, methodName);
            if (method == null)
            {
                Logger.LogInfo($"[Fall Guard] Optional patch skipped (not found): {type.Name}.{methodName}");
                return;
            }

            var postfix = new HarmonyMethod(typeof(FallPatch), nameof(FallPatch.TickPostfix));
            harmony.Patch(method, postfix: postfix);
            Logger.LogInfo($"[Fall Guard] Optional patch applied: {type.Name}.{methodName}");
        }

        public static PlayerFallState GetState(long id)
        {
            if (!States.TryGetValue(id, out var s))
            {
                s = new PlayerFallState();
                States[id] = s;
            }

            return s;
        }

        public static void LogDebug(string message)
        {
            Instance?.Logger.LogInfo(message);
        }
    }
}
