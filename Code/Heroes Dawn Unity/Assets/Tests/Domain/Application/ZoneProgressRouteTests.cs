#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Ouverture des zones selon l'acte courant du parcours (Agent D : New York, Agent M : Moscou).</summary>
    public sealed class ZoneProgressRouteTests
    {
        const string MosZone = "zone_test_mos";

        // Catalogue de test : le prototype + une zone de Moscou (copie du plateau 1.1 de Brooklyn), sans dépendre des vraies zones de Moscou.
        static readonly ContentCatalog C = Build();

        static ContentCatalog Build()
        {
            var c = PrototypeContent.Build();
            if (c.Zones.Any(z => z.Borough == "sergueiv")) return c;
            var src = c.Zones.First(z => z.Borough == "brooklyn");
            var mos = new ZoneDef(new ZoneId(MosZone), src.NameKey, src.Entry, src.Nodes, src.Links, src.Objectives, src.Encounters, "MT.1", 1, "sergueiv", 1);
            return c.WithZones(new List<ZoneDef> { mos });
        }

        static GameSession NewSession(string hero)
        {
            var s = new GameSession(C);
            s.ResetToNewProfile();
            s.Meta.onboarding.hero = hero;
            return s;
        }

        static ZoneDef First(ContentCatalog c, string borough) => c.Zones.First(z => z.Borough == borough);

        [Test, Description("Vérifie qu'un nouveau profil Agent D voit la première zone de Brooklyn ouverte et une zone de Moscou fermée — parce que Moscou est l'acte III d'Agent D")]
        public void AgentD_NewProfile_MoscowZoneIsLocked_BrooklynIsAvailable()
        {
            var s = NewSession("agent_d");
            Assert.That(ZoneProgress.StatusOf(s, First(C, "brooklyn")), Is.EqualTo(ZoneStatus.Available));
            Assert.That(ZoneProgress.StatusOf(s, First(C, "sergueiv")), Is.EqualTo(ZoneStatus.Locked));
        }

        [Test, Description("Vérifie qu'un nouveau profil Agent M voit la première zone de Moscou ouverte et celles de Brooklyn fermées — parce que Brooklyn est l'acte III d'Agent M")]
        public void AgentM_NewProfile_BrooklynZoneIsLocked_MoscowZoneIsAvailable()
        {
            var s = NewSession("agent_m");
            Assert.That(ZoneProgress.StatusOf(s, First(C, "brooklyn")), Is.EqualTo(ZoneStatus.Locked));
            Assert.That(ZoneProgress.StatusOf(s, First(C, "sergueiv")), Is.EqualTo(ZoneStatus.Available));
        }

        [Test, Description("Vérifie qu'une zone validée reste validée quel que soit l'acte courant — parce que la progression acquise ne doit jamais disparaître")]
        public void DoneZone_StaysDone_WhateverTheAct()
        {
            var s = NewSession("agent_m");
            var bk = First(C, "brooklyn");
            s.CompletedZones.Add(bk.Id.Value);
            Assert.That(ZoneProgress.StatusOf(s, bk), Is.EqualTo(ZoneStatus.Done));
            Assert.That(ZoneProgress.RawStatusOf(s, bk), Is.EqualTo(ZoneStatus.Done));
        }

        [Test, Description("Vérifie que la zone recommandée suit le parcours : Brooklyn pour Agent D, Moscou pour Agent M — parce que le bouton « continuer » ne doit pas proposer la ville de l'autre personnage")]
        public void Recommended_FollowsTheRoute()
        {
            Assert.That(ZoneProgress.Recommended(NewSession("agent_d")).Borough, Is.EqualTo("brooklyn"));
            Assert.That(ZoneProgress.Recommended(NewSession("agent_m")).Borough, Is.EqualTo("sergueiv"));
        }

        [Test, Description("Vérifie que RawStatusOf ne regarde que les prérequis (ancien comportement) — parce que la fin d'un acte se calcule sans dépendre de l'acte courant")]
        public void RawStatusOf_OnlyLooksAtPrerequisites()
        {
            var s = NewSession("agent_m");
            Assert.That(ZoneProgress.RawStatusOf(s, First(C, "brooklyn")), Is.EqualTo(ZoneStatus.Available));
        }
    }
}
