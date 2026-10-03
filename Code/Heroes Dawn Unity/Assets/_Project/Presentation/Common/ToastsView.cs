#nullable enable
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Messages éphémères en haut de l'écran (style « récitatif » jaune) ; chaque message est une instance du prefab Toast.</summary>
    public sealed class ToastsView : MonoBehaviour, IToastService
    {
        const int Max = 4;
        [SerializeField] RectTransform stack = null!;
        [SerializeField] ToastItemView toastPrefab = null!;

        public void Show(string text, ToastKind kind)
        {
            if (string.IsNullOrEmpty(text)) return;
            while (stack.childCount >= Max)
            {
                var old = stack.GetChild(0).gameObject;
                old.transform.SetParent(null);
                Destroy(old);
            }
            var item = Instantiate(toastPrefab, stack);
            item.Play(text, AccentOf(kind));
        }

        static Color AccentOf(ToastKind kind) => kind == ToastKind.Reward ? Theme.Success
                                              : kind == ToastKind.Warning ? Theme.Fatigue
                                              : kind == ToastKind.Error ? Theme.Danger : Theme.Info;
    }
}
