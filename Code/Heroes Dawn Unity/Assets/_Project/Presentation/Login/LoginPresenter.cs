#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Mvp;
using AppLogger = N29.HeroesDawn.Application.Ports.ILogger;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Connexion invité : une seule à la fois ; en cas de succès, messages de bienvenue puis demande du QG
    /// (la garde reste levée jusqu'au prochain affichage) ; en cas d'échec, erreur affichée et Jouer rendu.
    /// </summary>
    public sealed class LoginPresenter : PresenterBase<ILoginView>
    {
        readonly ISignInAsGuestUseCase _signIn;
        readonly ISessionReadModel _session;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly AppLogger _log;
        readonly IAudioService _audio;
        readonly IOnboardingFlow? _onboarding;
        readonly IAccountFlow? _account;
        bool _accountOpen;
        readonly BusyGate _gate = new BusyGate();

        public LoginPresenter(ILoginView view, ISignInAsGuestUseCase signIn, ISessionReadModel session, IPublisher<NavigateRequested> navigate,
                              IToastService toasts, ILocalization loc, AppLogger log, IAudioService? audio = null, IOnboardingFlow? onboarding = null, IAccountFlow? account = null) : base(view, null)
        {
            _signIn = signIn; _session = session; _navigate = navigate; _toasts = toasts; _loc = loc; _log = log;
            _audio = audio ?? NullAudioService.Instance;
            _onboarding = onboarding;
            _account = account;
        }

        protected override void OnStart()
        {
            View.PlayClicked += Play;
            View.AccountClicked += OpenAccount;
        }

        protected override void OnDispose()
        {
            View.PlayClicked -= Play;
            View.AccountClicked -= OpenAccount;
        }

        /// <summary>Ouvre le parcours Compte ; un seul à la fois. Le libellé du bouton suit l'état (« Se connecter » ou l'e-mail du compte).</summary>
        void OpenAccount()
        {
            if (_account == null || _accountOpen) return;
            _accountOpen = true;
            RunAccountAsync().Forget();
        }

        /// <summary>Après la connexion invité : rouvre le compte mémorisé et synchronise, sans bloquer l'entrée dans le jeu.</summary>
        void RestoreAccount()
        {
            if (_account == null) return;
            RestoreAccountAsync().Forget();
        }

        async UniTaskVoid RestoreAccountAsync()
        {
            try { await _account!.RestoreAsync(Lifetime); }
            catch (OperationCanceledException) { }
            catch (Exception e) { _log.Error("Compte : " + e.Message); }
            if (!Disposed) View.SetAccountLabel(_account!.ButtonLabel);
        }

        async UniTaskVoid RunAccountAsync()
        {
            try { await _account!.RunAsync(Lifetime); }
            catch (OperationCanceledException) { }
            catch (Exception e) { _log.Error("Compte : " + e.Message); }
            finally
            {
                _accountOpen = false;
                if (!Disposed) View.SetAccountLabel(_account!.ButtonLabel);
            }
        }

        protected override void OnShown()
        {
            _gate.Exit();
            View.SetBusy(false, "");
            if (_account != null) View.SetAccountLabel(_account.ButtonLabel);
            View.PlayIntro();
        }

        void Play()
        {
            if (!_gate.TryEnter()) return;
            PlayAsync().Forget();
        }

        async UniTaskVoid PlayAsync()
        {
            View.SetBusy(true, _loc.T("login.connecting"));
            try
            {
                var result = await _signIn.ExecuteAsync(Lifetime);
                if (Disposed) return;
                if (result.IsFailure)
                {
                    var error = _loc.Error(result.Error!);
                    View.SetBusy(false, error);
                    _toasts.Show(error, ToastKind.Error);
                    _gate.Exit();
                    return;
                }
                var report = result.Value;
                if (_onboarding != null) await _onboarding.RunIfNeededAsync(Lifetime); // première connexion : cinématique, pseudo, campagne
                if (Disposed) return;
                _audio.Play(SfxId.LoginStart);
                var name = _session.Account?.DisplayName ?? "";
                _toasts.Show(_loc.F(report.NewProfile ? "login.welcome_new" : "login.welcome_back", name), ToastKind.Info);
                if (report.FatigueRecovered > 0) _toasts.Show(_loc.F("login.recovered", report.FatigueRecovered), ToastKind.Reward);
                _navigate.Publish(new NavigateRequested(GameState.Hub));
                RestoreAccount();
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _log.Error("Connexion : " + e);
                if (Disposed) return;
                View.SetBusy(false, _loc.T("login.fail_network"));
                _gate.Exit();
            }
        }
    }
}
