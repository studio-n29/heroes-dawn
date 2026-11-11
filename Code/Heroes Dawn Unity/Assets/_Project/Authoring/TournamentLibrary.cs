#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    [Serializable]
    public sealed class TournamentRecord
    {
        public string id = "";
        public string nameKey = "";
        public string start = "";
        public string end = "";
        public string rewardsId = "";
    }

    [Serializable]
    public sealed class TournamentsFile
    {
        public List<TournamentRecord> tournaments = new List<TournamentRecord>();
    }

    /// <summary>Calendrier des tournois (Resources/Content/tournaments.json). Une ligne invalide est ignorée.</summary>
    public static class TournamentLibrary
    {
        public const string ResourceName = "Content/tournaments";

        public static IReadOnlyList<TournamentDef> Load()
        {
            var asset = Resources.Load<TextAsset>(ResourceName);
            return asset == null ? new List<TournamentDef>() : FromJson(asset.text);
        }

        public static IReadOnlyList<TournamentDef> FromJson(string json)
        {
            var list = new List<TournamentDef>();
            var file = JsonUtility.FromJson<TournamentsFile>(json);
            if (file == null) return list;
            foreach (var r in file.tournaments)
            {
                if (r.id.Length == 0 || !TournamentDef.TryParseDay(r.start, out var s) || !TournamentDef.TryParseDay(r.end, out var e)) continue;
                list.Add(new TournamentDef(r.id, r.nameKey, s, e, r.rewardsId));
            }
            return list;
        }
    }
}
