#nullable enable
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public sealed class WipPageView : ViewBase, IWipPageView
    {
        [SerializeField] TMP_Text featureTitle = null!;
        [SerializeField] TMP_Text planned = null!;
        [SerializeField] TMP_Text soon = null!;

        public void Show(WipPageData d)
        {
            featureTitle.text = d.FeatureTitle;
            planned.text = d.Planned;
            soon.text = d.Soon;
        }
    }
}
