#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using N29.HeroesDawn.Application.Events;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Combat ATB en temps actif (02_Combat). Fait avancer la simulation à pas fixes et joue ses événements.
    /// Saisie : allié prêt sélectionné automatiquement → compétence → cible (si mono-cible) → commande.
    /// </summary>
    public sealed class CombatScreen : UiScreen
    {
        CombatSimulation? _sim;
        readonly Dictionary<int, UnitCard> _cards = new Dictionary<int, UnitCard>();
        readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        readonly List<int> _readyOrder = new List<int>();
        readonly List<string> _log = new List<string>();

        RectTransform _enemyRow = null!;
        RectTransform _allyRow = null!;
        RectTransform _skillPanel = null!;
        readonly UiButton[] _skillButtons = new UiButton[3];
        readonly Text[] _skillDesc = new Text[3];
        UiButton _combo = null!;
        UiButton _auto = null!;
        UiButton _speed = null!;
        UiButton _retreat = null!;
        UiButton _debug = null!;
        Text _title = null!;
        Text _hint = null!;
        Text _logText = null!;
        Text _banner = null!;

        int _selected = -1;
        int _pendingSlot = -1;
        CombatSimulation.ComboOption? _pendingCombo;
        int _speedFactor = 1;
        float _accumulator;
        bool _ended;
        CancellationTokenSource? _cts;

        protected override void Build()
        {
            Backdrop(Theme.WithAlpha(Theme.Danger, 0.22f), Theme.Hex(0x120E14));
            ArtBackdrop(Sprites.BackgroundLandscape, 1f, 0.8f);

            // ---- Barre du haut
            var top = Ui.Panel(Rect, "TopBar", Theme.SurfaceDark, 0f, 0f, true).Frame().TopBand(90);
            _retreat = Ui.Btn(top, Loc.T("combat.retreat"), Theme.Surface3, Theme.Text, OnRetreat, 30);
            _retreat.Rect.At(0f, 0.5f, 20, 0, 240, 64);
            _title = Ui.Txt(top, "", 44, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            _title.rectTransform.At(0.5f, 0.5f, 0, 0, 900, 64);
            _title.Inked(2.5f);
            _auto = Ui.Btn(top, Loc.T("combat.auto"), Theme.Surface3, Theme.Text, ToggleAuto, 32);
            _auto.Rect.At(1f, 0.5f, -170, 0, 170, 64);
            _speed = Ui.Btn(top, Loc.F("combat.speed", 1), Theme.Surface3, Theme.Text, ToggleSpeed, 32);
            _speed.Rect.At(1f, 0.5f, -20, 0, 130, 64);
            _debug = Ui.Btn(top, Loc.T("combat.debug_win"), Theme.Hex(0x5B2A6B), Color.white, OnDebugWin, 20, FontKind.Body);
            _debug.Rect.At(0f, 0.5f, 280, 0, 220, 50);
            _debug.gameObject.SetActive(Debug.isDebugBuild);

            // ---- Ennemis (haut), alliés (bas)
            _enemyRow = Ui.Rect(Rect, "Enemies").At(0.5f, 1f, 0, -130, 1500, 210);
            _enemyRow.HLayout(40, 0, TextAnchor.MiddleCenter);
            _allyRow = Ui.Rect(Rect, "Allies").At(0.5f, 0f, 0, 24, 1840, 190);
            _allyRow.HLayout(24, 0, TextAnchor.MiddleCenter);

            // ---- Journal (gauche)
            var logFill = Ui.Panel(Rect, "Log", Theme.WithAlpha(Theme.SurfaceDark, 0.85f), 3f, 4f, false);
            logFill.Frame().At(0f, 0.5f, 24, 70, 560, 250);
            _logText = Ui.Txt(logFill.transform, "", 21, Theme.TextMuted, TextAnchor.LowerLeft, FontKind.Regular, "Lines");
            _logText.rectTransform.Stretch(14, 10, 14, 10);

            // ---- Bandeau central (combo, retour d'info)
            _banner = Ui.Txt(Rect, "", 70, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Banner");
            _banner.rectTransform.At(0.5f, 0.5f, 120, 80, 1100, 110);
            _banner.Inked(4f);
            _hint = Ui.Txt(Rect, "", 32, Color.white, TextAnchor.MiddleCenter, FontKind.Body, "Hint");
            _hint.rectTransform.At(0.5f, 0f, 0, 400, 1000, 44);
            _hint.Inked(1.5f);

            // ---- Compétences
            _skillPanel = Ui.Rect(Rect, "Skills").At(0.5f, 0f, 60, 236, 1380, 150);
            _skillPanel.HLayout(20, 0, TextAnchor.MiddleCenter);
            for (int i = 0; i < 3; i++)
            {
                int slot = i;
                var b = Ui.Btn(_skillPanel, "", Theme.Primary, Theme.TextDark, () => OnSkill(slot), 32);
                b.Rect.Pref(440, 146);
                b.Label.alignment = TextAnchor.UpperCenter;
                b.Label.rectTransform.Stretch(12, 60, 12, 8);
                var desc = Ui.Txt(b.Fill.transform, "", 19, Theme.TextDark, TextAnchor.UpperCenter, FontKind.Body, "Desc");
                desc.rectTransform.Stretch(14, 8, 14, 60);
                _skillButtons[i] = b;
                _skillDesc[i] = desc;
            }

            // ---- Combo
            _combo = Ui.Btn(Rect, Loc.T("combat.combo"), Theme.Danger, Color.white, OnCombo, 40);
            _combo.Rect.At(1f, 0f, -24, 400, 360, 130);
            _combo.Label.Inked(2f);
            var pulse = _combo.gameObject.AddComponent<Pulse>();
            pulse.Amplitude = 0.07f;
            _combo.gameObject.SetActive(false);
        }

        public override void OnShow()
        {
            var sim = Ctx.S.Combat.Current;
            if (sim == null)
            {
                Ctx.Nav.Show(Ctx.Board);
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
            _logText.text = "";
            _banner.text = "";
            BuildCards();
            var pending = Ctx.S.Combat.CurrentCase;
            _title.text = pending != null ? Loc.F("combat.rank", Loc.RankName(pending.Rank)).ToUpperInvariant() : "";
            RefreshAuto();
            RefreshAll();
            ShowBanner(Loc.T("combat.rank").Replace("{0}", "").Trim().ToUpperInvariant() + " !", Theme.Primary);
        }

        public override void OnHide()
        {
            _cts?.Cancel();
        }

        void BuildCards()
        {
            _enemyRow.Clear();
            _allyRow.Clear();
            _cards.Clear();
            _names.Clear();
            var sim = _sim!;
            var fighters = Ctx.S.Combat.Fighters;
            var content = Ctx.S.Session.Content;
            foreach (var u in sim.Units)
            {
                string name;
                Family fam;
                if (u.Side == Side.Ally)
                {
                    var def = u.Id < fighters.Count ? fighters[u.Id].Def : content.Member(new MemberId(u.DefId));
                    name = Loc.MemberName(def);
                    fam = def.Family;
                    var card = UnitCard.Create(_allyRow, u, name, fam, true, 430, 180);
                    card.Rect.Pref(430, 180);
                    card.Tapped += OnUnitTapped;
                    _cards[u.Id] = card;
                }
                else
                {
                    var def = content.Enemy(new EnemyId(u.DefId));
                    name = Loc.EnemyName(def);
                    fam = def.Family;
                    var card = UnitCard.Create(_enemyRow, u, name, fam, false, 440, 200);
                    card.Rect.Pref(440, 200);
                    card.Tapped += OnUnitTapped;
                    _cards[u.Id] = card;
                }
                _names[u.Id] = name;
            }
        }

        // ------------------------------------------------------------------ Boucle

        void Update()
        {
            var sim = _sim;
            if (sim == null || _ended) return;
            HandleKeys();
            if (Ctx.Modal.AnyOpen) return;

            int tickMs = Ctx.S.Session.Content.Balance.TickMs;
            _accumulator += Time.unscaledDeltaTime * 1000f * _speedFactor;
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
                Tween.Delay(1.0f, Finish, this);
            }
        }

        void HandleKeys()
        {
            if (Ctx.Modal.AnyOpen) return;
            if (Keys.Pressed(Key.Digit1) || Keys.Pressed(Key.Numpad1)) OnSkill(0);
            if (Keys.Pressed(Key.Digit2) || Keys.Pressed(Key.Numpad2)) OnSkill(1);
            if (Keys.Pressed(Key.Digit3) || Keys.Pressed(Key.Numpad3)) OnSkill(2);
            if (Keys.Pressed(Key.C) && _combo.gameObject.activeSelf) OnCombo();
            if (Keys.Pressed(Key.A)) ToggleAuto();
            if (Keys.Pressed(Key.Escape)) CancelTargeting();
            if (Keys.Pressed(Key.Tab)) CycleSelection();
        }

        void Play(IReadOnlyList<CombatEvent> events)
        {
            foreach (var e in events)
            {
                switch (e)
                {
                    case GaugeFullEvent g:
                        if (_sim!.Unit(g.Unit).Side == Side.Ally && !_readyOrder.Contains(g.Unit)) _readyOrder.Add(g.Unit);
                        break;
                    case SkillUsedEvent su:
                        Card(su.Actor)?.Act();
                        AddLog(Name(su.Actor) + " : " + SkillLabel(su.Actor, su.Skill));
                        break;
                    case DamageEvent d:
                        var dc = Card(d.Target);
                        if (dc != null)
                        {
                            dc.Float((d.Crit ? "!" : "") + "-" + d.Amount, d.Crit ? Theme.Primary : Color.white, d.Crit ? 56 : 44);
                            dc.Hit();
                        }
                        break;
                    case HealEvent h:
                        if (h.Amount > 0) Card(h.Target)?.Float("+" + h.Amount, Theme.Success);
                        break;
                    case ShieldAbsorbedEvent sa:
                        Card(sa.Target)?.Float("(" + sa.Amount + ")", Theme.Shield, 34);
                        break;
                    case MissEvent m:
                        Card(m.Target)?.Float(Loc.T("combat.miss"), Theme.TextMuted, 38);
                        break;
                    case StatusAppliedEvent st:
                        if (st.Status != StatusType.Shield) Card(st.Target)?.Float(Loc.StatusName(st.Status), Theme.Perception, 30);
                        break;
                    case TurnSkippedEvent ts:
                        Card(ts.Unit)?.Float(Loc.T("combat.skip"), Theme.Info, 36);
                        break;
                    case UnitDefeatedEvent ud:
                        AddLog(Name(ud.Unit) + " : K.O.");
                        _readyOrder.Remove(ud.Unit);
                        break;
                    case ComboUsedEvent cu:
                        var combo = FindCombo(cu.ComboId);
                        ShowBanner(Loc.T("combat.combo") + " " + (combo != null ? Loc.ComboName(combo) : cu.ComboId), Theme.Danger);
                        AddLog(Name(cu.A) + " + " + Name(cu.B) + " : " + (combo != null ? Loc.ComboName(combo) : cu.ComboId));
                        break;
                    case CombatEndedEvent ce:
                        ShowBanner(ce.Victory ? Loc.T("combat.victory") : Loc.T("combat.defeat"), ce.Victory ? Theme.Primary : Theme.Danger);
                        break;
                }
            }
        }

        // ------------------------------------------------------------------ Rafraîchissement

        void RefreshAll()
        {
            var sim = _sim;
            if (sim == null) return;
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

            foreach (var u in sim.Units)
            {
                if (!_cards.TryGetValue(u.Id, out var card)) continue;
                bool ready = manual && u.Side == Side.Ally && u.IsReady;
                bool targetable = (u.Side == Side.Enemy && targetEnemies) || (u.Side == Side.Ally && targetAllies);
                card.Refresh(u, ready, u.Id == _selected, targetable);
            }

            // Compétences de l'allié sélectionné.
            bool showSkills = manual && _selected >= 0;
            _skillPanel.gameObject.SetActive(showSkills);
            if (showSkills)
            {
                var u = sim.Unit(_selected);
                int count = CombatSimulation.ActiveSkillCount(u);
                for (int i = 0; i < 3; i++)
                {
                    var b = _skillButtons[i];
                    if (i >= count) { b.gameObject.SetActive(false); continue; }
                    b.gameObject.SetActive(true);
                    var s = u.Skills[i];
                    bool usable = sim.CanUse(u.Id, i);
                    string label = "[" + (i + 1) + "] " + Loc.SkillName(s);
                    if (u.Cooldowns[i] > 0) label += "  (" + Loc.F("combat.cooldown", u.Cooldowns[i]) + ")";
                    b.SetLabel(label);
                    _skillDesc[i].text = SkillText.Describe(s);
                    bool pending = _pendingSlot == i;
                    b.SetColors(pending ? Color.white : (i == 2 ? Theme.Danger : i == 1 ? Theme.Info : Theme.Primary),
                                i == 2 && !pending ? Color.white : Theme.TextDark);
                    _skillDesc[i].color = i == 2 && !pending ? Color.white : Theme.TextDark;
                    b.Interactable = usable;
                }
            }

            // Combo.
            var combos = manual ? sim.AvailableCombos() : Array.Empty<CombatSimulation.ComboOption>();
            bool comboVisible = combos.Count > 0;
            if (_combo.gameObject.activeSelf != comboVisible) _combo.gameObject.SetActive(comboVisible);
            if (comboVisible)
            {
                var c = combos[0];
                _combo.SetLabel(Loc.T("combat.combo") + "\n<size=24>" + Loc.ComboName(c.Combo) + "</size>");
            }

            _hint.text = targetEnemies ? Loc.T("combat.choose_target")
                       : targetAllies ? Loc.T("combat.choose_ally")
                       : manual && _selected < 0 ? Loc.T("combat.waiting") : "";
            _retreat.Interactable = !sim.IsOver;
        }

        void RefreshAuto()
        {
            bool on = _sim != null && _sim.Auto;
            _auto.SetColors(on ? Theme.Primary : Theme.Surface3, on ? Theme.TextDark : Theme.Text);
            _speed.SetLabel(Loc.F("combat.speed", _speedFactor));
        }

        // ------------------------------------------------------------------ Saisie

        void OnSkill(int slot)
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || sim.Auto || _selected < 0) return;
            if (!sim.CanUse(_selected, slot)) return;
            _pendingCombo = null;
            var rule = sim.Unit(_selected).Skills[slot].Target;
            if (rule == TargetRule.SingleEnemy && AliveCount(Side.Enemy) == 1)
            {
                Submit(CombatCommand.UseSkill(_selected, slot, FirstAlive(Side.Enemy)));
                return;
            }
            if (rule == TargetRule.SingleEnemy || rule == TargetRule.SingleAlly)
            {
                _pendingSlot = _pendingSlot == slot ? -1 : slot; // re-toucher annule
                return;
            }
            Submit(CombatCommand.UseSkill(_selected, slot, -1));
        }

        void OnCombo()
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || sim.Auto) return;
            var list = sim.AvailableCombos();
            if (list.Count == 0) return;
            var c = list[0];
            if (c.Combo.Effect.Target == TargetRule.SingleEnemy && AliveCount(Side.Enemy) > 1)
            {
                _pendingCombo = c;
                _pendingSlot = -1;
                return;
            }
            Submit(CombatCommand.UseCombo(c.A, c.B, FirstAlive(Side.Enemy)));
        }

        void OnUnitTapped(int unitId)
        {
            var sim = _sim;
            if (sim == null || sim.IsOver || sim.Auto) return;
            var u = sim.Unit(unitId);
            if (_pendingCombo.HasValue && u.Side == Side.Enemy && u.IsAlive)
            {
                var c = _pendingCombo.Value;
                Submit(CombatCommand.UseCombo(c.A, c.B, unitId));
                return;
            }
            if (_selected >= 0 && _pendingSlot >= 0)
            {
                var rule = sim.Unit(_selected).Skills[_pendingSlot].Target;
                if ((rule == TargetRule.SingleEnemy && u.Side == Side.Enemy && u.IsAlive) ||
                    (rule == TargetRule.SingleAlly && u.Side == Side.Ally && u.IsAlive))
                {
                    Submit(CombatCommand.UseSkill(_selected, _pendingSlot, unitId));
                    return;
                }
            }
            if (u.Side == Side.Ally && u.IsReady)
            {
                _selected = unitId;
                _pendingSlot = -1;
                _pendingCombo = null;
            }
        }

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

        void CancelTargeting()
        {
            _pendingSlot = -1;
            _pendingCombo = null;
        }

        void CycleSelection()
        {
            if (_readyOrder.Count < 2) return;
            int i = _readyOrder.IndexOf(_selected);
            _selected = _readyOrder[(i + 1) % _readyOrder.Count];
            _pendingSlot = -1;
        }

        void ToggleAuto()
        {
            var sim = _sim;
            if (sim == null || sim.IsOver) return;
            bool on = !sim.Auto;
            Ctx.S.Combat.SetAuto(on);
            // La commande est appliquée au prochain tick : on anticipe l'affichage.
            _auto.SetColors(on ? Theme.Primary : Theme.Surface3, on ? Theme.TextDark : Theme.Text);
            CancelTargeting();
        }

        void ToggleSpeed()
        {
            _speedFactor = _speedFactor == 1 ? 2 : _speedFactor == 2 ? 3 : 1;
            RefreshAuto();
        }

        void OnRetreat()
        {
            if (_sim == null || _sim.IsOver) return;
            Ctx.Modal.Confirm(Loc.T("combat.retreat.title"), Loc.T("combat.retreat.body"), Loc.T("ui.confirm"),
                              () => _sim?.Submit(CombatCommand.Retreat()), Loc.T("ui.cancel"), null, Theme.Danger);
        }

        void OnDebugWin() => Ctx.S.Combat.ForceWinForDebug();

        // ------------------------------------------------------------------ Fin

        void Finish()
        {
            var r = Ctx.S.Combat.Finish();
            if (r.IsFailure)
            {
                Ctx.Error(r.Error);
                Ctx.Nav.Show(Ctx.Board);
                return;
            }
            ShowResults(r.Value);
        }

        void ShowResults(CombatReport report)
        {
            var w = Ctx.Modal.Open(report.Victory ? Loc.T("combat.victory") : Loc.T("combat.defeat"), 1100, 720,
                                   report.Victory ? Theme.Primary : Theme.Danger);
            var reward = Ui.Txt(w.Content, report.Victory ? Loc.F("combat.reward", report.Credits) : "", 50, Theme.Primary,
                                TextAnchor.UpperCenter, FontKind.Title);
            reward.rectTransform.At(0.5f, 1f, 0, -6, 900, 60);
            reward.Inked(2.5f);
            var ft = Ui.Txt(w.Content, Loc.T("combat.fatigue"), 28, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body);
            ft.rectTransform.At(0.5f, 1f, 0, -76, 900, 36);

            var list = Ui.Rect(w.Content, "Fatigue").At(0.5f, 1f, 0, -118, 900, 260);
            list.VLayout(10, 0, TextAnchor.UpperCenter);
            var content = Ctx.S.Session.Content;
            foreach (var f in report.Fatigue)
            {
                var row = Ui.Rect(list, "Row");
                row.Pref(900, 52);
                var def = content.Member(f.Member);
                var n = Ui.Txt(row, Loc.MemberName(def), 28, Color.white, TextAnchor.MiddleLeft, FontKind.Title);
                n.rectTransform.At(0f, 0.5f, 0, 0, 220, 50);
                var bar = Ui.Bar(row, "Bar", Theme.FatigueColor(f.After), Theme.SurfaceDark, true, 18);
                ((RectTransform)bar.transform).At(0f, 0.5f, 230, 0, 520, 30);
                bar.Set(f.Before / 100f, true);
                bar.Set(f.After / 100f);
                bar.SetLabel(f.Before + " % → " + f.After + " %");
                var delta = Ui.Txt(row, "+" + (f.After - f.Before) + " %", 26, Theme.Fatigue, TextAnchor.MiddleRight, FontKind.Title);
                delta.rectTransform.At(1f, 0.5f, 0, 0, 130, 50);
            }

            var verify = Ui.Txt(w.Content, Loc.T("combat.verify.pending"), 22, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Regular);
            verify.rectTransform.At(0.5f, 0f, 0, 118, 900, 34);
            if (report.ReturnedToHq)
            {
                var hq = Ui.Txt(w.Content, Loc.T("combat.to_hq"), 26, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Body);
                hq.rectTransform.At(0.5f, 0f, 0, 150, 900, 34);
            }
            VerifyAsync(verify);

            var btn = Ui.Btn(w.Content, Loc.T("ui.continue"), Theme.Primary, Theme.TextDark, () =>
            {
                w.Close();
                Ctx.S.Combat.Clear();
                _sim = null;
                Ctx.Board.OnCombatReport(report);
            }, 42);
            btn.Rect.At(0.5f, 0f, 0, 0, 420, 100);
        }

        async void VerifyAsync(Text label)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            bool? ok;
            try
            {
                ok = await Ctx.S.Combat.VerifyLastAsync(_cts.Token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ok = null;
            }
            if (label == null) return;
            label.text = ok == null ? Loc.T("combat.verify.skip") : ok.Value ? Loc.T("combat.verify.ok") : Loc.T("combat.verify.ko");
            label.color = ok == false ? Theme.Danger : ok == true ? Theme.Success : Theme.TextMuted;
        }

        // ------------------------------------------------------------------ Aides

        UnitCard? Card(int id) => _cards.TryGetValue(id, out var c) ? c : null;
        string Name(int id) => _names.TryGetValue(id, out var n) ? n : "#" + id;

        string SkillLabel(int actor, SkillId id)
        {
            var u = _sim!.Unit(actor);
            foreach (var s in u.Skills) if (s.Id == id) return Loc.SkillName(s);
            foreach (var c in Ctx.S.Session.Content.Combos) if (c.Effect.Id == id) return Loc.ComboName(c);
            return id.Value;
        }

        ComboDef? FindCombo(string id)
        {
            foreach (var c in Ctx.S.Session.Content.Combos) if (c.Id == id) return c;
            return null;
        }

        bool ComboStillAvailable(CombatSimulation.ComboOption o)
        {
            foreach (var c in _sim!.AvailableCombos()) if (c.A == o.A && c.B == o.B) return true;
            return false;
        }

        int AliveCount(Side side)
        {
            int n = 0;
            foreach (var u in _sim!.Units) if (u.Side == side && u.IsAlive) n++;
            return n;
        }

        int FirstAlive(Side side)
        {
            foreach (var u in _sim!.Units) if (u.Side == side && u.IsAlive) return u.Id;
            return -1;
        }

        void AddLog(string line)
        {
            _log.Add(line);
            if (_log.Count > 8) _log.RemoveAt(0);
            _logText.text = string.Join("\n", _log);
        }

        void ShowBanner(string text, Color color)
        {
            _banner.text = text;
            _banner.color = color;
            var rt = _banner.rectTransform;
            Tween.PopIn(rt, 0.3f);
            var cg = _banner.GetComponent<CanvasGroup>();
            if (cg == null) cg = _banner.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            Tween.Delay(1.2f, () => Tween.Fade(cg, 0f, 0.4f), _banner);
        }
    }
}
