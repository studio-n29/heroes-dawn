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
        VisualElement _objectiveRows = null!;
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
            _objectiveRows = Q<VisualElement>("objectiveRows");
            _toHq = Q<UnityEngine.UIElements.Button>("toHq");
            ToHqButton = new HdButtonRef(_toHq, this);
            _toHq.clicked += () => _done?.TrySetResult();
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(ZoneCompleteData d)
        {
            _title.text = d.Title; _rewards.text = d.Rewards; _objectivesTitle.text = d.ObjectivesTitle;
            _objectives.text = d.Objectives; _rewardsTitle.text = d.RewardsTitle; _toHq.text = d.ToHq;
            FillObjectives(d.Objectives);
            bool rewards = !string.IsNullOrEmpty(d.Rewards);
            _rewardsTitle.style.display = rewards ? DisplayStyle.Flex : DisplayStyle.None;
            _rewards.style.display = rewards ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Une case à cocher du kit par objectif : la ligne reçue commence par « OK » (atteint) ou par un tiret (manqué).</summary>
        void FillObjectives(string? lines)
        {
            _objectiveRows.Clear();
            bool any = !string.IsNullOrEmpty(lines);
            _objectivesTitle.style.display = any ? DisplayStyle.Flex : DisplayStyle.None;
            _objectiveRows.style.display = any ? DisplayStyle.Flex : DisplayStyle.None;
            if (!any) return;
            foreach (var raw in lines!.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                bool done = line.StartsWith("OK ", System.StringComparison.Ordinal);
                if (done) line = line.Substring(2).TrimStart();
                else line = line.TrimStart('–', '-', ' ');
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("hd-m-check");
                row.EnableInClassList("hd-m-check--done", done);
                var box = new VisualElement { pickingMode = PickingMode.Ignore }; box.AddToClassList("hd-check"); box.AddToClassList("hd-check--sm"); box.AddToClassList("hd-m-check__box"); box.EnableInClassList("hd-check--on", done);
                var text = new Label(line) { pickingMode = PickingMode.Ignore }; text.AddToClassList("hd-m-check__text");
                row.Add(box); row.Add(text);
                _objectiveRows.Add(row);
            }
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
