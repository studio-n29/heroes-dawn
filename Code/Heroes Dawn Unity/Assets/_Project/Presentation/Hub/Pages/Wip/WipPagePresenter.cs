#nullable enable
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page à venir : affiche ce qui est prévu pour la fonction.</summary>
    public sealed class WipPagePresenter : PresenterBase<IWipPageView>, IPageTitle
    {
        readonly WipPageArgs _args;
        readonly ILocalization _loc;

        public WipPagePresenter(IWipPageView view, WipPageArgs args, ILocalization loc) : base(view, null)
        {
            _args = args; _loc = loc;
        }

        public string Title => _loc.T(_args.TitleKey);

        protected override void OnShown() => View.Show(new WipPageData
        {
            FeatureTitle = _loc.T(_args.Feature.TitleKey),
            Planned = _loc.T(_args.Feature.PlannedKey),
            Soon = _loc.T("wip.soon"),
        });
    }
}
