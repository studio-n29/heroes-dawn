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
        CombatSimulation.ComboOption? _pendingCombo;
        int _speedFactor = 1;
        float _accumulator;
        bool _ended;
        bool _firstRefresh;
        bool _autoShown;
        int _pendingAvatar = -1;
        readonly HashSet<int> _shown = new HashSet<int>();
        bool _pendingSync;

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
            View.AvatarClicked += Avatar;
            View.PauseClicked += Pause;
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
            View.AvatarClicked -= Avatar;
            View.PauseClicked -= Pause;
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
            _pendingAvatar = -1;
            _pendingSync = false;
            _readyOrder.Clear();
            _log.Clear();
            View.ShowLog(_log);
            BuildUnits(sim);
            _firstRefresh = true;
            _autoShown = sim.Auto;
            RefreshTopBar();
            RefreshAll();
            View.ShowBanner(_loc.T("combat.rank").Replace("{0}", "").Trim().ToUpperInvariant() + " !", Tone.Primary);
            _tutorial?.Hint("auto");
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
                cards.Add(new UnitCardData { Id = u.Id, Ally = u.Side == Side.Ally, Name = name.ToUpperInvariant(), Boss = u.IsBoss, Family = fam, Portrait = new PortraitData(name, fam, u.IsBoss), Sub = u.Side == Side.Ally && u.Id < fighters.Count ? _loc.F("ui.fatigue", fighters[u.Id].Fatigue) : "" });
            }
            View.BuildUnits(cards);
        }

        // ------------------------------------------------------------------ Boucle

        public void Tick()
        {
            var sim = _sim;
            if (sim == null || _ended || Disposed || !View.IsVisible || _modals.AnyOpen || (_tutorial?.PausesCombat ?? false)) return;
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
                        if (_sim!.Unit(su.Actor).Side == Side.Ally) _tutorial?.Signal(TutorialSignal.SkillUsed);
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
                    case AvatarSkillUsedEvent av:
                        _audio.Play(SfxId.SkillCast);
                        View.ShowBanner(AvatarName(_session.Content.AvatarSkills.First(a => a.Id == av.Skill)), Tone.Info);
                        AddLog(_loc.T("combat.avatar") + " : " + AvatarName(_session.Content.AvatarSkills.First(a => a.Id == av.Skill)));
                        foreach (var t in av.Targets) if (av.Category == SkillCategory.Damage) View.Hit(t);
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
            else if (_pendingAvatar >= 0) targetEnemies = true;
            else if (_selected >= 0 && _pendingSlot >= 0)
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
                    Gauge01 = u.GaugeMax == 0 ? 0 : (float)((double)u.Gauge / u.GaugeMax),
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

            var panel = new SkillPanelData { Visible = manual && _selected >= 0, UnitId = _selected, Slots = new SkillSlotData[3], Title = _selected >= 0 ? _loc.F("combat.ready", Name(_selected)).ToUpperInvariant() : "" };
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
            View.ShowAvatar(BuildAvatarBar(sim));

            var combos = manual ? sim.AvailableCombos() : Array.Empty<CombatSimulation.ComboOption>();
            var syncs = manual && combos.Count == 0 ? sim.AvailableSyncCombos() : Array.Empty<CombatSimulation.ComboOption>();
            if (combos.Count > 0) _tutorial?.Hint("combo");
            string comboLabel = combos.Count > 0 ? _loc.T("combat.combo") + "\n<size=24>" + _loc.ComboName(combos[0].Combo) + "</size>"
                              : syncs.Count > 0 ? _loc.T("combat.combo_sync") + "\n<size=24>" + _loc.ComboName(syncs[0].Combo) + "</size>"
                              : "";
            View.ShowCombo(combos.Count > 0 || syncs.Count > 0, comboLabel);
            View.ShowTimeline(BuildTimeline(sim, manual));

            View.SetHint(_pendingAvatar >= 0 ? _loc.F("combat.avatar_pick", AvatarName(sim.AvatarSkills[_pendingAvatar]))
                       : targetEnemies ? _loc.T("combat.choose_target")
                       : targetAllies ? _loc.T("combat.choose_ally")
                       : manual && _selected < 0 ? _loc.T("combat.waiting") : "");
            RefreshTopBar();
        }

        /// <summary>Les quatre icônes de compétence d'un agent : 1 et 2 propres à l'agent, 3 lourde, 4 réservée aux cartes du grimoire.</summary>
        SkillSlotData[] SkillIcons(CombatSimulation sim, CombatUnit u, bool active)
        {
            var slots = new SkillSlotData[4];
            int count = CombatSimulation.ActiveSkillCount(u);
            for (int i = 0; i < 3; i++)
            {
                if (i >= count) { slots[i] = new SkillSlotData { Visible = true, Locked = true, Label = "", Description = "", Glyph = "" }; continue; }
                var s = u.Skills[i];
                string name = _loc.SkillName(s);
                slots[i] = new SkillSlotData
                {
                    Visible = true,
                    Label = "[" + (i + 1) + "] " + name,
                    Description = _loc.DescribeSkill(s),
                    Glyph = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant(),
                    Cooldown = u.Cooldowns[i],
                    Pending = active && _pendingSlot == i,
                    Enabled = active && sim.CanUse(u.Id, i),
                };
            }
            slots[3] = new SkillSlotData { Visible = true, Locked = true, Label = "", Description = "", Glyph = "" };
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
            bool sync = false;
            if (list.Count == 0)
            {
                list = sim.AvailableSyncCombos(); // partenaire proche de 0 : le combo part quand même
                sync = true;
            }
            if (list.Count == 0) return;
            var c = list[0];
            _pendingAvatar = -1;
            if (c.Combo.Effect.Target == TargetRule.SingleEnemy && AliveCount(Side.Enemy) > 1)
            {
                _pendingCombo = c;
                _pendingSync = sync;
                _pendingSlot = -1;
            }
            else Submit(ComboCommand(c, sync, FirstAlive(Side.Enemy)));
            RefreshAll();
        }

        static CombatCommand ComboCommand(CombatSimulation.ComboOption c, bool sync, int target)
            => sync ? CombatCommand.SyncCombo(c.A, c.B, target) : CombatCommand.UseCombo(c.A, c.B, target);

        /// <summary>
        /// Compétence d'avatar : utilisable à tout moment (même pendant la pause d'action). Une cible unique ennemie demande un ciblage
        /// quand plusieurs ennemis sont vivants ; un second appui sur le même bouton annule.
        /// </summary>
        void Avatar(int index)
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || _modals.AnyOpen || Disposed || !sim.CanUseAvatar(index)) return;
            var def = sim.AvatarSkills[index];
            _pendingSlot = -1;
            _pendingCombo = null;
            if (_pendingAvatar == index) _pendingAvatar = -1;
            else if (def.Target == TargetRule.SingleEnemy && AliveCount(Side.Enemy) > 1) _pendingAvatar = index;
            else
            {
                Submit(CombatCommand.UseAvatar(index, def.Target == TargetRule.SingleEnemy ? FirstAlive(Side.Enemy) : -1));
                _tutorial?.Hint("avatar");
            }
            RefreshAll();
        }

        void UnitTapped(int unitId)
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || _modals.AnyOpen || Disposed) return;
            var u = sim.Unit(unitId);
            if (_pendingAvatar >= 0 && u.Side == Side.Enemy && u.IsAlive)
            {
                Submit(CombatCommand.UseAvatar(_pendingAvatar, unitId));
                RefreshAll();
                return;
            }
            if (!CanCommand) return;
            if (_pendingCombo.HasValue && u.Side == Side.Enemy && u.IsAlive)
            {
                var c = _pendingCombo.Value;
                Submit(ComboCommand(c, _pendingSync, unitId));
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
            _pendingSync = false;
            _pendingAvatar = -1;
            if (cmd.Type == CommandType.Skill || cmd.Type == CommandType.Combo)
            {
                _readyOrder.Remove(cmd.Actor);
                if (cmd.Type == CommandType.Combo) _readyOrder.Remove(cmd.Partner);
                _selected = -1;
            }
        }

        public bool HandleBack()
        {
            if (_pendingSlot < 0 && !_pendingCombo.HasValue && _pendingAvatar < 0) return false;
            _pendingSlot = -1;
            _pendingCombo = null;
            _pendingAvatar = -1;
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
            _pendingAvatar = -1;
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

        bool ComboStillAvailable(CombatSimulation.ComboOption o)
            => (_pendingSync ? _sim!.AvailableSyncCombos() : _sim!.AvailableCombos()).Any(c => c.A == o.A && c.B == o.B);

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
                    Position01 = u.GaugeMax == 0 ? 0f : Math.Max(0f, Math.Min(1f, (float)((double)u.Gauge / u.GaugeMax))),
                    Channelling = u.Gauge < 0,
                    Ready = u.IsReady,
                    InPrep = sim.InPrepZone(u),
                    PlannedSlot = u.PlannedSlot,
                    Boss = u.IsBoss,
                });
            }
            return new TimelineData
            {
                Markers = markers.ToArray(),
                PrepStart01 = b.TimelinePrepZonePercent / 100f,
            };
        }

        /// <summary>Barre des compétences d'avatar : nom court, recharge restante en secondes de temps actif, ciblage en cours.</summary>
        AvatarBarData BuildAvatarBar(CombatSimulation sim)
        {
            int tickMs = _session.Content.Balance.TickMs;
            var slots = new AvatarSlotData[sim.AvatarSkills.Count];
            for (int i = 0; i < slots.Length; i++)
            {
                var def = sim.AvatarSkills[i];
                int left = sim.AvatarCooldownLeft(i);
                slots[i] = new AvatarSlotData
                {
                    Visible = true,
                    Label = "[" + AvatarKeys[Math.Min(i, AvatarKeys.Length - 1)] + "] " + AvatarName(def)
                          + "\n<size=22>" + (left > 0 ? Math.Ceiling(left * tickMs / 1000.0) + " s" : _loc.T("combat.avatar_ready")) + "</size>",
                    Category = def.Category,
                    Enabled = sim.CanUseAvatar(i),
                    Pending = _pendingAvatar == i,
                    Cooldown01 = def.CooldownTicks <= 0 ? 0f : left / (float)def.CooldownTicks,
                };
            }
            return new AvatarBarData { Visible = !sim.IsOver, Title = _loc.T("combat.avatar"), Slots = slots };
        }

        static readonly string[] AvatarKeys = { "Q", "W", "E", "R", "T" };

        string AvatarName(AvatarSkillDef def) => _loc.Or("avatar." + def.Id.Value + ".name", "combat.avatar");

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
