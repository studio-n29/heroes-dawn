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
        public IReadOnlyList<SynergyDef> Synergies { get; }
        public IReadOnlyList<ZoneDef> Zones { get; }
        public IReadOnlyList<ActivityDef> Activities { get; }
        /// <summary>Objets, cartes de competences et boosters.</summary>
        public CollectionDef Collection { get; }
        /// <summary>Identifiants des agents de l équipe de départ d un nouveau profil.</summary>
        public IReadOnlyList<string> StarterTeam { get; }

        public ContentCatalog(BalanceConfig balance, IReadOnlyList<MemberDef> members, IReadOnlyList<EnemyDef> enemies,
                              IReadOnlyList<SynergyDef> synergies, IReadOnlyList<ZoneDef> zones,
                              IReadOnlyList<ActivityDef>? activities = null, CollectionDef? collection = null,
                              IReadOnlyList<string>? starterTeam = null)
        {
            Collection = collection ?? CollectionDef.Empty;
            StarterTeam = starterTeam ?? new string[0];
            Balance = balance; Members = members; Enemies = enemies; Synergies = synergies; Zones = zones;
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

        /// <summary>Même contenu avec ces zones : une zone de même identifiant remplace celle du catalogue (ordre conservé), les autres sont ajoutées.</summary>
        public ContentCatalog WithZones(IReadOnlyList<ZoneDef> zones)
        {
            if (zones.Count == 0) return this;
            var merged = new List<ZoneDef>();
            var byId = new Dictionary<ZoneId, ZoneDef>();
            foreach (var z in zones) byId[z.Id] = z;
            foreach (var z in Zones) merged.Add(byId.TryGetValue(z.Id, out var over) ? over : z);
            foreach (var z in zones) if (!_zones.ContainsKey(z.Id)) merged.Add(z);
            return new ContentCatalog(Balance, Members, Enemies, Synergies, merged, Activities, Collection, StarterTeam);
        }

        /// <summary>Meme contenu avec ces objets, cartes et boosters.</summary>
        public ContentCatalog WithCollection(CollectionDef collection)
            => new ContentCatalog(Balance, Members, Enemies, Synergies, Zones, Activities, collection, StarterTeam);

        /// <summary>Même contenu avec ces objets à la place de ceux du code (outil de design des objets).</summary>
        public ContentCatalog WithItems(IReadOnlyList<ItemDef> items)
            => WithCollection(new CollectionDef(items, Collection.Cards, Collection.Boosters));

        /// <summary>Même contenu avec ces agents à la place de ceux du code (outil de design des agents).</summary>
        public ContentCatalog WithMembers(IReadOnlyList<MemberDef> members)
            => new ContentCatalog(Balance, members, Enemies, Synergies, Zones, Activities, Collection, StarterTeam);

    }
}
