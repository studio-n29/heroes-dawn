#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
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
        /// <summary>Heure du téléphone (jour de boutique, compte à rebours).</summary>
        DateTime LocalNow { get; }
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
        UniTask<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct);
        void SignOut();
    }

    [Serializable]
    public sealed class MemberSave
    {
        public string id = "";
        public int level = 1;
        public int fatigue;
        public bool inSquad;
        /// <summary>Objets equipes (au plus 2).</summary>
        public List<string> items = new List<string>();
        /// <summary>Cartes equipees : emplacement 3 puis 4 (chaine vide = pas de carte).</summary>
        public List<string> cards = new List<string>();
    }

    /// <summary>Profil sauvegardé (M1 : fichier local ; M2 : Cloud Save + Economy).</summary>
    [Serializable]
    public sealed class ProfileData
    {
        public int schemaVersion = 4;
        public int credits;
        public int pr;
        public List<string> completedZones = new List<string>();
        public List<MemberSave> team = new List<MemberSave>();
        public long lastSeenUtcTicks;
        public bool autoCombat;
        /// <summary>Boutique éphémère (v3). Absente en v2 : JsonUtility crée un bloc vide (jour "").</summary>
        public ShopSave shop = new ShopSave();
        /// <summary>Méta-progression (v4) : gemmes, pass, missions, opérations, labo, archives, réglages, tutoriel. Absente avant : valeurs par défaut.</summary>
        public MetaSave meta = new MetaSave();
    }

    /// <summary>Boutique du jour : offres mémorisées pour qu'un achat ou une relance ne relance pas le tirage.</summary>
    [Serializable]
    public sealed class ShopSave
    {
        public string day = "";
        public int draw;
        public string seenDay = "";
        public List<string> offers = new List<string>();
        public List<bool> recruited = new List<bool>();
    }

    public interface IProfileGateway
    {
        UniTask<ProfileData?> LoadAsync(string playerId, CancellationToken ct);
        UniTask SaveAsync(string playerId, ProfileData data, CancellationToken ct);
        void Delete(string playerId);
    }

    /// <summary>Vérification serveur d'un combat (M1 : rejeu local ; M2 : Cloud Code).</summary>
    public interface ICombatVerifier
    {
        UniTask<bool> VerifyAsync(CombatSetup setup, IReadOnlyList<CombatInput> inputs, ulong expectedHash, CancellationToken ct);
    }
}
