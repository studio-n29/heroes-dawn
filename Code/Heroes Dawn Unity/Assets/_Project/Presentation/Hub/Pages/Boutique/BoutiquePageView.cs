#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Boutique unique (onglets Boosters, Objets, Gemmes et crédits, Cadeau ; l'onglet Héros est la page Recruter).</summary>
    public sealed class BoutiquePageView : ListPageView, IBoutiquePageView
    {
        [SerializeField] ShopTabsView shopTabs = null!;

        public event Action<int>? ShopTabClicked;

        public ShopTabsView Tabs => shopTabs;

        protected override void Awake()
        {
            base.Awake();
            shopTabs.TabClicked += i => ShopTabClicked?.Invoke(i);
        }

        public void ShowShopTab(int active) => shopTabs.Show(active);
    }
}
