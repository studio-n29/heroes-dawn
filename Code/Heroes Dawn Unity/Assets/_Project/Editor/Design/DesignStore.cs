#nullable enable
using System.IO;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>
    /// Fichiers de contenu des outils de design (Assets/_Project/Resources/Content/*.json), partagés entre les fenêtres :
    /// l'outil des agents voit les compétences en cours d'édition. Au premier usage, les fichiers sont créés depuis le contenu du code.
    /// </summary>
    public static class DesignStore
    {
        public const string Folder = "Assets/_Project/Resources/Content";

        public static ItemsFile Items { get; private set; } = new ItemsFile();
        public static SkillsFile Skills { get; private set; } = new SkillsFile();
        public static AgentsFile Agents { get; private set; } = new AgentsFile();
        public static bool ItemsDirty, SkillsDirty, AgentsDirty;
        static bool _loaded;

        static string PathOf(string name) => Folder + "/" + name + ".json";

        public static bool Exists(string name) => File.Exists(PathOf(name));

        /// <summary>Charge les trois fichiers (les crée depuis le code s'ils n'existent pas).</summary>
        public static void EnsureLoaded()
        {
            if (_loaded) return;
            if (!Exists(ContentDataLibrary.ItemsName) || !Exists(ContentDataLibrary.SkillsName) || !Exists(ContentDataLibrary.AgentsName)) ImportFromCode(overwrite: false);
            Reload();
        }

        public static void Reload()
        {
            Items = Read<ItemsFile>(ContentDataLibrary.ItemsName) ?? new ItemsFile();
            Skills = Read<SkillsFile>(ContentDataLibrary.SkillsName) ?? new SkillsFile();
            Agents = Read<AgentsFile>(ContentDataLibrary.AgentsName) ?? new AgentsFile();
            ItemsDirty = SkillsDirty = AgentsDirty = false;
            _loaded = true;
        }

        static T? Read<T>(string name) where T : class
            => Exists(name) ? JsonUtility.FromJson<T>(File.ReadAllText(PathOf(name))) : null;

        static void Write(string name, object data)
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathOf(name), JsonUtility.ToJson(data, true));
            AssetDatabase.ImportAsset(PathOf(name));
        }

        public static void SaveItems() { Write(ContentDataLibrary.ItemsName, Items); ItemsDirty = false; }
        public static void SaveSkills() { Write(ContentDataLibrary.SkillsName, Skills); SkillsDirty = false; }
        public static void SaveAgents() { Write(ContentDataLibrary.AgentsName, Agents); AgentsDirty = false; }

        /// <summary>Tous les identifiants de zones du jeu (code et zones créées dans l'outil de level design).</summary>
        public static string[] ZoneIds()
        {
            var content = PrototypeContent.Build();
            var fromAssets = ZoneLibrary.Load().Zones.Select(z => z.Id.Value);
            return content.Zones.Select(z => z.Id.Value).Concat(fromAssets).Distinct().OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
        }

        /// <summary>Remplit les fichiers avec le contenu du code (agents, compétences des agents, objets) et les noms de jeu actuels.</summary>
        public static void ImportFromCode(bool overwrite)
        {
            var c = PrototypeContent.Build();
            if (overwrite || !Exists(ContentDataLibrary.ItemsName))
            {
                var f = new ItemsFile();
                foreach (var i in c.Collection.Items) f.items.Add(ContentDataConverter.FromDef(i, Loc.T(i.NameKey)));
                Write(ContentDataLibrary.ItemsName, f);
            }
            if (overwrite || !Exists(ContentDataLibrary.SkillsName))
            {
                var f = new SkillsFile();
                foreach (var m in c.Members) foreach (var s in m.Skills) f.skills.Add(ContentDataConverter.FromDef(s, Loc.SkillName(s)));
                Write(ContentDataLibrary.SkillsName, f);
            }
            if (overwrite || !Exists(ContentDataLibrary.AgentsName))
            {
                var f = new AgentsFile();
                foreach (var m in c.Members) f.agents.Add(ContentDataConverter.FromDef(m, Loc.MemberName(m)));
                Write(ContentDataLibrary.AgentsName, f);
            }
            AssetDatabase.Refresh();
            _loaded = false;
        }

        [MenuItem("Heroes Dawn/Outils/Réinitialiser les données depuis le code")]
        static void ResetMenu()
        {
            if (!EditorUtility.DisplayDialog("Réinitialiser les données", "Les objets, compétences et agents reviennent au contenu du code. Les modifications faites dans les outils sont perdues.", "Réinitialiser", "Annuler")) return;
            ImportFromCode(overwrite: true);
            Reload();
        }

        public static void ImportBatch()
        {
            ImportFromCode(overwrite: false);
            Debug.Log("[Heroes Dawn] Données de design : " + Folder);
        }
    }
}
