#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Profils en mémoire : les tests PlayMode ne touchent jamais la vraie sauvegarde du poste.</summary>
    public sealed class MemoryProfileGateway : IProfileGateway
    {
        public readonly Dictionary<string, ProfileData> Store = new Dictionary<string, ProfileData>();
        public int Saves;

        public UniTask<ProfileData?> LoadAsync(string playerId, CancellationToken ct)
            => UniTask.FromResult(Store.TryGetValue(playerId, out var p) ? p : (ProfileData?)null);

        public UniTask SaveAsync(string playerId, ProfileData data, CancellationToken ct)
        {
            Store[playerId] = data;
            Saves++;
            return UniTask.CompletedTask;
        }

        public void Delete(string playerId) => Store.Remove(playerId);
    }

    /// <summary>Connexion invité instantanée, identifiant fixe, compteur d'appels.</summary>
    public sealed class InstantAuthGateway : IAuthGateway
    {
        public const string PlayerId = "playmode-test";
        public int Calls;
        /// <summary>Latence simulée en images (0 = réponse immédiate).</summary>
        public int DelayFrames;

        public async UniTask<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct)
        {
            Calls++;
            if (DelayFrames > 0) await UniTask.DelayFrame(DelayFrames, cancellationToken: ct);
            return Result<PlayerAccount>.Ok(new PlayerAccount(PlayerId, "Invité-TEST"));
        }

        public void SignOut() { }
    }

    /// <summary>Première connexion ignorée : les tests de bout en bout passent directement au QG.</summary>
    public sealed class SkippedOnboarding : N29.HeroesDawn.Presentation.Login.IOnboardingFlow
    {
        public UniTask RunIfNeededAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
