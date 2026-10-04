#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Case de pièce du QG (M10 ③) ; <see cref="WipId"/> non nul = pièce à venir (BIENTÔT).</summary>
    public struct RoomCardData
    {
        public HubPage Page;
        public string Title;
        public string Info;
        public string? WipId;
        /// <summary>Pastille de notification (0 : aucune).</summary>
        public int Badge;
    }

    /// <summary>Tout ce qu'affiche la page QG (M10).</summary>
    public struct QgPageData
    {
        public RoomCardData[] Rooms;
        public string[] DailyLines;
        public string DailyBadge;
        public string SecondaryText;
        public bool SecondaryActive;
        public string ShopAlert;
        public string FilSpeaker;
        public string FilText;
        public string MissionTitle;
        public string MissionSub;
        public bool MissionEnabled;
        public string MissionHint;
    }
}
