#nullable enable
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page à venir (UI Toolkit) : nom de la fonction, bandeau BIENTÔT, ce qui est prévu.</summary>
    public sealed class WipPageView : HdView, IWipPageView
    {
        Label _title = null!, _planned = null!, _soon = null!;
        WipPageData? _last;

        protected override void Bind()
        {
            _title = Q<Label>("featureTitle"); _planned = Q<Label>("planned"); _soon = Q<Label>("soon");
            if (_last.HasValue) Show(_last.Value);
        }

        public void Show(WipPageData d)
        {
            _last = d;
            if (_title == null) return;
            _title.text = d.FeatureTitle; _planned.text = d.Planned; _soon.text = d.Soon;
        }
    }
}
