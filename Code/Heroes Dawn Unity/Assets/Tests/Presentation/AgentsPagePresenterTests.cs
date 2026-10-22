using System;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Registre des 30 agents et textes de la roster (plan 2026-10-10, sous-projet N).</summary>
    public sealed class AgentsPagePresenterTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        GameSession _session = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _session = new GameSession(PrototypeContent.Build()); // les 30 agents
            _view = new FakeListView();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
        }

        AgentsPagePresenter Start()
        {
            var p = new AgentsPagePresenter(_view, _session, _nav, _loc, _toasts, _app.Subscriber<N29.HeroesDawn.Application.Features.Meta.MetaChanged>(),
                                            _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que le registre liste les 30 agents, ceux de l'équipe d'abord, avec le décompte à débloquer — parce que le joueur doit voir l'ensemble de la collection")]
        public void Registry_ListsAllThirty_OwnedFirst()
        {
            Start();
            Assert.That(_view.Rows.Length, Is.EqualTo(31), "30 agents + le personnage joueur choisi (Agent D par défaut)");
            int owned = _session.Roster.Count;
            Assert.That(_view.Rows.Take(owned).All(r => !r.Dim), Is.True, "les agents de l'équipe d'abord");
            Assert.That(_view.Rows.Skip(owned).All(r => r.Dim), Is.True, "les autres sont grisés avec leur condition");
            Assert.That(_view.Header.Line, Does.Contain("sur 31").And.Contain((31 - owned) + " à débloquer"));
        }

        [Test, Description("Vérifie que chaque voie d'obtention est expliquée par une phrase propre à l'agent (zone, week-end, prouesse) — parce qu'on ne débloque pas ce qu'on ne comprend pas")]
        public void Registry_ExplainsEachWayToUnlock()
        {
            var p = Start();
            string how(string id) => p.How(_session.Content.Member(new MemberId(id)));
            Assert.That(how("noah_kaplan"), Does.Contain("Arc narratif").And.Contain("1.1"));
            Assert.That(how("victor_sorel"), Does.Contain("1.2"));
            Assert.That(how("ada_okonkwo"), Does.Contain("seul agent").And.Contain("1.1"));
            Assert.That(how("eero_nieminen"), Does.Contain("sans aucune défaite").And.Contain("1.2"));
            Assert.That(how("louis_dubois"), Does.Contain("week-end").And.Contain("1.1"));
            Assert.That(how("amara_hughes"), Does.Contain("après la zone"));
            Assert.That(how("ilona_varga"), Does.Contain("Boutique"));
        }

        [Test, Description("Vérifie que l'événement du jour est mis en avant en tête des agents à débloquer le samedi, pas le lundi — parce que l'événement est temporaire")]
        public void Registry_HighlightsTodaysEvent()
        {
            _app.Time.LocalNow = new DateTime(2026, 10, 10, 12, 0, 0); // samedi
            Start();
            var highlighted = _view.Rows.Where(r => r.Highlight).ToList();
            Assert.That(highlighted.Count, Is.EqualTo(4), "les quatre événements du week-end");
            Assert.That(highlighted.All(r => r.Badge == "ÉVÉNEMENT"), Is.True);
            int owned = _session.Roster.Count;
            Assert.That(_view.Rows[owned].Highlight, Is.True, "en tête des agents à débloquer");

            _view = new FakeListView();
            _app.Time.LocalNow = new DateTime(2026, 10, 12, 12, 0, 0); // lundi
            Start();
            Assert.That(_view.Rows.Any(r => r.Highlight), Is.False);
            Assert.That(_view.Rows.First(r => r.Title == "Louis Dubois").Subtitle, Does.Contain("samedi ou le dimanche"));
        }

        [Test, Description("Vérifie que toucher un agent de l'équipe ouvre sa fiche et qu'un agent à débloquer n'ouvre rien — parce que seule l'équipe a une fiche d'agent")]
        public void Registry_TapOpensOnlyOwnedSheets()
        {
            Start();
            _view.SelectRow(0);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.MemberSheet));
            _nav.SelectTab(HubPage.Qg);
            _view.SelectRow(_session.Roster.Count + 2);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg), "agent verrouillé : rien ne s'ouvre");
        }

        [Test, Description("Vérifie que chaque agent du jeu a un nom et trois noms de compétence traduits (jamais une clé brute) — parce que l'interface ne doit jamais afficher « skills.xxx »")]
        public void EveryAgent_HasReadableNames()
        {
            foreach (var m in _session.Content.Members)
            {
                Assert.That(_loc.MemberName(m), Does.Not.StartWith("members."), m.Id.Value);
                foreach (var s in m.Skills) Assert.That(_loc.SkillName(s), Does.Not.StartWith("skills."), s.Id.Value);
            }
            Assert.That(_loc.MemberName(_session.Content.Member(new MemberId("ada_okonkwo"))), Is.EqualTo("Ada Okonkwo"));
            Assert.That(_loc.SkillName(_session.Content.Member(new MemberId("mei_lin_chen")).Skills[1]), Is.EqualTo("Salve · Mei-Lin Chen"));
        }
    }
}
