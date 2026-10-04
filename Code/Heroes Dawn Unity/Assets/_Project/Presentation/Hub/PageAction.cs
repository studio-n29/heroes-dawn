#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Une action de page à la fois (entraîner, soigner, escouade…) : erreur affichée en toast, suite en cas de succès ;
    /// rien après l'annulation du jeton (page détruite).
    /// </summary>
    public sealed class PageAction
    {
        readonly BusyGate _gate = new BusyGate();
        readonly IToastService _toasts;
        readonly ILocalization _loc;

        public PageAction(IToastService toasts, ILocalization loc) { _toasts = toasts; _loc = loc; }

        public bool IsBusy => _gate.IsBusy;

        public void Run(Func<CancellationToken, UniTask<Result>> action, CancellationToken ct, Action? onSuccess = null)
        {
            if (ct.IsCancellationRequested || !_gate.TryEnter()) return;
            RunAsync(action, ct, onSuccess).Forget();
        }

        async UniTaskVoid RunAsync(Func<CancellationToken, UniTask<Result>> action, CancellationToken ct, Action? onSuccess)
        {
            Result r;
            try { r = await action(ct); }
            catch (OperationCanceledException) { return; }
            finally { _gate.Exit(); }
            if (ct.IsCancellationRequested) return;
            if (r.IsFailure) { _toasts.Show(_loc.Error(r.Error!), ToastKind.Error); return; }
            onSuccess?.Invoke();
        }
    }
}
