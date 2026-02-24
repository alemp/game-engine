using System.Collections.Generic;
using System.Linq;
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
        private VisualElement _sectionResources;
        private VisualElement _sectionUpgrades;
        private bool _resourcesBound;
        private bool _upgradesBound;

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
            _sectionResources = _root.Q<VisualElement>("section-resources");
            _sectionUpgrades = _root.Q<VisualElement>("section-upgrades");

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            ApplyHudLayout();
            BindResourceDisplays();
            BindUpgradeButtons();
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
            _sectionResources = _root.Q<VisualElement>("section-resources");
            _sectionUpgrades = _root.Q<VisualElement>("section-upgrades");

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            ApplyHudLayout();
            BindResourceDisplays();
            BindUpgradeButtons();
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
        }

        private void ApplySectionOrder()
        {
            var order = _bootstrap?.HudConfig?.SectionOrder;
            if (order == null || order.Count == 0 || _hudRoot == null)
                return;

            var resources = _sectionResources;
            var upgrades = _sectionUpgrades;
            if (resources == null || upgrades == null)
                return;

            resources.RemoveFromHierarchy();
            upgrades.RemoveFromHierarchy();

            foreach (var id in order)
            {
                if (id == "resources")
                    _hudRoot.Add(resources);
                else if (id == "upgrades")
                    _hudRoot.Add(upgrades);
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
                    display.AddToClassList("resource-display--card");

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
                _resourceContainer.Add(display);
            }

            _resourcesBound = true;
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
                    display.SetValue(_idleModule.GetResource(resourceId));
            });
        }
    }
}
