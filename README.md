# Valheim Fall Guard

Portable server-side fall protection mod for Valheim (BepInEx).

## Build (Portable)

1) Create a local lib folder (not committed):

- BepInEx/core/BepInEx.dll
- BepInEx/core/0Harmony.dll
- BepInEx/unstripped_corlib/Assembly-CSharp.dll (preferred) or valheim_server_Data/Managed/Assembly-CSharp.dll
- BepInEx/unstripped_corlib/UnityEngine.dll (preferred) or valheim_server_Data/Managed/UnityEngine.dll
- BepInEx/unstripped_corlib/UnityEngine.CoreModule.dll (preferred) or valheim_server_Data/Managed/UnityEngine.CoreModule.dll

Copy those into a local `lib/` folder next to the project file.

2) Build in Release configuration.

## Install

Drop the built DLL into:

BepInEx/plugins/

Start the server. Config auto-generates under BepInEx/config/.
this can be used in line with the serversideqol portal hub. 
