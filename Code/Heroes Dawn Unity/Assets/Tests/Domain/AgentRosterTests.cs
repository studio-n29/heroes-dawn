using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Shop;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Les 30 agents de The Dawn et leurs déblocages : boutique, arc narratif, événement temporaire, prouesse (plan 2026-10-10, sous-projet N).</summary>
    public sealed class AgentRosterTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();
        static readonly ZoneId Z11 = PrototypeContent.Zone11;
        static readonly ZoneId Z12 = PrototypeZones.Zone12;
        static readonly ZoneId Z13 = PrototypeZones.Zone13;
        static readonly DateTime Saturday = new DateTime(2026, 10, 10, 12, 0, 0);
        static readonly DateTime Monday = new DateTime(2026, 10, 12, 12, 0, 0);

        static GameSession NewSession() => new GameSession(C);

        static IEnumerable<MemberDef> OfKind(UnlockKind k) => C.Members.Where(m => m.Unlock != null && m.Unlock.Kind == k);

        /// <summary>Plateau 1.1 où l'on a battu l'Élite : le journal contient un combat gagné.</summary>
        static ZoneRun RunWithWonFight(bool lostFirst = false)
        {
            var zone = C.Zone(Z11);
            var run = ZoneRun.ForTests(C, zone, 5UL, 1, true, "n22");
            run.MoveTo(new CaseId("n23"));
            if (lostFirst)
            {
                run.ResolveCombat(false);
                run.SetPmForTests(1);
                run.MoveTo(new CaseId("n23"));
            }
            run.ResolveCombat(true);
            return run;
        }

        [Test, Description("Vérifie que la première ville compte 30 agents aux identifiants uniques, avec trois compétences chacun, au moins quatre agents par famille et tous les rôles — parce que c'est l'objectif de la roster")]
        public void Roster_HasThirtyDistinctAgents_WithAFairSpread()
        {
            Assert.That(C.Members.Count, Is.EqualTo(32), "30 agents numérotés + Agent D + Agent M");
            Assert.That(C.Members.Select(m => m.Id.Value).Distinct().Count(), Is.EqualTo(32));
            Assert.That(C.Members.All(m => m.Skills.Count == 3), Is.True);
            foreach (Family f in Enum.GetValues(typeof(Family))) Assert.That(C.Members.Count(m => m.Family == f), Is.GreaterThanOrEqualTo(4), f.ToString());
            foreach (Role r in Enum.GetValues(typeof(Role))) Assert.That(C.Members.Any(m => m.Role == r), Is.True, r.ToString());
            Assert.That(C.Members.Count(m => m.Rarity == Rarity.Legend && m.Unlock?.Kind != UnlockKind.Hero), Is.EqualTo(4));
            Assert.That(C.Members.Count(m => m.Rarity == Rarity.Elite), Is.EqualTo(8));
            Assert.That(PrototypeAgents.StarterTeam.All(id => C.StarterTeam.Contains(id)), Is.True, "équipe de départ portée par le catalogue");
        }

        [Test, Description("Vérifie la répartition des sources : 4 agents d'arc, 4 d'événement, 4 de prouesse et le reste en boutique — parce que les quatre voies d'obtention doivent toutes exister")]
        public void Roster_SplitsAcrossTheFourSources()
        {
            Assert.That(OfKind(UnlockKind.Arc).Count(), Is.EqualTo(4));
            Assert.That(OfKind(UnlockKind.Event).Count(), Is.EqualTo(4));
            Assert.That(OfKind(UnlockKind.Feat).Count(), Is.EqualTo(4));
            Assert.That(C.Members.Count(m => m.InShop), Is.EqualTo(18));
            Assert.That(OfKind(UnlockKind.Event).All(m => m.Unlock!.IsEventDay(Saturday) && !m.Unlock.IsEventDay(Monday)), Is.True, "événements du week-end");
        }

        [Test, Description("Vérifie que chaque règle de déblocage vise une zone existante du jeu et que chaque compagnon a une prouesse — parce qu'une règle qui vise une zone inconnue ne se déclencherait jamais")]
        public void UnlockRules_TargetRealZones()
        {
            foreach (var m in C.Members.Where(m => m.Unlock != null && m.Unlock.Kind != UnlockKind.Shop && m.Unlock.Kind != UnlockKind.Hero))
            {
                Assert.That(m.Unlock!.Zone.HasValue, Is.True, m.Id.Value);
                Assert.That(C.Zones.Any(z => z.Id == m.Unlock.Zone!.Value), Is.True, m.Id.Value);
                if (m.Unlock.Kind == UnlockKind.Feat) Assert.That(m.Unlock.Feat, Is.Not.EqualTo(FeatKind.None), m.Id.Value);
            }
        }

        [Test, Description("Vérifie que la boutique de recrutement ne propose jamais un agent d'arc, d'événement ou de prouesse — parce que ces agents ne s'obtiennent que par leur voie")]
        public void Shop_NeverOffersNonShopAgents()
        {
            var eligible = ShopDraw.Eligible(C.Members, _ => false, _ => true);
            Assert.That(eligible.All(m => m.InShop), Is.True);
            Assert.That(eligible.Count, Is.EqualTo(18));
            for (ulong seed = 1; seed < 40; seed++)
            {
                var picks = ShopDraw.Draw(new Pcg32(seed), eligible, new ShopWeights(70, 25, 5));
                Assert.That(picks.All(id => C.Member(id).InShop), Is.True, "graine " + seed);
            }
        }

        [Test, Description("Vérifie qu'un agent d'arc rejoint l'équipe à la validation de sa zone, pas avant, et une seule fois — parce que la fin d'un arc récompense le joueur")]
        public void Arc_AgentJoinsWhenTheZoneIsValidated()
        {
            var s = NewSession();
            Assert.That(s.Owns(new MemberId("noah_kaplan")), Is.False);
            Assert.That(AgentUnlocks.OnZoneValidated(s, null, Z12, Saturday).Any(m => m.Id.Value == "noah_kaplan"), Is.False, "zone 1.2 : pas encore 1.1");
            s.CompletedZones.Add(Z11.Value);
            var joined = AgentUnlocks.OnZoneValidated(s, null, Z11, Monday);
            Assert.That(joined.Select(m => m.Id.Value), Is.EqualTo(new[] { "noah_kaplan" }));
            Assert.That(s.Owns(new MemberId("noah_kaplan")), Is.True);
            Assert.That(s.Find(new MemberId("noah_kaplan"))!.InSquad, Is.False, "l'agent arrive hors escouade");
            Assert.That(AgentUnlocks.OnZoneValidated(s, null, Z11, Monday), Is.Empty, "une seule fois");
        }

        [Test, Description("Vérifie qu'une sauvegarde où un arc est déjà accompli rattrape l'agent au chargement — parce que les anciens profils ne doivent rien perdre")]
        public void Arc_IsCaughtUpWhenLoadingAnOldProfile()
        {
            var profile = new ProfileData();
            profile.team.Add(new MemberSave { id = "darius_brooks", level = 5, inSquad = true });
            profile.completedZones.Add(Z11.Value);
            var s = NewSession();
            s.ApplyProfile(profile);
            Assert.That(s.Owns(new MemberId("noah_kaplan")), Is.True);
            Assert.That(s.Owns(new MemberId("victor_sorel")), Is.False, "zone 1.2 pas validée");
        }

        [Test, Description("Vérifie qu'un agent d'événement rejoint l'équipe seulement si la zone est validée un jour d'événement (week-end) — parce que l'événement est temporaire")]
        public void Event_AgentJoinsOnlyOnEventDays()
        {
            var s = NewSession();
            Assert.That(AgentUnlocks.OnZoneValidated(s, null, Z11, Monday).Any(m => m.Id.Value == "louis_dubois"), Is.False, "lundi : pas d'événement");
            var joined = AgentUnlocks.OnZoneValidated(s, null, Z11, Saturday);
            Assert.That(joined.Any(m => m.Id.Value == "louis_dubois"), Is.True, "samedi : événement de la zone 1.1");
            Assert.That(s.Owns(new MemberId("marco_bellini")), Is.False, "autre zone : pas concerné");
        }

        [Test, Description("Vérifie que la prouesse « un seul agent » débloque son compagnon seulement si un unique agent a combattu pendant le plateau — parce que c'est une prouesse, pas un cadeau")]
        public void Feat_SoloClear_NeedsASingleFighter()
        {
            var s = NewSession();
            var run = RunWithWonFight();
            s.Run = run;
            s.RunEngaged.Add("darius_brooks");
            s.RunEngaged.Add("mei_lin_chen");
            Assert.That(AgentUnlocks.OnZoneValidated(s, run, Z11, Monday).Any(m => m.Id.Value == "ada_okonkwo"), Is.False, "deux agents engagés");
            s.RunEngaged.Remove("mei_lin_chen");
            Assert.That(AgentUnlocks.OnZoneValidated(s, run, Z11, Monday).Any(m => m.Id.Value == "ada_okonkwo"), Is.True);
        }

        [Test, Description("Vérifie que la prouesse « sans défaite » est refusée après un recul et accordée sinon, et qu'aucun combat ne donne rien — parce qu'il faut avoir combattu et gagné")]
        public void Feat_NoDefeat_NeedsFightsAndNoLoss()
        {
            var s = NewSession();
            // Plateau 1.2 : on réutilise le journal d'un plateau 1.1 pour tester la règle, la zone validée étant 1.2.
            var clean = RunWithWonFight();
            s.Run = clean;
            Assert.That(AgentUnlocks.OnZoneValidated(s, clean, Z12, Monday).Any(m => m.Id.Value == "eero_nieminen"), Is.True, "aucune défaite");

            var s2 = NewSession();
            var lost = RunWithWonFight(lostFirst: true);
            s2.Run = lost;
            Assert.That(AgentUnlocks.OnZoneValidated(s2, lost, Z12, Monday).Any(m => m.Id.Value == "eero_nieminen"), Is.False, "une défaite en cours de route");

            var s3 = NewSession();
            var noFight = ZoneRun.ForTests(C, C.Zone(Z12), 5UL, 1, true);
            s3.Run = noFight;
            Assert.That(AgentUnlocks.OnZoneValidated(s3, noFight, Z12, Monday).Any(m => m.Id.Value == "eero_nieminen"), Is.False, "aucun combat");
        }

        [Test, Description("Vérifie que changer de plateau remet à zéro les agents engagés — parce que la prouesse se juge plateau par plateau")]
        public void RunEngaged_ResetsWhenTheRunChanges()
        {
            var s = NewSession();
            s.Run = ZoneRun.ForTests(C, C.Zone(Z11), 1UL, 1, true);
            s.RunEngaged.Add("darius_brooks");
            s.Run = ZoneRun.ForTests(C, C.Zone(Z11), 2UL, 1, true);
            Assert.That(s.RunEngaged, Is.Empty);
            s.RunEngaged.Add("mei_lin_chen");
            s.Run = s.Run;
            Assert.That(s.RunEngaged.Count, Is.EqualTo(1), "même plateau : rien ne change");
        }
    }
}
