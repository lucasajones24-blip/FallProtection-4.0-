using UnityEngine;

namespace ValheimFallGuard
{
    public class PlayerFallState
    {
        public bool IsFalling;
        public bool LastAirborne;
        public float FallStart;
        public float LastDamage = -999f; // Initialize to far past to allow first teleport
        public Vector3 LastSafePos;
        public float LastProcessTime = -999f;
        public float LastFallLogTime = -999f;
        public float LastCooldownLogTime = -999f;
        public float LastTeleportTime = -999f;
        public float LastSafeLogTime = -999f;

        public void Reset()
        {
            IsFalling = false;
            FallStart = 0f;
            LastAirborne = false;
            LastFallLogTime = -999f;
            LastCooldownLogTime = -999f;
        }
    }
}
