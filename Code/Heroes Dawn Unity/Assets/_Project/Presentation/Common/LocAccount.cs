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
            d["errors.account_nochoice"] = "Aucun choix de progression n'est en attente.";
            d["errors.account_cloud"] = "La sauvegarde en ligne est indisponible pour le moment.";
            d["errors.account_schema"] = "Ta sauvegarde en ligne vient d'une version plus récente du jeu : mets le jeu à jour.";

            // Écran titre
            d["login.account"] = "SE CONNECTER";

            // Fenêtre Compte
            d["account.title.signin"] = "CONNEXION";
            d["account.title.signup"] = "CRÉER UN COMPTE";
            d["account.tab.signin"] = "Connexion";
            d["account.tab.signup"] = "Créer un compte";
            d["account.email"] = "E-mail";
            d["account.password"] = "Mot de passe";
            d["account.name"] = "Pseudo (3 à 16 caractères)";
            d["account.submit.signin"] = "SE CONNECTER";
            d["account.submit.signup"] = "CRÉER MON COMPTE";
            d["account.google"] = "Continuer avec Google";
            d["account.apple"] = "Continuer avec Apple";
            d["account.forgot"] = "Mot de passe oublié ?";
            d["account.close"] = "Fermer";
            d["account.busy"] = "Connexion en cours…";
            d["account.error.email"] = "Adresse e-mail invalide.";
            d["account.error.name"] = "Le pseudo doit faire de 3 à 16 caractères.";
            d["account.confirm_sent"] = "E-mail de confirmation envoyé : clique sur le lien puis reviens te connecter.";
            d["account.reset_sent"] = "Si un compte existe pour cette adresse, un e-mail vient d'être envoyé.";

            // Résultats du parcours
            d["account.connected"] = "Connecté. Ta progression sera rattachée au compte quand tu lanceras la partie.";
            d["account.linked"] = "Compte lié : ta progression est sauvegardée en ligne.";
            d["account.pulled"] = "Progression du compte récupérée.";
            d["account.choice_later"] = "Rien n'a été modifié. Tu pourras choisir plus tard avec « Synchroniser » dans ton profil.";
            d["account.signed_out"] = "Déconnecté : tu continues en invité avec cette progression.";
            d["account.sync_done"] = "Progression synchronisée.";
            d["account.signout.title"] = "Se déconnecter ?";
            d["account.signout.body"] = "Tu continueras en invité avec la progression de cet appareil, qui ne sera plus sauvegardée en ligne.";
            d["account.signout.ok"] = "SE DÉCONNECTER";

            // Choix de progression
            d["account.choice.title"] = "QUELLE PROGRESSION GARDER ?";
            d["account.choice.hint"] = "Cet appareil et ton compte ont chacun de la progression. L'autre version est mise de côté : tu ne perds rien.";
            d["account.choice.device"] = "CET APPAREIL";
            d["account.choice.account"] = "LE COMPTE";
            d["account.choice.summary"] = "Niveau {0} · {1} Crédits · {2} zones\nDernière partie : {3}";
            d["account.choice.keep_device"] = "GARDER CET APPAREIL";
            d["account.choice.keep_account"] = "GARDER LE COMPTE";
            d["account.choice.later"] = "PLUS TARD";

            // Profil
            d["profile.account.guest"] = "Compte invité";
            d["profile.account.guest_sub"] = "Lie ton compte pour sauvegarder ta progression en ligne.";
            d["profile.account.link"] = "Lier mon compte";
            d["profile.account.connected"] = "Compte : {0}";
            d["profile.account.connected_sub"] = "{0} · Dernière synchronisation : {1}";
            d["profile.account.never"] = "jamais";
            d["profile.account.sync"] = "Synchroniser";
            d["profile.account.signout"] = "Se déconnecter";
            d["account.provider.email"] = "E-mail";
            d["account.provider.google"] = "Google";
            d["account.provider.apple"] = "Apple";
        }
    }
}
