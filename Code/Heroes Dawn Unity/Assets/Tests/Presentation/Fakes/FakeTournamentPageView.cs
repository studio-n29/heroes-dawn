#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeTournamentPageView : FakeView, ITournamentPageView
    {
        public event Action? ClaimClicked;
        public event Action? BackToQgClicked;
        public readonly List<TournamentScreenData> Shown = new List<TournamentScreenData>();
        public TournamentScreenData Last => Shown[Shown.Count - 1];
        public void Show(TournamentScreenData data) => Shown.Add(data);
        public void ClickClaim() => ClaimClicked?.Invoke();
        public void ClickBackToQg() => BackToQgClicked?.Invoke();
    }
}
