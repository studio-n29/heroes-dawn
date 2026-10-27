#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Survie en neige, vue de dessus : la caméra suit le joueur ; neige, herbe haute et sable sont des plaques colorées, les totems des foyers dont le cercle
    /// montre la zone de recharge. La jauge de survie est une barre en bas ; une flèche indique l'objet quand il est hors de vue. Joystick flottant.
    /// </summary>
    public sealed class SnowSurvivalDriver : MiniGameDriverBase
    {
        const float ViewMeters = 22f;

        sealed class Item { public Image Image = null!; public Vec2f Center; public Vec2f Size; }

        readonly SnowSurvivalMiniGame _game;
        readonly ActivityLevel _level;
        readonly ILocalization _loc;
        readonly List<Item> _items = new List<Item>();
        Image? _player, _arrow, _barBack, _barFill, _targetRing;
        Vec2f _target;
        float _clock, _scale;

        public SnowSurvivalDriver(ActivityPlan plan, ILocalization loc)
        {
            _level = plan.Level;
            _game = new SnowSurvivalMiniGame(plan.Level, plan.Parameters);
            _loc = loc;
            _target = _game.Inner.Target;
        }

        public override bool UsesStick => true;
        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.survival.score", Mathf.RoundToInt(_game.Inner.Gauge / 10f)), _game.TimeLeft, -1);

        static Color ToColor(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);

        protected override void OnBuild()
        {
            // Dans l'ordre de dessin : sol, plaques de terrain, cercles de totem, foyers, arbres et obstacles, objet.
            foreach (var role in new[] { "ground", "terrain_", "totem_ring", "totem", "tree", "obstacle" })
                foreach (var pr in _level.Primitives)
                {
                    bool match = role == "terrain_" ? pr.Role.StartsWith("terrain_") : role == "totem_ring" ? pr.Role == "totem" : pr.Role == role;
                    if (!match) continue;
                    var center = new Vec2f(pr.X / 100f, pr.Z / 100f);
                    if (role == "ground") AddItem(null, ToColor(pr.Color), center, new Vec2f(pr.SizeX / 100f, pr.SizeZ / 100f));
                    else if (role == "terrain_") AddItem(null, new Color(ToColor(pr.Color).r, ToColor(pr.Color).g, ToColor(pr.Color).b, 0.8f), center, new Vec2f(pr.SizeX / 100f, pr.SizeZ / 100f));
                    else if (role == "totem_ring") AddItem(Sprites.Ring, new Color(1f, 0.6f, 0.2f, 0.55f), center, new Vec2f(_game.Inner.TotemRadius * 2f, _game.Inner.TotemRadius * 2f));
                    else if (role == "totem") AddItem(Sprites.Circle, new Color(1f, 0.55f, 0.15f), center, new Vec2f(1.2f, 1.2f));
                    else if (role == "tree") AddItem(Sprites.Circle, ToColor(pr.Color), center, new Vec2f(pr.SizeX / 100f * 1.6f, pr.SizeX / 100f * 1.6f));
                    else AddItem(Sprites.Rounded, ToColor(pr.Color), center, new Vec2f(pr.SizeX / 100f, pr.SizeZ / 100f));
                }
            _targetRing = Img("Target", Sprites.Ring, Theme.Accent, Vector2.one);
            var core = AddItem(Sprites.Diamond, Theme.Accent, _target, new Vec2f(1.6f, 1.6f));
            _player = Img("Player", Sprites.Circle, Color.white, Vector2.one);
            _arrow = Img("Arrow", Sprites.Diamond, Theme.Accent, new Vector2(54, 54));
            _barBack = Img("BarBack", null, new Color(0f, 0f, 0f, 0.55f), new Vector2(400, 22));
            _barFill = Img("BarFill", null, Theme.Info, new Vector2(400, 22));
            _ = core;
            Tick(0f, default);   // mise en page avant le début
        }

        Item AddItem(Sprite? sprite, Color color, Vec2f center, Vec2f size)
        {
            var it = new Item { Image = Img("Item" + _items.Count, sprite, color, Vector2.one), Center = center, Size = size };
            _items.Add(it);
            return it;
        }

        Vector2 Rel(Vec2f world) => new Vector2((world.X - _game.Inner.Position.X) * _scale, -(world.Y - _game.Inner.Position.Y) * _scale);

        public override void Tick(float dt, MiniGameInput input)
        {
            _clock += dt;
            _scale = FieldHeight / ViewMeters;
            _game.SetMove(new Vec2f(input.Stick.x, input.Stick.y));
            _game.Step(dt);
            foreach (var it in _items)
            {
                it.Image.rectTransform.anchoredPosition = Rel(it.Center);
                it.Image.rectTransform.sizeDelta = new Vector2(it.Size.X * _scale, it.Size.Y * _scale);
            }
            float pulse = 1f + 0.12f * Mathf.Sin(_clock * 5f);
            _targetRing!.rectTransform.anchoredPosition = Rel(_target);
            _targetRing.rectTransform.sizeDelta = Vector2.one * (SnowSurvivalGame.FindRadius * 2f * _scale * pulse);
            _player!.rectTransform.anchoredPosition = Vector2.zero;
            _player.rectTransform.sizeDelta = Vector2.one * (SnowSurvivalGame.PlayerRadius * 2f * _scale * 1.4f);
            _player.color = _game.Inner.NearTotem ? new Color(1f, 0.85f, 0.5f) : Color.white;

            // Flèche vers l'objet quand il sort de la vue.
            var rel = Rel(_target);
            float hx = FieldWidth * 0.5f - 50f, hy = FieldHeight * 0.5f - 50f;
            bool offscreen = Mathf.Abs(rel.x) > hx || Mathf.Abs(rel.y) > hy;
            _arrow!.gameObject.SetActive(offscreen);
            if (offscreen)
            {
                float k = Mathf.Min(hx / Mathf.Max(1f, Mathf.Abs(rel.x)), hy / Mathf.Max(1f, Mathf.Abs(rel.y)));
                _arrow.rectTransform.anchoredPosition = rel * k;
                _arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(rel.y, rel.x) * Mathf.Rad2Deg + 90f);
            }

            // Jauge de survie.
            float g = Mathf.Clamp01(_game.Inner.Gauge / 1000f);
            var barPos = new Vector2(0f, -FieldHeight * 0.5f + 30f);
            _barBack!.rectTransform.anchoredPosition = barPos;
            _barFill!.rectTransform.sizeDelta = new Vector2(400f * g, 22f);
            _barFill.rectTransform.anchoredPosition = barPos + new Vector2(-200f + 200f * g, 0f);
            _barFill.color = _game.Inner.Gauge < 250f ? new Color(0.9f, 0.3f, 0.3f) : _game.Inner.NearTotem ? new Color(1f, 0.6f, 0.2f) : Theme.Info;
        }
    }
}
