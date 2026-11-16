#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Colonne d'onglets à icône de la boutique unique (UI Toolkit), posée à gauche des pages Recruter et Boutique.</summary>
    public sealed class HdShopTabs
    {
        static readonly string[] Keys = { "shop.tab.heroes", "shop.tab.boosters", "shop.tab.gems", "shop.tab.gift" };
        /// <summary>Icône de chaque onglet (classes USS de la zone boutique).</summary>
        static readonly string[] Icons = { "hd-ico--team", "hd-ico--booster", "hd-ico--gems", "hd-ico--gift" };
        readonly List<UnityEngine.UIElements.Button> _buttons = new List<UnityEngine.UIElements.Button>();
        readonly Component _owner;

        public event Action<int>? TabClicked;
        public int Active { get; private set; } = -1;
        public int Count => _buttons.Count;

        public HdShopTabs(VisualElement host, Component owner)
        {
            _owner = owner;
            host.Clear();
            for (int i = 0; i < Keys.Length; i++)
            {
                int index = i;
                var b = new UnityEngine.UIElements.Button();
                b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-shoptabs__tab");
                HdButtonContent.Fill(b, Loc.T(Keys[i]), Icons[i]);
                b.clicked += () => TabClicked?.Invoke(index);
                host.Add(b); _buttons.Add(b);
            }
        }

        public HdButtonRef Tab(int index) => new HdButtonRef(_buttons[index], _owner);

        public void Show(int active)
        {
            Active = active;
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].EnableInClassList("hd-btn--primary", i == active);
                _buttons[i].EnableInClassList("hd-btn--secondary", i != active);
            }
        }
    }
}
