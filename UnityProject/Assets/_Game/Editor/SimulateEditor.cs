using System.IO;
using System.Linq;
using GameEngine.Core.Config;
using GameEngine.Core.Economy;
using GameEngine.Core.EventBus;
using GameEngine.Core.Scheduler;
using GameEngine.Modules.Idle;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Game.Editor
{
    /// <summary>
    /// Runs economy simulation from the Editor. Menu: Tools → Engine → Simulate
    /// </summary>
    public static class SimulateEditor
    {
        private const string DefaultGameId = "SampleIdleGame";

        [MenuItem("Tools/Engine/Simulate 1h")]
        public static void Simulate1h()
        {
            RunSimulation(3600);
        }

        [MenuItem("Tools/Engine/Simulate 24h")]
        public static void Simulate24h()
        {
            RunSimulation(86400);
        }

        [MenuItem("Tools/Engine/Simulate 7d")]
        public static void Simulate7d()
        {
            RunSimulation(604800);
        }

        private static void RunSimulation(double durationSeconds)
        {
            var gamePath = ResolveGamePath(DefaultGameId);
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
            {
                Debug.LogError($"[Simulate] Game folder not found for {DefaultGameId}. Check _Games or StreamingAssets.");
                return;
            }

            var definitionsPath = Path.Combine(gamePath, "Definitions", "game.json");
            if (!File.Exists(definitionsPath))
            {
                Debug.LogError($"[Simulate] game.json not found at {definitionsPath}");
                return;
            }

            var loader = new GameLoader(gamePath);
            var gameConfig = loader.LoadGameConfig();
            var tickInterval = gameConfig.Economy?.TickIntervalSeconds ?? 1.0;
            var ticks = (int)(durationSeconds / tickInterval);

            var eventBus = new EventBus();
            var scheduler = new Scheduler(tickInterval);
            var idleModule = new IdleModule(eventBus, scheduler);

            foreach (var (id, amount) in loader.GetResourceDefinitions())
                idleModule.RegisterResource(id, amount);

            foreach (var (id, inputs, outputId, outputAmount, multiplier, trigger) in loader.GetProductionRules())
            {
                idleModule.AddProductionRule(new ProductionRule(id, inputs, outputId, outputAmount, multiplier, trigger));
            }

            idleModule.SimulateTicks(ticks);

            var durationLabel = durationSeconds switch
            {
                3600 => "1h",
                86400 => "24h",
                604800 => "7d",
                _ => $"{durationSeconds}s"
            };

            var log = $"[Simulate] {durationLabel} ({ticks} ticks @ {tickInterval}s/tick)\n" +
                      $"Game: {gameConfig.GameId}\n\nResources:\n";

            foreach (var (id, amount) in idleModule.GetResourceSnapshot().OrderBy(x => x.Key))
            {
                log += $"  {id}: {amount}\n";
            }

            Debug.Log(log.TrimEnd());
        }

        private static string ResolveGamePath(string gameId)
        {
            var assetsPath = Path.Combine(Application.dataPath, "_Games", gameId);
            if (Directory.Exists(assetsPath))
                return assetsPath;

            var streamingPath = Path.Combine(Application.streamingAssetsPath, "Game", gameId);
            if (Directory.Exists(streamingPath))
                return streamingPath;

            return assetsPath;
        }
    }
}
