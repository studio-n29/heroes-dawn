#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Placeholder du contrôle Cloud Code (M2) : rejoue le combat hors du thread principal et compare l'empreinte finale.
    /// </summary>
    public sealed class LocalCombatVerifier : ICombatVerifier
    {
        readonly ContentCatalog _content;
        readonly int _latencyMs;

        public LocalCombatVerifier(ContentCatalog content, int latencyMs = 300) { _content = content; _latencyMs = latencyMs; }

        public async UniTask<bool> VerifyAsync(CombatSetup setup, IReadOnlyList<CombatInput> inputs, ulong expectedHash, CancellationToken ct)
        {
            var copy = new List<CombatInput>(inputs);
            // Rejeu hors du thread principal, puis retour sur le thread principal (configureAwait par défaut).
            var replay = await UniTask.RunOnThreadPool(() => CombatReplay.Run(_content, setup, copy), cancellationToken: ct);
            await UniTask.Delay(_latencyMs, cancellationToken: ct);
            return replay.StateHash == expectedHash;
        }
    }
}
