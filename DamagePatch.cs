using HarmonyLib;
using UnityEngine;

namespace ValheimFallGuard
{
    [HarmonyPatch(typeof(Character), "Damage")]
    public class DamagePatch
    {
        static void Prefix(Character __instance)
        {
            if (!(__instance is Player p))
                return;

            long id = p.GetPlayerID();

            var state = Plugin.GetState(id);

            state.LastDamage = Time.time;
        }
    }
}
