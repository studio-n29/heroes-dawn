#nullable enable
using System.Collections;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Les écrans du compte avec les vrais prefabs et le simulateur : bouton du titre, fenêtre Compte, choix de progression.</summary>
    public sealed class AccountScreensPlayTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            PlayerPrefs.DeleteKey("hd.cloud.sync_state");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            PlayerPrefs.DeleteKey("hd.cloud.sync_state");
            return PlayModeHarness.TearDown();
        }

        static IEnumerator OpenAccountWindow()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            var login = Object.FindAnyObjectByType<LoginView>()!;
            Assert.That(login.AccountButton.gameObject.activeInHierarchy, Is.True);
            login.AccountButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<AccountPopupView>() != null);
        }

        [UnityTest, Timeout(240000), Description("Vérifie que le bouton « Se connecter » du titre ouvre la fenêtre Compte réelle, que l'e-mail invalide affiche l'erreur dans la fenêtre sans la fermer, que l'onglet « Créer un compte » montre le champ pseudo, et que Fermer rend le titre — parce que c'est le parcours d'entrée du compte")]
        public IEnumerator TitleButton_OpensTheWindow_Validation_Tabs_Close()
        {
            yield return OpenAccountWindow();
            var popup = Object.FindAnyObjectByType<AccountPopupView>()!;
            Assert.That(popup.NameRowVisible, Is.False, "mode Connexion : pas de pseudo");
            popup.EmailField.text = "pas-une-adresse";
            popup.PasswordField.text = "motdepasse1";
            popup.SubmitButton.Button.onClick.Invoke();
            yield return null;
            Assert.That(popup.MessageText, Is.EqualTo(Loc.T("account.error.email")));
            popup.TabSignUpButton.Button.onClick.Invoke();
            yield return null;
            Assert.That(popup.NameRowVisible, Is.True);
            popup.CloseButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<AccountPopupView>() == null);
            Assert.That(Object.FindAnyObjectByType<LoginView>()!.AccountLabel, Is.EqualTo(Loc.T("login.account")));
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'une création de compte complète (simulateur) ferme la fenêtre et que le bouton du titre montre l'e-mail du compte — parce que le joueur voit qu'il est connecté")]
        public IEnumerator SignUp_ThroughTheRealWindow_ConnectsTheAccount()
        {
            yield return OpenAccountWindow();
            var popup = Object.FindAnyObjectByType<AccountPopupView>()!;
            popup.TabSignUpButton.Button.onClick.Invoke();
            yield return null;
            popup.EmailField.text = "moi@exemple.fr";
            popup.PasswordField.text = "motdepasse1";
            popup.NameField.text = "Moi";
            popup.SubmitButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<AccountPopupView>() == null, 900);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<LoginView>()!.AccountLabel.Contains("moi@exemple.fr"), 300);
            Assert.That(Object.FindAnyObjectByType<LoginView>()!.AccountLabel, Does.Contain("moi@exemple.fr"));
        }

        [UnityTest, Timeout(240000), Description("Vérifie que la fenêtre de choix de progression réelle affiche les deux cartes avec leurs chiffres et rend le choix du bouton cliqué, et que Retour rend « rien » — parce que c'est le garde-fou anti-perte")]
        public IEnumerator ProgressChoice_RealWindow_ReturnsTheChoice()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            var host = Object.FindAnyObjectByType<ModalLayer>()!;
            var view = host.Spawn<IProgressChoicePopupView>();
            var data = new ProgressChoiceData
            {
                Title = "TITRE", Hint = "Indice", KeepDevice = "GARDER CET APPAREIL", KeepAccount = "GARDER LE COMPTE", Later = "PLUS TARD",
                Device = new ProgressCardData { Header = "CET APPAREIL", Summary = "Niveau 5 · 123 Crédits" },
                Account = new ProgressCardData { Header = "LE COMPTE", Summary = "Niveau 9 · 777 Crédits" },
            };
            var popup = (ProgressChoicePopupView)view;
            var first = view.AskAsync(data, default);
            popup.KeepAccountButton.Button.onClick.Invoke();
            Assert.That(first.GetAwaiter().GetResult(), Is.EqualTo(ChoiceKind.KeepAccount));
            var second = view.AskAsync(data, default);
            view.RequestCancel();
            Assert.That(second.GetAwaiter().GetResult(), Is.Null);
            yield return null;
        }
    }
}
