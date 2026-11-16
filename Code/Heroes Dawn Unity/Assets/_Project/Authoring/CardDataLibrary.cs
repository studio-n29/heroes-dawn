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

        /// <summary>Cartes de la source JSON donnée (null ou vide : absente), sinon celles du repli ; une source illisible ou sans carte avertit puis se replie.</summary>
        public static IReadOnlyList<SkillCardDef> Load(Func<string?> readJson, Func<IReadOnlyList<SkillCardDef>> fallback)
        {
            var json = readJson();
            if (string.IsNullOrWhiteSpace(json)) return fallback();
            try
            {
                var cards = FromJson(json!);
                if (cards.Count > 0) return cards;
                Debug.LogWarning("cards.json ne contient aucune carte : repli sur les cartes du code.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("cards.json illisible, repli sur les cartes du code : " + e.Message);
            }
            return fallback();
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
