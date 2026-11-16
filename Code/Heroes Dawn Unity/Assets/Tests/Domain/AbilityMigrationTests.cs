#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Migration vers les capacités (P0) : tout le catalogue passe par le constructeur historique, donc une capacité OnCast unique.</summary>
    public sealed class AbilityMigrationTests
    {
        static readonly ContentCatalog Catalog = TestContent.Build();

        static IEnumerable<(string Origin, SkillDef Skill)> AllSkills()
        {
            foreach (var m in Catalog.Members) foreach (var s in m.Skills) yield return ("agent " + m.Id.Value, s);
            foreach (var e in Catalog.Enemies)
            {
                foreach (var s in e.Skills) yield return ("ennemi " + e.Id.Value, s);
                foreach (var p in e.Phases)
                    if (p.Skills != null) foreach (var s in p.Skills) yield return ("phase de boss " + e.Id.Value + " " + p.NameKey, s);
            }
            foreach (var c in Catalog.Collection.Cards) yield return ("carte " + c.Id.Value, c.Skill);
        }

        static (EffectType, int, StatusType, int, int) Key(EffectDef e) => (e.Type, e.Value, e.Status, e.Duration, e.ChancePercent);

        [Test, Description("Vérifie que chaque compétence du catalogue (agents, ennemis, phases de boss, cartes) a une seule capacité OnCast dont les effets sont ceux de Effects — parce qu'aucune compétence existante ne doit contourner l'enveloppe OnCast de la migration")]
        public void EverySkillOfTheCatalog_HasASingleOnCastAbility()
        {
            var all = AllSkills().ToList();
            Assert.That(all.Count, Is.GreaterThan(0));
            Assert.That(all.Any(x => x.Origin.StartsWith("agent ")), Is.True);
            Assert.That(all.Any(x => x.Origin.StartsWith("ennemi ")), Is.True);
            Assert.That(all.Any(x => x.Origin.StartsWith("phase de boss ")), Is.True, "le catalogue doit contenir des kits de phase de boss");
            Assert.That(all.Any(x => x.Origin.StartsWith("carte ")), Is.True);
            foreach (var (origin, s) in all)
            {
                Assert.That(s.Abilities.Count, Is.EqualTo(1), origin + " / " + s.Id.Value);
                Assert.That(s.Abilities[0].Trigger, Is.EqualTo(AbilityTrigger.OnCast), origin + " / " + s.Id.Value);
                Assert.That(s.Effects.Select(Key), Is.EqualTo(s.Abilities[0].Effects.Select(Key)), origin + " / " + s.Id.Value);
            }
        }

        [Test, Description("Vérifie que chaque palier de chaque carte porte une seule capacité OnCast avec ses effets — parce que les paliers s'ajoutent à la compétence comme capacités OnCast supplémentaires")]
        public void EveryTierOfEveryCard_HasASingleOnCastAbility()
        {
            var tiers = Catalog.Collection.Cards.SelectMany(c => c.Tiers.Select(t => (Card: c, Tier: t))).ToList();
            Assert.That(tiers.Count, Is.GreaterThan(0), "le catalogue doit contenir des paliers de carte");
            foreach (var (card, tier) in tiers)
            {
                string where = card.Id.Value + " palier " + tier.Level;
                Assert.That(tier.Abilities.Count, Is.EqualTo(1), where);
                Assert.That(tier.Abilities[0].Trigger, Is.EqualTo(AbilityTrigger.OnCast), where);
                Assert.That(tier.Abilities[0].Effects.Select(Key), Is.EqualTo(tier.Effects.Select(Key)), where);
            }
        }

        [Test, Description("Vérifie que la conversion compétence vers fiche JSON puis retour garde effets (type, valeur, statut, durée, chance) et structure — parce que les outils de design ne doivent rien perdre des compétences du jeu")]
        public void ContentDataConverter_RoundTrip_KeepsEffects()
        {
            foreach (var (origin, s) in AllSkills())
            {
                var back = ContentDataConverter.ToDef(ContentDataConverter.FromDef(s, s.Id.Value));
                string where = origin + " / " + s.Id.Value;
                Assert.That(back.Id, Is.EqualTo(s.Id), where);
                Assert.That(back.Slot, Is.EqualTo(s.Slot), where);
                Assert.That(back.Cooldown, Is.EqualTo(s.Cooldown), where);
                Assert.That(back.Cost, Is.EqualTo(s.Cost), where);
                Assert.That(back.Target, Is.EqualTo(s.Target), where);
                Assert.That(back.Abilities.Count, Is.EqualTo(1), where);
                Assert.That(back.Abilities[0].Trigger, Is.EqualTo(AbilityTrigger.OnCast), where);
                Assert.That(back.Effects.Select(Key), Is.EqualTo(s.Effects.Select(Key)), where);
            }
        }
    }
}
