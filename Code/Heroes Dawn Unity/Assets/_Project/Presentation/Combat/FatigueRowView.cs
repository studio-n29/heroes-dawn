#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Ligne de fatigue des résultats (widget prefab) : nom, jauge animée avant → après, écart.</summary>
    public sealed class FatigueRowView : MonoBehaviour
    {
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] UiBar bar = null!;
        [SerializeField] TMP_Text delta = null!;

        public void Show(FatigueRowData d)
        {
            memberName.text = d.Name;
            bar.SetColor(Theme.FatigueColor(d.After));
            bar.Set(d.Before01, true);
            bar.Set(d.After01);
            bar.SetLabel(d.Label);
            delta.text = d.Delta;
        }
    }
}
