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
        sealed class Entry { public string Id = ""; public string Password = ""; public string Name = ""; public string Email = ""; public AuthProvider Provider = AuthProvider.Email; }

        readonly Dictionary<string, Entry> _accounts = new Dictionary<string, Entry>();
        readonly int _latencyMs;
        PlayerAccount? _session;

        public LocalAccountGateway(int latencyMs = 400) { _latencyMs = latencyMs; }

        public async UniTask<Result<PlayerAccount>> SignUpAsync(string email, string password, string displayName, CancellationToken ct)
        {
            if (_latencyMs > 0) await UniTask.Delay(_latencyMs, cancellationToken: ct);
            var key = Normalize(email);
            if (key.Length == 0 || !key.Contains("@")) return Result<PlayerAccount>.Fail(new DomainError("account.email", "errors.account_email"));
            if (password.Length < 8) return Result<PlayerAccount>.Fail(new DomainError("account.password", "errors.account_password"));
            if (_accounts.ContainsKey(key)) return Result<PlayerAccount>.Fail(new DomainError("account.exists", "errors.account_exists"));
            var entry = new Entry { Id = "acc-" + Guid.NewGuid().ToString("N").Substring(0, 12), Password = password, Name = displayName, Email = key };
            _accounts[key] = entry;
            var account = new PlayerAccount(entry.Id, entry.Name, key, AuthProvider.Email);
            _session = account;
            return Result<PlayerAccount>.Ok(account);
        }

        public async UniTask<Result<PlayerAccount>> SignInAsync(string email, string password, CancellationToken ct)
        {
            if (_latencyMs > 0) await UniTask.Delay(_latencyMs, cancellationToken: ct);
            if (!_accounts.TryGetValue(Normalize(email), out var entry) || entry.Password != password)
                return Result<PlayerAccount>.Fail(new DomainError("account.credentials", "errors.account_credentials"));
            var account = new PlayerAccount(entry.Id, entry.Name, entry.Email, entry.Provider);
            _session = account;
            return Result<PlayerAccount>.Ok(account);
        }

        public async UniTask<Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct)
        {
            if (_latencyMs > 0) await UniTask.Delay(_latencyMs, cancellationToken: ct);
            string key = "provider:" + provider;
            if (!_accounts.TryGetValue(key, out var entry))
            {
                entry = new Entry { Id = "acc-" + Guid.NewGuid().ToString("N").Substring(0, 12), Name = provider + "-joueur", Email = provider.ToString().ToLowerInvariant() + "@exemple.test", Provider = provider };
                _accounts[key] = entry;
            }
            var account = new PlayerAccount(entry.Id, entry.Name, entry.Email, provider);
            _session = account;
            return Result<PlayerAccount>.Ok(account);
        }

        public UniTask<Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct) => UniTask.FromResult(Result<PlayerAccount?>.Ok(_session));

        public UniTask<Result> SignOutAsync(CancellationToken ct)
        {
            _session = null;
            return UniTask.FromResult(Result.Ok());
        }

        public async UniTask<Result> ResetPasswordAsync(string email, CancellationToken ct)
        {
            if (_latencyMs > 0) await UniTask.Delay(_latencyMs, cancellationToken: ct);
            var key = Normalize(email);
            if (key.Length == 0 || !key.Contains("@")) return Result.Fail(new DomainError("account.email", "errors.account_email"));
            return Result.Ok();
        }

        static string Normalize(string email) => (email ?? "").Trim().ToLowerInvariant();
    }
}
