#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Polygone plein (quartier de la carte de la ville) : remplissage + contour. Les points sont en coordonnées normalisées
    /// du rectangle (0,0 = coin haut gauche, 1,1 = coin bas droit). Triangulé par découpe d'oreilles (polygone simple, concave accepté).
    /// Le toucher n'est valide qu'à l'intérieur du polygone (<see cref="ICanvasRaycastFilter"/>). Les UV portent la position en pixels / 64
    /// pour les hachures du matériau « verrouillé ».
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiPolygon : MaskableGraphic, ICanvasRaycastFilter
    {
        [SerializeField] Vector2[] points = System.Array.Empty<Vector2>();
        [SerializeField] Color outlineColor = Color.black;
        [SerializeField] float outlineWidth = 6f;

        public IReadOnlyList<Vector2> Points => points;
        public Color OutlineColor { get => outlineColor; set { outlineColor = value; SetVerticesDirty(); } }
        public float OutlineWidth { get => outlineWidth; set { outlineWidth = value; SetVerticesDirty(); } }

        public void SetPoints(Vector2[] normalized)
        {
            points = normalized;
            SetVerticesDirty();
        }

        /// <summary>Centre de gravité (coordonnées normalisées), pour poser une étiquette.</summary>
        public Vector2 Centroid()
        {
            if (points.Length == 0) return new Vector2(0.5f, 0.5f);
            float a = 0, cx = 0, cy = 0;
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var q = points[(i + 1) % points.Length];
                float cross = p.x * q.y - q.x * p.y;
                a += cross; cx += (p.x + q.x) * cross; cy += (p.y + q.y) * cross;
            }
            if (Mathf.Abs(a) < 1e-6f)
            {
                Vector2 s = Vector2.zero;
                foreach (var p in points) s += p;
                return s / points.Length;
            }
            return new Vector2(cx / (3f * a), cy / (3f * a));
        }

        Vector2 ToLocal(Vector2 n)
        {
            var r = rectTransform.rect;
            return new Vector2(r.xMin + n.x * r.width, r.yMax - n.y * r.height);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int n = points.Length;
            if (n < 3) return;
            var loc = new Vector2[n];
            for (int i = 0; i < n; i++) loc[i] = ToLocal(points[i]);

            // ---- Remplissage
            for (int i = 0; i < n; i++) vh.AddVert(loc[i], color, loc[i] / 64f);
            foreach (var tri in Triangulate(loc)) vh.AddTriangle(tri.a, tri.b, tri.c);

            // ---- Contour (un bandeau par arête, centré sur l'arête)
            if (outlineWidth <= 0f) return;
            float half = outlineWidth * 0.5f;
            for (int i = 0; i < n; i++)
            {
                var a = loc[i];
                var b = loc[(i + 1) % n];
                var d = (b - a);
                if (d.sqrMagnitude < 1e-4f) continue;
                var nrm = new Vector2(-d.y, d.x).normalized * half;
                int s = vh.currentVertCount;
                vh.AddVert(a + nrm, outlineColor, a / 64f);
                vh.AddVert(a - nrm, outlineColor, a / 64f);
                vh.AddVert(b - nrm, outlineColor, b / 64f);
                vh.AddVert(b + nrm, outlineColor, b / 64f);
                vh.AddTriangle(s, s + 1, s + 2);
                vh.AddTriangle(s, s + 2, s + 3);
            }
        }

        struct Tri { public int a, b, c; }

        /// <summary>Découpe d'oreilles ; renvoie des triangles d'indices (sens quelconque de l'entrée).</summary>
        static List<Tri> Triangulate(Vector2[] p)
        {
            var result = new List<Tri>();
            int n = p.Length;
            var idx = new List<int>(n);
            // Orientation : aire signée > 0 = anti-horaire.
            float area = 0;
            for (int i = 0; i < n; i++) { var u = p[i]; var v = p[(i + 1) % n]; area += u.x * v.y - v.x * u.y; }
            if (area > 0) for (int i = 0; i < n; i++) idx.Add(i);
            else for (int i = n - 1; i >= 0; i--) idx.Add(i);

            int guard = n * n;
            while (idx.Count > 3 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < idx.Count; i++)
                {
                    int ia = idx[(i + idx.Count - 1) % idx.Count], ib = idx[i], ic = idx[(i + 1) % idx.Count];
                    var a = p[ia]; var b = p[ib]; var c = p[ic];
                    if (Cross(a, b, c) <= 0f) continue; // sommet rentrant
                    bool inside = false;
                    for (int k = 0; k < idx.Count && !inside; k++)
                    {
                        int ik = idx[k];
                        if (ik == ia || ik == ib || ik == ic) continue;
                        if (PointInTriangle(p[ik], a, b, c)) inside = true;
                    }
                    if (inside) continue;
                    result.Add(new Tri { a = ia, b = ib, c = ic });
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break; // polygone dégénéré : on s'arrête avec ce qui est découpé
            }
            if (idx.Count == 3) result.Add(new Tri { a = idx[0], b = idx[1], c = idx[2] });
            return result;
        }

        static float Cross(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(a, b, p), d2 = Cross(b, c, p), d3 = Cross(c, a, p);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0;
            bool pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        /// <summary>Point (coordonnées normalisées) dans le polygone ? (lancer de rayon)</summary>
        public bool Contains(Vector2 normalized)
        {
            bool inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                var pi = points[i]; var pj = points[j];
                if ((pi.y > normalized.y) != (pj.y > normalized.y) &&
                    normalized.x < (pj.x - pi.x) * (normalized.y - pi.y) / (pj.y - pi.y) + pi.x)
                    inside = !inside;
            }
            return inside;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out var local)) return false;
            var r = rectTransform.rect;
            if (r.width <= 0f || r.height <= 0f) return false;
            return Contains(new Vector2((local.x - r.xMin) / r.width, (r.yMax - local.y) / r.height));
        }
    }
}
