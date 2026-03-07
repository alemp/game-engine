using System;
using System.Collections.Generic;
using System.IO;
using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using Newtonsoft.Json;

namespace GameEngine.Core.Config
{
    /// <summary>
    /// Loads game configuration from JSON.
    /// Returns parsed data for the bootstrap to apply to modules.
    /// </summary>
    public sealed class GameLoader
    {
        private readonly string _basePath;

        public GameLoader(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public GameConfigSchema LoadGameConfig()
        {
            var path = Path.Combine(_basePath, "Definitions", "game.json");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<GameConfigSchema>(json)
                ?? throw new InvalidOperationException("game.json deserialized to null.");
        }

        public ResourcesSchema LoadResources()
        {
            var path = Path.Combine(_basePath, "Content", "resources.json");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ResourcesSchema>(json)
                ?? throw new InvalidOperationException("resources.json deserialized to null.");
        }

        public ProductionSchema LoadProduction()
        {
            var path = Path.Combine(_basePath, "Content", "production.json");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ProductionSchema>(json)
                ?? throw new InvalidOperationException("production.json deserialized to null.");
        }

        /// <summary>
        /// Loads theme from Definitions/theme.json. Returns null if file does not exist.
        /// </summary>
        public ThemeSchema LoadTheme()
        {
            var path = Path.Combine(_basePath, "Definitions", "theme.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ThemeSchema>(json);
        }

        /// <summary>
        /// Returns resource definitions for the bootstrap to register.
        /// </summary>
        public IReadOnlyList<(string Id, BigNumber InitialAmount)> GetResourceDefinitions()
        {
            var schema = LoadResources();
            var list = new List<(string, BigNumber)>();
            foreach (var res in schema.Resources ?? new List<ResourceEntry>())
            {
                list.Add((res.Id, BigNumber.FromDouble(res.InitialAmount)));
            }
            return list;
        }

        /// <summary>
        /// Returns resource id → display key for localization.
        /// </summary>
        public IReadOnlyDictionary<string, string> GetResourceDisplayKeys()
        {
            var schema = LoadResources();
            var dict = new Dictionary<string, string>();
            foreach (var res in schema.Resources ?? new List<ResourceEntry>())
            {
                if (!string.IsNullOrEmpty(res.Id) && !string.IsNullOrEmpty(res.DisplayKey))
                    dict[res.Id] = res.DisplayKey;
            }
            return dict;
        }

        /// <summary>
        /// Returns resource id → icon path (relative to game folder, e.g. "Art/icons/gold").
        /// </summary>
        public IReadOnlyDictionary<string, string> GetResourceIconPaths()
        {
            var schema = LoadResources();
            var dict = new Dictionary<string, string>();
            foreach (var res in schema.Resources ?? new List<ResourceEntry>())
            {
                if (!string.IsNullOrEmpty(res.Id) && !string.IsNullOrEmpty(res.IconPath))
                    dict[res.Id] = res.IconPath;
            }
            return dict;
        }

        /// <summary>
        /// Returns resource IDs that persist across prestige (e.g. premium currency).
        /// </summary>
        public IReadOnlyList<string> GetPersistedResourceIds()
        {
            var schema = LoadResources();
            var list = new List<string>();
            foreach (var res in schema.Resources ?? new List<ResourceEntry>())
            {
                if (!string.IsNullOrEmpty(res.Id) && res.PersistsOnPrestige)
                    list.Add(res.Id);
            }
            return list;
        }

        /// <summary>
        /// Returns upgrade IDs that persist across prestige (epic/permanent upgrades).
        /// </summary>
        public IReadOnlyList<string> GetPersistedUpgradeIds()
        {
            var schema = LoadUpgrades();
            var list = new List<string>();
            foreach (var upg in schema?.Upgrades ?? new List<UpgradeEntry>())
            {
                if (!string.IsNullOrEmpty(upg.Id) && upg.PersistsOnPrestige)
                    list.Add(upg.Id);
            }
            return list;
        }

        /// <summary>
        /// Returns production rules for the bootstrap to add.
        /// </summary>
        public IReadOnlyList<(string Id, IReadOnlyList<(string ResourceId, BigNumber Amount)> Inputs, string OutputId, BigNumber OutputAmount, double Multiplier, string Trigger)> GetProductionRules()
        {
            var schema = LoadProduction();
            var list = new List<(string, IReadOnlyList<(string, BigNumber)>, string, BigNumber, double, string)>();
            foreach (var prod in schema.Productions ?? new List<ProductionEntry>())
            {
                var inputs = new List<(string, BigNumber)>();
                foreach (var input in prod.Inputs ?? new List<ProductionInput>())
                {
                    inputs.Add((input.ResourceId, BigNumber.FromDouble(input.Amount)));
                }
                var multiplier = prod.Multiplier > 0 ? prod.Multiplier : 1.0;
                var id = !string.IsNullOrEmpty(prod.Id) ? prod.Id : prod.OutputId ?? "unknown";
                var trigger = !string.IsNullOrEmpty(prod.Trigger) && prod.Trigger.Equals("manual", StringComparison.OrdinalIgnoreCase) ? "manual" : "tick";
                list.Add((id, inputs, prod.OutputId, BigNumber.FromDouble(prod.OutputAmount), multiplier, trigger));
            }
            return list;
        }

        /// <summary>
        /// Loads UI config from Definitions/ui.json. Returns null if file does not exist.
        /// </summary>
        public UiSchema LoadUi()
        {
            var path = Path.Combine(_basePath, "Definitions", "ui.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<UiSchema>(json);
        }

        /// <summary>
        /// Loads HUD layout from Definitions/hud.json. Returns null if file does not exist.
        /// </summary>
        public HudSchema LoadHud()
        {
            var path = Path.Combine(_basePath, "Definitions", "hud.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<HudSchema>(json);
        }

        /// <summary>
        /// Loads upgrades from Content/upgrades.json. Returns null if file does not exist.
        /// </summary>
        public UpgradesSchema LoadUpgrades()
        {
            var path = Path.Combine(_basePath, "Content", "upgrades.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<UpgradesSchema>(json);
        }

        /// <summary>
        /// Loads prestige config from Content/prestige.json. Returns null if file does not exist.
        /// </summary>
        public PrestigeSchema LoadPrestige()
        {
            var path = Path.Combine(_basePath, "Content", "prestige.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PrestigeSchema>(json);
        }

        /// <summary>
        /// Loads quests from Content/quests.json. Returns null if file does not exist.
        /// </summary>
        public QuestsSchema LoadQuests()
        {
            var path = Path.Combine(_basePath, "Content", "quests.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<QuestsSchema>(json);
        }

        /// <summary>
        /// Loads events from Content/events.json. Returns null if file does not exist.
        /// </summary>
        public EventsSchema LoadEvents()
        {
            var path = Path.Combine(_basePath, "Content", "events.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<EventsSchema>(json);
        }

        /// <summary>
        /// Loads random rewards from Content/random_rewards.json. Returns null if file does not exist.
        /// </summary>
        public RandomRewardsSchema LoadRandomRewards()
        {
            var path = Path.Combine(_basePath, "Content", "random_rewards.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<RandomRewardsSchema>(json);
        }

        /// <summary>
        /// Loads tiers from Content/tiers.json. Returns null if file does not exist.
        /// </summary>
        public TiersSchema LoadTiers()
        {
            var path = Path.Combine(_basePath, "Content", "tiers.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<TiersSchema>(json);
        }

        /// <summary>
        /// Loads artifacts from Content/artifacts.json. Returns null if file does not exist.
        /// </summary>
        public ArtifactsSchema LoadArtifacts()
        {
            var path = Path.Combine(_basePath, "Content", "artifacts.json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ArtifactsSchema>(json);
        }
    }
}
