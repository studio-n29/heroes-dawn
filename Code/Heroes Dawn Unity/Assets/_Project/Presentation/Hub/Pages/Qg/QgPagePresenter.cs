#nullable enable
using System;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// QG (M10) : les cinq pièces avec leur état réel (Salon, Infirmerie, Opérations, Labo, Archives), missions du jour,
    /// objectif secondaire avec compte à rebours, alerte de la boutique, fil de Rayner, MISSION (ouvre la carte de la ville).
    /// </summary>
    public sealed class QgPagePresenter : PresenterBase<IQgPageView>, IPageTitle, ITickable
    {
        readonly ISessionReadModel _session;
        readonly HubNavigator _nav;
        readonly MissionLauncher _mission;
        readonly IClock _clock;
        readonly ILocalization _loc;
        readonly MetaOverview _meta;
        readonly ISubscriber<RosterChanged> _roster;
        readonly ISubscriber<MetaChanged> _metaChanged;
        readonly QuestLogFlow? _quests;
        readonly IModalService? _modals;
        IDisposable? _subs;
        int _lastStamp = -1;

        public QgPagePresenter(IQgPageView view, ISessionReadModel session, HubNavigator nav, MissionLauncher mission, IClock clock,
                               ILocalization loc, MetaOverview meta, ISubscriber<RosterChanged> roster, ISubscriber<MetaChanged> metaChanged,
                               QuestLogFlow? quests = null, IModalService? modals = null)
            : base(view, null)
        {
            _session = session; _nav = nav; _mission = mission; _clock = clock; _loc = loc; _meta = meta; _roster = roster; _metaChanged = metaChanged; _quests = quests; _modals = modals;
        }

        public string Title => _loc.T("page.qg");

        protected override void OnStart()
        {
            View.RoomClicked += OpenRoom;
            View.MissionClicked += Launch;
            View.DailyClicked += OpenQuests;
            View.ShopAlertClicked += OnShopAlert;
            View.SecondaryClicked += OnSecondaryAlert;
            View.QuestsClicked += OpenQuests;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _metaChanged.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
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
            View.RoomClicked -= OpenRoom;
            View.MissionClicked -= Launch;
            View.DailyClicked -= OpenQuests;
            View.ShopAlertClicked -= OnShopAlert;
            View.SecondaryClicked -= OnSecondaryAlert;
            View.QuestsClicked -= OpenQuests;
        }

        /// <summary>Chaque seconde : le compte à rebours de l'objectif secondaire avance ; sinon une fois par minute (boutique).</summary>
        public void Tick()
        {
            if (Disposed || !View.IsVisible) return;
            if (Stamp(_clock.LocalNow, _meta.Secondary.Active.HasValue) != _lastStamp) Refresh();
        }

        static int Stamp(DateTime now, bool seconds) => seconds ? now.Second + now.Minute * 60 + now.Hour * 3600 : now.Hour * 60 + now.Minute;

        void OpenRoom(HubPage page) => _nav.Open(page);
        /// <summary>Icône Boutique : pop-up avec le compte à rebours ; « Voir » ouvre la boutique.</summary>
        void OnShopAlert() => AlertAsync(_loc.T("qg.alert.shop.title"), ShopCountdown.Text(_clock.LocalNow, _session.Content.Balance.ShopResetHour, _loc), _loc.T("qg.alert.shop.go"), () => _nav.SelectTab(HubPage.Shop)).Forget();

        /// <summary>Icône Objectif : pop-up avec l'objectif secondaire du moment ; « J'y vais » ouvre la carte.</summary>
        void OnSecondaryAlert()
        {
            var secondary = _meta.Secondary.Active;
            AlertAsync(_loc.T("qg.alert.objective.title"), secondary.HasValue ? SecondaryLine(secondary.Value) : _loc.T("qg.secondary.none"),
                       _loc.T("qg.alert.objective.go"), secondary.HasValue ? Launch : (Action?)null).Forget();
        }

        async UniTaskVoid AlertAsync(string title, string body, string goLabel, Action? go)
        {
            if (_modals == null) { go?.Invoke(); return; }
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(title, body, goLabel, _loc.T("ui.close"), Tone.Primary), Lifetime);
                if (ok) go?.Invoke();
            }
            catch (OperationCanceledException) { }
        }
        void Launch() => _nav.Open(HubPage.CityMap);
        void OpenQuests() { if (_quests != null) _quests.OpenAsync(_session, Lifetime).Forget(); }

        void Refresh()
        {
            if (Disposed) return;
            var s = _session;
            var now = _clock.LocalNow;
            var zone = ZoneProgress.Recommended(s);
            bool done = ZoneProgress.StatusOf(s, zone) == ZoneStatus.Done;
            int toTrain = s.Roster.Count(m => m.Level < MetaRules.MaxLevel);
            int tired = s.Roster.Count(m => m.Fatigue > 0);
            var secondary = _meta.Secondary.Active;
            _lastStamp = Stamp(now, secondary.HasValue);
            var activities = _meta.Quests.Activities();
            int claimable = activities.Count(a => a.Claimable);
            var firstAgent = s.Roster.Count > 0 ? s.Roster[0].Def.Id.Value : s.Route.HeroId;
            var firstCard = s.Content.Collection.Cards.Count > 0 ? s.Content.Collection.Cards[0].Id.Value : "card";
            var firstItem = s.Content.Collection.Items.Count > 0 ? s.Content.Collection.Items[0].Id.Value : "item";
            View.Show(new QgPageData
            {
                Rooms = new[]
                {
                    new RoomCardData { Page = HubPage.Team, Title = _loc.T("page.team_care"), Info = SalonInfo(toTrain, tired), Badge = tired > 0 ? tired : 0, Art = AgentArt.For(firstAgent) },
                    new RoomCardData { Page = HubPage.Lab, Title = _loc.T("page.lab"), Info = LabInfo(), Badge = _meta.LabBadge, Art = AgentArt.Environment(1) },
                    new RoomCardData { Page = HubPage.Operations, Title = _loc.T("page.operations"), Info = OperationsInfo(), Badge = _meta.OperationsBadge, Art = AgentArt.Environment(2) },
                    new RoomCardData { Page = HubPage.Grimoire, Title = _loc.T("page.grimoire"), Info = GrimoireInfo(), Art = AgentArt.Card(firstCard) },
                    new RoomCardData { Page = HubPage.Inventory, Title = _loc.T("page.inventory"), Info = _loc.F("qg.inventory.info", _session.OwnedItemCount), Art = AgentArt.Item(firstItem) },
                    new RoomCardData { Page = HubPage.Agents, Title = _loc.T("page.agents"), Info = _loc.F("qg.agents.info", s.Roster.Count, s.Content.Members.Count), Art = AgentArt.For(s.Route.HeroId) },
                },
                DailyLines = activities.Count == 0 ? new[] { _loc.T("activity.empty") }
                           : activities.Select(a => _loc.F("qg.activity.line", ActivityText(a), a.Claimed ? _loc.T("quests.state.done") : a.Progress + " / " + a.Target)).ToArray(),
                DailyBadge = claimable > 0 ? claimable.ToString() : "",
                SecondaryText = secondary.HasValue ? SecondaryLine(secondary.Value) : _loc.T("qg.secondary.none"),
                SecondaryActive = secondary.HasValue,
                ShopAlertBadge = s.Shop.Day.Length > 0 && s.Shop.SeenDay != s.Shop.Day ? "N" : "",
                ShopAlert = ShopCountdown.Text(now, s.Content.Balance.ShopResetHour, _loc),
                FilSpeaker = _loc.T("qg.fil.speaker"),
                FilText = done ? _loc.T("qg.fil.done") : _loc.Or("qg.fil." + zone.Id.Value, "qg.fil.done"),
                QuestsBadge = _meta.QuestsBadge > 0 ? _meta.QuestsBadge.ToString() : "",
                MissionTitle = _loc.T("qg.mission"),
                MissionSub = _loc.T(zone.NameKey),
                MissionEnabled = !s.SquadExhausted,
                MissionHint = s.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "",
            });
        }

        string ActivityText(DailyActivityView a)
        {
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == a.ZoneId);
            string name = zone != null ? _loc.T(zone.NameKey.Replace(".name", ".short")) : a.ZoneId;
            return a.Kind == ActivityKind.WinCombats ? _loc.F("qg.activity.wins", a.Target, name) : _loc.F("qg.activity.clear", name);
        }

        string SalonInfo(int toTrain, int tired)
            => tired > 0 ? Count("qg.infirmary", tired) : Count("qg.salon", toTrain);

        string SecondaryLine(SecondaryView v)
        {
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == v.ZoneId);
            string name = zone != null ? _loc.T(zone.NameKey.Replace(".name", ".short")) : v.ZoneId;
            return _loc.F("qg.secondary.line", name, MetaFormat.Duration(v.Left));
        }

        string GrimoireInfo()
        {
            var defs = _session.Content.Collection;
            int owned = _session.OwnedCardKinds;
            return _loc.F("qg.grimoire.info", owned, defs.Cards.Count);
        }

        string OperationsInfo()
        {
            int ready = _meta.OperationsBadge;
            if (ready > 0) return _loc.F("qg.ops.ready", ready);
            int running = _meta.Operations.SlotViews().Count(v => v.Running);
            return running > 0 ? _loc.F("qg.ops.running", running) : _loc.T("qg.ops.idle");
        }

        string LabInfo()
        {
            if (_meta.Lab.IsReady) return _loc.T("qg.lab.ready");
            var active = _meta.Lab.Views().FirstOrDefault(v => v.Active);
            return active.Tech != null ? _loc.F("qg.lab.running", MetaFormat.Duration(active.Left)) : _loc.T("qg.lab.idle");
        }

        string ArchivesInfo()
        {
            int unread = _meta.ArchivesBadge;
            return unread > 0 ? _loc.F("qg.archives.new", unread) : _loc.T("qg.archives.none");
        }

        /// <summary>« 1 membre … », « N membres … », ou la phrase « aucun ».</summary>
        string Count(string prefix, int n) => n == 0 ? _loc.T(prefix + ".none") : n == 1 ? _loc.T(prefix + ".one") : _loc.F(prefix + ".many", n);
    }
}
