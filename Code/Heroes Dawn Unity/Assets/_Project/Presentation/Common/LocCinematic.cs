#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Voix off de la cinématique de lancement (Brie Rayner), affichée en sous-titres. Version condensée de 02_Lore_Narration/05_Cinematique_Lancement.md.</summary>
    public static partial class Loc
    {
        /// <summary>Identifiants des plans qui ont une voix off (les autres sont muets).</summary>
        public static readonly string[] CinematicVoiceIds =
        {
            "S01", "S02", "S10", "S11", "S15", "S20", "S24", "S33", "S37", "S41", "S44",
        };

        static partial void ExtendCinematic(Dictionary<string, string> d)
        {
            d["cinematic.vo.S01"] = "Chaque aube naît de la nuit.";
            d["cinematic.vo.S02"] = "En 1973, Frank Khost change ce que signifie être humain.";
            d["cinematic.vo.S10"] = "Les Apex deviennent les protecteurs du monde.";
            d["cinematic.vo.S11"] = "La Guerre froide s'achève, et Viktor Volkov disparaît.";
            d["cinematic.vo.S15"] = "En 2026, Eda Ryle s'éveille, brûlant d'un feu noir.";
            d["cinematic.vo.S20"] = "Orik Kovvolt veut effacer les méta-humains.";
            d["cinematic.vo.S24"] = "Leur combat libère le démon Mephisto.";
            d["cinematic.vo.S33"] = "À Paris, il ouvre une porte vers les Enfers.";
            d["cinematic.vo.S37"] = "Eda devient le Phoenix Doré.";
            d["cinematic.vo.S41"] = "La lumière traverse le monde. Des êtres s'éveillent.";
            d["cinematic.vo.S44"] = "Alors j'ai fondé l'Aube, pour les protéger.";
        }
    }
}
