#nullable enable
using System.Linq;
using System.Collections;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Première connexion avec les vrais prefabs : cinématique, pseudo, campagne, puis le QG.</summary>
    public sealed class IntroPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Timeout(300000), Description("Vérifie qu'un nouveau joueur passe par la cinématique (passée d'un clic), ne peut pas choisir une campagne en cours de création, choisit New York puis arrive au QG avec son pseudo — parce que c'est le parcours de première connexion")]
        public IEnumerator FirstConnection_GoesThroughIntro_ThenTheHq()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway(), withIntro: true);
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<IntroPopupView>() != null);
            var intro = Object.FindAnyObjectByType<IntroPopupView>()!;
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Cinematic);
            intro.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign);

            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign);

            Assert.That(intro.Cards.Count, Is.EqualTo(2), "Agent D et Agent M");
            Assert.That(intro.CampaignConfirmButton.interactable, Is.False, "aucun personnage choisi");
            intro.SelectCampaign("agent_m");
            Assert.That(intro.CampaignConfirmButton.interactable, Is.False, "le parcours d Agent M est en cours de création");
            intro.SelectCampaign("agent_d");
            Assert.That(intro.CampaignConfirmButton.interactable, Is.True);
            intro.CampaignConfirmButton.onClick.Invoke();

            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            var session = GameBootstrap.Instance!.Scope.Container.Resolve<GameSession>();
            Assert.That(session.Account!.DisplayName, Is.EqualTo("Agent D"));
            Assert.That(session.Meta.onboarding.campaign, Is.EqualTo("new_york"));
            Assert.That(session.Meta.onboarding.hero, Is.EqualTo("agent_d"));
            Assert.That(session.Roster.Select(m => m.Id.Value), Is.EqualTo(new[] { "darius_brooks" }));
            Assert.That(Object.FindAnyObjectByType<IntroPopupView>(), Is.Null, "la popup est fermée");
        }

        [UnityTest, Timeout(300000), Description("Vérifie que la vidéo affiche une première image avant la connexion et que Passer ouvre le menu")]
        public IEnumerator Launch_PlaysBeforeLogin_AndSkipOpensLogin()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway(), skipLaunch: false);
            var intro = Object.FindAnyObjectByType<IntroPopupView>();
            Assert.That(intro, Is.Not.Null);
            Assert.That(intro!.Current, Is.EqualTo(IntroPopupView.Stage.Cinematic));
            Assert.That(PlayModeHarness.Visible<LoginView>(), Is.False);
            Assert.That(intro.Player.GetComponent<AudioSource>(), Is.Null, "aucune source de voix off");
            var audio = GameBootstrap.Instance!.Scope.Container.Resolve<N29.HeroesDawn.Presentation.Audio.IAudioService>();
            Assert.That(audio.CurrentMusic, Is.EqualTo(N29.HeroesDawn.Presentation.Audio.MusicId.Cinematic));
            var video = intro.Player.GetComponent<UnityEngine.Video.VideoPlayer>();
            float deadline = Time.realtimeSinceStartup + 45f;
            while (video.frame < 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(video.frame, Is.GreaterThanOrEqualTo(0), "une image vidéo doit être décodée avant de quitter le noir");
            Assert.That(video.targetTexture.IsCreated(), Is.True);
            float duration = CinematicVideoSequence.ShotDurationForVideo(video.length, CinematicVideoSequence.Steps[0].Seconds);
            Assert.That(video.playbackSpeed, Is.EqualTo(CinematicVideoSequence.PlaybackSpeedForDuration(video.length, duration)).Within(0.001f), "la vidéo entière doit couvrir la durée du plan");
            yield return new WaitForSecondsRealtime(1f);
            var fader = intro.Player.transform.Find("Fondu").GetComponent<UnityEngine.UI.Image>();
            Assert.That(fader.color.a, Is.LessThan(0.05f), "le voile noir doit révéler la vidéo");
            Assert.That(intro.Player.transform.Find("SousTitres").GetComponent<TMPro.TMP_Text>().text,
                Is.EqualTo(CinematicVideoSequence.Steps[0].Caption));
            intro.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<LoginView>());
            Assert.That(PlayModeHarness.Visible<LoginView>(), Is.True);
            Assert.That(Object.FindAnyObjectByType<IntroPopupView>(), Is.Null);
        }
        [Test, Description("Vérifie que le compte local accepte l'inscription puis la connexion, refuse un mot de passe faux, un e-mail déjà pris et des saisies invalides — parce qu'il joue le rôle de Supabase Auth en attendant le vrai serveur")]
        public void LocalAccountGateway_SignUpAndSignIn()
        {
            var gw = new LocalAccountGateway(0);
            var created = gw.SignUpAsync("maya@example.com", "motdepasse1", "Maya", default).GetAwaiter().GetResult();
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(created.Value.DisplayName, Is.EqualTo("Maya"));
            Assert.That(gw.SignUpAsync(" MAYA@example.com ", "motdepasse1", "Autre", default).GetAwaiter().GetResult().IsFailure, Is.True, "e-mail déjà pris");
            Assert.That(gw.SignUpAsync("pasunmail", "motdepasse1", "X", default).GetAwaiter().GetResult().IsFailure, Is.True);
            Assert.That(gw.SignUpAsync("b@example.com", "court", "X", default).GetAwaiter().GetResult().IsFailure, Is.True);
            var ok = gw.SignInAsync("MAYA@example.com", "motdepasse1", default).GetAwaiter().GetResult();
            Assert.That(ok.IsSuccess, Is.True);
            Assert.That(ok.Value.PlayerId, Is.EqualTo(created.Value.PlayerId));
            Assert.That(gw.SignInAsync("maya@example.com", "faux", default).GetAwaiter().GetResult().IsFailure, Is.True);
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(600000), Description("Produit une capture de chacun des 46 plans de la cinématique (à 2,4 s dans le plan, ou à l instant demandé) puis du choix du personnage joueur — parce que les cadrages, les lumières et les scènes dessinées par code se relisent à l œil")]
        public IEnumerator CaptureTheFirstConnection()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway(), withIntro: true, skipLaunch: false);

            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<IntroPopupView>() != null);
            var intro = Object.FindAnyObjectByType<IntroPopupView>()!;
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Cinematic);
            var player = intro.Player;
            yield return PlayModeHarness.WaitFor(() => player.CurrentShot >= 0, 600);
            var steps = CinematicVideoSequence.Steps;
            for (int shot = 0; shot < steps.Count; shot++)
            {
                // Avance de pas en pas par des touchers, puis laisse vivre le pas 2,4 s avant de le photographier.
                for (int guard = 0; player.CurrentShot < shot && guard < 400; guard++) { player.SkipShot(); yield return null; }
                yield return new WaitForSecondsRealtime(2.4f);
                yield return ScreenCaptureTests.Capture("cin-" + steps[shot].Id);
            }
            intro.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<LoginView>());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<IntroPopupView>() != null);
            intro = Object.FindAnyObjectByType<IntroPopupView>()!;
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign, 1200);
            intro.SelectCampaign("agent_d");
            yield return new WaitForSecondsRealtime(1f);
            yield return ScreenCaptureTests.Capture("cin-choix");
        }
    }
}
