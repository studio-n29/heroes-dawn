#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Auth
{
    /// <summary>Efface la sauvegarde du joueur courant et repart d'un profil neuf.</summary>
    public interface IResetProgressUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
