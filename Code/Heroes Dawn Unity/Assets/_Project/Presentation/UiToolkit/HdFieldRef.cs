#nullable enable
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>Accès d'une vue à un champ de saisie UI Toolkit, avec la forme de l'ancien champ (« champ.text »).</summary>
    public sealed class HdFieldRef
    {
        public TextField Element { get; }
        public HdFieldRef(TextField element) => Element = element;
        public string text { get => Element.value ?? ""; set => Element.value = value; }
    }
}
