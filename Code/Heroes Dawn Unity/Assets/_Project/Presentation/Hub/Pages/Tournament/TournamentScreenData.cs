#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>État d'une pastille de palier (planche 13.1) : validé, en cours, à venir.</summary>
    public enum TournamentTierState { Done, Current, Locked }

    public readonly struct TournamentRankData
    {
        public readonly int Rank;
        public readonly string Name, Points;
        public readonly bool IsPlayer;
        public TournamentRankData(int rank, string name, string points, bool isPlayer) { Rank = rank; Name = name; Points = points; IsPlayer = isPlayer; }
    }

    public readonly struct TournamentTierData
    {
        public readonly string Label;
        public readonly TournamentTierState State;
        public TournamentTierData(string label, TournamentTierState state) { Label = label; State = state; }
    }

    /// <summary>Tout ce que l'écran Tournoi affiche (planches 13.1 « en cours » et 13.2 « terminé » / à venir).</summary>
    public struct TournamentScreenData
    {
        public string Title, StateLabel;
        /// <summary>Vrai hors période de tournoi : l'écran annonce le prochain tournoi et propose Retour au QG.</summary>
        public bool Finished;
        public string RankingTitle, RankingEmpty;
        public TournamentRankData[] Ranking;
        public string TiersTitle;
        public TournamentTierData[] Tiers;
        public string StatusTitle, StatusMain, StatusSub;
        public float Progress;
        public string ClaimLabel, ClaimReason, DuelLabel, DuelReason, BackLabel;
        public bool CanClaim, CanDuel;
    }

    public interface ITournamentPageView : IView
    {
        event Action ClaimClicked;
        event Action BackToQgClicked;
        void Show(TournamentScreenData data);
    }
}
