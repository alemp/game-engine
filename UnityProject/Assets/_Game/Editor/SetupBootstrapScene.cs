using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.Game.Editor
{
    /// <summary>
    /// Creates the Bootstrap scene with GameBootstrap and GameHUD.
    /// Menu: Tools → Engine → Setup Bootstrap Scene
    /// </summary>
    public static class SetupBootstrapScene
    {
        private const string ScenePath = "Assets/_Game/Scenes/Bootstrap.unity";
        private const string GameHudUxmlPath = "Assets/_Game/UI/GameHUD.uxml";

        [MenuItem("Tools/Engine/Setup Bootstrap Scene")]
        public static void Execute()
        {
            EnsureScenesFolderExists();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameEngine.Game.Bootstrap.GameBootstrap>();

            var hudGo = CreateGameHud(bootstrapGo);
            if (hudGo != null)
            {
                var hud = hudGo.GetComponent<GameEngine.Game.UI.GameHUD>();
                if (hud != null)
                {
                    var bootstrapField = typeof(GameEngine.Game.UI.GameHUD)
                        .GetField("_bootstrap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bootstrapField?.SetValue(hud, bootstrapGo.GetComponent<GameEngine.Game.Bootstrap.GameBootstrap>());
                }
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Engine] Bootstrap scene created at {ScenePath}. Add it to Build Settings if needed.");
        }

        private static void EnsureScenesFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                AssetDatabase.CreateFolder("Assets", "_Game");
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
        }

        private static GameObject CreateGameHud(GameObject bootstrapGo)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameHudUxmlPath);
            if (uxml == null)
            {
                Debug.LogWarning($"[Engine] GameHUD.uxml not found at {GameHudUxmlPath}. Create UI Document manually.");
                return null;
            }

            var hudGo = new GameObject("GameHUD");
            var uiDocument = hudGo.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = uxml;

            var panelSettings = FindOrCreatePanelSettings();
            if (panelSettings != null)
                uiDocument.panelSettings = panelSettings;

            hudGo.AddComponent<GameEngine.Game.UI.GameHUD>();

            return hudGo;
        }

        private static PanelSettings FindOrCreatePanelSettings()
        {
            var guids = AssetDatabase.FindAssets("t:PanelSettings");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/"))
                    return AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game/UI"))
                AssetDatabase.CreateFolder("Assets/_Game", "UI");

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/_Game/UI/DefaultPanelSettings.asset");
            return settings;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var buildScenes = EditorBuildSettings.scenes;
            foreach (var s in buildScenes)
            {
                if (s.path == scenePath)
                    return;
            }

            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            System.Array.Copy(buildScenes, newScenes, buildScenes.Length);
            newScenes[buildScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
