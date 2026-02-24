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

1. Create a new scene: `File` → `New Scene`
2. Create an empty GameObject: `GameObject` → `Create Empty` → name it `GameBootstrap`
3. In the Inspector, click `Add Component` and add `GameBootstrap` (from `GameEngine.Game.Bootstrap`)
4. Save the scene as `Bootstrap.unity` in `Assets/_Game/Scenes/`
5. Set it as the default scene in `File` → `Build Settings` → `Add Open Scenes`

## Adding the Game HUD

1. Create a UI Document: `GameObject` → `UI` → `UI Document`
2. In the UI Document component, set **Source Asset** to `Assets/_Game/UI/GameHUD.uxml`
3. Add the `GameHUD` component to the same GameObject
4. Assign the **Bootstrap** reference (drag the GameBootstrap GameObject) or leave empty to auto-find
5. Set **Panel Settings** to a default or create one for your resolution

## Game Config Loading

- **Editor**: Config loads from `Assets/_Games/<gameId>/`
- **Build**: Config loads from `StreamingAssets/Game/<gameId>/`
- `StreamingAssets/Game/SampleIdleGame/` is pre-populated for builds
- To add a new game, copy config to both `_Games/` and `StreamingAssets/Game/`

## Phase 1 – What’s Implemented

- **EventBus** – Async event bus with typed payloads
- **Scheduler** – Tick-based timing (configurable per game)
- **BigNumber** – Arbitrary-precision numbers for idle economy
- **ConfigLoader** – Load JSON config from disk
- **GameLoader** – Deserialize game.json, resources.json, production.json
- **ConfigValidator** – Validation for game config
- **IdleModule** – Production chains (multiple inputs → one output, or generators with no input)
- **ThemeTokens** – ScriptableObject for design tokens
- **GameHUD** – UI Toolkit HUD displaying resource values
- **SampleIdleGame** – Full JSON config and content

## Next Steps

- Implement Save/Load system
- Add offline progress calculation
- Add UpgradeModule
- Add localization loading from JSON
