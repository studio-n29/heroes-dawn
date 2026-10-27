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
