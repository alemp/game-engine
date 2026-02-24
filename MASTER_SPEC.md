# 🎮 Game Engine Idle – Master Specification

This document is the **official specification** for the modular mobile game engine. For project setup and build instructions, see [README.md](README.md).

---

## 1. Project Overview

This document defines the **official project foundation** for creating a **modular mobile game engine**, initially focused on **Idle Games**, but architected to support **multiple game styles** in the future (runner, puzzle, match-3, etc.) **through configuration only**.

The document also serves as the **source of truth** for:

* Engine development
* Creation of new games
* Interactions with AI agents (content generation, code, design, balancing)

---

## 2. Main Objectives

* Create **a single reusable engine**
* Publish **multiple games** on Android and iOS using the same foundation
* Minimize game-specific code
* Maximize use of **configuration (data-driven)**
* Ensure each game has **its own visual identity and mechanics**
* Enable **production scale** with AI assistance

---

## 3. Platforms and Scope

* 📱 Android
* 📱 iOS
* 🎯 Mobile-first
* 🎮 Initially 2D

### 3.1 Language and Documentation

* **Development and documentation** must be in **English**
* **Games** will support **multiple translations** (localization)
* The **`/docs/`** folder is the **default location** for all project documentation (architecture, API, implementation notes, etc.)

---

## 4. Engine Core Concept

The engine is composed of **three main layers**:

### 4.1 Core Engine (immutable)

Generic layer, independent of genre.

Responsibilities:

* Scene and screen runtime
* Event system (Event Bus)
* Persistence (Save/Load, Offline Progress)
* Generic economy (resources, currencies, inventory)
* Scheduler (timers, ticks, offline)
* Configuration system (load + hot reload)
* Data validation
* Generic UI Framework
* Audio / VFX
* Mobile integrations (Ads, IAP, Analytics, Consent)

---

### 4.2 Game Modules (pluggable)

Modules enabled/disabled by configuration.

Examples:

* IdleModule
* UpgradeModule
* PrestigeModule
* QuestModule
* EventModule (temporary events)
* MinigameModule

In the future:

* RunnerModule
* PuzzleModule
* Match3Module

Each module:

* Does not depend directly on scenes
* Communicates via events and data
* Can be removed without breaking the Core

---

### 4.3 Game Definition (per game)

Each game is defined by **configuration + assets**.

A new game **does not require new code**, only:

* Configuration
* Visual theme
* Content

---

## 5. Recommended Folder Structure

```
/games/<game_id>/
  game.json        # active modules, global rules
  theme.json       # design tokens
  ui.json          # screen layouts and templates
  economy.json     # currencies, curves, progression
  content/
    resources.json
    production.json
    upgrades.json
    quests.json
  assets/
    icons/
    sfx/
    music/
    fonts/
```

---

## 6. Idle Game – Functional Requirements (V1)

### 6.1 Idle Economy

* Generation per tick
* Offline progress
* Production chains (input → output)
* Multipliers
* Configurable exponential curves

### 6.2 Upgrades

* Linear upgrades
* Multiplicative upgrades
* Conditional upgrades
* Progressive unlocks

### 6.3 Prestige

* Partial reset
* Special currency
* Permanent boost
* Configurable formula

---

## 7. Differentiation Between Games

Each game must differ on **at least two levels**:

### 7.1 Visual Theme (Theme Tokens)

Defined in `theme.json`:

* Color palette
* Typography
* Borders, shadows, spacing
* Animation speed

### 7.2 Layout and UI

Defined in `ui.json`:

* Screen templates
* HUD positions
* Navigation (bottom bar, side HUD, tabs)
* Visual density

### 7.3 Mechanical Loop

Configuration defines:

* Type of idle
* Restrictions
* Events
* Optional minigames

---

## 8. Design Tokens – Conceptual Example

```
colors.primary
colors.secondary
colors.background
colors.text

radii.button
radii.card

spacing.xs / sm / md / lg

typography.h1
typography.body
typography.numbers
```

---

## 9. AI Usage in the Project

### 9.1 AI CAN generate

* Content (levels, upgrades, production)
* Visual themes (tokens, palettes)
* Text and dialogues
* Initial balancing
* Code templates

### 9.2 AI should NOT decide alone

* Final economy
* Monetization curves
* Critical progression rules

### 9.3 AI agent constraints

AI agents must **never**:

* Mock or stub implementations when real code is expected
* Use deceptive or placeholder code
* Leave TODO comments as a substitute for actual implementation

---

## 10. AI Pipeline

1. AI generates content/configuration
2. Validator checks consistency
3. Simulator runs progression (1h / 24h / 7d)
4. Manual adjustments
5. Automatic build

---

