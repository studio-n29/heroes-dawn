#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Affinités d'un agent (popup) : son lien et le bonus de leur duo avec chaque agent de l'effectif ; se termine à la fermeture.</summary>
    public interface IAffinityPopupView : IPopupView
    {
        UniTask ShowAsync(AffinityPopupData data, CancellationToken ct);
    }

    /// <summary>Pop-up d'affinités (prefab) : liste défilante des agents possédés, avec le bonus que donne chaque duo. Retour = Fermer.</summary>
    public sealed class AffinityPopupView : MonoBehaviour, IAffinityPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text subtitle = null!;
        [SerializeField] TMP_Text empty = null!;
        [SerializeField] RectTransform list = null!;
        [SerializeField] AffinityRowView rowPrefab = null!;
        [SerializeField] UiButton close = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        readonly List<AffinityRowView> _rows = new List<AffinityRowView>();
        UniTaskCompletionSource? _done;

        public bool Cancelable => true;
        public IReadOnlyList<AffinityRowView> Rows => _rows;
        public UiButton CloseButton => close;

        void Awake() => close.Button.onClick.AddListener(() => _done?.TrySetResult());

        public UniTask ShowAsync(AffinityPopupData d, CancellationToken ct)
        {
            title.text = d.Title;
            subtitle.text = d.Subtitle;
            close.SetLabel(d.Close);
            list.Clear();
            _rows.Clear();
            foreach (var r in d.Rows)
            {
                var row = Instantiate(rowPrefab, list);
                row.Show(r);
                _rows.Add(row);
            }
            empty.text = d.Empty;
            empty.gameObject.SetActive(d.Rows.Length == 0);
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
