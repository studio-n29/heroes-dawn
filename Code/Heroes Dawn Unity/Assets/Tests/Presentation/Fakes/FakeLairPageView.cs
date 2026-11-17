#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeLairPageView : FakeView, ILairPageView
    {
        public event Action<int>? BossSelected;
        public event Action? EnterClicked;
        public readonly List<LairScreenData> Shown = new List<LairScreenData>();
        public LairScreenData Last => Shown[Shown.Count - 1];
        public void Show(LairScreenData data) => Shown.Add(data);
        public void SelectBoss(int i) => BossSelected?.Invoke(i);
        public void ClickEnter() => EnterClicked?.Invoke();
    }
}
