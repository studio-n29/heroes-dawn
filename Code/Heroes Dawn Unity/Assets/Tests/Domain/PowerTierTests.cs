#nullable enable
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    public sealed class PowerTierTests
    {
        [Test, Description("Vérifie que chaque agent du roster a une puissance entre 1 et 5, et que l'agent le plus puissant (Eero Nieminen) est à 5 — parce que la rareté affichée est désormais une échelle de puissance, pas une catégorie d'univers")]
        public void EveryAgent_HasATierFromOneToFive()
        {
            var members = PrototypeAgents.Members((r, ra) => PrototypeContent.BaseStats(r, ra)).ToList();
            Assert.That(members, Is.Not.Empty);
            Assert.That(members.All(m => m.PowerTier >= 1 && m.PowerTier <= 5), Is.True);
            Assert.That(members.Single(m => m.Id.Value == "eero_nieminen").PowerTier, Is.EqualTo(5));
        }
    }
}
