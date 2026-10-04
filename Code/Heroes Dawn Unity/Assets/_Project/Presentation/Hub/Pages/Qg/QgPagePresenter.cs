#nullable enable
using System;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// QG (M10) : pièces (Salon et Infirmerie avec infos réelles, le reste BIENTÔT), alertes (compte à rebours de la boutique,
    /// mis à jour chaque minute), fil de Rayner, MISSION.
    /// </summary>
    public sealed class QgPagePresenter : PresenterBase<IQgPageView>, IPageTitle, ITickable
    {
        readonly ISessionReadModel _session;
        readonly HubNavigator _nav;
        readonly MissionLauncher _mission;
        readonly IClock _clock;
        readonly ILocalization _loc;
        readonly ISubscriber<RosterChanged> _roster;
        IDisposable? _subs;
        int _lastMinute = -1;

        public QgPagePresenter(IQgPageView view, ISessionReadModel session, HubNavigator nav, MissionLauncher mission, IClock clock,
                               ILocalization loc, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _nav = nav; _mission = mission; _clock = clock; _loc = loc; _roster = roster;
        }

        public string Title => _loc.T("page.qg");

        protected override void OnStart()
        {
            View.RoomClicked += OpenRoom;
            View.MissionClicked += Launch;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            _subs = _roster.Subscribe(_ => Refresh());
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
        }

        /// <summary>Une fois par minute : le compte à rebours de la boutique avance.</summary>
        public void Tick()
        {
            if (Disposed || !View.IsVisible) return;
            var now = _clock.LocalNow;
            if (now.Hour * 60 + now.Minute != _lastMinute) Refresh();
        }

        void OpenRoom(HubPage page) => _nav.Open(page);

        void Launch() => _mission.Launch(Lifetime);

        void Refresh()
        {
            if (Disposed) return;
            var s = _session;
            var now = _clock.LocalNow;
            _lastMinute = now.Hour * 60 + now.Minute;
            var zone = s.Content.Zones.FirstOrDefault(z => z.Id == _mission.Zone);
            bool done = s.CompletedZones.Contains(_mission.Zone.Value);
            int toTrain = s.Roster.Count(m => m.Level < MetaRules.MaxLevel);
            int tired = s.Roster.Count(m => m.Fatigue > 0);
            View.Show(new QgPageData
            {
                Rooms = new[]
                {
                    new RoomCardData { Page = HubPage.Salon, Title = _loc.T("page.salon"), Info = Count("qg.salon", toTrain) },
                    new RoomCardData { Page = HubPage.Infirmary, Title = _loc.T("page.infirmary"), Info = Count("qg.infirmary", tired) },
                    Wip(HubPage.Operations, "page.operations", WipCatalog.Operations),
                    Wip(HubPage.Lab, "page.lab", WipCatalog.Lab),
                    Wip(HubPage.Archives, "page.archives", WipCatalog.Archives),
                },
                DailyMissionsWipId = WipCatalog.DailyMissions.Id,
                SecondaryObjectivesWipId = WipCatalog.SecondaryObjectives.Id,
                ShopAlert = ShopCountdown.Text(now, s.Content.Balance.ShopResetHour, _loc),
                FilSpeaker = _loc.T("qg.fil.speaker"),
                FilText = done ? _loc.T("qg.fil.done") : _loc.Or("qg.fil." + _mission.Zone.Value, "qg.fil.done"),
                MissionTitle = _loc.T("qg.mission"),
                MissionSub = zone != null ? _loc.T(zone.NameKey) : "",
                MissionEnabled = !s.SquadExhausted,
                MissionHint = s.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "",
            });
        }

        RoomCardData Wip(HubPage page, string titleKey, WipFeature feature)
            => new RoomCardData { Page = page, Title = _loc.T(titleKey), Info = _loc.T("wip.soon"), WipId = feature.Id };

        /// <summary>« 1 membre … », « N membres … », ou la phrase « aucun ».</summary>
        string Count(string prefix, int n) => n == 0 ? _loc.T(prefix + ".none") : n == 1 ? _loc.T(prefix + ".one") : _loc.F(prefix + ".many", n);
    }
}
