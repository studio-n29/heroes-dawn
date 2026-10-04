#nullable enable
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Message affiché quand le joueur touche un élément BIENTÔT.</summary>
    public static class WipNotice
    {
        public static string Message(ILocalization loc, string featureId)
            => loc.F("wip.toast", loc.T(WipCatalog.Get(featureId).TitleKey));
    }
}
