#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Membre de l'escouade (widget prefab, QG et plateau) : portrait, nom, jauge de fatigue.</summary>
    public sealed class SquadSlotView : MonoBehaviour
    {
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] UiBar fatigue = null!;

        public void Show(SquadSlotData d)
        {
            portrait.Show(d.Portrait);
            if (d.Exhausted) portrait.Disc.color = Theme.Danger;
            memberName.text = d.Name;
            fatigue.SetColor(Theme.FatigueColor(d.Fatigue));
            fatigue.Set(d.Fatigue01, true);
        }
    }
}
