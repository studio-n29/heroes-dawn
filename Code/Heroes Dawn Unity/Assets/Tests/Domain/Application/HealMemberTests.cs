using System.Linq;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Soin d'un seul agent depuis sa fiche (−30 points de fatigue pour 50 Crédits).</summary>
    public sealed class HealMemberTests
    {
        TestApp _app = null!;
        IHealMemberUseCase Heal => _app.Hub.HealMember;
        TeamMember Member => _app.Session.Roster[0];

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie qu'un soin dépense 50 Crédits, retire 30 de fatigue à l'agent visé et ne touche pas aux autres — parce que le soin est individuel")]
        public void Heal_SpendsCostAndReducesFatigue()
        {
            var other = _app.Session.Roster[1];
            Member.Fatigue = 50; other.Fatigue = 40;
            _app.Session.Wallet.Set(200, 0);
            Assert.That(Heal.ExecuteAsync(Member.Id).GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(Member.Fatigue, Is.EqualTo(50 - MetaRules.InfirmaryReduction));
            Assert.That(other.Fatigue, Is.EqualTo(40));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(200 - MetaRules.HealMemberCost));
        }

        [Test, Description("Vérifie que la fatigue ne descend jamais sous zéro — parce qu'un soin ne peut pas rendre une fatigue négative")]
        public void Heal_FatigueNeverBelowZero()
        {
            Member.Fatigue = 10;
            _app.Session.Wallet.Set(200, 0);
            Heal.ExecuteAsync(Member.Id).GetAwaiter().GetResult();
            Assert.That(Member.Fatigue, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un agent sans fatigue n'est pas soigné et que rien n'est dépensé — parce qu'on ne paie pas un soin inutile")]
        public void Heal_RefusedWithoutFatigue_SpendsNothing()
        {
            Member.Fatigue = 0;
            _app.Session.Wallet.Set(200, 0);
            Assert.That(Code(Heal.ExecuteAsync(Member.Id).GetAwaiter().GetResult()), Is.EqualTo("hub.no_fatigue"));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(200));
        }

        [Test, Description("Vérifie qu'avec 49 Crédits le soin est refusé et que la fatigue ne change pas — parce que le soin coûte 50 Crédits")]
        public void Heal_RefusedWithoutCredits_ChangesNothing()
        {
            Member.Fatigue = 50;
            _app.Session.Wallet.Set(MetaRules.HealMemberCost - 1, 0);
            Assert.That(Code(Heal.ExecuteAsync(Member.Id).GetAwaiter().GetResult()), Is.EqualTo("hub.credits"));
            Assert.That(Member.Fatigue, Is.EqualTo(50));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(MetaRules.HealMemberCost - 1));
        }

        [Test, Description("Vérifie qu'un identifiant absent de l'effectif est refusé — parce qu'on ne soigne que ses agents")]
        public void Heal_UnknownMember()
        {
            Assert.That(Code(Heal.ExecuteAsync(new MemberId("inconnu")).GetAwaiter().GetResult()), Is.EqualTo("hub.unknown"));
        }

        [Test, Description("Vérifie qu'un soin publie les changements de Crédits et d'effectif — parce que l'écran se met à jour par message")]
        public void Heal_PublishesWalletAndRoster()
        {
            Member.Fatigue = 50;
            _app.Session.Wallet.Set(200, 0);
            int wallets = _app.Received<WalletChanged>().Count, rosters = _app.Received<RosterChanged>().Count;
            Heal.ExecuteAsync(Member.Id).GetAwaiter().GetResult();
            Assert.That(_app.Received<WalletChanged>().Count, Is.EqualTo(wallets + 1));
            Assert.That(_app.Received<RosterChanged>().Count, Is.EqualTo(rosters + 1));
        }
    }
}
