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
