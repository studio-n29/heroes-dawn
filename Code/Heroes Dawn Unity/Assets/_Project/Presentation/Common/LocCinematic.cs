#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Légendes de la cinématique de lancement : la narration du PDF « N29 HQ » (version courte, 11 plans), affichées en sous-titres.
    /// Les voix off seront enregistrées plus tard (Resources/Cinematic/Voice/vo_&lt;plan&gt;.wav) : tant qu'elles manquent, la légende reste le temps de la lire.
    /// </summary>
    public static partial class Loc
    {
        /// <summary>Identifiants des plans de la version courte (tous ont une légende).</summary>
        public static readonly string[] CinematicVoiceIds =
        {
            "S01", "S02", "S10", "S11", "S15", "S24", "S33", "S37", "S41", "S44", "S46",
        };

        static partial void ExtendCinematic(Dictionary<string, string> d)
        {
            d["cinematic.vo.S01"] = "Chaque aube naît de la nuit.";
            d["cinematic.vo.S02"] = "Pendant la Guerre froide, l'Amérique a utilisé les recherches d'un généticien pour créer une arme aussi efficace que la bombe atomique, sans la fin du monde.";
            d["cinematic.vo.S10"] = "Après plusieurs essais ratés, malgré la tentative de l'URSS d'avoir son propre méta-humain, le monde a découvert l'Apex Unit : des humains génétiquement modifiés, aux pouvoirs extraordinaires, qui ont mis fin à la Guerre froide en quelques mois.";
            d["cinematic.vo.S11"] = "Une longue fausse paix a suivi, où l'Apex Unit était l'étendard d'une justice manipulatrice, fondée sur le rapport de force.";
            d["cinematic.vo.S15"] = "Jusqu'en 2026, où un jeune homme a réveillé ses pouvoirs enfouis : le premier méta-humain naturel de l'histoire.";
            d["cinematic.vo.S24"] = "Avec son éveil, le monde a été entraîné dans une bataille contre une créature biblique, alors au rang de légende : Mephisto.";
            d["cinematic.vo.S33"] = "À Paris, Mephisto s'est opposé à l'Apex Unit et a ouvert une brèche entre les mondes, qui allait tout anéantir…";
            d["cinematic.vo.S37"] = "… jusqu'à ce que le jeune homme apparaisse, déploie ses ailes et se sacrifie dans ce qu'on appellera La Supernova.";
            d["cinematic.vo.S41"] = "Le déchaînement de radiations du jeune homme, disparu dans la brèche avec Mephisto, a altéré l'ADN d'une grande partie de l'humanité : une vague de méta-humains sans précédent est née.";
            d["cinematic.vo.S44"] = "Aujourd'hui, cinq ans plus tard, les méta-humains cherchent leur place, tandis que l'Apex Unit a laissé la place à The Dawn.";
            d["cinematic.vo.S46"] = "Le monde est partagé entre acceptation, contrôle et anéantissement de ces nouveaux humains.";
        }
    }
}
