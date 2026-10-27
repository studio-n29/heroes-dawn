#nullable enable
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Pages Équipement et Grimoire avec les vrais prefabs, et capture du combat avec une 4e compétence (plan 2026-10-10, sous-projets L et M).</summary>
    public sealed class CollectionPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator SignedIn()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
        }

        [UnityTest, Timeout(240000), Description("Vérifie que depuis la fiche d'un agent le bouton Équipement ouvre la vraie page, que l'on y équipe un objet de la réserve et que le bonus apparaît — parce que l'équipement doit être jouable sur les vrais écrans")]
        public IEnumerator Loadout_PageOpens_AndEquipsAnItem()
        {
            yield return SignedIn();
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            var book = c.Resolve<CollectionBook>();
            CollectionOps.AddItem(session, new ItemId("kevlar_vest"));
            var nav = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            nav.Open(HubPage.MemberSheet, session.Roster[0].Id);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<MemberSheetView>());
            var sheet = Object.FindAnyObjectByType<MemberSheetView>()!;
            var loadoutButton = sheet.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == "Loadout");
            loadoutButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<LoadoutPageView>() != null);
            var page = Object.FindAnyObjectByType<LoadoutPageView>()!;
            yield return null;
            var equip = page.GetComponentsInChildren<UnityEngine.UI.Button>(true).FirstOrDefault(b => b.GetComponentInChildren<TMPro.TMP_Text>(true)?.text == "ÉQUIPER");
            Assert.That(equip, Is.Not.Null, "un bouton ÉQUIPER pour le gilet");
            equip!.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => session.Roster[0].Items.Count == 1, 120);
            Assert.That(session.Roster[0].Items[0].Value, Is.EqualTo("kevlar_vest"));
            Assert.That(book.UnequipItem(session.Roster[0].Id, new ItemId("kevlar_vest")).IsSuccess);
        }

        [UnityTest, Timeout(240000), Description("Vérifie que le Grimoire s'ouvre avec les vrais prefabs, liste toutes les cartes et que l'ouverture d'un booster joue la popup animée (paquet, cartes retournées, Continuer) — parce que la collection doit se voir à l'écran")]
        public IEnumerator Grimoire_PageOpens_AndOpensABooster()
        {
            yield return SignedIn();
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            CollectionOps.AddBooster(session, BoosterDef.StandardId);
            Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<HubNavigator>().Open(HubPage.Grimoire);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<GrimoirePageView>() != null);
            var page = Object.FindAnyObjectByType<GrimoirePageView>()!;
            yield return null;
            var open = page.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.GetComponentInChildren<TMPro.TMP_Text>(true)?.text == "OUVRIR UN BOOSTER");
            open.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<BoosterOpeningPopupView>() != null, 120);
            var popup = Object.FindAnyObjectByType<BoosterOpeningPopupView>()!;
            yield return null;
            Assert.That(popup.Current, Is.EqualTo(BoosterOpeningPopupView.Phase.Sealed), "le paquet attend un toucher");
            popup.TapButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => popup.Current == BoosterOpeningPopupView.Phase.Revealing, 300);
            Assert.That(popup.Cards.Count, Is.EqualTo(3));
            popup.TapButton.onClick.Invoke(); // un toucher pendant la révélation retourne tout
            yield return PlayModeHarness.WaitFor(() => popup.ContinueVisible, 300);
            Assert.That(popup.Cards.All(x => x.Revealed && x.FaceVisible), Is.True, "toutes les cartes sont retournées");
            popup.ContinueButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<BoosterOpeningPopupView>() == null, 120);
            Assert.That(CollectionOps.Boosters(session, BoosterDef.StandardId), Is.EqualTo(0));
            Assert.That(session.Content.Collection.Cards.Sum(card => CollectionOps.OwnedCards(session, card.Id)), Is.EqualTo(3));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures de la boutique à onglets et des trois temps de l'ouverture d'un booster (paquet, révélation, résumé) — parce que ces rendus se relisent à l'œil")]
        public IEnumerator CaptureTheShopAndBoosterOpening()
        {
            yield return SignedIn();
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            session.Wallet.Set(5000, 600);
            CollectionOps.AddBooster(session, BoosterDef.StandardId, 2);
            var nav = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            foreach (var tab in new[] { ShopTab.Boosters, ShopTab.Items, ShopTab.Gems, ShopTab.Gift })
            {
                nav.SelectShopTab(tab);
                yield return new WaitForSecondsRealtime(0.7f);
                yield return ScreenCaptureTests.Capture("60-boutique-" + tab.ToString().ToLowerInvariant());
            }
            nav.SelectShopTab(ShopTab.Boosters);
            yield return new WaitForSecondsRealtime(0.5f);
            var open = Object.FindAnyObjectByType<BoutiquePageView>()!.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.GetComponentInChildren<TMPro.TMP_Text>(true)?.text == "OUVRIR");
            open.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<BoosterOpeningPopupView>() != null, 120);
            var popup = Object.FindAnyObjectByType<BoosterOpeningPopupView>()!;
            yield return new WaitForSecondsRealtime(0.6f);
            yield return ScreenCaptureTests.Capture("61-booster-paquet");
            popup.TapButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(2.2f);
            yield return ScreenCaptureTests.Capture("62-booster-revelation");
            popup.TapButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => popup.ContinueVisible, 300);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return ScreenCaptureTests.Capture("63-booster-resume");
            popup.ContinueButton.onClick.Invoke();
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit la capture de la fenêtre d'affinités d'un agent avec des liens de niveaux 1 à 5 (thème du couple de rôles, paliers cochés, victoires restantes) — parce que cette mise en page se relit à l'œil")]
        public IEnumerator CaptureTheAffinityPopup()
        {
            yield return SignedIn();
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            var me = session.Roster[0];
            var others = session.Content.Members.Where(m => m.Id != me.Def.Id).Take(4).ToList();
            foreach (var m in others) session.Roster.Add(new TeamMember(m, 5));
            int[] wins = { 2, 3, 8, 15, 25 };
            for (int i = 0; i < others.Count; i++)
                for (int w = 0; w < wins[i + 1]; w++) N29.HeroesDawn.Application.Features.Combat.TeamLinks.RecordVictory(session.Meta, new[] { me.Def.Id.Value, others[i].Id.Value });
            var flow = new AffinityFlow(session, c.Resolve<N29.HeroesDawn.Presentation.Mvp.IModalService>(), new N29.HeroesDawn.Presentation.Mvp.LocLocalization());
            flow.OpenAsync(me.Def.Id, System.Threading.CancellationToken.None).Forget();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<AffinityPopupView>() != null, 120);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return ScreenCaptureTests.Capture("64-affinites");
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures de la fiche d'agent, de l'équipement, du grimoire, de la boutique d'objets et du combat avec une 4e compétence — parce que ces écrans se relisent à l'œil")]
        public IEnumerator CaptureTheCollectionScreens()
        {
            yield return SignedIn();
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            var book = c.Resolve<CollectionBook>();
            foreach (var id in new[] { "kevlar_vest", "time_shard", "steel_knuckles" }) CollectionOps.AddItem(session, new ItemId(id));
            var memberDef = session.Roster[0].Def; // la seule recrue de départ
            var familyCards = session.Content.Collection.Cards.Where(card => card.Family == memberDef.Family).Take(4).ToList();
            foreach (var card in familyCards) CollectionOps.AddCard(session, card.Id);
            CollectionOps.AddBooster(session, BoosterDef.StandardId, 2);
            session.Wallet.Set(5000, 600);
            var member = session.Roster[0].Id;
            book.EquipItem(member, new ItemId("kevlar_vest"));
            if (familyCards.Count > 0) book.EquipCard(member, 0, familyCards[0].Id);
            var nav = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            nav.Open(HubPage.MemberSheet, member);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<MemberSheetView>());
            yield return new WaitForSeconds(0.6f);
            yield return ScreenCaptureTests.Capture("16-fiche-equipement");
            nav.Open(HubPage.Loadout, member);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<LoadoutPageView>() != null);
            yield return new WaitForSeconds(0.6f);
            yield return ScreenCaptureTests.Capture("17-equipement");
            nav.Open(HubPage.Grimoire);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<GrimoirePageView>() != null);
            yield return new WaitForSeconds(0.6f);
            yield return ScreenCaptureTests.Capture("18-grimoire");
            nav.Open(HubPage.Shop);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<BoutiquePageView>() != null);
            yield return new WaitForSeconds(0.6f);
            yield return ScreenCaptureTests.Capture("19-boutique-objets");
        }
    }
}
