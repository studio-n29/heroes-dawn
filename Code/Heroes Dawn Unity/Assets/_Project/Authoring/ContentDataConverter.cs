#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Passage entre les fiches éditables (JSON) et les définitions du jeu, dans les deux sens.</summary>
    public static class ContentDataConverter
    {
        public static EffectDef ToDef(EffectRecord e) => new EffectDef(e.type, e.value, e.status, e.duration, e.chance);

        public static EffectRecord FromDef(EffectDef e)
            => new EffectRecord { type = e.Type, value = e.Value, status = e.Status, duration = e.Duration, chance = e.ChancePercent };

        public static SkillDef ToDef(SkillRecord s)
            => new SkillDef(new SkillId(s.id), s.slot, s.cooldown, s.target, s.effects.Select(ToDef).ToList());

        public static SkillRecord FromDef(SkillDef s, string name)
            => new SkillRecord { id = s.Id.Value, name = name, slot = s.Slot, cooldown = s.Cooldown, target = s.Target, effects = s.Effects.Select(FromDef).ToList() };

        public static ItemDef ToDef(ItemRecord i)
            => new ItemDef(new ItemId(i.id), "items." + i.id + ".name", i.rarity, i.hp, i.atk, i.def, i.spd, i.perk, i.perkValue, i.price);

        public static ItemRecord FromDef(ItemDef i, string name)
            => new ItemRecord { id = i.Id.Value, name = name, rarity = i.Rarity, hp = i.HpPercent, atk = i.AtkPercent, def = i.DefPercent, spd = i.SpdPercent,
                                perk = i.Perk, perkValue = i.PerkValue, price = i.Price };

        /// <summary>Agent du jeu à partir de sa fiche ; les compétences absentes sont ignorées (la validation les signale).</summary>
        public static MemberDef ToDef(AgentRecord a, IReadOnlyDictionary<string, SkillDef> skills)
        {
            var kit = a.skills.Where(id => skills.ContainsKey(id)).Select(id => skills[id]).ToList();
            var zone = a.unlockZone.Length > 0 ? new ZoneId(a.unlockZone) : (ZoneId?)null;
            UnlockDef? unlock = null;
            switch (a.unlockKind)
            {
                case UnlockKind.Arc: if (zone.HasValue) unlock = UnlockDef.Arc(zone.Value); break;
                case UnlockKind.Event: if (zone.HasValue) unlock = UnlockDef.WeekendEvent(zone.Value); break;
                case UnlockKind.Feat: if (zone.HasValue) unlock = UnlockDef.FeatOf(zone.Value, a.feat); break;
            }
            var shopZone = a.unlockKind == UnlockKind.Shop ? zone : null;
            return new MemberDef(new MemberId(a.id), "members." + a.id + ".name", a.family, a.role, a.rarity, new StatBlock(a.hp, a.atk, a.def, a.spd), kit, shopZone, unlock);
        }

        public static AgentRecord FromDef(MemberDef m, string name)
        {
            var kind = m.Unlock?.Kind ?? UnlockKind.Shop;
            var zone = m.Unlock?.Zone ?? m.UnlockZone;
            return new AgentRecord
            {
                id = m.Id.Value, name = name, family = m.Family, role = m.Role, rarity = m.Rarity,
                hp = m.BaseStats.Hp, atk = m.BaseStats.Atk, def = m.BaseStats.Def, spd = m.BaseStats.Spd,
                unlockKind = kind, unlockZone = zone.HasValue ? zone.Value.Value : "", feat = m.Unlock?.Feat ?? FeatKind.None,
                skills = m.Skills.Select(s => s.Id.Value).ToList(),
            };
        }
    }
}
