using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Definitions/hud.json. Configurable HUD layout.
    /// </summary>
    public sealed class HudSchema
    {
        [JsonProperty("resources")]
        public HudResourcesSchema Resources { get; set; }

        [JsonProperty("upgrades")]
        public HudUpgradesSchema Upgrades { get; set; }

        [JsonProperty("layout")]
        public string Layout { get; set; } = "wrap";
    }

    public sealed class HudResourcesSchema
    {
        /// <summary>
        /// Resource IDs in display order. If null/empty, use config default order.
        /// </summary>
        [JsonProperty("order")]
        public List<string> Order { get; set; }
    }

    public sealed class HudUpgradesSchema
    {
        [JsonProperty("visible")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Upgrade IDs in display order. If null/empty, use config default order.
        /// </summary>
        [JsonProperty("order")]
        public List<string> Order { get; set; }
    }
}
