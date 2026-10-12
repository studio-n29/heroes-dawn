#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IRecruitPageView : IView
    {
        /// <summary>Carte touchée hors bouton : ouvre la fiche du héros.</summary>
        event Action<int> CardTapped;
        event Action<int> RecruitClicked;
        event Action RerollClicked;
        /// <summary>Registre des agents : comment obtenir chacun des 30 agents.</summary>
        event Action RegistryClicked;
        /// <summary>Onglet de la boutique touché (index de <see cref="ShopTab"/>).</summary>
        event Action<int> ShopTabClicked;
        void ShowShopTab(int active);
        void Show(RecruitPageData data);
    }
}
