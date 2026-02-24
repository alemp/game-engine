using UnityEngine;

namespace GameEngine.UI.Theme
{
    /// <summary>
    /// Design tokens for theming (colors, radii, spacing, typography).
    /// Maps to theme.json / ThemeTokensSO.
    /// </summary>
    [CreateAssetMenu(fileName = "ThemeTokens", menuName = "Game Engine/Theme Tokens")]
    public sealed class ThemeTokens : ScriptableObject
    {
        [Header("Colors")]
        public Color Primary = new(0.2f, 0.6f, 1f);
        public Color Secondary = new(0.4f, 0.8f, 0.4f);
        public Color Background = new(0.1f, 0.1f, 0.12f);
        public Color Text = Color.white;

        [Header("Radii")]
        public float ButtonRadius = 8f;
        public float CardRadius = 12f;

        [Header("Spacing")]
        public float SpacingXs = 4f;
        public float SpacingSm = 8f;
        public float SpacingMd = 16f;
        public float SpacingLg = 24f;

        [Header("Typography")]
        public int H1Size = 32;
        public int BodySize = 16;
        public int NumbersSize = 18;
    }
}
