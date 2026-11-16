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
        public static EffectDef ToDef(EffectRecord e)
            => new EffectDef(e.type, e.value, e.status, e.duration, e.chance,
                             e.hasCondition ? new Condition(e.condKind, e.condStatus, e.condValue) : null,
                             e.mods.Count == 0 ? null : e.mods.Select(ToDef).ToList(),
                             e.hasReaction ? ToReaction(e) : null);

        /// <summary>Réaction décrite par les champs plats « reaction… » ; son effet imbriqué n'a ni condition, ni modificateur, ni réaction (la validation exige des champs plats seuls).</summary>
        static ReactionDef ToReaction(EffectRecord e)
            => new ReactionDef(e.reactionTrigger, e.reactionStatus, e.reactionTurns,
                               new EffectDef(e.reactionEffectType, e.reactionEffectValue, e.reactionEffectStatus, e.reactionEffectDuration, e.reactionEffectChance),
                               e.reactionTarget, e.reactionPick, e.reactionPickStatus, e.reactionOnTriggerUnit);

        public static EffectRecord FromDef(EffectDef e)
        {
            var record = new EffectRecord
            {
                type = e.Type, value = e.Value, status = e.Status, duration = e.Duration, chance = e.ChancePercent,
                hasCondition = e.When != null, condKind = e.When?.Kind ?? default, condStatus = e.When?.Status ?? StatusType.None, condValue = e.When?.Value ?? 0,
                mods = e.Mods.Select(FromDef).ToList(),
            };
            var r = e.Reaction;
            if (r == null) return record;
            record.hasReaction = true;
            record.reactionTrigger = r.Trigger; record.reactionStatus = r.Status; record.reactionTurns = r.Turns;
            record.reactionEffectType = r.Effect.Type; record.reactionEffectValue = r.Effect.Value; record.reactionEffectStatus = r.Effect.Status;
            record.reactionEffectDuration = r.Effect.Duration; record.reactionEffectChance = r.Effect.ChancePercent;
            record.reactionTarget = r.Target; record.reactionPick = r.Pick; record.reactionPickStatus = r.PickStatus; record.reactionOnTriggerUnit = r.OnTriggerUnit;
            return record;
        }

        public static ValueMod ToDef(ModRecord m)
            => new ValueMod(m.bonus, m.hasCondition ? new Condition(m.condKind, m.condStatus, m.condValue) : null,
                            m.hasScale ? m.per : (ScaleKind?)null, m.hasScale ? m.maxCount : 0,
                            m.hasArchetype ? m.perArchetype : (Archetype?)null);

        public static ModRecord FromDef(ValueMod m)
            => new ModRecord
            {
                hasCondition = m.When != null, condKind = m.When?.Kind ?? default, condStatus = m.When?.Status ?? StatusType.None, condValue = m.When?.Value ?? 0,
                bonus = m.Bonus, hasScale = m.Per.HasValue, per = m.Per ?? default, maxCount = m.MaxCount,
                hasArchetype = m.PerArchetype.HasValue, perArchetype = m.PerArchetype ?? default,
            };

        public static SkillDef ToDef(SkillRecord s)
            => new SkillDef(new SkillId(s.id), s.slot, s.cooldown, s.target, s.effects.Select(ToDef).ToList(), s.cost, s.pick, s.pickStatus);

        public static SkillRecord FromDef(SkillDef s, string name)
            => new SkillRecord
            {
                id = s.Id.Value, name = name, slot = s.Slot, cost = s.Cost, cooldown = s.Cooldown, target = s.Target, pick = s.Pick, pickStatus = s.PickStatus,
                effects = s.Effects.Select(FromDef).ToList(),
            };

        /// <summary>Carte du grimoire à partir de sa fiche : étoiles, famille, nature, archétypes, rôle, compétence et paliers (seule capacité : OnCast).</summary>
        public static SkillCardDef ToDef(CardRecord c)
            => new SkillCardDef(new CardId(c.id), "cards." + c.id + ".name", c.family, c.stars, ToDef(c.skill), c.nature,
                                c.tiers.Select(t => new CardTier(t.level, t.effects.Select(ToDef).ToArray())).ToList(), c.archetypes.ToList(), c.role);

        public static CardRecord FromDef(SkillCardDef c)
            => new CardRecord
            {
                id = c.Id.Value, family = c.Family, stars = c.Stars, nature = c.Nature, archetypes = c.Archetypes.ToList(), role = c.Role,
                skill = FromDef(c.Skill, ""),
                tiers = c.Tiers.Select(t => new TierRecord { level = t.Level, effects = t.Effects.Select(FromDef).ToList() }).ToList(),
            };

        public static ItemDef ToDef(ItemRecord i)
            => new ItemDef(new ItemId(i.id), "items." + i.id + ".name", i.rarity, i.hp, i.atk, i.def, i.spd, i.perk, i.perkValue, i.price,
                           System.Enum.IsDefined(typeof(ItemType), i.type) ? i.type : (ItemType?)null);

        public static ItemRecord FromDef(ItemDef i, string name)
            => new ItemRecord { id = i.Id.Value, name = name, rarity = i.Rarity, hp = i.HpPercent, atk = i.AtkPercent, def = i.DefPercent, spd = i.SpdPercent,
                                perk = i.Perk, perkValue = i.PerkValue, price = i.Price, type = i.Type };

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
                case UnlockKind.Hero: unlock = UnlockDef.Hero(); break;
            }
            var shopZone = a.unlockKind == UnlockKind.Shop ? zone : null;
            return new MemberDef(new MemberId(a.id), "members." + a.id + ".name", a.family, a.position, a.rank, new StatBlock(a.hp, a.atk, a.def, a.spd), kit, shopZone, unlock);
        }

        public static AgentRecord FromDef(MemberDef m, string name)
        {
            var kind = m.Unlock?.Kind ?? UnlockKind.Shop;
            var zone = m.Unlock?.Zone ?? m.UnlockZone;
            return new AgentRecord
            {
                id = m.Id.Value, name = name, family = m.Family, position = m.Position, rank = m.Rank,
                hp = m.BaseStats.Hp, atk = m.BaseStats.Atk, def = m.BaseStats.Def, spd = m.BaseStats.Spd,
                unlockKind = kind, unlockZone = zone.HasValue ? zone.Value.Value : "", feat = m.Unlock?.Feat ?? FeatKind.None,
                skills = m.Skills.Select(s => s.Id.Value).ToList(),
            };
        }
    }
}
