using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Definitions/ui.json. Screen templates, layout, and navigation.
    /// </summary>
    public sealed class UiSchema
    {
        [JsonProperty("screens")]
        public List<ScreenTemplateEntry> Screens { get; set; }

        [JsonProperty("defaultScreen")]
        public string DefaultScreen { get; set; } = "hud";

        [JsonProperty("navigation")]
        public NavigationSchema Navigation { get; set; }
    }

    public sealed class ScreenTemplateEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Resources path to VisualTreeAsset (no extension). e.g. "Game/UI/GameHUD"
        /// </summary>
        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Localization key for nav button label.
        /// </summary>
        [JsonProperty("labelKey")]
        public string LabelKey { get; set; }
    }

    /// <summary>
    /// Navigation bar config: bottom bar, side HUD, or tabs.
    /// </summary>
    public sealed class NavigationSchema
    {
        /// <summary>
        /// "bottom_bar" | "side" | "tabs"
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = "bottom_bar";

        /// <summary>
        /// "bottom" | "top" | "left" | "right"
        /// </summary>
        [JsonProperty("position")]
        public string Position { get; set; } = "bottom";

        /// <summary>
        /// Screen IDs to show in nav. Order matters.
        /// </summary>
        [JsonProperty("items")]
        public List<string> Items { get; set; }
    }
}
