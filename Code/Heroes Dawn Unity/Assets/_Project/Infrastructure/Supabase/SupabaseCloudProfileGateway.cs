#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>
    /// Sauvegarde du profil dans la table <c>hd_profiles</c> (PostgREST), avec le jeton du joueur : la sécurité par ligne ne lui laisse que sa ligne.
    /// L'écriture est conditionnelle : <c>PATCH … &amp;updated_at=eq.&lt;date lue&gt;</c> ne touche aucune ligne si le serveur a une version plus récente (conflit).
    /// </summary>
    public sealed class SupabaseCloudProfileGateway : ICloudProfileGateway
    {
        /// <summary>Taille maximale du profil (octets) : la même que la contrainte de la base.</summary>
        public const int MaxProfileBytes = 524288;

        [Serializable] sealed class RowDto { public ProfileData profile = new ProfileData(); public int schema_version; public string updated_at = ""; }
        [Serializable] sealed class RowsDto { public RowDto[] rows = new RowDto[0]; }

        readonly SupabaseConfig _config;
        readonly ISupabaseHttp _http;
        readonly SupabaseAccountGateway _accounts;

        public SupabaseCloudProfileGateway(SupabaseConfig config, ISupabaseHttp http, SupabaseAccountGateway accounts)
        {
            _config = config; _http = http; _accounts = accounts;
        }

        static DomainError CloudError() => new DomainError("cloud.unavailable", "errors.account_cloud");

        public async UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct)
        {
            var token = await _accounts.AccessTokenAsync(ct);
            if (token.IsFailure) return Result<CloudProfile?>.Fail(token.Error!);
            var r = await _http.SendAsync("GET", _config.Url + "/rest/v1/hd_profiles?select=profile,schema_version,updated_at&limit=1", Headers(token.Value), null, ct);
            if (!r.IsSuccess) return Result<CloudProfile?>.Fail(Failure(r));
            var rows = ParseRows(r.Body);
            if (rows == null) return Result<CloudProfile?>.Fail(CloudError());
            if (rows.Length == 0) return Result<CloudProfile?>.Ok(null);
            var row = rows[0];
            if (string.IsNullOrEmpty(row.updated_at)) return Result<CloudProfile?>.Fail(CloudError());
            return Result<CloudProfile?>.Ok(new CloudProfile(row.profile, row.updated_at));
        }

        public async UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expectedUpdatedAt, CancellationToken ct)
        {
            string profileJson = JsonUtility.ToJson(profile);
            if (Encoding.UTF8.GetByteCount(profileJson) > MaxProfileBytes) return Result<CloudWriteResult>.Fail(CloudError());
            var token = await _accounts.AccessTokenAsync(ct);
            if (token.IsFailure) return Result<CloudWriteResult>.Fail(token.Error!);
            string? userId = _accounts.UserId;
            if (string.IsNullOrEmpty(userId)) return Result<CloudWriteResult>.Fail(SupabaseErrors.Session());

            var headers = Headers(token.Value);
            headers["Prefer"] = "return=representation";
            HttpResult r;
            if (expectedUpdatedAt == null)
            {
                string body = "{\"user_id\":\"" + userId + "\",\"profile\":" + profileJson + ",\"schema_version\":" + profile.schemaVersion + "}";
                r = await _http.SendAsync("POST", _config.Url + "/rest/v1/hd_profiles", headers, body, ct);
                if (!r.NetworkError && r.Status == 409) return Result<CloudWriteResult>.Ok(new CloudWriteResult(CloudWriteStatus.Conflict, ""));
            }
            else
            {
                string url = _config.Url + "/rest/v1/hd_profiles?user_id=eq." + userId + "&updated_at=eq." + Uri.EscapeDataString(expectedUpdatedAt);
                string body = "{\"profile\":" + profileJson + ",\"schema_version\":" + profile.schemaVersion + "}";
                r = await _http.SendAsync("PATCH", url, headers, body, ct);
            }
            if (!r.IsSuccess) return Result<CloudWriteResult>.Fail(Failure(r));
            var rows = ParseRows(r.Body);
            if (rows == null) return Result<CloudWriteResult>.Fail(CloudError());
            if (rows.Length == 0) return Result<CloudWriteResult>.Ok(new CloudWriteResult(CloudWriteStatus.Conflict, ""));   // aucune ligne touchée : la date a changé
            if (string.IsNullOrEmpty(rows[0].updated_at)) return Result<CloudWriteResult>.Fail(CloudError());
            return Result<CloudWriteResult>.Ok(new CloudWriteResult(CloudWriteStatus.Ok, rows[0].updated_at));
        }

        Dictionary<string, string> Headers(string accessToken) => new Dictionary<string, string>
        {
            ["apikey"] = _config.AnonKey,
            ["Authorization"] = "Bearer " + accessToken,
            ["Content-Type"] = "application/json",
        };

        static DomainError Failure(HttpResult r) => !r.NetworkError && (r.Status == 401 || r.Status == 403) ? SupabaseErrors.Session() : CloudError();

        /// <summary>Les lignes d'une réponse PostgREST (un tableau JSON), ou <c>null</c> si le corps n'en est pas un.</summary>
        static RowDto[]? ParseRows(string body)
        {
            string trimmed = (body ?? "").Trim();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal)) return null;
            try { return JsonUtility.FromJson<RowsDto>("{\"rows\":" + trimmed + "}")?.rows; }
            catch (Exception) { return null; }
        }
    }
}
