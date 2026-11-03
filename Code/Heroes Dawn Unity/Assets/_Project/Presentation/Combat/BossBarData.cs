#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Barre de vie élargie du boss : nom, phase en cours, PV et repères des seuils de phase.</summary>
    public readonly struct BossBarData
    {
        public string Name { get; }
        /// <summary>« PHASE 2/3 · FUREUR » ; vide si le boss n'a qu'une phase.</summary>
        public string Phase { get; }
        public float Hp01 { get; }
        public string HpText { get; }
        /// <summary>Position (0 à 1, de gauche à droite) des seuils où le boss change de phase.</summary>
        public IReadOnlyList<float> Marks { get; }

        public BossBarData(string name, string phase, float hp01, string hpText, IReadOnlyList<float> marks)
        {
            Name = name; Phase = phase; Hp01 = hp01; HpText = hpText; Marks = marks;
        }
    }
}
