#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>Ce que rend un appui dans « Le rythme » : rien (aucune note à portée), bon, ou parfait.</summary>
    public enum RhythmHit { None, Good, Perfect }

    public enum NoteState { Pending, Hit, Missed }

    /// <summary>
    /// « Le rythme » (2D) : une séquence d'ennemis arrive sur des pistes, il faut toucher la bonne piste quand l'ennemi passe la ligne de frappe.
    /// Une note ratée coûte une vie ; la victoire est d'avoir résolu toutes les notes avec au moins une vie. Pas de minuteur : le tempo décide.
    /// Logique pure et déterministe : un pas de temps (<see cref="Step"/>), un appui (<see cref="Press"/>).
    /// </summary>
    public sealed class RhythmGame : IMiniGame
    {
        public sealed class Note
        {
            public int Id;
            public int Lane;
            /// <summary>Instant (secondes de la chanson) où la note passe la ligne de frappe.</summary>
            public float Time;
            public NoteState State;
        }

        /// <summary>Fenêtres de justesse autour de l'instant de la note (secondes).</summary>
        public const float PerfectWindow = 0.07f;
        public const float GoodWindow = 0.18f;
        /// <summary>Temps de descente visible d'une note avant la ligne de frappe (secondes) : sert au dessin.</summary>
        public const float Lead = 1.6f;

        readonly List<Note> _notes = new List<Note>();

        public GameStatus Status { get; private set; } = GameStatus.Running;
        /// <summary>Secondes écoulées depuis le début de la partie.</summary>
        public float Song { get; private set; }
        public int Lanes { get; }
        public int Score { get; private set; }
        public int Perfects { get; private set; }
        public int Lives { get; private set; }
        public int Target => _notes.Count;
        public float TimeLeft => -1f;
        public IReadOnlyList<Note> Notes => _notes;

        public RhythmGame(ulong seed, ActivityParameters p)
        {
            var rng = new Pcg32(seed).Fork(31);
            int count = p.Get("notes", 24);
            Lanes = p.Get("lanes", 3);
            Lives = p.Get("lives", 5);
            float speed = p.Get("speed_percent", 100) / 100f;
            float t = Lead + 0.8f;
            for (int i = 0; i < count; i++)
            {
                _notes.Add(new Note { Id = i, Lane = rng.Range(0, Lanes), Time = t, State = NoteState.Pending });
                t += (0.55f + 0.15f * rng.Range(0, 101) / 100f) / speed;
            }
        }

        public void Step(float dt)
        {
            if (Status != GameStatus.Running) return;
            Song += dt;
            foreach (var n in _notes)
            {
                if (n.State != NoteState.Pending || Song <= n.Time + GoodWindow) continue;
                n.State = NoteState.Missed;
                Lives--;
            }
            Conclude();
        }

        /// <summary>Un appui sur la piste <paramref name="lane"/> : touche la note en attente la plus proche dans la fenêtre « Bon », sinon ne fait rien.</summary>
        public RhythmHit Press(int lane)
        {
            if (Status != GameStatus.Running) return RhythmHit.None;
            Note? best = null; float bestD = GoodWindow + 1e-6f;
            foreach (var n in _notes)
            {
                if (n.State != NoteState.Pending || n.Lane != lane) continue;
                float d = Math.Abs(Song - n.Time);
                if (d <= bestD) { bestD = d; best = n; }
            }
            if (best == null) return RhythmHit.None;
            best.State = NoteState.Hit;
            Score++;
            var result = RhythmHit.Good;
            if (bestD <= PerfectWindow) { Perfects++; result = RhythmHit.Perfect; }
            Conclude();
            return result;
        }

        void Conclude()
        {
            if (Status != GameStatus.Running) return;
            if (Lives <= 0) { Status = GameStatus.Lost; return; }
            foreach (var n in _notes) if (n.State == NoteState.Pending) return;
            Status = GameStatus.Won;
        }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
