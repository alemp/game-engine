using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Content/prestige.json. Prestige (partial reset, special currency, boost).
    /// </summary>
    public sealed class PrestigeSchema
    {
        /// <summary>
        /// Resource ID for prestige currency (persists across resets).
        /// </summary>
        [JsonProperty("currencyResourceId")]
        public string CurrencyResourceId { get; set; } = "souls";

        /// <summary>
        /// Formula for prestige currency earned. "sqrt" | "log" | "linear" | "custom".
        /// sqrt: sqrt(sum of source resources). log: log10(sum+1). linear: sum * factor.
        /// </summary>
        [JsonProperty("formula")]
        public string Formula { get; set; } = "sqrt";

        /// <summary>
        /// For linear formula: amount = sum * factor.
        /// </summary>
        [JsonProperty("formulaFactor")]
        public double FormulaFactor { get; set; } = 0.01;

        /// <summary>
        /// Resource IDs to sum for prestige calculation. If null, use all non-prestige resources.
        /// </summary>
        [JsonProperty("sourceResourceIds")]
        public string[] SourceResourceIds { get; set; }

        /// <summary>
        /// Permanent production multiplier per prestige currency. effect = 1 + currency * boostPerUnit.
        /// </summary>
        [JsonProperty("boostPerUnit")]
        public double BoostPerUnit { get; set; } = 0.01;

        /// <summary>
        /// Minimum total resource value (sum) required to prestige.
        /// </summary>
        [JsonProperty("minResourceValue")]
        public double MinResourceValue { get; set; } = 100;
    }
}
