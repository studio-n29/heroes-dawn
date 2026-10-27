#nullable enable
using System;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// « Tir franc » (2D) : on maintient pour faire aller et venir une jauge de puissance, on relâche pour lancer le projectile vers la cible.
    /// La cible se trouve à une puissance donnée (centre et demi-largeur dans [0, 1]) ; elle change à chaque lancer.
    /// On gagne en touchant <see cref="Target"/> fois avant d'épuiser les lancers. Pas de minuteur.
    /// </summary>
    public sealed class FreeThrowGame : IMiniGame
    {
        readonly IRandom _rng;
        readonly float _speed;
        float _dir = 1f;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public bool Charging { get; private set; }
        /// <summary>Puissance courante (0..1) ; repart de zéro à chaque charge.</summary>
        public float Gauge { get; private set; }
        public float TargetCenter { get; private set; }
        public float TargetHalfWidth { get; private set; }
        public int ThrowsLeft { get; private set; }
        public int Score { get; private set; }
        public int Target { get; }
        public float LastPower { get; private set; }
        public bool LastHit { get; private set; }
        public float TimeLeft => -1f;
        /// <summary>Les essais restants tiennent lieu de vies.</summary>
        public int Lives => ThrowsLeft;

        public FreeThrowGame(ulong seed, ActivityParameters p)
        {
            _rng = new Pcg32(seed).Fork(32);
            ThrowsLeft = p.Get("throws", 5);
            Target = Math.Min(p.Get("hits", 3), ThrowsLeft);
            _speed = 1.2f * p.Get("speed_percent", 100) / 100f;
            NextTarget();
        }

        void NextTarget()
        {
            TargetHalfWidth = 0.05f + 0.04f * _rng.Range(0, 101) / 100f;
            float min = 0.3f, max = 0.9f;
            TargetCenter = min + (max - min) * _rng.Range(0, 1001) / 1000f;
        }

        public void BeginCharge()
        {
            if (Status != GameStatus.Running || Charging) return;
            Charging = true;
            Gauge = 0f;
            _dir = 1f;
        }

        public void Step(float dt)
        {
            if (Status != GameStatus.Running || !Charging) return;
            Gauge += _dir * _speed * dt;
            if (Gauge >= 1f) { Gauge = 2f - Gauge; _dir = -1f; }
            else if (Gauge <= 0f) { Gauge = -Gauge; _dir = 1f; }
            Gauge = Math.Max(0f, Math.Min(1f, Gauge));
        }

        /// <summary>Lance à la puissance courante ; vrai si la cible est touchée. Sans charge en cours, ne fait rien.</summary>
        public bool Release()
        {
            if (Status != GameStatus.Running || !Charging) return false;
            Charging = false;
            LastPower = Gauge;
            LastHit = Math.Abs(Gauge - TargetCenter) <= TargetHalfWidth;
            ThrowsLeft--;
            if (LastHit) Score++;
            NextTarget();
            if (Score >= Target) Status = GameStatus.Won;
            else if (ThrowsLeft <= 0) Status = GameStatus.Lost;
            return LastHit;
        }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
