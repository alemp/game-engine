# Unity Setup Guide

Step-by-step instructions to get the game engine running in Unity.

---

## 1. Open the Project

1. Open **Unity Hub**
2. Click **Add** → **Add project from disk**
3. Select the `UnityProject` folder (inside the game-engine repo)
4. Click **Open**
5. If Unity prompts to upgrade or create ProjectSettings, accept. Unity will initialize the project.

> If Unity reports errors (e.g. missing ProjectSettings), create a new Unity 6 project, then copy the contents of `UnityProject/Assets` into the new project's `Assets` folder.

---

## 2. Setup Bootstrap Scene (Automated)

1. In the Unity menu, go to **Tools** → **Engine** → **Setup Bootstrap Scene**
2. The script will:
   - Create `Assets/_Game/Scenes/Bootstrap.unity`
   - Add a `GameBootstrap` GameObject with the bootstrap component
   - Add a `GameHUD` GameObject with UI Document and GameHUD component
   - Create `DefaultPanelSettings` if none exist
   - Add the scene to Build Settings
3. Press **Play** – Gold should increase every second

---

## 3. Manual Setup (if the menu doesn't work)

### Create the Bootstrap Scene

1. **File** → **New Scene** (Basic 2D or Empty)
2. **GameObject** → **Create Empty** → name it `GameBootstrap`
3. **Add Component** → search `GameBootstrap` (GameEngine.Game.Bootstrap)
4. **File** → **Save As** → `Assets/_Game/Scenes/Bootstrap.unity`

### Add the HUD

1. **GameObject** → **UI** → **UI Document**
2. Rename to `GameHUD`
3. In **UI Document**:
   - **Source Asset**: assign `Assets/_Game/UI/GameHUD.uxml`
   - **Panel Settings**: create one via **Assets** → **Create** → **UI Toolkit** → **Panel Settings Asset**, save as `Assets/_Game/UI/DefaultPanelSettings.asset`, then assign it
4. **Add Component** → `GameHUD` (GameEngine.Game.UI)
5. **Bootstrap** (optional): drag the `GameBootstrap` GameObject here, or leave empty (auto-find)

### Build Settings

1. **File** → **Build Settings**
2. **Add Open Scenes** (with Bootstrap open)
3. Ensure `Bootstrap` is checked in the scene list

---

## 4. Verify

- Press **Play**
- A HUD should show "Gold" with a number increasing every second
- Config loads from `Assets/_Games/SampleIdleGame/` in the Editor

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Setup Bootstrap Scene" menu missing | Ensure the project compiles. Check Console for errors. The script is in `Assets/_Game/Editor/`. |
| UI not visible | Check that Panel Settings is assigned on the UI Document. Ensure the Game view is active (not Scene). |
| Gold stays at 0 | Ensure `GameBootstrap` runs before `GameHUD`. The HUD finds Bootstrap via `FindFirstObjectByType` if not assigned. |
| Config not loading | In Editor, config loads from `Assets/_Games/SampleIdleGame/`. Ensure the folder exists. |
