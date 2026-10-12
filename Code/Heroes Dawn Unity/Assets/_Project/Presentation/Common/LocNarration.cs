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
            d["narration.ny_1_1.n23.after.0"] = "Brick s'effondre contre un conteneur. L'échange est stoppé : cages, registres, bons de livraison, les preuves sont là.";
            d["narration.ny_1_1.n23.after.1"] = "Il ne reste qu'à fuir le quai avant que l'AMIF ne revienne.";
            d["speaker.lieutenant"] = "Lieutenant";
            d["speaker.crocodile"] = "Crocodile";
            d["speaker.stroke"] = "Stroke";
            d["speaker.general"] = "Le Général";
            d["speaker.rayner"] = "Rayner";
            d["narration.ny_1_2.n14.before.0"] = "« Crocodile m'a confié les cellules. Il sera ravi de vous y retrouver. »";
            d["narration.ny_1_2.n14.before.1"] = "Les otages sont derrière lui. On ne s'arrête pas.";
            d["narration.ny_1_2.n14.after.0"] = "« Vous ne savez pas ce que vous venez de déclencher. Crocodile vous le fera payer. »";
            d["narration.ny_1_2.n19.after.0"] = "Les otages sont libres et à l'abri. Bien joué, tout le monde.";
            d["narration.ny_1_2.n19.after.1"] = "Mais Crocodile n'oubliera pas : il va vouloir sa revanche.";
            d["narration.ny_1_3.n18.before.0"] = "« Vous êtes venus. C'est exactement ce que je voulais : toute la Guilde sur mon île. »";
            d["narration.ny_1_3.n18.before.1"] = "On savait que c'était un piège. Les civils sont à l'abri : on finit ça ici.";
            d["narration.ny_1_3.n18.after.0"] = "« Impossible... Vous... »";
            d["narration.ny_1_3.n18.after.1"] = "Crocodile est à terre. Mais Coney Island est ravagée : la victoire a un prix.";
            d["narration.ny_1_4.n18.before.0"] = "« Vous êtes cernés. Déposez vos pouvoirs et nous pourrons discuter. »";
            d["narration.ny_1_4.n18.before.1"] = "Tenez la ligne. On ne baisse pas les armes devant l'AMIF.";
            d["narration.ny_1_4.n18.after.0"] = "« Assez ! Stroke, j'ai de quoi révéler vos petites affaires à toute la ville. Voulez-vous que je le fasse ici, devant vos hommes ? »";
            d["narration.ny_1_4.n18.after.1"] = "« ... Repli. Tout le monde. »";
            d["narration.ny_1_4.n18.after.2"] = "« Crocodile sera désigné coupable de tout ceci. Il fera un excellent coupable. »";
            d["narration.ny_1_4.n18.after.3"] = "« Mais retenez ceci, Rayner : le tour de la Guilde viendra, une fois les Accords de Salem signés. »";
        }
    }
}
