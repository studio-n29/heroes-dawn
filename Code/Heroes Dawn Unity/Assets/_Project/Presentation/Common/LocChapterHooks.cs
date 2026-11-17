#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Branchement des textes des six chapitres suivants (Queens, Harlem, Manhattan, Kapotnya, Sadovoye Koltso, Place Rouge).</summary>
    public static partial class Loc
    {
        static void ExtendChapterTexts(Dictionary<string, string> d)
        {
            d["map.borough.harlem"] = "HARLEM";
            BrooklynFrench(d);
            d["choice.hint"] = "Ce choix est définitif.";
            d["choice.harlem_priority.title"] = "Qui défendre en premier ?";
            d["choice.harlem_priority.clinic"] = "La clinique";
            d["choice.harlem_priority.market"] = "Le marché";
            d["choice.place_rouge_trust.title"] = "Que faire du Conseiller ?";
            d["choice.place_rouge_trust.arrest"] = "L'arrêter";
            d["choice.place_rouge_trust.listen"] = "L'écouter";
            ExtendQueens(d); ExtendHarlem(d); ExtendManhattan(d);
            ExtendKapotnya(d); ExtendSadovoye(d); ExtendPlaceRouge(d);
        }

        static partial void ExtendQueens(Dictionary<string, string> d);
        static partial void ExtendHarlem(Dictionary<string, string> d);
        static partial void ExtendManhattan(Dictionary<string, string> d);
        static partial void ExtendKapotnya(Dictionary<string, string> d);
        static partial void ExtendSadovoye(Dictionary<string, string> d);
        static partial void ExtendPlaceRouge(Dictionary<string, string> d);

        static Dictionary<string, string> WithChapterEnglish(Dictionary<string, string> d)
        {
            d["choice.hint"] = "This choice is final.";
            d["choice.harlem_priority.title"] = "Who do you defend first?";
            d["choice.harlem_priority.clinic"] = "The clinic";
            d["choice.harlem_priority.market"] = "The market";
            d["choice.place_rouge_trust.title"] = "What do you do with the Councillor?";
            d["choice.place_rouge_trust.arrest"] = "Arrest him";
            d["choice.place_rouge_trust.listen"] = "Hear him out";
            d["speaker.stroke"] = "General Stroke";
            d["speaker.rayner"] = "Brie Rayner";
            d["map.borough.queens"] = "QUEENS";
            d["map.borough.harlem"] = "HARLEM";
            d["map.borough.manhattan"] = "MANHATTAN";
            BrooklynEnglish(d); QueensEnglish(d); HarlemEnglish(d); ManhattanEnglish(d);
            KapotnyaEnglish(d); SadovoyeEnglish(d); PlaceRougeEnglish(d);
            return d;
        }
    }
}
