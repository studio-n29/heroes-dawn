#nullable enable
using System;
using System.Threading;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>
    /// Base des présentateurs (C# pur) : suit l'affichage de la vue, enregistre son gestionnaire Retour
    /// quand la vue est visible, et coupe tout à <see cref="Dispose"/> (scène déchargée) : jeton annulé,
    /// plus aucun appel à <see cref="OnShown"/>/<see cref="OnHidden"/>.
    /// </summary>
    public abstract class PresenterBase<TView> : IStartable, IDisposable where TView : IView
    {
        readonly BackButtonRouter? _router;
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        bool _started;
        bool _shown;

        protected TView View { get; }
        protected CancellationToken Lifetime { get; }
        protected bool Disposed { get; private set; }

        protected PresenterBase(TView view, BackButtonRouter? router)
        {
            View = view;
            _router = router;
            Lifetime = _cts.Token;
        }

        /// <summary>Gestionnaire Retour de l'écran (null : l'écran ignore Retour).</summary>
        protected virtual IBackHandler? BackHandler => null;

        public void Start()
        {
            if (_started || Disposed) return;
            _started = true;
            View.Shown += HandleShown;
            View.Hidden += HandleHidden;
            OnStart();
            if (View.IsVisible) HandleShown();
        }

        void HandleShown()
        {
            if (Disposed || _shown) return;
            _shown = true;
            var h = BackHandler;
            if (h != null) _router?.Push(h);
            OnShown();
        }

        void HandleHidden()
        {
            if (!_shown) return;
            _shown = false;
            var h = BackHandler;
            if (h != null) _router?.Remove(h);
            if (!Disposed) OnHidden();
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            View.Shown -= HandleShown;
            View.Hidden -= HandleHidden;
            var h = BackHandler;
            if (h != null) _router?.Remove(h);
            _cts.Cancel();
            _cts.Dispose();
            OnDispose();
        }

        protected virtual void OnStart() { }
        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
        protected virtual void OnDispose() { }
    }
}
