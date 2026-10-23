#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Première connexion du joueur (remplaçable dans les tests de bout en bout).</summary>
    public interface IOnboardingFlow
    {
        UniTask RunIfNeededAsync(CancellationToken ct);
    }

    /// <summary>
    /// Première connexion : si le profil n'a encore rien joué, ouvre la popup plein écran (cinématique, choix du personnage joueur)
    /// et enregistre le résultat. Sans effet pour un profil qui a déjà joué.
    /// </summary>
    public sealed class OnboardingFlow : IOnboardingFlow
    {
        readonly OnboardingGuide _onboarding;
        readonly IModalService _modals;
        readonly ILocalization _loc;

        public OnboardingFlow(OnboardingGuide onboarding, IModalService modals, ILocalization loc)
        {
            _onboarding = onboarding; _modals = modals; _loc = loc;
        }

        public bool Needed => _onboarding.Needed;

        public async UniTask RunIfNeededAsync(CancellationToken ct)
        {
            if (!_onboarding.Needed) return;
            var view = await _modals.OpenAsync<IIntroPopupView>(ct);
            try
            {
                await view.PlayCinematicAsync(Slides(), new IntroCinematicTexts { Skip = _loc.T("intro.skip"), TapHint = _loc.T("intro.tap") }, ct);
                string campaign = await view.ChooseCampaignAsync(new ChooseCampaignData
                {
                    Title = _loc.T("onboarding.campaign.title"),
                    Confirm = _loc.T("onboarding.campaign.confirm"),
                    Cards = Cards(),
                }, ct);
                _onboarding.Complete(campaign);
            }
            finally { _modals.Close(view); }
        }

        /// <summary>Neuf écrans : texte et motif provisoire (les illustrations dessinées se déposent dans Resources/Intro/intro_01 … intro_09).</summary>
        public IReadOnlyList<IntroSlideData> Slides()
        {
            var motifs = new[] { IntroMotif.Battle, IntroMotif.Wormhole, IntroMotif.Arrival, IntroMotif.Phoenix, IntroMotif.Farewell, IntroMotif.Portal, IntroMotif.Wave, IntroMotif.Dna, IntroMotif.Guild };
            var tops = new[] { "#3a1020", "#1c0a2e", "#16213e", "#4a2a05", "#202040", "#4a1a00", "#0b0b14", "#14301f", "#1b2550" };
            var bottoms = new[] { "#b3392f", "#5a1a6e", "#3b4f8a", "#e0a030", "#6a5a9a", "#ffb347", "#25254a", "#4cb87a", "#a9cdfa" };
            var list = new List<IntroSlideData>();
            for (int i = 0; i < motifs.Length; i++)
            {
                ColorUtility.TryParseHtmlString(tops[i], out var top);
                ColorUtility.TryParseHtmlString(bottoms[i], out var bottom);
                list.Add(new IntroSlideData
                {
                    Text = _loc.T("intro.slide." + (i + 1)),
                    Motif = motifs[i], Top = top, Bottom = bottom,
                    Art = Resources.Load<Sprite>("Intro/intro_" + (i + 1).ToString("00")),
                });
            }
            return list;
        }

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
