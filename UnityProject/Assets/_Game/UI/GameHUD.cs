using GameEngine.Game.Bootstrap;
using GameEngine.Modules.Idle;
using GameEngine.UI.Components;
using GameEngine.UI.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.Game.UI
{
    /// <summary>
    /// HUD that displays idle game resources. Binds ResourceDisplay to all resources from config.
    /// </summary>
    public sealed class GameHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GameBootstrap _bootstrap;

        private IdleModule _idleModule;
        private VisualElement _root;
        private VisualElement _resourceContainer;
        private bool _resourcesBound;

        private void OnEnable()
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

            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
                return;

            _root = _uiDocument.rootVisualElement;
            if (_root == null)
                return;

            _resourceContainer = _root.Q<VisualElement>("resource-container");

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            BindResourceDisplays();
        }

        private void Update()
        {
            if (_idleModule == null)
            {
                TryBind();
                return;
            }

            UpdateResourceValues();
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
