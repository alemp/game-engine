using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for Definitions/ui.json. Screen templates and layout.
    /// </summary>
    public sealed class UiSchema
    {
        [JsonProperty("screens")]
        public List<ScreenTemplateEntry> Screens { get; set; }

        [JsonProperty("defaultScreen")]
        public string DefaultScreen { get; set; } = "hud";
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
    }
}
