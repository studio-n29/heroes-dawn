using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Ajout d'un ami par la saisie de son tag depuis la page Profil.</summary>
    public sealed class FriendAddTests
    {
        sealed class FakeFriendAddPopup : IFriendAddPopupView
        {
            public string? Answer;
            public bool Cancelable => true;
            public void RequestCancel() => Answer = null;
            public UniTask<string?> AskAsync(string title, string hint, string ok, string cancel, CancellationToken ct) => UniTask.FromResult(Answer);
        }

        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        FakeFriendAddPopup _popup = null!;
        readonly LocLocalization _loc = new LocLocalization();

        ProfilePagePresenter Profile()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _toasts = new FakeToasts();
            _popup = new FakeFriendAddPopup();
            var host = new FakeModalHost();
            host.Register<IFriendAddPopupView>(() => _popup);
            var p = new ProfilePagePresenter(_view, _app.Profile, _app.Friends, new HubNavigator(), _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                             _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, new ModalService(host));
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie qu'un tag valide saisi dans la fenêtre ajoute l'ami, affiche un message et met la liste à jour — parce que c'est le chemin normal d'ajout")]
        public void AddFriend_WithAValidTag_AddsAndToasts()
        {
            var p = Profile();
            _popup.Answer = "Mara#0007";
            _view.ClickHeader(1);
            Assert.That(_app.Friends.Friends().Select(f => f.Tag), Is.EqualTo(new[] { "Mara#0007" }));
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Reward && t.Text == _loc.T("profile.friend_added")));
            Assert.That(_view.Rows.Any(r => r.Title == "Mara"), Is.True);
            p.Dispose();
        }

        [Test, Description("Vérifie qu'annuler la fenêtre n'ajoute personne et n'affiche aucune erreur — parce qu'annuler est un choix, pas une faute")]
        public void AddFriend_Cancelled_ChangesNothing()
        {
            var p = Profile();
            _popup.Answer = null;
            _view.ClickHeader(1);
            Assert.That(_app.Friends.Friends(), Is.Empty);
            Assert.That(_toasts.Shown.Any(t => t.Kind == ToastKind.Error), Is.False);
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un tag mal formé ou en double affiche l'erreur correspondante — parce que le joueur doit savoir pourquoi l'ajout échoue")]
        public void AddFriend_InvalidOrDuplicate_ShowsTheErrorToast()
        {
            var p = Profile();
            _popup.Answer = "Mara";
            _view.ClickHeader(1);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error && t.Text == _loc.T("errors.friends_invalid")));
            _popup.Answer = "Mara#0007";
            _view.ClickHeader(1);
            _popup.Answer = "MARA#0007";
            _view.ClickHeader(1);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error && t.Text == _loc.T("errors.friends_duplicate")));
            Assert.That(_app.Friends.Friends().Count, Is.EqualTo(1));
            p.Dispose();
        }
    }
}
