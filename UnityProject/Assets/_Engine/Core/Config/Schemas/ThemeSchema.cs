using Newtonsoft.Json;

namespace GameEngine.Core.Config.Schemas
{
    /// <summary>
    /// JSON schema for theme.json. Maps to MASTER_SPEC §8 design tokens.
    /// </summary>
    public sealed class ThemeSchema
    {
        [JsonProperty("colors")]
        public ThemeColorsSchema Colors { get; set; }

        [JsonProperty("radii")]
        public ThemeRadiiSchema Radii { get; set; }

        [JsonProperty("spacing")]
        public ThemeSpacingSchema Spacing { get; set; }

        [JsonProperty("typography")]
        public ThemeTypographySchema Typography { get; set; }
    }

    public sealed class ThemeColorsSchema
    {
        [JsonProperty("primary")]
        public string Primary { get; set; } = "#3399ff";

        [JsonProperty("secondary")]
        public string Secondary { get; set; } = "#66cc66";

        [JsonProperty("background")]
        public string Background { get; set; } = "#1a1a1e";

        [JsonProperty("text")]
        public string Text { get; set; } = "#ffffff";

        [JsonProperty("accent")]
        public string Accent { get; set; } = "#ffd700";
    }

    public sealed class ThemeRadiiSchema
    {
        [JsonProperty("button")]
        public float Button { get; set; } = 8f;

        [JsonProperty("card")]
        public float Card { get; set; } = 12f;
    }

    public sealed class ThemeSpacingSchema
    {
        [JsonProperty("xs")]
        public float Xs { get; set; } = 4f;

        [JsonProperty("sm")]
        public float Sm { get; set; } = 8f;

        [JsonProperty("md")]
        public float Md { get; set; } = 16f;

        [JsonProperty("lg")]
        public float Lg { get; set; } = 24f;
    }

    public sealed class ThemeTypographySchema
    {
        [JsonProperty("h1")]
        public int H1 { get; set; } = 32;

        [JsonProperty("body")]
        public int Body { get; set; } = 16;

        [JsonProperty("numbers")]
        public int Numbers { get; set; } = 18;
    }
}
