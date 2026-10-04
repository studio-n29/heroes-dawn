using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page à venir (prefab Pages/WipPage) : Pass, Boutique, Opérations, Labo, Archives.</summary>
    public interface IWipPageView : IView
    {
        void Show(WipPageData data);
    }
}
