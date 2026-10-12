#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Texte à enregistrer dans les textes du jeu (clé → texte) pour une zone créée dans l'outil.</summary>
    public sealed class ZoneTexts
    {
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>();
    }

    /// <summary>Conversion entre les assets d'édition et les définitions du jeu (dans les deux sens, pour importer les zones du code).</summary>
    public static class ZoneAssetConverter
    {
        public static string NameKeyOf(ZoneAsset a) => string.IsNullOrEmpty(a.nameKey) ? "zones." + a.zoneId.Replace("zone_", "") + ".name" : a.nameKey;

        public static string ObjectiveKeyOf(ZoneAsset a, ObjectiveData o)
            => string.IsNullOrEmpty(o.nameKey) ? "objectives." + a.zoneId.Replace("zone_", "") + "." + o.id : o.nameKey;

        /// <summary>
        /// Zone jouable. Les surcharges de campagne (acte, code, prérequis) priment sur celles de l'asset quand elles sont données.
        /// </summary>
        public static ZoneDef ToDef(ZoneAsset a, int? act = null, string? code = null, IReadOnlyList<string>? requires = null)
        {
            var nodes = a.nodes.Select(n => new NodeDef(new CaseId(n.id), n.type, n.isFixed, n.rank, n.x, n.y,
                                                        string.IsNullOrEmpty(n.activity) ? (ActivityId?)null : new ActivityId(n.activity),
                                                        n.type == CaseType.Combat && n.hasCombat
                                                            ? new CombatSpec(n.enemyTable.Select(e => new EnemyId(e)).ToList(), n.rounds, n.enemiesMin, n.enemiesMax)
                                                            : null,
                                                        NarrationOf(a, n))).ToList();
            var links = a.links.Select(l => new LinkDef(new CaseId(l.a), new CaseId(l.b), l.cost, l.hasCurve ? new LinkCurve(l.control1.x, l.control1.y, l.control2.x, l.control2.y) : (LinkCurve?)null)).ToList();
            var objectives = a.objectives.Select(o => new ObjectiveDef(o.id, o.type, new CaseId(o.target), ObjectiveKeyOf(a, o))).ToList();
            var encounters = a.encounters.Select(e => new EncounterDef(e.rank, e.waves.Select(w => (IReadOnlyList<EnemyId>)w.enemies.Select(x => new EnemyId(x)).ToList()).ToList())).ToList();
            var req = (requires ?? a.requires).Select(r => new ZoneId(r)).ToList();
            ZoneBackground? bg = string.IsNullOrEmpty(a.backgroundKey) ? null
                : new ZoneBackground(a.backgroundKey, a.backgroundMinX, a.backgroundMinY, a.backgroundMaxX, a.backgroundMaxY);
            ZoneMapPoint? mp = a.hasMapPoint ? new ZoneMapPoint(a.mapPointPermille.x, a.mapPointPermille.y) : (ZoneMapPoint?)null;
            return new ZoneDef(new ZoneId(a.zoneId), NameKeyOf(a), new CaseId(a.entry), nodes, links, objectives, encounters,
                               code ?? a.code, act ?? a.act, a.borough, a.recommendedLevel, req, new ZoneEnvironment(a.biome, a.weather), bg, mp);
        }

        /// <summary>Clé de texte d'une réplique de narration saisie dans l'outil.</summary>
        public static string NarrationKeyOf(ZoneAsset a, NodeData n, NarrationPhase phase, int index)
            => "narration." + a.zoneId.Replace("zone_", "") + "." + n.id + "." + (phase == NarrationPhase.Before ? "before" : "after") + "." + index;

        static NarrationDef? NarrationOf(ZoneAsset a, NodeData n)
        {
            if (n.narrationBefore.Count == 0 && n.narrationAfter.Count == 0) return null;
            IReadOnlyList<NarrationLineDef> Lines(List<NarrationLineData> src, NarrationPhase phase)
                => src.Select((l, i) => new NarrationLineDef(l.speaker, NarrationKeyOf(a, n, phase, i))).ToList();
            return new NarrationDef(Lines(n.narrationBefore, NarrationPhase.Before), Lines(n.narrationAfter, NarrationPhase.After));
        }

        /// <summary>Textes saisis dans l'outil (nom, nom court, description, objectifs) ; vides = on garde les textes existants du jeu.</summary>
        public static ZoneTexts TextsOf(ZoneAsset a)
        {
            var t = new ZoneTexts();
            string key = NameKeyOf(a);
            if (!string.IsNullOrEmpty(a.displayName)) t.Values[key] = a.displayName;
            if (!string.IsNullOrEmpty(a.shortName)) t.Values[key.Replace(".name", ".short")] = a.shortName;
            if (!string.IsNullOrEmpty(a.description)) t.Values[key.Replace(".name", ".desc")] = a.description;
            foreach (var o in a.objectives) if (!string.IsNullOrEmpty(o.text)) t.Values[ObjectiveKeyOf(a, o)] = o.text;
            foreach (var n in a.nodes)
            {
                for (int i = 0; i < n.narrationBefore.Count; i++) t.Values[NarrationKeyOf(a, n, NarrationPhase.Before, i)] = n.narrationBefore[i].text;
                for (int i = 0; i < n.narrationAfter.Count; i++) t.Values[NarrationKeyOf(a, n, NarrationPhase.After, i)] = n.narrationAfter[i].text;
            }
            return t;
        }

        static List<NarrationLineData> LinesOf(IReadOnlyList<NarrationLineDef>? lines, System.Func<string, string>? textOf)
            => lines == null ? new List<NarrationLineData>()
               : lines.Select(l => new NarrationLineData { speaker = l.Speaker, text = textOf != null ? textOf(l.TextKey) : "" }).ToList();

        /// <summary>Remplit un asset d'après une zone du code (import) : même graphe, mêmes objectifs, mêmes rencontres.</summary>
        public static void FillFrom(ZoneAsset a, ZoneDef d, System.Func<string, string>? textOf = null)
        {
            a.zoneId = d.Id.Value;
            a.nameKey = d.NameKey;
            a.code = d.Code; a.act = d.Act; a.borough = d.Borough; a.recommendedLevel = d.RecommendedLevel;
            a.requires = d.Requires.Select(r => r.Value).ToList();
            a.biome = d.Environment.Biome; a.weather = d.Environment.Weather;
            a.entry = d.Entry.Value;
            a.nodes = d.Nodes.Select(n => new NodeData
            {
                id = n.Id.Value, type = n.Type, isFixed = n.IsFixed, rank = n.Rank, x = n.X, y = n.Y,
                activity = n.Activity.HasValue ? n.Activity.Value.Value : "",
                hasCombat = n.Combat != null,
                enemyTable = n.Combat != null ? n.Combat.EnemyTable.Select(e => e.Value).ToList() : new List<string>(),
                rounds = n.Combat?.Rounds ?? 1, enemiesMin = n.Combat?.EnemiesMin ?? 2, enemiesMax = n.Combat?.EnemiesMax ?? 3,
                narrationBefore = LinesOf(n.Narration?.Before, textOf),
                narrationAfter = LinesOf(n.Narration?.After, textOf),
            }).ToList();
            a.links = d.Links.Select(l => new LinkData
            {
                a = l.A.Value, b = l.B.Value, cost = l.Cost,
                hasCurve = l.Curve.HasValue,
                control1 = l.Curve.HasValue ? new UnityEngine.Vector2(l.Curve.Value.C1X, l.Curve.Value.C1Y) : UnityEngine.Vector2.zero,
                control2 = l.Curve.HasValue ? new UnityEngine.Vector2(l.Curve.Value.C2X, l.Curve.Value.C2Y) : UnityEngine.Vector2.zero,
            }).ToList();
            a.objectives = d.Objectives.Select(o => new ObjectiveData { id = o.Id, type = o.Type, target = o.Target.Value, nameKey = o.NameKey }).ToList();
            a.encounters = d.Encounters.Select(e => new EncounterData
            {
                rank = e.Rank,
                waves = e.Waves.Select(w => new WaveData { enemies = w.Select(x => x.Value).ToList() }).ToList(),
            }).ToList();
            a.hasMapPoint = d.MapPoint.HasValue;
            if (d.MapPoint.HasValue) a.mapPointPermille = new UnityEngine.Vector2Int(d.MapPoint.Value.XPermille, d.MapPoint.Value.YPermille);
            if (d.Background != null)
            {
                a.backgroundKey = d.Background.Key;
                a.backgroundMinX = d.Background.MinX; a.backgroundMinY = d.Background.MinY;
                a.backgroundMaxX = d.Background.MaxX; a.backgroundMaxY = d.Background.MaxY;
            }
        }
    }
}
