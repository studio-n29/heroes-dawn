#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
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
    /// Première connexion : si le profil n'a encore rien joué, ouvre la popup plein écran (accueil de Rayner en Full Art, fiche d identification, choix de la ville de départ)
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
                // (1) Rayner présente The Dawn et pose sa question ; (2) le joueur remplit la fiche ; (3) Rayner conclut ; (4) choix de la ville de départ.
                await view.ShowRaynerAsync(Scene(new[] { _loc.T("onboarding.rayner.intro.1"), _loc.T("onboarding.rayner.intro.2"),
                                                          _loc.T("onboarding.rayner.intro.3"), _loc.T("onboarding.rayner.intro.4"), _loc.T("onboarding.rayner.ask") }), ct);
                string error = "";
                bool concluded = false;
                while (true)
                {
                    var form = await view.FillFormAsync(BuildForm(error), ct);
                    if (!concluded)
                    {
                        concluded = true;
                        await view.ShowRaynerAsync(Scene(new[] { _loc.F("onboarding.rayner.after.1", form.FirstName.Trim()), _loc.T("onboarding.rayner.after.2") }), ct);
                    }
                    string route = await view.ChooseStartAsync(BuildStart(), ct);
                    // La vue ne valide que des champs corrects et des parcours ouverts : un refus du domaine reste possible (parcours fermé entre-temps), la fiche revient alors avec l'erreur.
                    string? key = _onboarding.CreateAgent(form, route);
                    if (key == null) break;
                    error = _loc.T(key);
                }
            }
            finally
            {
                SetMainTheme(1f);
                _modals.Close(view);
            }
        }

        RaynerSceneData Scene(string[] lines) => new RaynerSceneData
        {
            Speaker = _loc.T("onboarding.rayner.speaker"), PortraitId = "rayner", Lines = lines, Continue = _loc.T("onboarding.rayner.continue"),
        };

        AgentFormData BuildForm(string error)
        {
            var columns = new List<PowerColumn>();
            foreach (var family in HeroPowers.Families)
            {
                var options = new List<PowerOption>();
                foreach (var p in family.Powers) options.Add(new PowerOption(p.Id, _loc.T(p.NameKey)));
                columns.Add(new PowerColumn(family.Id, _loc.T(family.NameKey), options));
            }
            return new AgentFormData
            {
                Title = _loc.T("onboarding.form.title"),
                LastNameLabel = _loc.T("onboarding.form.last_name"), FirstNameLabel = _loc.T("onboarding.form.first_name"), AgeLabel = _loc.T("onboarding.form.age"),
                AvatarLabel = _loc.T("onboarding.form.avatar"), PowerLabel = _loc.T("onboarding.form.power"), PowerPlaceholder = _loc.T("onboarding.form.power_placeholder"),
                Confirm = _loc.T("onboarding.form.confirm"), Error = error,
                MaxNameLength = PlayerTag.MaxNameLength, MinAge = AgentForm.MinAge, MaxAge = AgentForm.MaxAge,
                Avatars = PlayerAvatars.Gallery, Powers = columns,
            };
        }

        StartChoiceData BuildStart()
        {
            var options = new List<StartOption>();
            foreach (var c in Campaigns.All)
                options.Add(new StartOption(c.Id, _loc.T("onboarding.start." + c.Id + ".city"), _loc.T(c.NameKey), _loc.T("onboarding.start." + c.Id + ".desc"),
                                            c.Available, _loc.T("onboarding.start.soon")));
            return new StartChoiceData { Title = _loc.T("onboarding.start.title"), Options = options };
        }

        /// <summary>Niveau du thème principal pendant la cinématique (0 : coupé, 1 : plein).</summary>
        void SetMainTheme(float level) => _audio?.Duck(DuckLayer.Modal, level, level > 0.5f ? 2.5f : 0.4f);
    }
}
