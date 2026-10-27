using System;
using System.Linq;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Noms des thèmes de lien et libellés des bonus de paire (lot D de la 0.14.0).</summary>
    public sealed class PairBonusTextTests
    {
        readonly LocLocalization _loc = new LocLocalization();
        static readonly Role[] Roles = (Role[])Enum.GetValues(typeof(Role));

        [Test, Description("Vérifie que chacun des 15 couples de rôles a un nom de thème français distinct (« Double rempart », « Rempart et lame », « Étau »…), dans les deux ordres de rôles — parce que le joueur lit le thème de chaque paire dans la fenêtre d'affinités")]
        public void EveryRolePair_HasItsOwnThemeName()
        {
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < Roles.Length; i++)
                for (int j = i; j < Roles.Length; j++)
                {
                    string name = _loc.PairTheme(Roles[i], Roles[j]);
                    Assert.That(name, Is.Not.Empty, Roles[i] + "+" + Roles[j]);
                    Assert.That(name, Does.Not.StartWith("link.theme"), "clé non traduite : " + Roles[i] + "+" + Roles[j]);
                    Assert.That(_loc.PairTheme(Roles[j], Roles[i]), Is.EqualTo(name));
                    names.Add(name);
                }
            Assert.That(names.Distinct().Count(), Is.EqualTo(15));
            Assert.That(_loc.PairTheme(Role.Tank, Role.Tank), Is.EqualTo("Double rempart"));
            Assert.That(_loc.PairTheme(Role.Tank, Role.Damage), Is.EqualTo("Rempart et lame"));
            Assert.That(_loc.PairTheme(Role.Tank, Role.Control), Is.EqualTo("Étau"));
            Assert.That(_loc.PairTheme(Role.Precision, Role.Precision), Is.EqualTo("Double regard"));
        }

        [TestCase(PairBonusKind.HpPercent, 4, "PV +4 %")]
        [TestCase(PairBonusKind.AtkPercent, 3, "ATQ +3 %")]
        [TestCase(PairBonusKind.DefPercent, 4, "DEF +4 %")]
        [TestCase(PairBonusKind.SpdPercent, 3, "VIT +3 %")]
        [TestCase(PairBonusKind.CritPercent, 5, "Critique +5 %")]
        [TestCase(PairBonusKind.OpeningShieldPercent, 8, "Bouclier d'ouverture 8 %")]
        [TestCase(PairBonusKind.StartGaugePercent, 10, "Jauge de départ +10 %")]
        [TestCase(PairBonusKind.StartEnergy, 10, "Énergie de départ +10")]
        [TestCase(PairBonusKind.EnergyPerStep, 1, "Énergie +1 par pas")]
        [Description("Vérifie les neuf libellés de bonus de paire — parce que chaque bonus débloqué est affiché au joueur")]
        public void PairBonusText_HasAClearLabelForEachKind(PairBonusKind kind, int value, string expected)
        {
            Assert.That(_loc.PairBonusText(new PairBonus(kind, value)), Is.EqualTo(expected));
        }
    }
}
