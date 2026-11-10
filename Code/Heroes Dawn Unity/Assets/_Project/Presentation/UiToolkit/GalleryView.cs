#nullable enable
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>Page « Composants » : tous les composants du thème dans tous leurs états (relecture visuelle, tests de navigation et de débordement).</summary>
    public sealed class GalleryView : HdView
    {
        public Button Primary { get; private set; } = null!;
        public VisualElement PanelNavy { get; private set; } = null!;
        public Label LongText { get; private set; } = null!;

        protected override void Bind()
        {
            Primary = Q<Button>("primary");
            PanelNavy = Q<VisualElement>("panel-navy");
            LongText = Q<Label>("long");
            // Le champ du kit : la TextField d'UI Toolkit habille son champ interne.
            var field = TryQ<TextField>("field");
            field?.Q(className: "unity-base-text-field__input")?.AddToClassList("hd-field");
        }
    }
}
