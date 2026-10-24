#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Ouvre le formulaire de retour de playtest. À l'envoi, le rapport est conservé dans le profil, écrit dans un fichier
    /// (playtest_feedback.txt, dossier de données du jeu) et copié dans le presse-papiers pour être collé dans un message.
    /// </summary>
    public sealed class PlaytestFormFlow
    {
        readonly PlaytestFeedback _feedback;
        readonly IModalService _modals;
        readonly ILocalization _loc;
        readonly IToastService _toasts;

        public PlaytestFormFlow(PlaytestFeedback feedback, IModalService modals, ILocalization loc, IToastService toasts)
        {
            _feedback = feedback; _modals = modals; _loc = loc; _toasts = toasts;
        }

        public PlaytestFormData Data() => new PlaytestFormData
        {
            Title = _loc.T("playtest.title"),
            Intro = _loc.T("playtest.intro"),
            Questions = Labels(),
            CommentLabel = _loc.T("playtest.comment"),
            CommentHint = _loc.T("playtest.comment_hint"),
            Send = _loc.T("playtest.send"),
            Later = _loc.T("playtest.later"),
            NeedAll = _loc.T("playtest.need_all"),
        };

        string[] Labels()
        {
            var l = new string[PlaytestFeedback.Questions];
            for (int i = 0; i < l.Length; i++) l[i] = _loc.T("playtest.q." + PlaytestFeedback.QuestionIds[i]);
            return l;
        }

        /// <summary>Affiche le formulaire ; renvoie vrai si le joueur l'a envoyé.</summary>
        public async UniTask<bool> AskAsync(CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IPlaytestFormPopupView>(ct);
            PlaytestAnswers? answers;
            try { answers = await view.AskAsync(Data(), ct); }
            finally { _modals.Close(view); }
            if (answers == null) return false;
            string report = _feedback.Submit(answers, UnityEngine.Application.version, Labels());
            Deliver(report);
            _toasts.Show(_loc.T("playtest.thanks"), ToastKind.Reward);
            return true;
        }

        static void Deliver(string report)
        {
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "playtest_feedback.txt"), report + "\n"); }
            catch (System.Exception) { }
            try { GUIUtility.systemCopyBuffer = report; }
            catch (System.Exception) { }
        }
    }
}
