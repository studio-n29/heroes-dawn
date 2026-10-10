using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Maps;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Outil de level design : validation des plateaux, combats définis par case, fond de carte généré, fusion des zones (spec 2026-10-10-outil-level-design).</summary>
    public sealed class LevelDesignDomainTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static NodeDef N(string id, CaseType t, int x, int y, bool fix = false, CombatRank r = CombatRank.Normal, CombatSpec combat = null, string activity = null)
            => new NodeDef(new CaseId(id), t, fix, r, x, y, activity == null ? (ActivityId?)null : new ActivityId(activity), combat);

        static LinkDef L(string a, string b) => new LinkDef(new CaseId(a), new CaseId(b));

        static ZoneDef Zone(List<NodeDef> nodes, List<LinkDef> links, List<ObjectiveDef> objectives = null, List<EncounterDef> encounters = null)
            => new ZoneDef(new ZoneId("zone_t"), "zones.t.name", new CaseId("n00"), nodes, links, objectives ?? new List<ObjectiveDef>(),
                           encounters ?? new List<EncounterDef> { new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug") }) });

        static List<NodeDef> Chain() => new List<NodeDef>
        {
            N("n00", CaseType.Entry, 0, 0, true), N("n01", CaseType.Empty, 1, 0), N("n02", CaseType.Combat, 2, 0, true, CombatRank.Elite), N("n03", CaseType.Empty, 3, 0),
        };

        static IEnumerable<string> Codes(IEnumerable<ZoneIssue> i) => i.Select(x => x.Code);

        [Test, Description("Vérifie que les trois zones du prototype sont jouables (aucune erreur de validation) — parce que l'outil doit accepter le contenu existant")]
        public void Prototype_Zones_AreValid()
        {
            foreach (var z in C.Zones)
            {
                var issues = ZoneValidator.Validate(z, C);
                Assert.That(ZoneValidator.IsPlayable(issues), Is.True, z.Id.Value + " : " + string.Join(" | ", issues.Where(i => i.Severity == IssueSeverity.Error)));
            }
        }

        [Test, Description("Vérifie que la validation détecte une case d'entrée absente, un lien vers une case inconnue, un lien en double, un lien sur soi et des cases superposées — parce que ces erreurs rendent le plateau injouable")]
        public void Validator_FlagsStructuralErrors()
        {
            var nodes = Chain(); nodes.Add(N("n04", CaseType.Empty, 3, 0));
            var z = Zone(nodes, new List<LinkDef> { L("n00", "n01"), L("n01", "n02"), L("n02", "n03"), L("n03", "n99"), L("n01", "n00"), L("n01", "n01"), L("n03", "n04") });
            var codes = Codes(ZoneValidator.Validate(z, C)).ToList();
            Assert.That(codes, Does.Contain("link.unknown"));
            Assert.That(codes, Does.Contain("link.duplicate"));
            Assert.That(codes, Does.Contain("link.self"));
            Assert.That(codes, Does.Contain("node.overlap"));
            var noEntry = Zone(new List<NodeDef> { N("n00", CaseType.Empty, 0, 0) }, new List<LinkDef>());
            Assert.That(Codes(ZoneValidator.Validate(noEntry, C)), Does.Contain("entry.type"));
        }

        [Test, Description("Vérifie qu'une case inaccessible depuis l'entrée est une erreur et qu'une impasse est un avertissement — parce que le GDD demande un plateau connecté et sans impasse")]
        public void Validator_FlagsUnreachableNodes_AndWarnsAboutDeadEnds()
        {
            var nodes = Chain(); nodes.Add(N("n09", CaseType.Empty, 9, 9));
            var z = Zone(nodes, new List<LinkDef> { L("n00", "n01"), L("n01", "n02"), L("n02", "n03") });
            var issues = ZoneValidator.Validate(z, C);
            Assert.That(issues.Single(i => i.Code == "node.unreachable").Severity, Is.EqualTo(IssueSeverity.Error));
            Assert.That(issues.First(i => i.Code == "node.deadend").Severity, Is.EqualTo(IssueSeverity.Warning));
            Assert.That(Codes(issues), Does.Contain("zone.branches"));
        }

        [Test, Description("Vérifie que les cases Combat sans table ni rencontre de zone, avec un ennemi inconnu, et que les activités inconnues sont des erreurs — parce que le combat ou l'activité ne pourrait pas se lancer")]
        public void Validator_ChecksCombatAndActivityCases()
        {
            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 0, true), N("n01", CaseType.Combat, 1, 0, combat: new CombatSpec(new List<EnemyId>())),
                N("n02", CaseType.Combat, 2, 0, combat: new CombatSpec(new[] { new EnemyId("fantome") })),
                N("n03", CaseType.MiniGame, 3, 0, activity: "n_existe_pas"), N("n04", CaseType.Combat, 4, 0),
            };
            var links = new List<LinkDef> { L("n00", "n01"), L("n01", "n02"), L("n02", "n03"), L("n03", "n04") };
            var codes = Codes(ZoneValidator.Validate(Zone(nodes, links, encounters: new List<EncounterDef>()), C)).ToList();
            Assert.That(codes, Does.Contain("combat.table"));
            Assert.That(codes, Does.Contain("combat.enemy"));
            Assert.That(codes, Does.Contain("activity.unknown"));
            Assert.That(codes, Does.Contain("combat.encounter"), "n04 : ni table ni rencontre de zone");
        }

        [Test, Description("Vérifie que les objectifs inconnus, inaccessibles ou de mauvais type (combat vaincu sur une case vide) sont signalés — parce qu'un objectif impossible bloque la zone")]
        public void Validator_ChecksObjectives()
        {
            var z = Zone(Chain(), new List<LinkDef> { L("n00", "n01"), L("n01", "n02"), L("n02", "n03") }, new List<ObjectiveDef>
            {
                new ObjectiveDef("a", ObjectiveType.DefeatCombat, new CaseId("n01"), "k"),
                new ObjectiveDef("b", ObjectiveType.ReachCase, new CaseId("zz"), "k"),
                new ObjectiveDef("a", ObjectiveType.DefeatCombat, new CaseId("n02"), "k"),
            });
            var codes = Codes(ZoneValidator.Validate(z, C)).ToList();
            Assert.That(codes, Does.Contain("objective.type"));
            Assert.That(codes, Does.Contain("objective.target"));
            Assert.That(codes, Does.Contain("objective.duplicate"));
        }

        [Test, Description("Vérifie que la rencontre générée par une case respecte le nombre de manches, la taille des vagues et la table d'ennemis, de façon reproductible — parce que le level designer définit la table et les manches")]
        public void EncounterGenerator_FollowsTheSpec_Deterministically()
        {
            var spec = new CombatSpec(new[] { new EnemyId("thug"), new EnemyId("brute") }, 3, 2, 4);
            var a = EncounterGenerator.Generate(spec, CombatRank.Normal, new Pcg32(5));
            var b = EncounterGenerator.Generate(spec, CombatRank.Normal, new Pcg32(5));
            Assert.That(a.Waves.Count, Is.EqualTo(3));
            Assert.That(a.Waves.All(w => w.Count >= 2 && w.Count <= 4));
            Assert.That(a.Waves.SelectMany(w => w).All(e => e.Value == "thug" || e.Value == "brute"));
            Assert.That(a.Waves.Select(w => string.Join(",", w.Select(e => e.Value))), Is.EqualTo(b.Waves.Select(w => string.Join(",", w.Select(e => e.Value)))));
            Assert.That(a.Rank, Is.EqualTo(CombatRank.Normal));
        }

        [Test, Description("Vérifie qu'une case Combat avec table et manches lance la rencontre générée (3 manches) et qu'une case sans table garde les rencontres de zone — parce que le combat de la case prime sur celui de la zone")]
        public void Board_CombatCaseWithSpec_UsesTheGeneratedEncounter()
        {
            var spec = new CombatSpec(new[] { new EnemyId("thug") }, 3, 2, 2);
            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 0, true), N("n01", CaseType.Combat, 1, 0, combat: spec), N("n02", CaseType.Combat, 2, 0),
            };
            var zone = Zone(nodes, new List<LinkDef> { L("n00", "n01"), L("n01", "n02") });
            var run = ZoneRun.ForTests(C, zone, 4UL, 1, true, "n00");
            run.MoveTo(new CaseId("n01"));
            Assert.That(run.PendingCase!.Encounter!.Waves.Count, Is.EqualTo(3));
            Assert.That(run.PendingCase.Encounter.Waves.All(w => w.Count == 2));
            run.ResolveCombat(true);
            run.SetPmForTests(1);
            run.MoveTo(new CaseId("n02"));
            Assert.That(run.PendingCase!.Encounter!.Waves.Count, Is.EqualTo(1), "rencontre de la zone");
        }

        // ------------------------------------------------------------------ Fond de carte

        static MapBackground Bg(Biome b, Weather w, ulong seed = 3, IReadOnlyList<MapRegion> regions = null)
            => MapBackgroundGenerator.Generate(765, 500, seed, b, w, regions);

        [Test, Description("Vérifie que le même plateau (graine, biome, météo, régions) redonne exactement le même fond et qu'une autre graine en donne un autre — parce que le fond est reproductible")]
        public void Background_IsDeterministic()
        {
            var a = Bg(Biome.City, Weather.Day); var b = Bg(Biome.City, Weather.Day); var other = Bg(Biome.City, Weather.Day, 4);
            Assert.That(a.Shapes.Select(s => (s.Kind, s.X0, s.Y0, s.Color)), Is.EqualTo(b.Shapes.Select(s => (s.Kind, s.X0, s.Y0, s.Color))));
            Assert.That(a.Shapes.Select(s => (s.X0, s.Y0)), Is.Not.EqualTo(other.Shapes.Select(s => (s.X0, s.Y0))));
        }

        [Test, Description("Vérifie que chaque biome produit un fond d'aspect différent (couleur du sol et nombre de formes) — parce que ville, forêt, port… doivent se reconnaître")]
        public void Background_DiffersPerBiome()
        {
            var grounds = new HashSet<int>();
            foreach (var biome in new[] { Biome.City, Biome.Harbor, Biome.Industrial, Biome.Forest, Biome.Snow, Biome.Desert })
            {
                var bg = Bg(biome, Weather.Day);
                Assert.That(bg.Shapes.Count, Is.GreaterThan(20), biome.ToString());
                grounds.Add(bg.Shapes[0].Color);
            }
            Assert.That(grounds.Count, Is.EqualTo(6), "un sol par biome");
        }

        [Test, Description("Vérifie que la météo change les couleurs (nuit plus sombre), ajoute des fenêtres éclairées la nuit, des traits sous la pluie et des flocons sous la neige — parce que la météo doit se voir sur la carte")]
        public void Background_WeatherChangesTheLook()
        {
            static int Bright(int argb) => ((argb >> 16) & 255) + ((argb >> 8) & 255) + (argb & 255);
            var day = Bg(Biome.City, Weather.Day); var night = Bg(Biome.City, Weather.Night);
            Assert.That(Bright(night.Shapes[0].Color), Is.LessThan(Bright(day.Shapes[0].Color)));
            Assert.That(night.Shapes.Count, Is.GreaterThan(day.Shapes.Count), "fenêtres éclairées");
            Assert.That(Bg(Biome.City, Weather.Rain).Shapes.Count(s => s.Kind == MapShapeKind.Line && ((s.Color >> 24) & 255) < 255), Is.GreaterThan(50), "traits de pluie");
            Assert.That(Bg(Biome.City, Weather.Snow).Shapes.Count(s => s.Kind == MapShapeKind.Circle && s.Size <= 2), Is.GreaterThan(50), "flocons");
            Assert.That(Bg(Biome.City, Weather.Fog).Shapes.Count, Is.GreaterThan(day.Shapes.Count));
        }

        [Test, Description("Vérifie qu'une région peinte ajoute un décor différent à l'endroit voulu (forêt dans une ville) — parce que le level designer peint des zones de biome sur la carte")]
        public void Background_RegionsAddTheirOwnBiome()
        {
            var plain = Bg(Biome.City, Weather.Day);
            var withForest = Bg(Biome.City, Weather.Day, 3, new[] { new MapRegion(Biome.Forest, 380, 250, 150) });
            Assert.That(withForest.Shapes.Count, Is.GreaterThan(plain.Shapes.Count));
            var disc = withForest.Shapes.First(s => s.Kind == MapShapeKind.Circle && s.X0 == 380 && s.Y0 == 250 && s.Size == 150);
            Assert.That(disc.Color, Is.Not.EqualTo(plain.Shapes[0].Color), "sol de forêt dans la ville");
        }

        // ------------------------------------------------------------------ Fusion des zones

        [Test, Description("Vérifie que WithZones remplace une zone de même identifiant en gardant l'ordre, ajoute les nouvelles et conserve le reste du contenu — parce que les plateaux de l'outil complètent ou remplacent ceux du code")]
        public void Catalog_WithZones_ReplacesAndAppends()
        {
            var replaced = Zone(Chain(), new List<LinkDef> { L("n00", "n01"), L("n01", "n02"), L("n02", "n03") });
            var first = C.Zones[0];
            var over = new ZoneDef(first.Id, "zones.over.name", first.Entry, replaced.Nodes, replaced.Links, replaced.Objectives, replaced.Encounters);
            var extra = new ZoneDef(new ZoneId("zone_extra"), "zones.extra.name", replaced.Entry, replaced.Nodes, replaced.Links, replaced.Objectives, replaced.Encounters);
            var merged = C.WithZones(new[] { over, extra });
            Assert.That(merged.Zones.Count, Is.EqualTo(C.Zones.Count + 1));
            Assert.That(merged.Zones[0].NameKey, Is.EqualTo("zones.over.name"));
            Assert.That(merged.Zones.Last().Id.Value, Is.EqualTo("zone_extra"));
            Assert.That(merged.Members.Count, Is.EqualTo(C.Members.Count));
            Assert.That(merged.Activities.Count, Is.EqualTo(C.Activities.Count));
            Assert.That(C.WithZones(new List<ZoneDef>()), Is.SameAs(C));
        }
    }
}
