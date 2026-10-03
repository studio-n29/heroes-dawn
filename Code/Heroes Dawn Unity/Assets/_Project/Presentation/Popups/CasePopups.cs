#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Fenêtres des cases interactives (repos, événement narratif, mini-jeu).</summary>
    public static class CasePopups
    {
        public static void Rest(UiContext ctx, Action<IReadOnlyList<BoardEvent>> done)
        {
            var w = ctx.Modal.Open(Loc.T("rest.title"), 980, 500, Theme.Success);
            var body = Ui.Txt(w.Content, Loc.T("rest.body"), 34, Theme.Text, TextAnchor.MiddleCenter);
            body.rectTransform.Stretch(0, 120, 0, 10);
            var row = Ui.Rect(w.Content, "Buttons").BottomBand(96);
            row.HLayout(40);
            var gate = new OnceGate();
            Ui.Btn(row, Loc.T("rest.skip"), Theme.Surface3, Theme.Text, () => Finish(ctx, w, gate, () => ctx.S.ResolveRest.ExecuteAsync(false), done), 34).Rect.Pref(280, 90);
            Ui.Btn(row, Loc.T("rest.take"), Theme.Success, Theme.TextDark, () => Finish(ctx, w, gate, () => ctx.S.ResolveRest.ExecuteAsync(true), done), 40).Rect.Pref(380, 90);
        }

        public static void Narrative(UiContext ctx, CaseId at, Action<IReadOnlyList<BoardEvent>> done)
        {
            string key = "ev." + at.Value;
            var w = ctx.Modal.Open(Loc.Or(key + ".title", "ev.default.title"), 1180, 660, Theme.Perception);
            w.Title.color = Color.white;
            w.Title.Inked(2f);
            var body = Ui.Txt(w.Content, Loc.Or(key + ".body", "ev.default.body"), 32, Theme.Text, TextAnchor.UpperCenter, FontKind.Regular);
            body.rectTransform.Stretch(10, 330, 10, 20);

            var list = Ui.Rect(w.Content, "Choices").BottomBand(310);
            list.VLayout(14, 0, TextAnchor.LowerCenter);
            var gate = new OnceGate();
            Choice(ctx, w, gate, list, Loc.F("ev.choice.credits", ResolveNarrativeUseCase.Credits), NarrativeChoice.Credits, Theme.Credits, done);
            Choice(ctx, w, gate, list, Loc.F("ev.choice.research", ResolveNarrativeUseCase.Pr), NarrativeChoice.Research, Theme.Pr, done);
            Choice(ctx, w, gate, list, Loc.F("ev.choice.morale", ResolveNarrativeUseCase.Morale), NarrativeChoice.Morale, Theme.Success, done);
        }

        static void Choice(UiContext ctx, ModalWindow w, OnceGate gate, RectTransform list, string label, NarrativeChoice c, Color color,
                           Action<IReadOnlyList<BoardEvent>> done)
        {
            var b = Ui.Btn(list, label, color, Theme.TextDark, () => Finish(ctx, w, gate, () => ctx.S.ResolveNarrative.ExecuteAsync(c), done), 30, FontKind.Body);
            b.Rect.Pref(-1, 86);
        }

        public static void MiniGame(UiContext ctx, CaseId at, Action<IReadOnlyList<BoardEvent>> done)
        {
            string key = "mg." + at.Value;
            var w = ctx.Modal.Open(Loc.Or(key + ".title", "mg.default.title"), 1200, 700, Theme.Info);
            var body = Ui.Txt(w.Content, Loc.Or(key + ".body", "mg.default.body") + "\n<size=26>" + Loc.T("mg.rules") + "</size>",
                              30, Theme.Text, TextAnchor.UpperCenter, FontKind.Regular);
            body.rectTransform.Stretch(10, 400, 10, 10);
            var gate = new OnceGate();
            var game = w.Content.gameObject.AddComponent<CodeTimingMiniGame>();
            game.Build(w.Content, success =>
            {
                var b = ctx.S.Session.Content.Balance;
                if (success) ctx.Toast(Loc.F("mg.success", b.MiniGameCredits, b.MiniGamePr), Application.Messages.ToastKind.Reward);
                else ctx.Toast(Loc.T("mg.fail"), Application.Messages.ToastKind.Warning);
                Finish(ctx, w, gate, () => ctx.S.ResolveMiniGame.ExecuteAsync(success), done);
            });
        }

        /// <summary>Une seule résolution par fenêtre, même en cas de double clic.</summary>
        sealed class OnceGate { public bool Used; }

        static void Finish(UiContext ctx, ModalWindow w, OnceGate gate, Func<UniTask<Result<IReadOnlyList<BoardEvent>>>> run,
                           Action<IReadOnlyList<BoardEvent>> done)
        {
            if (gate.Used) return;
            gate.Used = true;
            FinishAsync().Forget();

            async UniTaskVoid FinishAsync()
            {
                var r = await run();
                w.Close();
                if (r.IsFailure)
                {
                    ctx.Error(r.Error);
                    done(Array.Empty<BoardEvent>());
                    return;
                }
                done(r.Value);
            }
        }
    }

    /// <summary>Mini-jeu de réflexe (QTE) : arrêter un curseur dans une zone verte, 2 réussites sur 3 essais.</summary>
    public sealed class CodeTimingMiniGame : MonoBehaviour
    {
        const int Attempts = 3;
        const int Needed = 2;

        RectTransform _track = null!;
        RectTransform _cursor = null!;
        RectTransform _zone = null!;
        TMP_Text _score = null!;
        UiButton _stop = null!;
        Action<bool> _finished = null!;
        float _t;
        float _speed = 0.9f;
        int _dir = 1;
        int _hits;
        int _tries;
        bool _running;
        const float TrackWidth = 900f;

        public void Build(RectTransform content, Action<bool> finished)
        {
            _finished = finished;
            var trackFill = Ui.Panel(content, "Track", Theme.SurfaceDark, 4f, 4f, false);
            _track = trackFill.Frame().At(0.5f, 0f, 0, 230, TrackWidth + 8, 70);
            _zone = Ui.Img(trackFill.transform, "Zone", Theme.Success, Sprites.RoundedSmall).rectTransform;
            _zone.anchorMin = _zone.anchorMax = new Vector2(0f, 0.5f);
            _zone.pivot = new Vector2(0.5f, 0.5f);
            _cursor = Ui.Img(trackFill.transform, "Cursor", Theme.Primary, Sprites.RoundedSmall).rectTransform;
            _cursor.anchorMin = _cursor.anchorMax = new Vector2(0f, 0.5f);
            _cursor.pivot = new Vector2(0.5f, 0.5f);
            _cursor.sizeDelta = new Vector2(16, 90);
            _cursor.GetComponent<Image>().gameObject.AddComponent<Outline>().effectColor = Theme.Ink;

            _score = Ui.Txt(content, "", 28, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Score");
            _score.rectTransform.At(0.5f, 0f, 0, 170, 900, 40);
            _stop = Ui.Btn(content, Loc.T("mg.stop"), Theme.Primary, Theme.TextDark, Stop, 52);
            _stop.Rect.At(0.5f, 0f, 0, 20, 420, 110);
            NewRound();
        }

        void NewRound()
        {
            float width = Mathf.Lerp(0.2f, 0.14f, _tries / (float)Attempts);
            float center = UnityEngine.Random.Range(0.2f, 0.8f);
            _zone.sizeDelta = new Vector2(width * TrackWidth, 56);
            _zone.anchoredPosition = new Vector2(center * TrackWidth, 0);
            _speed = 0.8f + 0.25f * _tries;
            _t = UnityEngine.Random.value;
            _running = true;
            _stop.Interactable = true;
            UpdateScore();
        }

        void UpdateScore() => _score.text = Loc.F("mg.score", _hits, Attempts - _tries);

        void Update()
        {
            if (!_running) return;
            _t += _dir * _speed * Time.unscaledDeltaTime;
            if (_t > 1f) { _t = 1f; _dir = -1; }
            if (_t < 0f) { _t = 0f; _dir = 1; }
            _cursor.anchoredPosition = new Vector2(_t * TrackWidth, 0);
            if (Common.Keys.ConfirmPressed()) Stop();
        }

        void Stop()
        {
            if (!_running) return;
            _running = false;
            _stop.Interactable = false;
            float half = _zone.sizeDelta.x * 0.5f;
            float x = _cursor.anchoredPosition.x;
            bool hit = x >= _zone.anchoredPosition.x - half && x <= _zone.anchoredPosition.x + half;
            _tries++;
            if (hit) _hits++;
            var img = _zone.GetComponent<Image>();
            img.color = hit ? Color.white : Theme.Danger;
            Tween.Punch(_track, 0.05f, 0.2f);
            UpdateScore();

            bool won = _hits >= Needed;
            bool lost = _hits + (Attempts - _tries) < Needed;
            Tween.Delay(0.6f, () =>
            {
                img.color = Theme.Success;
                if (won || lost) _finished(won);
                else NewRound();
            }, this);
        }
    }
}
