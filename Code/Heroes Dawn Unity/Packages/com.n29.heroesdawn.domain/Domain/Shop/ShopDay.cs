#nullable enable
using System;
using System.Globalization;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Jour de boutique : de <c>resetHour</c>:00 locale à <c>resetHour</c>:00 le lendemain (heure murale).</summary>
    public static class ShopDay
    {
        /// <summary>Clé du jour de boutique (<c>yyyy-MM-dd</c>) : 8h59 appartient encore à la veille quand <paramref name="resetHour"/> = 9.</summary>
        public static string Of(DateTime local, int resetHour)
            => local.AddHours(-resetHour).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>Prochain renouvellement (<paramref name="resetHour"/>:00 locale) strictement après <paramref name="local"/>.</summary>
        public static DateTime NextReset(DateTime local, int resetHour)
        {
            var today = new DateTime(local.Year, local.Month, local.Day, resetHour, 0, 0, local.Kind);
            return local < today ? today : today.AddDays(1);
        }
    }
}
