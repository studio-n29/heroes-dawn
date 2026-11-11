#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Boutique unique en cartes (onglets Boosters, Objets, Gemmes et crédits, Cadeau ; l'onglet Héros est la page Recruter) : une tuile par offre, achat depuis la fiche à droite.</summary>
    public sealed class BoutiquePageView : HdCardGridPageView, IBoutiquePageView
    {
        HdShopTabs? _tabs;
        int _active = -1;

        public event Action<int>? ShopTabClicked;

        protected override bool HasShopTabs => true;
        public HdShopTabs Tabs => _tabs!;

        protected override void Bind()
        {
            base.Bind();
            _tabs = new HdShopTabs(TabsHost!, this);
            _tabs.TabClicked += i => ShopTabClicked?.Invoke(i);
            if (_active >= 0) _tabs.Show(_active);
        }

        public void ShowShopTab(int active)
        {
            _active = active;
            _tabs?.Show(active);
        }
    }
}
