using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GameEngine.Core.Config;
using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Localization;
using GameEngine.Core.Config.Validation;
using GameEngine.Core.Economy;
using GameEngine.Core.EventBus;
using GameEngine.Core.SaveSystem;
using GameEngine.Core.Scheduler;
using GameEngine.Modules.Idle;
using GameEngine.Modules.Upgrades;
using UnityEngine;

namespace GameEngine.Game.Bootstrap
{
    /// <summary>
    /// Bootstraps the engine: loads config, creates modules, wires dependencies.
    /// Game selection is driven by build config.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string _gameId = "SampleIdleGame";

        private EventBus _eventBus;
        private Scheduler _scheduler;
        private IdleModule _idleModule;
        private GameLoader _gameLoader;
        private SaveSystem _saveSystem;
        private GameConfigSchema _gameConfig;
        private ThemeSchema _theme;
        private HudSchema _hudConfig;
        private UiSchema _uiConfig;
        private LocalizationService _localization;
        private IReadOnlyDictionary<string, string> _resourceDisplayKeys;
        private UpgradeModule _upgradeModule;

        public IdleModule IdleModule => _idleModule;
        public UpgradeModule UpgradeModule => _upgradeModule;
        public ThemeSchema Theme => _theme;
        public HudSchema HudConfig => _hudConfig;
        public UiSchema UiConfig => _uiConfig;
        public LocalizationService Localization => _localization;
        public IReadOnlyDictionary<string, string> ResourceDisplayKeys => _resourceDisplayKeys;

        /// <summary>
        /// Fired when config is hot-reloaded (Editor only). UI should refresh.
        /// </summary>
        public event Action ConfigReloaded;

