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
- [x] Config hot reload (editor-only)

### Idle Module

- [x] Production chains (multiple inputs → one output)
- [x] Generators (no input, output only)
- [x] Resource registration and GetResource/AddResource/TrySpend
- [x] Multipliers
- [x] Configurable exponential curves

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

- [x] Screen templates from ui.json
- [x] HUD layout configurable
- [x] Navigation (bottom bar, side HUD, tabs)
- [x] ResourceDisplay bound to multiple resources dynamically
- [x] Upgrade UI (purchase buttons bound to UpgradeModule.TryPurchase)

### Idle Extensions

- [x] UpgradeModule (linear, multiplicative, conditional)
- [x] PrestigeModule (partial reset, special currency, boost)
- [x] QuestModule
- [x] EventModule (temporary events)
- [x] RandomRewardModule (periodic/chance rewards)
- [x] TierModule (progression tiers)
- [x] ArtifactModule (collectible bonuses)

### Monetization

- [ ] Ads (rewarded, interstitial) – service wrapper
- [ ] IAP (ad removal, boosters, packs) – service wrapper

### Analytics

- [ ] Analytics service wrapper
- [ ] Remote Config integration

### Economy Simulator

- [x] Simulator CLI (run progression, output report)
- [x] Editor menu: Tools → Engine → Simulate

---

## UI Redesign (Implementation Strategy)

*Reference: MASTER_SPEC §15.1. Style: Cookie Clicker–like but distinct theme. All configurable per game.*

### Phase A – Foundation

- [x] Extend theme.json (shadows, card styles, animation durations)
- [x] Card-based layout for resources and upgrades
- [x] Section headers (Resources, Upgrades, Actions)
- [x] hud.json: section order, layout options

### Phase B – Icons & Production

- [x] resources.json: add `iconPath` per resource
- [x] upgrades.json: add `iconPath` per upgrade
- [x] ResourceDisplay: support icon, production rate (+X/sec)
- [x] BigNumber formatting (K, M, B, etc.) for display
- [x] Per-game icon folder: `_Games/<id>/Art/icons/`

### Phase C – Module UI

- [x] Prestige: button + prestige currency display in HUD
- [x] Quests: panel with list, progress bars, claim button
- [ ] Events: active event banner + countdown timer
- [x] hud.json: configurable section visibility (resources, upgrades, actions, artifacts, quests)

### Phase D – Polish

- [ ] Purchase feedback (toast or short animation)
- [ ] Micro-animations (resource gain, button press)
- [ ] Safe-area handling for mobile
- [ ] theme.json: animation speed tokens

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

## Phase 2E – Idle V2 Extensions (Generic Egg Inc.–like)

See [docs/IDLE_V2_FEATURES.md](IDLE_V2_FEATURES.md) for full spec.

- [x] Resources: `persistsOnPrestige` (premium currency)
- [x] Upgrades: `persistsOnPrestige` (epic/permanent upgrades)
- [x] Production: `trigger: "manual"` (tap/click action)
- [x] IdleModule.TriggerManualProduction(productionId)
- [x] RandomRewardModule (random_rewards.json)
- [x] UI: Tap button bound to manual production
- [x] hud.json: configurable manual production display
- [x] UI: Tier ascend button, Artifact panel

---

## Idle V1 – Functional Gaps

From MASTER_SPEC §6:

- [x] Offline progress (apply ticks for elapsed time on app resume)
- [x] Upgrades (linear, multiplicative, conditional, progressive unlocks)
- [x] Prestige (partial reset, special currency, permanent boost, configurable formula)
- [x] Localization loading (JSON → runtime strings, fallback to English)
- [x] Multipliers in production
- [x] Configurable exponential curves

---

## Infrastructure

- [x] SaveSystem (persist resources, upgrades, prestige, quests)
- [ ] Diagnostics / logging
- [ ] Assembly definitions for Services (Ads, IAP, Analytics, RemoteConfig, Consent)
- [ ] Audio/SFX service structure

---

*Last updated when implementing features. Check boxes as you complete each item.*
