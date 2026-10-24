using System.Linq;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Sauvegarde et rechargement de compositions d'équipe.</summary>
    public sealed class SquadPresetsTests
    {
        TestApp _app = null!;
        SquadPresets P => _app.Hub.Presets;
        GameSession S => _app.Session;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            foreach (var def in S.Content.Members.Where(d => !S.Owns(d.Id)).Take(2)) S.Roster.Add(new TeamMember(def, 5) { InSquad = false });
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";

        string[] Squad() => S.Squad.Select(m => m.Id.Value).OrderBy(x => x).ToArray();

        [Test, Description("Vérifie qu'un emplacement libre ne se charge pas — parce qu'il n'y a rien à remettre")]
        public void LoadingAnEmptySlot_Fails()
        {
            Assert.That(P.IsSaved(0), Is.False);
            Assert.That(Code(P.Load(0)), Is.EqualTo("hub.preset_empty"));
        }

        [Test, Description("Vérifie qu'on enregistre l'escouade actuelle puis qu'on la retrouve après l'avoir changée — parce que c'est le but des compositions")]
        public void SaveThenLoad_RestoresTheSquad()
        {
            var saved = Squad();
            Assert.That(P.Save(1).IsSuccess);
            var other = S.Roster.First(m => !m.InSquad);
            foreach (var m in S.Roster) m.InSquad = m == other;
            Assert.That(Squad(), Is.Not.EqualTo(saved));
            Assert.That(P.Load(1).IsSuccess);
            Assert.That(Squad(), Is.EqualTo(saved));
        }

        [Test, Description("Vérifie que les trois emplacements sont indépendants — parce que chacun garde sa propre composition")]
        public void SlotsAreIndependent()
        {
            P.Save(0);
            var other = S.Roster.First(m => !m.InSquad);
            foreach (var m in S.Roster) m.InSquad = m == other;
            P.Save(2);
            Assert.That(P.Members(0), Is.Not.EqualTo(P.Members(2)));
            Assert.That(P.IsSaved(1), Is.False);
        }

        [Test, Description("Vérifie qu'on ne recharge pas une composition pendant une partie — parce que l'escouade est figée en mission")]
        public void Load_RefusedDuringARun()
        {
            P.Save(0);
            _app.Session.Run = new N29.HeroesDawn.Domain.Board.ZoneRun(S.Content, S.Content.Zones[0], 1UL);
            Assert.That(Code(P.Load(0)), Is.EqualTo("hub.in_mission"));
        }

        [Test, Description("Vérifie qu'un agent disparu de l'effectif est ignoré au chargement, et qu'une composition entièrement indisponible est refusée — parce qu'on ne doit jamais se retrouver sans escouade")]
        public void Load_IgnoresMissingAgents()
        {
            P.Save(0);
            var ids = P.Members(0).ToList();
            S.Meta.squadPresets[0].members = ids.Select(i => i).Concat(new[] { "inconnu" }).ToList();
            Assert.That(P.Load(0).IsSuccess);
            Assert.That(Squad().Length, Is.EqualTo(ids.Count));
            S.Meta.squadPresets[0].members = new System.Collections.Generic.List<string> { "inconnu" };
            Assert.That(Code(P.Load(0)), Is.EqualTo("hub.preset_unavailable"));
        }
    }
}
