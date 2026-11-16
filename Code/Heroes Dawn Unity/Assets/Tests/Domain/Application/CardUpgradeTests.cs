using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Session;
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

        [Test, Description("Vérifie le prix des 9 améliorations (doublons 1,1,1,2,2,2,3,3,3 soit 18 ; Crédits 200 × niveau soit 9 000) et la puissance (100 %, 140 % au niveau 5, 190 % au niveau 10) — parce que l'économie des cartes est calibrée sur ces valeurs")]
        public void CardLevels_Economy_Over10Levels()
        {
            var copies = new int[9]; int credits = 0;
            for (int l = 1; l <= 9; l++) { copies[l - 1] = CardLevels.CopiesToUpgrade(l); credits += CardLevels.CreditsToUpgrade(l); }
            Assert.That(copies, Is.EqualTo(new[] { 1, 1, 1, 2, 2, 2, 3, 3, 3 }));
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

        // ------------------------------------------------------------------ Économie des doublons (autres cartes de la même étoile)

        CollectionDef Defs => _app.Session.Content.Collection;

        /// <summary>Les autres cartes de la même étoile que <see cref="Wall"/>, par identifiant croissant.</summary>
        List<CardId> SameStars() => Defs.Cards.Where(c => c.Stars == Defs.Card(Wall).Stars && c.Id != Wall)
            .Select(c => c.Id).OrderBy(i => i.Value, System.StringComparer.Ordinal).ToList();

        CardId OtherStars() => Defs.Cards.First(c => c.Stars != Defs.Card(Wall).Stars).Id;

        int Owned(CardId c) => CollectionOps.OwnedCards(_app.Session, c);

        [Test, Description("Vérifie que les exemplaires propres de la carte sont consommés avant toute autre carte — parce que les doublons d'autres cartes ne servent qu'en complément")]
        public void Upgrade_UsesOwnCopiesFirst()
        {
            var a = SameStars()[0];
            CollectionOps.AddCard(_app.Session, Wall, 2);
            CollectionOps.AddCard(_app.Session, a, 5);
            Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess);
            Assert.That(Owned(Wall), Is.EqualTo(1));
            Assert.That(Owned(a), Is.EqualTo(5));
        }

        [Test, Description("Vérifie que sans doublon propre trois doublons d'une autre carte de la même étoile valent un exemplaire manquant — parce que c'est le taux de conversion de l'économie des doublons")]
        public void Upgrade_CompletesWithOtherCardsOfTheSameStars_ThreeForOne()
        {
            var a = SameStars()[0];
            CollectionOps.AddCard(_app.Session, Wall);
            CollectionOps.AddCard(_app.Session, a, 4);
            Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess);
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(2));
            Assert.That(Owned(Wall), Is.EqualTo(1));
            Assert.That(Owned(a), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que les doublons d'une carte d'une autre étoile ne comptent jamais — parce qu'une carte ne se complète qu'avec des cartes de sa propre étoile")]
        public void Upgrade_RefusesOtherStars()
        {
            var other = OtherStars();
            CollectionOps.AddCard(_app.Session, Wall);
            CollectionOps.AddCard(_app.Session, other, 10);
            Assert.That(Code(_app.Collection.UpgradeCard(Wall)), Is.EqualTo("collection.copies"));
            Assert.That(Owned(other), Is.EqualTo(10));
        }

        [Test, Description("Vérifie qu'un manque (3 doublons d'autres cartes valent moins que l'exemplaire manquant) refuse sans rien consommer ni dépenser — parce qu'un échec ne doit rien coûter")]
        public void Upgrade_RefusesWhenNotEnough_AndChangesNothing()
        {
            var a = SameStars()[0];
            CollectionOps.AddCard(_app.Session, Wall);
            CollectionOps.AddCard(_app.Session, a, 3);
            int credits = _app.Session.Wallet.Credits;
            Assert.That(Code(_app.Collection.UpgradeCard(Wall)), Is.EqualTo("collection.copies"));
            Assert.That(Owned(Wall), Is.EqualTo(1));
            Assert.That(Owned(a), Is.EqualTo(3));
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(1));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits));
        }

        [Test, Description("Vérifie, au niveau 4 (2 exemplaires), que l'amélioration prend le doublon propre puis les doublons des autres cartes par identifiant croissant sans descendre une carte sous un exemplaire — parce que c'est l'ordre promis et que chaque carte garde celle qui la possède")]
        public void Upgrade_ConsumesExactlyTheCopiesNeeded()
        {
            var others = SameStars();
            var a = others[0]; var b = others[1];
            CollectionOps.SetCardLevel(_app.Session, Wall, 4);
            CollectionOps.AddCard(_app.Session, Wall, 2);
            CollectionOps.AddCard(_app.Session, a, 3);
            CollectionOps.AddCard(_app.Session, b, 5);
            Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess);
            Assert.That(CollectionOps.CardLevel(_app.Session, Wall), Is.EqualTo(5));
            Assert.That(Owned(Wall), Is.EqualTo(1), "un doublon propre");
            Assert.That(Owned(a), Is.EqualTo(1), "2 doublons, la carte garde son exemplaire");
            Assert.That(Owned(b), Is.EqualTo(4), "le 3e doublon seulement");
        }

        [Test, Description("Vérifie que le plan d'amélioration annonce les exemplaires propres et les doublons d'autres cartes sans rien consommer, et vaut null quand les doublons ne suffisent pas — parce que le grimoire affiche ce coût avant l'achat")]
        public void PlanUpgrade_AnnouncesTheCost_WithoutConsuming()
        {
            var a = SameStars()[0];
            CollectionOps.AddCard(_app.Session, Wall);
            Assert.That(CollectionOps.PlanUpgrade(_app.Session, Wall), Is.Null);
            CollectionOps.AddCard(_app.Session, a, 4);
            var plan = CollectionOps.PlanUpgrade(_app.Session, Wall)!;
            Assert.That(new[] { plan.Needed, plan.Own, plan.Missing }, Is.EqualTo(new[] { 1, 0, 1 }));
            Assert.That(plan.Others.Select(o => (o.Key, o.Value)), Is.EqualTo(new[] { (a, 3) }));
            Assert.That(Owned(a), Is.EqualTo(4));
        }

        [Test, Description("Vérifie que les Crédits dépensés restent 200 × niveau même quand l'amélioration puise dans d'autres cartes — parce que seule la monnaie des doublons change")]
        public void Upgrade_KeepsCreditsCost()
        {
            var a = SameStars()[0];
            CollectionOps.SetCardLevel(_app.Session, Wall, 3);
            CollectionOps.AddCard(_app.Session, Wall);
            CollectionOps.AddCard(_app.Session, a, 4);
            int credits = _app.Session.Wallet.Credits;
            Assert.That(_app.Collection.UpgradeCard(Wall).IsSuccess);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits - 600));
        }

        [Test, Description("Vérifie qu'une sauvegarde enregistrée avant l'économie des doublons se recharge avec ses niveaux et ses exemplaires — parce qu'aucune migration n'est prévue")]
        public void ExistingSave_Reloads_AndKeepsItsLevels()
        {
            CollectionOps.AddCard(_app.Session, Wall, 3);
            CollectionOps.SetCardLevel(_app.Session, Wall, 7);
            var profile = _app.Session.ToProfile(System.DateTime.UtcNow);
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(UnityEngine.JsonUtility.FromJson<N29.HeroesDawn.Application.Ports.ProfileData>(UnityEngine.JsonUtility.ToJson(profile)));
            Assert.That(CollectionOps.CardLevel(restored, Wall), Is.EqualTo(7));
            Assert.That(CollectionOps.OwnedCards(restored, Wall), Is.EqualTo(3));
        }

        // ------------------------------------------------------------------ Paliers (lot C1)

        /// <summary>Carte de test Matière : dégâts 100 sur un ennemi ; paliers 3 (Brûlure 2 t), 6 (Bouclier 10 % 2 t), 10 (Vulnérable 2 t).</summary>
        static SkillCardDef TestCard() => new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, 1,
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

        [Test, Description("Vérifie qu'avec un objet de puissance (+15 %), la carte niveau 6 a ses dégâts augmentés par le niveau, le bonus de famille (+5 %) puis l objet, et son bouclier de palier par l'objet seulement — parce qu'aucune augmentation ne doit être comptée deux fois")]
        public void CardWithItemSkillPower_ScalesOnce()
        {
            var fx = Verre(6, "power_gauntlet").Skills[2].Effects;
            Assert.That(fx[0].Value, Is.EqualTo(150 * 105 / 100 * 115 / 100), "niveau, puis bonus de famille (+5 %), puis objet");
            Assert.That(fx[2].Value, Is.EqualTo(10 * 115 / 100));
        }
    }
}
