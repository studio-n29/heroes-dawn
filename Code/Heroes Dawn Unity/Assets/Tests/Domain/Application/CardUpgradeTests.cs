using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
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

        // ------------------------------------------------------------------ Paliers (lot C1)

        /// <summary>Carte de test Matière : dégâts 100 sur un ennemi ; paliers 3 (Brûlure 2 t), 6 (Bouclier 10 % 2 t), 10 (Vulnérable 2 t).</summary>
        static SkillCardDef TestCard() => new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, ItemRarity.Common,
            new SkillDef(new SkillId("card_test"), SkillSlot.A3, 0, TargetRule.SingleEnemy, new[] { new EffectDef(EffectType.Damage, 100) }, 0),
            tiers: new[]
            {
                new CardTier(3, new EffectDef(EffectType.ApplyStatus, 0, StatusType.Burn, 2)),
                new CardTier(6, new EffectDef(EffectType.Shield, 10, StatusType.Shield, 2)),
                new CardTier(10, new EffectDef(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2)),
            });

        [Test, Description("Vérifie que chaque palier ajoute ses effets à partir de son niveau (3, 6, 10), après les effets de base — parce que les effets secondaires se débloquent à des paliers précis")]
        public void SkillAt_AddsTierEffectsAtTheirLevel()
        {
            var card = TestCard();
            int Count(int level) => CardLevels.SkillAt(card, level).Effects.Count;
            Assert.That(new[] { Count(2), Count(3), Count(5), Count(6), Count(9), Count(10) }, Is.EqualTo(new[] { 1, 2, 2, 3, 3, 4 }));
            Assert.That(CardLevels.SkillAt(card, 10).Effects.Select(e => e.Type),
                        Is.EqualTo(new[] { EffectType.Damage, EffectType.ApplyStatus, EffectType.Shield, EffectType.ApplyStatus }));
        }

        [Test, Description("Vérifie que le niveau augmente les effets de base (dégâts 190 au niveau 10) mais pas les effets de palier (bouclier 10 %), et garde l'identité de la compétence — parce que le texte d'un palier doit rester exact")]
        public void SkillAt_ScalesOnlyTheBaseEffects()
        {
            var card = TestCard();
            var s = CardLevels.SkillAt(card, 10);
            Assert.That(s.Effects[0].Value, Is.EqualTo(190));
            Assert.That(s.Effects[2].Value, Is.EqualTo(10));
            Assert.That(s.Id, Is.EqualTo(card.Skill.Id));
            Assert.That(s.Slot, Is.EqualTo(card.Skill.Slot));
            Assert.That(s.Target, Is.EqualTo(card.Skill.Target));
            Assert.That(s.Cost, Is.EqualTo(card.Skill.Cost));
        }

        static CombatUnit Verre(int cardLevel, params string[] items)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { TestCard() }).ToList(), col.Boosters));
            var ally = new AllyEntry(new MemberId("verre"), 5, 0, 0, items.Select(i => new ItemId(i)).ToList(), new CardId?[] { new CardId("card_test") }, new[] { cardLevel });
            var sim = new CombatSimulation(catalog, new CombatSetup(7, CombatRank.Normal, new[] { ally }, new[] { new EnemyEntry(new EnemyId("brute"), 5) }, false));
            return sim.Unit(0);
        }

        [Test, Description("Vérifie qu'en combat la carte équipée au niveau 6 a ses deux premiers paliers — parce que le combat doit jouer la carte au niveau atteint")]
        public void CardInCombat_UsesItsLevelAndTiers()
        {
            Assert.That(Verre(6).Skills[2].Effects.Count, Is.EqualTo(3));
            Assert.That(Verre(2).Skills[2].Effects.Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'avec un objet de puissance (+15 %), la carte niveau 6 a ses dégâts augmentés par le niveau puis par l'objet, et son bouclier de palier par l'objet seulement — parce qu'aucune augmentation ne doit être comptée deux fois")]
        public void CardWithItemSkillPower_ScalesOnce()
        {
            var fx = Verre(6, "power_gauntlet").Skills[2].Effects;
            Assert.That(fx[0].Value, Is.EqualTo(150 * 115 / 100));
            Assert.That(fx[2].Value, Is.EqualTo(10 * 115 / 100));
        }
    }
}
