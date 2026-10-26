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

        [Test, Description("Vérifie qu'une carte monte jusqu'au niveau 10 (9 améliorations) puis que l'amélioration est refusée — parce que la carte ne monte pas au-delà de 10")]
        public void UpgradeCard_GoesUpTo10_ThenRefuses()
        {
            CollectionOps.AddCard(_app.Session, Wall, 30);
            _app.Session.Wallet.Set(10000, 0);
            for (int i = 1; i < 10; i++) Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess, "niveau " + i);
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(10));
            Assert.That(Code(_app.Collection.UpgradeCard(Wall)), Is.EqualTo("collection.max_level"));
        }

        [Test, Description("Vérifie le prix des 9 améliorations (doublons 1,1,2,2,3,3,4,4,5 soit 25 ; Crédits 200 × niveau soit 9 000) et la puissance (100 %, 140 % au niveau 5, 190 % au niveau 10) — parce que l'économie des cartes est calibrée sur ces valeurs")]
        public void CardLevels_Economy_Over10Levels()
        {
            var copies = new int[9]; int credits = 0;
            for (int l = 1; l <= 9; l++) { copies[l - 1] = CardLevels.CopiesToUpgrade(l); credits += CardLevels.CreditsToUpgrade(l); }
            Assert.That(copies, Is.EqualTo(new[] { 1, 1, 2, 2, 3, 3, 4, 4, 5 }));
            Assert.That(credits, Is.EqualTo(9000));
            Assert.That(CardLevels.EffectPercent(1), Is.EqualTo(100));
            Assert.That(CardLevels.EffectPercent(5), Is.EqualTo(140));
            Assert.That(CardLevels.EffectPercent(10), Is.EqualTo(190));
            Assert.That(CardLevels.TierLevels, Is.EqualTo(new[] { 3, 6, 10 }));
        }

        [Test, Description("Vérifie qu'un niveau sauvegardé au-delà de 10 (fichier modifié) est lu comme 10 — parce qu'une sauvegarde abîmée ne doit pas casser la carte")]
        public void SavedLevelAbove10_IsClampedTo10()
        {
            CollectionOps.AddCard(_app.Session, Wall);
            CollectionOps.SetCardLevel(_app.Session, Wall, 12);
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(10));
        }

        [Test, Description("Vérifie que chaque niveau augmente de 10 % les dégâts d'une carte et laisse les autres effets intacts — parce que c'est l'effet promis de l'amélioration")]
        public void Scale_RaisesDamageOnly()
        {
            var skill = new SkillDef(new SkillId("s"), SkillSlot.A1, 0, TargetRule.SingleEnemy,
                new[] { new EffectDef(EffectType.Damage, 100), new EffectDef(EffectType.ApplyStatus, 0, StatusType.Mark, 2, 50) });
            var l3 = CardLevels.Scale(skill, 3);
            Assert.That(l3.Effects[0].Value, Is.EqualTo(120));
            Assert.That(l3.Effects[1].ChancePercent, Is.EqualTo(50));
            Assert.That(CardLevels.Scale(skill, 1), Is.SameAs(skill));
        }
    }
}
