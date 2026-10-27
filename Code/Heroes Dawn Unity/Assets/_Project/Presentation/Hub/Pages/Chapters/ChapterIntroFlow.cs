#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ouvre la fenêtre d'ouverture d'un chapitre (au premier déploiement, ou pour la relire depuis la page Chapitres).</summary>
    public sealed class ChapterIntroFlow
    {
        readonly IModalService _modals;
        readonly ILocalization _loc;

        public ChapterIntroFlow(IModalService modals, ILocalization loc) { _modals = modals; _loc = loc; }

        /// <summary>« CHAPITRE 1 · BROOKLYN », le texte d'ouverture, l'illustration du chapitre si elle existe, le bouton Continuer.</summary>
        public ChapterPopupData DataOf(ChapterDef chapter)
            => new ChapterPopupData(_loc.F("chapter.heading", chapter.Order, _loc.T(chapter.TitleKey).ToUpperInvariant()), _loc.T(chapter.IntroKey),
                                    AgentArt.Chapter(chapter.Id), _loc.T("chapter.continue"));

        /// <summary>Affiche la fenêtre ; se termine quand le joueur la ferme (bouton ou Retour).</summary>
        public async UniTask ShowAsync(ChapterDef chapter, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IChapterPopupView>(ct);
            try { await view.ShowAsync(DataOf(chapter), ct); }
            finally { _modals.Close(view); }
        }
    }
}
