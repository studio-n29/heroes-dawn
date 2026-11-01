using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Première connexion : cinématique, création du personnage joueur (nom et avatar).</summary>
    public sealed class OnboardingFlowTests
    {
        TestApp _app = null!;
        FakeModalHost _host = null!;
        OnboardingGuide _service = null!;
        OnboardingFlow _flow = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _host = new FakeModalHost();
            _host.Register<IIntroPopupView>(() => new FakeIntroPopup());
            _service = new OnboardingGuide(_app.MetaCtx);
            _flow = new OnboardingFlow(_service, new ModalService(_host), new LocLocalization());
        }

        FakeIntroPopup Popup => (FakeIntroPopup)_host.Spawned.Single();

        [Test, Description("Vérifie que la première connexion commence à la création du personnage, la cinématique étant déjà passée au lancement — parce que le joueur se nomme avant de jouer")]
        public void Onboarding_StartsWithCharacterCreation()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Assert.That(Popup.MainTheme, Is.Null);
            Assert.That(Popup.Character, Is.Not.Null);
        }

        [Test, Description("Vérifie que la création propose la galerie d'avatars et la longueur maximale du nom — parce que le formulaire doit correspondre aux règles du domaine")]
        public void Character_ProposesTheGalleryAndTheNameLimit()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishCinematic();
            Assert.That(Popup.Character!.Value.Avatars, Is.EqualTo(PlayerAvatars.Gallery));
            Assert.That(Popup.Character!.Value.MaxNameLength, Is.EqualTo(PlayerTag.MaxNameLength));
        }

        [Test, Description("Vérifie que le parcours complet garde le nom et l'avatar saisis, enregistre l'état « terminé », renomme le compte et ferme la popup — parce que le nom du joueur est celui de son agent")]
        public void FullRun_SavesTheCharacterAndRenamesTheAccount()
        {
            var run = _flow.RunIfNeededAsync(default);
            var popup = Popup;
            popup.FinishCinematic();
            popup.Choose("Maya", "avatar_2");
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_app.Session.Meta.onboarding.done, Is.True);
            Assert.That(_app.Session.Meta.onboarding.pseudo, Is.EqualTo("Maya"));
            Assert.That(_app.Session.Meta.onboarding.avatar, Is.EqualTo("avatar_2"));
            Assert.That(_app.Session.Meta.onboarding.campaign, Is.EqualTo("new_york"));
            Assert.That(_app.Session.Meta.onboarding.hero, Is.EqualTo("agent_d"));
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Maya"));
            Assert.That(_host.Despawned, Does.Contain(popup));
            Assert.That(_flow.Needed, Is.False);
        }

        [Test, Description("Vérifie qu'un profil qui a déjà validé une zone ne revoit pas la première connexion — parce que les joueurs existants ne repassent pas par l'introduction")]
        public void ExistingPlayer_SkipsTheIntro()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var run = _flow.RunIfNeededAsync(default);
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_host.Spawned, Is.Empty);
        }

        [Test, Description("Vérifie que chaque texte de la première connexion existe — parce qu'une clé manquante s'afficherait telle quelle à l'écran")]
        public void EveryOnboardingTextExists()
        {
            foreach (var id in Loc.CinematicVoiceIds) Assert.That(Loc.Has("cinematic.vo." + id), Is.True, "cinematic.vo." + id);
            foreach (var key in new[] { "intro.skip",
                                        "onboarding.character.title", "onboarding.character.name_hint", "onboarding.character.confirm", "onboarding.campaign.unavailable" })
                Assert.That(Loc.Has(key), Is.True, key);
            foreach (var c in Campaigns.All) { Assert.That(Loc.Has(c.NameKey), Is.True, c.NameKey); Assert.That(Loc.Has(c.DescKey), Is.True, c.DescKey); }
        }
    }
}
