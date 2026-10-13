#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Ce que mesure une quête secondaire.</summary>
    public enum QuestMetric { CombatsWon, ZonesCleared, Recruits, Trainings }

    /// <summary>Quête secondaire de la campagne : un compteur à atteindre, une récompense, et la zone qui la débloque (vide : dès le départ).</summary>
    public sealed class QuestDef
    {
        public string Id { get; }
        public QuestMetric Metric { get; }
        public int Target { get; }
        public Reward Reward { get; }
        public string UnlockZone { get; }
        public QuestDef(string id, QuestMetric metric, int target, Reward reward, string unlockZone = "")
        { Id = id; Metric = metric; Target = target; Reward = reward; UnlockZone = unlockZone; }
        public string TitleKey => "quest." + Id + ".title";
        public string DescKey => "quest." + Id + ".desc";
    }

    public readonly struct QuestView
    {
        public readonly QuestDef Quest;
        public readonly int Progress;
        public readonly bool Claimed;
        public QuestView(QuestDef quest, int progress, bool claimed) { Quest = quest; Progress = progress; Claimed = claimed; }
        public bool Complete => Progress >= Quest.Target;
        public bool Claimable => Complete && !Claimed;
    }

    /// <summary>Quête principale : une par zone de la campagne, dans l'ordre de l'histoire.</summary>
    public readonly struct MainQuestView
    {
        public readonly ZoneDef Zone;
        public readonly ZoneStatus Status;
        public MainQuestView(ZoneDef zone, ZoneStatus status) { Zone = zone; Status = status; }
    }

    /// <summary>
    /// Suivi des quêtes du QG. Principales : les zones de la campagne (verrouillée, en cours, terminée). Secondaires : des compteurs
    /// qui se débloquent au fil des zones validées et se réclament une fois remplis.
    /// </summary>
    public sealed class QuestLog
    {
        public static readonly IReadOnlyList<QuestDef> Catalog = new[]
        {
            new QuestDef("docks", QuestMetric.CombatsWon, 3, new Reward(credits: 300, passXp: 20)),
            new QuestDef("reinforcements", QuestMetric.Recruits, 1, new Reward(gems: 5, passXp: 30), "zone_ny_1_1"),
            new QuestDef("drills", QuestMetric.Trainings, 2, new Reward(pr: 15, passXp: 25), "zone_ny_1_1"),
            new QuestDef("hostage_aid", QuestMetric.CombatsWon, 8, new Reward(credits: 600, gems: 5, passXp: 35), "zone_ny_1_2"),
            new QuestDef("coney_watch", QuestMetric.ZonesCleared, 2, new Reward(credits: 800, gems: 8, passXp: 40), "zone_ny_1_3"),
            new QuestDef("bridge_shield", QuestMetric.CombatsWon, 15, new Reward(credits: 1000, pr: 20, gems: 10, passXp: 60), "zone_ny_1_4"),
        };

        readonly MetaContext _c;
        readonly PassProgress _pass;

        public QuestLog(MetaContext context, PassProgress pass) { _c = context; _pass = pass; }

        public static QuestDef? Find(string id)
        {
            foreach (var q in Catalog) if (q.Id == id) return q;
            return null;
        }

        bool Unlocked(QuestDef q)
        {
            if (q.UnlockZone.Length == 0) return true;
            foreach (var z in _c.Session.CompletedZones) if (z == q.UnlockZone) return true;
            return false;
        }

        /// <summary>Les zones de la campagne avec leur état.</summary>
        public IReadOnlyList<MainQuestView> Main()
        {
            var list = new List<MainQuestView>();
            foreach (var z in ZoneProgress.InBorough(_c.Session, "brooklyn")) list.Add(new MainQuestView(z, ZoneProgress.StatusOf(_c.Session, z)));
            return list;
        }

        /// <summary>Première quête principale à jouer (zone ouverte et pas validée), sinon null (campagne terminée).</summary>
        public MainQuestView? CurrentMain()
        {
            foreach (var m in Main()) if (m.Status == ZoneStatus.Available) return m;
            return null;
        }

        /// <summary>Les quêtes secondaires débloquées, avec leur progression.</summary>
        public IReadOnlyList<QuestView> Side()
        {
            var list = new List<QuestView>();
            foreach (var q in Catalog)
            {
                if (!Unlocked(q)) continue;
                var s = Save(q.Id, create: false);
                list.Add(new QuestView(q, s?.progress ?? 0, s?.claimed ?? false));
            }
            return list;
        }

        QuestSave? Save(string id, bool create)
        {
            foreach (var s in _c.Meta.quests) if (s.id == id) return s;
            if (!create) return null;
            var made = new QuestSave { id = id };
            _c.Meta.quests.Add(made);
            return made;
        }

        /// <summary>Fait avancer les quêtes secondaires débloquées qui mesurent <paramref name="metric"/>.</summary>
        public void Record(QuestMetric metric, int amount = 1)
        {
            if (amount <= 0) return;
            bool changed = false;
            foreach (var q in Catalog)
            {
                if (q.Metric != metric || !Unlocked(q)) continue;
                var s = Save(q.Id, create: true)!;
                if (s.claimed || s.progress >= q.Target) continue;
                s.progress = Math.Min(q.Target, s.progress + amount);
                changed = true;
            }
            if (changed) _c.Touch(MetaArea.Quests);
        }

        public int ClaimableCount()
        {
            int n = 0;
            foreach (var v in Side()) if (v.Claimable) n++;
            return n;
        }

        public Result<Reward> Claim(string id)
        {
            var q = Find(id);
            if (q == null || !Unlocked(q)) return Result<Reward>.Fail(MetaErrors.Unknown);
            var s = Save(id, create: true)!;
            if (s.claimed) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            if (s.progress < q.Target) return Result<Reward>.Fail(MetaErrors.NotReady);
            s.claimed = true;
            _c.Grant(q.Reward);
            _pass.AddXp(q.Reward.PassXp);
            _c.Touch(MetaArea.Quests);
            return Result<Reward>.Ok(q.Reward);
        }
    }
}
