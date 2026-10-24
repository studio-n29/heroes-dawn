using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Paliers de niveau : bonus passifs permanents de l'agent (par exemple +20 % d'attaque au niveau 10).</summary>
    public sealed class LevelTiersTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        [Test, Description("Vérifie qu'un agent sous le premier palier n'a aucun bonus et qu'au niveau 10 il gagne +20 % d'attaque — parce que le palier 10 est l'exemple donné par le design")]
        public void Level10_GivesTwentyPercentAttack()
        {
            Assert.That(LevelTiers.BonusAt(9).IsNone, Is.True);
            Assert.That(LevelTiers.BonusAt(10).Atk, Is.EqualTo(20));
            Assert.That(LevelTiers.BonusAt(10).Hp, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que les bonus se cumulent : au niveau 50 l'agent a les cinq paliers — parce que monter de niveau ne retire jamais un bonus")]
        public void Bonuses_Accumulate()
        {
            var max = LevelTiers.BonusAt(50);
            Assert.That(max.Atk, Is.EqualTo(40));
            Assert.That(max.Hp, Is.EqualTo(20));
            Assert.That(max.Def, Is.EqualTo(20));
            Assert.That(max.Spd, Is.EqualTo(10));
        }

        [Test, Description("Vérifie qu'un allié de niveau 10 a plus d'attaque que ce que donne la seule courbe de niveau — parce que le palier doit s'appliquer en combat")]
        public void Tier_IsAppliedToTheCombatUnit()
        {
            var member = C.Members.First();
            var b = C.Balance;
            var unit = UnitFactory.Ally(1, member, new AllyEntry(member.Id, 10, 0), b);
            int curve = UnitFactory.ScaleStat(member.BaseStats.Atk, 10, b);
            Assert.That(unit.Atk, Is.EqualTo(curve * 120 / 100));
            var below = UnitFactory.Ally(2, member, new AllyEntry(member.Id, 9, 0), b);
            Assert.That(below.Atk, Is.EqualTo(UnitFactory.ScaleStat(member.BaseStats.Atk, 9, b)));
        }
    }
}
