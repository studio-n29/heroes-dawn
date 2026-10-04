using System;
using System.Linq;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class MissionLauncherTests
    {
        TestApp _app = null!;
        FakeToasts _toasts = null!;
        MissionLauncher _m = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _toasts = new FakeToasts();
            _m = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), _toasts, new LocLocalization());
        }

        int BoardRequests => _app.Received<NavigateRequested>().Count(n => n.Target == GameState.Board);

        [Test, Description("Vérifie que MISSION démarre la zone puis demande le plateau — parce que c'est l'action principale du QG (M10 ⑦)")]
        public void Launch_StartsZoneAndNavigatesToBoard()
        {
            _m.Launch();
            Assert.That(_app.Session.Run, Is.Not.Null);
            Assert.That(BoardRequests, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un double toucher sur MISSION ne démarre la zone qu'une fois — parce qu'un second démarrage relancerait la zone (correctif du sous-projet 2)")]
        public void Launch_Twice_StartsZoneOnce()
        {
            _m.Launch();
            var run = _app.Session.Run;
            _m.Launch();
            Assert.That(_app.Session.Run, Is.SameAs(run));
            Assert.That(BoardRequests, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un départ refusé affiche l'erreur et permet de réessayer — parce que la garde ne doit pas bloquer après un échec")]
        public void Launch_Failure_ToastsAndAllowsRetry()
        {
            foreach (var m in _app.Session.Roster) m.InSquad = false;
            _m.Launch();
            Assert.That(_toasts.Shown.Any(t => t.Kind == ToastKind.Error), Is.True);
            Assert.That(BoardRequests, Is.Zero);
            foreach (var m in _app.Session.Roster) m.InSquad = true;
            _m.Launch();
            Assert.That(BoardRequests, Is.EqualTo(1));
        }
    }

    public sealed class SquadCombosTests
    {
        static ContentCatalog C => PrototypeContent.Build();
        static MemberDef M(string id) => C.Member(new MemberId(id));

        [Test, Description("Vérifie que chaque paire de membres dont les familles forment un combo est listée une fois — parce que l'écran Équipe montre les combos de l'escouade (M20 ⑤)")]
        public void Pairs_MatchingFamilies_ListedOnce()
        {
            var c = C;
            var squad = new[] { M("givre"), M("echo"), M("seve") };
            var combos = SquadCombos.Of(squad, c.Combos);
            Assert.That(combos.Count, Is.EqualTo(3));
            Assert.That(combos.Select(x => (x.A.Id, x.B.Id)).Distinct().Count(), Is.EqualTo(3));
            Assert.That(combos.All(x => x.Combo.Matches(x.A.Family, x.B.Family)), Is.True);
        }

        [Test, Description("Vérifie qu'un membre seul ne forme aucun combo — parce qu'un combo demande deux membres")]
        public void SingleMember_NoCombo()
            => Assert.That(SquadCombos.Of(new[] { M("givre") }, C.Combos), Is.Empty);

        [Test, Description("Vérifie que deux membres de même famille forment le combo de résonance — parce que chaque famille a sa résonance (GDD §Combos)")]
        public void SameFamilyPair_UsesResonanceCombo()
        {
            var combos = SquadCombos.Of(new[] { M("ancre"), M("seve") }, C.Combos);
            Assert.That(combos.Single().Combo.NameKey, Is.EqualTo("combos.resonance_physical.name"));
        }
    }

    public sealed class MemberQueryTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
        }

        System.Collections.Generic.IReadOnlyList<ITeamMemberReadModel> All => ((ISessionReadModel)_app.Session).Roster;

        [Test, Description("Vérifie que le filtre par famille ne garde que cette famille — parce que l'écran Équipe filtre par famille (M20 ①)")]
        public void FilterByFamily()
            => Assert.That(MemberQuery.Apply(All, Family.Elemental, MemberSort.Level).Select(m => m.Id.Value), Is.EqualTo(new[] { "givre" }));

        [Test, Description("Vérifie le tri par fatigue décroissante — parce qu'on cherche d'abord les membres les plus fatigués")]
        public void SortByFatigueDescending()
        {
            _app.Session.Find(new MemberId("echo"))!.Fatigue = 50;
            _app.Session.Find(new MemberId("seve"))!.Fatigue = 20;
            var ids = MemberQuery.Apply(All, null, MemberSort.Fatigue).Select(m => m.Id.Value).ToArray();
            Assert.That(ids.Take(2), Is.EqualTo(new[] { "echo", "seve" }));
        }

        [Test, Description("Vérifie le tri par rareté puis par niveau — parce que les Légendes passent avant les Élites")]
        public void SortByRarityThenLevel()
        {
            _app.Session.Find(new MemberId("givre"))!.Level = 9;
            var ids = MemberQuery.Apply(All, null, MemberSort.Rarity).Select(m => m.Id.Value).ToArray();
            Assert.That(ids.First(), Is.EqualTo("givre"));
            Assert.That(ids.Last(), Is.EqualTo("seve"));
        }

        [Test, Description("Vérifie que sans filtre tous les membres restent — parce que « Tous » est le filtre par défaut")]
        public void AllFamilies_WhenNull()
            => Assert.That(MemberQuery.Apply(All, null, MemberSort.Level).Count, Is.EqualTo(All.Count));
    }

    public sealed class ShopCountdownTests
    {
        [Test, Description("Vérifie le compte à rebours en heures et minutes jusqu'à 9h00 — parce qu'il est affiché au QG et sur Recruter (spec §4)")]
        public void Hours_UntilNineAm()
            => Assert.That(ShopCountdown.Text(new DateTime(2026, 10, 3, 12, 0, 0), 9, new LocLocalization()), Is.EqualTo("Nouveaux héros dans 21 h 00"));

        [Test, Description("Vérifie que sous une heure le compte à rebours s'affiche en minutes — parce que « dans 0 h 12 » se lit mal")]
        public void Minutes_UnderOneHour()
            => Assert.That(ShopCountdown.Text(new DateTime(2026, 10, 3, 8, 48, 0), 9, new LocLocalization()), Is.EqualTo("Nouveaux héros dans 12 min"));
    }
}
