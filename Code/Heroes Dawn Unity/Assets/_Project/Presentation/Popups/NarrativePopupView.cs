#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Événement (fenêtre UI Toolkit) : texte et un bouton par choix. Non annulable.</summary>
    public sealed class NarrativePopupView : HdPopup, INarrativePopupView
    {
        readonly List<UnityEngine.UIElements.Button> _buttons = new List<UnityEngine.UIElements.Button>();
        Label _title = null!, _body = null!;
        VisualElement _choices = null!;
        UniTaskCompletionSource<NarrativeChoice>? _choice;
        NarrativePopupData? _last;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        public IReadOnlyList<UnityEngine.UIElements.Button> ChoiceButtons => _buttons;
        protected override VisualElement DefaultFocus => _buttons.Count > 0 ? _buttons[0] : _choices;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _body = Q<Label>("body"); _choices = Q<VisualElement>("choices");
            _buttons.Clear();
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(NarrativePopupData d)
        {
            _title.text = d.Title; _body.text = d.Body;
            _choices.Clear(); _buttons.Clear();
            foreach (var c in d.Choices)
            {
                var b = new UnityEngine.UIElements.Button { text = c.Label };
                b.AddToClassList("hd-btn");
                b.AddToClassList(c.Tone == Tone.Danger ? "hd-btn--danger" : c.Tone == Tone.Muted ? "hd-btn--secondary" : "hd-btn--primary");
                var choice = c.Choice;
                b.clicked += () => Choose(choice);
                _buttons.Add(b); _choices.Add(b);
            }
        }

        public UniTask<NarrativeChoice> ChooseAsync(NarrativePopupData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _choice = new UniTaskCompletionSource<NarrativeChoice>();
            ct.Register(() => _choice.TrySetCanceled());
            return _choice.Task;
        }

        void Choose(NarrativeChoice c)
        {
            foreach (var b in _buttons) b.SetEnabled(false);
            _choice?.TrySetResult(c);
        }
    }
}
