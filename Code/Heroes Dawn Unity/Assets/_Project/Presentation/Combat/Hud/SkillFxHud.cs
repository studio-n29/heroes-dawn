#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>Animations de compétence jouée (UI Toolkit) : projectile qui traverse le terrain, éclat propre à sa famille, anneaux, étincelles.</summary>
    public static class SkillFxHud
    {
        public const float FlightSeconds = 0.38f;

        /// <summary>Sprites de famille d'effet (FX/fx_famille_projectile et _impact), clés construites une fois ; null si le fichier manque (l'ancienne forme sert alors de repli).</summary>
        static readonly string[] ProjectileKeys = BuildKeys("projectile"), ImpactKeys = BuildKeys("impact");

        static string[] BuildKeys(string suffix)
        {
            var kinds = (SkillFxKind[])Enum.GetValues(typeof(SkillFxKind));
            var keys = new string[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
                keys[i] = "Missing20261116/FX/fx_" + kinds[i].ToString().ToLowerInvariant() + "_" + suffix;
            return keys;
        }

        static Sprite? FxSprite(SkillFxKind kind, bool projectile) => TacticalArt.Load((projectile ? ProjectileKeys : ImpactKeys)[(int)kind]);

        static VisualElement Sprite(VisualElement layer, Sprite sprite, Color color, float size)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.style.position = Position.Absolute;
            e.style.width = size; e.style.height = size;
            e.style.backgroundImage = new StyleBackground(Background.FromSprite(sprite));
            e.style.unityBackgroundImageTintColor = color;
            layer.Add(e);
            return e;
        }

        static void Place(VisualElement layer, VisualElement e, Vector2 center, float size)
        {
            e.style.left = center.x - size * 0.5f; e.style.top = center.y - size * 0.5f;
        }

        /// <summary>Forme qui traverse le terrain de l'attaquant à la cible, en arc ; sa forme suit la famille (éclair en trait, glace en éclat…).</summary>
        public static void Projectile(VisualElement layer, Vector2 from, Vector2 to, SkillFxKind kind, Color color, Action arrived)
        {
            // Image de la famille (orientée vers la droite, pivot au centre) ; à défaut l'ancienne forme teintée.
            var art = FxSprite(kind, true);
            float w = art != null ? 120f : kind == SkillFxKind.Lightning ? 120f : 56f, h = art != null ? 120f : kind == SkillFxKind.Lightning ? 26f : 56f;
            var sprite = kind == SkillFxKind.Ice ? Sprites.Diamond : kind == SkillFxKind.Lightning ? Sprites.Shine : kind == SkillFxKind.Chains || kind == SkillFxKind.Mark ? Sprites.Ring : Sprites.Circle;
            var e = Sprite(layer, art ?? sprite, art != null ? Color.white : color, w);
            e.style.height = h;
            var dir = to - from;
            float arc = kind == SkillFxKind.Lightning ? 0f : Mathf.Min(120f, dir.magnitude * 0.18f);
            HudAnim.SetRotate(e, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            HudAnim.Run(e, FlightSeconds, k =>
            {
                var p = Vector2.LerpUnclamped(from, to, k) + Vector2.down * Mathf.Sin(k * Mathf.PI) * arc;
                e.style.left = p.x - w * 0.5f; e.style.top = p.y - h * 0.5f;
                HudAnim.SetScale(e, kind == SkillFxKind.Mark ? Mathf.Lerp(1.8f, 0.9f, k) : Mathf.Lerp(0.8f, 1.2f, k));
                if (kind == SkillFxKind.Fire || kind == SkillFxKind.Acid) HudAnim.Sparks(layer, e, color, 1, 40f, 12f); // traînée
            }, () => { e.RemoveFromHierarchy(); arrived(); }, HudAnim.EaseOutCubic);
        }

        /// <summary>Éclat à l'arrivée : étincelles et anneau, réglés par famille.</summary>
        public static void Burst(VisualElement layer, VisualElement target, SkillFxKind kind, Color color)
        {
            // Image d'impact de la famille (fondu par l'opacité : pas de matériau additif en USS) ; à défaut les anciennes formes.
            var art = FxSprite(kind, false);
            if (art != null)
            {
                switch (kind)
                {
                    case SkillFxKind.Ice: PopArt(layer, target, art, 0.3f, 1.4f, 0.5f); break;
                    case SkillFxKind.Acid: case SkillFxKind.Heal: PopArt(layer, target, art, 0.5f, 1.6f, 0.55f); break;
                    case SkillFxKind.Shield: case SkillFxKind.Chains: case SkillFxKind.Time: PopArt(layer, target, art, 0.4f, 1.3f, 0.5f); break;
                    case SkillFxKind.Haste: PopArt(layer, target, art, 0.6f, 1.8f, 0.4f); break;
                    default: PopArt(layer, target, art, 0.4f, 1.5f, 0.45f); break;
                }
            }
            else
            switch (kind)
            {
                case SkillFxKind.Fire: case SkillFxKind.Lightning: case SkillFxKind.Impact: case SkillFxKind.Mark: Pop(layer, target, Sprites.Burst, color, 0.4f, 1.5f, 0.45f); break;
                case SkillFxKind.Ice: Pop(layer, target, Sprites.Diamond, color, 0.3f, 1.4f, 0.5f); break;
                case SkillFxKind.Acid: case SkillFxKind.Heal: Pop(layer, target, Sprites.Cloud, color, 0.5f, 1.6f, 0.55f); break;
                case SkillFxKind.Shield: case SkillFxKind.Chains: case SkillFxKind.Time: Pop(layer, target, Sprites.Circle, Theme.WithAlpha(color, 0.5f), 0.4f, 1.3f, 0.5f); break;
                case SkillFxKind.Haste: Pop(layer, target, Sprites.Shine, color, 0.6f, 1.8f, 0.4f); break;
            }
            switch (kind)
            {
                case SkillFxKind.Fire: HudAnim.Sparks(layer, target, color, 16, 150f, 20f); HudAnim.Sparks(layer, target, new Color(1f, 0.25f, 0.1f), 8, 90f, 26f); break;
                case SkillFxKind.Ice: HudAnim.Sparks(layer, target, color, 12, 130f, 24f); Ring(layer, target, color, 0.6f, 1.5f); break;
                case SkillFxKind.Lightning: HudAnim.Sparks(layer, target, color, 10, 190f, 9f); HudAnim.Flash(target); break;
                case SkillFxKind.Acid: HudAnim.Sparks(layer, target, color, 12, 95f, 22f); break;
                case SkillFxKind.Chains: Ring(layer, target, color, 1.6f, 0.9f); Ring(layer, target, color, 1.2f, 0.7f); HudAnim.Sparks(layer, target, color, 6, 80f, 14f); break;
                case SkillFxKind.Mark: Ring(layer, target, color, 1.9f, 0.8f); break;
                case SkillFxKind.Heal: HudAnim.Sparks(layer, target, color, 12, 110f, 16f); Ring(layer, target, color, 0.5f, 1.3f); break;
                case SkillFxKind.Shield: Ring(layer, target, color, 0.7f, 1.5f); Ring(layer, target, color, 0.5f, 1.2f); break;
                case SkillFxKind.Haste: HudAnim.Sparks(layer, target, color, 10, 160f, 10f); break;
                case SkillFxKind.Time: Ring(layer, target, color, 1.8f, 0.6f); HudAnim.Sparks(layer, target, color, 8, 100f, 14f); break;
                default: HudAnim.Sparks(layer, target, color, 12, 130f, 18f); break;
            }
        }

        /// <summary>Image d'impact de la famille : grossit au centre d'une carte, puis s'éteint (opacité).</summary>
        static void PopArt(VisualElement layer, VisualElement target, Sprite art, float fromScale, float toScale, float seconds)
        {
            if (!HudAnim.Allowed || target.panel == null) return;
            const float size = 260f;
            var e = Sprite(layer, art, Color.white, size);
            Place(layer, e, layer.WorldToLocal(target.worldBound.center), size);
            HudAnim.Run(e, seconds, k =>
            {
                HudAnim.SetScale(e, Mathf.Lerp(fromScale, toScale, k));
                e.style.opacity = 0.95f * (1f - k * k);
            }, () => e.RemoveFromHierarchy(), HudAnim.EaseOutCubic);
        }

        /// <summary>Forme pleine (étoile, losange, nuage…) qui grossit au centre d'une carte en tournant légèrement, puis s'éteint.</summary>
        static void Pop(VisualElement layer, VisualElement target, Sprite sprite, Color color, float fromScale, float toScale, float seconds)
        {
            if (!HudAnim.Allowed || target.panel == null) return;
            const float size = 260f;
            var e = Sprite(layer, sprite, color, size);
            Place(layer, e, layer.WorldToLocal(target.worldBound.center), size);
            float spin = UnityEngine.Random.Range(-25f, 25f);
            HudAnim.Run(e, seconds, k =>
            {
                HudAnim.SetScale(e, Mathf.Lerp(fromScale, toScale, k));
                HudAnim.SetRotate(e, spin * k);
                e.style.unityBackgroundImageTintColor = new Color(color.r, color.g, color.b, 0.85f * (1f - k * k));
            }, () => e.RemoveFromHierarchy(), HudAnim.EaseOutCubic);
        }

        /// <summary>Anneau qui grossit ou se resserre autour d'une carte puis s'éteint.</summary>
        static void Ring(VisualElement layer, VisualElement target, Color color, float fromScale, float toScale)
        {
            if (!HudAnim.Allowed || target.panel == null) return;
            const float size = 220f;
            var e = Sprite(layer, Sprites.Ring, color, size);
            Place(layer, e, layer.WorldToLocal(target.worldBound.center), size);
            HudAnim.Run(e, 0.5f, k =>
            {
                HudAnim.SetScale(e, Mathf.Lerp(fromScale, toScale, k));
                e.style.unityBackgroundImageTintColor = new Color(color.r, color.g, color.b, 0.9f * (1f - k));
            }, () => e.RemoveFromHierarchy(), HudAnim.EaseOutCubic);
        }
    }
}
