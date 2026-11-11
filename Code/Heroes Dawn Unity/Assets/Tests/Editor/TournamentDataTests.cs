using System;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class TournamentDataTests
    {
        [Test, Description("Vérifie que tournaments.json contient 8 tournois sans chevauchement sur toute l'année — parce qu'un seul tournoi doit être actif à la fois")]
        public void Le_fichier_JSON_contient_8_tournois_sans_chevauchement()
        {
            var all = TournamentLibrary.Load();
            Assert.That(all.Count, Is.EqualTo(8));
            for (var day = new DateTime(2027, 1, 1); day.Year == 2027; day = day.AddDays(1))
            {
                int active = 0;
                foreach (var t in all)
                    if (TournamentCalendar.ActiveOn(new[] { t }, day) != null) active++;
                Assert.That(active, Is.LessThanOrEqualTo(1), "chevauchement le " + day.ToString("dd/MM"));
            }
        }
    }
}
