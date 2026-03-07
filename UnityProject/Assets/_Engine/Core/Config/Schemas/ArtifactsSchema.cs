using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Content/artifacts.json. Collectible items with passive bonuses.
    /// </summary>
    public sealed class ArtifactsSchema
    {
        [JsonProperty("artifacts")]
        public List<ArtifactEntry> Artifacts { get; set; }
    }

    public sealed class ArtifactEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayKey")]
        public string DisplayKey { get; set; }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        /// <summary>
        /// Effect type. "production_multiplier" = multiplies all production output.
        /// </summary>
        [JsonProperty("effectType")]
        public string EffectType { get; set; } = "production_multiplier";

        /// <summary>
        /// Effect value (e.g. 1.1 = +10% for production_multiplier).
        /// </summary>
        [JsonProperty("effectValue")]
        public double EffectValue { get; set; } = 1.0;
    }
}
