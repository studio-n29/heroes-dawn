using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Tournoi JcJ simulé côté client (L13 de la 0.15.0) : adversaires, manches, récompenses, classement, sauvegarde.</summary>
    public sealed class TournamentTests
    {
        static TestApp Signed()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            return app;
        }

        [Test, Description("Vérifie que les trois adversaires du jour sont stables pour un même joueur et un même jour, aux noms distincts, et de plus en plus forts en moyenne — parce que le tournoi est déterministe et se corse à chaque manche")]
        public void Opponents_AreStable_Distinct_AndGrowing()
        {
            var app = Signed();
            var a = app.Tournament.Opponents();
            var b = app.Tournament.Opponents();
            Assert.That(a.Count, Is.EqualTo(Tournament.Rounds));
            Assert.That(a.Select(o => o.Name), Is.EqualTo(b.Select(o => o.Name)));
            Assert.That(a.Select(o => o.Power), Is.EqualTo(b.Select(o => o.Power)));
            Assert.That(a.Select(o => o.Name).Distinct().Count(), Is.EqualTo(3));
            Assert.That(a[2].Power, Is.GreaterThan(a[0].Power));
        }

        [Test, Description("Vérifie que les chances de gagner montent avec l'écart de puissance et restent entre 10 % et 90 % — parce qu'aucune manche n'est ni perdue d'avance ni gagnée d'avance")]
        public void WinChance_FollowsThePowerGap_WithinBounds()
        {
            Assert.That(Tournament.WinChance(100, 100), Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(Tournament.WinChance(150, 100), Is.GreaterThan(0.5f));
            Assert.That(Tournament.WinChance(50, 100), Is.LessThan(0.5f));
            Assert.That(Tournament.WinChance(10000, 1), Is.EqualTo(0.9f));
            Assert.That(Tournament.WinChance(1, 10000), Is.EqualTo(0.1f));
        }

        [Test, Description("Vérifie qu'on joue les manches une à une jusqu'à la victoire ou l'élimination, que les points suivent (+25 ou −15) et qu'on ne peut plus jouer ensuite — parce que le tournoi s'arrête dès qu'il est fini")]
        public void Playing_RunsUntilTheEnd_AndUpdatesTheRating()
        {
            var app = Signed();
            int start = app.Tournament.Rating;
            int played = 0;
            while (!app.Tournament.Finished)
            {
                var r = app.Tournament.PlayNext();
                Assert.That(r.IsSuccess, Is.True);
                Assert.That(r.Value.Round, Is.EqualTo(played));
                played++;
                Assert.That(played, Is.LessThanOrEqualTo(Tournament.Rounds));
            }
            int wins = app.Tournament.Round;
            Assert.That(app.Tournament.Rating, Is.EqualTo(app.Tournament.Eliminated ? start - Tournament.LossPoints : start + Tournament.WinPoints * wins));
            Assert.That(app.Tournament.Champion, Is.EqualTo(wins == Tournament.Rounds));
            Assert.That(app.Tournament.PlayNext().IsFailure, Is.True);
        }

        [Test, Description("Vérifie que le même jour donne les mêmes issues pour une même escouade — parce que recharger la page ne doit pas changer le résultat")]
        public void Outcomes_AreDeterministicForTheDay()
        {
            string Run()
            {
                var app = Signed();
                var s = "";
                while (!app.Tournament.Finished) s += app.Tournament.PlayNext().Value.Won ? "W" : "L";
                return s;
            }
            Assert.That(Run(), Is.EqualTo(Run()));
        }

        [Test, Description("Vérifie que la récompense ne se réclame qu'une fois le tournoi fini, une seule fois, et grandit avec le nombre de manches gagnées — parce qu'on est payé selon sa place")]
        public void Claim_RequiresTheEnd_OnceOnly_AndScalesWithWins()
        {
            var app = Signed();
            Assert.That(app.Tournament.Claim().IsFailure, Is.True, "tournoi pas fini");
            while (!app.Tournament.Finished) app.Tournament.PlayNext();
            int credits = app.Session.Wallet.Credits;
            var claimed = app.Tournament.Claim();
            Assert.That(claimed.IsSuccess, Is.True);
            Assert.That(claimed.Value.Credits, Is.EqualTo(Tournament.RewardFor(app.Tournament.Round).Credits));
            Assert.That(app.Session.Wallet.Credits, Is.EqualTo(credits + claimed.Value.Credits));
            Assert.That(app.Tournament.Claim().IsFailure, Is.True, "une seule fois");
            Assert.That(Tournament.RewardFor(3).Credits, Is.GreaterThan(Tournament.RewardFor(2).Credits));
            Assert.That(Tournament.RewardFor(2).Credits, Is.GreaterThan(Tournament.RewardFor(1).Credits));
            Assert.That(Tournament.RewardFor(1).Credits, Is.GreaterThan(Tournament.RewardFor(0).Credits));
        }

        [Test, Description("Vérifie que le classement compte dix lignes dont le joueur, trié du plus haut au plus bas, avec des rangs de 1 à 10 — parce que c'est la page du classement")]
        public void Leaderboard_HasTenSortedRows_IncludingThePlayer()
        {
            var app = Signed();
            var board = app.Tournament.Leaderboard();
            Assert.That(board.Count, Is.EqualTo(Tournament.BoardSize));
            Assert.That(board.Select(e => e.Rank), Is.EqualTo(Enumerable.Range(1, Tournament.BoardSize)));
            Assert.That(board.Count(e => e.IsPlayer), Is.EqualTo(1));
            Assert.That(board.Select(e => e.Rating), Is.Ordered.Descending);
            Assert.That(board.Where(e => !e.IsPlayer).Select(e => e.Name).Distinct().Count(), Is.EqualTo(Tournament.BoardSize - 1));
            Assert.That(app.Tournament.PlayerRank, Is.EqualTo(board.First(e => e.IsPlayer).Rank));
        }

        [Test, Description("Vérifie que les points et le tournoi en cours survivent à une sauvegarde et qu'une ancienne sauvegarde sans le champ repart à 1000 points — parce que l'ajout ne doit pas casser les profils existants")]
        public void State_SurvivesSaveAndLoad()
        {
            var app = Signed();
            app.Tournament.PlayNext();
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(app.Session.ToProfile(System.DateTime.UtcNow))));
            Assert.That(restored.Meta.tournament.rating, Is.EqualTo(app.Tournament.Rating));
            Assert.That(restored.Meta.tournament.day, Is.EqualTo(app.Session.Meta.tournament.day));
            Assert.That(JsonUtility.FromJson<MetaSave>("{\"gems\":5}").tournament.rating, Is.EqualTo(Tournament.StartRating));
        }
    }
}
