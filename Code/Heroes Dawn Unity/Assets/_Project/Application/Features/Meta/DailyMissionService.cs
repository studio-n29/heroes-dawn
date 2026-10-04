#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public enum DailyKind { WinCombats, TrainMember, RecruitHero, ValidateZone, EarnCredits }

    public sealed class DailyTemplate
    {
        public string Id { get; }
        public DailyKind Kind { get; }
        public int Target { get; }
        public Reward Reward { get; }
        public DailyTemplate(string id, DailyKind kind, int target, Reward reward) { Id = id; Kind = kind; Target = target; Reward = reward; }
    }

    public readonly struct DailyView
    {
        public readonly DailyTemplate Template;
        public readonly int Progress;
        public readonly bool Claimed;
        public DailyView(DailyTemplate template, int progress, bool claimed) { Template = template; Progress = progress; Claimed = claimed; }
        public bool Complete => Progress >= Template.Target;
        public bool Claimable => Complete && !Claimed;
    }

    /// <summary>Trois missions par jour (tirées d'un catalogue, identiques pour une date donnée), qui font avancer le pass.</summary>
    public sealed class DailyMissionService
    {
        public const int PerDay = 3;

        public static readonly IReadOnlyList<DailyTemplate> Catalog = new[]
        {
            new DailyTemplate("win2", DailyKind.WinCombats, 2, new Reward(credits: 200, passXp: 15)),
            new DailyTemplate("win3", DailyKind.WinCombats, 3, new Reward(credits: 300, passXp: 20)),
            new DailyTemplate("win5", DailyKind.WinCombats, 5, new Reward(credits: 500, gems: 5, passXp: 30)),
            new DailyTemplate("train1", DailyKind.TrainMember, 1, new Reward(pr: 10, passXp: 20)),
            new DailyTemplate("train2", DailyKind.TrainMember, 2, new Reward(pr: 15, passXp: 30)),
            new DailyTemplate("recruit1", DailyKind.RecruitHero, 1, new Reward(gems: 3, passXp: 30)),
            new DailyTemplate("zone1", DailyKind.ValidateZone, 1, new Reward(credits: 600, gems: 5, passXp: 40)),
            new DailyTemplate("earn1000", DailyKind.EarnCredits, 1000, new Reward(gems: 5, passXp: 25)),
        };

        readonly MetaContext _c;
        readonly PassService _pass;

        public DailyMissionService(MetaContext context, PassService pass) { _c = context; _pass = pass; }

        public static DailyTemplate? Find(string id)
        {
            foreach (var t in Catalog) if (t.Id == id) return t;
            return null;
        }

        /// <summary>Les missions du jour (créées au premier appel de la journée).</summary>
        public IReadOnlyList<DailyView> Today()
        {
            Ensure();
            var list = new List<DailyView>();
            foreach (var s in _c.Meta.daily)
            {
                var t = Find(s.id);
                if (t != null) list.Add(new DailyView(t, s.progress, s.claimed));
            }
            return list;
        }

        void Ensure()
        {
            var m = _c.Meta;
            if (m.dailyDay == _c.Today && m.daily.Count > 0) return;
            m.dailyDay = _c.Today;
            m.daily.Clear();
            var h = new Fnv1a64();
            h.Add(_c.Today);
            var rng = new Pcg32(h.Value);
            var pool = new List<DailyTemplate>(Catalog);
            var kinds = new HashSet<DailyKind>();
            while (m.daily.Count < PerDay && pool.Count > 0)
            {
                int i = rng.Range(0, pool.Count);
                var t = pool[i];
                pool.RemoveAt(i);
                if (!kinds.Add(t.Kind)) continue; // une mission par type
                m.daily.Add(new DailySave { id = t.Id });
            }
            _c.Touch(MetaArea.Daily);
        }

        public TimeSpan UntilReset() => _c.LocalNow.Date.AddDays(1) - _c.LocalNow;

        /// <summary>Fait avancer les missions du type donné.</summary>
        public void Record(DailyKind kind, int amount = 1)
        {
            if (amount <= 0) return;
            Ensure();
            bool changed = false;
            foreach (var s in _c.Meta.daily)
            {
                var t = Find(s.id);
                if (t == null || t.Kind != kind || s.claimed || s.progress >= t.Target) continue;
                s.progress = Math.Min(t.Target, s.progress + amount);
                changed = true;
            }
            if (changed) _c.Touch(MetaArea.Daily);
        }

        public int ClaimableCount()
        {
            int n = 0;
            foreach (var v in Today()) if (v.Claimable) n++;
            return n;
        }

        public Result<Reward> Claim(string id)
        {
            Ensure();
            foreach (var s in _c.Meta.daily)
            {
                if (s.id != id) continue;
                var t = Find(id);
                if (t == null) break;
                if (s.claimed) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
                if (s.progress < t.Target) return Result<Reward>.Fail(MetaErrors.NotReady);
                s.claimed = true;
                _c.Grant(t.Reward);
                _pass.AddXp(t.Reward.PassXp);
                _c.Touch(MetaArea.Daily);
                return Result<Reward>.Ok(t.Reward);
            }
            return Result<Reward>.Fail(MetaErrors.Unknown);
        }
    }
}
