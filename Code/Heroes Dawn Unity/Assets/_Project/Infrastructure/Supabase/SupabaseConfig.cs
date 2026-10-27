#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>
    /// Configuration du projet Supabase du jeu : URL, clé publique <c>anon</c> (publique par conception, jamais la clé <c>service_role</c>) et lien de retour OAuth.
    /// Lue dans <c>Resources/Supabase/supabase.json</c> ; sans fichier valide, le jeu utilise les simulateurs.
    /// </summary>
    public sealed class SupabaseConfig
    {
        [Serializable]
        sealed class Dto { public string url = ""; public string anonKey = ""; public string redirectUrl = ""; }

        public string Url { get; }
        public string AnonKey { get; }
        public string RedirectUrl { get; }

        public SupabaseConfig(string url, string anonKey, string redirectUrl)
        {
            Url = (url ?? "").TrimEnd('/'); AnonKey = anonKey ?? ""; RedirectUrl = redirectUrl ?? "";
        }

        /// <summary>Configuration utilisable : adresse en https et clé non vide.</summary>
        public bool IsValid => Url.StartsWith("https://", StringComparison.Ordinal) && Url.Length > "https://".Length && AnonKey.Length > 0;

        public static SupabaseConfig Parse(string json)
        {
            try
            {
                var dto = JsonUtility.FromJson<Dto>(json);
                return dto == null ? new SupabaseConfig("", "", "") : new SupabaseConfig(dto.url, dto.anonKey, dto.redirectUrl);
            }
            catch (Exception) { return new SupabaseConfig("", "", ""); }
        }

        /// <summary>La configuration du jeu, ou <c>null</c> si le fichier est absent ou invalide.</summary>
        public static SupabaseConfig? Load()
        {
            var asset = Resources.Load<TextAsset>("Supabase/supabase");
            if (asset == null) return null;
            var config = Parse(asset.text);
            return config.IsValid ? config : null;
        }
    }
}
