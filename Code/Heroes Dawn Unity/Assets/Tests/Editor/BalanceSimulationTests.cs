using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.EditorTools.Balance;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class BalanceSimulationTests
    {
        static readonly MemberId[] Team = { new("ancre"), new("verre"), new("givre"), new("echo") };

        static SimulationRequest Req(int runs, int maxTicks = 20000) => new SimulationRequest
        {
            Team = Team, TeamLevel = 5, FatiguePercent = 0, RunsPerEncounter = runs, BaseSeed = 42, MaxTicks = maxTicks,
        };

        [Test]
        public void Run_SameSeed_SameResults()
        {
            var c = PrototypeContent.Build();
            var a = BalanceSimulation.Run(c, PrototypeContent.Zone11, Req(20));
            var b = BalanceSimulation.Run(c, PrototypeContent.Zone11, Req(20));
            Assert.That(BalanceSimulation.ToCsv(a), Is.EqualTo(BalanceSimulation.ToCsv(b)));
        }

        [Test]
        public void Run_OneRowPerEncounter_AndCountsAddUp()
        {
            var c = PrototypeContent.Build();
            var s = BalanceSimulation.Run(c, PrototypeContent.Zone11, Req(10));
            Assert.That(s.Count, Is.EqualTo(c.Zone(PrototypeContent.Zone11).Encounters.Count));
            foreach (var e in s) Assert.That(e.Wins + e.Losses + e.Timeouts, Is.EqualTo(10));
        }

        [Test] // Review Focus 5 : combat interminable
        public void Run_TickCapReached_CountsTimeouts()
        {
            var s = BalanceSimulation.Run(PrototypeContent.Build(), PrototypeContent.Zone11, Req(3, maxTicks: 1));
            Assert.That(s.Sum(e => e.Timeouts), Is.EqualTo(s.Count * 3));
        }

        [Test]
        public void ToCsv_UsesSemicolonsAndInvariantCulture()
        {
            var csv = BalanceSimulation.ToCsv(new[]
            {
                new EncounterStats { EncounterIndex = 0, Rank = CombatRank.Normal, Runs = 4, Wins = 3, Losses = 1, AvgTicksOnWin = 250 },
            });
            StringAssert.StartsWith("encounter;rank;runs;wins;losses;timeouts;win_rate_pct;avg_seconds_on_win", csv);
            StringAssert.Contains("0;Normal;4;3;1;0;75;12.5", csv);
        }
    }
}
