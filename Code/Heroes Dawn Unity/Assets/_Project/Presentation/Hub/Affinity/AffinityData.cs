#nullable enable
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une ligne du pop-up d'affinités : un agent de l'effectif, le lien qui l'unit à l'agent choisi et le bonus unique de leur duo.</summary>
    public struct AffinityRowData
    {
        public PortraitData Portrait;
        public string Name;
        /// <summary>« Lien niv. 2 · 5 victoires ».</summary>
        public string Link;
        /// <summary>Le bonus du duo (« Point faible : +4 % Attaque »), ou « Aucune synergie ».</summary>
        public string Bonus;
        public bool HasBonus;
        /// <summary>Thème de la paire (couple de rôles), ses quatre paliers (● atteints, grisés à venir) et les victoires restantes ; texte enrichi multi-ligne.</summary>
        public string Pair;
    }

    public struct AffinityPopupData
    {
        public string Title;
        public string Subtitle;
        public string Empty;
        public string Close;
        public AffinityRowData[] Rows;
    }
}
