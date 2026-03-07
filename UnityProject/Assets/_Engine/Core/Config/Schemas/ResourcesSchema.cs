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

        /// <summary>
        /// Path to icon relative to game folder (e.g. "Art/icons/gold").
        /// </summary>
        [JsonProperty("iconPath")]
        public string IconPath { get; set; }

        /// <summary>
        /// When true, resource value is kept across prestige (e.g. premium currency).
        /// </summary>
        [JsonProperty("persistsOnPrestige")]
        public bool PersistsOnPrestige { get; set; }
    }
}
