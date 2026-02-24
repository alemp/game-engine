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
        private VisualElement _resourceContainer;
        private VisualElement _upgradesContainer;
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

        private void TryBind()
        {
            if (_bootstrap == null)
                _bootstrap = FindFirstObjectByType<GameBootstrap>();

            if (_bootstrap?.IdleModule == null)
                return;

            _idleModule = _bootstrap.IdleModule;
            _upgradeModule = _bootstrap.UpgradeModule;

            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
                return;

            _root = _uiDocument.rootVisualElement;
            if (_root == null)
                return;

            _resourceContainer = _root.Q<VisualElement>("resource-container");
            _upgradesContainer = _root.Q<VisualElement>("upgrades-container");

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            BindResourceDisplays();
            BindUpgradeButtons();
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

            foreach (var (resourceId, displayKey) in _bootstrap.ResourceDisplayKeys)
            {
                var display = new ResourceDisplay
                {
                    ResourceId = resourceId
                };

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

        private void BindUpgradeButtons()
        {
            if (_upgradesContainer == null || _upgradeModule == null)
                return;

            _upgradesContainer.Clear();
            _upgradesBound = false;

            foreach (var upgradeId in _upgradeModule.GetUpgradeIds())
            {
                var upgrade = _upgradeModule.GetUpgrade(upgradeId);
                if (upgrade == null)
                    continue;

                var button = new UpgradeButton
                {
                    UpgradeId = upgradeId
                };

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
