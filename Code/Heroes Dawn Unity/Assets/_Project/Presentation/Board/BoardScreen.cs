#nullable enable
using MessagePipe;
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Plateau : dé, déplacement, brouillard, objectifs ; ouvre les combats et les cases interactives.</summary>
    public sealed class BoardScreen : UiScreen
    {
        System.IDisposable? _subs;

        void OnDestroy() => _subs?.Dispose();
        BoardMapView _map = null!;
        Text _zoneTitle = null!;
        RectTransform _objectiveList = null!;
        RectTransform _squadStrip = null!;
        Text _hint = null!;
        Text _dieFace = null!;
        RectTransform _die = null!;
        Text _pm = null!;
        UiBar _reserve = null!;
        UiButton _roll = null!;
        UiButton _bonus = null!;
        UiButton _end = null!;
        bool _busy;
        ZoneRun? _run;

        protected override void Build()
        {
            Backdrop(Theme.WithAlpha(Theme.Info, 0.16f), Theme.Hex(0x0E1420));
            ArtBackdrop(Sprites.BackgroundLandscape, 1f, 0.86f);

            // ---- Barre du haut
            var top = Ui.Panel(Rect, "TopBar", Theme.SurfaceDark, 0f, 0f, true).Frame().TopBand(96);
            _zoneTitle = Ui.Txt(top, "", 46, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Zone");
            _zoneTitle.rectTransform.At(0f, 0.5f, 30, 0, 900, 70);
            _zoneTitle.Inked(2.5f);
            var wallet = WalletView.Create(top, Ctx.S.Session, Ctx.S.Wallet, Ctx.S.Saving);
            ((RectTransform)wallet.transform).At(1f, 0.5f, -290, 0, 700, 66);
            var back = Ui.Btn(top, Loc.T("board.back_hq"), Theme.Surface3, Theme.Text, OnBackToHq, 30);
            back.Rect.At(1f, 0.5f, -20, 0, 250, 66);

            // ---- Objectifs (récitatif en haut à gauche)
            var obj = Ui.Panel(Rect, "Objectives", Theme.Primary, 4f, 6f, false);
            obj.Frame().At(0f, 1f, 24, -112, 520, 156);
            var objTitle = Ui.Txt(obj.transform, Loc.T("board.objectives"), 30, Theme.TextDark, TextAnchor.UpperLeft, FontKind.Title);
            objTitle.rectTransform.At(0f, 1f, 16, -8, 480, 36);
            _objectiveList = Ui.Rect(obj.transform, "List").At(0f, 1f, 16, -46, 490, 120);
            _objectiveList.VLayout(2, 0, TextAnchor.UpperLeft);

            var legend = Ui.Txt(Rect, Loc.T("board.legend"), 22, Theme.TextMuted, TextAnchor.UpperRight, FontKind.Regular, "Legend");
            legend.rectTransform.At(1f, 1f, -30, -112, 900, 30);

            // ---- Carte
            var mapArea = Ui.Rect(Rect, "MapArea").At(0.5f, 0.5f, 0, -20, 1840, 600);
            _map = BoardMapView.Create(mapArea);
            ((RectTransform)_map.transform).Stretch();
            _map.NodeTapped += OnNodeTapped;

            // ---- HUD du bas
            var hud = Ui.Panel(Rect, "Hud", Theme.Surface, 5f, 8f, true).Frame().BottomBand(170, 0);
            hud.offsetMin = new Vector2(20, 16);
            hud.offsetMax = new Vector2(-20, 186);
            var hudFill = hud.GetChild(0);

            _squadStrip = Ui.Rect(hudFill, "Squad").At(0f, 0.5f, 20, 0, 560, 140);
            _squadStrip.HLayout(10, 0, TextAnchor.MiddleLeft);

            _hint = Ui.Txt(hudFill, "", 28, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Hint");
            _hint.rectTransform.At(0.5f, 0.5f, -120, 24, 560, 70);
            _pm = Ui.Txt(hudFill, "", 40, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Pm");
            _pm.rectTransform.At(0.5f, 0.5f, -120, -34, 300, 50);
            _pm.Inked(2f);
            _reserve = Ui.Bar(hudFill, "Reserve", Theme.Primary, Theme.SurfaceDark, true, 18);
            ((RectTransform)_reserve.transform).At(0.5f, 0f, -120, 10, 420, 26);

            var dieFill = Ui.Panel(hudFill, "Die", Color.white, 5f, 6f, false);
            _die = dieFill.Frame().At(1f, 0.5f, -640, 0, 124, 124);
            _dieFace = Ui.Txt(dieFill.transform, "-", 86, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Face");
            _dieFace.rectTransform.Stretch();

            _roll = Ui.Btn(hudFill, Loc.T("board.roll"), Theme.Primary, Theme.TextDark, OnRoll, 54);
            _roll.Rect.At(1f, 0.5f, -250, 26, 360, 90);
            _bonus = Ui.Btn(hudFill, "", Theme.Info, Theme.TextDark, OnBonus, 26);
            _bonus.Rect.At(1f, 0.5f, -20, 26, 220, 90);
            _end = Ui.Btn(hudFill, Loc.T("board.end_move"), Theme.Surface3, Theme.Text, OnEndMove, 30);
            _end.Rect.At(1f, 0.5f, -250, -46, 590, 52);
        }

        public override void OnShow()
        {
            var run = Ctx.S.Session.Run;
            if (run == null)
            {
                Ctx.Nav.Show(Ctx.Hub);
                return;
            }
            var squad = Ctx.S.Session.Squad;
            var leader = squad.Count > 0 ? squad[0].Def : Ctx.S.Session.Content.Members[0];
            if (_run != run)
            {
                _run = run;
                _busy = false;
                _dieFace.text = "-";
            }
            _map.Build(run, leader.Family, Loc.MemberName(leader));
            var zone = Ctx.S.Session.RunZone;
            _zoneTitle.text = zone != null ? Loc.T("zones.ny_1_1.code") + " · " + Loc.T(zone.NameKey) : "";
            _subs?.Dispose();
            _subs = Ctx.S.Roster.Subscribe(_ => RefreshSquad());
            RefreshAll();

            // Reprise d'une case interactive laissée ouverte.
            if (run.Phase == ZoneRunPhase.InCaseInteraction && run.PendingCase != null) OpenCase(run.PendingCase);
            else if (run.Phase == ZoneRunPhase.InCombat) Tween.Delay(0.3f, StartCombat, this);
        }

        public override void OnHide()
        {
            _subs?.Dispose();
            _subs = null;
        }

        void Update()
        {
            if (_run == null || _busy || Ctx.Modal.AnyOpen) return;
            if (_run.Phase == ZoneRunPhase.AwaitingRoll && Keys.Pressed(Key.Space)) OnRoll();
            else if (_run.Phase == ZoneRunPhase.ChoosingDestination && Keys.Pressed(Key.Enter)) OnEndMove();
            else if (Keys.Pressed(Key.B)) OnBonus();
        }

        // ------------------------------------------------------------------ Rafraîchissement

        void RefreshAll()
        {
            var run = _run;
            if (run == null) return;
            _map.Refresh(run);
            RefreshObjectives();
            RefreshSquad();
            RefreshHud();
        }

        void RefreshObjectives()
        {
            if (_run == null) return;
            _objectiveList.Clear();
            var done = new HashSet<string>(_run.CompletedObjectives);
            foreach (var o in _run.Graph.Zone.Objectives)
            {
                bool ok = done.Contains(o.Id);
                var t = Ui.Txt(_objectiveList, (ok ? "[X]  " : "[  ]  ") + Loc.T(o.NameKey), 24,
                               ok ? Theme.Hex(0x1E7A3A) : Theme.TextDark, TextAnchor.MiddleLeft, FontKind.Body);
                t.Pref(480, 34);
            }
        }

        void RefreshSquad()
        {
            _squadStrip.Clear();
            foreach (var m in Ctx.S.Session.Squad)
            {
                var cell = Ui.Rect(_squadStrip, "M");
                cell.Pref(128, 140);
                var name = Loc.MemberName(m.Def);
                var p = Portrait.Create(cell, name, m.Def.Family, 80);
                p.rectTransform.At(0.5f, 1f, 0, 0, 80, 80);
                if (m.Exhausted) p.color = Theme.Danger;
                Ui.Txt(cell, name, 20, Color.white, TextAnchor.MiddleCenter, FontKind.Body).rectTransform.At(0.5f, 1f, 0, -82, 128, 26);
                var bar = Ui.Bar(cell, "Fatigue", Theme.FatigueColor(m.Fatigue), Theme.SurfaceDark);
                ((RectTransform)bar.transform).At(0.5f, 1f, 0, -112, 112, 16);
                bar.Set(m.Fatigue / 100f, true);
            }
        }

        void RefreshHud()
        {
            var run = _run;
            if (run == null) return;
            var b = Ctx.S.Session.Content.Balance;
            bool awaiting = run.Phase == ZoneRunPhase.AwaitingRoll;
            bool choosing = run.Phase == ZoneRunPhase.ChoosingDestination;
            _roll.Interactable = awaiting && !_busy;
            _bonus.SetLabel(Loc.F("board.bonus", run.Dice.BonusDice));
            _bonus.Interactable = (awaiting || choosing) && run.Dice.BonusDice > 0 && !_busy;
            _end.Interactable = choosing && !_busy;
            _pm.text = run.PmInHand > 0 ? Loc.F("board.pm", run.PmInHand) : "";
            _reserve.Set(run.Dice.BonusDice >= b.MaxBonusDice ? 1f : run.Dice.StoredPm / (float)b.PmPerBonusDie);
            _reserve.SetLabel(Loc.F("board.reserve", run.Dice.StoredPm, b.PmPerBonusDie));
            _hint.text = run.Phase == ZoneRunPhase.Completed ? Loc.T("board.hint.done")
                       : choosing ? Loc.T("board.hint.choose")
                       : awaiting ? Loc.T("board.hint.roll") : "";
        }

        // ------------------------------------------------------------------ Actions

        void OnRoll()
        {
            if (_busy || _run == null || _run.Phase != ZoneRunPhase.AwaitingRoll) return;
            var r = Ctx.S.Zones.Roll();
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            AnimateDie(r.Value);
        }

        void OnBonus()
        {
            if (_busy || _run == null || _run.Dice.BonusDice == 0) return;
            var r = Ctx.S.Zones.UseBonusDie();
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            AnimateDie(r.Value);
        }

        void AnimateDie(IReadOnlyList<BoardEvent> events)
        {
            int value = 0;
            foreach (var e in events) if (e is DiceRolledEvent d) value = d.Value;
            _busy = true;
            RefreshHud();
            int faces = Ctx.S.Session.Content.Balance.DieFaces;
            Tween.Run(0.5f, k => { if (k < 0.95f) _dieFace.text = UnityEngine.Random.Range(1, faces + 1).ToString(); }, () =>
            {
                _dieFace.text = value.ToString();
                Tween.Punch(_die, 0.25f, 0.3f);
                _busy = false;
                RefreshAll();
            }, this, Tween.Linear);
        }

        void OnEndMove()
        {
            if (_busy || _run == null || _run.Phase != ZoneRunPhase.ChoosingDestination) return;
            var r = Ctx.S.Zones.EndMove();
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            ProcessEvents(r.Value);
        }

        void OnNodeTapped(CaseId id)
        {
            if (_busy || _run == null || Ctx.Modal.AnyOpen) return;
            if (_run.Phase != ZoneRunPhase.ChoosingDestination || !_run.Destinations.ContainsKey(id)) return;
            var r = Ctx.S.Zones.MoveTo(id);
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            ProcessEvents(r.Value);
        }

        /// <summary>Joue les événements du domaine dans l'ordre : déplacement animé, puis messages, puis case déclenchée.</summary>
        public void ProcessEvents(IReadOnlyList<BoardEvent> events)
        {
            if (_run == null) return;
            IReadOnlyList<CaseId>? path = null;
            CaseId? stepBack = null;
            foreach (var e in events)
            {
                if (e is PawnMovedEvent pm) path = pm.Path;
                if (e is SteppedBackEvent sb) stepBack = sb.To;
            }
            _busy = true;
            RefreshHud();
            Action after = () =>
            {
                if (this == null) return;
                _busy = false;
                AnnounceAndTrigger(events);
            };
            if (path != null) _map.AnimatePath(path, after);
            else if (stepBack.HasValue) _map.AnimatePath(new[] { stepBack.Value }, after);
            else after();
        }

        void AnnounceAndTrigger(IReadOnlyList<BoardEvent> events)
        {
            CaseOutcome? triggered = null;
            bool validated = false;
            foreach (var e in events)
            {
                switch (e)
                {
                    case ResourceCollectedEvent rc:
                        Ctx.Toast(Loc.F("board.resource", rc.Credits, rc.Pr), ToastKind.Reward);
                        break;
                    case FogRevealedEvent _:
                        Ctx.Toast(Loc.T("board.fog"), ToastKind.Info);
                        break;
                    case ObjectiveCompletedEvent oc:
                        Ctx.Toast(Loc.F("board.objective_done", ObjectiveName(oc.ObjectiveId)), ToastKind.Reward);
                        break;
                    case PmBankedEvent pb:
                        Ctx.Toast(Loc.F("board.banked", pb.Pm), ToastKind.Info);
                        break;
                    case SteppedBackEvent _:
                        Ctx.Toast(Loc.T("board.stepped_back"), ToastKind.Warning);
                        break;
                    case CaseTriggeredEvent ct:
                        triggered = ct.Outcome;
                        break;
                    case ZoneValidatedEvent _:
                        validated = true;
                        break;
                }
            }
            RefreshAll();
            if (validated) { Tween.Delay(0.5f, ShowZoneComplete, this); return; }
            if (triggered != null) Tween.Delay(0.35f, () => OpenCase(triggered), this);
        }

        string ObjectiveName(string id)
        {
            if (_run == null) return id;
            foreach (var o in _run.Graph.Zone.Objectives) if (o.Id == id) return Loc.T(o.NameKey);
            return id;
        }

        void OpenCase(CaseOutcome outcome)
        {
            if (_run == null || Ctx.Nav.Current != this) return;
            switch (outcome.Type)
            {
                case CaseType.Combat:
                    StartCombat();
                    break;
                case CaseType.MiniGame:
                    CasePopups.MiniGame(Ctx, outcome.Case, ProcessEvents);
                    break;
                case CaseType.Narrative:
                    CasePopups.Narrative(Ctx, outcome.Case, ProcessEvents);
                    break;
                case CaseType.Rest:
                    CasePopups.Rest(Ctx, ProcessEvents);
                    break;
            }
        }

        void StartCombat()
        {
            if (_run == null || _run.Phase != ZoneRunPhase.InCombat || Ctx.Nav.Current != this) return;
            var r = Ctx.S.Combat.Begin();
            if (r.IsFailure)
            {
                Ctx.Error(r.Error);
                return;
            }
            Ctx.Nav.Show(Ctx.Combat);
        }

        /// <summary>Retour de l'écran de combat : applique le rapport (recul, objectifs, validation, retour QG).</summary>
        public void OnCombatReport(CombatReport report)
        {
            if (report.ReturnedToHq)
            {
                Ctx.Toast(Loc.T("combat.to_hq"), ToastKind.Warning);
                Ctx.Nav.Show(Ctx.Hub);
                return;
            }
            Ctx.Nav.Show(this);
            ProcessEvents(report.BoardEvents);
        }

        void ShowZoneComplete()
        {
            var rewards = Ctx.S.Zones.LastRewards;
            var w = Ctx.Modal.Open(Loc.T("board.zone_complete"), 1000, 520, Theme.Success);
            string text = rewards == null ? ""
                        : rewards.FirstClear ? Loc.F("board.zone_rewards", rewards.Credits, rewards.Pr)
                        : Loc.F("board.zone_rewards.replay", rewards.Credits);
            var t = Ui.Txt(w.Content, text, 54, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title);
            t.rectTransform.Stretch(0, 130, 0, 20);
            t.Inked(3f);
            var btn = Ui.Btn(w.Content, Loc.T("board.to_hq"), Theme.Primary, Theme.TextDark, () =>
            {
                w.Close();
                Ctx.S.Zones.CloseCompletedRun();
                _run = null;
                Ctx.Nav.Show(Ctx.Hub);
            }, 44);
            btn.Rect.At(0.5f, 0f, 0, 0, 440, 100);
        }

        void OnBackToHq()
        {
            if (_busy) return;
            Ctx.Modal.Confirm(Loc.T("board.back_hq.title"), Loc.T("board.back_hq.body"), Loc.T("ui.confirm"), () =>
            {
                Ctx.S.Zones.Abandon();
                _run = null;
                Ctx.Nav.Show(Ctx.Hub);
            }, Loc.T("ui.cancel"), null, Theme.Danger);
        }
    }
}
