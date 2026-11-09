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
        [SerializeField] TMP_Text objectivesTitle = null!;
        [SerializeField] TMP_Text objectives = null!;
        [SerializeField] TMP_Text rewardsTitle = null!;
        [SerializeField] UiButton toHq = null!;
        UniTaskCompletionSource? _done;

        public UnityEngine.UI.Button ToHqButton => toHq.Button;

        void Awake() => toHq.Button.onClick.AddListener(() => _done?.TrySetResult());

        public UniTask ShowAsync(ZoneCompleteData d, CancellationToken ct)
        {
            Open(d.Title);
            rewards.text = d.Rewards;
            objectivesTitle.text = d.ObjectivesTitle;
            objectives.text = d.Objectives;
            rewardsTitle.text = d.RewardsTitle;
            toHq.SetLabel(d.ToHq);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }
    }
}
