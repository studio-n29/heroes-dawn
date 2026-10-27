#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Account
{
    public enum AccountMode { SignIn, SignUp }

    /// <summary>Ce que le joueur a saisi dans la fenêtre Compte.</summary>
    public struct AccountForm
    {
        public string Email;
        public string Password;
        public string DisplayName;
    }

    /// <summary>Textes de la fenêtre Compte (tous localisés) et mode initial.</summary>
    public sealed class AccountPopupData
    {
        public AccountMode Mode;
        public string Title = "", TabSignIn = "", TabSignUp = "", EmailHint = "", PasswordHint = "", NameHint = "";
        public string Submit = "", Google = "", Apple = "", Forgot = "", Close = "";
    }

    /// <summary>
    /// Fenêtre Compte (prefab Popups/AccountPopup) : deux onglets (Connexion, Créer un compte), e-mail, mot de passe, pseudo (création),
    /// Google, Apple, mot de passe oublié. La vue ne décide de rien : elle relaie les clics et affiche ce qu'on lui dit.
    /// </summary>
    public interface IAccountPopupView : IPopupView
    {
        event Action<AccountMode>? ModeChanged;
        event Action<AccountForm>? SubmitClicked;
        event Action<AuthProvider>? ProviderClicked;
        event Action<string>? ForgotClicked;
        event Action? CloseClicked;

        void Show(AccountPopupData data);
        void SetMode(AccountMode mode, string title, string submit);
        void SetBusy(bool busy, string text);
        void ShowMessage(string text, bool isError);
    }

    /// <summary>Une des deux progressions à comparer : intitulé et chiffres.</summary>
    public struct ProgressCardData
    {
        public string Header;
        public string Summary;
    }

    public sealed class ProgressChoiceData
    {
        public string Title = "", Hint = "", KeepDevice = "", KeepAccount = "", Later = "";
        public ProgressCardData Device, Account;
    }

    /// <summary>Choix de progression : « Garder cet appareil », « Garder le compte », ou Retour (nul : rien n'est écrit).</summary>
    public interface IProgressChoicePopupView : IPopupView
    {
        UniTask<ChoiceKind?> AskAsync(ProgressChoiceData data, CancellationToken ct);
    }
}
