#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Boutique unique en cartes (onglets Boosters, Gemmes et crédits, Cadeau ; l'onglet Héros est la page Recruter) : une tuile par offre,
    /// achat depuis la fiche à droite. Planche 10.2 : les onglets forment une colonne à gauche de la grille, les prix portent l'icône
    /// de leur monnaie au lieu de son code (PR, CR, GM) et l'onglet Boosters est un carrousel horizontal de packs sans cadre de tuile.
    /// </summary>
    public sealed class BoutiquePageView : HdCardGridPageView, IBoutiquePageView
    {
        /// <summary>Rang de l'onglet Boosters dans la colonne d'onglets de la boutique.</summary>
        const int BoostersTab = 1;

        HdShopTabs? _tabs;
        int _active = -1;
        bool _bound, _hooked, _carousel;

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
            // La grille vient d'être reconstruite par la base : son réglage de rangée à retour à la ligne est celui de départ.
            _carousel = false;
            Decorate();
        }

        public void ShowShopTab(int active)
        {
            _active = active;
            _tabs?.Show(active);
            if (_bound) Decorate();
        }

        /// <summary>Affiche les offres puis rhabille leurs prix (même contrat que la grille commune).</summary>
        public new void ShowRows(InfoRowData[] data)
        {
            base.ShowRows(data);
            Decorate();
        }

        /// <summary>Prix des tuiles, de la fiche et de ses boutons : icône de monnaie et montant ; aspect carrousel de l'onglet Boosters.</summary>
        void Decorate()
        {
            if (!_bound) return;
            for (int i = 0; i < 3; i++)
            {
                var b = DetailButton(i).Element;
                HdButtonContent.FillPrice(b, b.text, i == 0 ? "hd-ico--star" : null);
            }
            bool boosters = _active == BoostersTab;
            Q<VisualElement>("page").EnableInClassList("hd-shop--boosters", boosters);
            ApplyCarousel(boosters);
            // Copie de la liste : les pastilles sont insérées dans les tuiles pendant le parcours.
            var tiles = new List<VisualElement>();
            Root.Query<VisualElement>(className: "hd-tile").ForEach(tiles.Add);
            foreach (var tile in tiles)
            {
                EnsureHalo(tile);
                var sub = tile.Q<Label>(className: "hd-tile__sub");
                if (sub != null) PricePill(tile, sub);
            }
            var badge = TryQ<Label>("detailBadge");
            if (badge != null) PriceLabel(badge);
        }

        /// <summary>Onglet Boosters : une seule rangée centrée qui défile à l'horizontale (sans barre visible) ; sinon grille à retour à la ligne.</summary>
        void ApplyCarousel(bool boosters)
        {
            if (_carousel == boosters) return;
            _carousel = boosters;
            var grid = Q<ScrollView>("grid");
            var box = grid.contentContainer;
            grid.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            if (boosters)
            {
                grid.mode = ScrollViewMode.Horizontal;
                grid.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                box.style.flexDirection = FlexDirection.Row;
                box.style.flexWrap = Wrap.NoWrap;
                box.style.justifyContent = Justify.Center;
                box.style.alignItems = Align.Center;
                box.style.minWidth = Length.Percent(100);
                box.style.minHeight = Length.Percent(100);
            }
            else
            {
                grid.mode = ScrollViewMode.Vertical;
                grid.verticalScrollerVisibility = ScrollerVisibility.Auto;
                box.style.flexDirection = FlexDirection.Row;
                box.style.flexWrap = Wrap.Wrap;
                box.style.justifyContent = StyleKeyword.Null;
                box.style.alignItems = StyleKeyword.Null;
                box.style.minWidth = StyleKeyword.Null;
                box.style.minHeight = StyleKeyword.Null;
            }
        }

        /// <summary>Halo doré derrière l'image du pack (visible seulement dans le carrousel, sur le pack choisi).</summary>
        static void EnsureHalo(VisualElement tile)
        {
            if (tile.Q<VisualElement>(className: "hd-tile__halo") != null) return;
            var halo = new VisualElement { pickingMode = PickingMode.Ignore };
            halo.AddToClassList("hd-tile__halo");
            tile.Insert(0, halo);
        }

        /// <summary>
        /// Prix d'une tuile : pastille horizontale (icône de monnaie de 30 px puis valeur) posée sous le nom, à la place du libellé de prix,
        /// pour que l'icône ne chevauche jamais les chiffres. Un texte sans prix reste dans le libellé d'origine.
        /// </summary>
        static void PricePill(VisualElement tile, Label sub)
        {
            var currency = HdButtonContent.Split(sub.text, out string head, out string amount, out string tail);
            bool priced = currency != PriceCurrency.None && head.Length == 0;
            var pill = tile.Q<VisualElement>(className: "hd-tilepill");
            if (!priced)
            {
                if (pill != null) pill.style.display = DisplayStyle.None;
                return;
            }
            Label value;
            VisualElement icon;
            if (pill == null)
            {
                pill = new VisualElement { pickingMode = PickingMode.Ignore };
                pill.AddToClassList("hd-tilepill");
                icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("hd-tilepill__icon");
                value = new Label { pickingMode = PickingMode.Ignore };
                value.AddToClassList("hd-tilepill__value");
                pill.Add(icon); pill.Add(value);
                tile.Insert(tile.IndexOf(sub) + 1, pill);
            }
            else
            {
                icon = pill.Q<VisualElement>(className: "hd-tilepill__icon");
                value = pill.Q<Label>(className: "hd-tilepill__value");
            }
            icon.EnableInClassList("hd-price--credits", currency == PriceCurrency.Credits);
            icon.EnableInClassList("hd-price--pr", currency == PriceCurrency.Pr);
            icon.EnableInClassList("hd-price--gems", currency == PriceCurrency.Gems);
            value.text = tail.Length > 0 ? amount + " · " + tail : amount;
            pill.style.display = DisplayStyle.Flex;
            // Le libellé d'origine garde son texte (lu par les tests) mais n'est plus dessiné.
            sub.style.display = DisplayStyle.None;
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
