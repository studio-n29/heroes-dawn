#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Faux serveur d'arène en mémoire (tests et éditeur) : captures, points, journal des duels et réclamations, avec les mêmes règles que le serveur
    /// (plafond de 10 duels par jour, points bornés, jamais soi-même). L'horloge est injectée.
    /// </summary>
    public sealed class InMemoryArenaGateway : IArenaGateway
    {
        public sealed class MatchRow { public string TournamentId = ""; public string MatchId = ""; public string PlayerId = ""; public DuelMode Mode; public DuelOutcome Outcome; public int Points; public DateTime Day; }

        readonly Func<DateTime> _now;
        readonly Func<int, ulong, DefenseSnapshot> _fallback;
        readonly Dictionary<string, DefenseSnapshot> _defenses = new Dictionary<string, DefenseSnapshot>();
        readonly Dictionary<string, int> _points = new Dictionary<string, int>();
        readonly Dictionary<string, DateTime> _reached = new Dictionary<string, DateTime>();
        readonly HashSet<string> _claims = new HashSet<string>();
        ulong _matchCounter;

        /// <summary>Identifiant et nom du joueur connecté (celui dont le jeton sert).</summary>
        public string PlayerId { get; set; } = "me";
        public readonly List<MatchRow> Matches = new List<MatchRow>();
        public readonly List<DefenseSnapshot> Uploads = new List<DefenseSnapshot>();
        public bool Offline;

        public InMemoryArenaGateway(Func<DateTime> now, Func<int, ulong, DefenseSnapshot> fallback)
        {
            _now = now; _fallback = fallback;
        }

        static readonly DomainError Unavailable = new DomainError("arena.unavailable", "errors.arena_unavailable");
        static readonly DomainError CapReached = new DomainError("arena.cap", "errors.arena_cap");
        static readonly DomainError ClaimRefused = new DomainError("arena.claim", "errors.arena_claim");

        string Key(string tournamentId, string playerId) => tournamentId + "|" + playerId;

        /// <summary>Ajoute la capture d'un autre joueur (adversaire possible).</summary>
        public void AddDefense(DefenseSnapshot snapshot) => _defenses[snapshot.PlayerId] = snapshot;

        public void SetPoints(string tournamentId, string playerId, int points)
        {
            _points[Key(tournamentId, playerId)] = points;
            _reached[Key(tournamentId, playerId)] = _now();
        }

        public int PointsOf(string tournamentId, string playerId) => _points.TryGetValue(Key(tournamentId, playerId), out var p) ? p : 0;

        public UniTask<Result> UploadDefenseAsync(DefenseSnapshot snapshot, CancellationToken ct)
        {
            if (Offline) return UniTask.FromResult(Result.Fail(Unavailable));
            snapshot.PlayerId = PlayerId;
            Uploads.Add(snapshot);
            _defenses[PlayerId] = snapshot;
            return UniTask.FromResult(Result.Ok());
        }

        public UniTask<Result<DuelTicket>> FindOfflineOpponentAsync(string tournamentId, int rating, CancellationToken ct)
        {
            if (Offline) return UniTask.FromResult(Result<DuelTicket>.Fail(Unavailable));
            var pick = _defenses.Values.Where(d => d.PlayerId != PlayerId && d.Team.Count > 0)
                                .OrderBy(d => Math.Abs(d.Rating - rating)).ThenBy(d => d.PlayerId, StringComparer.Ordinal).FirstOrDefault();
            ulong seed = 1000 + ++_matchCounter * 7919;
            var ticket = new DuelTicket { MatchId = "m" + _matchCounter, Seed = seed };
            if (pick == null) { ticket.Opponent = _fallback(rating, seed); ticket.Generated = true; }
            else ticket.Opponent = pick;
            return UniTask.FromResult(Result<DuelTicket>.Ok(ticket));
        }

        public UniTask<Result<int>> SubmitDuelAsync(string tournamentId, string matchId, DuelMode mode, DuelOutcome outcome, CancellationToken ct)
        {
            if (Offline) return UniTask.FromResult(Result<int>.Fail(Unavailable));
            var day = _now().Date;
            int current = PointsOf(tournamentId, PlayerId);
            bool counts = ArenaRules.CountsAsDuel(mode, outcome);
            if (counts && Matches.Count(m => m.TournamentId == tournamentId && m.PlayerId == PlayerId && m.Day == day && m.Counted()) >= ArenaRules.DailyDuelCap)
                return UniTask.FromResult(Result<int>.Fail(CapReached));
            int delta = ArenaRules.PointsDelta(mode, outcome, current);
            Matches.Add(new MatchRow { TournamentId = tournamentId, MatchId = matchId, PlayerId = PlayerId, Mode = mode, Outcome = outcome, Points = delta, Day = day });
            if (delta != 0) SetPoints(tournamentId, PlayerId, current + delta);
            return UniTask.FromResult(Result<int>.Ok(PointsOf(tournamentId, PlayerId)));
        }

        List<(string Id, string Name, int Points, DateTime At)> Board(string tournamentId)
        {
            var rows = new List<(string, string, int, DateTime)>();
            foreach (var kv in _points)
            {
                if (!kv.Key.StartsWith(tournamentId + "|", StringComparison.Ordinal)) continue;
                string id = kv.Key.Substring(tournamentId.Length + 1);
                rows.Add((id, _defenses.TryGetValue(id, out var d) ? d.Name : id, kv.Value, _reached[kv.Key]));
            }
            return rows.OrderByDescending(r => r.Item3).ThenBy(r => r.Item4).ThenBy(r => r.Item1, StringComparer.Ordinal).ToList();
        }

        public UniTask<Result<IReadOnlyList<ScoreRow>>> LeaderboardAsync(string tournamentId, int top, CancellationToken ct)
        {
            if (Offline) return UniTask.FromResult(Result<IReadOnlyList<ScoreRow>>.Fail(Unavailable));
            var list = Board(tournamentId).Take(top).Select((r, i) => new ScoreRow { Rank = i + 1, PlayerId = r.Id, Name = r.Name, Points = r.Points, IsPlayer = r.Id == PlayerId }).ToList();
            return UniTask.FromResult(Result<IReadOnlyList<ScoreRow>>.Ok(list));
        }

        public UniTask<Result<Standing>> StandingAsync(string tournamentId, CancellationToken ct)
        {
            if (Offline) return UniTask.FromResult(Result<Standing>.Fail(Unavailable));
            var board = Board(tournamentId);
            int index = board.FindIndex(r => r.Id == PlayerId);
            return UniTask.FromResult(Result<Standing>.Ok(index < 0 ? new Standing(0, board.Count, 0) : new Standing(index + 1, board.Count, board[index].Points)));
        }

        public UniTask<Result> ClaimAsync(string tournamentId, int tier, DateTime endsOn, CancellationToken ct)
        {
            if (Offline) return UniTask.FromResult(Result.Fail(Unavailable));
            if (_now().Date > endsOn.Date.AddDays(ArenaRules.ClaimDays)) return UniTask.FromResult(Result.Fail(ClaimRefused));
            if (!_claims.Add(tournamentId + "|" + PlayerId + "|" + tier)) return UniTask.FromResult(Result.Fail(ClaimRefused));
            return UniTask.FromResult(Result.Ok());
        }
    }

    static class MatchRowExtensions
    {
        public static bool Counted(this InMemoryArenaGateway.MatchRow m) => ArenaRules.CountsAsDuel(m.Mode, m.Outcome);
    }
}
