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
using GameEngine.Modules.Prestige;
using GameEngine.Modules.Quests;
using GameEngine.Modules.Events;
using GameEngine.Modules.RandomRewards;
using GameEngine.Modules.Tiers;
using GameEngine.Modules.Artifacts;
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
        private IReadOnlyDictionary<string, string> _resourceIconPaths;
        private UpgradeModule _upgradeModule;
        private PrestigeModule _prestigeModule;
        private QuestModule _questModule;
        private EventModule _eventModule;
        private RandomRewardModule _randomRewardModule;
        private TierModule _tierModule;
        private ArtifactModule _artifactModule;

        public IdleModule IdleModule => _idleModule;
        public double TickIntervalSeconds => _scheduler?.TickIntervalSeconds ?? 1.0;
        public UpgradeModule UpgradeModule => _upgradeModule;
        public PrestigeModule PrestigeModule => _prestigeModule;
        public QuestModule QuestModule => _questModule;
        public EventModule EventModule => _eventModule;
        public TierModule TierModule => _tierModule;
        public ArtifactModule ArtifactModule => _artifactModule;
        public ThemeSchema Theme => _theme;

        /// <summary>
        /// Attempts to prestige. Resets scheduler and applies boost on success.
        /// </summary>
        public bool TryPrestige()
        {
            if (_prestigeModule == null)
                return false;
            return _prestigeModule.TryPrestige(() => _scheduler?.Reset());
        }

        /// <summary>
        /// Attempts to ascend to next tier. Resets resources, upgrades, scheduler on success.
        /// </summary>
        public bool TryAscendTier()
        {
            if (_tierModule == null)
                return false;
            return _tierModule.TryAscend(() => _scheduler?.Reset());
        }

        /// <summary>
        /// Resets all progress: resources, upgrades, prestige, tier, artifacts, quests, events, scheduler.
        /// Deletes the save file so progress does not persist. Editor shortcut: Tools → Engine → Reset Progress.
        /// </summary>
        public void ResetProgress()
        {
            if (_idleModule == null)
                return;

            var initial = new Dictionary<string, BigNumber>();
            foreach (var (id, amount) in _gameLoader.GetResourceDefinitions())
                initial[id] = amount;
            _idleModule.ApplyResources(initial);

            _upgradeModule?.ApplyPurchasedLevels(new Dictionary<string, int>());

            if (_prestigeModule != null)
            {
                _prestigeModule.SetPrestigeCurrency(BigNumber.Zero);
                var currencyId = _prestigeModule.GetCurrencyResourceId();
                if (_idleModule.Resources.ContainsKey(currencyId))
                    _idleModule.ApplyResources(new Dictionary<string, BigNumber> { { currencyId, BigNumber.Zero } });
            }

            _questModule?.SetCompletedQuests(null);
            _eventModule?.EndEvent();
            _tierModule?.SetTierIndex(0);
            _artifactModule?.SetCollectedIds(null);
            _randomRewardModule?.Reset();
            _scheduler?.Reset();

            _saveSystem?.DeleteSave(_gameId);
        }

        public HudSchema HudConfig => _hudConfig;
        public UiSchema UiConfig => _uiConfig;
        public LocalizationService Localization => _localization;
        public IReadOnlyDictionary<string, string> ResourceDisplayKeys => _resourceDisplayKeys;
        public IReadOnlyDictionary<string, string> ResourceIconPaths => _resourceIconPaths;
        public string GameId => _gameId;

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
            _resourceIconPaths = _gameLoader.GetResourceIconPaths();
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

                foreach (var (id, inputs, outputId, outputAmount, multiplier, trigger) in _gameLoader.GetProductionRules())
                {
                    _idleModule.AddProductionRule(new ProductionRule(id, inputs, outputId, outputAmount, multiplier, trigger));
                }

                _upgradeModule = new UpgradeModule(_idleModule);
                var upgrades = _gameLoader.LoadUpgrades();
                if (upgrades?.Upgrades != null)
                    _upgradeModule.RegisterUpgrades(upgrades.Upgrades);

                _prestigeModule = new PrestigeModule(_idleModule, _upgradeModule);
                var prestigeConfig = _gameLoader.LoadPrestige();
                if (prestigeConfig != null)
                {
                    _prestigeModule.Configure(prestigeConfig);
                    _prestigeModule.SetPersistedResourceIds(_gameLoader.GetPersistedResourceIds());
                    _prestigeModule.SetPersistedUpgradeIds(_gameLoader.GetPersistedUpgradeIds());
                    _idleModule.RegisterResourceIfNew(prestigeConfig.CurrencyResourceId, BigNumber.Zero);
                }

                _artifactModule = new ArtifactModule(_idleModule);
                var artifacts = _gameLoader.LoadArtifacts();
                if (artifacts?.Artifacts != null)
                    _artifactModule.RegisterArtifacts(artifacts.Artifacts);

                _questModule = new QuestModule(_idleModule, _upgradeModule, _prestigeModule, _artifactModule);
                var quests = _gameLoader.LoadQuests();
                if (quests?.Quests != null)
                    _questModule.RegisterQuests(quests.Quests);

                _eventModule = new EventModule(_idleModule);
                var events = _gameLoader.LoadEvents();
                if (events?.Events != null)
                    _eventModule.RegisterEvents(events.Events);

                _randomRewardModule = new RandomRewardModule(_idleModule);
                var randomRewards = _gameLoader.LoadRandomRewards();
                if (randomRewards?.Rewards != null)
                    _randomRewardModule.RegisterRewards(randomRewards.Rewards);

                _tierModule = new TierModule(_idleModule, _upgradeModule);
                var tiers = _gameLoader.LoadTiers();
                if (tiers?.Tiers != null && tiers.Tiers.Count > 0)
                {
                    _tierModule.RegisterTiers(tiers.Tiers);
                    _tierModule.SetPersistedResourceIds(_gameLoader.GetPersistedResourceIds());
                    _tierModule.SetPersistedUpgradeIds(_gameLoader.GetPersistedUpgradeIds());
                }
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
            var dt = Time.deltaTime;
            _scheduler?.Tick(dt);
            _eventModule?.Tick(dt);
            _randomRewardModule?.Tick(dt);
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

            if (_prestigeModule != null && saveData.Prestige != null)
            {
                var prestigeAmount = SaveSystem.FromSaveData(saveData.Prestige);
                _prestigeModule.SetPrestigeCurrency(prestigeAmount);
                var currencyId = _prestigeModule.GetCurrencyResourceId();
                if (_idleModule.Resources.ContainsKey(currencyId))
                    _idleModule.ApplyResources(new Dictionary<string, BigNumber> { { currencyId, prestigeAmount } });
            }

            if (_questModule != null && saveData.CompletedQuests != null)
                _questModule.SetCompletedQuests(saveData.CompletedQuests);

            if (_tierModule != null)
                _tierModule.SetTierIndex(saveData.CurrentTier);

            if (_artifactModule != null && saveData.CollectedArtifacts != null)
                _artifactModule.SetCollectedIds(saveData.CollectedArtifacts);
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
            var prestige = _prestigeModule != null ? SaveSystem.ToSaveData(_prestigeModule.GetPrestigeCurrency()) : null;
            var completedQuests = _questModule != null ? new List<string>(_questModule.GetCompletedQuestIds()) : null;
            var currentTier = _tierModule != null ? _tierModule.CurrentTierIndex : 0;
            var collectedArtifacts = _artifactModule != null ? new List<string>(_artifactModule.GetCollectedIds()) : null;

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
                Upgrades = upgrades != null ? new Dictionary<string, int>(upgrades) : null,
                Prestige = prestige,
                CompletedQuests = completedQuests,
                CurrentTier = currentTier,
                CollectedArtifacts = collectedArtifacts
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
                _resourceIconPaths = _gameLoader.GetResourceIconPaths();

                foreach (var (id, amount) in _gameLoader.GetResourceDefinitions())
                    _idleModule.RegisterResourceIfNew(id, amount);

                _idleModule.ClearProductionRules();
                foreach (var (id, inputs, outputId, outputAmount, multiplier, trigger) in _gameLoader.GetProductionRules())
                {
                    _idleModule.AddProductionRule(new ProductionRule(id, inputs, outputId, outputAmount, multiplier, trigger));
                }

                var upgrades = _gameLoader.LoadUpgrades();
                if (_upgradeModule != null && upgrades?.Upgrades != null)
                {
                    _upgradeModule.RegisterUpgrades(upgrades.Upgrades);
                    _upgradeModule.ApplyEffects();
                }

                var prestigeConfig = _gameLoader.LoadPrestige();
                if (_prestigeModule != null && prestigeConfig != null)
                {
                    _prestigeModule.Configure(prestigeConfig);
                    _prestigeModule.SetPersistedResourceIds(_gameLoader.GetPersistedResourceIds());
                    _prestigeModule.SetPersistedUpgradeIds(_gameLoader.GetPersistedUpgradeIds());
                    _idleModule.RegisterResourceIfNew(prestigeConfig.CurrencyResourceId, BigNumber.Zero);
                }

                var quests = _gameLoader.LoadQuests();
                if (_questModule != null && quests?.Quests != null)
                    _questModule.RegisterQuests(quests.Quests);

                var events = _gameLoader.LoadEvents();
                if (_eventModule != null && events?.Events != null)
                    _eventModule.RegisterEvents(events.Events);

                var randomRewards = _gameLoader.LoadRandomRewards();
                if (_randomRewardModule != null && randomRewards?.Rewards != null)
                    _randomRewardModule.RegisterRewards(randomRewards.Rewards);

                var tiers = _gameLoader.LoadTiers();
                if (_tierModule != null && tiers?.Tiers != null && tiers.Tiers.Count > 0)
                {
                    _tierModule.RegisterTiers(tiers.Tiers);
                    _tierModule.SetPersistedResourceIds(_gameLoader.GetPersistedResourceIds());
                    _tierModule.SetPersistedUpgradeIds(_gameLoader.GetPersistedUpgradeIds());
                }

                var artifacts = _gameLoader.LoadArtifacts();
                if (_artifactModule != null && artifacts?.Artifacts != null)
                    _artifactModule.RegisterArtifacts(artifacts.Artifacts);

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
            _resourceIconPaths ??= new Dictionary<string, string>();

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
