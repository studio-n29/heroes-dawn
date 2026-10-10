#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    public enum ArenaSkillKind { Melee, Projectile, Aoe, Dash, Burst }

    /// <summary>Compétence d'arène : portée en mètres, dégâts en % de l'attaque de l'agent, recharge en secondes.</summary>
    public sealed class ArenaSkill
    {
        public string Id { get; }
        public ArenaSkillKind Kind { get; }
        public float Range { get; }
        public int DamagePercent { get; }
        public float Cooldown { get; }
        public ArenaSkill(string id, ArenaSkillKind kind, float range, int damagePercent, float cooldown) { Id = id; Kind = kind; Range = range; DamagePercent = damagePercent; Cooldown = cooldown; }
    }

    /// <summary>L'agent choisi pour l'arène : points de vie, attaque, vitesse et ses deux compétences (selon sa famille de pouvoirs).</summary>
    public sealed class ArenaAgent
    {
        public string MemberId { get; }
        public Family Family { get; }
        public int MaxHp { get; }
        public int Attack { get; }
        public float Speed { get; }
        public ArenaSkill Primary { get; }
        public ArenaSkill Special { get; }

        public ArenaAgent(string memberId, Family family, int maxHp, int attack, float speed, ArenaSkill primary, ArenaSkill special)
        {
            MemberId = memberId; Family = family; MaxHp = maxHp; Attack = attack; Speed = speed; Primary = primary; Special = special;
        }

        /// <summary>Statistiques de combat du membre (croissance du niveau comme en combat) et compétences de sa famille.</summary>
        public static ArenaAgent From(MemberDef def, int level, BalanceConfig b)
        {
            int Scale(int v) { long x = v; for (int i = 1; i < level; i++) x = x * (1000 + b.StatGrowthPermillePerLevel) / 1000; return (int)x; }
            ArenaSkill a, s;
            switch (def.Family)
            {
                case Family.Matter:
                    a = new ArenaSkill("matter_slam", ArenaSkillKind.Melee, 2.4f, 100, 0.55f);
                    s = new ArenaSkill("matter_shockwave", ArenaSkillKind.Aoe, 4.2f, 220, 6f); break;
                case Family.Elemental:
                    a = new ArenaSkill("elemental_bolt", ArenaSkillKind.Projectile, 14f, 85, 0.45f);
                    s = new ArenaSkill("elemental_nova", ArenaSkillKind.Aoe, 5.2f, 250, 7f); break;
                case Family.Perception:
                    a = new ArenaSkill("perception_shot", ArenaSkillKind.Projectile, 16f, 95, 0.6f);
                    s = new ArenaSkill("perception_volley", ArenaSkillKind.Burst, 14f, 70, 6f); break;
                default:
                    a = new ArenaSkill("physical_strike", ArenaSkillKind.Melee, 1.9f, 75, 0.32f);
                    s = new ArenaSkill("physical_charge", ArenaSkillKind.Dash, 6f, 200, 5f); break;
            }
            return new ArenaAgent(def.Id.Value, def.Family, Math.Max(40, Scale(def.BaseStats.Hp) / 8), Scale(def.BaseStats.Atk), 4.6f + def.BaseStats.Spd / 200f, a, s);
        }
    }

    public enum ArenaEffectKind { Melee, Aoe, Dash, Shot }

    /// <summary>Effet visuel à jouer par la présentation (coup, onde, charge, tir) ; la logique ne dessine rien.</summary>
    public readonly struct ArenaEffect
    {
        public ArenaEffectKind Kind { get; }
        public Vec2f Position { get; }
        public Vec2f Direction { get; }
        public float Radius { get; }
        public ArenaEffect(ArenaEffectKind kind, Vec2f position, Vec2f direction, float radius) { Kind = kind; Position = position; Direction = direction; Radius = radius; }
    }

    public enum EnemyType { Chaser, Charger, Shooter }

    public sealed class ArenaEnemy
    {
        public int Id { get; internal set; }
        public EnemyType Type { get; internal set; }
        public Vec2f Position { get; internal set; }
        public float Hp { get; internal set; }
        public float MaxHp { get; internal set; }
        public bool Alive => Hp > 0f;
        /// <summary>Le Charger prépare sa charge (se fige et l'annonce) ; vrai pendant le préavis.</summary>
        public bool Winding { get; internal set; }
        internal float Timer;
        internal float AttackTimer;
        internal int Phase;
        internal Vec2f ChargeDir;
        internal bool Hit;
    }

    public sealed class ArenaProjectile
    {
        public Vec2f Position { get; internal set; }
        public Vec2f Velocity { get; internal set; }
        public bool FromPlayer { get; internal set; }
        public float Damage { get; internal set; }
        internal float Life;
    }

    /// <summary>
    /// Arène de combat (hack'n'slash, caméra du dessus) : l'agent se déplace à 360°, deux compétences, des vagues d'ennemis aux comportements
    /// simples (poursuite, charge annoncée, tir à distance). Logique pure : un pas de temps, un déplacement, une visée facultative, deux boutons.
    /// </summary>
    public sealed class ArenaGame
    {
        public const float PlayerRadius = 0.5f, EnemyRadius = 0.5f;
        const float IntermissionSeconds = 1.5f, InvulnerableAfterHit = 0.45f;
        static readonly (float C, float S)[] Spread = { (0.94f, -0.342f), (0.985f, -0.174f), (1f, 0f), (0.985f, 0.174f), (0.94f, 0.342f) };

        readonly List<ArenaEnemy> _enemies = new List<ArenaEnemy>();
        readonly List<ArenaProjectile> _projectiles = new List<ArenaProjectile>();
        readonly List<ArenaEffect> _effects = new List<ArenaEffect>();
        readonly ActivityLevel _level;
        readonly float _radius, _hpMul, _dmgMul;
        readonly ArenaAgent _agent;
        float _intermission, _dashLeft, _invulnerable, _hurtCd;
        Vec2f _dashDir;
        int _nextId;

        public ArenaAgent Agent => _agent;
        public Vec2f Position { get; private set; }
        public Vec2f Facing { get; private set; } = new Vec2f(0, 1);
        public float Hp { get; private set; }
        public float MaxHp => _agent.MaxHp;
        public float PrimaryCooldown { get; private set; }
        public float SpecialCooldown { get; private set; }
        public int Wave { get; private set; } = -1;
        public int WaveCount { get; }
        public int Defeated { get; private set; }
        public GameStatus Status { get; private set; } = GameStatus.Running;
        public float Elapsed { get; private set; }
        public float ArenaRadius => _radius;
        public IReadOnlyList<ArenaEnemy> Enemies => _enemies;
        public IReadOnlyList<ArenaProjectile> Projectiles => _projectiles;
        public bool Dashing => _dashLeft > 0f;

        /// <summary>Effets visuels produits depuis le dernier appel (la présentation les consomme).</summary>
        public IReadOnlyList<ArenaEffect> DrainEffects()
        {
            var copy = new List<ArenaEffect>(_effects);
            _effects.Clear();
            return copy;
        }
        public int AliveCount { get { int n = 0; foreach (var e in _enemies) if (e.Alive) n++; return n; } }

        public ArenaGame(ActivityLevel level, ActivityParameters p, ArenaAgent agent)
        {
            _level = level; _agent = agent;
            _radius = p.Get("arena_radius_m", 12);
            WaveCount = p.Get("waves", 3);
            _hpMul = p.Get("enemy_hp_percent", 100) / 100f;
            _dmgMul = p.Get("enemy_damage_percent", 100) / 100f;
            Hp = agent.MaxHp;
            Position = Vec2f.Zero;
            _intermission = 0.4f;
        }

        float PlayerDamage(ArenaSkill s) => _agent.Attack * s.DamagePercent / 100f / 3.5f;

        ArenaEnemy? Nearest(Vec2f from, float within)
        {
            ArenaEnemy? best = null; float bestD = within;
            foreach (var e in _enemies)
            {
                if (!e.Alive) continue;
                float d = Vec2f.Distance(from, e.Position);
                if (d <= bestD) { bestD = d; best = e; }
            }
            return best;
        }

        public void Step(float dt, Vec2f move, Vec2f? aim, bool primary, bool special)
        {
            if (Status != GameStatus.Running || dt <= 0f) return;
            Elapsed += dt;
            PrimaryCooldown = Math.Max(0f, PrimaryCooldown - dt);
            SpecialCooldown = Math.Max(0f, SpecialCooldown - dt);
            _invulnerable = Math.Max(0f, _invulnerable - dt);
            _hurtCd = Math.Max(0f, _hurtCd - dt);

            // Visée : celle donnée, sinon l'ennemi le plus proche, sinon la direction du déplacement.
            Vec2f facing = Facing;
            if (aim.HasValue && aim.Value.Length > 0.1f) facing = aim.Value.Normalized;
            else
            {
                var near = Nearest(Position, 30f);
                if (near != null) facing = (near.Position - Position).Normalized;
                else if (move.Length > 0.1f) facing = move.Normalized;
            }
            if (facing.Length > 0.1f) Facing = facing;

            PlayerMove(dt, move);
            if (primary && PrimaryCooldown <= 0f) UseSkill(_agent.Primary, true);
            if (special && SpecialCooldown <= 0f) UseSkill(_agent.Special, false);

            UpdateEnemies(dt);
            UpdateProjectiles(dt);
            Separate();
            UpdateWaves(dt);
            if (Hp <= 0f) { Hp = 0f; Status = GameStatus.Lost; }
        }

        void PlayerMove(float dt, Vec2f move)
        {
            Vec2f next;
            if (_dashLeft > 0f)
            {
                _dashLeft -= dt;
                next = Position + _dashDir * (24f * dt);
                foreach (var e in _enemies)
                    if (e.Alive && !e.Hit && Vec2f.Distance(next, e.Position) < PlayerRadius + EnemyRadius + 0.3f) { Damage(e, PlayerDamage(_agent.Special)); e.Hit = true; }
            }
            else next = Position + move.Clamped(1f) * (_agent.Speed * dt);
            float len = next.Length, max = _radius - PlayerRadius - 0.1f;
            Position = len > max ? next * (max / len) : next;
        }

        void UseSkill(ArenaSkill s, bool isPrimary)
        {
            if (isPrimary) PrimaryCooldown = s.Cooldown; else SpecialCooldown = s.Cooldown;
            float dmg = PlayerDamage(s);
            switch (s.Kind)
            {
                case ArenaSkillKind.Melee:
                    _effects.Add(new ArenaEffect(ArenaEffectKind.Melee, Position, Facing, s.Range));
                    foreach (var e in _enemies)
                    {
                        if (!e.Alive) continue;
                        var d = e.Position - Position;
                        if (d.Length <= s.Range + EnemyRadius && (d.Length < 0.6f || d.Normalized.Dot(Facing) >= 0.45f)) Damage(e, dmg);
                    }
                    break;
                case ArenaSkillKind.Aoe:
                    _effects.Add(new ArenaEffect(ArenaEffectKind.Aoe, Position, Facing, s.Range));
                    foreach (var e in _enemies)
                    {
                        if (!e.Alive || Vec2f.Distance(e.Position, Position) > s.Range + EnemyRadius) continue;
                        Damage(e, dmg);
                        var away = (e.Position - Position).Normalized;
                        e.Position = e.Position + away * 2f;
                    }
                    break;
                case ArenaSkillKind.Projectile:
                    Shoot(Facing, dmg, 14f, s.Range / 14f);
                    break;
                case ArenaSkillKind.Burst:
                    foreach (var (c, sn) in Spread) Shoot(new Vec2f(Facing.X * c - Facing.Y * sn, Facing.X * sn + Facing.Y * c), dmg, 14f, s.Range / 14f);
                    break;
                case ArenaSkillKind.Dash:
                    _effects.Add(new ArenaEffect(ArenaEffectKind.Dash, Position, Facing, s.Range));
                    _dashDir = Facing; _dashLeft = s.Range / 24f; _invulnerable = _dashLeft + 0.15f;
                    foreach (var e in _enemies) e.Hit = false;
                    break;
            }
        }

        void Shoot(Vec2f dir, float dmg, float speed, float life)
        {
            _projectiles.Add(new ArenaProjectile { Position = Position + dir * 0.7f, Velocity = dir.Normalized * speed, FromPlayer = true, Damage = dmg, Life = life });
            _effects.Add(new ArenaEffect(ArenaEffectKind.Shot, Position, dir, 0.5f));
        }

        void Damage(ArenaEnemy e, float amount)
        {
            if (!e.Alive) return;
            e.Hp -= amount;
            if (e.Hp <= 0f) { e.Hp = 0f; Defeated++; }
        }

        void Hurt(float amount)
        {
            if (_invulnerable > 0f) return;
            Hp -= amount;
            _invulnerable = InvulnerableAfterHit;
        }

        void UpdateEnemies(float dt)
        {
            foreach (var e in _enemies)
            {
                if (!e.Alive) continue;
                var toPlayer = Position - e.Position;
                float dist = toPlayer.Length;
                var dir = toPlayer.Normalized;
                e.AttackTimer = Math.Max(0f, e.AttackTimer - dt);
                switch (e.Type)
                {
                    case EnemyType.Chaser:
                        if (dist > PlayerRadius + EnemyRadius) e.Position = e.Position + dir * (3.2f * dt);
                        else if (e.AttackTimer <= 0f) { Hurt(8f * _dmgMul); e.AttackTimer = 0.8f; }
                        break;
                    case EnemyType.Charger:
                        if (e.Phase == 0)
                        {
                            e.Position = e.Position + dir * (2.2f * dt);
                            if (dist < 7f) { e.Phase = 1; e.Timer = 0.8f; e.Winding = true; e.ChargeDir = dir; }
                        }
                        else if (e.Phase == 1)
                        {
                            e.Timer -= dt;
                            if (e.Timer <= 0f) { e.Phase = 2; e.Timer = 0.6f; e.Winding = false; e.Hit = false; }
                        }
                        else if (e.Phase == 2)
                        {
                            e.Timer -= dt;
                            e.Position = e.Position + e.ChargeDir * (11f * dt);
                            if (!e.Hit && Vec2f.Distance(e.Position, Position) < PlayerRadius + EnemyRadius + 0.2f) { Hurt(16f * _dmgMul); e.Hit = true; }
                            if (e.Timer <= 0f) { e.Phase = 3; e.Timer = 1f; }
                        }
                        else { e.Timer -= dt; if (e.Timer <= 0f) e.Phase = 0; }
                        break;
                    case EnemyType.Shooter:
                        if (dist > 8f) e.Position = e.Position + dir * (2.5f * dt);
                        else if (dist < 5f) e.Position = e.Position - dir * (2.5f * dt);
                        if (e.AttackTimer <= 0f)
                        {
                            _projectiles.Add(new ArenaProjectile { Position = e.Position + dir * 0.6f, Velocity = dir * 8f, FromPlayer = false, Damage = 9f * _dmgMul, Life = 2.5f });
                            e.AttackTimer = 2.2f;
                        }
                        break;
                }
                float len = e.Position.Length, max = _radius - EnemyRadius;
                if (len > max) e.Position = e.Position * (max / len);
            }
        }

        void UpdateProjectiles(float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var p = _projectiles[i];
                p.Position = p.Position + p.Velocity * dt;
                p.Life -= dt;
                bool dead = p.Life <= 0f || p.Position.Length > _radius + 1f;
                if (!dead && p.FromPlayer)
                {
                    foreach (var e in _enemies)
                        if (e.Alive && Vec2f.Distance(p.Position, e.Position) <= EnemyRadius + 0.3f) { Damage(e, p.Damage); dead = true; break; }
                }
                else if (!dead && Vec2f.Distance(p.Position, Position) <= PlayerRadius + 0.25f) { Hurt(p.Damage); dead = true; }
                if (dead) _projectiles.RemoveAt(i);
            }
        }

        void Separate()
        {
            for (int i = 0; i < _enemies.Count; i++)
                for (int j = i + 1; j < _enemies.Count; j++)
                {
                    var a = _enemies[i]; var b = _enemies[j];
                    if (!a.Alive || !b.Alive) continue;
                    var d = b.Position - a.Position;
                    float len = d.Length, min = EnemyRadius * 2f;
                    if (len < min && len > 1e-4f)
                    {
                        var push = d * ((min - len) * 0.5f / len);
                        a.Position = a.Position - push; b.Position = b.Position + push;
                    }
                }
        }

        void UpdateWaves(float dt)
        {
            if (AliveCount > 0) return;
            if (Wave + 1 >= WaveCount && Wave >= 0) { Status = GameStatus.Won; return; }
            _intermission -= dt;
            if (_intermission > 0f) return;
            Wave++;
            _intermission = IntermissionSeconds;
            _enemies.RemoveAll(e => !e.Alive);
            int count = _level.Fact("enemies_per_wave", 4);
            int pattern = _level.Fact("wave" + Wave + "_pattern", 0);
            for (int i = 0; i < count; i++)
            {
                var type = pattern == 1 && i % 2 == 0 ? EnemyType.Charger : pattern == 2 && i % 2 == 0 ? EnemyType.Shooter : EnemyType.Chaser;
                float hp = (type == EnemyType.Charger ? 85f : type == EnemyType.Shooter ? 40f : 55f) * _hpMul;
                _level.TryMarker("spawn_w" + Wave + "_" + i, out var m);
                _enemies.Add(new ArenaEnemy { Id = _nextId++, Type = type, Position = new Vec2f(m.X / 100f, m.Z / 100f), Hp = hp, MaxHp = hp, AttackTimer = 1f });
            }
        }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Defeated);
    }
}
