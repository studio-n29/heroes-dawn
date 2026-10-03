#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Repos (popup prefab) : Passer / Se reposer.</summary>
    public sealed class RestPopupView : PopupViewBase, IRestPopupView
    {
        [SerializeField] TMP_Text body = null!;
        [SerializeField] UiButton skip = null!;
        [SerializeField] UiButton take = null!;
        UniTaskCompletionSource<bool>? _choice;

        void Awake()
        {
            skip.Button.onClick.AddListener(() => Choose(false));
            take.Button.onClick.AddListener(() => Choose(true));
        }

        void Choose(bool value)
        {
            skip.Interactable = take.Interactable = false; // une seule résolution, même en cas de double clic
            _choice?.TrySetResult(value);
        }

        public UniTask<bool> ChooseAsync(RestPopupData d, CancellationToken ct)
        {
            Open(d.Title);
            body.text = d.Body;
            skip.SetLabel(d.Skip);
            take.SetLabel(d.Take);
            _choice = new UniTaskCompletionSource<bool>();
            ct.Register(() => _choice.TrySetCanceled());
            return _choice.Task;
        }
    }
}
