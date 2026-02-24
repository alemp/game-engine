using System;
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
        public static readonly string LabelUssClassName = "upgrade-button__label";
        public static readonly string CostUssClassName = "upgrade-button__cost";
        public static readonly string LevelUssClassName = "upgrade-button__level";
        public static readonly string BuyUssClassName = "upgrade-button__buy";

        [UxmlAttribute("upgrade-id")]
        public string UpgradeId { get; set; }

        private readonly Label _label;
        private readonly Label _costLabel;
        private readonly Label _levelLabel;
        private readonly Button _buyButton;
        private Action _clickCallback;

        public UpgradeButton()
        {
            AddToClassList(UssClassName);

            _label = new Label { text = "—" };
            _label.AddToClassList(LabelUssClassName);
            Add(_label);

            _costLabel = new Label { text = "—" };
            _costLabel.AddToClassList(CostUssClassName);
            Add(_costLabel);

            _levelLabel = new Label { text = "0/1" };
            _levelLabel.AddToClassList(LevelUssClassName);
            Add(_levelLabel);

            _buyButton = new Button { text = "Buy" };
            _buyButton.AddToClassList(BuyUssClassName);
            _buyButton.clicked += OnBuyClicked;
            Add(_buyButton);
        }

        public void SetDisplayName(string name)
        {
            _label.text = name ?? "—";
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
