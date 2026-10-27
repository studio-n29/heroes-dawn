using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Framework d'activités : registre, paramètres, niveaux procéduraux, résultats, intégration au plateau (spec 2026-10-07-framework-activites).</summary>
    public sealed class ActivityFrameworkTests
    {
        static readonly ContentCatalog C = TestContent.Build();
        static readonly ActivityRegistry Reg = ActivityRegistry.Default;
        static readonly ZoneEnvironment Night = new ZoneEnvironment(Biome.Harbor, Weather.Night);

        static ActivityDef Def(string id) => C.Activity(new ActivityId(id));

        static ActivityPlan Plan(string id, ZoneEnvironment? env = null, ulong seed = 7, int attempt = 1, params string[] squad)
            => ActivityPlanner.Plan(Reg, Def(id), env ?? Night, seed, new CaseId("n15"), attempt, squad.Select(s => new MemberId(s)).ToList());

        // ------------------------------------------------------------------ Registre et paramètres

        [Test, Description("Vérifie que le registre contient les six types d'activité du prototype et que chaque activité du contenu a un type enregistré — parce qu'une activité sans type ne pourrait pas générer son niveau")]
        public void Registry_HasEveryKind_AndEveryActivityHasAKind()
        {
            var kinds = Reg.Kinds.Select(k => k.Id).OrderBy(x => x).ToArray();
            Assert.That(kinds, Is.EqualTo(new[] { "combat_arena", "crate_pick", "deflect", "dialogue", "dna_analysis", "free_throw", "gears", "infiltration", "potion", "reflex", "rescue_engineering", "rhythm", "rooftop_run", "snow_survival" }));
            foreach (var a in C.Activities) Assert.That(Reg.TryGet(a.Kind, out _), Is.True, a.Id.Value);
        }

        [Test, Description("Vérifie que les paramètres d'une activité sont complétés par les défauts du type et bornés par son schéma — parce que les réglages externes ne doivent jamais produire un niveau impossible")]
        public void Parameters_AreFilledWithDefaults_AndClamped()
        {
            var def = Def("deflect_docks").WithParameters(new ActivityParameters(new Dictionary<string, int> { ["time_limit_s"] = 99999, ["target"] = -5 }));
            var p = Reg.Resolve(def);
            Assert.That(p.Get("time_limit_s"), Is.EqualTo(120), "borné au maximum du schéma");
            Assert.That(p.Get("target"), Is.EqualTo(3), "borné au minimum du schéma");
            Assert.That(p.Get("lives"), Is.EqualTo(3), "défaut du type pour une clé absente");
            foreach (var k in Reg.Get("deflect").Schema) Assert.That(p.Has(k.Key), Is.True, k.Key);
        }

        [Test, Description("Vérifie que les réglages JSON (« activité.paramètre ») s'appliquent aux activités et que le JSON invalide est refusé — parce qu'on doit pouvoir régler une activité sans toucher au code")]
        public void ParamsJson_OverridesAnActivity_AndRejectsGarbage()
        {
            var parsed = ActivityParamsJson.Parse("{ \"deflect_docks.time_limit_s\": 120, \"arena_training.waves\": 5 }");
            Assert.That(parsed.IsSuccess, Is.True);
            var content = TestContent.Build(new BalanceConfig(), parsed.Value);
            Assert.That(Reg.Resolve(content.Activity(new ActivityId("deflect_docks"))).Get("time_limit_s"), Is.EqualTo(120));
            Assert.That(Reg.Resolve(content.Activity(new ActivityId("arena_training"))).Get("waves"), Is.EqualTo(5));
            Assert.That(Reg.Resolve(C.Activity(new ActivityId("arena_training"))).Get("waves"), Is.EqualTo(3), "le contenu d'origine n'est pas modifié");
            Assert.That(ActivityParamsJson.Parse("pas du json").IsFailure, Is.True);
            Assert.That(ActivityParamsJson.Parse("{ \"a.b\": \"x\" }").IsFailure, Is.True);
            Assert.That(ActivityParamsJson.Parse("{}").Value, Is.Empty);
        }

        // ------------------------------------------------------------------ Génération procédurale

        [Test, Description("Vérifie que la même graine et le même environnement redonnent exactement le même niveau, et qu'une autre tentative en donne un autre — parce que le niveau doit être reproductible mais varier d'une partie à l'autre")]
        public void Levels_AreDeterministic_PerSeed()
        {
            foreach (var id in new[] { "survival_snow", "arena_training" })
            {
                var a = Plan(id).Level; var b = Plan(id).Level; var other = Plan(id, attempt: 2).Level;
                Assert.That(a.Primitives.Select(p => (p.X, p.Z, p.Role)), Is.EqualTo(b.Primitives.Select(p => (p.X, p.Z, p.Role))), id);
                Assert.That(a.Primitives.Select(p => (p.X, p.Z)), Is.Not.EqualTo(other.Primitives.Select(p => (p.X, p.Z))), id + " : autre tentative");
            }
        }

        [Test, Description("Vérifie que le biome et la météo changent les couleurs plates du niveau (nuit plus sombre que jour, biome différent = sol différent) — parce que les assets V1 sont des primitives colorées selon l'environnement")]
        public void Environment_ChangesThePalette()
        {
            static int Brightness(int c) => ((c >> 16) & 255) + ((c >> 8) & 255) + (c & 255);
            var day = EnvironmentPalette.For(new ZoneEnvironment(Biome.Forest, Weather.Day));
            var night = EnvironmentPalette.For(new ZoneEnvironment(Biome.Forest, Weather.Night));
            var desert = EnvironmentPalette.For(new ZoneEnvironment(Biome.Desert, Weather.Day));
            Assert.That(Brightness(night.Ground), Is.LessThan(Brightness(day.Ground)));
            Assert.That(desert.Ground, Is.Not.EqualTo(day.Ground));
            Assert.That(night.Sky, Is.Not.EqualTo(day.Sky));
            Assert.That(night.Target, Is.EqualTo(day.Target), "les repères de jeu gardent leur couleur");
            var snowPlan = Plan("survival_snow", new ZoneEnvironment(Biome.Snow, Weather.Snow));
            Assert.That(snowPlan.Level.Palette.Ground, Is.EqualTo(EnvironmentPalette.For(new ZoneEnvironment(Biome.Snow, Weather.Snow)).Ground));
            Assert.That(snowPlan.Level.Primitives.First(p => p.Role == "ground").Color, Is.EqualTo(snowPlan.Level.Palette.Ground));
        }

        [Test, Description("Vérifie que la survie en neige génère le nombre de totems demandé, un objet à trouver à la distance voulue et plus de neige en biome neigeux qu'en désert — parce que le biome change la répartition des terrains")]
        public void SnowSurvival_Level_FollowsParametersAndBiome()
        {
            var plan = Plan("survival_snow");
            Assert.That(plan.Level.Primitives.Count(p => p.Role == "totem"), Is.EqualTo(plan.Parameters.Get("totem_count")));
            Assert.That(plan.Level.TryMarker("target", out var t), Is.True);
            long dist2 = (long)t.X * t.X + (long)t.Z * t.Z;
            long want = plan.Parameters.Get("target_distance_m") * 100L;
            Assert.That(dist2, Is.InRange(want * want * 90 / 100, want * want * 110 / 100), "à la distance demandée (≈, directions entières)");
            Assert.That(plan.Level.TryMarker("spawn", out _), Is.True);

            int SnowPatches(Biome b)
            {
                int n = 0;
                for (ulong seed = 1; seed <= 20; seed++)
                    n += Plan("survival_snow", new ZoneEnvironment(b, Weather.Day), seed).Level.Primitives.Count(p => p.Role == "terrain_snow");
                return n;
            }
            Assert.That(SnowPatches(Biome.Snow), Is.GreaterThan(SnowPatches(Biome.Desert) * 3));
        }

        [Test, Description("Vérifie que l'arène génère un point d'apparition par ennemi et par vague, et un comportement simple par vague — parce que les vagues d'ennemis suivent des patterns basiques")]
        public void Arena_Level_HasSpawnsAndPatternsPerWave()
        {
            var plan = Plan("arena_training");
            int waves = plan.Parameters.Get("waves"), per = plan.Parameters.Get("enemies_per_wave");
            Assert.That(plan.Level.Markers.Count(m => m.Name.StartsWith("spawn_w")), Is.EqualTo(waves * per));
            Assert.That(plan.Level.Fact("enemies_total"), Is.EqualTo(waves * per));
            for (int w = 0; w < waves; w++) Assert.That(plan.Level.Fact("wave" + w + "_pattern", -1), Is.InRange(0, 2));
        }

        // ------------------------------------------------------------------ Potion et dialogue

        [Test, Description("Vérifie que la recette de potion a le bon nombre d'étapes, qu'elle se valide avec les bons ingrédients dans le bon ordre (à la tolérance près) et pas autrement — parce que l'ordre et les quantités comptent")]
        public void Potion_Recipe_ValidatesOrderAndQuantities()
        {
            var plan = Plan("potion_field", squad: new[] { "ancre", "givre" });
            var recipe = plan.Level.PayloadAs<PotionRecipe>();
            Assert.That(recipe, Is.Not.Null);
            Assert.That(recipe!.Steps.Count, Is.EqualTo(plan.Parameters.Get("steps")));
            Assert.That(recipe.Evaluate(recipe.Steps), Is.True);
            var swapped = recipe.Steps.Reverse().ToList();
            Assert.That(recipe.Evaluate(swapped), Is.False, "mauvais ordre");
            var off = recipe.Steps.Select(s => new PotionStep(s.Ingredient, s.Quantity + 1)).ToList();
            Assert.That(recipe.Evaluate(off), Is.False, "mauvaises quantités (tolérance 0)");
            Assert.That(recipe.Evaluate(recipe.Steps.Take(1).ToList()), Is.False, "recette incomplète");
            Assert.That(recipe.Hints.Count(h => h.Kind == "effect"), Is.EqualTo(recipe.Ingredients.Count), "le livre décrit chaque ingrédient");
            Assert.That(recipe.Hints.Count(h => h.Kind == "order"), Is.EqualTo(recipe.Steps.Count), "le livre donne l'ordre");
            Assert.That(recipe.Ingredients.Select(i => i.Tag).Distinct().Count(), Is.EqualTo(recipe.Ingredients.Count), "propriétés distinctes");
        }

        [Test, Description("Vérifie que la potion tire un patient dans l'escouade (et aucun sans escouade) de façon reproductible — parce que le cas à aider est un membre de l'équipe")]
        public void Potion_PicksAPatient_FromTheSquad()
        {
            var squad = new[] { "ancre", "givre", "echo" };
            var a = Plan("potion_field", squad: squad); var b = Plan("potion_field", squad: squad);
            Assert.That(a.Patient, Is.Not.Null);
            Assert.That(squad, Does.Contain(a.Patient!.Value.Value));
            Assert.That(b.Patient, Is.EqualTo(a.Patient));
            Assert.That(Plan("potion_field").Patient, Is.Null);
            Assert.That(Plan("deflect_docks", squad: squad).Patient, Is.Null, "une activité sans patient n'en tire pas");
        }

        [Test, Description("Vérifie que chaque réplique du dialogue a une bonne, une mauvaise et des réponses acceptables, que les meilleures réponses réussissent et les pires échouent — parce que les réponses décident de la réussite")]
        public void Dialogue_Script_ScoresChoices()
        {
            var script = Plan("dialogue_dockers").Level.PayloadAs<DialogueScript>()!;
            Assert.That(script.Lines.Count, Is.EqualTo(4));
            foreach (var line in script.Lines)
            {
                Assert.That(line.Choices.Count(c => c.Score == 2), Is.EqualTo(1));
                Assert.That(line.Choices.Count(c => c.Score == 0), Is.EqualTo(1));
            }
            var best = script.Lines.Select(l => l.Choices.First(c => c.Score == 2).Index).ToList();
            var worst = script.Lines.Select(l => l.Choices.First(c => c.Score == 0).Index).ToList();
            Assert.That(script.Evaluate(best), Is.EqualTo((8, true)));
            Assert.That(script.Evaluate(worst), Is.EqualTo((0, false)));
            // 60 % requis : 2+2+1+0 = 5/8 (62 %) réussit ; 2+1+1+0 = 4/8 (50 %) échoue.
            int Mid(int line) => script.Lines[line].Choices.First(c => c.Score == 1).Index;
            Assert.That(script.Evaluate(new List<int> { best[0], best[1], Mid(2), worst[3] }), Is.EqualTo((5, true)));
            Assert.That(script.Evaluate(new List<int> { best[0], Mid(1), Mid(2), worst[3] }), Is.EqualTo((4, false)));
        }

        // ------------------------------------------------------------------ Résultats

        [Test, Description("Vérifie que les gains suivent le résultat : base à la réussite, pièces au prorata du score même en cas d'échec (pêche), rien d'autre à l'échec — parce que la quantité pêchée donne des pièces au prorata")]
        public void Rules_Rewards_AreProRata()
        {
            var plan = Plan("deflect_docks");
            var won = ActivityRules.Resolve(plan, ActivityResult.Won(6));
            Assert.That(won.Credits, Is.EqualTo(100 + 10 * 6));
            Assert.That(won.Pr, Is.EqualTo(5));
            var lost = ActivityRules.Resolve(plan, ActivityResult.Lost(2));
            Assert.That(lost.Credits, Is.EqualTo(10 * 2), "échec : pas de base, mais les projectiles interceptés rapportent");
            Assert.That(lost.Pr, Is.Zero);
            Assert.That(lost.Malus, Is.Null);
            Assert.That(lost.FollowUp, Is.Null);
        }

        [Test, Description("Vérifie qu'une potion ratée inflige un malus de 15 % au patient et qu'une potion réussie n'en inflige pas — parce que l'échec pénalise l'agent pendant tout le plateau")]
        public void Rules_PotionFailure_HurtsThePatient()
        {
            var plan = Plan("potion_field", squad: new[] { "ancre", "givre" });
            var lost = ActivityRules.Resolve(plan, ActivityResult.Lost());
            Assert.That(lost.Malus, Is.Not.Null);
            Assert.That(lost.Malus!.Member, Is.EqualTo(plan.Patient!.Value));
            Assert.That(lost.Malus.StatPercent, Is.EqualTo(-15));
            Assert.That(ActivityRules.Resolve(plan, ActivityResult.Won()).Malus, Is.Null);
            Assert.That(ActivityRules.Resolve(Plan("potion_field"), ActivityResult.Lost()).Malus, Is.Null, "sans patient, pas de malus");
        }

        [Test, Description("Vérifie qu'un dialogue raté enchaîne un combat Facile et qu'un dialogue réussi n'enchaîne rien — parce qu'un échec lance une activité liée")]
        public void Rules_DialogueFailure_ChainsACombat()
        {
            var plan = Plan("dialogue_dockers");
            var lost = ActivityRules.Resolve(plan, ActivityResult.Lost());
            Assert.That(lost.FollowUp, Is.Not.Null);
            Assert.That(lost.FollowUp!.Type, Is.EqualTo(FollowUpType.Combat));
            Assert.That(lost.FollowUp.Rank, Is.EqualTo(CombatRank.Easy));
            Assert.That(ActivityRules.Resolve(plan, ActivityResult.Won()).FollowUp, Is.Null);
        }

        // ------------------------------------------------------------------ Plateau

        static ZoneRun RunAt(ZoneId zone, string miniGameCase, ulong seed = 3)
        {
            var def = C.Zone(zone);
            var graph = new BoardGraph(def);
            var from = graph.Neighbors(new CaseId(miniGameCase)).First();
            var run = ZoneRun.ForTests(C, def, seed, 1, true, from.Value);
            Assert.That(run.Destinations.ContainsKey(new CaseId(miniGameCase)), Is.True, "case atteignable en 1 PM");
            run.MoveTo(new CaseId(miniGameCase));
            Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.InCaseInteraction));
            return run;
        }

        static IReadOnlyList<MemberId> Squad(params string[] ids) => ids.Select(i => new MemberId(i)).ToList();

        [Test, Description("Vérifie que l'activité de la case Mini-jeu est celle assignée au nœud, avec l'environnement de la zone, et qu'elle reste la même tant qu'elle n'est pas résolue — parce qu'une partie reprise doit retrouver la même activité")]
        public void Board_PlansTheNodeActivity_WithTheZoneEnvironment()
        {
            var run = RunAt(PrototypeContent.Zone11, "n15");
            var plan = run.PlanActivity(Squad("ancre", "givre"));
            Assert.That(plan.Def.Id.Value, Is.EqualTo("deflect_docks"));
            Assert.That(plan.Environment, Is.EqualTo(C.Zone(PrototypeContent.Zone11).Environment));
            Assert.That(run.PlanActivity(Squad("ancre")), Is.SameAs(plan), "idempotent");
            Assert.That(RunAt(PrototypeContent.Zone11, "n08").PlanActivity(Squad("ancre")).Def.Id, Is.EqualTo(ActivityIds.Reflex), "case sans activité : réflexe");
        }

        [Test, Description("Vérifie qu'une activité réussie résout la case et valide l'objectif éventuel, et qu'un échec la laisse à rejouer — parce que seul un succès compte pour les objectifs")]
        public void Board_Success_ResolvesTheCase_FailureLeavesItPlayable()
        {
            var run = RunAt(PrototypeContent.Zone11, "n15");
            run.PlanActivity(Squad("ancre"));
            run.ResolveActivity(ActivityResult.Lost(1));
            Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
            Assert.That(run.IsSpent(new CaseId("n15")), Is.False, "échec : la case reste à jouer");

            var again = RunAt(PrototypeContent.Zone11, "n15");
            var events = again.ResolveActivity(ActivityResult.Won(5));
            Assert.That(again.IsSpent(new CaseId("n15")), Is.True);
            Assert.That(again.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
            Assert.That(events.OfType<ActivityResolvedEvent>().Single().Outcome.Credits, Is.EqualTo(100 + 10 * 5));

            var objective = RunAt(PrototypeContent.Zone11, "n08");
            objective.ResolveActivity(ActivityResult.Won());
            Assert.That(objective.CompletedObjectives, Does.Contain("obj_generator"));
        }

        [Test, Description("Vérifie qu'une potion ratée ajoute un malus au patient pour tout le plateau (borné) et que la réussite n'en ajoute pas — parce que l'agent reste affaibli jusqu'à la fin du plateau")]
        public void Board_PotionFailure_AddsAMalusForTheWholeBoard()
        {
            var run = RunAt(PrototypeZones.Zone13, "n06");
            var plan = run.PlanActivity(Squad("ancre", "givre", "echo"));
            Assert.That(plan.Def.Id.Value, Is.EqualTo("potion_field"));
            run.ResolveActivity(ActivityResult.Lost());
            Assert.That(run.Maluses.Count, Is.EqualTo(1));
            var patient = plan.Patient!.Value;
            Assert.That(run.MalusPercent(patient), Is.EqualTo(-15));
            Assert.That(run.MalusPercent(new MemberId(Squad("ancre", "givre", "echo").First(m => m != patient).Value)), Is.Zero);
            for (int i = 0; i < 6; i++) run.AddMalusForTests(new AgentMalus(patient, -15, "x"));
            Assert.That(run.MalusPercent(patient), Is.EqualTo(-ActivityRules.MaxMalusPercent), "plafonné");
        }

        [Test, Description("Vérifie qu'un dialogue raté lance un combat Facile depuis la case, que gagner ce combat ne résout pas la case de dialogue (à rejouer) et que le perdre fait reculer — parce que l'échec enchaîne une activité liée sans valider le dialogue")]
        public void Board_DialogueFailure_LaunchesACombat_WithoutResolvingTheCase()
        {
            var win = RunAt(PrototypeContent.Zone11, "n21");
            var events = win.ResolveActivity(ActivityResult.Lost());
            Assert.That(win.Phase, Is.EqualTo(ZoneRunPhase.InCombat));
            Assert.That(win.PendingCase!.Type, Is.EqualTo(CaseType.Combat));
            Assert.That(win.PendingCase.Rank, Is.EqualTo(CombatRank.Easy));
            Assert.That(win.PendingCase.Encounter, Is.Not.Null);
            Assert.That(events.OfType<CaseTriggeredEvent>().Count(), Is.EqualTo(1));
            win.ResolveCombat(true);
            Assert.That(win.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
            Assert.That(win.IsSpent(new CaseId("n21")), Is.False, "le dialogue reste à rejouer");
            Assert.That(win.Position, Is.EqualTo(new CaseId("n21")));

            var lose = RunAt(PrototypeContent.Zone11, "n21");
            var before = lose.Position;
            lose.ResolveActivity(ActivityResult.Lost());
            lose.ResolveCombat(false);
            Assert.That(lose.Position, Is.Not.EqualTo(new CaseId("n21")), "défaite : recul d'une case");
        }

        [Test, Description("Vérifie qu'un malus d'agent réduit ses statistiques de combat du pourcentage voulu — parce que le malus de la potion pèse vraiment en combat")]
        public void Combat_AppliesTheAgentMalus()
        {
            CombatUnit Ally(int stat)
            {
                var setup = new CombatSetup(1UL, CombatRank.Normal, new[] { new AllyEntry(new MemberId("ancre"), 5, 0, stat) },
                                            new[] { new EnemyEntry(new EnemyId("thug"), 5) }, false);
                return new CombatSimulation(C, setup).Units.First(u => u.Side == Side.Ally);
            }
            var normal = Ally(0); var hurt = Ally(-20);
            Assert.That(hurt.MaxHp, Is.EqualTo(normal.MaxHp * 80 / 100).Within(1));
            Assert.That(hurt.Atk, Is.EqualTo(normal.Atk * 80 / 100).Within(1));
            Assert.That(hurt.Def, Is.EqualTo(normal.Def * 80 / 100).Within(1));
        }
    }
}
