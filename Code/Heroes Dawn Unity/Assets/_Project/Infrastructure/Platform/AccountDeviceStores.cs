#nullable enable
using System;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Platform
{
    /// <summary>État de synchronisation sur l'appareil (PlayerPrefs).</summary>
    public sealed class PlayerPrefsSyncStateStore : ICloudSyncStateStore
    {
        const string Key = "hd.cloud.sync_state";

        public CloudSyncState? Load()
        {
            var json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonUtility.FromJson<CloudSyncState>(json); }
            catch (Exception) { return null; }
        }

        public void Save(CloudSyncState state)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Sauvegardes de secours dans <c>persistentDataPath/backups</c> : un fichier JSON par sauvegarde, les cinq plus récentes de chaque raison sont gardées.
    /// Une panne d'écriture n'empêche pas le choix du joueur (elle est signalée par l'exception, à l'appelant de décider).
    /// </summary>
    public sealed class FileProgressBackupStore : IProgressBackupStore
    {
        const int Keep = 5;
        readonly string _folder;

        public FileProgressBackupStore(string? folder = null) { _folder = folder ?? Path.Combine(UnityEngine.Application.persistentDataPath, "backups"); }

        public void Save(string reason, ProfileData profile, DateTime utcNow)
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(Path.Combine(_folder, reason + "_" + utcNow.Ticks + ".json"), JsonUtility.ToJson(profile));
            foreach (var old in Directory.GetFiles(_folder, reason + "_*.json").OrderByDescending(f => f, StringComparer.Ordinal).Skip(Keep))
                File.Delete(old);
        }
    }
}
