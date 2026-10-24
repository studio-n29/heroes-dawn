#nullable enable
namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>
    /// Retour de playtest : une fois la zone 1.1 terminée pour la première fois, le jeu propose d'ouvrir le formulaire de retour.
    /// La proposition n'est faite qu'une fois par profil. Sans adresse de formulaire, elle est désactivée.
    /// </summary>
    public sealed class PlaytestFeedback
    {
        /// <summary>Adresse du formulaire de retour de playtest (à renseigner ; vide : rien n'est proposé).</summary>
        public const string FormUrl = "";

        public const string TriggerZone = "zone_ny_1_1";

        readonly MetaContext _c;

        string _url = FormUrl;


        public PlaytestFeedback(MetaContext context) { _c = context; }

        /// <summary>Remplace l'adresse du formulaire (tests).</summary>
        public PlaytestFeedback WithUrl(string formUrl) { _url = formUrl; return this; }

        public string Url => _url;

        public bool Enabled => _url.Length > 0;

        /// <summary>Vrai à la première validation de la zone 1.1, tant que le formulaire n'a pas été proposé.</summary>
        public bool ShouldAsk(string zoneId, bool firstClear) => Enabled && firstClear && zoneId == TriggerZone && !_c.Meta.playtestFormAsked;

        public void MarkAsked()
        {
            _c.Meta.playtestFormAsked = true;
            _c.Touch(MetaArea.Tutorial);
        }
    }
}
