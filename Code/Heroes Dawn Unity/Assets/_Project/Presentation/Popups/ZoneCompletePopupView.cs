#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Zone sécurisée (fenêtre UI Toolkit) : objectifs, récompenses et « Vers le QG ». Non annulable.</summary>
    public sealed class ZoneCompletePopupView : HdPopup, IZoneCompletePopupView
    {
        Label _title = null!, _rewards = null!, _objectivesTitle = null!, _objectives = null!, _rewardsTitle = null!;
        UnityEngine.UIElements.Button _toHq = null!;
        UniTaskCompletionSource? _done;
        ZoneCompleteData? _last;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        public HdButtonRef ToHqButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _toHq;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _rewards = Q<Label>("rewards"); _objectivesTitle = Q<Label>("objectivesTitle");
            _objectives = Q<Label>("objectives"); _rewardsTitle = Q<Label>("rewardsTitle");
            _toHq = Q<UnityEngine.UIElements.Button>("toHq");
            ToHqButton = new HdButtonRef(_toHq, this);
            _toHq.clicked += () => _done?.TrySetResult();
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(ZoneCompleteData d)
        {
            _title.text = d.Title; _rewards.text = d.Rewards; _objectivesTitle.text = d.ObjectivesTitle;
            _objectives.text = d.Objectives; _rewardsTitle.text = d.RewardsTitle; _toHq.text = d.ToHq;
        }

        public UniTask ShowAsync(ZoneCompleteData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }
    }
}
