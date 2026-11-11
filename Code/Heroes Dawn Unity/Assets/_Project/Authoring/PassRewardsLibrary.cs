#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    [Serializable]
    public sealed class PassLevelRecord
    {
        public int level;
        public int credits, pr, gems, speedUpMinutes;
        public List<ComponentGrantRecord> components = new List<ComponentGrantRecord>();
        public List<CatalystGrantRecord> catalysts = new List<CatalystGrantRecord>();
        public List<string> cosmetics = new List<string>();
    }

    [Serializable]
    public sealed class PassRewardsFile
    {
        public List<PassLevelRecord> free = new List<PassLevelRecord>();
        public List<PassLevelRecord> premium = new List<PassLevelRecord>();
    }

    /// <summary>Récompenses du pass saisonnier (Resources/Content/pass_rewards.json) : une ligne par palier et par piste.</summary>
    public static class PassRewardsLibrary
    {
        public const string ResourceName = "Content/pass_rewards";

        public static PassRewardTable Load()
        {
            var asset = Resources.Load<TextAsset>(ResourceName);
            return asset == null ? PassRewardTable.Empty : FromJson(asset.text);
        }

        public static PassRewardTable FromJson(string json)
        {
            var file = JsonUtility.FromJson<PassRewardsFile>(json);
            if (file == null) return PassRewardTable.Empty;
            return new PassRewardTable(ToDefs(file.free), ToDefs(file.premium));
        }

        // Les lignes sont rangées par numéro de palier ; un palier absent donne une récompense vide.
        static IReadOnlyList<PassRewardDef> ToDefs(List<PassLevelRecord> records)
        {
            int max = 0;
            foreach (var r in records) max = Math.Max(max, r.level);
            var defs = new PassRewardDef[max];
            for (int i = 0; i < max; i++) defs[i] = PassRewardDef.Empty;
            foreach (var r in records)
                if (r.level >= 1) defs[r.level - 1] = ToDef(r);
            return defs;
        }

        static PassRewardDef ToDef(PassLevelRecord r)
        {
            var components = new List<ComponentGrant>();
            foreach (var c in r.components)
                if (c.count > 0 && Enum.TryParse(c.kind, out ComponentKind k) && Enum.TryParse(c.grade, out ComponentGrade g)) components.Add(new ComponentGrant(k, g, c.count));
            var catalysts = new List<CatalystGrant>();
            foreach (var c in r.catalysts)
                if (c.count > 0 && Enum.TryParse(c.kind, out CatalystKind k)) catalysts.Add(new CatalystGrant(k, c.count));
            return new PassRewardDef(Math.Max(0, r.credits), Math.Max(0, r.pr), Math.Max(0, r.gems), Math.Max(0, r.speedUpMinutes), components, catalysts, r.cosmetics);
        }
    }
}
