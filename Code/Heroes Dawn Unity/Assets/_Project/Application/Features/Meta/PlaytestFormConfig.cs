#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>
    /// Réglage du formulaire Google Forms qui reçoit les retours : l'adresse « formResponse » et le numéro de champ (entry.N) de chaque réponse.
    /// Lu depuis un fichier texte « clé=valeur » ; une ligne vide ou commençant par # est ignorée.
    /// </summary>
    public sealed class PlaytestFormConfig
    {
        readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public static PlaytestFormConfig Parse(string text)
        {
            var c = new PlaytestFormConfig();
            foreach (var raw in (text ?? "").Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                c._values[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }
            return c;
        }

        public string Get(string key) => _values.TryGetValue(key, out var v) ? v : "";

        public string Url => Get("formResponse");

        /// <summary>Vrai quand l'adresse d'envoi est renseignée et que c'est bien une adresse Google Forms.</summary>
        public bool Enabled => Url.StartsWith("https://docs.google.com/forms/") && Url.EndsWith("/formResponse");

        /// <summary>Champs à envoyer : (entry.N, valeur) pour chaque réponse dont le champ est configuré.</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Fields(PlaytestAnswers a, string version)
        {
            var list = new List<KeyValuePair<string, string>>();
            void Add(string key, string value)
            {
                var entry = Get(key);
                if (entry.Length > 0) list.Add(new KeyValuePair<string, string>(entry, value));
            }
            for (int i = 0; i < PlaytestFeedback.Questions && i < a.Ratings.Length; i++) Add(PlaytestFeedback.QuestionIds[i], a.Ratings[i].ToString());
            Add("comment", a.Comment);
            Add("version", version);
            return list;
        }
    }
}
