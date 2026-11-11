#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Un tournoi annuel : dates récurrentes (mois et jour, la fin peut tomber l'année suivante) et lot de récompenses.</summary>
    public sealed class TournamentDef
    {
        public string Id = "";
        public string NameKey = "";
        public (int Month, int Day) Start;
        public (int Month, int Day) End;
        public string RewardsId = "";

        public TournamentDef() { }

        public TournamentDef(string id, string nameKey, (int Month, int Day) start, (int Month, int Day) end, string rewardsId)
        {
            Id = id; NameKey = nameKey; Start = start; End = end; RewardsId = rewardsId;
        }

        /// <summary>Lit une date « MM-JJ » ; renvoie faux si le texte est invalide.</summary>
        public static bool TryParseDay(string text, out (int Month, int Day) day)
        {
            day = default;
            var parts = (text ?? "").Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int m) || !int.TryParse(parts[1], out int d)) return false;
            if (m < 1 || m > 12 || d < 1 || d > DateTime.DaysInMonth(2024, m)) return false;
            day = (m, d);
            return true;
        }

        /// <summary>La fin tombe l'année suivante (ex. 29/12 → 10/01).</summary>
        public bool CrossesYear => End.Month * 100 + End.Day < Start.Month * 100 + Start.Day;
    }

    /// <summary>Règles du calendrier : quel tournoi est actif, quel est le suivant, dates réelles d'une édition.</summary>
    public static class TournamentCalendar
    {
        static DateTime Make(int year, (int Month, int Day) md)
            => new DateTime(year, md.Month, Math.Min(md.Day, DateTime.DaysInMonth(year, md.Month)));

        /// <summary>Le tournoi actif ce jour-là (jour inclus aux deux bornes, passage d'année géré), nul entre deux tournois.</summary>
        public static TournamentDef? ActiveOn(IReadOnlyList<TournamentDef> all, DateTime day)
        {
            int key = day.Month * 100 + day.Day;
            foreach (var t in all)
            {
                int s = t.Start.Month * 100 + t.Start.Day, e = t.End.Month * 100 + t.End.Day;
                bool active = s <= e ? key >= s && key <= e : key >= s || key <= e;
                if (active) return t;
            }
            return null;
        }

        /// <summary>Le prochain tournoi à démarrer après ce jour (y compris l'an prochain).</summary>
        public static TournamentDef Next(IReadOnlyList<TournamentDef> all, DateTime day)
        {
            if (all.Count == 0) throw new ArgumentException("Aucun tournoi défini.", nameof(all));
            TournamentDef? best = null; DateTime bestStart = DateTime.MaxValue;
            foreach (var t in all)
            {
                var start = Make(day.Year, t.Start);
                if (start <= day.Date) start = Make(day.Year + 1, t.Start);
                if (start < bestStart) { best = t; bestStart = start; }
            }
            return best!;
        }

        /// <summary>Dates réelles de l'édition qui contient la date de référence, ou à défaut de la suivante.</summary>
        public static (DateTime Start, DateTime End) Window(TournamentDef t, DateTime reference)
        {
            var r = reference.Date;
            int first = t.CrossesYear ? r.Year - 1 : r.Year;
            for (int ys = first; ys <= r.Year + 1; ys++)
            {
                var start = Make(ys, t.Start);
                var end = Make(t.CrossesYear ? ys + 1 : ys, t.End);
                if (r <= end) return (start, end);
            }
            var s = Make(r.Year + 1, t.Start);
            return (s, Make(t.CrossesYear ? r.Year + 2 : r.Year + 1, t.End));
        }
    }
}
