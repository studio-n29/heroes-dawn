#nullable enable
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// « Le rythme » : des pistes verticales, des ennemis qui descendent vers la ligne de frappe ; un toucher sur la piste au bon moment les vainc.
    /// </summary>
    public sealed class RhythmDriver : MiniGameDriverBase
    {
        const float HitY = 0.85f;
        static readonly string[] LaneNames = { "amber", "cyan", "pink" };
        static bool HasNoteArt => TacticalArt.Ext("MiniGames", "rhythm-note-amber") != null;
        static readonly Color[] LaneColors = { Theme.Info, Theme.Primary, Theme.Success, Theme.Danger };

        readonly RhythmGame _game;
        readonly ILocalization _loc;
        FieldSprite[] _lanes = new FieldSprite[0];
        FieldSprite[] _notes = new FieldSprite[0];
        FieldSprite? _hitLine;

        public RhythmDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new RhythmGame(plan.Level.Seed, plan.Parameters);
            _loc = loc;
        }

        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.rhythm.score", _game.Score, _game.Target), -1f, _game.Lives);

        protected override void OnBuild()
        {
            _lanes = new FieldSprite[_game.Lanes];
            for (int l = 0; l < _lanes.Length; l++)
                {
                var laneSprite = TacticalArt.Ext("MiniGames", "rhythm-lane-" + LaneNames[l % LaneNames.Length]);
                _lanes[l] = Img("Lane" + l, laneSprite, laneSprite != null ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 1f, 1f, l % 2 == 0 ? 0.06f : 0.12f), Vector2.one);
            }
            _hitLine = Img("HitLine", null, Theme.WithAlpha(Theme.Accent, 0.9f), Vector2.one);
            _notes = new FieldSprite[_game.Notes.Count];
            for (int i = 0; i < _notes.Length; i++)
                {
                var noteSprite = TacticalArt.Ext("MiniGames", "rhythm-note-" + LaneNames[_game.Notes[i].Lane % LaneNames.Length]);
                _notes[i] = Img("Note" + i, noteSprite != null ? noteSprite : Sprites.Circle, noteSprite != null ? Color.white : LaneColors[_game.Notes[i].Lane % LaneColors.Length], new Vector2(110, 110));
            }
            Layout();
        }

        void Layout()
        {
            float w = FieldWidth, h = FieldHeight;
            for (int l = 0; l < _lanes.Length; l++)
            {
                if (_lanes[l] == null) continue;
                _lanes[l].rectTransform.sizeDelta = new Vector2(w / _lanes.Length - 6f, h);
                _lanes[l].rectTransform.anchoredPosition = ToField((l + 0.5f) / _lanes.Length, 0.5f);
            }
            if (_hitLine != null)
            {
                _hitLine.rectTransform.sizeDelta = new Vector2(w, 10f);
                _hitLine.rectTransform.anchoredPosition = ToField(0.5f, HitY);
            }
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            int livesBefore = _game.Lives;
            _game.Step(dt);
            if (input.Down)
            {
                int lane = Mathf.Clamp(Mathf.FloorToInt(input.Pos.x * _game.Lanes), 0, _game.Lanes - 1);
                var hit = _game.Press(lane);
                var at = ToField((lane + 0.5f) / _game.Lanes, HitY);
                if (hit == RhythmHit.Perfect) Sparks(at, Theme.Accent, 9);
                else if (hit == RhythmHit.Good) Sparks(at, Theme.Info, 5);
            }
            if (_game.Lives < livesBefore) ShakeField();
            Layout();
            for (int i = 0; i < _notes.Length; i++)
            {
                var n = _game.Notes[i];
                var img = _notes[i];
                if (img == null) continue;
                bool visible = n.State == NoteState.Pending || (n.State == NoteState.Missed && _game.Song < n.Time + RhythmGame.GoodWindow + 0.25f);
                img.gameObject.SetActive(visible);
                if (!visible) continue;
                float y = (1f - (n.Time - _game.Song) / RhythmGame.Lead) * HitY;
                img.rectTransform.anchoredPosition = ToField((n.Lane + 0.5f) / _game.Lanes, y);
                img.color = n.State == NoteState.Missed ? Theme.WithAlpha(Color.gray, 0.5f) : (HasNoteArt ? Color.white : LaneColors[n.Lane % LaneColors.Length]);
            }
        }
    }
}
