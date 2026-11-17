#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Étapes de l'ouverture d'un booster.</summary>
    public enum RevealPhase
    {
        /// <summary>Le paquet fermé attend un toucher.</summary>
        Sealed,
        /// <summary>Animation du paquet qui s'ouvre (la vue appelle <see cref="BoosterRevealSequence.OpeningFinished"/>).</summary>
        Opening,
        /// <summary>Cartes face cachée : un toucher retourne la suivante.</summary>
        Revealing,
        /// <summary>Toutes les cartes sont retournées ; un toucher ou « Continuer » ouvre le récapitulatif.</summary>
        AllRevealed,
        /// <summary>Récapitulatif de toutes les cartes obtenues.</summary>
        Recap,
        Closed,
    }

    public enum SeqActionKind { None, StartOpening, Reveal, ToRecap }

    /// <summary>Ce qu'un toucher a provoqué ; <see cref="Index"/> vaut l'indice de la carte retournée (sinon -1).</summary>
    public readonly struct SeqAction
    {
        public readonly SeqActionKind Kind;
        public readonly int Index;
        public SeqAction(SeqActionKind kind, int index = -1) { Kind = kind; Index = index; }
        public static readonly SeqAction None = new SeqAction(SeqActionKind.None);
    }

    /// <summary>Une carte tirée vue par la séquence : rang de rareté (0 à 3) et « nouvelle » (sinon doublon).</summary>
    public readonly struct RevealCard
    {
        public readonly int Rank;
        public readonly bool IsNew;
        public RevealCard(int rank, bool isNew) { Rank = rank; IsNew = isNew; }
    }

    /// <summary>Mise en scène du retournement d'une carte selon sa rareté (la vue l'adapte au mode « réduire les animations »).</summary>
    public readonly struct RevealStyle
    {
        /// <summary>Particules à l'éclat.</summary>
        public readonly int Sparks;
        /// <summary>Pause avant le retournement (suspense).</summary>
        public readonly float PauseSeconds;
        /// <summary>Flash teinté sur tout l'écran.</summary>
        public readonly bool ScreenFlash;
        /// <summary>Rayons dorés derrière la carte.</summary>
        public readonly bool Rays;
        /// <summary>Secousse de la carte.</summary>
        public readonly bool Punch;
        public RevealStyle(int sparks, float pause, bool flash, bool rays, bool punch) { Sparks = sparks; PauseSeconds = pause; ScreenFlash = flash; Rays = rays; Punch = punch; }
    }

    public static class RevealStyles
    {
        public static RevealStyle For(int rank)
        {
            switch (Math.Max(0, Math.Min(3, rank)))
            {
                case 0: return new RevealStyle(6, 0f, false, false, false);
                case 1: return new RevealStyle(14, 0f, false, true, false);
                case 2: return new RevealStyle(24, 0f, false, true, true);
                default: return new RevealStyle(40, 0.4f, true, true, true);
            }
        }
    }

    /// <summary>
    /// Logique pure de l'ouverture d'un booster (sans Unity) : phases, révélation carte par carte, passer, récapitulatif.
    /// La vue la pilote et joue les animations ; le tirage des cartes n'est pas de son ressort.
    /// </summary>
    public sealed class BoosterRevealSequence
    {
        readonly RevealCard[] _cards;
        readonly bool[] _revealed;

        public BoosterRevealSequence(IReadOnlyList<RevealCard> cards)
        {
            _cards = new RevealCard[cards.Count];
            for (int i = 0; i < cards.Count; i++) _cards[i] = cards[i];
            _revealed = new bool[_cards.Length];
        }

        public RevealPhase Phase { get; private set; } = RevealPhase.Sealed;
        public int Count => _cards.Length;
        public int RevealedCount { get; private set; }
        public bool OpenAnother { get; private set; }
        public RevealCard Card(int i) => _cards[i];
        public bool IsRevealed(int i) => _revealed[i];

        /// <summary>Indice de la prochaine carte à retourner, -1 s'il n'en reste pas.</summary>
        public int NextIndex
        {
            get { for (int i = 0; i < _revealed.Length; i++) if (!_revealed[i]) return i; return -1; }
        }

        public int NewCount { get { int n = 0; foreach (var c in _cards) if (c.IsNew) n++; return n; } }
        public int DuplicateCount => _cards.Length - NewCount;

        /// <summary>La carte mise en avant au récapitulatif : la plus rare, puis une nouvelle, puis la première ; -1 sans carte.</summary>
        public int BestIndex
        {
            get
            {
                int best = -1;
                for (int i = 0; i < _cards.Length; i++)
                {
                    if (best < 0 || _cards[i].Rank > _cards[best].Rank || (_cards[i].Rank == _cards[best].Rank && _cards[i].IsNew && !_cards[best].IsNew)) best = i;
                }
                return best;
            }
        }

        /// <summary>Un toucher du joueur (sur le paquet, une carte ou l'écran).</summary>
        public SeqAction Tap()
        {
            switch (Phase)
            {
                case RevealPhase.Sealed:
                    Phase = RevealPhase.Opening;
                    return new SeqAction(SeqActionKind.StartOpening);
                case RevealPhase.Revealing:
                    int i = NextIndex;
                    if (i < 0) { Phase = RevealPhase.AllRevealed; return SeqAction.None; }
                    _revealed[i] = true; RevealedCount++;
                    if (NextIndex < 0) Phase = RevealPhase.AllRevealed;
                    return new SeqAction(SeqActionKind.Reveal, i);
                case RevealPhase.AllRevealed:
                    Phase = RevealPhase.Recap;
                    return new SeqAction(SeqActionKind.ToRecap);
                default:
                    return SeqAction.None;
            }
        }

        /// <summary>La vue a fini d'animer l'ouverture du paquet (tout de suite en mode « réduire les animations »).</summary>
        public void OpeningFinished()
        {
            if (Phase != RevealPhase.Opening) return;
            Phase = NextIndex < 0 ? RevealPhase.AllRevealed : RevealPhase.Revealing;
        }

        /// <summary>Passer : retourne tout et va au récapitulatif.</summary>
        public void Skip()
        {
            if (Phase == RevealPhase.Recap || Phase == RevealPhase.Closed) return;
            RevealAll();
            Phase = RevealPhase.Recap;
        }

        /// <summary>Ferme (Continuer, Retour) ; le tirage étant déjà enregistré, toutes les cartes comptent comme reçues.</summary>
        public void Close(bool openAnother)
        {
            if (Phase == RevealPhase.Closed) return;
            RevealAll();
            OpenAnother = openAnother;
            Phase = RevealPhase.Closed;
        }

        void RevealAll()
        {
            for (int i = 0; i < _revealed.Length; i++) _revealed[i] = true;
            RevealedCount = _revealed.Length;
        }
    }
}
