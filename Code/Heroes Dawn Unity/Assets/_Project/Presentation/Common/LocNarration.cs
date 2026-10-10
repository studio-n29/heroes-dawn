#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes de la boîte de dialogue de narration, noms des personnages non joueurs et dialogues du prototype.</summary>
    public static partial class Loc
    {
        static partial void ExtendNarration(Dictionary<string, string> d)
        {
            d["narration.skip"] = "PASSER";
            d["narration.next"] = "▼ Touche pour continuer";
            d["speaker.brick"] = "Brick";
            d["speaker.dockmaster"] = "Contremaître";
            d["narration.ny_1_1.n23.before.0"] = "« Vous avez traversé tout mon quai pour finir ici. Courageux. Ou idiots. »";
            d["narration.ny_1_1.n23.before.1"] = "On ne recule pas. Anchor, tiens la ligne : Frost, Echo, vous couvrez ses angles.";
            d["narration.ny_1_1.n23.after.0"] = "Brick s'effondre contre un conteneur. Le générateur du quai se tait enfin.";
            d["narration.ny_1_1.n23.after.1"] = "La grue du terminal est à portée : plus qu'un pas.";
        }
    }
}
