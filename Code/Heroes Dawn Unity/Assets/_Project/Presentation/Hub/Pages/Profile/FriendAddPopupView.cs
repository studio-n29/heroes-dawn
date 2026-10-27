#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Saisie du tag d'un ami (popup) : renvoie le texte saisi, ou null si le joueur annule.</summary>
    public interface IFriendAddPopupView : IPopupView
    {
        UniTask<string?> AskAsync(string title, string hint, string ok, string cancel, CancellationToken ct);
    }

    /// <summary>Popup (prefab) : un champ de texte sur une ligne, Ajouter / Annuler. Retour = Annuler.</summary>
    public sealed class FriendAddPopupView : MonoBehaviour, IFriendAddPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_InputField field = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] UiButton ok = null!;
        [SerializeField] UiButton cancel = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        UniTaskCompletionSource<string?>? _answer;

        public bool Cancelable => true;
        public TMP_InputField Field => field;
        public UiButton OkButton => ok;
        public UiButton CancelButton => cancel;

        void Awake()
        {
            ok.Button.onClick.AddListener(() => _answer?.TrySetResult(field.text));
            cancel.Button.onClick.AddListener(() => _answer?.TrySetResult(null));
        }

        public UniTask<string?> AskAsync(string titleText, string hintText, string okLabel, string cancelLabel, CancellationToken ct)
        {
            title.text = titleText;
            hint.text = hintText;
            ok.SetLabel(okLabel);
            cancel.SetLabel(cancelLabel);
            field.text = "";
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _answer = new UniTaskCompletionSource<string?>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public void RequestCancel() => _answer?.TrySetResult(null);
    }
}
