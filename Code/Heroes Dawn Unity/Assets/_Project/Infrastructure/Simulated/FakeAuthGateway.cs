#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Placeholder d'UGS Authentication (M2) : identifiant invité persistant dans PlayerPrefs, latence réseau simulée.
    /// </summary>
    public sealed class FakeAuthGateway : IAuthGateway
    {
        const string GuestKey = "hd.guest_id";
        readonly int _latencyMs;

        /// <summary>Force l'échec de la prochaine connexion (tests de l'écran d'erreur).</summary>
        public bool FailNextSignIn { get; set; }

        public FakeAuthGateway(int latencyMs = 700) { _latencyMs = latencyMs; }

        public async UniTask<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct)
        {
            await UniTask.Delay(_latencyMs, cancellationToken: ct);
            if (FailNextSignIn)
            {
                FailNextSignIn = false;
                return Result<PlayerAccount>.Fail(new DomainError("auth.unreachable", "errors.network"));
            }
            string id = PlayerPrefs.GetString(GuestKey, "");
            if (string.IsNullOrEmpty(id))
            {
                id = "guest-" + Guid.NewGuid().ToString("N").Substring(0, 12);
                PlayerPrefs.SetString(GuestKey, id);
                PlayerPrefs.Save();
            }
            string suffix = id.Substring(id.Length - 4).ToUpperInvariant();
            return Result<PlayerAccount>.Ok(new PlayerAccount(id, "Invité-" + suffix));
        }

        public void SignOut() { }
    }
}
