#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class MemberDef
    {
        public MemberId Id { get; }
        public string NameKey { get; }
        public Family Family { get; }
        public Position Position { get; }
        /// <summary>Rang AMIF de 1 à 5 (voir <see cref="AgentRanks"/>) : remplace la rareté d agent et la puissance.</summary>
        public int Rank { get; }
        public StatBlock BaseStats { get; }
        public IReadOnlyList<SkillDef> Skills { get; }
        /// <summary>Zone à valider avant que la recrue entre dans la boutique ; nul = toujours disponible.</summary>
        public ZoneId? UnlockZone { get; }
        /// <summary>Comment l agent rejoint le joueur ; nul = boutique de recrutement.</summary>
        public UnlockDef? Unlock { get; }
        /// <summary>Vrai si l agent entre dans la boutique de recrutement (pas d arc, d evenement ni de prouesse).</summary>
        public bool InShop => Unlock == null || Unlock.Kind == UnlockKind.Shop;

        public MemberDef(MemberId id, string nameKey, Family family, Position position, int rank, StatBlock baseStats, IReadOnlyList<SkillDef> skills,
                         ZoneId? unlockZone = null, UnlockDef? unlock = null)
        {
            Id = id; NameKey = nameKey; Family = family; Position = position; Rank = AgentRanks.Clamp(rank); BaseStats = baseStats; Skills = skills;
            UnlockZone = unlockZone; Unlock = unlock;
        }
    }
}
