#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Socle des pilotes : garde la zone de jeu, crée des images à la volée (toutes détruites à <see cref="Dispose"/>) et convertit les
    /// positions normalisées (0..1, y vers le bas) en positions d'ancrage dans la zone.
    /// </summary>
    public abstract class MiniGameDriverBase : IMiniGameDriver
    {
        readonly List<GameObject> _owned = new List<GameObject>();

        protected RectTransform Field { get; private set; } = null!;

        public virtual bool UsesStick => false;
        public abstract IMiniGame Game { get; }
        public abstract MiniGameHud Hud { get; }

        public void Build(RectTransform field)
        {
            Field = field;
            OnBuild();
        }

        protected abstract void OnBuild();
        public abstract void Tick(float dt, MiniGameInput input);

        public void Dispose()
        {
            foreach (var go in _owned) if (go != null) Object.Destroy(go);
            _owned.Clear();
        }

        /// <summary>Position d'ancrage (centre de la zone = origine) d'un point normalisé de la zone.</summary>
        protected Vector2 ToField(float x, float y)
        {
            var r = Field.rect;
            return new Vector2(r.xMin + x * r.width, r.yMax - y * r.height);
        }

        protected float FieldWidth => Field.rect.width;
        protected float FieldHeight => Field.rect.height;

        /// <summary>Image centrée dans la zone ; le pilote la déplace par <c>anchoredPosition</c>.</summary>
        protected Image Img(string name, Sprite? sprite, Color color, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.SetParent(Field, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            _owned.Add(go);
            return img;
        }

        /// <summary>Effet d'étincelles en un point de la zone (sans effet hors du mode jeu).</summary>
        protected void Sparks(Vector2 anchored, Color color, int count = 8)
        {
            var go = new GameObject("Fx", typeof(RectTransform)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.SetParent(Field, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchored;
            rt.sizeDelta = new Vector2(10, 10);
            Feedback.Fx.Sparks(rt, color, count, 120f, 14f);
            Object.Destroy(go, 1.2f);
        }

        protected void ShakeField() => Feedback.Fx.Shake(Field, 8f);
    }
}
