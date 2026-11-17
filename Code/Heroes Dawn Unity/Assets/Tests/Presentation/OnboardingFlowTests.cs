using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>
    /// Première connexion : Rayner en Full Art présente The Dawn et demande la fiche d'identification, le joueur la remplit,
    /// Rayner conclut, puis le joueur choisit sa ville de départ (New York avec l'agent D, Moscou avec l'agent M).
    /// </summary>
    public sealed class OnboardingFlowTests
    {
        TestApp _app = null!;
        FakeModalHost _host = null!;
        OnboardingGuide _service = null!;
        OnboardingFlow _flow = null!;
        readonly ILocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _host = new FakeModalHost();
            _host.Register<IIntroPopupView>(() => new FakeIntroPopup());
            _service = new OnboardingGuide(_app.MetaCtx);
            _flow = new OnboardingFlow(_service, new ModalService(_host), _loc);
        }

        FakeIntroPopup Popup => (FakeIntroPopup)_host.Spawned.Single();

        static AgentForm Form(string power = "electrogenese") => new AgentForm("Rivera", "Maya", 27, "avatar_2", power);

        [Test, Description("Vérifie que la première connexion commence par Rayner en Full Art, qui parle plusieurs fois et finit par demander la fiche — parce que le joueur doit être accueilli avant de remplir quoi que ce soit")]
        public void Onboarding_StartsWithRaynerFullArt_EndingOnTheQuestion()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Assert.That(Popup.Steps, Is.EqualTo(new[] { "rayner" }));
            var scene = Popup.RaynerScenes[0];
            Assert.That(scene.PortraitId, Is.EqualTo("rayner"));
            Assert.That(scene.Lines.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(scene.Lines.Last(), Is.EqualTo(_loc.T("onboarding.rayner.ask")));
        }

        [Test, Description("Vérifie que la fiche est présentée après l'accueil, avec la galerie d'avatars, les bornes et les quatre familles de pouvoirs — parce que le formulaire doit refléter les règles du domaine")]
        public void Form_FollowsTheWelcome_WithTheGalleryTheLimitsAndThePowers()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishRayner();
            Assert.That(Popup.Steps, Is.EqualTo(new[] { "rayner", "form" }));
            var form = Popup.Forms[0];
            Assert.That(form.Avatars, Is.EqualTo(PlayerAvatars.Gallery));
            Assert.That(form.MaxNameLength, Is.EqualTo(PlayerTag.MaxNameLength));
            Assert.That(form.MinAge, Is.EqualTo(AgentForm.MinAge));
            Assert.That(form.MaxAge, Is.EqualTo(AgentForm.MaxAge));
            Assert.That(form.Powers.Select(c => c.FamilyId), Is.EqualTo(HeroPowers.Families.Select(f => f.Id)));
            Assert.That(form.Powers.Sum(c => c.Powers.Count), Is.EqualTo(HeroPowers.All.Count));
            Assert.That(form.Error, Is.Empty);
        }

        [Test, Description("Vérifie que Rayner reprend la parole après la fiche, en citant le prénom, puis que le choix de ville arrive — parce que la séquence suit l'ordre voulu : fiche, conclusion, départ")]
        public void AfterTheForm_RaynerSpeaksAgain_ThenTheStartChoice()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishRayner();
            Popup.Submit(Form());
            Assert.That(Popup.Steps, Is.EqualTo(new[] { "rayner", "form", "rayner" }));
            var after = Popup.RaynerScenes[1];
            Assert.That(after.Lines.Count, Is.InRange(1, 3), "elle reprend « rapidement » la parole");
            Assert.That(string.Join(" ", after.Lines), Does.Contain("Maya"));
            Popup.FinishRayner();
            Assert.That(Popup.Steps, Is.EqualTo(new[] { "rayner", "form", "rayner", "start" }));
        }

        [Test, Description("Vérifie que le choix de ville propose New York avec l'agent D et Moscou avec l'agent M, Moscou restant grisée tant que son parcours est fermé — parce qu'on ne démarre que dans une ville jouable")]
        public void StartChoice_OffersNewYorkAndMoscow_WithAvailability()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishRayner(); Popup.Submit(Form()); Popup.FinishRayner();
            var options = Popup.Start!.Value.Options;
            Assert.That(options.Select(o => o.RouteId), Is.EqualTo(new[] { StoryRoutes.AgentD, StoryRoutes.AgentM }));
            Assert.That(options[0].Available, Is.True);
            Assert.That(options[1].Available, Is.EqualTo(StoryRoutes.M.Available));
            Assert.That(options[0].Agent, Is.EqualTo(_loc.T("campaign.agent_d.name")));
            Assert.That(options[1].Agent, Is.EqualTo(_loc.T("campaign.agent_m.name")));
        }

        [Test, Description("Vérifie que le parcours complet enregistre la fiche dans le profil, renomme le compte, démarre New York avec l'agent D et ferme la popup — parce que les données du formulaire doivent survivre à la première connexion")]
        public void FullRun_SavesTheFormAndStartsInNewYork()
        {
            var run = _flow.RunIfNeededAsync(default);
            var popup = Popup;
            popup.FinishRayner(); popup.Submit(Form()); popup.FinishRayner(); popup.PickStart(StoryRoutes.AgentD);
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            var s = _app.Session.Meta.onboarding;
            Assert.That(s.done, Is.True);
            Assert.That(s.lastName, Is.EqualTo("Rivera"));
            Assert.That(s.firstName, Is.EqualTo("Maya"));
            Assert.That(s.age, Is.EqualTo(27));
            Assert.That(s.avatar, Is.EqualTo("avatar_2"));
            Assert.That(s.power, Is.EqualTo("electrogenese"));
            Assert.That(s.campaign, Is.EqualTo("new_york"));
            Assert.That(s.hero, Is.EqualTo("agent_d"));
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Maya"));
            Assert.That(_host.Despawned, Does.Contain(popup));
            Assert.That(_flow.Needed, Is.False);
        }

        [Test, Description("Vérifie que rien n'est enregistré tant que le joueur n'a pas choisi sa ville — parce qu'un joueur qui quitte à mi-parcours doit revoir l'accueil")]
        public void NothingIsSaved_BeforeTheStartChoice()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishRayner(); Popup.Submit(Form()); Popup.FinishRayner();
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
            Assert.That(_flow.Needed, Is.True);
        }

        [Test, Description("Vérifie qu'une fiche refusée par le domaine est représentée avec un message d'erreur — parce que le joueur ne doit jamais rester bloqué sans explication")]
        public void RefusedForm_IsAskedAgain_WithAnError()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishRayner(); Popup.Submit(Form()); Popup.FinishRayner();
            // Un parcours indisponible est refusé : la fiche revient avec l'erreur.
            Popup.PickStart("atlantis");
            Assert.That(Popup.Forms.Count, Is.EqualTo(2));
            Assert.That(Popup.Forms[1].Error, Is.EqualTo(_loc.T("onboarding.campaign.unavailable")));
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
        }

        [Test, Description("Vérifie qu'un profil qui a déjà validé une zone ne revoit pas la première connexion — parce que les joueurs existants ne repassent pas par l'introduction")]
        public void ExistingPlayer_SkipsTheIntro()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var run = _flow.RunIfNeededAsync(default);
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_host.Spawned, Is.Empty);
        }

        [Test, Description("Vérifie que chaque texte de la première connexion existe en français et en anglais — parce qu'une clé manquante s'afficherait telle quelle à l'écran")]
        public void EveryOnboardingTextExists_InBothLanguages()
        {
            foreach (var id in Loc.CinematicVoiceIds) Assert.That(Loc.Has("cinematic.vo." + id), Is.True, "cinematic.vo." + id);
            var keys = new System.Collections.Generic.List<string>
            {
                "intro.skip", "onboarding.campaign.unavailable", "onboarding.name.invalid", "onboarding.age.invalid", "onboarding.power.invalid",
                "onboarding.rayner.speaker", "onboarding.rayner.continue", "onboarding.rayner.ask",
                "onboarding.rayner.intro.1", "onboarding.rayner.intro.2", "onboarding.rayner.intro.3", "onboarding.rayner.intro.4",
                "onboarding.rayner.after.1", "onboarding.rayner.after.2",
                "onboarding.form.title", "onboarding.form.last_name", "onboarding.form.first_name", "onboarding.form.age", "onboarding.form.avatar",
                "onboarding.form.power", "onboarding.form.power_placeholder", "onboarding.form.confirm",
                "onboarding.start.title", "onboarding.start.soon",
                "onboarding.start.agent_d.city", "onboarding.start.agent_d.desc", "onboarding.start.agent_m.city", "onboarding.start.agent_m.desc",
            };
            foreach (var f in HeroPowers.Families)
            {
                keys.Add(f.NameKey);
                foreach (var p in f.Powers) keys.Add(p.NameKey);
            }
            foreach (var key in keys)
            {
                Assert.That(Loc.Has(key), Is.True, "FR " + key);
                Assert.That(Loc.HasEnglish(key), Is.True, "EN " + key);
            }
            foreach (var c in Campaigns.All) { Assert.That(Loc.Has(c.NameKey), Is.True, c.NameKey); Assert.That(Loc.Has(c.DescKey), Is.True, c.DescKey); }
        }
    }
}
