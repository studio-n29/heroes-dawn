using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Calendrier annuel des tournois — bloc D de la 1.0.0.</summary>
    public sealed class TournamentCalendarTests
    {
        static TournamentDef T(string id, int sm, int sd, int em, int ed) => new TournamentDef(id, "tournament." + id, (sm, sd), (em, ed), id);

        static readonly IReadOnlyList<TournamentDef> All = new[]
        {
            T("new_year", 12, 29, 1, 10), T("valentine", 2, 7, 2, 16), T("spring", 3, 20, 4, 3), T("summer", 6, 14, 6, 28),
            T("holidays", 7, 14, 7, 31), T("back_to_school", 9, 1, 9, 14), T("halloween", 10, 20, 11, 3), T("christmas", 12, 1, 12, 26),
        };

        static string Active(int y, int m, int d) => TournamentCalendar.ActiveOn(All, new DateTime(y, m, d))?.Id ?? "";

        [Test, Description("Vérifie que Nouvel An est actif le 31/12 et le 02/01 — parce que le tournoi chevauche deux années")]
        public void Nouvel_An_est_actif_le_31_12_et_le_02_01()
        {
            Assert.That(Active(2026, 12, 31), Is.EqualTo("new_year"));
            Assert.That(Active(2027, 1, 2), Is.EqualTo("new_year"));
            Assert.That(Active(2026, 12, 29), Is.EqualTo("new_year"));
            Assert.That(Active(2027, 1, 10), Is.EqualTo("new_year"));
        }

        [Test, Description("Vérifie que Nouvel An n'est actif ni le 28/12 ni le 11/01 — parce que les bornes sont incluses mais pas au-delà")]
        public void Nouvel_An_n_est_pas_actif_le_28_12_ni_le_11_01()
        {
            Assert.That(Active(2026, 12, 28), Is.Not.EqualTo("new_year"));
            Assert.That(Active(2027, 1, 11), Is.EqualTo(""));
        }

        [Test, Description("Vérifie que Noël est actif du 01/12 au 26/12 inclus — parce que les deux bornes comptent")]
        public void Noel_est_actif_du_01_12_au_26_12_inclus()
        {
            Assert.That(Active(2026, 11, 30), Is.EqualTo(""));
            Assert.That(Active(2026, 12, 1), Is.EqualTo("christmas"));
            Assert.That(Active(2026, 12, 26), Is.EqualTo("christmas"));
        }

        [Test, Description("Vérifie qu'aucun tournoi n'est actif le 27/12 — parce que c'est l'intervalle entre Noël et Nouvel An")]
        public void Aucun_tournoi_le_27_12()
        {
            Assert.That(TournamentCalendar.ActiveOn(All, new DateTime(2026, 12, 27)), Is.Null);
        }

        [Test, Description("Vérifie que le prochain tournoi du 27/12 est Nouvel An — parce que la page affiche le suivant entre deux tournois")]
        public void Next_le_27_12_est_Nouvel_An()
        {
            Assert.That(TournamentCalendar.Next(All, new DateTime(2026, 12, 27)).Id, Is.EqualTo("new_year"));
        }

        [Test, Description("Vérifie que le prochain tournoi du 11/01 est la Saint-Valentin — parce que Nouvel An est fini")]
        public void Next_le_11_01_est_Saint_Valentin()
        {
            Assert.That(TournamentCalendar.Next(All, new DateTime(2027, 1, 11)).Id, Is.EqualTo("valentine"));
        }

        [Test, Description("Vérifie que le prochain tournoi du 15/12 est Nouvel An — parce que Noël est en cours et que le suivant démarre le 29/12")]
        public void Next_pendant_un_tournoi_donne_le_suivant()
        {
            Assert.That(TournamentCalendar.Next(All, new DateTime(2026, 12, 15)).Id, Is.EqualTo("new_year"));
        }

        [Test, Description("Vérifie que l'édition de Nouvel An qui contient le 02/01 commence l'année précédente — parce que les dates réelles servent aux délais de réclamation")]
        public void Window_de_Nouvel_An_le_02_01_commence_l_annee_precedente()
        {
            var w = TournamentCalendar.Window(All[0], new DateTime(2027, 1, 2));
            Assert.That(w.Start, Is.EqualTo(new DateTime(2026, 12, 29)));
            Assert.That(w.End, Is.EqualTo(new DateTime(2027, 1, 10)));
        }

        [Test, Description("Vérifie que, l'édition passée, Window renvoie l'édition suivante — parce que la page annonce le prochain tournoi")]
        public void Window_apres_la_fin_renvoie_l_edition_suivante()
        {
            var w = TournamentCalendar.Window(All[1], new DateTime(2026, 3, 1));
            Assert.That(w.Start, Is.EqualTo(new DateTime(2027, 2, 7)));
            var n = TournamentCalendar.Window(All[0], new DateTime(2027, 1, 11));
            Assert.That(n.Start, Is.EqualTo(new DateTime(2027, 12, 29)));
        }

        [Test, Description("Vérifie que TryParseDay refuse les dates impossibles — parce qu'une ligne invalide du fichier doit être ignorée")]
        public void TryParseDay_refuse_les_dates_impossibles()
        {
            Assert.That(TournamentDef.TryParseDay("13-01", out _), Is.False);
            Assert.That(TournamentDef.TryParseDay("02-30", out _), Is.False);
            Assert.That(TournamentDef.TryParseDay("12-29", out var d), Is.True);
            Assert.That(d, Is.EqualTo((12, 29)));
        }
    }
}
