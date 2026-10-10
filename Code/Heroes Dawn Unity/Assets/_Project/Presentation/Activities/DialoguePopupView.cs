#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Le dialogue (prefab) : la réplique, les réponses au choix, la progression et la réaction à chaque réponse.</summary>
    public sealed class DialoguePopupView : PopupViewBase, IDialoguePopupView
    {
        [SerializeField] TMP_Text prompt = null!;
        [SerializeField] TMP_Text progress = null!;
        [SerializeField] TMP_Text reaction = null!;
        [SerializeField] RectTransform choiceRow = null!;
        [SerializeField] UiButton choicePrefab = null!;

        public int ChoiceButtons => choiceRow.childCount;
        public UnityEngine.UI.Button ChoiceButton(int i) => choiceRow.GetChild(i).GetComponent<UiButton>().Button;
        public string Prompt => prompt.text;

        public async UniTask<ActivityResult> PlayAsync(DialogueRequest r, CancellationToken ct)
        {
            Open(r.Title);
            reaction.text = "";
            var g = r.Game;
            while (!g.Finished)
            {
                var line = g.Current!;
                prompt.text = r.Text(line.PromptKey);
                progress.text = r.Loc.F("stage.dialogue.progress", g.Index + 1, g.Script.Lines.Count);
                choiceRow.Clear();
                var picked = new UniTaskCompletionSource<int>();
                foreach (var c in line.Choices)
                {
                    int index = c.Index;
                    var b = Instantiate(choicePrefab, choiceRow);
                    b.SetLabel(r.Text(c.TextKey));
                    b.Button.onClick.AddListener(() => picked.TrySetResult(index));
                }
                int choice = await picked.Task.AttachExternalCancellation(ct);
                int score = g.Choose(choice);
                choiceRow.Clear();
                reaction.text = score == 2 ? "<color=#6BE88A>" + r.Loc.T("stage.dialogue.good") + "</color>"
                              : score == 1 ? "<color=#FFD24D>" + r.Loc.T("stage.dialogue.ok") + "</color>"
                              : "<color=#FF6B6B>" + r.Loc.T("stage.dialogue.bad") + "</color>";
                await UniTask.Delay(TimeSpan.FromSeconds(0.9), ignoreTimeScale: true, cancellationToken: ct);
            }
            prompt.text = g.Success ? r.Loc.T("stage.dialogue.success") : r.Loc.T("stage.dialogue.failure");
            await UniTask.Delay(TimeSpan.FromSeconds(1.4), ignoreTimeScale: true, cancellationToken: ct);
            return g.Result();
        }
    }
}
