#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Popup (prefab Popups/AccountPopup) : onglets Connexion / Créer un compte, champs e-mail, mot de passe et pseudo, Valider, Google, Apple,
    /// mot de passe oublié, Fermer. Retour = Fermer. La vue relaie les clics à <see cref="AccountPopupController"/>.
    /// </summary>
    public sealed class AccountPopupView : MonoBehaviour, IAccountPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] UiButton tabSignIn = null!;
        [SerializeField] UiButton tabSignUp = null!;
        [SerializeField] TMP_InputField emailField = null!;
        [SerializeField] TMP_InputField passwordField = null!;
        [SerializeField] TMP_InputField nameField = null!;
        [SerializeField] TMP_Text emailHint = null!;
        [SerializeField] TMP_Text passwordHint = null!;
        [SerializeField] TMP_Text nameHint = null!;
        [SerializeField] GameObject nameRow = null!;
        [SerializeField] TMP_Text message = null!;
        [SerializeField] UiButton submit = null!;
        [SerializeField] UiButton google = null!;
        [SerializeField] UiButton apple = null!;
        [SerializeField] UiButton forgot = null!;
        [SerializeField] UiButton close = null!;
        [SerializeField] GameObject busyOverlay = null!;
        [SerializeField] TMP_Text busyText = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        public event Action<AccountMode>? ModeChanged;
        public event Action<AccountForm>? SubmitClicked;
        public event Action<AuthProvider>? ProviderClicked;
        public event Action<string>? ForgotClicked;
        public event Action? CloseClicked;

        public bool Cancelable => true;
        public void RequestCancel() => CloseClicked?.Invoke();

        // Accès des tests PlayMode (clics et saisie réels).
        public TMP_InputField EmailField => emailField;
        public TMP_InputField PasswordField => passwordField;
        public TMP_InputField NameField => nameField;
        public UiButton SubmitButton => submit;
        public UiButton TabSignInButton => tabSignIn;
        public UiButton TabSignUpButton => tabSignUp;
        public UiButton GoogleButton => google;
        public UiButton AppleButton => apple;
        public UiButton ForgotButton => forgot;
        public UiButton CloseButton => close;
        public string MessageText => message.text;
        public bool NameRowVisible => nameRow.activeSelf;

        void Awake()
        {
            tabSignIn.Button.onClick.AddListener(() => ModeChanged?.Invoke(AccountMode.SignIn));
            tabSignUp.Button.onClick.AddListener(() => ModeChanged?.Invoke(AccountMode.SignUp));
            submit.Button.onClick.AddListener(() => SubmitClicked?.Invoke(new AccountForm { Email = emailField.text, Password = passwordField.text, DisplayName = nameField.text }));
            google.Button.onClick.AddListener(() => ProviderClicked?.Invoke(AuthProvider.Google));
            apple.Button.onClick.AddListener(() => ProviderClicked?.Invoke(AuthProvider.Apple));
            forgot.Button.onClick.AddListener(() => ForgotClicked?.Invoke(emailField.text));
            close.Button.onClick.AddListener(() => CloseClicked?.Invoke());
        }

        public void Show(AccountPopupData data)
        {
            tabSignIn.SetLabel(data.TabSignIn);
            tabSignUp.SetLabel(data.TabSignUp);
            emailHint.text = data.EmailHint;
            passwordHint.text = data.PasswordHint;
            nameHint.text = data.NameHint;
            google.SetLabel(data.Google);
            apple.SetLabel(data.Apple);
            forgot.SetLabel(data.Forgot);
            close.SetLabel(data.Close);
            emailField.text = ""; passwordField.text = ""; nameField.text = "";
            passwordField.contentType = TMP_InputField.ContentType.Password;
            message.text = "";
            busyOverlay.SetActive(false);
            SetMode(data.Mode, data.Title, data.Submit);
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
        }

        public void SetMode(AccountMode mode, string titleText, string submitLabel)
        {
            title.text = titleText;
            submit.SetLabel(submitLabel);
            nameRow.SetActive(mode == AccountMode.SignUp);
            tabSignIn.Interactable = mode != AccountMode.SignIn;
            tabSignUp.Interactable = mode != AccountMode.SignUp;
            message.text = "";
        }

        public void SetBusy(bool busy, string text)
        {
            busyOverlay.SetActive(busy);
            busyText.text = text;
        }

        public void ShowMessage(string text, bool isError)
        {
            message.text = text;
            message.color = isError ? Theme.Danger : Theme.Success;
        }
    }
}
