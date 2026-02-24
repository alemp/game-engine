# Next Steps – Implementation Tracker

Checkboxes track implementation progress. Update this file when completing each item.

---

## Phase 1 – Foundation

### Core Engine

- [x] EventBus (async, typed payloads)
- [x] Scheduler (tick-based, configurable per game)
- [x] BigNumber (arbitrary-precision economy)
- [x] Config Loader (JSON from disk)
- [x] Config Validator (game.json validation)
- [x] GameLoader (deserialize game.json, resources.json, production.json)
- [x] SaveSystem (Save/Load)
- [x] Offline progress calculation (max time cap per game)
- [ ] Config hot reload (editor-only)

### Idle Module

- [x] Production chains (multiple inputs → one output)
- [x] Generators (no input, output only)
- [x] Resource registration and GetResource/AddResource/TrySpend
- [x] Multipliers
- [ ] Configurable exponential curves

### Theme System

- [x] ThemeTokens ScriptableObject (colors, radii, spacing, typography)
- [x] Theme loading from theme.json
- [x] Apply theme to UI at runtime

### Bootstrap & Setup

- [x] GameBootstrap (load config, wire modules)
- [x] Setup Bootstrap Scene (Editor menu: Tools → Engine)
- [x] Bootstrap.unity with GameBootstrap + GameHUD
- [x] DefaultPanelSettings for UI Toolkit

### Tools

- [x] Validator CLI (standalone, game.json validation)
- [x] Simulator CLI (progression 1h / 24h / 7d)

### Sample Game

- [x] SampleIdleGame config (game.json, resources.json, production.json)
- [x] StreamingAssets copy for builds
- [x] GameHUD (display Gold)
- [x] Localization folder structure (en.json)

---

## Phase 2 – Production

### UI Templates

- [ ] Screen templates from ui.json
- [ ] HUD layout configurable
- [ ] Navigation (bottom bar, side HUD, tabs)
- [ ] ResourceDisplay bound to multiple resources dynamically

### Idle Extensions

- [ ] UpgradeModule (linear, multiplicative, conditional)
- [ ] PrestigeModule (partial reset, special currency, boost)
- [ ] QuestModule
- [ ] EventModule (temporary events)

### Monetization

- [ ] Ads (rewarded, interstitial) – service wrapper
- [ ] IAP (ad removal, boosters, packs) – service wrapper

### Analytics

- [ ] Analytics service wrapper
- [ ] Remote Config integration

### Economy Simulator

- [ ] Simulator CLI (run progression, output report)
- [ ] Editor menu: Tools → Engine → Simulate

---

## Phase 3 – Scale

### AI Pipeline

- [ ] AI output folder structure (/AI/output/)
- [ ] JSON/CSV import to ScriptableObjects (Editor tool)
- [ ] Validate + Simulate in CI

### Multi-Game

- [ ] Addressables per game (theme, ui, audio, content)
- [ ] Game selection via build config
- [ ] Multiple games in single app

### New Genre Modules

- [ ] RunnerModule (stub)
- [ ] PuzzleModule (stub)
- [ ] Match3Module (stub)

---

## Idle V1 – Functional Gaps

From MASTER_SPEC §6:

- [x] Offline progress (apply ticks for elapsed time on app resume)
- [ ] Upgrades (linear, multiplicative, conditional, progressive unlocks)
- [ ] Prestige (partial reset, special currency, permanent boost, configurable formula)
- [ ] Localization loading (JSON → runtime strings, fallback to English)
- [x] Multipliers in production
- [ ] Configurable exponential curves

---

## Infrastructure

- [x] SaveSystem (persist resources, upgrades, prestige state)
- [ ] Diagnostics / logging
- [ ] Assembly definitions for Services (Ads, IAP, Analytics, RemoteConfig, Consent)
- [ ] Audio/SFX service structure

---

*Last updated when implementing features. Check boxes as you complete each item.*
