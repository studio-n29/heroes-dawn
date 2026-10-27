#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>
    /// Génère <c>Presentation/Common/LocLore.cs</c> depuis <c>02_Lore_Narration/04_Agents_The_Dawn.md</c> : un texte par champ du dossier
    /// d'un agent jouable (clé <c>lore.&lt;idAgent&gt;.&lt;champ&gt;</c>). Le Markdown reste la source unique ; le fichier généré est commité.
    /// Menu : Heroes Dawn ▸ Setup ▸ Générer les textes de lore.
    /// </summary>
    public static class LoreTextGenerator
    {
        /// <summary>Champ de la fiche Markdown → suffixe de clé, dans l'ordre d'affichage.</summary>
        static readonly (string Markdown, string Key)[] Fields =
        {
            ("Âge", "age"), ("Sexe", "sex"), ("Origine", "origin"), ("Lien à la Supernova", "supernova"),
            ("Origin Story", "story"), ("Visuel", "look"), ("Pouvoir", "power"),
        };

        const string MarkdownPath = "02_Lore_Narration/04_Agents_The_Dawn.md";
        const string AgentsJson = "Assets/_Project/Resources/Content/agents.json";
        const string OutputPath = "Assets/_Project/Presentation/Common/LocLore.cs";

        /// <summary>« Brooks, Darius » → <c>darius_brooks</c> ; un surnom entre « » remplace le prénom ; « Agent D » → <c>agent_d</c>.</summary>
        public static string AgentIdOf(string name)
        {
            string n = name.Trim();
            int comma = n.IndexOf(',');
            string last = comma < 0 ? n : n.Substring(0, comma);
            string first = comma < 0 ? "" : n.Substring(comma + 1);
            var nick = Regex.Match(first, "«\\s*([^»]+?)\\s*»");
            if (nick.Success) first = nick.Groups[1].Value;
            return Slug((first + " " + last).Trim());
        }

        static string Slug(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '_');
            }
            return Regex.Replace(sb.ToString(), "_+", "_").Trim('_');
        }

        /// <summary>Champs de chaque agent présent dans <paramref name="agentIds"/>, dans l'ordre du Markdown puis de <see cref="Fields"/>.</summary>
        public static IReadOnlyDictionary<string, Dictionary<string, string>> Parse(string markdown, ISet<string> agentIds)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            var order = new List<string>();
            var field = new Regex("^- \\*\\*(.+?)\\*\\* ?: ?(.*)$");
            var sections = Regex.Split(markdown.Replace("\r\n", "\n"), "\n(?=#{2,3} )");
            foreach (var section in sections)
            {
                var values = new Dictionary<string, string>();
                foreach (var line in section.Split('\n'))
                {
                    var m = field.Match(line.Trim());
                    if (m.Success) values[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Replace("**", "").Trim();
                }
                // Agent D et Agent M n'ont pas de « Nom » mais un « Nom de code » (leur nom civil est un autre champ).
                if (!values.TryGetValue("Nom", out var name) && !values.TryGetValue("Nom de code", out name)) continue;
                string id = AgentIdOf(name);
                if (!agentIds.Contains(id)) continue;
                var texts = new Dictionary<string, string>();
                foreach (var (markdownField, key) in Fields)
                    if (values.TryGetValue(markdownField, out var text) && text.Length > 0) texts[key] = text;
                result[id] = texts;
                order.Add(id);
            }
            // Dictionary conserve l'ordre d'insertion tant qu'on ne supprime rien : on le fige quand même.
            return order.ToDictionary(id => id, id => result[id]);
        }

        static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");

        /// <summary>Code de <c>Loc.ExtendLore</c> (fins de ligne LF).</summary>
        public static string Render(IReadOnlyDictionary<string, Dictionary<string, string>> lore)
        {
            var sb = new StringBuilder();
            sb.Append("#nullable enable\n");
            sb.Append("using System.Collections.Generic;\n\n");
            sb.Append("namespace N29.HeroesDawn.Presentation.Common\n{\n");
            sb.Append("    /// <summary>Dossiers de lore des agents jouables. Fichier généré depuis 02_Lore_Narration/04_Agents_The_Dawn.md (menu Heroes Dawn ▸ Setup ▸ Générer les textes de lore) : ne pas modifier à la main.</summary>\n");
            sb.Append("    public static partial class Loc\n    {\n");
            sb.Append("        static partial void ExtendLore(Dictionary<string, string> d)\n        {\n");
            foreach (var agent in lore)
                foreach (var (_, key) in Fields)
                    if (agent.Value.TryGetValue(key, out var text))
                        sb.Append("            d[\"lore.").Append(agent.Key).Append('.').Append(key).Append("\"] = \"").Append(Escape(text)).Append("\";\n");
            sb.Append("        }\n    }\n}\n");
            return sb.ToString();
        }

        static string RepoRoot => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../.."));

        [MenuItem("Heroes Dawn/Setup/Générer les textes de lore")]
        public static void Run()
        {
            string markdown = File.ReadAllText(Path.Combine(RepoRoot, MarkdownPath));
            string json = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "..", AgentsJson));
            var ids = new HashSet<string>(Regex.Matches(json, "\"id\"\\s*:\\s*\"([^\"]+)\"").Cast<Match>().Select(m => m.Groups[1].Value));
            var lore = Parse(markdown, ids);
            string path = Path.Combine(UnityEngine.Application.dataPath, "..", OutputPath);
            string old = File.Exists(path) ? File.ReadAllText(path) : "";
            string code = Render(lore);
            File.WriteAllText(path, old.Contains("\r\n") ? code.Replace("\n", "\r\n") : code, new UTF8Encoding(false));
            AssetDatabase.Refresh();
            Debug.Log($"[Heroes Dawn] Textes de lore générés : {lore.Count} agents.");
        }
    }
}
