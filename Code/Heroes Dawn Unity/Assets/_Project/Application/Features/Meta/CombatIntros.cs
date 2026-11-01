#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>
    /// Entrées en combat (bandeaux des combattants) : mémorise celles que le joueur a déjà vues pour abréger les introductions répétées.
    /// La clé d'un affrontement est la liste triée de ses ennemis.
    /// </summary>
    public sealed class CombatIntros
    {
        readonly MetaContext _c;

        public CombatIntros(MetaContext context) { _c = context; }

        /// <summary>Valeur de <see cref="Enabled"/> à la création : les tests de parcours la coupent pour ne pas attendre les bandeaux.</summary>
        public static bool DefaultEnabled { get; set; } = true;

        /// <summary>Désactivé, aucune entrée en combat n'est jouée.</summary>
        public bool Enabled { get; set; } = DefaultEnabled;

        public static string KeyOf(IEnumerable<string> enemyIds) => string.Join(",", enemyIds.OrderBy(id => id, System.StringComparer.Ordinal));

        public bool Seen(string key) => _c.Meta.seenIntros.Contains(key);

        public void MarkSeen(string key)
        {
            if (Seen(key)) return;
            _c.Meta.seenIntros.Add(key);
            _c.Touch(MetaArea.Profile);
        }
    }
}
