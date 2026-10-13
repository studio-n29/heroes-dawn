#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Authoring
{
    // Données de contenu éditables par les outils de design (Heroes Dawn ▸ Outils) : fichiers JSON dans Resources/Content.
    // Classes simples et listes seulement (JsonUtility). Les champs publics sont dessinés tels quels par les fenêtres d'édition.

    [Serializable]
    public sealed class EffectRecord
    {
        public EffectType type;
        [Tooltip("Dégâts, soin, bouclier, recul, élan : pourcentage. Jauge : points de jauge (-100 = -10 %).")] public int value;
        [Tooltip("Statut appliqué (type « ApplyStatus »), ou statut du bouclier.")] public StatusType status;
        [Tooltip("Durée du statut en tours.")] public int duration;
        [Tooltip("Chance en % (100 = toujours).")] public int chance = 100;

        public EffectRecord Clone() => (EffectRecord)MemberwiseClone();
    }

    [Serializable]
    public sealed class SkillRecord
    {
        public string id = "";
        [Tooltip("Nom affiché en jeu.")] public string name = "";
        public SkillSlot slot;
        [Tooltip("Tours de recharge après usage.")] public int cooldown;
        public TargetRule target;
        [Tooltip("Premier effet = effet de base ; les suivants = effets secondaires.")] public List<EffectRecord> effects = new List<EffectRecord>();
    }

    [Serializable]
    public sealed class ItemRecord
    {
        public string id = "";
        [Tooltip("Nom affiché en jeu.")] public string name = "";
        public ItemRarity rarity;
        [Tooltip("Bonus de PV en %.")] public int hp;
        [Tooltip("Bonus d'attaque en %.")] public int atk;
        [Tooltip("Bonus de défense en %.")] public int def;
        [Tooltip("Bonus de vitesse en %.")] public int spd;
        [Tooltip("Effet spécial : jauge de départ, bouclier d'ouverture ou chances de critique.")] public ItemPerk perk;
        [Tooltip("Valeur de l'effet spécial (en % : jauge, bouclier, critique).")] public int perkValue;
        [Tooltip("Prix en Crédits (0 : pas en vente).")] public int price;
    }

    [Serializable]
    public sealed class AgentRecord
    {
        public string id = "";
        [Tooltip("Nom de code affiché en jeu.")] public string name = "";
        public Family family;
        public Role role;
        public Rarity rarity;
        public int hp = 1000;
        public int atk = 100;
        public int def = 100;
        public int spd = 100;
        [Tooltip("Boutique : en vente ; Arc : rejoint à la fin d'une zone ; Event : week-end ; Feat : prouesse.")] public UnlockKind unlockKind;
        [Tooltip("Zone concernée (identifiant). Boutique : zone à valider avant la mise en vente (vide : tout de suite).")] public string unlockZone = "";
        [Tooltip("Prouesse à accomplir (type Feat).")] public FeatKind feat;
        [Tooltip("Identifiants des compétences : emplacements A1, A2, A3.")] public List<string> skills = new List<string> { "", "", "" };
    }

    /// <summary>Texte du jeu modifié dans l outil de narration : il remplace le texte d origine de la clé.</summary>
    [Serializable]
    public sealed class TextRecord
    {
        [Tooltip("Clé du texte (ex. tutorial.step.Move.body).")] public string key = "";
        public string text = "";
    }

    [Serializable] public sealed class TextsFile { public List<TextRecord> texts = new List<TextRecord>(); }
    [Serializable] public sealed class ItemsFile { public List<ItemRecord> items = new List<ItemRecord>(); }
    [Serializable] public sealed class SkillsFile { public List<SkillRecord> skills = new List<SkillRecord>(); }
    [Serializable] public sealed class AgentsFile { public List<AgentRecord> agents = new List<AgentRecord>(); }
}
