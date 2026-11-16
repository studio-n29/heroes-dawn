#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using N29.HeroesDawn.Authoring;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Editor
{
    /// <summary>
    /// Génère les noms de cartes des fiches <c>Code/docs/catalogue/cartes-*.md</c> (colonnes Nom et Name) dans deux fichiers de <c>Presentation/Common</c> :
    /// <c>LocCollectionNames.cs</c> (français, uniquement les cartes absentes des fichiers écrits à la main) et <c>LocCollectionNamesEn.cs</c> (anglais, toutes les cartes).
    /// La génération (<see cref="Generate(IReadOnlyList{CatalogRow}, ISet{string})"/>) est une fonction pure sur des lignes de catalogue.
    /// En batch : <c>-executeMethod N29.HeroesDawn.Editor.GenerateCardNamesMenu.GenerateBatch</c> (code de sortie 1 en cas d'échec).
    /// </summary>
    public static class GenerateCardNamesMenu
    {
        public const string FrenchFileName = "LocCollectionNames.cs";
        public const string EnglishFileName = "LocCollectionNamesEn.cs";

        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));
        static string CommonFolder => Path.Combine(UnityEngine.Application.dataPath, "_Project", "Presentation", "Common");

        [MenuItem("Heroes Dawn/Contenu/Générer les noms de cartes")]
        public static void GenerateMenu()
        {
            try { Apply(); }
            catch (Exception e)
            {
                Debug.LogError("Génération des noms de cartes impossible : " + e.Message);
                EditorUtility.DisplayDialog("Noms de cartes", e.Message, "OK");
            }
        }

        /// <summary>Point d'entrée de <c>-executeMethod</c> : mêmes effets que le menu, sans boîte de dialogue, sortie en erreur en cas d'échec.</summary>
        public static void GenerateBatch()
        {
            try { Apply(); }
            catch (Exception e)
            {
                Debug.LogError("Génération des noms de cartes impossible : " + e.Message);
                EditorApplication.Exit(1);
            }
        }

        static void Apply()
        {
            var rows = ReadSheets();
            var (fr, en) = Generate(rows);
            var utf8 = new UTF8Encoding(false);
            File.WriteAllText(Path.Combine(CommonFolder, FrenchFileName), fr, utf8);
            File.WriteAllText(Path.Combine(CommonFolder, EnglishFileName), en, utf8);
            AssetDatabase.Refresh();
            Debug.Log($"Noms de cartes générés : {rows.Count} fiche(s), {FrenchFileName} et {EnglishFileName} écrits.");
        }

        /// <summary>Lignes de toutes les fiches <c>cartes-*.md</c>, dans l'ordre des fichiers puis des lignes.</summary>
        public static IReadOnlyList<CatalogRow> ReadSheets()
            => Directory.GetFiles(CatalogueFolder, "cartes-*.md").OrderBy(f => f, StringComparer.Ordinal)
                .SelectMany(f => CatalogImporter.ReadTable(File.ReadAllText(f, Encoding.UTF8))).ToList();

        /// <summary>Identifiants de cartes dont le nom français est écrit à la main dans <c>Presentation/Common/*.cs</c> (hors fichiers générés).</summary>
        public static ISet<string> HandWrittenFrenchIds()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var rx = new Regex(@"\[""skills\.([A-Za-z0-9_]+)""\]\s*=");
            foreach (var file in Directory.GetFiles(CommonFolder, "*.cs"))
            {
                string name = Path.GetFileName(file);
                if (name == FrenchFileName || name == EnglishFileName) continue;
                foreach (Match m in rx.Matches(File.ReadAllText(file, Encoding.UTF8))) ids.Add(m.Groups[1].Value);
            }
            return ids;
        }

        /// <summary>Texte des deux fichiers générés (français, anglais) ; le français ignore les cartes déjà écrites à la main.</summary>
        public static (string French, string English) Generate(IReadOnlyList<CatalogRow> rows) => Generate(rows, HandWrittenFrenchIds());

        /// <summary>Fonction pure : le français ne contient pas les identifiants de <paramref name="alreadyDefined"/>, l'anglais contient toutes les cartes.</summary>
        public static (string French, string English) Generate(IReadOnlyList<CatalogRow> rows, ISet<string> alreadyDefined)
        {
            var fr = new StringBuilder();
            Header(fr, "français");
            fr.Append("        static partial void ExtendCardNames(Dictionary<string, string> d)\n        {\n");
            foreach (var r in rows)
                if (!alreadyDefined.Contains(r.Id)) fr.Append("            d[\"skills.").Append(r.Id).Append("\"] = \"").Append(Escape(r.Nom)).Append("\";\n");
            fr.Append("        }\n    }\n}\n");

            var en = new StringBuilder();
            Header(en, "anglais");
            en.Append("        static Dictionary<string, string> WithCardNamesEnglish(Dictionary<string, string> d)\n        {\n");
            foreach (var r in rows)
                en.Append("            d[\"skills.").Append(r.Id).Append("\"] = \"").Append(Escape(r.Name)).Append("\";\n");
            en.Append("            return d;\n        }\n    }\n}\n");
            return (fr.ToString(), en.ToString());
        }

        static void Header(StringBuilder sb, string language)
        {
            sb.Append("// <auto-generated>\n");
            sb.Append("// Fichier GÉNÉRÉ par Heroes Dawn > Contenu > Générer les noms de cartes (GenerateCardNamesMenu) depuis Code/docs/catalogue/cartes-*.md (noms ").Append(language).Append(").\n");
            sb.Append("// Ne pas modifier à la main : corriger la fiche puis relancer la génération.\n");
            sb.Append("// </auto-generated>\n");
            sb.Append("#nullable enable\nusing System.Collections.Generic;\n\nnamespace N29.HeroesDawn.Presentation.Common\n{\n    public static partial class Loc\n    {\n");
        }

        static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
