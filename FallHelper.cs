using UnityEngine;

namespace ValheimFallGuard
{
    public static class FallHelper
    {
        private static float _lastGroundFallbackLogTime = -999f;

        public static bool IsAdmin(Player p)
        {
            if (ZNet.instance == null)
                return false;

            return ZNet.instance.IsAdmin(p.GetPlayerID().ToString());
        }

        public static void TeleportSafe(Player p, Vector3 pos)
        {
            var groundedPos = GetGroundedPosition(pos);
            p.TeleportTo(groundedPos, p.transform.rotation, true);

            ClearVelocity(p);
        }

        public static Vector3 GetGroundedPosition(Vector3 origin)
        {
            var start = origin + Vector3.up * 2f;
            const float maxDistance = 2000f;

            if (Physics.Raycast(start, Vector3.down, out var hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * 0.2f;
            }

            if (TryGetHeightmapHeight(origin, out var height))
            {
                LogGroundFallback($"Raycast miss at {origin}, using heightmap y={height:F2}");
                return new Vector3(origin.x, height + 0.2f, origin.z);
            }

            LogGroundFallback($"Raycast miss at {origin}, no heightmap hit; using origin");

            return origin;
        }

        private static bool TryGetHeightmapHeight(Vector3 origin, out float height)
        {
            height = 0f;

            var heightmapType = typeof(ZNet).Assembly.GetType("Heightmap");
            if (heightmapType == null)
                return false;

            var methods = heightmapType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.Name != "GetHeight")
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Vector3) && method.ReturnType == typeof(float))
                {
                    height = (float)method.Invoke(null, new object[] { origin });
                    return true;
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(Vector3) && parameters[1].ParameterType == typeof(float).MakeByRefType())
                {
                    object[] args = { origin, 0f };
                    var result = method.Invoke(null, args);
                    if (result is bool ok && ok)
                    {
                        height = (float)args[1];
                        return true;
                    }

                    if (result is float f)
                    {
                        height = f;
                        return true;
                    }
                }
            }

            return false;
        }

        private static void LogGroundFallback(string message)
        {
            if (Time.time - _lastGroundFallbackLogTime < 2f)
                return;

            _lastGroundFallbackLogTime = Time.time;
            Plugin.LogDebug($"[Fall Guard] {message}");
        }

        private static void ClearVelocity(Player p)
        {
            Rigidbody rb = null;

            rb = p.GetComponent<Rigidbody>();
            if (rb == null)
                rb = p.GetComponentInChildren<Rigidbody>();

            if (rb == null)
            {
                var field = typeof(Character).GetField("m_body", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                    rb = field.GetValue(p) as Rigidbody;
            }

            if (rb == null)
                return;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
