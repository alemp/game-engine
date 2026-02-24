using System;
using System.IO;
using GameEngine.Core.Config;
using GameEngine.Core.Config.Validation;
using GameEngine.Core.Economy;
using GameEngine.Core.EventBus;
using GameEngine.Core.Scheduler;
using GameEngine.Modules.Idle;
using UnityEngine;

namespace GameEngine.Game.Bootstrap
{
    /// <summary>
    /// Bootstraps the engine: loads config, creates modules, wires dependencies.
    /// Game selection is driven by build config.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string _gameId = "SampleIdleGame";

        private EventBus _eventBus;
        private Scheduler _scheduler;
        private IdleModule _idleModule;
        private GameLoader _gameLoader;

        public IdleModule IdleModule => _idleModule;

        private void Awake()
        {
            _eventBus = new EventBus();
            var configPath = ResolveGameConfigPath(_gameId);
            _gameLoader = new GameLoader(configPath);

            var gameConfig = _gameLoader.LoadGameConfig();
            var validator = new ConfigValidator();
            var validation = validator.Validate(gameConfig);
            if (!validation.IsValid)
            {
                foreach (var err in validation.Errors)
                    Debug.LogError($"[Config] {err}");
                FallbackToHardcodedSetup();
                return;
            }

            var tickInterval = gameConfig.Economy?.TickIntervalSeconds ?? 1.0;
            _scheduler = new Scheduler(tickInterval);
            _idleModule = new IdleModule(_eventBus, _scheduler);

            try
            {
                foreach (var (id, amount) in _gameLoader.GetResourceDefinitions())
                    _idleModule.RegisterResource(id, amount);

                foreach (var (inputs, outputId, outputAmount) in _gameLoader.GetProductionRules())
                {
                    _idleModule.AddProductionRule(new ProductionRule(inputs, outputId, outputAmount));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Config] Failed to load game config: {ex.Message}");
                FallbackToHardcodedSetup();
            }

            _scheduler.Reset();
        }

        private void Update()
        {
            _scheduler.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _idleModule?.Shutdown();
        }

        private static string ResolveGameConfigPath(string gameId)
        {
#if UNITY_EDITOR
            var assetsPath = Path.Combine(Application.dataPath, "_Games", gameId);
            if (Directory.Exists(assetsPath))
                return assetsPath;
#endif
            var streamingPath = Path.Combine(Application.streamingAssetsPath, "Game", gameId);
            if (Directory.Exists(streamingPath))
                return streamingPath;

#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "_Games", gameId);
#else
            return streamingPath;
#endif
        }

        private void FallbackToHardcodedSetup()
        {
            _scheduler ??= new Scheduler(1.0);
            _idleModule ??= new IdleModule(_eventBus, _scheduler);
            _idleModule.RegisterResource("gold", BigNumber.One);
            _idleModule.AddProductionRule(new ProductionRule(
                System.Array.Empty<(string, BigNumber)>(),
                "gold",
                new BigNumber(1, 0)
            ));
        }
    }
}
