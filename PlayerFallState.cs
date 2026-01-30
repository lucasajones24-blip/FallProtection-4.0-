using UnityEngine;

namespace ValheimFallGuard
{
    public class PlayerFallState
    {
        public bool IsFalling;
        public float FallStart;
        public float LastDamage;
        public Vector3 LastSafePos;

        public void Reset()
        {
            IsFalling = false;
            FallStart = 0f;
        }
    }
}
