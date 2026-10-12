#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Tutorial
{
    /// <summary>Texte d'une étape à afficher dans la bulle de Rayner.</summary>
    public struct TutorialStepView
    {
        public string Title;
        public string Body;
        public string Progress;
        public string OkLabel;
        public string SkipLabel;
        public bool ShowOk;
    }

    /// <summary>
    /// Couche du tutoriel (prefab Common/TutorialOverlay) : voile bleu nuit avec une découpe rectangulaire sur l'élément à toucher,
    /// cadre à coins coupés pulsé, équerres animées et flèche qui le désignent, bulle de BD de Rayner (titre jaune, 2 lignes, progression),
    /// et carte « Astuce » non bloquante pour les mécaniques contextuelles.
    /// </summary>
    public sealed class TutorialOverlayView : MonoBehaviour
    {
        const float Pad = 8f;

        [SerializeField] RectTransform veilRoot = null!;
        [SerializeField] Image veilTop = null!;
        [SerializeField] Image veilBottom = null!;
        [SerializeField] Image veilLeft = null!;
        [SerializeField] Image veilRight = null!;
        [SerializeField] Image ring = null!;
        [SerializeField] Image ripple = null!;
        [SerializeField] RectTransform[] brackets = System.Array.Empty<RectTransform>();
        [SerializeField] RectTransform pointer = null!;
        [SerializeField] RectTransform bubble = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text body = null!;
        [SerializeField] TMP_Text progress = null!;
        [SerializeField] UiButton ok = null!;
        [SerializeField] UiButton skip = null!;
        [SerializeField] RectTransform hintCard = null!;
        [SerializeField] TMP_Text hintTitle = null!;
        [SerializeField] TMP_Text hintBody = null!;
        [SerializeField] UiButton hintOk = null!;

        float _clock;
        bool _hasHole;
        float _hintLeft;

        public event Action? AcknowledgeClicked;
        public event Action? SkipClicked;
        public event Action? HintClosed;

        public bool StepVisible => veilRoot.gameObject.activeSelf;
        public bool HintVisible => hintCard.gameObject.activeSelf;
        public UiButton OkButton => ok;
        public UiButton SkipButton => skip;
        public UiButton HintOkButton => hintOk;
        public RectTransform Bubble => bubble;
        /// <summary>Sprite du cadre de mise en valeur (rectangle à coins coupés) et nombre d'équerres visibles (tests).</summary>
        public Sprite? FrameSprite => ring.sprite;
        public int VisibleBrackets { get { int n = 0; foreach (var b in brackets) if (b.gameObject.activeSelf) n++; return n; } }

        void Awake()
        {
            ok.Button.onClick.AddListener(() => AcknowledgeClicked?.Invoke());
            skip.Button.onClick.AddListener(() => SkipClicked?.Invoke());
            hintOk.Button.onClick.AddListener(() => { HideHint(); HintClosed?.Invoke(); });
            Hide();
            HideHint();
        }

        void Update()
        {
            _clock += Time.unscaledDeltaTime;
            if (HintVisible && (_hintLeft -= Time.unscaledDeltaTime) <= 0f) { HideHint(); HintClosed?.Invoke(); }
            if (!StepVisible || !_hasHole) return;
            float k = (_clock * 0.9f) % 1f;
            LayoutBrackets(Mathf.Sin(_clock * 5f) * 0.5f + 0.5f);
            ripple.rectTransform.localScale = Vector3.one * (1f + 0.07f * k);
            ripple.color = new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.8f * (1f - k));
            pointer.anchoredPosition = _pointerBase + new Vector2(0f, Mathf.Sin(_clock * 6f) * 10f);
        }

        Vector2 _pointerBase;
        Rect _hole;

        /// <summary>Équerres aux quatre coins du cadre ; elles « respirent » vers l'extérieur (0 à 1).</summary>
        void LayoutBrackets(float breathe)
        {
            if (brackets.Length < 4) return;
            float o = 2f + 8f * breathe;
            var c = new[] { new Vector2(_hole.xMin - o, _hole.yMin - o), new Vector2(_hole.xMax + o, _hole.yMin - o), new Vector2(_hole.xMax + o, _hole.yMax + o), new Vector2(_hole.xMin - o, _hole.yMax + o) };
            for (int i = 0; i < 4; i++)
            {
                brackets[i].anchoredPosition = c[i];
                brackets[i].localRotation = Quaternion.Euler(0f, 0f, 90f * i);
            }
        }

        /// <summary>Affiche une étape. <paramref name="hole"/> : zone à laisser libre, en coordonnées locales de la couche (null : voile plein). <paramref name="blockHole"/> : la découpe elle-même ne répond pas (étapes « Compris »).</summary>
        public void ShowStep(Rect? hole, TutorialStepView text, bool bubbleAtTop, bool blockHole, bool dim = true)
        {
            veilRoot.gameObject.SetActive(true);
            var veilTint = Theme.WithAlpha(Theme.Navy, dim ? 0.5f : 0f);
            foreach (var v in new[] { veilTop, veilBottom, veilLeft, veilRight }) { v.color = veilTint; v.raycastTarget = dim; }
            var area = ((RectTransform)transform).rect;
            _hasHole = hole.HasValue;
            if (hole.HasValue)
            {
                var h = Rect.MinMaxRect(hole.Value.xMin - Pad, hole.Value.yMin - Pad, hole.Value.xMax + Pad, hole.Value.yMax + Pad);
                Place(veilTop, area.xMin, h.yMax, area.xMax, area.yMax);
                Place(veilBottom, area.xMin, area.yMin, area.xMax, h.yMin);
                Place(veilLeft, area.xMin, h.yMin, h.xMin, h.yMax);
                Place(veilRight, h.xMax, h.yMin, area.xMax, h.yMax);
                ring.gameObject.SetActive(true);
                ripple.gameObject.SetActive(true);
                pointer.gameObject.SetActive(true);
                _hole = h;
                foreach (var b in brackets) b.gameObject.SetActive(true);
                LayoutBrackets(0f);
                ring.rectTransform.anchoredPosition = h.center;
                ring.rectTransform.sizeDelta = h.size;
                ring.raycastTarget = blockHole;
                ripple.rectTransform.anchoredPosition = h.center;
                ripple.rectTransform.sizeDelta = h.size;
                // La flèche est posée du côté opposé à la bulle, pointant vers la cible.
                _pointerBase = new Vector2(h.center.x, bubbleAtTop ? h.yMin - 38f : h.yMax + 38f);
                pointer.localRotation = Quaternion.Euler(0, 0, bubbleAtTop ? 0f : 180f);
            }
            else
            {
                Place(veilTop, area.xMin, area.yMin, area.xMax, area.yMax);
                Place(veilBottom, 0, 0, 0, 0);
                Place(veilLeft, 0, 0, 0, 0);
                Place(veilRight, 0, 0, 0, 0);
                ring.gameObject.SetActive(false);
                ripple.gameObject.SetActive(false);
                pointer.gameObject.SetActive(false);
                foreach (var b in brackets) b.gameObject.SetActive(false);
            }

            title.text = text.Title;
            body.text = text.Body;
            progress.text = text.Progress;
            ok.SetLabel(text.OkLabel);
            skip.SetLabel(text.SkipLabel);
            ok.gameObject.SetActive(text.ShowOk);
            bubble.gameObject.SetActive(true);
            bubble.anchorMin = bubble.anchorMax = new Vector2(0.5f, bubbleAtTop ? 1f : 0f);
            bubble.pivot = new Vector2(0.5f, bubbleAtTop ? 1f : 0f);
            bubble.anchoredPosition = new Vector2(0f, bubbleAtTop ? -130f : 140f);
        }

        void Place(Image img, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(xMin, yMin);
            rt.sizeDelta = new Vector2(Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
            img.enabled = xMax > xMin && yMax > yMin;
        }

        public void Hide()
        {
            veilRoot.gameObject.SetActive(false);
            bubble.gameObject.SetActive(false);
        }

        public void ShowHint(string heading, string text, string okLabel)
        {
            hintTitle.text = heading;
            hintBody.text = text;
            hintOk.SetLabel(okLabel);
            hintCard.gameObject.SetActive(true);
            _hintLeft = 9f;
            Tween.PopIn(hintCard, 0.25f);
        }

        public void HideHint() => hintCard.gameObject.SetActive(false);
    }
}
