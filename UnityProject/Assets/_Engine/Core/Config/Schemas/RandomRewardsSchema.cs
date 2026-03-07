using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Content/random_rewards.json. Periodic/chance-based rewards (e.g. drones, trucks).
    /// </summary>
    public sealed class RandomRewardsSchema
    {
        [JsonProperty("rewards")]
        public List<RandomRewardEntry> Rewards { get; set; }
    }

    public sealed class RandomRewardEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Approximate seconds between spawns. Actual timing may vary with jitter.
        /// </summary>
        [JsonProperty("intervalSeconds")]
        public double IntervalSeconds { get; set; } = 120;

        /// <summary>
        /// Optional jitter: interval = base ± (random * jitterSeconds). 0 = no jitter.
        /// </summary>
        [JsonProperty("jitterSeconds")]
        public double JitterSeconds { get; set; }

        [JsonProperty("rewards")]
        public List<RewardOption> RewardOptions { get; set; }
    }

    public sealed class RewardOption
    {
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; } = 1;

        /// <summary>
        /// Weight for random selection. Higher = more likely. Default 1.
        /// </summary>
        [JsonProperty("weight")]
        public double Weight { get; set; } = 1;
    }
}
