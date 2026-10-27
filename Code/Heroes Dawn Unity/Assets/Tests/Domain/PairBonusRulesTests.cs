using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Bonus propres à chaque paire d'agents, par couple de rôles et niveau de lien (lot D de la 0.14.0).</summary>
    public sealed class PairBonusRulesTests
    {
        static readonly Role[] Roles = (Role[])Enum.GetValues(typeof(Role));

        static IEnumerable<(Role A, Role B)> Pairs()
        {
            for (int i = 0; i < Roles.Length; i++)
                for (int j = i; j < Roles.Length; j++) yield return (Roles[i], Roles[j]);
        }

        [Test, Description("Vérifie que les 15 couples de rôles n'ont aucun bonus au niveau 1 (ou en dessous), deux au niveau 3 et quatre au niveau 5 — parce que le lien débloque un bonus par niveau à partir du niveau 2")]
        public void Table_Has15Pairs_AndNothingAtLevel1()
        {
            Assert.That(Pairs().Count(), Is.EqualTo(15));
            foreach (var (a, b) in Pairs())
            {
                Assert.That(PairBonusRules.Of(a, b, 1), Is.Empty, a + "+" + b);
                Assert.That(PairBonusRules.Of(a, b, 0), Is.Empty, a + "+" + b);
                Assert.That(PairBonusRules.Of(a, b, 3).Count, Is.EqualTo(2), a + "+" + b);
                Assert.That(PairBonusRules.Of(a, b, 5).Count, Is.EqualTo(4), a + "+" + b);
            }
        }

        static void Expect(PairBonus bonus, PairBonusKind kind, int value, string where)
        {
            Assert.That((bonus.Kind, bonus.Value), Is.EqualTo((kind, value)), where);
        }

        [Test, Description("Vérifie quelques lignes de la table de la spec (Tank+Tank, Dégâts+Précision, Soutien+Soutien, Précision+Précision, Contrôle+Contrôle) — parce que le contenu doit suivre la table validée")]
        public void Table_MatchesTheSpec()
        {
            Expect(PairBonusRules.At(Role.Tank, Role.Tank, 2), PairBonusKind.DefPercent, 4, "TT 2");
            Expect(PairBonusRules.At(Role.Tank, Role.Tank, 4), PairBonusKind.OpeningShieldPercent, 8, "TT 4");
            Expect(PairBonusRules.At(Role.Tank, Role.Tank, 5), PairBonusKind.OpeningShieldPercent, 7, "TT 5");
            Expect(PairBonusRules.At(Role.Damage, Role.Precision, 3), PairBonusKind.CritPercent, 4, "DP 3");
            Expect(PairBonusRules.At(Role.Damage, Role.Precision, 5), PairBonusKind.AtkPercent, 4, "DP 5");
            Expect(PairBonusRules.At(Role.Support, Role.Support, 5), PairBonusKind.EnergyPerStep, 1, "SS 5");
            Expect(PairBonusRules.At(Role.Precision, Role.Precision, 5), PairBonusKind.StartGaugePercent, 10, "PP 5");
            Expect(PairBonusRules.At(Role.Control, Role.Control, 4), PairBonusKind.StartGaugePercent, 10, "CC 4");
            Expect(PairBonusRules.At(Role.Tank, Role.Support, 2), PairBonusKind.HpPercent, 4, "TS 2");
            Expect(PairBonusRules.At(Role.Tank, Role.Support, 3), PairBonusKind.DefPercent, 3, "TS 3");
        }

        [Test, Description("Vérifie que l'ordre des deux rôles n'a aucune importance (mêmes bonus, même thème) pour les 25 combinaisons ordonnées — parce qu'une paire A+B est la même que B+A")]
        public void OrderOfRoles_DoesNotMatter()
        {
            foreach (var a in Roles)
                foreach (var b in Roles)
                {
                    var ab = PairBonusRules.Of(a, b, 5).Select(x => (x.Kind, x.Value));
                    var ba = PairBonusRules.Of(b, a, 5).Select(x => (x.Kind, x.Value));
                    Assert.That(ab, Is.EqualTo(ba), a + "+" + b);
                    Assert.That(PairBonusRules.ThemeKey(a, b), Is.EqualTo(PairBonusRules.ThemeKey(b, a)), a + "+" + b);
                }
            Assert.That(Pairs().Select(p => PairBonusRules.ThemeKey(p.A, p.B)).Distinct().Count(), Is.EqualTo(15));
            Assert.That(PairBonusRules.ThemeKey(Role.Damage, Role.Tank), Is.EqualTo("link.theme.tank_damage"));
        }

        [Test, Description("Vérifie qu'un niveau de lien au-delà de 5 donne les bonus du niveau 5 — parce que le niveau maximum est 5")]
        public void LevelAbove5_IsClampedTo5()
        {
            Assert.That(PairBonusRules.Of(Role.Tank, Role.Damage, 9).Count, Is.EqualTo(4));
        }

        static PairBonus[] Many(PairBonusKind kind, int value, int count) => Enumerable.Repeat(new PairBonus(kind, value), count).ToArray();

        [Test, Description("Vérifie que le cumul des bonus d'un agent est plafonné par type (12 % par statistique, 15 de critique, 20 de bouclier, de jauge et d'énergie de départ, 1 d'énergie par pas) — parce qu'un agent dans plusieurs paires ne doit pas empiler sans limite")]
        public void Total_SumsThenCaps()
        {
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.HpPercent, 4, 12)).Hp, Is.EqualTo(12));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.AtkPercent, 4, 5)).Atk, Is.EqualTo(12));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.DefPercent, 4, 5)).Def, Is.EqualTo(12));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.SpdPercent, 4, 5)).Spd, Is.EqualTo(12));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.CritPercent, 4, 5)).Crit, Is.EqualTo(15));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.OpeningShieldPercent, 8, 4)).OpeningShield, Is.EqualTo(20));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.StartGaugePercent, 10, 3)).StartGauge, Is.EqualTo(20));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.StartEnergy, 10, 3)).StartEnergy, Is.EqualTo(20));
            Assert.That(PairBonusRules.Total(Many(PairBonusKind.EnergyPerStep, 1, 3)).EnergyPerStep, Is.EqualTo(1));
            Assert.That(PairBonusRules.Total(new PairBonus[0]).IsNone, Is.True);
            var small = PairBonusRules.Total(new[] { new PairBonus(PairBonusKind.HpPercent, 4), new PairBonus(PairBonusKind.HpPercent, 3) });
            Assert.That((small.Hp, small.IsNone), Is.EqualTo((7, false)), "sous le plafond : la somme exacte");
        }
    }
}
