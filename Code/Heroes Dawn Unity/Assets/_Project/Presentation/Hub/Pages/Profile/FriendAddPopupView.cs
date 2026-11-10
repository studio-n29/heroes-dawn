#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Saisie du tag d'un ami (popup) : renvoie le texte saisi, ou null si le joueur annule.</summary>
    public interface IFriendAddPopupView : IPopupView
    {
        UniTask<string?> AskAsync(string title, string hint, string ok, string cancel, CancellationToken ct);
    }

    /// <summary>Fenêtre UI Toolkit : un champ de texte sur une ligne, Ajouter / Annuler. Retour = Annuler.</summary>
    public sealed class FriendAddPopupView : HdPopup, IFriendAddPopupView
    {
        Label _title = null!, _hint = null!;
        TextField _field = null!;
        UnityEngine.UIElements.Button _ok = null!, _cancel = null!;
        UniTaskCompletionSource<string?>? _answer;
        (string Title, string Hint, string Ok, string Cancel)? _last;

        public override bool Cancelable => true;
        public TextField Field => _field;
        public HdButtonRef OkButton { get; private set; } = null!;
        public HdButtonRef CancelButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _field;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _hint = Q<Label>("hint"); _field = Q<TextField>("field");
            _ok = Q<UnityEngine.UIElements.Button>("ok"); _cancel = Q<UnityEngine.UIElements.Button>("cancel");
            OkButton = new HdButtonRef(_ok, this); CancelButton = new HdButtonRef(_cancel, this);
            _ok.clicked += () => _answer?.TrySetResult(_field.value);
            _cancel.clicked += () => _answer?.TrySetResult(null);
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill((string Title, string Hint, string Ok, string Cancel) d)
        {
            _title.text = d.Title; _hint.text = d.Hint; _ok.text = d.Ok; _cancel.text = d.Cancel;
            _field.value = "";
        }

        public UniTask<string?> AskAsync(string titleText, string hintText, string okLabel, string cancelLabel, CancellationToken ct)
        {
            _last = (titleText, hintText, okLabel, cancelLabel);
            if (_title != null) Fill(_last.Value);
            _answer = new UniTaskCompletionSource<string?>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public override void RequestCancel() => _answer?.TrySetResult(null);
    }
}
