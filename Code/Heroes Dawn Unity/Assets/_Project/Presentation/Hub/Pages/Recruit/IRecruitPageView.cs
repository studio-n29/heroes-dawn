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
        void Show(RecruitPageData data);
    }
}
