#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Première connexion, séquence de Rayner : accueil en Full Art, fiche d'identification de The Dawn (nom, prénom, âge, avatar, pouvoir),
    /// conclusion et choix de la ville de départ. Le français est ici, l'anglais dans <see cref="WithOnboardingEnglish"/> (mêmes clés).
    /// Ton : Rayner tutoie, parle court et franc (voir les bulles du tutoriel) ; lore : cinq ans après la Supernova, l'AMIF, les Apex.
    /// </summary>
    public static partial class Loc
    {
        static void ExtendOnboardingRayner(Dictionary<string, string> d)
        {
            d["onboarding.rayner.speaker"] = "BRIE RAYNER";
            d["onboarding.rayner.continue"] = "TOUCHE POUR CONTINUER";
            d["onboarding.rayner.intro.1"] = "Te voilà enfin. Je suis Brie Rayner, fondatrice de The Dawn : l’Aube.";
            d["onboarding.rayner.intro.2"] = "Il y a cinq ans, la Supernova de Paris a réécrit quelque chose en nous. Un pouvoir endormi, qui ne se réveille qu’au premier grand choc.";
            d["onboarding.rayner.intro.3"] = "L’AMIF les étudie, les prélève. Nous, on les protège : eux, et tous ceux qui n’ont rien demandé.";
            d["onboarding.rayner.intro.4"] = "On les appelle les Apex. Peut-être en es-tu un. En tout cas, tu es à ta place ici.";
            d["onboarding.rayner.ask"] = "Tu peux me remplir ta fiche d’identification pour les fichiers de The Dawn ?";
            d["onboarding.rayner.after.1"] = "Merci, {0}. Ton dossier rejoint nos archives : à partir de maintenant, l’Aube compte sur toi.";
            d["onboarding.rayner.after.2"] = "Deux de mes agents t’attendent, chacun dans sa ville. Par où veux-tu commencer ?";

            d["onboarding.form.title"] = "FICHE D’IDENTIFICATION";
            d["onboarding.form.last_name"] = "NOM";
            d["onboarding.form.first_name"] = "PRÉNOM";
            d["onboarding.form.age"] = "ÂGE";
            d["onboarding.form.avatar"] = "AVATAR";
            d["onboarding.form.power"] = "POUVOIR";
            d["onboarding.form.power_placeholder"] = "Choisir un pouvoir…";
            d["onboarding.form.confirm"] = "VALIDER LA FICHE";
            d["onboarding.age.invalid"] = "L’âge doit être compris entre 12 et 99 ans.";
            d["onboarding.power.invalid"] = "Choisis un pouvoir dans le tableau.";

            d["onboarding.start.title"] = "OÙ COMMENCES-TU ?";
            d["onboarding.start.soon"] = "BIENTÔT";
            d["onboarding.start.agent_d.city"] = "NEW YORK";
            d["onboarding.start.agent_d.desc"] = "Agent D t’y attend : illustrateur de rue à Harlem, ce qu’il imagine devient réel.";
            d["onboarding.start.agent_m.city"] = "MOSCOU";
            d["onboarding.start.agent_m.desc"] = "Agent M t’y attend : restauratrice de fresques, elle voit le passé, le présent et le futur.";

            d["power.family.matiere.name"] = "MATIÈRE";
            d["power.family.elementaire.name"] = "ÉLÉMENTAIRE";
            d["power.family.physique_bio.name"] = "PHYSIQUE / BIO";
            d["power.family.perception.name"] = "PERCEPTION";

            d["power.ferromodelage.name"] = "Ferromodelage";
            d["power.lithomodelage.name"] = "Lithomodelage";
            d["power.cristallisation.name"] = "Cristallisation";
            d["power.polymorphie_partielle.name"] = "Polymorphie partielle";
            d["power.materialisation.name"] = "Matérialisation";
            d["power.transmutation.name"] = "Transmutation";
            d["power.polymorphie_totale.name"] = "Polymorphie totale";

            d["power.electrogenese.name"] = "Électrogenèse";
            d["power.pyrochimie.name"] = "Pyrochimie";
            d["power.cryogenie.name"] = "Cryogénie";
            d["power.liquide_corrosif.name"] = "Liquide corrosif";
            d["power.decharges_electriques.name"] = "Décharges électriques";
            d["power.vapeur.name"] = "Maîtrise de la vapeur";
            d["power.magnetisme.name"] = "Magnétisme";

            d["power.force_colossale.name"] = "Force colossale";
            d["power.insensibilite_douleur.name"] = "Insensibilité à la douleur";
            d["power.hyper_regeneration.name"] = "Hyper-régénération";
            d["power.adaptation_sous_marine.name"] = "Adaptation sous-marine";
            d["power.detoxification.name"] = "Détoxification";
            d["power.adherence.name"] = "Adhérence";
            d["power.thermoregulation.name"] = "Thermorégulation";

            d["power.camouflage_optique.name"] = "Camouflage optique";
            d["power.bullet_time.name"] = "Bullet-time";
            d["power.echolocation.name"] = "Écholocation";
            d["power.vigilance_continue.name"] = "Vigilance continue";
            d["power.magnetoreception.name"] = "Magnétoréception";
            d["power.vision_thermique.name"] = "Vision thermique";
            d["power.precognition.name"] = "Précognition";
        }

        /// <summary>Ajoute les textes anglais de la première connexion au dictionnaire anglais (méthode statique sans état : sûre dans un initialiseur de champ).</summary>
        static Dictionary<string, string> WithOnboardingEnglish(Dictionary<string, string> d)
        {
            d["intro.skip"] = "SKIP";
            d["onboarding.campaign.unavailable"] = "This route isn’t available yet.";
            d["campaign.agent_d.name"] = "AGENT D";
            d["campaign.agent_m.name"] = "AGENT M";
            d["onboarding.rayner.speaker"] = "BRIE RAYNER";
            d["onboarding.rayner.continue"] = "TAP TO CONTINUE";
            d["onboarding.rayner.intro.1"] = "There you are. I’m Brie Rayner, founder of The Dawn.";
            d["onboarding.rayner.intro.2"] = "Five years ago, the Paris Supernova rewrote something inside us. A dormant power that only wakes at the first great shock.";
            d["onboarding.rayner.intro.3"] = "The AMIF studies them, harvests them. We protect them: them, and everyone who never asked for any of this.";
            d["onboarding.rayner.intro.4"] = "They call them the Apex. Maybe you are one. Either way, you belong here.";
            d["onboarding.rayner.ask"] = "Can you fill in your ID file for The Dawn’s records?";
            d["onboarding.rayner.after.1"] = "Thank you, {0}. Your file joins our archives: from now on, The Dawn counts on you.";
            d["onboarding.rayner.after.2"] = "Two of my agents are waiting for you, each in their own city. Where do you want to start?";

            d["onboarding.form.title"] = "IDENTIFICATION FILE";
            d["onboarding.form.last_name"] = "LAST NAME";
            d["onboarding.form.first_name"] = "FIRST NAME";
            d["onboarding.form.age"] = "AGE";
            d["onboarding.form.avatar"] = "AVATAR";
            d["onboarding.form.power"] = "POWER";
            d["onboarding.form.power_placeholder"] = "Choose a power…";
            d["onboarding.form.confirm"] = "SUBMIT FILE";
            d["onboarding.age.invalid"] = "Age must be between 12 and 99.";
            d["onboarding.power.invalid"] = "Pick a power from the table.";

            d["onboarding.start.title"] = "WHERE DO YOU START?";
            d["onboarding.start.soon"] = "COMING SOON";
            d["onboarding.start.agent_d.city"] = "NEW YORK";
            d["onboarding.start.agent_d.desc"] = "Agent D is waiting there: a street illustrator in Harlem, whatever he imagines becomes real.";
            d["onboarding.start.agent_m.city"] = "MOSCOW";
            d["onboarding.start.agent_m.desc"] = "Agent M is waiting there: a fresco restorer who sees the past, the present and the future.";

            d["power.family.matiere.name"] = "MATTER";
            d["power.family.elementaire.name"] = "ELEMENTAL";
            d["power.family.physique_bio.name"] = "PHYSICAL / BIO";
            d["power.family.perception.name"] = "PERCEPTION";

            d["power.ferromodelage.name"] = "Metal shaping";
            d["power.lithomodelage.name"] = "Stone shaping";
            d["power.cristallisation.name"] = "Crystallization";
            d["power.polymorphie_partielle.name"] = "Partial shapeshifting";
            d["power.materialisation.name"] = "Materialization";
            d["power.transmutation.name"] = "Transmutation";
            d["power.polymorphie_totale.name"] = "Full shapeshifting";

            d["power.electrogenese.name"] = "Electrogenesis";
            d["power.pyrochimie.name"] = "Pyrochemistry";
            d["power.cryogenie.name"] = "Cryogenics";
            d["power.liquide_corrosif.name"] = "Corrosive liquid";
            d["power.decharges_electriques.name"] = "Electric discharges";
            d["power.vapeur.name"] = "Steam control";
            d["power.magnetisme.name"] = "Magnetism";

            d["power.force_colossale.name"] = "Colossal strength";
            d["power.insensibilite_douleur.name"] = "Pain immunity";
            d["power.hyper_regeneration.name"] = "Hyper-regeneration";
            d["power.adaptation_sous_marine.name"] = "Underwater adaptation";
            d["power.detoxification.name"] = "Detoxification";
            d["power.adherence.name"] = "Adhesion";
            d["power.thermoregulation.name"] = "Thermoregulation";

            d["power.camouflage_optique.name"] = "Optical camouflage";
            d["power.bullet_time.name"] = "Bullet time";
            d["power.echolocation.name"] = "Echolocation";
            d["power.vigilance_continue.name"] = "Constant vigilance";
            d["power.magnetoreception.name"] = "Magnetoreception";
            d["power.vision_thermique.name"] = "Thermal vision";
            d["power.precognition.name"] = "Precognition";
            return d;
        }
    }
}
