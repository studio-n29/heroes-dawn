#nullable enable
using System;
using N29.HeroesDawn.Application.Ports;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>Session Supabase mémorisée sur l'appareil : jetons, expiration et identité. Jamais envoyée au cloud ni écrite dans les journaux.</summary>
    [Serializable]
    public sealed class SupabaseSession
    {
        public string AccessToken = "";
        public string RefreshToken = "";
        public long ExpiresAtUnix;
        public string UserId = "";
        public string Email = "";
        public string DisplayName = "";
        public AuthProvider Provider;

        public PlayerAccount ToAccount() => new PlayerAccount(UserId, DisplayName, Email, Provider);
    }

    public interface ISupabaseTokenStore
    {
        SupabaseSession? Load();
        void Save(SupabaseSession session);
        void Clear();
    }

    /// <summary>Mémoire du processus (tests, éditeur).</summary>
    public sealed class MemoryTokenStore : ISupabaseTokenStore
    {
        SupabaseSession? _session;
        public SupabaseSession? Load() => _session;
        public void Save(SupabaseSession session) => _session = session;
        public void Clear() => _session = null;
    }

    /// <summary>Stockage de l'appareil (PlayerPrefs).</summary>
    public sealed class PlayerPrefsTokenStore : ISupabaseTokenStore
    {
        const string Key = "hd.supabase.session";

        public SupabaseSession? Load()
        {
            var json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var s = JsonUtility.FromJson<SupabaseSession>(json);
                return s != null && !string.IsNullOrEmpty(s.RefreshToken) ? s : null;
            }
            catch (Exception) { return null; }
        }

        public void Save(SupabaseSession session)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(session));
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
