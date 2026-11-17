#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Vue Profil de test : garde chaque affichage et déclenche les événements comme le ferait l'écran.</summary>
    public sealed class FakeProfilePageView : FakeView, IProfilePageView
    {
        public event Action? TagClicked;
        public event Action? AvatarClicked;
        public event Action<int>? AccountClicked;
        public event Action<int>? TileClicked;
        public event Action? AddFriendClicked;
        public event Action<int>? FriendRemoveClicked;
        public event Action<int>? FriendCopyClicked;
        public event Action<int>? LinkClicked;
        public event Action? BackClicked;

        public readonly List<ProfileScreenData> Shown = new List<ProfileScreenData>();
        public ProfileScreenData Last => Shown[Shown.Count - 1];
        public void Show(ProfileScreenData data) => Shown.Add(data);

        public void ClickTag() => TagClicked?.Invoke();
        public void ClickAvatar() => AvatarClicked?.Invoke();
        public void ClickAccount(int button) => AccountClicked?.Invoke(button);
        public void ClickTile(string id) => TileClicked?.Invoke(Array.FindIndex(Last.Tiles, t => t.Id == id));
        public void ClickAddFriend() => AddFriendClicked?.Invoke();
        public void ClickFriendRemove(int i) => FriendRemoveClicked?.Invoke(i);
        public void ClickFriendCopy(int i) => FriendCopyClicked?.Invoke(i);
        public void ClickLink(int i) => LinkClicked?.Invoke(i);
        public void ClickBack() => BackClicked?.Invoke();
    }
}
