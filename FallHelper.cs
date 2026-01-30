using UnityEngine;

namespace ValheimFallGuard
{
    public static class FallHelper
    {
        public static bool IsAdmin(Player p)
        {
            if (ZNet.instance == null)
                return false;

            return ZNet.instance.IsAdmin(p.GetPlayerID());
        }

        public static void TeleportSafe(Player p, Vector3 pos)
        {
            p.TeleportTo(pos, p.transform.rotation, true);

            var rb = p.GetComponent<Rigidbody>();

            if (rb != null)
                rb.velocity = Vector3.zero;
        }
    }
}
