#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Pastille du coût en PM d'un chemin, posée au milieu de son tracé.</summary>
    public sealed class BoardLinkCostView : MonoBehaviour
    {
        [SerializeField] Image background = null!;
        [SerializeField] TMP_Text label = null!;

        public int Cost { get; private set; }

        public void Show(int cost)
        {
            Cost = cost;
            label.text = cost.ToString();
        }

        /// <summary>Mise en évidence quand le chemin fait partie du trajet prévisualisé.</summary>
        public void SetHighlighted(bool on)
        {
            background.color = on ? Theme.Info : Theme.Surface3;
            label.color = on ? Theme.TextDark : Theme.Text;
        }
    }
}
