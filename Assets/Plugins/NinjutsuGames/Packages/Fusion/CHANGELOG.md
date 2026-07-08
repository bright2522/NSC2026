# Changelog — Fusion Module

## 1.3.9 (26th October 2025)

Fixed
- Fixed network prop attachment synchronization bug (Thanks Tosh)
- Fixed cached rpc initial invocation bug
- Support for latest Game Creator version 2.18.58

New
- Added default Profanity Filter asset

# 1.3.8 (14th September 2025)

New
- Region selection tools — Select Best Region instruction and ping shown in dropdowns
- Tick timers — Stop TickTimer instruction
- Register Character Models instruction — Quickly register model prefabs for replication
- NetworkCharacter — Addressable model support for character models
- Network status & helpers — Conditions for session/internet status plus simple disconnect/shutdown checks
- Session & player data — Last player left, visibility/open checks, user ID and last joined player info
- Chat & authentication — Optional profanity filter and custom authentication; centralized auth settings
- Fail‑Safe system — Configurable protections against common runtime issues
- Error messages library — Curated, editable network message texts
- WebGL — Clipboard copy support

Enhanced
- Scene loading — Smoother transitions with clearer Start/Done events and fewer allocations
- Region selection — Auto‑best option, cleaned lists, improved ping detection and display
- Networking stability — Safer authority guards
- Pooling & capacity — Configurable defaults and right‑sized containers
- UI/UX — More reliable room chat, lobby control activations, clearer shutdown reasons and error messages
- NetworkSceneManager — Better orchestration for multi‑scene management

Changed
- Authority handling — Automatic ownership transfer when overriding authority; request authority for orphaned objects
- Spawning — Aligned Spawn and SpawnAsync behavior
- Pooling options — Can disable pooling per module; increased capacities where needed

Fixed
- Stability — Many null‑reference protections and safer error paths
- Scene, spawn & lifecycle — Reliable Spawned/Despawned events and player spawn/despawn; regressions addressed in NetworkSceneManager
- Chat & lobby — Robust initialization even if prefabs or addressables aren’t loaded; authentication method support
- Regions & ping — Edge‑case handling and invalid values; dropdowns show accurate ping
- WebGL & platform — Loading and Network Object Provider behavior; clipboard follow‑ups; compile/build fixes
- Authority & state — Correct dead‑state sync and facing on authority change; guard checks
- Addressables & errors — Better shutdown reason retrieval; auto‑release of handles; safer StartGameAsync and related flows
