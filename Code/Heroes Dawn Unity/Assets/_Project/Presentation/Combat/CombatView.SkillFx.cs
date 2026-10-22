#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Animation d'une compétence jouée dans le terrain central : titre, projectile vers chaque cible, éclat propre à sa famille.</summary>
    public sealed partial class CombatView
    {
        const float FlightSeconds = 0.38f;

        /// <inheritdoc />
        public float PlaySkillFx(SkillFxData d, float seconds)
        {
            if (!AnimationsAllowed || arena == null || seconds <= 0f) return 0f;
            var color = SkillFx.ColorOf(d.Kind);
            ShowArenaTitle(d.Title, color, seconds);
            if (!_cards.TryGetValue(d.ActorId, out var actor)) return seconds;
            var canvas = GetComponentInParent<Canvas>();
            var layer = canvas != null ? canvas.rootCanvas.transform : transform;
            var actorRect = (RectTransform)actor.transform;
            var from = actorRect.TransformPoint(actorRect.rect.center);
            bool toAllies = d.Kind == SkillFxKind.Heal || d.Kind == SkillFxKind.Shield || d.Kind == SkillFxKind.Haste;
            int shown = 0;
            foreach (var id in d.TargetIds)
            {
                if (shown++ >= 4 || !_cards.TryGetValue(id, out var target)) continue;
                var targetRect = (RectTransform)target.transform;
                var to = targetRect.TransformPoint(targetRect.rect.center);
                if (toAllies || id == d.ActorId) Tween.Delay(0.12f, () => { if (targetRect != null) Burst(targetRect, d.Kind, color); }, targetRect);
                else Projectile(layer, from, to, d.Kind, color, () => { if (targetRect != null) Burst(targetRect, d.Kind, color); });
            }
            return seconds;
        }

        void ShowArenaTitle(string title, Color color, float seconds)
        {
            if (arenaTitle == null) return;
            arenaTitle.text = title.ToUpperInvariant();
            arenaTitle.color = color;
            arenaTitle.gameObject.SetActive(true);
            Tween.PopIn(arenaTitle.rectTransform, 0.25f);
            Tween.Delay(seconds, () => { if (arenaTitle != null) arenaTitle.gameObject.SetActive(false); }, arenaTitle);
        }

        /// <summary>Forme qui traverse le terrain de l'attaquant à la cible, en arc ; sa forme suit la famille (éclair en trait, glace en éclat…).</summary>
        static void Projectile(Transform layer, Vector3 from, Vector3 to, SkillFxKind kind, Color color, System.Action arrived)
        {
            var go = new GameObject("SkillProjectile", typeof(RectTransform), typeof(Image)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.SetParent(layer, false);
            rt.sizeDelta = new Vector2(kind == SkillFxKind.Lightning ? 120f : 56f, kind == SkillFxKind.Lightning ? 26f : 56f);
            rt.position = from;
            var img = go.GetComponent<Image>();
            img.sprite = kind == SkillFxKind.Ice ? Sprites.Diamond : kind == SkillFxKind.Lightning ? Sprites.Shine : kind == SkillFxKind.Chains || kind == SkillFxKind.Mark ? Sprites.Ring : Sprites.Circle;
            img.color = color;
            img.raycastTarget = false;
            var dir = to - from;
            float arc = kind == SkillFxKind.Lightning ? 0f : Mathf.Min(120f, dir.magnitude * 0.18f);
            rt.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            Tween.Run(FlightSeconds, k =>
            {
                if (rt == null) return;
                rt.position = Vector3.LerpUnclamped(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * arc;
                rt.localScale = Vector3.one * (kind == SkillFxKind.Mark ? Mathf.Lerp(1.8f, 0.9f, k) : Mathf.Lerp(0.8f, 1.2f, k));
                if (kind == SkillFxKind.Fire || kind == SkillFxKind.Acid) Fx.Sparks(rt, color, 1, 40f, 12f); // traînée
            }, () =>
            {
                if (rt != null) Object.Destroy(rt.gameObject);
                arrived();
            }, rt, Tween.EaseOutCubic);
        }

        /// <summary>Éclat à l'arrivée : étincelles et anneau, réglés par famille.</summary>
        static void Burst(RectTransform target, SkillFxKind kind, Color color)
        {
            switch (kind)
            {
                case SkillFxKind.Fire: Fx.Sparks(target, color, 16, 150f, 20f); Fx.Sparks(target, new Color(1f, 0.25f, 0.1f), 8, 90f, 26f); break;
                case SkillFxKind.Ice: Fx.Sparks(target, color, 12, 130f, 24f); Ring(target, color, 0.6f, 1.5f); break;
                case SkillFxKind.Lightning: Fx.Sparks(target, color, 10, 190f, 9f); Fx.Flash(target.GetComponentInChildren<Graphic>()); break;
                case SkillFxKind.Acid: Fx.Sparks(target, color, 12, 95f, 22f); break;
                case SkillFxKind.Chains: Ring(target, color, 1.6f, 0.9f); Ring(target, color, 1.2f, 0.7f); Fx.Sparks(target, color, 6, 80f, 14f); break;
                case SkillFxKind.Mark: Ring(target, color, 1.9f, 0.8f); break;
                case SkillFxKind.Heal: Fx.Sparks(target, color, 12, 110f, 16f); Ring(target, color, 0.5f, 1.3f); break;
                case SkillFxKind.Shield: Ring(target, color, 0.7f, 1.5f); Ring(target, color, 0.5f, 1.2f); break;
                case SkillFxKind.Haste: Fx.Sparks(target, color, 10, 160f, 10f); break;
                case SkillFxKind.Time: Ring(target, color, 1.8f, 0.6f); Fx.Sparks(target, color, 8, 100f, 14f); break;
                default: Fx.Sparks(target, color, 12, 130f, 18f); break;
            }
        }

        /// <summary>Anneau qui grossit ou se resserre autour d'une carte puis s'éteint.</summary>
        static void Ring(RectTransform target, Color color, float fromScale, float toScale)
        {
            if (!UnityEngine.Application.isPlaying || MotionSettings.Reduced) return;
            var go = new GameObject("FxRing", typeof(RectTransform), typeof(Image)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.SetParent(target, false);
            rt.sizeDelta = new Vector2(220f, 220f);
            var img = go.GetComponent<Image>();
            img.sprite = Sprites.Ring;
            img.raycastTarget = false;
            Tween.Run(0.5f, k =>
            {
                if (rt == null) return;
                rt.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, k);
                img.color = new Color(color.r, color.g, color.b, 0.9f * (1f - k));
            }, () => { if (rt != null) Object.Destroy(rt.gameObject); }, rt, Tween.EaseOutCubic);
        }
    }
}
