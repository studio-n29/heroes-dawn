using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Objets laissés par les combats gagnés, et objets qui renforcent les compétences.</summary>
    public sealed class CombatDropTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        static int Drops(CombatRank rank, bool boss, int tries)
        {
            int n = 0;
            for (int i = 0; i < tries; i++)
                if (CollectionRules.RollCombatDrop(C.Collection, new Pcg32(CollectionRules.Seed("p", "drop", i)), rank, boss) != null) n++;
            return n;
        }

        [Test, Description("Vérifie qu'un boss laisse toujours un objet et que la chance monte avec le rang du combat — parce que les objets s'obtiennent en combat et que les combats difficiles récompensent plus")]
        public void DropChance_RisesWithRank()
        {
            Assert.That(Drops(CombatRank.Easy, true, 50), Is.EqualTo(50), "boss : toujours");
            int easy = Drops(CombatRank.Easy, false, 600), normal = Drops(CombatRank.Normal, false, 600), elite = Drops(CombatRank.Elite, false, 600);
            Assert.That(easy, Is.LessThan(normal));
            Assert.That(normal, Is.LessThan(elite));
        }

        [Test, Description("Vérifie que le tirage est déterministe pour une même graine — parce que les sauvegardes et les tests doivent rejouer le même résultat")]
        public void Drop_IsDeterministic()
        {
            var a = CollectionRules.RollCombatDrop(C.Collection, new Pcg32(42), CombatRank.Elite, false);
            var b = CollectionRules.RollCombatDrop(C.Collection, new Pcg32(42), CombatRank.Elite, false);
            Assert.That(a?.Id, Is.EqualTo(b?.Id));
        }

        [Test, Description("Vérifie qu'un objet de puissance de compétence augmente les dégâts de toutes les compétences de l'agent — parce que les objets peuvent renforcer les compétences en plus des statistiques")]
        public void SkillPowerItem_BoostsEverySkill()
        {
            var member = C.Members.First();
            var coll = new CollectionDef(
                new[] { new ItemDef(new ItemId("test_power"), "x", ItemRarity.Rare, perk: ItemPerk.SkillPower, perkValue: 50) },
                C.Collection.Cards, C.Collection.Boosters);
            var plain = UnitFactory.Ally(1, member, new AllyEntry(member.Id, 5, 0), C.Balance, coll);
            var boosted = UnitFactory.Ally(2, member, new AllyEntry(member.Id, 5, 0, 0, new[] { new ItemId("test_power") }), C.Balance, coll);
            int Dmg(CombatUnit u) => u.Skills.SelectMany(s => s.Effects).Where(e => e.Type == EffectType.Damage).Sum(e => e.Value);
            Assert.That(Dmg(boosted), Is.GreaterThan(Dmg(plain)));
            Assert.That(boosted.Skills[0].Id, Is.EqualTo(plain.Skills[0].Id));
        }
    }
}
