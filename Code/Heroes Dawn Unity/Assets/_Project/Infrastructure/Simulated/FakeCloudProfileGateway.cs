#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Sauvegarde en ligne simulée en mémoire (éditeur sans configuration Supabase, tests) : une seule ligne, date d'écriture = compteur croissant,
    /// profil copié à l'entrée et à la sortie pour qu'aucune référence ne soit partagée.
    /// </summary>
    public sealed class FakeCloudProfileGateway : ICloudProfileGateway
    {
        string? _json;
        long _version;

        /// <summary>Force l'échec de la prochaine opération (réseau indisponible).</summary>
        public bool FailNext { get; set; }
        public int Reads { get; private set; }
        public int Writes { get; private set; }
        public string UpdatedAt => _version == 0 ? "" : _version.ToString();

        public UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct)
        {
            Reads++;
            if (TakeFailure()) return UniTask.FromResult(Result<CloudProfile?>.Fail(new DomainError("cloud.unavailable", "errors.account_cloud")));
            if (_json == null) return UniTask.FromResult(Result<CloudProfile?>.Ok(null));
            return UniTask.FromResult(Result<CloudProfile?>.Ok(new CloudProfile(JsonUtility.FromJson<ProfileData>(_json), UpdatedAt)));
        }

        public UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expectedUpdatedAt, CancellationToken ct)
        {
            if (TakeFailure()) return UniTask.FromResult(Result<CloudWriteResult>.Fail(new DomainError("cloud.unavailable", "errors.account_cloud")));
            bool first = expectedUpdatedAt == null;
            bool conflict = first ? _json != null : _json == null || expectedUpdatedAt != UpdatedAt;
            if (conflict) return UniTask.FromResult(Result<CloudWriteResult>.Ok(new CloudWriteResult(CloudWriteStatus.Conflict, "")));
            _json = JsonUtility.ToJson(profile);
            _version++;
            Writes++;
            return UniTask.FromResult(Result<CloudWriteResult>.Ok(new CloudWriteResult(CloudWriteStatus.Ok, UpdatedAt)));
        }

        /// <summary>Pose directement un profil (simule une écriture venue d'un autre appareil).</summary>
        public void SeedFromOtherDevice(ProfileData profile)
        {
            _json = JsonUtility.ToJson(profile);
            _version++;
        }

        bool TakeFailure()
        {
            if (!FailNext) return false;
            FailNext = false;
            return true;
        }
    }
}
