#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Crée chaque page du hub dans un scope VContainer enfant (vue instanciée depuis le catalogue + présentateur),
    /// démarre le présentateur à la main (les points d'entrée d'un scope enfant ne démarrent pas seuls) ; le handle détruit tout.
    /// </summary>
    public sealed class HubPageFactory : IHubPageFactory
    {
        readonly IObjectResolver _resolver;
        readonly IPageHost _host;
        readonly ScreenCatalog _catalog;

        public HubPageFactory(IObjectResolver resolver, IHubShellView shell, ScreenCatalog catalog)
        {
            _resolver = resolver; _host = shell.Pages; _catalog = catalog;
        }

        public IHubPageHandle Create(HubLocation location) => location.Page switch
        {
            HubPage.Pass => Wip(WipCatalog.Pass, "page.pass"),
            HubPage.Shop => Wip(WipCatalog.Shop, "page.shop"),
            HubPage.Operations => Wip(WipCatalog.Operations, "page.operations"),
            HubPage.Lab => Wip(WipCatalog.Lab, "page.lab"),
            HubPage.Archives => Wip(WipCatalog.Archives, "page.archives"),
            _ => throw new InvalidOperationException("Page du hub pas encore disponible : " + location.Page),
        };

        IHubPageHandle Wip(WipFeature feature, string titleKey)
            => Make<WipPageView, IWipPageView, WipPagePresenter>(b => b.RegisterInstance(new WipPageArgs(feature, titleKey)));

        /// <summary>Instancie la page <typeparamref name="TView"/> et son présentateur, inscrits dans un scope enfant.</summary>
        IHubPageHandle Make<TView, TIView, TPresenter>(Action<IContainerBuilder>? extra = null)
            where TView : Component, TIView
            where TIView : class
            where TPresenter : class, IPageTitle, IStartable
        {
            var go = _host.Spawn(_catalog.PageFor<TView>());
            try
            {
                var view = go.GetComponent<TView>();
                var scope = _resolver.CreateScope(b =>
                {
                    b.RegisterInstance<TIView>(view);
                    b.Register<TPresenter>(Lifetime.Scoped);
                    extra?.Invoke(b);
                });
                var presenter = scope.Resolve<TPresenter>();
                presenter.Start();
                return new PageHandle(presenter.Title, scope, go, _host);
            }
            catch
            {
                _host.Despawn(go);
                throw;
            }
        }

        sealed class PageHandle : IHubPageHandle
        {
            readonly IDisposable _scope;
            readonly GameObject _view;
            readonly IPageHost _host;
            bool _disposed;

            public PageHandle(string title, IDisposable scope, GameObject view, IPageHost host)
            {
                Title = title; _scope = scope; _view = view; _host = host;
            }

            public string Title { get; }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _scope.Dispose();
                _host.Despawn(_view);
            }
        }
    }
}
