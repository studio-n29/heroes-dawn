#nullable enable
using System;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Fenêtre Compte (UI Toolkit) : onglets Connexion / Créer un compte, champs e-mail, mot de passe et pseudo, Valider, Google, Apple,
    /// mot de passe oublié, Fermer. Retour = Fermer. La vue relaie les clics à <see cref="AccountPopupController"/>.
    /// </summary>
    public sealed class AccountPopupView : HdPopup, IAccountPopupView
    {
        Label _title = null!, _message = null!, _busyText = null!;
        UnityEngine.UIElements.Button _tabIn = null!, _tabUp = null!, _submit = null!, _google = null!, _apple = null!, _forgot = null!, _close = null!, _busyCancel = null!;
        TextField _email = null!, _password = null!, _name = null!;
        VisualElement _busy = null!;

        public event Action<AccountMode>? ModeChanged;
        public event Action<AccountForm>? SubmitClicked;
        public event Action<AuthProvider>? ProviderClicked;
        public event Action<string>? ForgotClicked;
        public event Action? CancelClicked;
        public event Action? CloseClicked;

        public override void RequestCancel() => CloseClicked?.Invoke();
        protected override VisualElement DefaultFocus => _email;

        // Accès des tests PlayMode (clics et saisie réels).
        public HdFieldRef EmailField { get; private set; } = null!;
        public HdFieldRef PasswordField { get; private set; } = null!;
        public HdFieldRef NameField { get; private set; } = null!;
        public HdButtonRef SubmitButton { get; private set; } = null!;
        public HdButtonRef TabSignInButton { get; private set; } = null!;
        public HdButtonRef TabSignUpButton { get; private set; } = null!;
        public HdButtonRef GoogleButton { get; private set; } = null!;
        public HdButtonRef AppleButton { get; private set; } = null!;
        public HdButtonRef ForgotButton { get; private set; } = null!;
        public HdButtonRef CloseButton { get; private set; } = null!;
        public string MessageText => _message.text;
        public bool NameRowVisible => _name.style.display != DisplayStyle.None;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _message = Q<Label>("message"); _busyText = Q<Label>("busyText");
            _tabIn = Q<UnityEngine.UIElements.Button>("tabSignIn"); _tabUp = Q<UnityEngine.UIElements.Button>("tabSignUp");
            _submit = Q<UnityEngine.UIElements.Button>("submit"); _google = Q<UnityEngine.UIElements.Button>("google"); _apple = Q<UnityEngine.UIElements.Button>("apple");
            _forgot = Q<UnityEngine.UIElements.Button>("forgot"); _close = Q<UnityEngine.UIElements.Button>("close"); _busyCancel = Q<UnityEngine.UIElements.Button>("busyCancel");
            _email = Q<TextField>("email"); _password = Q<TextField>("password"); _name = Q<TextField>("name");
            _busy = Q<VisualElement>("busy");
            EmailField = new HdFieldRef(_email); PasswordField = new HdFieldRef(_password); NameField = new HdFieldRef(_name);
            SubmitButton = new HdButtonRef(_submit, this); TabSignInButton = new HdButtonRef(_tabIn, this); TabSignUpButton = new HdButtonRef(_tabUp, this);
            GoogleButton = new HdButtonRef(_google, this); AppleButton = new HdButtonRef(_apple, this);
            ForgotButton = new HdButtonRef(_forgot, this); CloseButton = new HdButtonRef(_close, this);

            _tabIn.clicked += () => ModeChanged?.Invoke(AccountMode.SignIn);
            _tabUp.clicked += () => ModeChanged?.Invoke(AccountMode.SignUp);
            _submit.clicked += () => SubmitClicked?.Invoke(new AccountForm { Email = _email.value, Password = _password.value, DisplayName = _name.value });
            _google.clicked += () => ProviderClicked?.Invoke(AuthProvider.Google);
            _apple.clicked += () => ProviderClicked?.Invoke(AuthProvider.Apple);
            _forgot.clicked += () => ForgotClicked?.Invoke(_email.value);
            _close.clicked += () => CloseClicked?.Invoke();
            _busyCancel.clicked += () => CancelClicked?.Invoke();
            _password.isPasswordField = true;
            _password.maskChar = '•';
        }

        public void Show(AccountPopupData data)
        {
            _tabIn.text = data.TabSignIn; _tabUp.text = data.TabSignUp;
            _email.textEdition.placeholder = data.EmailHint; _password.textEdition.placeholder = data.PasswordHint; _name.textEdition.placeholder = data.NameHint;
            _google.text = data.Google; _apple.text = data.Apple; _forgot.text = data.Forgot; _close.text = data.Close;
            _email.value = ""; _password.value = ""; _name.value = "";
            _message.text = "";
            _busy.style.display = DisplayStyle.None;
            SetMode(data.Mode, data.Title, data.Submit);
        }

        public void SetMode(AccountMode mode, string titleText, string submitLabel)
        {
            _title.text = titleText;
            _submit.text = submitLabel;
            _name.style.display = mode == AccountMode.SignUp ? DisplayStyle.Flex : DisplayStyle.None;
            _tabIn.EnableInClassList("hd-tab--active", mode == AccountMode.SignIn);
            _tabUp.EnableInClassList("hd-tab--active", mode == AccountMode.SignUp);
            _message.text = "";
        }

        public void SetBusy(bool busy, string text)
        {
            _busy.style.display = busy ? DisplayStyle.Flex : DisplayStyle.None;
            _busyText.text = text;
            if (busy) _busyCancel.Focus();
        }

        public void ShowMessage(string text, bool isError)
        {
            _message.text = text;
            _message.EnableInClassList("hd-form__message--error", isError);
            _message.EnableInClassList("hd-form__message--ok", !isError);
        }
    }
}
