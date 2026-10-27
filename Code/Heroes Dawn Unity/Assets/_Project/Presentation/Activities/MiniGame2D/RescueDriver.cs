#nullable enable
using System.Linq;
using System.Text;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// « Ingénierie de secours » : remplit le panneau du prefab (<see cref="RescuePanelView"/>) avec le manuel, la réserve de pièces et les emplacements.
    /// Les boutons sont inactifs jusqu'au démarrage de la partie : le premier appui sur la fenêtre sert à commencer.
    /// </summary>
    public sealed class RescueDriver : MiniGameDriverBase, IRescuePanelUser
    {
        readonly RescueEngineeringGame _game;
        readonly ILocalization _loc;
        RescuePanelView? _panel;
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

        public void Bind(RescuePanelView panel) => _panel = panel;

        protected override void OnBuild()
        {
            var p = _panel;
            if (p == null) return;
            var sb = new StringBuilder("<color=#FFCF33>").Append(_loc.T("mg2d.rescue.manual")).Append("</color>\n");
            foreach (var c in _game.Clues) sb.Append("• ").Append(ClueText(c)).Append('\n');
            p.Manual.text = sb.ToString();
            p.Status.text = "";
            for (int i = 0; i < p.PoolCapacity; i++)
            {
                var b = p.PoolButton(i);
                b.Button.onClick.RemoveAllListeners();
                bool used = i < _game.Pool.Count;
                b.gameObject.SetActive(used);
                if (!used) continue;
                string part = _game.Pool[i];
                b.SetLabel(PartName(part));
                b.Button.onClick.AddListener(() => _game.Choose(part));
            }
            for (int i = 0; i < p.SlotCapacity; i++) p.SlotBox(i).gameObject.SetActive(i < _game.Target);
            p.Undo.SetLabel(_loc.T("mg2d.rescue.undo"));
            p.Submit.SetLabel(_loc.T("mg2d.rescue.submit"));
            p.Undo.Button.onClick.RemoveAllListeners();
            p.Submit.Button.onClick.RemoveAllListeners();
            p.Undo.Button.onClick.AddListener(() => _game.Undo());
            p.Submit.Button.onClick.AddListener(OnSubmit);
            SetInteractable(false);
            RefreshSlots();
        }

        void OnSubmit()
        {
            int attempts = _game.AttemptsLeft;
            if (_game.Submit()) { _panel!.Status.text = ""; Sparks(Vector2.zero, Color.green, 14); return; }
            if (_game.AttemptsLeft < attempts)
            {
                _panel!.Status.text = _loc.F("mg2d.rescue.wrong", _game.LastCorrect, _game.Target);
                ShakeField();
            }
        }

        void SetInteractable(bool on)
        {
            var p = _panel!;
            // Avant le début, les boutons ne captent pas le toucher : le premier appui, où qu'il tombe, lance la partie.
            for (int i = 0; i < _game.Pool.Count; i++) { p.PoolButton(i).Interactable = on; p.PoolButton(i).Fill.raycastTarget = on; }
            p.Undo.Interactable = on; p.Undo.Fill.raycastTarget = on;
            p.Submit.Interactable = on; p.Submit.Fill.raycastTarget = on;
        }

        void RefreshSlots()
        {
            var p = _panel!;
            for (int i = 0; i < _game.Target && i < p.SlotCapacity; i++)
                p.SlotText(i).text = i < _game.Chosen.Count ? PartName(_game.Chosen[i]) : (i + 1).ToString();
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            var p = _panel;
            if (p == null) return;
            if (!_armed) { _armed = true; SetInteractable(true); }
            bool running = _game.Status == GameStatus.Running;
            for (int i = 0; i < _game.Pool.Count; i++)
                p.PoolButton(i).Interactable = running && !_game.Chosen.Contains(_game.Pool[i]) && _game.Chosen.Count < _game.Target;
            p.Undo.Interactable = running && _game.Chosen.Count > 0;
            p.Submit.Interactable = running && _game.Chosen.Count == _game.Target;
            RefreshSlots();
        }
    }
}
