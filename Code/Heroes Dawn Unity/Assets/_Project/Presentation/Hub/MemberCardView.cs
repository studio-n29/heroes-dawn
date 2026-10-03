#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>En-tête commun des cartes de membre : bande de famille, portrait, nom, rareté, rôle, niveau, stats, compétences.</summary>
    public abstract class MemberCardView : MonoBehaviour
    {
        [SerializeField] protected Image fill = null!;
        [SerializeField] Image strip = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] TMP_Text role = null!;
        [SerializeField] TMP_Text level = null!;
        [SerializeField] TMP_Text stats = null!;
        [SerializeField] TMP_Text skills = null!;

        protected void ShowCard(MemberCardData d)
        {
            strip.color = Theme.FamilyColor(d.Family);
            portrait.Show(d.Portrait);
            memberName.text = d.Name;
            rarity.text = d.RarityText;
            rarity.color = Theme.RarityColor(d.Rarity);
            role.text = d.RoleLine;
            level.text = d.LevelText;
            stats.text = d.Stats;
            skills.text = d.Skills;
            fill.color = Theme.Surface;
        }
    }
}
