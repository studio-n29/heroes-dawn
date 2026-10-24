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
    /// Chaque scope reçoit l'emplacement demandé (<see cref="HubLocation"/>) et son propre <see cref="RecruitFlow"/> ; <see cref="MissionLauncher"/> est partagé au niveau du hub.
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
            HubPage.Qg => Make<QgPageView, IQgPageView, QgPagePresenter>(location),
            HubPage.Team => Make<TeamPageView, ITeamPageView, TeamPagePresenter>(location),
            HubPage.MemberSheet => Make<MemberSheetView, IMemberSheetView, MemberSheetPresenter>(location),
            HubPage.Recruit => Make<RecruitPageView, IRecruitPageView, RecruitPagePresenter>(location),
            HubPage.Salon => Make<LoungePageView, ILoungePageView, LoungePagePresenter>(location),
            HubPage.Infirmary => Make<LoungePageView, ILoungePageView, LoungePagePresenter>(location),
            HubPage.Settings => Make<SettingsPageView, ISettingsPageView, SettingsPagePresenter>(location),
            HubPage.CityMap => Make<CityMapPageView, ICityMapPageView, CityMapPagePresenter>(location),
            HubPage.Pass => Make<PassPageView, IPassPageView, PassPagePresenter>(location),
            HubPage.Shop => Make<BoutiquePageView, IBoutiquePageView, BoutiquePagePresenter>(location),
            HubPage.Operations => Make<OperationsPageView, IOperationsPageView, OperationsPagePresenter>(location),
            HubPage.Lab => Make<LabPageView, ILabPageView, LabPagePresenter>(location),
            HubPage.Archives => Make<ArchivesPageView, IArchivesPageView, ArchivesPagePresenter>(location),
            HubPage.Loadout => Make<LoadoutPageView, ILoadoutPageView, LoadoutPagePresenter>(location),
            HubPage.Grimoire => Make<GrimoirePageView, IGrimoirePageView, GrimoirePagePresenter>(location),
            HubPage.Agents => Make<AgentsPageView, IAgentsPageView, AgentsPagePresenter>(location),
            HubPage.Inventory => Make<InventoryPageView, IInventoryPageView, InventoryPagePresenter>(location),
            _ => throw new InvalidOperationException("Page du hub pas encore disponible : " + location.Page),
        };

        IHubPageHandle Wip(HubLocation location, WipFeature feature, string titleKey)
            => Make<WipPageView, IWipPageView, WipPagePresenter>(location, b => b.RegisterInstance(new WipPageArgs(feature, titleKey)));

        /// <summary>Instancie la page <typeparamref name="TView"/> et son présentateur, inscrits dans un scope enfant.</summary>
        IHubPageHandle Make<TView, TIView, TPresenter>(HubLocation location, Action<IContainerBuilder>? extra = null)
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
                    b.RegisterInstance(location);
                    b.Register<RecruitFlow>(Lifetime.Scoped);
                    b.Register<QuestLogFlow>(Lifetime.Scoped);
                    b.Register<AffinityFlow>(Lifetime.Scoped);
                    b.Register<TPresenter>(Lifetime.Scoped);
                    extra?.Invoke(b);
                });
                var presenter = scope.Resolve<TPresenter>();
                presenter.Start();
                return new PageHandle(presenter.Title, presenter as ITickable, scope, go, _host);
            }
            catch
            {
                _host.Despawn(go);
                throw;
            }
        }

        sealed class PageHandle : IHubPageHandle
        {
            readonly ITickable? _tickable;
            readonly IDisposable _scope;
            readonly GameObject _view;
            readonly IPageHost _host;
            bool _disposed;

            public PageHandle(string title, ITickable? tickable, IDisposable scope, GameObject view, IPageHost host)
            {
                Title = title; _tickable = tickable; _scope = scope; _view = view; _host = host;
            }

            public string Title { get; }

            public void Tick()
            {
                if (!_disposed) _tickable?.Tick();
            }

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
