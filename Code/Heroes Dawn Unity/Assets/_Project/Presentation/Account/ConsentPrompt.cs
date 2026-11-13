#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Demande, une seule fois par profil, si le joueur accepte la mesure d'usage anonyme (opt-in). Refuser, ou fermer la fenêtre par Retour,
    /// vaut refus : rien n'est mesuré. La réponse est mémorisée ; le joueur peut la changer dans Réglages ▸ Compte et confidentialité.
    /// </summary>
    public sealed class ConsentPrompt
    {
        /// <summary>Valeur de <see cref="Enabled"/> à la création : les tests de parcours la coupent pour ne pas voir la fenêtre.</summary>
        public static bool DefaultEnabled { get; set; } = true;

        readonly PlayerSettings _settings;
        readonly IModalService _modals;
        readonly ILocalization _loc;

        public ConsentPrompt(PlayerSettings settings, IModalService modals, ILocalization loc)
        {
            _settings = settings; _modals = modals; _loc = loc;
        }

        public bool Enabled { get; set; } = DefaultEnabled;

        /// <summary>Pose la question si le profil n'a jamais répondu ; ne fait rien sinon. Annulée (le jeu se ferme), elle ne mémorise rien.</summary>
        public async UniTask RunIfNeededAsync(CancellationToken ct)
        {
            if (!Enabled || _settings.Current.consentAnswered) return;
            bool accepted = await _modals.ConfirmAsync(new ConfirmData(
                _loc.T("consent.title"), _loc.T("consent.body"), _loc.T("consent.accept"), _loc.T("consent.decline"), Tone.Primary), ct);
            _settings.Update(s => { s.consentStats = accepted; s.consentAnswered = true; });
        }
    }
}
