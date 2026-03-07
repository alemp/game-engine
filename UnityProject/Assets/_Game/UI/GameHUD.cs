using System.Collections.Generic;
using System.Linq;
using GameEngine.Core.Economy;
using GameEngine.Game.Bootstrap;
using GameEngine.Modules.Idle;
using GameEngine.Modules.Upgrades;
using GameEngine.UI.Components;
using GameEngine.UI.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.Game.UI
{
    /// <summary>
    /// HUD that displays idle game resources and upgrade purchase buttons.
    /// </summary>
    public sealed class GameHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GameBootstrap _bootstrap;

        private IdleModule _idleModule;
        private UpgradeModule _upgradeModule;
        private VisualElement _root;
        private VisualElement _hudRoot;
        private VisualElement _resourceContainer;
        private VisualElement _upgradesContainer;
        private VisualElement _actionsContainer;
        private VisualElement _artifactsContainer;
        private VisualElement _sectionResources;
        private VisualElement _sectionUpgrades;
        private VisualElement _sectionActions;
        private VisualElement _sectionArtifacts;
        private bool _resourcesBound;
        private bool _upgradesBound;
        private bool _actionsBound;
        private int _lastArtifactCount = -1;

        private void OnEnable()
        {
            TryBind();
            if (_bootstrap != null)
                _bootstrap.ConfigReloaded += OnConfigReloaded;
        }

        private void OnDisable()
        {
            if (_bootstrap != null)
                _bootstrap.ConfigReloaded -= OnConfigReloaded;
        }

        private void OnConfigReloaded()
        {
            TryBind();
        }

        /// <summary>
        /// Binds to a root element (e.g. when used with GameRoot navigation).
        /// Call when the hud screen is shown.
        /// </summary>
        public void BindToRoot(VisualElement root)
        {
            if (root == null || _bootstrap == null)
                return;

            _idleModule = _bootstrap.IdleModule;
            _upgradeModule = _bootstrap.UpgradeModule;
            if (_idleModule == null)
                return;

            _root = root;
            _hudRoot = _root.Q<VisualElement>("hud-root") ?? _root;
            _resourceContainer = _root.Q<VisualElement>("resource-container");
            _upgradesContainer = _root.Q<VisualElement>("upgrades-container");
            _actionsContainer = _root.Q<VisualElement>("actions-container");
            _artifactsContainer = _root.Q<VisualElement>("artifacts-container");
            _sectionResources = _root.Q<VisualElement>("section-resources");
            _sectionUpgrades = _root.Q<VisualElement>("section-upgrades");
            _sectionActions = _root.Q<VisualElement>("section-actions");
            _sectionArtifacts = _root.Q<VisualElement>("section-artifacts");

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            ApplyHudLayout();
            BindResourceDisplays();
            BindActionButtons();
            BindUpgradeButtons();
            BindArtifacts();
        }

        private void TryBind()
        {
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameBootstrap>();

            if (_bootstrap?.IdleModule == null)
                return;

            _idleModule = _bootstrap.IdleModule;
            _upgradeModule = _bootstrap.UpgradeModule;

            if (GetComponent<GameRoot>() != null)
                return;

            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
                return;

            TryLoadTemplateFromConfig();

            _root = _uiDocument.rootVisualElement;
            if (_root == null)
                return;

            _hudRoot = _root.Q<VisualElement>("hud-root") ?? _root;
            _resourceContainer = _root.Q<VisualElement>("resource-container");
            _upgradesContainer = _root.Q<VisualElement>("upgrades-container");
            _actionsContainer = _root.Q<VisualElement>("actions-container");
            _artifactsContainer = _root.Q<VisualElement>("artifacts-container");
            _sectionResources = _root.Q<VisualElement>("section-resources");
            _sectionUpgrades = _root.Q<VisualElement>("section-upgrades");
            _sectionActions = _root.Q<VisualElement>("section-actions");
            _sectionArtifacts = _root.Q<VisualElement>("section-artifacts");

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            ApplyHudLayout();
            BindResourceDisplays();
            BindActionButtons();
            BindUpgradeButtons();
            BindArtifacts();
        }

        private void TryLoadTemplateFromConfig()
        {
            var template = _bootstrap?.UiConfig?.Screens?.Find(s => s.Id == "hud")?.Template;
            if (string.IsNullOrEmpty(template))
                return;

            var asset = Resources.Load<VisualTreeAsset>(template);
            if (asset == null)
                return;

            _uiDocument.visualTreeAsset = asset;
        }

        private void ApplyHudLayout()
        {
            var hud = _bootstrap?.HudConfig;
            var layout = hud?.Layout ?? "wrap";

            if (_resourceContainer != null)
            {
                _resourceContainer.style.flexDirection = layout == "column" ? FlexDirection.Column : FlexDirection.Row;
                _resourceContainer.style.flexWrap = layout == "wrap" ? Wrap.Wrap : Wrap.NoWrap;
            }

            if (_upgradesContainer != null)
            {
                _upgradesContainer.style.flexDirection = layout == "column" ? FlexDirection.Column : FlexDirection.Row;
                _upgradesContainer.style.flexWrap = layout == "wrap" ? Wrap.Wrap : Wrap.NoWrap;
                if (_sectionUpgrades != null)
                    _sectionUpgrades.style.display = (hud?.Upgrades?.Visible ?? true) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_sectionActions != null)
                _sectionActions.style.display = (hud?.ActionsVisible ?? true) ? DisplayStyle.Flex : DisplayStyle.None;

            if (_sectionArtifacts != null)
                _sectionArtifacts.style.display = (hud?.ArtifactsVisible ?? true) ? DisplayStyle.Flex : DisplayStyle.None;

            ApplySectionHeaders();
            ApplySectionOrder();
        }

        private void ApplySectionHeaders()
        {
            var hud = _bootstrap?.HudConfig;
            var labels = hud?.SectionLabels;
            if (labels == null)
                return;

            if (labels.TryGetValue("resources", out var resKey) && _sectionResources != null)
            {
                var header = _sectionResources.Q<Label>();
                if (header != null)
                    header.text = _bootstrap.Localization?.GetString(resKey) ?? resKey;
            }

            if (labels.TryGetValue("upgrades", out var upgKey) && _sectionUpgrades != null)
            {
                var header = _sectionUpgrades.Q<Label>();
                if (header != null)
                    header.text = _bootstrap.Localization?.GetString(upgKey) ?? upgKey;
            }

            if (labels.TryGetValue("actions", out var actKey) && _sectionActions != null)
            {
                var header = _sectionActions.Q<Label>();
                if (header != null)
                    header.text = _bootstrap.Localization?.GetString(actKey) ?? actKey;
            }

            if (labels.TryGetValue("artifacts", out var artKey) && _sectionArtifacts != null)
            {
                var header = _sectionArtifacts.Q<Label>();
                if (header != null)
                    header.text = _bootstrap.Localization?.GetString(artKey) ?? artKey;
            }
        }

        private void ApplySectionOrder()
        {
            var order = _bootstrap?.HudConfig?.SectionOrder;
            if (order == null || order.Count == 0 || _hudRoot == null)
                return;

            var sections = new Dictionary<string, VisualElement>();
            if (_sectionResources != null) sections["resources"] = _sectionResources;
            if (_sectionUpgrades != null) sections["upgrades"] = _sectionUpgrades;
            if (_sectionActions != null) sections["actions"] = _sectionActions;
            if (_sectionArtifacts != null) sections["artifacts"] = _sectionArtifacts;

            foreach (var s in sections.Values)
                s.RemoveFromHierarchy();

            foreach (var id in order)
            {
                if (sections.TryGetValue(id, out var section))
                    _hudRoot.Add(section);
            }
        }

        private void Update()
        {
            if (_idleModule == null)
            {
                TryBind();
                return;
            }

            UpdateResourceValues();
            UpdateUpgradeButtons();
            UpdateActionButtons();

            var artifactCount = _bootstrap?.ArtifactModule?.GetCollectedIds().Count ?? 0;
            if (artifactCount != _lastArtifactCount)
            {
                _lastArtifactCount = artifactCount;
                BindArtifacts();
            }
        }

        private void BindResourceDisplays()
        {
            if (_resourceContainer == null || _bootstrap?.ResourceDisplayKeys == null)
                return;

            _resourceContainer.Clear();
            _resourcesBound = false;

            var ordered = GetOrderedResourceIds();
            foreach (var resourceId in ordered)
            {
                if (!_bootstrap.ResourceDisplayKeys.TryGetValue(resourceId, out var displayKey))
                    continue;

                var display = new ResourceDisplay
                {
                    ResourceId = resourceId
                };

                if (_bootstrap.HudConfig?.CardLayout ?? true)
                    display.AddToClassList("resource-display--badge");

                if (_bootstrap.Localization != null)
                {
                    var localized = _bootstrap.Localization.GetString(displayKey);
                    display.SetDisplayName(localized);
                }
                else
                {
                    display.SetDisplayName(displayKey);
                }

                display.SetValue(_idleModule.GetResource(resourceId));

                if (_bootstrap.ResourceIconPaths != null &&
                    _bootstrap.ResourceIconPaths.TryGetValue(resourceId, out var iconPath))
                {
                    var texture = LoadResourceIcon(resourceId, iconPath);
                    display.SetIcon(texture);
                }
                else
                {
                    display.SetIcon(null);
                }

                var productionPerSec = GetProductionRatePerSecond(resourceId);
                display.SetProductionRate(productionPerSec);

                _resourceContainer.Add(display);
            }

            _resourcesBound = true;
        }

        private void BindActionButtons()
        {
            if (_actionsContainer == null)
                return;

            _actionsContainer.Clear();
            _actionsBound = false;

            var manualIds = _idleModule?.GetManualProductionIds();
            if (manualIds != null && manualIds.Count > 0)
            {
                foreach (var prodId in manualIds)
                {
                    var btn = new Button { text = "Tap" };
                    btn.AddToClassList("hud-action-button");
                    btn.AddToClassList("hud-action-button--primary");
                    var capture = prodId;
                    btn.clicked += () =>
                    {
                        if (_bootstrap?.IdleModule != null)
                            _bootstrap.IdleModule.TriggerManualProduction(capture);
                    };
                    _actionsContainer.Add(btn);
                }
            }

            if (_bootstrap?.PrestigeModule != null)
            {
                var prestigeBtn = new Button();
                prestigeBtn.AddToClassList("hud-action-button");
                prestigeBtn.clicked += () => _bootstrap.TryPrestige();
                _actionsContainer.Add(prestigeBtn);
            }

            if (_bootstrap?.TierModule != null)
            {
                var ascendBtn = new Button();
                ascendBtn.AddToClassList("hud-action-button");
                ascendBtn.clicked += () => _bootstrap.TryAscendTier();
                _actionsContainer.Add(ascendBtn);
            }

            _actionsBound = true;
        }

        private void BindArtifacts()
        {
            if (_artifactsContainer == null || _bootstrap?.ArtifactModule == null)
                return;

            _artifactsContainer.Clear();
            _lastArtifactCount = _bootstrap.ArtifactModule.GetCollectedIds().Count;

            var collected = _bootstrap.ArtifactModule.GetCollectedIds();
            foreach (var id in collected)
            {
                var artifact = _bootstrap.ArtifactModule.GetArtifact(id);
                if (artifact == null)
                    continue;

                var badge = new VisualElement();
                badge.AddToClassList("artifact-badge");

                var icon = new VisualElement();
                icon.AddToClassList("artifact-badge__icon");
                if (!string.IsNullOrEmpty(artifact.IconPath))
                {
                    var texture = LoadArtifactIcon(id, artifact.IconPath);
                    if (texture != null)
                    {
                        icon.style.backgroundImage = new StyleBackground(texture);
                        icon.style.display = DisplayStyle.Flex;
                    }
                }
                badge.Add(icon);

                var label = new Label();
                label.AddToClassList("artifact-badge__label");
                var displayKey = !string.IsNullOrEmpty(artifact.DisplayKey) ? artifact.DisplayKey : $"artifact.{id}";
                label.text = _bootstrap.Localization?.GetString(displayKey) ?? displayKey;
                badge.Add(label);

                _artifactsContainer.Add(badge);
            }
        }

        private IEnumerable<string> GetOrderedResourceIds()
        {
            var order = _bootstrap?.HudConfig?.Resources?.Order;
            if (order != null && order.Count > 0)
            {
                var set = order.ToHashSet();
                foreach (var id in order)
                    if (_bootstrap.ResourceDisplayKeys.ContainsKey(id))
                        yield return id;
                foreach (var (id, _) in _bootstrap.ResourceDisplayKeys)
                    if (!set.Contains(id))
                        yield return id;
            }
            else
            {
                foreach (var (id, _) in _bootstrap.ResourceDisplayKeys)
                    yield return id;
            }
        }

        private void BindUpgradeButtons()
        {
            if (_upgradesContainer == null || _upgradeModule == null)
                return;

            if (!(_bootstrap?.HudConfig?.Upgrades?.Visible ?? true))
            {
                _upgradesBound = true;
                return;
            }

            _upgradesContainer.Clear();
            _upgradesBound = false;

            var ordered = GetOrderedUpgradeIds();
            foreach (var upgradeId in ordered)
            {
                var upgrade = _upgradeModule.GetUpgrade(upgradeId);
                if (upgrade == null)
                    continue;

                var button = new UpgradeButton
                {
                    UpgradeId = upgradeId
                };

                if (_bootstrap.HudConfig?.CardLayout ?? true)
                    button.AddToClassList("upgrade-button--card");

                var displayKey = !string.IsNullOrEmpty(upgrade.DisplayKey)
                    ? upgrade.DisplayKey
                    : $"upgrade.{upgradeId}";

                if (_bootstrap.Localization != null)
                {
                    var localized = _bootstrap.Localization.GetString(displayKey);
                    button.SetDisplayName(localized);
                }
                else
                {
                    button.SetDisplayName(displayKey);
                }

                if (!string.IsNullOrEmpty(upgrade.IconPath))
                {
                    var texture = LoadUpgradeIcon(upgradeId, upgrade.IconPath);
                    button.SetIcon(texture);
                }
                else
                {
                    button.SetIcon(null);
                }

                var upgradeIdCapture = upgradeId;
                button.SetClickCallback(() => _upgradeModule.TryPurchase(upgradeIdCapture));

                _upgradesContainer.Add(button);
            }

            _upgradesBound = true;
        }

        private IEnumerable<string> GetOrderedUpgradeIds()
        {
            var allIds = _upgradeModule.GetUpgradeIds().ToList();
            var order = _bootstrap?.HudConfig?.Upgrades?.Order;
            if (order != null && order.Count > 0)
            {
                var set = order.ToHashSet();
                foreach (var id in order)
                    if (allIds.Contains(id))
                        yield return id;
                foreach (var id in allIds)
                    if (!set.Contains(id))
                        yield return id;
            }
            else
            {
                foreach (var id in allIds)
                    yield return id;
            }
        }

        private void UpdateUpgradeButtons()
        {
            if (!_upgradesBound || _upgradesContainer == null || _upgradeModule == null)
                return;

            _upgradesContainer.Query<UpgradeButton>().ForEach(button =>
            {
                var upgradeId = button.UpgradeId;
                if (string.IsNullOrEmpty(upgradeId))
                    return;

                var upgrade = _upgradeModule.GetUpgrade(upgradeId);
                if (upgrade == null)
                    return;

                var level = _upgradeModule.GetLevel(upgradeId);
                var cost = _upgradeModule.GetCostForNextLevel(upgradeId);
                var canPurchase = _upgradeModule.CanPurchase(upgradeId);

                var costResourceName = GetResourceDisplayName(upgrade.CostResourceId);
                button.SetCost($"{cost} {costResourceName}");
                button.SetLevel(level, upgrade.MaxLevel);
                button.SetCanPurchase(canPurchase);
            });
        }

        private string GetResourceDisplayName(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                return resourceId ?? "—";

            if (_bootstrap?.ResourceDisplayKeys != null &&
                _bootstrap.ResourceDisplayKeys.TryGetValue(resourceId, out var displayKey) &&
                _bootstrap.Localization != null)
            {
                return _bootstrap.Localization.GetString(displayKey);
            }

            return resourceId;
        }

        private void UpdateResourceValues()
        {
            if (!_resourcesBound || _resourceContainer == null)
                return;

            _resourceContainer.Query<ResourceDisplay>().ForEach(display =>
            {
                var resourceId = display.ResourceId;
                if (!string.IsNullOrEmpty(resourceId))
                {
                    display.SetValue(_idleModule.GetResource(resourceId));
                    display.SetProductionRate(GetProductionRatePerSecond(resourceId));
                }
            });
        }

        private BigNumber GetProductionRatePerSecond(string resourceId)
        {
            if (_idleModule == null || _bootstrap == null)
                return BigNumber.Zero;

            var perTick = _idleModule.GetNetProductionPerTick(resourceId);
            var tickInterval = _bootstrap.TickIntervalSeconds;
            if (tickInterval <= 0)
                return BigNumber.Zero;

            return perTick / BigNumber.FromDouble(tickInterval);
        }

        private Texture2D LoadResourceIcon(string resourceId, string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath) || _bootstrap == null)
                return null;

            var resourcesPath = "Game/" + _bootstrap.GameId + "/" + iconPath;
            return Resources.Load<Texture2D>(resourcesPath);
        }

        private Texture2D LoadUpgradeIcon(string upgradeId, string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath) || _bootstrap == null)
                return null;

            var resourcesPath = "Game/" + _bootstrap.GameId + "/" + iconPath;
            return Resources.Load<Texture2D>(resourcesPath);
        }

        private Texture2D LoadArtifactIcon(string artifactId, string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath) || _bootstrap == null)
                return null;

            var resourcesPath = "Game/" + _bootstrap.GameId + "/" + iconPath;
            return Resources.Load<Texture2D>(resourcesPath);
        }

        private void UpdateActionButtons()
        {
            if (!_actionsBound || _actionsContainer == null)
                return;

            var buttons = _actionsContainer.Query<Button>().ToList();
            var idx = 0;

            if (_idleModule?.GetManualProductionIds() is { Count: > 0 })
                idx += _idleModule.GetManualProductionIds().Count;

            if (_bootstrap?.PrestigeModule != null && idx < buttons.Count)
            {
                var prestigeBtn = buttons[idx++];
                var canPrestige = _bootstrap.PrestigeModule.CanPrestige();
                var currencyId = _bootstrap.PrestigeModule.GetCurrencyResourceId();
                var amount = _bootstrap.IdleModule.GetResource(currencyId);
                var currencyName = GetResourceDisplayName(currencyId);
                prestigeBtn.text = $"Prestige ({amount} {currencyName})";
                prestigeBtn.SetEnabled(canPrestige);
            }

            if (_bootstrap?.TierModule != null && idx < buttons.Count)
            {
                var ascendBtn = buttons[idx];
                var canAscend = _bootstrap.TierModule.CanAscend();
                var next = _bootstrap.TierModule.GetNextTier();
                var nextName = next != null && _bootstrap.Localization != null
                    ? _bootstrap.Localization.GetString(next.DisplayKey) ?? next.Id
                    : "Ascend";
                ascendBtn.text = $"Ascend → {nextName}";
                ascendBtn.SetEnabled(canAscend);
            }
        }
    }
}
