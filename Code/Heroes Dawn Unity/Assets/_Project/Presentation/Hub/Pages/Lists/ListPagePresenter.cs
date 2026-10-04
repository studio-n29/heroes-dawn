#nullable enable
using System;
using System.Collections.Generic;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Mise en forme des récompenses et des durées pour les pages de méta-progression.</summary>
    public static class MetaFormat
    {
        public static string RewardText(Reward r, ILocalization loc)
        {
            var parts = new List<string>(4);
            if (r.Credits != 0) parts.Add(r.Credits + " CR");
            if (r.Pr != 0) parts.Add(r.Pr + " PR");
            if (r.Gems != 0) parts.Add(r.Gems + " " + loc.T("ui.gems_short"));
            if (r.PassXp != 0) parts.Add(r.PassXp + " " + loc.T("ui.xp_short"));
            return parts.Count == 0 ? "—" : string.Join(" · ", parts);
        }

        /// <summary>« 2 j 04 h », « 3 h 12 », « 12 min », « 45 s ».</summary>
        public static string Duration(TimeSpan t)
        {
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;
            if (t.TotalDays >= 1) return $"{(int)t.TotalDays} j {t.Hours:00} h";
            if (t.TotalHours >= 1) return $"{(int)t.TotalHours} h {t.Minutes:00}";
            if (t.TotalMinutes >= 1) return $"{(int)t.TotalMinutes} min";
            return $"{(int)Math.Ceiling(t.TotalSeconds)} s";
        }
    }

    /// <summary>
    /// Base des pages « liste » : se rafraîchit à chaque changement de méta-progression, de portefeuille ou d'effectif,
    /// et une fois par seconde quand la page affiche un minuteur. Les services de méta-progression sont synchrones.
    /// </summary>
    public abstract class ListPagePresenter<TView> : PresenterBase<TView>, IPageTitle, ITickable where TView : class, IListPageView
    {
        readonly ISubscriber<MetaChanged> _meta;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly IClock _clock;
        IDisposable? _subs;
        int _lastSecond = -1;

        protected ILocalization Loc { get; }
        protected IToastService Toasts { get; }

        protected ListPagePresenter(TView view, ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta,
                                    ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, null)
        {
            Loc = loc; Toasts = toasts; _meta = meta; _wallet = wallet; _roster = roster; _clock = clock;
        }

        public abstract string Title { get; }

        /// <summary>Calcule et affiche l'en-tête, les lignes et la colonne de droite.</summary>
        protected abstract void Render();

        /// <summary>Vrai si la page affiche un compte à rebours (rafraîchie chaque seconde).</summary>
        protected virtual bool HasTimers => false;

        protected virtual void OnHeader(int index) { }
        protected virtual void OnRowButton(int row, int button) { }
        protected virtual void OnRowSelected(int row) { }
        protected virtual void OnSideButton(int row, int button) { }
        protected virtual void OnSideRowSelected(int row) { }
        protected virtual void OnPageShown() { }

        protected override void OnStart()
        {
            View.HeaderClicked += OnHeader;
            View.RowButtonClicked += OnRowButton;
            View.RowSelected += OnRowSelected;
            View.SideButtonClicked += OnSideButton;
            View.SideRowSelected += OnSideRowSelected;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _meta.Subscribe(_ => Refresh()).AddTo(bag);
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            OnPageShown();
            Refresh();
        }

        protected override void OnHidden()
        {
            _subs?.Dispose();
            _subs = null;
        }

        protected override void OnDispose()
        {
            _subs?.Dispose();
            View.HeaderClicked -= OnHeader;
            View.RowButtonClicked -= OnRowButton;
            View.RowSelected -= OnRowSelected;
            View.SideButtonClicked -= OnSideButton;
            View.SideRowSelected -= OnSideRowSelected;
        }

        public void Tick()
        {
            if (Disposed || !View.IsVisible || !HasTimers) return;
            int second = _clock.UtcNow.Second + _clock.UtcNow.Minute * 60;
            if (second != _lastSecond) Refresh();
        }

        protected void Refresh()
        {
            if (Disposed) return;
            _lastSecond = _clock.UtcNow.Second + _clock.UtcNow.Minute * 60;
            Render();
        }

        /// <summary>Succès : toast de récompense ; échec : toast d'erreur. Renvoie true en cas de succès.</summary>
        protected bool Report(Result r, string? successText = null)
        {
            if (r.IsFailure) { Toasts.Show(Loc.Error(r.Error!), ToastKind.Error); return false; }
            if (successText != null) Toasts.Show(successText, ToastKind.Reward);
            return true;
        }

        protected bool Report(Result<Reward> r, string sourceKey)
        {
            if (r.IsFailure) { Toasts.Show(Loc.Error(r.Error!), ToastKind.Error); return false; }
            Toasts.Show(Loc.F(sourceKey, MetaFormat.RewardText(r.Value, Loc)), ToastKind.Reward);
            return true;
        }
    }
}
