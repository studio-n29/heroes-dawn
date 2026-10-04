#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeSalonPageView : FakeView, ISalonPageView
    {
        public event Action<MemberId>? TrainClicked;
        public readonly List<SalonPageData> Shown = new List<SalonPageData>();
        public SalonPageData Last => Shown[Shown.Count - 1];
        public void Show(SalonPageData d) => Shown.Add(d);
        public void ClickTrain(MemberId id) => TrainClicked?.Invoke(id);
    }

    public sealed class FakeInfirmaryPageView : FakeView, IInfirmaryPageView
    {
        public event Action? HealClicked;
        public readonly List<InfirmaryPageData> Shown = new List<InfirmaryPageData>();
        public InfirmaryPageData Last => Shown[Shown.Count - 1];
        public void Show(InfirmaryPageData d) => Shown.Add(d);
        public void ClickHeal() => HealClicked?.Invoke();
    }

    public sealed class FakeSettingsPageView : FakeView, ISettingsPageView
    {
        public event Action<int>? CategorySelected;
        public event Action<int, int>? RowAction;
        public event Action? ResetCategoryClicked;
        public readonly List<SettingsPageData> Shown = new List<SettingsPageData>();
        public SettingsPageData Last => Shown[Shown.Count - 1];
        public void Show(SettingsPageData d) => Shown.Add(d);
        public void SelectCategory(int i) => CategorySelected?.Invoke(i);
        public void Act(int row, int action = 0) => RowAction?.Invoke(row, action);
        public void ClickResetCategory() => ResetCategoryClicked?.Invoke();
        public int RowOf(string label) => Array.FindIndex(Last.Rows, r => r.Label == label);
    }
}
