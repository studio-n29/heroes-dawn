#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Auth
{
    /// <summary>Connexion invité, chargement du profil, récupération passive de fatigue, sauvegarde.</summary>
    public interface ISignInAsGuestUseCase
    {
        UniTask<Result<SignInReport>> ExecuteAsync(CancellationToken ct = default);
    }
}
