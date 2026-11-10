#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Repos (fenêtre UI Toolkit) : fatigue avant → après de l'escouade, Passer / Se reposer. Non annulable.</summary>
    public sealed class RestPopupView : HdPopup, IRestPopupView
    {
        Label _title = null!, _body = null!, _fatigueTitle = null!;
        VisualElement _fatigue = null!;
        UnityEngine.UIElements.Button _skip = null!, _take = null!;
        UniTaskCompletionSource<bool>? _choice;
        RestPopupData? _last;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef TakeButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _take;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _body = Q<Label>("body"); _fatigueTitle = Q<Label>("fatigueTitle");
            _fatigue = Q<VisualElement>("fatigue");
            _skip = Q<UnityEngine.UIElements.Button>("skip"); _take = Q<UnityEngine.UIElements.Button>("take");
            SkipButton = new HdButtonRef(_skip, this); TakeButton = new HdButtonRef(_take, this);
            _skip.clicked += () => Choose(false);
            _take.clicked += () => Choose(true);
            if (_last.HasValue) Fill(_last.Value);
        }

        void Choose(bool value)
        {
            _skip.SetEnabled(false); _take.SetEnabled(false); // une seule résolution, même en cas de double clic
            _choice?.TrySetResult(value);
        }

        void Fill(RestPopupData d)
        {
            _title.text = d.Title; _body.text = d.Body;
            bool rows = d.Fatigue != null && d.Fatigue.Length > 0;
            _fatigueTitle.style.display = rows ? DisplayStyle.Flex : DisplayStyle.None;
            _fatigue.style.display = rows ? DisplayStyle.Flex : DisplayStyle.None;
            _fatigueTitle.text = d.FatigueTitle;
            HdPopupRows.FillFatigue(_fatigue, d.Fatigue);
            _skip.text = d.Skip; _take.text = d.Take;
            _skip.SetEnabled(true); _take.SetEnabled(true);
        }

        public UniTask<bool> ChooseAsync(RestPopupData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _choice = new UniTaskCompletionSource<bool>();
            ct.Register(() => _choice.TrySetCanceled());
            return _choice.Task;
        }
    }
}
