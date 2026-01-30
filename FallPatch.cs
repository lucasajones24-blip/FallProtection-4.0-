using HarmonyLib;
using UnityEngine;

namespace ValheimFallGuard
{
    [HarmonyPatch(typeof(Character), "UpdateGroundContact")]
    public class FallPatch
    {
        static void Postfix(Character __instance)
        {
            if (!(__instance is Player p))
                return;

            // Admins ignored
            if (FallHelper.IsAdmin(p))
                return;

            long id = p.GetPlayerID();

            var state = Plugin.GetState(id);

            bool grounded = __instance.m_groundContact > 0;

            // Grounded = safe
            if (grounded)
            {
                state.LastSafePos = p.transform.position;
                state.Reset();
                return;
            }

            // Start falling
            if (!state.IsFalling)
            {
                state.IsFalling = true;
                state.FallStart = Time.time;
                return;
            }

            // Already falling
            float fallTime = Time.time - state.FallStart;

            if (fallTime < Plugin.FallTime.Value)
                return;

            float sinceDamage = Time.time - state.LastDamage;

            if (sinceDamage < Plugin.DamageCooldown.Value)
                return;

            // Teleport
            FallHelper.TeleportSafe(p, state.LastSafePos);

            state.Reset();
        }
    }
}
