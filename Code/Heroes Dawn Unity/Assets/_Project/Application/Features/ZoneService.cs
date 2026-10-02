#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Core;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features
{
    /// <summary>Reste de l'ancien service de zone, utilisé par <see cref="CombatService"/> (supprimé à la tâche 4).</summary>
    public sealed class ZoneService
    {
        readonly ZoneEffects _fx;
        readonly ILeaveZoneUseCase _leave;

        public ZoneService(ZoneEffects fx, ILeaveZoneUseCase leave) { _fx = fx; _leave = leave; }

        internal Result<IReadOnlyList<BoardEvent>> ResolveCombat(bool victory) => ZoneActions.Apply(_fx, r => r.ResolveCombat(victory));

        public void Abandon() => _leave.ExecuteAsync().Forget();
    }
}
