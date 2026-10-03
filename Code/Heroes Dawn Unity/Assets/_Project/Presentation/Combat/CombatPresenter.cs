#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Combat : fait avancer la simulation au rythme du temps réel (×1, ×2, ×3, pause derrière une modale),
    /// traduit ses événements en retours visuels, gère sélection, ciblage, combo, auto et retraite,
    /// puis affiche les résultats et publie <see cref="CombatResultsClosed"/> à « Continuer ».
    /// </summary>
    public sealed class CombatPresenter : PresenterBase<ICombatView>, ITickable, IBackHandler
    {
        readonly IActiveCombatReadModel _combat;
        readonly ISessionReadModel _session;
        readonly ISetAutoCombatUseCase _setAuto;
        readonly IFinishCombatUseCase _finish;
        readonly IVerifyCombatUseCase _verify;
        readonly IForceCombatWinUseCase _forceWin;
        readonly IPublisher<CombatResultsClosed> _closed;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly IFrameClock _clock;
        readonly IAudioService _audio;

        CombatSimulation? _sim;
        readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        readonly List<int> _readyOrder = new List<int>();
        readonly List<string> _log = new List<string>();
        readonly BusyGate _finishing = new BusyGate();
        int _selected = -1;
        int _pendingSlot = -1;
        CombatSimulation.ComboOption? _pendingCombo;
        int _speedFactor = 1;
        float _accumulator;
        bool _ended;
        bool _firstRefresh;
        bool _autoShown;

        public CombatPresenter(ICombatView view, IActiveCombatReadModel combat, ISessionReadModel session, ISetAutoCombatUseCase setAuto,
                               IFinishCombatUseCase finish, IVerifyCombatUseCase verify, IForceCombatWinUseCase forceWin,
                               IPublisher<CombatResultsClosed> closed, IPublisher<NavigateRequested> navigate, IModalService modals,
                               IToastService toasts, ILocalization loc, IFrameClock clock, BackButtonRouter router,
                               IAudioService? audio = null)
            : base(view, router)
        {
            _combat = combat; _session = session; _setAuto = setAuto; _finish = finish; _verify = verify; _forceWin = forceWin;
            _closed = closed; _navigate = navigate; _modals = modals; _toasts = toasts; _loc = loc; _clock = clock;
            _audio = audio ?? NullAudioService.Instance;
        }

        protected override IBackHandler BackHandler => this;

        protected override void OnStart()
        {
            View.RetreatClicked += Retreat;
            View.AutoClicked += ToggleAuto;
            View.SpeedClicked += ToggleSpeed;
            View.DebugWinClicked += DebugWin;
            View.ComboClicked += Combo;
            View.CycleClicked += CycleSelection;
            View.SkillClicked += Skill;
            View.UnitTapped += UnitTapped;
        }

        protected override void OnDispose()
        {
            View.RetreatClicked -= Retreat;
            View.AutoClicked -= ToggleAuto;
            View.SpeedClicked -= ToggleSpeed;
            View.DebugWinClicked -= DebugWin;
            View.ComboClicked -= Combo;
            View.CycleClicked -= CycleSelection;
            View.SkillClicked -= Skill;
            View.UnitTapped -= UnitTapped;
        }

        protected override void OnShown()
        {
            var sim = _combat.Simulation;
            if (sim == null)
            {
                _navigate.Publish(new NavigateRequested(GameState.Board));
                return;
            }
            if (sim == _sim) return;
            _sim = sim;
            _ended = false;
            _accumulator = 0f;
            _selected = -1;
            _pendingSlot = -1;
            _pendingCombo = null;
            _readyOrder.Clear();
            _log.Clear();
            View.ShowLog(_log);
            BuildUnits(sim);
            _firstRefresh = true;
            _autoShown = sim.Auto;
            RefreshTopBar();
            RefreshAll();
            View.ShowBanner(_loc.T("combat.rank").Replace("{0}", "").Trim().ToUpperInvariant() + " !", Tone.Primary);
        }

        void BuildUnits(CombatSimulation sim)
        {
            _names.Clear();
            var fighters = _combat.Fighters;
            var content = _session.Content;
            var cards = new List<UnitCardData>();
            foreach (var u in sim.Units)
            {
                string name;
                Family fam;
                if (u.Side == Side.Ally)
                {
                    var def = u.Id < fighters.Count ? fighters[u.Id].Def : content.Member(new MemberId(u.DefId));
                    name = _loc.MemberName(def);
                    fam = def.Family;
                }
                else
                {
                    var def = content.Enemy(new EnemyId(u.DefId));
                    name = _loc.EnemyName(def);
                    fam = def.Family;
                }
                _names[u.Id] = name;
                cards.Add(new UnitCardData { Id = u.Id, Ally = u.Side == Side.Ally, Name = name.ToUpperInvariant(), Boss = u.IsBoss, Family = fam, Portrait = new PortraitData(name, fam, u.IsBoss) });
            }
            View.BuildUnits(cards);
        }

        // ------------------------------------------------------------------ Boucle

        public void Tick()
        {
            var sim = _sim;
            if (sim == null || _ended || Disposed || !View.IsVisible || _modals.AnyOpen) return;
            int tickMs = _session.Content.Balance.TickMs;
            _accumulator += _clock.UnscaledDeltaTime * 1000f * _speedFactor;
            int steps = 0;
            while (_accumulator >= tickMs && steps < 12 && !sim.IsOver)
            {
                _accumulator -= tickMs;
                steps++;
                var events = sim.Step(1);
                if (events.Count > 0) Play(events);
            }
            if (_accumulator > tickMs * 12) _accumulator = 0f;
            RefreshAll();
            if (sim.IsOver && !_ended)
            {
                _ended = true;
                FinishAsync().Forget();
            }
        }

        void Play(IReadOnlyList<CombatEvent> events)
        {
            foreach (var e in events)
            {
                switch (e)
                {
                    case GaugeFullEvent g:
                        if (_sim!.Unit(g.Unit).Side == Side.Ally && !_readyOrder.Contains(g.Unit))
                        {
                            _readyOrder.Add(g.Unit);
                            if (!_sim!.Auto) _audio.Play(SfxId.AllyReady);
                        }
                        break;
                    case SkillUsedEvent su:
                        View.Act(su.Actor);
                        _audio.Play(SfxId.SkillCast);
                        AddLog(Name(su.Actor) + " : " + SkillLabel(su.Actor, su.Skill));
                        break;
                    case DamageEvent d:
                        View.Float(d.Target, (d.Crit ? "!" : "") + "-" + d.Amount, d.Crit ? Tone.Primary : Tone.Default, d.Crit ? 56 : 44);
                        View.Hit(d.Target);
                        _audio.Play(d.Crit ? SfxId.CritHit : SfxId.Hit);
                        break;
                    case HealEvent h:
                        if (h.Amount > 0)
                        {
                            View.Float(h.Target, "+" + h.Amount, Tone.Success, 44);
                            _audio.Play(SfxId.HealFx);
                        }
                        break;
                    case ShieldAbsorbedEvent sa:
                        View.Float(sa.Target, "(" + sa.Amount + ")", Tone.Shield, 34);
                        _audio.Play(SfxId.Shield);
                        break;
                    case MissEvent m:
                        View.Float(m.Target, _loc.T("combat.miss"), Tone.Muted, 38);
                        _audio.Play(SfxId.Miss);
                        break;
                    case StatusAppliedEvent st:
                        if (st.Status != StatusType.Shield) View.Float(st.Target, _loc.StatusName(st.Status), Tone.Perception, 30);
                        _audio.Play(st.Status == StatusType.Shield ? SfxId.Shield : SfxId.Status);
                        break;
                    case TurnSkippedEvent ts:
                        View.Float(ts.Unit, _loc.T("combat.skip"), Tone.Info, 36);
                        break;
                    case UnitDefeatedEvent ud:
                        AddLog(Name(ud.Unit) + " : K.O.");
                        _audio.Play(SfxId.KnockOut);
                        _readyOrder.Remove(ud.Unit);
                        break;
                    case ComboUsedEvent cu:
                        var combo = FindCombo(cu.ComboId);
                        var comboName = combo != null ? _loc.ComboName(combo) : cu.ComboId;
                        View.ShowBanner(_loc.T("combat.combo") + " " + comboName, Tone.Danger);
                        _audio.Play(SfxId.Combo);
                        AddLog(Name(cu.A) + " + " + Name(cu.B) + " : " + comboName);
                        break;
                    case CombatEndedEvent ce:
                        View.ShowBanner(ce.Victory ? _loc.T("combat.victory") : _loc.T("combat.defeat"), ce.Victory ? Tone.Primary : Tone.Danger);
                        _audio.Play(ce.Victory ? SfxId.Victory : SfxId.Defeat); // atténue la musique (catalogue)
                        break;
                }
            }
        }

        // ------------------------------------------------------------------ Affichage

        void RefreshAll()
        {
            var sim = _sim;
            if (sim == null || Disposed) return;
            _readyOrder.RemoveAll(id => !sim.Unit(id).IsReady);
            bool manual = !sim.Auto && !sim.IsOver;
            if (manual && (_selected < 0 || !sim.Unit(_selected).IsReady))
            {
                _selected = _readyOrder.Count > 0 ? _readyOrder[0] : -1;
                _pendingSlot = -1;
            }
            if (!manual) { _selected = -1; _pendingSlot = -1; _pendingCombo = null; }
            if (_pendingCombo.HasValue && !ComboStillAvailable(_pendingCombo.Value)) _pendingCombo = null;

            bool targetEnemies = false, targetAllies = false;
            if (_pendingCombo.HasValue) targetEnemies = true;
            else if (_selected >= 0 && _pendingSlot >= 0)
            {
                var rule = sim.Unit(_selected).Skills[_pendingSlot].Target;
                targetEnemies = rule == TargetRule.SingleEnemy;
                targetAllies = rule == TargetRule.SingleAlly;
            }

            var states = new List<UnitStateData>(sim.Units.Count);
            foreach (var u in sim.Units)
            {
                bool ally = u.Side == Side.Ally;
                states.Add(new UnitStateData
                {
                    Id = u.Id,
                    Ally = ally,
                    Hp01 = u.MaxHp == 0 ? 0 : u.Hp / (float)u.MaxHp,
                    HpText = u.Hp + " / " + u.MaxHp,
                    Gauge01 = u.GaugeMax == 0 ? 0 : (float)((double)u.Gauge / u.GaugeMax),
                    GaugeFull = u.IsReady,
                    Statuses = Statuses(u),
                    Alive = u.IsAlive,
                    Highlight = manual && ally && u.IsReady,
                    Selected = u.Id == _selected && u.IsAlive,
                    Targetable = u.IsAlive && ((!ally && targetEnemies) || (ally && targetAllies)),
                    Instant = _firstRefresh,
                });
            }
            _firstRefresh = false;
            View.ShowUnits(states);

            var panel = new SkillPanelData { Visible = manual && _selected >= 0, Slots = new SkillSlotData[3] };
            if (panel.Visible)
            {
                var u = sim.Unit(_selected);
                int count = CombatSimulation.ActiveSkillCount(u);
                for (int i = 0; i < 3; i++)
                {
                    if (i >= count) continue;
                    var s = u.Skills[i];
                    string label = "[" + (i + 1) + "] " + _loc.SkillName(s);
                    if (u.Cooldowns[i] > 0) label += "  (" + _loc.F("combat.cooldown", u.Cooldowns[i]) + ")";
                    panel.Slots[i] = new SkillSlotData { Visible = true, Label = label, Description = _loc.DescribeSkill(s), Pending = _pendingSlot == i, Enabled = sim.CanUse(u.Id, i) };
                }
            }
            View.ShowSkills(panel);

            var combos = manual ? sim.AvailableCombos() : Array.Empty<CombatSimulation.ComboOption>();
            View.ShowCombo(combos.Count > 0, combos.Count > 0 ? _loc.T("combat.combo") + "\n<size=24>" + _loc.ComboName(combos[0].Combo) + "</size>" : "");

            View.SetHint(targetEnemies ? _loc.T("combat.choose_target")
                       : targetAllies ? _loc.T("combat.choose_ally")
                       : manual && _selected < 0 ? _loc.T("combat.waiting") : "");
            RefreshTopBar();
        }

        string Statuses(CombatUnit u)
        {
            var sb = new StringBuilder();
            foreach (var s in u.Statuses)
            {
                if (sb.Length > 0) sb.Append("  ");
                sb.Append(_loc.StatusName(s.Type));
                if (s.Type == StatusType.Shield) sb.Append(' ').Append(s.Value);
                else if (s.Type != StatusType.Mark) sb.Append(' ').Append(s.TurnsLeft);
            }
            return sb.ToString();
        }

        void RefreshTopBar()
        {
            var pending = _combat.Case;
            View.ShowTopBar(new CombatTopBarData
            {
                Title = pending != null ? _loc.F("combat.rank", _loc.RankName(pending.Rank)).ToUpperInvariant() : "",
                AutoOn = _autoShown,
                SpeedText = _loc.F("combat.speed", _speedFactor),
                RetreatEnabled = _sim != null && !_sim.IsOver,
                DebugVisible = UnityEngine.Debug.isDebugBuild,
            });
        }

        // ------------------------------------------------------------------ Saisie

        bool CanCommand => _sim != null && !_sim.IsOver && !_sim.Auto && !_modals.AnyOpen && !Disposed;

        void Skill(int slot)
        {
            var sim = _sim;
            if (!CanCommand || _selected < 0 || !sim!.CanUse(_selected, slot)) return;
            _pendingCombo = null;
            var rule = sim.Unit(_selected).Skills[slot].Target;
            if (rule == TargetRule.SingleEnemy && AliveCount(Side.Enemy) == 1) Submit(CombatCommand.UseSkill(_selected, slot, FirstAlive(Side.Enemy)));
            else if (rule == TargetRule.SingleEnemy || rule == TargetRule.SingleAlly) _pendingSlot = _pendingSlot == slot ? -1 : slot; // re-toucher annule
            else Submit(CombatCommand.UseSkill(_selected, slot, -1));
            RefreshAll();
        }

        void Combo()
        {
            var sim = _sim;
            if (!CanCommand) return;
            var list = sim!.AvailableCombos();
            if (list.Count == 0) return;
            var c = list[0];
            if (c.Combo.Effect.Target == TargetRule.SingleEnemy && AliveCount(Side.Enemy) > 1)
            {
                _pendingCombo = c;
                _pendingSlot = -1;
            }
            else Submit(CombatCommand.UseCombo(c.A, c.B, FirstAlive(Side.Enemy)));
            RefreshAll();
        }

        void UnitTapped(int unitId)
        {
            var sim = _sim;
            if (!CanCommand) return;
            var u = sim!.Unit(unitId);
            if (_pendingCombo.HasValue && u.Side == Side.Enemy && u.IsAlive)
            {
                var c = _pendingCombo.Value;
                Submit(CombatCommand.UseCombo(c.A, c.B, unitId));
            }
            else if (_selected >= 0 && _pendingSlot >= 0 && IsValidTarget(sim.Unit(_selected).Skills[_pendingSlot].Target, u))
                Submit(CombatCommand.UseSkill(_selected, _pendingSlot, unitId));
            else if (u.Side == Side.Ally && u.IsReady)
            {
                _selected = unitId;
                _pendingSlot = -1;
                _pendingCombo = null;
            }
            RefreshAll();
        }

        static bool IsValidTarget(TargetRule rule, CombatUnit u)
            => u.IsAlive && ((rule == TargetRule.SingleEnemy && u.Side == Side.Enemy) || (rule == TargetRule.SingleAlly && u.Side == Side.Ally));

        void Submit(CombatCommand cmd)
        {
            _sim?.Submit(cmd);
            _pendingSlot = -1;
            _pendingCombo = null;
            if (cmd.Type == CommandType.Skill || cmd.Type == CommandType.Combo)
            {
                _readyOrder.Remove(cmd.Actor);
                if (cmd.Type == CommandType.Combo) _readyOrder.Remove(cmd.Partner);
                _selected = -1;
            }
        }

        public bool HandleBack()
        {
            if (_pendingSlot < 0 && !_pendingCombo.HasValue) return false;
            _pendingSlot = -1;
            _pendingCombo = null;
            RefreshAll();
            return true;
        }

        void CycleSelection()
        {
            if (!CanCommand || _readyOrder.Count < 2) return;
            int i = _readyOrder.IndexOf(_selected);
            _selected = _readyOrder[(i + 1) % _readyOrder.Count];
            _pendingSlot = -1;
            RefreshAll();
        }

        void ToggleAuto()
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || _modals.AnyOpen || Disposed) return;
            bool on = !sim.Auto;
            _setAuto.ExecuteAsync(on, Lifetime).Forget();
            // La commande est appliquée au prochain tick : on anticipe l'affichage.
            _autoShown = on;
            _pendingSlot = -1;
            _pendingCombo = null;
            RefreshTopBar();
        }

        void ToggleSpeed()
        {
            _speedFactor = _speedFactor == 1 ? 2 : _speedFactor == 2 ? 3 : 1;
            RefreshTopBar();
        }

        void Retreat()
        {
            if (_sim == null || _sim.IsOver || Disposed) return;
            RetreatAsync().Forget();
        }

        async UniTaskVoid RetreatAsync()
        {
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("combat.retreat.title"), _loc.T("combat.retreat.body"),
                                                                     _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Danger), Lifetime);
                if (ok) _sim?.Submit(CombatCommand.Retreat());
            }
            catch (OperationCanceledException) { }
        }

        void DebugWin() => _forceWin.ExecuteAsync(Lifetime).Forget();

        // ------------------------------------------------------------------ Fin

        async UniTaskVoid FinishAsync()
        {
            if (!_finishing.TryEnter()) return;
            try
            {
                await View.PauseAsync(1f, Lifetime);
                var r = await _finish.ExecuteAsync(Lifetime);
                if (Disposed) return;
                if (r.IsFailure)
                {
                    _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
                    _navigate.Publish(new NavigateRequested(GameState.Board));
                    return;
                }
                await ShowResultsAsync(r.Value);
            }
            catch (OperationCanceledException) { }
            finally { _finishing.Exit(); }
        }

        async UniTask ShowResultsAsync(CombatReport report)
        {
            var view = await _modals.OpenAsync<ICombatResultsPopupView>(Lifetime);
            try
            {
                var content = _session.Content;
                var data = new CombatResultsData
                {
                    Title = report.Victory ? _loc.T("combat.victory") : _loc.T("combat.defeat"),
                    Victory = report.Victory,
                    Reward = report.Victory ? _loc.F("combat.reward", report.Credits) : "",
                    FatigueTitle = _loc.T("combat.fatigue"),
                    Fatigue = report.Fatigue.Select(f => new FatigueRowData
                    {
                        Name = _loc.MemberName(content.Member(f.Member)),
                        Before01 = f.Before / 100f,
                        After01 = f.After / 100f,
                        After = f.After,
                        Label = f.Before + " % → " + f.After + " %",
                        Delta = "+" + (f.After - f.Before) + " %",
                    }).ToArray(),
                    Verify = _loc.T("combat.verify.pending"),
                    ToHq = report.ReturnedToHq ? _loc.T("combat.to_hq") : "",
                    Continue = _loc.T("ui.continue"),
                };
                var shown = view.ShowAsync(data, Lifetime);
                VerifyAsync(view).Forget();
                await shown;
            }
            finally { _modals.Close(view); }
            if (Disposed) return;
            _combat.Clear();
            _sim = null;
            if (report.ReturnedToHq) _toasts.Show(_loc.T("combat.to_hq"), ToastKind.Warning);
            // La machine d'états décharge le combat puis renvoie au plateau (ou au QG si l'escouade est épuisée).
            _closed.Publish(new CombatResultsClosed(report));
        }

        async UniTaskVoid VerifyAsync(ICombatResultsPopupView view)
        {
            bool? ok;
            try { ok = await _verify.ExecuteAsync(Lifetime); }
            catch (OperationCanceledException) { return; }
            catch (Exception) { ok = null; }
            if (Disposed) return;
            view.SetVerification(ok == null ? _loc.T("combat.verify.skip") : ok.Value ? _loc.T("combat.verify.ok") : _loc.T("combat.verify.ko"),
                                 ok == false ? Tone.Danger : ok == true ? Tone.Success : Tone.Muted);
        }

        // ------------------------------------------------------------------ Aides

        string Name(int id) => _names.TryGetValue(id, out var n) ? n : "#" + id;

        string SkillLabel(int actor, SkillId id)
        {
            var u = _sim!.Unit(actor);
            foreach (var s in u.Skills) if (s.Id == id) return _loc.SkillName(s);
            foreach (var c in _session.Content.Combos) if (c.Effect.Id == id) return _loc.ComboName(c);
            return id.Value;
        }

        ComboDef? FindCombo(string id) => _session.Content.Combos.FirstOrDefault(c => c.Id == id);

        bool ComboStillAvailable(CombatSimulation.ComboOption o) => _sim!.AvailableCombos().Any(c => c.A == o.A && c.B == o.B);

        int AliveCount(Side side) => _sim!.Units.Count(u => u.Side == side && u.IsAlive);

        int FirstAlive(Side side) => _sim!.Units.FirstOrDefault(u => u.Side == side && u.IsAlive)?.Id ?? -1;

        void AddLog(string line)
        {
            _log.Add(line);
            if (_log.Count > 8) _log.RemoveAt(0);
            View.ShowLog(_log.ToArray());
        }
    }
}
