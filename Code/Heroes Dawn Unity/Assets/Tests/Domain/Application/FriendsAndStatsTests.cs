using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Amis (port simulé local) et compteurs de statistiques du profil.</summary>
    public sealed class FriendsAndStatsTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie qu'un tag d'ami est refusé s'il est mal formé et enregistré rogné et normalisé sinon — parce que la liste d'amis ne doit contenir que des tags valides")]
        public void Add_StoresATrimmedNormalizedTag()
        {
            Assert.That(Code(_app.Friends.Add("  Mara#7 ")), Is.EqualTo("friends.invalid"));
            Assert.That(_app.Friends.Add(" Mara#0007 ").IsSuccess, Is.True);
            var friends = _app.Friends.Friends();
            Assert.That(friends.Count, Is.EqualTo(1));
            Assert.That((friends[0].Tag, friends[0].Name), Is.EqualTo(("Mara#0007", "Mara")));
        }

        [Test, Description("Vérifie le refus de son propre tag, des doublons (sans tenir compte de la casse) et d'un 51e ami — parce que chaque ami est unique et la liste est bornée")]
        public void Add_RefusesSelfDuplicatesAndOverflow()
        {
            string own = PlayerTag.Of("p1", _app.Session.Meta.onboarding.pseudo);
            Assert.That(Code(_app.Friends.Add(own)), Is.EqualTo("friends.self"));
            Assert.That(_app.Friends.Add("Mara#0007").IsSuccess, Is.True);
            Assert.That(Code(_app.Friends.Add("mara#0007")), Is.EqualTo("friends.duplicate"));
            for (int i = 1; i < LocalFriendsPort.MaxFriends; i++) Assert.That(_app.Friends.Add("Ami#" + (1000 + i)).IsSuccess, Is.True, "ami " + i);
            Assert.That(Code(_app.Friends.Add("Trop#0001")), Is.EqualTo("friends.full"));
            Assert.That(_app.Friends.Friends().Count, Is.EqualTo(LocalFriendsPort.MaxFriends));
        }

        [Test, Description("Vérifie que retirer un ami ne tient pas compte de la casse et qu'en retirer un absent ne change rien — parce que Retirer doit être sans risque")]
        public void Remove_IsCaseInsensitive_AndIdempotent()
        {
            _app.Friends.Add("Mara#0007"); _app.Friends.Add("Iris#0002");
            Assert.That(_app.Friends.Remove("MARA#0007").IsSuccess, Is.True);
            Assert.That(_app.Friends.Friends().Select(f => f.Tag), Is.EqualTo(new[] { "Iris#0002" }));
            Assert.That(_app.Friends.Remove("Zed#9999").IsSuccess, Is.True);
            Assert.That(_app.Friends.Friends().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que la liste d'amis et son ordre survivent à une sauvegarde et qu'un profil neuf n'a ni ami ni compteur — parce que les anciennes sauvegardes doivent rester valides")]
        public void Friends_SurviveSaveAndReload_OldSaveHasNone()
        {
            _app.Friends.Add("Mara#0007"); _app.Friends.Add("Iris#0002");
            Stats.Add(_app.Session.Meta, Stats.CombatsWon, 3);
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(_app.Session.ToProfile(System.DateTime.UtcNow))));
            Assert.That(restored.Meta.friends, Is.EqualTo(new[] { "Mara#0007", "Iris#0002" }));
            Assert.That(Stats.Get(restored.Meta, Stats.CombatsWon), Is.EqualTo(3));
            var fresh = new MetaSave();
            Assert.That(fresh.friends, Is.Empty);
            Assert.That(Stats.Get(fresh, Stats.CreditsEarned), Is.EqualTo(0));
        }

        static CombatReport Report(bool victory) => new CombatReport(victory, 200, CombatRank.Easy, false, new List<FatigueChange>(), new List<BoardEvent>(), false);

        [Test, Description("Vérifie que le suivi compte les combats gagnés et perdus et tous les Crédits gagnés (récompenses versées comprises), mais pas les dépenses — parce que le profil affiche ces totaux")]
        public void Tracker_CountsCombatsAndCredits()
        {
            _app.Tracker.Start();
            _app.Publisher<GameStateChanged>().Publish(new GameStateChanged(GameState.Login, GameState.Hub));
            var m = _app.Session.Meta;
            _app.Publisher<CombatFinished>().Publish(new CombatFinished(Report(true)));
            Assert.That((Stats.Get(m, Stats.CombatsWon), Stats.Get(m, Stats.CombatsLost)), Is.EqualTo((1, 0)));
            _app.Publisher<CombatFinished>().Publish(new CombatFinished(Report(false)));
            _app.Publisher<CombatFinished>().Publish(new CombatFinished(Report(false)));
            Assert.That((Stats.Get(m, Stats.CombatsWon), Stats.Get(m, Stats.CombatsLost)), Is.EqualTo((1, 2)));

            _app.Session.Wallet.Add(300, 0);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(_app.Session.Wallet.Credits, _app.Session.Wallet.Pr));
            Assert.That(Stats.Get(m, Stats.CreditsEarned), Is.EqualTo(300));
            _app.MetaCtx.Grant(new Reward(credits: 5000));
            Assert.That(Stats.Get(m, Stats.CreditsEarned), Is.EqualTo(5300));
            _app.Session.Wallet.TrySpend(100, 0);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(_app.Session.Wallet.Credits, _app.Session.Wallet.Pr));
            Assert.That(Stats.Get(m, Stats.CreditsEarned), Is.EqualTo(5300));
        }

        [Test, Description("Vérifie que rien n'est compté avant l'entrée dans le hub — parce que le contenu du profil chargé ne doit pas gonfler les statistiques")]
        public void Tracker_CountsNothing_BeforeTheHub()
        {
            _app.Tracker.Start();
            _app.Session.Wallet.Add(9999, 0);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(_app.Session.Wallet.Credits, 0));
            Assert.That(Stats.Get(_app.Session.Meta, Stats.CreditsEarned), Is.EqualTo(0));
        }
    }
}
