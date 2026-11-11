#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Faux canal temps réel (tests et éditeur) : l'adversaire est scripté. Aucune attente réelle : un délai écoulé est simulé en renvoyant « personne » ou « silence ».
    /// </summary>
    public sealed class InMemoryDuelChannel : IDuelChannel
    {
        /// <summary>Le duel trouvé par la file ; nul : personne n'est venu.</summary>
        public DuelTicket? Match;
        /// <summary>Messages de l'adversaire dans l'ordre ; quand la liste est vide, il est muet.</summary>
        public readonly Queue<DuelMessage> Incoming = new Queue<DuelMessage>();
        public readonly List<DuelMessage> Sent = new List<DuelMessage>();
        public TimeSpan? LastJoinTimeout;
        public TimeSpan? LastReceiveTimeout;
        public bool Left;
        public int Joins;
        /// <summary>Simule une connexion coupée pendant la file d'attente.</summary>
        public bool DropOnJoin;
        /// <summary>Appelé à chaque message envoyé : permet à un faux adversaire de répondre.</summary>
        public Action<DuelMessage>? OnSent;

        public UniTask<DuelTicket?> JoinQueueAsync(string tournamentId, DefenseSnapshot me, TimeSpan timeout, CancellationToken ct)
        {
            Joins++;
            LastJoinTimeout = timeout;
            if (DropOnJoin) throw new System.IO.IOException("connexion perdue");
            return UniTask.FromResult(Match);
        }

        public UniTask SendAsync(DuelMessage m, CancellationToken ct)
        {
            Sent.Add(m);
            OnSent?.Invoke(m);
            return UniTask.CompletedTask;
        }

        public UniTask<DuelMessage?> ReceiveAsync(TimeSpan timeout, CancellationToken ct)
        {
            LastReceiveTimeout = timeout;
            return UniTask.FromResult(Incoming.Count > 0 ? Incoming.Dequeue() : null);
        }

        public UniTask LeaveAsync(CancellationToken ct)
        {
            Left = true;
            return UniTask.CompletedTask;
        }
    }
}
