#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Feedback
{
    /// <summary>Compteur qui « roule » de l'ancienne à la nouvelle valeur (ressources), avec ralentissement final.</summary>
    public static class CounterRoll
    {
        public const float MinDuration = 0.25f, MaxDuration = 0.8f;

        /// <summary>Valeur affichée à l'avancement <paramref name="t"/> (0 → 1).</summary>
        public static int At(int from, int to, float t)
        {
            if (t >= 1f) return to;
            if (t <= 0f) return from;
            return from + (int)Math.Round((to - from) * (double)Tween.EaseOutCubic(t));
        }

        /// <summary>Durée selon l'écart : 0,25 s pour une unité, 0,8 s à partir de 10 000.</summary>
        public static float DurationFor(int from, int to)
        {
            long diff = Math.Abs((long)to - from);
            if (diff <= 1) return MinDuration;
            float k = Mathf.Clamp01((float)Math.Log10(diff) / 4f);
            return MinDuration + (MaxDuration - MinDuration) * k;
        }
    }

    /// <summary>Cascade d'entrée des cartes : 40 ms de décalage, plafonné.</summary>
    public static class Stagger
    {
        public static float DelayFor(int index, float step = 0.04f, int max = 12) => Mathf.Clamp(index, 0, max) * step;
    }

    /// <summary>
    /// Mouvements réduits (accessibilité) : coupe secousses, flashs et éclats. Interrupteur interne, désactivé par défaut ;
    /// relié aux Paramètres ▸ Accessibilité au lot suivant.
    /// </summary>
    public static class MotionSettings
    {
        public static bool Reduced { get; set; }

        /// <summary>Accessibilité : mini-jeux plus lents et zones de réussite plus larges.</summary>
        public static bool ExtendedMiniGame { get; set; }

        public static void ResetForTests() { Reduced = false; ExtendedMiniGame = false; }
    }

    /// <summary>Effets ponctuels qui respectent <see cref="MotionSettings"/> ; rien hors du mode jeu (tests EditMode).</summary>
    public static class Fx
    {
        static bool Allowed => !MotionSettings.Reduced && UnityEngine.Application.isPlaying;

        /// <summary>Secousse horizontale (bouton grisé, refus) ; false si coupée.</summary>
        public static bool Shake(RectTransform? rt, float strength = 8f)
        {
            if (rt == null || !Allowed) return false;
            // Une secousse par élément : la précédente est arrêtée et sa position d'origine rétablie (sinon, touchers répétés = dérive).
            if (Shaking.Count > 16)
                foreach (var dead in System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(Shaking.Keys, k => k == null))) Shaking.Remove(dead);
            if (Shaking.TryGetValue(rt, out var running))
            {
                Tween.Stop(running.Run);
                rt.anchoredPosition = running.Origin;
            }
            var origin = rt.anchoredPosition;
            var run = Tween.Run(0.22f, k => { if (rt != null) rt.anchoredPosition = origin + new Vector2(Mathf.Sin(k * 40f) * strength * (1f - k), 0f); },
                                () => { if (rt != null) rt.anchoredPosition = origin; Shaking.Remove(rt!); }, rt, Tween.Linear);
            Shaking[rt] = (run, origin);
            return true;
        }

        static readonly System.Collections.Generic.Dictionary<RectTransform, (Coroutine Run, Vector2 Origin)> Shaking =
            new System.Collections.Generic.Dictionary<RectTransform, (Coroutine Run, Vector2 Origin)>();

        /// <summary>Éclat : grossissement bref (recrutement, entrée dans l'escouade) ; false si coupé.</summary>
        public static bool Burst(Transform? t, float amount = 0.14f)
        {
            if (t == null || !Allowed) return false;
            Tween.Punch(t, amount, 0.3f);
            return true;
        }

        /// <summary>
        /// Gerbe d étincelles (impact, recrutement, récompense) : petits disques qui partent du centre de <paramref name="target"/>, ralentissent et s éteignent.
        /// Coupée par les mouvements réduits ; rien hors du mode jeu.
        /// </summary>
        public static bool Sparks(RectTransform? target, Color color, int count = 9, float radius = 110f, float size = 16f)
        {
            if (target == null || !Allowed) return false;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Spark", typeof(RectTransform), typeof(UnityEngine.UI.Image)) { layer = 5 };
                var rt = (RectTransform)go.transform;
                rt.SetParent(target, false);
                rt.sizeDelta = new Vector2(size, size);
                var img = go.GetComponent<UnityEngine.UI.Image>();
                img.sprite = Sprites.Circle;
                img.color = color;
                img.raycastTarget = false;
                float angle = (i + UnityEngine.Random.value * 0.6f) / count * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float reach = radius * (0.6f + UnityEngine.Random.value * 0.6f);
                var spark = rt;
                Tween.Run(0.45f, k =>
                {
                    if (spark == null) return;
                    spark.anchoredPosition = dir * reach * k;
                    spark.localScale = Vector3.one * (1f - k * 0.8f);
                    img.color = new Color(color.r, color.g, color.b, color.a * (1f - k * k));
                }, () => { if (spark != null) UnityEngine.Object.Destroy(spark.gameObject); }, spark, Tween.EaseOutCubic);
            }
            return true;
        }

        /// <summary>Éclair blanc bref sur une image (coup reçu) puis retour à sa couleur ; false si coupé.</summary>
        public static bool Flash(UnityEngine.UI.Graphic? g, float seconds = 0.14f)
        {
            if (g == null || !Allowed) return false;
            var baseColor = g.color;
            g.color = Color.white;
            Tween.Delay(seconds, () => { if (g != null) g.color = baseColor; }, g);
            return true;
        }

        /// <summary>Entrée en cascade : apparition avec rebond après <paramref name="delay"/> (toujours jouée, non bloquante).</summary>
        public static void PopInAfter(Transform t, float delay)
        {
            if (!UnityEngine.Application.isPlaying || t == null) return;
            if (delay <= 0f) { Tween.PopIn(t, 0.24f); return; }
            t.localScale = Vector3.zero;
            Tween.Delay(delay, () => { if (t != null) Tween.PopIn(t, 0.24f); }, t);
        }

        /// <summary>Retournement d'une carte (renouvellement de la boutique) après <paramref name="delay"/>.</summary>
        public static void FlipIn(Transform t, float delay)
        {
            if (!UnityEngine.Application.isPlaying || t == null) return;
            t.localScale = new Vector3(0f, 1f, 1f);
            Tween.Delay(delay, () =>
            {
                if (t != null) Tween.Run(0.22f, k => { if (t != null) t.localScale = new Vector3(k, 1f, 1f); }, null, t, Tween.EaseOutCubic);
            }, t);
        }
    }
}
