#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Popup (prefab Popups/ProgressChoicePopup) : deux cartes (cet appareil, le compte) avec leurs chiffres et un bouton chacune, plus « Plus tard ».
    /// Retour = Plus tard : rien n'est écrit.
    /// </summary>
    public sealed class ProgressChoicePopupView : MonoBehaviour, IProgressChoicePopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] TMP_Text deviceHeader = null!;
        [SerializeField] TMP_Text deviceSummary = null!;
        [SerializeField] TMP_Text accountHeader = null!;
        [SerializeField] TMP_Text accountSummary = null!;
        [SerializeField] UiButton keepDevice = null!;
        [SerializeField] UiButton keepAccount = null!;
        [SerializeField] UiButton later = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        UniTaskCompletionSource<ChoiceKind?>? _answer;

        public bool Cancelable => true;
        public UiButton KeepDeviceButton => keepDevice;
        public UiButton KeepAccountButton => keepAccount;
        public UiButton LaterButton => later;

        void Awake()
        {
            keepDevice.Button.onClick.AddListener(() => _answer?.TrySetResult(ChoiceKind.KeepDevice));
            keepAccount.Button.onClick.AddListener(() => _answer?.TrySetResult(ChoiceKind.KeepAccount));
            later.Button.onClick.AddListener(() => _answer?.TrySetResult(null));
        }

        public UniTask<ChoiceKind?> AskAsync(ProgressChoiceData data, CancellationToken ct)
        {
            title.text = data.Title;
            hint.text = data.Hint;
            deviceHeader.text = data.Device.Header;
            deviceSummary.text = data.Device.Summary;
            accountHeader.text = data.Account.Header;
            accountSummary.text = data.Account.Summary;
            keepDevice.SetLabel(data.KeepDevice);
            keepAccount.SetLabel(data.KeepAccount);
            later.SetLabel(data.Later);
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _answer = new UniTaskCompletionSource<ChoiceKind?>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public void RequestCancel() => _answer?.TrySetResult(null);
    }
}
