#nullable enable
using System;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>QG : onglets Équipe / Recrutement, salle des opérations (zone, escouade, infirmerie, déploiement).</summary>
    public interface IHubView : IView
    {
        event Action<HubTab>? TabClicked;
        event Action? LogoutClicked;
        event Action? ResetClicked;
        event Action? HealClicked;
        event Action? DeployClicked;
        event Action<MemberId>? SquadToggleClicked;
        event Action<MemberId>? TrainClicked;
        /// <summary>Recruter le héros d'un emplacement de la boutique.</summary>
        event Action<int>? RecruitClicked;
        event Action? RerollClicked;

        IWalletView Wallet { get; }
        void Show(HubData data);
        void ScrollToTop();
    }
}
