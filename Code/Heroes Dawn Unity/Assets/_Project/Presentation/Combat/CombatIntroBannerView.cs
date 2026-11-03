#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Bandeau d'un combattant (widget prefab) : portrait, nom, ligne de détail, filet de la couleur de sa famille ; le côté droit est en miroir.</summary>
    public sealed class CombatIntroBannerView : MonoBehaviour
    {
        [SerializeField] Image accent = null!;
        [SerializeField] RectTransform portraitRoot = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text nameText = null!;
        [SerializeField] TMP_Text subText = null!;

        public string Name => nameText.text;
        public string Sub => subText.text;

        public void Setup(CombatIntroFighter fighter, bool left)
        {
            portrait.Show(fighter.Portrait);
            nameText.text = fighter.Name;
            subText.text = fighter.Sub;
            var family = Theme.FamilyColor(fighter.Portrait.Family);
            accent.color = family;
            float x = left ? 0f : 1f;
            // Les alliés ont le portrait à gauche ; les ennemis l'ont à droite et leur texte s'aligne de ce côté.
            portraitRoot.anchorMin = portraitRoot.anchorMax = new Vector2(x, 0.5f);
            portraitRoot.pivot = new Vector2(x, 0.5f);
            portraitRoot.anchoredPosition = new Vector2(left ? 22f : -22f, 0f);
            accent.rectTransform.anchorMin = new Vector2(x, 0f);
            accent.rectTransform.anchorMax = new Vector2(x, 1f);
            accent.rectTransform.pivot = new Vector2(x, 0.5f);
            accent.rectTransform.anchoredPosition = Vector2.zero;
            foreach (var t in new[] { nameText, subText })
            {
                t.alignment = left ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
                t.rectTransform.offsetMin = new Vector2(left ? 170f : 24f, t.rectTransform.offsetMin.y);
                t.rectTransform.offsetMax = new Vector2(left ? -24f : -170f, t.rectTransform.offsetMax.y);
            }
        }
    }
}
