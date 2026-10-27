#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du compte joueur (lot I de la 0.14.0) : erreurs, fenêtre Compte, choix de progression, profil.</summary>
    public static partial class Loc
    {
        static partial void ExtendAccount(Dictionary<string, string> d)
        {
            // Erreurs (errors.account_email, _password, _exists et _credentials sont déjà dans LocOnboarding).
            d["errors.account_network"] = "Impossible de joindre le serveur. Vérifie ta connexion.";
            d["errors.account_unconfirmed"] = "Confirme ton adresse e-mail avant de te connecter.";
            d["errors.account_weak_password"] = "Mot de passe trop faible.";
            d["errors.account_rate_limit"] = "Trop d'essais : réessaie dans un instant.";
            d["errors.account_oauth"] = "La connexion a échoué ou a été annulée.";
            d["errors.account_session"] = "Ta session a expiré : reconnecte-toi.";
            d["errors.account_cloud"] = "La sauvegarde en ligne est indisponible pour le moment.";
            d["errors.account_schema"] = "Ta sauvegarde en ligne vient d'une version plus récente du jeu : mets le jeu à jour.";
        }
    }
}
