#nullable enable
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>Portrait d'un personnage en UI Toolkit : l'illustration de l'agent, ou à défaut son initiale sur la couleur de sa famille.</summary>
    public static class HdPortrait
    {
        public static void Fill(VisualElement frame, Label initial, PortraitData data)
        {
            var family = Theme.FamilyColor(data.Family);
            frame.style.backgroundColor = Color.Lerp(family, Theme.Navy, data.Boss ? 0.45f : 0.25f);
            frame.style.backgroundImage = data.Art != null ? new StyleBackground(Background.FromSprite(data.Art)) : new StyleBackground(StyleKeyword.None);
            frame.style.borderTopColor = frame.style.borderBottomColor = frame.style.borderLeftColor = frame.style.borderRightColor = family;
            initial.text = data.Art != null ? "" : data.Initial;
        }
    }
}
