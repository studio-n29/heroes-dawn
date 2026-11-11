#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Polygone plein avec contour (quartiers de la carte). Les points sont normalisés (0..1, y vers le bas) et prennent la taille de l'élément ;
    /// le toucher n'est capté qu'à l'intérieur de la forme.
    /// </summary>
    public sealed class HdPolygon : VisualElement
    {
        Vector2[] _points = System.Array.Empty<Vector2>();

        public Color Fill { get; set; } = Color.clear;
        public Color Outline { get; set; } = Color.white;
        public float OutlineWidth { get; set; } = 4f;

        public HdPolygon() => generateVisualContent += Draw;

        public void SetPoints(Vector2[] points)
        {
            _points = points;
            MarkDirtyRepaint();
        }

        public void Repaint() => MarkDirtyRepaint();

        void Draw(MeshGenerationContext ctx)
        {
            if (_points.Length < 3) return;
            var rect = contentRect;
            var p = ctx.painter2D;
            p.BeginPath();
            for (int i = 0; i < _points.Length; i++)
            {
                var v = new Vector2(_points[i].x * rect.width, _points[i].y * rect.height);
                if (i == 0) p.MoveTo(v); else p.LineTo(v);
            }
            p.ClosePath();
            p.fillColor = Fill;
            p.Fill();
            p.strokeColor = Outline;
            p.lineWidth = OutlineWidth;
            p.lineJoin = LineJoin.Round;
            p.Stroke();
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            if (_points.Length < 3) return false;
            var rect = contentRect;
            float x = localPoint.x / Mathf.Max(1f, rect.width), y = localPoint.y / Mathf.Max(1f, rect.height);
            bool inside = false;
            for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
            {
                var a = _points[i]; var b = _points[j];
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }
    }
}
