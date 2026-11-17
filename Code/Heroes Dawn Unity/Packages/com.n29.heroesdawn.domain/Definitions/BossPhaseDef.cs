#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Palier d'un boss : quand ses PV passent sous <see cref="ThresholdPercent"/> % de son maximum, il entre dans la phase suivante
    /// et gagne (ou perd) ces pourcentages de force, de défense et de vitesse, de façon permanente.
    /// </summary>
    public sealed class BossPhaseDef
    {
        /// <summary>Seuil en % des PV maximum (1 à 99) ; la phase s'ouvre dès que les PV tombent à ce niveau ou en dessous.</summary>
        public int ThresholdPercent { get; }
        /// <summary>Clé de localisation du nom de la phase (ex. « Fureur du caïman »).</summary>
        public string NameKey { get; }
        public int AtkPercent { get; }
        public int DefPercent { get; }
        public int SpdPercent { get; }
        /// <summary>Nouveau jeu de compétences du boss à l ouverture de la phase ; nul : le jeu précédent est conservé.</summary>
        public IReadOnlyList<SkillDef>? Skills { get; }
        /// <summary>Nouveau style d IA à l ouverture de la phase ; nul : le style précédent est conservé.</summary>
        public AiStyle? Style { get; }
        /// <summary>Répliques du boss à l ouverture de la phase (script narratif) : lues par l interface, sans effet sur la simulation.</summary>
        public IReadOnlyList<NarrationLineDef> Lines { get; }

        public BossPhaseDef(int thresholdPercent, string nameKey, int atkPercent, int defPercent, int spdPercent,
                            IReadOnlyList<SkillDef>? skills = null, AiStyle? style = null, IReadOnlyList<NarrationLineDef>? lines = null)
        {
            ThresholdPercent = thresholdPercent; NameKey = nameKey; AtkPercent = atkPercent; DefPercent = defPercent; SpdPercent = spdPercent;
            Skills = skills; Style = style; Lines = lines ?? System.Array.Empty<NarrationLineDef>();
        }
    }
}
