#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Maps;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.EditorTools.LevelDesign
{
    /// <summary>
    /// Opérations de l'éditeur de plateaux sur un <see cref="ZoneAsset"/> (sans interface : testables) : placer, déplacer, relier, supprimer
    /// des cases, objectifs, fond de carte, validation. La fenêtre ne fait que les appeler (et enregistrer l'annulation).
    /// </summary>
    public static class LevelEditorModel
    {
        /// <summary>Résolution du fond généré : pixels par case de grille (moitié de la taille en jeu : style plat, peu de poids).</summary>
        public const int PxPerCellX = 85;
        public const int PxPerCellY = 125;
        /// <summary>Marge autour des cases, en cases de grille.</summary>
        public const int MarginCells = 1;

        public static string NextNodeId(ZoneAsset z)
        {
            for (int i = 0; ; i++)
            {
                string id = "n" + i.ToString("00");
                if (z.Node(id) == null) return id;
            }
        }

        public static NodeData? NodeAt(ZoneAsset z, int x, int y) => z.nodes.Find(n => n.x == x && n.y == y);

        /// <summary>Place une case ; refuse (null) une cellule occupée. La première case d'un plateau vide est l'entrée.</summary>
        public static NodeData? AddNode(ZoneAsset z, CaseType type, int x, int y)
        {
            if (NodeAt(z, x, y) != null) return null;
            if (z.nodes.Count == 0) type = CaseType.Entry;
            var n = new NodeData { id = NextNodeId(z), x = x, y = y };
            z.nodes.Add(n);
            SetType(z, n, type);
            return n;
        }

        /// <summary>Change le type d'une case. Il n'y a qu'une entrée : en désigner une rend l'ancienne vide ; les cases fixes par nature le deviennent.</summary>
        public static void SetType(ZoneAsset z, NodeData n, CaseType type)
        {
            if (type == CaseType.Entry)
            {
                foreach (var other in z.nodes) if (other != n && other.type == CaseType.Entry) other.type = CaseType.Empty;
                z.entry = n.id;
                n.isFixed = true;
            }
            else if (n.type == CaseType.Entry && z.entry == n.id) z.entry = "";
            n.type = type;
            if (type != CaseType.Combat) n.hasCombat = false;
            if (type != CaseType.MiniGame) n.activity = "";
        }

        public static bool MoveNode(ZoneAsset z, string id, int x, int y)
        {
            var n = z.Node(id);
            if (n == null) return false;
            var at = NodeAt(z, x, y);
            if (at != null && at != n) return false;
            int dx = x - n.x, dy = y - n.y;
            n.x = x; n.y = y;
            // Une courbe saisie suit l'extrémité déplacée.
            foreach (var l in z.links)
            {
                if (!l.hasCurve) continue;
                if (l.a == id) l.control1 += new UnityEngine.Vector2(dx, dy);
                if (l.b == id) l.control2 += new UnityEngine.Vector2(dx, dy);
            }
            return true;
        }

        /// <summary>Supprime une case, ses liens et les objectifs qui la visaient ; l'entrée supprimée n'est plus désignée.</summary>
        public static void DeleteNode(ZoneAsset z, string id)
        {
            z.nodes.RemoveAll(n => n.id == id);
            z.links.RemoveAll(l => l.a == id || l.b == id);
            z.objectives.RemoveAll(o => o.target == id);
            if (z.entry == id) z.entry = "";
        }

        public static bool HasLink(ZoneAsset z, string a, string b) => z.links.Any(l => (l.a == a && l.b == b) || (l.a == b && l.b == a));

        public static bool Connect(ZoneAsset z, string a, string b)
        {
            if (a == b || z.Node(a) == null || z.Node(b) == null || HasLink(z, a, b)) return false;
            z.links.Add(string.CompareOrdinal(a, b) < 0 ? new LinkData { a = a, b = b } : new LinkData { a = b, b = a });
            return true;
        }

        /// <summary>Chemin entre deux cases, quel que soit le sens.</summary>
        public static LinkData? FindLink(ZoneAsset z, string a, string b)
            => z.links.FirstOrDefault(l => (l.a == a && l.b == b) || (l.a == b && l.b == a));

        /// <summary>Courbe du chemin en unités de grille : celle saisie, sinon la courbe générée (la même qu'en jeu).</summary>
        public static BezierCurve CurveOf(ZoneAsset z, LinkData l)
        {
            var a = z.Node(l.a)!; var b = z.Node(l.b)!;
            var p0 = new PointF(a.x, a.y); var p3 = new PointF(b.x, b.y);
            if (!l.hasCurve) return LinkCurves.Auto(p0, p3, l.a, l.b);
            return new BezierCurve(p0, new PointF(l.control1.x, l.control1.y), new PointF(l.control2.x, l.control2.y), p3);
        }

        /// <summary>Fige la courbe générée en points de contrôle modifiables.</summary>
        public static void CustomizeCurve(ZoneAsset z, LinkData l)
        {
            if (l.hasCurve) return;
            var c = CurveOf(z, l);
            l.control1 = new UnityEngine.Vector2(c.C1.X, c.C1.Y);
            l.control2 = new UnityEngine.Vector2(c.C2.X, c.C2.Y);
            l.hasCurve = true;
        }

        public static bool Disconnect(ZoneAsset z, string a, string b)
            => z.links.RemoveAll(l => (l.a == a && l.b == b) || (l.a == b && l.b == a)) > 0;

        public static ObjectiveData AddObjective(ZoneAsset z)
        {
            int i = 1;
            while (z.objectives.Any(o => o.id == "obj_" + i)) i++;
            var o = new ObjectiveData { id = "obj_" + i, type = ObjectiveType.ReachCase, target = z.entry };
            z.objectives.Add(o);
            return o;
        }

        /// <summary>Rectangle de grille couvert (cases + marge) : minX, minY, maxX, maxY.</summary>
        public static (int MinX, int MinY, int MaxX, int MaxY) GridBounds(ZoneAsset z)
        {
            if (z.nodes.Count == 0) return (0, 0, 1 + 2 * MarginCells, 1 + 2 * MarginCells);
            return (z.nodes.Min(n => n.x) - MarginCells, z.nodes.Min(n => n.y) - MarginCells,
                    z.nodes.Max(n => n.x) + MarginCells, z.nodes.Max(n => n.y) + MarginCells);
        }

        /// <summary>Fond de carte de la zone : taille d'après la grille, zones peintes converties en pixels, graine, biome, météo.</summary>
        public static MapBackground BuildBackground(ZoneAsset z)
        {
            var b = GridBounds(z);
            int w = Math.Max(1, (b.MaxX - b.MinX) * PxPerCellX), h = Math.Max(1, (b.MaxY - b.MinY) * PxPerCellY);
            var regions = z.regions.Select(r => new MapRegion(r.biome,
                                                              (int)Math.Round((r.x - b.MinX) * PxPerCellX), (int)Math.Round((r.y - b.MinY) * PxPerCellY),
                                                              Math.Max(1, (int)Math.Round(r.radius * PxPerCellX)))).ToList();
            return MapBackgroundGenerator.Generate(w, h, (ulong)Math.Max(1, z.backgroundSeed), z.biome, z.weather, regions);
        }

        public static IReadOnlyList<ZoneIssue> Validate(ZoneAsset z, ContentCatalog content)
        {
            ZoneDef def;
            try { def = ZoneAssetConverter.ToDef(z); }
            catch (Exception e) { return new[] { new ZoneIssue(IssueSeverity.Error, "zone.convert", "Plateau illisible : " + e.Message) }; }
            return ZoneValidator.Validate(def, content);
        }

        /// <summary>Acte suivant dans la campagne : codes « acte.rang » (voir <see cref="CampaignAsset.CodeOf"/>).</summary>
        public static void AddCampaignZone(CampaignAsset c, ZoneAsset zone, int act)
        {
            if (c.entries.Any(e => e.zone == zone)) return;
            c.entries.Add(new CampaignEntry { zone = zone, act = act });
        }

        public static bool MoveCampaignEntry(CampaignAsset c, int index, int delta)
        {
            int to = index + delta;
            if (index < 0 || index >= c.entries.Count || to < 0 || to >= c.entries.Count) return false;
            var e = c.entries[index];
            c.entries.RemoveAt(index);
            c.entries.Insert(to, e);
            return true;
        }

        /// <summary>Prérequis par défaut : chaque zone demande la précédente du même acte (la première de chaque acte, la dernière de l'acte précédent).</summary>
        public static void ChainRequirements(CampaignAsset c)
        {
            for (int i = 0; i < c.entries.Count; i++)
            {
                var e = c.entries[i];
                e.requires.Clear();
                if (i == 0 || e.zone == null) continue;
                var prev = c.entries[i - 1];
                if (prev.zone != null) e.requires.Add(prev.zone.zoneId);
            }
        }
    }
}
