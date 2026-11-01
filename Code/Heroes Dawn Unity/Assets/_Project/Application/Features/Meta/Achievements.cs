#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Un succès : une mesure du profil à atteindre (cible), indépendant de la date de déblocage.</summary>
    public sealed class AchievementDef
    {
        public string Id { get; }
        public AchievementMetric Metric { get; }
        public int Target { get; }
        public AchievementDef(string id, AchievementMetric metric, int target) { Id = id; Metric = metric; Target = target; }
        public string NameKey => "ach." + Id + ".name";
        public string DescKey => "ach." + Id + ".desc";
    }

    /// <summary>Ce que mesure un succès : une statistique du profil.</summary>
    public enum AchievementMetric { CombatsWon, ZonesCleared, Agents, CraftedItems, Friends }

    /// <summary>Un succès dans l'état du joueur : progression, déblocage et jour de déblocage (aaaa-mm-jj, vide si verrouillé).</summary>
    public readonly struct AchievementView
    {
        public readonly AchievementDef Def;
        public readonly int Progress;
        public readonly bool Unlocked;
        public readonly string UnlockedDay;
        public AchievementView(AchievementDef def, int progress, bool unlocked, string unlockedDay)
        { Def = def; Progress = progress; Unlocked = unlocked; UnlockedDay = unlockedDay; }
    }

    /// <summary>
    /// Succès du profil. Un succès débloqué garde le jour de sa première constatation (<see cref="MetaSave.achievements"/>) ;
    /// il ne se perd jamais, même si la mesure redescend.
    /// </summary>
    public sealed class Achievements
    {
        public static readonly IReadOnlyList<AchievementDef> Catalog = new[]
        {
            new AchievementDef("first_win", AchievementMetric.CombatsWon, 1),
            new AchievementDef("wins_10", AchievementMetric.CombatsWon, 10),
            new AchievementDef("wins_50", AchievementMetric.CombatsWon, 50),
            new AchievementDef("zone_1", AchievementMetric.ZonesCleared, 1),
            new AchievementDef("zone_4", AchievementMetric.ZonesCleared, 4),
            new AchievementDef("agents_5", AchievementMetric.Agents, 5),
            new AchievementDef("agents_15", AchievementMetric.Agents, 15),
            new AchievementDef("craft_1", AchievementMetric.CraftedItems, 1),
            new AchievementDef("friend_1", AchievementMetric.Friends, 1),
        };

        readonly MetaContext _c;
        readonly PlayerProfile _profile;

        public Achievements(MetaContext context, PlayerProfile profile) { _c = context; _profile = profile; }

        /// <summary>État de chaque succès ; enregistre le jour des déblocages nouveaux.</summary>
        public IReadOnlyList<AchievementView> List()
        {
            var s = _profile.Stats();
            int friends = _c.Meta.friends.Count;
            var list = new List<AchievementView>();
            foreach (var def in Catalog)
            {
                int measure = def.Metric switch
                {
                    AchievementMetric.CombatsWon => s.CombatsWon,
                    AchievementMetric.ZonesCleared => s.ZonesCleared,
                    AchievementMetric.Agents => s.Agents,
                    AchievementMetric.CraftedItems => s.CraftedItems,
                    _ => friends,
                };
                int progress = System.Math.Min(measure, def.Target);
                var saved = _c.Meta.achievements.FirstOrDefault(a => a.id == def.Id);
                if (saved == null && measure >= def.Target)
                {
                    saved = new AchievementSave { id = def.Id, day = _c.Today };
                    _c.Meta.achievements.Add(saved);
                }
                bool unlocked = saved != null;
                list.Add(new AchievementView(def, progress, unlocked, saved?.day ?? ""));
            }
            return list;
        }

        /// <summary>Le dernier succès débloqué (le plus récent), ou nul si aucun.</summary>
        public AchievementView? Last() => List().Where(a => a.Unlocked).OrderByDescending(a => a.UnlockedDay).Cast<AchievementView?>().FirstOrDefault();
    }
}
