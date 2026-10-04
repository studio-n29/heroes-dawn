#nullable enable
using System;
using N29.HeroesDawn.Domain.Shop;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>« Nouveaux héros dans 5 h 12 » ; en minutes sous une heure (QG et Recruter).</summary>
    public static class ShopCountdown
    {
        public static string Text(DateTime local, int resetHour, ILocalization loc)
        {
            var next = ShopDay.NextReset(local, resetHour);
            int minutes = Math.Max(1, (int)Math.Ceiling((next - local).TotalMinutes));
            return minutes < 60 ? loc.F("shop.countdown.m", minutes) : loc.F("shop.countdown.h", minutes / 60, minutes % 60);
        }
    }
}
