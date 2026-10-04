using System;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class ShopUseCaseTests
    {
        TestApp _app = null!;
        TestApp.ShopUseCases Sh => _app.Shop;
        ShopState Shop => _app.Session.Shop;

        static readonly DateTime Noon = new DateTime(2026, 9, 30, 12, 0, 0);

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Time.LocalNow = Noon;
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";
        Result Open() => Sh.Open.ExecuteAsync().GetAwaiter().GetResult();
        Result Recruit(int slot) => Sh.Recruit.ExecuteAsync(slot).GetAwaiter().GetResult();
        Result Reroll() => Sh.Reroll.ExecuteAsync().GetAwaiter().GetResult();
        string[] Ids => Shop.Offers.Select(o => o.Id.Value).ToArray();

        [Test, Description("Vérifie qu'à la première ouverture la boutique tire 3 héros du jour (Arc, Rempart, Étincelle au départ) et l'annonce — parce que le QG propose 3 héros par jour (spec §1)")]
        public void Open_FirstTime_DrawsUpToThreeOffers_PublishesShopChanged()
        {
            Assert.That(Code(Open()), Is.EqualTo("ok"));
            Assert.That(Shop.Day, Is.EqualTo("2026-09-30"));
            Assert.That(Shop.Draw, Is.EqualTo(0));
            Assert.That(Ids, Is.EquivalentTo(new[] { "arc", "rempart", "etincelle" }));
            Assert.That(_app.Received<ShopChanged>().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'une nouvelle ouverture le même jour garde les mêmes offres sans rien annoncer — parce que la boutique ne change qu'à 9h00 (spec §3)")]
        public void Open_SameDay_KeepsOffers()
        {
            Open();
            var before = Ids;
            _app.Time.LocalNow = Noon.AddHours(8);
            Open();
            Assert.That(Ids, Is.EqualTo(before));
            Assert.That(_app.Received<ShopChanged>().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'après 9h00 le lendemain la boutique est retirée et le compteur de renouvellements remis à zéro — parce que le renouvellement gratuit est quotidien (spec §3)")]
        public void Open_After9_NewDraw_ResetsDraw()
        {
            Open();
            Reroll();
            Assert.That(Shop.Draw, Is.EqualTo(1));
            _app.Time.LocalNow = new DateTime(2026, 10, 1, 9, 0, 0);
            Open();
            Assert.That(Shop.Day, Is.EqualTo("2026-10-01"));
            Assert.That(Shop.Draw, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'une horloge reculée (jour antérieur au jour sauvegardé) donne simplement un nouveau tirage — parce qu'un jour différent ne doit jamais casser la boutique (Review Focus 3)")]
        public void Open_DayEarlierThanSaved_Redraws()
        {
            Open();
            _app.Time.LocalNow = Noon.AddDays(-2);
            Assert.That(Code(Open()), Is.EqualTo("ok"));
            Assert.That(Shop.Day, Is.EqualTo("2026-09-28"));
            Assert.That(Shop.Offers.Count, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que recruter débite le prix de la rareté en PR, ajoute le héros au niveau 5, marque l'offre Recruté et laisse les autres offres — parce qu'un achat ne relance pas le tirage (spec §3)")]
        public void Recruit_Succeeds_DebitsPr_AddsLevel5_MarksRecruited_KeepsOtherOffers()
        {
            Open();
            _app.Session.Wallet.Set(0, 2000);
            var before = Ids;
            var id = Shop.Offers[0].Id;
            var def = _app.Session.Content.Members.First(m => m.Id == id);
            Assert.That(Code(Recruit(0)), Is.EqualTo("ok"));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(2000 - MetaRules.RecruitCost(def.Rarity)));
            Assert.That(_app.Session.Find(id)!.Level, Is.EqualTo(MetaRules.RecruitLevel));
            Assert.That(Shop.Offers[0].Recruited, Is.True);
            Assert.That(Shop.Offers.Skip(1).All(o => !o.Recruited), Is.True);
            Assert.That(Ids, Is.EqualTo(before));
            Assert.That(_app.Received<RosterChanged>().Count, Is.GreaterThan(0));
            Assert.That(_app.Received<WalletChanged>().Count, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie qu'on ne peut pas recruter sans assez de PR — parce que le recrutement se paie en PR (GDD §Recrutement)")]
        public void Recruit_NotEnoughPr_Fails()
        {
            Open();
            _app.Session.Wallet.Set(0, 0);
            Assert.That(Code(Recruit(0)), Is.EqualTo("hub.pr"));
            Assert.That(Shop.Offers[0].Recruited, Is.False);
        }

        [Test, Description("Vérifie qu'une offre déjà recrutée ne peut pas l'être une seconde fois — parce que la boutique n'a pas de doublons")]
        public void Recruit_AlreadyRecruited_Fails()
        {
            Open();
            _app.Session.Wallet.Set(0, 5000);
            Recruit(0);
            Assert.That(Code(Recruit(0)), Is.EqualTo("shop.recruited"));
        }

        [Test, Description("Vérifie qu'un emplacement inexistant est refusé — parce qu'une vue ne doit pas pouvoir acheter hors de la boutique")]
        public void Recruit_UnknownSlot_Fails()
        {
            Open();
            Assert.That(Code(Recruit(7)), Is.EqualTo("shop.slot"));
        }

        [Test, Description("Vérifie qu'un achat sur une boutique expirée (9h00 passée) est refusé et que la nouvelle boutique est ouverte — parce qu'on n'achète pas une offre qui n'existe plus (spec §6)")]
        public void Recruit_AfterDayChange_ExpiredAndReopened()
        {
            Open();
            _app.Session.Wallet.Set(0, 5000);
            _app.Time.LocalNow = new DateTime(2026, 10, 1, 10, 0, 0);
            Assert.That(Code(Recruit(0)), Is.EqualTo("shop.expired"));
            Assert.That(Shop.Day, Is.EqualTo("2026-10-01"));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(5000));
        }

        [Test, Description("Vérifie qu'une recrue achetée pendant une partie en cours rejoint l'équipe hors escouade — parce que l'escouade engagée est figée jusqu'au retour au QG (Review Focus 5)")]
        public void Recruit_DuringRun_NotInSquad()
        {
            Assert.That(_app.Hub.ToggleSquad.ExecuteAsync(new MemberId("ancre")).GetAwaiter().GetResult().IsSuccess);
            _app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            Open();
            _app.Session.Wallet.Set(0, 5000);
            var id = Shop.Offers[0].Id;
            Assert.That(Code(Recruit(0)), Is.EqualTo("ok"));
            Assert.That(_app.Session.Find(id)!.InSquad, Is.False);
        }

        [Test, Description("Vérifie qu'une recrue achetée hors partie rejoint l'escouade s'il y a une place — parce que c'était la règle du recrutement et elle reste (spec §4)")]
        public void Recruit_SquadHasRoom_JoinsSquad()
        {
            Assert.That(_app.Hub.ToggleSquad.ExecuteAsync(new MemberId("ancre")).GetAwaiter().GetResult().IsSuccess);
            Open();
            _app.Session.Wallet.Set(0, 5000);
            var id = Shop.Offers[0].Id;
            Assert.That(Code(Recruit(0)), Is.EqualTo("ok"));
            Assert.That(_app.Session.Find(id)!.InSquad, Is.True);
        }

        [Test, Description("Vérifie que le premier renouvellement du jour est gratuit et que le suivant coûte 100 Crédits — parce que ce sont les règles validées (spec §3)")]
        public void Reroll_FirstFree_ThenCosts100()
        {
            Open();
            _app.Session.Wallet.Set(1000, 0);
            Assert.That(Code(Reroll()), Is.EqualTo("ok"));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000));
            Assert.That(Shop.Draw, Is.EqualTo(1));
            Assert.That(Code(Reroll()), Is.EqualTo("ok"));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(900));
            Assert.That(Shop.Draw, Is.EqualTo(2));
            Assert.That(_app.Received<ShopChanged>().Count, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'un renouvellement payant sans assez de Crédits est refusé sans rien changer — parce qu'on ne retire pas sans payer")]
        public void Reroll_NotEnoughCredits_Fails_NoChange()
        {
            Open();
            Reroll();
            _app.Session.Wallet.Set(50, 0);
            var before = Ids;
            Assert.That(Code(Reroll()), Is.EqualTo("hub.credits"));
            Assert.That(Shop.Draw, Is.EqualTo(1));
            Assert.That(Ids, Is.EqualTo(before));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(50));
        }

        [Test, Description("Vérifie qu'un renouvellement demandé après 9h00 (confirmation restée ouverte) est refusé sans débit, et que la boutique du nouveau jour est affichée — parce que le joueur ne doit pas payer pour une boutique expirée (Review Focus 1)")]
        public void Reroll_AfterDayChange_RefusedNoCharge()
        {
            Open();
            Reroll();
            _app.Session.Wallet.Set(1000, 0);
            _app.Time.LocalNow = new DateTime(2026, 10, 1, 9, 30, 0);
            Assert.That(Code(Reroll()), Is.EqualTo("shop.expired"));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000));
            Assert.That(Shop.Day, Is.EqualTo("2026-10-01"));
            Assert.That(Shop.Draw, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'après un recrutement, un renouvellement ne propose plus le héros acheté — parce qu'on ne tire jamais un héros déjà possédé (Review Focus 4)")]
        public void Reroll_AfterRecruit_ExcludesOwned()
        {
            Open();
            _app.Session.Wallet.Set(0, 5000);
            var bought = Shop.Offers[0].Id;
            Recruit(0);
            Reroll();
            Assert.That(Shop.Offers.Any(o => o.Id == bought), Is.False);
            Assert.That(Shop.Offers.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que la boutique sauvegardée puis rechargée est identique (jour, tirage, offres, achats) — parce que relancer le jeu ne doit pas relancer le tirage (spec §1)")]
        public void Save_ThenApply_RestoresSameShop()
        {
            Open();
            _app.Session.Wallet.Set(0, 5000);
            Recruit(1);
            var p = _app.Session.ToProfile(DateTime.UtcNow);
            Assert.That(p.schemaVersion, Is.EqualTo(3));

            var other = new TestApp();
            other.SignInDirect("p1");
            other.Time.LocalNow = Noon;
            other.Session.ApplyProfile(p);
            Assert.That(other.Session.Shop.Day, Is.EqualTo(Shop.Day));
            Assert.That(other.Session.Shop.Draw, Is.EqualTo(Shop.Draw));
            Assert.That(other.Session.Shop.Offers.Select(o => (o.Id.Value, o.Recruited)), Is.EqualTo(Shop.Offers.Select(o => (o.Id.Value, o.Recruited))));
            other.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(other.Received<ShopChanged>(), Is.Empty, "même jour : pas de nouveau tirage");
        }

        [Test, Description("Vérifie qu'un profil v2 (sans boutique) se charge et que la boutique est tirée à la première ouverture — parce que les sauvegardes existantes doivent rester valides (spec §4)")]
        public void ApplyProfile_V2WithoutShop_OpensFreshShop()
        {
            var p = _app.Session.ToProfile(DateTime.UtcNow);
            p.schemaVersion = 2;
            p.shop = new ShopSave();
            var other = new TestApp();
            other.SignInDirect("p1");
            other.Time.LocalNow = Noon;
            other.Session.ApplyProfile(p);
            Assert.That(other.Session.Shop.Day, Is.EqualTo(""));
            other.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(other.Session.Shop.Offers.Count, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'une offre sauvegardée dont la recrue n'existe plus dans le contenu est ignorée au chargement — parce qu'une mise à jour du contenu ne doit pas casser une sauvegarde (Review Focus 2)")]
        public void ApplyProfile_UnknownOfferId_Dropped()
        {
            Open();
            var p = _app.Session.ToProfile(DateTime.UtcNow);
            p.shop.offers[0] = "inconnu";
            var other = new TestApp();
            other.SignInDirect("p1");
            other.Session.ApplyProfile(p);
            Assert.That(other.Session.Shop.Offers.Count, Is.EqualTo(2));
            Assert.That(other.Session.Shop.Offers.Any(o => o.Id.Value == "inconnu"), Is.False);
        }

        Result MarkSeen() => Sh.MarkSeen.ExecuteAsync().GetAwaiter().GetResult();

        [Test, Description("Vérifie que marquer la boutique comme vue retient le jour courant et sauvegarde — parce que le badge « N » de l'onglet Recruter disparaît une fois la boutique vue (spec §5)")]
        public void MarkSeen_SetsSeenDayAndSaves()
        {
            Open();
            int saves = _app.Profiles.Saves;
            Assert.That(Code(MarkSeen()), Is.EqualTo("ok"));
            Assert.That(Shop.SeenDay, Is.EqualTo(Shop.Day));
            Assert.That(_app.Profiles.Saves, Is.GreaterThan(saves));
        }

        [Test, Description("Vérifie que marquer deux fois n'annonce le changement qu'une fois — parce que la coque ne doit pas se rafraîchir pour rien")]
        public void MarkSeen_Twice_PublishesOnce()
        {
            Open();
            int before = _app.Received<ShopChanged>().Count;
            MarkSeen();
            MarkSeen();
            Assert.That(_app.Received<ShopChanged>().Count, Is.EqualTo(before + 1));
        }

        [Test, Description("Vérifie que le jour vu est sauvegardé puis restauré — parce que le badge ne doit pas réapparaître au relancement")]
        public void Save_ThenApply_RestoresSeenDay()
        {
            Open();
            MarkSeen();
            var p = _app.Session.ToProfile(DateTime.UtcNow);
            Assert.That(p.shop.seenDay, Is.EqualTo(Shop.Day));
            var other = new TestApp();
            other.SignInDirect("p1");
            other.Session.ApplyProfile(p);
            Assert.That(other.Session.Shop.SeenDay, Is.EqualTo(Shop.Day));
        }

        [Test, Description("Vérifie qu'un profil sans jour vu se charge comme « pas vu » — parce que les sauvegardes actuelles n'ont pas ce champ (spec §5)")]
        public void ApplyProfile_WithoutSeenDay_IsNotSeen()
        {
            Open();
            var p = _app.Session.ToProfile(DateTime.UtcNow);
            p.shop.seenDay = "";
            var other = new TestApp();
            other.SignInDirect("p1");
            other.Session.ApplyProfile(p);
            Assert.That(other.Session.Shop.SeenDay, Is.EqualTo(""));
            Assert.That(other.Session.Shop.SeenDay, Is.Not.EqualTo(other.Session.Shop.Day));
        }
    }
}
