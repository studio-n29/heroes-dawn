#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Première connexion (popup plein écran) : cinématique d'écrans fondus au noir (zoom lent, braises, bandes de cinéma, texte qui s'écrit),
    /// pseudo demandé sous forme de dialogue, choix de la campagne. Les illustrations sont provisoires : un motif par écran (dans le prefab),
    /// remplacé par l'image dessinée dès qu'elle existe (Resources/Intro/intro_NN).
    /// </summary>
    public sealed class IntroPopupView : MonoBehaviour, IIntroPopupView
    {
        public enum Stage { None, Cinematic, Pseudo, Campaign }

        [SerializeField] CanvasGroup group = null!;
        [SerializeField] RectTransform cinematic = null!;
        [SerializeField] RectTransform artRoot = null!;
        [SerializeField] Image artBack = null!;
        [SerializeField] Image artTop = null!;
        [SerializeField] Image artPicture = null!;
        [SerializeField, Tooltip("Un motif provisoire par valeur de IntroMotif, dans l'ordre.")] RectTransform[] motifs = Array.Empty<RectTransform>();
        [SerializeField] RectTransform embers = null!;
        [SerializeField] RectTransform[] emberViews = Array.Empty<RectTransform>();
        [SerializeField] Image blackout = null!;
        [SerializeField] TMP_Text caption = null!;
        [SerializeField] TMP_Text tapHint = null!;
        [SerializeField] Button tapArea = null!;
        [SerializeField] UiButton skip = null!;
        [SerializeField] RectTransform pseudoStage = null!;
        [SerializeField] TMP_Text speaker = null!;
        [SerializeField] TMP_Text line = null!;
        [SerializeField] TMP_InputField input = null!;
        [SerializeField] TMP_Text inputError = null!;
        [SerializeField] UiButton pseudoConfirm = null!;
        [SerializeField] RectTransform campaignStage = null!;
        [SerializeField] TMP_Text campaignTitle = null!;
        [SerializeField] RectTransform cardRow = null!;
        [SerializeField] CampaignCardView cardPrefab = null!;
        [SerializeField] UiButton campaignConfirm = null!;

        readonly List<float> _emberSpeed = new List<float>();
        readonly List<float> _emberPhase = new List<float>();
        readonly List<CampaignCardView> _cards = new List<CampaignCardView>();
        bool _tapped, _skipped, _typing, _skipTyping;
        float _slideTime;
        const float SlideLength = 8f;
        string? _selectedCampaign;

        public bool Cancelable => false;
        public void RequestCancel() { }

        public Stage Current { get; private set; } = Stage.None;
        public Button TapButton => tapArea;
        public Button SkipButton => skip.Button;
        public TMP_InputField Input => input;
        public Button PseudoConfirmButton => pseudoConfirm.Button;
        public Button CampaignConfirmButton => campaignConfirm.Button;
        public string Caption => caption.text;
        public string InputError => inputError.text;
        public IReadOnlyList<CampaignCardView> Cards => _cards;
        /// <summary>Motif provisoire affiché (null si une illustration dessinée est montrée).</summary>
        public int ShownMotif { get; private set; } = -1;

        static bool Animated => !MotionSettings.Reduced && UnityEngine.Application.isPlaying;

        void Awake()
        {
            tapArea.onClick.AddListener(() => { if (_typing) _skipTyping = true; else _tapped = true; });
            skip.Button.onClick.AddListener(() => { _skipped = true; _tapped = true; });
            for (int i = 0; i < emberViews.Length; i++)
            {
                _emberSpeed.Add(UnityEngine.Random.Range(40f, 110f));
                _emberPhase.Add(UnityEngine.Random.value * 10f);
            }
        }

        void Update()
        {
            if (Current != Stage.Cinematic || !Animated) return;
            _slideTime += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_slideTime / SlideLength);
            artRoot.localScale = Vector3.one * (1f + 0.1f * k); // zoom lent, façon plan d'ouverture
            var rect = embers.rect;
            for (int i = 0; i < emberViews.Length; i++)
            {
                float t = _slideTime * _emberSpeed[i] + _emberPhase[i] * 100f;
                float y = Mathf.Repeat(t, rect.height + 40f) - rect.height * 0.5f;
                float x = Mathf.Sin(_slideTime * 0.7f + _emberPhase[i]) * 90f + (_emberPhase[i] / 10f - 0.5f) * rect.width;
                emberViews[i].anchoredPosition = new Vector2(x, y);
                float fade = 1f - Mathf.Abs(y) / (rect.height * 0.55f);
                var img = emberViews[i].GetComponent<Image>();
                var c = img.color; c.a = Mathf.Clamp01(fade) * 0.55f; img.color = c;
            }
        }

        void SetStage(Stage s)
        {
            Current = s;
            cinematic.gameObject.SetActive(s == Stage.Cinematic);
            pseudoStage.gameObject.SetActive(s == Stage.Pseudo);
            campaignStage.gameObject.SetActive(s == Stage.Campaign);
            if (group.alpha < 1f) Tween.Fade(group, 1f, Animated ? 0.2f : 0f);
        }

        // ------------------------------------------------------------------ Cinématique

        public async UniTask PlayCinematicAsync(IReadOnlyList<IntroSlideData> slides, IntroCinematicTexts texts, CancellationToken ct)
        {
            skip.SetLabel(texts.Skip);
            tapHint.text = texts.TapHint;
            _skipped = false;
            blackout.color = Color.black;
            SetStage(Stage.Cinematic);
            foreach (var slide in slides)
            {
                if (_skipped) break;
                ShowArt(slide);
                caption.text = "";
                tapHint.gameObject.SetActive(false);
                _slideTime = 0f;
                await FadeBlackAsync(0f, 0.9f, ct);
                await TypeAsync(slide.Text, ct);
                tapHint.gameObject.SetActive(true);
                _tapped = false;
                await UniTask.WaitUntil(() => _tapped || _skipped, cancellationToken: ct);
                tapHint.gameObject.SetActive(false);
                await FadeBlackAsync(1f, 0.6f, ct);
            }
            caption.text = "";
        }

        async UniTask FadeBlackAsync(float to, float seconds, CancellationToken ct)
        {
            float from = blackout.color.a;
            if (!Animated || _skipped) { SetBlack(to); return; }
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                SetBlack(Mathf.Lerp(from, to, t / seconds));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                if (_skipped) break;
            }
            SetBlack(to);
        }

        void SetBlack(float a) { var c = blackout.color; c.a = a; blackout.color = c; }

        /// <summary>Le texte s'écrit lettre à lettre ; un toucher l'affiche en entier.</summary>
        async UniTask TypeAsync(string text, CancellationToken ct)
        {
            _typing = true;
            _skipTyping = false;
            try
            {
                if (!Animated) { caption.text = text; return; }
                for (int i = 1; i <= text.Length && !_skipTyping && !_skipped; i++)
                {
                    caption.text = text.Substring(0, i);
                    await UniTask.Delay(28, DelayType.UnscaledDeltaTime, cancellationToken: ct);
                }
                caption.text = text;
            }
            finally { _typing = false; }
        }

        /// <summary>Illustration d'un écran : l'image dessinée si elle existe, sinon le fond dégradé et le motif provisoire.</summary>
        void ShowArt(IntroSlideData slide)
        {
            artRoot.localScale = Vector3.one;
            bool picture = slide.Art != null;
            artPicture.gameObject.SetActive(picture);
            artBack.gameObject.SetActive(!picture);
            artTop.gameObject.SetActive(!picture);
            int motif = picture ? -1 : (int)slide.Motif;
            ShownMotif = motif;
            for (int i = 0; i < motifs.Length; i++) motifs[i].gameObject.SetActive(i == motif);
            if (picture) { artPicture.sprite = slide.Art; return; }
            artBack.color = slide.Bottom;
            artTop.color = slide.Top;
        }

        // ------------------------------------------------------------------ Pseudo

        public async UniTask<string> AskPseudoAsync(PseudoPromptData d, Func<string, string?> validate, CancellationToken ct)
        {
            speaker.text = d.Speaker;
            line.text = d.Line;
            ((TMP_Text)input.placeholder).text = d.Placeholder;
            input.text = d.Suggestion;
            inputError.text = "";
            pseudoConfirm.SetLabel(d.Confirm);
            SetStage(Stage.Pseudo);
            if (Animated) Tween.PopIn(pseudoStage, 0.3f);
            var done = new UniTaskCompletionSource<string>();
            void Confirm()
            {
                var error = validate(input.text);
                if (error != null) { inputError.text = error; if (Animated) Fx.Shake((RectTransform)input.transform); return; }
                done.TrySetResult(input.text);
            }
            pseudoConfirm.Button.onClick.AddListener(Confirm);
            input.onSubmit.AddListener(_ => Confirm());
            try { return await done.Task.AttachExternalCancellation(ct); }
            finally { pseudoConfirm.Button.onClick.RemoveListener(Confirm); input.onSubmit.RemoveAllListeners(); }
        }

        // ------------------------------------------------------------------ Campagne

        public async UniTask<string> ChooseCampaignAsync(ChooseCampaignData d, CancellationToken ct)
        {
            campaignTitle.text = d.Title;
            campaignConfirm.SetLabel(d.Confirm);
            campaignConfirm.Interactable = false;
            _selectedCampaign = null;
            cardRow.Clear();
            _cards.Clear();
            var done = new UniTaskCompletionSource<string>();
            foreach (var c in d.Cards)
            {
                var card = Instantiate(cardPrefab, cardRow);
                card.Setup(c);
                card.Clicked += SelectCampaign;
                _cards.Add(card);
            }
            SetStage(Stage.Campaign);
            if (Animated) Tween.PopIn(campaignStage, 0.3f);
            void Confirm() { if (_selectedCampaign != null) done.TrySetResult(_selectedCampaign); }
            campaignConfirm.Button.onClick.AddListener(Confirm);
            try { return await done.Task.AttachExternalCancellation(ct); }
            finally { campaignConfirm.Button.onClick.RemoveListener(Confirm); }
        }

        /// <summary>Choisit une campagne (clic sur sa carte) ; ignoré si elle n'est pas disponible.</summary>
        public void SelectCampaign(string id)
        {
            CampaignCardView? chosen = null;
            foreach (var c in _cards) if (c.Id == id) chosen = c;
            if (chosen == null || !chosen.Available) return;
            _selectedCampaign = id;
            campaignConfirm.Interactable = true;
            foreach (var c in _cards) c.SetSelected(c == chosen);
        }
    }
}
