using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    public sealed class BalanceJsonTests
    {
        [Test, Description("Vérifie que toutes les valeurs d'équilibrage survivent à l'écriture puis la relecture — parce que le même fichier sert au client et à Remote Config")]
        public void RoundTrip_PreservesEveryIntProperty()
        {
            var c = new BalanceConfig { DefK = 321, FatiguePerWin = 9, RankEasyLevelOffset = -4 };
            var back = BalanceJson.Deserialize(BalanceJson.Serialize(c)).Value;
            foreach (var p in typeof(BalanceConfig).GetProperties())
                Assert.That(p.GetValue(back), Is.EqualTo(p.GetValue(c)), p.Name);
        }

        [Test, Description("Vérifie qu'un fichier partiel garde les valeurs par défaut pour les clés absentes — parce qu'un import incomplet ne doit pas mettre des zéros dans les règles")]
        public void Deserialize_PartialJson_KeepsDefaultsForMissingKeys()
        {
            var b = BalanceJson.Deserialize("{ \"DefK\": 500 }").Value;
            Assert.That(b.DefK, Is.EqualTo(500));
            Assert.That(b.CritBasePercent, Is.EqualTo(15));
        }

        [Test, Description("Vérifie qu'un balance.json illisible est refusé avec une erreur explicite — parce que le jeu doit alors garder les valeurs par défaut au lieu de planter (Review Focus 4 du plan tout-Unity)")] // Review Focus 4 : balance.json corrompu → échec explicite, le jeu garde les défauts
        public void Deserialize_Garbage_Fails()
        {
            var r = BalanceJson.Deserialize("pas du json");
            Assert.That(r.IsFailure);
            Assert.That(r.Error!.Code, Is.EqualTo("balance.invalid_json"));
        }

        [Test, Description("Vérifie que le contenu est construit avec l'équilibrage fourni — parce que les valeurs importées du tableur doivent réellement s'appliquer au jeu")]
        public void Build_UsesGivenBalance()
        {
            Assert.That(PrototypeContent.Build(new BalanceConfig { DefK = 999 }).Balance.DefK, Is.EqualTo(999));
        }
    }
}
