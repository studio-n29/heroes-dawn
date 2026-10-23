#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Première connexion du joueur (remplaçable dans les tests de bout en bout).</summary>
    public interface IOnboardingFlow
    {
        UniTask RunIfNeededAsync(CancellationToken ct);
    }

    /// <summary>
    /// Première connexion : si le profil n'a encore rien joué, ouvre la popup plein écran (cinématique de lancement, choix du personnage joueur)
    /// et enregistre le résultat. Sans effet pour un profil qui a déjà joué. Le thème principal du jeu est coupé pendant la cinématique
    /// et revient pour sa dernière séquence (l'Aube).
    /// </summary>
    public sealed class OnboardingFlow : IOnboardingFlow
    {
        readonly OnboardingGuide _onboarding;
        readonly IModalService _modals;
        readonly ILocalization _loc;
        readonly IAudioService? _audio;

        public OnboardingFlow(OnboardingGuide onboarding, IModalService modals, ILocalization loc, IAudioService? audio = null)
        {
            _onboarding = onboarding; _modals = modals; _loc = loc; _audio = audio;
        }

        public bool Needed => _onboarding.Needed;

        public async UniTask RunIfNeededAsync(CancellationToken ct)
        {
            if (!_onboarding.Needed) return;
            var view = await _modals.OpenAsync<IIntroPopupView>(ct);
            try
            {
                await view.PlayCinematicAsync(CinematicScript.Shots, new IntroCinematicTexts { Skip = _loc.T("intro.skip") }, SetMainTheme, ct);
                SetMainTheme(1f);
                string campaign = await view.ChooseCampaignAsync(new ChooseCampaignData
                {
                    Title = _loc.T("onboarding.campaign.title"),
                    Confirm = _loc.T("onboarding.campaign.confirm"),
                    Cards = Cards(),
                }, ct);
                _onboarding.Complete(campaign);
            }
            finally
            {
                SetMainTheme(1f);
                _modals.Close(view);
            }
        }

        /// <summary>Niveau du thème principal pendant la cinématique (0 : coupé, 1 : plein).</summary>
        void SetMainTheme(float level) => _audio?.Duck(DuckLayer.Modal, level, level > 0.5f ? 2.5f : 0.4f);

        IReadOnlyList<CampaignCardData> Cards()
        {
            var list = new List<CampaignCardData>();
            foreach (var c in Campaigns.All)
                list.Add(new CampaignCardData
                {
                    Id = c.Id, Name = _loc.T(c.NameKey), Description = _loc.T(c.DescKey), Available = c.Available,
                    Badge = _loc.T(c.Available ? "onboarding.campaign.open" : "onboarding.campaign.wip"),
                });
            return list;
        }
    }
}
