using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Liens entre agents (niveau de synergie) et effets uniques de composition d'équipe (notes d'amélioration du combat, 2026-10-22).</summary>
    public sealed class LinkAndTeamEffectTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        static CombatSimulation Sim(string[] allies, params LinkEntry[] links)
            => new CombatSimulation(C, new CombatSetup(11UL, CombatRank.Normal,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                new[] { new EnemyEntry(new EnemyId("thug"), 5) }, false).WithLinks(links));

        [TestCase(0, 1), TestCase(2, 1), TestCase(3, 2), TestCase(7, 2), TestCase(8, 3), TestCase(15, 4), TestCase(25, 5), TestCase(500, 5)]
        [Description("Vérifie les paliers du lien : 3, 8, 15 et 25 victoires communes pour les niveaux 2 à 5, plafonné au niveau 5 — parce que la progression doit être lisible pour le joueur")]
        public void Level_FollowsTheThresholds(int wins, int expected) => Assert.That(LinkRules.Level(wins), Is.EqualTo(expected));

        [Test, Description("Vérifie que chaque niveau de lien ajoute 25 % à l'effet passif (100, 125, 150, 175, 200 %) et que les victoires restantes avant le niveau suivant sont justes — parce que l'interface les affiche")]
        public void Multiplier_AndWinsToNext()
        {
            Assert.That(Enumerable.Range(1, 5).Select(LinkRules.MultiplierPercent), Is.EqualTo(new[] { 100, 125, 150, 175, 200 }));
            Assert.That(LinkRules.WinsToNext(0), Is.EqualTo(3));
            Assert.That(LinkRules.WinsToNext(7), Is.EqualTo(1));
            Assert.That(LinkRules.WinsToNext(25), Is.EqualTo(0));
        }

        [Test, Description("Vérifie que le niveau de lien multiplie le bonus de la synergie (6 % au niveau 3 : 9 %) et reste soumis au plafond — parce que le lien amplifie l'effet passif sans le rendre infini")]
        public void Total_IsScaledByTheLinkLevel_AndStillCapped()
        {
            var s = new SynergyDef("s", "n", Family.Matter, Family.Elemental, SynergyStat.Atk, 6);
            Assert.That(SynergyRules.Total(new[] { (s, 3) }).Atk, Is.EqualTo(9));
            Assert.That(SynergyRules.Total(new[] { (s, 1) }).Atk, Is.EqualTo(6));
            Assert.That(SynergyRules.Total(Enumerable.Repeat((s, 5), 10)).Atk, Is.EqualTo(SynergyRules.CapPercent));
        }

        [Test, Description("Vérifie qu'un lien de niveau 5 entre deux agents double le bonus de PV de leur résonance de famille en combat — parce que le niveau de synergie augmente l'effet du bonus passif")]
        public void Combat_UsesTheLinkLevelOfThePair()
        {
            var alone = Sim(new[] { "ancre" }).Unit(0).MaxHp;
            var level1 = Sim(new[] { "ancre", "seve" }).Unit(0).MaxHp;          // +4 % de PV
            var level5 = Sim(new[] { "ancre", "seve" }, new LinkEntry(new MemberId("ancre"), new MemberId("seve"), 5)).Unit(0).MaxHp; // +8 %
            Assert.That(level1, Is.EqualTo(alone * 104 / 100).Within(2));
            Assert.That(level5, Is.EqualTo(alone * 108 / 100).Within(2));
        }

        [Test, Description("Vérifie la détection des quatre effets de composition : quatre familles, tank + dégâts + soutien, deux tanks, deux contrôles — parce que chaque composition doit déclencher son effet unique")]
        public void TeamEffects_AreDetectedFromTheComposition()
        {
            var kinds = TeamEffects.Active(new[] { Family.PhysicalBio, Family.Matter, Family.Elemental, Family.Perception },
                                           new[] { Role.Tank, Role.Control, Role.Damage, Role.Support });
            Assert.That(kinds, Is.EquivalentTo(new[] { TeamEffectKind.Quartet, TeamEffectKind.Trinity }));
            Assert.That(TeamEffects.Active(new[] { Family.Matter, Family.Matter }, new[] { Role.Tank, Role.Tank }), Is.EqualTo(new[] { TeamEffectKind.DoubleFront }));
            Assert.That(TeamEffects.Active(new[] { Family.Matter, Family.Elemental }, new[] { Role.Control, Role.Control }), Is.EqualTo(new[] { TeamEffectKind.Spark }));
            Assert.That(TeamEffects.Active(new[] { Family.Matter }, new[] { Role.Damage }), Is.Empty);
        }

        [Test, Description("Vérifie que les effets de composition s'appliquent aux alliés en combat : critique du Quatuor, régénération d'énergie de la Trinité, bouclier du Double front, énergie de départ de l'Étincelle — parce qu'un effet affiché doit exister")]
        public void TeamEffects_AreAppliedToEveryAlly()
        {
            var quartet = Sim(new[] { "ancre", "verre", "givre", "echo" });
            Assert.That(quartet.Units.Where(u => u.Side == Side.Ally).All(u => u.CritBonusPercent >= TeamEffects.QuartetCritPercent), Is.True);

            var trinity = Sim(new[] { "ancre", "givre", "seve" });
            Assert.That(trinity.Units.Where(u => u.Side == Side.Ally).All(u => u.EnergyRegenBonus == TeamEffects.TrinityEnergyRegen), Is.True);

            var wall = Sim(new[] { "ancre", "rempart" });
            Assert.That(wall.Units.Where(u => u.Side == Side.Ally).All(u => u.Has(StatusType.Shield)), Is.True);

            var spark = Sim(new[] { "verre", "etincelle" });
            Assert.That(spark.Units.Where(u => u.Side == Side.Ally).All(u => u.Energy == C.Balance.EnergyStart + TeamEffects.SparkStartEnergy), Is.True);

            var none = Sim(new[] { "givre" });
            Assert.That(none.Unit(0).CritBonusPercent, Is.EqualTo(0));
            Assert.That(none.Unit(0).Energy, Is.EqualTo(C.Balance.EnergyStart));
        }

        [Test, Description("Vérifie que la Trinité accélère vraiment la régénération d'énergie d'un pas — parce que +1 par pas doit se voir en combat")]
        public void Trinity_RegeneratesOneMoreEnergyPerTick()
        {
            var s = Sim(new[] { "ancre", "givre", "seve" });
            foreach (var u in s.Units.Where(x => x.Side == Side.Enemy)) u.Point = 0;
            var ally = s.Unit(0);
            ally.Point = 0;
            int before = ally.Energy;
            s.Step(1);
            Assert.That(ally.Energy, Is.EqualTo(before + C.Balance.EnergyRegenPerTick + TeamEffects.TrinityEnergyRegen));
        }

        [Test, Description("Vérifie que chaque victoire fait progresser toutes les paires d'agents engagés, quel que soit l'ordre des identifiants, et que seuls les liens de niveau 2 et plus sont transmis au combat — parce que le lien se gagne en combattant ensemble")]
        public void RecordVictory_ProgressesEveryPair()
        {
            var meta = new MetaSave();
            for (int i = 0; i < 3; i++) TeamLinks.RecordVictory(meta, new[] { "ancre", "givre", "echo" });
            Assert.That(TeamLinks.Wins(meta, "ancre", "givre"), Is.EqualTo(3));
            Assert.That(TeamLinks.Wins(meta, "givre", "ancre"), Is.EqualTo(3), "paire non ordonnée");
            Assert.That(meta.links.Count, Is.EqualTo(3), "trois paires");
            Assert.That(TeamLinks.Level(meta, "ancre", "echo"), Is.EqualTo(2));
            Assert.That(TeamLinks.Wins(meta, "ancre", "seve"), Is.EqualTo(0));
            var entries = TeamLinks.Entries(meta, new[] { "ancre", "givre", "seve" });
            Assert.That(entries.Count, Is.EqualTo(1));
            Assert.That(entries[0].Level, Is.EqualTo(2));
            TeamLinks.RecordVictory(meta, new[] { "ancre" });
            Assert.That(meta.links.Count, Is.EqualTo(3), "un agent seul ne crée aucun lien");
        }
    }
}
