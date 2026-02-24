using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema-compatible structure for game.json.
    /// </summary>
    public sealed class GameConfigSchema
    {
        [JsonProperty("gameId")]
        public string GameId { get; set; }

        [JsonProperty("activeModules")]
        public List<string> ActiveModules { get; set; }

        [JsonProperty("economy")]
        public EconomyConfigSchema Economy { get; set; }
    }

    public sealed class EconomyConfigSchema
    {
        [JsonProperty("tickIntervalSeconds")]
        public double TickIntervalSeconds { get; set; } = 1.0;

        [JsonProperty("maxOfflineSeconds")]
        public double MaxOfflineSeconds { get; set; } = 86400; // 24h default
    }
}
