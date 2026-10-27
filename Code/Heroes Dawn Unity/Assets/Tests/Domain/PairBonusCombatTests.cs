using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Bonus de paire en combat (lot D de la 0.14.0) : appliqués aux deux agents liés, seulement dans l'escouade, plafonnés.</summary>
    public sealed class PairBonusCombatTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        static LinkEntry Link(string a, string b, int level) => new LinkEntry(new MemberId(a), new MemberId(b), level);

        static CombatSimulation Sim(string[] allies, params LinkEntry[] links)
            => new CombatSimulation(C, new CombatSetup(11UL, CombatRank.Normal,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                new[] { new EnemyEntry(new EnemyId("thug"), 5) }, false).WithLinks(links));

        static CombatUnit Unit(CombatSimulation s, string member) => s.Units.First(u => u.Side == Side.Ally && u.DefId == member);

        static int ShieldValue(CombatUnit u) => u.Statuses.First(x => x.Type == StatusType.Shield).Value;

        [Test, Description("Vérifie que le critique d'une paire Dégâts + Précision (niveau 4 : +4 puis +4) va aux deux agents liés et pas au troisième — parce qu'un bonus de paire ne concerne que la paire")]
        public void Crit_GoesToBothAgentsOfThePairOnly()
        {
            var s = Sim(new[] { "givre", "echo", "verre" }, Link("givre", "echo", 4));
            Assert.That(Unit(s, "givre").CritBonusPercent, Is.EqualTo(8));
            Assert.That(Unit(s, "echo").CritBonusPercent, Is.EqualTo(8));
            Assert.That(Unit(s, "verre").CritBonusPercent, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que le bouclier d'ouverture d'une paire Tank + Soutien (niveau 4 : 8 % des PV) est donné aux deux agents liés et pas au troisième — parce que le bouclier de paire s'ajoute aux effets d'équipe")]
        public void OpeningShield_GoesToBothAgentsOfThePairOnly()
        {
            var s = Sim(new[] { "ancre", "seve", "verre" }, Link("ancre", "seve", 4));
            foreach (var id in new[] { "ancre", "seve" }) Assert.That(ShieldValue(Unit(s, id)), Is.EqualTo(Unit(s, id).MaxHp * 8 / 100), id);
            Assert.That(Unit(s, "verre").Statuses.Any(x => x.Type == StatusType.Shield), Is.False);
        }

        [Test, Description("Vérifie que l'énergie de départ d'une paire Tank + Dégâts (niveau 4 : +10) va aux deux agents liés seulement — parce que chaque brique doit atteindre sa cible")]
        public void StartEnergy_GoesToBothAgentsOfThePairOnly()
        {
            var s = Sim(new[] { "ancre", "givre", "verre" }, Link("ancre", "givre", 4));
            int baseline = Unit(s, "verre").Energy;
            Assert.That(Unit(s, "ancre").Energy, Is.EqualTo(baseline + 10));
            Assert.That(Unit(s, "givre").Energy, Is.EqualTo(baseline + 10));
        }

        [Test, Description("Vérifie que la jauge de départ d'une paire Tank + Contrôle (niveau 4 : +10 % de la frise) avance les deux agents liés seulement — parce que la jauge de départ est une brique de paire")]
        public void StartGauge_GoesToBothAgentsOfThePairOnly()
        {
            var s = Sim(new[] { "ancre", "verre", "givre" }, Link("ancre", "verre", 4));
            Assert.That(Unit(s, "ancre").StartPointBonus, Is.GreaterThan(0));
            Assert.That(Unit(s, "verre").StartPointBonus, Is.GreaterThan(0));
            Assert.That(Unit(s, "givre").StartPointBonus, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que l'énergie par pas d'une paire Soutien + Soutien (niveau 5 : +1) va aux deux agents liés seulement — parce que la régénération d'énergie est une brique de paire")]
        public void EnergyPerStep_GoesToBothAgentsOfThePairOnly()
        {
            var s = Sim(new[] { "seve", "tide", "verre" }, Link("seve", "tide", 5));
            Assert.That(Unit(s, "seve").EnergyRegenBonus, Is.EqualTo(1));
            Assert.That(Unit(s, "tide").EnergyRegenBonus, Is.EqualTo(1));
            Assert.That(Unit(s, "verre").EnergyRegenBonus, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que les bonus de statistiques d'une paire (PV, ATQ, DEF, VIT) augmentent les statistiques de l'agent dans les proportions annoncées — parce que la fabrique d'unités doit les appliquer comme les bonus d'objets")]
        public void StatBonuses_AreAppliedByTheUnitFactory()
        {
            var member = C.Member(new MemberId("ancre"));
            var entry = new AllyEntry(member.Id, 5, 0);
            var plain = UnitFactory.Ally(0, member, entry, C.Balance);
            var boosted = UnitFactory.Ally(0, member, entry, C.Balance, null, default, new PairBonusTotal(10, 10, 10, 10, 0, 0, 0, 0, 0));
            Assert.That(boosted.MaxHp, Is.EqualTo(plain.MaxHp * 110 / 100).Within(2));
            Assert.That(boosted.Atk, Is.EqualTo(plain.Atk * 110 / 100).Within(2));
            Assert.That(boosted.Def, Is.EqualTo(plain.Def * 110 / 100).Within(2));
            Assert.That(boosted.Spd, Is.EqualTo(plain.Spd * 110 / 100).Within(1));
        }

        [Test, Description("Vérifie qu'un lien de niveau 5 dont un seul des deux agents est dans l'escouade ne donne aucun bonus à l'agent présent — parce que les bonus de paire n'existent que si les deux sont dans la même équipe")]
        public void NoBonus_IfOnlyOneAgentIsInTheSquad()
        {
            var alone = Sim(new[] { "givre", "verre", "ancre" });
            var linked = Sim(new[] { "givre", "verre", "ancre" }, Link("givre", "echo", 5));
            Assert.That(Unit(linked, "givre").CritBonusPercent, Is.EqualTo(0));
            Assert.That(Unit(linked, "givre").MaxHp, Is.EqualTo(Unit(alone, "givre").MaxHp));
            Assert.That(Unit(linked, "givre").Atk, Is.EqualTo(Unit(alone, "givre").Atk));
        }

        [Test, Description("Vérifie qu'un lien déclaré dans un sens ou dans l'autre donne les mêmes bonus — parce qu'une paire A+B est la même que B+A")]
        public void LinkOrderInTheSetup_DoesNotMatter()
        {
            var ab = Sim(new[] { "givre", "echo", "verre" }, Link("givre", "echo", 5));
            var ba = Sim(new[] { "givre", "echo", "verre" }, Link("echo", "givre", 5));
            Assert.That(Unit(ab, "givre").CritBonusPercent, Is.EqualTo(8));
            Assert.That(Unit(ba, "givre").CritBonusPercent, Is.EqualTo(8));
            Assert.That(Unit(ab, "echo").MaxHp, Is.EqualTo(Unit(ba, "echo").MaxHp));
        }

        [Test, Description("Vérifie qu'un agent dans trois paires de niveau 5 reçoit un bouclier plafonné à 20 % de ses PV (et non 3 × 15 %) — parce que le cumul des paires est plafonné")]
        public void AnAgentInThreePairs_IsCapped()
        {
            string[] tanks = { "ancre", "rempart", "bastion", "vanguard" };
            var links = tanks.SelectMany((a, i) => tanks.Skip(i + 1).Select(b => Link(a, b, 5))).ToArray();
            var s = Sim(tanks, links);
            foreach (var id in tanks) Assert.That(ShieldValue(Unit(s, id)), Is.EqualTo(Unit(s, id).MaxHp * 20 / 100), id);
        }

        [Test, Description("Vérifie qu'avec des liens de niveau 1 (ou sans lien), les unités ont exactement les mêmes statistiques, énergie et statuts qu'avant les bonus de paire — parce que le niveau 1 ne change rien et que les simulations existantes restent identiques")]
        public void Level1_ChangesNothing()
        {
            var none = Sim(new[] { "ancre", "seve", "verre" });
            var level1 = Sim(new[] { "ancre", "seve", "verre" }, Link("ancre", "seve", 1), Link("ancre", "verre", 1), Link("seve", "verre", 1));
            foreach (var id in new[] { "ancre", "seve", "verre" })
            {
                var a = Unit(none, id); var b = Unit(level1, id);
                Assert.That((b.MaxHp, b.Atk, b.Def, b.Spd, b.Energy, b.CritBonusPercent, b.StartPointBonus, b.EnergyRegenBonus, b.Statuses.Count),
                            Is.EqualTo((a.MaxHp, a.Atk, a.Def, a.Spd, a.Energy, a.CritBonusPercent, a.StartPointBonus, a.EnergyRegenBonus, a.Statuses.Count)), id);
            }
        }

        [Test, Description("Vérifie qu'un combat automatique d'une escouade de quatre agents tous liés au niveau 5 se termine et se rejoue à l'identique depuis son journal — parce que la vérification serveur repose sur le rejeu")]
        public void AutoCombatWithLevel5Links_EndsAndReplays()
        {
            string[] squad = { "ancre", "givre", "echo", "seve" };
            var links = squad.SelectMany((a, i) => squad.Skip(i + 1).Select(b => Link(a, b, 5))).ToArray();
            var setup = new CombatSetup(21UL, CombatRank.Normal, squad.Select(a => new AllyEntry(new MemberId(a), 8, 0)).ToArray(),
                new[] { new EnemyEntry(new EnemyId("brute"), 8), new EnemyEntry(new EnemyId("thug"), 8), new EnemyEntry(new EnemyId("brute"), 8) }, true).WithLinks(links);
            var sim = new CombatSimulation(C, setup);
            int guard = 0;
            while (!sim.IsOver && guard++ < 40000) sim.Step(1);
            Assert.That(sim.IsOver, Is.True);
            Assert.That(CombatReplay.Run(C, setup, sim.InputLog).StateHash, Is.EqualTo(sim.StateHash()));
        }

        [Test, Description("Vérifie qu'un profil avec un lien de 25 victoires et un profil sans lien se chargent sans erreur, le premier au niveau de lien 5 et le second au niveau 1 — parce que les sauvegardes existantes doivent rester valides")]
        public void SavedLinks_StillLoadAndKeepTheirLevel()
        {
            ProfileData Profile()
            {
                var p = new ProfileData();
                foreach (var id in new[] { "ancre", "seve" }) p.team.Add(new MemberSave { id = id, level = 5, inSquad = true });
                return p;
            }
            var withLink = Profile();
            withLink.meta.links.Add(new LinkSave { a = "ancre", b = "seve", wins = 25 });
            var loaded = new GameSession(C);
            loaded.ApplyProfile(UnityEngine.JsonUtility.FromJson<ProfileData>(UnityEngine.JsonUtility.ToJson(withLink)));
            Assert.That(TeamLinks.Level(loaded.Meta, "seve", "ancre"), Is.EqualTo(5));
            var withoutLink = new GameSession(C);
            withoutLink.ApplyProfile(Profile());
            Assert.That(TeamLinks.Level(withoutLink.Meta, "ancre", "seve"), Is.EqualTo(1));
        }
    }
}
