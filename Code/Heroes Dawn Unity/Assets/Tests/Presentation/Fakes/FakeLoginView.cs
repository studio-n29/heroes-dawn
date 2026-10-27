#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Login;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeLoginView : FakeView, ILoginView
    {
        public event Action? PlayClicked;
        public event Action? AccountClicked;
        public string AccountLabel = "";
        public readonly List<(bool Busy, string Status)> BusyStates = new List<(bool, string)>();
        public int Intros;

        public void ClickPlay() => PlayClicked?.Invoke();
        public void ClickAccount() => AccountClicked?.Invoke();
        public void SetAccountLabel(string label) => AccountLabel = label;
        public void SetBusy(bool busy, string status) => BusyStates.Add((busy, status));
        public void PlayIntro() => Intros++;
        public (bool Busy, string Status) Last => BusyStates[BusyStates.Count - 1];
    }
}
