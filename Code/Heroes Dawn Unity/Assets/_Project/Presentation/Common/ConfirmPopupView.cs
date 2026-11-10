#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Confirmation (UI Toolkit, planche 16.2) : titre, texte, Annuler / Valider. Retour (B, Échap) = Annuler.</summary>
    public sealed class ConfirmPopupView : HdPopup, IConfirmPopupView
    {
        UnityEngine.UIElements.Button _ok = null!;
        UnityEngine.UIElements.Button _cancel = null!;
        Label _title = null!;
        Label _body = null!;
        UniTaskCompletionSource<bool>? _answer;

        public HdButtonRef OkButton { get; private set; } = null!;
        public HdButtonRef CancelButton { get; private set; } = null!;

        protected override VisualElement DefaultFocus => _ok;

        protected override void Bind()
        {
            _title = Q<Label>("title");
            _body = Q<Label>("body");
            _ok = Q<UnityEngine.UIElements.Button>("ok");
            _cancel = Q<UnityEngine.UIElements.Button>("cancel");
            OkButton = new HdButtonRef(_ok, this);
            CancelButton = new HdButtonRef(_cancel, this);
            _ok.clicked += () => _answer?.TrySetResult(true);
            _cancel.clicked += () => _answer?.TrySetResult(false);
        }

        public UniTask<bool> AskAsync(ConfirmData d, CancellationToken ct)
        {
            _title.text = d.Title;
            _body.text = d.Body;
            _ok.text = d.Ok;
            // Une action risquée (quitter, supprimer) prend le bouton rouge ; sinon le bouton principal ivoire.
            _ok.EnableInClassList("hd-btn--danger", d.OkTone == Tone.Danger);
            _ok.EnableInClassList("hd-btn--primary", d.OkTone != Tone.Danger);
            _cancel.text = d.Cancel;
            _cancel.style.display = string.IsNullOrEmpty(d.Cancel) ? DisplayStyle.None : DisplayStyle.Flex; // un message simple n'a qu'un bouton
            _answer = new UniTaskCompletionSource<bool>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public override void RequestCancel() => _answer?.TrySetResult(false);
    }
}
