#nullable enable
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Compétence sur la fiche : nom et description générée.</summary>
    public struct SkillLineData
    {
        public string Name;
        public string Description;
    }

    public enum SheetTileKind { Fixed, Card, Item, Empty, Locked }

    /// <summary>Case de la fiche (compétence ou objet) : titre, sous-titre, illustration éventuelle, couleur de cadre.</summary>
    public struct SheetTile
    {
        public string Title;
        public string Sub;
        public Sprite? Art;
        public Color Frame;
        public SheetTileKind Kind;
        /// <summary>Tuile grisée (ex. objet d'un type déjà porté) : elle reste touchable pour dire pourquoi.</summary>
        public bool Dim;
    }

    /// <summary>Palier de niveau : niveau, statistiques à ce niveau, atteint ou non.</summary>
    public struct TierLineData
    {
        public string Level;
        public string Stats;
        public bool Reached;
        public bool Current;
    }

    /// <summary>Choix proposé dans le panneau d'inventaire (objets ou cartes disponibles pour un emplacement).</summary>
    public struct PickerData
    {
        public string Title;
        public string Hint;
        public SheetTile[] Options;
        public bool CanRemove;
    }

    /// <summary>Tout ce qu'affiche la fiche d'un membre (M21).</summary>
    public struct MemberSheetData
    {
        public MemberCardData Card;
        public bool Owned;
        public string FatigueText;
        public float Fatigue01;
        public int Fatigue;
        public SkillLineData[] Skills;
        /// <summary>Synergies possibles pour la famille du membre : « Point faible · +8 % Attaque · avec Perception ».</summary>
        public string[] Synergies;
        public string PrimaryLabel;
        public bool PrimaryEnabled;
        /// <summary>Pourquoi l'action principale est grisée (« Il manque 400 CR ») ; vide sinon.</summary>
        public string PrimaryReason;
        public bool ShowArrows;
        /// <summary>Bouton Soigner (membre possédé) : libellé avec le prix, état et raison s'il est grisé.</summary>
        /// <summary>Bouton Dossier : affiché seulement si l'agent a du lore.</summary>
        public bool ShowDossierButton;
        public string DossierButtonLabel;
        public bool ShowHeal;
        public string HealLabel;
        public bool HealEnabled;
        public string HealReason;
        /// <summary>Bouton Equipement : libelle avec les emplacements remplis ; cache sur la fiche d un heros de la boutique.</summary>
        public string LoadoutLabel;
        public bool ShowLoadout;

        // ---- Maquette « fiche agent » ----
        public string Bio;
        public string LevelBarText;
        public float Level01;
        public string TiersTitle;
        public TierLineData[] Tiers;
        /// <summary>Quatre cases : compétences 1 et 2 (fixes), cartes 3 et 4.</summary>
        public SheetTile[] SkillTiles;
        /// <summary>Trois cases d'objets (la troisième est verrouillée).</summary>
        public SheetTile[] ItemTiles;
        public PortraitData[] Affinity;
        public string AffinityTitle;
        /// <summary>Les cases s'équipent sur la fiche d'un agent possédé seulement.</summary>
        public bool Equippable;
    }
}
