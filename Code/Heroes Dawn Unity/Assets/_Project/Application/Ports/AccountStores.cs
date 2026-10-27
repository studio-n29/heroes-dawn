#nullable enable
using System;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>
    /// Ce que cet appareil sait de la synchronisation avec le compte : pour quel joueur, et quelle version du profil en ligne (date d'écriture du serveur)
    /// il a vue en dernier. Sert à reconnaître « seul cet appareil a avancé » (envoi simple) et de garde-fou à l'écriture conditionnelle.
    /// </summary>
    public sealed class CloudSyncState
    {
        public string UserId = "";
        public string LastUpdatedAt = "";
        public long LastSyncUtcTicks;
    }

    public interface ICloudSyncStateStore
    {
        CloudSyncState? Load();
        void Save(CloudSyncState state);
        void Clear();
    }

    /// <summary>
    /// Sauvegardes de secours : avant d'écraser la progression locale ou celle du compte, l'autre version est gardée ici (quelques-unes par raison).
    /// </summary>
    public interface IProgressBackupStore
    {
        /// <param name="reason"><c>local</c> (profil de l'appareil écrasé) ou <c>cloud</c> (profil du compte écrasé).</param>
        void Save(string reason, ProfileData profile, DateTime utcNow);
    }
}
