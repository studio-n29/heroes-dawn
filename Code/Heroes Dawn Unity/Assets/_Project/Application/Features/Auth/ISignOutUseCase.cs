#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Auth
{
    /// <summary>Déconnexion : sauvegarde, puis oubli du compte et de la partie en cours.</summary>
    public interface ISignOutUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
