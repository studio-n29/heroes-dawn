#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Mini-jeu (popup prefab) : consignes et jeu de réflexe <see cref="TimingMiniGame"/>.</summary>
    public sealed class MiniGamePopupView : PopupViewBase, IMiniGamePopupView
    {
        [SerializeField] TMP_Text body = null!;
        [SerializeField] TimingMiniGame game = null!;

        public UniTask<bool> PlayAsync(MiniGamePopupData d, CancellationToken ct)
        {
            Open(d.Title);
            body.text = d.Body + "\n<size=26>" + d.Rules + "</size>";
            var done = new UniTaskCompletionSource<bool>();
            ct.Register(() => done.TrySetCanceled());
            game.Begin(d.Stop, d.ScoreFormat, won => done.TrySetResult(won));
            return done.Task;
        }
    }
}
