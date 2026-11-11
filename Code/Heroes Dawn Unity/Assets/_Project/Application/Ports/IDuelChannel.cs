#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Ports
{
    public enum DuelMessageType { Command, Hash, Leave }

    /// <summary>Ce qui s'échange pendant un duel en ligne : une commande (avec le tick), l'empreinte de l'état à un tick de contrôle, ou le départ.</summary>
    public sealed class DuelMessage
    {
        public DuelMessageType Type;
        public int Tick;
        public CombatCommand? Command;
        public ulong Hash;

        public static DuelMessage OfCommand(int tick, CombatCommand c) => new DuelMessage { Type = DuelMessageType.Command, Tick = tick, Command = c };
        public static DuelMessage OfHash(int tick, ulong hash) => new DuelMessage { Type = DuelMessageType.Hash, Tick = tick, Hash = hash };
        public static DuelMessage Leave() => new DuelMessage { Type = DuelMessageType.Leave };
    }

    /// <summary>Canal temps réel d'un duel en ligne (file d'attente puis échange de messages avec l'adversaire).</summary>
    public interface IDuelChannel
    {
        /// <summary>Entre dans la file ; renvoie le duel quand un adversaire est trouvé, nul si personne n'est venu avant le délai.</summary>
        UniTask<DuelTicket?> JoinQueueAsync(string tournamentId, DefenseSnapshot me, TimeSpan timeout, CancellationToken ct);
        UniTask SendAsync(DuelMessage m, CancellationToken ct);
        /// <summary>Le prochain message de l'adversaire, nul s'il reste muet pendant le délai.</summary>
        UniTask<DuelMessage?> ReceiveAsync(TimeSpan timeout, CancellationToken ct);
        UniTask LeaveAsync(CancellationToken ct);
    }
}
