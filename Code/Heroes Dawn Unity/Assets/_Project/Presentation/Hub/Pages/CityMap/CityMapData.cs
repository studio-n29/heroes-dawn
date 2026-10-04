#nullable enable
using N29.HeroesDawn.Application.Features.World;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Quartier dessiné sur la carte ; verrouillé = grisé et hachuré.</summary>
    public struct BoroughData
    {
        public string Id;
        public string Name;
        public Vector2[] Polygon;
        public bool Locked;
        public bool Highlight;
        public string Tag;
        public Vector2 LabelAt;
    }

    /// <summary>Point précis de mission : zone jouable (<see cref="ZoneId"/> non nul) ou point à venir d'un quartier verrouillé.</summary>
    public struct ZonePointData
    {
        public string? ZoneId;
        public string Label;
        public string Sub;
        public Vector2 Position;
        public ZoneStatus Status;
        public bool Selected;
    }

    /// <summary>Ligne de la liste des zones de l'acte (panneau de droite).</summary>
    public struct ZoneRowData
    {
        public string ZoneId;
        public string Title;
        public string StatusText;
        public ZoneStatus Status;
        public bool Selected;
    }

    /// <summary>Fiche de la zone choisie (M61 intégrée au panneau).</summary>
    public struct ZoneDetailData
    {
        public string Title;
        public string Description;
        public string Chips;
        public string[] Objectives;
        public bool[] ObjectivesDone;
        public string Rewards;
        public string ButtonLabel;
        public bool ButtonEnabled;
        public string Reason;
    }

    public struct CityMapPageData
    {
        public string ActTitle;
        public BoroughData[] Boroughs;
        public ZonePointData[] Points;
        public ZoneRowData[] Rows;
        public ZoneDetailData Detail;
    }
}
