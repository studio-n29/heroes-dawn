#nullable enable
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Combat.Hud;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Socle des pilotes : garde la zone de jeu, crée des images à la volée (toutes retirées à <see cref="Dispose"/>) et convertit les
    /// positions normalisées (0..1, y vers le bas) en positions dans la zone (origine au centre, y vers le haut).
    /// </summary>
    public abstract class MiniGameDriverBase : IMiniGameDriver
    {
        protected FieldSurface Field { get; private set; } = null!;

        public virtual bool UsesStick => false;
        public abstract IMiniGame Game { get; }
        public abstract MiniGameHud Hud { get; }

        public void Build(FieldSurface field)
        {
            Field = field;
            OnBuild();
        }

        protected abstract void OnBuild();
        public abstract void Tick(float dt, MiniGameInput input);

        public void Dispose() => Field?.Clear();

        /// <summary>Position (centre de la zone = origine) d'un point normalisé de la zone.</summary>
        protected Vector2 ToField(float x, float y)
        {
            var r = Field.rect;
            return new Vector2(r.xMin + x * r.width, r.yMax - y * r.height);
        }

        protected float FieldWidth => Field.rect.width;
        protected float FieldHeight => Field.rect.height;

        /// <summary>Image centrée dans la zone ; le pilote la déplace par <c>rectTransform.anchoredPosition</c>.</summary>
        protected FieldSprite Img(string name, Sprite? sprite, Color color, Vector2 size) => Field.Add(name, sprite, color, size);

        /// <summary>Effet d'étincelles en un point de la zone.</summary>
        protected void Sparks(Vector2 anchored, Color color, int count = 8)
        {
            var s = Field.Size;
            HudAnim.Sparks(Field.Element, new Vector2(s.x * 0.5f + anchored.x, s.y * 0.5f - anchored.y), color, count, 120f, 14f);
        }

        protected void ShakeField() => HudAnim.Shake(Field.Element, 8f, 0.25f);
    }
}
