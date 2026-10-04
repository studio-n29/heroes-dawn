#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.Wip;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Parcours du hub au clic réel : barre de navigation, pièces, Retour, BIENTÔT, MISSION, achat, renouvellement, déconnexion.</summary>
    public sealed class HubNavigationFlowTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();
        static BackButtonRouter Router => Container.Resolve<BackButtonRouter>();
        static HubShellView Shell => Object.FindAnyObjectByType<HubShellView>()!;

        static IEnumerator OnHub()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True, "connexion → QG");
        }

        static IEnumerator Tab(HubPage tab)
        {
            Shell.TabButton(tab).Button.onClick.Invoke();
            yield return null;
            yield return null;
        }

        /// <summary>Nombre de pages vivantes sous la zone de page de la coque.</summary>
        static int LivePages() => Object.FindObjectsByType<ViewBase>(FindObjectsSortMode.None)
            .Count(v => v != null && v.gameObject.activeInHierarchy && !(v is HubShellView) && v.GetComponentInParent<HubShellView>() != null);

        [UnityTest, Description("Vérifie que les 5 onglets affichent chacun leur page, une seule à la fois — parce que la barre de navigation est la colonne vertébrale du hub (spec §3.2)")]
        public IEnumerator TabBar_SwitchesAllFivePages()
        {
            yield return OnHub();
            yield return Tab(HubPage.Team);
            Assert.That(PlayModeHarness.Visible<TeamPageView>(), Is.True);
            yield return Tab(HubPage.Recruit);
            Assert.That(PlayModeHarness.Visible<RecruitPageView>(), Is.True);
            yield return Tab(HubPage.Pass);
            Assert.That(PlayModeHarness.Visible<WipPageView>(), Is.True);
            yield return Tab(HubPage.Shop);
            Assert.That(PlayModeHarness.Visible<WipPageView>(), Is.True);
            yield return Tab(HubPage.Qg);
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True);
            Assert.That(LivePages(), Is.EqualTo(1), "une seule page vivante");
        }

        [UnityTest, Description("Vérifie qu'une pièce s'ouvre depuis le QG et que Retour (bouton puis touche) ramène au QG — parce qu'une pièce s'empile sur le QG (spec §3.3)")]
        public IEnumerator Room_OpenFromQg_BackReturnsToQg()
        {
            yield return OnHub();
            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Salon)!.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<SalonPageView>());
            Assert.That(PlayModeHarness.Visible<SalonPageView>(), Is.True);
            Assert.That(Shell.BackButton.gameObject.activeSelf, Is.True);
            Shell.BackButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True);

            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Infirmary)!.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<InfirmaryPageView>());
            Assert.That(Router.Dispatch(), Is.True);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True);
        }

        [UnityTest, Description("Vérifie que Retour sur le QG demande « Quitter le jeu ? » et qu'un second Retour referme la question sans quitter — parce que le jeu ne se ferme jamais sans confirmation (Review Focus 4)")]
        public IEnumerator Back_OnQg_AsksQuitAndCancelClosesIt()
        {
            yield return OnHub();
            Assert.That(Router.Dispatch(), Is.True);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ConfirmPopupView>() != null);
            Assert.That(Object.FindAnyObjectByType<ConfirmPopupView>(), Is.Not.Null, "« Quitter le jeu ? » est ouverte");
            Assert.That(Router.Dispatch(), Is.True);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ConfirmPopupView>() == null);
            Assert.That(Object.FindAnyObjectByType<ConfirmPopupView>(), Is.Null);
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub));
        }

        [UnityTest, Description("Vérifie que toucher une pièce BIENTÔT affiche le toast « … arrive bientôt » sans changer de page — parce que les fonctions à venir restent visibles mais inactives (spec §3.5)")]
        public IEnumerator WipRoom_TapShowsSoonToast_NoNavigation()
        {
            yield return OnHub();
            var mark = Object.FindObjectsByType<WipMark>(FindObjectsSortMode.None)
                .First(m => m.gameObject.activeInHierarchy && m.FeatureId == WipCatalog.Operations.Id);
            ExecuteEvents.Execute(mark.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            var expected = Loc.F("wip.toast", Loc.T(WipCatalog.Operations.TitleKey));
            yield return PlayModeHarness.WaitFor(() => Object.FindObjectsByType<ToastItemView>(FindObjectsSortMode.None)
                .Any(t => t.GetComponentInChildren<TMP_Text>().text == expected));
            Assert.That(Object.FindObjectsByType<ToastItemView>(FindObjectsSortMode.None)
                .Any(t => t.GetComponentInChildren<TMP_Text>().text == expected), Is.True, "toast BIENTÔT affiché");
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True, "aucune navigation");
        }

        [UnityTest, Description("Vérifie qu'un MISSION sur le QG puis un MISSION sur l'Équipe avant le départ du hub ne démarrent la zone qu'une fois — parce qu'un second départ remplacerait la partie (relecture, M5)")]
        public IEnumerator Mission_ThenTeamMissionBeforeUnload_StartsZoneOnce()
        {
            yield return OnHub();
            var session = Container.Resolve<GameSession>();
            yield return PlayModeHarness.Deploy(Object.FindAnyObjectByType<QgPageView>()!.MissionButton);
            var firstRun = session.Run;
            PlayModeHarness.DeployNow(); // second départ avant le déchargement du hub
            Assert.That(firstRun, Is.Not.Null);
            Assert.That(session.Run, Is.SameAs(firstRun), "la partie n'est pas remplacée");
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
        }

        [UnityTest, Description("Vérifie que des touchers répétés sur un bouton grisé le laissent à sa place — parce qu'une secousse qui part d'une position déjà décalée ferait dériver le bouton (relecture, M2)")]
        public IEnumerator DisabledButton_RepeatedTaps_NoDrift()
        {
            yield return OnHub();
            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Infirmary)!.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<InfirmaryPageView>());
            var heal = Object.FindAnyObjectByType<InfirmaryPageView>()!.HealButton.GetComponent<UiButton>();
            Assert.That(heal.Interactable, Is.False, "aucune fatigue : soin grisé");
            var origin = heal.Rect.anchoredPosition;
            for (int i = 0; i < 4; i++)
            {
                heal.OnPointerClick(new PointerEventData(EventSystem.current));
                yield return null;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.That(Vector2.Distance(heal.Rect.anchoredPosition, origin), Is.LessThan(0.01f));
        }

        [UnityTest, Timeout(120000), Description("Vérifie que MISSION lance la zone depuis le QG puis depuis l'Équipe — parce que les deux boutons mènent au plateau (spec §4)")]
        public IEnumerator Mission_FromQgAndFromTeam_LaunchesZone()
        {
            yield return OnHub();
            yield return PlayModeHarness.Deploy(Object.FindAnyObjectByType<QgPageView>()!.MissionButton);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board), "QG → plateau");

            yield return PlayModeHarness.BackToHqConfirmed();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning, 600);
            yield return Tab(HubPage.Team);
            yield return PlayModeHarness.Deploy(Object.FindAnyObjectByType<TeamPageView>()!.MissionButton);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board), "Équipe → plateau");
        }

        [UnityTest, Description("Vérifie qu'un achat passe par la confirmation et ajoute le héros à l'équipe — parce que toute dépense de PR est confirmée (M31)")]
        public IEnumerator Recruit_ViaConfirmation_AddsMember()
        {
            yield return OnHub();
            yield return Tab(HubPage.Recruit);
            var session = Container.Resolve<GameSession>();
            yield return PlayModeHarness.WaitFor(() => session.Shop.Offers.Count > 0);
            int roster = session.Roster.Count;
            yield return PlayModeHarness.RecruitAffordable();
            Assert.That(session.Roster.Count, Is.EqualTo(roster + 1), "recrue ajoutée");
            Assert.That(Object.FindAnyObjectByType<RecruitConfirmPopupView>(), Is.Null, "confirmation refermée");
        }

        [UnityTest, Description("Vérifie qu'un clic réel sur Renouveler (gratuit) passe au tirage suivant et réaffiche les cartes — parce que c'est le geste quotidien de la boutique éphémère")]
        public IEnumerator Reroll_ShowsNewOffers()
        {
            yield return OnHub();
            yield return Tab(HubPage.Recruit);
            var session = Container.Resolve<GameSession>();
            yield return PlayModeHarness.WaitFor(() => session.Shop.Offers.Count > 0);
            Assert.That(session.Shop.Draw, Is.EqualTo(0));
            var page = Object.FindAnyObjectByType<RecruitPageView>()!;
            page.RerollButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => session.Shop.Draw == 1);
            Assert.That(session.Shop.Draw, Is.EqualTo(1));
            int shown = Enumerable.Range(0, 3).Count(i => page.Card(i).gameObject.activeSelf);
            Assert.That(shown, Is.EqualTo(session.Shop.Offers.Count));
        }

        [UnityTest, Description("Vérifie que Paramètres ▸ Se déconnecter ramène à l'écran de connexion — parce que ce bouton a quitté l'en-tête du QG (spec §4)")]
        public IEnumerator Settings_Logout_ReturnsToLogin()
        {
            yield return OnHub();
            Shell.SettingsButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<SettingsPageView>());
            var settings = Object.FindAnyObjectByType<SettingsPageView>()!;
            settings.CategoryButton(6).onClick.Invoke(); // Compte
            yield return null;
            settings.ActionButton(N29.HeroesDawn.Presentation.Common.Loc.T("settings.logout"))!.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<LoginView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Login));
        }
    }
}
