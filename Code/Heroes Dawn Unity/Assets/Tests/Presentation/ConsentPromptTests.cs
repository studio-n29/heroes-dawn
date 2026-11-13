using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class ConsentPromptTests
    {
        TestApp _app = null!;
        FakeConfirmPopup _popup = null!;
        ConsentPrompt _prompt = null!;
        int _opened;

        [SetUp]
        public void SetUp()
        {
            _opened = 0;
            _app = new TestApp();
            _app.SignInDirect("p1");
            var host = new FakeModalHost();
            _popup = new FakeConfirmPopup();
            host.Register<IConfirmPopupView>(() => { _opened++; return _popup; });
            _prompt = new ConsentPrompt(_app.Settings, new ModalService(host), new LocLocalization()) { Enabled = true };
        }

        [Test, Description("Vérifie que la question s'affiche à un profil qui n'a jamais répondu, avec les textes de consentement — parce que la mesure est opt-in et doit être demandée explicitement")]
        public void NeverAnswered_AsksTheQuestion()
        {
            var task = _prompt.RunIfNeededAsync(CancellationToken.None);
            Assert.That(_opened, Is.EqualTo(1));
            Assert.That(_popup.Asked!.Value.Title, Is.EqualTo(Loc.T("consent.title")));
            Assert.That(_popup.Asked.Value.Ok, Is.EqualTo(Loc.T("consent.accept")));
            Assert.That(_popup.Asked.Value.Cancel, Is.EqualTo(Loc.T("consent.decline")));
            _popup.Answer(true);
            task.GetAwaiter().GetResult();
        }

        [Test, Description("Vérifie qu'accepter mémorise le oui et la réponse — parce que la mesure ne démarre qu'avec les deux")]
        public void Accepting_StoresConsent()
        {
            var task = _prompt.RunIfNeededAsync(CancellationToken.None);
            _popup.Answer(true);
            task.GetAwaiter().GetResult();
            Assert.That(_app.Settings.Current.consentStats, Is.True);
            Assert.That(_app.Settings.Current.consentAnswered, Is.True);
        }

        [Test, Description("Vérifie que refuser, ou fermer la fenêtre par Retour, mémorise un refus et que la question ne revient pas — parce qu'ignorer la fenêtre ne doit jamais valoir accord")]
        public void DecliningOrBack_StoresRefusal_AndDoesNotAskAgain()
        {
            var task = _prompt.RunIfNeededAsync(CancellationToken.None);
            _popup.RequestCancel(); // Retour
            task.GetAwaiter().GetResult();
            Assert.That(_app.Settings.Current.consentStats, Is.False);
            Assert.That(_app.Settings.Current.consentAnswered, Is.True);
            int before = _opened;
            _prompt.RunIfNeededAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_opened, Is.EqualTo(before), "déjà répondu : pas de seconde question");
        }

        [Test, Description("Vérifie que la question est sautée quand elle est désactivée — parce que les tests de parcours ne doivent pas être gênés par la fenêtre")]
        public void Disabled_DoesNothing()
        {
            _prompt.Enabled = false;
            _prompt.RunIfNeededAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_opened, Is.EqualTo(0));
            Assert.That(_app.Settings.Current.consentAnswered, Is.False);
        }
    }
}
