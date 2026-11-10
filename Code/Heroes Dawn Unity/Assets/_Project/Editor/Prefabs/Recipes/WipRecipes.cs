#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Bandeau BIENTÔT à étirer sur un élément pas encore développé : voile + bande jaune en biais ; capte le toucher.</summary>
    public sealed class WipMarkRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/WipMark.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("WipMark");
            var veil = Ui.Img(root, "Veil", Theme.WithAlpha(Theme.Navy, 0.62f), Sprites.Rounded, true);
            veil.type = UnityEngine.UI.Image.Type.Sliced;
            veil.rectTransform.Stretch();
            var band = Ui.Img(root, "Band", Theme.Primary, Sprites.RoundedSmall);
            band.type = UnityEngine.UI.Image.Type.Sliced;
            band.rectTransform.At(0.5f, 0.5f, 0, 0, 220, 44);
            band.rectTransform.localRotation = Quaternion.Euler(0, 0, 10);
            Ui.LocTxt(band.transform, "wip.soon", 26, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Soon").rectTransform.Stretch();
            root.gameObject.AddComponent<WipMark>();
            return root.gameObject;
        }
    }

    /// <summary>Page à venir (Pass, Boutique, Opérations, Labo, Archives) : nom de la fonction, bandeau BIENTÔT, ce qui est prévu.</summary>
    public sealed class WipPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/WipPage.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<WipPageView>("WipPage", "Wip/Wip.uxml", 10);
    }
}
