using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    public sealed class ProductionSchema
    {
        [JsonProperty("productions")]
        public List<ProductionEntry> Productions { get; set; }
    }

    public sealed class ProductionEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("inputs")]
        public List<ProductionInput> Inputs { get; set; }

        [JsonProperty("outputId")]
        public string OutputId { get; set; }

        [JsonProperty("outputAmount")]
        public double OutputAmount { get; set; }
    }

    public sealed class ProductionInput
    {
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }
    }
}
