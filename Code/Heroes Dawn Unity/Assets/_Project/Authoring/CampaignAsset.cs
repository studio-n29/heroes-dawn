#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Une zone dans la campagne : son acte, son code (« 1.2 », calculé si vide) et les zones à valider avant elle.</summary>
    [Serializable]
    public sealed class CampaignEntry
    {
        public ZoneAsset? zone;
        public int act = 1;
        public string codeOverride = "";
        public List<string> requires = new List<string>();
    }

    /// <summary>
    /// La carte de campagne : l'ordre des zones, leur acte (1.1, 1.2, 2.1…) et leurs prérequis. Si une campagne existe dans
    /// Resources/Zones, elle est la liste des zones jouables ; sinon toutes les zones du dossier le sont.
    /// </summary>
    [CreateAssetMenu(menuName = "Heroes Dawn/Campagne", fileName = "Campaign")]
    public sealed class CampaignAsset : ScriptableObject
    {
        public List<CampaignEntry> entries = new List<CampaignEntry>();

        /// <summary>Code de la zone <paramref name="index"/> : « acte.rang dans l'acte » (ex. 1.2 = deuxième zone de l'acte 1), sauf surcharge.</summary>
        public string CodeOf(int index)
        {
            var e = entries[index];
            if (!string.IsNullOrEmpty(e.codeOverride)) return e.codeOverride;
            int rank = 1;
            for (int i = 0; i < index; i++) if (entries[i].act == e.act) rank++;
            return e.act + "." + rank;
        }
    }
}
