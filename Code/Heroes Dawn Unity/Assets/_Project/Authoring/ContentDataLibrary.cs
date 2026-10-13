#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>
    /// Contenu créé dans les outils de design et chargé au démarrage (Resources/Content/items.json, skills.json, agents.json) :
    /// définitions et textes à enregistrer. Un fichier absent laisse le contenu du code ; un fichier présent le remplace en entier
    /// (supprimer une ligne dans l'outil supprime l'élément du jeu).
    /// </summary>
    public sealed class ContentDataLibrary
    {
        public const string ResourcesFolder = "Content";
        public const string ItemsName = "items", SkillsName = "skills", AgentsName = "agents";

        /// <summary>Objets du fichier, ou null s'il n'existe pas.</summary>
        public IReadOnlyList<ItemDef>? Items { get; }
        /// <summary>Agents du fichier, ou null s'il n'existe pas.</summary>
        public IReadOnlyList<MemberDef>? Members { get; }
        public IReadOnlyDictionary<string, string> Texts { get; }
        /// <summary>Éléments ignorés parce qu'invalides (message pour le journal).</summary>
        public IReadOnlyList<string> Skipped { get; }

        ContentDataLibrary(IReadOnlyList<ItemDef>? items, IReadOnlyList<MemberDef>? members, IReadOnlyDictionary<string, string> texts, IReadOnlyList<string> skipped)
        {
            Items = items; Members = members; Texts = texts; Skipped = skipped;
        }

        public static ContentDataLibrary Empty => new ContentDataLibrary(null, null, new Dictionary<string, string>(), new List<string>());

        public static ContentDataLibrary Load()
            => FromFiles(Read<ItemsFile>(ItemsName), Read<SkillsFile>(SkillsName), Read<AgentsFile>(AgentsName));

        static T? Read<T>(string name) where T : class
        {
            var asset = Resources.Load<TextAsset>(ResourcesFolder + "/" + name);
            return asset == null ? null : JsonUtility.FromJson<T>(asset.text);
        }

        public static ContentDataLibrary FromFiles(ItemsFile? items, SkillsFile? skills, AgentsFile? agents)
        {
            var texts = new Dictionary<string, string>();
            var skipped = new List<string>();
            List<ItemDef>? itemDefs = null;
            if (items != null)
            {
                itemDefs = new List<ItemDef>();
                foreach (var i in items.items)
                {
                    if (ContentDataValidator.Validate(i).Count > 0) { skipped.Add("objet ignoré : " + i.id); continue; }
                    itemDefs.Add(ContentDataConverter.ToDef(i));
                    texts["items." + i.id + ".name"] = i.name;
                }
            }

            List<MemberDef>? memberDefs = null;
            if (agents != null)
            {
                var skillDefs = new Dictionary<string, SkillDef>();
                if (skills != null)
                    foreach (var s in skills.skills)
                    {
                        if (ContentDataValidator.Validate(s).Count > 0) { skipped.Add("compétence ignorée : " + s.id); continue; }
                        skillDefs[s.id] = ContentDataConverter.ToDef(s);
                        texts["skills." + s.id] = s.name;
                    }
                memberDefs = new List<MemberDef>();
                foreach (var a in agents.agents)
                {
                    if (ContentDataValidator.Validate(a).Count > 0 || a.skills.Any(id => !skillDefs.ContainsKey(id))) { skipped.Add("agent ignoré : " + a.id); continue; }
                    memberDefs.Add(ContentDataConverter.ToDef(a, skillDefs));
                    texts["members." + a.id + ".name"] = a.name;
                }
            }
            return new ContentDataLibrary(itemDefs, memberDefs, texts, skipped);
        }
    }
}
