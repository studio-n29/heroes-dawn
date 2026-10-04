using System;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class MetaServiceTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code<T>(Result<T> r) => r.Error?.Code ?? "ok";
        static string Code(Result r) => r.Error?.Code ?? "ok";

        // ------------------------------------------------------------------ Pass

        [Test, Description("Vérifie que 100 points font un niveau et que les récompenses d'un niveau atteint ne se prennent qu'une fois — parce que le pass est un barème de progression, pas une fontaine")]
        public void Pass_LevelsFromXp_ClaimOnlyOnce()
        {
            _app.Pass.AddXp(250);
            Assert.That(_app.Pass.Level, Is.EqualTo(2));
            int before = _app.Session.Wallet.Credits;
            var first = _app.Pass.Claim(1, premium: false);
            Assert.That(first.IsSuccess);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(before + PassTrack.Free(1).Credits));
            Assert.That(Code(_app.Pass.Claim(1, false)), Is.EqualTo("meta.claimed"));
            Assert.That(Code(_app.Pass.Claim(3, false)), Is.EqualTo("meta.level_locked"));
        }

        [Test, Description("Vérifie que la piste premium exige le pass premium (300 Gemmes) et qu'il ne s'achète qu'une fois — parce que la piste gratuite et la piste payante sont distinctes")]
        public void Pass_Premium_RequiresPurchase_CostsGems_Once()
        {
            _app.Pass.AddXp(100);
            Assert.That(Code(_app.Pass.Claim(1, true)), Is.EqualTo("meta.premium"));
            Assert.That(Code(_app.Pass.BuyPremium()), Is.EqualTo("meta.gems"));
            _app.Session.Wallet.SetGems(PassTrack.PremiumCost + 10);
            Assert.That(_app.Pass.BuyPremium().IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(10));
            Assert.That(Code(_app.Pass.BuyPremium()), Is.EqualTo("meta.done"));
            Assert.That(_app.Pass.Claim(1, true).IsSuccess);
        }

        [Test, Description("Vérifie que les points s'arrêtent au niveau 30 — parce qu'un pass a un sommet")]
        public void Pass_Xp_IsCappedAtLevel30()
        {
            _app.Pass.AddXp(1_000_000);
            Assert.That(_app.Pass.Level, Is.EqualTo(PassTrack.Levels));
            Assert.That(_app.Pass.IsMax);
        }

        [Test, Description("Vérifie qu'une saison de 30 jours terminée repart de zéro (points, pass premium, récompenses) — parce que le pass est saisonnier")]
        public void Pass_Season_ResetsAfter30Days()
        {
            _app.Pass.AddXp(500);
            _app.Session.Wallet.SetGems(500);
            _app.Pass.BuyPremium();
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(PassTrack.SeasonDays + 1);
            _app.Pass.EnsureSeason();
            Assert.That(_app.Pass.Xp, Is.EqualTo(0));
            Assert.That(_app.Pass.PremiumOwned, Is.False);
        }

        // ------------------------------------------------------------------ Missions du jour

        [Test, Description("Vérifie que chaque jour apporte trois missions de types différents, les mêmes pour une même date — parce que les missions sont tirées d'une graine de date")]
        public void Daily_ThreeDistinctMissions_StableForTheDay()
        {
            var a = _app.Daily.Today().Select(v => v.Template.Id).ToList();
            Assert.That(a.Count, Is.EqualTo(3));
            Assert.That(_app.Daily.Today().Select(v => v.Template.Kind).Distinct().Count(), Is.EqualTo(3));
            var again = new DailyMissions(_app.MetaCtx, _app.Pass).Today().Select(v => v.Template.Id).ToList();
            Assert.That(again, Is.EqualTo(a));
        }

        [Test, Description("Vérifie qu'une mission terminée se récupère une fois, verse sa récompense et fait avancer le pass — parce que les missions du jour nourrissent le pass")]
        public void Daily_Complete_ClaimPaysAndAddsPassXp_Once()
        {
            var m = _app.Daily.Today()[0];
            Assert.That(Code(_app.Daily.Claim(m.Template.Id)), Is.EqualTo("meta.not_ready"));
            _app.Daily.Record(m.Template.Kind, m.Template.Target);
            int credits = _app.Session.Wallet.Credits, xp = _app.Pass.Xp;
            Assert.That(_app.Daily.Claim(m.Template.Id).IsSuccess);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + m.Template.Reward.Credits));
            Assert.That(_app.Pass.Xp, Is.EqualTo(xp + m.Template.Reward.PassXp));
            Assert.That(Code(_app.Daily.Claim(m.Template.Id)), Is.EqualTo("meta.claimed"));
        }

        [Test, Description("Vérifie que le lendemain apporte de nouvelles missions avec une progression remise à zéro — parce que les missions se renouvellent chaque jour")]
        public void Daily_NextDay_ResetsProgress()
        {
            var m = _app.Daily.Today()[0];
            _app.Daily.Record(m.Template.Kind, m.Template.Target);
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(1);
            Assert.That(_app.Daily.Today().All(v => v.Progress == 0 && !v.Claimed));
        }

        // ------------------------------------------------------------------ Suivi

        [Test, Description("Vérifie que l'entraînement et le recrutement sont déduits des changements de l'effectif — parce que les missions du jour doivent avancer sans modifier les cas d'usage existants")]
        public void Tracker_DetectsTrainingAndRecruitment_FromRosterChanges()
        {
            _app.Tracker.Start();
            _app.Publisher<GameStateChanged>().Publish(new GameStateChanged(GameState.Login, GameState.Hub));
            var train = DailyMissions.Catalog.First(t => t.Kind == DailyKind.TrainMember);
            var recruit = DailyMissions.Catalog.First(t => t.Kind == DailyKind.RecruitHero);
            _app.Session.Meta.daily.Clear();
            _app.Session.Meta.dailyDay = _app.MetaCtx.Today;
            _app.Session.Meta.daily.Add(new DailySave { id = train.Id });
            _app.Session.Meta.daily.Add(new DailySave { id = recruit.Id });

            _app.Session.Find(new MemberId("ancre"))!.Level++;
            _app.Publisher<RosterChanged>().Publish(default);
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(new MemberId("etincelle")), 5));
            _app.Publisher<RosterChanged>().Publish(default);

            var views = _app.Daily.Today();
            Assert.That(views.First(v => v.Template.Id == train.Id).Progress, Is.EqualTo(1));
            Assert.That(views.First(v => v.Template.Id == recruit.Id).Progress, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que les Crédits versés par une récompense ne comptent pas comme « Crédits gagnés » — parce qu'une mission ne doit pas se valider elle-même")]
        public void Tracker_IgnoresGrantedRewards_ForEarnedCredits()
        {
            _app.Tracker.Start();
            _app.Publisher<GameStateChanged>().Publish(new GameStateChanged(GameState.Login, GameState.Hub));
            var earn = DailyMissions.Catalog.First(t => t.Kind == DailyKind.EarnCredits);
            _app.Session.Meta.daily.Clear();
            _app.Session.Meta.dailyDay = _app.MetaCtx.Today;
            _app.Session.Meta.daily.Add(new DailySave { id = earn.Id });
            _app.MetaCtx.Grant(new Reward(credits: 5000));
            Assert.That(_app.Daily.Today()[0].Progress, Is.EqualTo(0));
            _app.Session.Wallet.Add(300, 0);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(_app.Session.Wallet.Credits, _app.Session.Wallet.Pr));
            Assert.That(_app.Daily.Today()[0].Progress, Is.EqualTo(300));
        }

        [Test, Description("Vérifie que le contenu du profil chargé à la connexion ne compte pas comme progression — parce qu'un joueur qui revient ne doit pas valider des missions en se connectant")]
        public void Tracker_DoesNothing_BeforeEnteringTheHub()
        {
            _app.Tracker.Start();
            var earn = DailyMissions.Catalog.First(t => t.Kind == DailyKind.EarnCredits);
            _app.Session.Meta.dailyDay = _app.MetaCtx.Today;
            _app.Session.Meta.daily.Clear();
            _app.Session.Meta.daily.Add(new DailySave { id = earn.Id });
            _app.Session.Wallet.Add(9999, 0);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(_app.Session.Wallet.Credits, 0));
            Assert.That(_app.Daily.Today()[0].Progress, Is.EqualTo(0));
        }

        // ------------------------------------------------------------------ Objectifs secondaires

        [Test, Description("Vérifie qu'aucun objectif secondaire n'apparaît sans zone validée, puis qu'il apparaît pour une zone validée avec une échéance de 3 h — parce que ce sont des rappels de zones déjà sécurisées")]
        public void Secondary_AppearsOnlyForValidatedZone_WithDeadline()
        {
            Assert.That(_app.Secondary.Active, Is.Null);
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var a = _app.Secondary.Active;
            Assert.That(a, Is.Not.Null);
            Assert.That(a!.Value.ZoneId, Is.EqualTo("zone_ny_1_1"));
            Assert.That(a.Value.Left.TotalMinutes, Is.InRange(170, 181));
        }

        [Test, Description("Vérifie que valider la zone de l'objectif verse la récompense et lance un délai de repos, et qu'une autre zone ne le remplit pas — parce que l'objectif porte sur une zone précise")]
        public void Secondary_CompletedByItsZone_Only()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            _ = _app.Secondary.Active;
            Assert.That(_app.Secondary.OnZoneValidated("zone_ny_1_2"), Is.Null);
            int gems = _app.Session.Wallet.Gems;
            var prize = _app.Secondary.OnZoneValidated("zone_ny_1_1");
            Assert.That(prize, Is.Not.Null);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems + SecondaryObjectives.Prize.Gems));
            Assert.That(_app.Secondary.Active, Is.Null, "délai de repos après un objectif rempli");
        }

        [Test, Description("Vérifie qu'un objectif expiré disparaît puis qu'un nouveau n'arrive qu'après le délai de repos — parce que le joueur ne doit pas être harcelé")]
        public void Secondary_Expires_ThenCooldown()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            _ = _app.Secondary.Active;
            _app.Time.UtcNow = _app.Time.UtcNow.Add(SecondaryObjectives.Duration + TimeSpan.FromMinutes(1));
            Assert.That(_app.Secondary.Active, Is.Null);
            _app.Time.UtcNow = _app.Time.UtcNow.Add(SecondaryObjectives.Cooldown + TimeSpan.FromMinutes(1));
            Assert.That(_app.Secondary.Active, Is.Not.Null);
        }

        // ------------------------------------------------------------------ Opérations

        void FreeMembers(int n)
        {
            foreach (var id in new[] { "etincelle", "arc", "rempart", "verre" })
            {
                if (n-- <= 0) break;
                _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(new MemberId(id)), 10));
            }
        }

        [Test, Description("Vérifie qu'une expédition exige des membres libres hors escouade — parce que l'escouade reste disponible pour les missions")]
        public void Operations_NeedFreeMembersOutsideSquad()
        {
            Assert.That(Code(_app.Operations.Start(0, "patrol")), Is.EqualTo("meta.no_members"));
            FreeMembers(1);
            Assert.That(_app.Operations.Start(0, "patrol").IsSuccess);
            Assert.That(Code(_app.Operations.Start(0, "patrol")), Is.EqualTo("meta.busy"));
            Assert.That(Code(_app.Operations.Start(1, "patrol")), Is.EqualTo("meta.no_members"), "le seul membre libre est déjà parti");
        }

        [Test, Description("Vérifie qu'on ne récupère une expédition qu'à son retour, avec une récompense majorée par le niveau moyen et de la fatigue — parce que le temps réel est le prix de la récompense")]
        public void Operations_ClaimAfterTime_PaysBonusAndTires()
        {
            FreeMembers(1);
            _app.Operations.Start(0, "patrol");
            Assert.That(Code(_app.Operations.Claim(0)), Is.EqualTo("meta.not_ready"));
            _app.Time.UtcNow = _app.Time.UtcNow.AddMinutes(16);
            int credits = _app.Session.Wallet.Credits;
            var r = _app.Operations.Claim(0);
            Assert.That(r.IsSuccess);
            Assert.That(r.Value.Credits, Is.EqualTo(250 * 110 / 100), "niveau 10 : +10 %");
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + r.Value.Credits));
            Assert.That(_app.Session.Find(new MemberId("etincelle"))!.Fatigue, Is.EqualTo(Expeditions.FatiguePerTrip));
            Assert.That(_app.Operations.SlotViews()[0].Empty);
        }

        [Test, Description("Vérifie qu'accélérer une expédition coûte des Gemmes (1 par 5 min restantes, au moins 1) et la rend récupérable — parce que les Gemmes achètent du temps")]
        public void Operations_SpeedUp_CostsGems()
        {
            FreeMembers(1);
            _app.Operations.Start(0, "patrol");
            _app.Session.Wallet.SetGems(0);
            Assert.That(Code(_app.Operations.SpeedUp(0)), Is.EqualTo("meta.gems"));
            _app.Session.Wallet.SetGems(10);
            Assert.That(_app.Operations.SpeedUp(0).IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(10 - 3));
            Assert.That(_app.Operations.Claim(0).IsSuccess);
        }

        [Test, Description("Vérifie qu'un membre en expédition ne peut pas rejoindre l'escouade — parce qu'il n'est pas au QG")]
        public void Operations_BusyMember_CannotJoinSquad()
        {
            FreeMembers(1);
            _app.Operations.Start(0, "patrol");
            var r = _app.Hub.ToggleSquad.ExecuteAsync(new MemberId("etincelle")).GetAwaiter().GetResult();
            Assert.That(Code(r), Is.EqualTo("hub.expedition"));
        }

        // ------------------------------------------------------------------ Labo

        [Test, Description("Vérifie qu'une recherche coûte des Crédits, dure plusieurs minutes, n'a lieu qu'une à la fois et améliore le niveau à son terme — parce que le labo est un investissement à long terme")]
        public void Lab_ResearchCostsTimeAndCredits_OneAtATime()
        {
            _app.Session.Wallet.Set(0, 0);
            Assert.That(Code(_app.Lab.Start(ResearchLab.Training)), Is.EqualTo("meta.credits"));
            _app.Session.Wallet.Set(10_000, 0);
            Assert.That(_app.Lab.Start(ResearchLab.Training).IsSuccess);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(10_000 - 500));
            Assert.That(Code(_app.Lab.Start(ResearchLab.Loot)), Is.EqualTo("meta.busy"));
            Assert.That(_app.Lab.Refresh(), Is.Null);
            _app.Time.UtcNow = _app.Time.UtcNow.AddMinutes(11);
            Assert.That(_app.Lab.Refresh()?.Id, Is.EqualTo(ResearchLab.Training));
            Assert.That(LabBonuses.Level(_app.Session.Meta, ResearchLab.Training), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que la recherche d'entraînement réduit le prix de 4 % par niveau dans le cas d'usage réel — parce qu'un bonus affiché doit s'appliquer")]
        public void Lab_TrainingBonus_ReducesTrainCost()
        {
            int level = 10;
            Assert.That(_app.Session.TrainCostFor(level), Is.EqualTo(MetaRules.TrainCost(level)));
            _app.Session.Meta.SetCounter(_app.Session.Meta.labLevels, ResearchLab.Training, 2);
            Assert.That(_app.Session.TrainCostFor(level), Is.EqualTo(MetaRules.TrainCost(level) * 92 / 100));
        }

        [Test, Description("Vérifie qu'on ne dépasse pas le niveau maximum d'une recherche — parce que chaque recherche a 5 niveaux")]
        public void Lab_MaxLevel_Refused()
        {
            _app.Session.Meta.SetCounter(_app.Session.Meta.labLevels, ResearchLab.Loot, 5);
            _app.Session.Wallet.Set(1_000_000, 0);
            Assert.That(Code(_app.Lab.Start(ResearchLab.Loot)), Is.EqualTo("meta.max_level"));
        }

        // ------------------------------------------------------------------ Archives

        [Test, Description("Vérifie que les fiches se débloquent avec la progression (héros recrutés, zones validées, ennemis de ces zones) et qu'une fiche lue n'est plus « non lue » — parce que les archives récompensent l'exploration")]
        public void Archives_UnlockWithProgress_AndTrackUnread()
        {
            var start = _app.Archives.Entries();
            Assert.That(start.First(e => e.Id == "lore.fracture").Unlocked);
            Assert.That(start.First(e => e.Id == "zone.zone_ny_1_1").Unlocked, Is.False);
            Assert.That(start.First(e => e.Id == "enemy.brick").Unlocked, Is.False);
            Assert.That(start.First(e => e.Id == "hero.ancre").Unlocked, "héros de départ");
            int unread = _app.Archives.UnreadCount();
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var after = _app.Archives.Entries();
            Assert.That(after.First(e => e.Id == "enemy.brick").Unlocked);
            Assert.That(after.First(e => e.Id == "zone.zone_ny_1_1").Unlocked);
            Assert.That(_app.Archives.UnreadCount(), Is.GreaterThan(unread));
            _app.Archives.MarkRead("enemy.brick");
            Assert.That(_app.Archives.Entries().First(e => e.Id == "enemy.brick").Unread, Is.False);
        }

        // ------------------------------------------------------------------ Sauvegarde

        [Test, Description("Vérifie que toute la méta-progression survit à un aller-retour par la sauvegarde JSON — parce que Gemmes, pass, expéditions et réglages ne doivent pas se perdre")]
        public void Meta_SurvivesProfileJsonRoundTrip()
        {
            _app.Pass.AddXp(321);
            _app.Session.Wallet.SetGems(77);
            FreeMembers(1);
            _app.Operations.Start(1, "patrol");
            _app.Session.Meta.settings.music = 33;
            var json = JsonUtility.ToJson(_app.Session.ToProfile(_app.Time.UtcNow));
            var loaded = JsonUtility.FromJson<ProfileData>(json);
            var fresh = new TestApp();
            fresh.Session.ApplyProfile(loaded);
            Assert.That(fresh.Session.Wallet.Gems, Is.EqualTo(77));
            Assert.That(fresh.Pass.Xp, Is.EqualTo(321));
            Assert.That(fresh.Session.Meta.expeditions.Count, Is.EqualTo(1));
            Assert.That(fresh.Session.Meta.settings.music, Is.EqualTo(33));
        }

        [Test, Description("Vérifie qu'un ancien profil sans bloc méta se charge avec les valeurs par défaut (50 Gemmes) — parce que la mise à jour ne doit casser aucune sauvegarde existante")]
        public void OldProfile_WithoutMeta_LoadsWithDefaults()
        {
            var old = JsonUtility.FromJson<ProfileData>("{\"schemaVersion\":3,\"credits\":10,\"pr\":5}");
            _app.Session.ApplyProfile(old);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(50));
            Assert.That(_app.Session.Meta.settings.master, Is.EqualTo(100));
        }
    }
}
