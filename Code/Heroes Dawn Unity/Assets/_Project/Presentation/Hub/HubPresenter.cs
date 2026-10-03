#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Domain.Shop;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// QG : construit les cartes et la salle des opérations depuis la session, exécute les actions une à une
    /// (erreur affichée, message de réussite), déploie l'escouade une seule fois puis demande le plateau.
    /// </summary>
    public sealed class HubPresenter : PresenterBase<IHubView>, VContainer.Unity.ITickable
    {
        readonly ISessionReadModel _session;
        readonly ZoneId _firstZone;
        readonly IToggleSquadUseCase _toggleSquad;
        readonly ITrainMemberUseCase _train;
        readonly IOpenShopUseCase _openShop;
        readonly IRecruitFromShopUseCase _recruit;
        readonly IRerollShopUseCase _reroll;
        readonly IClock _clock;
        readonly ISubscriber<ShopChanged> _shopChanged;
        readonly IHealRosterUseCase _heal;
        readonly IStartZoneUseCase _startZone;
        readonly ISignOutUseCase _signOut;
        readonly IResetProgressUseCase _reset;
        readonly ISubscriber<RosterChanged> _rosterChanged;
        readonly ISubscriber<WalletChanged> _walletChanged;
        readonly ISubscriber<SaveStateChanged> _saving;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;

        readonly BusyGate _acting = new BusyGate();
        readonly BusyGate _deploying = new BusyGate();
        HubTab _tab = HubTab.Team;
        WalletBinder? _wallet;
        IDisposable? _subs;

        /// <summary>Nombre de rafraîchissements (diagnostic des abonnements, lu par les tests PlayMode).</summary>
        public int RefreshCount { get; private set; }

        public HubPresenter(IHubView view, ISessionReadModel session, ZoneId firstZone,
                            IToggleSquadUseCase toggleSquad, ITrainMemberUseCase train, IHealRosterUseCase heal,
                            IOpenShopUseCase openShop, IRecruitFromShopUseCase recruit, IRerollShopUseCase reroll, IClock clock,
                            IStartZoneUseCase startZone, ISignOutUseCase signOut, IResetProgressUseCase reset,
                            ISubscriber<RosterChanged> rosterChanged, ISubscriber<WalletChanged> walletChanged, ISubscriber<SaveStateChanged> saving,
                            ISubscriber<ShopChanged> shopChanged, IPublisher<NavigateRequested> navigate,
                            IModalService modals, IToastService toasts, ILocalization loc)
            : base(view, null)
        {
            _session = session; _firstZone = firstZone; _toggleSquad = toggleSquad; _train = train; _heal = heal;
            _openShop = openShop; _recruit = recruit; _reroll = reroll; _clock = clock; _shopChanged = shopChanged;
            _startZone = startZone; _signOut = signOut; _reset = reset; _rosterChanged = rosterChanged; _walletChanged = walletChanged;
            _saving = saving; _navigate = navigate; _modals = modals; _toasts = toasts; _loc = loc;
        }

        int _lastMinute = -1;

        /// <summary>Une fois par minute, tant que le QG est affiché : rouvre la boutique (nouveau tirage à 9h00) et met à jour le compte à rebours.</summary>
        public void Tick()
        {
            if (Disposed || !View.IsVisible) return;
            var now = _clock.LocalNow;
            int minute = now.Hour * 60 + now.Minute;
            if (minute == _lastMinute) return;
            _lastMinute = minute;
            OpenShop();
            // Seul le compte à rebours bouge : l'onglet Équipe n'est pas reconstruit (un clic au changement de minute serait perdu).
            if (_tab == HubTab.Recruit) Refresh();
        }

        void OpenShop() => _openShop.ExecuteAsync(Lifetime).Forget();

        protected override void OnStart()
        {
            _wallet = new WalletBinder(View.Wallet, _session, _walletChanged, _saving);
            View.TabClicked += SetTab;
            View.LogoutClicked += Logout;
            View.ResetClicked += Reset;
            View.HealClicked += Heal;
            View.DeployClicked += Deploy;
            View.SquadToggleClicked += ToggleSquad;
            View.TrainClicked += Train;
            View.RecruitClicked += Recruit;
            View.RerollClicked += Reroll;
        }

        protected override void OnDispose()
        {
            _subs?.Dispose();
            _wallet?.Dispose();
            View.TabClicked -= SetTab;
            View.LogoutClicked -= Logout;
            View.ResetClicked -= Reset;
            View.HealClicked -= Heal;
            View.DeployClicked -= Deploy;
            View.SquadToggleClicked -= ToggleSquad;
            View.TrainClicked -= Train;
            View.RecruitClicked -= Recruit;
            View.RerollClicked -= Reroll;
        }

        protected override void OnShown()
        {
            _deploying.Exit();
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _rosterChanged.Subscribe(_ => Refresh()).AddTo(bag);
            _walletChanged.Subscribe(_ => Refresh()).AddTo(bag);
            _shopChanged.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            var now = _clock.LocalNow;
            _lastMinute = now.Hour * 60 + now.Minute;
            OpenShop();
            Refresh();
        }

        protected override void OnHidden()
        {
            _subs?.Dispose();
            _subs = null;
        }

        void SetTab(HubTab t)
        {
            _tab = t;
            Refresh();
            View.ScrollToTop();
        }

        // ------------------------------------------------------------------ Données

        void Refresh()
        {
            if (Disposed) return;
            RefreshCount++;
            var s = _session;
            var d = HubData.Empty;
            d.PlayerLine = s.Account != null ? s.Account.DisplayName + " · " + s.Account.PlayerId : "";
            d.Tab = _tab;
            if (_tab == HubTab.Team)
            {
                d.Hint = _loc.F("hub.squad", s.Squad.Count);
                d.Team = s.Roster.Select(TeamCard).ToArray();
            }
            else
            {
                d.Hint = _loc.T("shop.title");
                var offers = s.Shop.Offers;
                d.Recruits = offers.Select(RecruitCard).ToArray();
                d.EmptyRecruitText = offers.Count == 0 ? _loc.T("shop.empty") : "";
                d.ShopCountdown = Countdown();
                int cost = ShopRules.RerollCost(s.Shop.Draw, s.Content.Balance);
                d.RerollLabel = cost == 0 ? _loc.T("shop.reroll.free") : _loc.F("shop.reroll.cost", cost);
                d.RerollEnabled = offers.Count > 0 && s.Wallet.Credits >= cost;
            }

            var zone = s.Content.Zones.FirstOrDefault(z => z.Id == _firstZone);
            d.ZoneObjectives = zone == null ? Array.Empty<string>() : zone.Objectives.Select(o => "•  " + _loc.T(o.NameKey)).ToArray();
            d.ZoneDone = s.CompletedZones.Contains(_firstZone.Value);
            d.ZoneStatus = d.ZoneDone ? _loc.T("hub.zone.done") : _loc.T("hub.zone.new");
            d.SquadTitle = _loc.F("hub.squad", s.Squad.Count);
            d.Squad = s.Squad.Select(SquadSlot).ToArray();
            bool anyFatigue = s.Roster.Any(m => m.Fatigue > 0);
            d.HealLabel = _loc.F("hub.heal", MetaRules.InfirmaryCost + " CR");
            d.HealEnabled = anyFatigue && s.Wallet.Credits >= MetaRules.InfirmaryCost;
            d.DeployEnabled = !s.SquadExhausted;
            d.DeployHint = s.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "";
            View.Show(d);
        }

        MemberCardData Card(MemberDef def, int level)
        {
            var b = _session.Content.Balance;
            var name = _loc.MemberName(def);
            return new MemberCardData
            {
                Id = def.Id,
                Name = name.ToUpperInvariant(),
                Family = def.Family,
                RarityText = _loc.RarityName(def.Rarity),
                Rarity = def.Rarity,
                RoleLine = _loc.RoleName(def.Role) + " · " + _loc.FamilyName(def.Family),
                LevelText = _loc.F("ui.level", level),
                Stats = _loc.F("hub.stats", UnitFactory.ScaleStat(def.BaseStats.Hp, level, b), UnitFactory.ScaleStat(def.BaseStats.Atk, level, b),
                               UnitFactory.ScaleStat(def.BaseStats.Def, level, b), def.BaseStats.Spd),
                Skills = string.Join(" · ", def.Skills.Select(_loc.SkillName)),
                Portrait = new PortraitData(name, def.Family),
            };
        }

        TeamCardData TeamCard(ITeamMemberReadModel m)
        {
            bool max = m.Level >= MetaRules.MaxLevel;
            int cost = MetaRules.TrainCost(m.Level);
            return new TeamCardData
            {
                Card = Card(m.Def, m.Level),
                Fatigue01 = m.Fatigue / 100f,
                Fatigue = m.Fatigue,
                FatigueText = _loc.F("ui.fatigue", m.Fatigue),
                InSquad = m.InSquad,
                SquadLabel = m.InSquad ? _loc.T("hub.squad.in") : _loc.T("hub.squad.add"),
                TrainLabel = max ? _loc.T("hub.train.max") : _loc.F("hub.train", cost + " CR"),
                TrainEnabled = !max && _session.Wallet.Credits >= cost,
                Exhausted = m.Exhausted,
            };
        }

        RecruitCardData RecruitCard(ShopOffer offer, int slot)
        {
            var d = _session.Content.Members.First(m => m.Id == offer.Id);
            int cost = MetaRules.RecruitCost(d.Rarity);
            return new RecruitCardData
            {
                Card = Card(d, MetaRules.RecruitLevel),
                Slot = slot,
                Recruited = offer.Recruited,
                StampText = _loc.T("shop.recruited"),
                RecruitLabel = _loc.F("hub.recruit", cost),
                RecruitEnabled = !offer.Recruited && _session.Wallet.Pr >= cost,
            };
        }

        /// <summary>« Nouveaux héros dans 5 h 12 » ; en minutes sous une heure.</summary>
        string Countdown()
        {
            var now = _clock.LocalNow;
            var next = ShopDay.NextReset(now, _session.Content.Balance.ShopResetHour);
            int minutes = Math.Max(1, (int)Math.Ceiling((next - now).TotalMinutes));
            return minutes < 60 ? _loc.F("shop.countdown.m", minutes) : _loc.F("shop.countdown.h", minutes / 60, minutes % 60);
        }

        SquadSlotData SquadSlot(ITeamMemberReadModel m)
        {
            var name = _loc.MemberName(m.Def);
            return new SquadSlotData { Name = name, Portrait = new PortraitData(name, m.Def.Family), Fatigue01 = m.Fatigue / 100f, Fatigue = m.Fatigue, Exhausted = m.Exhausted };
        }

        // ------------------------------------------------------------------ Actions

        /// <summary>Une action du QG à la fois : erreur affichée, puis suite en cas de succès (si l'écran vit encore).</summary>
        void Act(Func<CancellationToken, UniTask<Result>> action, Action? onSuccess = null)
        {
            if (!_acting.TryEnter()) return;
            ActAsync(action, onSuccess).Forget();
        }

        async UniTaskVoid ActAsync(Func<CancellationToken, UniTask<Result>> action, Action? onSuccess)
        {
            Result r;
            try { r = await action(Lifetime); }
            catch (OperationCanceledException) { return; }
            finally { _acting.Exit(); }
            if (Disposed) return;
            if (r.IsFailure) { _toasts.Show(_loc.Error(r.Error!), ToastKind.Error); return; }
            onSuccess?.Invoke();
        }

        ITeamMemberReadModel? Member(MemberId id) => _session.Roster.FirstOrDefault(m => m.Id == id);

        void ToggleSquad(MemberId id) => Act(ct => _toggleSquad.ExecuteAsync(id, ct));

        void Train(MemberId id) => Act(ct => _train.ExecuteAsync(id, ct), () =>
        {
            var m = Member(id);
            if (m != null) _toasts.Show(_loc.F("hub.trained", _loc.MemberName(m.Def), m.Level), ToastKind.Reward);
        });

        void Recruit(int slot)
        {
            var offers = _session.Shop.Offers;
            var def = slot >= 0 && slot < offers.Count ? _session.Content.Members.FirstOrDefault(m => m.Id == offers[slot].Id) : null;
            Act(ct => _recruit.ExecuteAsync(slot, ct), () =>
            {
                if (def != null) _toasts.Show(_loc.F("hub.recruited", _loc.MemberName(def)), ToastKind.Reward);
            });
        }

        /// <summary>Renouveler : direct si gratuit, sinon après confirmation de la dépense.</summary>
        void Reroll()
        {
            int cost = ShopRules.RerollCost(_session.Shop.Draw, _session.Content.Balance);
            if (cost == 0) { Act(ct => _reroll.ExecuteAsync(ct)); return; }
            if (_acting.IsBusy) return;
            RerollPaidAsync(cost, _session.Shop.Day, _session.Shop.Draw).Forget();
        }

        async UniTaskVoid RerollPaidAsync(int cost, string day, int draw)
        {
            bool ok;
            try
            {
                ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("shop.reroll.confirm.title"), _loc.F("shop.reroll.confirm.body", cost),
                                                                _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Primary), Lifetime);
            }
            catch (OperationCanceledException) { return; }
            if (!ok || Disposed) return;
            // La boutique a changé pendant la confirmation (9h00 passée) : ce qui a été accepté n'existe plus.
            if (_session.Shop.Day != day || _session.Shop.Draw != draw)
            {
                _toasts.Show(_loc.T("errors.shop.expired"), ToastKind.Error);
                Refresh();
                return;
            }
            Act(ct => _reroll.ExecuteAsync(ct));
        }

        void Heal() => Act(ct => _heal.ExecuteAsync(ct), () => _toasts.Show(_loc.T("hub.healed"), ToastKind.Reward));

        void Logout() => Act(ct => _signOut.ExecuteAsync(ct), () => _navigate.Publish(new NavigateRequested(GameState.Login)));

        void Reset() => ResetAsync().Forget();

        async UniTaskVoid ResetAsync()
        {
            bool ok;
            try
            {
                ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("login.reset.title"), _loc.T("login.reset.body"),
                                                                _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Danger), Lifetime);
            }
            catch (OperationCanceledException) { return; }
            // Nouvelle partie : boutique neuve tout de suite (sinon « collection complète » jusqu'à la minute suivante).
            if (ok && !Disposed) Act(ct => _reset.ExecuteAsync(ct), OpenShop);
        }

        /// <summary>Déployer : démarre la zone puis demande le plateau. Après succès, la garde reste levée jusqu'au prochain affichage.</summary>
        void Deploy()
        {
            if (!_deploying.TryEnter()) return;
            DeployAsync().Forget();
        }

        async UniTaskVoid DeployAsync()
        {
            Result r;
            try { r = await _startZone.ExecuteAsync(_firstZone, Lifetime); }
            catch (OperationCanceledException) { return; }
            catch { _deploying.Exit(); throw; }
            if (Disposed) return;
            if (r.IsFailure)
            {
                _deploying.Exit();
                _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
                return;
            }
            _navigate.Publish(new NavigateRequested(GameState.Board));
        }
    }
}
