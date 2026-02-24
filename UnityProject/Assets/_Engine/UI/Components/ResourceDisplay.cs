using GameEngine.Core.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameEngine.UI.Components
{
    /// <summary>
    /// UI Toolkit component that displays a resource amount.
    /// </summary>
    public sealed class ResourceDisplay : VisualElement
    {
        public new static readonly string UssClassName = "resource-display";
        public static readonly string LabelUssClassName = "resource-display__label";
        public static readonly string ValueUssClassName = "resource-display__value";

        private readonly Label _label;
        private readonly Label _valueLabel;

        public string ResourceId { get; set; }
        public string DisplayName { get; set; }

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
            _label.text = name;
        }

        public void SetValue(BigNumber value)
        {
            _valueLabel.text = value.ToString();
        }

        public new class UxmlFactory : UxmlFactory<ResourceDisplay, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _resourceId = new() { name = "resource-id" };
            private readonly UxmlStringAttributeDescription _displayName = new() { name = "display-name" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var display = (ResourceDisplay)ve;
                display.ResourceId = _resourceId.GetValueFromBag(bag, cc);
                display.SetDisplayName(_displayName.GetValueFromBag(bag, cc) ?? display.ResourceId);
            }
        }
    }
}
