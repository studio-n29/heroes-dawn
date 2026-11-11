#nullable enable
namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Pass saisonnier : 30 niveaux et missions du jour (UI Toolkit).</summary>
    public sealed class PassPageView : HdListPageView, IPassPageView
    {
        protected override bool WideSide => true;
    }
}
