#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>
    /// Complète <c>LocIdentityTests</c> (textes localisés) pour les textes qui ne passent pas par le dictionnaire : attributs <c>text=</c> et
    /// <c>tooltip=</c> des UXML, littéraux de chaîne du code de jeu, fichiers texte des ressources. « Aube tactique » est un nom de travail de
    /// la direction artistique (spec refonte UI, règle d'identité) : jamais visible du joueur. Les commentaires et le code d'éditeur ne comptent pas.
    /// </summary>
    public sealed class VisibleTextIdentityTests
    {
        static string Project => Path.Combine(Application.dataPath, "_Project");
        const string Forbidden = "aube tactique";

        static IEnumerable<string> Files(string pattern, params string[] skipFolders)
            => Directory.EnumerateFiles(Project, pattern, SearchOption.AllDirectories)
                .Where(f => !skipFolders.Any(s => f.Replace('\\', '/').Contains("/" + s + "/")));

        [Test, Description("Vérifie qu'aucun texte affiché par un UXML (text, tooltip, label) ne contient « Aube tactique » — parce que les écrans UI Toolkit portent du texte en dur hors du dictionnaire localisé")]
        public void Uxml_DisplayedAttributes_NeverContainTheWorkingArtDirectionName()
        {
            var files = Files("*.uxml").ToList();
            Assert.That(files.Count, Is.GreaterThan(30), "les UXML du jeu sont bien parcourus");
            var attr = new Regex("(text|tooltip|label|value)=\"([^\"]*)\"", RegexOptions.IgnoreCase);
            var offenders = new List<string>();
            foreach (var f in files)
                foreach (Match m in attr.Matches(File.ReadAllText(f)))
                    if (m.Groups[2].Value.IndexOf(Forbidden, System.StringComparison.OrdinalIgnoreCase) >= 0) offenders.Add(Path.GetFileName(f) + " : " + m.Value);
            Assert.That(offenders, Is.Empty, string.Join("\n", offenders));
        }

        [Test, Description("Vérifie qu'aucun littéral de chaîne du code de jeu (hors commentaires et hors outils d'éditeur) ne contient « Aube tactique » — parce que les messages construits en code (toasts, bannières, libellés) contournent le dictionnaire")]
        public void GameCode_StringLiterals_NeverContainTheWorkingArtDirectionName()
        {
            var literal = new Regex("\"(?:[^\"\\\\\\r\\n]|\\\\.)*\"");
            var offenders = new List<string>();
            int scanned = 0;
            foreach (var f in Files("*.cs", "Editor"))
            {
                scanned++;
                int n = 0;
                foreach (var raw in File.ReadLines(f))
                {
                    n++;
                    var line = raw.TrimStart();
                    if (line.StartsWith("//") || line.StartsWith("*") || line.StartsWith("/*")) continue;
                    foreach (Match m in literal.Matches(raw))
                        if (m.Value.IndexOf(Forbidden, System.StringComparison.OrdinalIgnoreCase) >= 0) offenders.Add($"{Path.GetFileName(f)}:{n} {m.Value}");
                }
            }
            Assert.That(scanned, Is.GreaterThan(300), "le code de jeu est bien parcouru");
            Assert.That(offenders, Is.Empty, string.Join("\n", offenders));
        }

        [Test, Description("Vérifie qu'aucun fichier texte des ressources (JSON, texte, hors métadonnées de génération d'art) ne contient « Aube tactique » — parce que les contenus de campagne et de dialogue se chargent depuis ces fichiers")]
        public void ResourceTextFiles_NeverContainTheWorkingArtDirectionName()
        {
            var offenders = new List<string>();
            foreach (var pattern in new[] { "*.json", "*.txt", "*.csv" })
                foreach (var f in Files(pattern, "Art", "AddressableGroups"))
                    if (File.ReadAllText(f).IndexOf(Forbidden, System.StringComparison.OrdinalIgnoreCase) >= 0) offenders.Add(f.Substring(Project.Length));
            Assert.That(offenders, Is.Empty, string.Join("\n", offenders));
        }
    }
}
