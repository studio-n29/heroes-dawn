#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    [Serializable]
    public sealed class CosmeticRecord
    {
        public string id = "";
        public string nameKey = "";
        public string type = "Outfit";
        public int price;
        public string source = "Shop";
    }

    [Serializable]
    public sealed class CosmeticsFile { public List<CosmeticRecord> cosmetics = new List<CosmeticRecord>(); }

    /// <summary>Catalogue des cosmétiques (Resources/Content/cosmetics.json).</summary>
    public static class CosmeticLibrary
    {
        public const string ResourceName = "Content/cosmetics";

        public static IReadOnlyList<CosmeticDef> Load()
        {
            var asset = Resources.Load<TextAsset>(ResourceName);
            return asset == null ? new CosmeticDef[0] : FromJson(asset.text);
        }

        public static IReadOnlyList<CosmeticDef> FromJson(string json)
        {
            var file = JsonUtility.FromJson<CosmeticsFile>(json);
            var list = new List<CosmeticDef>();
            if (file == null) return list;
            foreach (var r in file.cosmetics)
                if (!string.IsNullOrEmpty(r.id) && Enum.TryParse(r.type, out CosmeticType t) && Enum.TryParse(r.source, out CosmeticSource s))
                    list.Add(new CosmeticDef(r.id, string.IsNullOrEmpty(r.nameKey) ? "cosmetic." + r.id + ".name" : r.nameKey, t, Math.Max(0, r.price), s));
            return list;
        }
    }
}
