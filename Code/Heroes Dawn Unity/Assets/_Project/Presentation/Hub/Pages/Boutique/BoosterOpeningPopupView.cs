#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Ouverture d'un booster (popup plein écran) : le paquet attend un toucher, s'agite puis éclate en un flash, les cartes sortent et se
    /// retournent une à une (éclat selon la rareté) ; un toucher pendant la révélation retourne tout, « Continuer » ferme.
    /// </summary>
    public sealed class BoosterOpeningPopupView : MonoBehaviour, IBoosterOpeningPopupView
    {
        public enum Phase { Idle, Sealed, Opening, Revealing, Done }

        [SerializeField] CanvasGroup group = null!;
        [SerializeField] Button tapArea = null!;
        [SerializeField] RectTransform pack = null!;
        [SerializeField] Image flash = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] RectTransform cardRow = null!;
        [SerializeField] BoosterCardView cardPrefab = null!;
        [SerializeField] UiButton skip = null!;
        [SerializeField] UiButton continueButton = null!;

        readonly List<BoosterCardView> _cards = new List<BoosterCardView>();
        UniTaskCompletionSource? _done;

        public bool Cancelable => true;
        public void RequestCancel() => _done?.TrySetResult();

        public Phase Current { get; private set; } = Phase.Idle;
        public IReadOnlyList<BoosterCardView> Cards => _cards;
        public Button TapButton => tapArea;
        public Button SkipButton => skip.Button;
        public Button ContinueButton => continueButton.Button;
        public bool ContinueVisible => continueButton.gameObject.activeSelf;

        static bool Animated => !MotionSettings.Reduced && UnityEngine.Application.isPlaying;

        void Awake()
        {
            tapArea.onClick.AddListener(OnTap);
            skip.Button.onClick.AddListener(RevealAll);
            continueButton.Button.onClick.AddListener(() => _done?.TrySetResult());
        }

        public UniTask ShowAsync(BoosterOpeningData d, CancellationToken ct)
        {
            title.text = d.Title;
            hint.text = d.TapHint;
            skip.SetLabel(d.SkipLabel);
            continueButton.SetLabel(d.ContinueLabel);
            cardRow.Clear();
            _cards.Clear();
            foreach (var c in d.Cards)
            {
                var card = Instantiate(cardPrefab, cardRow);
                card.Setup(c, d.NewLabel);
                card.gameObject.SetActive(false);
                _cards.Add(card);
            }
            flash.color = new Color(1f, 1f, 1f, 0f);
            pack.gameObject.SetActive(true);
            pack.localScale = Vector3.one;
            continueButton.gameObject.SetActive(false);
            skip.gameObject.SetActive(false);
            hint.gameObject.SetActive(true);
            Current = Phase.Sealed;
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(pack, 0.3f);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        void OnTap()
        {
            if (Current == Phase.Sealed) StartOpening();
            else if (Current == Phase.Revealing) RevealAll();
        }

        /// <summary>Le paquet s'agite, un flash couvre l'écran, les cartes sortent face cachée.</summary>
        void StartOpening()
        {
            Current = Phase.Opening;
            hint.gameObject.SetActive(false);
            if (!Animated)
            {
                ShowCards(false);
                Current = Phase.Revealing;
                RevealAll();
                return;
            }
            Tween.Shake(pack, 16f, 0.45f);
            Tween.Run(0.45f, k => { if (flash != null) flash.color = new Color(1f, 1f, 1f, k * k); }, () =>
            {
                if (this == null) return;
                pack.gameObject.SetActive(false);
                ShowCards(true);
                Tween.Run(0.35f, k => { if (flash != null) flash.color = new Color(1f, 1f, 1f, 1f - k); }, null, this, Tween.Linear);
                StartReveals();
            }, this, Tween.Linear);
        }

        void ShowCards(bool animate)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].gameObject.SetActive(true);
                if (animate) Fx.PopInAfter(_cards[i].transform, Stagger.DelayFor(i, 0.1f));
            }
            skip.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardRow);
        }

        void StartReveals()
        {
            Current = Phase.Revealing;
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                Tween.Delay(0.6f + i * 0.6f, () =>
                {
                    if (this == null || Current != Phase.Revealing) return;
                    card.Reveal(true);
                    CheckDone(0.4f);
                }, this);
            }
        }

        /// <summary>Retourne toutes les cartes tout de suite (toucher pendant la révélation, « Passer »).</summary>
        public void RevealAll()
        {
            if (Current == Phase.Sealed) { StartOpening(); return; }
            if (Current == Phase.Done || Current == Phase.Idle) return;
            if (pack.gameObject.activeSelf) pack.gameObject.SetActive(false);
            foreach (var c in _cards) { c.gameObject.SetActive(true); c.transform.localScale = Vector3.one; c.Reveal(false); }
            flash.color = new Color(1f, 1f, 1f, 0f);
            Current = Phase.Revealing;
            CheckDone(0f);
        }

        void CheckDone(float afterSeconds)
        {
            foreach (var c in _cards) if (!c.Revealed) return;
            if (Current == Phase.Done) return;
            Current = Phase.Done;
            if (afterSeconds <= 0f) EndReveal();
            else Tween.Delay(afterSeconds, EndReveal, this);
        }

        void EndReveal()
        {
            if (this == null) return;
            skip.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(true);
            Tween.PopIn(continueButton.transform, 0.22f);
        }
    }
}
