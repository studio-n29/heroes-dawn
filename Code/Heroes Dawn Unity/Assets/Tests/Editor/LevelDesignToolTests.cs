using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Maps;
using N29.HeroesDawn.EditorTools.LevelDesign;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Outil de level design : opérations de l'éditeur, conversion asset ↔ zone, campagne, rastérisation du fond (spec 2026-10-10-outil-level-design).</summary>
    public sealed class LevelDesignToolTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _created) if (o != null) Object.DestroyImmediate(o);
            _created.Clear();
        }

        ZoneAsset NewAsset(string id = "zone_test")
        {
            var z = ScriptableObject.CreateInstance<ZoneAsset>();
            z.zoneId = id;
            _created.Add(z);
            return z;
        }

        ZoneAsset Imported(ZoneDef def)
        {
            var a = NewAsset();
            ZoneAssetConverter.FillFrom(a, def);
            return a;
        }

        // ------------------------------------------------------------------ Éditeur

        [Test, Description("Vérifie que la première case posée est l'entrée, qu'une cellule occupée refuse une seconde case et que les identifiants sont réutilisés au plus petit libre — parce que le plateau naît avec son entrée et ses cases ne se superposent pas")]
        public void Model_AddNode_FirstIsEntry_RefusesOccupiedCells()
        {
            var z = NewAsset();
            var first = LevelEditorModel.AddNode(z, CaseType.Combat, 0, 0);
            Assert.That(first!.type, Is.EqualTo(CaseType.Entry));
            Assert.That(z.entry, Is.EqualTo(first.id));
            Assert.That(LevelEditorModel.AddNode(z, CaseType.Empty, 0, 0), Is.Null);
            var second = LevelEditorModel.AddNode(z, CaseType.Empty, 1, 0)!;
            Assert.That(second.id, Is.EqualTo("n01"));
            LevelEditorModel.DeleteNode(z, "n00");
            Assert.That(LevelEditorModel.NextNodeId(z), Is.EqualTo("n00"));
        }

        [Test, Description("Vérifie qu'il n'y a qu'une entrée (en désigner une rend l'ancienne vide) et qu'un changement de type efface les réglages de combat et d'activité — parce que l'entrée est unique et les réglages ne valent que pour leur type")]
        public void Model_SetType_KeepsASingleEntry_AndClearsTypeSettings()
        {
            var z = NewAsset();
            var a = LevelEditorModel.AddNode(z, CaseType.Entry, 0, 0)!;
            var b = LevelEditorModel.AddNode(z, CaseType.Combat, 1, 0)!;
            b.hasCombat = true; b.enemyTable.Add("thug");
            LevelEditorModel.SetType(z, b, CaseType.Entry);
            Assert.That(z.entry, Is.EqualTo(b.id));
            Assert.That(a.type, Is.EqualTo(CaseType.Empty));
            Assert.That(z.nodes.Count(n => n.type == CaseType.Entry), Is.EqualTo(1));
            Assert.That(b.hasCombat, Is.False, "plus un combat");
            var m = LevelEditorModel.AddNode(z, CaseType.MiniGame, 2, 0)!;
            m.activity = "deflect_docks";
            LevelEditorModel.SetType(z, m, CaseType.Rest);
            Assert.That(m.activity, Is.Empty);
        }

        [Test, Description("Vérifie que relier refuse les doublons et les liens sur soi, que supprimer une case retire ses liens et les objectifs qui la visaient, et que déplacer refuse une cellule occupée — parce que le graphe doit rester cohérent pendant l'édition")]
        public void Model_LinksAndDeletion_KeepTheGraphConsistent()
        {
            var z = NewAsset();
            LevelEditorModel.AddNode(z, CaseType.Entry, 0, 0);
            LevelEditorModel.AddNode(z, CaseType.Empty, 1, 0);
            LevelEditorModel.AddNode(z, CaseType.Combat, 2, 0);
            Assert.That(LevelEditorModel.Connect(z, "n00", "n01"), Is.True);
            Assert.That(LevelEditorModel.Connect(z, "n01", "n00"), Is.False, "doublon");
            Assert.That(LevelEditorModel.Connect(z, "n01", "n01"), Is.False, "sur soi");
            Assert.That(LevelEditorModel.Connect(z, "n01", "n99"), Is.False, "case inconnue");
            LevelEditorModel.Connect(z, "n01", "n02");
            var o = LevelEditorModel.AddObjective(z); o.target = "n02";
            Assert.That(LevelEditorModel.MoveNode(z, "n02", 1, 0), Is.False, "occupée");
            Assert.That(LevelEditorModel.MoveNode(z, "n02", 5, 3), Is.True);
            LevelEditorModel.DeleteNode(z, "n02");
            Assert.That(z.links.Count, Is.EqualTo(1));
            Assert.That(z.objectives, Is.Empty);
            Assert.That(LevelEditorModel.Disconnect(z, "n00", "n01"), Is.True);
            Assert.That(z.links, Is.Empty);
        }

        [Test, Description("Vérifie que la validation de l'éditeur remonte les problèmes du plateau en cours d'édition (case inaccessible) — parce que le level designer voit tout de suite ce qui bloque")]
        public void Model_Validate_ReportsProblemsWhileEditing()
        {
            var z = NewAsset();
            LevelEditorModel.AddNode(z, CaseType.Entry, 0, 0);
            LevelEditorModel.AddNode(z, CaseType.Empty, 3, 3);
            var issues = LevelEditorModel.Validate(z, C);
            Assert.That(issues.Any(i => i.Code == "node.unreachable" && i.Severity == IssueSeverity.Error), Is.True);
        }

        [Test, Description("Vérifie que le fond généré couvre la grille des cases plus une marge, à la résolution fixée, et que les zones peintes sont converties en pixels — parce que le fond doit se superposer exactement au plateau en jeu")]
        public void Model_Background_CoversTheGridWithAMargin()
        {
            var z = NewAsset();
            LevelEditorModel.AddNode(z, CaseType.Entry, 2, 1);
            LevelEditorModel.AddNode(z, CaseType.Empty, 6, 3);
            z.regions.Add(new RegionData { biome = Biome.Forest, x = 4f, y = 2f, radius = 1f });
            var b = LevelEditorModel.GridBounds(z);
            Assert.That(b, Is.EqualTo((1, 0, 7, 4)));
            var bg = LevelEditorModel.BuildBackground(z);
            Assert.That(bg.Width, Is.EqualTo(6 * LevelEditorModel.PxPerCellX));
            Assert.That(bg.Height, Is.EqualTo(4 * LevelEditorModel.PxPerCellY));
            var disc = bg.Shapes.First(s => s.Kind == MapShapeKind.Circle && s.Size == LevelEditorModel.PxPerCellX);
            Assert.That((disc.X0, disc.Y0), Is.EqualTo((3 * LevelEditorModel.PxPerCellX, 2 * LevelEditorModel.PxPerCellY)));
        }

        // ------------------------------------------------------------------ Conversion

        [Test, Description("Vérifie que chaque zone du prototype survit à l'aller-retour code → asset → zone (cases, liens, objectifs, rencontres, décor) et reste jouable — parce que l'import ne doit rien changer au jeu")]
        public void Converter_RoundTrip_PreservesThePrototypeZones()
        {
            foreach (var def in C.Zones)
            {
                var back = ZoneAssetConverter.ToDef(Imported(def));
                Assert.That(back.Id, Is.EqualTo(def.Id));
                Assert.That(back.NameKey, Is.EqualTo(def.NameKey));
                Assert.That(back.Entry, Is.EqualTo(def.Entry));
                Assert.That(back.Nodes.Select(n => (n.Id.Value, n.Type, n.IsFixed, n.Rank, n.X, n.Y, n.Activity?.Value)),
                            Is.EqualTo(def.Nodes.Select(n => (n.Id.Value, n.Type, n.IsFixed, n.Rank, n.X, n.Y, n.Activity?.Value))), def.Id.Value);
                Assert.That(back.Links.Select(l => (l.A.Value, l.B.Value)), Is.EqualTo(def.Links.Select(l => (l.A.Value, l.B.Value))));
                Assert.That(back.Objectives.Select(o => (o.Id, o.Type, o.Target.Value, o.NameKey)), Is.EqualTo(def.Objectives.Select(o => (o.Id, o.Type, o.Target.Value, o.NameKey))));
                Assert.That(back.Encounters.Select(e => (e.Rank, string.Join(";", e.Waves.Select(w => string.Join(",", w.Select(x => x.Value)))))),
                            Is.EqualTo(def.Encounters.Select(e => (e.Rank, string.Join(";", e.Waves.Select(w => string.Join(",", w.Select(x => x.Value))))))));
                Assert.That((back.Code, back.Act, back.Borough, back.RecommendedLevel), Is.EqualTo((def.Code, def.Act, def.Borough, def.RecommendedLevel)));
                Assert.That(back.Requires, Is.EqualTo(def.Requires));
                Assert.That(back.Environment.Biome, Is.EqualTo(def.Environment.Biome));
                Assert.That(back.Environment.Weather, Is.EqualTo(def.Environment.Weather));
                Assert.That(ZoneValidator.IsPlayable(ZoneValidator.Validate(back, C)), Is.True);
            }
        }

        [Test, Description("Vérifie que la table d'ennemis et les manches d'une case Combat passent dans la zone jouable, et qu'un réglage de combat sur une case d'un autre type est ignoré — parce que le combat de la case ne vaut que pour une case Combat")]
        public void Converter_CarriesTheCombatSpec_OnCombatNodesOnly()
        {
            var z = NewAsset();
            LevelEditorModel.AddNode(z, CaseType.Entry, 0, 0);
            var fight = LevelEditorModel.AddNode(z, CaseType.Combat, 1, 0)!;
            fight.hasCombat = true; fight.enemyTable = new List<string> { "thug", "brute" }; fight.rounds = 3; fight.enemiesMin = 2; fight.enemiesMax = 4;
            var other = LevelEditorModel.AddNode(z, CaseType.Empty, 2, 0)!;
            other.hasCombat = true; other.enemyTable = new List<string> { "thug" };
            var def = ZoneAssetConverter.ToDef(z);
            var spec = def.Nodes.First(n => n.Id.Value == fight.id).Combat!;
            Assert.That(spec.EnemyTable.Select(e => e.Value), Is.EqualTo(new[] { "thug", "brute" }));
            Assert.That((spec.Rounds, spec.EnemiesMin, spec.EnemiesMax), Is.EqualTo((3, 2, 4)));
            Assert.That(def.Nodes.First(n => n.Id.Value == other.id).Combat, Is.Null);
        }

        [Test, Description("Vérifie que les textes saisis dans l'outil (nom, nom court, description, objectifs) sont publiés sous les clés du jeu et que des champs vides ne remplacent rien — parce que les zones créées dans l'outil doivent s'afficher sans toucher au code")]
        public void Converter_Texts_UseTheGameKeys()
        {
            var z = NewAsset("zone_ny_2_1");
            z.displayName = "QUEENS — LE PORT"; z.shortName = "Queens"; z.description = "Un port.";
            z.objectives.Add(new ObjectiveData { id = "o1", text = "Atteindre la grue" });
            z.objectives.Add(new ObjectiveData { id = "o2", nameKey = "objectives.existant" });
            var t = ZoneAssetConverter.TextsOf(z).Values;
            Assert.That(t["zones.ny_2_1.name"], Is.EqualTo("QUEENS — LE PORT"));
            Assert.That(t["zones.ny_2_1.short"], Is.EqualTo("Queens"));
            Assert.That(t["zones.ny_2_1.desc"], Is.EqualTo("Un port."));
            Assert.That(t["objectives.ny_2_1.o1"], Is.EqualTo("Atteindre la grue"));
            Assert.That(t.ContainsKey("objectives.existant"), Is.False);
            Assert.That(ZoneAssetConverter.TextsOf(NewAsset("zone_vide")).Values, Is.Empty);
        }

        [Test, Description("Vérifie que le fond généré, la position sur la carte de la ville et l'environnement de l'asset arrivent dans la zone jouable — parce que le plateau doit afficher son fond et la ville son point")]
        public void Converter_CarriesBackgroundMapPointAndEnvironment()
        {
            var z = NewAsset();
            z.biome = Biome.Forest; z.weather = Weather.Rain;
            z.backgroundKey = "zone_test_bg"; z.backgroundMinX = -1; z.backgroundMinY = 0; z.backgroundMaxX = 9; z.backgroundMaxY = 5;
            z.hasMapPoint = true; z.mapPointPermille = new Vector2Int(250, 600);
            var def = ZoneAssetConverter.ToDef(z);
            Assert.That((def.Environment.Biome, def.Environment.Weather), Is.EqualTo((Biome.Forest, Weather.Rain)));
            Assert.That((def.Background!.Key, def.Background.MinX, def.Background.MaxY), Is.EqualTo(("zone_test_bg", -1, 5)));
            Assert.That((def.MapPoint!.Value.XPermille, def.MapPoint.Value.YPermille), Is.EqualTo((250, 600)));
            z.backgroundKey = ""; z.hasMapPoint = false;
            var plain = ZoneAssetConverter.ToDef(z);
            Assert.That(plain.Background, Is.Null);
            Assert.That(plain.MapPoint, Is.Null);
        }

        // ------------------------------------------------------------------ Campagne

        CampaignAsset NewCampaign(params int[] acts)
        {
            var c = ScriptableObject.CreateInstance<CampaignAsset>();
            _created.Add(c);
            for (int i = 0; i < acts.Length; i++) LevelEditorModel.AddCampaignZone(c, NewAsset("zone_c" + i), acts[i]);
            return c;
        }

        [Test, Description("Vérifie que les codes de la campagne sont « acte.rang dans l'acte » (1.1, 1.2, 1.3, 2.1), qu'une surcharge prime et que réordonner recalcule les codes — parce que la carte de campagne se lit en 1.1, 1.2, 2.1…")]
        public void Campaign_Codes_FollowActsAndOrder()
        {
            var c = NewCampaign(1, 1, 1, 2);
            Assert.That(Enumerable.Range(0, 4).Select(c.CodeOf), Is.EqualTo(new[] { "1.1", "1.2", "1.3", "2.1" }));
            c.entries[1].codeOverride = "1.B";
            Assert.That(c.CodeOf(1), Is.EqualTo("1.B"));
            Assert.That(LevelEditorModel.MoveCampaignEntry(c, 3, -3), Is.True);
            Assert.That(c.entries[0].act, Is.EqualTo(2));
            Assert.That(c.CodeOf(0), Is.EqualTo("2.1"));
            Assert.That(c.CodeOf(1), Is.EqualTo("1.1"));
            Assert.That(LevelEditorModel.MoveCampaignEntry(c, 0, -1), Is.False, "hors limites");
        }

        [Test, Description("Vérifie que la campagne ne contient pas deux fois la même zone et que « chaîner les prérequis » fait demander à chaque zone la précédente — parce qu'une zone n'a qu'une place dans la campagne")]
        public void Campaign_NoDuplicates_AndChainedRequirements()
        {
            var c = NewCampaign(1, 1);
            LevelEditorModel.AddCampaignZone(c, c.entries[0].zone!, 1);
            Assert.That(c.entries.Count, Is.EqualTo(2));
            LevelEditorModel.AddCampaignZone(c, NewAsset("zone_c9"), 2);
            LevelEditorModel.ChainRequirements(c);
            Assert.That(c.entries[0].requires, Is.Empty);
            Assert.That(c.entries[1].requires, Is.EqualTo(new[] { "zone_c0" }));
            Assert.That(c.entries[2].requires, Is.EqualTo(new[] { "zone_c1" }));
        }

        [Test, Description("Vérifie que la bibliothèque de zones applique les codes, actes et prérequis de la campagne aux zones jouables et publie leurs textes — parce que la campagne est la liste des plateaux jouables")]
        public void Library_FromCampaign_AppliesCodesActsAndRequirements()
        {
            var c = NewCampaign(1, 2);
            c.entries[0].zone!.displayName = "Premier";
            c.entries[1].requires = new List<string> { "zone_c0" };
            var lib = ZoneLibrary.FromCampaign(c);
            Assert.That(lib.Zones.Select(z => (z.Code, z.Act)), Is.EqualTo(new[] { ("1.1", 1), ("2.1", 2) }));
            Assert.That(lib.Zones[1].Requires.Select(r => r.Value), Is.EqualTo(new[] { "zone_c0" }));
            Assert.That(lib.Texts["zones.c0.name"], Is.EqualTo("Premier"));
            Assert.That(ZoneLibrary.Empty.Zones, Is.Empty);
        }

        // ------------------------------------------------------------------ Rastérisation

        static float Average(Texture2D t)
        {
            var px = t.GetPixels32();
            long sum = 0;
            foreach (var p in px) sum += p.r + p.g + p.b;
            return sum / (3f * px.Length);
        }

        [Test, Description("Vérifie que la rastérisation produit une image de la bonne taille qui n'est pas unie, plus sombre la nuit, et que la pluie translucide ne casse pas le rendu — parce que le fond doit se dessiner correctement pour chaque météo")]
        public void Rasterizer_RendersTheBackground_ForEveryWeather()
        {
            foreach (var weather in new[] { Weather.Day, Weather.Night, Weather.Rain, Weather.Snow, Weather.Fog })
            {
                var tex = MapRasterizer.Render(MapBackgroundGenerator.Generate(340, 250, 9, Biome.City, weather));
                _created.Add(tex);
                Assert.That((tex.width, tex.height), Is.EqualTo((340, 250)));
                var px = tex.GetPixels32();
                Assert.That(px.Select(p => (p.r, p.g, p.b)).Distinct().Count(), Is.GreaterThan(5), weather + " : image unie");
                Assert.That(px.All(p => p.a == 255), Is.True);
            }
            var day = MapRasterizer.Render(MapBackgroundGenerator.Generate(340, 250, 9, Biome.City, Weather.Day));
            var night = MapRasterizer.Render(MapBackgroundGenerator.Generate(340, 250, 9, Biome.City, Weather.Night));
            _created.Add(day); _created.Add(night);
            Assert.That(Average(night), Is.LessThan(Average(day) * 0.7f));
            Assert.That(day.EncodeToPNG().Length, Is.GreaterThan(100));
        }
    }
}
