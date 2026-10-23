#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Voix off de la cinématique de lancement (Brie Rayner), affichée en sous-titres. Source : 02_Lore_Narration/05_Cinematique_Lancement.md.</summary>
    public static partial class Loc
    {
        /// <summary>Identifiants des plans qui ont une voix off (les autres sont muets).</summary>
        public static readonly string[] CinematicVoiceIds =
        {
            "S01", "S02", "S03", "S05", "S06", "S07", "S08", "S09", "S10", "S11", "S12", "S13", "S14", "S15", "S16", "S17", "S18", "S19", "S20", "S21", "S22",
            "S23", "S24", "S25", "S26", "S27", "S28", "S29", "S30", "S31", "S32", "S33", "S34", "S35", "S36", "S37", "S38", "S39", "S41", "S42", "S43", "S44", "S45",
        };

        static partial void ExtendCinematic(Dictionary<string, string> d)
        {
            d["cinematic.vo.S01"] = "Chaque aube commence dans la nuit la plus noire.";
            d["cinematic.vo.S02"] = "En 1973, un homme a changé ce que signifiait être humain. Il s'appelait Frank Khost.";
            d["cinematic.vo.S03"] = "Il croyait pouvoir guérir le monde.";
            d["cinematic.vo.S05"] = "Huit ans plus tard, en pleine Guerre froide, l'Amérique a vu dans ses travaux une arme. Une dissuasion sans retombées.";
            d["cinematic.vo.S06"] = "Adam West. Le soldat qui avait perdu un bras.";
            d["cinematic.vo.S07"] = "Moi, Brie Rayner. Ingénieure à la NASA.";
            d["cinematic.vo.S08"] = "John Guerin. L'artiste. Le premier à avoir survécu.";
            d["cinematic.vo.S09"] = "Erik Bronson. Rien ne lui échappait, pas même derrière les murs.";
            d["cinematic.vo.S10"] = "Ils nous ont appelés l'Apex Unit.";
            d["cinematic.vo.S11"] = "L'Union soviétique a répondu avec un seul homme. Viktor Volkov, maître de la matière.";
            d["cinematic.vo.S12"] = "Puis le bloc soviétique s'est effondré.";
            d["cinematic.vo.S13"] = "Volkov a disparu.";
            d["cinematic.vo.S14"] = "Et nous sommes devenus les protecteurs du monde.";
            d["cinematic.vo.S15"] = "Trente-cinq ans plus tard, un jeune homme de vingt-cinq ans n'avait plus la force de continuer. Il s'appelait Eda Ryle.";
            d["cinematic.vo.S16"] = "Les médecins ont parlé d'un malaise. Son cœur ne battait plus qu'une fois par minute.";
            d["cinematic.vo.S17"] = "Pendant vingt-quatre heures, il est resté suspendu entre la vie et la mort.";
            d["cinematic.vo.S18"] = "Puis il s'est réveillé. Intact. Et brûlant d'un feu noir et vert.";
            d["cinematic.vo.S19"] = "Il a choisi de le cacher.";
            d["cinematic.vo.S20"] = "Un homme est venu le chercher. Orik Kovvolt, chef de la X Society. Il voulait effacer les méta-humains de la surface de la Terre.";
            d["cinematic.vo.S21"] = "Pour forcer Eda à le suivre, il lui a tout pris.";
            d["cinematic.vo.S22"] = "Eda l'a traqué. Et sous ce visage, il a trouvé Viktor Volkov.";
            d["cinematic.vo.S23"] = "Leur combat était un rituel. Leurs sangs mêlés ont réveillé une machine.";
            d["cinematic.vo.S24"] = "Et le démon qui vivait en Volkov depuis des décennies s'est libéré. Mephisto.";
            d["cinematic.vo.S25"] = "Les Apex l'ont rejeté. L'AMIF l'a traqué. J'ai été la seule à le croire.";
            d["cinematic.vo.S26"] = "Dans la fuite, nous sommes devenus bien plus que des alliés.";
            d["cinematic.vo.S27"] = "Dans l'Oregon, Mephisto a libéré les oubliés du Protocole Khost.";
            d["cinematic.vo.S28"] = "Il visait le cœur de la foi.";
            d["cinematic.vo.S29"] = "Là-bas, j'ai été touchée.";
            d["cinematic.vo.S30"] = "Et Eda a tout réduit en cendres.";
            d["cinematic.vo.S31"] = "Mephisto lui a révélé la vérité. Il était le Phoenix Noir. Né des expériences de Volkov : Khost, du sang d'ange, et du sang de démon.";
            d["cinematic.vo.S32"] = "Revenu une fois de plus, il a abandonné.";
            d["cinematic.vo.S33"] = "À Paris, Mephisto a ouvert une porte vers les Enfers.";
            d["cinematic.vo.S34"] = "Les Apex ont combattu. Moi aussi.";
            d["cinematic.vo.S35"] = "Au moment où j'allais tomber, je l'ai appelé.";
            d["cinematic.vo.S36"] = "Il a entendu. Il a accepté ce qu'il était.";
            d["cinematic.vo.S37"] = "Et le Phoenix Noir est devenu le Phoenix Doré.";
            d["cinematic.vo.S38"] = "Il a arraché Mephisto au corps de Volkov.";
            d["cinematic.vo.S39"] = "Puis il a franchi la porte.";
            d["cinematic.vo.S41"] = "La lumière a traversé le monde. Toutes les villes se sont éteintes. Et quelque chose, en nous, a changé.";
            d["cinematic.vo.S42"] = "Paris en porte encore les cicatrices. Et un minerai que la Terre n'avait jamais connu : l'edenite.";
            d["cinematic.vo.S43"] = "Partout, des femmes et des hommes se sont éveillés.";
            d["cinematic.vo.S44"] = "Alors j'ai fondé l'Aube. Pour les trouver. Pour les protéger.";
            d["cinematic.vo.S45"] = "Cinq ans ont passé. Et l'Aube a besoin de vous.";
        }
    }
}
