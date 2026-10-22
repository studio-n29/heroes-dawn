#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes de la première connexion : cinématique d'introduction, pseudo narratif, choix de la campagne, comptes.</summary>
    public static partial class Loc
    {
        static partial void ExtendOnboarding(Dictionary<string, string> d)
        {
            d["intro.skip"] = "PASSER";
            d["intro.tap"] = "Touche pour continuer";
            d["intro.slide.1"] = "Il y a un an, les méta-humains de l'Apex Unit affrontaient Méphisto, le général des Enfers, en plein cœur de Paris.";
            d["intro.slide.2"] = "Tout semblait perdu : le monde s'effondrait sur lui-même.";
            d["intro.slide.3"] = "Mais un jeune homme, qui avait perdu espoir, est venu tout changer.";
            d["intro.slide.4"] = "Eda, le Phoenix Noir, a déployé ses ailes d'or et renvoyé le mal en Enfer…";
            d["intro.slide.5"] = "… et fit l'ultime sacrifice pour sauver le monde.";
            d["intro.slide.6"] = "En déclenchant une supernova, le portail s'est effondré…";
            d["intro.slide.7"] = "Le monde a plongé dans un blackout à cause des radiations…";
            d["intro.slide.8"] = "… et l'ADN de nombreux humains a été altéré.";
            d["intro.slide.9"] = "Aujourd'hui, le monde a changé… et notre devoir est d'aider chacun à trouver sa place.";

            d["onboarding.speaker"] = "ACCUEIL DE LA GUILDE";
            d["onboarding.pseudo.line"] = "Bienvenue à la Guilde, recrue. Avant d'ouvrir ton dossier : comment doit-on t'appeler, Agent ?";
            d["onboarding.pseudo.placeholder"] = "Ton nom d'agent";
            d["onboarding.pseudo.suggestion"] = "";
            d["onboarding.pseudo.confirm"] = "VALIDER";
            d["onboarding.pseudo.short"] = "Au moins {0} caractères.";
            d["onboarding.pseudo.long"] = "Pas plus de {1} caractères.";
            d["onboarding.pseudo.chars"] = "Lettres, chiffres, espaces, tirets seulement.";
            d["onboarding.campaign.title"] = "CHOISIS TON AGENT";
            d["onboarding.campaign.unavailable"] = "Ce parcours n est pas encore disponible.";
            d["campaign.agent_d.name"] = "AGENT D";
            d["campaign.agent_d.desc"] = "Illustrateur de rue à Harlem : ce qu il imagine devient réel. Son parcours : New York, Paris, Moscou, puis Salem.";
            d["campaign.agent_m.name"] = "AGENT M";
            d["campaign.agent_m.desc"] = "Restauratrice de fresques à Moscou : elle voit le passé, le présent et le futur. Son parcours : Moscou, Paris, New York, puis Salem.";
            d["onboarding.campaign.confirm"] = "COMMENCER";
            d["onboarding.campaign.open"] = "DISPONIBLE";
            d["onboarding.campaign.wip"] = "EN CRÉATION";

            d["errors.account_email"] = "Adresse e-mail invalide.";
            d["errors.account_password"] = "Mot de passe trop court (8 caractères minimum).";
            d["errors.account_exists"] = "Un compte existe déjà avec cette adresse.";
            d["errors.account_credentials"] = "E-mail ou mot de passe incorrect.";
        }
    }
}
