#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Logique de la fenêtre Compte : valide la saisie avant tout appel (e-mail, mot de passe de 8 caractères, pseudo de 3 à 16), un seul envoi à la fois,
    /// erreurs traduites, mot de passe oublié. <see cref="RunAsync"/> rend le résultat du rattachement quand la connexion réussit, ou <c>null</c> quand
    /// la fenêtre est fermée (ou que l'adresse doit d'abord être confirmée : un message l'explique et la fenêtre reste ouverte).
    /// </summary>
    public sealed class AccountPopupController
    {
        readonly IAccountPopupView _view;
        readonly AccountLink _account;
        readonly ILocalization _loc;
        readonly UniTaskCompletionSource<LinkOutcome?> _done = new UniTaskCompletionSource<LinkOutcome?>();
        AccountMode _mode = AccountMode.SignIn;
        bool _busy;
        CancellationToken _ct;
        CancellationTokenSource? _requests;

        public AccountPopupController(IAccountPopupView view, AccountLink account, ILocalization loc)
        {
            _view = view; _account = account; _loc = loc;
        }

        public UniTask<LinkOutcome?> RunAsync(CancellationToken ct)
        {
            _requests = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _ct = _requests.Token;   // fermer la fenêtre annule la requête en cours
            _view.ModeChanged += OnMode;
            _view.SubmitClicked += OnSubmit;
            _view.ProviderClicked += OnProvider;
            _view.ForgotClicked += OnForgot;
            _view.CloseClicked += OnClose;
            ct.Register(() => _done.TrySetCanceled());
            _view.Show(new AccountPopupData
            {
                Mode = _mode,
                Title = _loc.T("account.title.signin"),
                TabSignIn = _loc.T("account.tab.signin"), TabSignUp = _loc.T("account.tab.signup"),
                EmailHint = _loc.T("account.email"), PasswordHint = _loc.T("account.password"), NameHint = _loc.T("account.name"),
                Submit = _loc.T("account.submit.signin"),
                Google = _loc.T("account.google"), Apple = _loc.T("account.apple"), Forgot = _loc.T("account.forgot"), Close = _loc.T("account.close"),
            });
            return Finish();
        }

        async UniTask<LinkOutcome?> Finish()
        {
            try { return await _done.Task; }
            finally
            {
                _view.ModeChanged -= OnMode;
                _view.SubmitClicked -= OnSubmit;
                _view.ProviderClicked -= OnProvider;
                _view.ForgotClicked -= OnForgot;
                _view.CloseClicked -= OnClose;
            }
        }

        void OnClose()
        {
            _requests?.Cancel();
            _done.TrySetResult(null);
        }

        void OnMode(AccountMode mode)
        {
            if (_busy) return;
            _mode = mode;
            _view.SetMode(mode, _loc.T(mode == AccountMode.SignIn ? "account.title.signin" : "account.title.signup"),
                          _loc.T(mode == AccountMode.SignIn ? "account.submit.signin" : "account.submit.signup"));
        }

        static bool LooksLikeEmail(string email)
        {
            string mail = (email ?? "").Trim();
            int at = mail.IndexOf('@');
            return at > 0 && at < mail.Length - 1 && mail.IndexOf('@', at + 1) < 0;
        }

        string? Validate(AccountForm f)
        {
            if (!LooksLikeEmail(f.Email)) return _loc.T("account.error.email");
            if ((f.Password ?? "").Length < 8) return _loc.T("errors.account_password");
            if (_mode == AccountMode.SignUp)
            {
                int n = (f.DisplayName ?? "").Trim().Length;
                if (n < 3 || n > 16) return _loc.T("account.error.name");
            }
            return null;
        }

        void OnSubmit(AccountForm form)
        {
            if (_busy) return;
            var error = Validate(form);
            if (error != null) { _view.ShowMessage(error, true); return; }
            RunAsync(_mode == AccountMode.SignUp
                ? _account.SignUpAsync(form.Email, form.Password, form.DisplayName.Trim(), _ct)
                : _account.SignInAsync(form.Email, form.Password, _ct)).Forget();
        }

        void OnProvider(AuthProvider provider)
        {
            if (_busy) return;
            RunAsync(_account.SignInWithProviderAsync(provider, _ct)).Forget();
        }

        async UniTaskVoid RunAsync(UniTask<Result<LinkOutcome?>> operation)
        {
            _busy = true;
            _view.SetBusy(true, _loc.T("account.busy"));
            try
            {
                var result = await operation;
                if (result.IsFailure) { _view.ShowMessage(_loc.Error(result.Error!), true); return; }
                if (result.Value == null) { _view.ShowMessage(_loc.T("account.confirm_sent"), false); return; }   // adresse à confirmer : la fenêtre reste ouverte
                _done.TrySetResult(result.Value);
            }
            catch (OperationCanceledException) { _done.TrySetResult(null); }
            finally
            {
                _busy = false;
                _view.SetBusy(false, "");
            }
        }

        void OnForgot(string email)
        {
            if (_busy) return;
            if (!LooksLikeEmail(email)) { _view.ShowMessage(_loc.T("account.error.email"), true); return; }
            ForgotAsync(email.Trim()).Forget();
        }

        async UniTaskVoid ForgotAsync(string email)
        {
            _busy = true;
            _view.SetBusy(true, _loc.T("account.busy"));
            try
            {
                var r = await _account.ResetPasswordAsync(email, _ct);
                _view.ShowMessage(r.IsFailure ? _loc.Error(r.Error!) : _loc.T("account.reset_sent"), r.IsFailure);
            }
            catch (OperationCanceledException) { }
            finally { _busy = false; _view.SetBusy(false, ""); }
        }
    }
}
