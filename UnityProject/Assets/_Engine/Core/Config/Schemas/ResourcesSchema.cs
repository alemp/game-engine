using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    public sealed class ResourcesSchema
    {
        [JsonProperty("resources")]
        public List<ResourceEntry> Resources { get; set; }
    }

    public sealed class ResourceEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayKey")]
        public string DisplayKey { get; set; }

        [JsonProperty("initialAmount")]
        public double InitialAmount { get; set; } = 1;
    }
}
