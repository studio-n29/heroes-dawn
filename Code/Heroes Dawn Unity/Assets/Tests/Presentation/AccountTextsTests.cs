using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes du compte joueur (lot I).</summary>
    public sealed class AccountTextsTests
    {
        [Test, Description("Vérifie que chaque erreur de compte a un texte français (aucune clé brute affichée) — parce que le joueur lit ces messages")]
        public void AccountErrors_AllHaveAFrenchText()
        {
            var loc = new N29.HeroesDawn.Presentation.Mvp.LocLocalization();
            foreach (var key in new[]
            {
                "errors.account_email", "errors.account_password", "errors.account_exists", "errors.account_credentials", "errors.account_network",
                "errors.account_unconfirmed", "errors.account_weak_password", "errors.account_rate_limit", "errors.account_oauth",
                "errors.account_session", "errors.account_nochoice", "errors.account_cloud", "errors.account_schema", "account.connected",
            })
            {
                Assert.That(loc.Has(key), Is.True, key);
                Assert.That(loc.T(key), Is.Not.EqualTo(key), key);
            }
        }
    }
}
