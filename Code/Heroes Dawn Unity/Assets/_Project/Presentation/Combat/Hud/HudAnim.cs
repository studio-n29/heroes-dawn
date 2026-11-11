#nullable enable
using System;
using N29.HeroesDawn.Presentation.Feedback;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>Petites animations d'éléments UI Toolkit du combat (le pendant de Tween pour uGUI) : temps non mis à l'échelle, coupées par « réduire les mouvements ».</summary>
    public static class HudAnim
    {
        public static float EaseOutCubic(float t) { t = 1f - t; return 1f - t * t * t; }
        public static float EaseOutBack(float t) { const float c1 = 1.70158f, c3 = c1 + 1f; t -= 1f; return 1f + c3 * t * t * t + c1 * t * t; }
        public static float Linear(float t) => t;

        public static bool Allowed => !MotionSettings.Reduced && UnityEngine.Application.isPlaying;

        /// <summary>Joue <paramref name="step"/> de 0 à 1 sur <paramref name="seconds"/> ; <paramref name="done"/> à la fin (même si l'élément est détaché).</summary>
        public static void Run(VisualElement host, float seconds, Action<float> step, Action? done = null, Func<float, float>? ease = null)
        {
            float t0 = Time.unscaledTime;
            IVisualElementScheduledItem? item = null;
            item = host.schedule.Execute(() =>
            {
                float k = seconds <= 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - t0) / seconds);
                step(ease != null ? ease(k) : k);
                if (k >= 1f) { item!.Pause(); done?.Invoke(); }
            }).Every(16);
        }

        public static void After(VisualElement host, float seconds, Action action) => host.schedule.Execute(action).ExecuteLater((long)(seconds * 1000f));

        public static void SetScale(VisualElement e, float s) => e.style.scale = new Scale(new Vector2(s, s));
        public static void SetScale(VisualElement e, Vector2 s) => e.style.scale = new Scale(s);
        public static void SetRotate(VisualElement e, float degrees) => e.style.rotate = new Rotate(new Angle(degrees, AngleUnit.Degree));
        public static void SetTranslate(VisualElement e, float x, float y) => e.style.translate = new Translate(x, y);

        public static void Punch(VisualElement e, float amount, float seconds)
        {
            if (!Allowed) return;
            Run(e, seconds, k => SetScale(e, 1f + amount * Mathf.Sin(k * Mathf.PI)), () => SetScale(e, 1f));
        }

        public static void PopIn(VisualElement e, float seconds, float from = 0.6f)
        {
            if (!Allowed) return;
            Run(e, seconds, k => SetScale(e, Mathf.LerpUnclamped(from, 1f, k)), () => SetScale(e, 1f), EaseOutBack);
        }

        public static void Shake(VisualElement e, float strength, float seconds)
        {
            if (!Allowed) return;
            Run(e, seconds, k => SetTranslate(e, UnityEngine.Random.Range(-1f, 1f) * strength * (1f - k), UnityEngine.Random.Range(-1f, 1f) * strength * (1f - k) * 0.5f), () => SetTranslate(e, 0f, 0f));
        }

        /// <summary>Étincelles qui partent du centre de <paramref name="from"/> dans la couche <paramref name="layer"/> (couvre tout le panneau).</summary>
        public static void Sparks(VisualElement layer, VisualElement from, Color color, int count, float speed, float size)
        {
            if (!Allowed || from.panel == null) return;
            var c = layer.WorldToLocal(from.worldBound.center);
            for (int i = 0; i < count; i++)
            {
                var spark = new VisualElement { pickingMode = PickingMode.Ignore };
                spark.style.position = Position.Absolute;
                spark.style.width = size; spark.style.height = size;
                spark.style.backgroundColor = color;
                spark.style.borderTopLeftRadius = spark.style.borderTopRightRadius = spark.style.borderBottomLeftRadius = spark.style.borderBottomRightRadius = size;
                layer.Add(spark);
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f), dist = UnityEngine.Random.Range(0.4f, 1f) * speed;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                Run(spark, 0.45f, k =>
                {
                    spark.style.left = c.x + dir.x * k - size * 0.5f;
                    spark.style.top = c.y + dir.y * k - size * 0.5f;
                    spark.style.opacity = 1f - k;
                }, () => spark.RemoveFromHierarchy(), EaseOutCubic);
            }
        }

        /// <summary>Étincelles autour d'un point (coordonnées locales de <paramref name="layer"/>, y vers le bas).</summary>
        public static void Sparks(VisualElement layer, Vector2 localCenter, Color color, int count, float speed, float size)
        {
            if (!Allowed) return;
            for (int i = 0; i < count; i++)
            {
                var spark = new VisualElement { pickingMode = PickingMode.Ignore };
                spark.style.position = Position.Absolute;
                spark.style.width = size; spark.style.height = size;
                spark.style.backgroundColor = color;
                spark.style.borderTopLeftRadius = spark.style.borderTopRightRadius = spark.style.borderBottomLeftRadius = spark.style.borderBottomRightRadius = size;
                layer.Add(spark);
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f), dist = UnityEngine.Random.Range(0.4f, 1f) * speed;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                Run(spark, 0.45f, k =>
                {
                    spark.style.left = localCenter.x + dir.x * k - size * 0.5f;
                    spark.style.top = localCenter.y + dir.y * k - size * 0.5f;
                    spark.style.opacity = 1f - k;
                }, () => spark.RemoveFromHierarchy(), EaseOutCubic);
            }
        }

        public static void Flash(VisualElement e)
        {
            if (!Allowed) return;
            e.AddToClassList("hd-fx-flash");
            After(e, 0.12f, () => e.RemoveFromClassList("hd-fx-flash"));
        }
    }
}
