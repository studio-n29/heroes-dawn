#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Confirmation (popup prefab) : titre, texte, Annuler / Valider. Retour = Annuler.</summary>
    public sealed class ConfirmPopupView : MonoBehaviour, IConfirmPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text body = null!;
        [SerializeField] UiButton ok = null!;
        [SerializeField] UiButton cancel = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;
        UniTaskCompletionSource<bool>? _answer;

        public bool Cancelable => true;
        public UiButton OkButton => ok;
        public UiButton CancelButton => cancel;

        void Awake()
        {
            ok.Button.onClick.AddListener(() => _answer?.TrySetResult(true));
            cancel.Button.onClick.AddListener(() => _answer?.TrySetResult(false));
        }

        public UniTask<bool> AskAsync(ConfirmData d, CancellationToken ct)
        {
            title.text = d.Title;
            body.text = d.Body;
            ok.SetLabel(d.Ok);
            ok.SetColors(ToneColors.Of(d.OkTone), Theme.TextDark);
            cancel.SetLabel(d.Cancel);
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _answer = new UniTaskCompletionSource<bool>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public void RequestCancel() => _answer?.TrySetResult(false);
    }
}
