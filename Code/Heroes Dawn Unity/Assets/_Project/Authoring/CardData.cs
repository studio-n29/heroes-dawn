#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Authoring
{
    // Enregistrements du catalogue de cartes (Resources/Content/cards.json). Champs plats, listes et classes simples (JsonUtility) :
    // condition = hasCondition + condKind/condStatus/condValue ; modificateurs = liste de ModRecord ; ciblage fin = pick/pickStatus de la compétence.

    /// <summary>Palier d'une carte : niveau (3, 6 ou 10) et effets ajoutés à ce niveau.</summary>
    [Serializable]
    public sealed class TierRecord
    {
        public int level;
        public List<EffectRecord> effects = new List<EffectRecord>();
    }

    /// <summary>Carte du grimoire. Le nom de texte est dérivé de l'identifiant (« cards.&lt;id&gt;.name »).</summary>
    [Serializable]
    public sealed class CardRecord
    {
        public string id = "";
        public Family family;
        public int stars = 1;
        public CardNature nature;
        public List<Archetype> archetypes = new List<Archetype>();
        public CardRole role;
        public SkillRecord skill = new SkillRecord();
        public List<TierRecord> tiers = new List<TierRecord>();
    }

    [Serializable] public sealed class CardsFile { public List<CardRecord> cards = new List<CardRecord>(); }
}
