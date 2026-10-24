#nullable enable
using System.Text;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Réponses du formulaire de retour de playtest : une note de 1 à 5 par question, et un commentaire libre.</summary>
    public sealed class PlaytestAnswers
    {
        public int[] Ratings { get; }
        public string Comment { get; }
        public PlaytestAnswers(int[] ratings, string comment) { Ratings = ratings; Comment = comment ?? ""; }
    }

    /// <summary>
    /// Retour de playtest : une fois la zone 1.1 terminée pour la première fois, le jeu propose un formulaire (quatre notes, un commentaire).
    /// La proposition n'est faite qu'une fois par profil ; chaque envoi est mis en forme en un rapport texte, conservé dans le profil.
    /// </summary>
    public sealed class PlaytestFeedback
    {
        public const string TriggerZone = "zone_ny_1_1";
        public const int Questions = 4;
        public static readonly string[] QuestionIds = { "fun", "clarity", "difficulty", "tutorial" };

        readonly MetaContext _c;

        public PlaytestFeedback(MetaContext context) { _c = context; }

        /// <summary>Vrai à la première validation de la zone 1.1, tant que le formulaire n'a pas été proposé.</summary>
        public bool ShouldAsk(string zoneId, bool firstClear) => firstClear && zoneId == TriggerZone && !_c.Meta.playtestFormAsked;

        public void MarkAsked()
        {
            _c.Meta.playtestFormAsked = true;
            _c.Touch(MetaArea.Tutorial);
        }

        public static bool IsComplete(PlaytestAnswers a)
        {
            if (a.Ratings == null || a.Ratings.Length != Questions) return false;
            foreach (var r in a.Ratings) if (r < 1 || r > 5) return false;
            return true;
        }

        /// <summary>Met les réponses en forme (version, joueur, notes, commentaire), les conserve dans le profil et renvoie le rapport.</summary>
        public string Submit(PlaytestAnswers answers, string version, string[] questionLabels)
        {
            var sb = new StringBuilder();
            sb.Append("[Retour playtest Heroes Dawn] version ").Append(version).Append(" · ").Append(_c.Today).Append('\n');
            string player = _c.Session.Account?.PlayerId ?? "local";
            sb.Append("Joueur : ").Append(player).Append(" · zone 1.1 terminée · agents : ").Append(_c.Session.Roster.Count).Append('\n');
            for (int i = 0; i < Questions && i < answers.Ratings.Length; i++)
                sb.Append(i < questionLabels.Length ? questionLabels[i] : QuestionIds[i]).Append(" : ").Append(answers.Ratings[i]).Append("/5\n");
            if (answers.Comment.Trim().Length > 0) sb.Append("Commentaire : ").Append(answers.Comment.Trim()).Append('\n');
            string report = sb.ToString();
            _c.Meta.playtestReports.Add(report);
            _c.Touch(MetaArea.Tutorial);
            return report;
        }
    }
}
