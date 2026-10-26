#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Tutorial;
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
        readonly TutorialGuide? _tutorial;

        CombatSimulation? _sim;
        readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        readonly List<int> _readyOrder = new List<int>();
        readonly List<string> _log = new List<string>();
        readonly BusyGate _finishing = new BusyGate();
        int _selected = -1;
        int _pendingSlot = -1;
        int _dragSlot = -1;
        int _speedFactor = 2; // vitesse ×2 par défaut (spec 2026-10-12 : combat plus rapide)
        const float StepEpsilon = 0.0005f; // evite qu une erreur de flottant retarde un pas d une image
        float _stepTimer;
        float _pauseLeft;
        float _turnLeft;
        bool _waitingForPlayer;
        System.Random _aiRandom = new System.Random(1);
        /// <summary>Pause en cours en secondes (fenetre d interruption ou reflexion de l IA) ; tests.</summary>
        public float PauseLeft => _pauseLeft;
        /// <summary>Temps restant au joueur pour choisir au point d action, en secondes ; tests.</summary>
        public float TurnLeft => _turnLeft;
        bool _ended;
        bool _firstRefresh;
        bool _autoShown;
        readonly HashSet<int> _shown = new HashSet<int>();

        public CombatPresenter(ICombatView view, IActiveCombatReadModel combat, ISessionReadModel session, ISetAutoCombatUseCase setAuto,
                               IFinishCombatUseCase finish, IVerifyCombatUseCase verify, IForceCombatWinUseCase forceWin,
                               IPublisher<CombatResultsClosed> closed, IPublisher<NavigateRequested> navigate, IModalService modals,
                               IToastService toasts, ILocalization loc, IFrameClock clock, BackButtonRouter router,
                               IAudioService? audio = null, TutorialGuide? tutorial = null)
            : base(view, router)
        {
            _combat = combat; _session = session; _setAuto = setAuto; _finish = finish; _verify = verify; _forceWin = forceWin;
            _closed = closed; _navigate = navigate; _modals = modals; _toasts = toasts; _loc = loc; _clock = clock;
            _audio = audio ?? NullAudioService.Instance;
            _tutorial = tutorial;
        }

        protected override IBackHandler BackHandler => this;

        protected override void OnStart()
        {
            View.RetreatClicked += Retreat;
            View.AutoClicked += ToggleAuto;
            View.SpeedClicked += ToggleSpeed;
            View.DebugWinClicked += DebugWin;
            View.PauseClicked += Pause;
            View.CycleClicked += CycleSelection;
            View.SkillClicked += Skill;
            View.SkillDragBegan += SkillDragBegan;
            View.SkillDropped += SkillDropped;
            View.UnitTapped += UnitTapped;
        }

        protected override void OnDispose()
        {
            View.RetreatClicked -= Retreat;
            View.AutoClicked -= ToggleAuto;
            View.SpeedClicked -= ToggleSpeed;
            View.DebugWinClicked -= DebugWin;
            View.PauseClicked -= Pause;
            View.CycleClicked -= CycleSelection;
            View.SkillClicked -= Skill;
            View.SkillDragBegan -= SkillDragBegan;
            View.SkillDropped -= SkillDropped;
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
            _stepTimer = _session.Content.Balance.TimelineStepMs / 1000f;
            _pauseLeft = 0f;
            _waitingForPlayer = false;
            _aiRandom = new System.Random((int)(sim.Setup.Seed & 0x7fffffff));
            _selected = -1;
            _pendingSlot = -1;
            _readyOrder.Clear();
            _log.Clear();
            View.ShowLog(_log);
            BuildUnits(sim);
            _firstRefresh = true;
            _autoShown = sim.Auto;
            RefreshTopBar();
            RefreshAll();
            View.ShowBanner(_loc.T("combat.rank").Replace("{0}", "").Trim().ToUpperInvariant() + " !", Tone.Primary);
            LogSynergies(sim);
            _tutorial?.Hint("auto");
        }

        /// <summary>Synergies d'équipe actives (bonus permanents) : une ligne du journal par paire, au début du combat.</summary>
        void LogSynergies(CombatSimulation sim)
        {
            var allies = sim.Units.Where(u => u.Side == Side.Ally).ToList();
            var active = SynergyRules.Active(allies.Select(u => u.Family).ToList(), _session.Content.Synergies);
            var chips = new List<BonusChipData>();
            foreach (var a in active)
            {
                string effectText = _loc.SynergyEffect(a.Synergy, sim.Setup.LinkLevel(new MemberId(allies[a.A].DefId), new MemberId(allies[a.B].DefId)));
                AddLog(_loc.F("combat.synergy", _loc.SynergyName(a.Synergy), effectText, Name(allies[a.A].Id), Name(allies[a.B].Id)));
                chips.Add(new BonusChipData { Name = _loc.SynergyName(a.Synergy), Effect = effectText, Color = Theme.Accent });
            }
            foreach (var effect in TeamEffects.Active(allies.Select(u => u.Family).ToList(), allies.Select(u => u.Role).ToList()))
            {
                AddLog(_loc.F("combat.team_effect", _loc.TeamEffect(effect)));
                chips.Add(new BonusChipData { Name = _loc.T("team_effect." + effect + ".name"), Effect = _loc.T("team_effect." + effect + ".short"), Color = Theme.Success });
            }
            const int MaxChips = 6;
            if (chips.Count > MaxChips)
            {
                int more = chips.Count - (MaxChips - 1);
                chips.RemoveRange(MaxChips - 1, more);
                chips.Add(new BonusChipData { Name = _loc.F("combat.bonus_more", more), Effect = _loc.T("combat.bonus_more_sub"), Color = Theme.Surface2 });
            }
            View.ShowBonuses(chips);
        }

        void BuildUnits(CombatSimulation sim)
        {
            _names.Clear();
            _shown.Clear();
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
                if (u.Side == Side.Enemy && !u.IsAlive) continue; // ennemis des vagues précédentes
                _shown.Add(u.Id);
                cards.Add(new UnitCardData { Id = u.Id, Ally = u.Side == Side.Ally, Name = name.ToUpperInvariant(), Boss = u.IsBoss, Family = fam, Portrait = new PortraitData(name, fam, u.IsBoss, u.Side == Side.Ally ? AgentArt.For(u.DefId) : null), Sub = u.Side == Side.Ally && u.Id < fighters.Count ? _loc.F("ui.fatigue", fighters[u.Id].Fatigue) : "" });
            }
            View.BuildUnits(cards);
        }

        // ------------------------------------------------------------------ Boucle

        public void Tick()
        {
            var sim = _sim;
            if (sim == null || _ended || Disposed || !View.IsVisible || _modals.AnyOpen || (_tutorial?.PausesCombat ?? false)) return;
            var balance = _session.Content.Balance;
            float real = _clock.UnscaledDeltaTime;
            float dt = real * _speedFactor;
            var pendingEvents = sim.ApplyCommands(); // ordres du joueur traites des qu ils arrivent (Auto, interruptions, combos)
            if (pendingEvents.Count > 0) Play(pendingEvents);
            if (sim.IsAwaitingAction)
            {
                // Tour d un agent : la frise est figee ; le joueur a au plus TimelineTurnTimeoutMs pour choisir, sinon l unite passe son tour.
                if (!_waitingForPlayer) { _waitingForPlayer = true; _turnLeft = balance.TimelineTurnTimeoutMs / 1000f; }
                _turnLeft -= real;
                if (_turnLeft <= 0f && !sim.Auto)
                {
                    int ready = _readyOrder.Count > 0 ? _readyOrder[0] : FirstReadyAlly(sim);
                    if (ready >= 0) { sim.Submit(CombatCommand.Pass(ready)); ToastTurnPassed(ready); }
                    _turnLeft = balance.TimelineTurnTimeoutMs / 1000f;
                }
                Play(sim.Step(1));
            }
            else
            {
                _waitingForPlayer = false;
                if (_pauseLeft > 0f)
                {
                    // Pause (point d action vide ou reflexion de l IA) : le joueur peut interrompre, la frise reprend a la fin.
                    _pauseLeft -= dt;
                }
                else
                {
                    _stepTimer -= dt;
                    int steps = 0;
                    float stepSeconds = balance.TimelineStepMs / 1000f;
                    while (_stepTimer <= StepEpsilon && steps < 4 && !sim.IsOver && _pauseLeft <= 0f && !sim.IsAwaitingAction)
                    {
                        _stepTimer += stepSeconds;
                        steps++;
                        var events = sim.Step(1);
                        if (events.Count > 0) Play(events);
                    }
                    if (_stepTimer < -stepSeconds * 4) _stepTimer = 0f;
                }
            }
            RefreshAll();
            if (sim.IsOver && !_ended)
            {
                _ended = true;
                FinishAsync().Forget();
            }
        }

        /// <summary>Delai de reflexion de l IA, entre les bornes de l equilibrage (presentation seulement).</summary>
        float AiThinkingSeconds()
        {
            var b = _session.Content.Balance;
            return _aiRandom.Next(b.TimelineAiDelayMinMs, b.TimelineAiDelayMaxMs + 1) / 1000f;
        }

        int FirstReadyAlly(CombatSimulation sim)
        {
            foreach (var u in sim.Units) if (u.Side == Side.Ally && u.IsReady) return u.Id;
            return -1;
        }

        /// <summary>Animation de la compétence dans le terrain ; la frise attend la fin de la pause de résolution (0 sans animation).</summary>
        void PlaySkillFx(SkillUsedEvent su)
        {
            SkillDef? skill = null;
            foreach (var s in _sim!.Unit(su.Actor).Skills) if (s.Id == su.Skill) { skill = s; break; }
            if (skill == null) return;
            _lastFx = new SkillFxData
            {
                ActorId = su.Actor, TargetIds = su.Targets, Kind = SkillFx.KindOf(skill), Title = _loc.SkillName(skill),
            };
            float seconds = View.PlaySkillFx(_lastFx.Value, _session.Content.Balance.CombatResolutionMs / 1000f);
            _pauseLeft = Math.Max(_pauseLeft, seconds);
        }

        /// <summary>Dernière compétence animée : un rebond rejoue son impact de la première cible vers la seconde.</summary>
        SkillFxData? _lastFx;

        void PlayBounce(BounceEvent b)
        {
            View.Hit(b.To);
            if (_lastFx == null) return;
            var fx = _lastFx.Value;
            fx.ActorId = b.From;
            fx.TargetIds = new[] { b.To };
            View.PlaySkillFx(fx, _session.Content.Balance.CombatResolutionMs / 2000f);
        }

        void ToastTurnPassed(int unitId) => AddLog(Name(unitId) + " : " + _loc.T("combat.turn_passed"));

        internal void Play(IReadOnlyList<CombatEvent> events)
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
                        if (_sim!.Unit(su.Actor).Side == Side.Ally) _tutorial?.Signal(TutorialSignal.SkillUsed);
                        else if (!_sim.Auto) _pauseLeft = Math.Max(_pauseLeft, AiThinkingSeconds());
                        PlaySkillFx(su);
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
                    case ExecutedEvent ex:
                        View.Float(ex.Target, _loc.T("combat.executed"), Tone.Danger, 56);
                        break;
                    case PurgedEvent pu:
                        View.Float(pu.Target, _loc.T("combat.purged"), Tone.Info, 34);
                        break;
                    case BounceEvent bo:
                        PlayBounce(bo);
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
                    case EncounterEvent enc:
                        View.ShowBanner(_loc.T("combat.parade"), Tone.Info);
                        AddLog(Name(enc.A) + " + " + Name(enc.B) + " : " + _loc.T("combat.parade"));
                        break;
                    case ActionPlannedEvent ap:
                        if (_sim!.Unit(ap.Unit).Side == Side.Enemy)
                        {
                            View.Float(ap.Unit, ap.Slot == 2 ? _loc.T("combat.planned_heavy") : _loc.T("combat.planned"), ap.Slot == 2 ? Tone.Danger : Tone.Primary, 34);
                            AddLog(Name(ap.Unit) + " : " + _loc.F("combat.prepares", SkillLabel(ap.Unit, _sim.Unit(ap.Unit).Skills[ap.Slot].Id)));
                            _tutorial?.Hint("timeline");
                        }
                        break;
                    case ActionInterruptedEvent ai:
                        View.Float(ai.Unit, _loc.T("combat.interrupted"), _sim!.Unit(ai.Unit).Side == Side.Enemy ? Tone.Success : Tone.Danger, 40);
                        View.Hit(ai.Unit);
                        AddLog(Name(ai.Unit) + " : " + _loc.T("combat.interrupted"));
                        break;
                    case PushedBackEvent pb:
                        View.Float(pb.Unit, _loc.F("combat.pushed", pb.Percent), Tone.Perception, 30);
                        break;
                    case AdvancedEvent ad:
                        View.Float(ad.Unit, _loc.F("combat.advanced", ad.Percent), Tone.Success, 34);
                        if (ad.Ready) { View.Act(ad.Unit); AddLog(Name(ad.Unit) + " : " + _loc.T("combat.propelled")); }
                        break;
                    case WaveStartedEvent ws:
                        BuildUnits(_sim!);
                        _firstRefresh = true;
                        View.ShowBanner(_loc.F("combat.wave_banner", ws.Wave + 1), Tone.Primary);
                        AddLog(_loc.F("combat.wave", ws.Wave + 1, _sim!.WaveCount));
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
            if (!manual) { _selected = -1; _pendingSlot = -1; }

            bool targetEnemies = false, targetAllies = false;
            if (_selected >= 0 && _pendingSlot >= 0)
            {
                var rule = sim.Unit(_selected).Skills[_pendingSlot].Target;
                targetEnemies = rule == TargetRule.SingleEnemy;
                targetAllies = rule == TargetRule.SingleAlly;
            }

            var states = new List<UnitStateData>(sim.Units.Count);
            foreach (var u in sim.Units)
            {
                if (!_shown.Contains(u.Id)) continue;
                bool ally = u.Side == Side.Ally;
                states.Add(new UnitStateData
                {
                    Id = u.Id,
                    Ally = ally,
                    Hp01 = u.MaxHp == 0 ? 0 : u.Hp / (float)u.MaxHp,
                    HpText = u.Hp + " / " + u.MaxHp,
                    Gauge01 = u.ActionPoint == 0 ? 0 : Math.Max(0f, Math.Min(1f, u.Point / (float)u.ActionPoint)),
                    Energy01 = u.MaxEnergy == 0 ? 0f : Math.Max(0f, Math.Min(1f, u.Energy / (float)u.MaxEnergy)),
                    EnergyText = u.Energy + " / " + u.MaxEnergy,
                    GaugeFull = u.IsReady,
                    Statuses = Statuses(u),
                    Alive = u.IsAlive,
                    Highlight = manual && ally && u.IsReady,
                    Selected = u.Id == _selected && u.IsAlive,
                    Targetable = u.IsAlive && ((!ally && targetEnemies) || (ally && targetAllies)),
                    Instant = _firstRefresh,
                    Skills = ally ? SkillIcons(sim, u, manual && u.Id == _selected && u.IsAlive) : null,
                });
            }
            _firstRefresh = false;
            View.ShowUnits(states);

            var panel = new SkillPanelData { Visible = manual && _selected >= 0, UnitId = _selected, Slots = new SkillSlotData[4], Title = _selected >= 0 ? _loc.F("combat.ready", Name(_selected)).ToUpperInvariant() : "" };
            if (panel.Visible)
            {
                var u = sim.Unit(_selected);
                int count = CombatSimulation.ActiveSkillCount(u);
                for (int i = 0; i < 4; i++)
                {
                    if (i >= count) continue;
                    var s = u.Skills[i];
                    string label = "[" + (i + 1) + "] " + _loc.SkillName(s);
                    if (s.Cost > 0) label += "  (" + _loc.F("combat.cost", s.Cost) + ")";
                    panel.Slots[i] = new SkillSlotData { Visible = true, Label = label, Description = _loc.DescribeSkill(s, u.Atk, u.MaxHp), Pending = _pendingSlot == i, Enabled = sim.CanUse(u.Id, i) };
                }
            }
            View.ShowSkills(panel);
            View.ShowTimeline(BuildTimeline(sim, manual));

            View.SetHint(targetEnemies ? _loc.T("combat.choose_target")
                       : targetAllies ? _loc.T("combat.choose_ally")
                       : manual && _selected < 0 ? _loc.T("combat.waiting") : "");
            RefreshTopBar();
        }

        /// <summary>Les quatre icônes de compétence d'un agent : 1 et 2 propres à l'agent, 3 lourde, 4 réservée aux cartes du grimoire.</summary>
        SkillSlotData[] SkillIcons(CombatSimulation sim, CombatUnit u, bool active)
        {
            var slots = new SkillSlotData[4];
            int count = CombatSimulation.ActiveSkillCount(u);
            for (int i = 0; i < 4; i++)
            {
                if (i >= count) { slots[i] = new SkillSlotData { Visible = true, Locked = true, Label = "", Description = "", Glyph = "" }; continue; }
                var s = u.Skills[i];
                string name = _loc.SkillName(s);
                slots[i] = new SkillSlotData
                {
                    Visible = true,
                    Label = "[" + (i + 1) + "] " + name,
                    Description = _loc.DescribeSkill(s, u.Atk, u.MaxHp),
                    Glyph = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant(),
                    Cost = s.Cost,
                    Affordable = u.Energy >= s.Cost,
                    Pending = active && _pendingSlot == i,
                    Enabled = active && sim.CanUse(u.Id, i),
                    NeedsTarget = NeedsTarget(s.Target),
                };
            }
            return slots;
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
                Title = (pending != null ? _loc.F("combat.rank", _loc.RankName(pending.Rank)).ToUpperInvariant() : "")
                        + (_sim != null && _sim.WaveCount > 1 ? "  ·  " + _loc.F("combat.wave", _sim.WaveIndex + 1, _sim.WaveCount) : ""),
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
            var rule = sim.Unit(_selected).Skills[slot].Target;
            if (rule == TargetRule.SingleEnemy && AliveCount(Side.Enemy) == 1) Submit(CombatCommand.UseSkill(_selected, slot, FirstAlive(Side.Enemy)));
            else if (rule == TargetRule.SingleEnemy || rule == TargetRule.SingleAlly) _pendingSlot = _pendingSlot == slot ? -1 : slot; // re-toucher annule
            else Submit(CombatCommand.UseSkill(_selected, slot, -1));
            RefreshAll();
        }

        /// <summary>Une compétence se pose sur une cible s'il faut en choisir une (un seul ennemi vivant : la cible est évidente).</summary>
        bool NeedsTarget(TargetRule rule)
            => (rule == TargetRule.SingleEnemy && AliveCount(Side.Enemy) > 1) || rule == TargetRule.SingleAlly;

        /// <summary>Carte prise : une compétence ciblée allume ses cibles valides, une générale attend d'être lâchée dans le vide.</summary>
        void SkillDragBegan(int slot)
        {
            var sim = _sim;
            if (!CanCommand || _selected < 0 || !sim!.CanUse(_selected, slot)) return;
            _dragSlot = slot;
            if (NeedsTarget(sim.Unit(_selected).Skills[slot].Target)) _pendingSlot = slot;
            RefreshAll();
        }

        /// <summary>Carte lâchée : sur une cible valide (compétence ciblée), dans le vide (compétence générale), sinon retour sans effet.</summary>
        void SkillDropped(int slot, int where)
        {
            var sim = _sim;
            if (_dragSlot != slot) return;
            _dragSlot = -1;
            if (!CanCommand || _selected < 0 || !sim!.CanUse(_selected, slot) || where == ICombatView.DropCancel)
            {
                _pendingSlot = -1;
                RefreshAll();
                return;
            }
            var rule = sim.Unit(_selected).Skills[slot].Target;
            if (!NeedsTarget(rule)) { Skill(slot); return; }
            if (where >= 0 && IsValidTarget(rule, sim.Unit(where)) && _pendingSlot == slot) { UnitTapped(where); return; }
            _pendingSlot = -1;
            RefreshAll();
        }

        void UnitTapped(int unitId)
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || _modals.AnyOpen || Disposed) return;
            var u = sim.Unit(unitId);
            if (!CanCommand) return;
            if (_selected >= 0 && _pendingSlot >= 0 && IsValidTarget(sim.Unit(_selected).Skills[_pendingSlot].Target, u))
                Submit(CombatCommand.UseSkill(_selected, _pendingSlot, unitId));
            else if (u.Side == Side.Ally && u.IsReady)
            {
                _selected = unitId;
                _pendingSlot = -1;
            }
            RefreshAll();
        }

        static bool IsValidTarget(TargetRule rule, CombatUnit u)
            => u.IsAlive && ((rule == TargetRule.SingleEnemy && u.Side == Side.Enemy) || (rule == TargetRule.SingleAlly && u.Side == Side.Ally));

        void Submit(CombatCommand cmd)
        {
            _sim?.Submit(cmd);
            if (cmd.Type == CommandType.Skill) View.PlayCard(cmd.SlotIndex, cmd.Target);
            // Tutoriel : l'ordre donné lève la pause de l'étape (un autre allié prêt garderait sinon la cible « compétences » active et figerait le combat).
            if (cmd.Type == CommandType.Skill) _tutorial?.Signal(TutorialSignal.SkillUsed);
            _pendingSlot = -1;
            if (cmd.Type == CommandType.Skill)
            {
                _readyOrder.Remove(cmd.Actor);
                _selected = -1;
            }
        }

        public bool HandleBack()
        {
            if (_pendingSlot < 0) return false;
            _pendingSlot = -1;
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

        /// <summary>Pause : le combat est figé tant que la modale est ouverte ; « Abandonner » passe par la confirmation de retraite.</summary>
        void Pause()
        {
            if (_sim == null || _sim.IsOver || Disposed || _modals.AnyOpen) return;
            PauseAsync().Forget();
        }

        async UniTaskVoid PauseAsync()
        {
            try
            {
                bool resume = await _modals.ConfirmAsync(new ConfirmData(_loc.T("combat.pause.title"), _loc.T("combat.pause.body"),
                                                                         _loc.T("combat.pause.resume"), _loc.T("combat.pause.abandon"), Tone.Primary), Lifetime);
                if (!resume) Retreat();
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

        string DropLine(CombatReport report, ContentCatalog content)
            => report.Drop.HasValue ? System.Environment.NewLine + _loc.F("combat.drop", _loc.T(content.Collection.Item(report.Drop.Value).NameKey)) : "";

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
                    Reward = report.Victory ? _loc.F("combat.reward", report.Credits) + DropLine(report, content) : "",
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
            return id.Value;
        }

        /// <summary>Frise chronologique : position de chaque unité, actions annoncées, Time Bank et bouton DÉPASSER.</summary>
        TimelineData BuildTimeline(CombatSimulation sim, bool manual)
        {
            var b = _session.Content.Balance;
            var markers = new List<TimelineMarkerData>(sim.Units.Count);
            foreach (var u in sim.Units)
            {
                if (!_shown.Contains(u.Id)) continue;
                string name = _names.TryGetValue(u.Id, out var n) ? n : "?";
                markers.Add(new TimelineMarkerData
                {
                    Id = u.Id,
                    Ally = u.Side == Side.Ally,
                    Alive = u.IsAlive,
                    Initial = name.Length > 0 ? name.Substring(0, 1).ToUpperInvariant() : "?",
                    Family = u.Family,
                    Position01 = u.ActionPoint == 0 ? 0f : Math.Max(0f, Math.Min(1f, u.Point / (float)u.ActionPoint)),
                    Point = u.Point,
                    Channelling = u.IsChannelling,
                    Ready = u.IsReady,
                    InPrep = sim.InPrepZone(u),
                    PlannedSlot = u.PlannedSlot,
                    Boss = u.IsBoss,
                });
            }
            return new TimelineData
            {
                Markers = markers.ToArray(),
                PrepStart01 = b.TimelinePrepZonePoint / (float)b.TimelinePoints,
                Points = b.TimelinePoints,
            };
        }

        int AliveCount(Side side) => _sim!.Units.Count(u => u.Side == side && u.IsAlive);

        int FirstAlive(Side side) => _sim!.Units.FirstOrDefault(u => u.Side == side && u.IsAlive)?.Id ?? -1;

        void AddLog(string line)
        {
            _log.Add(line);
            if (_log.Count > 5) _log.RemoveAt(0);
            View.ShowLog(_log.ToArray());
        }
    }
}
