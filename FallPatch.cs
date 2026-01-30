using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ValheimFallGuard
{
    [HarmonyPatch(typeof(Character))]
    [HarmonyPatch("UpdateGroundContact")]
    public class FallPatch
    {
        static void Postfix(Character __instance)
        {
            Process(__instance, "UpdateGroundContact");
        }

        public static void TickPostfix(Character __instance, MethodBase __originalMethod)
        {
            Process(__instance, __originalMethod?.Name ?? "Tick");
        }

        public static void Process(Character __instance, string source)
        {
            if (!(__instance is Player p))
                return;

            // Admins ignored
            if (FallHelper.IsAdmin(p))
                return;

            long id = p.GetPlayerID();
            var state = Plugin.GetState(id);

            if (Time.time - state.LastProcessTime < 0.05f)
                return;

            state.LastProcessTime = Time.time;

            if (Time.time - state.LastTeleportTime < Plugin.TeleportCooldown.Value)
                return;

            var rb = p.GetComponent<Rigidbody>();
            float vy = rb != null ? rb.linearVelocity.y : 0f;
            bool grounded = __instance.IsOnGround();
            bool airborneByVelocity = rb != null && vy <= Plugin.FallVelocityThreshold.Value;
            bool airborne = !grounded || airborneByVelocity;

            if (airborne != state.LastAirborne)
            {
                state.LastAirborne = airborne;
                if (airborne)
                    Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: AIRBORNE (vy={vy:F2})");
                else
                    Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: GROUNDED");
            }

            // Only track fall state if not grounded
            // Grounded = safe
            if (!airborne)
            {
                var groundedPos = FallHelper.GetGroundedPosition(p.transform.position);
                if (state.LastSafePos == Vector3.zero)
                {
                    state.LastSafePos = groundedPos;
                    Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: Initialized SafePos={state.LastSafePos}");
                }
                else
                {
                    state.LastSafePos = groundedPos;
                    if (Time.time - state.LastSafeLogTime > 2f)
                    {
                        state.LastSafeLogTime = Time.time;
                        Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: Updated SafePos={state.LastSafePos}");
                    }
                }

                state.Reset();
                return;
            }

            // Start falling
            if (!state.IsFalling)
            {
                state.IsFalling = true;
                state.FallStart = Time.time;
                Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: START FALLING at time {state.FallStart}");
                return;
            }

            // Already falling
            float fallTime = Time.time - state.FallStart;
            if (Time.time - state.LastFallLogTime > 1f)
            {
                state.LastFallLogTime = Time.time;
                Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: FALLING - {fallTime:F2}s / {Plugin.FallTime.Value:F2}s (vy={vy:F2})");
            }

            if (fallTime < Plugin.FallTime.Value)
                return;

            float sinceDamage = Time.time - state.LastDamage;
            Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: TIMEOUT REACHED! TimeSinceDamage: {sinceDamage:F2}s / CooldownThreshold: {Plugin.DamageCooldown.Value:F2}s");

            if (sinceDamage < Plugin.DamageCooldown.Value)
            {
                if (Time.time - state.LastCooldownLogTime > 1f)
                {
                    state.LastCooldownLogTime = Time.time;
                    Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: COOLDOWN ACTIVE - Cannot teleport yet");
                }
                return;
            }

            // Teleport safe
            Plugin.LogDebug($"[Fall Guard] ({source}) Player {id}: TELEPORTING to {state.LastSafePos}");
            FallHelper.TeleportSafe(p, state.LastSafePos);
            state.LastTeleportTime = Time.time;
            state.Reset();
        }
    }

}
