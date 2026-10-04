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
            var veil = Ui.Img(root, "Veil", new Color(0.06f, 0.07f, 0.09f, 0.62f), Sprites.Rounded, true);
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

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("WipPage");
            var fill = Ui.Panel(rt, "Card", Theme.Surface, 5f, 10f, true);
            var card = fill.Frame().At(0.5f, 0.5f, 0, 0, 1200, 560);
            var title = Ui.Txt(fill.transform, "", 64, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Feature");
            title.rectTransform.At(0.5f, 1f, 0, -60, 1080, 90);
            title.Inked();
            var band = Ui.Img(fill.transform, "SoonBand", Theme.Primary, Sprites.RoundedSmall);
            band.type = UnityEngine.UI.Image.Type.Sliced;
            band.rectTransform.At(0.5f, 1f, 0, -170, 300, 60);
            band.rectTransform.localRotation = Quaternion.Euler(0, 0, -4);
            var soon = Ui.Txt(band.transform, "", 36, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Soon");
            soon.rectTransform.Stretch();
            var planned = Ui.Txt(fill.transform, "", 32, Theme.Text, TextAnchor.UpperCenter, FontKind.Body, "Planned");
            planned.rectTransform.At(0.5f, 0f, 0, 60, 1000, 220);
            var view = rt.gameObject.AddComponent<WipPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "featureTitle", title);
            ctx.Wire(view, "planned", planned);
            ctx.Wire(view, "soon", soon);
            return rt.gameObject;
        }
    }
}
