#nullable enable
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
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
    }
}
