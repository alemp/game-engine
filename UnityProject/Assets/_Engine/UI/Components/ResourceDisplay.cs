using GameEngine.Core.Economy;
using UnityEngine.UIElements;

namespace GameEngine.UI.Components
{
    /// <summary>
    /// UI Toolkit component that displays a resource amount.
    /// </summary>
    [UxmlElement]
    public partial class ResourceDisplay : VisualElement
    {
        public static readonly string UssClassName = "resource-display";
        public static readonly string LabelUssClassName = "resource-display__label";
        public static readonly string ValueUssClassName = "resource-display__value";

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
        private readonly Label _label;
        private readonly Label _valueLabel;

        public ResourceDisplay()
        {
            AddToClassList(UssClassName);

            _label = new Label { text = "—" };
            _label.AddToClassList(LabelUssClassName);
            Add(_label);

            _valueLabel = new Label { text = "0" };
            _valueLabel.AddToClassList(ValueUssClassName);
            Add(_valueLabel);
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
    }
}