        private void Awake()
        {
            _eventBus = new EventBus();
            var configPath = ResolveGameConfigPath(_gameId);
            _gameLoader = new GameLoader(configPath);
            _saveSystem = new SaveSystem(Application.persistentDataPath);

            var gameConfig = _gameLoader.LoadGameConfig();
            _gameConfig = gameConfig;
            _theme = _gameLoader.LoadTheme();
            _hudConfig = _gameLoader.LoadHud();
            _uiConfig = _gameLoader.LoadUi();
            _localization = new LocalizationService();
            _localization.Load(configPath, GetSystemLocale());
            _resourceDisplayKeys = _gameLoader.GetResourceDisplayKeys();
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

                foreach (var (id, inputs, outputId, outputAmount, multiplier) in _gameLoader.GetProductionRules())
                {
                    _idleModule.AddProductionRule(new ProductionRule(id, inputs, outputId, outputAmount, multiplier));
                }

                _upgradeModule = new UpgradeModule(_idleModule);
                var upgrades = _gameLoader.LoadUpgrades();
                if (upgrades?.Upgrades != null)
                    _upgradeModule.RegisterUpgrades(upgrades.Upgrades);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Config] Failed to load game config: {ex.Message}");
                FallbackToHardcodedSetup();
            }

            var saveData = _saveSystem.Load(_gameId);
            if (saveData != null)
            {
                ApplySave(saveData);
                ApplyOfflineProgress(saveData);
            }
            else
            {
                _scheduler.Reset();
            }
        }

        private void Update()
        {
            _scheduler?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            Save();
            _idleModule?.Shutdown();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void ApplySave(SaveDataSchema saveData)
        {
            var resources = new Dictionary<string, BigNumber>();
            if (saveData.Resources != null)
            {
                foreach (var (id, data) in saveData.Resources)
                    resources[id] = SaveSystem.FromSaveData(data);
            }
            _idleModule.ApplyResources(resources);

            if (saveData.Scheduler != null)
                _scheduler.SetState(saveData.Scheduler.TickCount, saveData.Scheduler.AccumulatedTime);

            if (_upgradeModule != null && saveData.Upgrades != null)
                _upgradeModule.ApplyPurchasedLevels(saveData.Upgrades);
        }

        private void ApplyOfflineProgress(SaveDataSchema saveData)
        {
            if (string.IsNullOrEmpty(saveData.LastSavedUtc))
                return;

            if (!DateTime.TryParse(saveData.LastSavedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastSaved))
                return;

            var elapsed = (DateTime.UtcNow - lastSaved).TotalSeconds;
            var maxOffline = _gameConfig?.Economy?.MaxOfflineSeconds ?? 86400;
            var capped = Math.Min(elapsed, maxOffline);
            var ticks = _scheduler.GetTicksForElapsedSeconds(capped);
            if (ticks > 0)
            {
                _idleModule.SimulateTicks(ticks);
                var tickInterval = _scheduler.TickIntervalSeconds;
                var remainingTime = capped - (ticks * tickInterval);
                var baseTickCount = saveData.Scheduler?.TickCount ?? 0;
                _scheduler.SetState(baseTickCount + ticks, Math.Max(0, remainingTime));
            }
        }

        private void Save()
        {
            if (_idleModule == null || _scheduler == null)
                return;

            var resources = new Dictionary<string, BigNumberSaveData>();
            foreach (var (id, amount) in _idleModule.GetResourceSnapshot())
                resources[id] = SaveSystem.ToSaveData(amount);

            var upgrades = _upgradeModule != null ? _upgradeModule.GetPurchasedLevels() : null;

            var saveData = new SaveDataSchema
            {
                GameId = _gameId,
                Version = 1,
                LastSavedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Scheduler = new SchedulerSaveData
                {
                    TickCount = _scheduler.TickCount,
                    AccumulatedTime = _scheduler.AccumulatedTime
                },
                Resources = resources,
                Upgrades = upgrades != null ? new Dictionary<string, int>(upgrades) : null
            };

            try
            {
                _saveSystem?.Save(saveData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Save] Failed to save: {ex.Message}");
            }
        }

        private static string GetSystemLocale()
        {
            return Application.systemLanguage switch
            {
                SystemLanguage.Portuguese => "pt",
                SystemLanguage.Spanish => "es",
                SystemLanguage.French => "fr",
                SystemLanguage.German => "de",
                SystemLanguage.Japanese => "ja",
                SystemLanguage.Korean => "ko",
                SystemLanguage.Chinese => "zh",
                SystemLanguage.ChineseSimplified => "zh-CN",
                SystemLanguage.ChineseTraditional => "zh-TW",
                SystemLanguage.Russian => "ru",
                _ => "en"
            };
        }

        /// <summary>
        /// Reloads config from disk. Preserves runtime state (resources, upgrades, scheduler).
        /// Editor only. Call when config files change.
        /// </summary>
        public void ReloadConfig()
        {
            if (_gameLoader == null || _idleModule == null)
                return;

            try
            {
                var gameConfig = _gameLoader.LoadGameConfig();
                var validation = new ConfigValidator().Validate(gameConfig);
                if (!validation.IsValid)
                {
                    foreach (var err in validation.Errors)
                        Debug.LogWarning($"[Config Hot Reload] {err}");
                    return;
                }

                _gameConfig = gameConfig;
                _theme = _gameLoader.LoadTheme();
                _hudConfig = _gameLoader.LoadHud();
                _uiConfig = _gameLoader.LoadUi();
                _localization.Load(ResolveGameConfigPath(_gameId), GetSystemLocale());
                _resourceDisplayKeys = _gameLoader.GetResourceDisplayKeys();

                foreach (var (id, amount) in _gameLoader.GetResourceDefinitions())
                    _idleModule.RegisterResourceIfNew(id, amount);

                _idleModule.ClearProductionRules();
                foreach (var (id, inputs, outputId, outputAmount, multiplier) in _gameLoader.GetProductionRules())
                {
                    _idleModule.AddProductionRule(new ProductionRule(id, inputs, outputId, outputAmount, multiplier));
                }

                var upgrades = _gameLoader.LoadUpgrades();
                if (_upgradeModule != null && upgrades?.Upgrades != null)
                {
                    _upgradeModule.RegisterUpgrades(upgrades.Upgrades);
                    _upgradeModule.ApplyEffects();
                }

                ConfigReloaded?.Invoke();
                Debug.Log("[Config Hot Reload] Config reloaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Config Hot Reload] Failed: {ex.Message}");
            }
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
            var configPath = ResolveGameConfigPath(_gameId);
            _localization ??= new LocalizationService();
            _localization.Load(configPath, GetSystemLocale());
            _resourceDisplayKeys ??= new Dictionary<string, string> { ["gold"] = "resource.gold" };

            _scheduler ??= new Scheduler(1.0);
            _idleModule ??= new IdleModule(_eventBus, _scheduler);
            _idleModule.RegisterResource("gold", BigNumber.One);
            _idleModule.AddProductionRule(new ProductionRule(
                "gold_generator",
                System.Array.Empty<(string, BigNumber)>(),
                "gold",
                new BigNumber(1, 0)
            ));
        }
    }
}
