#nullable enable
using System;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Carte de New York (M60/M61) ouverte par MISSION : les quartiers de l'acte en cours, ceux des actes suivants grisés,
    /// les points précis des missions, la liste des zones et la fiche de la zone choisie ; DÉPLOYER lance la zone.
    /// </summary>
    public sealed class CityMapPagePresenter : PresenterBase<ICityMapPageView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly MissionLauncher _mission;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<RosterChanged> _roster;
        IDisposable? _subs;
        string _selected;

        public CityMapPagePresenter(ICityMapPageView view, ISessionReadModel session, MissionLauncher mission, IToastService toasts,
                                    ILocalization loc, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _mission = mission; _toasts = toasts; _loc = loc; _roster = roster;
            _selected = ZoneProgress.Recommended(session).Id.Value;
        }

        public string Title => _loc.T("page.citymap");

        protected override void OnStart()
        {
            View.ZoneSelected += Select;
            View.DeployClicked += Deploy;
            View.LockedTapped += OnLocked;
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
            View.ZoneSelected -= Select;
            View.DeployClicked -= Deploy;
            View.LockedTapped -= OnLocked;
        }

        void Select(string zoneId)
        {
            if (_session.Content.Zones.All(z => z.Id.Value != zoneId)) return;
            _selected = zoneId;
            Refresh();
        }

        void Deploy()
        {
            var zone = Selected();
            if (zone == null) return;
            if (ZoneProgress.StatusOf(_session, zone) == ZoneStatus.Locked)
            {
                _toasts.Show(_loc.F("map.locked_zone", ReqNames(zone)), ToastKind.Warning);
                return;
            }
            _mission.Launch(zone.Id, Lifetime);
        }

        void OnLocked(string label) => _toasts.Show(_loc.F("map.locked_act", label), ToastKind.Info);

        string Short(ZoneDef z) => _loc.T(z.NameKey.Replace(".name", ".short"));

        ZoneDef? Selected() => _session.Content.Zones.FirstOrDefault(z => z.Id.Value == _selected);

        string ReqNames(ZoneDef zone)
            => string.Join(", ", zone.Requires.Where(r => !_session.CompletedZones.Contains(r.Value))
                                              .Select(r => Short(_session.Content.Zone(r))));

        void Refresh()
        {
            if (Disposed) return;
            var s = _session;
            var zones = ZoneProgress.InBorough(s, "brooklyn");
            var sel = Selected() ?? zones[0];
            View.Show(new CityMapPageData
            {
                ActTitle = _loc.T("map.act1"),
                Boroughs = CityMapLayout.Boroughs.Select(b => new BoroughData
                {
                    Id = b.Id,
                    Name = _loc.T("map.borough." + b.Id),
                    Polygon = b.Polygon,
                    Locked = b.Act > 1,
                    Highlight = b.Id == sel.Borough,
                    Tag = b.Act > 1 ? _loc.F("map.act_soon", b.Act) : _loc.F("map.act", b.Act),
                    LabelAt = b.LabelAt,
                }).ToArray(),
                Points = CityMapLayout.Spots.Select(p => PointOf(p, sel)).ToArray(),
                Rows = zones.Select(z => RowOf(z, sel)).ToArray(),
                Detail = DetailOf(sel),
            });
        }

        ZonePointData PointOf(MapSpot spot, ZoneDef sel)
        {
            if (spot.ZoneId == null)
                return new ZonePointData { ZoneId = null, Label = spot.Label, Sub = "", Position = spot.Position, Status = ZoneStatus.Locked };
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == spot.ZoneId);
            return new ZonePointData
            {
                ZoneId = spot.ZoneId,
                Label = spot.Label,
                Sub = zone != null ? Short(zone) : "",
                Position = spot.Position,
                Status = zone != null ? ZoneProgress.StatusOf(_session, zone) : ZoneStatus.Locked,
                Selected = spot.ZoneId == sel.Id.Value,
            };
        }

        ZoneRowData RowOf(ZoneDef z, ZoneDef sel)
        {
            var st = ZoneProgress.StatusOf(_session, z);
            return new ZoneRowData
            {
                ZoneId = z.Id.Value,
                Title = z.Code + " · " + Short(z),
                StatusText = _loc.T("map.status." + st),
                Status = st,
                Selected = z.Id == sel.Id,
            };
        }

        ZoneDetailData DetailOf(ZoneDef z)
        {
            var st = ZoneProgress.StatusOf(_session, z);
            var b = _session.Content.Balance;
            int combats = z.Nodes.Count(n => n.Type == CaseType.Combat);
            string button = st == ZoneStatus.Done ? _loc.T("map.replay") : _loc.T("map.deploy");
            return new ZoneDetailData
            {
                Title = z.Code + " · " + _loc.T(z.NameKey),
                Description = _loc.Or(z.NameKey.Replace(".name", ".desc"), "zones.ny_1_1.desc"),
                Chips = _loc.F("map.chips", z.RecommendedLevel, z.Nodes.Count, combats),
                Objectives = z.Objectives.Select(o => _loc.T(o.NameKey)).ToArray(),
                ObjectivesDone = z.Objectives.Select(_ => st == ZoneStatus.Done).ToArray(),
                Rewards = st == ZoneStatus.Done
                    ? _loc.F("board.zone_rewards.replay", MetaRules.ZoneReplayCredits)
                    : _loc.F("map.rewards", b.ZoneValidationCredits, b.ZoneValidationPr),
                ButtonLabel = st == ZoneStatus.Locked ? _loc.T("map.locked") : button,
                ButtonEnabled = st != ZoneStatus.Locked && !_session.SquadExhausted,
                Reason = st == ZoneStatus.Locked ? _loc.F("map.locked_zone", ReqNames(z))
                       : _session.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "",
            };
        }
    }
}
