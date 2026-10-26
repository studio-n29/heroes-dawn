#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Case de la fiche agent (compétence, objet ou choix d'inventaire) : cadre coloré, illustration, titre, sous-titre.</summary>
    public sealed class SheetTileView : MonoBehaviour
    {
        [SerializeField] Button button = null!;
        [SerializeField] Image frame = null!;
        [SerializeField] Image art = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text sub = null!;
        [SerializeField] GameObject locked = null!;

        public Button Button => button;

        public void Show(SheetTile t)
        {
            bool empty = t.Kind == SheetTileKind.Empty || t.Kind == SheetTileKind.Locked;
            title.text = t.Title;
            sub.text = t.Sub;
            frame.color = empty ? Theme.Outline : t.Frame;
            art.enabled = t.Art != null;
            art.sprite = t.Art;
            if (t.Art != null)
            {
                var fitter = art.GetComponent<AspectRatioFitter>();
                if (fitter != null) fitter.aspectRatio = t.Art.rect.width / t.Art.rect.height;
            }
            locked.SetActive(t.Kind == SheetTileKind.Locked);
            title.color = empty || t.Dim ? Theme.TextMuted : Theme.Text;
            art.color = t.Dim ? Theme.WithAlpha(Color.white, 0.45f) : Color.white;
            if (t.Dim) frame.color = Theme.WithAlpha(frame.color, 0.45f);
        }
    }
}
