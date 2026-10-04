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
using N29.HeroesDawn.Presentation.Mvp;
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
        IDisposable? _subs;
        int _lastStamp = -1;

        public QgPagePresenter(IQgPageView view, ISessionReadModel session, HubNavigator nav, MissionLauncher mission, IClock clock,
                               ILocalization loc, MetaOverview meta, ISubscriber<RosterChanged> roster, ISubscriber<MetaChanged> metaChanged)
            : base(view, null)
        {
            _session = session; _nav = nav; _mission = mission; _clock = clock; _loc = loc; _meta = meta; _roster = roster; _metaChanged = metaChanged;
        }

        public string Title => _loc.T("page.qg");

        protected override void OnStart()
        {
            View.RoomClicked += OpenRoom;
            View.MissionClicked += Launch;
            View.DailyClicked += OpenPass;
            View.SecondaryClicked += Launch;
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
            View.DailyClicked -= OpenPass;
            View.SecondaryClicked -= Launch;
        }

        /// <summary>Chaque seconde : le compte à rebours de l'objectif secondaire avance ; sinon une fois par minute (boutique).</summary>
        public void Tick()
        {
            if (Disposed || !View.IsVisible) return;
            if (Stamp(_clock.LocalNow, _meta.Secondary.Active.HasValue) != _lastStamp) Refresh();
        }

        static int Stamp(DateTime now, bool seconds) => seconds ? now.Second + now.Minute * 60 + now.Hour * 3600 : now.Hour * 60 + now.Minute;

        void OpenRoom(HubPage page) => _nav.Open(page);
        void OpenPass() => _nav.Open(HubPage.Pass);
        void Launch() => _nav.Open(HubPage.CityMap);

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
            var daily = _meta.Daily.Today();
            int claimable = daily.Count(d => d.Claimable);
            View.Show(new QgPageData
            {
                Rooms = new[]
                {
                    new RoomCardData { Page = HubPage.Salon, Title = _loc.T("page.salon"), Info = Count("qg.salon", toTrain) },
                    new RoomCardData { Page = HubPage.Infirmary, Title = _loc.T("page.infirmary"), Info = Count("qg.infirmary", tired) },
                    new RoomCardData { Page = HubPage.Operations, Title = _loc.T("page.operations"), Info = OperationsInfo(), Badge = _meta.OperationsBadge },
                    new RoomCardData { Page = HubPage.Lab, Title = _loc.T("page.lab"), Info = LabInfo(), Badge = _meta.LabBadge },
                    new RoomCardData { Page = HubPage.Archives, Title = _loc.T("page.archives"), Info = ArchivesInfo(), Badge = _meta.ArchivesBadge },
                },
                DailyLines = daily.Select(d => _loc.F("qg.daily.line", _loc.F("daily." + d.Template.Id, d.Template.Target), d.Progress, d.Template.Target)).ToArray(),
                DailyBadge = claimable > 0 ? claimable.ToString() : "",
                SecondaryText = secondary.HasValue ? SecondaryLine(secondary.Value) : _loc.T("qg.secondary.none"),
                SecondaryActive = secondary.HasValue,
                ShopAlert = ShopCountdown.Text(now, s.Content.Balance.ShopResetHour, _loc),
                FilSpeaker = _loc.T("qg.fil.speaker"),
                FilText = done ? _loc.T("qg.fil.done") : _loc.Or("qg.fil." + zone.Id.Value, "qg.fil.done"),
                MissionTitle = _loc.T("qg.mission"),
                MissionSub = _loc.T(zone.NameKey),
                MissionEnabled = !s.SquadExhausted,
                MissionHint = s.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "",
            });
        }

        string SecondaryLine(SecondaryView v)
        {
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == v.ZoneId);
            string name = zone != null ? _loc.T(zone.NameKey.Replace(".name", ".short")) : v.ZoneId;
            return _loc.F("qg.secondary.line", name, MetaFormat.Duration(v.Left));
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
