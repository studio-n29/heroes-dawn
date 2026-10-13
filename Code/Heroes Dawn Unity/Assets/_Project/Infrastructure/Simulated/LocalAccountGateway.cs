#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>
    /// Placeholder de Supabase Auth : comptes e-mail + mot de passe gardés en mémoire pour la session, latence simulée.
    /// À remplacer par un client Supabase (URL et clé du projet à fournir) sans toucher au reste du jeu.
    /// </summary>
    public sealed class LocalAccountGateway : IAccountGateway
    {
        sealed class Entry { public string Id = ""; public string Password = ""; public string Name = ""; }

        readonly Dictionary<string, Entry> _accounts = new Dictionary<string, Entry>();
        readonly int _latencyMs;

        public LocalAccountGateway(int latencyMs = 400) { _latencyMs = latencyMs; }

        public async UniTask<Result<PlayerAccount>> SignUpAsync(string email, string password, string displayName, CancellationToken ct)
        {
            if (_latencyMs > 0) await UniTask.Delay(_latencyMs, cancellationToken: ct);
            var key = Normalize(email);
            if (key.Length == 0 || !key.Contains("@")) return Result<PlayerAccount>.Fail(new DomainError("account.email", "errors.account_email"));
            if (password.Length < 8) return Result<PlayerAccount>.Fail(new DomainError("account.password", "errors.account_password"));
            if (_accounts.ContainsKey(key)) return Result<PlayerAccount>.Fail(new DomainError("account.exists", "errors.account_exists"));
            var entry = new Entry { Id = "acc-" + Guid.NewGuid().ToString("N").Substring(0, 12), Password = password, Name = displayName };
            _accounts[key] = entry;
            return Result<PlayerAccount>.Ok(new PlayerAccount(entry.Id, entry.Name));
        }

        public async UniTask<Result<PlayerAccount>> SignInAsync(string email, string password, CancellationToken ct)
        {
            if (_latencyMs > 0) await UniTask.Delay(_latencyMs, cancellationToken: ct);
            if (!_accounts.TryGetValue(Normalize(email), out var entry) || entry.Password != password)
                return Result<PlayerAccount>.Fail(new DomainError("account.credentials", "errors.account_credentials"));
            return Result<PlayerAccount>.Ok(new PlayerAccount(entry.Id, entry.Name));
        }

        static string Normalize(string email) => (email ?? "").Trim().ToLowerInvariant();
    }
}
