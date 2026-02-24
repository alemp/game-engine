using UnityEditor;
using UnityEngine;

namespace GameEngine.Game.Editor
{
    /// <summary>
    /// Config hot reload: detects changes to game config files and reloads at runtime.
    /// Menu: Tools → Engine → Reload Config (manual trigger).
    /// </summary>
    public static class ConfigHotReload
    {
        private const string GamesPath = "Assets/_Games";

        [MenuItem("Tools/Engine/Reload Config", true)]
        private static bool ValidateReloadConfig()
        {
            return Application.isPlaying;
        }

        [MenuItem("Tools/Engine/Reload Config")]
        public static void ReloadConfig()
        {
            var bootstrap = Object.FindFirstObjectByType<GameEngine.Game.Bootstrap.GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("[Config Hot Reload] No GameBootstrap found in scene. Start Play mode first.");
                return;
            }

            bootstrap.ReloadConfig();
        }
    }

    /// <summary>
    /// Listens for config file changes and triggers hot reload when in Play mode.
    /// </summary>
    public sealed class ConfigHotReloadProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!Application.isPlaying)
                return;

            foreach (var path in importedAssets)
            {
                if (!IsGameConfigPath(path))
                    continue;

                var bootstrap = Object.FindFirstObjectByType<GameEngine.Game.Bootstrap.GameBootstrap>();
                if (bootstrap != null)
                {
                    bootstrap.ReloadConfig();
                    break;
                }
            }
        }

        private static bool IsGameConfigPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(GamesPath))
                return false;

            if (!assetPath.EndsWith(".json"))
                return false;

            return assetPath.Contains("/Definitions/") ||
                   assetPath.Contains("/Content/") ||
                   assetPath.Contains("/Localization/");
        }
    }
}
