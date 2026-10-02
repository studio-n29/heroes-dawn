#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Combat
{
    public sealed class VerifyCombatUseCase : IVerifyCombatUseCase
    {
        readonly ActiveCombat _active;
        readonly ICombatVerifier _verifier;
        readonly ILogger _log;

        public VerifyCombatUseCase(ActiveCombat active, ICombatVerifier verifier, ILogger log) { _active = active; _verifier = verifier; _log = log; }

        public async UniTask<bool?> ExecuteAsync(CancellationToken ct = default)
        {
            var sim = _active.Simulation;
            var setup = _active.Setup;
            if (sim == null || setup == null || _active.DebugUsed) return null;
            try
            {
                bool ok = await _verifier.VerifyAsync(setup, sim.InputLog, sim.StateHash(), ct);
                if (!ok) _log.Warn("Rejeu du combat divergent : le serveur aurait refusé ce résultat.");
                return ok;
            }
            catch (OperationCanceledException) { return null; }
            catch (Exception e)
            {
                _log.Error("Vérification du combat : " + e.Message);
                return null;
            }
        }
    }
}
