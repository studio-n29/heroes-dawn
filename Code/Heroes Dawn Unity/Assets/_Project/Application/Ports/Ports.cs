#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>Fournit les graines aléatoires (M1 : locales ; M2 : Cloud Code).</summary>
    public interface ISeedProvider
    {
        ulong NextSeed(string purpose);
    }

    /// <summary>Heure courante (injectable pour les tests).</summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }

    public sealed class PlayerAccount
    {
        public string PlayerId { get; }
        public string DisplayName { get; }
        public PlayerAccount(string playerId, string displayName) { PlayerId = playerId; DisplayName = displayName; }
    }

    /// <summary>Authentification (M1 : simulée ; M2 : UGS Authentication).</summary>
    public interface IAuthGateway
    {
        Task<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct);
        void SignOut();
    }

    [Serializable]
    public sealed class MemberSave
    {
        public string id = "";
        public int level = 1;
        public int fatigue;
        public bool inSquad;
    }

    /// <summary>Profil sauvegardé (M1 : fichier local ; M2 : Cloud Save + Economy).</summary>
    [Serializable]
    public sealed class ProfileData
    {
        public int schemaVersion = 2;
        public int credits;
        public int pr;
        public List<string> completedZones = new List<string>();
        public List<MemberSave> team = new List<MemberSave>();
        public long lastSeenUtcTicks;
        public bool autoCombat;
    }

    public interface IProfileGateway
    {
        Task<ProfileData?> LoadAsync(string playerId, CancellationToken ct);
        Task SaveAsync(string playerId, ProfileData data, CancellationToken ct);
        void Delete(string playerId);
    }

    /// <summary>Vérification serveur d'un combat (M1 : rejeu local ; M2 : Cloud Code).</summary>
    public interface ICombatVerifier
    {
        UniTask<bool> VerifyAsync(CombatSetup setup, IReadOnlyList<CombatInput> inputs, ulong expectedHash, CancellationToken ct);
    }
}
