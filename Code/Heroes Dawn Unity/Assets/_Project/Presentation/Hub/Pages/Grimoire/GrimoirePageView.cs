#nullable enable
using System;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Grimoire : la collection de cartes en grille avec fiche détaillée, l'ouverture des boosters, et la recherche par nom.</summary>
    public sealed class GrimoirePageView : HdCardGridPageView, IGrimoirePageView
    {
        public event Action<string>? SearchChanged;

        protected override bool HasSearch => true;

        protected override void Bind()
        {
            base.Bind();
            Search!.textEdition.placeholder = N29.HeroesDawn.Presentation.Common.Loc.T("grimoire.search");
            Search!.RegisterValueChangedCallback(e => SearchChanged?.Invoke(e.newValue));
        }
    }
}
