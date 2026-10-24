#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Ce que demande une activité du jour.</summary>
    public enum ActivityKind { WinCombats, ClearZone }

    /// <summary>Une activité du jour : une zone déjà terminée, un objectif à y remplir, une récompense.</summary>
    public readonly struct DailyActivityView
    {
        public readonly int Index;
        public readonly string ZoneId;
        public readonly ActivityKind Kind;
        public readonly int Target;
        public readonly int Progress;
        public readonly bool Claimed;
        public readonly Reward Reward;
        public DailyActivityView(int index, string zoneId, ActivityKind kind, int target, int progress, bool claimed, Reward reward)
        { Index = index; ZoneId = zoneId; Kind = kind; Target = target; Progress = progress; Claimed = claimed; Reward = reward; }
        public bool Complete => Progress >= Target;
        public bool Claimable => Complete && !Claimed;
    }

    /// <summary>
    /// Activités du jour (à la Mo.Co) : chaque jour, parmi les zones terminées, quelques activités apparaissent avec un objectif à remplir
    /// (gagner des combats dans la zone, ou la valider à nouveau). Elles se renouvellent à minuit.
    /// </summary>
    public sealed partial class QuestLog
    {
        public const int MaxActivities = 3;

        /// <summary>Les zones du jeu (pour nommer les activités).</summary>
        public IReadOnlyList<N29.HeroesDawn.Definitions.ZoneDef> Zones => _c.Session.Content.Zones;

        /// <summary>Nombre d'activités du jour pour ce nombre de zones terminées (une par zone, trois au plus).</summary>
        public static int ActivityCount(int doneZones) => Math.Min(MaxActivities, doneZones);

        /// <summary>Les activités d'aujourd'hui (tirées au premier appel de la journée, à partir des zones terminées).</summary>
        public IReadOnlyList<DailyActivityView> Activities()
        {
            var m = _c.Meta;
            if (m.activityDay != _c.Today || (m.activities.Count == 0 && _c.Session.CompletedZones.Count > 0))
            {
                m.activityDay = _c.Today;
                m.activities.Clear();
                var zones = new List<string>(_c.Session.CompletedZones);
                zones.RemoveAll(z => z == "zone_tuto");
                zones.Sort(StringComparer.Ordinal);
                var h = new Fnv1a64();
                string player = _c.Session.Account?.PlayerId ?? "local";
                h.Add(player.Length); h.Add(player); h.Add(_c.Today.Length); h.Add(_c.Today);
                var rng = new Pcg32(h.Value);
                int n = ActivityCount(zones.Count);
                for (int i = 0; i < n; i++)
                {
                    int pick = rng.Range(0, zones.Count);
                    var zone = zones[pick];
                    zones.RemoveAt(pick);
                    bool wins = rng.Range(0, 100) < 65;
                    m.activities.Add(new ActivitySave { zoneId = zone, kind = wins ? (int)ActivityKind.WinCombats : (int)ActivityKind.ClearZone, target = wins ? rng.Range(2, 5) : 1 });
                }
                _c.Touch(MetaArea.Quests);
            }
            var list = new List<DailyActivityView>();
            for (int i = 0; i < m.activities.Count; i++)
            {
                var a = m.activities[i];
                list.Add(new DailyActivityView(i, a.zoneId, (ActivityKind)a.kind, a.target, a.progress, a.claimed, RewardOf(a)));
            }
            return list;
        }

        static Reward RewardOf(ActivitySave a) => a.kind == (int)ActivityKind.ClearZone
            ? new Reward(credits: 400, pr: 10, passXp: 30)
            : new Reward(credits: 150 * a.target, passXp: 15 * a.target);

        /// <summary>Un combat vient d'être gagné dans cette zone.</summary>
        public void RecordCombatWon(string zoneId) => Advance(ActivityKind.WinCombats, zoneId);

        /// <summary>Cette zone vient d'être validée (première fois ou rejeu).</summary>
        public void RecordZoneValidated(string zoneId) => Advance(ActivityKind.ClearZone, zoneId);

        void Advance(ActivityKind kind, string zoneId)
        {
            Activities(); // s'assure que les activités du jour existent
            bool changed = false;
            foreach (var a in _c.Meta.activities)
            {
                if (a.kind != (int)kind || a.zoneId != zoneId || a.claimed || a.progress >= a.target) continue;
                a.progress++;
                changed = true;
            }
            if (changed) _c.Touch(MetaArea.Quests);
        }

        public Result<Reward> ClaimActivity(int index)
        {
            Activities();
            var list = _c.Meta.activities;
            if (index < 0 || index >= list.Count) return Result<Reward>.Fail(MetaErrors.Unknown);
            var a = list[index];
            if (a.claimed) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            if (a.progress < a.target) return Result<Reward>.Fail(MetaErrors.NotReady);
            a.claimed = true;
            var reward = RewardOf(a);
            _c.Grant(reward);
            _pass.AddXp(reward.PassXp);
            _c.Touch(MetaArea.Quests);
            return Result<Reward>.Ok(reward);
        }
    }
}
