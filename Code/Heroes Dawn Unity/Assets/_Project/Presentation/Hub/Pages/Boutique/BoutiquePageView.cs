#nullable enable
using System;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Boutique unique en cartes (onglets Boosters, Gemmes et crédits, Cadeau ; l'onglet Héros est la page Recruter) : une tuile par offre,
    /// achat depuis la fiche à droite. Planche 10.2 : les onglets forment une colonne à gauche de la grille et les prix portent l'icône
    /// de leur monnaie au lieu de son code (PR, CR, GM).
    /// </summary>
    public sealed class BoutiquePageView : HdCardGridPageView, IBoutiquePageView
    {
        HdShopTabs? _tabs;
        int _active = -1;
        bool _bound, _hooked;

        public event Action<int>? ShopTabClicked;

        protected override bool HasShopTabs => true;
        public HdShopTabs Tabs => _tabs!;

        protected override void Bind()
        {
            base.Bind();
            // La page reprend le modèle commun des grilles : seule la boutique place ses onglets en colonne, à gauche de la grille.
            var page = Q<VisualElement>("page");
            page.AddToClassList("hd-shop");
            var host = TabsHost!;
            host.RemoveFromHierarchy();
            page.Insert(0, host);
            _tabs = new HdShopTabs(host, this);
            _tabs.TabClicked += i => ShopTabClicked?.Invoke(i);
            if (_active >= 0) _tabs.Show(_active);
            // Choisir une tuile redessine la grille et la fiche : les prix sont rhabillés ensuite.
            if (!_hooked) { _hooked = true; RowSelected += _ => Decorate(); }
            _bound = true;
            Decorate();
        }

        public void ShowShopTab(int active)
        {
            _active = active;
            _tabs?.Show(active);
        }

        /// <summary>Affiche les offres puis rhabille leurs prix (même contrat que la grille commune).</summary>
        public new void ShowRows(InfoRowData[] data)
        {
            base.ShowRows(data);
            Decorate();
        }

        /// <summary>Prix des tuiles, de la fiche et de ses boutons : icône de monnaie et montant.</summary>
        void Decorate()
        {
            if (!_bound) return;
            for (int i = 0; i < 3; i++)
            {
                var b = DetailButton(i).Element;
                HdButtonContent.FillPrice(b, b.text, i == 0 ? "hd-ico--star" : null);
            }
            Root.Query<Label>(className: "hd-tile__sub").ForEach(PriceLabel);
            var badge = TryQ<Label>("detailBadge");
            if (badge != null) PriceLabel(badge);
        }

        /// <summary>Un libellé déjà rhabillé (texte inchangé depuis) est laissé tel quel.</summary>
        static void PriceLabel(Label label)
        {
            if (label.userData is string done && done == label.text) return;
            HdButtonContent.Price(label, label.text);
            label.userData = label.text;
        }
    }
}
