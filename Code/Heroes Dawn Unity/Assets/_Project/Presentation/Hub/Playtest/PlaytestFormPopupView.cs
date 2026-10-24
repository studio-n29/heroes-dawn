#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Textes du formulaire de retour de playtest.</summary>
    public struct PlaytestFormData
    {
        public string Title;
        public string Intro;
        public string[] Questions;
        public string CommentLabel;
        public string CommentHint;
        public string Send;
        public string Later;
        public string NeedAll;
    }

    /// <summary>Formulaire de retour de playtest (popup) : renvoie les réponses, ou null si le joueur remet à plus tard.</summary>
    public interface IPlaytestFormPopupView : IPopupView
    {
        UniTask<PlaytestAnswers?> AskAsync(PlaytestFormData data, CancellationToken ct);
    }

    /// <summary>Formulaire (prefab) : quatre questions notées de 1 à 5, un champ de commentaire, Envoyer / Plus tard. Retour = Plus tard.</summary>
    public sealed class PlaytestFormPopupView : MonoBehaviour, IPlaytestFormPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text intro = null!;
        [SerializeField] TMP_Text[] questionLabels = System.Array.Empty<TMP_Text>();
        /// <summary>Boutons de note à plat : question 0 (1 à 5), question 1 (1 à 5)…</summary>
        [SerializeField] UiButton[] ratingButtons = System.Array.Empty<UiButton>();
        [SerializeField] TMP_Text commentLabel = null!;
        [SerializeField] TMP_InputField comment = null!;
        [SerializeField] TMP_Text commentHint = null!;
        [SerializeField] UiButton send = null!;
        [SerializeField] UiButton later = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        readonly int[] _ratings = new int[PlaytestFeedback.Questions];
        UniTaskCompletionSource<PlaytestAnswers?>? _answer;
        string _needAll = "";

        public bool Cancelable => true;
        public UiButton SendButton => send;
        public UiButton LaterButton => later;
        public UiButton RatingButton(int question, int rating) => ratingButtons[question * 5 + rating - 1];
        public TMP_InputField CommentField => comment;

        void Awake()
        {
            for (int i = 0; i < ratingButtons.Length; i++)
            {
                int q = i / 5, r = i % 5 + 1;
                ratingButtons[i].SetLabel(r.ToString());
                ratingButtons[i].Button.onClick.AddListener(() => Rate(q, r));
            }
            send.Button.onClick.AddListener(Send);
            later.Button.onClick.AddListener(() => _answer?.TrySetResult(null));
        }

        void Rate(int question, int rating)
        {
            _ratings[question] = rating;
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < ratingButtons.Length; i++)
            {
                bool on = _ratings[i / 5] == i % 5 + 1;
                ratingButtons[i].SetColors(on ? Theme.Primary : Theme.Surface3, on ? Theme.TextDark : Theme.Text);
            }
            send.Interactable = PlaytestFeedback.IsComplete(new PlaytestAnswers(_ratings, ""));
            send.DisabledReason = _needAll;
        }

        void Send()
        {
            var answers = new PlaytestAnswers((int[])_ratings.Clone(), comment.text);
            if (PlaytestFeedback.IsComplete(answers)) _answer?.TrySetResult(answers);
        }

        public UniTask<PlaytestAnswers?> AskAsync(PlaytestFormData d, CancellationToken ct)
        {
            title.text = d.Title;
            intro.text = d.Intro;
            for (int i = 0; i < questionLabels.Length; i++) questionLabels[i].text = i < d.Questions.Length ? d.Questions[i] : "";
            commentLabel.text = d.CommentLabel;
            commentHint.text = d.CommentHint;
            send.SetLabel(d.Send);
            later.SetLabel(d.Later);
            _needAll = d.NeedAll;
            System.Array.Clear(_ratings, 0, _ratings.Length);
            comment.text = "";
            Refresh();
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _answer = new UniTaskCompletionSource<PlaytestAnswers?>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public void RequestCancel() => _answer?.TrySetResult(null);
    }
}
