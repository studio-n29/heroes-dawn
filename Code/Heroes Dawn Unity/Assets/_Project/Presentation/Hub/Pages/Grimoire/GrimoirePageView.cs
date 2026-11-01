#nullable enable
using System;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Grimoire : la collection de cartes en grille avec fiche détaillée, l ouverture des boosters, et la recherche par nom.</summary>
    public sealed class GrimoirePageView : CardGridPageView, IGrimoirePageView
    {
        [SerializeField] TMP_InputField search = null!;

        public event Action<string>? SearchChanged;

        protected override void Awake()
        {
            base.Awake();
            search.onValueChanged.AddListener(q => SearchChanged?.Invoke(q));
        }
    }
}
