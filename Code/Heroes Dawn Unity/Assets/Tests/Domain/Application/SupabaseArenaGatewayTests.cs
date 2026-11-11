using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Infrastructure.Supabase;
using N29.HeroesDawn.Tests.Domain.Application;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Passerelles Supabase de l'arène (PostgREST et Realtime) avec un faux client HTTP et un faux socket — bloc D de la 1.0.0.</summary>
    public sealed class SupabaseArenaGatewayTests
    {
        const string Url = "https://projet.supabase.co";
        const string Uid = "11111111-2222-3333-4444-555555555555";
        static readonly CancellationToken Ct = CancellationToken.None;

        FakeSupabaseHttp _http = null!;
        MemoryTokenStore _tokens = null!;
        SupabaseArenaGateway _arena = null!;
        SupabaseAccountGateway _accounts = null!;
        SupabaseConfig _config = null!;

        [SetUp]
        public void SetUp()
        {
            _http = new FakeSupabaseHttp();
            _tokens = new MemoryTokenStore();
            _tokens.Save(new SupabaseSession { AccessToken = "JETON", RefreshToken = "R", ExpiresAtUnix = 4102444800, UserId = Uid, Email = "moi@exemple.fr", DisplayName = "Moi" });
            _config = new SupabaseConfig(Url, "cle-anon", "heroesdawn://auth");
            _accounts = new SupabaseAccountGateway(_config, _http, _tokens, new FixedClock());
            _arena = new SupabaseArenaGateway(_config, _http, _accounts, (rating, seed) => new DefenseSnapshot { PlayerId = "fallback", Rating = rating });
        }

        static DefenseSnapshot Snap(int rating = 40)
        {
            var s = new DefenseSnapshot { PlayerId = "x", Name = "Moi", Rating = rating };
            s.Team.Add(new AllyEntrySave { member = "darius_brooks", level = 4 });
            return s;
        }

        [Test, Description("Vérifie que l'envoi de la défense fait un upsert POST sur hd_arena_defenses avec l'identifiant du joueur, la capture et le score, la clé publique et le jeton du joueur — parce que c'est le contrat avec PostgREST et la sécurité par ligne")]
        public void UploadDefense_envoie_un_upsert_sur_hd_arena_defenses()
        {
            _http.Enqueue(201, "");
            var r = _arena.UploadDefenseAsync(Snap(), Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            var req = _http.Requests.Single();
            Assert.That(req.Method, Is.EqualTo("POST"));
            Assert.That(req.Url, Is.EqualTo(Url + "/rest/v1/hd_arena_defenses?on_conflict=user_id"));
            Assert.That(req.Headers["Prefer"], Does.Contain("resolution=merge-duplicates"));
            Assert.That(req.Headers["apikey"], Is.EqualTo("cle-anon"));
            Assert.That(req.Headers["Authorization"], Is.EqualTo("Bearer JETON"));
            Assert.That(req.Body, Does.Contain("\"user_id\":\"" + Uid + "\""));
            Assert.That(req.Body, Does.Contain("darius_brooks"));
            Assert.That(req.Body, Does.Contain("\"rating\":40"));
        }

        [Test, Description("Vérifie qu'une capture de plus de 64 Ko est refusée avant tout envoi — parce que la base refuserait cette ligne")]
        public void UploadDefense_refuse_une_capture_trop_grosse()
        {
            var big = Snap();
            for (int i = 0; i < 4000; i++) big.Team.Add(new AllyEntrySave { member = "agent_tres_long_pour_gonfler_la_capture_" + i, level = 1 });
            var r = _arena.UploadDefenseAsync(big, Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(_http.Requests, Is.Empty);
        }

        [Test, Description("Vérifie que le versement d'un duel appelle la fonction RPC hd_submit_duel avec l'édition, le duel, le mode et le résultat, et lit les points totaux — parce que le serveur applique points et plafond")]
        public void SubmitDuel_appelle_la_fonction_rpc_hd_submit_duel_avec_le_mode_et_le_resultat()
        {
            _http.Enqueue(200, "30");
            var r = _arena.SubmitDuelAsync("christmas:2026", "m1", DuelMode.Online, DuelOutcome.Win, Ct).GetAwaiter().GetResult();
            Assert.That(r.Value, Is.EqualTo(30));
            var req = _http.Requests.Single();
            Assert.That((req.Method, req.Url), Is.EqualTo(("POST", Url + "/rest/v1/rpc/hd_submit_duel")));
            Assert.That(req.Body, Does.Contain("\"p_tournament\":\"christmas:2026\""));
            Assert.That(req.Body, Does.Contain("\"p_match\":\"m1\""));
            Assert.That(req.Body, Does.Contain("\"p_mode\":\"online\""));
            Assert.That(req.Body, Does.Contain("\"p_result\":\"win\""));
        }

        [Test, Description("Vérifie que la limite quotidienne du serveur donne l'erreur « limite atteinte » et une réponse illisible une erreur localisée — parce que la présentation doit pouvoir l'expliquer")]
        public void SubmitDuel_traduit_les_erreurs_du_serveur()
        {
            _http.Enqueue(400, "{\"code\":\"P0001\",\"message\":\"daily_cap\"}");
            Assert.That(_arena.SubmitDuelAsync("christmas:2026", "m1", DuelMode.Offline, DuelOutcome.Loss, Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.arena_cap"));
            _http.Enqueue(200, "pas un nombre");
            Assert.That(_arena.SubmitDuelAsync("christmas:2026", "m2", DuelMode.Offline, DuelOutcome.Loss, Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.arena_unavailable"));
        }

        [Test, Description("Vérifie que le classement lit le top et marque la ligne du joueur — parce que la page met le joueur en évidence")]
        public void Leaderboard_lit_le_top_et_marque_le_joueur()
        {
            _http.Enqueue(200, "[{\"rank\":1,\"user_id\":\"a\",\"name\":\"Alice\",\"points\":120,\"is_player\":false},{\"rank\":2,\"user_id\":\"" + Uid + "\",\"name\":\"Moi\",\"points\":90,\"is_player\":true}]");
            var r = _arena.LeaderboardAsync("christmas:2026", 100, Ct).GetAwaiter().GetResult().Value;
            Assert.That(r.Select(x => x.Name), Is.EqualTo(new[] { "Alice", "Moi" }));
            Assert.That(r.Select(x => x.IsPlayer), Is.EqualTo(new[] { false, true }));
            Assert.That(r[1].Rank, Is.EqualTo(2));
            Assert.That(_http.Requests.Single().Body, Does.Contain("\"p_top\":100"));
        }

        [Test, Description("Vérifie que l'adversaire hors ligne est lu depuis la fonction du serveur avec l'identifiant de l'adversaire, et qu'une réponse vide donne l'équipe de repli — parce que le duel ne doit jamais rester bloqué")]
        public void FindOfflineOpponent_lit_la_capture_ou_prend_le_repli()
        {
            _http.Enqueue(200, "[{\"match_id\":\"m-9\",\"seed\":777,\"opponent_id\":\"rival-1\",\"snapshot\":" + JsonUtility.ToJson(Snap(55)) + "}]");
            var t = _arena.FindOfflineOpponentAsync("christmas:2026", 50, Ct).GetAwaiter().GetResult().Value;
            Assert.That((t.MatchId, t.Seed, t.Opponent.PlayerId, t.Opponent.Rating, t.Generated), Is.EqualTo(("m-9", 777UL, "rival-1", 55, false)));
            _http.Enqueue(200, "[]");
            var f = _arena.FindOfflineOpponentAsync("christmas:2026", 50, Ct).GetAwaiter().GetResult().Value;
            Assert.That(f.Generated, Is.True);
            Assert.That(f.Opponent.PlayerId, Is.EqualTo("fallback"));
        }

        [Test, Description("Vérifie que la réclamation appelle hd_claim_tier avec le palier et la date de fin, et qu'un refus du serveur donne l'erreur de réclamation — parce que le délai de 7 jours est aussi vérifié côté serveur")]
        public void Claim_appelle_hd_claim_tier()
        {
            _http.Enqueue(204, "");
            Assert.That(_arena.ClaimAsync("christmas:2026", 3, new DateTime(2026, 12, 26), Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            var req = _http.Requests.Single();
            Assert.That(req.Url, Is.EqualTo(Url + "/rest/v1/rpc/hd_claim_tier"));
            Assert.That(req.Body, Does.Contain("\"p_tier\":3"));
            Assert.That(req.Body, Does.Contain("\"p_ends_on\":\"2026-12-26\""));
            _http.Enqueue(400, "{\"code\":\"P0001\",\"message\":\"already_claimed\"}");
            Assert.That(_arena.ClaimAsync("christmas:2026", 3, new DateTime(2026, 12, 26), Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.arena_claim"));
        }

        [Test, Description("Vérifie qu'un 401 sur un jeton qui semblait valide renouvelle le jeton puis réessaie une seule fois avec le nouveau — parce qu'une expiration passagère ne doit pas faire perdre un résultat de duel")]
        public void Une_reponse_401_rafraichit_le_jeton_puis_reessaie()
        {
            _http.Enqueue(401, "{\"message\":\"JWT expired\"}");
            _http.Enqueue(200, "{\"access_token\":\"ACCESS-2\",\"refresh_token\":\"R2\",\"expires_in\":3600,\"expires_at\":4102444800,\"user\":{\"id\":\"" + Uid + "\",\"email\":\"moi@exemple.fr\",\"app_metadata\":{\"provider\":\"email\"},\"user_metadata\":{\"display_name\":\"Moi\"}}}");
            _http.Enqueue(200, "20");
            var r = _arena.SubmitDuelAsync("christmas:2026", "m1", DuelMode.Online, DuelOutcome.Win, Ct).GetAwaiter().GetResult();
            Assert.That(r.Value, Is.EqualTo(20));
            Assert.That(_http.Requests.Count, Is.EqualTo(3));
            Assert.That(_http.Requests[2].Headers["Authorization"], Is.EqualTo("Bearer ACCESS-2"));
        }

        [Test, Description("Vérifie que les erreurs réseau, 5xx et l'absence de session donnent des erreurs localisées sans exception — parce que le mobile perd souvent la connexion")]
        public void Les_erreurs_reseau_donnent_une_erreur_localisee()
        {
            _http.EnqueueNetworkError();
            Assert.That(_arena.SubmitDuelAsync("christmas:2026", "m1", DuelMode.Offline, DuelOutcome.Win, Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_network"));
            _http.Enqueue(503, "oups");
            Assert.That(_arena.LeaderboardAsync("christmas:2026", 10, Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_network"));
            _tokens.Clear();
            int before = _http.Requests.Count;
            Assert.That(_arena.UploadDefenseAsync(Snap(), Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_session"));
            Assert.That(_http.Requests.Count, Is.EqualTo(before));
        }


        // ---- Canal Realtime
        /// <summary>Faux « serveur » : relie les sockets (la diffusion d'un client arrive chez les autres) et peut réagir à une trame.</summary>
        sealed class Hub
        {
            public readonly List<FakeSocket> Sockets = new List<FakeSocket>();
            public DateTime Now = new DateTime(2026, 12, 10, 12, 0, 0, DateTimeKind.Utc);
            public Action<FakeSocket, string>? OnBroadcast;
            public FakeSocket Create() { var s = new FakeSocket(this); Sockets.Add(s); return s; }
        }

        sealed class FakeSocket : IRealtimeSocket
        {
            readonly Hub _hub;
            public readonly Queue<string> Inbox = new Queue<string>();
            public readonly List<string> Sent = new List<string>();
            public bool Closed;
            public FakeSocket(Hub hub) { _hub = hub; }
            public UniTask ConnectAsync(string url, CancellationToken ct) => UniTask.CompletedTask;
            public UniTask SendAsync(string text, CancellationToken ct)
            {
                Sent.Add(text);
                if (text.Contains("\"event\":\"broadcast\""))
                {
                    foreach (var s in _hub.Sockets) if (s != this) s.Inbox.Enqueue(text);
                    _hub.OnBroadcast?.Invoke(this, text);
                }
                return UniTask.CompletedTask;
            }
            public UniTask<string?> ReceiveAsync(TimeSpan timeout, CancellationToken ct) => UniTask.FromResult(Inbox.Count > 0 ? Inbox.Dequeue() : null);
            public UniTask CloseAsync() { Closed = true; return UniTask.CompletedTask; }
        }

        SupabaseDuelChannel Channel(Hub hub, string userId, out FakeSocket socket)
        {
            var tokens = new MemoryTokenStore();
            tokens.Save(new SupabaseSession { AccessToken = "JETON-" + userId, RefreshToken = "R", ExpiresAtUnix = 4102444800, UserId = userId, Email = "e@x.fr", DisplayName = userId });
            var accounts = new SupabaseAccountGateway(_config, _http, tokens, new FixedClock());
            var s = hub.Create();
            socket = s;
            return new SupabaseDuelChannel(_config, accounts, () => s, () => hub.Now, by => hub.Now += by);
        }

        static string Frame(string msgJson)
            => "{\"topic\":\"realtime:arena:christmas:2026:0\",\"event\":\"broadcast\",\"payload\":{\"type\":\"broadcast\",\"event\":\"arena\",\"payload\":" + msgJson + "}}";

        static string Escaped(DefenseSnapshot s) => JsonUtility.ToJson(s).Replace("\\", "\\\\").Replace("\"", "\\\"");

        static string MatchOf(string frame) => System.Text.RegularExpressions.Regex.Match(frame, "\"match\":\"([^\"]*)\"").Groups[1].Value;

        /// <summary>bbb est déjà en file (son signal attend chez aaa) ; quand aaa propose le duel, bbb « répond » avec sa capture.</summary>
        void ScriptAckFromBbb(Hub hub, FakeSocket sockA)
        {
            sockA.Inbox.Enqueue(Frame("{\"t\":\"hello\",\"from\":\"bbb\",\"rating\":45}"));
            hub.OnBroadcast = (from, text) =>
            {
                if (from == sockA && text.Contains("\"t\":\"match\""))
                    sockA.Inbox.Enqueue(Frame("{\"t\":\"ack\",\"from\":\"bbb\",\"to\":\"aaa\",\"match\":\"" + MatchOf(text) + "\",\"snap\":\"" + Escaped(Snap(45)) + "\"}"));
            };
        }

        [Test, Description("Vérifie que sans adversaire le délai écoulé rend « personne » et ferme la connexion, après avoir rejoint le canal de la tranche de score — parce que le duel passe alors hors ligne au bout de 15 secondes")]
        public void Sans_adversaire_la_file_rend_nul_au_bout_du_delai()
        {
            var hub = new Hub();
            var ch = Channel(hub, "aaa", out var socket);
            var t = ch.JoinQueueAsync("christmas:2026", Snap(40), TimeSpan.FromSeconds(15), Ct).GetAwaiter().GetResult();
            Assert.That(t, Is.Null);
            Assert.That(socket.Closed, Is.True);
            Assert.That(socket.Sent.Any(x => x.Contains("phx_join") && x.Contains("realtime:arena:christmas:2026:0") && x.Contains("JETON-aaa")), Is.True);
            Assert.That(hub.Now - new DateTime(2026, 12, 10, 12, 0, 0, DateTimeKind.Utc), Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(15)));
        }

        [Test, Description("Vérifie que deux joueurs en file se trouvent (le plus petit identifiant propose, l'autre accepte), avec le même duel, la même graine et la capture de l'autre, puis échangent commandes, empreintes et départ — parce que c'est le cœur du duel en ligne")]
        public void Deux_joueurs_se_trouvent_et_echangent_des_messages()
        {
            var hub = new Hub();
            var a = Channel(hub, "aaa", out var sockA);
            var b = Channel(hub, "bbb", out var sockB);
            ScriptAckFromBbb(hub, sockA);
            var ta = a.JoinQueueAsync("christmas:2026", Snap(40), TimeSpan.FromSeconds(15), Ct).GetAwaiter().GetResult();
            Assert.That(ta, Is.Not.Null);
            Assert.That(ta!.Opponent.PlayerId, Is.EqualTo("bbb"));
            Assert.That(ta.Opponent.Rating, Is.EqualTo(45));
            Assert.That(ta.MatchId, Does.StartWith("aaa-bbb-"));

            // bbb reçoit la proposition de aaa (relayée par le serveur) : il accepte et obtient le même duel.
            hub.OnBroadcast = null;
            var matchFrame = sockA.Sent.First(x => x.Contains("\"t\":\"match\""));
            sockB.Inbox.Clear();
            sockB.Inbox.Enqueue(matchFrame);
            var tb = b.JoinQueueAsync("christmas:2026", Snap(45), TimeSpan.FromSeconds(15), Ct).GetAwaiter().GetResult();
            Assert.That(tb, Is.Not.Null);
            Assert.That((tb!.MatchId, tb.Seed, tb.Opponent.PlayerId), Is.EqualTo((ta.MatchId, ta.Seed, "aaa")));

            sockA.Inbox.Clear();
            sockB.Inbox.Clear();
            a.SendAsync(DuelMessage.OfCommand(12, CombatCommand.UseSkill(1, 2, 5)), Ct).GetAwaiter().GetResult();
            var got = b.ReceiveAsync(TimeSpan.FromSeconds(5), Ct).GetAwaiter().GetResult();
            Assert.That(got!.Type, Is.EqualTo(DuelMessageType.Command));
            Assert.That((got.Tick, got.Command!.Type, got.Command.Actor, got.Command.SlotIndex, got.Command.Target), Is.EqualTo((12, CommandType.Skill, 1, 2, 5)));
            b.SendAsync(DuelMessage.OfHash(20, 9876543210UL), Ct).GetAwaiter().GetResult();
            Assert.That(a.ReceiveAsync(TimeSpan.FromSeconds(5), Ct).GetAwaiter().GetResult()!.Hash, Is.EqualTo(9876543210UL));
            b.SendAsync(DuelMessage.Leave(), Ct).GetAwaiter().GetResult();
            Assert.That(a.ReceiveAsync(TimeSpan.FromSeconds(5), Ct).GetAwaiter().GetResult()!.Type, Is.EqualTo(DuelMessageType.Leave));
        }

        [Test, Description("Vérifie que le silence de l'adversaire rend « rien » au bout du délai demandé et que les messages d'un autre duel du même canal sont ignorés — parce que le service déclare l'adversaire parti après 45 secondes")]
        public void Le_silence_de_l_adversaire_rend_nul_apres_le_delai()
        {
            var hub = new Hub();
            var a = Channel(hub, "aaa", out var sockA);
            ScriptAckFromBbb(hub, sockA);
            Assert.That(a.JoinQueueAsync("christmas:2026", Snap(40), TimeSpan.FromSeconds(15), Ct).GetAwaiter().GetResult(), Is.Not.Null);
            hub.OnBroadcast = null;
            sockA.Inbox.Clear();
            sockA.Inbox.Enqueue(Frame("{\"t\":\"leave\",\"from\":\"ccc\",\"to\":\"aaa\",\"match\":\"autre-duel\"}"));
            var start = hub.Now;
            Assert.That(a.ReceiveAsync(TimeSpan.FromSeconds(45), Ct).GetAwaiter().GetResult(), Is.Null);
            Assert.That(hub.Now - start, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(44)));
        }
    }
}
