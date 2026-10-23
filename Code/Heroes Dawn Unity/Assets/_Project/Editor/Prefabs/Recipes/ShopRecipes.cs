#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Bandeau d'onglets de la boutique unique : Héros, Boosters, Objets, Gemmes et crédits, Cadeau.</summary>
    public sealed class ShopTabsRecipe : IPrefabRecipe
    {
        public const float Height = 96f;
        public string RelativePath => "Widgets/ShopTabs.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ShopTabs", 1500, Height);
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(1f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.sizeDelta = new Vector2(0f, Height);
            var h = root.HLayout(10, 0, TextAnchor.MiddleLeft);
            h.childForceExpandWidth = true;
            h.childControlWidth = true;
            string[] keys = { "shop.tab.heroes", "shop.tab.boosters", "shop.tab.items", "shop.tab.gems", "shop.tab.gift" };
            var tabs = new Object[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                var b = Ui.LocBtn(root, keys[i], Theme.Surface2, Theme.Text, null!, 30, FontKind.Title, "Tab" + i);
                var le = b.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                le.preferredHeight = Height;
                le.flexibleWidth = 1f;
                tabs[i] = b;
            }
            var view = root.gameObject.AddComponent<ShopTabsView>();
            ctx.WireArray(view, "tabs", tabs);
            return root.gameObject;
        }
    }
}
