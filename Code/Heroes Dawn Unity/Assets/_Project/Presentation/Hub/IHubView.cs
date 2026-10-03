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
        event Action<MemberId>? RecruitClicked;

        IWalletView Wallet { get; }
        void Show(HubData data);
        void ScrollToTop();
    }
}
