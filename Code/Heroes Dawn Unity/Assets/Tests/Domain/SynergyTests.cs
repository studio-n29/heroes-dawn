using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Synergies d'équipe : boost permanent donné par les paires de familles de l'escouade (remplace les combos, version 0.9.0).</summary>
    public sealed class SynergyTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static CombatSimulation Sim(params string[] allies)
            => new CombatSimulation(C, new CombatSetup(11UL, CombatRank.Normal,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                new[] { new EnemyEntry(new EnemyId("thug"), 5) }, false));

        [Test, Description("Vérifie que chaque paire de membres dont les familles forment une synergie est listée une fois — parce que les synergies se comptent par paire d'agents")]
        public void Active_ListsEachMatchingPairOnce()
        {
            var families = new[] { Family.Matter, Family.Elemental, Family.PhysicalBio };
            var active = SynergyRules.Active(families, C.Synergies);
            Assert.That(active.Count, Is.EqualTo(3));
            Assert.That(active.All(a => a.Synergy.Matches(families[a.A], families[a.B])), Is.True);
        }

        [Test, Description("Vérifie que le cumul d'une statistique est plafonné — parce qu'une escouade de même famille ne doit pas devenir invincible")]
        public void Total_IsCapped()
        {
            var many = Enumerable.Range(0, 10).Select(i => new SynergyDef("s" + i, "n", Family.Matter, Family.Matter, SynergyStat.Atk, 10)).ToList();
            Assert.That(SynergyRules.Total(many).Atk, Is.EqualTo(SynergyRules.CapPercent));
            Assert.That(SynergyRules.Total(many).Def, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un allié combat avec les statistiques gonflées dès que son escouade forme une synergie — parce que la synergie est un boost permanent, sans action à lancer")]
        public void Ally_GetsThePermanentBoost()
        {
            var alone = Sim("ancre").Unit(0);
            var paired = Sim("ancre", "seve").Unit(0); // deux Physique/Bio : résonance vitale, bonus de PV
            Assert.That(paired.MaxHp, Is.GreaterThan(alone.MaxHp));
            Assert.That(paired.MaxHp, Is.EqualTo(alone.MaxHp * 104 / 100).Within(2));
        }

        [Test, Description("Vérifie qu'aucune compétence ni commande de combo ou d'avatar n'existe plus — parce que le joueur n'a plus de compétences propres : seules les cartes des agents se jouent")]
        public void NoComboNorAvatarCommands()
        {
            var names = System.Enum.GetNames(typeof(CommandType));
            Assert.That(names, Is.EquivalentTo(new[] { "Skill", "SetAuto", "Retreat", "Pass" }));
        }
    }
}
