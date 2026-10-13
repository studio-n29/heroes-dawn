#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Contrôles des fiches avant sauvegarde : chaque message est une erreur qui empêche d'enregistrer.</summary>
    public static class ContentDataValidator
    {
        static readonly Regex IdPattern = new Regex("^[a-z][a-z0-9_]*$");

        static void CheckId(string id, List<string> errors)
        {
            if (string.IsNullOrEmpty(id)) errors.Add("identifiant vide");
            else if (!IdPattern.IsMatch(id)) errors.Add("identifiant « " + id + " » : minuscules, chiffres et _ seulement, en commençant par une lettre");
        }

        public static IReadOnlyList<string> Validate(ItemRecord i)
        {
            var e = new List<string>();
            CheckId(i.id, e);
            if (string.IsNullOrWhiteSpace(i.name)) e.Add("nom vide");
            foreach (var (label, v) in new[] { ("PV", i.hp), ("ATK", i.atk), ("DEF", i.def), ("VIT", i.spd) })
                if (v < -90 || v > 200) e.Add("bonus " + label + " hors de -90 à 200 %");
            if (i.price < 0) e.Add("prix négatif");
            if (i.perk != ItemPerk.None && i.perkValue <= 0) e.Add("effet spécial sans valeur");
            if (i.perk == ItemPerk.None && i.perkValue != 0) e.Add("valeur d'effet spécial sans effet");
            return e;
        }

        public static IReadOnlyList<string> Validate(EffectRecord x)
        {
            var e = new List<string>();
            switch (x.type)
            {
                case EffectType.Damage: case EffectType.Heal: case EffectType.PushBack: case EffectType.Advance:
                    if (x.value <= 0) e.Add(x.type + " : valeur positive attendue"); break;
                case EffectType.Shield:
                    if (x.value <= 0) e.Add("Shield : valeur positive attendue");
                    if (x.duration <= 0) e.Add("Shield : durée attendue"); break;
                case EffectType.ApplyStatus:
                    if (x.status == StatusType.None) e.Add("ApplyStatus : statut attendu");
                    if (x.duration <= 0) e.Add("ApplyStatus : durée attendue"); break;
                case EffectType.GaugeDelta:
                    if (x.value == 0) e.Add("GaugeDelta : valeur non nulle attendue"); break;
            }
            if (x.chance < 1 || x.chance > 100) e.Add("chance hors de 1 à 100 %");
            return e;
        }

        public static IReadOnlyList<string> Validate(SkillRecord s)
        {
            var e = new List<string>();
            CheckId(s.id, e);
            if (string.IsNullOrWhiteSpace(s.name)) e.Add("nom vide");
            if (s.cooldown < 0 || s.cooldown > 20) e.Add("recharge hors de 0 à 20 tours");
            if (s.effects.Count == 0) e.Add("au moins un effet");
            for (int i = 0; i < s.effects.Count; i++)
                foreach (var m in Validate(s.effects[i])) e.Add("effet " + (i + 1) + " : " + m);
            return e;
        }

        public static IReadOnlyList<string> Validate(AgentRecord a)
        {
            var e = new List<string>();
            CheckId(a.id, e);
            if (string.IsNullOrWhiteSpace(a.name)) e.Add("nom vide");
            if (a.hp <= 0 || a.atk <= 0 || a.def <= 0 || a.spd <= 0) e.Add("PV, ATK, DEF et VIT doivent être positifs");
            if (a.skills.Count != 3) e.Add("trois compétences exactement");
            if (a.unlockKind != UnlockKind.Shop && string.IsNullOrEmpty(a.unlockZone)) e.Add("zone de déblocage manquante");
            if (a.unlockKind == UnlockKind.Feat && a.feat == FeatKind.None) e.Add("prouesse manquante");
            if (a.unlockKind != UnlockKind.Feat && a.feat != FeatKind.None) e.Add("prouesse sans déblocage par prouesse");
            return e;
        }

        /// <summary>Contrôles de l'ensemble : identifiants uniques, compétences existantes et non partagées, zones connues.</summary>
        public static IReadOnlyList<string> ValidateAll(ItemsFile? items, SkillsFile? skills, AgentsFile? agents, IReadOnlyCollection<string>? zoneIds = null)
        {
            var e = new List<string>();
            if (items != null)
            {
                foreach (var g in items.items.GroupBy(i => i.id).Where(g => g.Key.Length > 0 && g.Count() > 1)) e.Add("objet « " + g.Key + " » en double");
                foreach (var i in items.items) foreach (var m in Validate(i)) e.Add("objet « " + i.id + " » : " + m);
            }
            var skillIds = new HashSet<string>();
            if (skills != null)
            {
                foreach (var g in skills.skills.GroupBy(s => s.id).Where(g => g.Key.Length > 0 && g.Count() > 1)) e.Add("compétence « " + g.Key + " » en double");
                foreach (var s in skills.skills) { skillIds.Add(s.id); foreach (var m in Validate(s)) e.Add("compétence « " + s.id + " » : " + m); }
            }
            if (agents != null)
            {
                foreach (var g in agents.agents.GroupBy(a => a.id).Where(g => g.Key.Length > 0 && g.Count() > 1)) e.Add("agent « " + g.Key + " » en double");
                foreach (var g in agents.agents.SelectMany(a => a.skills.Select(s => (a.id, s))).Where(t => t.s.Length > 0).GroupBy(t => t.s).Where(g => g.Select(t => t.id).Distinct().Count() > 1))
                    e.Add("compétence « " + g.Key + " » utilisée par plusieurs agents (" + string.Join(", ", g.Select(t => t.id).Distinct()) + ")");
                foreach (var a in agents.agents)
                {
                    foreach (var m in Validate(a)) e.Add("agent « " + a.id + " » : " + m);
                    foreach (var sid in a.skills)
                        if (skills != null && !skillIds.Contains(sid)) e.Add("agent « " + a.id + " » : compétence « " + sid + " » introuvable");
                    if (zoneIds != null && a.unlockZone.Length > 0 && !zoneIds.Contains(a.unlockZone)) e.Add("agent « " + a.id + " » : zone « " + a.unlockZone + " » inconnue");
                }
            }
            return e;
        }
    }
}
