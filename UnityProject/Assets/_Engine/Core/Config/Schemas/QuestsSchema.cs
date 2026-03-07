using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Content/quests.json.
    /// </summary>
    public sealed class QuestsSchema
    {
        [JsonProperty("quests")]
        public List<QuestEntry> Quests { get; set; }
    }

    public sealed class QuestEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayKey")]
        public string DisplayKey { get; set; }

        /// <summary>
        /// "reach_amount" | "buy_upgrade" | "prestige"
        /// </summary>
        [JsonProperty("objectiveType")]
        public string ObjectiveType { get; set; } = "reach_amount";

        [JsonProperty("targetResourceId")]
        public string TargetResourceId { get; set; }

        [JsonProperty("targetAmount")]
        public double TargetAmount { get; set; }

        [JsonProperty("targetUpgradeId")]
        public string TargetUpgradeId { get; set; }

        [JsonProperty("targetLevel")]
        public int TargetLevel { get; set; } = 1;

        [JsonProperty("rewardResourceId")]
        public string RewardResourceId { get; set; }

        [JsonProperty("rewardAmount")]
        public double RewardAmount { get; set; }

        /// <summary>
        /// Optional artifact to grant on claim. Collected via ArtifactModule.
        /// </summary>
        [JsonProperty("rewardArtifactId")]
        public string RewardArtifactId { get; set; }
    }
}
