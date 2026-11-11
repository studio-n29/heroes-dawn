#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Service de tournoi JcJ (bloc D de la 1.0.0) : duels, points, plafond, repli, réclamations — faux serveur et faux canal en mémoire.</summary>
    public sealed class TournamentArenaTests
    {
        static readonly DateTime Christmas = new DateTime(2026, 12, 10, 12, 0, 0);
        const string Key = "christmas:2026";

        static TestApp Ready(DateTime? day = null, bool signed = true)
        {
            var app = new TestApp();
            if (signed) { app.SignInDirect("me"); app.Arena.PlayerId = "me"; }
            Move(app, day ?? Christmas);
            return app;
        }

        static void Move(TestApp app, DateTime when)
        {
            app.Time.LocalNow = when;
            app.Time.UtcNow = DateTime.SpecifyKind(when, DateTimeKind.Utc);
        }

        static DefenseSnapshot Rival(TestApp app, string id, int levelBonus = 0)
        {
            var snap = ArenaSnapshots.Capture(app.Session, app.Time.UtcNow);
            snap.PlayerId = id; snap.Name = "Rival " + id;
            foreach (var t in snap.Team) t.level += levelBonus;
            snap.Rating = ArenaSnapshots.RatingOf(snap.Team.Select(t => t.level));
            return snap;
        }

        static DuelTicket Ticket(DefenseSnapshot opponent) => new DuelTicket { MatchId = "online-1", Seed = 4242, Opponent = opponent };

        /// <summary>Fixe les points du joueur dans l'édition en cours (côté sauvegarde et côté faux serveur).</summary>
        static void SeedPoints(TestApp app, int points)
        {
            app.Tournament.Current();
            app.Session.Meta.arena.points = points;
            app.Arena.SetPoints(Key, "me", points);
        }

        static T Run<T>(UniTask<T> task) => task.GetAwaiter().GetResult();
        static void Run(UniTask task) => task.GetAwaiter().GetResult();

        static DuelSession Start(TestApp app, DuelMode mode)
        {
            var r = Run(app.Tournament.StartDuelAsync(mode, default));
            Assert.That(r.IsSuccess, Is.True, r.IsFailure ? r.Error!.Code : "");
            return r.Value;
        }

        [Test, Description("Vérifie qu'un duel hors ligne gagné donne 10 points et compte pour un duel du jour — parce que la victoire hors ligne vaut +10")]
        public void Un_duel_hors_ligne_gagne_donne_10_points_et_compte_un_duel()
        {
            var app = Ready();
            app.Arena.AddDefense(Rival(app, "rival"));
            var duel = Start(app, DuelMode.Offline);
            Assert.That(duel.Mode, Is.EqualTo(DuelMode.Offline));
            Assert.That(duel.Setup.OpponentTeam.Count, Is.GreaterThan(0));
            Assert.That(duel.Setup.EnemyControl, Is.EqualTo(SideControl.Ai));
            var report = Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Win, default)).Value;
            Assert.That(report.PointsDelta, Is.EqualTo(10));
            Assert.That(report.TotalPoints, Is.EqualTo(10));
            Assert.That(report.Counted, Is.True);
            var view = app.Tournament.Current();
            Assert.That(view.Points, Is.EqualTo(10));
            Assert.That(view.DuelsLeft, Is.EqualTo(ArenaRules.DailyDuelCap - 1));
        }

        [Test, Description("Vérifie qu'un duel en ligne gagné donne 20 points, l'hôte étant l'identifiant le plus petit et le camp adverse mené de l'extérieur — parce que la victoire en ligne vaut +20 et que les deux appareils doivent construire le même combat")]
        public void Un_duel_en_ligne_gagne_donne_20_points()
        {
            var app = Ready();
            app.Duels.Match = Ticket(Rival(app, "zed"));
            var duel = Start(app, DuelMode.Online);
            Assert.That(duel.Mode, Is.EqualTo(DuelMode.Online));
            Assert.That(duel.IAmHost, Is.True, "« me » < « zed »");
            Assert.That(duel.MySide, Is.EqualTo(Side.Ally));
            Assert.That(duel.Setup.EnemyControl, Is.EqualTo(SideControl.External));
            Assert.That(duel.Link, Is.Not.Null);
            Assert.That(Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Win, default)).Value.TotalPoints, Is.EqualTo(20));
        }

        [Test, Description("Vérifie que l'invité (identifiant le plus grand) joue le camp Enemy et que les deux appareils construisent la même équipe hôte — parce que la simulation canonique place l'hôte en Ally des deux côtés")]
        public void L_invite_joue_le_camp_Enemy_avec_le_meme_combat()
        {
            var host = Ready();
            host.Duels.Match = Ticket(Rival(host, "zed"));
            var guest = Ready();
            guest.SignInDirect("zed"); guest.Arena.PlayerId = "zed";
            guest.Duels.Match = Ticket(Rival(guest, "me"));
            var a = Start(host, DuelMode.Online);
            var b = Start(guest, DuelMode.Online);
            Assert.That(a.IAmHost, Is.True);
            Assert.That(b.IAmHost, Is.False);
            Assert.That(b.MySide, Is.EqualTo(Side.Enemy));
            var sa = new CombatSimulation(N29.HeroesDawn.Tests.Domain.TestContent.BuildCore(), a.Setup);
            var sb = new CombatSimulation(N29.HeroesDawn.Tests.Domain.TestContent.BuildCore(), b.Setup);
            Assert.That(sb.StateHash(), Is.EqualTo(sa.StateHash()));
        }

        [Test, Description("Vérifie que sans adversaire au bout de 15 secondes le duel passe hors ligne — parce que la file d'attente ne doit pas bloquer le joueur")]
        public void Sans_adversaire_apres_15_secondes_le_duel_passe_hors_ligne()
        {
            var app = Ready();
            app.Arena.AddDefense(Rival(app, "rival"));
            app.Duels.Match = null;
            var duel = Start(app, DuelMode.Online);
            Assert.That(duel.Mode, Is.EqualTo(DuelMode.Offline));
            Assert.That(app.Duels.LastJoinTimeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
            Assert.That(app.Duels.Left, Is.True);
        }

        [Test, Description("Vérifie que quitter un duel en ligne retire 10 points sans descendre sous 0 (25 → 15, 4 → 0) — parce que la pénalité est bornée par le score")]
        public void Quitter_en_ligne_fait_perdre_10_points_sans_descendre_sous_0()
        {
            var app = Ready();
            SeedPoints(app, 25);
            app.Duels.Match = Ticket(Rival(app, "zed"));
            var report = Run(app.Tournament.FinishDuelAsync(Start(app, DuelMode.Online), DuelOutcome.Quit, default)).Value;
            Assert.That(report.TotalPoints, Is.EqualTo(15));
            Assert.That(app.Duels.Sent.Last().Type, Is.EqualTo(DuelMessageType.Leave));
            SeedPoints(app, 4);
            app.Duels.Match = Ticket(Rival(app, "zed"));
            Assert.That(Run(app.Tournament.FinishDuelAsync(Start(app, DuelMode.Online), DuelOutcome.Quit, default)).Value.TotalPoints, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que quitter un duel hors ligne ne change rien, ne compte pas et n'écrit rien sur le serveur — parce que le duel hors ligne n'engage personne")]
        public void Quitter_hors_ligne_ne_compte_pas()
        {
            var app = Ready();
            SeedPoints(app, 30);
            var report = Run(app.Tournament.FinishDuelAsync(Start(app, DuelMode.Offline), DuelOutcome.Quit, default)).Value;
            Assert.That(report.TotalPoints, Is.EqualTo(30));
            Assert.That(report.Counted, Is.False);
            Assert.That(app.Arena.Matches, Is.Empty);
            Assert.That(app.Tournament.Current().DuelsLeft, Is.EqualTo(ArenaRules.DailyDuelCap));
        }

        [Test, Description("Vérifie qu'une désynchronisation (empreintes différentes au tick 20) est détectée et que le duel annulé ne rapporte ni ne retire rien — parce que personne n'est fautif")]
        public void Une_desynchronisation_annule_le_duel_sans_points()
        {
            var app = Ready();
            SeedPoints(app, 12);
            app.Duels.Match = Ticket(Rival(app, "zed"));
            var duel = Start(app, DuelMode.Online);
            app.Duels.Incoming.Enqueue(DuelMessage.OfHash(ArenaRules.HashEveryTicks, 222));
            Run(duel.Link!.SendHashAsync(ArenaRules.HashEveryTicks, 111, default));
            var ev = Run(duel.Link.NextAsync(default));
            Assert.That(ev.Kind, Is.EqualTo(DuelLinkKind.Desync));
            Assert.That(duel.Link.Desynced, Is.True);
            var report = Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Cancelled, default)).Value;
            Assert.That(report.TotalPoints, Is.EqualTo(12));
            Assert.That(report.Counted, Is.False);
            Assert.That(app.Arena.Matches, Is.Empty);
        }

        [Test, Description("Vérifie que des empreintes identiques ne déclenchent aucune alerte et que les commandes de l'adversaire sont relayées — parce que seule la commande est échangée")]
        public void Des_empreintes_identiques_ne_declenchent_rien()
        {
            var app = Ready();
            app.Duels.Match = Ticket(Rival(app, "zed"));
            var duel = Start(app, DuelMode.Online);
            app.Duels.Incoming.Enqueue(DuelMessage.OfHash(20, 111));
            app.Duels.Incoming.Enqueue(DuelMessage.OfCommand(25, CombatCommand.Pass(4)));
            Run(duel.Link!.SendHashAsync(20, 111, default));
            var ev = Run(duel.Link.NextAsync(default));
            Assert.That(ev.Kind, Is.EqualTo(DuelLinkKind.Command));
            Assert.That(ev.Tick, Is.EqualTo(25));
            Assert.That(ev.Command!.Type, Is.EqualTo(CommandType.Pass));
            Assert.That(duel.Link.Desynced, Is.False);
        }

        [Test, Description("Vérifie qu'un adversaire muet 45 secondes est déclaré parti et que le joueur qui reste gagne sans pénalité — parce qu'on ne punit pas celui qui reste")]
        public void Un_adversaire_muet_45_secondes_est_declare_parti()
        {
            var app = Ready();
            app.Duels.Match = Ticket(Rival(app, "zed"));
            var duel = Start(app, DuelMode.Online);
            var ev = Run(duel.Link!.NextAsync(default));
            Assert.That(ev.Kind, Is.EqualTo(DuelLinkKind.PeerSilent));
            Assert.That(app.Duels.LastReceiveTimeout, Is.EqualTo(TimeSpan.FromSeconds(45)));
            Assert.That(Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Win, default)).Value.TotalPoints, Is.EqualTo(20));
        }

        [Test, Description("Vérifie que le départ annoncé de l'adversaire est signalé — parce que le joueur qui reste doit pouvoir conclure")]
        public void Le_depart_de_l_adversaire_est_signale()
        {
            var app = Ready();
            app.Duels.Match = Ticket(Rival(app, "zed"));
            var duel = Start(app, DuelMode.Online);
            app.Duels.Incoming.Enqueue(DuelMessage.Leave());
            Assert.That(Run(duel.Link!.NextAsync(default)).Kind, Is.EqualTo(DuelLinkKind.PeerLeft));
        }

        [Test, Description("Vérifie que le 11e duel du jour est refusé avant d'engager le combat et que minuit remet le compteur à zéro — parce que le plafond est de 10 duels par jour")]
        public void Le_11e_duel_du_jour_est_refuse_et_minuit_remet_le_compteur_a_zero()
        {
            var app = Ready();
            for (int i = 0; i < ArenaRules.DailyDuelCap; i++)
                Assert.That(Run(app.Tournament.FinishDuelAsync(Start(app, DuelMode.Offline), DuelOutcome.Loss, default)).IsSuccess, Is.True);
            int rows = app.Arena.Matches.Count;
            var refused = Run(app.Tournament.StartDuelAsync(DuelMode.Offline, default));
            Assert.That(refused.IsFailure, Is.True);
            Assert.That(refused.Error, Is.SameAs(TournamentErrors.DailyCap));
            Assert.That(app.Arena.Matches.Count, Is.EqualTo(rows), "aucun duel engagé");
            Move(app, new DateTime(2026, 12, 11, 0, 5, 0));
            Assert.That(app.Tournament.Current().DuelsLeft, Is.EqualTo(ArenaRules.DailyDuelCap));
            Assert.That(Run(app.Tournament.StartDuelAsync(DuelMode.Offline, default)).IsSuccess, Is.True);
        }

        [Test, Description("Vérifie que hors tournoi, sans compte ou sans équipe le duel est refusé avec trois messages distincts — parce que le joueur doit savoir quoi faire")]
        public void Hors_tournoi_ou_hors_connexion_le_duel_est_refuse()
        {
            var off = Run(Ready(new DateTime(2026, 9, 30, 12, 0, 0)).Tournament.StartDuelAsync(DuelMode.Offline, default));
            var anon = Run(Ready(Christmas, signed: false).Tournament.StartDuelAsync(DuelMode.Offline, default));
            Assert.That(off.Error, Is.SameAs(TournamentErrors.NotActive));
            Assert.That(anon.Error, Is.SameAs(TournamentErrors.NeedsAccount));
            Assert.That(off.Error!.LocKey, Is.Not.EqualTo(anon.Error!.LocKey));
            var empty = Ready();
            foreach (var m in empty.Session.Roster) m.InSquad = false;
            Assert.That(Run(empty.Tournament.StartDuelAsync(DuelMode.Offline, default)).Error, Is.SameAs(MetaErrors.NoMembers));
        }

        [Test, Description("Vérifie que sans capture disponible une équipe de repli est proposée, sans exception ni duel bloqué — parce qu'en début de saison le joueur peut être seul")]
        public void Sans_capture_disponible_une_equipe_de_repli_est_proposee()
        {
            var app = Ready();
            var duel = Start(app, DuelMode.Offline);
            Assert.That(duel.OpponentGenerated, Is.True);
            Assert.That(duel.Setup.OpponentTeam.Count, Is.GreaterThan(0));
            var sim = new CombatSimulation(app.Session.Content, duel.Setup);
            Assert.That(sim.Units.Count(u => u.Side == Side.Enemy), Is.EqualTo(duel.Setup.OpponentTeam.Count));
        }

        [Test, Description("Vérifie que le joueur ne s'affronte jamais lui-même (sa propre capture est ignorée) et que la capture la plus récente d'un adversaire est celle utilisée — parce qu'un joueur sur deux appareils envoie plusieurs captures")]
        public void Le_meme_joueur_ne_s_affronte_jamais()
        {
            var app = Ready();
            Run(app.Tournament.UploadDefenseIfNeededAsync(default));
            Assert.That(app.Arena.Uploads.Count, Is.EqualTo(1));
            var old = Rival(app, "rival");
            app.Arena.AddDefense(old);
            var recent = Rival(app, "rival", levelBonus: 3);
            app.Arena.AddDefense(recent);
            var duel = Start(app, DuelMode.Offline);
            Assert.That(duel.Opponent.PlayerId, Is.EqualTo("rival"));
            Assert.That(duel.Opponent.Team[0].level, Is.EqualTo(recent.Team[0].level));
            Assert.That(duel.Opponent.PlayerId, Is.Not.EqualTo("me"));
        }

        [Test, Description("Vérifie qu'une connexion perdue pendant la file d'attente renvoie une erreur sans pénalité ni duel compté — parce que le joueur retourne simplement à l'écran du tournoi")]
        public void Une_deconnexion_en_file_d_attente_est_sans_penalite()
        {
            var app = Ready();
            SeedPoints(app, 40);
            app.Duels.DropOnJoin = true;
            var r = Run(app.Tournament.StartDuelAsync(DuelMode.Online, default));
            Assert.That(r.Error, Is.SameAs(TournamentErrors.Network));
            Assert.That(app.Tournament.Current().Points, Is.EqualTo(40));
            Assert.That(app.Tournament.Current().DuelsLeft, Is.EqualTo(ArenaRules.DailyDuelCap));
        }

        [Test, Description("Vérifie qu'un palier réclamé deux fois est refusé, qu'il donne ses Crédits, Composants et cosmétiques une seule fois, et qu'un palier non atteint est refusé — parce qu'il ne faut jamais de double récompense")]
        public void Reclamer_un_palier_donne_les_recompenses_une_fois()
        {
            var app = Ready();
            SeedPoints(app, 60);
            int credits = app.Session.Wallet.Credits;
            int comps = N29.HeroesDawn.Application.Features.Meta.CraftingOps.Components(app.Session, ComponentKind.Weapon, ComponentGrade.Common);
            Assert.That(Run(app.Tournament.ClaimTierAsync(2, default)).Error, Is.SameAs(TournamentErrors.NotEarned));
            Assert.That(app.Tournament.Current().ClaimableTiers, Is.EqualTo(new[] { 1 }));
            Assert.That(Run(app.Tournament.ClaimTierAsync(1, default)).IsSuccess, Is.True);
            Assert.That(app.Session.Wallet.Credits, Is.EqualTo(credits + 300));
            Assert.That(N29.HeroesDawn.Application.Features.Meta.CraftingOps.Components(app.Session, ComponentKind.Weapon, ComponentGrade.Common), Is.EqualTo(comps + 4));
            Assert.That(app.Session.Meta.arena.cosmetics, Is.EqualTo(new[] { "cosmetic:christmas_bronze" }));
            Assert.That(Run(app.Tournament.ClaimTierAsync(1, default)).Error, Is.SameAs(MetaErrors.AlreadyClaimed));
            Assert.That(app.Session.Wallet.Credits, Is.EqualTo(credits + 300), "pas de double récompense");
            Assert.That(app.Tournament.Current().ClaimableTiers, Is.Empty);
        }

        [Test, Description("Vérifie qu'une récompense de l'édition précédente reste réclamable 7 jours après la fin puis est refusée — parce que le délai de réclamation est de 7 jours")]
        public void Reclamer_apres_7_jours_est_refuse()
        {
            var app = Ready();
            SeedPoints(app, 60);
            Move(app, new DateTime(2026, 12, 31, 12, 0, 0));   // Nouvel An est actif : Noël devient l'édition précédente
            var view = app.Tournament.Current();
            Assert.That(view.EditionKey, Is.EqualTo("new_year:2026"));
            Assert.That(view.PreviousEditionKey, Is.EqualTo(Key));
            Assert.That(Run(app.Tournament.ClaimTierAsync(1, default, Key)).IsSuccess, Is.True, "5 jours après la fin de Noël");
            SeedPointsPrevious(app, 160);
            Move(app, new DateTime(2027, 1, 3, 12, 0, 0));    // 8 jours après le 26/12
            Assert.That(Run(app.Tournament.ClaimTierAsync(2, default, Key)).Error, Is.SameAs(TournamentErrors.ClaimExpired));
        }

        static void SeedPointsPrevious(TestApp app, int points) => app.Session.Meta.arena.previousPoints = points;

        [Test, Description("Vérifie que le lot du top 3 n'est réclamable qu'après la fin du tournoi et au rang 3 ou mieux, et qu'il apprend la recette de tournoi — parce que le classement n'est connu qu'à la fin")]
        public void Le_lot_du_top_3_exige_la_fin_et_le_rang()
        {
            var app = Ready();
            SeedPoints(app, 90);
            Assert.That(Run(app.Tournament.ClaimTierAsync(ArenaRewardTable.TierTop3, default)).Error, Is.SameAs(TournamentErrors.NotFinished));
            for (int i = 0; i < 4; i++) app.Arena.SetPoints(Key, "r" + i, 100 + i);
            Move(app, new DateTime(2026, 12, 27, 12, 0, 0));
            Assert.That(Run(app.Tournament.ClaimTierAsync(ArenaRewardTable.TierTop3, default)).Error, Is.SameAs(TournamentErrors.NotEarned), "rang 5");
            app.Arena.SetPoints(Key, "me", 500);
            Assert.That(Run(app.Tournament.ClaimTierAsync(ArenaRewardTable.TierTop3, default)).IsSuccess, Is.True);
            Assert.That(N29.HeroesDawn.Application.Features.Meta.CraftingOps.Knows(app.Session, "synapse_coil"), Is.True);
            Assert.That(app.Session.Meta.arena.cosmetics, Does.Contain("cosmetic:christmas_frame"));
            Assert.That(Run(app.Tournament.ClaimTierAsync(ArenaRewardTable.TierTop3, default)).Error, Is.SameAs(MetaErrors.AlreadyClaimed));
        }

        [Test, Description("Vérifie que la capture de défense est envoyée à la première connexion puis seulement quand l'équipe change — parce que le serveur n'a pas besoin de la même capture à chaque écran")]
        public void La_defense_est_envoyee_a_la_connexion_et_au_changement_d_equipe()
        {
            var app = Ready();
            Run(app.Tournament.UploadDefenseIfNeededAsync(default));
            Assert.That(app.Arena.Uploads.Count, Is.EqualTo(1));
            Assert.That(app.Arena.Uploads[0].Team.Count, Is.EqualTo(app.Session.Squad.Count));
            Run(app.Tournament.UploadDefenseIfNeededAsync(default));
            Assert.That(app.Arena.Uploads.Count, Is.EqualTo(1), "équipe inchangée");
            app.Session.Squad[0].Level += 1;
            Run(app.Tournament.UploadDefenseIfNeededAsync(default));
            Assert.That(app.Arena.Uploads.Count, Is.EqualTo(2), "équipe changée");
        }

        [Test, Description("Vérifie que la capture envoyée sans compte connecté n'a pas lieu — parce que le tournoi demande un compte")]
        public void Sans_compte_aucune_capture_n_est_envoyee()
        {
            var app = Ready(signed: false);
            Run(app.Tournament.UploadDefenseIfNeededAsync(default));
            Assert.That(app.Arena.Uploads, Is.Empty);
        }

        [Test, Description("Vérifie qu'une panne du serveur au moment de verser le résultat laisse le duel rejouable et les points intacts — parce qu'un résultat ne doit pas se perdre ni s'appliquer à moitié")]
        public void Une_panne_serveur_laisse_le_duel_non_reporte()
        {
            var app = Ready();
            var duel = Start(app, DuelMode.Offline);
            app.Arena.Offline = true;
            Assert.That(Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Win, default)).IsFailure, Is.True);
            Assert.That(duel.Reported, Is.False);
            Assert.That(app.Tournament.Current().Points, Is.EqualTo(0));
            app.Arena.Offline = false;
            Assert.That(Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Win, default)).Value.TotalPoints, Is.EqualTo(10));
            Assert.That(Run(app.Tournament.FinishDuelAsync(duel, DuelOutcome.Win, default)).IsFailure, Is.True, "déjà reporté");
        }

        [Test, Description("Vérifie que la vue du tournoi indique le prochain tournoi entre deux tournois — parce que la page affiche le suivant quand rien n'est en cours")]
        public void Entre_deux_tournois_la_vue_donne_le_prochain()
        {
            var v = Ready(new DateTime(2026, 12, 27, 12, 0, 0)).Tournament.Current();
            Assert.That(v.IsActive, Is.False);
            Assert.That(v.Def.Id, Is.EqualTo("new_year"));
            Assert.That(v.Start, Is.EqualTo(new DateTime(2026, 12, 29)));
        }
    }
}
