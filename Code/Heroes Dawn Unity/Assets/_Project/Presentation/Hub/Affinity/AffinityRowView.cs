#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne d'affinité (widget prefab) : portrait, nom, lien, bonus du duo.</summary>
    public sealed class AffinityRowView : MonoBehaviour
    {
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text agentName = null!;
        [SerializeField] TMP_Text link = null!;
        [SerializeField] TMP_Text bonus = null!;
        [SerializeField] TMP_Text pairText = null!;

        public AffinityRowData Data { get; private set; }

        public void Show(AffinityRowData d)
        {
            Data = d;
            portrait.Show(d.Portrait);
            agentName.text = d.Name;
            link.text = d.Link;
            bonus.text = d.Bonus;
            bonus.color = d.HasBonus ? Theme.Success : Theme.TextMuted;
            pairText.text = d.Pair;
        }
    }
}
