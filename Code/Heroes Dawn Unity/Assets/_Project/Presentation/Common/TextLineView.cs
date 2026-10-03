#nullable enable
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Ligne de texte d'une liste (objectifs du QG et du plateau, journal).</summary>
    public sealed class TextLineView : MonoBehaviour
    {
        [SerializeField] TMP_Text label = null!;

        public void Show(string text, Color color)
        {
            label.text = text;
            label.color = color;
        }
    }
}
