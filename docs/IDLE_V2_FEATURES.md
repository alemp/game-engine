# Idle V2 – Generic Features (Egg Inc.–like)

This document describes extensions to support a full idle game experience similar to popular titles, using **generic terminology** (resources, currencies, tiers) rather than domain-specific terms (eggs, chickens).

---

## 1. Reference Model

The design is inspired by successful idle games but implemented generically:

- **Resources** = any producible/consumable (e.g. gold, souls, energy)
- **Currencies** = resources used for purchases; some reset on prestige, some persist
- **Upgrades** = purchasable improvements; some reset (common), some persist (epic)
- **Manual production** = player-triggered production (tap/click)
- **Random rewards** = periodic or chance-based bonuses (drones, trucks, etc.)

---

## 2. Feature Mapping

| Feature | Generic Concept | Engine Status |
|---------|-----------------|---------------|
| Main resource production | Tick-based production chains | ✅ IdleModule |
| Secondary currency (prestige) | Prestige currency, persists | ✅ PrestigeModule |
| Premium currency | Resource that never resets | 🆕 `persistsOnPrestige` on resources |
| Common upgrades (reset) | Upgrades that reset on prestige | ✅ Default behavior |
| Epic upgrades (permanent) | Upgrades that persist on prestige | 🆕 `persistsOnPrestige` on upgrades |
| Tap/click action | Manual production trigger | 🆕 `trigger: "manual"` on production |
| Random rewards | Periodic/chance rewards | 🆕 RandomRewardModule |
| Tiers (e.g. egg types) | Progression tiers | 📋 Future (TierModule) |
| Artifacts | Collectible bonuses | 📋 Future (ArtifactModule) |

---

## 3. Schema Extensions

### 3.1 Resources (`resources.json`)

```json
{
  "resources": [
    {
      "id": "gold",
      "displayKey": "resource.gold",
      "initialAmount": 1,
      "iconPath": "Art/icons/gold",
      "persistsOnPrestige": false
    },
    {
      "id": "premium",
      "displayKey": "resource.premium",
      "initialAmount": 0,
      "iconPath": "Art/icons/premium",
      "persistsOnPrestige": true
    }
  ]
}
```

- **`persistsOnPrestige`** (default: `false`): When true, resource value is kept across prestige. Use for premium currency (earned via ads, IAP, events).

### 3.2 Upgrades (`upgrades.json`)

```json
{
  "upgrades": [
    {
      "id": "common_upgrade",
      "costResourceId": "gold",
      "persistsOnPrestige": false
    },
    {
      "id": "epic_upgrade",
      "costResourceId": "premium",
      "persistsOnPrestige": true
    }
  ]
}
```

- **`persistsOnPrestige`** (default: `false`): When true, upgrade level is kept across prestige. Typically paired with premium currency cost.

### 3.3 Production (`production.json`)

```json
{
  "productions": [
    {
      "id": "auto_generator",
      "inputs": [],
      "outputId": "gold",
      "outputAmount": 1,
      "trigger": "tick"
    },
    {
      "id": "tap_action",
      "inputs": [],
      "outputId": "gold",
      "outputAmount": 0.1,
      "trigger": "manual"
    }
  ]
}
```

- **`trigger`** (default: `"tick"`): `"tick"` = runs every scheduler tick. `"manual"` = runs only when `IdleModule.TriggerManualProduction(id)` is called (e.g. from UI tap).

### 3.4 Random Rewards (`random_rewards.json`)

```json
{
  "rewards": [
    {
      "id": "drone",
      "intervalSeconds": 120,
      "rewards": [
        { "resourceId": "gold", "amount": 10, "weight": 70 },
        { "resourceId": "premium", "amount": 1, "weight": 30 }
      ]
    }
  ]
}
```

- **`intervalSeconds`**: Approximate time between spawns.
- **`rewards`**: Pool of possible rewards; one chosen by weight when triggered.
- **`amount`**: Can be fixed or scaled by current economy (configurable).

---

## 4. Implementation Checklist

### Phase 2E – Idle V2 Extensions

- [x] **Resources**: `persistsOnPrestige` – PrestigeModule keeps these on reset
- [x] **Upgrades**: `persistsOnPrestige` – PrestigeModule preserves levels for these
- [x] **Production**: `trigger: "manual"` – IdleModule.TriggerManualProduction(productionId)
- [x] **RandomRewardModule**: New module, config from `random_rewards.json`
- [x] **GameLoader**: Load random_rewards.json, expose to bootstrap
- [ ] **UI**: Tap button bound to manual production; random reward notification (optional)

---

## 5. TierModule (Progression Tiers)

Multiple progression tiers: reach a threshold to ascend to the next tier. Higher tier = higher production multiplier. Ascending resets resources and upgrades (except persisted), similar to prestige.

### 5.1 Schema (`tiers.json`)

```json
{
  "tiers": [
    {
      "id": "tier_1",
      "displayKey": "tier.basic",
      "productionMultiplier": 1.0,
      "unlockResourceId": null,
      "unlockMinAmount": 0
    },
    {
      "id": "tier_2",
      "displayKey": "tier.advanced",
      "productionMultiplier": 10.0,
      "unlockResourceId": "gold",
      "unlockMinAmount": 1000
    }
  ]
}
```

- **`productionMultiplier`**: Multiplier applied to all productions when in this tier.
- **`unlockResourceId`** / **`unlockMinAmount`**: Condition to ascend from previous tier. First tier has null/0.

### 5.2 Behavior

- Player starts at tier 1. `TryAscend()` checks if current resources meet next tier's unlock condition.
- On ascend: reset resources (keep persisted), reset upgrades (keep persisted), reset scheduler. Increment tier level. Apply new tier's production multiplier.

---

## 6. ArtifactModule (Collectible Bonuses)

Collectible items that provide passive bonuses (e.g. production multiplier). Artifacts are acquired via quests, events, random rewards, or IAP.

### 6.1 Schema (`artifacts.json`)

```json
{
  "artifacts": [
    {
      "id": "artifact_speed",
      "displayKey": "artifact.speed",
      "iconPath": "Art/icons/artifact_speed",
      "effectType": "production_multiplier",
      "effectValue": 1.1
    }
  ]
}
```

- **`effectType`**: `"production_multiplier"` = multiplies all production output. (Extensible for future types.)
- **`effectValue`**: Multiplier value (1.1 = +10%).

### 6.2 Behavior

- Artifacts are collected via `CollectArtifact(artifactId)`. Collected artifacts persist in save.
- Combined effect: product of all collected artifacts' effectValues for the same effectType.
- ArtifactModule calls `IdleModule.SetArtifactMultiplier(combined)` when artifacts change.

---

## 7. Implementation Status

### TierModule
- [x] TiersSchema, tiers.json
- [x] TierModule (ascend, persisted resources/upgrades)
- [x] IdleModule.SetTierMultiplier
- [x] Save/Load, GameBootstrap

### ArtifactModule
- [x] ArtifactsSchema, artifacts.json
- [x] ArtifactModule (CollectArtifact, production_multiplier effect)
- [x] IdleModule.SetArtifactMultiplier
- [x] Save/Load, GameBootstrap

### Integration
- [x] Quest rewards can grant artifacts (rewardArtifactId in QuestEntry)
- [ ] UI: Tier ascend button, Artifact collection display

---

## 8. Future Considerations

- **ContractsModule**: Cooperative goals (requires backend for multiplayer)

---

*Last updated when implementing features.*
