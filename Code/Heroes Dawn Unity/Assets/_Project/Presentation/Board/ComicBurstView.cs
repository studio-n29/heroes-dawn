#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Étoile de bande dessinée au centre de l'écran : « COMBAT ! », « BANG ! »… Elle surgit, tremble, puis s'efface.</summary>
    public sealed class ComicBurstView : MonoBehaviour
    {
        [SerializeField] RectTransform root = null!;
        [SerializeField] Image shadow = null!;
        [SerializeField] Image burst = null!;
        [SerializeField] TMP_Text label = null!;

        const float PopTime = 0.22f, HoldTime = 0.55f, OutTime = 0.2f;

        public bool Playing { get; private set; }
        public string Text => label.text;

        void Awake() => root.gameObject.SetActive(false);

        public async UniTask PlayAsync(ComicCueData cue, CancellationToken ct)
        {
            if (Playing) return;
            Playing = true;
            try
            {
                label.text = cue.Text;
                burst.color = cue.Color;
                label.color = Theme.TextDark;
                root.localRotation = Quaternion.Euler(0, 0, cue.Tilt);
                root.localScale = Vector3.zero;
                root.gameObject.SetActive(true);
                root.SetAsLastSibling();

                await Run(PopTime, k => root.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, k), Tween.EaseOutBack, ct);
                if (!MotionSettings.Reduced) Tween.Shake(root, 7f, HoldTime);
                await UniTask.Delay(System.TimeSpan.FromSeconds(HoldTime), ignoreTimeScale: true, cancellationToken: ct);
                await Run(OutTime, k => root.localScale = Vector3.one * Mathf.Lerp(1f, 1.35f, k) , Tween.Linear, ct);
            }
            finally
            {
                if (root != null) { root.localScale = Vector3.zero; root.gameObject.SetActive(false); }
                Playing = false;
            }
        }

        UniTask Run(float seconds, System.Action<float> step, System.Func<float, float> ease, CancellationToken ct)
        {
            var done = new UniTaskCompletionSource();
            var co = Tween.Run(seconds, step, () => done.TrySetResult(), this, ease);
            ct.Register(() => { Tween.Stop(co); done.TrySetCanceled(); });
            return done.Task;
        }
    }
}
