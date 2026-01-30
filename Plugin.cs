using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace ValheimFallGuard
{
    [BepInPlugin("com.yourname.valheimfallguard", "Valheim Fall Guard", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        public static Dictionary<long, PlayerFallState> States =
            new Dictionary<long, PlayerFallState>();

        // Config (portable tuning)
        public static ConfigEntry<float> FallTime;
        public static ConfigEntry<float> DamageCooldown;

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

            var harmony = new Harmony("com.yourname.valheimfallguard");
            harmony.PatchAll();

            Logger.LogInfo("Valheim Fall Guard loaded");
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
    }
}
