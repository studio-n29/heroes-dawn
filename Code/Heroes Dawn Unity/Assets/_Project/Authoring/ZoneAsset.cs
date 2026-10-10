#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    [Serializable]
    public sealed class NodeData
    {
        public string id = "";
        public CaseType type = CaseType.Empty;
        public bool isFixed;
        public CombatRank rank = CombatRank.Normal;
        /// <summary>Position en unités de grille (comme <see cref="NodeDef.X"/>) ; le Y croît vers le haut, comme à l'écran en jeu.</summary>
        public int x;
        public int y;
        /// <summary>Case Mini-jeu : identifiant d'activité (vide : réflexe).</summary>
        public string activity = "";
        /// <summary>Case Combat : table d'ennemis et manches (désactivé : rencontres de la zone par rang).</summary>
        public bool hasCombat;
        public List<string> enemyTable = new List<string>();
        public int rounds = 1;
        public int enemiesMin = 2;
        public int enemiesMax = 3;
    }

    [Serializable]
    public sealed class LinkData
    {
        public string a = "";
        public string b = "";
        /// <summary>PM dépensés pour emprunter ce chemin (au moins 1).</summary>
        public int cost = 1;
        /// <summary>Courbe saisie (points de contrôle en unités de grille, de a vers b) ; sinon courbe générée.</summary>
        public bool hasCurve;
        public Vector2 control1;
        public Vector2 control2;
    }

    [Serializable]
    public sealed class ObjectiveData
    {
        public string id = "";
        public ObjectiveType type = ObjectiveType.ReachCase;
        public string target = "";
        /// <summary>Clé de texte (vide : générée). Le texte est dans <see cref="text"/> ou dans les textes du jeu.</summary>
        public string nameKey = "";
        public string text = "";
    }

    [Serializable]
    public sealed class WaveData
    {
        public List<string> enemies = new List<string>();
    }

    /// <summary>Rencontre de repli de la zone (par rang) pour les cases Combat sans table propre.</summary>
    [Serializable]
    public sealed class EncounterData
    {
        public CombatRank rank = CombatRank.Easy;
        public List<WaveData> waves = new List<WaveData> { new WaveData() };
    }

    /// <summary>Zone peinte sur le fond de carte : un décor différent du biome de base (cercle, en unités de grille).</summary>
    [Serializable]
    public sealed class RegionData
    {
        public Biome biome = Biome.Forest;
        public float x;
        public float y;
        public float radius = 1.5f;
    }

    /// <summary>
    /// Un plateau (zone) de la campagne, édité par l'outil de level design (Heroes Dawn ▸ Level Design) : graphe des cases et des chemins,
    /// types de case, activités et combats, objectifs, décor et météo, fond de carte généré. Converti en <see cref="ZoneDef"/> au chargement.
    /// </summary>
    [CreateAssetMenu(menuName = "Heroes Dawn/Plateau (zone)", fileName = "Zone")]
    public sealed class ZoneAsset : ScriptableObject
    {
        public string zoneId = "zone_new";
        public string nameKey = "";
        public string displayName = "";
        public string shortName = "";
        [TextArea] public string description = "";
        public string code = "";
        public int act = 1;
        public string borough = "brooklyn";
        public int recommendedLevel = 1;
        public List<string> requires = new List<string>();
        public Biome biome = Biome.City;
        public Weather weather = Weather.Day;
        public string entry = "n00";
        public List<NodeData> nodes = new List<NodeData>();
        public List<LinkData> links = new List<LinkData>();
        public List<ObjectiveData> objectives = new List<ObjectiveData>();
        public List<EncounterData> encounters = new List<EncounterData>();
        public List<RegionData> regions = new List<RegionData>();

        /// <summary>Position sur la carte de la ville (0 à 1000, depuis le coin haut gauche).</summary>
        public bool hasMapPoint;
        public Vector2Int mapPointPermille = new Vector2Int(500, 500);

        /// <summary>Fond généré : nom de la ressource (Resources/Zones/&lt;nom&gt;) et rectangle de grille couvert ; vide tant que non généré.</summary>
        public string backgroundKey = "";
        public int backgroundMinX;
        public int backgroundMinY;
        public int backgroundMaxX;
        public int backgroundMaxY;
        public int backgroundSeed = 1;

        public NodeData? Node(string id) => nodes.Find(n => n.id == id);
    }
}