## 11. Automatic Validation (Mandatory)

* Non-existent references
* Broken curves
* Production overflow
* Regressive costs
* Unreachable content

---

## 12. Key Metrics (KPIs)

* Retention D1 / D7 / D30
* DAU / MAU
* ARPDAU
* Average session time
* IAP conversion

Minimum criteria to scale a game:

* D1 ≥ 35%
* D7 ≥ 12%
* Crash-free ≥ 99.5%

---

## 13. Monetization Strategy

* Ads: rewarded + interstitial
* IAP: ad removal, boosters, packs
* Economy prepared for whales (1–3%)

---

## 14. Scaling Strategy

* Few active games (3–8)
* Continuous iteration
* Temporary events
* A/B testing via remote config
* Weak games are discontinued quickly

---

## 15. Initial Roadmap

### Phase 1 – Foundation

* Core Engine
* IdleModule
* Config Loader + Validator
* Theme System

### Phase 2 – Production

* UI Templates
* Monetization
* Analytics
* Economy simulator

### Phase 3 – Scale

* AI integrated into pipeline
* Multiple games
* New genre modules

---

## 16. Guiding Principles

* Config > Code
* Decoupled modules
* Data always validated
* Engine serves the business
* Scale with low risk

---

## 17. Base Prompt for AI Agents

> "You are helping develop a modular mobile game engine focused on Idle Games. Generate only content or code compatible with a data-driven, modular, validatable, and configuration-oriented architecture. Never assume hardcoded logic. Never mock, stub, or use placeholder code when real implementation is expected. Never leave TODO comments instead of delivering working code."

---

## 18. Repository Template (Unity)

This repository should be used as the **official template** for all projects based on the engine, using **Unity** as the runtime/editor.

> Philosophy: the "engine" is a **framework** within Unity: **Core (immutable) + Modules (pluggable) + Games (configs + assets)**.

---

### 18.1 Recommended Structure (Unity)

The structure below is designed to:

* separate **Runtime vs Editor**
* facilitate **Addressables**
* allow **UPM Package** (engine as package) *or* engine in the repo itself

```
/<repo-root>/
├─ README.md
├─ MASTER_SPEC.md
├─ LICENSE
├─ .gitignore
├─ .editorconfig
│
├─ /docs/                                 # Default folder for all documentation
│
├─ /UnityProject/                         # Unity Project
│   ├─ Assets/
│   │   ├─ _Game/                         # Active game (bootstrap + Game Definition selection)
│   │   │   ├─ Bootstrap/
│   │   │   ├─ Resources/
│   │   │   └─ Scenes/
│   │   │
│   │   ├─ _Engine/                       # Engine (Runtime)
│   │   │   ├─ Core/
│   │   │   │   ├─ EventBus/
│   │   │   │   ├─ Scheduler/
│   │   │   │   ├─ SaveSystem/
│   │   │   │   ├─ Config/
│   │   │   │   │   ├─ Loaders/
│   │   │   │   │   ├─ Schemas/
│   │   │   │   │   └─ Validation/
│   │   │   │   ├─ Economy/
│   │   │   │   └─ Diagnostics/
│   │   │   │
│   │   │   ├─ UI/
│   │   │   │   ├─ Theme/
│   │   │   │   ├─ Components/
│   │   │   │   ├─ Templates/
│   │   │   │   └─ Binding/
│   │   │   │
│   │   │   ├─ Services/                  # Mobile services (wrappers)
│   │   │   │   ├─ Ads/
│   │   │   │   ├─ IAP/
│   │   │   │   ├─ Analytics/
│   │   │   │   ├─ RemoteConfig/
│   │   │   │   └─ Consent/
│   │   │   │
│   │   │   └─ Audio/
│   │   │       ├─ SFX/
│   │   │       └─ Music/
│   │   │
│   │   ├─ _Modules/                      # Pluggable modules (Runtime)
│   │   │   ├─ Idle/
│   │   │   ├─ Upgrades/
│   │   │   ├─ Prestige/
│   │   │   ├─ Quests/
│   │   │   └─ Events/
│   │   │
│   │   ├─ _Games/                        # Game definitions (configs + assets)
│   │   │   ├─ SampleIdleGame/
│   │   │   │   ├─ Definitions/           # ScriptableObjects/JSON
│   │   │   │   ├─ Content/               # tables: resources, production, upgrades...
│   │   │   │   ├─ UI/                    # layouts/templating
│   │   │   │   ├─ Localization/         # translation files: <locale>.json (e.g. en.json, pt-BR.json)
│   │   │   │   └─ Art/                   # sprites, fonts, audio
│   │   │   └─ README.md
│   │   │
│   │   └─ _ThirdParty/                   # if needed
│   │
│   ├─ Packages/
│   │   ├─ manifest.json
│   │   └─ packages-lock.json
│   │
│   ├─ ProjectSettings/
│   └─ UserSettings/
│
├─ /Tools/                                # Tools outside Unity (optional)
│   ├─ Validator/
│   ├─ Simulator/
│   └─ BuildScripts/
│
└─ /AI/
    ├─ prompts/
    └─ rules.md
```

