#nullable enable
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation
{
    /// <summary>
    /// Tout ce dont les écrans ont besoin : services, fenêtres, messages et navigation.
    /// Aucun écran n'en référence un autre : on navigue en demandant un état à la machine d'états.
    /// </summary>
    public sealed class UiContext
    {
        public UiServices S { get; }
        public ModalLayer Modal { get; }
        public Toasts Toasts { get; }

        public UiContext(UiServices s, ModalLayer modal, Toasts toasts)
        {
            S = s; Modal = modal; Toasts = toasts;
        }

        /// <summary>Demande une navigation (publie <see cref="NavigateRequested"/>).</summary>
        public void Navigate(GameState target) => S.Navigation.Publish(new NavigateRequested(target));

        public void Toast(string text, ToastKind kind = ToastKind.Info)
        {
            Color c = kind == ToastKind.Reward ? Theme.Success : kind == ToastKind.Warning ? Theme.Fatigue
                    : kind == ToastKind.Error ? Theme.Danger : Theme.Info;
            Toasts.Show(text, c);
        }

        public void Error(DomainError? e) { if (e != null) Toast(Loc.Error(e), ToastKind.Error); }
    }
}
