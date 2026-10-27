#nullable enable
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>Réponse HTTP : code, corps, ou erreur réseau (pas de réponse du tout).</summary>
    public sealed class HttpResult
    {
        public int Status { get; }
        public string Body { get; }
        public bool NetworkError { get; }
        public HttpResult(int status, string body, bool networkError) { Status = status; Body = body ?? ""; NetworkError = networkError; }
        public bool IsSuccess => !NetworkError && Status >= 200 && Status < 300;
    }

    /// <summary>Couche HTTP du client Supabase : <see cref="UnityWebRequestHttp"/> en jeu, faux client en test.</summary>
    public interface ISupabaseHttp
    {
        UniTask<HttpResult> SendAsync(string method, string url, IReadOnlyDictionary<string, string> headers, string? body, CancellationToken ct);
    }

    /// <summary>Client HTTP réel (UnityWebRequest). Une erreur de connexion devient <see cref="HttpResult.NetworkError"/>, jamais une exception.</summary>
    public sealed class UnityWebRequestHttp : ISupabaseHttp
    {
        readonly int _timeoutSeconds;
        public UnityWebRequestHttp(int timeoutSeconds = 20) { _timeoutSeconds = timeoutSeconds; }

        public async UniTask<HttpResult> SendAsync(string method, string url, IReadOnlyDictionary<string, string> headers, string? body, CancellationToken ct)
        {
            using var request = new UnityWebRequest(url, method);
            request.timeout = _timeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();
            if (body != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.uploadHandler.contentType = "application/json";
            }
            foreach (var kv in headers) request.SetRequestHeader(kv.Key, kv.Value);
            try { await request.SendWebRequest().WithCancellation(ct); }
            catch (UnityWebRequestException) { /* erreur HTTP ou réseau : lue ci-dessous */ }
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
                return new HttpResult(0, "", true);
            return new HttpResult((int)request.responseCode, request.downloadHandler?.text ?? "", false);
        }
    }
}
