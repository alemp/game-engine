using UnityEditor;
using UnityEngine;

namespace GameEngine.Game.Editor
{
    /// <summary>
    /// Editor shortcut to reset resources, upgrades, and prestige. Tools → Engine → Reset Progress.
    /// </summary>
    public static class ResetProgressEditor
    {
        [MenuItem("Tools/Engine/Reset Progress", true)]
        private static bool ValidateResetProgress()
        {
            return Application.isPlaying;
        }

        [MenuItem("Tools/Engine/Reset Progress")]
        public static void ResetProgress()
        {
            var bootstrap = Object.FindFirstObjectByType<GameEngine.Game.Bootstrap.GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("[Reset Progress] No GameBootstrap found. Start Play mode first.");
                return;
            }

            bootstrap.ResetProgress();
            Debug.Log("[Reset Progress] All progress reset: resources, upgrades, prestige, tier, artifacts, quests, events, multipliers. Save file deleted.");
        }
    }
}
