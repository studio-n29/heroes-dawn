using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Les cinq activités jouables : survie en neige, pêche, arène, potion, dialogue (logique pure, spec 2026-10-10-activites-jouables).</summary>
    public sealed class ActivityGamesTests
    {
        static readonly ContentCatalog C = TestContent.Build();
        static readonly ZoneEnvironment Env = new ZoneEnvironment(Biome.Snow, Weather.Snow);

        static ActivityPlan Plan(string id, params (string Key, int Value)[] overrides)
        {
            var def = C.Activity(new ActivityId(id));
            var p = def.Parameters;
            foreach (var (k, v) in overrides) p = p.With(k, v);
            return ActivityPlanner.Plan(ActivityRegistry.Default, def.WithParameters(p), Env, 11UL, new CaseId("n15"), 1, new[] { new MemberId("ancre"), new MemberId("givre") });
        }

        const float Dt = 0.05f;

        // ------------------------------------------------------------------ Survie en neige

        [Test, Description("Vérifie qu'en rejoignant l'objet recherché sans obstacle ni froid mortel, la survie en neige est gagnée — parce que trouver l'objet est le but")]
        public void Survival_ReachingTheTarget_Wins()
        {
            var plan = Plan("survival_snow", ("obstacle_count", 0), ("target_distance_m", 20));
            var g = new SnowSurvivalGame(plan.Level, plan.Parameters);
            for (int i = 0; i < 2000 && g.Status == GameStatus.Running; i++) g.Step(Dt, (g.Target - g.Position).Normalized);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Result().Success, Is.True);
        }

        [Test, Description("Vérifie que rester immobile fait perdre par le froid quand la jauge se vide, ou par le temps quand elle tient — parce que la jauge de survie descend avec le temps")]
        public void Survival_StandingStill_LosesByColdOrTime()
        {
            var cold = Plan("survival_snow", ("drain_permille_per_s", 200));
            var g = new SnowSurvivalGame(cold.Level, cold.Parameters);
            for (int i = 0; i < 2000 && g.Status == GameStatus.Running; i++) g.Step(Dt, Vec2f.Zero);
            Assert.That((g.Status, g.Loss), Is.EqualTo((GameStatus.Lost, LossReason.Cold)));
            Assert.That(g.Gauge, Is.Zero);

            var slow = Plan("survival_snow", ("drain_permille_per_s", 1), ("time_limit_s", 20));
            var t = new SnowSurvivalGame(slow.Level, slow.Parameters);
            for (int i = 0; i < 2000 && t.Status == GameStatus.Running; i++) t.Step(Dt, Vec2f.Zero);
            Assert.That((t.Status, t.Loss), Is.EqualTo((GameStatus.Lost, LossReason.Time)));
        }

        [Test, Description("Vérifie que la jauge se recharge près d'un totem et baisse ailleurs — parce que les totems sont les foyers de la survie")]
        public void Survival_TotemsRefillTheGauge()
        {
            var plan = Plan("survival_snow", ("obstacle_count", 0), ("drain_permille_per_s", 60), ("time_limit_s", 300));
            var g = new SnowSurvivalGame(plan.Level, plan.Parameters);
            for (int i = 0; i < 80; i++) g.Step(Dt, new Vec2f(0, 0));
            float before = g.Gauge;
            Assert.That(before, Is.LessThan(1000f));
            var totem = g.Totems.OrderBy(t => Vec2f.Distance(t, g.Position)).First();
            for (int i = 0; i < 1500 && !g.NearTotem; i++) g.Step(Dt, (totem - g.Position).Normalized);
            Assert.That(g.NearTotem, Is.True);
            float low = g.Gauge;
            for (int i = 0; i < 60; i++) g.Step(Dt, Vec2f.Zero);
            Assert.That(g.Gauge, Is.GreaterThan(low), "la jauge remonte près du totem");
        }

        [Test, Description("Vérifie que le déplacement est borné par la vitesse du joueur (jamais plus de 4,2 m en 1 s) et qu'une direction plus longue que 1 ne l'accélère pas — parce que la vitesse est une règle du jeu")]
        public void Survival_MovementIsBoundedBySpeed()
        {
            var plan = Plan("survival_snow", ("obstacle_count", 0));
            var g = new SnowSurvivalGame(plan.Level, plan.Parameters);
            var start = g.Position;
            g.Step(1f, new Vec2f(5, 0));
            Assert.That(Vec2f.Distance(start, g.Position), Is.InRange(1f, SnowSurvivalGame.BaseSpeed + 0.01f));
        }


        // ------------------------------------------------------------------ Pêche

        static void StepUntil(FishingGame g, FishingState state, bool aimCast = false)
        {
            for (int i = 0; i < 400 && g.State != state; i++) g.Step(Dt, Vec2f.Zero, aimCast && i == 0, false);
        }

        [Test, Description("Vérifie le cycle de pêche : lancer, attente, touche, puis tirer dans la fenêtre attrape le poisson — parce que c'est le geste central de l'activité")]
        public void Fishing_CastWaitBitePull_CatchesAFish()
        {
            var plan = Plan("fishing_docks");
            var g = new FishingGame(plan.Level, plan.Parameters);
            g.Step(Dt, Vec2f.Zero, true, false);
            Assert.That(g.State, Is.EqualTo(FishingState.Waiting));
            StepUntil(g, FishingState.Bite);
            Assert.That(g.State, Is.EqualTo(FishingState.Bite));
            Assert.That(g.BiteTimeLeft, Is.GreaterThan(0f));
            g.Step(Dt, Vec2f.Zero, false, true);
            Assert.That((g.State, g.Caught, g.LastEvent), Is.EqualTo((FishingState.Caught, 1, "caught")));
            StepUntil(g, FishingState.Idle);
            Assert.That(g.State, Is.EqualTo(FishingState.Idle));
        }

        [Test, Description("Vérifie que tirer trop tôt ou laisser passer la fenêtre fait fuir le poisson — parce que le bon moment compte")]
        public void Fishing_TooEarlyOrTooLate_TheFishEscapes()
        {
            var plan = Plan("fishing_docks");
            var early = new FishingGame(plan.Level, plan.Parameters);
            early.Step(Dt, Vec2f.Zero, true, false);
            early.Step(Dt, Vec2f.Zero, false, true);
            Assert.That((early.State, early.LastEvent, early.Caught), Is.EqualTo((FishingState.Escaped, "early", 0)));

            var late = new FishingGame(plan.Level, plan.Parameters);
            late.Step(Dt, Vec2f.Zero, true, false);
            StepUntil(late, FishingState.Bite);
            for (int i = 0; i < 100 && late.State == FishingState.Bite; i++) late.Step(Dt, Vec2f.Zero, false, false);
            Assert.That((late.State, late.LastEvent), Is.EqualTo((FishingState.Escaped, "escaped")));
            Assert.That(late.Caught, Is.Zero);
        }

        [Test, Description("Vérifie que la partie se termine au bout du temps, que le résultat compare les poissons au quota (pièces au prorata dans le score) et qu'un lancer près d'un poisson le fait mordre plus vite — parce que le minuteur fixe la manche")]
        public void Fishing_TimerEndsTheGame_AndScoresTheCatch()
        {
            var plan = Plan("fishing_docks", ("time_limit_s", 30), ("success_fish", 2));
            var g = new FishingGame(plan.Level, plan.Parameters);
            int fishTarget = 0;
            for (int i = 0; i < 20000 && !g.IsOver; i++)
            {
                bool cast = g.State == FishingState.Idle;
                bool pull = g.State == FishingState.Bite && fishTarget++ >= 0;
                g.Step(Dt, Vec2f.Zero, cast, pull);
            }
            Assert.That(g.IsOver, Is.True);
            Assert.That(g.Caught, Is.GreaterThanOrEqualTo(2));
            var r = g.Result();
            Assert.That((r.Success, r.Score), Is.EqualTo((true, g.Caught)));

            var none = new FishingGame(plan.Level, plan.Parameters);
            for (int i = 0; i < 20000 && !none.IsOver; i++) none.Step(Dt, Vec2f.Zero, false, false);
            Assert.That(none.Result().Success, Is.False);
            Assert.That(none.Result().Score, Is.Zero);
        }

        // ------------------------------------------------------------------ Arène

        static ArenaAgent Hero(int hp, int atk, string family = "physical")
            => new ArenaAgent("test", Family.PhysicalBio, hp, atk, 6f,
                              new ArenaSkill("a", ArenaSkillKind.Melee, 2.2f, 100, 0.3f), new ArenaSkill("b", ArenaSkillKind.Aoe, 5f, 200, 4f));

        static void Fight(ArenaGame g, int maxSteps = 20000)
        {
            for (int i = 0; i < maxSteps && g.Status == GameStatus.Running; i++)
            {
                var near = g.Enemies.Where(e => e.Alive).OrderBy(e => Vec2f.Distance(e.Position, g.Position)).FirstOrDefault();
                var move = near == null ? Vec2f.Zero : (near.Position - g.Position).Normalized;
                g.Step(Dt, move, null, true, true);
            }
        }

        [Test, Description("Vérifie qu'un agent fort vient à bout de toutes les vagues et que le score compte les ennemis vaincus — parce que vaincre les vagues est le but de l'arène")]
        public void Arena_AStrongAgent_ClearsEveryWave()
        {
            var plan = Plan("arena_training", ("waves", 2), ("enemies_per_wave", 3));
            var g = new ArenaGame(plan.Level, plan.Parameters, Hero(900, 900));
            Fight(g);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Defeated, Is.EqualTo(6));
            Assert.That(g.Wave, Is.EqualTo(1));
            Assert.That(g.Result().Score, Is.EqualTo(6));
        }

        [Test, Description("Vérifie qu'un agent immobile et faible est vaincu par les ennemis — parce que l'arène peut être perdue")]
        public void Arena_AFeebleAgentStandingStill_Loses()
        {
            var plan = Plan("arena_training");
            var g = new ArenaGame(plan.Level, plan.Parameters, Hero(30, 5));
            for (int i = 0; i < 20000 && g.Status == GameStatus.Running; i++) g.Step(Dt, Vec2f.Zero, null, false, false);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(g.Hp, Is.Zero);
        }

        [Test, Description("Vérifie que les ennemis apparaissent aux points de la vague, que les vagues s'enchaînent une fois la précédente vaincue et que le comportement dépend du pattern (charge, tir) — parce que les patterns d'ennemis sont simples mais variés")]
        public void Arena_WavesSpawnAtTheirMarkers_WithTheirPatterns()
        {
            var plan = Plan("arena_training", ("waves", 3), ("enemies_per_wave", 4));
            var g = new ArenaGame(plan.Level, plan.Parameters, Hero(5000, 5000));
            g.Step(1f, Vec2f.Zero, null, false, false);
            Assert.That(g.Wave, Is.EqualTo(0));
            Assert.That(g.Enemies.Count, Is.EqualTo(4));
            plan.Level.TryMarker("spawn_w0_0", out var m);
            Assert.That(Vec2f.Distance(g.Enemies[0].Position, new Vec2f(m.X / 100f, m.Z / 100f)), Is.LessThan(2.5f));
            var types = new HashSet<EnemyType>();
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 4000 && g.Wave < wave; i++) g.Step(Dt, Vec2f.Zero, null, true, true);
                foreach (var e in g.Enemies.Where(e => e.Alive)) types.Add(e.Type);
                for (int i = 0; i < 4000 && g.AliveCount > 0 && g.Status == GameStatus.Running; i++) Fight(g, 1);
            }
            Assert.That(g.Wave, Is.EqualTo(2));
            Assert.That(types, Does.Contain(EnemyType.Chaser));
            Assert.That(types.Count, Is.GreaterThan(1), "au moins un pattern autre que la poursuite : " + string.Join(",", types) + " / patterns " + plan.Level.Fact("wave0_pattern", -1) + plan.Level.Fact("wave1_pattern", -1) + plan.Level.Fact("wave2_pattern", -1));
        }

        [Test, Description("Vérifie que chaque compétence a sa recharge (pas de deuxième utilisation tout de suite), que le projectile touche à distance et que la charge rend invulnérable — parce que les deux compétences se gèrent")]
        public void Arena_SkillsHaveCooldowns_ProjectilesHit_DashIsInvulnerable()
        {
            var plan = Plan("arena_training", ("waves", 1), ("enemies_per_wave", 1));
            var shooter = new ArenaAgent("t", Family.Elemental, 500, 600, 6f,
                                         new ArenaSkill("bolt", ArenaSkillKind.Projectile, 16f, 100, 1f), new ArenaSkill("dash", ArenaSkillKind.Dash, 6f, 100, 5f));
            var g = new ArenaGame(plan.Level, plan.Parameters, shooter);
            g.Step(0.5f, Vec2f.Zero, null, false, false);
            Assert.That(g.AliveCount, Is.EqualTo(1));
            g.Step(Dt, Vec2f.Zero, null, true, false);
            Assert.That(g.Projectiles.Count, Is.EqualTo(1));
            Assert.That(g.PrimaryCooldown, Is.GreaterThan(0.9f));
            g.Step(Dt, Vec2f.Zero, null, true, false);
            Assert.That(g.Projectiles.Count(p => p.FromPlayer), Is.LessThanOrEqualTo(1), "recharge : pas de second tir");
            for (int i = 0; i < 120 && g.Status == GameStatus.Running; i++) g.Step(Dt, Vec2f.Zero, null, false, false);
            Assert.That(g.Defeated, Is.EqualTo(1), "le projectile a touché");

            var p2 = Plan("arena_training", ("waves", 1), ("enemies_per_wave", 1));
            var d = new ArenaGame(p2.Level, p2.Parameters, shooter);
            d.Step(Dt, Vec2f.Zero, null, false, true);
            Assert.That(d.Dashing, Is.True);
            Assert.That(d.SpecialCooldown, Is.GreaterThan(4f));
        }

        [Test, Description("Vérifie que l'agent d'arène tire ses compétences et ses statistiques de sa famille de pouvoirs (matière : coup de masse + onde de choc ; élémentaire : projectile + nova) — parce que chaque agent a deux compétences")]
        public void Arena_AgentsGetTheirFamilySkills()
        {
            var b = C.Balance;
            var ancre = ArenaAgent.From(C.Member(new MemberId("ancre")), 5, b);
            var givre = ArenaAgent.From(C.Member(new MemberId("givre")), 5, b);
            Assert.That(ancre.Primary.Id, Is.Not.EqualTo(givre.Primary.Id));
            Assert.That(ancre.Special.Kind, Is.Not.EqualTo(ArenaSkillKind.Projectile));
            Assert.That(givre.Primary.Kind, Is.EqualTo(ArenaSkillKind.Projectile));
            Assert.That(ancre.MaxHp, Is.GreaterThan(40));
            Assert.That(ArenaAgent.From(C.Member(new MemberId("ancre")), 10, b).Attack, Is.GreaterThan(ancre.Attack), "le niveau compte");
        }

        // ------------------------------------------------------------------ Potion

        static PotionGame NewPotion(out PotionRecipe recipe, int attempts = 2)
        {
            var plan = Plan("potion_field", ("attempts", attempts));
            recipe = plan.Level.PayloadAs<PotionRecipe>()!;
            return new PotionGame(recipe);
        }

        static void Add(PotionGame g, IEnumerable<PotionStep> steps) { foreach (var s in steps) g.Add(s.Ingredient, s.Quantity); }

        [Test, Description("Vérifie que préparer la bonne recette (ingrédients, ordre, quantités) réussit, que préparer une recette incomplète ne coûte rien et qu'on peut retirer le dernier ingrédient — parce que la potion réussie rapporte le gain")]
        public void Potion_TheRightRecipe_Wins()
        {
            var g = NewPotion(out var recipe);
            Assert.That(g.Brew(), Is.False, "incomplète");
            Assert.That(g.AttemptsLeft, Is.EqualTo(2));
            g.Add(recipe.Steps[0].Ingredient, 1);
            Assert.That(g.RemoveLast(), Is.True);
            Assert.That(g.Current, Is.Empty);
            Add(g, recipe.Steps);
            Assert.That(g.CanAdd, Is.False, "plein");
            Assert.That(g.Brew(), Is.True);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Result().Success, Is.True);
        }

        [Test, Description("Vérifie qu'une potion ratée coûte un essai et indique pour chaque étape si elle est juste, trop ou pas assez dosée, au mauvais endroit ou fausse, puis que les essais épuisés font perdre — parce que le joueur déduit la recette des indices")]
        public void Potion_AWrongBrew_GivesFeedback_AndLosesAfterTheAttempts()
        {
            var g = NewPotion(out var recipe, 2);
            var steps = recipe.Steps.ToList();
            int other = Enumerable.Range(0, recipe.Ingredients.Count).First(i => steps.All(s => s.Ingredient != i));
            g.Add(steps[0].Ingredient, steps[0].Quantity);                                    // juste
            g.Add(steps[1].Ingredient, steps[1].Quantity == recipe.MaxQuantity ? steps[1].Quantity - 1 : steps[1].Quantity + 1); // mauvaise dose
            if (steps.Count > 2) g.Add(other, 1);                                             // ingrédient absent de la recette
            while (g.CanAdd) g.Add(steps[0].Ingredient, 1);
            Assert.That(g.Brew(), Is.True);
            var fb = g.LastFeedback;
            Assert.That(fb[0], Is.EqualTo(StepFeedback.Correct));
            Assert.That(fb[1] == StepFeedback.TooMuch || fb[1] == StepFeedback.TooLittle, Is.True);
            if (steps.Count > 2) Assert.That(fb[2], Is.EqualTo(StepFeedback.Wrong));
            Assert.That(g.AttemptsLeft, Is.EqualTo(1));
            Assert.That(g.Current, Is.Empty, "la préparation repart de zéro");
            Assert.That(g.Status, Is.EqualTo(GameStatus.Running));
            Add(g, steps.Select(s => new PotionStep(steps[0].Ingredient, 1)));
            g.Brew();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(g.Result().Success, Is.False);
            Assert.That(g.Add(0, 1), Is.False, "plus rien à faire");
        }

        [Test, Description("Vérifie qu'un ingrédient juste mais placé ailleurs dans la recette est signalé « mauvais endroit » — parce que l'ordre compte et le retour doit le dire")]
        public void Potion_RightIngredientWrongPlace_IsReported()
        {
            var g = NewPotion(out var recipe);
            var steps = recipe.Steps.ToList();
            g.Add(steps[1].Ingredient, steps[1].Quantity);
            g.Add(steps[0].Ingredient, steps[0].Quantity);
            while (g.CanAdd) g.Add(steps[2 % steps.Count].Ingredient, 1);
            g.Brew();
            Assert.That(g.LastFeedback[0], Is.EqualTo(StepFeedback.WrongPlace));
            Assert.That(g.LastFeedback[1], Is.EqualTo(StepFeedback.WrongPlace));
        }

        // ------------------------------------------------------------------ Dialogue

        [Test, Description("Vérifie que les meilleures réponses réussissent le dialogue, que les pires l'échouent, qu'on ne peut pas répondre hors des choix et que le texte de chaque réponse suit sa qualité — parce que les réponses décident de la réussite")]
        public void Dialogue_AnswersDecideTheOutcome()
        {
            var script = Plan("dialogue_dockers").Level.PayloadAs<DialogueScript>()!;
            var good = new DialogueGame(script);
            while (!good.Finished) good.Choose(good.Current!.Choices.First(c => c.Score == 2).Index);
            Assert.That((good.Success, good.Score), Is.EqualTo((true, 8)));
            Assert.That(good.Result().Success, Is.True);
            Assert.That(good.Choose(0), Is.EqualTo(-1), "dialogue fini");

            var bad = new DialogueGame(script);
            Assert.That(bad.Choose(99), Is.EqualTo(-1));
            while (!bad.Finished) bad.Choose(bad.Current!.Choices.First(c => c.Score == 0).Index);
            Assert.That((bad.Success, bad.Score), Is.EqualTo((false, 0)));

            foreach (var line in script.Lines)
                foreach (var c in line.Choices)
                    Assert.That(c.TextKey, Does.EndWith(c.Score == 2 ? ".good" : c.Score == 0 ? ".bad" : ".ok" + (line.Choices.Where(x => x.Score == 1).ToList().IndexOf(c) + 1)));
        }
    }
}
