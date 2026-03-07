# Getting Started

## Opening the Project in Unity

1. **Option A – New project from template**
   - Create a new Unity 6 project (or 2022.3 LTS) via Unity Hub
   - Copy the contents of `UnityProject/Assets` into your project's `Assets` folder
   - Copy `UnityProject/Packages/manifest.json` over your project's `Packages/manifest.json` (or merge dependencies)
   - Add a scene with a GameObject that has the `GameBootstrap` component

2. **Option B – Open existing folder**
   - In Unity Hub: `Add` → `Add project from disk`
   - Select the `UnityProject` folder
   - Unity will create `ProjectSettings` if missing
   - If Unity reports an error, use Option A or create a new project and copy our files

## Creating the Bootstrap Scene

**Recommended:** Use **Tools** → **Engine** → **Setup Bootstrap Scene** to create the scene automatically. See [docs/SETUP_UNITY.md](SETUP_UNITY.md) for details.

**Manual:**
1. Create a new scene: `File` → `New Scene`
2. Create an empty GameObject: `GameObject` → `Create Empty` → name it `GameBootstrap`
3. In the Inspector, click `Add Component` and add `GameBootstrap` (from `GameEngine.Game.Bootstrap`)
4. Save the scene as `Bootstrap.unity` in `Assets/_Game/Scenes/`
5. Add a UI Document with `GameHUD.uxml` and `GameHUD` component (see [SETUP_UNITY.md](SETUP_UNITY.md))
6. Set it as the default scene in `File` → `Build Settings` → `Add Open Scenes`

## Game Config Loading

- **Editor**: Config loads from `Assets/_Games/<gameId>/`
- **Build**: Config loads from `StreamingAssets/Game/<gameId>/`
- `StreamingAssets/Game/SampleIdleGame/` is pre-populated for builds
- To add a new game, copy config to both `_Games/` and `StreamingAssets/Game/`
- Config files: game.json, resources.json, production.json, upgrades.json, prestige.json, quests.json, events.json, random_rewards.json, tiers.json, artifacts.json
- For resource/upgrade icons: place PNGs in `_Games/<gameId>/Art/icons/` and mirror to `Resources/Game/<gameId>/Art/icons/` for runtime loading

## Phase 1 – What’s Implemented

- **EventBus** – Async event bus with typed payloads
- **Scheduler** – Tick-based timing (configurable per game)
- **BigNumber** – Arbitrary-precision numbers for idle economy
- **ConfigLoader** – Load JSON config from disk
- **GameLoader** – Deserialize game.json, resources.json, production.json
- **ConfigValidator** – Validation for game config
- **SaveSystem** – Persist resources and scheduler state to JSON
- **Offline progress** – Apply capped ticks on app resume (maxOfflineSeconds per game)
- **IdleModule** – Production chains (multiple inputs → one output, or generators with no input)
- **ThemeTokens** – ScriptableObject for design tokens
- **Theme loading** – theme.json from Definitions/, applied to UI at runtime via USS variables
- **GameHUD** – UI Toolkit HUD (resources, upgrades, actions, artifacts, quests)
- **SampleIdleGame** – Full JSON config and content

## CLI Tools

- **Validator** – Validate game.json: `dotnet run --project Tools/Validator -- <path-to-game-folder>`
- **Simulator** – Run economy progression: `dotnet run --project Tools/Simulator -- <path-to-game-folder> <1h|24h|7d>`

Example: `dotnet run --project Tools/Simulator -- UnityProject/Assets/_Games/SampleIdleGame 24h`

## Game Design Docs

- [AI Idle](AI_IDLE_GAME_DESIGN.md) – Design document for the AI Idle game concept (tokens, agentes, carreira via prestige)

## Next Steps

See [docs/NEXT_STEPS.md](NEXT_STEPS.md) for the full implementation tracker with checkboxes.
