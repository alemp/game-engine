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
        private const string GameRootUxmlPath = "Assets/_Game/UI/GameRoot.uxml";
        private const string GameHudUxmlPath = "Assets/_Game/UI/GameHUD.uxml";

        [MenuItem("Tools/Engine/Setup Bootstrap Scene")]
        public static void Execute()
        {
            EnsureScenesFolderExists();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameEngine.Game.Bootstrap.GameBootstrap>();

            var rootGo = CreateGameRoot(bootstrapGo);
            if (rootGo != null)
            {
                var root = rootGo.GetComponent<GameEngine.Game.UI.GameRoot>();
                var hud = rootGo.GetComponent<GameEngine.Game.UI.GameHUD>();
                var bootstrap = bootstrapGo.GetComponent<GameEngine.Game.Bootstrap.GameBootstrap>();
                if (root != null && bootstrap != null)
                {
                    var rootBootstrapField = typeof(GameEngine.Game.UI.GameRoot)
                        .GetField("_bootstrap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    rootBootstrapField?.SetValue(root, bootstrap);
                }
                if (hud != null && bootstrap != null)
                {
                    var hudBootstrapField = typeof(GameEngine.Game.UI.GameHUD)
                        .GetField("_bootstrap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    hudBootstrapField?.SetValue(hud, bootstrap);
                }
                if (root != null && hud != null)
                {
                    var hudField = typeof(GameEngine.Game.UI.GameRoot)
                        .GetField("_gameHud", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    hudField?.SetValue(root, hud);
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

        private static GameObject CreateGameRoot(GameObject bootstrapGo)
        {
            var rootUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameRootUxmlPath);
            if (rootUxml == null)
            {
                var hudUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GameHudUxmlPath);
                if (hudUxml == null)
                {
                    Debug.LogWarning("[Engine] GameRoot.uxml and GameHUD.uxml not found. Create UI Document manually.");
                    return null;
                }
                var hudGo = new GameObject("GameHUD");
                var hudDoc = hudGo.AddComponent<UIDocument>();
                hudDoc.visualTreeAsset = hudUxml;
                var hudPanel = FindOrCreatePanelSettings();
                if (hudPanel != null)
                    hudDoc.panelSettings = hudPanel;
                hudGo.AddComponent<GameEngine.Game.UI.GameHUD>();
                return hudGo;
            }

            var rootGo = new GameObject("GameRoot");
            var rootDoc = rootGo.AddComponent<UIDocument>();
            rootDoc.visualTreeAsset = rootUxml;

            var rootPanel = FindOrCreatePanelSettings();
            if (rootPanel != null)
                rootDoc.panelSettings = rootPanel;

            rootGo.AddComponent<GameEngine.Game.UI.GameRoot>();
            rootGo.AddComponent<GameEngine.Game.UI.GameHUD>();

            return rootGo;
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
