using System;
using UnityEngine.UIElements;

namespace GameEngine.UI.Components
{
    /// <summary>
    /// UI Toolkit component that displays a quest with progress bar and claim button.
    /// </summary>
    [UxmlElement]
    public partial class QuestRow : VisualElement
    {
        public static readonly string UssClassName = "quest-row";
        public static readonly string LabelUssClassName = "quest-row__label";
        public static readonly string ProgressUssClassName = "quest-row__progress";
        public static readonly string ClaimUssClassName = "quest-row__claim";

        [UxmlAttribute("quest-id")]
        public string QuestId { get; set; }

        private readonly Label _label;
        private readonly ProgressBar _progressBar;
        private readonly Button _claimButton;
        private Action _claimCallback;
        private string _displayName;

        public QuestRow()
        {
            AddToClassList(UssClassName);

            _label = new Label { text = "—" };
            _label.AddToClassList(LabelUssClassName);
            Add(_label);

            _progressBar = new ProgressBar { lowValue = 0, highValue = 1, value = 0 };
            _progressBar.AddToClassList(ProgressUssClassName);
            Add(_progressBar);

            _claimButton = new Button { text = "Claim" };
            _claimButton.AddToClassList(ClaimUssClassName);
            _claimButton.clicked += OnClaimClicked;
            Add(_claimButton);
        }

        public void SetDisplayName(string name)
        {
            _displayName = name ?? "—";
            _label.text = _displayName;
        }

        public void SetClaimButtonText(string text)
        {
            _claimButton.text = text ?? "Claim";
        }

        public void SetProgress(double progress)
        {
            _progressBar.value = (float)Math.Clamp(progress, 0, 1);
        }

        public void SetCanClaim(bool canClaim)
        {
            _claimButton.SetEnabled(canClaim);
            _claimButton.style.display = canClaim ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetCompleted(bool completed)
        {
            if (completed)
            {
                _progressBar.value = 1f;
                _claimButton.style.display = DisplayStyle.None;
                _label.text = _displayName + " ✓";
                AddToClassList("quest-row--completed");
            }
        }

        public void SetClaimCallback(Action callback)
        {
            _claimCallback = callback;
        }

        private void OnClaimClicked()
        {
            _claimCallback?.Invoke();
        }
    }
}
