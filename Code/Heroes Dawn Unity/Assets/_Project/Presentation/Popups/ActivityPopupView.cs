#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Fenêtre d'activité (popup prefab) : description, décor, réglages, niveau généré, et deux boutons pour simuler le résultat.</summary>
    public sealed class ActivityPopupView : PopupViewBase, IActivityPopupView
    {
        [SerializeField] TMP_Text body = null!;
        [SerializeField] TMP_Text details = null!;
        [SerializeField] TMP_Text wip = null!;
        [SerializeField] UiButton win = null!;
        [SerializeField] UiButton fail = null!;

        public UnityEngine.UI.Button WinButton => win.Button;
        public UnityEngine.UI.Button FailButton => fail.Button;

        public UniTask<ActivityResult> PlayAsync(ActivityPopupData d, CancellationToken ct)
        {
            Open(d.Title);
            body.text = d.Body;
            details.text = string.Join("\n", d.Lines);
            wip.text = d.Wip;
            win.SetLabel(d.WinLabel);
            fail.SetLabel(d.FailLabel);
            var done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => done.TrySetCanceled());
            win.Button.onClick.RemoveAllListeners();
            fail.Button.onClick.RemoveAllListeners();
            win.Button.onClick.AddListener(() => done.TrySetResult(ActivityResult.Won(d.WinScore)));
            fail.Button.onClick.AddListener(() => done.TrySetResult(ActivityResult.Lost(d.FailScore)));
            return done.Task;
        }
    }
}
