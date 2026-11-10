using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Équipement, boutique d'objets, boosters et sources de récompenses (plan 2026-10-10, sous-projets L et M).</summary>
    public sealed class CollectionBookTests
    {
        TestApp _app = null!;
        static MemberId Verre => new MemberId("verre");
        static MemberId Ancre => new MemberId("ancre");

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(Verre), 5));
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";

        GameSession S => _app.Session;

        [Test, Description("Vérifie qu'on n'équipe qu'un objet possédé, jamais plus de deux par agent, et qu'un objet n'est pas prêté deux fois — parce que l'inventaire fait foi")]
        public void EquipItem_NeedsStock_AndAtMostTwoPerAgent()
        {
            var vest = new ItemId("kevlar_vest"); var shoes = new ItemId("running_shoes"); var shield = new ItemId("riot_shield");
            Assert.That(Code(_app.Collection.EquipItem(Ancre, vest)), Is.EqualTo("collection.not_owned"));
            CollectionOps.AddItem(S, vest); CollectionOps.AddItem(S, shoes); CollectionOps.AddItem(S, shield);
            Assert.That(_app.Collection.EquipItem(Ancre, vest).IsSuccess);
            Assert.That(Code(_app.Collection.EquipItem(Verre, vest)), Is.EqualTo("collection.not_owned"), "un seul exemplaire");
            Assert.That(_app.Collection.EquipItem(Ancre, shoes).IsSuccess);
            Assert.That(Code(_app.Collection.EquipItem(Ancre, shield)), Is.EqualTo("collection.no_slot"));
            Assert.That(_app.Collection.UnequipItem(Ancre, vest).IsSuccess);
            Assert.That(_app.Collection.EquipItem(Verre, vest).IsSuccess, "libéré, il s'équipe ailleurs");
            Assert.That(Code(_app.Collection.UnequipItem(Ancre, vest)), Is.EqualTo("collection.nothing"));
        }

        CardId[] Own(params string[] ids)
        {
            var cards = ids.Select(i => new CardId(i)).ToArray();
            foreach (var c in cards) CollectionOps.AddCard(S, c);
            return cards;
        }

        [Test, Description("Vérifie qu'on équipe une carte d'une autre famille que l'agent — parce que la famille ne limite plus l'équipement, elle donne seulement un bonus")]
        public void On_equipe_une_carte_d_une_autre_famille()
        {
            var ember = Own("card_ember")[0];   // carte Élémentaire sur un agent Matière
            Assert.That(Code(_app.Collection.EquipCard(Verre, 0, ember)), Is.EqualTo("ok"));
            Assert.That(S.Find(Verre)!.Cards[0], Is.EqualTo(ember));
        }

        [Test, Description("Vérifie qu'on équipe trois cartes mais qu'un 4e emplacement n'existe pas — parce que la limite passe de 2 à 3 cartes équipables")]
        public void On_equipe_trois_cartes_puis_la_quatrieme_emplacement_n_existe_pas()
        {
            var c = Own("card_glass_wall", "card_shackle", "card_ember", "card_storm");
            for (int slot = 0; slot < 3; slot++) Assert.That(Code(_app.Collection.EquipCard(Verre, slot, c[slot])), Is.EqualTo("ok"), "emplacement " + slot);
            Assert.That(Code(_app.Collection.EquipCard(Verre, 3, c[3])), Is.EqualTo("collection.no_slot"));
        }

        [Test, Description("Vérifie que la même carte ne s'équipe pas deux fois sur un agent, même avec deux exemplaires — parce que le doublon sur un agent est interdit")]
        public void Le_meme_carte_deux_fois_sur_un_agent_est_refusee()
        {
            var wall = Own("card_glass_wall")[0];
            CollectionOps.AddCard(S, wall);   // deux exemplaires possédés
            Assert.That(Code(_app.Collection.EquipCard(Verre, 0, wall)), Is.EqualTo("ok"));
            Assert.That(Code(_app.Collection.EquipCard(Verre, 1, wall)), Is.EqualTo("collection.duplicate_card"));
            Assert.That(S.Find(Verre)!.Cards[1], Is.Null);
        }

        [Test, Description("Vérifie qu'on équipe le 3e emplacement sans avoir rempli les deux premiers — parce que l'équipement est libre")]
        public void On_peut_equiper_l_emplacement_3_sans_les_deux_premiers()
        {
            var wall = Own("card_glass_wall")[0];
            Assert.That(Code(_app.Collection.EquipCard(Verre, 2, wall)), Is.EqualTo("ok"));
            Assert.That(S.Find(Verre)!.Cards[2], Is.EqualTo(wall));
        }

        [Test, Description("Vérifie que retirer une carte fait remonter les suivantes et qu'une carte inconnue est refusée — parce que les cartes restent groupées dans les premiers emplacements")]
        public void Retirer_une_carte_fait_remonter_les_suivantes()
        {
            var c = Own("card_glass_wall", "card_shackle", "card_ember");
            for (int slot = 0; slot < 3; slot++) _app.Collection.EquipCard(Verre, slot, c[slot]);
            Assert.That(_app.Collection.UnequipCard(Verre, 0).IsSuccess);
            var verre = S.Find(Verre)!;
            Assert.That(verre.Cards.Select(x => x?.Value), Is.EqualTo(new[] { "card_shackle", "card_ember", null }));
            Assert.That(Code(_app.Collection.EquipCard(Verre, 0, new CardId("absent"))), Is.EqualTo("collection.unknown"));
        }

        static GameSession ReloadWithCards(params string[] cards)
        {
            var profile = new N29.HeroesDawn.Application.Ports.ProfileData();
            var m = new N29.HeroesDawn.Application.Ports.MemberSave { id = "verre", level = 5, inSquad = true };
            m.cards.AddRange(cards);
            profile.team.Add(m);
            var s = new GameSession(TestContent.Build());
            s.ApplyProfile(profile);
            return s;
        }

        [Test, Description("Vérifie qu'une sauvegarde 0.15.0 à deux cartes donne deux cartes équipées et un 3e emplacement libre — parce que les cartes déjà équipées sont conservées à la migration")]
        public void Une_sauvegarde_a_deux_cartes_donne_deux_cartes_et_un_emplacement_libre()
        {
            var verre = ReloadWithCards("card_glass_wall", "card_shackle").Find(Verre)!;
            Assert.That(verre.Cards.Select(x => x?.Value), Is.EqualTo(new[] { "card_glass_wall", "card_shackle", null }));
        }

        [Test, Description("Vérifie que la seconde occurrence d'une carte déjà équipée dans la sauvegarde est ignorée — parce qu'un agent ne porte jamais deux fois la même carte")]
        public void Un_doublon_dans_la_sauvegarde_est_ignore()
        {
            var verre = ReloadWithCards("card_glass_wall", "card_glass_wall", "card_shackle").Find(Verre)!;
            Assert.That(verre.Cards.Select(x => x?.Value), Is.EqualTo(new[] { "card_glass_wall", "card_shackle", null }));
        }

        [Test, Description("Vérifie qu'une carte seule en 2e place est remontée en 1re au chargement — parce que les cartes équipées sont toujours groupées au début")]
        public void Une_carte_seule_en_2e_place_est_remontee_en_1re()
        {
            var verre = ReloadWithCards("", "card_shackle").Find(Verre)!;
            Assert.That(verre.Cards.Select(x => x?.Value), Is.EqualTo(new[] { "card_shackle", null, null }));
        }

        [Test, Description("Vérifie que la boutique d'objets débite les Crédits, marque l'offre vendue et se renouvelle le jour suivant — parce que la boutique est à rotation")]
        public void ItemShop_BuysWithCredits_AndRotates()
        {
            var offers = _app.Collection.ItemOffers();
            Assert.That(offers.Count, Is.EqualTo(CollectionBook.ItemOfferCount));
            S.Wallet.Set(100000, 0);
            int before = S.Wallet.Credits;
            Assert.That(_app.Collection.BuyItem(0).IsSuccess);
            Assert.That(S.Wallet.Credits, Is.EqualTo(before - offers[0].Item.Price));
            Assert.That(CollectionOps.OwnedItems(S, offers[0].Item.Id), Is.EqualTo(1));
            Assert.That(_app.Collection.ItemOffers()[0].Bought, Is.True);
            Assert.That(Code(_app.Collection.BuyItem(0)), Is.EqualTo("collection.sold_out"));
            S.Wallet.Set(0, 0);
            Assert.That(Code(_app.Collection.BuyItem(1)), Is.EqualTo("meta.credits"));
            Assert.That(Code(_app.Collection.BuyItem(9)), Is.EqualTo("collection.unknown"));
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(1);
            Assert.That(_app.Collection.ItemOffers().All(o => !o.Bought), Is.True, "nouveau jour, nouvelles offres");
        }

        [Test, Description("Vérifie qu'un booster s'achète en PR, qu'il s'ouvre une fois, ajoute ses cartes à la collection et fait avancer la pitié — parce que c'est la boucle de collection")]
        public void Booster_BuyThenOpen()
        {
            Assert.That(Code(_app.Collection.OpenBooster(BoosterDef.StandardId)), Is.EqualTo("collection.no_booster"));
            S.Wallet.Set(0, 0);
            Assert.That(Code(_app.Collection.BuyBooster(BoosterDef.StandardId)), Is.EqualTo("collection.pr"));
            S.Wallet.Set(0, 1000);
            Assert.That(_app.Collection.BuyBooster(BoosterDef.StandardId).IsSuccess);
            Assert.That(S.Wallet.Pr, Is.EqualTo(1000 - S.Content.Collection.Booster(BoosterDef.StandardId).PricePr));
            Assert.That(CollectionOps.Boosters(S, BoosterDef.StandardId), Is.EqualTo(1));
            var opened = _app.Collection.OpenBooster(BoosterDef.StandardId);
            Assert.That(opened.IsSuccess);
            Assert.That(opened.Value.Count, Is.EqualTo(3));
            Assert.That(CollectionOps.Boosters(S, BoosterDef.StandardId), Is.EqualTo(0));
            Assert.That(opened.Value.Sum(c => CollectionOps.OwnedCards(S, c)), Is.EqualTo(3), "chaque carte tirée est possédée");
            Assert.That(S.Meta.collection.opened, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que l'équipement, l'inventaire, les cartes, les boosters et la pitié survivent à une sauvegarde puis un chargement — parce que la progression ne doit pas se perdre")]
        public void Collection_SurvivesSaveAndLoad()
        {
            var vest = new ItemId("kevlar_vest"); var wall = new CardId("card_glass_wall");
            CollectionOps.AddItem(S, vest); CollectionOps.AddCard(S, wall); CollectionOps.AddBooster(S, BoosterDef.StandardId, 2);
            S.Meta.collection.pity = 3;
            _app.Collection.EquipItem(Verre, vest);
            _app.Collection.EquipCard(Verre, 0, wall);
            var json = UnityEngine.JsonUtility.ToJson(S.ToProfile(System.DateTime.UtcNow));
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(UnityEngine.JsonUtility.FromJson<N29.HeroesDawn.Application.Ports.ProfileData>(json));
            var verre = restored.Find(Verre)!;
            Assert.That(verre.Items, Is.EqualTo(new[] { vest }));
            Assert.That(verre.Cards[0], Is.EqualTo(wall));
            Assert.That(CollectionOps.OwnedItems(restored, vest), Is.EqualTo(1));
            Assert.That(CollectionOps.Boosters(restored, BoosterDef.StandardId), Is.EqualTo(2));
            Assert.That(restored.Meta.collection.pity, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'un agent ne peut pas équiper un 2e objet du même type (deux armures), mais un objet d'un autre type, et que l'objet refusé reste en stock — parce qu'un agent ne porte pas deux objets du même type")]
        public void EquipItem_RefusesASecondItemOfTheSameType()
        {
            var vest = new ItemId("kevlar_vest"); var shield = new ItemId("riot_shield"); var shoes = new ItemId("running_shoes");
            CollectionOps.AddItem(S, vest); CollectionOps.AddItem(S, shield); CollectionOps.AddItem(S, shoes);
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(Ancre), 5));
            Assert.That(_app.Collection.EquipItem(Ancre, vest).IsSuccess);
            Assert.That(Code(_app.Collection.EquipItem(Ancre, shield)), Is.EqualTo("collection.same_type"));
            Assert.That(_app.Collection.EquipItem(Ancre, shoes).IsSuccess);
            Assert.That(CollectionOps.AvailableItems(S, shield), Is.EqualTo(1));
        }

        /// <summary>Profil où Verre porte ces objets (le stock contient un exemplaire de chaque objet connu), rechargé dans une session neuve.</summary>
        GameSession Reload(params string[] worn)
        {
            foreach (var id in worn) if (S.Content.Collection.HasItem(new ItemId(id))) CollectionOps.AddItem(S, new ItemId(id));
            var profile = S.ToProfile(System.DateTime.UtcNow);
            var save = profile.team.Single(m => m.id == Verre.Value);
            save.items.Clear(); save.items.AddRange(worn);
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(UnityEngine.JsonUtility.FromJson<N29.HeroesDawn.Application.Ports.ProfileData>(UnityEngine.JsonUtility.ToJson(profile)));
            return restored;
        }

        [Test, Description("Vérifie qu'une sauvegarde où un agent porte deux armures (et un objet disparu du jeu) se charge avec la première armure seulement, la seconde revenant au stock, et que recharger ne change plus rien — parce que les sauvegardes d'avant la règle doivent être remises en règle sans rien perdre")]
        public void LoadedSave_WithTwoItemsOfTheSameType_ReturnsTheSecondToTheStock()
        {
            var restored = Reload("unknown_item", "kevlar_vest", "riot_shield");
            Assert.That(restored.Find(Verre)!.Items, Is.EqualTo(new[] { new ItemId("kevlar_vest") }));
            Assert.That(CollectionOps.AvailableItems(restored, new ItemId("riot_shield")), Is.EqualTo(1));
            var again = new GameSession(TestContent.Build());
            again.ApplyProfile(UnityEngine.JsonUtility.FromJson<N29.HeroesDawn.Application.Ports.ProfileData>(UnityEngine.JsonUtility.ToJson(restored.ToProfile(System.DateTime.UtcNow))));
            Assert.That(again.Find(Verre)!.Items, Is.EqualTo(new[] { new ItemId("kevlar_vest") }));
        }

        [Test, Description("Vérifie qu'une sauvegarde valide (une arme et une armure) se recharge telle quelle — parce que la remise en règle ne doit toucher que les doublons de type")]
        public void LoadedSave_Valid_IsUntouched()
        {
            var restored = Reload("steel_knuckles", "kevlar_vest");
            Assert.That(restored.Find(Verre)!.Items, Is.EqualTo(new[] { new ItemId("steel_knuckles"), new ItemId("kevlar_vest") }));
        }

        [Test, Description("Vérifie qu'un profil sans équipement (ancienne sauvegarde) se charge avec des agents sans objet ni carte — parce que les anciennes sauvegardes doivent rester valides")]
        public void OldSaves_LoadWithoutEquipment()
        {
            var profile = new N29.HeroesDawn.Application.Ports.ProfileData();
            profile.team.Add(new N29.HeroesDawn.Application.Ports.MemberSave { id = "ancre", level = 5, inSquad = true });
            var s = new GameSession(TestContent.Build());
            s.ApplyProfile(profile);
            Assert.That(s.Find(Ancre)!.Items, Is.Empty);
            Assert.That(s.Find(Ancre)!.Cards.All(c => !c.HasValue), Is.True);
        }

        [Test, Description("Vérifie qu'un objet ou une carte inconnu du jeu est ignoré au chargement et qu'au plus deux objets sont repris — parce que le contenu peut changer entre deux versions")]
        public void LoadingDropsUnknownOrExtraEquipment()
        {
            var profile = new N29.HeroesDawn.Application.Ports.ProfileData();
            var m = new N29.HeroesDawn.Application.Ports.MemberSave { id = "verre", level = 5, inSquad = true };
            m.items.AddRange(new[] { "kevlar_vest", "ghost", "running_shoes", "riot_shield" });
            m.cards.AddRange(new[] { "", "card_shackle" });
            profile.team.Add(m);
            var s = new GameSession(TestContent.Build());
            s.ApplyProfile(profile);
            var verre = s.Find(Verre)!;
            Assert.That(verre.Items.Count, Is.EqualTo(2));
            Assert.That(verre.Cards[0], Is.EqualTo(new CardId("card_shackle")), "la carte du 2e emplacement remonte quand le 1er est vide");
            Assert.That(verre.Cards[1], Is.Null);
        }

        [Test, Description("Vérifie que résoudre un événement narratif peut faire trouver un objet (de façon déterministe par plateau et case) et qu'il rejoint l'inventaire — parce que les événements sont une source d'objets")]
        public void NarrativeEvent_CanFindAnItem()
        {
            // Un plateau qui trouve toujours : chance 100 %.
            var balance = new BalanceConfig { EventItemChancePercent = 100 };
            var content = TestContent.Build(balance);
            var session = new GameSession(content);
            var zone = content.Zone(PrototypeContent.Zone11);
            var run = ZoneRun.ForTests(content, zone, 7UL, 1, true, "n04");
            session.Run = run; session.RunZone = zone;
            var effects = new ZoneEffects(session, _app.Publisher<N29.HeroesDawn.Application.Messages.WalletChanged>(), _app.Publisher<N29.HeroesDawn.Application.Messages.ZoneValidated>(), _app.Save, _app.Logger);
            ZoneActions.Apply(effects, r => r.MoveTo(new CaseId("n05")));
            var events = ZoneActions.Apply(effects, r => r.ResolveCase(true)).Value;
            var found = events.OfType<ItemFoundEvent>().ToList();
            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(CollectionOps.OwnedItems(session, found[0].Item), Is.EqualTo(1));
            var again = ZoneRun.ForTests(content, zone, 7UL, 1, true, "n04");
            again.MoveTo(new CaseId("n05"));
            Assert.That(again.ResolveCase(true).OfType<ItemFoundEvent>().Single().Item, Is.EqualTo(found[0].Item), "même graine, même objet");
        }

        [Test, Description("Vérifie qu'une victoire contre une case Élite offre un booster d'exploit — parce que les exploits de jeu sont une source de boosters")]
        public void Booster_FromAnEliteVictory()
        {
            var content = _app.Session.Content;
            var zone = content.Zone(PrototypeContent.Zone11);
            var run = ZoneRun.ForTests(content, zone, 7UL, 1, true, "n22");
            run.MoveTo(new CaseId("n23"));
            var victory = run.ResolveCombat(true);
            var earned = victory.OfType<BoosterEarnedEvent>().ToList();
            Assert.That(earned.Count, Is.EqualTo(1));
            Assert.That(earned[0].Source, Is.EqualTo(BoosterSource.Exploit));

        }
    }
}
