#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Événement (popup prefab) : texte et un bouton par choix (widget ChoiceButton).</summary>
    public sealed class NarrativePopupView : PopupViewBase, INarrativePopupView
    {
        [SerializeField] TMP_Text body = null!;
        [SerializeField] RectTransform choices = null!;
        [SerializeField] UiButton choicePrefab = null!;
        readonly List<UiButton> _buttons = new List<UiButton>();
        UniTaskCompletionSource<NarrativeChoice>? _choice;

        public UniTask<NarrativeChoice> ChooseAsync(NarrativePopupData d, CancellationToken ct)
        {
            Open(d.Title);
            body.text = d.Body;
            choices.Clear();
            _buttons.Clear();
            foreach (var c in d.Choices)
            {
                var b = Instantiate(choicePrefab, choices);
                b.SetLabel(c.Label);
                b.SetColors(ToneColors.Of(c.Tone), Theme.TextDark);
                var choice = c.Choice;
                b.Button.onClick.AddListener(() => Choose(choice));
                _buttons.Add(b);
            }
            _choice = new UniTaskCompletionSource<NarrativeChoice>();
            ct.Register(() => _choice.TrySetCanceled());
            return _choice.Task;
        }

        void Choose(NarrativeChoice c)
        {
            foreach (var b in _buttons) b.Interactable = false;
            _choice?.TrySetResult(c);
        }
    }
}
