#nullable enable
using N29.HeroesDawn.Presentation.Wip;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Fonction affichée par une page WIP et clé du titre de la barre haute (enregistré dans le scope de la page).</summary>
    public sealed class WipPageArgs
    {
        public WipFeature Feature { get; }
        public string TitleKey { get; }
        public WipPageArgs(WipFeature feature, string titleKey) { Feature = feature; TitleKey = titleKey; }
    }
}
