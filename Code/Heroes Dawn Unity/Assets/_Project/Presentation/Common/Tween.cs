using System;
using System.Collections;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Mini-moteur d'animations à base de coroutines (temps non mis à l'échelle). Remplacé par PrimeTween au M2.</summary>
    public sealed class Tween : MonoBehaviour
    {
        static Tween _instance;

        static Tween I
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Tween]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<Tween>();
                }
                return _instance;
            }
        }

        public static float EaseOutCubic(float t) { t = 1f - t; return 1f - t * t * t; }
        public static float EaseOutBack(float t) { const float c1 = 1.70158f, c3 = c1 + 1f; t -= 1f; return 1f + c3 * t * t * t + c1 * t * t; }
        public static float Linear(float t) => t;

        /// <summary>Lance une interpolation ; <paramref name="owner"/> détruit ⇒ l'animation s'arrête sans rappel.</summary>
        public static Coroutine Run(float duration, Action<float> step, Action done = null, UnityEngine.Object owner = null, Func<float, float> ease = null)
            => I.StartCoroutine(Co(duration, step, done, owner, owner != null, ease ?? EaseOutCubic));

        static IEnumerator Co(float d, Action<float> step, Action done, UnityEngine.Object owner, bool hasOwner, Func<float, float> ease)
        {
            float t = 0f;
            while (t < d)
            {
                if (hasOwner && owner == null) yield break;
                step(ease(Mathf.Clamp01(t / d)));
                yield return null;
                t += Time.unscaledDeltaTime;
            }
            if (hasOwner && owner == null) yield break;
            step(1f);
            done?.Invoke();
        }

        public static void Stop(Coroutine c) { if (c != null && _instance != null) _instance.StopCoroutine(c); }

        public static Coroutine Delay(float seconds, Action action, UnityEngine.Object owner = null)
            => Run(seconds, _ => { }, action, owner, Linear);

        public static Coroutine Punch(Transform t, float amount = 0.12f, float duration = 0.25f)
        {
            if (t == null) return null;
            return Run(duration, k => { if (t != null) t.localScale = Vector3.one * (1f + amount * Mathf.Sin(k * Mathf.PI)); },
                       () => { if (t != null) t.localScale = Vector3.one; }, t, Linear);
        }

        public static Coroutine PopIn(Transform t, float duration = 0.28f)
        {
            if (t == null) return null;
            t.localScale = Vector3.one * 0.6f;
            return Run(duration, k => { if (t != null) t.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, k); }, null, t, EaseOutBack);
        }

        public static Coroutine Fade(CanvasGroup cg, float to, float duration, Action done = null)
        {
            if (cg == null) return null;
            float from = cg.alpha;
            return Run(duration, k => { if (cg != null) cg.alpha = Mathf.Lerp(from, to, k); }, done, cg);
        }

        public static Coroutine Move(RectTransform rt, Vector2 to, float duration, Action done = null, Func<float, float> ease = null)
        {
            if (rt == null) return null;
            Vector2 from = rt.anchoredPosition;
            return Run(duration, k => { if (rt != null) rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k); }, done, rt, ease);
        }

        /// <summary>Petit tremblement horizontal (impact).</summary>
        public static Coroutine Shake(RectTransform rt, float strength = 10f, float duration = 0.25f)
        {
            if (rt == null) return null;
            Vector2 origin = rt.anchoredPosition;
            return Run(duration, k => { if (rt != null) rt.anchoredPosition = origin + new Vector2(Mathf.Sin(k * 40f) * strength * (1f - k), 0f); },
                       () => { if (rt != null) rt.anchoredPosition = origin; }, rt, Linear);
        }
    }

}
