using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class HubNavigatorTests
    {
        HubNavigator _nav = null!;
        List<HubLocation> _changes = null!;

        [SetUp]
        public void SetUp()
        {
            _nav = new HubNavigator();
            _changes = new List<HubLocation>();
            _nav.Changed += l => _changes.Add(l);
        }

        [Test, Description("Vérifie que toucher un onglet remplace la pile par cet onglet — parce qu'un menu = un onglet, sans empiler l'historique (maquettes §1)")]
        public void SelectTab_ReplacesStack()
        {
            _nav.Open(HubPage.Salon);
            _nav.SelectTab(HubPage.Team);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Team));
            Assert.That(_nav.Stack.Count, Is.EqualTo(1));
            Assert.That(_nav.IsOnTab, Is.True);
        }

        [Test, Description("Vérifie qu'ouvrir une pièce l'empile au-dessus de l'onglet courant — parce que Retour doit ramener d'où l'on vient")]
        public void Open_Room_PushesAboveCurrentTab()
        {
            _nav.Open(HubPage.Infirmary);
            Assert.That(_nav.Stack.Select(l => l.Page), Is.EqualTo(new[] { HubPage.Qg, HubPage.Infirmary }));
            Assert.That(_nav.IsOnTab, Is.False);
        }

        [Test, Description("Vérifie que Retour depuis une pièce ouverte au QG revient au QG — parce que pièce → QG (maquettes §1)")]
        public void Back_FromRoomOpenedOnQg_PopsToQg()
        {
            _nav.Open(HubPage.Salon);
            Assert.That(_nav.Back(), Is.EqualTo(BackOutcome.Popped));
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
        }

        [Test, Description("Vérifie que Retour depuis une pièce ouverte par une fiche revient à la fiche — parce que Retour remonte la pile réelle")]
        public void Back_FromRoomOpenedFromMemberSheet_PopsToSheet()
        {
            _nav.SelectTab(HubPage.Team);
            _nav.Open(HubPage.MemberSheet, new MemberId("givre"));
            _nav.Open(HubPage.Salon);
            _nav.Back();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.MemberSheet));
            Assert.That(_nav.Current.Member, Is.EqualTo(new MemberId("givre")));
        }

        [Test, Description("Vérifie que Retour sur un onglet autre que QG ramène au QG — parce que onglet → QG (maquettes §1)")]
        public void Back_FromNonQgTab_GoesToQg()
        {
            _nav.SelectTab(HubPage.Recruit);
            Assert.That(_nav.Back(), Is.EqualTo(BackOutcome.WentToQg));
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
        }

        [Test, Description("Vérifie que Retour sur le QG demande à quitter sans changer la pile — parce que QG → « Quitter le jeu ? » (maquettes §1)")]
        public void Back_OnQg_AsksQuit()
        {
            int before = _changes.Count;
            Assert.That(_nav.Back(), Is.EqualTo(BackOutcome.AskQuit));
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
            Assert.That(_changes.Count, Is.EqualTo(before));
        }

        [Test, Description("Vérifie qu'ouvrir une fiche transporte le membre et l'origine boutique — parce que la même fiche sert à l'effectif et à la boutique (spec §4)")]
        public void Open_MemberSheet_CarriesMemberIdAndFromShop()
        {
            _nav.Open(HubPage.MemberSheet, new MemberId("arc"), fromShop: true);
            Assert.That(_nav.Current.Member, Is.EqualTo(new MemberId("arc")));
            Assert.That(_nav.Current.FromShop, Is.True);
        }

        [Test, Description("Vérifie que chaque navigation est annoncée une seule fois — parce que la coque crée une page par annonce")]
        public void Changed_RaisedOncePerNavigation()
        {
            _nav.SelectTab(HubPage.Team);
            _nav.Open(HubPage.MemberSheet, new MemberId("givre"));
            _nav.Back();
            Assert.That(_changes.Select(l => l.Page), Is.EqualTo(new[] { HubPage.Team, HubPage.MemberSheet, HubPage.Team }));
        }

        [Test, Description("Vérifie qu'ouvrir deux fois la même page ne l'empile pas deux fois — parce qu'un double toucher ne doit pas dupliquer la page (Review Focus 1)")]
        public void Open_SamePageTwice_DoesNotDuplicate()
        {
            _nav.Open(HubPage.Salon);
            _nav.Open(HubPage.Salon);
            Assert.That(_nav.Stack.Count, Is.EqualTo(2));
            Assert.That(_changes.Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que retoucher l'onglet courant ne change rien — parce que des touchers rapides ne doivent pas recréer la page (Review Focus 1)")]
        public void SelectTab_SameTab_NoChange()
        {
            _nav.SelectTab(HubPage.Qg);
            Assert.That(_changes, Is.Empty);
        }
    }
}
