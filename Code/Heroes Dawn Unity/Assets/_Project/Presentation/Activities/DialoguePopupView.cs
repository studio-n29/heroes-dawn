#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Le dialogue (fenêtre UI Toolkit) : la réplique, les réponses au choix, la progression et la réaction à chaque réponse. Non annulable.</summary>
    public sealed class DialoguePopupView : HdPopup, IDialoguePopupView
    {
        readonly List<UnityEngine.UIElements.Button> _choiceButtons = new List<UnityEngine.UIElements.Button>();
        Label _title = null!, _prompt = null!, _progress = null!, _reaction = null!;
        VisualElement _choices = null!;
        string _pendingTitle = "";

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        public int ChoiceButtons => _choiceButtons.Count;
        public HdButtonRef ChoiceButton(int i) => new HdButtonRef(_choiceButtons[i], this);
        public string Prompt => _prompt != null ? _prompt.text : "";
        protected override VisualElement DefaultFocus => _choiceButtons.Count > 0 ? _choiceButtons[0] : _choices;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _prompt = Q<Label>("prompt"); _progress = Q<Label>("progress");
            _reaction = Q<Label>("reaction"); _choices = Q<VisualElement>("choices");
            _title.text = _pendingTitle;
            _choiceButtons.Clear();
        }

        public async UniTask<ActivityResult> PlayAsync(DialogueRequest r, CancellationToken ct)
        {
            _pendingTitle = r.Title;
            await UniTask.WaitUntil(() => _reaction != null, cancellationToken: ct); // le document se monte au premier affichage
            if (_title != null) _title.text = r.Title;
            _reaction.text = "";
            var g = r.Game;
            while (!g.Finished)
            {
                var line = g.Current!;
                _prompt.text = r.Text(line.PromptKey);
                _progress.text = r.Loc.F("stage.dialogue.progress", g.Index + 1, g.Script.Lines.Count);
                _choices.Clear(); _choiceButtons.Clear();
                var picked = new UniTaskCompletionSource<int>();
                foreach (var c in line.Choices)
                {
                    int index = c.Index;
                    var b = new UnityEngine.UIElements.Button { text = r.Text(c.TextKey) };
                    b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--primary");
                    b.clicked += () => picked.TrySetResult(index);
                    _choices.Add(b); _choiceButtons.Add(b);
                }
                if (_choiceButtons.Count > 0) _choiceButtons[0].schedule.Execute(() => _choiceButtons[0].Focus()).StartingIn(30);
                int choice = await picked.Task.AttachExternalCancellation(ct);
                int score = g.Choose(choice);
                _choices.Clear(); _choiceButtons.Clear();
                _reaction.text = score == 2 ? "<color=#6BE88A>" + r.Loc.T("stage.dialogue.good") + "</color>"
                              : score == 1 ? "<color=#FFD24D>" + r.Loc.T("stage.dialogue.ok") + "</color>"
                              : "<color=#FF6B6B>" + r.Loc.T("stage.dialogue.bad") + "</color>";
                await UniTask.Delay(TimeSpan.FromSeconds(0.9), ignoreTimeScale: true, cancellationToken: ct);
            }
            _prompt.text = g.Success ? r.Loc.T("stage.dialogue.success") : r.Loc.T("stage.dialogue.failure");
            await UniTask.Delay(TimeSpan.FromSeconds(1.4), ignoreTimeScale: true, cancellationToken: ct);
            return g.Result();
        }
    }
}
