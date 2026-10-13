#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>Outil des objets : ajouter, modifier, dupliquer ou supprimer un objet équipable.</summary>
    public sealed class ItemsDesignWindow : DesignWindowBase<ItemRecord>
    {
        [MenuItem("Heroes Dawn/Outils/Objets")]
        static void Open() => GetWindow<ItemsDesignWindow>("Outil objets");

        protected override string Noun => "objet";
        protected override List<ItemRecord> Records => DesignStore.Items.items;
        protected override string IdOf(ItemRecord r) => r.id;
        protected override string LabelOf(ItemRecord r) => r.name + "  ·  " + r.rarity;
        protected override ItemRecord NewRecord() => new ItemRecord { id = UniqueId("nouvel_objet"), name = "Nouvel objet", price = 300 };
        protected override void SetId(ItemRecord r, string id) { r.id = id; r.name += " (copie)"; }
        protected override bool Dirty { get => DesignStore.ItemsDirty; set => DesignStore.ItemsDirty = value; }
        protected override void SaveFile() => DesignStore.SaveItems();
        protected override IReadOnlyList<string> Problems() => ContentDataValidator.ValidateAll(DesignStore.Items, null, null);
        protected override bool DrawForm(ItemRecord r) => RecordDrawer.Draw(r);

        protected override void DrawPreview(ItemRecord r)
        {
            var parts = new List<string>();
            if (r.hp != 0) parts.Add((r.hp > 0 ? "+" : "") + r.hp + " % PV");
            if (r.atk != 0) parts.Add((r.atk > 0 ? "+" : "") + r.atk + " % attaque");
            if (r.def != 0) parts.Add((r.def > 0 ? "+" : "") + r.def + " % défense");
            if (r.spd != 0) parts.Add((r.spd > 0 ? "+" : "") + r.spd + " % vitesse");
            if (r.perk != ItemPerk.None) parts.Add(r.perk + " " + r.perkValue + " %");
            EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(r.name + " (" + r.rarity + ")\n" + (parts.Count == 0 ? "aucun bonus" : string.Join(" · ", parts)) + "\nPrix : " + (r.price == 0 ? "pas en vente" : r.price + " CR"), MessageType.None);
        }

        static string UniqueId(string baseId)
        {
            string id = baseId;
            for (int n = 2; DesignStore.Items.items.Any(i => i.id == id); n++) id = baseId + n;
            return id;
        }
    }

    /// <summary>Outil des compétences : ajouter, modifier, dupliquer ou supprimer une compétence d'agent et ses effets.</summary>
    public sealed class SkillsDesignWindow : DesignWindowBase<SkillRecord>
    {
        [MenuItem("Heroes Dawn/Outils/Compétences")]
        static void Open() => GetWindow<SkillsDesignWindow>("Outil compétences");

        protected override string Noun => "compétence";
        protected override List<SkillRecord> Records => DesignStore.Skills.skills;
        protected override string IdOf(SkillRecord r) => r.id;
        protected override string LabelOf(SkillRecord r) => r.name + "  ·  " + r.id;
        protected override SkillRecord NewRecord()
        {
            string id = "nouvelle_competence";
            for (int n = 2; DesignStore.Skills.skills.Any(s => s.id == id); n++) id = "nouvelle_competence" + n;
            return new SkillRecord { id = id, name = "Nouvelle compétence", target = TargetRule.SingleEnemy, effects = new List<EffectRecord> { new EffectRecord { type = EffectType.Damage, value = 100 } } };
        }
        protected override void SetId(SkillRecord r, string id) { r.id = id; r.name += " (copie)"; }
        protected override bool Dirty { get => DesignStore.SkillsDirty; set => DesignStore.SkillsDirty = value; }
        protected override void SaveFile() => DesignStore.SaveSkills();
        protected override IReadOnlyList<string> Problems() => ContentDataValidator.ValidateAll(null, DesignStore.Skills, DesignStore.Agents);
        protected override bool DrawForm(SkillRecord r) => RecordDrawer.Draw(r);

        protected override void DrawPreview(SkillRecord r)
        {
            EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
            var users = DesignStore.Agents.agents.Where(a => a.skills.Contains(r.id)).Select(a => a.name).ToList();
            string text = ContentDataValidator.Validate(r).Count == 0 ? SkillText.Describe(ContentDataConverter.ToDef(r)) : "(corrige les erreurs pour voir la description)";
            EditorGUILayout.HelpBox(r.name + " · " + r.slot + " · recharge " + r.cooldown + "\n" + text + "\nUtilisée par : " + (users.Count == 0 ? "aucun agent" : string.Join(", ", users)), MessageType.None);
        }
    }

    /// <summary>Outil des agents : ajouter, modifier, dupliquer ou supprimer un agent complet (identité, statistiques, déblocage, compétences).</summary>
    public sealed class AgentsDesignWindow : DesignWindowBase<AgentRecord>
    {
        static readonly string[] Starter = { "ancre", "givre", "echo", "seve" };
        static readonly HashSet<string> Hidden = new HashSet<string> { "skills" };
        string[]? _zones;

        [MenuItem("Heroes Dawn/Outils/Agents")]
        static void Open() => GetWindow<AgentsDesignWindow>("Outil agents");

        protected override string Noun => "agent";
        protected override List<AgentRecord> Records => DesignStore.Agents.agents;
        protected override string IdOf(AgentRecord r) => r.id;
        protected override string LabelOf(AgentRecord r) => r.name + "  ·  " + r.role + "  ·  " + r.rarity;
        protected override AgentRecord NewRecord()
        {
            string id = "nouvel_agent";
            for (int n = 2; DesignStore.Agents.agents.Any(a => a.id == id); n++) id = "nouvel_agent" + n;
            var a = new AgentRecord { id = id, name = "Nouvel agent", role = Role.Damage, family = Family.Matter, rarity = Rarity.Agent };
            ApplyBaseStats(a);
            return a;
        }
        protected override void SetId(AgentRecord r, string id) { r.id = id; r.name += " (copie)"; r.skills = new List<string> { "", "", "" }; }
        protected override bool Dirty { get => DesignStore.AgentsDirty; set => DesignStore.AgentsDirty = value; }
        protected override void SaveFile() { DesignStore.SaveAgents(); if (DesignStore.SkillsDirty) DesignStore.SaveSkills(); }

        protected override IReadOnlyList<string> Problems()
        {
            _zones ??= DesignStore.ZoneIds();
            return ContentDataValidator.ValidateAll(null, DesignStore.Skills, DesignStore.Agents, _zones);
        }

        protected override IReadOnlyList<string> Warnings()
            => Starter.Where(s => DesignStore.Agents.agents.All(a => a.id != s)).Select(s => "« " + s + " » fait partie de l'équipe de départ : sans lui, les nouveaux joueurs démarrent avec une équipe plus petite.").ToList();

        protected override bool DrawForm(AgentRecord a)
        {
            _zones ??= DesignStore.ZoneIds();
            bool changed = RecordDrawer.Draw(a, Hidden, (owner, f) => f.Name == "unlockZone" ? _zones : null);
            EditorGUILayout.Space(6);
            if (GUILayout.Button("Statistiques de départ du rôle et de la rareté")) { ApplyBaseStats(a); changed = true; }
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Compétences", EditorStyles.boldLabel);
            var ids = DesignStore.Skills.skills.Select(s => s.id).ToArray();
            string[] slots = { "A1 (légère)", "A2 (intermédiaire)", "A3 (lourde ou soutien)" };
            while (a.skills.Count < 3) a.skills.Add("");
            for (int i = 0; i < 3; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var all = new[] { "" }.Concat(ids).ToArray();
                int index = Mathf.Max(0, System.Array.IndexOf(all, a.skills[i]));
                int pick = EditorGUILayout.Popup(slots[i], index, all.Select(s => s.Length == 0 ? "(aucune)" : s).ToArray());
                if (all[pick] != a.skills[i]) { a.skills[i] = all[pick]; changed = true; }
                if (GUILayout.Button("Nouvelle", GUILayout.Width(70))) { a.skills[i] = NewSkillFor(a, i); changed = true; }
                EditorGUILayout.EndHorizontal();
                var rec = DesignStore.Skills.skills.FirstOrDefault(s => s.id == a.skills[i]);
                if (rec != null && ContentDataValidator.Validate(rec).Count == 0)
                    EditorGUILayout.LabelField("   " + rec.name + " : " + SkillText.Describe(ContentDataConverter.ToDef(rec)), EditorStyles.wordWrappedMiniLabel);
            }
            if (GUILayout.Button("Générer le kit du rôle (3 nouvelles compétences)")) { GenerateKit(a); changed = true; }
            return changed;
        }

        static void ApplyBaseStats(AgentRecord a)
        {
            var s = PrototypeContent.BaseStats(a.role, a.rarity);
            a.hp = s.Hp; a.atk = s.Atk; a.def = s.Def; a.spd = s.Spd;
        }

        /// <summary>Crée une compétence vide pour l'emplacement donné et l'ajoute au fichier des compétences.</summary>
        static string NewSkillFor(AgentRecord a, int slot)
        {
            string id = a.id + "_a" + (slot + 1);
            for (int n = 2; DesignStore.Skills.skills.Any(s => s.id == id); n++) id = a.id + "_a" + (slot + 1) + "_" + n;
            DesignStore.Skills.skills.Add(new SkillRecord
            {
                id = id, name = a.name + " · compétence " + (slot + 1), slot = (SkillSlot)slot, cooldown = slot == 0 ? 0 : 3 + slot,
                target = TargetRule.SingleEnemy, effects = new List<EffectRecord> { new EffectRecord { type = EffectType.Damage, value = 100 } },
            });
            DesignStore.SkillsDirty = true;
            return id;
        }

        /// <summary>Remplace les trois compétences de l'agent par le kit standard de son rôle et de sa famille.</summary>
        static void GenerateKit(AgentRecord a)
        {
            var entry = new PrototypeAgents.Entry(a.id, a.name, a.family, a.role, a.rarity);
            var kit = PrototypeAgents.Kit(entry);
            a.skills = new List<string>();
            for (int i = 0; i < kit.Count; i++)
            {
                var s = kit[i];
                var rec = ContentDataConverter.FromDef(s, a.name + " · " + (i == 0 ? "attaque" : i == 1 ? "technique" : "maîtrise"));
                var existing = DesignStore.Skills.skills.FindIndex(x => x.id == rec.id);
                if (existing >= 0) DesignStore.Skills.skills[existing] = rec; else DesignStore.Skills.skills.Add(rec);
                a.skills.Add(rec.id);
            }
            DesignStore.SkillsDirty = true;
        }

        protected override void OnDeleting(AgentRecord a)
        {
            var own = a.skills.Where(id => DesignStore.Agents.agents.Where(x => x != a).All(x => !x.skills.Contains(id))).ToList();
            if (own.Count > 0 && EditorUtility.DisplayDialog("Compétences de l'agent", "Supprimer aussi ses " + own.Count + " compétences (" + string.Join(", ", own) + ") ?", "Les supprimer", "Les garder"))
            {
                DesignStore.Skills.skills.RemoveAll(s => own.Contains(s.id));
                DesignStore.SkillsDirty = true;
            }
        }

        protected override void DrawPreview(AgentRecord a)
        {
            EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
            string unlock = a.unlockKind == UnlockKind.Shop ? (a.unlockZone.Length == 0 ? "en vente dès le départ" : "en vente après la zone " + a.unlockZone)
                          : a.unlockKind == UnlockKind.Arc ? "rejoint à la fin de la zone " + a.unlockZone
                          : a.unlockKind == UnlockKind.Event ? "événement du week-end, zone " + a.unlockZone
                          : "prouesse " + a.feat + ", zone " + a.unlockZone;
            EditorGUILayout.HelpBox(a.name + " · " + a.family + " · " + a.role + " · " + a.rarity + "\nPV " + a.hp + " · ATK " + a.atk + " · DEF " + a.def + " · VIT " + a.spd + "\n" + unlock, MessageType.None);
        }
    }
}
