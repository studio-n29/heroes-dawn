#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    public struct IntroCinematicTexts
    {
        public string Skip;
    }


    public struct CampaignCardData
    {
        public string Id;
        public string Name;
        public string Description;
        public bool Available;
        public string Badge;
    }

    public struct ChooseCampaignData
    {
        public string Title;
        public string Confirm;
        public System.Collections.Generic.IReadOnlyList<CampaignCardData> Cards;
    }
}
