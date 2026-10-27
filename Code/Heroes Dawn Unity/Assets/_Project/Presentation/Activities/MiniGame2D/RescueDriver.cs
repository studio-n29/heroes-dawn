#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// « Ingénierie de secours » : à gauche le manuel d'indices et les emplacements de l'outil, à droite la réserve de pièces (boutons) puis Annuler / Valider.
    /// Les boutons sont inactifs jusqu'au démarrage de la partie : le premier appui sur la fenêtre sert à commencer.
    /// </summary>
    public sealed class RescueDriver : MiniGameDriverBase
    {
        readonly RescueEngineeringGame _game;
        readonly ILocalization _loc;
        readonly List<UiButton> _poolButtons = new List<UiButton>();
        readonly List<TextMeshProUGUI> _slotTexts = new List<TextMeshProUGUI>();
        UiButton? _undo, _submit;
        TextMeshProUGUI? _status;
        bool _armed;

        public RescueDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new RescueEngineeringGame(plan.Level.Seed, plan.Parameters);
            _loc = loc;
        }

        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.rescue.score", _game.Score, _game.Target), -1f, _game.AttemptsLeft);

        string PartName(string id) => _loc.T("mg2d.rescue.part." + id);

        string ClueText(RescueEngineeringGame.Clue c) => c.Kind switch
        {
            "first" => _loc.F("mg2d.rescue.clue.first", PartName(c.A)),
            "last" => _loc.F("mg2d.rescue.clue.last", PartName(c.A)),
            "after" => _loc.F("mg2d.rescue.clue.after", PartName(c.A), PartName(c.B)),
            _ => _loc.F("mg2d.rescue.clue.unused", PartName(c.A)),
        };

        protected override void OnBuild()
        {
            float w = FieldWidth, h = FieldHeight;
            float left = -w * 0.5f + 24f, leftW = w * 0.40f;
            float rightX = -w * 0.5f + w * 0.43f, rightW = w * 0.55f;

            // Manuel
            var panel = Img("ManualPanel", Sprites.Panel, new Color(0f, 0f, 0f, 0.45f), new Vector2(leftW, h * 0.66f));
            panel.rectTransform.anchoredPosition = new Vector2(left + leftW * 0.5f, h * 0.5f - 24f - h * 0.33f);
            var sb = new StringBuilder("<color=#FFCF33>").Append(_loc.T("mg2d.rescue.manual")).Append("</color>\n");
            foreach (var c in _game.Clues) sb.Append("• ").Append(ClueText(c)).Append('\n');
            var manual = Own(Ui.Txt(Field, sb.ToString(), 28, Color.white, TextAnchor.UpperLeft, FontKind.Body, "Manual"));
            manual.rectTransform.anchorMin = manual.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            manual.rectTransform.sizeDelta = new Vector2(leftW - 36f, h * 0.66f - 24f);
            manual.rectTransform.anchoredPosition = panel.rectTransform.anchoredPosition;

            // Emplacements de l'outil
            int parts = _game.Target;
            float slotW = Mathf.Min(120f, (leftW - 12f * (parts - 1)) / parts);
            for (int i = 0; i < parts; i++)
            {
                var box = Img("Slot" + i, Sprites.Rounded, new Color(1f, 1f, 1f, 0.16f), new Vector2(slotW, 84f));
                var pos = new Vector2(left + slotW * 0.5f + i * (slotW + 12f), -h * 0.5f + 150f);
                box.rectTransform.anchoredPosition = pos;
                var t = Own(Ui.Txt(Field, "", 22, Color.white, TextAnchor.MiddleCenter, FontKind.Body, "SlotText" + i));
                t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                t.rectTransform.sizeDelta = new Vector2(slotW - 6f, 80f);
                t.rectTransform.anchoredPosition = pos;
                _slotTexts.Add(t);
            }
            _status = Own(Ui.Txt(Field, "", 28, Theme.Accent, TextAnchor.MiddleLeft, FontKind.Body, "Status"));
            _status.rectTransform.anchorMin = _status.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _status.rectTransform.sizeDelta = new Vector2(leftW, 60f);
            _status.rectTransform.anchoredPosition = new Vector2(left + leftW * 0.5f, -h * 0.5f + 60f);

            // Réserve : 4 colonnes de boutons
            int cols = 4;
            float bw = (rightW - 10f * (cols - 1)) / cols, bh = 96f;
            for (int i = 0; i < _game.Pool.Count; i++)
            {
                string part = _game.Pool[i];
                var b = Own(Ui.Btn(Field, PartName(part), Theme.Surface2, Theme.Text, () => _game.Choose(part), 26, FontKind.Body, "Part" + part));
                b.Rect.anchorMin = b.Rect.anchorMax = b.Rect.pivot = new Vector2(0.5f, 0.5f);
                b.Rect.sizeDelta = new Vector2(bw, bh);
                b.Rect.anchoredPosition = new Vector2(rightX + bw * 0.5f + (i % cols) * (bw + 10f), h * 0.5f - 24f - bh * 0.5f - (i / cols) * (bh + 8f));
                _poolButtons.Add(b);
            }
            _undo = Own(Ui.Btn(Field, _loc.T("mg2d.rescue.undo"), Theme.Surface3, Theme.Text, () => _game.Undo(), 28, FontKind.Title, "Undo"));
            _submit = Own(Ui.Btn(Field, _loc.T("mg2d.rescue.submit"), Theme.Primary, Theme.TextDark, OnSubmit, 28, FontKind.Title, "Submit"));
            foreach (var (b, x) in new[] { (_undo, rightX + rightW * 0.25f), (_submit, rightX + rightW * 0.75f) })
            {
                b.Rect.anchorMin = b.Rect.anchorMax = b.Rect.pivot = new Vector2(0.5f, 0.5f);
                b.Rect.sizeDelta = new Vector2(rightW * 0.46f, 96f);
                b.Rect.anchoredPosition = new Vector2(x, -h * 0.5f + 70f);
            }
            SetInteractable(false);
        }

        void OnSubmit()
        {
            int attempts = _game.AttemptsLeft;
            if (_game.Submit()) { _status!.text = ""; Sparks(Vector2.zero, Theme.Success, 14); return; }
            if (_game.AttemptsLeft < attempts)
            {
                _status!.text = _loc.F("mg2d.rescue.wrong", _game.LastCorrect, _game.Target);
                ShakeField();
            }
        }

        void SetInteractable(bool on)
        {
            foreach (var b in _poolButtons) b.Interactable = on;
            _undo!.Interactable = on;
            _submit!.Interactable = on;
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            if (!_armed) { _armed = true; SetInteractable(true); }
            for (int i = 0; i < _poolButtons.Count; i++)
                _poolButtons[i].Interactable = !_game.Chosen.Contains(_game.Pool[i]) && _game.Chosen.Count < _game.Target && _game.Status == GameStatus.Running;
            _undo!.Interactable = _game.Chosen.Count > 0 && _game.Status == GameStatus.Running;
            _submit!.Interactable = _game.Chosen.Count == _game.Target && _game.Status == GameStatus.Running;
            for (int i = 0; i < _slotTexts.Count; i++)
                _slotTexts[i].text = i < _game.Chosen.Count ? PartName(_game.Chosen[i]) : (i + 1).ToString();
        }
    }
}
