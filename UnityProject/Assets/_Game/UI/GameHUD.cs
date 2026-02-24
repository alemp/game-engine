using GameEngine.Core.Config.Schemas;
using GameEngine.Game.Bootstrap;
using GameEngine.Modules.Idle;
using GameEngine.UI.Components;
using GameEngine.UI.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.Game.UI
{
    /// <summary>
    /// HUD that displays idle game resources. Add to a UI Document.
    /// </summary>
    public sealed class GameHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GameBootstrap _bootstrap;

        private IdleModule _idleModule;
        private VisualElement _root;
        private Label _goldLabel;

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

            if (_bootstrap.Theme != null)
                ThemeApplier.Apply(_root, _bootstrap.Theme);

            ApplyLocalization();

            _goldLabel = _root.Q<Label>("gold-value");
            if (_goldLabel == null)
                _goldLabel = _root.Q<Label>(className: "resource-display__value");
        }

        private void Update()
        {
            if (_idleModule == null || _goldLabel == null)
            {
                TryBind();
                return;
            }

            var gold = _idleModule.GetResource("gold");
            _goldLabel.text = gold.ToString();
        }

        private void ApplyLocalization()
        {
            if (_bootstrap?.Localization == null || _bootstrap?.ResourceDisplayKeys == null)
                return;

            _root.Query<ResourceDisplay>().ForEach(display =>
            {
                var resourceId = display.ResourceId;
                if (string.IsNullOrEmpty(resourceId))
                    return;

                if (_bootstrap.ResourceDisplayKeys.TryGetValue(resourceId, out var displayKey))
                {
                    var localized = _bootstrap.Localization.GetString(displayKey);
                    display.SetDisplayName(localized);
                }
            });
        }
    }
}
