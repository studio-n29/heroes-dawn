#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ligne de texte de liste (objectifs, journal).</summary>
    public sealed class TextLineRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/TextLine.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("TextLine", 540, 32);
            var t = Ui.Txt(root, "", 24, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Label");
            t.rectTransform.Stretch();
            root.Pref(540, 32);
            var view = root.gameObject.AddComponent<TextLineView>();
            ctx.Wire(view, "label", t);
            return root.gameObject;
        }
    }

    /// <summary>Membre de l'escouade : portrait, nom, jauge de fatigue (reprise de HubScreen.RefreshOps).</summary>
    public sealed class SquadSlotRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/SquadSlot.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var cell = PrefabContext.NewRoot("SquadSlot", 125, 150);
            cell.Pref(125, 150);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", cell, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 1f, 0, 0, 90, 90);
            var name = Ui.Txt(cell, "", 22, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Body, "Name");
            name.rectTransform.At(0.5f, 1f, 0, -92, 125, 26);
            var bar = Ui.Bar(cell, "Fatigue", Theme.Success, Theme.SurfaceDark);
            ((RectTransform)bar.transform).At(0.5f, 1f, 0, -122, 110, 16);
            var view = cell.gameObject.AddComponent<SquadSlotView>();
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "fatigue", bar);
            return cell.gameObject;
        }
    }
}
