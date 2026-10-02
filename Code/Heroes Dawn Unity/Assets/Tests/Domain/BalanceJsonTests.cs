using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    public sealed class BalanceJsonTests
    {
        [Test]
        public void RoundTrip_PreservesEveryIntProperty()
        {
            var c = new BalanceConfig { DefK = 321, FatiguePerWin = 9, RankEasyLevelOffset = -4 };
            var back = BalanceJson.Deserialize(BalanceJson.Serialize(c)).Value;
            foreach (var p in typeof(BalanceConfig).GetProperties())
                Assert.That(p.GetValue(back), Is.EqualTo(p.GetValue(c)), p.Name);
        }

        [Test]
        public void Deserialize_PartialJson_KeepsDefaultsForMissingKeys()
        {
            var b = BalanceJson.Deserialize("{ \"DefK\": 500 }").Value;
            Assert.That(b.DefK, Is.EqualTo(500));
            Assert.That(b.CritBasePercent, Is.EqualTo(15));
        }

        [Test] // Review Focus 4 : balance.json corrompu → échec explicite, le jeu garde les défauts
        public void Deserialize_Garbage_Fails()
        {
            var r = BalanceJson.Deserialize("pas du json");
            Assert.That(r.IsFailure);
            Assert.That(r.Error!.Code, Is.EqualTo("balance.invalid_json"));
        }

        [Test]
        public void Build_UsesGivenBalance()
        {
            Assert.That(PrototypeContent.Build(new BalanceConfig { DefK = 999 }).Balance.DefK, Is.EqualTo(999));
        }
    }
}