---

### 18.2 Assembly Definitions (asmdef) – Mandatory

Create `asmdef` files to isolate dependencies and speed up compilation:

* `_Engine.Core.asmdef`
* `_Engine.UI.asmdef`
* `_Engine.Services.asmdef`
* `_Modules.Idle.asmdef` etc.
* `_Game.Bootstrap.asmdef`

Rule: **/Editor** always in a separate assembly:

* `_Engine.Core.Editor.asmdef`
* `_Modules.Idle.Editor.asmdef`

---

### 18.3 Addressables (recommended)

Use Addressables for:

* assets per game (skins, audio, fonts)
* loading content by `game_id`

Strategy:

* Addressables group per game: `Game/<game_id>/...`
* Labels: `theme`, `ui`, `audio`, `content`

---

### 18.4 Configuration Format: ScriptableObjects + (optional) JSON

Unity recommendation:

* **ScriptableObjects** as the main format (easy to edit, editor validation)
* **JSON** optional for bulk generation by AI, imported into ScriptableObjects via tool

The generic JSON structure (game.json, theme.json, economy.json, etc.) is the **logical model** that maps to ScriptableObjects in Unity.

Pattern:

* `GameDefinitionSO` (game manifest)
* `ThemeTokensSO`
* `UILayoutSO`
* `EconomyConfigSO`
* `ContentTableSO` (Upgrades, Production, Resources, Quests)

---

### 18.5 Bootstrapping (how to select a game)

At runtime:

* a single scene `Bootstrap.unity`
* `GameLoader` loads `GameDefinitionSO` (by build config / remote config / debug menu)
* instantiates modules according to `featureFlags`

**Production**: Build config is the primary method for game selection.

---

### 18.6 Pattern for Creating a New Game

Checklist:

1. Duplicate `/Assets/_Games/SampleIdleGame` → `/Assets/_Games/<GameId>`
2. Create a new `GameDefinitionSO`
3. Create exclusive `ThemeTokensSO` and `UILayoutSO`
4. Generate content (AI) → import → validate
5. Run Validator + Simulator
6. Adjust Addressables groups/labels
7. Build Android/iOS

---

### 18.7 AI Integration (Unity-first)

AI agents must:

* Read `MASTER_SPEC.md`
* Generate content in **importable** format (JSON/CSV) inside `/AI/output/` *or* directly as `.asset` via internal tools
* Never modify `_Engine/Core` without explicit instruction

---

### 18.8 Editor Folders and Internal Tools (highly recommended)

Create Unity tools for:

* Import JSON/CSV → ScriptableObjects
* Validate configs (menu `Tools/Engine/Validate`)
* Simulate progression (menu `Tools/Engine/Simulate`) with reports
* Generate UI theme previews

---

### 18.9 Suggested Unity Packages (optional)

* Addressables
* Unity IAP
* Unity Analytics / equivalent services
* TextMeshPro
* (If using) Localization

---

## 19. Implementation Decisions

Resolved technical decisions for implementation:

| Area | Decision |
|------|----------|
| **Unity** | Unity 6 (6000) when stable |
| **C# / .NET** | .NET Standard 2.1 |
| **UI** | UI Toolkit |
| **Config model** | Generic JSON structure maps to ScriptableObjects |
| **Validation** | JSON schemas |
| **Config hot reload** | Editor-only (during development) |
| **Event Bus** | Asynchronous, typed payloads |
| **Offline progress** | Max time cap configurable per game |
| **Tick rate** | Configurable per game |
| **Production chains** | Multiple inputs → one output |
| **Number precision** | Custom BigNumber type |
| **Prestige reset** | What is preserved: configurable per game |
| **Prestige currency** | Single per game |
| **Build model** | Single app, multiple games |
| **Game selection** | Build config (primary for production) |
| **Localization** | JSON files in `_Games/<GameId>/Localization/<locale>.json` (e.g. `en.json`, `pt-BR.json`); English as fallback |
| **Validator / Simulator** | Standalone CLI tools |
| **Phase 1 approach** | Start small, grow every iteration |
| **SampleIdleGame** | Complete reference game |

---

📌 **This document is the official project foundation and must be respected by humans and AI agents.**

---

📦 **This repository should be used as the official TEMPLATE for all Unity projects based on the engine.**
