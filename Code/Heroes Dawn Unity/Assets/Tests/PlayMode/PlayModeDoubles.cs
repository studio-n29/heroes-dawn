#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Profils en mémoire : les tests PlayMode ne touchent jamais la vraie sauvegarde du poste.</summary>
    public sealed class MemoryProfileGateway : IProfileGateway
    {
        public readonly Dictionary<string, ProfileData> Store = new Dictionary<string, ProfileData>();
        public int Saves;

        public Task<ProfileData?> LoadAsync(string playerId, CancellationToken ct)
            => Task.FromResult(Store.TryGetValue(playerId, out var p) ? p : (ProfileData?)null);

        public Task SaveAsync(string playerId, ProfileData data, CancellationToken ct)
        {
            Store[playerId] = data;
            Saves++;
            return Task.CompletedTask;
        }

        public void Delete(string playerId) => Store.Remove(playerId);
    }

    /// <summary>Connexion invité instantanée, identifiant fixe, compteur d'appels.</summary>
    public sealed class InstantAuthGateway : IAuthGateway
    {
        public const string PlayerId = "playmode-test";
        public int Calls;

        public Task<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct)
        {
            Calls++;
            return Task.FromResult(Result<PlayerAccount>.Ok(new PlayerAccount(PlayerId, "Invité-TEST")));
        }

        public void SignOut() { }
    }
}
