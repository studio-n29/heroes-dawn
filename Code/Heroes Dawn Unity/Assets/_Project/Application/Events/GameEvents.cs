#nullable enable
using System;

namespace N29.HeroesDawn.Application.Events
{
    public enum ToastKind { Info, Reward, Warning, Error }

    /// <summary>
    /// Bus d'événements applicatifs minimal (le M2 le remplacera par MessagePipe).
    /// Les services publient ; la présentation s'abonne. Tout se passe sur le thread principal.
    /// </summary>
    public sealed class GameEvents
    {
        public event Action? WalletChanged;
        public event Action? RosterChanged;
        public event Action<string, ToastKind>? Toast;
        public event Action<bool>? SaveStateChanged; // true = sauvegarde en cours

        public void RaiseWallet() => WalletChanged?.Invoke();
        public void RaiseRoster() => RosterChanged?.Invoke();
        public void RaiseToast(string text, ToastKind kind = ToastKind.Info) => Toast?.Invoke(text, kind);
        public void RaiseSaving(bool saving) => SaveStateChanged?.Invoke(saving);
    }
}
