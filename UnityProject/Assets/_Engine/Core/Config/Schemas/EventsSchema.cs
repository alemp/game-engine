using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Content/events.json. Temporary events with modifiers.
    /// </summary>
    public sealed class EventsSchema
    {
        [JsonProperty("events")]
        public List<EventEntry> Events { get; set; }
    }

    public sealed class EventEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayKey")]
        public string DisplayKey { get; set; }

        /// <summary>
        /// Duration in seconds. 0 = until manually ended.
        /// </summary>
        [JsonProperty("durationSeconds")]
        public double DurationSeconds { get; set; }

        /// <summary>
        /// Production ID -> multiplier. e.g. {"gold_generator": 2} = 2x gold.
        /// </summary>
        [JsonProperty("productionMultipliers")]
        public Dictionary<string, double> ProductionMultipliers { get; set; }
    }
}
