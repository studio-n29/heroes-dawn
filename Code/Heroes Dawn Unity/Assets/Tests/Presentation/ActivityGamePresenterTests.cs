using System.Collections.Generic;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Case Mini-jeu : chaque activité ouvre sa fenêtre de jeu (potion, dialogue, survie, pêche, arène) ; les types sans jeu dédié gardent la fenêtre générique.</summary>
    public sealed class ActivityGamePresenterTests
    {
        TestApp _app = null!;
        FakeModalHost _host = null!;
        FakeToasts _toasts = null!;
        FakeCaseResolutions _resolve = null!;
        FakeActivityPopup _generic = null!;
        FakeStagePopup _stage = null!;
        FakeHeroGamePopup _hero = null!;
        FakePotionPopup _potion = null!;
        FakeDialoguePopup _dialogue = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _host = new FakeModalHost();
            _generic = new FakeActivityPopup();
            _stage = new FakeStagePopup();
            _hero = new FakeHeroGamePopup();
            _potion = new FakePotionPopup();
            _dialogue = new FakeDialoguePopup();
            _host.Register<IMiniGamePopupView>(() => new FakeMiniGamePopup());
            _host.Register<IActivityPopupView>(() => _generic);
            _host.Register<IStageGameView>(() => _stage);
            _host.Register<IHeroGamePopupView>(() => _hero);
            _host.Register<IPotionPopupView>(() => _potion);
            _host.Register<IDialoguePopupView>(() => _dialogue);
            _toasts = new FakeToasts();
            _resolve = new FakeCaseResolutions();
        }

        static CaseOutcome Case() => new CaseOutcome(new CaseId("n07"), CaseType.MiniGame, CombatRank.Easy, null);

        CasePopupPresenter Presenter(IPlanActivityUseCase plan)
            => new CasePopupPresenter(new ModalService(_host), _resolve, _resolve, plan, _resolve, _app.Session, _toasts, new LocLocalization());

        void Run(IPlanActivityUseCase plan) => Presenter(plan).RunAsync(Case(), CancellationToken.None).GetAwaiter().GetResult();

        bool Toasted(ToastKind kind, string text) => _toasts.Shown.Any(t => t.Kind == kind && t.Text == text);

        ContentCatalog With(params ActivityDef[] extra)
        {
            var c = _app.Session.Content;
            return new ContentCatalog(c.Balance, c.Members, c.Enemies, c.Synergies, c.Zones, extra.Concat(c.Activities).ToList());
        }

        /// <summary>Type d'activité sans jeu dédié : ouvre la fenêtre générique (extensibilité du framework).</summary>
        sealed class PlaceholderKind : IActivityKind
        {
            public string Id => "test_placeholder";
            public IReadOnlyList<ParameterDef> Schema { get; } = new[] { new ParameterDef("difficulty", 2, 1, 5) };
            public ActivityLevel Generate(ActivityContext c) => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0]);
        }

        [Test, Description("Vérifie qu'un type d'activité sans jeu dédié ouvre la fenêtre générique (titre, décor, réglages, niveau généré) et résout la case avec le score simulé — parce que les activités à venir fonctionnent avant d'avoir leur propre jeu")]
        public void UnknownGame_OpensTheGenericWindow()
        {
            ActivityRegistry.Default.Register(new PlaceholderKind());
            var def = new ActivityDef(new ActivityId("placeholder"), "test_placeholder", "activities.arena.name", "activities.arena.desc", credits: 50);
            Run(new FakePlanActivity(With(def), "placeholder"));
            var data = _generic.Data!.Value;
            Assert.That(data.Title, Is.EqualTo("ARÈNE DE COMBAT"));
            Assert.That(data.Lines, Has.Some.EqualTo("Décor : Port · nuit"));
            Assert.That(data.Lines, Has.Some.Contains("Niveau généré"));
            Assert.That(((ActivityResult)_resolve.Calls.Single()).Success, Is.True);
            Assert.That(Toasted(ToastKind.Reward, Loc.F("mg.success", 50, 0)), Is.True);
        }

        [Test, Description("Vérifie que Rempart ouvre la fenêtre 2D de héros avec sa partie, ses consignes et son titre, et que le score rendu donne base + crédits par projectile — parce que la quantité interceptée est récompensée")]
        public void Deflect_OpensTheHeroWindow_AndPaysProRata()
        {
            _hero.Result = ActivityResult.Won(14);
            Run(new FakePlanActivity(_app.Session.Content, "deflect_docks"));
            var r = _hero.Request!;
            Assert.That(r.Kind, Is.EqualTo(HeroGameKind.Deflect));
            Assert.That(r.Title, Is.EqualTo("REMPART"));
            Assert.That(r.Deflect, Is.Not.Null);
            Assert.That(r.Instructions, Does.Contain("projectiles"));
            Assert.That(Toasted(ToastKind.Reward, Loc.F("mg.success", 100 + 10 * 14, 5)), Is.True);
            Assert.That(_host.Spawned.OfType<FakeActivityPopup>(), Is.Empty);
        }

        [Test, Description("Vérifie qu'un Rempart raté paie quand même au prorata des projectiles interceptés — parce que chaque interception compte même sous l'objectif")]
        public void DeflectFailure_StillPaysProRata()
        {
            _hero.Result = ActivityResult.Lost(6);
            Run(new FakePlanActivity(_app.Session.Content, "deflect_docks"));
            Assert.That(((ActivityResult)_resolve.Calls.Single()).Score, Is.EqualTo(6));
            Assert.That(Toasted(ToastKind.Warning, Loc.T("mg.fail")), Is.True);
            Assert.That(Toasted(ToastKind.Reward, Loc.F("mg.success", 60, 0)), Is.True);
        }

        [Test, Description("Vérifie que la course des toits ouvre la fenêtre 2D de héros avec sa partie de couloirs — parce que c'est le second mini-jeu 2D")]
        public void RooftopRun_OpensTheHeroWindow()
        {
            _hero.Result = ActivityResult.Won(10);
            Run(new FakePlanActivity(_app.Session.Content, "rooftop_rescue"));
            var r = _hero.Request!;
            Assert.That(r.Kind, Is.EqualTo(HeroGameKind.RooftopRun));
            Assert.That(r.Run, Is.Not.Null);
            Assert.That(r.Title, Is.EqualTo("COURSE DES TOITS"));
        }

        [Test, Description("Vérifie que l'arène ouvre la fenêtre 3D avec la bonne partie et qu'elle propose les agents de l'escouade avec leur fabrique de partie — parce que l'arène commence par le choix d'un agent")]
        public void Arena_OpensTheStageWindow()
        {
            _stage.Result = ActivityResult.Won();
            Run(new FakePlanActivity(_app.Session.Content, "arena_training"));
            var arena = _stage.Request!;
            Assert.That(arena.Kind, Is.EqualTo(StageKind.Arena));
            Assert.That(arena.Agents.Count, Is.EqualTo(_app.Session.Squad.Count(m => !m.Exhausted)));
            Assert.That(arena.Agents.All(a => a.Name == a.Name.ToUpperInvariant() && a.Detail.Length > 0));
            var game = arena.ArenaFactory!(0);
            Assert.That(game.Agent.MemberId, Is.EqualTo(_app.Session.Squad.First(m => !m.Exhausted).Id.Value));
            Assert.That(game.WaveCount, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que la potion présente le patient et son mal, un livre qui décrit chaque ingrédient et chaque étape, les ingrédients colorés par propriété, et qu'une potion ratée annonce le malus du patient — parce que le joueur déduit la recette du livre")]
        public void Potion_ShowsTheBook_AndAnnouncesTheMalusOnFailure()
        {
            _potion.Result = ActivityResult.Lost();
            var plan = new FakePlanActivity(_app.Session.Content, "potion_field");
            Run(plan);
            var loc = new LocLocalization();
            var patient = loc.MemberName(_app.Session.Content.Member(plan.Plans[0].Patient!.Value));
            var r = _potion.Request!;
            Assert.That(r.Intro, Does.Contain(patient));
            Assert.That(r.IngredientNames.Count, Is.EqualTo(6));
            Assert.That(r.IngredientColors.Count, Is.EqualTo(6));
            Assert.That(r.Book, Does.Contain("Étape 1").And.Contain("Étape 3"));
            Assert.That(r.Game.StepsNeeded, Is.EqualTo(3));
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Warning && t.Text.Contains(patient) && t.Text.Contains("-15 %")));
        }

        [Test, Description("Vérifie que le dialogue ouvre sa fenêtre avec le texte rédigé de chaque réplique et réponse, et qu'un échec annonce le combat enchaîné — parce que l'échec du dialogue lance une activité liée")]
        public void Dialogue_ShowsTheLines_AndAnnouncesTheCombatOnFailure()
        {
            _dialogue.Result = ActivityResult.Lost();
            Run(new FakePlanActivity(_app.Session.Content, "dialogue_dockers"));
            var r = _dialogue.Request!;
            var script = r.Game.Script;
            Assert.That(r.Text(script.Lines[0].PromptKey), Does.Contain("contremaître"));
            foreach (var line in script.Lines)
                foreach (var c in line.Choices)
                    Assert.That(r.Text(c.TextKey), Does.StartWith("«"), c.TextKey);
            Assert.That(Toasted(ToastKind.Warning, Loc.T("activity.followup_combat")), Is.True);
        }

        [Test, Description("Vérifie qu'un échec qui enchaîne une autre activité la fait jouer ensuite sur la même case — parce que les activités peuvent s'enchaîner")]
        public void Failure_ChainsTheNextActivity()
        {
            var chain = new ActivityDef(new ActivityId("chain_a"), ActivityKinds.Deflect, "activities.deflect.name", "activities.deflect.desc",
                                        failureFollowUp: FollowUpDef.ThenActivity(new ActivityId("deflect_docks")));
            var plan = new FakePlanActivity(With(chain), "chain_a", "deflect_docks");
            _resolve.ActivityEvents = r => new BoardEvent[] { new ActivityResolvedEvent(ActivityRules.Resolve(plan.Plans.Last(), r)) };
            _hero.Result = ActivityResult.Lost();
            Run(plan);
            Assert.That(plan.Plans.Select(p => p.Def.Id.Value), Is.EqualTo(new[] { "chain_a", "deflect_docks" }));
            Assert.That(_resolve.Calls.Count, Is.EqualTo(2));
            Assert.That(Toasted(ToastKind.Info, Loc.T("activity.followup_activity")), Is.True);
        }
    }
}
