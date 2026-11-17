#nullable enable
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>La campagne importée en assets (Resources/Zones) est celle qui se joue : mêmes zones, mêmes scènes de narration que le code.</summary>
    public sealed class CampaignAssetsTests
    {
        [Test, Description("Vérifie que la campagne importée contient les quatre zones de Brooklyn, dans l'ordre et à déblocage linéaire — parce que c'est cette bibliothèque qui remplace le contenu du code en jeu")]
        public void BakedCampaign_HasTheFourBrooklynZones()
        {
            var zones = ZoneLibrary.Load().Zones.Where(z => z.Borough == "brooklyn").OrderBy(z => z.Code).ToList();
            Assert.That(zones.Select(z => z.Code), Is.EqualTo(new[] { "1.1", "1.2", "1.3", "1.4" }));
            for (int i = 1; i < zones.Count; i++) Assert.That(zones[i].Requires.Single().Value, Is.EqualTo(zones[i - 1].Id.Value), zones[i].Code);
            Assert.That(zones[0].Objectives.Count, Is.EqualTo(5));
        }

        [Test, Description("Vérifie que la campagne importée contient les quatre zones de Moscou dans l'ordre, à déblocage linéaire, avec leurs scènes de narration et leurs textes — parce que c'est cette bibliothèque qui se joue")]
        public void Campaign_ContainsMoscowZones_InOrder()
        {
            var lib = ZoneLibrary.Load();
            var zones = lib.Zones.Where(z => z.Borough == "sergueiv").ToList();
            Assert.That(zones.Select(z => z.Code), Is.EqualTo(new[] { "M1.1", "M1.2", "M1.3", "M1.4" }));
            Assert.That(zones[0].Requires, Is.Empty);
            for (int i = 1; i < zones.Count; i++) Assert.That(zones[i].Requires.Single().Value, Is.EqualTo(zones[i - 1].Id.Value), zones[i].Code);
            Assert.That(lib.Zones.Select(z => z.Id.Value).Distinct().Count(), Is.EqualTo(lib.Zones.Count), "aucune zone en double");
            NarrationDef Of(string zone, string node) => zones.Single(z => z.Id.Value == zone).Nodes.Single(n => n.Id.Value == node).Narration!;
            Assert.That(Of("zone_mos_1_1", "n14").Before.Select(l => l.Speaker), Is.EqualTo(new[] { "enforcer", "ilona_varga" }));
            // Parcours d'Agent M : variantes .m et indices de la cathédrale compris ; les clés des répliques sont celles du script (le texte vit dans les dictionnaires de langue).
            var after = NarrationLineDef.ForRoute(Of("zone_mos_1_4", "n18").After, "agent_m");
            Assert.That(after.Select(l => l.Speaker), Is.EqualTo(new[] { "passeur", "agent_m", "passeur", "ilona_varga", "passeur", "narrateur" }));
            Assert.That(after[0].TextKey, Is.EqualTo("narration.mos_1_4.boss.after.0"));
            Assert.That(N29.HeroesDawn.Presentation.Common.Loc.Has(after[0].TextKey) && N29.HeroesDawn.Presentation.Common.Loc.T(after[0].TextKey).Length > 10, Is.True);
        }

        [Test, Description("Vérifie que le validateur accepte les quatre zones de Moscou importées (aucune erreur) et liste leurs avertissements — parce qu'une zone invalide ne se jouerait pas")]
        public void ZoneValidator_AcceptsTheFourMoscowZones()
        {
            var content = N29.HeroesDawn.Content.Prototype.PrototypeContent.Build().WithZones(ZoneLibrary.Load().Zones);
            foreach (var z in content.Zones.Where(z => z.Borough == "sergueiv"))
            {
                var issues = N29.HeroesDawn.Domain.Board.ZoneValidator.Validate(z, content);
                var errors = issues.Where(m => m.Severity == IssueSeverity.Error).Select(m => z.Id.Value + " : " + m.Message).ToList();
                Assert.That(errors, Is.Empty, string.Join("\n", errors));
                foreach (var w in issues.Where(m => m.Severity == IssueSeverity.Warning)) UnityEngine.Debug.Log("Avertissement " + z.Id.Value + " : " + w.Message);
            }
        }

        [Test, Description("Vérifie que les scènes de narration des zones (Brick, lieutenant, Crocodile, Rayner et le Général) sont dans les assets avec leur texte — parce que l'import doit conserver les dialogues de l'histoire")]
        public void BakedCampaign_CarriesTheNarration()
        {
            var lib = ZoneLibrary.Load();
            NarrationDef Of(string zone, string node) => lib.Zones.Single(z => z.Id.Value == zone).Nodes.Single(n => n.Id.Value == node).Narration!;
            Assert.That(Of("zone_ny_1_1", "n23").After.Count, Is.EqualTo(2));
            Assert.That(Of("zone_ny_1_2", "n14").Before.Select(l => l.Speaker), Is.EqualTo(new[] { "lieutenant", "darius_brooks" }));
            Assert.That(Of("zone_ny_1_3", "n18").Before[0].Speaker, Is.EqualTo("crocodile"));
            Assert.That(Of("zone_ny_1_4", "n18").After.Select(l => l.Speaker), Is.EqualTo(new[] { "rayner", "stroke", "general", "general" }));
            foreach (var key in new[] { "narration.ny_1_4.n18.after.0", "narration.ny_1_3.n18.before.0", "narration.ny_1_2.n14.after.0" })
                Assert.That(lib.Texts.ContainsKey(key) && lib.Texts[key].Length > 10, Is.True, key);
        }

        [Test, Description("Vérifie que chaque case Mini-jeu des zones en assets lance une activité qui existe au catalogue — parce qu'une activité supprimée (la pêche) faisait planter la case : le mini-jeu ne se lançait pas et le plateau restait bloqué")]
        public void BakedCampaign_MiniGameCases_UseExistingActivities()
        {
            var content = N29.HeroesDawn.Content.Prototype.PrototypeContent.Build().WithZones(ZoneLibrary.Load().Zones);
            var missing = content.Zones
                .SelectMany(z => z.Nodes.Where(n => n.Activity.HasValue && !content.HasActivity(n.Activity.Value)).Select(n => z.Id.Value + "/" + n.Id.Value + " → " + n.Activity!.Value.Value))
                .ToList();
            Assert.That(missing, Is.Empty);
        }
    }
}
