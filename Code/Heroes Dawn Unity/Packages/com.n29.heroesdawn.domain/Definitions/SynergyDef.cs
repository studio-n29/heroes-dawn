using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Statistique que gonfle une synergie.</summary>
    public enum SynergyStat { Hp, Atk, Def, Spd }

    /// <summary>
    /// Synergie d'équipe : une paire de familles présente dans l'escouade donne à toute l'équipe un bonus permanent de
    /// <see cref="Percent"/> % sur <see cref="Stat"/> pendant le combat. Il n'y a rien à lancer.
    /// </summary>
    public sealed class SynergyDef
    {
        public string Id { get; }
        public string NameKey { get; }
        public Family A { get; }
        public Family B { get; }
        public SynergyStat Stat { get; }
        public int Percent { get; }

        public SynergyDef(string id, string nameKey, Family a, Family b, SynergyStat stat, int percent)
        {
            Id = id; NameKey = nameKey; A = a; B = b; Stat = stat; Percent = percent;
        }

        public bool Matches(Family x, Family y) => (x == A && y == B) || (x == B && y == A);
    }
}
