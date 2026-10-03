#nullable enable
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Placeholder de Cloud Save (M2) : profil JSON dans persistentDataPath/profiles, latence simulée.
    /// </summary>
    public sealed class LocalProfileGateway : IProfileGateway
    {
        readonly string _dir;
        readonly int _latencyMs;

        public LocalProfileGateway(int latencyMs = 250)
        {
            _latencyMs = latencyMs;
            _dir = Path.Combine(UnityEngine.Application.persistentDataPath, "profiles");
        }

        string PathFor(string playerId) => Path.Combine(_dir, Sanitize(playerId) + ".json");

        static string Sanitize(string id)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) id = id.Replace(c, '_');
            return id;
        }

        public async UniTask<ProfileData?> LoadAsync(string playerId, CancellationToken ct)
        {
            await UniTask.Delay(_latencyMs, cancellationToken: ct);
            var path = PathFor(playerId);
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonUtility.FromJson<ProfileData>(json);
        }

        public async UniTask SaveAsync(string playerId, ProfileData data, CancellationToken ct)
        {
            await UniTask.Delay(_latencyMs / 2, cancellationToken: ct);
            Directory.CreateDirectory(_dir);
            var path = PathFor(playerId);
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        public void Delete(string playerId)
        {
            var path = PathFor(playerId);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
