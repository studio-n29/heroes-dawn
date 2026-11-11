#nullable enable
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>Connexion WebSocket texte (protocole Phoenix de Supabase Realtime) ; un faux en test.</summary>
    public interface IRealtimeSocket
    {
        UniTask ConnectAsync(string url, CancellationToken ct);
        UniTask SendAsync(string text, CancellationToken ct);
        /// <summary>Le prochain message texte, nul si rien n'arrive pendant le délai.</summary>
        UniTask<string?> ReceiveAsync(TimeSpan timeout, CancellationToken ct);
        UniTask CloseAsync();
    }

    /// <summary>Socket réel (System.Net.WebSockets).</summary>
    public sealed class ClientWebSocketRealtime : IRealtimeSocket
    {
        ClientWebSocket? _ws;
        readonly byte[] _buffer = new byte[16384];

        public async UniTask ConnectAsync(string url, CancellationToken ct)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(url), ct);
        }

        public async UniTask SendAsync(string text, CancellationToken ct)
        {
            if (_ws == null) throw new InvalidOperationException("socket fermé");
            await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)), WebSocketMessageType.Text, true, ct);
        }

        public async UniTask<string?> ReceiveAsync(TimeSpan timeout, CancellationToken ct)
        {
            if (_ws == null) return null;
            using var timer = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timer.CancelAfter(timeout);
            var sb = new StringBuilder();
            try
            {
                WebSocketReceiveResult r;
                do
                {
                    r = await _ws.ReceiveAsync(new ArraySegment<byte>(_buffer), timer.Token);
                    if (r.MessageType == WebSocketMessageType.Close) return null;
                    sb.Append(Encoding.UTF8.GetString(_buffer, 0, r.Count));
                } while (!r.EndOfMessage);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return null; }
            return sb.ToString();
        }

        public async UniTask CloseAsync()
        {
            var ws = _ws;
            _ws = null;
            if (ws == null) return;
            try { if (ws.State == WebSocketState.Open) await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); }
            catch (Exception) { /* déjà fermé */ }
            ws.Dispose();
        }
    }

    /// <summary>
    /// Duel en ligne sur Supabase Realtime (broadcast seulement, aucune table) : canal <c>arena:&lt;édition&gt;:&lt;tranche de score&gt;</c>.
    /// File d'attente : chaque joueur en attente se signale toutes les 2 s ; le plus petit identifiant de deux joueurs en vue propose le duel
    /// (graine et identifiant dérivés des deux identifiants), l'autre répond avec sa capture. Les messages de jeu portent l'identifiant du duel
    /// et le destinataire ; ceux des autres duels du canal sont ignorés.
    /// </summary>
    public sealed class SupabaseDuelChannel : IDuelChannel
    {
        /// <summary>Largeur d'une tranche de score pour la file : deux joueurs de tranches voisines ne se voient pas.</summary>
        public const int RatingBand = 200;
        public const int HelloSeconds = 2;
        public const int HeartbeatSeconds = 25;

        [Serializable] sealed class Msg
        {
            public string t = "";       // hello, match, ack, cmd, hash, leave
            public string to = "";
            public string from = "";
            public string match = "";
            public long seed;
            public int rating;
            public int tick;
            public int ctype;           // CommandType
            public int actor, slot, target;
            public string hash = "";
            public string snap = "";    // capture JSON (match et ack)
        }
        [Serializable] sealed class Inner { public string type = ""; public string @event = ""; public Msg payload = new Msg(); }
        [Serializable] sealed class Frame { public string topic = ""; public string @event = ""; public Inner payload = new Inner(); }

        readonly SupabaseConfig _config;
        readonly SupabaseAccountGateway _accounts;
        readonly Func<IRealtimeSocket> _socketFactory;
        readonly Func<DateTime> _now;
        readonly Action<TimeSpan>? _advance;
        IRealtimeSocket? _socket;
        string _topic = "";
        string _me = "";
        string _matchId = "";
        string _peer = "";
        int _ref;
        DateTime _lastHeartbeat;

        /// <param name="advance">Pour les tests : fait avancer l'horloge injectée quand l'attente d'un message échoue.</param>
        public SupabaseDuelChannel(SupabaseConfig config, SupabaseAccountGateway accounts, Func<IRealtimeSocket> socketFactory, Func<DateTime> now, Action<TimeSpan>? advance = null)
        {
            _config = config; _accounts = accounts; _socketFactory = socketFactory; _now = now; _advance = advance;
        }

        string Url => _config.Url.Replace("https://", "wss://") + "/realtime/v1/websocket?apikey=" + Uri.EscapeDataString(_config.AnonKey) + "&vsn=1.0.0";

        string Next() => (++_ref).ToString();

        async UniTask WriteAsync(string topic, string ev, string payloadJson, CancellationToken ct)
            => await _socket!.SendAsync("{\"topic\":\"" + topic + "\",\"event\":\"" + ev + "\",\"payload\":" + payloadJson + ",\"ref\":\"" + Next() + "\"}", ct);

        async UniTask BroadcastAsync(Msg m, CancellationToken ct)
            => await WriteAsync(_topic, "broadcast", "{\"type\":\"broadcast\",\"event\":\"arena\",\"payload\":" + JsonUtility.ToJson(m) + "}", ct);

        public async UniTask<DuelTicket?> JoinQueueAsync(string tournamentId, DefenseSnapshot me, TimeSpan timeout, CancellationToken ct)
        {
            string? uid = _accounts.UserId;
            if (string.IsNullOrEmpty(uid)) throw new InvalidOperationException("session absente");
            var token = await _accounts.AccessTokenAsync(ct);
            if (token.IsFailure) throw new InvalidOperationException("jeton indisponible");
            _me = uid!;
            me.PlayerId = _me;
            _topic = "realtime:arena:" + tournamentId + ":" + Math.Max(0, me.Rating) / RatingBand;
            _socket = _socketFactory();
            await _socket.ConnectAsync(Url, ct);
            await WriteAsync(_topic, "phx_join", "{\"config\":{\"broadcast\":{\"self\":false,\"ack\":false},\"presence\":{\"key\":\"" + _me + "\"},\"private\":false},\"access_token\":\"" + token.Value + "\"}", ct);
            _lastHeartbeat = _now();

            var deadline = _now() + timeout;
            var nextHello = _now();
            var seen = new Dictionary<string, DateTime>();
            string snap = JsonUtility.ToJson(me);
            while (_now() < deadline)
            {
                if (_now() >= nextHello)
                {
                    await BroadcastAsync(new Msg { t = "hello", from = _me, rating = me.Rating }, ct);
                    nextHello = _now() + TimeSpan.FromSeconds(HelloSeconds);
                }
                var wait = TimeSpan.FromSeconds(1);
                var m = await ReadAsync(wait, ct);
                if (m == null) continue;
                if (m.t == "hello" && m.from != _me)
                {
                    seen[m.from] = _now();
                    if (string.CompareOrdinal(_me, m.from) < 0)   // le plus petit identifiant propose le duel
                    {
                        string match = _me + "-" + m.from + "-" + _now().Ticks;
                        await BroadcastAsync(new Msg { t = "match", from = _me, to = m.from, match = match, seed = SeedOf(match), snap = snap }, ct);
                        _matchId = match; _peer = m.from;
                    }
                }
                else if (m.t == "match" && m.to == _me)
                {
                    _matchId = m.match; _peer = m.from;
                    await BroadcastAsync(new Msg { t = "ack", from = _me, to = m.from, match = m.match, snap = snap }, ct);
                    return Ticket(m.match, m.seed, m.snap);
                }
                else if (m.t == "ack" && m.to == _me && m.match == _matchId)
                {
                    return Ticket(m.match, SeedOf(m.match), m.snap);
                }
            }
            await CloseSocketAsync();
            return null;
        }

        static long SeedOf(string match)
        {
            var h = new Fnv1a64();
            h.Add(match);
            return (long)(h.Value & 0x7fffffffffffffffUL);
        }

        DuelTicket Ticket(string match, long seed, string snapJson)
        {
            var opponent = JsonUtility.FromJson<DefenseSnapshot>(snapJson) ?? new DefenseSnapshot();
            opponent.PlayerId = _peer;
            return new DuelTicket { MatchId = match, Seed = (ulong)seed, Opponent = opponent };
        }

        public async UniTask SendAsync(DuelMessage m, CancellationToken ct)
        {
            if (_socket == null) return;
            var msg = new Msg { from = _me, to = _peer, match = _matchId, tick = m.Tick };
            switch (m.Type)
            {
                case DuelMessageType.Command:
                    if (m.Command == null) return;
                    msg.t = "cmd"; msg.ctype = (int)m.Command.Type; msg.actor = m.Command.Actor; msg.slot = m.Command.SlotIndex; msg.target = m.Command.Target;
                    break;
                case DuelMessageType.Hash: msg.t = "hash"; msg.hash = m.Hash.ToString(); break;
                default: msg.t = "leave"; break;
            }
            await BroadcastAsync(msg, ct);
        }

        public async UniTask<DuelMessage?> ReceiveAsync(TimeSpan timeout, CancellationToken ct)
        {
            if (_socket == null) return null;
            var deadline = _now() + timeout;
            while (_now() < deadline)
            {
                var left = deadline - _now();
                var m = await ReadAsync(left > TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : left, ct);
                if (m == null || m.to != _me || m.match != _matchId) continue;
                switch (m.t)
                {
                    case "cmd":
                        var cmd = (CommandType)m.ctype == CommandType.Pass ? CombatCommand.Pass(m.actor)
                                : (CommandType)m.ctype == CommandType.Skill ? CombatCommand.UseSkill(m.actor, m.slot, m.target) : null;
                        if (cmd != null) return DuelMessage.OfCommand(m.tick, cmd);
                        break;
                    case "hash":
                        if (ulong.TryParse(m.hash, out var h)) return DuelMessage.OfHash(m.tick, h);
                        break;
                    case "leave": return DuelMessage.Leave();
                }
            }
            return null;
        }

        public UniTask LeaveAsync(CancellationToken ct) => CloseSocketAsync();

        async UniTask CloseSocketAsync()
        {
            var s = _socket;
            _socket = null;
            if (s != null) await s.CloseAsync();
        }

        /// <summary>Lit un message de diffusion ; envoie le battement de cœur en route. Les autres trames (réponses, présence) sont ignorées.</summary>
        async UniTask<Msg?> ReadAsync(TimeSpan wait, CancellationToken ct)
        {
            if (_socket == null) return null;
            if (_now() - _lastHeartbeat >= TimeSpan.FromSeconds(HeartbeatSeconds))
            {
                await WriteAsync("phoenix", "heartbeat", "{}", ct);
                _lastHeartbeat = _now();
            }
            var text = await _socket.ReceiveAsync(wait, ct);
            if (text == null) { _advance?.Invoke(wait); return null; }
            try
            {
                var frame = JsonUtility.FromJson<Frame>(text);
                if (frame == null || frame.@event != "broadcast" || frame.payload.@event != "arena") return null;
                return frame.payload.payload;
            }
            catch (Exception) { return null; }
        }
    }
}
