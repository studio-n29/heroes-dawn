#nullable enable
using System.Collections;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Captures des écrans pour relecture visuelle (Code/docs/test-reports/captures, ignoré par git).
    /// Explicite : ne tourne que lorsqu'on le demande par -testFilter.
    /// </summary>
    public sealed class ScreenCaptureTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();

        static string CaptureArg(string key, string fallback)
        {
            var args = System.Environment.GetCommandLineArgs();
            int index = System.Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
        }

        internal static string Folder => Path.GetFullPath(CaptureArg("-captureFolder", Path.Combine(UnityEngine.Application.dataPath, "../../docs/test-reports/captures")));

        internal static IEnumerator Capture(string name)
        {
            for (int i = 0; i < 20; i++) yield return null; // fondus et mises en page terminés
            // En batchmode, la fin d'image n'arrive jamais (WaitForEndOfFrame bloque) : rendu explicite par une caméra.
            int w = int.Parse(CaptureArg("-captureWidth", "2160"));
            int h = int.Parse(CaptureArg("-captureHeight", "972"));
            var rt = new RenderTexture(w, h, 24);
            var camGo = new GameObject("CaptureCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.targetTexture = rt;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var modes = new RenderMode[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode;
                if (!canvases[i].isRootCanvas) continue;
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = cam;
                canvases[i].planeDistance = 10f - canvases[i].sortingOrder * 0.01f;
            }
            Canvas.ForceUpdateCanvases();
            // Let canvas scalers and responsive layouts observe the render target's aspect.
            yield return null;
            Canvas.ForceUpdateCanvases();
            foreach (var layout in Object.FindObjectsByType<TacticalPageLayout>(FindObjectsSortMode.None)) layout.Reflow();
            Canvas.ForceUpdateCanvases();
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            for (int i = 0; i < canvases.Length; i++) if (canvases[i] != null && canvases[i].isRootCanvas) canvases[i].renderMode = modes[i];
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            Object.Destroy(camGo);
            rt.Release();
        }

        /// <summary>Capture après la fin des animations d'entrée (cascade, glissement : temps réel).</summary>
        static IEnumerator CaptureSettled(string name)
        {
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture(name);
        }

        static HubShellView Shell => Object.FindAnyObjectByType<HubShellView>()!;

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Relit les huit catégories de réglages et les archives dans le viewport choisi, avec les vrais contrôles de catégorie")]
        public IEnumerator CaptureAllSettingsAndArchives()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            var nav = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            Shell.SettingsButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<SettingsPageView>());
            var settings = Object.FindAnyObjectByType<SettingsPageView>()!;
            var names = new[] { "80-parametres-audio", "81-parametres-graphismes", "82-parametres-accessibilite", "83-parametres-langue", "84-parametres-notifications", "86-parametres-controles", "87-parametres-compte", "85-parametres-support" };
            for (int i = 0; i < names.Length; i++)
            {
                settings.CategoryButton(i).onClick.Invoke();
                yield return CaptureSettled(names[i]);
                Assert.That(settings.Rows.Count, Is.GreaterThan(0));
            }
            nav.SelectTab(HubPage.Profile);
            yield return null;
            nav.Open(HubPage.Archives);
            yield return CaptureSettled("15b-archives");
        }

        static IEnumerator Tab(HubPage tab)
        {
            Shell.TabButton(tab).Button.onClick.Invoke();
            yield return null;
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit une capture de chaque page du hub — parce que chaque page se compare à sa maquette (M10, M11, M12, M20, M21, M30, M31, M80, M85)")]
        public IEnumerator CaptureHubPages()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            session.Find(new N29.HeroesDawn.Core.MemberId("giulia_rossi"))!.Fatigue = 30; // la première recrue de l'équipe de départ
            yield return CaptureSettled("10-qg");
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("10b-qg-sans-tuto");
            Object.FindAnyObjectByType<QgPageView>()!.MissionButton.onClick.Invoke();
            yield return CaptureSettled("13-carte");
            Shell.BackButton.Button.onClick.Invoke();
            yield return null;

            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Lab)!.onClick.Invoke();
            yield return CaptureSettled("11-labo");
            Shell.BackButton.Button.onClick.Invoke();
            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Lab)!.onClick.Invoke();

            yield return Tab(HubPage.Team);
            yield return CaptureSettled("20-equipe");
            Object.FindAnyObjectByType<TeamPageView>()!.Tiles[0].TapButton.onClick.Invoke();
            yield return CaptureSettled("21-fiche");
            Object.FindAnyObjectByType<MemberSheetView>()!.ItemTileButton(0).onClick.Invoke();
            yield return CaptureSettled("21b-fiche-objets");
            Object.FindAnyObjectByType<MemberSheetView>()!.SkillTileButton(2).onClick.Invoke();
            yield return CaptureSettled("21c-fiche-cartes");

            yield return Tab(HubPage.Recruit);
            yield return CaptureSettled("30-recruter");
            Object.FindAnyObjectByType<RecruitPageView>()!.Card(0).RecruitButton.onClick.Invoke();
            yield return CaptureSettled("31-confirmation");
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.RecruitConfirmPopupView>()!.CancelButton.Button.onClick.Invoke();
            yield return null;

            yield return Tab(HubPage.Pass);
            yield return CaptureSettled("40-pass");
            yield return Tab(HubPage.Shop);
            yield return CaptureSettled("50-boutique");
            foreach (var shopTab in new[] { (ShopTab.Boosters, "50b-boutique-boosters"), (ShopTab.Gems, "50c-boutique-gemmes"), (ShopTab.Gift, "50d-boutique-cadeau") })
            {
                Object.FindAnyObjectByType<ShopTabsView>()!.Tab((int)shopTab.Item1).Button.onClick.Invoke();
                yield return CaptureSettled(shopTab.Item2);
            }
            yield return Tab(HubPage.Qg);
            yield return null;
            foreach (var room in new[] { (HubPage.Operations, "13-operations"), (HubPage.Lab, "14-labo"), (HubPage.Grimoire, "16-grimoire"), (HubPage.Inventory, "17-inventaire") })
            {
                if (room.Item1 == HubPage.Operations)
                {
                    // Des agents hors escouade, dont un déjà parti en patrouille, pour montrer la fiche de mission.
                    var extra = session.Content.Members.Where(d => session.Find(d.Id) == null).Take(3).ToList();
                    foreach (var d in extra) session.Roster.Add(new TeamMember(d, 6));
                    Container.Resolve<N29.HeroesDawn.Application.Features.Meta.Expeditions>().Launch("patrol", new[] { extra[0].Id.Value });
                }
                Object.FindAnyObjectByType<QgPageView>()!.RoomButton(room.Item1)!.onClick.Invoke();                yield return CaptureSettled(room.Item2);
                Shell.BackButton.Button.onClick.Invoke();
                yield return null;
            }
            yield return Tab(HubPage.Profile);
            yield return CaptureSettled("15-profil");
            Shell.SettingsButton.Button.onClick.Invoke();
            yield return CaptureSettled("80-parametres-audio");
            Object.FindAnyObjectByType<SettingsPageView>()!.CategoryButton(2).onClick.Invoke();
            yield return CaptureSettled("82-parametres-accessibilite");
            Object.FindAnyObjectByType<SettingsPageView>()!.CategoryButton(7).onClick.Invoke();
            yield return CaptureSettled("85-parametres-support");
            Assert.That(File.Exists(Path.Combine(Folder, "85-parametres-support.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit les captures du tournoi simulé : départ puis tournoi fini — parce que la page se relit à l'œil (L13)")]
        public IEnumerator CaptureTournament()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);
            Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<N29.HeroesDawn.Presentation.Hub.HubNavigator>().Open(HubPage.Tournament);
            yield return CaptureSettled("92b-tournoi");
            var tournament = Container.Resolve<N29.HeroesDawn.Application.Features.Meta.Tournament>();
            while (!tournament.Finished) tournament.PlayNext();
            yield return CaptureSettled("92c-tournoi-fini");
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit les captures de la narration : page Chapitres, dossier d'un agent et fenêtre d'ouverture d'un chapitre — parce que ces écrans se relisent à l'œil (lot G)")]
        public IEnumerator CaptureNarration()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);

            Object.FindAnyObjectByType<QgPageView>()!.MissionButton.onClick.Invoke();
            yield return CaptureSettled("90-carte-bouton-chapitres");
            // Deux zones validées : deux souvenirs débloqués dans la colonne de droite, dont un ouvert.
            var narrationSession = Container.Resolve<GameSession>();
            narrationSession.CompletedZones.Add("zone_ny_1_1");
            narrationSession.CompletedZones.Add("zone_ny_1_2");
            Object.FindAnyObjectByType<CityMapPageView>()!.ChaptersButton.onClick.Invoke();
            yield return CaptureSettled("91-chapitres");
            Object.FindAnyObjectByType<ChaptersPageView>()!.SideRows[0].Buttons[0].Button.onClick.Invoke();
            yield return CaptureSettled("91b-chapitres-souvenir");
            // Repaire : Brooklyn terminé, Brick déjà vaincu ; le bouton REPAIRE du bandeau ouvre la page, puis ENTRER demande confirmation.
            narrationSession.CompletedZones.Add("zone_ny_1_3");
            narrationSession.CompletedZones.Add("zone_ny_1_4");
            N29.HeroesDawn.Application.Features.World.LairBook.RecordBossDefeat(narrationSession.Meta, "zone_lair_amif_vault", "n04");
            Object.FindAnyObjectByType<ChaptersPageView>()!.HeaderButtonA.Button.onClick.Invoke();
            yield return CaptureSettled("91c-repaire");
            Object.FindAnyObjectByType<LairPageView>()!.HeaderButtonA.Button.onClick.Invoke();
            yield return CaptureSettled("91d-repaire-confirmation");
            Object.FindAnyObjectByType<ConfirmPopupView>()!.CancelButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ConfirmPopupView>() == null);

            yield return Tab(HubPage.Team);
            Object.FindAnyObjectByType<TeamPageView>()!.Tiles[0].TapButton.onClick.Invoke();
            yield return CaptureSettled("92-fiche-bouton-dossier");
            Object.FindAnyObjectByType<MemberSheetView>()!.DossierButton.onClick.Invoke();
            yield return CaptureSettled("93-dossier");
            Shell.BackButton.Button.onClick.Invoke();
            yield return null;
            yield return Tab(HubPage.Qg);

            Object.FindAnyObjectByType<QgPageView>()!.MissionButton.onClick.Invoke();
            yield return null;
            Object.FindAnyObjectByType<CityMapPageView>()!.DeployButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ChapterPopupView>() != null);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return Capture("94-ouverture-chapitre");
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit une capture en cours de partie de chacun des mini-jeux 2D du lot H (fenêtre commune, fond, éléments de jeu, joystick, panneau de l'ingénierie) — parce que ces écrans se relisent à l'œil")]
        public IEnumerator CaptureMiniGames()
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeContent.Zone11, "n15");
            var loc = new N29.HeroesDawn.Presentation.Mvp.LocLocalization();
            var host = Object.FindAnyObjectByType<ModalLayer>()!;
            foreach (var (id, env, name) in new[]
            {
                ("dna_lab", new ZoneEnvironment(Biome.Industrial, Weather.Day), "95-minijeu-adn"),
                ("infiltration_camp", new ZoneEnvironment(Biome.Forest, Weather.Night), "96-minijeu-infiltration"),
                ("gears_workshop", new ZoneEnvironment(Biome.Industrial, Weather.Day), "97-minijeu-engrenage"),
                ("survival_snow", new ZoneEnvironment(Biome.Snow, Weather.Snow), "98-minijeu-survie"),
                ("rhythm_rooftop", new ZoneEnvironment(Biome.City, Weather.Night), "99-minijeu-rythme"),
                ("free_throw_court", new ZoneEnvironment(Biome.City, Weather.Day), "100-minijeu-tir-franc"),
                ("rescue_tool", new ZoneEnvironment(Biome.Industrial, Weather.Day), "101-minijeu-ingenierie"),
                ("crate_pick_depot", new ZoneEnvironment(Biome.Harbor, Weather.Day), "102-minijeu-caisses"),
            })
            {
                var plan = ActivityGamesPlayTests.PlanOf(id, env);
                var driver = MiniGameDrivers.Create(plan, loc)!;
                var request = ActivityGamesPlayTests.CasePresenter().MiniGame2DRequestFor(plan, driver);
                var view = host.Spawn<IMiniGame2DPopupView>();
                var task = view.PlayAsync(request, default);
                var popup = (MiniGame2DPopupView)view;
                if (id == "dna_lab" || id == "rescue_tool") { yield return new WaitForSecondsRealtime(0.5f); yield return Capture("103-minijeu-intro-" + id); }
                popup.Input.PressAt(new Vector2(0.5f, 0.5f));
                yield return null;
                popup.Input.ReleaseNow();
                if (driver.UsesStick) { popup.Input.PressAt(new Vector2(0.5f, 0.5f)); popup.Input.DragTo(new Vector2(0.58f, 0.5f)); }
                if (driver.Game is N29.HeroesDawn.Domain.Activities.Games.RescueEngineeringGame rescue)
                {
                    var panel = popup.GetComponentInChildren<RescuePanelView>(true);
                    yield return null;
                    for (int i = 0; i < 2; i++) panel.PoolButton(System.Linq.Enumerable.ToList(rescue.Pool).IndexOf(rescue.Solution[i])).Button.onClick.Invoke();
                }
                yield return new WaitForSecondsRealtime(id == "rhythm_rooftop" ? 2.6f : 0.8f);
                yield return Capture(name);
                popup.Input.ReleaseNow();
                popup.AbandonButton.Button.onClick.Invoke();
                while (!task.Status.IsCompleted()) yield return null;
                Object.Destroy(popup.gameObject);
                yield return null;
            }
            Assert.That(File.Exists(Path.Combine(Folder, "102-minijeu-caisses.png")));
        }

        static bool HasView(string typeName) => Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Any(m => m != null && m.GetType().Name == typeName);

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Capture les écrans qui n'avaient pas de capture : journal de quêtes, confirmation de retour, potion, dialogue à choix — parce que la comparaison avec les planches 16 et 19 en a besoin")]
        public IEnumerator CaptureMissingScreens()
        {
            // Journal de quêtes (QG).
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeContent.Zone11, "n21");
            // Retour au QG depuis le plateau : fenêtre de confirmation.
            Object.FindAnyObjectByType<BoardView>()!.BackButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ConfirmPopupView>() != null, 300);
            yield return CaptureSettled("120-retour-qg-confirmation");
            Object.FindAnyObjectByType<ConfirmPopupView>()!.CancelButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ConfirmPopupView>() == null, 300);

            // Dialogue à choix (case n21 de la zone 1.1).
            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n21"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialoguePopupView>() != null, 600);
            yield return CaptureSettled("121-dialogue-choix");
            var dialogue = Object.FindAnyObjectByType<DialoguePopupView>()!;
            var script = Container.Resolve<GameSession>().Run!.CurrentActivity!.Level.PayloadAs<N29.HeroesDawn.Domain.Activities.DialogueScript>()!;
            foreach (var line in script.Lines)
            {
                yield return PlayModeHarness.WaitFor(() => dialogue.ChoiceButtons == line.Choices.Count);
                dialogue.ChoiceButton(line.Choices.First(c => c.Score == 2).Index).onClick.Invoke();
                yield return PlayModeHarness.WaitFor(() => dialogue.ChoiceButtons == 0, 120);
            }
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialoguePopupView>() == null, 900);
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Capture la potion et le journal de quêtes")]
        public IEnumerator CapturePotionAndQuests()
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeZones.Zone13, "n06", prototypeZone: true);
            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n06"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<PotionPopupView>() != null, 600);
            yield return CaptureSettled("122-potion");

            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);
            Object.FindAnyObjectByType<QgPageView>()!.QuestsButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => HasView("QuestLogPopupView"), 300);
            yield return CaptureSettled("123-journal-quetes");
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures du compte : titre avec le bouton, fenêtre Compte (connexion, erreur, création), choix de progression, profil invité et connecté — parce que ces écrans se relisent à l'œil (lot I)")]
        public IEnumerator CaptureAccount()
        {
            PlayerPrefs.DeleteKey("hd.cloud.sync_state");
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            yield return CaptureSettled("110-titre-compte");
            Object.FindAnyObjectByType<LoginView>()!.AccountButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Account.AccountPopupView>() != null);
            var popup = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Account.AccountPopupView>()!;
            yield return CaptureSettled("111-compte-connexion");
            popup.EmailField.text = "pas-une-adresse";
            popup.PasswordField.text = "motdepasse1";
            popup.SubmitButton.Button.onClick.Invoke();
            yield return CaptureSettled("112-compte-erreur");
            popup.TabSignUpButton.Button.onClick.Invoke();
            popup.EmailField.text = "moi@exemple.fr";
            popup.NameField.text = "Moi";
            yield return CaptureSettled("113-compte-creation");
            popup.CloseButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Account.AccountPopupView>() == null);

            var host = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ModalLayer>()!;
            var choice = host.Spawn<N29.HeroesDawn.Presentation.Account.IProgressChoicePopupView>();
            _ = choice.AskAsync(new N29.HeroesDawn.Presentation.Account.ProgressChoiceData
            {
                Title = Loc.T("account.choice.title"), Hint = Loc.T("account.choice.hint"), KeepDevice = Loc.T("account.choice.keep_device"),
                KeepAccount = Loc.T("account.choice.keep_account"), Later = Loc.T("account.choice.later"),
                Device = new N29.HeroesDawn.Presentation.Account.ProgressCardData { Header = Loc.T("account.choice.device"), Summary = Loc.F("account.choice.summary", 5, 123, 2, "28/10/2026 21:10") },
                Account = new N29.HeroesDawn.Presentation.Account.ProgressCardData { Header = Loc.T("account.choice.account"), Summary = Loc.F("account.choice.summary", 9, 777, 4, "27/10/2026 18:42") },
            }, default);
            yield return CaptureSettled("114-choix-progression");
            ((Component)choice).GetComponent<N29.HeroesDawn.Presentation.Account.ProgressChoicePopupView>().LaterButton.Button.onClick.Invoke();
            yield return null;
            Object.Destroy(((Component)choice).gameObject);   // fenêtre créée à la main pour la capture : elle ne passe pas par le service de modales
            yield return null;

            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Tab(HubPage.Profile);
            yield return CaptureSettled("115-profil-invite");
            var link = Container.Resolve<N29.HeroesDawn.Application.Features.Account.AccountLink>();
            var task = link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", default);
            yield return PlayModeHarness.WaitFor(() => task.Status.IsCompleted(), 600);
            Container.Resolve<MessagePipe.IPublisher<N29.HeroesDawn.Application.Features.Meta.MetaChanged>>().Publish(default);
            yield return CaptureSettled("116-profil-connecte");
            PlayerPrefs.DeleteKey("hd.cloud.sync_state");
            Assert.That(File.Exists(Path.Combine(Folder, "116-profil-connecte.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit les captures du plateau agrandi : portée avec cases cachées, fiche d'une destination (chemin illuminé, dézoom), fiche d'une case sous le brouillard — parce que la prévisualisation du déplacement se relit à l'œil")]
        public IEnumerator CaptureBoardMove()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);

            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 6, false);
            session.RunZone = zone;
            session.Meta.tutorial.done = true; // pas de tutoriel : la capture joue elle-même une compétence
            session.Meta.tutorial.hintsSeen.AddRange(new[] { "auto", "defeat", "fatigue", "timeline", "zone_done" }); // ni d astuce qui bloque le combat
            var run = session.Run!;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            yield return CaptureSettled("03-plateau-portee");

            var map = Object.FindAnyObjectByType<BoardMapView>()!;
            var far = run.Destinations.OrderByDescending(d => d.Value.Cost).First().Key;
            map.NodeButton(far).onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("03b-plateau-fiche");
            Assert.That(Object.FindAnyObjectByType<BoardView>()!.PreviewVisible, Is.True);

            Object.FindAnyObjectByType<BoardView>()!.PreviewCancelButton.onClick.Invoke();
            var hiddenDest = run.Destinations.Keys.First(id => !run.Fog.IsRevealed(id));
            map.NodeButton(hiddenDest).onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("03c-plateau-brouillard");
            Assert.That(File.Exists(Path.Combine(Folder, "03c-plateau-brouillard.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit la capture de la fenêtre d'activité (pêche du port de nuit) — parce que la mise en page du framework d'activités se relit à l'œil")]
        public IEnumerator CaptureActivityWindow()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            var from = new BoardGraph(zone).Neighbors(new N29.HeroesDawn.Core.CaseId("n15")).First();
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, from.Value);
            session.RunZone = zone;
            session.Meta.tutorial.done = true; // pas de tutoriel : la capture joue elle-même une compétence
            session.Meta.tutorial.hintsSeen.AddRange(new[] { "auto", "defeat", "fatigue", "timeline", "zone_done" }); // ni d astuce qui bloque le combat
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n15"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.ActivityPopupView>() != null);
            yield return CaptureSettled("06-activite-peche");
            Assert.That(File.Exists(Path.Combine(Folder, "06-activite-peche.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Description("Produit une capture de la connexion, du QG, du plateau et du combat — parce que le passage à TextMeshPro change le rendu des polices et doit être relu à l'œil avant toute conversion (spec §11)")]
        public IEnumerator CaptureEveryScreen()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            yield return Capture("01-connexion");

            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            yield return Capture("02-qg");

            Object.FindAnyObjectByType<HubShellView>()!.TabButton(HubPage.Recruit).Button.onClick.Invoke();
            for (int i = 0; i < 10; i++) yield return null;
            yield return Capture("02b-boutique");

            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            session.Meta.tutorial.done = true; // pas de tutoriel : la capture joue elle-même une compétence
            session.Meta.tutorial.hintsSeen.AddRange(new[] { "auto", "defeat", "fatigue", "timeline", "zone_done" }); // ni d astuce qui bloque le combat
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            yield return Capture("03-plateau");

            // Écran « zone sécurisée » ouvert directement (la victoire du test ne valide pas la zone).
            ActivityGamesPlayTests.CasePresenter().ShowZoneCompleteAsync(new N29.HeroesDawn.Application.Features.Zone.ZoneRewards(1500, 40, true), default).Forget();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.ZoneCompletePopupView>() != null, 300);
            for (int i = 0; i < 30; i++) yield return null;
            yield return Capture("05b-zone-securisee");
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.ZoneCompletePopupView>()!.ToHqButton.onClick.Invoke();
            for (int i = 0; i < 30; i++) yield return null;

            // Fenêtre « Repos » ouverte directement avec la fatigue des agents de l'escouade.
            var restSquad = Container.Resolve<N29.HeroesDawn.Application.Session.ISessionReadModel>().Squad;
            var restRows = new System.Collections.Generic.List<N29.HeroesDawn.Presentation.Combat.FatigueRowData>();
            int fat = 30;
            foreach (var m in restSquad)
            {
                restRows.Add(new N29.HeroesDawn.Presentation.Combat.FatigueRowData
                {
                    Name = m.Def.Id.Value, Before01 = fat / 100f, After01 = (fat - 20) / 100f, After = fat - 20, Label = fat + " % → " + (fat - 20) + " %", Delta = "",
                });
                fat += 15;
            }
            var restView = Container.Resolve<N29.HeroesDawn.Presentation.Mvp.IModalService>();
            var restPopup = restView.OpenAsync<N29.HeroesDawn.Presentation.Popups.IRestPopupView>(default);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.RestPopupView>() != null, 300);
            var restPrefab = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.RestPopupView>()!;
            restPrefab.ChooseAsync(new N29.HeroesDawn.Presentation.Popups.RestPopupData
            {
                Title = "Repos", Body = "Se reposer fait baisser la fatigue de toute l'escouade.", Skip = "Continuer", Take = "Se reposer",
                FatigueTitle = "Fatigue actuelle → après repos", Fatigue = restRows.ToArray(),
            }, default).Forget();
            for (int i = 0; i < 30; i++) yield return null;
            yield return Capture("05c-repos");
            restPrefab.RequestCancel();
            restView.Close(restPrefab);
            for (int i = 0; i < 30; i++) yield return null;

            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !Flow.IsTransitioning, 600);
            yield return new WaitForSecondsRealtime(5f); // le temps qu un allié soit prêt (panneau de compétences)
            yield return Capture("04-combat");

            // Animation de compétence : une compétence jouée, capture en plein vol du projectile.
            var tip = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Tutorial.TutorialOverlayView>();
            if (tip != null) { tip.OkButton.Button.onClick.Invoke(); yield return new WaitForSecondsRealtime(0.5f); } // ferme l astuce de la frise
            var combatView = Object.FindAnyObjectByType<CombatView>()!;
            // Animations de competence (appel direct de la vue sur le combat reel) : feu vers un ennemi, puis soin sur un allie.
            int enemyId = combatView.EnemyCards()[0].UnitId;
            combatView.PlaySkillFx(new SkillFxData { ActorId = 0, TargetIds = new[] { enemyId }, Kind = SkillFxKind.Fire, Title = "Jet de flammes" }, 1.3f);
            yield return new WaitForSecondsRealtime(0.17f);
            yield return Capture("04b-combat-animation");
            yield return new WaitForSecondsRealtime(1.2f);
            combatView.PlaySkillFx(new SkillFxData { ActorId = 2, TargetIds = new[] { 0, 1 }, Kind = SkillFxKind.Heal, Title = "Choeur de guerison" }, 1.3f);
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("04c-combat-animation");

            Container.Resolve<IForceCombatWinUseCase>().ExecuteAsync().Forget();
            for (int i = 0; i < 120; i++) yield return null;
            yield return Capture("05-resultats");
            Assert.That(File.Exists(Path.Combine(Folder, "05-resultats.png")));
        }
    }
}

