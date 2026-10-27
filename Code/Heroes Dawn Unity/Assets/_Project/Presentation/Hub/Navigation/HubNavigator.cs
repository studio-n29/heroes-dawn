#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page courante et pile de retour du hub (C# pur). Un onglet remplace la pile ; une pièce, une fiche ou les réglages s'empilent.
    /// Retour : dépile ; sinon onglet → QG ; sinon QG → « Quitter le jeu ? ».
    /// </summary>
    public sealed class HubNavigator
    {
        public static readonly HubPage[] Tabs = { HubPage.Qg, HubPage.Team, HubPage.Recruit, HubPage.Pass, HubPage.Shop, HubPage.Profile };

        readonly List<HubLocation> _stack = new List<HubLocation> { new HubLocation(HubPage.Qg) };

        public event Action<HubLocation>? Changed;

        /// <summary>Onglet de la boutique affiché (mémorisé entre deux ouvertures). Les héros vivent sur la page Recruter, les autres sections sur la page Boutique.</summary>
        public ShopTab ShopTab { get; set; } = ShopTab.Heroes;

        /// <summary>Onglet de la barre de navigation qui représente une page : Recruter appartient à la Boutique.</summary>
        public static HubPage NavTabOf(HubPage page) => page == HubPage.Recruit ? HubPage.Shop : page;

        /// <summary>Change d'onglet de boutique : les héros ouvrent la page Recruter, les autres sections la page Boutique.</summary>
        public void SelectShopTab(ShopTab tab)
        {
            ShopTab = tab;
            SelectTab(tab == ShopTab.Heroes ? HubPage.Recruit : HubPage.Shop);
        }

        public HubLocation Current => _stack[_stack.Count - 1];
        public IReadOnlyList<HubLocation> Stack => _stack;
        public bool IsOnTab => _stack.Count == 1;

        /// <summary>Carte à mettre en avant à la prochaine ouverture du grimoire (lue puis effacée par la page).</summary>
        public string? FocusCard { get; set; }

        public static bool IsTab(HubPage page) => Array.IndexOf(Tabs, page) >= 0;

        public void SelectTab(HubPage tab)
        {
            if (!IsTab(tab)) throw new ArgumentException("Pas un onglet : " + tab, nameof(tab));
            if (IsOnTab && Current.Page == tab) return;
            _stack.Clear();
            _stack.Add(new HubLocation(tab));
            Changed?.Invoke(Current);
        }

        public void Open(HubPage page, MemberId? member = null, bool fromShop = false)
        {
            if (IsTab(page)) { SelectTab(page); return; }
            var to = new HubLocation(page, member, fromShop);
            if (Current.Equals(to)) return;
            _stack.Add(to);
            Changed?.Invoke(Current);
        }

        /// <summary>
        /// Remplace l'emplacement courant sans recréer de page (pas d'événement <see cref="Changed"/>) : la fiche y note le membre
        /// atteint par ses flèches, pour qu'un raccourci puis Retour y ramène.
        /// </summary>
        public void ReplaceCurrent(HubLocation location)
        {
            if (IsTab(location.Page) != IsTab(Current.Page)) throw new ArgumentException("Un onglet ne remplace qu'un onglet", nameof(location));
            _stack[_stack.Count - 1] = location;
        }

        public BackOutcome Back()
        {
            if (_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 1);
                Changed?.Invoke(Current);
                return BackOutcome.Popped;
            }
            if (Current.Page != HubPage.Qg)
            {
                SelectTab(HubPage.Qg);
                return BackOutcome.WentToQg;
            }
            return BackOutcome.AskQuit;
        }
    }
}
