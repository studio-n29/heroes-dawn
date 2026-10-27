using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Infrastructure.Supabase;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Client HTTP factice : enregistre les requêtes et rend des réponses préparées (file d'attente, ou fonction).</summary>
    public sealed class FakeSupabaseHttp : ISupabaseHttp
    {
        public sealed class Request
        {
            public string Method = "", Url = "", Body = "";
            public Dictionary<string, string> Headers = new Dictionary<string, string>();
        }

        readonly Queue<HttpResult> _queue = new Queue<HttpResult>();
        public readonly List<Request> Requests = new List<Request>();
        /// <summary>Fonction de réponse prioritaire sur la file (null : on utilise la file).</summary>
        public Func<Request, HttpResult?>? Responder;
        /// <summary>Bloque l'envoi suivant jusqu'à <see cref="Release"/> (pour tester les appels simultanés).</summary>
        public UniTaskCompletionSource? Gate;

        public void Enqueue(int status, string body = "{}") => _queue.Enqueue(new HttpResult(status, body, false));
        public void EnqueueNetworkError() => _queue.Enqueue(new HttpResult(0, "", true));
        public void Release() => Gate?.TrySetResult();

        public async UniTask<HttpResult> SendAsync(string method, string url, IReadOnlyDictionary<string, string> headers, string? body, CancellationToken ct)
        {
            var r = new Request { Method = method, Url = url, Body = body ?? "", Headers = new Dictionary<string, string>(headers) };
            Requests.Add(r);
            if (Gate != null) await Gate.Task;
            var custom = Responder?.Invoke(r);
            if (custom != null) return custom;
            return _queue.Count > 0 ? _queue.Dequeue() : new HttpResult(0, "", true);
        }
    }

    public sealed class FixedClock : IClock
    {
        public DateTime UtcNow { get; set; } = new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);
        public DateTime LocalNow => UtcNow;
        public void Advance(TimeSpan by) => UtcNow += by;
    }

    /// <summary>Journal espion : garde toutes les lignes pour vérifier qu'aucun jeton n'y passe.</summary>
    public sealed class SpyLogger : ILogger
    {
        public readonly List<string> Lines = new List<string>();
        public void Info(string message) => Lines.Add(message);
        public void Warn(string message) => Lines.Add(message);
        public void Error(string message) => Lines.Add(message);
    }
}
