#nullable enable
using System;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// « Caisse au choix » (2D) : trois caisses scellées sont alignées au sol, un curseur les survole en va-et-vient à vitesse constante.
    /// Maintenir charge l'attaque, relâcher fracasse la caisse sous le curseur. Une seule caisse cache la récompense : elle est montrée une seconde
    /// au début de chaque manche, puis cachée. La bonne caisse marque un point, une autre coûte une vie ; on gagne à <see cref="Target"/> points.
    /// </summary>
    public sealed class CratePickGame : IMiniGame
    {
        public const int Crates = 3;
        /// <summary>Charge minimale (0..1) pour qu'un relâchement frappe.</summary>
        public const float MinCharge = 0.15f;
        public const float ChargeSeconds = 0.6f;
        public const float RevealSeconds = 1f;

        readonly IRandom _rng;
        readonly float _speed;
        float _dir = 1f;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        /// <summary>Position du curseur (0..1) sur la rangée de caisses.</summary>
        public float Cursor { get; private set; }
        public bool Charging { get; private set; }
        public float Charge { get; private set; }
        /// <summary>Caisse (0..2) qui cache la récompense de la manche.</summary>
        public int Reward { get; private set; }
        public bool Revealed => _revealLeft > 0f;
        /// <summary>Nombre de frappes déjà faites (manche en cours = Round + 1).</summary>
        public int Round { get; private set; }
        /// <summary>Dernière caisse frappée (-1 avant la première frappe).</summary>
        public int LastStruck { get; private set; } = -1;
        public int Score { get; private set; }
        public int Target { get; }
        public int Lives { get; private set; }
        public float TimeLeft => -1f;

        float _revealLeft;

        public CratePickGame(ulong seed, ActivityParameters p)
        {
            _rng = new Pcg32(seed).Fork(33);
            Target = p.Get("rounds", 3);
            Lives = p.Get("lives", 3);
            _speed = 0.9f * p.Get("speed_percent", 100) / 100f;
            StartRound();
        }

        void StartRound()
        {
            Reward = _rng.Range(0, Crates);
            _revealLeft = RevealSeconds;
        }

        /// <summary>La caisse sous une position du curseur : tiers gauche, milieu, droit.</summary>
        public static int CrateAt(float x) => x < 1f / 3f ? 0 : x < 2f / 3f ? 1 : 2;

        public void BeginCharge()
        {
            if (Status != GameStatus.Running || Charging) return;
            Charging = true;
            Charge = 0f;
        }

        public void Step(float dt)
        {
            if (Status != GameStatus.Running) return;
            Cursor += _dir * _speed * dt;
            if (Cursor >= 1f) { Cursor = 2f - Cursor; _dir = -1f; }
            else if (Cursor <= 0f) { Cursor = -Cursor; _dir = 1f; }
            Cursor = Math.Max(0f, Math.Min(1f, Cursor));
            if (_revealLeft > 0f) _revealLeft -= dt;
            if (Charging) Charge = Math.Min(1f, Charge + dt / ChargeSeconds);
        }

        /// <summary>Frappe la caisse sous le curseur ; vrai si c'est la caisse récompense. Une charge trop courte ne frappe rien.</summary>
        public bool Release()
        {
            if (Status != GameStatus.Running || !Charging) return false;
            Charging = false;
            if (Charge < MinCharge) { Charge = 0f; return false; }
            Charge = 0f;
            LastStruck = CrateAt(Cursor);
            bool hit = LastStruck == Reward;
            if (hit) Score++; else Lives--;
            Round++;
            if (Score >= Target) Status = GameStatus.Won;
            else if (Lives <= 0) Status = GameStatus.Lost;
            else StartRound();
            return hit;
        }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
