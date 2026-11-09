#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class LabCraftingTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _toasts = new FakeToasts();
        }

        LabPagePresenter Lab()
        {
            var p = new LabPagePresenter(_view, _app.Lab, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                         _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Workshop);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que le filtre par catégorie ne garde que les recettes du type choisi — parce que l'atelier doit se parcourir par type d'objet")]
        public void Lab_FilterByCategory_KeepsOnlyThatType()
        {
            var p = Lab();
            var all = _app.Workshop.Recipes();
            var type = all[0].Recipe.Type;
            int expected = all.Count(r => r.Recipe.Type == type);
            int before = _view.Rows.Length;
            _view.ClickFilter(0, 1 + (int)type);
            Assert.That(_view.Rows.Length, Is.EqualTo(before - (all.Count - expected)));
            p.Dispose();
        }

        [Test, Description("Vérifie que Fabriquer est grisé sans les matériaux et devient possible dès que les scraps sont là, avec la raison affichée — parce que le joueur doit savoir ce qui manque avant d'appuyer")]
        public void Lab_Fabricate_DisabledWithoutMaterials_EnabledWithScraps()
        {
            var p = Lab();
            var recipes = _app.Workshop.Recipes();
            int firstRecipe = System.Array.FindIndex(_view.Rows, r => r.Title == _loc.T(recipes[0].Recipe.NameKey));
            _view.SelectRow(firstRecipe);
            var type = recipes[0].Recipe.Type;
            var craft = _view.Side.Rows.Last();
            Assert.That(craft.A.Enabled, Is.False, "aucun matériau");
            Assert.That(craft.A.Reason, Is.EqualTo(_loc.T("errors.not_enough_scraps")));
            CraftingOps.AddScraps(_app.Session, N29.HeroesDawn.Definitions.ScrapKinds.For(type), 1000);
            CraftingOps.AddScraps(_app.Session, ScrapKind.Core, 1000);
            _view.SelectRow(firstRecipe);
            Assert.That(_view.Side.Rows.Last().A.Enabled, Is.True, "avec des scraps, la fabrication est possible");
            p.Dispose();
        }
    }
}
