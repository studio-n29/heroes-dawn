#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ouvre la fenêtre d'ouverture d'un chapitre (au premier déploiement, ou pour la relire depuis la page Chapitres).</summary>
    public sealed class ChapterIntroFlow
    {
        readonly IModalService _modals;
        readonly ILocalization _loc;

        readonly N29.HeroesDawn.Application.Session.ISessionReadModel? _session;

        public ChapterIntroFlow(IModalService modals, ILocalization loc, N29.HeroesDawn.Application.Session.ISessionReadModel? session = null) { _modals = modals; _loc = loc; _session = session; }

        /// <summary>Texte d'ouverture, suivi de la phrase propre au parcours du joueur (clé <c>chapter.&lt;id&gt;.intro.agent_d</c> ou <c>.agent_m</c>) quand elle existe.</summary>
        string IntroText(ChapterDef chapter)
        {
            string text = _loc.T(chapter.IntroKey);
            string? hero = _session?.Route.HeroId;
            string key = chapter.IntroKey + "." + hero;
            return hero != null && _loc.Has(key) ? text + "\n\n" + _loc.T(key) : text;
        }

        /// <summary>« CHAPITRE 1 · BROOKLYN », le texte d'ouverture, l'illustration du chapitre si elle existe, le bouton Continuer.</summary>
        public ChapterPopupData DataOf(ChapterDef chapter)
            => new ChapterPopupData(_loc.F("chapter.heading", chapter.Order, _loc.T(chapter.TitleKey).ToUpperInvariant()), IntroText(chapter),
                                    AgentArt.Chapter(chapter.Id), _loc.T("chapter.continue"));

        /// <summary>Lecture d'un souvenir : titre, texte, illustration 16:9 au-dessus du texte, bouton de fermeture.</summary>
        public async UniTask ShowMemoryAsync(string title, string body, Sprite? art, string close, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IChapterPopupView>(ct);
            try { await view.ShowAsync(new ChapterPopupData(title, body, null, close, art), ct); }
            finally { _modals.Close(view); }
        }

        /// <summary>Affiche la fenêtre ; se termine quand le joueur la ferme (bouton ou Retour).</summary>
        public async UniTask ShowAsync(ChapterDef chapter, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IChapterPopupView>(ct);
            try { await view.ShowAsync(DataOf(chapter), ct); }
            finally { _modals.Close(view); }
        }
    }
}
