#nullable enable
using System;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Équipe (M20) : grille filtrée par famille et triée, escouade de 4 places composée ici, combos de l'escouade,
    /// toucher une carte ouvre la fiche, « Préparer une mission » = MISSION.
    /// </summary>
    public sealed class TeamPagePresenter : PresenterBase<ITeamPageView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly IToggleSquadUseCase _toggle;
        readonly MissionLauncher _mission;
        readonly HubNavigator _nav;
        readonly ILocalization _loc;
        readonly ISubscriber<RosterChanged> _roster;
        readonly PageAction _action;
        IDisposable? _subs;
        int _filter;
        MemberSort _sort = MemberSort.Level;

        public TeamPagePresenter(ITeamPageView view, ISessionReadModel session, IToggleSquadUseCase toggle, MissionLauncher mission,
                                 HubNavigator nav, IToastService toasts, ILocalization loc, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _toggle = toggle; _mission = mission; _nav = nav; _loc = loc; _roster = roster;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.F("page.team", _session.Roster.Count);

        protected override void OnStart()
        {
            View.FilterClicked += SetFilter;
            View.SortClicked += SetSort;
            View.MemberTapped += OpenSheet;
            View.SquadToggled += Toggle;
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
            View.FilterClicked -= SetFilter;
            View.SortClicked -= SetSort;
            View.MemberTapped -= OpenSheet;
            View.SquadToggled -= Toggle;
            View.MissionClicked -= Launch;
        }

        void SetFilter(int index)
        {
            _filter = index;
            Refresh();
        }

        void SetSort(MemberSort sort)
        {
            _sort = sort;
            Refresh();
        }

        void OpenSheet(MemberId id) => _nav.Open(HubPage.MemberSheet, id);

        void Toggle(MemberId id) => _action.Run(ct => _toggle.ExecuteAsync(id, ct), Lifetime);

        void Launch() => _mission.Launch(Lifetime);

        void Refresh()
        {
            if (Disposed) return;
            var s = _session;
            Family? family = _filter <= 0 ? (Family?)null : (Family)(_filter - 1);
            var squad = s.Squad;
            var combos = SquadCombos.Of(squad.Select(m => m.Def).ToList(), s.Content.Combos);
            View.Show(new TeamPageData
            {
                Members = MemberQuery.Apply(s.Roster, family, _sort).Select(Tile).ToArray(),
                FilterIndex = _filter,
                Sort = _sort,
                SquadTitle = _loc.F("hub.squad", squad.Count),
                Squad = squad.Select(m => MemberCards.Slot(m, _loc)).ToArray(),
                Combos = combos.Select(c => _loc.F("team.combo_line", _loc.ComboName(c.Combo), _loc.MemberName(c.A), _loc.MemberName(c.B))).ToArray(),
                MissionEnabled = !s.SquadExhausted,
                MissionHint = s.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "",
            });
        }

        MemberTileData Tile(ITeamMemberReadModel m) => new MemberTileData
        {
            Id = m.Id,
            Card = MemberCards.Card(m.Def, m.Level, _session.Content.Balance, _loc),
            InSquad = m.InSquad,
            Fatigue = m.Fatigue,
            Fatigue01 = m.Fatigue / 100f,
            FatigueText = _loc.F("ui.fatigue", m.Fatigue),
            Exhausted = m.Exhausted,
            Status = m.Exhausted ? _loc.T("team.exhausted") : m.InSquad ? _loc.T("hub.squad.in") : "",
            SquadLabel = m.InSquad ? _loc.T("team.squad.remove") : _loc.T("hub.squad.add"),
        };
    }
}
