#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>
    /// Catalogue de cartes en données : lu depuis Resources/Content/cards.json ; fichier absent, vide ou illisible : repli sur les cartes du code
    /// (PrototypeCollection.LegacyCards, passé en paramètre car l'assemblage Authoring ne connaît pas le contenu du prototype).
    /// </summary>
    public static class CardDataLibrary
    {
        public const string ResourceName = ContentDataLibrary.ResourcesFolder + "/cards";

        /// <summary>Cartes du fichier Resources/Content/cards.json, sinon celles du repli.</summary>
        public static IReadOnlyList<SkillCardDef> Load(Func<IReadOnlyList<SkillCardDef>> fallback) => Load(ReadResource, fallback);

        /// <summary>
        /// Cartes de la source JSON donnée, sinon celles du repli : une source absente, vide, illisible, sans carte ou invalide (voir <see cref="Validate"/>)
        /// avertit (un avertissement par erreur) puis se replie.
        /// </summary>
        public static IReadOnlyList<SkillCardDef> Load(Func<string?> readJson, Func<IReadOnlyList<SkillCardDef>> fallback)
        {
            var json = readJson();
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("cards.json est absent ou vide : repli sur les cartes du code.");
                return fallback();
            }
            try
            {
                var cards = FromJson(json!);
                var errors = Validate(cards);
                if (errors.Count == 0) return cards;
                foreach (var error in errors) Debug.LogWarning("cards.json invalide : " + error);
                Debug.LogWarning("cards.json rejeté (" + errors.Count + " erreur(s)) : repli sur les cartes du code.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("cards.json illisible, repli sur les cartes du code : " + e.Message);
            }
            return fallback();
        }

        /// <summary>Erreurs du catalogue : liste vide, identifiant vide ou en double, valeur d'enum hors plage (JsonUtility lit un entier quelconque). Liste vide = valide.</summary>
        public static IReadOnlyList<string> Validate(IReadOnlyList<SkillCardDef> cards)
        {
            var errors = new List<string>();
            if (cards.Count == 0) errors.Add("aucune carte");
            var seen = new HashSet<string>();
            foreach (var c in cards)
            {
                var id = c.Id.Value;
                if (string.IsNullOrEmpty(id)) errors.Add("carte sans identifiant");
                else if (!seen.Add(id)) errors.Add(id + " : identifiant en double");
                var who = string.IsNullOrEmpty(id) ? "(sans id)" : id;
                CheckEnum(errors, who, "famille", c.Family);
                CheckEnum(errors, who, "nature", c.Nature);
                CheckEnum(errors, who, "rôle", c.Role);
                foreach (var a in c.Archetypes) CheckEnum(errors, who, "archétype", a);
                CheckEnum(errors, who, "emplacement", c.Skill.Slot);
                CheckEnum(errors, who, "cible", c.Skill.Target);
                CheckEnum(errors, who, "ciblage fin", c.Skill.Pick);
                CheckEnum(errors, who, "statut de ciblage", c.Skill.PickStatus);
                foreach (var e in c.Skill.Effects) CheckEffect(errors, who, e);
                foreach (var t in c.Tiers) foreach (var e in t.Effects) CheckEffect(errors, who, e);
            }
            return errors;
        }

        static void CheckEffect(List<string> errors, string who, EffectDef e)
        {
            CheckEnum(errors, who, "type d'effet", e.Type);
            CheckEnum(errors, who, "statut", e.Status);
            CheckCondition(errors, who, e.When);
            foreach (var m in e.Mods)
            {
                CheckCondition(errors, who, m.When);
                if (m.Per.HasValue) CheckEnum(errors, who, "échelle", m.Per.Value);
            }
        }

        static void CheckCondition(List<string> errors, string who, Condition? c)
        {
            if (c == null) return;
            CheckEnum(errors, who, "condition", c.Kind);
            CheckEnum(errors, who, "statut de condition", c.Status);
        }

        static void CheckEnum<T>(List<string> errors, string who, string what, T value) where T : struct, Enum
        {
            if (!Enum.IsDefined(typeof(T), value)) errors.Add(who + " : " + what + " hors plage (" + Convert.ToInt32(value) + ")");
        }

        static string? ReadResource()
        {
            var asset = Resources.Load<TextAsset>(ResourceName);
            return asset == null ? null : asset.text;
        }

        public static IReadOnlyList<SkillCardDef> FromJson(string json)
            => JsonUtility.FromJson<CardsFile>(json).cards.Select(ContentDataConverter.ToDef).ToList();

        public static string ToJson(IReadOnlyList<SkillCardDef> cards)
            => JsonUtility.ToJson(new CardsFile { cards = cards.Select(ContentDataConverter.FromDef).ToList() }, true);
    }
}
