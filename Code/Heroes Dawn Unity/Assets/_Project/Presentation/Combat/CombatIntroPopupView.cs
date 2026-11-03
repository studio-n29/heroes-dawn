#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Entrée en combat (prefab) : les agents du joueur entrent par la gauche, les ennemis par la droite, un bandeau chacun.
    /// Un toucher ou « Passer » ferme l'introduction ; une introduction déjà vue est jouée sans glissement et plus courte.
    /// </summary>
    public sealed class CombatIntroPopupView : MonoBehaviour, ICombatIntroView
    {
        [SerializeField] CanvasGroup group = null!;
        [SerializeField] Button tapArea = null!;
        [SerializeField] Button skip = null!;
        [SerializeField] RectTransform allyColumn = null!;
        [SerializeField] RectTransform enemyColumn = null!;
        [SerializeField] TMP_Text allyTitle = null!;
        [SerializeField] TMP_Text enemyTitle = null!;
        [SerializeField] CombatIntroBannerView bannerPrefab = null!;

        /// <summary>Durées en secondes : glissement d'un bandeau, décalage entre deux bandeaux, temps de lecture (première fois, répétition).</summary>
        const float Slide = 0.3f, Stagger = 0.18f, HoldFirst = 1.5f, HoldRepeat = 0.5f, SlideDistance = 900f;

        readonly List<CombatIntroBannerView> _banners = new List<CombatIntroBannerView>();

        public bool Cancelable => false;
        public void RequestCancel() { }

        public Button TapButton => tapArea;
        public Button SkipButton => skip;
        /// <summary>Bandeaux affichés, alliés d'abord (tests).</summary>
        public IReadOnlyList<CombatIntroBannerView> Banners => _banners;
        public string AllyTitle => allyTitle.text;
        public string EnemyTitle => enemyTitle.text;

        public async UniTask PlayAsync(CombatIntroData data, CancellationToken ct)
        {
            Clear();
            allyTitle.text = data.AllyTitle;
            enemyTitle.text = data.EnemyTitle;
            var targets = new List<(RectTransform Rt, bool Left)>();
            foreach (var f in data.Allies) targets.Add((Spawn(f, allyColumn, true), true));
            foreach (var f in data.Enemies) targets.Add((Spawn(f, enemyColumn, false), false));
            LayoutRebuilder.ForceRebuildLayoutImmediate(allyColumn);
            LayoutRebuilder.ForceRebuildLayoutImmediate(enemyColumn);

            bool closed = false;
            tapArea.onClick.RemoveAllListeners();
            tapArea.onClick.AddListener(() => closed = true);
            skip.onClick.RemoveAllListeners();
            skip.onClick.AddListener(() => closed = true);

            bool animate = !data.Repeat && !MotionSettings.Reduced;
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            float t = 0f;
            float total = animate ? Stagger * targets.Count + Slide + HoldFirst : data.Repeat ? HoldRepeat : HoldFirst * 0.5f;
            var rest = new List<Vector2>();
            foreach (var target in targets) rest.Add(target.Rt.anchoredPosition);
            while (!closed && t < total)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    float k = animate ? Mathf.Clamp01((t - Stagger * i) / Slide) : 1f;
                    float eased = 1f - (1f - k) * (1f - k);
                    float offset = (1f - eased) * SlideDistance * (targets[i].Left ? -1f : 1f);
                    targets[i].Rt.anchoredPosition = rest[i] + new Vector2(offset, 0f);
                }
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                t += Time.unscaledDeltaTime;
            }
            tapArea.onClick.RemoveAllListeners();
            skip.onClick.RemoveAllListeners();
        }

        RectTransform Spawn(CombatIntroFighter fighter, RectTransform column, bool left)
        {
            var banner = Instantiate(bannerPrefab, column);
            banner.Setup(fighter, left);
            _banners.Add(banner);
            return (RectTransform)banner.transform;
        }

        void Clear()
        {
            foreach (var b in _banners) if (b != null) Destroy(b.gameObject);
            _banners.Clear();
        }
    }
}
