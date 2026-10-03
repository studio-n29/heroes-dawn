#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>
    /// File des modales (C# pur) : une seule vue affichée ; une demande arrivée pendant ce temps attend,
    /// puis sa vue est créée à la fermeture de la précédente. Une demande annulée en attente n'est jamais créée.
    /// </summary>
    public sealed class ModalService : IModalService
    {
        sealed class Waiter
        {
            public Func<IPopupView> Spawn = null!;
            public UniTaskCompletionSource<IPopupView> Done = new UniTaskCompletionSource<IPopupView>();
            public bool Cancelled;
        }

        readonly IPopupHost _host;
        readonly Queue<Waiter> _waiting = new Queue<Waiter>();
        IPopupView? _current;

        public ModalService(IPopupHost host) => _host = host;

        public bool AnyOpen => _current != null;

        public async UniTask<TView> OpenAsync<TView>(CancellationToken ct) where TView : class, IPopupView
        {
            ct.ThrowIfCancellationRequested();
            if (_current == null && _waiting.Count == 0)
            {
                _current = _host.Spawn<TView>();
                return (TView)_current;
            }
            var w = new Waiter { Spawn = () => _host.Spawn<TView>() };
            _waiting.Enqueue(w);
            using (ct.Register(() => { w.Cancelled = true; w.Done.TrySetCanceled(); }))
                return (TView)await w.Done.Task;
        }

        public void Close(IPopupView view)
        {
            if (!ReferenceEquals(view, _current)) return;
            _host.Despawn(view);
            _current = null;
            while (_waiting.Count > 0)
            {
                var next = _waiting.Dequeue();
                if (next.Cancelled) continue;
                _current = next.Spawn();
                next.Done.TrySetResult(_current);
                return;
            }
        }

        public bool TryCancelTop()
        {
            var top = _current;
            if (top == null || !top.Cancelable) return false;
            top.RequestCancel();
            return true;
        }

        public async UniTask<bool> ConfirmAsync(ConfirmData data, CancellationToken ct)
        {
            var view = await OpenAsync<IConfirmPopupView>(ct);
            try { return await view.AskAsync(data, ct); }
            finally { Close(view); }
        }
    }
}
