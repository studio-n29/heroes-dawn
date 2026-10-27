#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>Le profil d'un joueur tel qu'il est stocké en ligne, avec la date de dernière écriture donnée par le serveur (texte, gardé tel quel).</summary>
    public sealed class CloudProfile
    {
        public ProfileData Profile { get; }
        public string UpdatedAt { get; }
        public CloudProfile(ProfileData profile, string updatedAt) { Profile = profile; UpdatedAt = updatedAt; }
    }

    public enum CloudWriteStatus { Ok, Conflict }

    public sealed class CloudWriteResult
    {
        public CloudWriteStatus Status { get; }
        /// <summary>Nouvelle date de dernière écriture (vide en cas de conflit).</summary>
        public string UpdatedAt { get; }
        public CloudWriteResult(CloudWriteStatus status, string updatedAt) { Status = status; UpdatedAt = updatedAt; }
    }

    /// <summary>
    /// Sauvegarde en ligne du profil du joueur connecté (une ligne par joueur). L'écriture est conditionnelle : elle n'a lieu que si la ligne
    /// est toujours celle que le client a lue (<c>expectedUpdatedAt</c>), sinon c'est un conflit et rien n'est écrasé.
    /// </summary>
    public interface ICloudProfileGateway
    {
        /// <summary>Le profil en ligne, ou <c>null</c> (succès) quand le joueur n'en a pas encore.</summary>
        UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct);

        /// <summary>Écrit le profil. <c>expectedUpdatedAt</c> nul : première écriture (conflit si une ligne existe déjà).</summary>
        UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expectedUpdatedAt, CancellationToken ct);
    }
}
