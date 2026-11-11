#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>
    /// Données de l'arène sur Supabase : captures dans <c>hd_arena_defenses</c> (PostgREST), points, classement et réclamations par les fonctions
    /// <c>hd_submit_duel</c>, <c>hd_arena_leaderboard</c>, <c>hd_arena_standing</c>, <c>hd_arena_find_opponent</c> et <c>hd_claim_tier</c> (script 002_hd_arena.sql).
    /// Le jeton du joueur sert partout ; la sécurité par ligne et les fonctions font respecter les règles (jamais de clé service_role).
    /// </summary>
    public sealed class SupabaseArenaGateway : IArenaGateway
    {
        /// <summary>Taille maximale d'une capture (octets) : la même que la contrainte de la base.</summary>
        public const int MaxSnapshotBytes = 65536;

        [Serializable] sealed class OpponentRow { public string match_id = ""; public long seed; public string opponent_id = ""; public DefenseSnapshot snapshot = new DefenseSnapshot(); }
        [Serializable] sealed class OpponentRows { public OpponentRow[] rows = new OpponentRow[0]; }
        [Serializable] sealed class BoardRow { public int rank; public string user_id = ""; public string name = ""; public int points; public bool is_player; }
        [Serializable] sealed class BoardRows { public BoardRow[] rows = new BoardRow[0]; }
        [Serializable] sealed class StandingRow { public int rank; public int total; public int points; }
        [Serializable] sealed class StandingRows { public StandingRow[] rows = new StandingRow[0]; }
        [Serializable] sealed class ErrorBody { public string message = ""; public string code = ""; }

        readonly SupabaseConfig _config;
        readonly ISupabaseHttp _http;
        readonly SupabaseAccountGateway _accounts;
        readonly Func<int, ulong, DefenseSnapshot> _fallback;

        /// <param name="fallback">Équipe de repli quand le serveur n'a aucune capture à proposer (rating, graine).</param>
        public SupabaseArenaGateway(SupabaseConfig config, ISupabaseHttp http, SupabaseAccountGateway accounts, Func<int, ulong, DefenseSnapshot> fallback)
        {
            _config = config; _http = http; _accounts = accounts; _fallback = fallback;
        }

        public static DomainError Unavailable() => new DomainError("arena.unavailable", "errors.arena_unavailable");
        static DomainError CapReached() => new DomainError("arena.cap", "errors.arena_cap");
        static DomainError ClaimRefused() => new DomainError("arena.claim", "errors.arena_claim");

        // ---- IArenaGateway
        public async UniTask<Result> UploadDefenseAsync(DefenseSnapshot snapshot, CancellationToken ct)
        {
            string? uid = _accounts.UserId;
            if (string.IsNullOrEmpty(uid)) return Result.Fail(SupabaseErrors.Session());
            string json = JsonUtility.ToJson(snapshot);
            if (Encoding.UTF8.GetByteCount(json) > MaxSnapshotBytes) return Result.Fail(Unavailable());
            string body = "{\"user_id\":\"" + uid + "\",\"snapshot\":" + json + ",\"rating\":" + Math.Max(0, snapshot.Rating) + "}";
            var sent = await SendAuthorizedAsync("POST", _config.Url + "/rest/v1/hd_arena_defenses?on_conflict=user_id", body, "resolution=merge-duplicates,return=minimal", ct);
            if (sent.Error != null) return Result.Fail(sent.Error);
            return sent.Response.IsSuccess ? Result.Ok() : Result.Fail(Failure(sent.Response));
        }

        public async UniTask<Result<DuelTicket>> FindOfflineOpponentAsync(string tournamentId, int rating, CancellationToken ct)
        {
            var sent = await RpcAsync("hd_arena_find_opponent", "{\"p_tournament\":" + Quote(tournamentId) + ",\"p_rating\":" + Math.Max(0, rating) + "}", ct);
            if (sent.Error != null) return Result<DuelTicket>.Fail(sent.Error);
            if (!sent.Response.IsSuccess) return Result<DuelTicket>.Fail(Failure(sent.Response));
            var rows = Parse<OpponentRows>(sent.Response.Body)?.rows;
            if (rows == null) return Result<DuelTicket>.Fail(Unavailable());
            if (rows.Length == 0)
            {
                ulong seed = (ulong)(DateTime.UtcNow.Ticks & 0x7fffffff);
                return Result<DuelTicket>.Ok(new DuelTicket { MatchId = "fb-" + seed, Seed = seed, Opponent = _fallback(rating, seed), Generated = true });
            }
            var row = rows[0];
            row.snapshot.PlayerId = row.opponent_id;
            return Result<DuelTicket>.Ok(new DuelTicket { MatchId = row.match_id, Seed = (ulong)Math.Max(0, row.seed), Opponent = row.snapshot });
        }

        public async UniTask<Result<int>> SubmitDuelAsync(string tournamentId, string matchId, DuelMode mode, DuelOutcome outcome, CancellationToken ct)
        {
            string body = "{\"p_tournament\":" + Quote(tournamentId) + ",\"p_match\":" + Quote(matchId)
                        + ",\"p_mode\":\"" + (mode == DuelMode.Online ? "online" : "offline") + "\",\"p_result\":\"" + outcome.ToString().ToLowerInvariant() + "\"}";
            var sent = await RpcAsync("hd_submit_duel", body, ct);
            if (sent.Error != null) return Result<int>.Fail(sent.Error);
            if (!sent.Response.IsSuccess) return Result<int>.Fail(Failure(sent.Response));
            return int.TryParse(sent.Response.Body.Trim(), out int total) ? Result<int>.Ok(total) : Result<int>.Fail(Unavailable());
        }

        public async UniTask<Result<IReadOnlyList<ScoreRow>>> LeaderboardAsync(string tournamentId, int top, CancellationToken ct)
        {
            var sent = await RpcAsync("hd_arena_leaderboard", "{\"p_tournament\":" + Quote(tournamentId) + ",\"p_top\":" + Math.Max(1, Math.Min(100, top)) + "}", ct);
            if (sent.Error != null) return Result<IReadOnlyList<ScoreRow>>.Fail(sent.Error);
            if (!sent.Response.IsSuccess) return Result<IReadOnlyList<ScoreRow>>.Fail(Failure(sent.Response));
            var rows = Parse<BoardRows>(sent.Response.Body)?.rows;
            if (rows == null) return Result<IReadOnlyList<ScoreRow>>.Fail(Unavailable());
            var list = new List<ScoreRow>();
            foreach (var r in rows) list.Add(new ScoreRow { Rank = r.rank, PlayerId = r.user_id, Name = r.name, Points = r.points, IsPlayer = r.is_player });
            return Result<IReadOnlyList<ScoreRow>>.Ok(list);
        }

        public async UniTask<Result<Standing>> StandingAsync(string tournamentId, CancellationToken ct)
        {
            var sent = await RpcAsync("hd_arena_standing", "{\"p_tournament\":" + Quote(tournamentId) + "}", ct);
            if (sent.Error != null) return Result<Standing>.Fail(sent.Error);
            if (!sent.Response.IsSuccess) return Result<Standing>.Fail(Failure(sent.Response));
            var rows = Parse<StandingRows>(sent.Response.Body)?.rows;
            if (rows == null || rows.Length == 0) return Result<Standing>.Fail(Unavailable());
            return Result<Standing>.Ok(new Standing(rows[0].rank, rows[0].total, rows[0].points));
        }

        public async UniTask<Result> ClaimAsync(string tournamentId, int tier, DateTime endsOn, CancellationToken ct)
        {
            string body = "{\"p_tournament\":" + Quote(tournamentId) + ",\"p_tier\":" + tier + ",\"p_ends_on\":\"" + endsOn.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + "\"}";
            var sent = await RpcAsync("hd_claim_tier", body, ct);
            if (sent.Error != null) return Result.Fail(sent.Error);
            return sent.Response.IsSuccess ? Result.Ok() : Result.Fail(Failure(sent.Response));
        }

        // ---- Réseau
        readonly struct Authorized
        {
            public readonly HttpResult Response;
            public readonly DomainError? Error;
            public Authorized(HttpResult response, DomainError? error) { Response = response; Error = error; }
        }

        UniTask<Authorized> RpcAsync(string function, string jsonBody, CancellationToken ct)
            => SendAuthorizedAsync("POST", _config.Url + "/rest/v1/rpc/" + function, jsonBody, null, ct);

        /// <summary>Envoie avec le jeton du joueur ; un 401 (jeton refusé alors qu'il semblait valide) déclenche un seul renouvellement puis un seul nouvel essai.</summary>
        async UniTask<Authorized> SendAuthorizedAsync(string method, string url, string? body, string? prefer, CancellationToken ct)
        {
            var token = await _accounts.AccessTokenAsync(ct);
            if (token.IsFailure) return new Authorized(null!, token.Error!);
            var r = await _http.SendAsync(method, url, Headers(token.Value, prefer), body, ct);
            if (!r.NetworkError && r.Status == 401)
            {
                var again = await _accounts.ForceRefreshAsync(ct);
                if (again.IsFailure) return new Authorized(null!, again.Error!);
                r = await _http.SendAsync(method, url, Headers(again.Value, prefer), body, ct);
            }
            return new Authorized(r, null);
        }

        Dictionary<string, string> Headers(string accessToken, string? prefer)
        {
            var h = new Dictionary<string, string>
            {
                ["apikey"] = _config.AnonKey,
                ["Authorization"] = "Bearer " + accessToken,
                ["Content-Type"] = "application/json",
            };
            if (prefer != null) h["Prefer"] = prefer;
            return h;
        }

        /// <summary>Erreur du jeu pour une réponse non réussie (jamais le texte brut du serveur).</summary>
        static DomainError Failure(HttpResult r)
        {
            if (r.NetworkError || r.Status >= 500 || r.Status == 0) return SupabaseErrors.Network();
            if (r.Status == 401 || r.Status == 403) return SupabaseErrors.Session();
            string message = "";
            try { message = JsonUtility.FromJson<ErrorBody>(r.Body)?.message ?? ""; }
            catch (Exception) { /* corps illisible */ }
            switch (message)
            {
                case "daily_cap": return CapReached();
                case "already_claimed":
                case "claim_expired":
                case "not_earned":
                case "not_finished": return ClaimRefused();
            }
            return Unavailable();
        }

        /// <summary>Les lignes d'une réponse PostgREST (un tableau JSON) enveloppées pour JsonUtility, ou nul si ce n'est pas un tableau.</summary>
        static T? Parse<T>(string body) where T : class
        {
            string trimmed = (body ?? "").Trim();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal)) return null;
            try { return JsonUtility.FromJson<T>("{\"rows\":" + trimmed + "}"); }
            catch (Exception) { return null; }
        }

        static string Quote(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
