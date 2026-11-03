#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes des chapitres (lot G) : titres et ouvertures des 8 chapitres de New York et de Moscou, page Chapitres.</summary>
    public static partial class Loc
    {
        static partial void ExtendChapters(Dictionary<string, string> d)
        {
            d["dossier.button"] = "DOSSIER";
            d["dossier.close"] = "FERMER";
            d["dossier.identity"] = "Identité";
            d["dossier.supernova"] = "Lien à la Supernova";
            d["dossier.story"] = "Histoire d'origine";
            d["dossier.power"] = "Pouvoir";
            d["dossier.look"] = "Apparence";
            d["page.chapters"] = "Chapitres";
            d["chapters.header"] = "CHAPITRES";
            d["chapters.line"] = "{0} / {1} chapitres terminés";
            d["chapters.row_sub"] = "{0} · {1}";
            d["chapters.button"] = "CHAPITRES";
            d["chapter.relire"] = "RELIRE";
            d["chapter.soon_reason"] = "Ce chapitre arrive bientôt.";
            d["chapter.status.Done"] = "Terminé";
            d["chapter.status.Current"] = "En cours";
            d["chapter.status.Locked"] = "Verrouillé";
            d["chapter.status.Soon"] = "Bientôt";
            d["chapter.heading"] = "CHAPITRE {0} · {1}";
            d["chapter.continue"] = "CONTINUER";

            // Fragments de souvenirs (L11) : récits courts gagnés en validant une zone, relus depuis la page Chapitres.
            d["fragments.title"] = "SOUVENIRS · {0} ({1} / {2})";
            d["fragments.none"] = "Ce chapitre ne contient pas encore de souvenir.";
            d["fragments.locked"] = "Souvenir verrouillé";
            d["fragments.unlock"] = "Valide : {0}";
            d["fragments.read"] = "LIRE";
            d["fragments.close"] = "FERMER";
            d["fragments.new"] = "{0} NOUVEAU";
            d["fragments.audio"] = "Voix disponible";
            d["fragments.no_audio"] = "Audio à venir.";
            d["fragment.redhook_docks.title"] = "La nuit des quais";
            d["fragment.redhook_docks.text"] = "Les conteneurs de Red Hook n'ont jamais été vides. Cette nuit-là, les dockers regardaient ailleurs et les camions de l'AMIF sont repartis sans un bruit. C'est en voyant les mêmes plaques des deux côtés de la grille que l'Aube a compris qui se parlait vraiment, derrière la guerre affichée.";
            d["fragment.brownsville_hostages.title"] = "Les otages de Brownsville";
            d["fragment.brownsville_hostages.text"] = "Ils étaient onze, dans une cave sans fenêtre, et ils ne demandaient pas qu'on les sauve : ils demandaient qu'on se taise. Le gang les gardait pour que l'AMIF puisse « les retrouver » en héros. L'Aube a choisi de ne pas faire partie de la mise en scène.";
            d["fragment.coney_trap.title"] = "Le piège de Coney Island";
            d["fragment.coney_trap.text"] = "La grande roue tournait pour personne. Tout avait été préparé pour que l'équipe entre par le mauvais côté : une porte ouverte, une lumière allumée, un message laissé exprès. On ne se méfie jamais assez d'un chemin trop facile.";
            d["fragment.dumbo_bridge.title"] = "Le pont de Dumbo";
            d["fragment.dumbo_bridge.text"] = "De l'autre côté du pont, Manhattan brillait comme si rien ne s'était passé. Brooklyn était derrière eux, et ils savaient que chaque victoire avait été vue, comptée, archivée. Le prochain pas ne se ferait plus dans l'ombre.";

            d["chapter.ny_brooklyn.title"] = "Brooklyn";
            d["chapter.ny_brooklyn.intro"] = "Red Hook, Gowanus, Dumbo : Brooklyn est le premier front. L'AMIF et les gangs, qui prétendent se combattre, se partagent la ville en secret. L'Aube y fait ses preuves, mais chaque succès attire un peu plus l'attention de l'AMIF.";
            d["chapter.ny_queens.title"] = "Queens";
            d["chapter.ny_queens.intro"] = "Flushing, Astoria, Jamaica : dans le Queens, le Syndicat fait régner la peur sur les marchés et les gares de triage. Des captifs, des indics, des caméras : l'Aube découvre que le Syndicat n'agit pas seul.";
            d["chapter.ny_harlem.title"] = "Harlem";
            d["chapter.ny_harlem.intro"] = "À Harlem, la Supernova a laissé des cicatrices que les gangs et le culte de Mephisto exploitent. Des quartiers brûlent, des familles fuient. L'Aube doit choisir qui protéger en premier, et à quel prix.";
            d["chapter.ny_manhattan.title"] = "Manhattan";
            d["chapter.ny_manhattan.intro"] = "Midtown, Times Square, le siège de l'ONU : tout converge vers Manhattan. Les cercles de pouvoir, l'AMIF et l'Aube s'y affrontent à visage découvert. C'est ici que la ville bascule.";
            d["chapter.mos_sergueiv.title"] = "Sergueiv";
            d["chapter.mos_sergueiv.intro"] = "Sergueiv, ses monastères et sa ligne de train futuriste : Moscou s'est armée face aux changements du monde, mais elle accueille l'Aube. Derrière la façade, quelqu'un tire déjà les ficelles.";
            d["chapter.mos_kapotnya.title"] = "Kapotnya";
            d["chapter.mos_kapotnya.intro"] = "Kapotnya, ses raffineries et son brouillard industriel : les oligarques y cachent ce qu'ils ne veulent pas montrer. L'Aube suit des convois, des fuites et une mafia alliée à l'AMIF dans l'ombre.";
            d["chapter.mos_sadovoye.title"] = "Sadovoye Koltso";
            d["chapter.mos_sadovoye.intro"] = "Sur le Jardin, l'anneau de la ville, les tours des oligarques brillent de mille hologrammes. L'Aube comprend que le pouvoir ne se cache plus : il se met en scène.";
            d["chapter.mos_place_rouge.title"] = "La Place Rouge";
            d["chapter.mos_place_rouge.intro"] = "La Place Rouge, le Kremlin, le couvre-feu : le dernier chapitre de Moscou est un face-à-face. L'Aube doit décider si elle fait confiance à ceux qui l'accueillent, ou à ceux qui la manipulent.";
        }
    }
}
