#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Anglais de Brooklyn (chapitre 1 de New York) issu du script narratif : dialogues, fragments, textes de fiche.</summary>
    public static partial class Loc
    {
        /// <summary>Textes français ajoutés à Brooklyn par le script narratif (phrases d ouverture par parcours).</summary>
        static void BrooklynFrench(Dictionary<string, string> d)
        {
            d["chapter.ny_brooklyn.intro.agent_d"] = "Il revient sur les quais où l'on vend ce qu'on ne voit pas. Brooklyn est le premier endroit où il se montre.";
            d["chapter.ny_brooklyn.intro.agent_m"] = "Elle a vu à Moscou comment une agence peut se payer un gang. Brooklyn lui montre comment on s'en débarrasse.";
        }

        static Dictionary<string, string> BrooklynEnglish(Dictionary<string, string> d)
        {
            d["chapter.ny_brooklyn.intro.agent_d"] = "He returns to the docks where the unseen is sold. Brooklyn is the first place where he shows himself.";
            d["chapter.ny_brooklyn.intro.agent_m"] = "She saw in Moscow how an agency can buy a gang. Brooklyn shows her how one gets rid of it.";
            d["narration.ny_1_1.n23.before.0"] = "\"You crossed my kingdom of rust and salt to die on this dock. Admirable. Pointless.\"";
            d["narration.ny_1_1.n23.before.1"] = "Anchor, you're the wall. Frost, Echo: cover him. This night is ours, or no one will see it end.";
            d["narration.ny_1_1.n23.after.0"] = "Brick collapses among the containers, and the roar of the port falls silent. The cages are open, the ledgers speak: the truth is in your hands.";
            d["narration.ny_1_1.n23.after.1"] = "Leave before dawn. The AMIF will be back, and when it returns, it will find heroes.";
            d["narration.ny_1_2.n14.before.0"] = "\"Crocodile gave me the keys to hell. Come in: he's kept a seat for you.\"";
            d["narration.ny_1_2.n14.before.1"] = "Innocents behind him. A door in front of us. We don't stop, not tonight.";
            d["narration.ny_1_2.n14.after.0"] = "\"You think you've won? You've just lit the fuse. Crocodile will rain fire down on your heads.\"";
            d["narration.ny_1_2.n19.after.0"] = "The chains fall. The hostages breathe free air and, for the first time in a long while, look up at a sky that doesn't lie to them.";
            d["narration.ny_1_2.n19.after.1"] = "But the shadows are lengthening. Crocodile never forgets, and what he swears, he writes in blood.";
            d["narration.ny_1_3.n18.before.0"] = "\"All of The Dawn on my island... I wouldn't have dared dream of such a gift. Welcome to your own funeral.\"";
            d["narration.ny_1_3.n18.before.1"] = "We knew it was a trap. No matter: when the world calls, we answer. Civilians first, then him.";
            d["narration.ny_1_3.n18.after.0"] = "\"Impossible... I am... a king...\"";
            d["narration.ny_1_3.n18.after.1"] = "Crocodile collapses, and the dark rides of Coney Island fall silent. Victory tastes of ash: some wounds only heal as legend.";
            d["narration.ny_1_4.n18.before.0"] = "\"You're surrounded. Lay down your powers at the feet of History, and perhaps History will spare you.\"";
            d["narration.ny_1_4.n18.before.1"] = "Hold the line. A bridge is only an oath in stone: today, we are the ones holding it.";
            d["narration.ny_1_4.n18.after.0"] = "\"Enough! Stroke, I have enough to put the whole city face to face with your little secrets. Shall I tell them here, in front of your men?\"";
            d["narration.ny_1_4.n18.after.1"] = "\"... Fall back. Everyone.\"";
            d["narration.ny_1_4.n18.after.2"] = "\"Crocodile will carry the crime. One more shadow in the picture: he will make an admirable culprit.\"";
            d["narration.ny_1_4.n18.after.3"] = "\"Remember this day, Rayner. When the Salem Accords are signed, The Dawn will answer for everything.\"";
            d["fragment.redhook_docks.title"] = "Cages Under the Cranes";
            d["fragment.redhook_docks.text"] = "There were cages on the dock, with numbers painted on them. You understood that the people being chained had no name to the ones selling them. That night, The Dawn became a name.";
            d["fragment.brownsville_hostages.title"] = "Crocodile's Cells";
            d["fragment.brownsville_hostages.text"] = "Behind every door, a name no one dared say. You opened the cells one by one, and each time someone looked up as though the light were a promise.";
            d["fragment.coney_trap.title"] = "A King on an Island";
            d["fragment.coney_trap.text"] = "He waited for you at the end of his trap, and you walked in anyway. The civilians got out before the last burst of laughter. Some victories smell of ash.";
            d["fragment.dumbo_bridge.title"] = "The Stone Bridge";
            d["fragment.dumbo_bridge.text"] = "A bridge is only an oath in stone. You held it until someone else spoke the name you didn't dare say. Stroke backed down, and you understood the war was only beginning.";
            d["speaker.brick"] = "Brick";
            d["speaker.lieutenant"] = "Crocodile's Lieutenant";
            d["speaker.crocodile"] = "Crocodile";
            d["speaker.general"] = "The General";
            return d;
        }
    }
}
