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

        /// <summary>
        /// Optional multiplier applied to output (e.g. 2.0 = 2x). Default 1.0 when omitted.
        /// </summary>
        [JsonProperty("multiplier")]
        public double Multiplier { get; set; } = 1.0;

        /// <summary>
        /// "tick" (default) = runs every scheduler tick. "manual" = runs only when TriggerManualProduction is called.
        /// </summary>
        [JsonProperty("trigger")]
        public string Trigger { get; set; } = "tick";
    }

    public sealed class ProductionInput
    {
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }
    }
}
