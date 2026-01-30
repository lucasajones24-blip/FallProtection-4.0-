# Valheim Fall Guard

Server-side fall protection for Valheim (BepInEx). Designed for dedicated servers and other server-only setups.

## Overview

Valheim Fall Guard monitors player falls and teleports players back to a safe, grounded position when a long fall would otherwise cause death or repeated damage loops.

## Features

- Server-side fall detection (no client install required)
- Teleport to last grounded safe position after a configurable fall duration
- Cooldown after recent damage to prevent abuse
- Velocity-based airborne detection for reliable fall tracking
- Ground clamping for safe teleport placement

## Installation

1) Build or download the DLL.
2) Copy it into your server’s plugin folder:

```
BepInEx/plugins/
```

3) Start the server. Configuration is generated in:

```
BepInEx/config/
```

## Configuration

The following settings are generated on first run:

- `FallTime`: Seconds falling before teleport.
- `DamageCooldown`: Seconds after damage before teleport is allowed.
- `FallVelocityThreshold`: Vertical velocity (m/s) at or below which a player is considered falling.
- `TeleportCooldown`: Seconds to ignore fall detection after teleport.

## Build (Portable)

1) Create a local `lib/` folder (not committed) next to the project file and place these assemblies:

- `BepInEx/core/BepInEx.dll`
- `BepInEx/core/0Harmony.dll`
- `BepInEx/unstripped_corlib/Assembly-CSharp.dll` (preferred) or `valheim_server_Data/Managed/Assembly-CSharp.dll`
- `BepInEx/unstripped_corlib/UnityEngine.dll` (preferred) or `valheim_server_Data/Managed/UnityEngine.dll`
- `BepInEx/unstripped_corlib/UnityEngine.CoreModule.dll` (preferred) or `valheim_server_Data/Managed/UnityEngine.CoreModule.dll`

2) Build in Release configuration.

## Notes

- This mod can be used alongside ServersideQoL portal hub features.
- If you see a BepInEx version warning, update BepInEx or rebuild against your server’s version.
