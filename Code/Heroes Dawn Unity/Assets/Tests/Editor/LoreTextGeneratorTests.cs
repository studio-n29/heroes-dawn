using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using N29.HeroesDawn.EditorTools.Design;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Textes de lore des agents générés depuis 02_Lore_Narration/04_Agents_The_Dawn.md (lot G de la 0.14.0).</summary>
    public sealed class LoreTextGeneratorTests
    {
        const string Sample = @"# Les agents

## 3. Animation de matière inerte

- **Nom** : Brooks, Darius
- **Âge** : 41 ans
- **Sexe** : Homme
- **Origine** : Détroit, États-Unis → New York
- **Lien à la Supernova** : Un courant dans ses mains.
- **Origin Story** : Mécanicien « sauveur ».
- **Visuel** : Costaud, barbe grisonnante.
- **Pouvoir** : Anime les objets.

## 9. Inconnu

- **Nom** : Inconnu, Quelqu'un
- **Âge** : 30 ans
";

        static string Root => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../.."));

        [Test, Description("Vérifie que l'identifiant d'agent se déduit du nom (accents et ponctuation retirés, surnom entre « » prioritaire, Agent D sans virgule) — parce que les clés de lore sont indexées par identifiant d'agent")]
        public void AgentIdOf_Normalizes()
        {
            Assert.That(LoreTextGenerator.AgentIdOf("Brooks, Darius"), Is.EqualTo("darius_brooks"));
            Assert.That(LoreTextGenerator.AgentIdOf("Hallström, Astrid"), Is.EqualTo("astrid_hallstrom"));
            Assert.That(LoreTextGenerator.AgentIdOf("Lefèvre, Clément"), Is.EqualTo("clement_lefevre"));
            Assert.That(LoreTextGenerator.AgentIdOf("Park, Ji-Hoon"), Is.EqualTo("ji_hoon_park"));
            Assert.That(LoreTextGenerator.AgentIdOf("Okafor, Samuel « Sam »"), Is.EqualTo("sam_okafor"));
            Assert.That(LoreTextGenerator.AgentIdOf("Agent D"), Is.EqualTo("agent_d"));
        }

        [Test, Description("Vérifie que l'analyse ne garde que les agents demandés, avec leurs sept champs dans le bon ordre — parce que seuls les agents jouables ont un dossier")]
        public void Parse_ReadsTheFieldsOfTheRequestedAgents()
        {
            var lore = LoreTextGenerator.Parse(Sample, new HashSet<string> { "darius_brooks" });
            Assert.That(lore.Keys, Is.EqualTo(new[] { "darius_brooks" }));
            Assert.That(lore["darius_brooks"].Keys, Is.EqualTo(new[] { "age", "sex", "origin", "supernova", "story", "look", "power" }));
            Assert.That(lore["darius_brooks"]["age"], Is.EqualTo("41 ans"));
            Assert.That(lore["darius_brooks"]["story"], Is.EqualTo("Mécanicien « sauveur »."));
        }

        [Test, Description("Vérifie que le rendu échappe les guillemets, les barres inverses et les sauts de ligne — parce que le code généré doit compiler")]
        public void Render_EscapesAndOrders()
        {
            var lore = new Dictionary<string, Dictionary<string, string>>
            {
                ["a"] = new Dictionary<string, string> { ["age"] = "Il dit \"oui\" \\ non" },
            };
            string code = LoreTextGenerator.Render(lore);
            Assert.That(code, Does.Contain("d[\"lore.a.age\"] = \"Il dit \\\"oui\\\" \\\\ non\";"));
            Assert.That(code, Does.Contain("static partial void ExtendLore"));
        }

        static HashSet<string> PlayableAgentIds()
        {
            string json = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "_Project/Resources/Content/agents.json"));
            return new HashSet<string>(Regex.Matches(json, "\"id\"\\s*:\\s*\"([^\"]+)\"").Cast<Match>().Select(m => m.Groups[1].Value));
        }

        [Test, Description("Vérifie que LocLore.cs est identique à ce que génère le Markdown actuel — parce que le fichier généré doit suivre le lore sans retouche à la main")]
        public void LocLore_IsUpToDateWithTheMarkdown()
        {
            string markdown = File.ReadAllText(Path.Combine(Root, "02_Lore_Narration/04_Agents_The_Dawn.md"));
            string expected = LoreTextGenerator.Render(LoreTextGenerator.Parse(markdown, PlayableAgentIds())).Replace("\r\n", "\n");
            string actual = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "_Project/Presentation/Common/LocLore.cs")).Replace("\r\n", "\n");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Description("Vérifie que chacun des 32 agents jouables a ses textes (âge, sexe, origine, lien à la Supernova, histoire d'origine, apparence, pouvoir), sans texte vide — parce que la fiche affiche le dossier complet")]
        public void EveryPlayableAgent_HasItsDossier()
        {
            var ids = PlayableAgentIds();
            Assert.That(ids.Count, Is.EqualTo(32));
            foreach (var id in ids)
            {
                var fields = new List<string> { "age", "sex", "origin", "supernova", "story", "look", "power" };
                foreach (var f in fields)
                {
                    string key = "lore." + id + "." + f;
                    Assert.That(Loc.Has(key), Is.True, key);
                    Assert.That(Loc.T(key).Trim(), Is.Not.Empty, key);
                }
            }
        }
    }
}
