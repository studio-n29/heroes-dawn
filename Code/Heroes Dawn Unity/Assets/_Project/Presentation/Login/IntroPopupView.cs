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
    /// Première connexion (popup plein écran) : la cinématique de lancement (vidéos des 46 plans et voix off, joués par <see cref="CinematicVideoPlayer"/>),
    /// puis le choix du personnage joueur (Agent D ou Agent M).
    /// Un toucher passe au plan suivant ; « Passer » termine la cinématique.
    /// </summary>
    public sealed class IntroPopupView : MonoBehaviour, IIntroPopupView
    {
        public enum Stage { None, Cinematic, Campaign }

        [SerializeField] CanvasGroup group = null!;
        [SerializeField] RectTransform cinematic = null!;
        [SerializeField] CinematicVideoPlayer player = null!;
        [SerializeField] Button tapArea = null!;
        [SerializeField] UiButton skip = null!;
        [SerializeField] RectTransform campaignStage = null!;
        [SerializeField] TMP_Text campaignTitle = null!;
        [SerializeField] RectTransform cardRow = null!;
        [SerializeField] CampaignCardView cardPrefab = null!;
        [SerializeField] UiButton campaignConfirm = null!;

        readonly List<CampaignCardView> _cards = new List<CampaignCardView>();
        string? _selectedCampaign;

        public bool Cancelable => false;
        public void RequestCancel() { }

        public Stage Current { get; private set; } = Stage.None;
        public Button TapButton => tapArea;
        public Button SkipButton => skip.Button;
        public Button CampaignConfirmButton => campaignConfirm.Button;
        public IReadOnlyList<CampaignCardView> Cards => _cards;
        public CinematicVideoPlayer Player => player;

        static bool Animated => !MotionSettings.Reduced && UnityEngine.Application.isPlaying;

        void Awake()
        {
            // Keep a comfortable touch target, but reduce the visual weight of Skip.
            skip.Rect.anchorMin = skip.Rect.anchorMax = Vector2.one;
            skip.Rect.pivot = Vector2.one;
            skip.Rect.sizeDelta = new Vector2(170f, 64f);
            skip.Label.fontSize = 24f;
            skip.Label.color = new Color(1f, 1f, 1f, 0.75f);
            skip.Fill.color = new Color(0f, 0f, 0f, 0.25f);
            tapArea.onClick.AddListener(() => player.SkipShot());
            skip.Button.onClick.AddListener(() => player.SkipAll());
        }

        void LateUpdate()
        {
            // Convert the physical safe-area corner to the popup's canvas coordinates.
            var parent = (RectTransform)skip.Rect.parent;
            var canvas = GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Screen.safeArea.max, camera, out var corner))
                skip.Rect.anchoredPosition = corner - parent.rect.max - new Vector2(20f, 16f);
        }

        void SetStage(Stage s)
        {
            Current = s;
            cinematic.gameObject.SetActive(s == Stage.Cinematic);
            campaignStage.gameObject.SetActive(s == Stage.Campaign);
            if (group.alpha < 1f) Tween.Fade(group, 1f, Animated ? 0.2f : 0f);
        }

        // ------------------------------------------------------------------ Cinématique

        public async UniTask PlayCinematicAsync(IntroCinematicTexts texts, Action<float>? mainTheme, CancellationToken ct)
        {
            skip.SetLabel(texts.Skip);
            SetStage(Stage.Cinematic);
            await player.PlayAsync(mainTheme, ct);
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
