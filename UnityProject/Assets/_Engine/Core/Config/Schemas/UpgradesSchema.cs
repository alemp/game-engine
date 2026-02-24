using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    public sealed class UpgradesSchema
    {
        [JsonProperty("upgrades")]
        public List<UpgradeEntry> Upgrades { get; set; }
    }

    public sealed class UpgradeEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayKey")]
        public string DisplayKey { get; set; }

        [JsonProperty("targetProductionId")]
        public string TargetProductionId { get; set; }

        [JsonProperty("effectType")]
        public string EffectType { get; set; } = "multiplier";

        [JsonProperty("effectValue")]
        public double EffectValue { get; set; } = 1;

        [JsonProperty("costResourceId")]
        public string CostResourceId { get; set; }

        [JsonProperty("costAmount")]
        public double CostAmount { get; set; }

        [JsonProperty("costPerLevel")]
        public double CostPerLevel { get; set; }

        [JsonProperty("maxLevel")]
        public int MaxLevel { get; set; } = 1;

        [JsonProperty("unlockCondition")]
        public UnlockConditionSchema UnlockCondition { get; set; }
    }

    public sealed class UnlockConditionSchema
    {
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("minAmount")]
        public double MinAmount { get; set; }
    }
}
