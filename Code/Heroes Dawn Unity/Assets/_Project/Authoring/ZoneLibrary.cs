#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Zones créées dans l'outil et chargées au démarrage (Resources/Zones) : définitions + textes à enregistrer.</summary>
    public sealed class ZoneLibrary
    {
        public const string ResourcesFolder = "Zones";

        public IReadOnlyList<ZoneDef> Zones { get; }
        public IReadOnlyDictionary<string, string> Texts { get; }

        public ZoneLibrary(IReadOnlyList<ZoneDef> zones, IReadOnlyDictionary<string, string> texts) { Zones = zones; Texts = texts; }

        public static ZoneLibrary Empty => new ZoneLibrary(new List<ZoneDef>(), new Dictionary<string, string>());

        /// <summary>Charge la campagne de Resources/Zones (sinon toutes les zones du dossier, triées par identifiant).</summary>
        public static ZoneLibrary Load()
        {
            var campaign = Resources.LoadAll<CampaignAsset>(ResourcesFolder).FirstOrDefault();
            if (campaign != null) return FromCampaign(campaign);
            var assets = Resources.LoadAll<ZoneAsset>(ResourcesFolder).OrderBy(a => a.zoneId, System.StringComparer.Ordinal).ToList();
            return FromZones(assets);
        }

        public static ZoneLibrary FromZones(IEnumerable<ZoneAsset> assets)
        {
            var zones = new List<ZoneDef>();
            var texts = new Dictionary<string, string>();
            foreach (var a in assets)
            {
                zones.Add(ZoneAssetConverter.ToDef(a));
                foreach (var kv in ZoneAssetConverter.TextsOf(a).Values) texts[kv.Key] = kv.Value;
            }
            return new ZoneLibrary(zones, texts);
        }

        public static ZoneLibrary FromCampaign(CampaignAsset campaign)
        {
            var zones = new List<ZoneDef>();
            var texts = new Dictionary<string, string>();
            for (int i = 0; i < campaign.entries.Count; i++)
            {
                var e = campaign.entries[i];
                if (e.zone == null) continue;
                zones.Add(ZoneAssetConverter.ToDef(e.zone, e.act, campaign.CodeOf(i), e.requires));
                foreach (var kv in ZoneAssetConverter.TextsOf(e.zone).Values) texts[kv.Key] = kv.Value;
            }
            return new ZoneLibrary(zones, texts);
        }
    }
}
