using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.UI.Components
{
    /// <summary>
    /// UI Toolkit component that displays an upgrade with a purchase button.
    /// </summary>
    [UxmlElement]
    public partial class UpgradeButton : VisualElement
    {
        public static readonly string UssClassName = "upgrade-button";
        public static readonly string ContentUssClassName = "upgrade-button__content";
        public static readonly string TopRowUssClassName = "upgrade-button__top";
        public static readonly string BottomRowUssClassName = "upgrade-button__bottom";
        public static readonly string IconUssClassName = "upgrade-button__icon";
        public static readonly string LabelUssClassName = "upgrade-button__label";
        public static readonly string CostUssClassName = "upgrade-button__cost";
        public static readonly string LevelUssClassName = "upgrade-button__level";
        public static readonly string BuyUssClassName = "upgrade-button__buy";

        [UxmlAttribute("upgrade-id")]
        public string UpgradeId { get; set; }

        private readonly VisualElement _icon;
        private readonly Label _label;
        private readonly Label _costLabel;
        private readonly Label _levelLabel;
        private readonly Button _buyButton;
        private Action _clickCallback;

        public UpgradeButton()
        {
            AddToClassList(UssClassName);

            var content = new VisualElement();
            content.AddToClassList(ContentUssClassName);

            var topRow = new VisualElement();
            topRow.AddToClassList(TopRowUssClassName);

            _icon = new VisualElement();
            _icon.AddToClassList(IconUssClassName);
            topRow.Add(_icon);

            _label = new Label { text = "—" };
            _label.AddToClassList(LabelUssClassName);
            topRow.Add(_label);

            content.Add(topRow);

            var bottomRow = new VisualElement();
            bottomRow.AddToClassList(BottomRowUssClassName);

            _costLabel = new Label { text = "—" };
            _costLabel.AddToClassList(CostUssClassName);
            bottomRow.Add(_costLabel);

            _levelLabel = new Label { text = "0/1" };
            _levelLabel.AddToClassList(LevelUssClassName);
            bottomRow.Add(_levelLabel);

            content.Add(bottomRow);

            Add(content);

            _buyButton = new Button { text = "Buy" };
            _buyButton.AddToClassList(BuyUssClassName);
            _buyButton.clicked += OnBuyClicked;
            Add(_buyButton);
        }

        public void SetDisplayName(string name)
        {
            _label.text = name ?? "—";
        }

        /// <summary>
        /// Sets the upgrade icon. Pass null to hide.
        /// </summary>
        public void SetIcon(Texture2D texture)
        {
            if (texture != null)
            {
                _icon.style.backgroundImage = new StyleBackground(texture);
                _icon.style.display = DisplayStyle.Flex;
            }
            else
            {
                _icon.style.backgroundImage = StyleKeyword.Null;
                _icon.style.display = DisplayStyle.None;
            }
        }

        public void SetCost(string costText)
        {
            _costLabel.text = costText ?? "—";
        }

        public void SetLevel(int current, int max)
        {
            _levelLabel.text = $"{current}/{max}";
        }

        public void SetCanPurchase(bool canPurchase)
        {
            _buyButton.SetEnabled(canPurchase);
        }

        public void SetClickCallback(Action callback)
        {
            _clickCallback = callback;
        }

        private void OnBuyClicked()
        {
            _clickCallback?.Invoke();
        }
    }
}
