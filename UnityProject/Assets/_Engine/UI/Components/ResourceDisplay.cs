using GameEngine.Core.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.UI.Components
{
    /// <summary>
    /// UI Toolkit component that displays a resource amount, optional icon, and production rate.
    /// </summary>
    [UxmlElement]
    public partial class ResourceDisplay : VisualElement
    {
        public static readonly string UssClassName = "resource-display";
        public static readonly string IconUssClassName = "resource-display__icon";
        public static readonly string LabelUssClassName = "resource-display__label";
        public static readonly string ValueUssClassName = "resource-display__value";
        public static readonly string RateUssClassName = "resource-display__rate";

        [UxmlAttribute("resource-id")]
        public string ResourceId { get; set; }

        [UxmlAttribute("display-name")]
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                if (_label != null)
                    _label.text = value ?? "—";
            }
        }

        private string _displayName;
        private readonly VisualElement _icon;
        private readonly Label _label;
        private readonly Label _valueLabel;
        private readonly Label _rateLabel;

        public ResourceDisplay()
        {
            AddToClassList(UssClassName);

            _icon = new VisualElement();
            _icon.AddToClassList(IconUssClassName);
            Add(_icon);

            _label = new Label { text = "—" };
            _label.AddToClassList(LabelUssClassName);
            Add(_label);

            _valueLabel = new Label { text = "0" };
            _valueLabel.AddToClassList(ValueUssClassName);
            Add(_valueLabel);

            _rateLabel = new Label { text = "" };
            _rateLabel.AddToClassList(RateUssClassName);
            Add(_rateLabel);
        }

        public void SetDisplayName(string name)
        {
            DisplayName = name;
            _label.text = name ?? "—";
        }

        public void SetValue(BigNumber value)
        {
            _valueLabel.text = value.ToString();
        }

        /// <summary>
        /// Sets the resource icon. Pass null to hide.
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

        /// <summary>
        /// Sets the production rate display (+X/sec). Hides if zero.
        /// </summary>
        public void SetProductionRate(BigNumber perSecond)
        {
            if (perSecond == BigNumber.Zero || perSecond < BigNumber.Zero)
            {
                _rateLabel.text = "";
                _rateLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _rateLabel.text = "+" + perSecond.ToString() + "/sec";
                _rateLabel.style.display = DisplayStyle.Flex;
            }
        }
    }
}
