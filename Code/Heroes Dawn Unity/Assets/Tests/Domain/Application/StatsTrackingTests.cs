using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Statistiques du profil : cases traversées et favoris (agents et cartes les plus utilisés).</summary>
    public sealed class StatsTrackingTests
    {
        [Test, Description("Vérifie que les cases traversées s'additionnent sur chaque déplacement — parce que la statistique compte les cases, pas les déplacements")]
        public void RecordMoves_AddsTheCasesCrossed()
        {
            var meta = new MetaSave();
            var path = new List<CaseId> { new CaseId("a"), new CaseId("b"), new CaseId("c") };
            Stats.RecordMoves(meta, new List<BoardEvent> { new PawnMovedEvent(path), new PawnMovedEvent(new List<CaseId> { new CaseId("d") }) });
            Assert.That(Stats.Get(meta, Stats.Tiles), Is.EqualTo(4));
        }

        [Test, Description("Vérifie que les favoris classent les plus utilisés en premier, à égalité par identifiant, et s'arrêtent au nombre demandé — parce que la page Profil n'affiche que trois favoris")]
        public void Favorites_RanksByUsage_AndCapsTheCount()
        {
            var meta = new MetaSave();
            Stats.Add(meta, Stats.AgentUsePrefix + "lucas_mendes", 2);
            Stats.Add(meta, Stats.AgentUsePrefix + "giulia_rossi", 5);
            Stats.Add(meta, Stats.AgentUsePrefix + "ilona_varga", 2);
            Stats.Add(meta, Stats.AgentUsePrefix + "rin_takeda", 1);
            Stats.Add(meta, Stats.SkillUsePrefix + "matter_slam", 9);
            var agents = Stats.Favorites(meta, Stats.AgentUsePrefix, Stats.FavoriteCount);
            Assert.That(agents, Is.EqualTo(new[] { "giulia_rossi", "ilona_varga", "lucas_mendes" }));
            Assert.That(Stats.Favorites(meta, Stats.SkillUsePrefix, Stats.FavoriteCount), Is.EqualTo(new[] { "matter_slam" }), "les préfixes restent séparés");
        }

        [Test, Description("Vérifie qu'un agent jamais utilisé n'entre pas dans les favoris — parce qu'un zéro ne dit rien de la préférence du joueur")]
        public void Favorites_IgnoreZeroUsage()
        {
            var meta = new MetaSave();
            Stats.Add(meta, Stats.AgentUsePrefix + "rin_takeda", 0);
            Assert.That(Stats.Favorites(meta, Stats.AgentUsePrefix, Stats.FavoriteCount), Is.Empty);
        }
    }
}
