#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Données d'affichage communes d'un membre (carte, place d'escouade).</summary>
    public static class MemberCards
    {
        public static MemberCardData Card(MemberDef def, int level, BalanceConfig b, ILocalization loc)
        {
            var name = loc.MemberName(def);
            return new MemberCardData
            {
                Id = def.Id,
                Name = name.ToUpperInvariant(),
                Family = def.Family,
                RarityText = loc.RarityName(def.Rarity),
                Rarity = def.Rarity,
                RoleLine = loc.RoleName(def.Role) + " · " + loc.FamilyName(def.Family),
                LevelText = loc.F("ui.level", level),
                Stats = loc.F("hub.stats", UnitFactory.ScaleStat(def.BaseStats.Hp, level, b), UnitFactory.ScaleStat(def.BaseStats.Atk, level, b),
                              UnitFactory.ScaleStat(def.BaseStats.Def, level, b), def.BaseStats.Spd),
                Skills = string.Join(" · ", def.Skills.Select(loc.SkillName)),
                Portrait = new PortraitData(name, def.Family, art: AgentArt.For(def.Id.Value)),
            };
        }

        public static SquadSlotData Slot(ITeamMemberReadModel m, ILocalization loc)
        {
            var name = loc.MemberName(m.Def);
            return new SquadSlotData { Name = name, Portrait = new PortraitData(name, m.Def.Family, art: AgentArt.For(m.Def.Id.Value)), Fatigue01 = m.Fatigue / 100f, Fatigue = m.Fatigue, Exhausted = m.Exhausted };
        }
    }
}
