#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Fenêtre (UI Toolkit) : deux cartes (cet appareil, le compte) avec leurs chiffres et un bouton chacune, plus « Plus tard ».
    /// Retour = Plus tard : rien n'est écrit.
    /// </summary>
    public sealed class ProgressChoicePopupView : HdPopup, IProgressChoicePopupView
    {
        Label _title = null!, _hint = null!, _deviceHeader = null!, _deviceSummary = null!, _accountHeader = null!, _accountSummary = null!;
        UnityEngine.UIElements.Button _keepDevice = null!, _keepAccount = null!, _later = null!;
        UniTaskCompletionSource<ChoiceKind?>? _answer;

        public HdButtonRef KeepDeviceButton { get; private set; } = null!;
        public HdButtonRef KeepAccountButton { get; private set; } = null!;
        public HdButtonRef LaterButton { get; private set; } = null!;

        protected override VisualElement DefaultFocus => _keepAccount;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _hint = Q<Label>("hint");
            _deviceHeader = Q<Label>("deviceHeader"); _deviceSummary = Q<Label>("deviceSummary");
            _accountHeader = Q<Label>("accountHeader"); _accountSummary = Q<Label>("accountSummary");
            _keepDevice = Q<UnityEngine.UIElements.Button>("keepDevice"); _keepAccount = Q<UnityEngine.UIElements.Button>("keepAccount"); _later = Q<UnityEngine.UIElements.Button>("later");
            KeepDeviceButton = new HdButtonRef(_keepDevice, this); KeepAccountButton = new HdButtonRef(_keepAccount, this); LaterButton = new HdButtonRef(_later, this);
            _keepDevice.clicked += () => _answer?.TrySetResult(ChoiceKind.KeepDevice);
            _keepAccount.clicked += () => _answer?.TrySetResult(ChoiceKind.KeepAccount);
            _later.clicked += () => _answer?.TrySetResult(null);
        }

        public UniTask<ChoiceKind?> AskAsync(ProgressChoiceData data, CancellationToken ct)
        {
            _title.text = data.Title;
            _hint.text = data.Hint;
            _deviceHeader.text = data.Device.Header;
            _deviceSummary.text = data.Device.Summary;
            _accountHeader.text = data.Account.Header;
            _accountSummary.text = data.Account.Summary;
            _keepDevice.text = data.KeepDevice;
            _keepAccount.text = data.KeepAccount;
            _later.text = data.Later;
            _answer = new UniTaskCompletionSource<ChoiceKind?>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public override void RequestCancel() => _answer?.TrySetResult(null);
    }
}
