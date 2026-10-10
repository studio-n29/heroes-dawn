using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Dialogues de narration saisis dans l'outil de level design (plan 2026-10-10, sous-projet I).</summary>
    public sealed class NarrationAuthoringTests
    {
        static ZoneAsset Zone()
        {
            var z = ScriptableObject.CreateInstance<ZoneAsset>();
            z.zoneId = "zone_test";
            z.entry = "n00";
            z.nodes.Add(new NodeData { id = "n00", type = CaseType.Entry, x = 0, y = 0 });
            var n = new NodeData { id = "n01", type = CaseType.Combat, x = 1, y = 0 };
            n.narrationBefore.Add(new NarrationLineData { speaker = "brick", text = "Halte-là !" });
            n.narrationAfter.Add(new NarrationLineData { speaker = "ancre", text = "C'est fini." });
            n.narrationAfter.Add(new NarrationLineData { speaker = "ancre", text = "Allons-y." });
            z.nodes.Add(n);
            z.links.Add(new LinkData { a = "n00", b = "n01" });
            return z;
        }

        [Test, Description("Vérifie que les dialogues saisis sur une case deviennent la narration de la case jouable, avec une clé de texte par réplique — parce que l'outil doit produire exactement ce que le plateau joue")]
        public void ToDef_BuildsTheNarrationWithKeys()
        {
            var z = Zone();
            var def = ZoneAssetConverter.ToDef(z);
            var node = def.Nodes.Single(n => n.Id.Value == "n01");
            Assert.That(node.Narration, Is.Not.Null);
            Assert.That(node.Narration!.Before.Single().Speaker, Is.EqualTo("brick"));
            Assert.That(node.Narration.Before.Single().TextKey, Is.EqualTo("narration.test.n01.before.0"));
            Assert.That(node.Narration.After.Select(l => l.TextKey), Is.EqualTo(new[] { "narration.test.n01.after.0", "narration.test.n01.after.1" }));
            Assert.That(def.Nodes.Single(n => n.Id.Value == "n00").Narration, Is.Null, "une case sans dialogue n'en a pas");
            Object.DestroyImmediate(z);
        }

        [Test, Description("Vérifie que les textes saisis sont proposés au jeu sous les clés des répliques — parce que la boîte de dialogue lit ses textes dans les textes du jeu")]
        public void TextsOf_CarriesEveryLine()
        {
            var z = Zone();
            var texts = ZoneAssetConverter.TextsOf(z).Values;
            Assert.That(texts["narration.test.n01.before.0"], Is.EqualTo("Halte-là !"));
            Assert.That(texts["narration.test.n01.after.1"], Is.EqualTo("Allons-y."));
            Object.DestroyImmediate(z);
        }
    }
}
