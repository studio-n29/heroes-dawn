#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ce que montre la fenêtre d'ouverture d'un chapitre : intitulé, texte, illustration éventuelle, libellé du bouton.</summary>
    public readonly struct ChapterPopupData
    {
        public readonly string Heading;
        public readonly string Text;
        public readonly Sprite? Art;
        public readonly string Continue;
        public ChapterPopupData(string heading, string text, Sprite? art, string @continue) { Heading = heading; Text = text; Art = art; Continue = @continue; }
    }

    /// <summary>Fenêtre d'ouverture d'un chapitre : se termine quand le joueur continue (ou revient en arrière).</summary>
    public interface IChapterPopupView : IPopupView
    {
        UniTask ShowAsync(ChapterPopupData data, CancellationToken ct);
    }

    /// <summary>Popup (prefab) : image large (masquée sans illustration), intitulé, texte, bouton Continuer. Retour = Continuer.</summary>
    public sealed class ChapterPopupView : MonoBehaviour, IChapterPopupView
    {
        [SerializeField] TMP_Text heading = null!;
        [SerializeField] TMP_Text text = null!;
        [SerializeField] Image art = null!;
        [SerializeField] UiButton continueButton = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        UniTaskCompletionSource? _done;

        public bool Cancelable => true;
        public UiButton ContinueButton => continueButton;

        void Awake() => continueButton.Button.onClick.AddListener(() => _done?.TrySetResult());

        public UniTask ShowAsync(ChapterPopupData data, CancellationToken ct)
        {
            heading.text = data.Heading;
            text.text = data.Text;
            continueButton.SetLabel(data.Continue);
            art.gameObject.SetActive(data.Art != null);
            if (data.Art != null)
            {
                art.sprite = data.Art;
                var fitter = art.GetComponent<AspectRatioFitter>();
                if (fitter != null) fitter.aspectRatio = data.Art.rect.width / data.Art.rect.height;
            }
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public void RequestCancel() => _done?.TrySetResult();
    }
}
