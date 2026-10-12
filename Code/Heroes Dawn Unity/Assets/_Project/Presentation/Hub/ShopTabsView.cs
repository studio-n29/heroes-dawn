#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Bandeau d'onglets de la boutique unique (widget prefab, posé en haut des pages Recruter et Boutique).</summary>
    public sealed class ShopTabsView : MonoBehaviour
    {
        [SerializeField] UiButton[] tabs = Array.Empty<UiButton>();

        public event Action<int>? TabClicked;
        public int Count => tabs.Length;
        public UiButton Tab(int index) => tabs[index];
        /// <summary>Onglet actuellement mis en valeur (tests).</summary>
        public int Active { get; private set; } = -1;

        void Awake()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                tabs[i].Button.onClick.AddListener(() => TabClicked?.Invoke(index));
            }
        }

        public void Show(int active)
        {
            Active = active;
            for (int i = 0; i < tabs.Length; i++)
                tabs[i].SetColors(i == active ? Theme.Navy : Theme.Surface2, i == active ? Theme.TextOnDark : Theme.Navy);
        }
    }
}
