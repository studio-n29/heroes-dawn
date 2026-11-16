#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

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

    /// <summary>Formulaire (UI Toolkit) : quatre questions notées de 1 à 5, un champ de commentaire, Envoyer / Plus tard. Retour = Plus tard.</summary>
    public sealed class PlaytestFormPopupView : HdPopup, IPlaytestFormPopupView
    {
        readonly int[] _ratings = new int[PlaytestFeedback.Questions];
        readonly List<Label> _questionLabels = new List<Label>();
        readonly List<UnityEngine.UIElements.Button> _rating = new List<UnityEngine.UIElements.Button>();
        Label _title = null!, _intro = null!, _commentLabel = null!, _commentHint = null!;
        TextField _comment = null!;
        UnityEngine.UIElements.Button _send = null!, _later = null!;
        UniTaskCompletionSource<PlaytestAnswers?>? _answer;
        PlaytestFormData? _last;
        string _needAll = "";

        public override bool Cancelable => true;
        public HdButtonRef SendButton { get; private set; } = null!;
        public HdButtonRef LaterButton { get; private set; } = null!;
        public HdButtonRef RatingButton(int question, int rating) => new HdButtonRef(_rating[question * 5 + rating - 1], this);
        public TextField CommentField => _comment;
        protected override VisualElement DefaultFocus => _rating.Count > 0 ? _rating[0] : _later;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _intro = Q<Label>("intro"); _commentLabel = Q<Label>("commentLabel"); _commentHint = Q<Label>("commentHint");
            _comment = Q<TextField>("comment");
            _send = Q<UnityEngine.UIElements.Button>("send"); _later = Q<UnityEngine.UIElements.Button>("later");
            SendButton = new HdButtonRef(_send, this); LaterButton = new HdButtonRef(_later, this);
            _send.clicked += Send;
            _later.clicked += () => _answer?.TrySetResult(null);
            var box = Q<VisualElement>("questions");
            box.Clear(); _questionLabels.Clear(); _rating.Clear();
            for (int q = 0; q < PlaytestFeedback.Questions; q++)
            {
                // Planche 16.4 : la question à gauche, ses cinq notes en cases à droite sur la même ligne.
                var line = new VisualElement(); line.AddToClassList("hd-poll__line");
                var label = new Label(); label.AddToClassList("hd-pop__text"); label.AddToClassList("hd-poll__question");
                var row = new VisualElement(); row.AddToClassList("hd-poll__row");
                for (int r = 1; r <= 5; r++)
                {
                    int question = q, rating = r;
                    var b = new UnityEngine.UIElements.Button { text = r.ToString() };
                    b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-poll__rate"); b.AddToClassList("hd-btn--plain");
                    b.clicked += () => { _ratings[question] = rating; Refresh(); };
                    row.Add(b); _rating.Add(b);
                }
                line.Add(label); line.Add(row);
                box.Add(line);
                _questionLabels.Add(label);
            }
            if (_last.HasValue) Fill(_last.Value);
        }

        void Refresh()
        {
            for (int i = 0; i < _rating.Count; i++)
            {
                bool on = _ratings[i / 5] == i % 5 + 1;
                _rating[i].EnableInClassList("hd-btn--primary", on);
                _rating[i].EnableInClassList("hd-btn--secondary", !on);
            }
            _send.SetEnabled(PlaytestFeedback.IsComplete(new PlaytestAnswers(_ratings, "")));
            _send.tooltip = _needAll;
        }

        void Send()
        {
            var answers = new PlaytestAnswers((int[])_ratings.Clone(), _comment.value);
            if (PlaytestFeedback.IsComplete(answers)) _answer?.TrySetResult(answers);
        }

        void Fill(PlaytestFormData d)
        {
            _title.text = d.Title; _intro.text = d.Intro;
            for (int i = 0; i < _questionLabels.Count; i++) _questionLabels[i].text = i < d.Questions.Length ? d.Questions[i] : "";
            _commentLabel.text = d.CommentLabel; _commentHint.text = d.CommentHint;
            _send.text = d.Send; _later.text = d.Later;
            _needAll = d.NeedAll;
            System.Array.Clear(_ratings, 0, _ratings.Length);
            _comment.value = "";
            Refresh();
        }

        public UniTask<PlaytestAnswers?> AskAsync(PlaytestFormData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _answer = new UniTaskCompletionSource<PlaytestAnswers?>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public override void RequestCancel() => _answer?.TrySetResult(null);
    }
}
