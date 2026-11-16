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

        /// <summary>Erreurs du catalogue : liste vide, identifiant vide ou en double, valeur d'enum hors plage (JsonUtility lit un entier quelconque), condition ou modificateur sur un effet qui ne les lit pas, plafond sans échelle, échelle d'archétype sans archétype (ou archétype pour une autre échelle), réaction incohérente (absente, durée hors de 1 à 5, effet imbriqué interdit), Consomme ou Propage sans statut ou sans valeur ni maximum de tours, niveau de palier autre que 3, 6, 10 ou répété. Liste vide = valide.</summary>
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
                var levels = new HashSet<int>();
                foreach (var t in c.Tiers)
                {
                    if (!TierLevels.Contains(t.Level)) errors.Add(who + " : palier de niveau " + t.Level + " (seuls 3, 6 et 10 existent)");
                    else if (!levels.Add(t.Level)) errors.Add(who + " : palier de niveau " + t.Level + " en double");
                }
            }
            return errors;
        }

        static readonly HashSet<int> TierLevels = new HashSet<int> { 3, 6, 10 };

        /// <summary>Types d'effet dont le moteur lit la condition (Lifesteal, Bounce, Execute et Purge ne la lisent pas comme les autres).</summary>
        static readonly HashSet<EffectType> ConditionTypes = new HashSet<EffectType>
            { EffectType.Damage, EffectType.Heal, EffectType.Shield, EffectType.Advance, EffectType.PushBack, EffectType.GaugeDelta, EffectType.ApplyStatus };

        /// <summary>Types d'effet dont le moteur lit les modificateurs de valeur.</summary>
        static readonly HashSet<EffectType> ModTypes = new HashSet<EffectType>
            { EffectType.Damage, EffectType.Heal, EffectType.Shield, EffectType.Advance, EffectType.PushBack, EffectType.GaugeDelta };

        /// <summary>Types d'effet admis dans une réaction : un seul effet simple, de valeur fixe.</summary>
        static readonly HashSet<EffectType> ReactionEffectTypes = new HashSet<EffectType>
            { EffectType.Damage, EffectType.Heal, EffectType.Shield, EffectType.ApplyStatus, EffectType.PushBack, EffectType.Advance, EffectType.GaugeDelta, EffectType.Purge };

        static void CheckEffect(List<string> errors, string who, EffectDef e)
        {
            CheckEnum(errors, who, "type d'effet", e.Type);
            CheckEnum(errors, who, "statut", e.Status);
            if (e.When != null && !ConditionTypes.Contains(e.Type)) errors.Add(who + " : une condition sur " + e.Type + " n'est pas lue par le moteur");
            if (e.Mods.Count > 0 && !ModTypes.Contains(e.Type)) errors.Add(who + " : un modificateur sur " + e.Type + " n'est pas lu par le moteur");
            CheckCondition(errors, who, e.When);
            foreach (var m in e.Mods)
            {
                CheckCondition(errors, who, m.When);
                if (m.Per.HasValue) CheckEnum(errors, who, "échelle", m.Per.Value);
                else if (m.MaxCount != 0) errors.Add(who + " : un plafond (maxCount " + m.MaxCount + ") sur un modificateur sans échelle");
                if (m.PerArchetype.HasValue) CheckEnum(errors, who, "archétype d'échelle", m.PerArchetype.Value);
                if (m.Per == ScaleKind.EquippedArchetypeCards && !m.PerArchetype.HasValue) errors.Add(who + " : une échelle EquippedArchetypeCards sans archétype ne compterait rien");
                else if (m.PerArchetype.HasValue && m.Per != ScaleKind.EquippedArchetypeCards) errors.Add(who + " : un archétype (" + m.PerArchetype.Value + ") pour une échelle autre que EquippedArchetypeCards");
            }
            CheckReaction(errors, who, e);
            if (e.Type == EffectType.ConsumeDamage || e.Type == EffectType.ConsumeHeal)
            {
                if (e.Status == StatusType.None) errors.Add(who + " : " + e.Type + " sans statut à consommer");
                if (e.Value < 1) errors.Add(who + " : " + e.Type + " de valeur " + e.Value + " (1 au moins attendu)");
                if (e.Duration < 1) errors.Add(who + " : " + e.Type + " sans maximum de tours (durée " + e.Duration + ", 1 au moins attendu)");
            }
            if (e.Type == EffectType.Spread && e.Status == StatusType.None) errors.Add(who + " : Spread sans statut à propager");
        }

        static void CheckReaction(List<string> errors, string who, EffectDef e)
        {
            var r = e.Reaction;
            if (r == null)
            {
                if (e.Type == EffectType.ArmReaction) errors.Add(who + " : ArmReaction sans réaction");
                return;
            }
            if (e.Type != EffectType.ArmReaction) { errors.Add(who + " : une réaction sur " + e.Type + " n'est pas lue par le moteur (ArmReaction seulement)"); }
            CheckEnum(errors, who, "déclencheur de réaction", r.Trigger);
            CheckEnum(errors, who, "statut de réaction", r.Status);
            CheckEnum(errors, who, "cible de réaction", r.Target);
            CheckEnum(errors, who, "ciblage de réaction", r.Pick);
            CheckEnum(errors, who, "statut de ciblage de réaction", r.PickStatus);
            if ((r.Trigger == ReactionTrigger.EnemyReceivesStatus || r.Trigger == ReactionTrigger.AllyReceivesStatus) && r.Status == StatusType.None)
                errors.Add(who + " : le déclencheur " + r.Trigger + " exige un statut");
            if (r.Turns < 1 || r.Turns > 5) errors.Add(who + " : réaction de " + r.Turns + " tour(s) (1 à 5 attendus)");
            var n = r.Effect;
            CheckEnum(errors, who, "type d'effet de réaction", n.Type);
            CheckEnum(errors, who, "statut d'effet de réaction", n.Status);
            if (!ReactionEffectTypes.Contains(n.Type)) errors.Add(who + " : l'effet d'une réaction ne peut pas être " + n.Type);
            if (n.When != null || n.Mods.Count > 0 || n.Reaction != null) errors.Add(who + " : l'effet d'une réaction est de valeur fixe, sans condition, modificateur ni réaction");
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
