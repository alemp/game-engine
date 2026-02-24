using System.Collections.Generic;
using System.Linq;
using GameEngine.Core.Config.Schemas;
using GameEngine.Game.Bootstrap;
using GameEngine.UI.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.Game.UI
{
    /// <summary>
    /// Root UI with configurable navigation (bottom bar, side, tabs) and screen switching.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GameBootstrap _bootstrap;
        [SerializeField] private GameHUD _gameHud;

        private VisualElement _root;
        private VisualElement _contentSlot;
        private VisualElement _navBar;
        private string _activeScreenId;
        private readonly Dictionary<string, VisualElement> _screenCache = new();

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

            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
                return;

            _root = _uiDocument.rootVisualElement;
            if (_root == null)
                return;

            _contentSlot = _root.Q<VisualElement>("content-slot");
            _navBar = _root.Q<VisualElement>("nav-bar");

            if (_contentSlot == null || _navBar == null)
                return;

            if (_gameHud == null)
                _gameHud = GetComponent<GameHUD>();

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            var nav = _bootstrap.UiConfig?.Navigation;
            if (nav != null && nav.Items != null && nav.Items.Count > 0)
            {
                SetupNavigation(nav);
                ShowScreen(_bootstrap.UiConfig.DefaultScreen ?? "hud");
            }
            else
            {
                _navBar.style.display = DisplayStyle.None;
                ShowScreen(_bootstrap.UiConfig?.DefaultScreen ?? "hud");
            }
        }

        private void SetupNavigation(NavigationSchema nav)
        {
            _navBar.style.display = DisplayStyle.Flex;
            _navBar.Clear();

            var isVertical = nav.Type == "side" && (nav.Position == "left" || nav.Position == "right");
            if (isVertical)
                _navBar.AddToClassList("nav-bar--vertical");
            else
                _navBar.RemoveFromClassList("nav-bar--vertical");

            ApplyNavPosition(nav);

            foreach (var screenId in nav.Items)
            {
                var screen = _bootstrap.UiConfig?.Screens?.Find(s => s.Id == screenId);
                if (screen == null || !screen.Visible)
                    continue;

                var label = GetNavLabel(screen);
                var button = new Button { text = label };
                button.AddToClassList("nav-bar__item");

                var screenIdCapture = screenId;
                button.clicked += () => ShowScreen(screenIdCapture);

                _navBar.Add(button);
            }
        }

        private void ApplyNavPosition(NavigationSchema nav)
        {
            var appRoot = _root.Q("app-root") ?? _root;

            appRoot.style.flexDirection = nav.Position switch
            {
                "top" => FlexDirection.Column,
                "bottom" => FlexDirection.Column,
                "left" => FlexDirection.Row,
                "right" => FlexDirection.Row,
                _ => FlexDirection.Column
            };

            if (nav.Position == "top" || nav.Position == "left")
                _navBar.BringToFront();
            else
                _navBar.SendToBack();
        }

        private string GetNavLabel(ScreenTemplateEntry screen)
        {
            var key = !string.IsNullOrEmpty(screen.LabelKey) ? screen.LabelKey : $"nav.{screen.Id}";
            return _bootstrap.Localization?.GetString(key) ?? screen.Id;
        }

        private void ShowScreen(string screenId)
        {
            if (_activeScreenId == screenId)
                return;

            _activeScreenId = screenId;

            if (!_screenCache.TryGetValue(screenId, out var screenRoot))
            {
                var screen = _bootstrap.UiConfig?.Screens?.Find(s => s.Id == screenId);
                if (screen == null || string.IsNullOrEmpty(screen.Template))
                    return;

                var asset = Resources.Load<VisualTreeAsset>(screen.Template);
                if (asset == null)
                    return;

                screenRoot = asset.CloneTree();
                _screenCache[screenId] = screenRoot;
            }

            _contentSlot.Clear();
            _contentSlot.Add(screenRoot);

            UpdateNavSelection();

            if (_gameHud != null && (screenId == "hud" || screenId == "upgrades"))
                _gameHud.BindToRoot(screenRoot);
        }

        private void UpdateNavSelection()
        {
            var buttons = _navBar.Query<Button>().ToList();
            var items = _bootstrap.UiConfig?.Navigation?.Items;
            if (items == null)
                return;

            for (var i = 0; i < buttons.Count && i < items.Count; i++)
            {
                var isActive = items[i] == _activeScreenId;
                if (isActive)
                    buttons[i].AddToClassList("nav-bar__item--active");
                else
                    buttons[i].RemoveFromClassList("nav-bar__item--active");
            }
        }
    }
}
