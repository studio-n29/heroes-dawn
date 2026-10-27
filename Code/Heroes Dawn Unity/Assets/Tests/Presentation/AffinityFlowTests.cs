using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Pop-up d'affinités : un agent face à tous les agents possédés, un seul bonus par duo.</summary>
    public sealed class AffinityFlowTests
    {
        TestApp _app = null!;
        AffinityFlow _flow = null!;
        static MemberId Verre => new MemberId("verre");
        static MemberId Ancre => new MemberId("ancre");

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(Verre), 5));
            _flow = new AffinityFlow(_app.Session, new ModalService(new FakeModalHost()), new LocLocalization());
        }

        [Test, Description("Vérifie que le pop-up liste tous les autres agents possédés, jamais l'agent lui-même — parce que l'affinité se lit entre deux agents différents")]
        public void ListsEveryOtherAgent()
        {
            var data = _flow.Build(Ancre);
            Assert.That(data.Rows.Length, Is.EqualTo(_app.Session.Roster.Count - 1));
            var names = data.Rows.Select(r => r.Name).ToList();
            Assert.That(names, Does.Not.Contain(new LocLocalization().MemberName(_app.Session.Content.Member(Ancre))));
        }

        [Test, Description("Vérifie que les agents au lien le plus fort viennent d'abord et que le lien affiche ses victoires — parce que c'est l'information que la zone d'affinité doit donner")]
        public void StrongestLinkFirst()
        {
            TeamLinks.RecordVictory(_app.Session.Meta, new[] { "ancre", "verre" });
            TeamLinks.RecordVictory(_app.Session.Meta, new[] { "ancre", "verre" });
            var first = _flow.Build(Ancre).Rows[0];
            Assert.That(first.Name, Is.EqualTo(new LocLocalization().MemberName(_app.Session.Content.Member(Verre))));
            Assert.That(first.Link, Does.Contain("2"));
        }

        // ------------------------------------------------------------------ Bonus de paire (lot D)

        static void Wins(TestApp app, string a, string b, int count)
        {
            for (int i = 0; i < count; i++) TeamLinks.RecordVictory(app.Session.Meta, new[] { a, b });
        }

        AffinityRowData RowOf(MemberId other) => _flow.Build(Ancre).Rows.Single(r => r.Name == new LocLocalization().MemberName(_app.Session.Content.Member(other)));

        [Test, Description("Vérifie que la ligne d'une paire montre le thème de son couple de rôles (Tank + Contrôle : Étau), les paliers atteints avec ● (niveaux 2 et 3), le palier à venir sans ● et les victoires qui manquent — parce que le joueur doit voir ce que le lien débloque")]
        public void Row_ShowsThePairThemeAndTiers()
        {
            Wins(_app, "ancre", "verre", 8); // niveau de lien 3
            string pair = RowOf(Verre).Pair;
            Assert.That(pair, Does.Contain("Étau"));
            Assert.That(pair, Does.Contain("● Niv. 2 · DEF +3 %").And.Contain("● Niv. 3 · VIT +3 %"));
            Assert.That(pair, Does.Contain("Niv. 4 · Jauge de départ +10 %"));
            Assert.That(pair, Does.Not.Contain("● Niv. 4").And.Not.Contain("● Niv. 5"));
            Assert.That(pair, Does.Contain("encore 7 victoire"));
        }

        [Test, Description("Vérifie qu'un lien au niveau maximum montre ses quatre paliers cochés et « lien au maximum » — parce qu'il n'y a plus rien à attendre")]
        public void Row_AtMaxLevel_SaysSo()
        {
            Wins(_app, "ancre", "verre", 25);
            string pair = RowOf(Verre).Pair;
            Assert.That(pair.Split('●').Length - 1, Is.EqualTo(4));
            Assert.That(pair, Does.Contain("lien au maximum"));
        }

        [Test, Description("Vérifie qu'une paire sans victoire commune montre les quatre paliers à venir (aucun ●) et 3 victoires à remporter — parce que le premier palier est au niveau 2")]
        public void Row_WithNoLink_ListsAllTiersAsUpcoming()
        {
            string pair = RowOf(Verre).Pair;
            Assert.That(pair, Does.Not.Contain("●"));
            Assert.That(pair, Does.Contain("Niv. 2").And.Contain("Niv. 5"));
            Assert.That(pair, Does.Contain("encore 3 victoire"));
        }

        [Test, Description("Vérifie qu'un lien sauvegardé vers un agent absent du jeu n'apparaît pas et ne fait rien échouer — parce qu'une sauvegarde ancienne peut citer un agent retiré")]
        public void Row_ForAnAgentRemovedFromTheGame_DoesNotThrow()
        {
            _app.Session.Meta.links.Add(new N29.HeroesDawn.Application.Ports.LinkSave { a = "ancre", b = "agent_disparu", wins = 9 });
            var data = _flow.Build(Ancre);
            Assert.That(data.Rows.Length, Is.EqualTo(_app.Session.Roster.Count - 1));
            Assert.That(data.Rows.All(r => r.Pair.Length > 0), Is.True);
        }

        [Test, Description("Vérifie que chaque ligne n'affiche qu'un seul bonus (celui de la synergie du duo) ou « aucune synergie » — parce que le design demande un seul bonus par duo")]
        public void EachRowHasOneBonusText()
        {
            foreach (var row in _flow.Build(Ancre).Rows)
            {
                Assert.That(row.Bonus, Is.Not.Empty);
                Assert.That(row.Bonus.Contains('\n'), Is.False);
            }
        }
    }
}
