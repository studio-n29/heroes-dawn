#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Zone sécurisée (popup prefab) : récompenses et « Vers le QG ».</summary>
    public sealed class ZoneCompletePopupView : PopupViewBase, IZoneCompletePopupView
    {
        [SerializeField] TMP_Text rewards = null!;
        [SerializeField] UiButton toHq = null!;
        UniTaskCompletionSource? _done;

        void Awake() => toHq.Button.onClick.AddListener(() => _done?.TrySetResult());

        public UniTask ShowAsync(ZoneCompleteData d, CancellationToken ct)
        {
            Open(d.Title);
            rewards.text = d.Rewards;
            toHq.SetLabel(d.ToHq);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }
    }
}
