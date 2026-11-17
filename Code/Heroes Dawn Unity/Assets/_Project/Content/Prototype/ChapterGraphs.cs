using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Deux plateaux supplémentaires pour les chapitres suivants de New York et de Moscou, en plus des graphes A (Brownsville) et B (pont) de
    /// <see cref="PrototypeZones"/> : C « Trois voies » (23 cases) et D « Boucle » (22 cases). Même vocabulaire que A et B : case d'entrée en n00,
    /// élite fixe, mini-jeux fixes (cibles d'objectifs), sortie ou boss fixe en dernière case, un mini-jeu libre « potion_field ».
    /// </summary>
    internal static class ChapterGraphs
    {
        static NodeDef N(string id, CaseType t, int x, int y, bool fixedNode = false, CombatRank r = CombatRank.Normal, string activity = null, NarrationDef narration = null)
            => PrototypeZones.N(id, t, x, y, fixedNode, r, activity, narration);

        /// <summary>
        /// Graphe C « Trois voies » : après n02, trois couloirs parallèles (nord, centre, sud) qui se rejoignent en n12, puis une seconde fourche (n13/n15) jusqu'à n17.
        /// Cases fixes : n04 (récit, scène d entrée), n13 (mini-jeu A), n14 (élite), n17 (carrefour ; scène intermédiaire facultative <c>midTalk</c>, ex. la scène « stage » de Manhattan 4.3), n21 (mini-jeu B), n22 (boss ou sortie).
        /// </summary>
        public static List<NodeDef> ThreeLanesNodes(NarrationDef eliteTalk, NarrationDef endTalk, string miniGameA, string miniGameB, bool bossAtEnd, NarrationDef introTalk = null, NarrationDef midTalk = null)
            => new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 3, true),
                N("n01", CaseType.Resource, 1, 3), N("n02", CaseType.Combat, 2, 3, r: CombatRank.Easy),
                N("n03", CaseType.Resource, 3, 1), N("n04", CaseType.Narrative, 4, 1, true, narration: introTalk), N("n05", CaseType.Combat, 5, 1),
                N("n06", CaseType.MiniGame, 3, 3, activity: "potion_field"), N("n07", CaseType.Combat, 4, 3, r: CombatRank.Easy), N("n08", CaseType.Resource, 5, 3),
                N("n09", CaseType.Resource, 3, 5), N("n10", CaseType.Rest, 4, 5), N("n11", CaseType.Combat, 5, 5),
                N("n12", CaseType.Combat, 6, 3),
                N("n13", CaseType.MiniGame, 7, 2, true, activity: miniGameA), N("n14", CaseType.Combat, 8, 2, true, CombatRank.Elite, narration: eliteTalk),
                N("n15", CaseType.Resource, 7, 4), N("n16", CaseType.Narrative, 8, 4),
                N("n17", CaseType.Empty, 9, 3, true, narration: midTalk),
                N("n18", CaseType.Resource, 10, 3), N("n19", CaseType.Combat, 11, 2), N("n20", CaseType.Rest, 11, 4),
                N("n21", CaseType.MiniGame, 12, 3, true, activity: miniGameB),
                bossAtEnd ? N("n22", CaseType.Combat, 13, 3, true, CombatRank.Boss, narration: endTalk) : N("n22", CaseType.Empty, 13, 3, true, narration: endTalk),
            };

        public static List<LinkDef> ThreeLanesLinks()
            => PrototypeZones.Links(("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"), ("n02", "n09"),
                                    ("n03", "n04"), ("n04", "n05"), ("n06", "n07"), ("n07", "n08"), ("n09", "n10"), ("n10", "n11"),
                                    ("n05", "n12"), ("n08", "n12"), ("n11", "n12"), ("n12", "n13"), ("n12", "n15"),
                                    ("n13", "n14"), ("n15", "n16"), ("n14", "n17"), ("n16", "n17"), ("n17", "n18"),
                                    ("n18", "n19"), ("n18", "n20"), ("n19", "n21"), ("n20", "n21"), ("n21", "n22"));

        /// <summary>
        /// Graphe D « Boucle » : deux arcs (nord et sud) qui se rejoignent en n09, une échelle à barreaux (n11-n12, n13-n14) puis une seconde boucle jusqu'à n20.
        /// Cases fixes : n03 (récit), n11 (mini-jeu A), n13 (élite), n15 (carrefour), n20 (mini-jeu B), n21 (boss ou sortie).
        /// </summary>
        public static List<NodeDef> LoopNodes(NarrationDef eliteTalk, NarrationDef endTalk, string miniGameA, string miniGameB, bool bossAtEnd, NarrationDef introTalk = null)
            => new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 3, true),
                N("n01", CaseType.Resource, 1, 3), N("n02", CaseType.Combat, 2, 3, r: CombatRank.Easy),
                N("n03", CaseType.Narrative, 3, 1, true, narration: introTalk), N("n04", CaseType.Combat, 4, 0), N("n05", CaseType.Resource, 5, 1),
                N("n06", CaseType.Resource, 3, 5), N("n07", CaseType.MiniGame, 4, 6, activity: "potion_field"), N("n08", CaseType.Combat, 5, 5, r: CombatRank.Easy),
                N("n09", CaseType.Rest, 6, 3), N("n10", CaseType.Combat, 7, 3),
                N("n11", CaseType.MiniGame, 8, 2, true, activity: miniGameA), N("n12", CaseType.Resource, 8, 4),
                N("n13", CaseType.Combat, 9, 2, true, CombatRank.Elite, narration: eliteTalk), N("n14", CaseType.Narrative, 9, 4),
                N("n15", CaseType.Empty, 10, 3, true),
                N("n16", CaseType.Combat, 11, 2), N("n17", CaseType.Rest, 12, 2),
                N("n18", CaseType.Resource, 11, 4), N("n19", CaseType.Combat, 12, 4),
                N("n20", CaseType.MiniGame, 13, 3, true, activity: miniGameB),
                bossAtEnd ? N("n21", CaseType.Combat, 14, 3, true, CombatRank.Boss, narration: endTalk) : N("n21", CaseType.Empty, 14, 3, true, narration: endTalk),
            };

        public static List<LinkDef> LoopLinks()
            => PrototypeZones.Links(("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"),
                                    ("n03", "n04"), ("n04", "n05"), ("n06", "n07"), ("n07", "n08"), ("n05", "n09"), ("n08", "n09"),
                                    ("n09", "n10"), ("n10", "n11"), ("n10", "n12"), ("n11", "n12"), ("n11", "n13"), ("n12", "n14"), ("n13", "n14"),
                                    ("n13", "n15"), ("n14", "n15"), ("n15", "n16"), ("n15", "n18"), ("n16", "n17"), ("n18", "n19"),
                                    ("n17", "n20"), ("n19", "n20"), ("n20", "n21"));
    }
}
