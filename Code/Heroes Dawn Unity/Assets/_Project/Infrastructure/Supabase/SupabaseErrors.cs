#nullable enable
using System;
using N29.HeroesDawn.Core;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>Traduit une réponse d'erreur de Supabase (ou une panne réseau) en erreur du jeu à clé de texte.</summary>
    public static class SupabaseErrors
    {
        [Serializable]
        sealed class ErrorDto { public int code; public string error_code = ""; public string msg = ""; public string error = ""; public string error_description = ""; }

        public static DomainError Network() => new DomainError("account.network", "errors.account_network");
        public static DomainError Session() => new DomainError("account.session", "errors.account_session");

        /// <summary>Erreur du jeu pour une réponse non réussie (jamais le texte brut du serveur : il peut contenir des données sensibles).</summary>
        public static DomainError Map(HttpResult r)
        {
            if (r.NetworkError) return Network();
            string code = "";
            try
            {
                var dto = JsonUtility.FromJson<ErrorDto>(r.Body);
                if (dto != null) code = !string.IsNullOrEmpty(dto.error_code) ? dto.error_code : dto.error ?? "";
            }
            catch (Exception) { /* corps illisible : on s'en tient au code HTTP */ }
            if (r.Status == 429 || code == "over_request_rate_limit" || code == "over_email_send_rate_limit") return new DomainError("account.rate_limit", "errors.account_rate_limit");
            switch (code)
            {
                case "invalid_credentials": return new DomainError("account.credentials", "errors.account_credentials");
                case "email_not_confirmed": return new DomainError("account.unconfirmed", "errors.account_unconfirmed");
                case "user_already_exists":
                case "email_exists": return new DomainError("account.exists", "errors.account_exists");
                case "weak_password": return new DomainError("account.weak_password", "errors.account_weak_password");
                case "validation_failed": return new DomainError("account.email", "errors.account_email");
            }
            if (r.Status == 401 || r.Status == 403) return Session();
            if (r.Status >= 500 || r.Status == 0) return Network();
            return new DomainError("account.credentials", "errors.account_credentials");
        }
    }
}
