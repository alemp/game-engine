using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Content/tiers.json. Progression tiers with unlock conditions.
    /// </summary>
    public sealed class TiersSchema
    {
        [JsonProperty("tiers")]
        public List<TierEntry> Tiers { get; set; }
    }

    public sealed class TierEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayKey")]
        public string DisplayKey { get; set; }

        /// <summary>
        /// Production multiplier when in this tier.
        /// </summary>
        [JsonProperty("productionMultiplier")]
        public double ProductionMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Resource to check for unlock. Null for first tier.
        /// </summary>
        [JsonProperty("unlockResourceId")]
        public string UnlockResourceId { get; set; }

        /// <summary>
        /// Minimum amount of unlockResourceId required to ascend from previous tier.
        /// </summary>
        [JsonProperty("unlockMinAmount")]
        public double UnlockMinAmount { get; set; }
    }
}
