using GameEngine.Core.Config.Schemas;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.UI.Theme
{
    /// <summary>
    /// Applies theme from theme.json to a UI root at runtime via direct style properties.
    /// Unity's IStyle does not support SetProperty for USS variables, so we apply to root and known classes.
    /// </summary>
    public static class ThemeApplier
    {
        private const string ResourceDisplayClass = "resource-display";
        private const string ResourceDisplayLabelClass = "resource-display__label";
        private const string ResourceDisplayValueClass = "resource-display__value";

        /// <summary>
        /// Applies theme to the root and descendant elements using direct style properties.
        /// </summary>
        public static void Apply(VisualElement root, ThemeSchema theme)
        {
            if (root == null || theme == null)
                return;

            if (theme.Colors != null)
            {
                if (TryParseColor(theme.Colors.Background, out var bg))
                    root.style.backgroundColor = bg;
            }

            if (theme.Spacing != null)
            {
                root.Query().Class(ResourceDisplayLabelClass).ForEach(el =>
                {
                    el.style.marginRight = theme.Spacing.Sm;
                });
            }

            if (theme.Colors != null && theme.Typography != null)
            {
                root.Query().Class(ResourceDisplayLabelClass).ForEach(el =>
                {
                    if (TryParseColor(theme.Colors.Text, out var c))
                        el.style.color = c;
                    el.style.fontSize = theme.Typography.Body;
                });
                root.Query().Class(ResourceDisplayValueClass).ForEach(el =>
                {
                    if (TryParseColor(theme.Colors.Accent, out var c))
                        el.style.color = c;
                    el.style.fontSize = theme.Typography.Numbers;
                });
            }
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = Color.clear;
            if (string.IsNullOrEmpty(hex))
                return false;
            if (hex.StartsWith("#") && (hex.Length == 7 || hex.Length == 9))
            {
                if (ColorUtility.TryParseHtmlString(hex, out color))
                    return true;
            }
            return false;
        }
    }
}
