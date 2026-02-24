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
        /// Returns production rules for the bootstrap to add.
        /// </summary>
        public IReadOnlyList<(IReadOnlyList<(string ResourceId, BigNumber Amount)> Inputs, string OutputId, BigNumber OutputAmount)> GetProductionRules()
        {
            var schema = LoadProduction();
            var list = new List<(IReadOnlyList<(string, BigNumber)>, string, BigNumber)>();
            foreach (var prod in schema.Productions ?? new List<ProductionEntry>())
            {
                var inputs = new List<(string, BigNumber)>();
                foreach (var input in prod.Inputs ?? new List<ProductionInput>())
                {
                    inputs.Add((input.ResourceId, BigNumber.FromDouble(input.Amount)));
                }
                list.Add((inputs, prod.OutputId, BigNumber.FromDouble(prod.OutputAmount)));
            }
            return list;
        }
    }
}
