using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.Localization
{
    /// <summary>
    /// Loads and resolves localized strings from JSON. Fallback to English when key or locale missing.
    /// </summary>
    public sealed class LocalizationService
    {
        private const string FallbackLocale = "en";
        private readonly Dictionary<string, string> _strings = new();
        private readonly Dictionary<string, string> _fallbackStrings = new();

        /// <summary>
        /// Loads localization from Localization/&lt;locale&gt;.json. Always loads "en" as fallback.
        /// </summary>
        public void Load(string basePath, string locale)
        {
            _strings.Clear();
            _fallbackStrings.Clear();

            LoadLocale(basePath, FallbackLocale, _fallbackStrings);
            if (locale != FallbackLocale)
            {
                LoadLocale(basePath, locale, _strings);
            }
            else
            {
                foreach (var (k, v) in _fallbackStrings)
                    _strings[k] = v;
            }
        }

        /// <summary>
        /// Returns localized string for key. Falls back to English, then to key itself.
        /// </summary>
        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "—";

            if (_strings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                return value;

            if (_fallbackStrings.TryGetValue(key, out var fallback) && !string.IsNullOrEmpty(fallback))
                return fallback;

            return key;
        }

        private static void LoadLocale(string basePath, string locale, Dictionary<string, string> target)
        {
            var path = System.IO.Path.Combine(basePath, "Localization", $"{locale}.json");
            if (!System.IO.File.Exists(path))
                return;

            var json = System.IO.File.ReadAllText(path);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (dict != null)
            {
                foreach (var (k, v) in dict)
                    target[k] = v;
            }
        }
    }
}
