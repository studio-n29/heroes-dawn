#nullable enable
using System;
using System.Collections.Generic;
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
    /// Équipe (M20) : grille filtrée par famille et triée, escouade de 4 places composée ici, synergies de l'escouade,
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
        readonly TeamViewState _state;
        readonly SquadPresets? _presets;
        readonly IHealRosterUseCase? _healAll;
        bool _saveMode;
        readonly PageAction _action;
        readonly IToastService _toasts;
        IDisposable? _subs;

        public TeamPagePresenter(ITeamPageView view, ISessionReadModel session, IToggleSquadUseCase toggle, MissionLauncher mission,
                                 HubNavigator nav, TeamViewState state, IToastService toasts, ILocalization loc, ISubscriber<RosterChanged> roster, SquadPresets? presets = null,
                                 IHealRosterUseCase? healAll = null)
            : base(view, null)
        {
            _session = session; _toggle = toggle; _mission = mission; _nav = nav; _loc = loc; _roster = roster;
            _state = state; _presets = presets; _toasts = toasts; _healAll = healAll;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _session.Roster.Count == 1 ? _loc.T("page.team_one") : _loc.F("page.team", _session.Roster.Count);

        protected override void OnStart()
        {
            View.FilterClicked += SetFilter;
            View.SortClicked += SetSort;
            View.MemberTapped += OpenSheet;
            View.SquadToggled += Toggle;
            View.MissionClicked += Launch;
            View.HealAllClicked += HealAll;
            View.PresetClicked += OnPreset;
            View.PresetSaveClicked += OnPresetSave;
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
            View.HealAllClicked -= HealAll;
            View.PresetClicked -= OnPreset;
            View.PresetSaveClicked -= OnPresetSave;
        }

        void SetFilter(int index)
        {
            _state.FilterIndex = index;
            Refresh();
        }

        void SetSort(MemberSort sort)
        {
            _state.Sort = sort;
            Refresh();
        }

        void OpenSheet(MemberId id) => _nav.Open(HubPage.MemberSheet, id);

        void Toggle(MemberId id) => _action.Run(ct => _toggle.ExecuteAsync(id, ct), Lifetime);

        void Launch() => _nav.Open(HubPage.CityMap);

        /// <summary>Pourquoi « Soigner toute l'équipe » est grisé (aucune fatigue, Crédits manquants) ; vide s'il est possible.</summary>
        string HealAllReason(ISessionReadModel s)
        {
            if (!s.Roster.Any(m => m.Fatigue > 0)) return _loc.T("qg.infirmary.none");
            int missing = MetaRules.InfirmaryCost - s.Wallet.Credits;
            return missing > 0 ? _loc.F("hub.train.missing", missing) : "";
        }

        void HealAll()
        {
            if (_healAll == null) return;
            _action.Run(ct => _healAll.ExecuteAsync(ct), Lifetime, () => _toasts.Show(_loc.T("hub.healed"), ToastKind.Reward));
        }

        void Report(Result r, string ok)
        {
            if (r.IsFailure) _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
            else _toasts.Show(ok, ToastKind.Info);
        }

        void OnPresetSave()
        {
            _saveMode = !_saveMode;
            Refresh();
        }

        /// <summary>Hors mode sauvegarde : charge la composition ; en mode sauvegarde : y enregistre l'escouade actuelle.</summary>
        void OnPreset(int slot)
        {
            if (_presets == null) return;
            if (_saveMode)
            {
                _saveMode = false;
                Report(_presets.Save(slot), _loc.F("team.preset.saved", slot + 1));
                Refresh();
                return;
            }
            Report(_presets.Load(slot), _loc.F("team.preset.loaded", slot + 1));
        }

        /// <summary>Bonus de paire actifs dans l'escouade (liens de niveau 2 et plus) : une ligne par paire.</summary>
        IEnumerable<string> PairLines(IReadOnlyList<ITeamMemberReadModel> squad)
        {
            for (int i = 0; i < squad.Count; i++)
                for (int j = i + 1; j < squad.Count; j++)
                {
                    var a = squad[i]; var b = squad[j];
                    int level = _session.LinkLevel(a.Id, b.Id);
                    if (level < PairBonusRules.MinLevel) continue;
                    yield return _loc.F("team.pair_line", _loc.PairTheme(a.Def.Role, b.Def.Role), _loc.MemberName(a.Def), _loc.MemberName(b.Def), level,
                                        string.Join(", ", PairBonusRules.Merge(PairBonusRules.Of(a.Def.Role, b.Def.Role, level)).Select(_loc.PairBonusText)));
                }
        }

        void Refresh()
        {
            if (Disposed) return;
            var s = _session;
            Family? family = _state.FilterIndex <= 0 ? (Family?)null : (Family)(_state.FilterIndex - 1);
            var squad = s.Squad;
            var synergies = SquadSynergies.Of(squad.Select(m => m.Def).ToList(), s.Content.Synergies);
            View.Show(new TeamPageData
            {
                Members = MemberQuery.Apply(s.Roster, family, _state.Sort).Select(Tile).ToArray(),
                FilterIndex = _state.FilterIndex,
                Sort = _state.Sort,
                SquadTitle = _loc.F("hub.squad", squad.Count),
                Squad = squad.Select(m => MemberCards.Slot(m, _loc)).ToArray(),
                Synergies = synergies.Select(c => _loc.F("team.synergy_line", _loc.SynergyName(c.Synergy), _loc.SynergyEffect(c.Synergy, s.LinkLevel(c.A.Id, c.B.Id)), _loc.MemberName(c.A), _loc.MemberName(c.B)))
                    .Concat(TeamEffects.Active(squad.Select(m => m.Def.Family).ToList(), squad.Select(m => m.Def.Role).ToList()).Select(_loc.TeamEffect))
                    .Concat(PairLines(squad.ToList())).ToArray(),
                HealAllLabel = _loc.F("hub.heal", MetaRules.InfirmaryCost + " CR"),
                HealAllEnabled = HealAllReason(s).Length == 0 && _healAll != null,
                HealAllReason = HealAllReason(s),
                MissionEnabled = !s.SquadExhausted,
                MissionHint = s.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "",
                PresetLabels = _presets == null ? new string[0] : Enumerable.Range(0, SquadPresets.Slots)
                    .Select(i => _presets.IsSaved(i) ? _loc.F("team.preset.filled", i + 1, _presets.Members(i).Count) : _loc.F("team.preset.empty", i + 1)).ToArray(),
                PresetSaved = _presets == null ? new bool[0] : Enumerable.Range(0, SquadPresets.Slots).Select(_presets.IsSaved).ToArray(),
                SaveMode = _saveMode,
                SaveLabel = _loc.T(_saveMode ? "team.preset.saving" : "team.preset.save"),
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
