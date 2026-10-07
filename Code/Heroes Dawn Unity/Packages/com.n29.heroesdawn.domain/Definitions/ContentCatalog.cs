#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Tout le contenu du jeu, immuable, indexé par identifiant.</summary>
    public sealed class ContentCatalog
    {
        readonly Dictionary<MemberId, MemberDef> _members = new Dictionary<MemberId, MemberDef>();
        readonly Dictionary<EnemyId, EnemyDef> _enemies = new Dictionary<EnemyId, EnemyDef>();
        readonly Dictionary<ZoneId, ZoneDef> _zones = new Dictionary<ZoneId, ZoneDef>();
        readonly Dictionary<ActivityId, ActivityDef> _activities = new Dictionary<ActivityId, ActivityDef>();

        public BalanceConfig Balance { get; }
        public IReadOnlyList<MemberDef> Members { get; }
        public IReadOnlyList<EnemyDef> Enemies { get; }
        public IReadOnlyList<ComboDef> Combos { get; }
        public IReadOnlyList<ZoneDef> Zones { get; }
        public IReadOnlyList<AvatarSkillDef> AvatarSkills { get; }
        public IReadOnlyList<ActivityDef> Activities { get; }

        public ContentCatalog(BalanceConfig balance, IReadOnlyList<MemberDef> members, IReadOnlyList<EnemyDef> enemies,
                              IReadOnlyList<ComboDef> combos, IReadOnlyList<ZoneDef> zones, IReadOnlyList<AvatarSkillDef>? avatarSkills = null,
                              IReadOnlyList<ActivityDef>? activities = null)
        {
            Balance = balance; Members = members; Enemies = enemies; Combos = combos; Zones = zones;
            AvatarSkills = avatarSkills ?? new List<AvatarSkillDef>();
            Activities = activities ?? new List<ActivityDef>();
            foreach (var m in members) _members.Add(m.Id, m);
            foreach (var e in enemies) _enemies.Add(e.Id, e);
            foreach (var z in zones) _zones.Add(z.Id, z);
            foreach (var a in Activities) _activities.Add(a.Id, a);
        }

        public MemberDef Member(MemberId id) => _members[id];
        public EnemyDef Enemy(EnemyId id) => _enemies[id];
        public ZoneDef Zone(ZoneId id) => _zones[id];
        public ActivityDef Activity(ActivityId id) => _activities[id];
        public bool HasActivity(ActivityId id) => _activities.ContainsKey(id);

        /// <summary>Premier combo correspondant, dans l'ordre de la liste (ordre stable).</summary>
        public ComboDef? FindCombo(Family a, Family b)
        {
            foreach (var c in Combos) if (c.Matches(a, b)) return c;
            return null;
        }
    }
}
