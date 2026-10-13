#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>
    /// Exporte le contenu du jeu (agents, compétences, quêtes, objets) en fichiers CSV pour les tableurs de conception.
    /// Menu : Heroes Dawn ▸ Équilibrage ▸ Exporter le contenu en CSV. Batchmode : -executeMethod ...ExportBatch -outDir &lt;dossier&gt;.
    /// </summary>
    public static class ContentCsvExport
    {
        static readonly ILocalization Loc = new LocLocalization();

        [MenuItem("Heroes Dawn/Équilibrage/Exporter le contenu en CSV")]
        public static void ExportMenu()
        {
            var dir = EditorUtility.SaveFolderPanel("Dossier d'export", "", "");
            if (!string.IsNullOrEmpty(dir)) Export(dir);
        }

        public static void ExportBatch()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-outDir");
            Export(i >= 0 && i + 1 < args.Length ? args[i + 1] : Path.Combine(UnityEngine.Application.dataPath, "../Data/export"));
        }

        public static void Export(string dir)
        {
            Directory.CreateDirectory(dir);
            var c = PrototypeContent.Build();
            Write(dir, "agents.csv", Agents(c));
            Write(dir, "competences.csv", Skills(c));
            Write(dir, "quetes.csv", Quests(c));
            Write(dir, "objets.csv", Items(c));
            Debug.Log("[Heroes Dawn] Export CSV : " + dir);
        }

        static void Write(string dir, string file, List<string[]> rows)
            => File.WriteAllText(Path.Combine(dir, file), string.Join("\n", rows.Select(r => string.Join(",", r.Select(Cell)))) + "\n", new UTF8Encoding(false));

        static string Cell(string s) => s.IndexOfAny(new[] { ',', '"', '\n' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

        static string T(string key) => global::N29.HeroesDawn.Presentation.Common.Loc.Has(key) ? global::N29.HeroesDawn.Presentation.Common.Loc.T(key) : "";

        static string Unlock(MemberDef m)
        {
            if (m.Unlock == null) return m.UnlockZone.HasValue ? "Boutique après " + m.UnlockZone.Value.Value : "Boutique";
            var u = m.Unlock;
            switch (u.Kind)
            {
                case UnlockKind.Arc: return "Arc : " + u.Zone?.Value;
                case UnlockKind.Event: return "Événement week-end : " + u.Zone?.Value;
                case UnlockKind.Feat: return "Exploit " + u.Feat + " : " + u.Zone?.Value;
                default: return "Boutique";
            }
        }

        static List<string[]> Agents(ContentCatalog c)
        {
            var rows = new List<string[]> { new[] { "id", "nom", "famille", "rôle", "rareté", "PV", "ATK", "DEF", "VIT", "déblocage", "compétence 1", "compétence 2", "compétence 3" } };
            foreach (var m in c.Members)
            {
                var skills = m.Skills.Select(s => s.Id.Value).ToList();
                rows.Add(new[]
                {
                    m.Id.Value, Loc.MemberName(m), Loc.FamilyName(m.Family), Loc.RoleName(m.Role), Loc.RarityName(m.Rarity),
                    m.BaseStats.Hp.ToString(), m.BaseStats.Atk.ToString(), m.BaseStats.Def.ToString(), m.BaseStats.Spd.ToString(), Unlock(m),
                    skills.Count > 0 ? skills[0] : "", skills.Count > 1 ? skills[1] : "", skills.Count > 2 ? skills[2] : "",
                });
            }
            return rows;
        }

        static string Effects(SkillDef s)
            => string.Join(" ; ", s.Effects.Select(e => e.Type + (e.Value != 0 ? " " + e.Value : "") + (e.Status != StatusType.None ? " " + e.Status + " " + e.Duration + "t" : "")
                                                       + (e.ChancePercent < 100 ? " (" + e.ChancePercent + " %)" : "")));

        static List<string[]> Skills(ContentCatalog c)
        {
            var rows = new List<string[]> { new[] { "id", "possesseur", "type de possesseur", "emplacement", "nom", "description", "recharge", "cible", "catégorie", "effets" } };
            void Add(string owner, string ownerType, SkillDef s)
                => rows.Add(new[] { s.Id.Value, owner, ownerType, s.Slot.ToString(), Loc.SkillName(s), Loc.DescribeSkill(s), s.Cooldown.ToString(), s.Target.ToString(), s.Category.ToString(), Effects(s) });
            foreach (var m in c.Members) foreach (var s in m.Skills) Add(m.Id.Value, "agent", s);
            foreach (var e in c.Enemies) foreach (var s in e.Skills) Add(e.Id.Value, "ennemi", s);
            foreach (var card in c.Collection.Cards) Add(card.Id.Value + " (" + Loc.FamilyName(card.Family) + ", " + card.Rarity + ")", "carte du grimoire", card.Skill);
            return rows;
        }

        static string Money(Reward r) => (r.Credits != 0 ? r.Credits + " CR " : "") + (r.Pr != 0 ? r.Pr + " PR " : "") + (r.Gems != 0 ? r.Gems + " gemmes " : "") + (r.PassXp != 0 ? r.PassXp + " XP pass" : "");

        static List<string[]> Quests(ContentCatalog c)
        {
            var rows = new List<string[]> { new[] { "type", "id", "titre", "description", "mesure ou type d'objectif", "objectif", "récompense", "déblocage ou zone" } };
            foreach (var z in c.Zones.Where(z => z.Borough == "brooklyn"))
            {
                rows.Add(new[] { "principale", z.Id.Value, Loc.T(z.NameKey), T("qg.fil." + z.Id.Value), "zone", "valider la zone", "récompenses de zone", z.Requires.Count == 0 ? "dès le départ" : "après " + string.Join(", ", z.Requires.Select(r => r.Value)) });
                foreach (var o in z.Objectives)
                    rows.Add(new[] { "objectif de zone", o.Id, Loc.T(o.NameKey), "", o.Type.ToString(), o.Target.Value, "", z.Id.Value });
            }
            foreach (var q in QuestLog.Catalog)
                rows.Add(new[] { "secondaire", q.Id, Loc.T(q.TitleKey), Loc.T(q.DescKey), q.Metric.ToString(), q.Target.ToString(), Money(q.Reward), q.UnlockZone.Length == 0 ? "dès le départ" : "après " + q.UnlockZone });
            foreach (var d in DailyMissions.Catalog)
                rows.Add(new[] { "mission du jour", d.Id, "", T("daily." + d.Id), d.Kind.ToString(), d.Target.ToString(), Money(d.Reward), "tirée au sort chaque jour" });
            return rows;
        }

        static List<string[]> Items(ContentCatalog c)
        {
            var rows = new List<string[]> { new[] { "id", "nom", "rareté", "PV %", "ATK %", "DEF %", "VIT %", "effet spécial", "valeur de l'effet", "prix (CR)" } };
            foreach (var i in c.Collection.Items)
                rows.Add(new[] { i.Id.Value, Loc.T(i.NameKey), i.Rarity.ToString(), i.HpPercent.ToString(), i.AtkPercent.ToString(), i.DefPercent.ToString(), i.SpdPercent.ToString(), i.Perk.ToString(), i.PerkValue.ToString(), i.Price.ToString() });
            return rows;
        }
    }
}
