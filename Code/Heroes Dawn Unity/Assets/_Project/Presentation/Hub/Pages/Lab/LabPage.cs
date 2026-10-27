#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ILabPageView : IListPageView { }

    public sealed class LabPagePresenter : ListPagePresenter<ILabPageView>
    {
        readonly ResearchLab _lab;
        readonly ISessionReadModel _session;
        readonly Workshop _workshop;
        IReadOnlyList<RecipeView> _recipes = new List<RecipeView>();

        public LabPagePresenter(ILabPageView view, ResearchLab lab, ISessionReadModel session, ILocalization loc, IToastService toasts,
                                ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock,
                                Workshop workshop)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _lab = lab; _session = session; _workshop = workshop;
        }

        public override string Title => Loc.T("page.lab");
        protected override bool HasTimers => true;

        protected override void OnPageShown() => Finish();

        void Finish()
        {
            var done = _lab.Refresh();
            if (done != null) Toasts.Show(Loc.F("lab.done", Loc.T("lab." + done.Id)), ToastKind.Reward);
        }

        protected override void Render()
        {
            Finish();
            var views = _lab.Views();
            var active = views.FirstOrDefault(v => v.Active);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("lab.title"),
                Line = active.Tech != null ? Loc.F("lab.running", Loc.T("lab." + active.Tech.Id), MetaFormat.Duration(active.Left)) : Loc.T("lab.idle"),
                Progress = -1f,
                A = active.Tech != null
                    ? RowButtonData.Of(Loc.F("lab.speed", ResearchLab.SpeedUpCost(active.Left)), _session.Wallet.Gems >= ResearchLab.SpeedUpCost(active.Left), Loc.T("errors.not_enough_gems"), 1)
                    : default,
            });
            var rows = new InfoRowData[views.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var v = views[i];
                var r = InfoRowData.Basic(Loc.F("lab.level", Loc.T("lab." + v.Tech.Id), v.Level, v.Tech.MaxLevel), Loc.T("lab." + v.Tech.Id + ".desc"));
                r.Tag = v.Level.ToString();
                r.TagColor = v.Maxed ? Theme.Success : Theme.Info;
                r.Highlight = v.Active;
                if (v.Active)
                {
                    int minutes = v.Tech.MinutesFor(v.Level);
                    r.Progress = Mathf.Clamp01(1f - (float)(v.Left.TotalMinutes / minutes));
                    r.ProgressLabel = MetaFormat.Duration(v.Left);
                }
                if (v.Maxed) r.A = RowButtonData.Of(Loc.T("lab.max"), false, "", 3);
                else
                {
                    int cost = v.Tech.CostFor(v.Level);
                    bool busy = active.Tech != null;
                    bool afford = _session.Wallet.Credits >= cost;
                    r.A = RowButtonData.Of(Loc.F("lab.start", cost), !busy && afford, busy ? Loc.T("lab.busy") : Loc.F("hub.train.missing", cost - _session.Wallet.Credits), 0);
                    r.Subtitle += " · " + Loc.F("lab.time", MetaFormat.Duration(System.TimeSpan.FromMinutes(v.Tech.MinutesFor(v.Level))));
                }
                rows[i] = r;
            }
            var all = new List<InfoRowData>(rows);
            AddWorkshopRows(all);
            View.ShowRows(all.ToArray());
        }

        /// <summary>Atelier : titre (avec les scraps), une rangée par compétence de création, une par recette connue.</summary>
        void AddWorkshopRows(List<InfoRowData> rows)
        {
            var scraps = new List<string>();
            for (int k = 0; k < ScrapKinds.Count; k++) scraps.Add(CollectionText.Scrap(Loc, (ScrapKind)k, _workshop.Scraps((ScrapKind)k)));
            var title = InfoRowData.Basic(Loc.T("lab.workshop"), string.Join(" · ", scraps));
            title.Dim = true;
            rows.Add(title);
            foreach (var s in _workshop.Skills())
            {
                var r = InfoRowData.Basic(Loc.F("lab.skill", CollectionText.ItemTypeName(Loc, s.Type), s.Level, CraftingRules.MaxSkill), Loc.T("lab.skill.desc"));
                r.Tag = s.Level.ToString();
                r.TagColor = s.Maxed ? Theme.Success : Theme.Info;
                if (!s.Maxed) { r.Progress = Mathf.Clamp01(s.XpInLevel / (float)s.XpToNext); r.ProgressLabel = s.XpInLevel + " / " + s.XpToNext; }
                rows.Add(r);
            }
            _recipes = _workshop.Recipes();
            foreach (var v in _recipes)
            {
                string name = Loc.T(v.Recipe.NameKey);
                var r = InfoRowData.Basic(name, RecipeLine(v));
                r.Tag = CollectionText.Tag(name);
                r.TagColor = CollectionText.RarityColor(v.Recipe.Rarity);
                r.A = RowButtonData.Of(Loc.T("lab.craft"), v.CanAfford, Loc.T("errors.not_enough_scraps"), 0);
                rows.Add(r);
            }
        }

        string RecipeLine(RecipeView v)
        {
            var cost = CollectionText.Scrap(Loc, ScrapKinds.For(v.Recipe.Type), v.Recipe.ScrapCost);
            if (v.Recipe.CoreCost > 0) cost += " + " + CollectionText.Scrap(Loc, ScrapKind.Core, v.Recipe.CoreCost);
            var odds = new List<string>();
            for (int q = 0; q < v.Odds.Count; q++)
                if (v.Odds[q] > 0) odds.Add(Loc.F("lab.odds", CollectionText.QualityName(Loc, (CraftQuality)q), v.Odds[q]));
            return Loc.F("lab.recipe_sub", cost, v.Recipe.Difficulty, string.Join(" · ", odds));
        }

        protected override void OnHeader(int index) => Report(_lab.SpeedUp(), Loc.T("lab.sped"));

        protected override void OnRowButton(int row, int button)
        {
            if (row < 0) return;
            if (row >= ResearchLab.Techs.Count)
            {
                int recipe = row - FirstRecipeRow;
                if (recipe >= 0 && recipe < _recipes.Count) Craft(_recipes[recipe].Recipe.Id);
                return;
            }
            Report(_lab.Start(ResearchLab.Techs[row].Id), Loc.T("lab.started"));
        }

        // Les recherches, le titre de l'atelier puis les 4 compétences précèdent les recettes.
        static int FirstRecipeRow => ResearchLab.Techs.Count + 1 + 4;

        void Craft(string recipeId)
        {
            var r = _workshop.Craft(recipeId);
            if (r.IsFailure) { Report(r); return; }
            var o = r.Value;
            Toasts.Show(Loc.F("lab.crafted", CollectionText.ItemName(Loc, o.Item), CollectionText.ItemEffects(Loc, o.Item)), ToastKind.Reward);
            if (o.LeveledUp) Toasts.Show(Loc.F("lab.skill_up", CollectionText.ItemTypeName(Loc, o.Item.Type), o.NewSkill), ToastKind.Reward);
        }
    }
}
