using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Amélioration des cartes de compétence : doublons et Crédits.</summary>
    public sealed class CardUpgradeTests
    {
        TestApp _app = null!;
        static readonly CardId Wall = new CardId("card_glass_wall");

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Wallet.Set(5000, 0);
        }

        static string Code(N29.HeroesDawn.Core.Result r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie qu'on ne peut pas améliorer une carte qu'on n'a pas, ni sans exemplaire en doublon — parce que l'amélioration consomme des doublons et garde toujours un exemplaire")]
        public void Upgrade_NeedsADuplicate()
        {
            Assert.That(Code(_app.Collection.UpgradeCard(Wall)), Is.EqualTo("collection.not_owned"));
            CollectionOps.AddCard(_app.Session, Wall);
            Assert.That(Code(_app.Collection.UpgradeCard(Wall)), Is.EqualTo("collection.copies"));
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'une amélioration consomme un doublon et des Crédits et monte la carte d'un niveau — parce que c'est le prix annoncé dans le grimoire")]
        public void Upgrade_SpendsCopiesAndCredits()
        {
            CollectionOps.AddCard(_app.Session, Wall, 2);
            int credits = _app.Session.Wallet.Credits;
            Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess);
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(2));
            Assert.That(CollectionOps.OwnedCards(_app.Session, Wall), Is.EqualTo(1));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits - CardLevels.CreditsToUpgrade(1)));
        }

        [Test, Description("Vérifie qu'une amélioration refuse faute de Crédits sans rien consommer — parce qu'un échec ne doit pas coûter de doublon")]
        public void Upgrade_NeedsCredits_AndConsumesNothingOnFailure()
        {
            CollectionOps.AddCard(_app.Session, Wall, 2);
            _app.Session.Wallet.Set(10, 0);
            Assert.That(_app.Collection.UpgradeCard(Wall).IsFailure);
            Assert.That(CollectionOps.OwnedCards(_app.Session, Wall), Is.EqualTo(2));
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le niveau maximum est refusé — parce que la carte ne monte pas au-delà de 5")]
        public void Upgrade_StopsAtMaxLevel()
        {
            CollectionOps.AddCard(_app.Session, Wall, 20);
            for (int i = 1; i < CardLevels.Max; i++) Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess, "niveau " + i);
            Assert.That(Code(_app.Collection.UpgradeCard(Wall)), Is.EqualTo("collection.max_level"));
        }

        [Test, Description("Vérifie que chaque niveau augmente de 15 % les dégâts d'une carte et laisse les autres effets intacts — parce que c'est l'effet promis de l'amélioration")]
        public void Scale_RaisesDamageOnly()
        {
            var skill = new SkillDef(new SkillId("s"), SkillSlot.A1, 0, TargetRule.SingleEnemy,
                new[] { new EffectDef(EffectType.Damage, 100), new EffectDef(EffectType.ApplyStatus, 0, StatusType.Mark, 2, 50) });
            var l3 = CardLevels.Scale(skill, 3);
            Assert.That(l3.Effects[0].Value, Is.EqualTo(130));
            Assert.That(l3.Effects[1].ChancePercent, Is.EqualTo(50));
            Assert.That(CardLevels.Scale(skill, 1), Is.SameAs(skill));
        }
    }
}
