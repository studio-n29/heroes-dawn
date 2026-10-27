#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>Parcours Compte, utilisé par l'écran titre et par le profil.</summary>
    public interface IAccountFlow
    {
        /// <summary>« Se connecter » pour un invité, l'e-mail du compte pour un joueur connecté.</summary>
        string ButtonLabel { get; }
        UniTask RunAsync(CancellationToken ct);
        UniTask SignOutAsync(CancellationToken ct);
        UniTask SyncNowAsync(CancellationToken ct);
    }

    /// <summary>
    /// Enchaîne la fenêtre Compte, le choix de progression (quand l'appareil et le compte ont chacun avancé) et les messages de résultat.
    /// Retour sur le choix n'écrit rien : le choix reste en attente. Un conflit détecté par la synchronisation rouvre le même choix.
    /// </summary>
    public sealed class AccountFlow : IAccountFlow
    {
        const int MaxChoiceRounds = 3;

        readonly IModalService _modals;
        readonly AccountLink _account;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        bool _choiceOpen;

        public AccountFlow(IModalService modals, AccountLink account, IToastService toasts, ILocalization loc)
        {
            _modals = modals; _account = account; _toasts = toasts; _loc = loc;
            _account.ChoiceRequired += outcome => ChooseAsync(outcome, CancellationToken.None).Forget();
        }

        public string ButtonLabel
        {
            get
            {
                var cloud = _account.Cloud;
                if (cloud == null) return _loc.T("login.account");
                return string.IsNullOrEmpty(cloud.Email) ? cloud.DisplayName : cloud.Email;
            }
        }

        public async UniTask RunAsync(CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IAccountPopupView>(ct);
            LinkOutcome? outcome;
            try { outcome = await new AccountPopupController(view, _account, _loc).RunAsync(ct); }
            finally { _modals.Close(view); }
            if (outcome == null) return;
            await ShowOutcomeAsync(outcome, ct);
        }

        async UniTask ShowOutcomeAsync(LinkOutcome outcome, CancellationToken ct)
        {
            switch (outcome.Status)
            {
                case LinkStatus.Uploaded: _toasts.Show(_loc.T("account.linked"), ToastKind.Reward); break;
                case LinkStatus.Pulled: _toasts.Show(_loc.T("account.pulled"), ToastKind.Reward); break;
                case LinkStatus.NeedsChoice: await ChooseAsync(outcome, ct); break;
            }
        }

        async UniTask ChooseAsync(LinkOutcome outcome, CancellationToken ct)
        {
            if (_choiceOpen) return;
            _choiceOpen = true;
            try
            {
                for (int round = 0; round < MaxChoiceRounds; round++)
                {
                    var view = await _modals.OpenAsync<IProgressChoicePopupView>(ct);
                    ChoiceKind? choice;
                    try { choice = await view.AskAsync(ChoiceData(outcome), ct); }
                    finally { _modals.Close(view); }
                    if (choice == null) { _toasts.Show(_loc.T("account.choice_later"), ToastKind.Info); return; }
                    var r = await _account.ResolveAsync(choice.Value, ct);
                    if (r.IsFailure) { _toasts.Show(_loc.Error(r.Error!), ToastKind.Error); return; }
                    if (r.Value!.Status == LinkStatus.NeedsChoice) { outcome = r.Value; continue; }   // le compte a encore changé : on repropose
                    await ShowOutcomeAsync(r.Value, ct);
                    return;
                }
            }
            catch (OperationCanceledException) { }
            finally { _choiceOpen = false; }
        }

        ProgressChoiceData ChoiceData(LinkOutcome o) => new ProgressChoiceData
        {
            Title = _loc.T("account.choice.title"),
            Hint = _loc.T("account.choice.hint"),
            KeepDevice = _loc.T("account.choice.keep_device"),
            KeepAccount = _loc.T("account.choice.keep_account"),
            Later = _loc.T("account.choice.later"),
            Device = new ProgressCardData { Header = _loc.T("account.choice.device"), Summary = Summary(o.Local!) },
            Account = new ProgressCardData { Header = _loc.T("account.choice.account"), Summary = Summary(o.Cloud!) },
        };

        string Summary(ProfileSummary s)
        {
            string date = s.LastSeenUtc == DateTime.MinValue ? "—" : s.LastSeenUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            return _loc.F("account.choice.summary", s.Level, s.Credits, s.ZonesCleared, date);
        }

        public async UniTask SignOutAsync(CancellationToken ct)
        {
            bool yes = await _modals.ConfirmAsync(new ConfirmData(_loc.T("account.signout.title"), _loc.T("account.signout.body"), _loc.T("account.signout.ok"), _loc.T("ui.cancel"), Tone.Danger), ct);
            if (!yes) return;
            await _account.SignOutAsync(ct);
            _toasts.Show(_loc.T("account.signed_out"), ToastKind.Info);
        }

        public async UniTask SyncNowAsync(CancellationToken ct)
        {
            var r = await _account.SyncNowAsync(ct);
            _toasts.Show(r.IsFailure ? _loc.Error(r.Error!) : _loc.T("account.sync_done"), r.IsFailure ? ToastKind.Error : ToastKind.Reward);
        }
    }
}
