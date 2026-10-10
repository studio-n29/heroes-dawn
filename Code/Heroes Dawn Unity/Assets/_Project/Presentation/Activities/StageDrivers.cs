#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Entrées d'un jeu en 3D : déplacement (joystick / clavier), deux boutons (maintenus et appuyés).</summary>
    public struct StageInput
    {
        public Vector2 Move;
        public bool A;
        public bool B;
        public bool APressed;
        public bool BPressed;
    }

    /// <summary>Ce que la vue affiche pendant le jeu : barres, lignes de texte, boutons d'action avec recharge, bandeau.</summary>
    public struct StageHud
    {
        public string Line1;
        public string Line2;
        public bool ShowBar;
        public string BarLabel;
        public float Bar01;
        public Color BarColor;
        public string ButtonA;
        public string ButtonB;
        public bool ShowButtons;
        public float CooldownA01;
        public float CooldownB01;
        public string Banner;
        public Color BannerColor;
    }

    /// <summary>Un jeu 3D branché sur la scène : avance sa logique, anime les acteurs et décrit le HUD.</summary>
    public interface IStageDriver : IDisposable
    {
        StageHud Hud { get; }
        bool Finished { get; }
        ActivityResult Result { get; }
        void Tick(float dt, StageInput input);
        /// <summary>Abandon du joueur : la partie est perdue avec son score actuel.</summary>
        void Abandon();
    }

    static class StageUtil
    {
        public static Vec2f V(Vector2 v) => new Vec2f(v.x, v.y);
        public static Vector3 P(Vec2f p, float y = 0f) => new Vector3(p.X, y, p.Y);
        public static string WithCooldown(string name, float left) => left > 0.05f ? name + "\n" + left.ToString("0.0") + " s" : name;

        public static string Compass(Vec2f from, Vec2f to)
        {
            var d = to - from;
            float deg = Mathf.Atan2(d.X, d.Y) * Mathf.Rad2Deg; // 0 = nord (+z), 90 = est (+x)
            string[] names = { "N", "NE", "E", "SE", "S", "SO", "O", "NO" };
            return names[((Mathf.RoundToInt(deg / 45f) % 8) + 8) % 8];
        }
    }

    /// <summary>Survie en neige : le joueur (capsule) traverse le niveau, la caméra le suit, la jauge est la barre du HUD.</summary>
    public sealed class SurvivalDriver : IStageDriver
    {
        readonly ActivityStage _stage;
        readonly SnowSurvivalGame _game;
        readonly ILocalization _loc;
        readonly GameObject _player;
        bool _abandoned;

        public SurvivalDriver(ActivityStage stage, SnowSurvivalGame game, ILocalization loc)
        {
            _stage = stage; _game = game; _loc = loc;
            var pal = stage.Level.Palette;
            _player = stage.Spawn(PrimitiveShape.Capsule, ActivityStage.ToColor(pal.Accent), StageUtil.P(game.Position, 0.5f), new Vector3(0.8f, 1f, 0.8f));
            stage.Spawn(PrimitiveShape.Cylinder, ActivityStage.ToColor(pal.Target), StageUtil.P(game.Target, 4f), new Vector3(0.18f, 8f, 0.18f)); // balise de l'objet
            stage.Frame(StageUtil.P(game.Position), 17f, 9f);
        }

        public bool Finished => _game.Status != GameStatus.Running || _abandoned;
        public ActivityResult Result => _abandoned ? ActivityResult.Lost() : _game.Result();

        public StageHud Hud
        {
            get
            {
                float dist = Vec2f.Distance(_game.Position, _game.Target);
                string terrain = _game.Terrain.HasValue ? _loc.T("stage.terrain." + _game.Terrain.Value) : _loc.T("stage.terrain.ground");
                return new StageHud
                {
                    Line1 = _loc.F("stage.survival.time", Mathf.CeilToInt(_game.TimeLeft)),
                    Line2 = _game.NearTotem ? _loc.T("stage.survival.totem") : _loc.F("stage.survival.target", Mathf.RoundToInt(dist), StageUtil.Compass(_game.Position, _game.Target), terrain),
                    ShowBar = true,
                    BarLabel = _loc.T("stage.survival.gauge"),
                    Bar01 = _game.Gauge / 1000f,
                    BarColor = _game.Gauge < 250f ? new Color(0.9f, 0.3f, 0.3f) : _game.NearTotem ? new Color(1f, 0.6f, 0.2f) : new Color(0.3f, 0.8f, 1f),
                    Banner = Finished ? (Result.Success ? _loc.T("stage.won") : _game.Loss == LossReason.Cold ? _loc.T("stage.survival.cold") : _loc.T("stage.lost")) : "",
                    BannerColor = Result.Success ? new Color(0.4f, 0.9f, 0.5f) : new Color(1f, 0.4f, 0.4f),
                };
            }
        }

        public void Tick(float dt, StageInput input)
        {
            _game.Step(dt, new Vec2f(input.Move.x, input.Move.y));
            _player.transform.localPosition = StageUtil.P(_game.Position, 0.5f);
            var cam = StageUtil.P(_game.Position);
            _stage.Frame(cam, 17f, 9f);
        }

        public void Abandon() => _abandoned = true;
        public void Dispose() { }
    }

    /// <summary>Pêche : réticule de visée, flotteur, poissons sous l'eau qui disparaissent une fois pêchés.</summary>
    public sealed class FishingDriver : IStageDriver
    {
        readonly ActivityStage _stage;
        readonly FishingGame _game;
        readonly ILocalization _loc;
        readonly GameObject _reticle, _bobber, _angler;
        readonly List<GameObject> _fish;
        int _caughtSeen;
        bool _abandoned;

        public FishingDriver(ActivityStage stage, FishingGame game, ILocalization loc)
        {
            _stage = stage; _game = game; _loc = loc;
            var pal = stage.Level.Palette;
            _reticle = stage.Spawn(PrimitiveShape.Cylinder, Color.white, new Vector3(0, 0.08f, 0), new Vector3(1.4f, 0.05f, 1.4f));
            _bobber = stage.Spawn(PrimitiveShape.Sphere, new Color(0.95f, 0.25f, 0.25f), new Vector3(0, 0.15f, 0), new Vector3(0.4f, 0.4f, 0.4f));
            stage.Level.TryMarker("spawn", out var spawn);
            _angler = stage.Spawn(PrimitiveShape.Capsule, ActivityStage.ToColor(pal.Accent), new Vector3(spawn.X / 100f, 0.5f, spawn.Z / 100f), new Vector3(0.8f, 1f, 0.8f));
            _fish = stage.ByRole.TryGetValue("fish", out var list) ? list : new List<GameObject>();
            float r = stage.Level.Fact("pond_radius_cm", 1400) / 100f;
            stage.Frame(new Vector3(0f, 0f, -r * 0.25f), r * 1.35f, r * 1.0f);
        }

        public bool Finished => _game.IsOver || _abandoned;
        public ActivityResult Result => _abandoned ? new ActivityResult(false, _game.Caught) : _game.Result();

        public StageHud Hud
        {
            get
            {
                string state;
                Color banner = Color.white;
                switch (_game.State)
                {
                    case FishingState.Idle: state = _loc.T("stage.fishing.aim"); break;
                    case FishingState.Waiting: state = _loc.T("stage.fishing.wait"); break;
                    case FishingState.Bite: state = _loc.T("stage.fishing.pull"); banner = new Color(1f, 0.85f, 0.2f); break;
                    case FishingState.Caught: state = _loc.T("stage.fishing.caught"); banner = new Color(0.4f, 0.9f, 0.5f); break;
                    case FishingState.Escaped: state = _loc.T(_game.LastEvent == "early" ? "stage.fishing.early" : "stage.fishing.escaped"); banner = new Color(1f, 0.5f, 0.4f); break;
                    default: state = ""; break;
                }
                if (Finished) { state = Result.Success ? _loc.T("stage.won") : _loc.T("stage.lost"); banner = Result.Success ? new Color(0.4f, 0.9f, 0.5f) : new Color(1f, 0.4f, 0.4f); }
                return new StageHud
                {
                    Line1 = _loc.F("stage.fishing.time", Mathf.CeilToInt(_game.TimeLeft)),
                    Line2 = _loc.F("stage.fishing.count", _game.Caught, _game.SuccessFish),
                    ShowBar = _game.State == FishingState.Bite,
                    BarLabel = _loc.T("stage.fishing.window"),
                    Bar01 = _game.State == FishingState.Bite ? Mathf.Clamp01(_game.BiteTimeLeft / 1.5f) : 0f,
                    BarColor = new Color(1f, 0.85f, 0.2f),
                    ShowButtons = true,
                    ButtonA = _loc.T("stage.fishing.cast"),
                    ButtonB = _loc.T("stage.fishing.pullbutton"),
                    Banner = state,
                    BannerColor = banner,
                };
            }
        }

        public void Tick(float dt, StageInput input)
        {
            bool cast = input.APressed && _game.State == FishingState.Idle;
            bool pull = input.BPressed || (input.APressed && _game.State != FishingState.Idle);
            _game.Step(dt, new Vec2f(input.Move.x, input.Move.y), cast, pull);
            var aim = new Vector3(_game.Aim.X, 0f, _game.Aim.Y);
            _reticle.SetActive(_game.State == FishingState.Idle);
            _reticle.transform.localPosition = aim + new Vector3(0, 0.08f, 0);
            bool float_ = _game.State == FishingState.Waiting || _game.State == FishingState.Bite;
            _bobber.SetActive(_game.State != FishingState.Idle);
            float bob = _game.State == FishingState.Bite ? Mathf.Sin(Time.unscaledTime * 30f) * 0.12f : float_ ? Mathf.Sin(Time.unscaledTime * 3f) * 0.04f : 0f;
            _bobber.transform.localPosition = aim + new Vector3(0, 0.18f + bob, 0);
            while (_caughtSeen < _game.Caught)
            {
                _caughtSeen++;
                GameObject? best = null; float bestD = float.MaxValue;
                foreach (var f in _fish)
                {
                    if (!f.activeSelf) continue;
                    float d = (f.transform.localPosition - aim).sqrMagnitude;
                    if (d < bestD) { bestD = d; best = f; }
                }
                if (best != null) best.SetActive(false);
            }
        }

        public void Abandon() => _abandoned = true;
        public void Dispose() { }
    }

    /// <summary>Arène : l'agent, les ennemis (poursuite, charge annoncée, tir), les projectiles et les effets des compétences.</summary>
    public sealed class ArenaDriver : IStageDriver
    {
        sealed class Flash { public GameObject Go = null!; public float Life; public float Max; public Vector3 From; public Vector3 To; }

        readonly ActivityStage _stage;
        readonly ArenaGame _game;
        readonly ILocalization _loc;
        readonly GameObject _player, _nose;
        readonly Dictionary<int, GameObject> _enemies = new Dictionary<int, GameObject>();
        readonly List<GameObject> _bullets = new List<GameObject>();
        readonly List<Flash> _flashes = new List<Flash>();
        readonly Dictionary<int, float> _lastHp = new Dictionary<int, float>();
        readonly Dictionary<int, float> _hurt = new Dictionary<int, float>();
        readonly Color _playerColor;
        int _announcedWave = -2;
        float _bannerTime;
        bool _abandoned;

        public ArenaDriver(ActivityStage stage, ArenaGame game, ILocalization loc)
        {
            _stage = stage; _game = game; _loc = loc;
            _playerColor = ActivityStage.ToColor(stage.Level.Palette.Accent);
            _player = stage.Spawn(PrimitiveShape.Capsule, _playerColor, new Vector3(0, 0.5f, 0), new Vector3(1f, 1f, 1f));
            _nose = stage.Spawn(PrimitiveShape.Cube, Color.white, new Vector3(0, 0.6f, 0.6f), new Vector3(0.25f, 0.25f, 0.5f));
            stage.Frame(Vector3.zero, game.ArenaRadius * 1.9f, game.ArenaRadius * 0.85f);
        }

        public bool Finished => _game.Status != GameStatus.Running || _abandoned;
        public ActivityResult Result => _abandoned ? new ActivityResult(false, _game.Defeated) : _game.Result();

        public StageHud Hud
        {
            get
            {
                string banner = "";
                var color = Color.white;
                if (Finished) { banner = Result.Success ? _loc.T("stage.won") : _loc.T("stage.lost"); color = Result.Success ? new Color(0.4f, 0.9f, 0.5f) : new Color(1f, 0.4f, 0.4f); }
                else if (_bannerTime > 0f) { banner = _loc.F("stage.arena.wave_banner", _game.Wave + 1); color = new Color(1f, 0.85f, 0.2f); }
                return new StageHud
                {
                    Line1 = _loc.F("stage.arena.wave", Mathf.Max(1, _game.Wave + 1), _game.WaveCount),
                    Line2 = _loc.F("stage.arena.enemies", _game.AliveCount, _game.Defeated),
                    ShowBar = true,
                    BarLabel = _loc.F("stage.arena.hp", Mathf.CeilToInt(_game.Hp), Mathf.CeilToInt(_game.MaxHp)),
                    Bar01 = _game.Hp / Mathf.Max(1f, _game.MaxHp),
                    BarColor = _game.Hp < _game.MaxHp * 0.3f ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.4f, 0.85f, 0.4f),
                    ShowButtons = true,
                    ButtonA = StageUtil.WithCooldown(_loc.T("stage.skill." + _game.Agent.Primary.Id), _game.PrimaryCooldown),
                    ButtonB = StageUtil.WithCooldown(_loc.T("stage.skill." + _game.Agent.Special.Id), _game.SpecialCooldown),
                    CooldownA01 = _game.Agent.Primary.Cooldown <= 0f ? 0f : _game.PrimaryCooldown / _game.Agent.Primary.Cooldown,
                    CooldownB01 = _game.Agent.Special.Cooldown <= 0f ? 0f : _game.SpecialCooldown / _game.Agent.Special.Cooldown,
                    Banner = banner,
                    BannerColor = color,
                };
            }
        }

        static Vector3 BaseScale(EnemyType t) => t == EnemyType.Charger ? new Vector3(1.3f, 1.1f, 1.3f) : t == EnemyType.Shooter ? new Vector3(0.9f, 0.65f, 0.9f) : new Vector3(0.9f, 0.5f, 0.9f);

        GameObject EnemyObject(ArenaEnemy e)
        {
            if (_enemies.TryGetValue(e.Id, out var go)) return go;
            switch (e.Type)
            {
                case EnemyType.Charger: go = _stage.Spawn(PrimitiveShape.Cube, new Color(0.95f, 0.5f, 0.15f), Vector3.zero, new Vector3(1.3f, 1.1f, 1.3f)); break;
                case EnemyType.Shooter: go = _stage.Spawn(PrimitiveShape.Cylinder, new Color(0.65f, 0.35f, 0.9f), Vector3.zero, new Vector3(0.9f, 1.3f, 0.9f)); break;
                default: go = _stage.Spawn(PrimitiveShape.Capsule, new Color(0.85f, 0.25f, 0.25f), Vector3.zero, new Vector3(0.9f, 1f, 0.9f)); break;
            }
            _enemies[e.Id] = go;
            _lastHp[e.Id] = e.Hp;
            return go;
        }

        public void Tick(float dt, StageInput input)
        {
            _bannerTime = Mathf.Max(0f, _bannerTime - dt);
            _game.Step(dt, new Vec2f(input.Move.x, input.Move.y), null, input.A, input.BPressed || input.B);
            if (_game.Wave != _announcedWave) { _announcedWave = _game.Wave; if (_game.Wave >= 0) _bannerTime = 1.6f; }

            _player.transform.localPosition = StageUtil.P(_game.Position, 0.5f);
            _nose.transform.localPosition = StageUtil.P(_game.Position + _game.Facing * 0.65f, 0.6f);
            _stage.SetColor(_player, _game.Dashing ? Color.white : _playerColor);

            foreach (var e in _game.Enemies)
            {
                var go = EnemyObject(e);
                go.SetActive(e.Alive);
                if (!e.Alive) continue;
                go.transform.localPosition = StageUtil.P(e.Position, 0.55f);
                if (_lastHp.TryGetValue(e.Id, out var last) && e.Hp < last - 0.01f) _hurt[e.Id] = 0.12f;
                _lastHp[e.Id] = e.Hp;
                float h = _hurt.TryGetValue(e.Id, out var hv) ? hv : 0f;
                if (h > 0f) _hurt[e.Id] = h - dt;
                var baseColor = e.Type == EnemyType.Charger ? new Color(0.95f, 0.5f, 0.15f) : e.Type == EnemyType.Shooter ? new Color(0.65f, 0.35f, 0.9f) : new Color(0.85f, 0.25f, 0.25f);
                _stage.SetColor(go, h > 0f ? Color.white : e.Winding ? new Color(1f, 0.9f, 0.2f) : baseColor);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = BaseScale(e.Type) * (e.Winding ? 1.25f + Mathf.Sin(Time.unscaledTime * 25f) * 0.1f : 1f);
            }

            while (_bullets.Count < _game.Projectiles.Count) _bullets.Add(_stage.Spawn(PrimitiveShape.Sphere, Color.white, Vector3.zero, new Vector3(0.35f, 0.35f, 0.35f)));
            for (int i = 0; i < _bullets.Count; i++)
            {
                bool active = i < _game.Projectiles.Count;
                _bullets[i].SetActive(active);
                if (!active) continue;
                var p = _game.Projectiles[i];
                _bullets[i].transform.localPosition = StageUtil.P(p.Position, 0.7f);
                _stage.SetColor(_bullets[i], p.FromPlayer ? _playerColor : new Color(1f, 0.3f, 0.3f));
            }

            foreach (var fx in _game.DrainEffects()) SpawnFlash(fx);
            for (int i = _flashes.Count - 1; i >= 0; i--)
            {
                var f = _flashes[i];
                f.Life -= dt;
                if (f.Life <= 0f) { UnityEngine.Object.Destroy(f.Go); _flashes.RemoveAt(i); continue; }
                float k = 1f - f.Life / f.Max;
                f.Go.transform.localScale = Vector3.Lerp(f.From, f.To, k);
            }
        }

        void SpawnFlash(ArenaEffect fx)
        {
            if (fx.Kind == ArenaEffectKind.Shot) return;
            var color = fx.Kind == ArenaEffectKind.Aoe ? new Color(1f, 0.9f, 0.4f) : fx.Kind == ArenaEffectKind.Dash ? new Color(0.7f, 0.9f, 1f) : Color.white;
            var go = fx.Kind == ArenaEffectKind.Melee
                ? _stage.Spawn(PrimitiveShape.Cube, color, StageUtil.P(fx.Position + fx.Direction * (fx.Radius * 0.5f), 0.6f), new Vector3(fx.Radius * 0.8f, 0.2f, fx.Radius))
                : _stage.Spawn(PrimitiveShape.Cylinder, color, StageUtil.P(fx.Position, 0.1f), new Vector3(0.5f, 0.1f, 0.5f));
            if (fx.Kind == ArenaEffectKind.Melee) go.transform.localRotation = Quaternion.LookRotation(new Vector3(fx.Direction.X, 0f, fx.Direction.Y));
            var to = fx.Kind == ArenaEffectKind.Melee ? go.transform.localScale * 1.1f : new Vector3(fx.Radius * 2f, 0.1f, fx.Radius * 2f);
            _flashes.Add(new Flash { Go = go, Life = 0.25f, Max = 0.25f, From = go.transform.localScale, To = to });
        }

        public void Abandon() => _abandoned = true;
        public void Dispose() { foreach (var f in _flashes) if (f.Go != null) UnityEngine.Object.Destroy(f.Go); _flashes.Clear(); }
    }
}
