# Skyloft — Unity Developer Case Study

A small wave-survival action game built in Unity 6. The player controls a hero with a virtual
joystick, surviving timed waves of enemies across three progressively harder levels. When the
timer runs out the player wins, and the total number of enemies defeated is persisted across
sessions.

---

## Tech Stack

| | |
|---|---|
| **Engine** | Unity `6000.4.5f1` |
| **Render Pipeline** | Universal Render Pipeline (URP `17.4.0`) |
| **Dependency Injection** | [Zenject / Extenject](https://github.com/modesttree/Zenject) |
| **Async** | [UniTask](https://github.com/Cysharp/UniTask) |
| **Tweening** | [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) |
| **Input** | [Joystick Pack](https://github.com/Brackeys/Mobile-Joystick) |
| **Navigation** | Unity AI Navigation (NavMesh) `2.0.13` |
| **UI / Text** | uGUI + TextMeshPro |
| **Shading** | Toony Colors Pro (JMO Assets) |
| **Target platform** | Android (APK) |

All gameplay code lives under the `SkyloftGame` assembly (`Assets/Scripts/SkyloftGame.asmdef`).

---

## Case Study Requirements — Coverage

| Requirement | Where |
|---|---|
| Joystick character movement | `Player/PlayerController.cs` (Joystick Pack) |
| Enemies approach & are defeated | `Enemy/EnemyAI.cs` (NavMesh), `Player/PlayerShooter.cs` → `Combat/Projectile.cs` |
| Timer + "Game Won" on timeout | `Gameplay/CountdownTimer.cs`, `States/GameWonState.cs`, `UI/GameWonView.cs` |
| Game Won shows enemies defeated | `UI/GameWonView.cs` (run kills + persisted total) |
| Proceed to next level | `UI/Buttons/NextLevelButton.cs`, `GameStateManager.GoToNextLevel()` |
| Persistent defeated count | `Data/EncryptedJsonDataService.cs`, `Data/GameData.cs` |
| Three levels, increasing difficulty | `Data/Level/Level 1·2·3.asset`, `Level/LevelDatabase.asset` |
| **State Machine pattern** | `StateMachine/GameStateMachine.cs`, `States/*` |
| **Level system** | `Level/LevelManager.cs`, `Level/LevelDatabase.cs` |
| **Animator** | `Player/PlayerAnimator.cs`, `Enemy/EnemyAnimator.cs` |
| **NavMesh AI** | `Enemy/EnemyAI.cs`, `Gameplay/EnemySpawner.cs` |
| **Object pooling** | `Pool/ObjectPooler.cs`, `Pool/GameObjectPool.cs`, `Pool/PooledObject.cs` |
| **Data persistence** | AES-256 + SHA-256 encrypted JSON in PlayerPrefs |
| **UI** | `UI/UIController.cs` + per-screen views |
| **Tweening** | PrimeTween (UI count-up, hit flash, HUD blink) |
| **Particle effects** | `VFX/*` (spawn / hit / death VFX, pooled) |
| **Optimization** | Pooling, live-enemy cap, throttled NavMesh queries, 60 FPS target |

---

## Architecture

The game is composed through **Zenject** (`Infrastructure/GameInstaller.cs`) and driven by a
lightweight **finite state machine**.

### Game states
`Menu → Playing → GameWon / GameLost`, managed by `GameStateMachine` and orchestrated through
`GameStateManager`. Each state (`MenuState`, `PlayingState`, `GameWonState`, `GameLostState`)
owns its enter/exit behavior — e.g. entering `Playing` starts the timer and spawner, while
`GameWon/GameLost/Menu` stop and clear the spawner.

### Win / lose conditions
- **Win** is purely time-based: enemies keep spawning in looped waves, and the player wins when
  the 3-minute timer elapses (per the case study brief).
- **Lose** happens if the player's health reaches zero before the timer ends.

### Wave spawning
`EnemySpawner` spawns waves on a fixed time cadence and **loops them until the level timer ends**.
Each `LevelData` defines the cadence and the enemy composition of a wave (see
[Configuring levels](#configuring-levels)). A live-enemy cap (`maxAliveEnemies`) throttles
spawning to protect performance during large waves.

### Enemy AI
Each enemy is a NavMesh agent with a tiny `Chase ↔ Attack` state machine. State is re-evaluated
on a configurable interval (`pathUpdateInterval`); the animator reacts to state changes, and
attack damage is gated again at the hit frame so a moving player can dodge.

### Object pooling
Projectiles, enemies and VFX are pooled (`ObjectPooler` + `GameObjectPool`). Pooled objects
implement `IPoolable` (`OnSpawn` / `OnDespawn`) so they reset cleanly on reuse.

### Data persistence
`EncryptedJsonDataService` serializes `GameData` to JSON, encrypts it with **AES-256 (CBC)** using
a **SHA-256**-derived key, and stores it base64-encoded in `PlayerPrefs`. Corrupt saves are backed
up and the game falls back to defaults instead of crashing.

---

## Project Structure

```
Assets/
├── Scenes/
│   └── Game Scene.unity          # the single playable scene
├── Scripts/
│   ├── Infrastructure/           # Zenject installer, app bootstrap (FPS)
│   ├── StateMachine/  States/    # game FSM + concrete states
│   ├── GameStateManager.cs       # top-level orchestrator
│   ├── Player/                   # controller, shooter, health, animator, targeting
│   ├── Enemy/                    # AI, animator, registry, data
│   ├── Combat/                   # projectiles
│   ├── Gameplay/                 # timer, spawner, score, pause
│   ├── Level/                    # level data, database, manager
│   ├── Pool/                     # object pooling
│   ├── Data/                     # save/load (encrypted JSON)
│   ├── UI/                       # views, buttons, HUD
│   ├── VFX/  Audio/  Core/       # particles, audio events, camera/locator
└── Data/                         # ScriptableObject assets (levels, enemies, pools, weapons…)
```

---

## Getting Started

### Requirements
- Unity **6000.4.5f1** (Unity 6) with the **Android Build Support** module.

### Run in the editor
1. Open the project in Unity Hub with the matching editor version.
2. Open `Assets/Scenes/Game Scene.unity`.
3. Press **Play**.

### Build an APK
1. **File → Build Settings → Android** (switch platform if needed).
2. Ensure `Game Scene` is the only scene in the build list.
3. Set a real **Company Name** and **Package Name** under *Project Settings → Player*
   (the project still ships with the Unity template identifier).
4. **Build** (or *Build and Run* on a connected device).

The build targets **60 FPS** (`Infrastructure/ApplicationBootstrap.cs`); Android otherwise
defaults to 30.

---

## Configuring Levels

Levels are data-driven `ScriptableObject`s (`Assets/Data/Level/`) referenced by `LevelDatabase`.
Each `LevelData` exposes:

| Field | Meaning |
|---|---|
| `durationSeconds` | Survival time; the level is won when this reaches 0 (default 180s / 3 min) |
| `firstWaveDelay` | Delay before the first wave |
| `timeBetweenWaves` | Cadence; a new wave spawns every N seconds and **loops** until time runs out |
| `spawnInterval` | Drip interval between individual enemies inside a wave |
| `enemiesPerWave` | List of `{ enemy (PoolId), count }` — **every wave** spawns this set |
| `spawnRadius` / `minSpawnDistance` | Ring around the player where enemies appear |
| `maxAliveEnemies` | Hard cap on simultaneously alive enemies (performance) |

**Difficulty scales** across the three levels via larger `enemiesPerWave` counts and shorter
`timeBetweenWaves` / `spawnInterval`.

### Adding a new enemy type to a level
1. Create an **Enemy Pool ID** asset and wire its prefab into the pooler.
2. Open the target `LevelData` and add a row to **Enemies Per Wave**: assign the enemy `PoolId`
   and a count. Every wave will then spawn that many of the new enemy, alongside the existing rows.

---

## Third-Party Assets

Zenject, UniTask, PrimeTween, Joystick Pack and Toony Colors Pro are included under
`Assets/Plugins` and `Assets/JMO Assets`. They are the property of their respective authors and
used here under their original licenses for the purposes of this case study.
