#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>Un agent de la capture d'équipe (champs publics et listes seulement : JsonUtility). Une carte vide = emplacement libre.</summary>
    [Serializable]
    public sealed class AllyEntrySave
    {
        public string member = "";
        public int level = 1;
        public List<string> items = new List<string>();
        public List<string> cards = new List<string>();
        public List<int> cardLevels = new List<int>();
    }

    /// <summary>Niveau de lien d'une paire d'agents de la capture.</summary>
    [Serializable]
    public sealed class LinkEntrySave { public string a = ""; public string b = ""; public int level = 1; }

    /// <summary>L'équipe de défense d'un joueur, telle qu'envoyée au serveur : agents, niveaux, cartes, objets et liens.</summary>
    [Serializable]
    public sealed class DefenseSnapshot
    {
        public string PlayerId = "";
        public string Name = "";
        /// <summary>Puissance de l'équipe (somme des niveaux × 10) : sert à choisir un adversaire proche.</summary>
        public int Rating;
        public List<AllyEntrySave> Team = new List<AllyEntrySave>();
        public List<LinkEntrySave> Links = new List<LinkEntrySave>();
        /// <summary>Date de la capture (ISO 8601, UTC) ; la plus récente d'un joueur est celle qui sert.</summary>
        public string UpdatedUtc = "";
    }

    /// <summary>Une ligne du classement d'un tournoi.</summary>
    public sealed class ScoreRow
    {
        public int Rank;
        public string PlayerId = "";
        public string Name = "";
        public int Points;
        public bool IsPlayer;
    }

    /// <summary>Place du joueur dans un tournoi : rang, nombre de classés et points (rang 0 : pas classé).</summary>
    public readonly struct Standing
    {
        public readonly int Rank, Total, Points;
        public Standing(int rank, int total, int points) { Rank = rank; Total = total; Points = points; }
    }

    /// <summary>Un duel proposé : identifiant du match, graine donnée par le serveur et capture de l'adversaire.</summary>
    public sealed class DuelTicket
    {
        public string MatchId = "";
        public ulong Seed;
        public DefenseSnapshot Opponent = new DefenseSnapshot();
        /// <summary>Vrai quand l'équipe a été générée faute de capture disponible.</summary>
        public bool Generated;
    }

    /// <summary>Données de l'arène côté serveur (captures, points, classement, réclamations). Implémentations : en mémoire (tests, éditeur) et Supabase.</summary>
    public interface IArenaGateway
    {
        UniTask<Result> UploadDefenseAsync(DefenseSnapshot snapshot, CancellationToken ct);
        /// <summary>Un adversaire de score proche, jamais le joueur lui-même ; sans capture disponible, une équipe de repli générée.</summary>
        UniTask<Result<DuelTicket>> FindOfflineOpponentAsync(string tournamentId, int rating, CancellationToken ct);
        /// <summary>Verse l'issue d'un duel ; renvoie les points totaux du joueur dans ce tournoi.</summary>
        UniTask<Result<int>> SubmitDuelAsync(string tournamentId, string matchId, DuelMode mode, DuelOutcome outcome, CancellationToken ct);
        UniTask<Result<IReadOnlyList<ScoreRow>>> LeaderboardAsync(string tournamentId, int top, CancellationToken ct);
        UniTask<Result<Standing>> StandingAsync(string tournamentId, CancellationToken ct);
        /// <summary>Réclame un palier (1 à 5) ou un lot de classement ; refusé si déjà réclamé, hors délai ou non mérité. <c>endsOn</c> : dernier jour du tournoi.</summary>
        UniTask<Result> ClaimAsync(string tournamentId, int tier, DateTime endsOn, CancellationToken ct);
    }
}
