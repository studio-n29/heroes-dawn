#nullable enable
using System;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Comment un agent rejoint le joueur.</summary>
    public enum UnlockKind
    {
        /// <summary>Boutique de recrutement (rotation quotidienne, paiement en PR).</summary>
        Shop,
        /// <summary>Arc narratif : l'agent rejoint l'équipe quand la zone est validée.</summary>
        Arc,
        /// <summary>Événement temporaire : l'agent rejoint l'équipe si la zone est validée un jour de l'événement.</summary>
        Event,
        /// <summary>Prouesse : l'agent rejoint l'équipe si la zone est validée en accomplissant l'exploit.</summary>
        Feat,
        /// <summary>Personnage joueur (Agent D ou Agent M) : rejoint l équipe au choix de la première connexion, jamais en boutique.</summary>
        Hero,
    }

    /// <summary>Prouesses qui débloquent un compagnon.</summary>
    public enum FeatKind
    {
        None,
        /// <summary>Valider la zone avec un seul agent engagé au combat.</summary>
        SoloClear,
        /// <summary>Valider la zone sans aucune défaite au combat.</summary>
        NoDefeat,
    }

    /// <summary>Règle de déblocage d'un agent : le type, la zone concernée et, selon le type, les jours de l'événement ou la prouesse.</summary>
    public sealed class UnlockDef
    {
        public UnlockKind Kind { get; }
        public ZoneId? Zone { get; }
        /// <summary>Événement : jours de la semaine actifs (un bit par <see cref="DayOfWeek"/>, dimanche = bit 0).</summary>
        public int WeekdayMask { get; }
        public FeatKind Feat { get; }

        public UnlockDef(UnlockKind kind, ZoneId? zone = null, int weekdayMask = 0, FeatKind feat = FeatKind.None)
        {
            Kind = kind; Zone = zone; WeekdayMask = weekdayMask; Feat = feat;
        }

        public static UnlockDef Arc(ZoneId zone) => new UnlockDef(UnlockKind.Arc, zone);
        public static UnlockDef WeekendEvent(ZoneId zone) => new UnlockDef(UnlockKind.Event, zone, DayMask(DayOfWeek.Saturday) | DayMask(DayOfWeek.Sunday));
        public static UnlockDef Hero() => new UnlockDef(UnlockKind.Hero);
        public static UnlockDef FeatOf(ZoneId zone, FeatKind feat) => new UnlockDef(UnlockKind.Feat, zone, 0, feat);

        public static int DayMask(DayOfWeek d) => 1 << (int)d;

        /// <summary>Vrai si l'événement est actif ce jour-là.</summary>
        public bool IsEventDay(DateTime localNow) => (WeekdayMask & DayMask(localNow.DayOfWeek)) != 0;
    }
}
