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

            d["onboarding.campaign.title"] = "INCARNE TON AGENT";
            d["onboarding.campaign.unavailable"] = "Ce parcours n est pas encore disponible.";
            d["campaign.agent_d.name"] = "AGENT D";
            d["campaign.agent_d.desc"] = "Illustrateur de rue à Harlem : ce qu il imagine devient réel. Tu diriges l unité à travers lui, sans qu il combatte. Parcours : New York, Paris, Moscou, puis Salem. Première recrue : Darius Brooks.";
            d["campaign.agent_m.name"] = "AGENT M";
            d["campaign.agent_m.desc"] = "Restauratrice de fresques à Moscou : elle voit le passé, le présent et le futur. Tu diriges l unité à travers elle, sans qu elle combatte. Parcours : Moscou, Paris, New York, puis Salem. Première recrue : Sam Okafor.";
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
