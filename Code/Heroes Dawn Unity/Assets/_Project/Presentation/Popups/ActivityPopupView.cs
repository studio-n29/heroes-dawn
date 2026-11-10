#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Fenêtre d'activité (UI Toolkit) : description, décor, réglages, niveau généré, et deux boutons pour simuler le résultat. Non annulable.</summary>
    public sealed class ActivityPopupView : HdPopup, IActivityPopupView
    {
        Label _title = null!, _body = null!, _details = null!, _wip = null!;
        UnityEngine.UIElements.Button _win = null!, _fail = null!;
        UniTaskCompletionSource<ActivityResult>? _done;
        ActivityPopupData? _last;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        public HdButtonRef WinButton { get; private set; } = null!;
        public HdButtonRef FailButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _win;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _body = Q<Label>("body"); _details = Q<Label>("details"); _wip = Q<Label>("wip");
            _win = Q<UnityEngine.UIElements.Button>("win"); _fail = Q<UnityEngine.UIElements.Button>("fail");
            WinButton = new HdButtonRef(_win, this); FailButton = new HdButtonRef(_fail, this);
            _win.clicked += () => { if (_last.HasValue) _done?.TrySetResult(ActivityResult.Won(_last.Value.WinScore)); };
            _fail.clicked += () => { if (_last.HasValue) _done?.TrySetResult(ActivityResult.Lost(_last.Value.FailScore)); };
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(ActivityPopupData d)
        {
            _title.text = d.Title; _body.text = d.Body; _details.text = string.Join("\n", d.Lines); _wip.text = d.Wip;
            _win.text = d.WinLabel; _fail.text = d.FailLabel;
        }

        public UniTask<ActivityResult> PlayAsync(ActivityPopupData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }
    }
}
