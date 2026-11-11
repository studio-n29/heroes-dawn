#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    [Serializable]
    public sealed class ComponentGrantRecord { public string kind = "Weapon"; public string grade = "Common"; public int count; }

    [Serializable]
    public sealed class CatalystGrantRecord { public string kind = "Metal"; public int count; }

    [Serializable]
    public sealed class ArenaRewardRecord
    {
        public int credits;
        public List<ComponentGrantRecord> components = new List<ComponentGrantRecord>();
        public List<CatalystGrantRecord> catalysts = new List<CatalystGrantRecord>();
        public string recipe = "";
        public List<string> cosmetics = new List<string>();
    }

    [Serializable]
    public sealed class ArenaRewardsFile
    {
        public List<ArenaRewardRecord> tiers = new List<ArenaRewardRecord>();
        public ArenaRewardRecord top10 = new ArenaRewardRecord();
        public ArenaRewardRecord top3 = new ArenaRewardRecord();
    }

    /// <summary>Lots de l'arène (Resources/Content/arena_rewards.json). Valeurs de départ à relire par le design.</summary>
    public static class ArenaRewardsLibrary
    {
        public const string ResourceName = "Content/arena_rewards";

        public static ArenaRewardTable Load()
        {
            var asset = Resources.Load<TextAsset>(ResourceName);
            return asset == null ? ArenaRewardTable.Empty : FromJson(asset.text);
        }

        public static ArenaRewardTable FromJson(string json)
        {
            var file = JsonUtility.FromJson<ArenaRewardsFile>(json);
            if (file == null) return ArenaRewardTable.Empty;
            var tiers = new List<ArenaRewardDef>();
            foreach (var t in file.tiers) tiers.Add(ToDef(t));
            return new ArenaRewardTable(tiers, ToDef(file.top10), ToDef(file.top3));
        }

        static ArenaRewardDef ToDef(ArenaRewardRecord r)
        {
            var components = new List<ComponentGrant>();
            foreach (var c in r.components)
                if (c.count > 0 && Enum.TryParse(c.kind, out ComponentKind k) && Enum.TryParse(c.grade, out ComponentGrade g)) components.Add(new ComponentGrant(k, g, c.count));
            var catalysts = new List<CatalystGrant>();
            foreach (var c in r.catalysts)
                if (c.count > 0 && Enum.TryParse(c.kind, out CatalystKind k)) catalysts.Add(new CatalystGrant(k, c.count));
            return new ArenaRewardDef(Math.Max(0, r.credits), components, catalysts, string.IsNullOrEmpty(r.recipe) ? null : r.recipe, r.cosmetics);
        }
    }
}
