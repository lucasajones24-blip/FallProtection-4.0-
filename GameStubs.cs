// Valheim game type stubs for portable compilation
// These satisfy the compiler; at runtime, the actual game DLLs provide the real implementations

using UnityEngine;
using System;

public class HitData { }

namespace UnityEngine
{
    public class Rigidbody : Component
    {
        public Vector3 velocity { get; set; }
    }
}

public class Character : MonoBehaviour
{
    public float m_groundContact;
    public bool IsOnGround() => false;
    public virtual void Damage(HitData hit) { }
    public virtual void UpdateGroundContact() { }
}

public class Player : Character
{
    public long GetPlayerID() => 0L;
    public void TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport) { }
}

public class ZNet
{
    public static ZNet instance;
    public bool IsAdmin(long playerID) => false;
}
